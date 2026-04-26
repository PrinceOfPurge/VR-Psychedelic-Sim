using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using FronkonGames.Weird.Crystal;
using FronkonGames.Weird.Kaleidoscope;
using FronkonGames.Weird.Extruder;
using FronkonGames.Weird.Spiral;
using Haze.Runtime;
using UnityEngine.Rendering.Universal;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    public Material fadeMaterial;
    public float fadeDuration = 2.0f;
    public string fadePropertyName = "_MasterFade";
    
    [Header("Trippy Sequence Settings")]
    public float fogDuration = 5f;
    public float targetFogDensity = 30f;
    public float ppSequenceDuration = 30f;
    public List<PPSequenceStep> ppSteps;
    
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
            Destroy(this);
            return;
        }

        fadePropID = Shader.PropertyToID(fadePropertyName);
        ResetFadeEffect();
    }

    #region Basic Fade (Locksmith/Hospital)
    
    /// <param name="isInReverse">False = 0 to 1 (Blackout). True = 1 to 0 (Wake up).</param>
    /// <param name="nextScene">Leave empty if just fading in/out within the same scene.</param>
    /// <param name="useBlink">If true, adds the 'eyelid' snap math for waking up.</param>
    /// <param name="fadeDurationOverride">Overrides the transition duration</param>
    public void PerformFade(bool isInReverse = false, string nextScene = "", bool useBlink = false, float fadeDurationOverride = 0f)
    {
        if (fadeDurationOverride != 0) fadeDuration = fadeDurationOverride;
        StartCoroutine(FadeRoutine(isInReverse, nextScene, useBlink));
    }

    private IEnumerator FadeRoutine(bool isInReverse, string nextScene, bool useBlink)
    {
        float elapsed = 0f;
        float startVol = AudioManager.instance != null ? AudioManager.instance.masterVolume : 1f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            
            // MATH: Determine base T (0->1 or 1->0)
            float t = isInReverse ? 1f - normalizedTime : normalizedTime;

            // BLINK LOGIC: Only applies if waking up (isInReverse)
            if (isInReverse && useBlink)
            {
                // Simple Blink Math: Oscillates quickly at start, slows down
                // If sin is negative, we force T to 1 (Black)
                float blink = Mathf.Sin(elapsed * 12f); 
                if (blink < 0 && normalizedTime < 0.6f) t = 1f; 
            }

            // Apply to Material (Primary) and Global (Fallback)
            if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, t);
            Shader.SetGlobalFloat(fadePropID, t);

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
        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, finalT);
        Shader.SetGlobalFloat(fadePropID, finalT);

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    public void ResetBasicFade()
    {
        // Apply to Material (Primary) and Global (Fallback)
        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, 0);
        Shader.SetGlobalFloat(fadePropID, 0);
    }
    
    #endregion

    #region Trippy Transition (Hogan to Starweaver)
    
    public void StartTrippyEffects(Volume sceneVolume, string nextScene = "", float peakWaitTime = 0f)
    {
        currentTripVolume = sceneVolume;
        CacheComponents(currentTripVolume);
        
        UpdatePPStepIntensities(0f); 
        //SetFogDensity(currentTripVolume, 0f);
        
        if (activeTripRoutine != null) StopCoroutine(activeTripRoutine);
        activeTripRoutine = StartCoroutine(TrippyAscentRoutine(nextScene, peakWaitTime));
    }

    // Update EndTrippyEffects to accept the "Peak" volume from the current scene
    public void EndTrippyEffects(Volume localPeakVolume, Volume starweaverVolume, float duration = 5f)
    {
        // RE-SYNC: Update the reference to the local duplicate in the new scene
        currentTripVolume = localPeakVolume;
        CacheComponents(currentTripVolume); // Re-link the Kaleidoscope/Fog components

        if (activeTripRoutine != null) StopCoroutine(activeTripRoutine);
        activeTripRoutine = StartCoroutine(TrippyDescentRoutine(starweaverVolume, duration));
    }

    private IEnumerator TrippyAscentRoutine(string nextScene, float peakWaitTime)
    {
        // Phase 1: Fog
        float elapsed = 0f;
        while (elapsed < fogDuration)
        {
            elapsed += Time.deltaTime;
            SetFogDensity(currentTripVolume, (elapsed / fogDuration) * targetFogDensity);
            yield return null;
        }

        // Phase 2: Weird PP Sequence
        elapsed = 0f;
        while (elapsed < ppSequenceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ppSequenceDuration;
            UpdatePPStepIntensities(t);
            yield return null;
        }
        
        // Phase 3: Hold at Peak Intensity
        // This allows the player to soak in the "peak" visuals before the load
        yield return new WaitForSeconds(peakWaitTime);

        // Phase 4: Trigger Scene Load
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
            float t = elapsed / duration; // t goes 0 to 1

            // 1. SYNCED FOG: Ramp fog down from its target density to 0 
            // using the same 't' as the post-processing sequence.
            float currentFogT = 1f - t;
            SetFogDensity(currentTripVolume, currentFogT * targetFogDensity);

            // 2. PP SEQUENCE: Ramp intensities down (1 to 0)
            UpdatePPStepIntensities(currentFogT);

            // 3. CROSS-FADE VOLUMES:
            if (currentTripVolume != null) currentTripVolume.weight = 1f - t;
            //if (starweaverVolume != null) starweaverVolume.weight = t;
            if (starweaverVolume != null) starweaverVolume.weight = 1f; // Snapping this to 1 should be okay

            yield return null;
        }
    
        // Final cleanup to ensure everything is absolute zero
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
    
    #region Exposure Transition (Starweaver to integration)
    
    /// <summary>
    /// Transitions exposure to white, loads a scene, and resets exposure.
    /// </summary>
    public void PerformEgoDeathTransition(Volume targetVolume, float transitionDuration, float waitDuration, string nextSceneName)
    {
        StartCoroutine(EgoDeathRoutine(targetVolume, transitionDuration, waitDuration, nextSceneName));
    }

    private IEnumerator EgoDeathRoutine(Volume targetVolume, float duration, float waitDuration, string nextSceneName)
    {
        if (targetVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            float startExposure = colorAdjustments.postExposure.value;
            float targetExposure = 30f; // Blown out white
            float elapsed = 0;

            // 1. Ramp Up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, targetExposure, elapsed / duration);
                yield return null;
            }
            
            colorAdjustments.postExposure.value = targetExposure;

            // 2. The "Void" - wait in the white-out
            yield return new WaitForSeconds(waitDuration);

            // 3. Trigger Load (Simple Load)
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No ColorAdjustments found on the passed Volume!");
            SceneManager.LoadScene(nextSceneName); // Fallback load
        }
    }
    
    #endregion
    
    
    #region Helpers (Reflection/Cleanup)
    
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