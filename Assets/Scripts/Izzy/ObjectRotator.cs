/*
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees per second on each axis")] [SerializeField] private Vector3 rotationSpeed = new Vector3(2f, 5f, 1.5f);

    [Header("Psychedelic Pulsing")]
    [SerializeField] private bool useChaoticMotion = true;
    [SerializeField] private float pulseFrequency = 0.5f;
    [SerializeField] private float pulseIntensity = 2.0f;

    void Update()
    {
        Vector3 finalRotation = rotationSpeed;

        if (useChaoticMotion)
        {
            // Varies the speed over time so it doesn't feel like a simple loop
            float pulse = Mathf.Sin(Time.time * pulseFrequency) * pulseIntensity;
            finalRotation += new Vector3(pulse, pulse * 0.5f, pulse * 1.2f);
        }

        // Apply rotation independent of framerate
        transform.Rotate(finalRotation * Time.deltaTime);
    }
}
*/
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