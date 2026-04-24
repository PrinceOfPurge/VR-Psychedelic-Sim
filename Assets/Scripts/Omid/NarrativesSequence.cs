using UnityEngine;
using TMPro;
using System.Collections;

public class NarrativeSequence : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI narrativeText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public string nextSceneName = "HoganHut";
    public float fadeDuration = 1.5f;
    public float extraReadTime = 2.0f; // Extra time after audio finishes

    // Updated story beats to match your 19 dialogue lines
    private string[] storyBeats = {
        "Ever since the hospital… nothing’s felt real.",
        "One minute I was at work… then I woke up with a doctor telling me I was dying.",
        "Since then, it’s just been one thing after another.",
        "Appointments… pills… people telling me to stay positive.",
        "None of it’s working.",
        "I can feel it getting worse.",
        "And no matter what I do… I can’t quiet my mind.",
        "I keep seeing things.",
        "Dreams… memories… my father… my kids…",
        "I don’t know what’s real anymore.",
        "Everything feels out of place… like I don’t belong in my own life anymore.",
        "I don’t feel like I’m walking in beauty anymore.",
        "They told me this might help.",
        "Psychedelic-assisted therapy.",
        "I don’t really know what to expect.",
        "I just know I can’t keep living like this.",
        "So… here I am.",
        "If this is my last chance to walk in beauty…",
        "Then I have to try."
    };

    void Start()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0;
        
        StartCoroutine(PlayNarrative());
        
        if (FMODEvents.instance != null && AudioManager.instance != null)
        {
            // Start the background music
            AudioManager.instance.CreateInstance(FMODEvents.instance.NarrativeMusic).start();
        }
    }

    IEnumerator PlayNarrative()
    {
        yield return new WaitForSeconds(2.0f); // Initial silence for atmosphere

        for (int i = 0; i < storyBeats.Length; i++)
        {
            narrativeText.text = storyBeats[i];

            // 1. Play Audio Line
            if (FMODEvents.instance.narrativeLines.Length > i)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.narrativeLines[i], transform.position);
            }

            // 2. Fade In Text
            yield return StartCoroutine(FadeCanvas(0, 1, fadeDuration));

            // 3. Wait (Wait for the audio to mostly finish + extra reading time)
            // If line 5 or 11, add a slightly longer pause for emotional weight
            float pause = (i == 4 || i == 10) ? extraReadTime + 2.0f : extraReadTime;
            yield return new WaitForSeconds(pause);

            // 4. Fade Out Text
            yield return StartCoroutine(FadeCanvas(1, 0, fadeDuration));

            yield return new WaitForSeconds(1.0f); // Gap between lines
        }

        // 5. Transition to Hogan Hut
        if (SceneTransitionManager.Instance != null)
        {
            // Use your manager to fade the master volume and screen to black
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName, false, 3.0f);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
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