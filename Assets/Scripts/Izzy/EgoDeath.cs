using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public struct SDFSequenceStep
{
    public string label;            
    public Texture3D sdfTexture;    
    public float duration;          // How long to stay in this form
    public Gradient colorGradient;  // HDR Gradient for this phase
    public float attractionSpeed;   // Snap speed to the shape
    public float stickForce;        // "Glue" strength to the surface
    public float turbulence;        // Vibration/Noise
    public float vfxScale;          // Scale the VFX for "Envelopment"
}

public class EgoDeath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect vfxGraph;
    [SerializeField] private Transform earthTransform;

    [Header("Transition Settings")]
    [SerializeField] private List<SDFSequenceStep> sequence;
    [SerializeField] private float morphDuration = 4f; 

    [Header("The Final Shatter (Ego Dissolution)")]
    [SerializeField] private float shatterDuration = 8f;
    [SerializeField] private float shatterTurbulence = 60f;

    [Header("Earth Overview Effect")]
    [SerializeField] private float earthTargetScale = 0.02f;
    [SerializeField] private float earthPushDistance = 100f;
    [SerializeField] private float zoomDuration = 15f;
    [SerializeField] private float zoomStartAtStepIndex = 1; // 0 = Hand, 1 = DNA, etc.

    // VFX Graph Property Keys - Check your Graph to ensure these match exactly!
    private const string KEY_SDF = "SDF";
    private const string KEY_COLOR = "GradientColor";
    private const string KEY_ATTR_SPEED = "AttractionSpeed";
    private const string KEY_STICK_FORCE = "StickForce";
    private const string KEY_TURBULENCE = "TurbulenceIntensity"; // Based on your screenshot

    public void StartEgoDeath()
    {
        StartCoroutine(EgoDeathConductor());
    }

    private IEnumerator EgoDeathConductor()
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            // Trigger Earth Zoom partway through the sequence
            if (i == zoomStartAtStepIndex) 
                StartCoroutine(StartEarthZoom(sequence[i].duration + morphDuration));

            yield return StartCoroutine(MorphToSDF(sequence[i]));
            yield return new WaitForSeconds(sequence[i].duration);
        }

        yield return StartCoroutine(ShatterEgo());
    }

    private IEnumerator MorphToSDF(SDFSequenceStep step)
    {
        // 1. Swap Texture and Gradient immediately
        vfxGraph.SetTexture(KEY_SDF, step.sdfTexture);
        vfxGraph.SetGradient(KEY_COLOR, step.colorGradient);

        float elapsed = 0;
        float startAttr = vfxGraph.GetFloat(KEY_ATTR_SPEED);
        float startStick = vfxGraph.GetFloat(KEY_STICK_FORCE);
        float startTurb = vfxGraph.GetFloat(KEY_TURBULENCE);
        Vector3 startScale = vfxGraph.transform.localScale;

        while (elapsed < morphDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / morphDuration;
            float ease = Mathf.SmoothStep(0, 1, t);

            vfxGraph.SetFloat(KEY_ATTR_SPEED, Mathf.Lerp(startAttr, step.attractionSpeed, ease));
            vfxGraph.SetFloat(KEY_STICK_FORCE, Mathf.Lerp(startStick, step.stickForce, ease));
            vfxGraph.SetFloat(KEY_TURBULENCE, Mathf.Lerp(startTurb, step.turbulence, ease));
            vfxGraph.transform.localScale = Vector3.Lerp(startScale, Vector3.one * step.vfxScale, ease);

            yield return null;
        }
    }

    private IEnumerator StartEarthZoom(float duration)
    {
        float elapsed = 0;
        Vector3 startScale = earthTransform.localScale;
        Vector3 startPos = earthTransform.position;
        Vector3 targetPos = startPos + (Vector3.forward * earthPushDistance);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ease = Mathf.SmoothStep(0, 1, t);

            earthTransform.localScale = Vector3.Lerp(startScale, Vector3.one * earthTargetScale, ease);
            earthTransform.position = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }
    }

    private IEnumerator ShatterEgo()
    {
        float elapsed = 0;
        float startAttr = vfxGraph.GetFloat(KEY_ATTR_SPEED);
        float startStick = vfxGraph.GetFloat(KEY_STICK_FORCE);
        float startTurb = vfxGraph.GetFloat(KEY_TURBULENCE);

        while (elapsed < shatterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shatterDuration;

            // Kill the "Self" (Forces) and explode into "Chaos" (Turbulence)
            // Using t * t for an exponential "snap" at the end
            vfxGraph.SetFloat(KEY_ATTR_SPEED, Mathf.Lerp(startAttr, 0f, t * t));
            vfxGraph.SetFloat(KEY_STICK_FORCE, Mathf.Lerp(startStick, 0f, t * t));
            vfxGraph.SetFloat(KEY_TURBULENCE, Mathf.Lerp(startTurb, shatterTurbulence, t));

            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) StartEgoDeath();
    }
}