using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class NPCBiologicalMotion : MonoBehaviour
{
    [Header("Gaze (Rotation)")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private bool shouldLookAtPlayer = false;
    [SerializeField] private float initialPlayerLookDelay = 0f;
    
    [Header("Breathing (Scaling)")]
    [SerializeField] private float breatheFrequency = 0.15f; 
    [SerializeField] private float breatheAmount = 0.1f;    
    
    [Header("Idle Bobbing (Floating)")]
    [SerializeField] private float bobFrequency = 0.5f;
    [SerializeField] private float bobAmount = 0.05f;       

    [Header("Light Pulsing (Body)")]
    [SerializeField] private Light npcBodyLight;
    [SerializeField] private float bodyLightFrequency = 0.5f;
    [FormerlySerializedAs("lightMinIntensity"),SerializeField] private float bodyLightMinIntensity = 1.0f;
    [FormerlySerializedAs("lightMaxIntensity"),SerializeField] private float bodyLightMaxIntensity = 3.0f;

    [Header("Head Light Audio Sync")]
    [SerializeField] private Light npcHeadLight;
    [Range(0f, 1f)] public float headLightIntensity = 0f; // Driven by FMOD Amplitude (Teammate's Slider)
    [SerializeField] private float headLightMin = 0.5f;
    [SerializeField] private float headLightMax = 5.0f;
    
    [Header("Talking Effect (Scene Sequence Sync)")]
    [Range(0f, 1f)] public float talkingWeight = 0f; // Driven by HoganSceneInitializer
    [SerializeField] private float talkingLightBoost = 2.0f; // Static glow added when it's their turn to talk

    [Header("Environmental Sync")]
    [SerializeField] private Light hutLight;
    [SerializeField] private float hutLightMin = 0.5f;
    [SerializeField] private float hutLightMax = 1.5f;
    [SerializeField] bool invertHutSync = false; 

    private Vector3 initialScale;
    private Vector3 initialPosition;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.localPosition;
        
        if (npcBodyLight == null) npcBodyLight = GetComponentInChildren<Light>();
        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;

        StartCoroutine(BeginLookingAtPlayer());
    }

    private IEnumerator BeginLookingAtPlayer()
    {
        yield return new WaitForSeconds(initialPlayerLookDelay);
        shouldLookAtPlayer = true;
    }

    void Update()
    {
        float time = Time.time;

        // 1. Breathing (Scale)
        float breatheSin = Mathf.Sin(time * breatheFrequency * (2 * Mathf.PI));
        transform.localScale = initialScale + (Vector3.one * (breatheSin * breatheAmount));

        // 2. Idle Bobbing (Y-Position)
        float bobSin = Mathf.Sin(time * bobFrequency * (2 * Mathf.PI));
        transform.localPosition = initialPosition + (Vector3.up * (bobSin * bobAmount));

        // 3. Smooth Gaze
        if (playerTransform != null && shouldLookAtPlayer)
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0; 
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        
        // 4. Light Syncing
        float bodySin = Mathf.Sin(time * bodyLightFrequency * (2 * Mathf.PI));
        float bodyNormalized = (bodySin + 1f) / 2f; 
        if (npcBodyLight != null)
        {
            npcBodyLight.intensity = Mathf.Lerp(bodyLightMinIntensity, bodyLightMaxIntensity, bodyNormalized);
        }

        // --- COMBINED HEAD LIGHT LOGIC ---
        if (npcHeadLight != null)
        {
            // Calculate the jittery audio pulse from your teammate's slider
            float audioPulse = Mathf.Lerp(headLightMin, headLightMax, headLightIntensity);
            
            // Calculate a steady "glow" based on who the dialogue script says is talking
            float constantGlow = talkingWeight * talkingLightBoost;

            // Apply both: It pulses with audio, but stays bright while they are the active speaker
            npcHeadLight.intensity = audioPulse + constantGlow;
        }

        // 5. Hut Light
        if (hutLight != null)
        {
            float hutT = invertHutSync ? (1f - bodyNormalized) : bodyNormalized;
            hutLight.intensity = Mathf.Lerp(hutLightMin, hutLightMax, hutT);
        }
    }
    
    public void ResetBasePosition() => initialPosition = transform.localPosition;
}