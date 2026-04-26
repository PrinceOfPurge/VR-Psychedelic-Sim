using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using FronkonGames.Weird.Crystal;
using FronkonGames.Weird.Kaleidoscope;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[Serializable]
public struct SDFSequenceStep
{
    public string label;            
    public Texture3D sdfTexture;
    public Vector3 sdfSize;         // <-- ADD THIS: Match the 'Size' from the Bake Tool
    public Vector3 sdfCenter;       // <-- ADD THIS: Match the 'Center' from the Bake Tool
    public float duration;          
    [GradientUsage(true)]
    public Gradient colorGradient;  
    public float attractionSpeed;   
    public float stickForce;        
    public float turbulence;        
    public float vfxScale;          
}

public class EgoDeath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect vfxGraph;
    [SerializeField] private Transform earthTransform;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private string nextSceneName;
    
    [Header("Warp Drive Settings")]
    [SerializeField] private VisualEffect warpVFX;
    [SerializeField] private Material warpShaderMat;
    [SerializeField] private float warpLaunchDuration = 3f;
    [SerializeField] private float warpCoolDownDuration = 2f;
    [SerializeField] private float maxWarpSpeed = 1f;
    [SerializeField] private float minVignetteIntensity = 0.2f;
    [SerializeField] private float maxVignetteIntensity = 0.45f;
    [SerializeField] private FMODUnity.EventReference warpZoomSFX; // <-- NEW SFX SLOT HERE

    [Header("Transition Settings")]
    [SerializeField] private List<SDFSequenceStep> sequence;
    [SerializeField] private float morphDuration = 4f;
    [SerializeField] private int handDissolveInjectionPoint = 1;

    [Header("The Final Shatter (Ego Dissolution)")] 
    [SerializeField] private float shatterDuration = 8f;
    [SerializeField] private float shatterTurbulence = 60f;
    [SerializeField, GradientUsage(true)] private Gradient finalShatterGradient;
    [SerializeField] private AnimationCurve crystalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float finalTransitionDuration = 3f;
    [SerializeField] private float finalTransitionWaitTime = 2f;
    
    [Header("Earth Overview Effect")]
    [SerializeField] private float earthTargetScale = 0.02f;
    [SerializeField] private float earthPushDistance = 100f;
    [SerializeField] private Vector3 earthPushDirection = new Vector3(0, 0, 1);
    
    [Header("Organic Zoom Settings")]
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float floatFrequency = 0.15f; 
    [SerializeField] private float floatAmplitude = 0.5f;   

    // Constants for VFX Graph Properties
    private const string KEY_SDF = "SDF";
    private const string KEY_COLOR = "GradientColor";
    private const string KEY_ATTR_SPEED = "AttractionSpeed";
    private const string KEY_STICK_FORCE = "StickForce";
    private const string KEY_TURBULENCE = "TurbulenceIntensity";
    private const string KEY_SPAWN_INTENSITY = "SpawnIntensity"; 
    private const string KEY_WARP_SPEED = "WarpSpeed";
    private const string KEY_SDF_SIZE = "SDFSize";
    private const string KEY_SDF_CENTER = "SDFCenter";
    
    private Gradient currentRuntimeGradient = new Gradient();
    private Gradient lastStepGradient;
    private CrystalVolume crystalComp;
    private KaleidoscopeVolume kaleidoscopeComp;
    private Vignette vignetteComp;
    private ChromaticAberration chromaticComp;
    private Vector3 earthAnchorPos;

    private bool egoDeathActive = false;

    private void Start()
    {
        CacheVolumeComponents();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.CreateInstance(FMODEvents.instance.KaleidoscopeMusic).start();
        }

        if (vignetteComp != null) vignetteComp.intensity.value = minVignetteIntensity;
        if (sequence.Count > 0) lastStepGradient = sequence[0].colorGradient;
        
        SafeSetFloat(KEY_SPAWN_INTENSITY, 0f);
        earthAnchorPos = earthTransform.position;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !egoDeathActive) 
        {
            StartCoroutine(WarpLaunchSequence());
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            StopAllCoroutines();
            StartCoroutine(SkipToSkullSequence());
        }

        Vector3 drift = new Vector3(
            Mathf.Sin(Time.time * floatFrequency) * floatAmplitude,
            Mathf.Cos(Time.time * floatFrequency * 0.8f) * floatAmplitude,
            Mathf.Sin(Time.time * floatFrequency * 0.5f) * (floatAmplitude * 0.5f)
        );

        earthTransform.position = earthAnchorPos + drift;
    }

    // ==========================================
    // SAFE VFX GRAPH SETTERS (Prevents Errors)
    // ==========================================
    private float SafeGetFloat(string key, float fallback = 0f)
    {
        if (vfxGraph != null && vfxGraph.HasFloat(key)) return vfxGraph.GetFloat(key);
        return fallback;
    }

    private void SafeSetFloat(string key, float value)
    {
        if (vfxGraph != null && vfxGraph.HasFloat(key)) vfxGraph.SetFloat(key, value);
    }

    private void SafeSetTexture(string key, Texture texture)
    {
        if (vfxGraph != null && vfxGraph.HasTexture(key) && texture != null) vfxGraph.SetTexture(key, texture);
    }

    private void SafeSetGradient(string key, Gradient gradient)
    {
        if (vfxGraph != null && vfxGraph.HasGradient(key) && gradient != null) vfxGraph.SetGradient(key, gradient);
    }
    
    private void SafeSetVector3(string key, Vector3 value)
    {
        if (vfxGraph != null && vfxGraph.HasVector3(key)) vfxGraph.SetVector3(key, value);
    }

    private Vector3 SafeGetVector3(string key, Vector3 fallback = default)
    {
        if (vfxGraph != null && vfxGraph.HasVector3(key)) return vfxGraph.GetVector3(key);
        return fallback;
    }

    // ==========================================
    // SEQUENCES
    // ==========================================
    private IEnumerator SkipToSkullSequence()
    {
        egoDeathActive = true;
        SafeSetFloat(KEY_SPAWN_INTENSITY, 1f);

        if (sequence.Count > 2)
        {
            yield return StartCoroutine(MorphToSDF(sequence[2]));
            yield return StartCoroutine(HandleDialogueSequence(2));
        }
        else
        {
            Debug.LogError("Sequence list needs at least 3 elements for the Skull phase to trigger!");
        }

        yield return StartCoroutine(ShatterEgo());
    }
    
    public IEnumerator WarpLaunchSequence()
    {
        egoDeathActive = true;

        // <-- TRIGGER NEW SFX HERE EXACTLY WHEN PUSH STARTS -->
        if (AudioManager.instance != null && !warpZoomSFX.IsNull)
        {
            AudioManager.instance.PlayOneShot(warpZoomSFX, transform.position);
        }

        float elapsed = 0;
        Vector3 startAnchor = earthAnchorPos; 
        Vector3 startScale = earthTransform.localScale;
        Vector3 targetAnchor = startAnchor + (earthPushDirection * earthPushDistance);
        
        while (elapsed < warpLaunchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / warpLaunchDuration;
            float ease = zoomCurve.Evaluate(t);
            float currentWarp = Mathf.Lerp(0, maxWarpSpeed, ease);
            
            if (warpVFX != null && warpVFX.HasFloat(KEY_WARP_SPEED)) warpVFX.SetFloat(KEY_WARP_SPEED, currentWarp);
            if (warpShaderMat != null) warpShaderMat.SetFloat($"_{KEY_WARP_SPEED}", currentWarp);
            
            if (vignetteComp != null)
                vignetteComp.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, ease);
                
            earthAnchorPos = Vector3.Lerp(startAnchor, targetAnchor, ease);
            earthTransform.localScale = Vector3.Lerp(startScale, Vector3.one * earthTargetScale, ease);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < warpCoolDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / warpCoolDownDuration;
            float ease = zoomCurve.Evaluate(1f - t);
            float currentWarp = Mathf.Lerp(0, maxWarpSpeed, ease);
            
            if (warpVFX != null && warpVFX.HasFloat(KEY_WARP_SPEED)) warpVFX.SetFloat(KEY_WARP_SPEED, currentWarp);
            if (warpShaderMat != null) warpShaderMat.SetFloat($"_{KEY_WARP_SPEED}", currentWarp);
            
            if (vignetteComp != null)
                vignetteComp.intensity.value = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, ease);
            yield return null;
        }
        
        StartCoroutine(EgoDeathConductor());
    }
    
    private IEnumerator EgoDeathConductor()
    {
        StartCoroutine(FadeInShapeParticles(2f));
        for (int i = 0; i < sequence.Count; i++)
        {
            if (i == handDissolveInjectionPoint && HandDissolver.Instance != null) 
                HandDissolver.Instance.StartHandDissolve();
                
            yield return StartCoroutine(MorphToSDF(sequence[i]));
            yield return StartCoroutine(HandleDialogueSequence(i));
            
            // Wait for any remaining duration assigned in the inspector
            yield return new WaitForSeconds(sequence[i].duration); 
            lastStepGradient = sequence[i].colorGradient;
        }
        yield return StartCoroutine(ShatterEgo());
    }

    private IEnumerator HandleDialogueSequence(int stepIndex)
    {
        var lines = FMODEvents.instance.dialogueLines;

        if (stepIndex == 0) // DNA
        {
            yield return PlayLineAndWait(lines[0], 5.0f); 
            yield return PlayLineAndWait(lines[1], 5.0f); 
        }
        else if (stepIndex == 1) // Key
        {
            yield return PlayLineAndWait(lines[2], 5.0f); 
            yield return PlayLineAndWait(lines[3], 5.0f); 
            yield return PlayLineAndWait(lines[4], 5.0f); 
        }
        else if (stepIndex == 2) // Skull
        {
            yield return PlayLineAndWait(lines[5], 6.0f); // Fear of losing...
            yield return PlayLineAndWait(lines[6], 6.0f); // But look closer...
            yield return PlayLineAndWait(lines[7], 6.0f); // You're still here...
            
            // MASSIVE BUFFER to guarantee no overlap
            yield return PlayLineAndWait(lines[8], 15.0f); 
        }
    }

    private IEnumerator PlayLineAndWait(FMODUnity.EventReference line, float duration)
    {
        if (AudioManager.instance != null && !line.IsNull)
        {
            AudioManager.instance.PlayOneShot(line, transform.position);
        }
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator ShatterEgo()
    {
        float elapsed = 0;
        float startAttr = SafeGetFloat(KEY_ATTR_SPEED);
        float startStick = SafeGetFloat(KEY_STICK_FORCE);
        float startTurb = SafeGetFloat(KEY_TURBULENCE);

        // MUTED Therapist Line 9: "When you're ready..."
        // if (AudioManager.instance != null && FMODEvents.instance.dialogueLines.Length > 9)
        //    AudioManager.instance.PlayOneShot(FMODEvents.instance.dialogueLines[9], transform.position);

        while (elapsed < shatterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shatterDuration;
            float ease = t * t; 

            LerpGradients(lastStepGradient, finalShatterGradient, t);
            SafeSetGradient(KEY_COLOR, currentRuntimeGradient);
            SafeSetFloat(KEY_ATTR_SPEED, Mathf.Lerp(startAttr, 0f, ease));
            SafeSetFloat(KEY_STICK_FORCE, Mathf.Lerp(startStick, 0f, ease));
            SafeSetFloat(KEY_TURBULENCE, Mathf.Lerp(startTurb, shatterTurbulence, t));
            
            if (chromaticComp != null) chromaticComp.intensity.value = t;
            if (crystalComp != null) crystalComp.intensity.value = crystalCurve.Evaluate(t);
            if (kaleidoscopeComp != null) kaleidoscopeComp.intensity.value = crystalCurve.Evaluate(t);
            yield return null;
        }

        // START Therapist Line 10: "Open your eyes"
        if (AudioManager.instance != null && FMODEvents.instance.dialogueLines.Length > 10)
            AudioManager.instance.PlayOneShot(FMODEvents.instance.dialogueLines[10], transform.position);
            
        // DROPPED THIS PAUSE DOWN FROM 8.0f TO 2.0f
        yield return new WaitForSeconds(2.0f);

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformEgoDeathTransition(globalVolume, finalTransitionDuration, finalTransitionWaitTime, nextSceneName);
    }

    private void CacheVolumeComponents()
    {
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out crystalComp);
            globalVolume.profile.TryGet(out kaleidoscopeComp);
            globalVolume.profile.TryGet(out vignetteComp);
            globalVolume.profile.TryGet(out chromaticComp);
        }
    }

    private IEnumerator FadeInShapeParticles(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SafeSetFloat(KEY_SPAWN_INTENSITY, elapsed / duration);
            yield return null;
        }
    }

    private IEnumerator MorphToSDF(SDFSequenceStep step)
    {
        SafeSetTexture(KEY_SDF, step.sdfTexture);
        
        float elapsed = 0;
        float startAttr = SafeGetFloat(KEY_ATTR_SPEED);
        float startStick = SafeGetFloat(KEY_STICK_FORCE);
        float startTurb = SafeGetFloat(KEY_TURBULENCE);
        
        // Capture starting bounds for the lerp
        Vector3 startSdfSize = SafeGetVector3(KEY_SDF_SIZE, Vector3.one * 3f);
        Vector3 startSdfCenter = SafeGetVector3(KEY_SDF_CENTER, Vector3.zero);
        Vector3 startScale = vfxGraph != null ? vfxGraph.transform.localScale : Vector3.one;

        while (elapsed < morphDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / morphDuration;
            float ease = Mathf.SmoothStep(0, 1, t);
            
            LerpGradients(lastStepGradient, step.colorGradient, ease);
            SafeSetGradient(KEY_COLOR, currentRuntimeGradient);
            SafeSetFloat(KEY_ATTR_SPEED, Mathf.Lerp(startAttr, step.attractionSpeed, ease));
            SafeSetFloat(KEY_STICK_FORCE, Mathf.Lerp(startStick, step.stickForce, ease));
            SafeSetFloat(KEY_TURBULENCE, Mathf.Lerp(startTurb, step.turbulence, ease));
            
            // Dynamic Bounding Box Update
            SafeSetVector3(KEY_SDF_SIZE, Vector3.Lerp(startSdfSize, step.sdfSize, ease));
            SafeSetVector3(KEY_SDF_CENTER, Vector3.Lerp(startSdfCenter, step.sdfCenter, ease));
            
            if (vfxGraph != null)
                vfxGraph.transform.localScale = Vector3.Lerp(startScale, Vector3.one * step.vfxScale, ease);
                
            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        if (crystalComp != null) { crystalComp.intensity.overrideState = true; crystalComp.intensity.value = 0f; }
    }

    private void LerpGradients(Gradient a, Gradient b, float t)
    {
        if (a == null || b == null) return;
        
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];
        for (int i = 0; i < 5; i++)
        {
            float sampleTime = i * 0.25f; 
            colorKeys[i].color = Color.Lerp(a.Evaluate(sampleTime), b.Evaluate(sampleTime), t);
            colorKeys[i].time = sampleTime;
            alphaKeys[i].alpha = Mathf.Lerp(a.Evaluate(sampleTime).a, b.Evaluate(sampleTime).a, t);
            alphaKeys[i].time = sampleTime;
        }
        currentRuntimeGradient.SetKeys(colorKeys, alphaKeys);
    }
}