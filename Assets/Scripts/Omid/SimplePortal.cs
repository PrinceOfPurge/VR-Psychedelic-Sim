using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePortal : MonoBehaviour
{
    [SerializeField] private GameObject fadePanel;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object touching the portal is the player
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation()
    {
        if (fadePanel != null) fadePanel.SetActive(true);
    }

    public void SwitchScenes()
    {
        SceneManager.LoadScene("IzzySandbox");
    }
}