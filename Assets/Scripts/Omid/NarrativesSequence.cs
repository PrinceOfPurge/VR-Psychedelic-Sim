using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class NarrativeSequence : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI narrativeText;
    public CanvasGroup canvasGroup; // Add a CanvasGroup component to your Canvas!

    [Header("Settings")]
    public string nextSceneName = "HoganHut";
    public float fadeDuration = 2.0f;
    public float displayDuration = 4.0f;

    // The story you want to tell
    private string[] storyBeats = {
        "The weight of the desert's shadows was no longer a burden I could carry alone.",
        "The cycle of the valley had to end.",
        "Seeking a path toward healing, I turned to the ancient ways...",
        "Psychedelic Assisted Therapy (PAT) in the sacred space of my ancestors.",
        "I find myself at the threshold of a Hogan. The ceremony is about to begin."
    };

    void Start()
    {
        // Ensure the text starts invisible
        if (canvasGroup != null) canvasGroup.alpha = 0;
        
        StartCoroutine(PlayNarrative());
        
        if (FMODEvents.instance != null)
        {
            AudioManager.instance.CreateInstance(FMODEvents.instance.KaleidoscopeMusic).start();
        }
    }

    IEnumerator PlayNarrative()
    {
        yield return new WaitForSeconds(1.0f); // Short initial pause

        foreach (string beat in storyBeats)
        {
            narrativeText.text = beat;

            // Fade In
            yield return StartCoroutine(FadeCanvas(0, 1, fadeDuration));

            // Wait while player reads
            yield return new WaitForSeconds(displayDuration);

            // Fade Out
            yield return StartCoroutine(FadeCanvas(1, 0, fadeDuration));

            yield return new WaitForSeconds(1.0f); // Gap between lines
        }

        // Transition to the Hogan Hut scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}