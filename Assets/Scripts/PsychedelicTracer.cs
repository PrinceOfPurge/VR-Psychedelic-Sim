using UnityEngine;
using System.Collections.Generic;

public class PsychedelicTracer : MonoBehaviour
{
    [Header("References")]
    public Transform[] anchorPoints; // Assign Fingertips, Knuckles, and Wrist here
    public Material trailMaterial;  // Use an Additive or Alpha Blended shader

    [Header("Fluidity Settings")]
    public float trailTime = 0.3f;      // How long the "smear" stays
    public float minVertexDistance = 0.01f; // Smoothness of the curve
    public float startWidth = 0.05f;
    public float endWidth = 0.0f;

    [Header("Movement Check")]
    public float velocityThreshold = 0.05f; // Only show if moving faster than this
    
    private List<TrailRenderer> trails = new List<TrailRenderer>();
    private Vector3 lastPosition;

    void Start()
    {
        // Setup a trail for every anchor point (fingers, etc.)
        foreach (Transform anchor in anchorPoints)
        {
            GameObject trailObj = new GameObject("Tracer_Ribbon");
            trailObj.transform.SetParent(anchor);
            trailObj.transform.localPosition = Vector3.zero;

            TrailRenderer tr = trailObj.AddComponent<TrailRenderer>();
            
            // Visual Setup
            tr.time = trailTime;
            tr.minVertexDistance = minVertexDistance;
            tr.startWidth = startWidth;
            tr.endWidth = endWidth;
            tr.material = trailMaterial;
            
            // Gradient (Constant color, fading out)
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.2f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            tr.colorGradient = gradient;

            trails.Add(tr);
        }
        
        lastPosition = transform.position;
    }

    void Update()
    {
        // 1. Calculate Velocity
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // 2. Minimum Movement Check
        // If moving slow, we stop emitting to prevent "glow blobs"
        bool shouldEmit = speed > velocityThreshold;

        foreach (TrailRenderer tr in trails)
        {
            tr.emitting = shouldEmit;
        }
    }
}