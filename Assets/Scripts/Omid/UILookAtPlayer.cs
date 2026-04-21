using UnityEngine;

public class UILookAtPlayer : MonoBehaviour
{
    [Tooltip("If null, it will automatically find the Main Camera (Player's head)")]
    public Transform playerCamera;

    [Tooltip("Check this if you want the UI to only rotate left/right, not tilt up/down")]
    public bool verticalLock = true;

    void Start()
    {
        // Auto-assign the player camera if you forgot to drag it in
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Calculate the direction from the UI to the player
        Vector3 targetDirection = playerCamera.position - transform.position;

        if (verticalLock)
        {
            // By zeroing out the Y, the UI won't tilt up or down
            targetDirection.y = 0;
        }

        // If the player is standing exactly inside the UI, don't rotate (avoids errors)
        if (targetDirection != Vector3.zero)
        {
            // Create a rotation that looks away from the player 
            // (Canvas 'forward' is technically the back, so we look away to face the front toward player)
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            
            transform.rotation = targetRotation;
        }
    }
}