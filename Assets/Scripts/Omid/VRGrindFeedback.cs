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
    public string radioParameterName = "TimePassage";
    private EventInstance grinderInstance;
    private EventInstance radioInstance;    // For RadioMusic (The one that speeds up)
    private EventInstance locksmithInstance; // For LocksmithMusic (The background track)

    [Header("Progression System")]
    public List<GameObject> objectsToActivate;
    public float secondsPerObject = 2.0f;
    private float totalGrindTime = 0f;
    private float maxGrindTime = 0f;
    private int lastActivatedIndex = -1;

    [Header("Scene Transition & Shader")]
    public Material fadeMaterial; 
    public string shaderReferenceName = "_MasterFade";
    public float transitionOffsetTime = 1.5f;
    public string nextSceneName = "Hospital";
    
    private bool isTransitioning = false;
    private int fadePropID;

    void Start()
    {
        grinderCollider = GetComponent<Collider>();
        if (sparkVFX != null) sparkVFX.Stop();

        fadePropID = Shader.PropertyToID(shaderReferenceName);

        if (fadeMaterial != null) fadeMaterial.SetFloat(fadePropID, 0f);
        Shader.SetGlobalFloat(shaderReferenceName, 0f);

        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;
        foreach (GameObject obj in objectsToActivate) if (obj != null) obj.SetActive(false);
        maxGrindTime = objectsToActivate.Count * secondsPerObject;

        // --- AUDIO INITIALIZATION ---
        // 1. Grinder SFX
        grinderInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.grinding);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(grinderInstance, transform);

        // 2. Radio Music (The one with the TimePassage parameter)
        radioInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.RadioMusic);
        radioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(radioInstance, transform);
        radioInstance.start();

        // 3. Locksmith Music (The general scene music)
        locksmithInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.LocksmithMusic);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(locksmithInstance, transform);
        locksmithInstance.start();
    }

    void Update()
    {
        // Spatial update for music instances
        if (radioInstance.isValid()) radioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        if (locksmithInstance.isValid()) locksmithInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));

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
            if (sparkVFX != null)
            {
                sparkVFX.transform.position = grinderCollider.ClosestPoint(other.transform.position);
                if (!sparkVFX.isPlaying) sparkVFX.Play();
            }

            totalGrindTime += Time.deltaTime;
            CheckProgression();

            // --- UPDATE RADIO SPEED ---
            if (maxGrindTime > 0)
            {
                float progress = Mathf.Clamp01(totalGrindTime / maxGrindTime);
    
                // Safety: Set the parameter
                radioInstance.setParameterByName(radioParameterName, progress);

                // DEBUG: Check your console! If this stays at 0, your 'totalGrindTime' isn't increasing.
                // If it reaches 1.0, the code is fine and the issue is inside FMOD Studio.
                //Debug.Log($"Radio Progress: {progress} | Parameter: {radioParameterName}");
            }

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
        
        // Stop all audio instances smoothly
        grinderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        radioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        locksmithInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        yield return new WaitForSeconds(transitionOffsetTime);
    
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
        
        yield return null;
    }

    private void OnDestroy()
    {
        // Cleanup to prevent memory leaks
        radioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        radioInstance.release();
        locksmithInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        locksmithInstance.release();
        grinderInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        grinderInstance.release();
    }
}