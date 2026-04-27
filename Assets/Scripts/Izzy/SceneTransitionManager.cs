using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using FronkonGames.Weird.Crystal;
using FronkonGames.Weird.Kaleidoscope;
using FronkonGames.Weird.Extruder;
using FronkonGames.Weird.Spiral;
using UnityEngine.Rendering.Universal;


[Serializable]
public struct DistortedUVsInfoContainer
{
    public Material mat;
    public string shaderEffectParamName;
}

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    public Material fadeMaterial;
    public float fadeDuration = 2.0f;
    public float newSceneFadeInDuration = 2.0f; // Speed of fading in after load
    public string fadePropertyName = "_MasterFade";
    
    [Header("Trippy Sequence Settings")]
    public float fogDuration = 5f;
    public float targetFogDensity = 30f;
    public float ppSequenceDuration = 30f;
    public List<PPSequenceStep> ppSteps;
    
    private DistortedUVsInfoContainer[] activeHutMaterials;
    private int fadePropID;
    private Volume currentTripVolume;
    private Coroutine activeTripRoutine;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject); // Standard DDOL pattern: destroy the object, not just script
            return;
        }

        fadePropID = Shader.PropertyToID(fadePropertyName);
        ResetFadeEffect();
    }

    private void OnEnable()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Automatically called by Unity when a new scene is ready
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Every time we enter a new scene, start a fade-in (1 to 0)
        // Set useBlink to true if you want the "eye opening" effect on every load
        PerformFade(true, "", false, newSceneFadeInDuration);
    }

    #region Basic Fade (Locksmith/Hospital)
    
    /// <param name="isInReverse">False = 0 to 1 (Blackout). True = 1 to 0 (Wake up).</param>
    /// <param name="nextScene">Leave empty if just fading in/out within the same scene.</param>
    /// <param name="useBlink">If true, adds the 'eyelid' snap math for waking up.</param>
    /// <param name="fadeDurationOverride">Overrides the transition duration</param>
    public void PerformFade(bool isInReverse = false, string nextScene = "", bool useBlink = false, float fadeDurationOverride = 0f)
    {
        float duration = (fadeDurationOverride != 0) ? fadeDurationOverride : fadeDuration;
        StartCoroutine(FadeRoutine(isInReverse, nextScene, useBlink, duration));
    }

    private IEnumerator FadeRoutine(bool isInReverse, string nextScene, bool useBlink, float duration)
    {
        float elapsed = 0f;
        float startVol = AudioManager.instance != null ? AudioManager.instance.masterVolume : 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // MATH: Determine base T (0->1 or 1->0)
            float t = isInReverse ? 1f - normalizedTime : normalizedTime;

            // BLINK LOGIC: Only applies if waking up (isInReverse)
            if (isInReverse && useBlink)
            {
                float blink = Mathf.Sin(elapsed * 12f); 
                if (blink < 0 && normalizedTime < 0.6f) t = 1f; 
            }

            ApplyFadeValue(t);

            // Audio Ducking
            if (AudioManager.instance != null)
            {
                float targetVol = isInReverse ? Mathf.Lerp(0f, 1f, normalizedTime) : Mathf.Lerp(startVol, 0f, normalizedTime);
                AudioManager.instance.masterVolume = targetVol;
            }

            yield return null;
        }

        // Finalize state
        float finalT = isInReverse ? 0f : 1f;
        ApplyFadeValue(finalT);

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    public void ResetBasicFade() => ApplyFadeValue(0f);
    
    #endregion

    #region Trippy Transition (Hogan to Starweaver)
    
    public void StartTrippyEffects(Volume sceneVolume, string nextScene = "", float peakWaitTime = 0f)
    {
        currentTripVolume = sceneVolume;
        CacheComponents(currentTripVolume);
        UpdatePPStepIntensities(0f); 
        
        if (activeTripRoutine != null) StopCoroutine(activeTripRoutine);
        activeTripRoutine = StartCoroutine(TrippyAscentRoutine(nextScene, peakWaitTime));
    }

    public void EndTrippyEffects(Volume localPeakVolume, Volume starweaverVolume, float duration = 5f)
    {
        currentTripVolume = localPeakVolume;
        CacheComponents(currentTripVolume);

        if (activeTripRoutine != null) StopCoroutine(activeTripRoutine);
        activeTripRoutine = StartCoroutine(TrippyDescentRoutine(starweaverVolume, duration));
    }

    private IEnumerator TrippyAscentRoutine(string nextScene, float peakWaitTime)
    {
        float elapsed = 0f;
        while (elapsed < fogDuration)
        {
            elapsed += Time.deltaTime;
            SetFogDensity(currentTripVolume, (elapsed / fogDuration) * targetFogDensity);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < ppSequenceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ppSequenceDuration;
            UpdatePPStepIntensities(t);
            yield return null;
        }
        
        yield return new WaitForSeconds(peakWaitTime);

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    private IEnumerator TrippyDescentRoutine(Volume starweaverVolume, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentFogT = 1f - t;
            SetFogDensity(currentTripVolume, currentFogT * targetFogDensity);
            UpdatePPStepIntensities(currentFogT);

            if (currentTripVolume != null) currentTripVolume.weight = 1f - t;
            if (starweaverVolume != null) starweaverVolume.weight = 1f; 

            yield return null;
        }
    
        if (currentTripVolume != null) 
        {
            currentTripVolume.weight = 0f;
            SetFogDensity(currentTripVolume, 0f);
        }
        starweaverVolume.weight = 1f;
    }

    private void UpdatePPStepIntensities(float globalT)
    {
        foreach (var step in ppSteps)
        {
            if (step.component == null) continue;
            float localT = Mathf.InverseLerp(step.startAtNormalized, step.endAtNormalized, globalT);
            ApplyWeirdIntensity(step.component, step.intensityCurve.Evaluate(localT));
        }
    }
    
    #endregion
    
    #region Exposure Transition
    
    public void PerformEgoDeathTransition(Volume targetVolume, float transitionDuration, float waitDuration, string nextSceneName)
    {
        StartCoroutine(EgoDeathRoutine(targetVolume, transitionDuration, waitDuration, nextSceneName));
    }

    private IEnumerator EgoDeathRoutine(Volume targetVolume, float duration, float waitDuration, string nextSceneName)
    {
        if (targetVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            float startExposure = colorAdjustments.postExposure.value;
            float targetExposure = 30f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, targetExposure, elapsed / duration);
                yield return null;
            }
            
            colorAdjustments.postExposure.value = targetExposure;
            yield return new WaitForSeconds(waitDuration);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
    
    #endregion
    
    #region Helpers
    
    private void ApplyFadeValue(float t)
    {
        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, t);
        Shader.SetGlobalFloat(fadePropID, t);
    }

    private void CacheComponents(Volume vol)
    {
        if (vol == null || vol.profile == null) return;
        foreach (var step in ppSteps)
        {
            foreach (var comp in vol.profile.components)
            {
                if (comp.name.Contains(step.componentName)) step.component = comp;
            }
        }
    }

    private void SetFogDensity(Volume vol, float value)
    {
        foreach (var comp in vol.profile.components)
        {
            if (comp.name.Contains("Haze"))
            {
                var field = comp.GetType().GetField("globalDensityMultiplier");
                if (field != null)
                {
                    var param = field.GetValue(comp) as FloatParameter;
                    if (param != null) { param.overrideState = true; param.value = value; }
                }
            }
        }
    }

    private void ApplyWeirdIntensity(VolumeComponent comp, float value)
    {
        if (comp is KaleidoscopeVolume k) k.intensity.value = value;
        else if (comp is ExtruderVolume e) e.intensity.value = value;
        else if (comp is CrystalVolume c) c.intensity.value = value;
        else if (comp is SpiralVolume s) s.wrap.value = value;
    }

    public void ResetFadeEffect() => ApplyFadeValue(0f);
    private void OnApplicationQuit() => ResetFadeEffect();
    
    #endregion
}