using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ForceGrab : MonoBehaviour
{
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private XRBaseInteractor handInteractor; 

    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        if (interactionManager == null) 
            interactionManager = FindObjectOfType<XRInteractionManager>();
        
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();

        // --- PREVENT GLITCHING ---
        // 1. Turn off gravity and physics immediately
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; 
        }
        // 2. Disable the collider so it doesn't bounce off the floor at spawn
        if (col != null) col.enabled = false;

        if (grabInteractable != null && handInteractor != null)
        {
            StartCoroutine(ForceGrabRoutine(grabInteractable));
        }
    }

    private IEnumerator ForceGrabRoutine(XRGrabInteractable interactable)
    {
        // Wait a few frames for the XR system to stabilize
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Perform the grab
        interactionManager.SelectEnter((IXRSelectInteractor)handInteractor, (IXRSelectInteractable)interactable);
        
        // Wait one more frame to ensure the grab is registered
        yield return new WaitForFixedUpdate();

        // --- RE-ENABLE PHYSICS (Only after it's in the hand) ---
        if (col != null) col.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false; // Allow physics again
            rb.useGravity = true;
        }

        Debug.Log("Key is now hard-locked to hand.");
    }
}