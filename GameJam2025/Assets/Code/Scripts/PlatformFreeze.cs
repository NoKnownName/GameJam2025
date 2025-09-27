using System.Collections.Generic;
using UnityEngine;

public class PlatformFreeze : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 3f;

    private bool active;
    private float timer;

    private PhysicsMaterial2D iceMat;
    private readonly List<Collider2D> cols = new();
    private readonly List<PhysicsMaterial2D> originalMats = new();

    private readonly List<SpriteRenderer> srs = new();
    private readonly List<Color> originalColors = new();

    private readonly Dictionary<Rigidbody2D, float> originalLinearDamping = new();

    private void Awake()
    {
        GetComponentsInChildren(true, cols);
        GetComponentsInChildren(true, srs);

        foreach (var c in cols) originalMats.Add(c.sharedMaterial);
        foreach (var sr in srs) originalColors.Add(sr.color);

        iceMat = new PhysicsMaterial2D("IceMat") { friction = 0f, bounciness = 0f };
    }

    public void Activate(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;

        for (int i = 0; i < srs.Count; i++)
            srs[i].color = new Color(0.55f, 0.75f, 1f, srs[i].color.a);

        for (int i = 0; i < cols.Count; i++)
            cols[i].sharedMaterial = iceMat;

        active = true;
        timer = duration;
        enabled = true;
    }

    private void Update()
    {
        if (!active) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) Deactivate();
    }

    public void Deactivate()
    {
        for (int i = 0; i < srs.Count; i++)
            srs[i].color = originalColors[i];

        for (int i = 0; i < cols.Count; i++)
            cols[i].sharedMaterial = originalMats[i];

        foreach (var kv in originalLinearDamping)
            if (kv.Key != null) kv.Key.linearDamping = kv.Value;
        originalLinearDamping.Clear();

        active = false;
        enabled = false;
    }

    private void OnDestroy()
    {
        if (active) Deactivate();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!active) return;

        var rb = col.rigidbody;
        if (rb == null) return;

        if (rb.GetComponent<PlayerMovement>() == null) return;

        if (!originalLinearDamping.ContainsKey(rb))
        {
            originalLinearDamping[rb] = rb.linearDamping;
            rb.linearDamping = 0f;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        var rb = col.rigidbody;
        if (rb == null) return;

        if (originalLinearDamping.TryGetValue(rb, out var damp))
        {
            rb.linearDamping = damp;
            originalLinearDamping.Remove(rb);
        }
    }
}
