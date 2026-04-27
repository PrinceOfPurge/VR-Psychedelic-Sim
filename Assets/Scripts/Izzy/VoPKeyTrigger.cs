using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VoPKeyTrigger : MonoBehaviour
{
    [Header("1. References")]
    [SerializeField] private VisualEffect[] vineGrowingGraphs;
    [SerializeField] private VisualEffect fireVFX;
    [SerializeField] private VisualEffect fireflyVFX; 
    [SerializeField] private BloodPoolGrow waterPool; 
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private string nextSceneName = "Scene8_Integration";

    [Header("2. Character Escape Settings")]
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float runDuration = 3.0f;
    [SerializeField] private Vector3 escapeDirection = new Vector3(0, 0, 1);

    [Header("3. Firefly Stage (First)")]
    [SerializeField] private float fireflyRampDuration = 4.0f; 
    [SerializeField] private float delayBeforeFireStarts = 2.0f;

    [Header("4. Fire Growth Stage (Second)")]
    [SerializeField] private float fireRampUpDuration = 4.0f;
    [SerializeField] private float delayAfterFireBeforeVines = 2.0f;

    [Header("5. Vine Growth Stage (Third)")]
    [SerializeField] private float delayAfterVinesBeforeChaos = 5.0f;

    [Header("6. Fire Chaos & Final Transition (Last)")]
    [SerializeField] private float fireChaosRampDuration = 2.0f;
    [SerializeField] private float targetTurbulence = 50.0f; 
    [SerializeField] private float finalExposurePeak = 10f;
    [SerializeField] private float finalSceneFadeDuration = 3.0f;
    [SerializeField] private float sceneLoadWaitTime = 2.0f;

    [Header("Post-Processing (Mood Shift)")]
    [SerializeField] private float moodTransitionDuration = 5.0f;
    [SerializeField] private float targetTemperature = 30f;
    [SerializeField] private float targetSaturation = 20f;
    [SerializeField] private float targetBloom = 5f;

    private WhiteBalance whiteBalance;
    private ColorAdjustments colorAdjustments;
    private Bloom bloom;
    private bool isTransitioning = false;
    private Animator myAnimator;

    private float startTemp, startSat, startBloom, startTurbulence;

    private void Start()
    {
        myAnimator = GetComponent<Animator>();
        CacheComponents();
        ResetVFX();
    }

    private void CacheComponents()
    {
        if (postProcessVolume.profile.TryGet(out whiteBalance) &&
            postProcessVolume.profile.TryGet(out colorAdjustments) &&
            postProcessVolume.profile.TryGet(out bloom))
        {
            startTemp = whiteBalance.temperature.value;
            startSat = colorAdjustments.saturation.value;
            startBloom = bloom.intensity.value;
        }

        if (fireVFX != null) startTurbulence = fireVFX.GetFloat("TurbulenceIntensity");
    }

    private void ResetVFX()
    {
        foreach (var vGraph in vineGrowingGraphs) vGraph.Stop();
        
        if (fireVFX != null) {
            fireVFX.Stop();
            fireVFX.SetFloat("SpawnIntensity", 0);
        }

        if (fireflyVFX != null) {
            fireflyVFX.Stop();
            fireflyVFX.enabled = false; 
            //fireflyVFX.SetFloat("SpawnIntensity", 0);
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
        StartCoroutine(MasterSequenceConductor());
        StartCoroutine(CharacterEscapeSequence());
    }

    // This handles the timing of all visual elements
    private IEnumerator MasterSequenceConductor()
    {
        // STEP 1: Fireflies Start
        if (fireflyVFX != null) fireflyVFX.Play();
        
        //StartCoroutine(LerpFireflyIntensity(1f, fireflyRampDuration));
        fireflyVFX.enabled = true; // Changing the intensity doesn't really work because of a high lifetime
        
        StartCoroutine(TransitionPostProcessingMood()); // Starts alongside fireflies

        yield return new WaitForSeconds(delayBeforeFireStarts);

        // STEP 2: Fire Grows
        if (fireVFX != null) fireVFX.Play();
        float fireElapsed = 0;
        while (fireElapsed < fireRampUpDuration)
        {
            fireElapsed += Time.deltaTime;
            fireVFX.SetFloat("SpawnIntensity", Mathf.Lerp(0, 1, fireElapsed / fireRampUpDuration));
            yield return null;
        }

        yield return new WaitForSeconds(delayAfterFireBeforeVines);

        // STEP 3: Vines & Water
        foreach (var vGraph in vineGrowingGraphs) vGraph.Play();
        if (waterPool != null) waterPool.StartPool();

        yield return new WaitForSeconds(delayAfterVinesBeforeChaos);

        // STEP 4: Fire Chaos & Scene Transition
        float chaosElapsed = 0;
        float startExposure = colorAdjustments.postExposure.value;

        while (chaosElapsed < fireChaosRampDuration)
        {
            chaosElapsed += Time.deltaTime;
            float t = chaosElapsed / fireChaosRampDuration;
            
            fireVFX.SetFloat("TurbulenceIntensity", Mathf.Lerp(startTurbulence, targetTurbulence, t));
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, finalExposurePeak, t);
            yield return null;
        }

        if (AudioManager.instance != null) AudioManager.instance.StopValleyMusic();

        // Final Transition Trigger
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformEgoDeathTransition(
                postProcessVolume, 
                finalSceneFadeDuration, 
                sceneLoadWaitTime, 
                nextSceneName
            );
        }
    }

    private IEnumerator LerpFireflyIntensity(float target, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fireflyVFX.SetFloat("SpawnIntensity", Mathf.Lerp(0, target, elapsed / duration));
            yield return null;
        }
    }

    private IEnumerator TransitionPostProcessingMood()
    {
        float elapsed = 0;
        while (elapsed < moodTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / moodTransitionDuration);
            
            whiteBalance.temperature.value = Mathf.Lerp(startTemp, targetTemperature, t);
            colorAdjustments.saturation.value = Mathf.Lerp(startSat, targetSaturation, t);
            bloom.intensity.value = Mathf.Lerp(startBloom, targetBloom, t);
            yield return null;
        }
    }

    private IEnumerator CharacterEscapeSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (myAnimator != null)
        {
            transform.rotation = Quaternion.LookRotation(escapeDirection);
            float elapsed = 0;
            while (elapsed < runDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / runDuration;
                myAnimator.SetFloat(speedParameterName, Mathf.Lerp(0f, 1f, t * 2f));
                transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);
                yield return null;
            }

            // HIDE CHARACTER WITHOUT KILLING SCRIPT
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allRenderers) r.enabled = false;
            myAnimator.enabled = false; 
        }
    }
}