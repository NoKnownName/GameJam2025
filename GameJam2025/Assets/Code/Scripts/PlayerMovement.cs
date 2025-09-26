using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats moveStats;
    [SerializeField] private Collider2D feetCollider;
    [SerializeField] private Collider2D bodyCollider;

    private Rigidbody2D rb;

    //Movement variables
    private Vector2 moveVelocity;
    private bool isFacingRight;

    //Collision check variables
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;

    //Jump variables
    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    //Apex variables
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    //Jump buffer variables
    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    //Coyote time variables
    private float coyoteTimer;

    private void Awake()
    {
        isFacingRight = true;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionCheck();
        Jump();

        if (isGrounded)
            Move(moveStats.groundAcceleration, moveStats.groundDeceleration, InputManager.movement);
        else
            Move(moveStats.airAcceleration, moveStats.airDeceleration, InputManager.movement);
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.runIsHeld)
                targetVelocity = new Vector2(moveInput.x, 0f) * moveStats.maxRunSpeed;
            else
                targetVelocity = new Vector2(moveInput.x, 0f) * moveStats.maxWalkSpeed;

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
            Turn(false);
        else if (!isFacingRight && moveInput.x > 0)
            Turn(true);
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        //Press jump button
        if (InputManager.jumpWasPressed)
        {
            jumpBufferTimer = moveStats.jumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }

        //Release jump button
        if (InputManager.jumpWasReleased)
        {
            if (jumpBufferTimer > 0f)
                jumpReleasedDuringBuffer = true;

            if (isJumping && VerticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = moveStats.timeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiate jump with buffering and coyote time
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //Doublejump
        else if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < moveStats.numberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        //Air jump after coyote time
        else if (jumpBufferTimer > 0f && isFalling && numberOfJumpsUsed < moveStats.numberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }

        //Player Landed
        if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int jumpsUsed)
    {
        if (!isJumping)
            isJumping = true;

        jumpBufferTimer = 0f;
        numberOfJumpsUsed += jumpsUsed;
        VerticalVelocity = moveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        //Apply gravity while jumping
        if (isJumping)
        {
            if (bumpedHead)
                isFastFalling = true;

            //Gravity ascending
            if (VerticalVelocity >= 0f)
            {
                //Apex controls
                apexPoint = Mathf.InverseLerp(moveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (apexPoint > moveStats.apexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < moveStats.apexHangTime)
                            VerticalVelocity = 0f;
                        else
                        {
                            VerticalVelocity = -0.5f;
                            isPastApexThreshold = false;
                        }
                    }
                }

                //Gravity ascending but not past apex threshold
                else
                {
                    VerticalVelocity += moveStats.Gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                        isPastApexThreshold = false;
                }
            }

            //Gravity descending
            else if (isFastFalling)
                VerticalVelocity += moveStats.Gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;

            else if (VerticalVelocity < 0f)
                if (!isFalling)
                    isFalling = true;

            if (!isPastApexThreshold)
                VerticalVelocity += moveStats.Gravity * Time.fixedDeltaTime;
        }

        //Jump cut
        if (isFastFalling)
        {
            if (fastFallTime >= moveStats.timeForUpwardsCancel)
                VerticalVelocity += moveStats.Gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            else if (fastFallTime < moveStats.timeForUpwardsCancel)
                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / moveStats.timeForUpwardsCancel));

            fastFallTime += Time.fixedDeltaTime;
        }

        //Normal gravity while falling
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
                isFalling = true;

            VerticalVelocity += moveStats.Gravity * Time.fixedDeltaTime;
        }

        //Clamp fallspeed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -moveStats.maxFallSpeed, 50f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision checks whilst movement

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x,  moveStats.groundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, moveStats.groundDetectionRayLength, moveStats.groundLayer);
        if (groundHit.collider != null)
            isGrounded = true;
        else
            isGrounded = false;
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x * moveStats.headWidth, moveStats.headDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, moveStats.headDetectionRayLength, moveStats.groundLayer);
        if (headHit.collider != null)
            bumpedHead = true;
        else
            bumpedHead = false;
    }

    private void CollisionCheck()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded)
            coyoteTimer -= Time.deltaTime;
        else
            coyoteTimer = moveStats.jumpCoyoteTime;
    }

    #endregion
}
