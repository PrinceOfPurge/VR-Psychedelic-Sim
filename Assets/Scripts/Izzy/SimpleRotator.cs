using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [Tooltip("Degrees per second around the X, Y, and Z axes")]
    public Vector3 rotationSpeed = new Vector3(0, 10f, 0); // Default to Y-axis rotation for Earth

    void Update()
    {
        // Rotate the object around its local axes
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}