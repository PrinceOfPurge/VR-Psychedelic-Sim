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
    [Tooltip("Drag the Material used in your FullScreen Pass Renderer Feature here.")]
    public Material fadeMaterial; 
    public string shaderReferenceName = "_MasterFade";
    public float transitionOffsetTime = 1.5f;
    public float fadeDuration = 2.0f;
    public string nextSceneName = "Hospital";
    
    private bool isTransitioning = false;
    private int fadePropID;

    void Start()
    {
        grinderCollider = GetComponent<Collider>();
        if (sparkVFX != null) sparkVFX.Stop();

        // CACHE Property ID for performance and reliability
        fadePropID = Shader.PropertyToID(shaderReferenceName);

        // Reset Shader and Audio
        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, 0f);
        Shader.SetGlobalFloat(shaderReferenceName, 0f);

        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;
        foreach (GameObject obj in objectsToActivate) if (obj != null) obj.SetActive(false);
        maxGrindTime = objectsToActivate.Count * secondsPerObject;

        // FMOD Grinder Sound
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
        if (other.CompareTag("Key") || other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>()?.gameObject.CompareTag("Key") == true)
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
            // Play VFX
            if (sparkVFX != null)
            {
                sparkVFX.transform.position = grinderCollider.ClosestPoint(other.transform.position);
                if (!sparkVFX.isPlaying) sparkVFX.Play();
            }

            // Progression
            totalGrindTime += Time.deltaTime;
            CheckProgression();

            // Radio FMOD Parameter
            if (radioEmitter != null && maxGrindTime > 0)
                radioEmitter.SetParameter(radioParameterName, totalGrindTime / maxGrindTime);

            // Haptics
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

        float elapsed = 0f;
        float startAudioVol = AudioManager.instance.masterVolume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Visual Fade (Direct Material + Global fallback)
            if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, t);
            Shader.SetGlobalFloat(shaderReferenceName, t);
            
            // Audio Fade (via your AudioManager instance)
            if (AudioManager.instance != null)
                AudioManager.instance.masterVolume = Mathf.Lerp(startAudioVol, 0f, t);

            yield return null;
        }

        Shader.SetGlobalFloat(shaderReferenceName, 1f);
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnApplicationQuit()
    {
        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, 0f);
        Shader.SetGlobalFloat(shaderReferenceName, 0f);
        if (AudioManager.instance != null) AudioManager.instance.masterVolume = 1f;
    }
}