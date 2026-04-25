using UnityEngine;

public class PillEater : MonoBehaviour
{
    private HoganSceneInitializer sceneController;

    void Start()
    {
        // Find the scene controller automatically
        sceneController = FindObjectOfType<HoganSceneInitializer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object is the pill
        if (other.CompareTag("Pill"))
        {
            Debug.Log("Pill Taken!");

            // 2. Tell the scene controller to continue the dialogue
            if (sceneController != null)
            {
                sceneController.SetPillTaken(true);
            }

            // 3. pill sound??
            // AudioManager.instance.PlayOneShot(FMODEvents.instance.PillSwallow, transform.position);

            // 4. Destroy the pill object
            Destroy(other.gameObject);
        }
    }
}