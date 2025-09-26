using UnityEngine;

/// <summary>
/// Simples Pool-Item ("Fass"):
/// - bewegt sich konstant nach links (speedX)
/// - eigene Gravitation wie der Player (über PlayerMovementStats)
/// - gibt sich beim Treffen der DeathZone an den PooledSpawner zurück
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BarrelItem : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speedX = 3f;        // konstante Linksbewegung
    [SerializeField] private float rollAngularVel = -180f; // optional: "rollen"

    [Header("Gravity (wie Player)")]
    public PlayerMovementStats moveStats;              // dein ScriptableObject wiederverwenden

    [Header("Ground Check (für sauberes Fallen)")]
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private float groundRayLength = 0.02f;

    [Header("Release bei DeathZone")]
    [SerializeField] private LayerMask deathZoneMask;  // im Inspector die DeathZone-Layer anhaken

    private Rigidbody2D rb;
    private float verticalVelocity;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;           // wir rechnen die Gravitation selbst
        rb.freezeRotation = false;      // darf rotieren
    }

    private void OnEnable()
    {
        verticalVelocity = 0f;
        if (rb != null) rb.angularVelocity = rollAngularVel;

        // Falls wir IN der DeathZone spawnen, sofort releasen
        TryReleaseIfInsideDeathZone();
    }

    private void FixedUpdate()
    {
        GroundCheck();

        // einfache Custom-Gravity
        if (!isGrounded)
            verticalVelocity += moveStats.Gravity * Time.fixedDeltaTime;
        else if (verticalVelocity < 0f)
            verticalVelocity = 0f;

        verticalVelocity = Mathf.Clamp(verticalVelocity, -moveStats.maxFallSpeed, 50f);

        // konstante Linksbewegung + vertikale Geschwindigkeit
        rb.linearVelocity = new Vector2(-Mathf.Abs(speedX), verticalVelocity);
    }

    private void GroundCheck()
    {
        if (bodyCollider == null)
            return;

        Vector2 origin = new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y);
        Vector2 size = new Vector2(bodyCollider.bounds.size.x, groundRayLength);
        var hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundRayLength, moveStats.groundLayer);
        isGrounded = hit.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // LayerMask-Check für DeathZone (robust & schnell)
        if (((1 << other.gameObject.layer) & deathZoneMask) != 0)
        {
            if (PooledSpawner.Instance != null)
                PooledSpawner.Instance.Release(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    private void TryReleaseIfInsideDeathZone()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;

        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(deathZoneMask);

        Collider2D[] results = new Collider2D[1];
        // Neu: Overlap statt OverlapCollider
        if (col.Overlap(filter, results) > 0)
        {
            if (PooledSpawner.Instance != null)
                PooledSpawner.Instance.Release(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

}
