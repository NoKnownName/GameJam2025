using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Einfacher Singleton-Pool mit Queue. Erfüllt die Hausaufgabe:
/// - Get(Vector3, Quaternion): holt Objekt aus Queue oder instanziert neu, setzt Pos/Rot und aktiviert.
/// - Release(GameObject): deaktiviert und hängt ans Ende der Queue.
/// - Zähler: CountInPool (wie viele in der Queue) + TotalCreated (wie viele insgesamt erzeugt).
/// Optional: Auto-Spawn im Intervall.
/// </summary>
public class PooledSpawner : MonoBehaviour
{
    public static PooledSpawner Instance { get; private set; }

    [Header("Pool Limits")]
    [SerializeField] private int maxItemsInPool = 50;

    [Header("Setup")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform poolParent = null; // LEER lassen, um Scale-Probleme zu vermeiden
    [SerializeField] private int prewarm = 0;

    [Header("Auto Spawn (optional)")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField, Min(0.05f)] private float spawnInterval = 0.8f;
    [SerializeField] private bool randomizeInBox = false;
    [SerializeField] private BoxCollider2D spawnZone = null; // optional; nur wenn randomizeInBox=true

    // interne Queue
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    [Header("Debug Counters (read-only)")]
    [SerializeField] private int countInPool;     // wie viele aktuell in der Queue sind
    [SerializeField] private int totalCreated;    // wie viele insgesamt erzeugt wurden

    public int CountInPool => pool.Count;
    public int TotalCreated => totalCreated;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Vorbefüllen
        for (int i = 0; i < prewarm; i++)
        {
            var go = Instantiate(prefab);                 // ohne Parent, um Scale zu vermeiden
            go.transform.localScale = prefab.transform.localScale;
            if (poolParent != null) go.transform.SetParent(poolParent, true);
            go.SetActive(false);
            pool.Enqueue(go);
            totalCreated++;
        }
        UpdateCounters();
    }

    private void OnEnable()
    {
        if (autoSpawn) StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            if (TotalCreated < maxItemsInPool || CountInPool > 0)
            {
                Vector3 pos = GetSpawnPosition();
                Get(pos, Quaternion.identity);
            }
            yield return wait;
        }
    }


    private Vector3 GetSpawnPosition()
    {
        if (spawnZone != null)
        {
            var b = spawnZone.bounds;

            if (!randomizeInBox)
            {
                // Mitte der SpawnZone (Z aus dem Spawner übernehmen)
                return new Vector3(b.center.x, b.center.y, transform.position.z);
            }
            else
            {
                // Zufällig in der Zone
                float x = Random.Range(b.min.x, b.max.x);
                float y = Random.Range(b.min.y, b.max.y);
                return new Vector3(x, y, transform.position.z);
            }
        }

        // Fallback: Spawner-Position
        return transform.position;
    }

    /// <summary>
    /// Gibt ein Objekt aus dem Pool zurück oder erzeugt ein neues, setzt Pos/Rot und aktiviert es.
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject go = null;

        if (pool.Count == 0)
        {
            if (totalCreated < maxItemsInPool)
            {
                go = Instantiate(prefab, position, rotation);
                totalCreated++;
            }
            else
            {
                // Limit erreicht → kein Spawn möglich
                Debug.LogWarning("PooledSpawner: maxItemsInPool erreicht!");
                return null;
            }
        }
        else
        {
            go = pool.Dequeue();
            go.transform.SetPositionAndRotation(position, rotation);
        }

        if (poolParent != null)
            go.transform.SetParent(poolParent, true);

        go.transform.localScale = prefab.transform.localScale;

        go.SetActive(true);
        UpdateCounters();
        return go;
    }


    /// <summary>
    /// Deaktiviert das übergebene Objekt und hängt es an die Queue an.
    /// </summary>
    public void Release(GameObject gameObjectToPool)
    {
        if (gameObjectToPool == null) return;

        gameObjectToPool.SetActive(false);
        // Optional vom Parent lösen, wenn du absolut sicher jegliche Parent-Effekte vermeiden willst:
        // gameObjectToPool.transform.SetParent(null, true);

        pool.Enqueue(gameObjectToPool);
        UpdateCounters();
    }

    private void UpdateCounters()
    {
        countInPool = pool.Count;
        // totalCreated wird an den relevanten Stellen erhöht
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (randomizeInBox && spawnZone != null)
        {
            var b = spawnZone.bounds;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.12f);
        }
    }
#endif
}
