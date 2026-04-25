using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// These two namespaces are critical for IXRSelectInteractable
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VicinityDrop : MonoBehaviour
{
    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check for the tag (ensure your Key object has the "Key" tag in the Inspector)
        if (other.CompareTag("Key"))
        {
            // 2. Try to get the SelectInteractable interface
            // This is the modern way to check if an object can be "Selected" by a hand/socket
            if (other.TryGetComponent(out IXRSelectInteractable interactable))
            {
                // 3. Check if it's currently being held
                if (interactable.isSelected)
                {
                    // 4. Force the hand (the interactor) to let go
                    IXRSelectInteractor hand = interactable.interactorsSelecting[0];
                    socket.interactionManager.SelectExit(hand, interactable);
                    
                    Debug.Log("VicinityDrop: Hand forced to release key.");
                }
            }
        }
    }
}