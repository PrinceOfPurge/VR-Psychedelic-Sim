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
    [Range(0f, 1f)] public float headLightIntensity = 0f; 
    [SerializeField] private float headLightMin = 0.5f;
    [SerializeField] private float headLightMax = 4.0f; // Slightly lowered for safety
    
    [Header("Audio Remapping Settings")]
    [Tooltip("The range coming from FMOD/Audio input")]
    [SerializeField] private Vector2 audioInputRange = new Vector2(0.8f, 1.0f);
    [Tooltip("The range we map it to for the light intensity")]
    [SerializeField] private Vector2 lightOutputRange = new Vector2(0.0f, 1.0f);

    [Header("Talking Effect (Scene Sequence Sync)")]
    [Range(0f, 1f)] public float talkingWeight = 0f; 
    [SerializeField] private float talkingLightBoost = 1.5f; 

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
        
        // 4. Body Light Pulse
        float bodySin = Mathf.Sin(time * bodyLightFrequency * (2 * Mathf.PI));
        float bodyNormalized = (bodySin + 1f) / 2f; 
        if (npcBodyLight != null)
        {
            npcBodyLight.intensity = Mathf.Lerp(bodyLightMinIntensity, bodyLightMaxIntensity, bodyNormalized);
        }

        // --- IMPROVED HEAD LIGHT LOGIC ---
        if (npcHeadLight != null)
        {
            // 1. Remap the audio input (0.8 - 1.0) to a clean (0.0 - 1.0)
            float remappedAudio = Map(headLightIntensity, audioInputRange.x, audioInputRange.y, lightOutputRange.x, lightOutputRange.y);
            
            // 2. Apply that remapped value to our intensity range
            float audioPulse = Mathf.Lerp(headLightMin, headLightMax, remappedAudio);
            
            // 3. Calculate constant glow
            float constantGlow = talkingWeight * talkingLightBoost;

            // 4. Combine and CLAMP to prevent "Black Box" bloom artifacts
            // This ensures the light never hits a value that breaks the post-processing
            npcHeadLight.intensity = Mathf.Clamp(audioPulse + constantGlow, 0f, 8f);
        }

        // 5. Hut Light
        if (hutLight != null)
        {
            float hutT = invertHutSync ? (1f - bodyNormalized) : bodyNormalized;
            hutLight.intensity = Mathf.Lerp(hutLightMin, hutLightMax, hutT);
        }
    }

    // Helper function to remap ranges
    private float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        // Prevent division by zero if the input range min and max are identical
        if (Mathf.Abs(toSource - fromSource) < Mathf.Epsilon)
        {
            return fromTarget; 
        }
    
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
    
    public void ResetBasePosition() => initialPosition = transform.localPosition;
}