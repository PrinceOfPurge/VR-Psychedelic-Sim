using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class StarweaverSceneInitializer : MonoBehaviour
{
    [Header("Volumes")]
    [SerializeField] private Volume descentVolume; 
    [SerializeField] private Volume starweaverVolume;

    [Header("Sequence Scripts")]
    [SerializeField] private EgoDeath egoDeathScript;

    [Header("Timing Settings")]
    [SerializeField] private float transitionBackDuration = 6f;
    [SerializeField] private float delayBeforeWarp = 2f; // Wait for fog to clear slightly

    
    void Start()
    {
        // 1. Immediate visual sync from the previous scene load
        if (descentVolume != null) descentVolume.weight = 1f;
        if (starweaverVolume != null) starweaverVolume.weight = 0f;

        // 2. Start the master sequence
        StartCoroutine(StarweaverSequenceRoutine());
    }

    private IEnumerator StarweaverSequenceRoutine()
    {
        // 3. Begin ramping down the Hogan visuals and ramping up the Space volume
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.EndTrippyEffects(descentVolume, starweaverVolume, transitionBackDuration);
        }

        // 4. THE WAIT: Give the player a moment to see the Earth/Stars before the Warp
        // This ensures the "Warp" doesn't happen while the screen is still 100% kaleidoscope
        yield return new WaitForSeconds(transitionBackDuration + delayBeforeWarp);

        // 5. TRIGGER WARP LAUNCH
        if (egoDeathScript != null)
            StartCoroutine(egoDeathScript.WarpLaunchSequence());
        else
            Debug.LogError("EgoDeath script reference is missing on StarweaverInitializer!");
    }
}