using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VoPKeyTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect[] vineGrowingGraphs;
    [SerializeField] private VisualEffect fireVFX; 
    [SerializeField] private BloodPoolGrow waterPool; 
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject kidCharacter;

    [Header("Kid Animation & Movement")]
    [SerializeField] private string climbAnim = "Climbing";
    [SerializeField] private string danceAnim = "Dancing1";
    [SerializeField] private float kidSpawnDelay = 1.5f;    
    [SerializeField] private float climbDuration = 3.0f;   
    [SerializeField] private float sinkDepth = 1.5f;

    [Header("Fire VFX & Peak Intensity")]
    [SerializeField] private float fireRampUpDuration = 4.0f;
    [SerializeField] private float fireStayDuration = 2.0f;
    [SerializeField] private float fireChaosDuration = 2.0f;
    [SerializeField] private float startTurbulence = 2.0f; // "Drastic" increase
    [SerializeField] private float targetTurbulence = 50.0f; // "Drastic" increase

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
    
    private Vector3 targetPosition, startPosition;
    private float startTemp, startSat, startBloom;

    private void Start()
    {
        foreach (var vGraph in vineGrowingGraphs) vGraph.Stop();
        if (fireVFX != null) {
            fireVFX.Stop();
            fireVFX.SetFloat("SpawnIntensity", 0);
            fireVFX.SetFloat("TurbulenceIntensity", startTurbulence);
        }
        
        if (kidCharacter != null)
        {
            kidAnimator = kidCharacter.GetComponent<Animator>();
            targetPosition = kidCharacter.transform.position;
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
        if (Input.GetKeyDown(KeyCode.F) && !isTransitioning) TriggerHealingSequence();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key") && !isTransitioning) TriggerHealingSequence();
    }

    private void TriggerHealingSequence()
    {
        isTransitioning = true;
        foreach (var vGraph in vineGrowingGraphs) vGraph.Play();
        if (waterPool != null) waterPool.StartPool();

        StartCoroutine(TransitionPostProcessing());
        StartCoroutine(SpawnKidSequence());
    }

    private IEnumerator SpawnKidSequence()
    {
        yield return new WaitForSeconds(kidSpawnDelay);

        if (kidCharacter != null && kidAnimator != null)
        {
            kidCharacter.transform.position = startPosition;
            kidCharacter.SetActive(true);
            kidAnimator.CrossFade(climbAnim, 0.2f);

            float elapsed = 0;
            while (elapsed < climbDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / climbDuration);
                kidCharacter.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            kidCharacter.transform.position = targetPosition;
            kidAnimator.CrossFade(danceAnim, 0.3f);
        }
        
        // Wait for the player to soak in the "Happy Valley" before the peak hits
        yield return new WaitForSeconds(beforeTransitionWaitTime);
        
        // Start the fire peak intensity sequence
        StartCoroutine(FirePeakSequence());
    }

    private IEnumerator FirePeakSequence()
    {
        if (fireVFX == null) yield break;

        fireVFX.Play();
        float elapsed = 0;

        // 1. Ramp SpawnIntensity 0 -> 1
        while (elapsed < fireRampUpDuration)
        {
            elapsed += Time.deltaTime;
            fireVFX.SetFloat("SpawnIntensity", Mathf.Lerp(0, 1, elapsed / fireRampUpDuration));
            yield return null;
        }

        // 2. Stay at 1
        yield return new WaitForSeconds(fireStayDuration);

        // 3. Chaos Phase: Turbulence increases + Ramp Exposure to white
        elapsed = 0;
        float startExposure = colorAdjustments.postExposure.value;

        while (elapsed < fireChaosDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fireChaosDuration;

            fireVFX.SetFloat("TurbulenceIntensity", Mathf.Lerp(startTurbulence, targetTurbulence, t));
            
            // Manual Exposure ramp before the manager takes over for total white-out
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, 10f, t);

            yield return null;
        }

        // 4. Final Scene Transition (Ego Death)
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