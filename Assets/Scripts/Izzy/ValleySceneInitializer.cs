using UnityEngine;

public class ValleySceneInitializer : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float fadeInDuration = 3f;
    
    void Start()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(true, fadeDurationOverride: fadeInDuration);
        }
    }
}
