using UnityEngine;
using System.Collections;
using FMOD.Studio;

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

    private EventInstance musicInstance;
    private EventInstance tinnitusInstance; // To track the ringing
    
    void Start()
    {
        // 1. Initialize and Start Music immediately
        if (FMODEvents.instance != null && !FMODEvents.instance.HospitalMusic.IsNull)
        {
            musicInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.HospitalMusic);
            musicInstance.start();
        }

        StartCoroutine(HospitalSequenceRoutine());
    }

    private IEnumerator HospitalSequenceRoutine()
    {
        // --- THE WAKE UP MOMENT ---
        
        // 1. Play the Gasp (Sudden intake of breath)
        AudioManager.instance.PlayOneShot(FMODEvents.instance.gasp, transform.position);

        // 2. Start Tinnitus Ringing
        tinnitusInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.tinnitus);
        tinnitusInstance.start();

        // 3. TRIGGER REVERSE FADE (Blink open)
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(true, "", true, fadeInDuration);
        }
        
        yield return new WaitForSeconds(fadeInDuration);

        // 4. Fade out Tinnitus over the first few lines of dialogue
        // This makes the doctor's voice feel like it's "breaking through" the trauma
        StartCoroutine(FadeOutTinnitus(5f));

        var lines = FMODEvents.instance.hospitalLines;

        // --- PHASE 1: Entry (Intensity is 0) ---
        yield return PlayDialogue(lines[0]);
        yield return PlayDialogue(lines[1]);
        yield return PlayDialogue(lines[2]);
        yield return PlayDialogue(lines[3]);
        yield return PlayDialogue(lines[4]);
        yield return PlayDialogue(lines[5]);

        // --- PHASE 2: Diagnosis (Intensity Ramps UP) ---
        float elapsed = 0f;
        AudioManager.instance.PlayOneShot(lines[6], transform.position); 

        while (elapsed < minToMaxIntensityDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / minToMaxIntensityDuration;
            
            if (hospitalOrbController != null) 
                hospitalOrbController.SceneIntensity = t;

            yield return null; 
        }
        
        // --- PHASE 3: Continue Dialogue ---
        yield return PlayDialogue(lines[7]);
        yield return PlayDialogue(lines[8]);
        yield return PlayDialogue(lines[9]);
        yield return new WaitForSeconds(2f);
        yield return PlayDialogue(lines[10]);
        yield return PlayDialogue(lines[11]);

        // --- PHASE 4: Dissociation ---
        yield return PlayDialogue(lines[12]); 
        yield return PlayDialogue(lines[13]);
        yield return PlayDialogue(lines[14]);
        yield return PlayDialogue(lines[15]);

        // --- PHASE 5: Final Lines ---
        yield return PlayDialogue(lines[16]);
        yield return PlayDialogue(lines[17]);
        yield return PlayDialogue(lines[18]);

        yield return new WaitForSeconds(waitAtMaxDuration);

        // --- CLEANUP ---
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        //musicInstance.release();
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName);
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator PlayDialogue(FMODUnity.EventReference line)
    {
        AudioManager.instance.PlayOneShot(line, transform.position);
        yield return new WaitForSeconds(linePadding + 2.0f); 
    }

    // New helper to fade the ringing noise out smoothly
    private IEnumerator FadeOutTinnitus(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tinnitusInstance.setVolume(Mathf.Lerp(1f, 0f, elapsed / duration));
            yield return null;
        }
        tinnitusInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        tinnitusInstance.release();
    }

    private void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
        musicInstance.release();
    
        tinnitusInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        tinnitusInstance.release();
    }
}