using UnityEngine;
using System.Collections.Generic;

public class ObjectRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(1.5f, 3.2f, 0.8f);

    [Header("Distance (D) Pulsing")]
    public float baseDistance = 200f;
    public float pulseAmount = 50f; // How far it moves from base
    public float pulseSpeed = 0.5f;

    // Internal data to track child directions
    private struct ChildData
    {
        public Transform transform;
        public Vector3 direction; // Normalized direction from center
    }
    private List<ChildData> children = new List<ChildData>();

    void Start()
    {
        // Identify all children (Fx1, Fx2, etc.)
        foreach (Transform child in transform)
        {
            if (child.localPosition != Vector3.zero)
            {
                children.Add(new ChildData {
                    transform = child,
                    direction = child.localPosition.normalized
                });
            }
        }
    }

    void Update()
    {
        // 1. Handle Global Rotation
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // 2. Handle Spatial Pulse (D)
        // Math: D = base + (sin(time) * amount)
        float currentD = baseDistance + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);

        foreach (var child in children)
        {
            child.transform.localPosition = child.direction * currentD;
        }
    }
}