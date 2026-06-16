using UnityEngine;

public class VRSceneLoader : MonoBehaviour
{
    [Header("Scene Transition")]
    public string nextSceneName = "Level1_Locksmith";
    private bool isTransitioning = false;

    public void ExecuteSceneLoad()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log($"Handing over scene switch to SceneTransitionManager for scene: {nextSceneName}");

        if (SceneTransitionManager.Instance != null)
        {
            // Matches your specific method layout: isInReverse = false, nextScene = nextSceneName
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
        }
        else
        {
            // Safe fallback if testing in an isolated scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}