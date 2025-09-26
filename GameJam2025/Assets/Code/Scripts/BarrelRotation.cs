using UnityEngine;

public class BarrelRotation : MonoBehaviour
{
    [SerializeField] Transform visual;
    [SerializeField] float spinSpeed = 180f; // Grad/Sek

    void Update()
    {
        visual.Rotate(0, 0, -spinSpeed * Time.deltaTime);
    }
}
