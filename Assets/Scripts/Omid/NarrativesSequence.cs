using UnityEngine;
using TMPro;
using System.Collections;

public class NarrativeSequence : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI narrativeText;
    public CanvasGroup canvasGroup;

    [Header("Settings - Pacing")]
    public string nextSceneName = "HoganHut";
    public float fadeDuration = 0.5f;       // Reduced from 1.5s for snappiness
    public float baseReadTime = 1.0f;       // Minimum time text stays on screen
    public float charReadModifier = 0.04f;  // Extra time per character (keeps it dynamic)
    public float lineGap = 0.3f;            // Reduced from 1.0s to keep momentum

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
            AudioManager.instance.CreateInstance(FMODEvents.instance.NarrativeMusic).start();
        }
    }

    IEnumerator PlayNarrative()
    {
        // Initial atmosphere (shortened)
        yield return new WaitForSeconds(1.0f); 

        for (int i = 0; i < storyBeats.Length; i++)
        {
            narrativeText.text = storyBeats[i];

            // 1. Play Audio Line
            if (FMODEvents.instance.narrativeLines.Length > i)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.narrativeLines[i], transform.position);
            }

            // 2. Fade In Text (Faster)
            yield return StartCoroutine(FadeCanvas(0, 1, fadeDuration));

            // 3. Dynamic Wait 
            // Calculates time based on sentence length, clamped between 1.5s and 4s
            float dynamicPause = baseReadTime + (storyBeats[i].Length * charReadModifier);
            float finalPause = Mathf.Clamp(dynamicPause, 1.5f, 4.0f);
            
            // Add extra weight to the emotional beats
            if (i == 4 || i == 10) finalPause += 1.5f;

            // NEW: Wait for time OR user click to skip ahead
            float timer = 0;
            while (timer < finalPause)
            {
                timer += Time.deltaTime;
                // Allow player to click/tap to advance immediately
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) 
                    break; 
                yield return null;
            }

            // 4. Fade Out Text (Faster)
            yield return StartCoroutine(FadeCanvas(1, 0, fadeDuration));

            // 5. Gap between lines (Snappier)
            yield return new WaitForSeconds(lineGap);
        }

        // Transition to Hogan Hut
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName, false, 2.0f);
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