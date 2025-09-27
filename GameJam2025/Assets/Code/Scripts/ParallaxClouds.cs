using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class ParallaxClouds : MonoBehaviour
{
    [SerializeField] private Image cloudSprite;
    [SerializeField] private float maxLeft = -1200;
    [SerializeField] private float maxRight = 1200;
    [SerializeField] private float maxUp = 700;
    [SerializeField] private float maxDown = -700;
    public static bool rotating;
    void Update()
    {
        if (cloudSprite != null)
        {
            cloudSprite.transform.Translate(Vector2.left);
            if (rotating)
            {
                StartCoroutine(RotateObject(cloudSprite, Random.Range(1f, 3f), new Vector3(0f, 0f, 180f)));
            }
            if (cloudSprite.transform.localPosition.x < maxLeft || cloudSprite.transform.localPosition.x > maxRight || cloudSprite.transform.localPosition.y < maxDown || cloudSprite.transform.localPosition.y > maxUp)
            {
                Destroy(cloudSprite.gameObject);
            }
        }
    }
    private IEnumerator RotateObject(Image target, float duration, Vector3 eulerPerSecond)
    {
        float t = 0f;
        while (t < duration)
        {
            target.transform.Rotate(eulerPerSecond * Time.deltaTime, Space.Self);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
