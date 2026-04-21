using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class VRGrindFeedback : MonoBehaviour
{
    [Header("UI Prompt Settings")]
    public GameObject grindPromptUI;
    public Transform playerTransform;
    public float hideDistance = 1.5f;

    [Header("Visual Feedback")]
    public ParticleSystem sparkVFX;
    private Collider grinderCollider;

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
    [Tooltip("Drag the actual Material asset from your project folder here.")]
    public Material fadeMaterial; 
    public float transitionOffsetTime = 1.5f;
    public float fadeDuration = 2.0f;
    public string nextSceneName = "Hospital";
    
    // THE LINK: This caches the property ID for the GPU
    private int fadePropertyID;
    private bool isTransitioning = false;

    void Start()
    {
        grinderCollider = GetComponent<Collider>();
        if (sparkVFX != null) sparkVFX.Stop();

        // 1. INITIALIZE THE LINK TO THE MATERIAL
        if (fadeMaterial != null)
        {
            // This 'calls' the property from the shader and turns it into a number the GPU understands
            fadePropertyID = Shader.PropertyToID("_MasterFade");
            
            // Immediately force it to 0 (Clear) so the player can see
            fadeMaterial.SetFloat(fadePropertyID, 0f);
            Debug.Log("✅ Material Linked: Ready to fade " + fadeMaterial.name);
        }
        else
        {
            Debug.LogError("❌ ERROR: No Material assigned to the 'Fade Material' slot!");
        }

        // Setup Player & FMOD
        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;
        foreach (GameObject obj in objectsToActivate) if (obj != null) obj.SetActive(false);
        maxGrindTime = objectsToActivate.Count * secondsPerObject;

        grinderInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.grinding);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(grinderInstance, transform);
    }

    void Update()
    {
        if (grindPromptUI != null && playerTransform != null && !isTransitioning)
        {
            if (totalGrindTime > 0) { grindPromptUI.SetActive(false); return; }
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            grindPromptUI.SetActive(distance > hideDistance);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabItem != null && grabItem.gameObject.CompareTag("Key")) grinderInstance.start();
    }

    void OnTriggerStay(Collider other)
    {
        if (isTransitioning) return;
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            if (sparkVFX != null)
            {
                sparkVFX.transform.position = grinderCollider.ClosestPoint(other.transform.position);
                if (!sparkVFX.isPlaying) sparkVFX.Play();
            }

            totalGrindTime += Time.deltaTime;
            CheckProgression();

            if (radioEmitter != null && maxGrindTime > 0)
                radioEmitter.SetParameter(radioParameterName, totalGrindTime / maxGrindTime);

            // Simple Haptic Logic
            if (grabItem.isSelected)
            {
                foreach (var interactor in grabItem.interactorsSelecting)
                {
                    var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(interactor.transform.name.Contains("Left") ? UnityEngine.XR.XRNode.LeftHand : UnityEngine.XR.XRNode.RightHand);
                    if (device.isValid) device.SendHapticImpulse(0u, 0.5f, 0.1f);
                }
            }
        }
    }

    void CheckProgression()
    {
        int targetIndex = Mathf.FloorToInt(totalGrindTime / secondsPerObject);
        if (targetIndex > lastActivatedIndex && targetIndex < objectsToActivate.Count)
        {
            if (objectsToActivate[targetIndex] != null) objectsToActivate[targetIndex].SetActive(true);
            lastActivatedIndex = targetIndex;
        }
        if (targetIndex >= objectsToActivate.Count && !isTransitioning) StartCoroutine(TransitionSequence());
    }

    void OnTriggerExit(Collider other)
    {
        if (sparkVFX != null) sparkVFX.Stop();
        grinderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;
        if (sparkVFX != null) sparkVFX.Stop();
        grinderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        yield return new WaitForSeconds(transitionOffsetTime);

        if (fadeMaterial != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                // This is the active 'call' to the material property
                fadeMaterial.SetFloat(fadePropertyID, Mathf.Lerp(0f, 1f, elapsed / fadeDuration));
                yield return null;
            }
            fadeMaterial.SetFloat(fadePropertyID, 1f);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    void OnApplicationQuit()
    {
        // Cleanup: Reset the material asset so your editor isn't stuck on black
        if (fadeMaterial != null) fadeMaterial.SetFloat("_MasterFade", 0f);
    }
}