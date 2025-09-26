using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "test")
        {
            Destroy(collision.gameObject);
        }
    }
}
