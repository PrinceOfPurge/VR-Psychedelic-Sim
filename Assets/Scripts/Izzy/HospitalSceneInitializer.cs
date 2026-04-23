using UnityEngine;
using System.Collections;

public class HospitalSceneInitializer : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float fadeInDuration = 3f;
    [SerializeField] private float minToMaxIntensityDuration = 8f;
    [SerializeField] private float waitAtMaxDuration = 2f;
    [SerializeField] private string nextSceneName = "Level3_Valley";

    [Header("References")]
    [SerializeField] private HospitalOrbController hospitalOrbController;
    
    void Start()
    {
        // Start the sequence as soon as the level loads
        StartCoroutine(HospitalSequenceRoutine());
    }

    private IEnumerator HospitalSequenceRoutine()
    {
        // 1. TRIGGER REVERSE FADE (Waking up)
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(true, fadeDurationOverride: fadeInDuration);
        }
        
        // Wait for the fade to finish before starting the orb intensity
        yield return new WaitForSeconds(fadeInDuration);

        // 2. LERP SCENE INTENSITY
        float elapsed = 0f;
        while (elapsed < minToMaxIntensityDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / minToMaxIntensityDuration;
            
            if (hospitalOrbController != null) 
                hospitalOrbController.SceneIntensity = t;

            yield return null; // This tells Unity: "Finish this frame and come back here on the next one"
        }
        
        Debug.Log("Max intensity reached!");

        // 3. THE WAIT TIME
        yield return new WaitForSeconds(waitAtMaxDuration);

        // 4. FADE OUT TO NEXT SCENE
        if (SceneTransitionManager.Instance != null)
        {
            // Non-reverse (0 to 1), passes the next scene name to load
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
        }
    }
}