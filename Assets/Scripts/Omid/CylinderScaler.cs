using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CylinderScaler : MonoBehaviour
{
    private Vector3 originalScale;
    public float hoverYScale = 2.5f;
    public Transform textContainer; 

    [Header("Haptic Settings")]
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    [Header("Scene Transition")]
    public string nextSceneName = "Level1_Locksmith";

    private bool isHovered = false;
    private bool isTransitioning = false;
    private UnityEngine.XR.XRNode activeHandNode = UnityEngine.XR.XRNode.RightHand;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void ScaleUp(HoverEnterEventArgs args)
    {
        if (isTransitioning) return;
        
        isHovered = true;
        transform.localScale = new Vector3(originalScale.x, hoverYScale, originalScale.z);
        
        if (textContainer != null)
        {
            textContainer.localScale = new Vector3(1f, originalScale.y / hoverYScale, 1f);
        }

        if (args.interactorObject != null && args.interactorObject.transform != null)
        {
            string lowerName = args.interactorObject.transform.name.ToLower();
            activeHandNode = (lowerName.Contains("left") || lowerName.Contains("_l") || lowerName == "l") 
                ? UnityEngine.XR.XRNode.LeftHand 
                : UnityEngine.XR.XRNode.RightHand;

            TriggerLegacyHaptic(activeHandNode);
        }
    }

    public void ScaleReset()
    {
        isHovered = false;
        if (isTransitioning) return;
        
        transform.localScale = originalScale;
        if (textContainer != null)
        {
            textContainer.localScale = Vector3.one;
        }
    }

    void Update()
    {
        if (isHovered && !isTransitioning)
        {
            var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(activeHandNode);
            if (device.isValid)
            {
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed) && gripPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
                {
                    ExecuteSceneLoad();
                }
            }
        }
    }

    private void ExecuteSceneLoad()
    {
        isTransitioning = true;
        isHovered = false; 
        Debug.Log($"Action detected! Handing over load sequence to SceneTransitionManager for scene: {nextSceneName}");

        // Calls your project's native asynchronous fade loader
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
        }
        else
        {
            // Fallback: If your master system isn't in the scene, use standard loading so it doesn't break
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private void TriggerLegacyHaptic(UnityEngine.XR.XRNode targetNode)
    {
        var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(targetNode);
        if (device.isValid)
        {
            device.SendHapticImpulse(0u, hapticIntensity, hapticDuration);
        }
    }
}