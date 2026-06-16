using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransisitonManager : MonoBehaviour
{
    public void GoToFirstLevel()
    {
        Debug.Log("Trigger pulled! Loading Level1_Locksmith...");
        SceneManager.LoadScene("Level1_Locksmith");
    }
}
