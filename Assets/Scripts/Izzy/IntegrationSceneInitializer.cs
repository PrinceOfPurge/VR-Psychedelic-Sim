using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Serialization;

public class IntegrationSceneInitializer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float startExposure = 15f; 
    [SerializeField] private float targetExposure = 0f;
    [SerializeField] private float trippyTransitionDuration = 12f;
    [SerializeField] private float fadeTransitionDuration = 5f; 
    [SerializeField] private float pauseDuration = 1.5f; 
    [SerializeField] private DistortedUVsInfoContainer[] activeHutMaterials;
    
    [Header("Cloud NPCs (Talking Sync)")]
    [SerializeField] private NPCBiologicalMotion therapistCloud;
    [SerializeField] private NPCBiologicalMotion healerCloud;
    [SerializeField] private float talkingFadeDuration = 0.4f;

    [Header("Ethereal Lighting")]
    [SerializeField] private Light fireLight;
    [SerializeField] private Gradient tripColorGradient; 

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
        StartCoroutine(ClearHutDistortions(trippyTransitionDuration));
        yield return StartCoroutine(FadeInRoutine());

        yield return StartCoroutine(PlayDialogueSequence());

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
        yield return StartCoroutine(PlayLine(0, true));  // T: Welcome back.
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(1, true));  // T: Take your time.
        yield return StartCoroutine(PlayLine(2, true));  // T: You are here.
        yield return new WaitForSeconds(3f);      
        yield return StartCoroutine(PlayLine(3, false)); // H: You’ve travelled far.
        yield return StartCoroutine(PlayLine(4, false)); // H: And you’ve returned.
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 2: REFLECTION ---
        yield return StartCoroutine(PlayLine(5, true));  // T: What you experienced...
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(6, true));  // T: No need to understand...
        yield return StartCoroutine(PlayLine(7, true));  // T: Begin to notice...
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(PlayLine(8, true));  // T: What felt important?
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 3: MIRRORING ---
        yield return StartCoroutine(PlayLine(9, true));   // T: Sometimes these experiences...
        yield return StartCoroutine(PlayLine(10, true));  // T: ...long time.
        yield return StartCoroutine(PlayLine(11, true));  // T: Memories...
        yield return StartCoroutine(PlayLine(12, false)); // H: What is seen...
        yield return StartCoroutine(PlayLine(13, false)); // H: Can be understood.
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 4: CONNECTING ---
        yield return StartCoroutine(PlayLine(14, true)); // T: What matters now...
        yield return StartCoroutine(PlayLine(15, true)); // T: How you choose...
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(16, true)); // T: Something you want to change?
        yield return new WaitForSeconds(pauseDuration);
        yield return StartCoroutine(PlayLine(17, true)); // T: Reconnect with?
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 5: GROUNDING ---
        yield return StartCoroutine(PlayLine(18, false)); // H: Path continues...
        yield return StartCoroutine(PlayLine(19, false)); // H: What you carry forward...
        yield return StartCoroutine(PlayLine(20, true));  // T: Don’t have to do all at once.
        yield return StartCoroutine(PlayLine(21, true));  // T: Just one step.
        yield return new WaitForSeconds(pauseDuration);

        // --- PHASE 6: ENDING ---
        yield return StartCoroutine(PlayLine(22, true));  // T: You can return.
        yield return StartCoroutine(PlayLine(23, false)); // H: Walk in beauty.
    }

    private IEnumerator PlayLine(int index, bool isTherapist)
    {
        if (index >= FMODEvents.instance.integrationLines.Length) yield break;
        
        EventReference reference = FMODEvents.instance.integrationLines[index];
        if (reference.IsNull) yield break;

        // 1. Assign speaker
        NPCBiologicalMotion speaker = isTherapist ? therapistCloud : healerCloud;

        // 2. Start Light Glow
        if (speaker != null) StartCoroutine(FadeTalkingWeight(speaker, 1f));

        // 3. Play Audio
        EventInstance instance = RuntimeManager.CreateInstance(reference);
        instance.start();

        // 4. OLD RELIABLE: Wait for STOPPED state
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPED)
        {
            instance.getPlaybackState(out state);
            yield return null;
        }

        // 5. Stop Light Glow and Clean up
        if (speaker != null) StartCoroutine(FadeTalkingWeight(speaker, 0f));
        instance.release();
    }

    private IEnumerator FadeTalkingWeight(NPCBiologicalMotion npc, float target)
    {
        float start = npc.talkingWeight;
        float elapsed = 0;
        while (elapsed < talkingFadeDuration)
        {
            elapsed += Time.deltaTime;
            npc.talkingWeight = Mathf.Lerp(start, target, elapsed / talkingFadeDuration);
            yield return null;
        }
        npc.talkingWeight = target;
    }

    public IEnumerator ClearHutDistortions(float duration)
    {
        if (activeHutMaterials == null) yield break;
        float elapsed = 0f;
        Dictionary<Material, float> startValues = new Dictionary<Material, float>();
        foreach (var info in activeHutMaterials)
        {
            if (info.mat != null) startValues[info.mat] = info.mat.GetFloat(info.shaderEffectParamName);
        }
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            foreach (var info in activeHutMaterials)
            {
                if (info.mat != null && startValues.ContainsKey(info.mat))
                {
                    float currentVal = Mathf.Lerp(startValues[info.mat], 0f, t);
                    info.mat.SetFloat(info.shaderEffectParamName, currentVal);
                }
            }
            if (fireLight != null) fireLight.color = tripColorGradient.Evaluate(t);
            yield return null;
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0;
        colorAdjustments.postExposure.value = startExposure;
        while (elapsed < fadeTransitionDuration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, targetExposure, elapsed / fadeTransitionDuration);
            yield return null;
        }
        colorAdjustments.postExposure.value = targetExposure;
    }
}