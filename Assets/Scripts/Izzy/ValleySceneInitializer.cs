using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ValleySceneInitializer : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool useExposureFade = false;
    [SerializeField] private float startExposure = 15f; 
    [SerializeField] private float targetExposure = 0f;
    [SerializeField] private float transitionDuration = 3f;

    void Start()
    {
        // Reset fade value from previous scene
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ResetBasicFade();
            
            if (!useExposureFade)
                SceneTransitionManager.Instance.PerformFade(isInReverse: true, fadeDurationOverride: transitionDuration);
        }
        
        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            if (useExposureFade)
                StartCoroutine(FadeInRoutine(colorAdjustments));
        }
    }

    private IEnumerator FadeInRoutine(ColorAdjustments colorAdjustments)
    {
        float elapsed = 0;
        
        // Ensure we start at the high exposure immediately
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
