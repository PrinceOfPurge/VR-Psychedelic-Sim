using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object touching the portal is the player
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            // Instantly load the sandbox scene
            SceneManager.LoadScene("IzzySandbox");
        }
    }
}