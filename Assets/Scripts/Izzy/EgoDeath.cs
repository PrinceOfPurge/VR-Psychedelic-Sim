using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using FronkonGames.Weird.Crystal;
using FronkonGames.Weird.Kaleidoscope;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct SDFSequenceStep
{
    public string label;            
    public Texture3D sdfTexture;    
    public float duration;          // How long to stay in this form
    [GradientUsage(true)]
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
    [SerializeField] private Volume globalVolume;
    
    [Header("Warp Drive Settings")]
    [SerializeField] private VisualEffect warpVFX;
    [SerializeField] private Material warpShaderMat;
    [SerializeField] private float warpLaunchDuration = 3f;
    [SerializeField] private float warpCoolDownDuration = 2f;
    [SerializeField] private float maxWarpSpeed = 1f;
    [SerializeField] private float minVignetteIntensity = 0.2f;
    [SerializeField] private float maxVignetteIntensity = 0.45f;

    [Header("Transition Settings")]
    [SerializeField] private List<SDFSequenceStep> sequence;
    [SerializeField] private float morphDuration = 4f;
    [SerializeField] private int handDissolveInjectionPoint = 1;

    [Header("The Final Shatter (Ego Dissolution)")] 
    [SerializeField] private float shatterDuration = 8f;
    [SerializeField] private float shatterTurbulence = 60f;
    [SerializeField, GradientUsage(true)] private Gradient finalShatterGradient;
    [SerializeField] private AnimationCurve crystalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Earth Overview Effect")]
    [SerializeField] private float earthTargetScale = 0.02f;
    [SerializeField] private float earthPushDistance = 100f;
    [SerializeField] private Vector3 earthPushDirection = new Vector3(0, 0, 1);
    
    [Header("Organic Zoom Settings")]
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float floatFrequency = 0.15f; // Very slow "drift"
    [SerializeField] private float floatAmplitude = 0.5f;   // Subtle "float" movement

    // VFX Graph Property Keys - Check your Graph to ensure these match exactly!
    private const string KEY_SDF = "SDF";
    private const string KEY_COLOR = "GradientColor";
    private const string KEY_ATTR_SPEED = "AttractionSpeed";
    private const string KEY_STICK_FORCE = "StickForce";
    private const string KEY_TURBULENCE = "TurbulenceIntensity";
    private const string KEY_SPAWN_INTENSITY = "SpawnIntensity"; // NEW: Control shape visibility
    private const string KEY_WARP_SPEED = "WarpSpeed";
    
    private Gradient currentRuntimeGradient = new Gradient();
    private Gradient lastStepGradient;
    private CrystalVolume crystalComp;
    private KaleidoscopeVolume kaleidoscopeComp;
    private Vignette vignetteComp;
    private Vector3 earthAnchorPos;

    private void Start()
    {
        CacheVolumeComponents();
        if (vignetteComp != null)
        {
            vignetteComp.intensity.value = minVignetteIntensity;
        }
        
        if (sequence.Count > 0) lastStepGradient = sequence[0].colorGradient;
        if (vfxGraph != null) vfxGraph.SetFloat(KEY_SPAWN_INTENSITY, 0f);
        earthAnchorPos = earthTransform.position;
    }
    
    private void Update()
    {
        // Debug call
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(WarpLaunchSequence());
        
        // 1. Calculate the drift offset
        Vector3 drift = new Vector3(
            Mathf.Sin(Time.time * floatFrequency) * floatAmplitude,
            Mathf.Cos(Time.time * floatFrequency * 0.8f) * floatAmplitude,
            Mathf.Sin(Time.time * floatFrequency * 0.5f) * (floatAmplitude * 0.5f) // Added Z-drift for 3D depth
        );

        // 2. Apply position: Anchor + Drift
        earthTransform.position = earthAnchorPos + drift;
    }
    
    private void CacheVolumeComponents()
    {
        /*
        if (globalVolume != null && globalVolume.profile.TryGet(out CrystalVolume crystal))
        {
            crystalComp = crystal;
        }
        
        if (globalVolume != null && globalVolume.profile.TryGet(out KaleidoscopeVolume kal))
        {
            kaleidoscopeComp = kal;
        }
        
        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette vig))
        {
            vignetteComp = vig;
        }
        */
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out crystalComp);
            globalVolume.profile.TryGet(out kaleidoscopeComp);
            globalVolume.profile.TryGet(out vignetteComp);
        }
    }
    
    public IEnumerator WarpLaunchSequence()
    {
        float elapsed = 0;
        Vector3 startAnchor = earthAnchorPos; // Store where we started
        Vector3 startScale = earthTransform.localScale;
        Vector3 targetAnchor = startAnchor + (earthPushDirection * earthPushDistance);

        // --- PHASE 1: Launch (Ramp Up + Move Earth + Increase Vignette) ---
        while (elapsed < warpLaunchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / warpLaunchDuration;
            float ease = zoomCurve.Evaluate(t);

            // Speeding up
            float currentWarp = Mathf.Lerp(0, maxWarpSpeed, ease);
            warpVFX.SetFloat(KEY_WARP_SPEED, currentWarp);
            warpShaderMat.SetFloat($"_{KEY_WARP_SPEED}", currentWarp);
            
            // Increasing vignette
            if (vignetteComp != null)
            {
                vignetteComp.intensity.overrideState = true;
                vignetteComp.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, ease);
            }

            // Pushing Earth (anchor)
            earthAnchorPos = Vector3.Lerp(startAnchor, targetAnchor, ease);
            earthTransform.localScale = Vector3.Lerp(startScale, Vector3.one * earthTargetScale, ease);
            
            yield return null;
        }

        // --- PHASE 2: Cool Down (Ramp Down to Stillness) ---
        elapsed = 0;
        while (elapsed < warpCoolDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / warpCoolDownDuration;
            float ease = Mathf.SmoothStep(0, 1, t);

            // Slowing down back to 0
            float currentWarp = Mathf.Lerp(maxWarpSpeed, 0, ease);
            warpVFX.SetFloat(KEY_WARP_SPEED, currentWarp);
            warpShaderMat.SetFloat($"_{KEY_WARP_SPEED}", currentWarp);
            
            // Default vignette
            if (vignetteComp != null)
            {
                vignetteComp.intensity.value = Mathf.Lerp(maxVignetteIntensity, minVignetteIntensity, ease);
            }

            yield return null;
        }

        // --- PHASE 3: Ego Death Starts ---
        StartEgoDeath();
    }
    
    private void StartEgoDeath()
    {
        if (sequence.Count > 0)
        {
            // Initialize the "last" gradient so the first transition has a starting point
            lastStepGradient = sequence[0].colorGradient;
            StartCoroutine(EgoDeathConductor());
        }
    }

    private IEnumerator EgoDeathConductor()
    {
        // Slowly fade in the shape particles (vfxGraph) now that Earth is gone
        StartCoroutine(FadeInShapeParticles(2f));

        for (int i = 0; i < sequence.Count; i++)
        {
            if (i == handDissolveInjectionPoint) HandDissolver.Instance.StartHandDissolve();
            
            yield return StartCoroutine(MorphToSDF(sequence[i]));
            yield return new WaitForSeconds(sequence[i].duration);
            lastStepGradient = sequence[i].colorGradient;
        }

        yield return StartCoroutine(ShatterEgo());
    }
    
    private IEnumerator FadeInShapeParticles(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            vfxGraph.SetFloat(KEY_SPAWN_INTENSITY, elapsed / duration);
            yield return null;
        }
    }

    private IEnumerator MorphToSDF(SDFSequenceStep step)
    {
        // 1. Swap Texture and Gradient immediately
        vfxGraph.SetTexture(KEY_SDF, step.sdfTexture);

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

            // 1. Smoothly Lerp Gradients
            LerpGradients(lastStepGradient, step.colorGradient, ease);
            vfxGraph.SetGradient(KEY_COLOR, currentRuntimeGradient);

            // 2. Lerp Other Params
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
    
        // Use the Earth's forward vector to push it away
        Vector3 targetPos = startPos + (earthTransform.forward * earthPushDistance);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 1. Evaluate the Organic Curve
            // Design this curve in the Inspector to have a long "tail" at the end
            float ease = zoomCurve.Evaluate(t);

            // 2. The Push & Scale
            earthTransform.localScale = Vector3.Lerp(startScale, Vector3.one * earthTargetScale, ease);
        
            // 3. Add a "Float" Offset
            // This adds a tiny bit of Sine-wave drift so the Earth isn't on a perfect laser-line
            Vector3 drift = new Vector3(
                Mathf.Sin(Time.time * floatFrequency) * floatAmplitude,
                Mathf.Cos(Time.time * floatFrequency * 0.8f) * floatAmplitude,
                0
            );

            earthTransform.position = Vector3.Lerp(startPos, targetPos, ease) + drift;

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
            float ease = t * t; // Use exponential for a "snap" dissolve

            // Lerp to the final "Ascension" color
            LerpGradients(lastStepGradient, finalShatterGradient, t);
            vfxGraph.SetGradient(KEY_COLOR, currentRuntimeGradient);
            
            // Zero out the ego (forces) and maximize the chaos (turbulence)
            vfxGraph.SetFloat(KEY_ATTR_SPEED, Mathf.Lerp(startAttr, 0f, ease));
            vfxGraph.SetFloat(KEY_STICK_FORCE, Mathf.Lerp(startStick, 0f, ease));
            vfxGraph.SetFloat(KEY_TURBULENCE, Mathf.Lerp(startTurb, shatterTurbulence, t));
            
            // Ramp Weird Post-Processing
            if (crystalComp != null)
            {
                crystalComp.intensity.overrideState = true;
                crystalComp.intensity.value = crystalCurve.Evaluate(t);
            }
            
            if (kaleidoscopeComp != null)
            {
                kaleidoscopeComp.intensity.overrideState = true;
                kaleidoscopeComp.intensity.value = crystalCurve.Evaluate(t);
            }

            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        if (crystalComp != null)
        {
            crystalComp.intensity.overrideState = true;
            crystalComp.intensity.value = 0f;
        }
    }


    /// <summary>
    /// Manually blends two gradients by sampling 5 keys.
    /// </summary>
    private void LerpGradients(Gradient a, Gradient b, float t)
    {
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];

        for (int i = 0; i < 5; i++)
        {
            float sampleTime = i * 0.25f; // Samples at 0, 0.25, 0.5, 0.75, 1
            colorKeys[i].color = Color.Lerp(a.Evaluate(sampleTime), b.Evaluate(sampleTime), t);
            colorKeys[i].time = sampleTime;

            alphaKeys[i].alpha = Mathf.Lerp(a.Evaluate(sampleTime).a, b.Evaluate(sampleTime).a, t);
            alphaKeys[i].time = sampleTime;
        }

        currentRuntimeGradient.SetKeys(colorKeys, alphaKeys);
    }
}