using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private float spawnCooldown = 3;

    void Update()
    {
        if (spawnCooldown <= 0)
        {
            Instantiate(cloudPrefab, new Vector3(2100, Random.Range(0, 1100)), new Quaternion(0, 0, 0, 0), GameObject.FindWithTag("Canvas").transform);
            spawnCooldown = Random.Range(2f, 3f);
        }
        else if (spawnCooldown > 0)
        {
            spawnCooldown -= Time.deltaTime;
        }
    }
}
