using UnityEngine;
using System.Collections.Generic; // Required for Lists

public class VRGrindFeedback : MonoBehaviour
{
    [Header("Visual Feedback")]
    public ParticleSystem sparkVFX;
    private Collider grinderCollider;

    [Header("Haptic Feedback")]
    [Range(0f, 1f)]
    public float hapticIntensity = 0.7f;

    [Header("Progression System")]
    [Tooltip("Drag objects from your scene/table here in the order you want them to appear.")]
    public List<GameObject> objectsToActivate;
    public float secondsPerObject = 2.0f; // How long to grind to spawn the next item
    
    private float totalGrindTime = 0f;
    private int lastActivatedIndex = -1;

    void Start()
    {
        grinderCollider = GetComponent<Collider>();
        if (sparkVFX != null) sparkVFX.Stop();

        // Optional: Ensure all objects in the list start as inactive
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            // 1. VISUALS
            if (sparkVFX != null)
            {
                sparkVFX.transform.position = grinderCollider.ClosestPoint(other.transform.position);
                if (!sparkVFX.isPlaying) sparkVFX.Play();
            }

            // 2. PROGRESSION LOGIC
            totalGrindTime += Time.deltaTime;
            CheckProgression();

            // 3. HAPTICS (Improved Check)
            if (grabItem.isSelected) 
            {
                foreach (var interactor in grabItem.interactorsSelecting)
                {
                    // Using XRBaseControllerInteractor for better compatibility in URP/Unity 6
                    if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
                    {
                        controllerInteractor.SendHapticImpulse(hapticIntensity, 0.1f);
                    }
                }
            }
        }
    }

    void CheckProgression()
    {
        // Calculate which index we should be at based on time
        int targetIndex = Mathf.FloorToInt(totalGrindTime / secondsPerObject);

        // If we have reached a new index and it's within the list bounds
        if (targetIndex > lastActivatedIndex && targetIndex < objectsToActivate.Count)
        {
            if (objectsToActivate[targetIndex] != null)
            {
                objectsToActivate[targetIndex].SetActive(true);
                Debug.Log($"Progression: Activated {objectsToActivate[targetIndex].name}");
            }
            lastActivatedIndex = targetIndex;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            if (sparkVFX != null) sparkVFX.Stop();
        }
    }
}