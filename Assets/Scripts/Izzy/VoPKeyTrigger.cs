using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VoPKeyTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect[] vineGrowingGraphs;
    [SerializeField] private BloodPoolGrow waterPool; 
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject kidCharacter;

    [Header("Kid Animation & Movement")]
    [SerializeField] private string climbAnim = "Climbing";
    [SerializeField] private string danceAnim = "Dancing1";
    [SerializeField] private float kidSpawnDelay = 1.5f;    
    [SerializeField] private float climbDuration = 3.0f;   
    [SerializeField] private float sinkDepth = 1.5f;       // How far underground they start

    [Header("Transition Settings")]
    [SerializeField] private float beforeTransitionWaitTime = 12.0f;
    [SerializeField] private float transitionDuration = 3.0f;
    [SerializeField] private float sceneTransitionWaitTime = 2.0f;
    [SerializeField] private float targetTemperature = 30f;
    [SerializeField] private float targetSaturation = 20f;
    [SerializeField] private float targetBloom = 5f;
    [SerializeField] private string nextSceneName = "Scene8_Integration";

    private WhiteBalance whiteBalance;
    private ColorAdjustments colorAdjustments;
    private Bloom bloom;
    private bool isTransitioning = false;
    private Animator kidAnimator;
    
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float startTemp, startSat, startBloom;

    private void Start()
    {
        foreach (var vGraph in vineGrowingGraphs)
        {
            vGraph.Stop();
        }
        
        if (kidCharacter != null)
        {
            kidAnimator = kidCharacter.GetComponent<Animator>();
            
            // Save where the kid is supposed to end up (the current Inspector position)
            targetPosition = kidCharacter.transform.position;
            // Calculate where they start (down in the sand)
            startPosition = targetPosition - (Vector3.up * sinkDepth);
            
            kidCharacter.SetActive(false); 
        }

        if (postProcessVolume.profile.TryGet(out whiteBalance) &&
            postProcessVolume.profile.TryGet(out colorAdjustments) &&
            postProcessVolume.profile.TryGet(out bloom))
        {
            startTemp = whiteBalance.temperature.value;
            startSat = colorAdjustments.saturation.value;
            startBloom = bloom.intensity.value;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isTransitioning)
        {
            TriggerHealingSequence();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key") && !isTransitioning)
        {
            TriggerHealingSequence();
        }
    }

    private void TriggerHealingSequence()
    {
        isTransitioning = true;
        foreach (var vGraph in vineGrowingGraphs)
        {
            vGraph.Play();
        }
        if (waterPool != null) waterPool.StartPool();

        StartCoroutine(TransitionPostProcessing());
        StartCoroutine(SpawnKidSequence());
    }

    private IEnumerator SpawnKidSequence()
    {
        yield return new WaitForSeconds(kidSpawnDelay);

        if (kidCharacter != null && kidAnimator != null)
        {
            // 1. Set them to the underground start position and enable them
            kidCharacter.transform.position = startPosition;
            kidCharacter.SetActive(true);
            
            // 2. Start the climb animation
            kidAnimator.CrossFade(climbAnim, 0.2f);

            // 3. Lerp the actual transform position upward
            float elapsed = 0;
            while (elapsed < climbDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / climbDuration;
                
                // Using SmoothStep here too makes the "rising" feel less robotic
                float curvedT = Mathf.SmoothStep(0, 1, t);
                kidCharacter.transform.position = Vector3.Lerp(startPosition, targetPosition, curvedT);
                
                yield return null;
            }

            // 4. Ensure they are exactly at the target position and start dancing
            kidCharacter.transform.position = targetPosition;
            kidAnimator.CrossFade(danceAnim, 0.3f);
        }
        
        yield return new WaitForSeconds(beforeTransitionWaitTime);
        
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformEgoDeathTransition(postProcessVolume, transitionDuration, 
                sceneTransitionWaitTime, nextSceneName);
    }

    private IEnumerator TransitionPostProcessing()
    {
        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);

            whiteBalance.temperature.value = Mathf.Lerp(startTemp, targetTemperature, t);
            colorAdjustments.saturation.value = Mathf.Lerp(startSat, targetSaturation, t);
            bloom.intensity.value = Mathf.Lerp(startBloom, targetBloom, t);

            yield return null;
        }
    }
}