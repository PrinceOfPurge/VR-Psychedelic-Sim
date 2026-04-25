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

    [Header("Sequence References")]
    [SerializeField] private StartMedicine medicineScript;
    [SerializeField] private float linePadding = 1.0f;

    [Header("Interaction Visuals")]
    [SerializeField] private GameObject keyPreview; 
    [SerializeField] private GameObject tableSocket;
    [SerializeField] private GameObject pillObject; 
    [SerializeField] private bool enablePulseEffect = true;

    [Header("After Effects (Hands/Camera)")]
    [SerializeField] private MonoBehaviour[] visualEffects; // Drag both hand shader scripts here

    private EventInstance drumInstance;
    private EventInstance navajoInstance;
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

        // Ensure all hand/camera effects are disabled at the start
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

        // --- WAITING FOR PILL ---
        while (!pillTaken) yield return null;

        // ACTIVATE ALL SHADERS (Left Hand, Right Hand, etc.)
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

        yield return StartCoroutine(FadeOutAllMusic(5.0f));

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.PerformFade(false, nextSceneName, false, 5.0f);
    }
    
    private IEnumerator PlayLine(FMODUnity.EventReference line, GameObject source)
    {
        Vector3 pos = (source != null) ? source.transform.position : Camera.main.transform.position;
        AudioManager.instance.PlayOneShot(line, pos);
        yield return new WaitForSeconds(linePadding + 2.5f); 
    }

    private IEnumerator FadeOutAllMusic(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float vol = Mathf.Lerp(1f, 0f, elapsed / duration);
            if (drumInstance.isValid()) drumInstance.setVolume(vol);
            if (navajoInstance.isValid()) navajoInstance.setVolume(vol);
            yield return null;
        }
        if (drumInstance.isValid()) { drumInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); drumInstance.release(); }
        if (navajoInstance.isValid()) { navajoInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); navajoInstance.release(); }
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
        if (drumInstance.isValid()) { drumInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); drumInstance.release(); }
        if (navajoInstance.isValid()) { navajoInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); navajoInstance.release(); }
    }

    public void SetKeyPlaced(bool v) => keyPlaced = v;
    public void SetPillTaken(bool v) => pillTaken = v;
}