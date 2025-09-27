using UnityEngine;

public class SpawnPlattform : MonoBehaviour
{
    [Header("Areas (je ein BoxCollider2D)")]
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private BoxCollider2D preloadArea;
    [SerializeField] private BoxCollider2D cullArea;

    [Header("Plattform & Layer")]
    [SerializeField] private GameObject spawnablePlatform;
    [SerializeField] private LayerMask platformLayer;
    [Header("Taktung (optional)")]
    [SerializeField] private float checkEverySeconds = 0.2f;
    private float _nextCheck;

    private void Update()
    {
        if (Time.time < _nextCheck) return;
        _nextCheck = Time.time + checkEverySeconds;

        if (!spawnablePlatform) return;

        if (cullArea) CullPlatformsInArea(cullArea);

        if (preloadArea) SpawnOneIfAreaEmpty(preloadArea);

        if (spawnArea)
        {
            if (spawnArea.isActiveAndEnabled)
            {
                SpawnOneIfAreaEmpty(spawnArea);
            }
        }
    }

    // --- Helpers ---

    private void SpawnOneIfAreaEmpty(BoxCollider2D area)
    {
        if (!area) return;

        Bounds b = area.bounds;
        Vector2 center = b.center;
        Vector2 size = b.size;
        float angle = area.transform.eulerAngles.z;

        bool hasPlatform = Physics2D.OverlapBox(center, size, angle, platformLayer) != null;
        if (hasPlatform) return;

        // zufällige Position innerhalb der Area
        Vector2 spawnPos = GetRandomPointInBounds(b);
        Instantiate(spawnablePlatform, spawnPos, Quaternion.identity);
    }

    private void CullPlatformsInArea(BoxCollider2D area)
    {
        if (!area) return;

        Bounds b = area.bounds;
        Vector2 center = b.center;
        Vector2 size = b.size;
        float angle = area.transform.eulerAngles.z;

        // Alle Plattform-Collider in der Area holen und deren GameObjects zerstören
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle, platformLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].gameObject != null)
            {
                Destroy(hits[i].gameObject);
            }
        }
    }

    private Vector2 GetRandomPointInBounds(Bounds b)
    {
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        return new Vector2(x, y);
    }

    // Gizmos zur Visualisierung
    private void OnDrawGizmosSelected()
    {
        DrawAreaGizmo(spawnArea, new Color(0.2f, 0.8f, 0.2f, 1f));   // grün
        DrawAreaGizmo(preloadArea, new Color(0.2f, 0.6f, 1f, 1f));    // blau
        DrawAreaGizmo(cullArea, new Color(1f, 0.3f, 0.3f, 1f));       // rot
    }

    private void DrawAreaGizmo(BoxCollider2D area, Color c)
    {
        if (!area) return;
        Gizmos.color = c;
        var b = area.bounds;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
