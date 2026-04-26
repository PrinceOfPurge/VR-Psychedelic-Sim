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
    [SerializeField] private VisualEffect fireflyVFX; 
    [SerializeField] private BloodPoolGrow waterPool; 
    [SerializeField] private Volume postProcessVolume;

    [Header("Animator Settings")]
    [SerializeField] private string speedParameterName = "Speed"; // The parameter in your Blend Tree
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float runDuration = 3.0f;
    [SerializeField] private Vector3 escapeDirection = new Vector3(0, 0, 1);

    [Header("Fire VFX & Peak Intensity")]
    [SerializeField] private float fireRampUpDuration = 4.0f;
    [SerializeField] private float fireStayDuration = 2.0f;
    [SerializeField] private float fireChaosDuration = 2.0f;
    [SerializeField] private float startTurbulence = 2.0f; 
    [SerializeField] private float targetTurbulence = 50.0f; 

    [Header("Transition Settings")]
    [SerializeField] private float fireflyRampDuration = 4.0f; 
    [SerializeField] private float beforeTransitionWaitTime = 8.0f;
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
    private Animator myAnimator;

    private float startTemp, startSat, startBloom;

    private void Start()
    {
        myAnimator = GetComponent<Animator>();
        
        // Ensure the Blend Tree starts at 0 (Show Animation)
        if (myAnimator != null) myAnimator.SetFloat(speedParameterName, 0f);

        foreach (var vGraph in vineGrowingGraphs) vGraph.Stop();
        
        if (fireVFX != null) {
            fireVFX.Stop();
            fireVFX.SetFloat("SpawnIntensity", 0);
            fireVFX.SetFloat("TurbulenceIntensity", startTurbulence);
        }

        if (fireflyVFX != null) {
            fireflyVFX.Stop();
            fireflyVFX.SetFloat("SpawnIntensity", 0);
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
        if (other.CompareTag("Key") && !isTransitioning)
        {
            Destroy(other.gameObject);
            TriggerHealingSequence();
        }
    }

    private void TriggerHealingSequence()
    {
        isTransitioning = true;
        foreach (var vGraph in vineGrowingGraphs) vGraph.Play();
        if (fireflyVFX != null) fireflyVFX.Play(); 
        if (waterPool != null) waterPool.StartPool();

        StartCoroutine(TransitionPostProcessing());
        StartCoroutine(CharacterEscapeSequence());
    }

    private IEnumerator TransitionPostProcessing()
    {
        float elapsed = 0;
        float maxDuration = Mathf.Max(transitionDuration, fireflyRampDuration);

        while (elapsed < maxDuration)
        {
            elapsed += Time.deltaTime;
            float tPost = Mathf.Clamp01(elapsed / transitionDuration);
            float curvedTPost = Mathf.SmoothStep(0, 1, tPost);
            
            whiteBalance.temperature.value = Mathf.Lerp(startTemp, targetTemperature, curvedTPost);
            colorAdjustments.saturation.value = Mathf.Lerp(startSat, targetSaturation, curvedTPost);
            bloom.intensity.value = Mathf.Lerp(startBloom, targetBloom, curvedTPost);

            if (fireflyVFX != null) {
                float tFirefly = Mathf.Clamp01(elapsed / fireflyRampDuration);
                fireflyVFX.SetFloat("SpawnIntensity", Mathf.SmoothStep(0, 1, tFirefly));
            }
            yield return null;
        }
    }

    private IEnumerator CharacterEscapeSequence()
    {
        yield return new WaitForSeconds(1.0f);

        if (myAnimator != null)
        {
            // Face away
            transform.rotation = Quaternion.LookRotation(escapeDirection);

            float elapsed = 0;
            while (elapsed < runDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / runDuration;

                // Lerp the Blend Tree parameter from 0 to 1
                // This will blend from 'Show' to 'Normal Walk' to 'Sad Run'
                myAnimator.SetFloat(speedParameterName, Mathf.Lerp(0f, 1f, t * 2f)); // Multiply by 2 to reach Sad Run faster

                // Move forward
                transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);
                yield return null;
            }

            gameObject.SetActive(false); 
        }
        
        yield return new WaitForSeconds(beforeTransitionWaitTime);
        StartCoroutine(FirePeakSequence());
    }

    private IEnumerator FirePeakSequence()
    {
        if (fireVFX == null) yield break;

        fireVFX.Play();
        float elapsed = 0;

        while (elapsed < fireRampUpDuration)
        {
            elapsed += Time.deltaTime;
            fireVFX.SetFloat("SpawnIntensity", Mathf.Lerp(0, 1, elapsed / fireRampUpDuration));
            yield return null;
        }

        yield return new WaitForSeconds(fireStayDuration);
        
        elapsed = 0;
        float startExposure = colorAdjustments.postExposure.value;
        while (elapsed < fireChaosDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fireChaosDuration;
            fireVFX.SetFloat("TurbulenceIntensity", Mathf.Lerp(startTurbulence, targetTurbulence, t));
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, 10f, t);
            yield return null;
        }

        if (AudioManager.instance != null) AudioManager.instance.StopValleyMusic();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformEgoDeathTransition(postProcessVolume, transitionDuration, 
                sceneTransitionWaitTime, nextSceneName);
    }
}