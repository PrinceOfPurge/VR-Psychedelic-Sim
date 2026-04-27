using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using FMOD.Studio; 
using FMODUnity;   

public class HoganSceneInitializer : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private float fadeDuration = 3.0f;
    [SerializeField] private string nextSceneName = "Level5_Starweaver";
    
    [Header("Therapist Entry")]
    [SerializeField] private GameObject therapistNPC;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float entryDelay = 3.0f; 
    [SerializeField] private float walkDuration = 5.0f;

    [Header("Cloud NPCs (Talking Sync)")]
    [SerializeField] private NPCBiologicalMotion therapistCloud;
    [SerializeField] private NPCBiologicalMotion healerCloud;
    [SerializeField] private float talkingFadeDuration = 0.4f;

    [Header("Sequence References")]
    [SerializeField] private StartMedicine medicineScript;
    [SerializeField] private float linePadding = 1.0f;

    [Header("Interaction Visuals")]
    [SerializeField] private GameObject keyPreview; 
    [SerializeField] private GameObject tableSocket;
    [SerializeField] private GameObject pillObject; 
    [SerializeField] private bool enablePulseEffect = true;

    [Header("After Effects (Hands/Camera)")]
    [SerializeField] private MonoBehaviour[] visualEffects; 

    private EventInstance drumInstance;
    private EventInstance navajoInstance;
    private EventInstance currentDialogueInstance; // Track dialogue for fading
    public bool keyPlaced = false;
    public bool pillTaken = false;

    private Coroutine mainSequence;
    private bool isSkipping = false;
    private bool shouldPulse = false;
    private Vector3 originalPreviewScale;

    private void Start()
    {
        if (FMODEvents.instance != null)
        {
            if (!FMODEvents.instance.HoganDesMusic.IsNull)
                drumInstance = RuntimeManager.CreateInstance(FMODEvents.instance.HoganDesMusic);
            
            if (!FMODEvents.instance.NavajoMusic.IsNull)
                navajoInstance = RuntimeManager.CreateInstance(FMODEvents.instance.NavajoMusic);
        }

        if (keyPreview != null) { originalPreviewScale = keyPreview.transform.localScale; keyPreview.SetActive(false); }
        if (pillObject != null) { pillObject.SetActive(false); }

        if (visualEffects != null)
        {
            foreach (var effect in visualEffects)
            {
                if (effect != null) effect.enabled = false;
            }
        }

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformFade(true, "", true, fadeDuration);
    
        mainSequence = StartCoroutine(MainNarrativeSequence());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && !keyPlaced && !isSkipping)
        {
            StopCoroutine(mainSequence);
            isSkipping = true;
            StartCoroutine(SkipToKeyPhase());
        }

        if (shouldPulse && keyPreview != null && enablePulseEffect)
        {
            float pulse = (Mathf.Sin(Time.time * 2.5f) * 0.12f) + 1.0f;
            keyPreview.transform.localScale = originalPreviewScale * pulse;
        }
    }

    private IEnumerator SkipToKeyPhase()
    {
        therapistNPC.transform.position = endPoint.position;
        yield return null; 
        mainSequence = StartCoroutine(MainNarrativeSequence(true));
    }

    private IEnumerator MainNarrativeSequence(bool jumpedToKey = false)
    {
        var h = FMODEvents.instance.hoganDialogue;  
        var healer = FMODEvents.instance.healerDialogue;

        if (!jumpedToKey)
        {
            yield return new WaitForSeconds(entryDelay);
            StartCoroutine(MoveTherapistRoutine()); 

            yield return PlayLine(h[0], therapistNPC);        
            yield return PlayLine(h[1], therapistNPC);        
            yield return PlayLine(healer[0], null);           
            yield return PlayLine(healer[1], null);           

            yield return PlayLine(h[2], Camera.main.gameObject); 
            yield return PlayLine(h[3], Camera.main.gameObject); 
            yield return PlayLine(h[4], Camera.main.gameObject); 

            yield return PlayLine(h[5], therapistNPC);        
            yield return PlayLine(h[6], therapistNPC);        
            yield return PlayLine(h[7], therapistNPC);        
            yield return PlayLine(h[8], therapistNPC);        
            yield return PlayLine(h[9], therapistNPC);        

            yield return PlayLine(healer[2], null);           
            yield return PlayLine(healer[3], null);           
            yield return PlayLine(h[10], therapistNPC);       
            yield return PlayLine(h[11], therapistNPC);       
            yield return new WaitForSeconds(3f);
        }

        if (keyPreview != null) { keyPreview.SetActive(true); shouldPulse = true; }
        if (tableSocket != null) tableSocket.SetActive(true);

        yield return PlayLine(healer[4], null); 
        yield return PlayLine(healer[5], null); 
        yield return PlayLine(healer[6], null); 

        while (!keyPlaced) yield return null; 

        shouldPulse = false;
        if (keyPreview != null) keyPreview.SetActive(false);
        if (tableSocket != null)
        {
            var socket = tableSocket.GetComponent<XRSocketInteractor>();
            if (socket != null) socket.showInteractableHoverMeshes = false;
        }

        yield return PlayLine(h[12], therapistNPC); 
        yield return PlayLine(h[13], therapistNPC); 
        yield return PlayLine(h[14], therapistNPC); 
        yield return PlayLine(h[15], therapistNPC); 

        yield return PlayLine(h[16], therapistNPC); 
        yield return PlayLine(h[17], therapistNPC); 
        yield return PlayLine(h[18], therapistNPC); 
        yield return PlayLine(healer[7], null);     
        yield return PlayLine(healer[8], null);     
        yield return PlayLine(h[19], therapistNPC); 

        if (pillObject != null) pillObject.SetActive(true);

        while (!pillTaken) yield return null;

        if (visualEffects != null)
        {
            foreach (var effect in visualEffects)
            {
                if (effect != null) effect.enabled = true;
            }
        }

        if (medicineScript != null) medicineScript.StartTrip();
        
        yield return PlayLine(h[20], therapistNPC); 
        yield return PlayLine(h[21], therapistNPC); 
        yield return PlayLine(h[22], therapistNPC); 

        if (drumInstance.isValid()) drumInstance.start(); 

        yield return PlayLine(healer[9], null);  
        yield return PlayLine(healer[10], null); 
        yield return new WaitForSeconds(2f);
        yield return PlayLine(healer[11], null); 
        yield return PlayLine(healer[12], null); 
        yield return PlayLine(healer[13], null); 
        
        yield return PlayLine(h[23], therapistNPC); 
        yield return PlayLine(h[24], therapistNPC); 

        if (navajoInstance.isValid()) navajoInstance.start(); 
        yield return PlayLine(h[25], therapistNPC); 

        yield return PlayLine(healer[14], null);    
        yield return PlayLine(h[26], therapistNPC); 
        yield return PlayLine(h[27], therapistNPC); 

        // --- FADE OUT START ---
        // We fade everything together over 5 seconds
        yield return StartCoroutine(FadeOutAllMusic(5.0f));

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName, false, 5.0f);
    }
    
    private IEnumerator PlayLine(FMODUnity.EventReference line, GameObject source)
    {
        if (line.IsNull) yield break;

        NPCBiologicalMotion currentSpeaker = null;
        if (source == therapistNPC) currentSpeaker = therapistCloud;
        else if (source == null) currentSpeaker = healerCloud;

        if (currentSpeaker != null) StartCoroutine(FadeTalkingWeight(currentSpeaker, 1f));

        Vector3 pos = (source != null) ? source.transform.position : Camera.main.transform.position;
        currentDialogueInstance = AudioManager.instance.CreateInstance(line);
        currentDialogueInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(pos));
        currentDialogueInstance.start();

        PLAYBACK_STATE state;
        currentDialogueInstance.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPED)
        {
            currentDialogueInstance.getPlaybackState(out state);
            yield return null; 
        }

        yield return new WaitForSeconds(linePadding);

        if (currentSpeaker != null) StartCoroutine(FadeTalkingWeight(currentSpeaker, 0f));
        currentDialogueInstance.release();
    }

    private IEnumerator FadeTalkingWeight(NPCBiologicalMotion npc, float target)
    {
        float start = npc.talkingWeight;
        float elapsed = 0;
        while (elapsed < talkingFadeDuration)
        {
            elapsed += Time.deltaTime;
            npc.talkingWeight = Mathf.Lerp(start, target, elapsed / talkingFadeDuration);
            yield return null;
        }
        npc.talkingWeight = target;
    }

    private IEnumerator FadeOutAllMusic(float duration)
    {
        float elapsed = 0;
        // Also capture the volume of current dialogue if it's still playing
        float dialogueVol = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float vol = Mathf.Lerp(1f, 0f, t);
            
            if (drumInstance.isValid()) drumInstance.setVolume(vol);
            if (navajoInstance.isValid()) navajoInstance.setVolume(vol);
            
            // Fade the dialogue too just in case one is active
            if (currentDialogueInstance.isValid()) currentDialogueInstance.setVolume(vol);

            yield return null;
        }

        // Clean stop all
        if (drumInstance.isValid()) { drumInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); drumInstance.release(); }
        if (navajoInstance.isValid()) { navajoInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); navajoInstance.release(); }
        if (currentDialogueInstance.isValid()) { currentDialogueInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); currentDialogueInstance.release(); }
    }

    private IEnumerator MoveTherapistRoutine()
    {
        float elapsed = 0;
        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / walkDuration);
            therapistNPC.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            therapistNPC.GetComponent<NPCBiologicalMotion>()?.ResetBasePosition();
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (drumInstance.isValid()) { drumInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); drumInstance.release(); }
        if (navajoInstance.isValid()) { navajoInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); navajoInstance.release(); }
        if (currentDialogueInstance.isValid()) { currentDialogueInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); currentDialogueInstance.release(); }
    }

    public void SetKeyPlaced(bool v) => keyPlaced = v;
    public void SetPillTaken(bool v) => pillTaken = v;
}