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
    public float attractionSpeed;   // Snap speed to the shape
    public float stickForce;        // "Glue" strength to the surface
    public float turbulence;        // Vibration/Noise
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

    // VFX Graph Property Keys - Check your Graph to ensure these match exactly!
    private const string KEY_SDF = "SDF";
    private const string KEY_ATTR_SPEED = "AttractionSpeed";
    private const string KEY_STICK_FORCE = "StickForce";
    private const string KEY_TURBULENCE = "TurbulenceIntensity"; // Based on your screenshot

    public void StartEgoDeath()
    {
        if (sequence.Count > 0 && vfxGraph != null)
        {
            StartCoroutine(EgoDeathConducter());
        }
    }

    private IEnumerator EgoDeathConducter()
    {
        // 1. Morph through the shapes (Hand -> DNA -> Loom)
        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(MorphToSDF(sequence[i]));
            yield return new WaitForSeconds(sequence[i].duration);
        }

        // 2. Shatter the form into the void
        yield return StartCoroutine(ShatterEgo());

        // 3. Final Zoom into the Overview Effect
        yield return StartCoroutine(StartEarthZoom());
    }

    private IEnumerator MorphToSDF(SDFSequenceStep step)
    {
        // Swap the texture immediately
        vfxGraph.SetTexture(KEY_SDF, step.sdfTexture);

        float elapsed = 0;
        float startAttr = vfxGraph.GetFloat(KEY_ATTR_SPEED);
        float startStick = vfxGraph.GetFloat(KEY_STICK_FORCE);
        float startTurb = vfxGraph.GetFloat(KEY_TURBULENCE);

        while (elapsed < morphDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / morphDuration;
            float ease = Mathf.SmoothStep(0, 1, t);

            vfxGraph.SetFloat(KEY_ATTR_SPEED, Mathf.Lerp(startAttr, step.attractionSpeed, ease));
            vfxGraph.SetFloat(KEY_STICK_FORCE, Mathf.Lerp(startStick, step.stickForce, ease));
            vfxGraph.SetFloat(KEY_TURBULENCE, Mathf.Lerp(startTurb, step.turbulence, ease));

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

    private IEnumerator StartEarthZoom()
    {
        float elapsed = 0;
        Vector3 startScale = earthTransform.localScale;
        Vector3 startPos = earthTransform.position;
        // Push the Earth forward away from the player
        Vector3 targetPos = startPos + (earthTransform.forward * earthPushDistance);

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            float ease = Mathf.SmoothStep(0, 1, t);

            // Shrink Earth to a marble while pushing it away
            earthTransform.localScale = Vector3.Lerp(startScale, Vector3.one * earthTargetScale, ease);
            earthTransform.position = Vector3.Lerp(startPos, targetPos, ease);

            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) StartEgoDeath();
    }
}