using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    public Material fadeMaterial;
    public float fadeDuration = 2.0f;
    public string fadePropertyName = "_MasterFade";
    
    private int fadePropID;

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);

        fadePropID = Shader.PropertyToID(fadePropertyName);
        ResetFadeEffect();
    }

    /// <param name="isInReverse">False = 0 to 1 (Blackout). True = 1 to 0 (Wake up).</param>
    /// <param name="nextScene">Leave empty if just fading in/out within the same scene.</param>
    /// <param name="useBlink">If true, adds the 'eyelid' snap math for waking up.</param>
    /// <param name="fadeDurationOverride">Overrides the transition duration</param>
    public void PerformFade(bool isInReverse, string nextScene = "", bool useBlink = false, float fadeDurationOverride = 0f)
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

    private void OnApplicationQuit() => ResetFadeEffect();

    private void ResetFadeEffect()
    {
        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, 0f);
        Shader.SetGlobalFloat(fadePropID, 0f);
    }
}