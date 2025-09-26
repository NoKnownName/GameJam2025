using UnityEngine;

public class CameraPlayerFollowScript : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float followSpeed = 5f;

    [Tooltip("Halbe Breite des Rechtecks (links/rechts vom Spieler).")]
    public float halfWidth = 3f;

    [Tooltip("Halbe Höhe des Rechtecks (oben/unten vom Spieler).")]
    public float halfHeight = 5f;

    [Tooltip("Toleranz, um Rand-Flattern zu vermeiden.")]
    public float epsilon = 0.05f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 current = transform.position;
        Vector3 targetPos = target.position;
        targetPos.x = current.x;
        targetPos.z = current.z;

        Vector2 offset = (Vector2)(current - targetPos);

        float desiredX;
        float desiredY;

        if (Mathf.Abs(offset.x) > (halfWidth + epsilon))
        {
            desiredX = targetPos.x + Mathf.Sign(offset.x) * halfWidth;
        }
        else
        {
            desiredX = Mathf.MoveTowards(current.x, targetPos.x, followSpeed * Time.deltaTime);
        }

        if (Mathf.Abs(offset.y) > (halfHeight + epsilon))
        {
            desiredY = targetPos.y + Mathf.Sign(offset.y) * halfHeight;
        }
        else
        {
            desiredY = Mathf.MoveTowards(current.y, targetPos.y, followSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(desiredX, desiredY, current.z);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Gizmos.color = Color.cyan;

        Vector3 center = new Vector3(target.position.x, target.position.y, transform.position.z);

        Vector3 tl = new Vector3(center.x - halfWidth, center.y + halfHeight, center.z);
        Vector3 tr = new Vector3(center.x + halfWidth, center.y + halfHeight, center.z);
        Vector3 bl = new Vector3(center.x - halfWidth, center.y - halfHeight, center.z);
        Vector3 br = new Vector3(center.x + halfWidth, center.y - halfHeight, center.z);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
#endif
}
