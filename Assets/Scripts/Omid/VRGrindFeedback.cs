using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine.SceneManagement; // Needed for the industry standard Async scene loading

public class VRGrindFeedback : MonoBehaviour
{
    [Header("UI Prompt Settings")]
    public GameObject grindPromptUI;
    public Transform playerTransform;
    public float hideDistance = 1.5f;

    [Header("Visual Feedback")]
    public ParticleSystem sparkVFX;
    private Collider grinderCollider;

    [Header("Haptic Feedback")]
    [Range(0f, 1f)]
    public float hapticIntensity = 0.7f;
    public float hapticInterval = 0.15f;
    private float nextHapticTime = 0f;

    [Header("FMOD Audio")]
    public FMODUnity.StudioEventEmitter radioEmitter;
    public string radioParameterName = "TimePassage";
    private EventInstance grinderInstance;

    [Header("Progression System")]
    public List<GameObject> objectsToActivate;
    public float secondsPerObject = 2.0f;
    private float totalGrindTime = 0f;
    private float maxGrindTime = 0f;
    private int lastActivatedIndex = -1;

    [Header("Scene Transition & Shader")]
    [Tooltip("Drag the Material that has your friend's shader on it here.")]
    public Material fadeMaterial;
    
    [Tooltip("How many seconds to wait after the final object appears before fading out.")]
    public float transitionOffsetTime = 1.5f;
    
    [Tooltip("How long the fade to black/out actually takes.")]
    public float fadeDuration = 2.0f;
    
    [Tooltip("The exact name of the next scene in your Build Settings.")]
    public string nextSceneName = "Hospital";
    
    private bool isTransitioning = false;

    void Start()
    {
        grinderCollider = GetComponent<Collider>();
        if (sparkVFX != null) sparkVFX.Stop();

        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(false);
        }

        maxGrindTime = objectsToActivate.Count * secondsPerObject;

        // Reset the shader fade to 0 so you don't start the game blind if the material saved its state!
        if (fadeMaterial != null)
        {
            fadeMaterial.SetFloat("_MasterFade", 0f);
        }

        grinderInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.grinding);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(grinderInstance, transform);
    }

    void Update()
    {
        if (grindPromptUI != null && playerTransform != null && !isTransitioning)
        {
            if (totalGrindTime > 0)
            {
                if (grindPromptUI.activeSelf) grindPromptUI.SetActive(false);
                return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldBeVisible = distance > hideDistance;
            if (grindPromptUI.activeSelf != shouldBeVisible)
            {
                grindPromptUI.SetActive(shouldBeVisible);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't let them start grinding again if we are already fading out
        if (isTransitioning) return;

        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            grinderInstance.start();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isTransitioning) return;

        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            // 1. VISUALS
            if (sparkVFX != null)
            {
                sparkVFX.transform.position = grinderCollider.ClosestPoint(other.transform.position);
                if (!sparkVFX.isPlaying) sparkVFX.Play();
            }

            // 2. PROGRESSION & RADIO AUDIO
            totalGrindTime += Time.deltaTime;
            CheckProgression();

            if (radioEmitter != null && maxGrindTime > 0)
            {
                float timeProgress = Mathf.Clamp01(totalGrindTime / maxGrindTime);
                radioEmitter.SetParameter(radioParameterName, timeProgress);
            }

            // 3. HAPTICS
            if (grabItem.isSelected && Time.time >= nextHapticTime)
            {
                float pulseDuration = 0.15f;
                foreach (var interactor in grabItem.interactorsSelecting)
                {
                    UnityEngine.XR.XRNode handNode = interactor.transform.name.Contains("Left") ?
                    UnityEngine.XR.XRNode.LeftHand : UnityEngine.XR.XRNode.RightHand;

                    UnityEngine.XR.InputDevice device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(handNode);
                    if (device.isValid)
                    {
                        device.SendHapticImpulse(0u, hapticIntensity, pulseDuration);
                    }
                }
                nextHapticTime = Time.time + pulseDuration;
            }
        }
    }

    void CheckProgression()
    {
        int targetIndex = Mathf.FloorToInt(totalGrindTime / secondsPerObject);

        if (targetIndex > lastActivatedIndex && targetIndex < objectsToActivate.Count)
        {
            if (objectsToActivate[targetIndex] != null)
            {
                objectsToActivate[targetIndex].SetActive(true);
            }
            lastActivatedIndex = targetIndex;
        }

        // TRIGGER THE END SEQUENCE
        if (targetIndex >= objectsToActivate.Count && !isTransitioning)
        {
            StartCoroutine(TransitionSequence());
        }
    }

    void OnTriggerExit(Collider other)
    {
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            if (sparkVFX != null) sparkVFX.Stop();
            grinderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    // --- NEW: THE TRANSITION COROUTINE ---
    private IEnumerator TransitionSequence()
    {
        isTransitioning = true; // Lock the script so this only fires once
        
        // Turn off VFX and stop the grinder audio immediately
        if (sparkVFX != null) sparkVFX.Stop();
        grinderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        // 1. Wait for your custom offset time
        yield return new WaitForSeconds(transitionOffsetTime);

        // 2. Animate the shader from 0 to 1
        if (fadeMaterial != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float currentFade = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeMaterial.SetFloat("_MasterFade", currentFade);
                yield return null; // Wait for the next frame
            }
            // Ensure it firmly hits 1.0 at the end
            fadeMaterial.SetFloat("_MasterFade", 1f); 
        }
        else
        {
            Debug.LogWarning("VRGrindFeedback: No Fade Material assigned! Skipping fade animation.");
        }

        // 3. Load the Hospital scene asynchronously
        // Note: You must add "Hospital" to your Build Settings (File -> Build Settings -> Scenes in Build) for this to work!
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        
        // Optional: Wait until it's fully loaded (though the current scene will just destroy itself when it's done)
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    // Always keep this for FMOD cleanup!
    void OnDestroy()
    {
        if (grinderInstance.isValid())
        {
            grinderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            grinderInstance.release();
        }
    }
}