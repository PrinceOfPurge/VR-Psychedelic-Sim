using UnityEngine;

public class VRGrindFeedback : MonoBehaviour
{
    [Header("Visual Feedback")]
    public ParticleSystem sparkVFX;
    private Collider grinderCollider;

    [Header("Haptic Feedback")]
    [Range(0f, 1f)]
    public float hapticIntensity = 0.7f;

    void Start()
    {
        grinderCollider = GetComponent<Collider>();
        if (sparkVFX != null) sparkVFX.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        // 1. Bulletproof check: Look for the XRGrabInteractable anywhere on the object hitting the belt
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // 2. If it is a grab item, AND its main object is tagged "Key"
        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            Debug.Log("SUCCESS: The Key is touching the grinder!");

            // VISUALS
            if (sparkVFX != null)
            {
                sparkVFX.transform.position = grinderCollider.ClosestPoint(other.transform.position);
                if (!sparkVFX.isPlaying) sparkVFX.Play();
            }

            // HAPTICS
            if (grabItem.isSelected) 
            {
                Debug.Log("SUCCESS: The Key is being held, sending haptics!");
                foreach (var interactor in grabItem.interactorsSelecting)
                {
                    if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
                    {
                        controllerInteractor.SendHapticImpulse(hapticIntensity, 0.1f);
                    }
                }
            }
            else
            {
                Debug.Log("WARNING: Key is touching, but is NOT being held by a player.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        var grabItem = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabItem != null && grabItem.gameObject.CompareTag("Key"))
        {
            Debug.Log("EXIT: Key removed from grinder.");
            if (sparkVFX != null) sparkVFX.Stop();
        }
    }
}