using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using FMOD.Studio;
using FMODUnity;

public class IntegrationSceneInitializer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float startExposure = 15f; 
    [SerializeField] private float targetExposure = 0f;
    [SerializeField] private float transitionDuration = 5f; 
    [SerializeField] private float pauseDuration = 1.5f; 

    [Header("Transition Out")]
    [SerializeField] private string nextSceneName = "Scene9_Credits";

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile.TryGet(out colorAdjustments))
        {
            StartCoroutine(MainSequence());
        }
    }

    private IEnumerator MainSequence()
    {
        // 1. Initial Fade In
        yield return StartCoroutine(FadeInRoutine());

        // 2. Start Dialogue Sequence
        yield return StartCoroutine(PlayDialogueSequence());

        // 3. Final Scene Transition
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(isInReverse: false, fadeDurationOverride: 4f);
            yield return new WaitForSeconds(4.5f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator PlayDialogueSequence()
    {
        if (FMODEvents.instance == null) yield break;

        // --- PHASE 1: ORIENTATION ---
        yield return StartCoroutine(PlayLine(0)); // Welcome back.
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(1)); // Take your time.
        yield return StartCoroutine(PlayLine(2)); // You are here.
        yield return new WaitForSeconds(3f);      // Breathe/Look around
        yield return StartCoroutine(PlayLine(3)); // You’ve travelled far.
        yield return StartCoroutine(PlayLine(4)); // And you’ve returned.
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 2: REFLECTION ---
        yield return StartCoroutine(PlayLine(5)); // What you experienced...
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(6)); // No need to understand...
        yield return StartCoroutine(PlayLine(7)); // Begin to notice...
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(PlayLine(8)); // What felt important?
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 3: MIRRORING ---
        yield return StartCoroutine(PlayLine(9));  // Sometimes these experiences...
        yield return StartCoroutine(PlayLine(10)); // ...long time.
        yield return StartCoroutine(PlayLine(11)); // Memories...
        yield return StartCoroutine(PlayLine(12)); // Does not need to be feared.
        yield return StartCoroutine(PlayLine(13)); // Can be understood.
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 4: CONNECTING ---
        yield return StartCoroutine(PlayLine(14)); // What matters now...
        yield return StartCoroutine(PlayLine(15)); // How you choose...
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(16)); // Something you want to change?
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(17)); // Reconnect with?
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 5: GROUNDING ---
        yield return StartCoroutine(PlayLine(18)); // Path continues...
        yield return StartCoroutine(PlayLine(19)); // What you carry forward...
        yield return StartCoroutine(PlayLine(20)); // Don’t have to do all at once.
        yield return StartCoroutine(PlayLine(21)); // Just one step.
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 6: ENDING ---
        yield return StartCoroutine(PlayLine(22)); // You can return.
        yield return StartCoroutine(PlayLine(23)); // Walk in beauty.
    }

    private IEnumerator PlayLine(int index)
    {
        // Safety check to ensure the array has the line
        if (index >= FMODEvents.instance.integrationLines.Length) yield break;
        
        EventReference reference = FMODEvents.instance.integrationLines[index];
        if (reference.IsNull) yield break;

        EventInstance instance = RuntimeManager.CreateInstance(reference);
        instance.start();

        // Wait for the dialogue to finish before continuing the script
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPED)
        {
            instance.getPlaybackState(out state);
            yield return null;
        }

        instance.release();
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0;
        colorAdjustments.postExposure.value = startExposure;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, targetExposure, elapsed / transitionDuration);
            yield return null;
        }
        colorAdjustments.postExposure.value = targetExposure;
    }
}