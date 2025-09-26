using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 movement;
    public static bool jumpWasPressed;
    public static bool jumpIsHeld;
    public static bool jumpWasReleased;
    public static bool runIsHeld;

    private Camera myCamera;
    private float cooldown;
    private GameObject clickedObject;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        moveAction = PlayerInput.actions["Move"];
        jumpAction = PlayerInput.actions["Jump"];
        runAction = PlayerInput.actions["Run"];

        myCamera = Camera.main;
    }

    private void Update()
    {
        movement = moveAction.ReadValue<Vector2>();

        jumpWasPressed = jumpAction.WasPressedThisFrame();
        jumpIsHeld = jumpAction.IsPressed();
        jumpWasReleased = jumpAction.WasReleasedThisFrame();

        runIsHeld = runAction.IsPressed();

        if (clickedObject != null)
        {
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
                clickedObject.SetActive(false);
            }
            else
            {
                clickedObject.SetActive(true);
                cooldown = 0;
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(myCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        cooldown = 3;

        clickedObject = rayHit.collider.gameObject;
    }
}
