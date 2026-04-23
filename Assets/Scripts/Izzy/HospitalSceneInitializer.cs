using UnityEngine;
using System.Collections;

public class HospitalSceneInitializer : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float fadeInDuration = 3f;
    [SerializeField] private float minToMaxIntensityDuration = 8f;
    [SerializeField] private float waitAtMaxDuration = 2f;
    [SerializeField] private string nextSceneName = "Level3_Valley";
    
    [Header("Dialogue Padding")]
    [Tooltip("Extra time to wait after each line so they don't overlap.")]
    [SerializeField] private float linePadding = 1.5f;

    [Header("References")]
    [SerializeField] private HospitalOrbController hospitalOrbController;
    
    void Start()
    {
        // Start the sequence as soon as the level loads
        StartCoroutine(HospitalSequenceRoutine());
    }

    private IEnumerator HospitalSequenceRoutine()
    {
        // 1. TRIGGER REVERSE FADE (Waking up)
        if (SceneTransitionManager.Instance != null)
        {
            // true = reverse, "" = no scene load yet, true = use blink effect
            SceneTransitionManager.Instance.PerformFade(true, "", true, fadeInDuration);
        }
        
        yield return new WaitForSeconds(fadeInDuration);

        // Get dialogue from our FMOD Library
        var lines = FMODEvents.instance.hospitalLines;

        // --- PHASE 1: Entry (Intensity is 0) ---
        yield return PlayDialogue(lines[0]); // "Hey... can you hear me?"
        yield return PlayDialogue(lines[1]); // "Do you know where you are?"
        yield return PlayDialogue(lines[2]); // "You're in the hospital."
        yield return PlayDialogue(lines[3]); // "You passed out earlier."
        yield return PlayDialogue(lines[4]); // "We ran some tests..."
        yield return PlayDialogue(lines[5]); // "I need to talk to you..."

        // --- PHASE 2: Diagnosis (Intensity Ramps UP) ---
        // This is your EXACT original intensity logic, triggered alongside Line 7
        float elapsed = 0f;
        
        // Start playing the "Big Reveal" line
        AudioManager.instance.PlayOneShot(lines[6], transform.position); // "We found a mass..."

        while (elapsed < minToMaxIntensityDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / minToMaxIntensityDuration;
            
            if (hospitalOrbController != null) 
                hospitalOrbController.SceneIntensity = t;

            yield return null; 
        }
        
        // --- PHASE 3: Continue Dialogue while Intensity is MAX ---
        yield return PlayDialogue(lines[7]);  // "It's in your frontal lobe."
        yield return PlayDialogue(lines[8]);  // "Glioblastoma."
        yield return PlayDialogue(lines[9]);  // "Aggressive..."
        yield return new WaitForSeconds(2f);  // Impact pause
        yield return PlayDialogue(lines[10]); // "At this stage..."
        yield return PlayDialogue(lines[11]); // "Terminal."

        // --- PHASE 4: Dissociation ---
        yield return PlayDialogue(lines[12]); 
        yield return PlayDialogue(lines[13]);
        yield return PlayDialogue(lines[14]);
        yield return PlayDialogue(lines[15]);

        // --- PHASE 5: Final Lines ---
        yield return PlayDialogue(lines[16]);
        yield return PlayDialogue(lines[17]);
        yield return PlayDialogue(lines[18]);

        Debug.Log("Dialogue and Max intensity reached!");

        // 3. THE WAIT TIME
        yield return new WaitForSeconds(waitAtMaxDuration);

        // 4. FADE OUT TO NEXT SCENE
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
        }
    }

    // Helper method to play a line and wait for it to finish
    private IEnumerator PlayDialogue(FMODUnity.EventReference line)
    {
        AudioManager.instance.PlayOneShot(line, transform.position);
        // We wait for a base time (padding) so the doctor isn't rapid-firing lines
        yield return new WaitForSeconds(linePadding + 2.0f); 
    }
}