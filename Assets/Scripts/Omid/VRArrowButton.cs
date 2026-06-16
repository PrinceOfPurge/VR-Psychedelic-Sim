using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRArrowButton : MonoBehaviour
{
    [Header("Linked Systems")]
    [Tooltip("Drag your main About Button Cylinder here.")]
    public CylinderScaler aboutButtonScript;

    [Header("Spacy Effects")]
    public ParticleSystem hoverParticles;

    [Header("Haptic Settings")]
    public float hapticIntensity = 0.4f;
    public float hapticDuration = 0.08f;

    private UnityEngine.XR.XRNode activeHandNode = UnityEngine.XR.XRNode.RightHand;
    private bool isHovered = false;
    private bool isClickedThisFrame = false;

    // Called via XR Simple Interactable "Hover Entered"
    public void OnArrowHoverEnter(HoverEnterEventArgs args)
    {
        isHovered = true;
        isClickedThisFrame = false;

        if (hoverParticles != null && !hoverParticles.isPlaying)
        {
            hoverParticles.Play();
        }

        if (args.interactorObject != null && args.interactorObject.transform != null)
        {
            string lowerName = args.interactorObject.transform.name.ToLower();
            activeHandNode = (lowerName.Contains("left") || lowerName.Contains("_l") || lowerName == "l") 
                ? UnityEngine.XR.XRNode.LeftHand 
                : UnityEngine.XR.XRNode.RightHand;

            TriggerArrowHaptic(activeHandNode);
        }
    }

    // Called via XR Simple Interactable "Hover Exited"
    public void OnArrowHoverExit()
    {
        isHovered = false;
        isClickedThisFrame = false;

        if (hoverParticles != null)
        {
            hoverParticles.Stop();
        }
    }

    void Update()
    {
        // Mirrors your exact cylinder button input tracking loop
        if (isHovered && !isClickedThisFrame)
        {
            var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(activeHandNode);
            if (device.isValid)
            {
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed) && gripPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
                {
                    OnArrowClicked();
                }
            }
        }
    }

    private void OnArrowClicked()
    {
        // Simple debounce latch so a single button hold down doesn't hyper-skip lines
        isClickedThisFrame = true; 
        Debug.Log("Arrow manual button execution detected!");

        if (aboutButtonScript != null)
        {
            aboutButtonScript.AdvanceDialogueFromSeparateButton();
        }
        else
        {
            CylinderScaler foundScript = FindFirstObjectByType<CylinderScaler>();
            if (foundScript != null)
            {
                foundScript.AdvanceDialogueFromSeparateButton();
            }
        }
        
        TriggerArrowHaptic(activeHandNode);
        
        // Reset the input latch after a brief delay so they can click again for the next page
        Invoke(nameof(ResetInputLatch), 0.4f);
    }

    private void ResetInputLatch()
    {
        // Only clear the click lock if the player is still physically pointing at the arrow
        if (isHovered)
        {
            isClickedThisFrame = false;
        }
    }

    private void TriggerArrowHaptic(UnityEngine.XR.XRNode targetNode)
    {
        var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(targetNode);
        if (device.isValid)
        {
            device.SendHapticImpulse(0u, hapticIntensity, hapticDuration);
        }
    }
}