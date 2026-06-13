using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using TMPro; 

public class CylinderScaler : MonoBehaviour
{
    private Vector3 originalScale;
    
    [Header("Press Animation")]
    [Tooltip("0.8 means it shrinks to 80% of its original height when hovered.")]
    public float pressPercentage = 0.8f; 
    [Tooltip("0.6 means it shrinks to 60% of its original height when clicked/selected.")]
    public float selectPercentage = 0.6f;
    
    public Transform textContainer; 

    [Header("Haptic Settings")]
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    [Header("Button Purpose")]
    [Tooltip("Leave this blank if this is the About Button. Enter scene name if it's the Start Button.")]
    public string nextSceneName = "Level1_Locksmith";
    
    [Header("About Panel Dialogue System")]
    [Tooltip("Assign your floating World-Space Canvas/Panel here.")]
    public GameObject aboutPanelToEnable;
    [Tooltip("Drag the TextMeshPro component from inside your 3D panel here.")]
    public TextMeshProUGUI dialogueTextDisplay;

    [Header("Menu Lock System")]
    [Tooltip("Drag BOTH your Start and About button gameobjects into this list so they can be frozen.")]
    public XRSimpleInteractable[] allButtons;

    private bool isHovered = false;
    private bool isTransitioning = false;
    private bool isSelected = false;
    private UnityEngine.XR.XRNode activeHandNode = UnityEngine.XR.XRNode.RightHand;

    private string[] dialoguePages;
    private int currentDialogueIndex = 0;

    void Start()
    {
        originalScale = transform.localScale;
        
        if (aboutPanelToEnable != null)
        {
            aboutPanelToEnable.SetActive(false);
        }

        dialoguePages = new string[]
        {
            "The game is about psychedelic assisted therapy, where a Native man loses touch of his true first nation culture life and working the western life as a locksmith grinding away at a key his whole life, he finds out he gets cancer and this news breaks his spirit, however he decides he must go back to his roots and seek a healing session with his hokake with a psychedelic and there is also a therapist there to help train therapist how to approach this new form of psychedelic training.",
            
            "We hope you enjoy.\n\nCreated by: Omid Fanaei, Tyler Reeds, Ishaan Kishore, Rachel Marinic\n\nSpecial thanks to: David Chandross, Bill Kapralos and Dr. Allen Kalpin for their support and guidance throughout the journey."
        };
    }

    public void ScaleUp(HoverEnterEventArgs args)
    {
        if (isTransitioning || isSelected) return;
        
        isHovered = true;
        ApplyScaleEffect(pressPercentage);

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
        if (isTransitioning || isSelected) return;
        
        transform.localScale = originalScale;
        if (textContainer != null)
        {
            textContainer.localScale = Vector3.one;
        }
    }

    void Update()
    {
        if (!enabled) return;

        if (isHovered && !isTransitioning && !isSelected)
        {
            var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(activeHandNode);
            if (device.isValid)
            {
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed) && gripPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed ||
                    device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
                {
                    ExecuteButtonAction();
                }
            }
        }
    }

    private void ExecuteButtonAction()
    {
        isSelected = true;
        ApplyScaleEffect(selectPercentage);

        // ROUTE A: Start Button
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            isTransitioning = true;
            isHovered = false; 
            SetButtonsInteractable(false); 
            
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
        // ROUTE B: About Button
        else if (aboutPanelToEnable != null && dialogueTextDisplay != null)
        {
            if (!aboutPanelToEnable.activeSelf)
            {
                aboutPanelToEnable.SetActive(true);
                currentDialogueIndex = 0;
                dialogueTextDisplay.text = dialoguePages[currentDialogueIndex];
                
                // CRITICAL FIX: Wipe the hover flag and force physical scale back to base 
                // so it doesn't cache a false hover input while it is disabled.
                isHovered = false;
                transform.localScale = originalScale;
                if (textContainer != null) textContainer.localScale = Vector3.one;

                // Completely disables interaction and cuts off colliders safely
                SetButtonsInteractable(false); 
            }
            
            Invoke(nameof(ResetAfterClick), 0.2f);
        }
    }

    public void AdvanceDialogueFromSeparateButton()
    {
        currentDialogueIndex++;
        Debug.Log($"Advancing text index to: {currentDialogueIndex}");

        if (currentDialogueIndex < dialoguePages.Length)
        {
            if (dialogueTextDisplay != null)
            {
                dialogueTextDisplay.text = dialoguePages[currentDialogueIndex];
            }
        }
        else
        {
            CloseAboutPanel();
        }
    }

    public void CloseAboutPanel()
    {
        if (aboutPanelToEnable != null)
        {
            aboutPanelToEnable.SetActive(false);
        }
        
        // Double check flags are clean upon exit
        isHovered = false;
        isSelected = false;
        
        SetButtonsInteractable(true); 
        currentDialogueIndex = 0;
    }

    private void SetButtonsInteractable(bool state)
    {
        if (allButtons == null) return;
        foreach (var button in allButtons)
        {
            if (button != null)
            {
                button.enabled = state;

                Collider buttonCollider = button.GetComponent<Collider>();
                if (buttonCollider != null)
                {
                    buttonCollider.enabled = state;
                }
            }
        }
    }

    private void ApplyScaleEffect(float percentage)
    {
        float targetYScale = originalScale.y * percentage;
        transform.localScale = new Vector3(originalScale.x, targetYScale, originalScale.z);
        
        if (textContainer != null)
        {
            textContainer.localScale = new Vector3(1f, originalScale.y / targetYScale, 1f);
        }
    }

    private void ResetAfterClick()
    {
        isSelected = false;
        // Only run ScaleReset if we didn't just hide our own tracking state
        if (enabled && isHovered)
        {
            ScaleReset();
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