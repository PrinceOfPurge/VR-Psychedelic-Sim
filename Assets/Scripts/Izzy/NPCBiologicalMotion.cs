using System.Collections;
using UnityEngine;

public class NPCBiologicalMotion : MonoBehaviour
{
    [Header("Gaze (Rotation)")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private bool shouldLookAtPlayer = false;
    [SerializeField] private float initialPlayerLookDelay = 0f;
    
    [Header("Breathing (Scaling)")]
    [SerializeField] private float breatheFrequency = 0.15f; // 1 breath every ~6.5s
    [SerializeField] private float breatheAmount = 0.1f;    // 10% scale change
    
    [Header("Idle Bobbing (Floating)")]
    [SerializeField] private float bobFrequency = 0.5f;
    [SerializeField] private float bobAmount = 0.05f;       // 5cm bobbing

    [Header("Light Pulsing (Self)")]
    [SerializeField] private Light npcLight;
    [SerializeField] private float lightMinIntensity = 1.0f;
    [SerializeField] private float lightMaxIntensity = 3.0f;
    
    [Header("Environmental Sync")]
    [Tooltip("Reference to the main hut/environmental light. IMPORTANT: Only one NPC in the scene should have this assigned to prevent conflicting intensity updates.")]
    [SerializeField] private Light hutLight;
    [SerializeField] private float hutLightMin = 0.5f;
    [SerializeField] private float hutLightMax = 1.5f;
    [SerializeField] bool invertHutSync = false; // Optional: Makes room dim when NPC glows

    private Vector3 initialScale;
    private Vector3 initialPosition;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.localPosition;
        
        if (npcLight == null) npcLight = GetComponentInChildren<Light>();
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
        // Formula: Scale = Base + sin(Time * Freq) * Amount
        float breatheSin = Mathf.Sin(time * breatheFrequency * (2 * Mathf.PI));
        transform.localScale = initialScale + (Vector3.one * (breatheSin * breatheAmount));

        // 2. Idle Bobbing (Y-Position)
        float bobSin = Mathf.Sin(time * bobFrequency * (2 * Mathf.PI));
        transform.localPosition = initialPosition + (Vector3.up * (bobSin * bobAmount));

        // 3. Smooth Gaze
        if (playerTransform != null)
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0; // Keep the cloud level
            
            if (direction != Vector3.zero && shouldLookAtPlayer)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        
        // 4. Light Syncing
        float normalizedSin = (breatheSin + 1f) / 2f; 

        // Pulse Self
        if (npcLight != null)
        {
            npcLight.intensity = Mathf.Lerp(lightMinIntensity, lightMaxIntensity, normalizedSin);
        }

        // Pulse Hut Light
        if (hutLight != null)
        {
            float hutT = invertHutSync ? (1f - normalizedSin) : normalizedSin;
            hutLight.intensity = Mathf.Lerp(hutLightMin, hutLightMax, hutT);
        }
    }
    
    // Call this if the Initializer moves the object to reset its "base" position
    public void ResetBasePosition() => initialPosition = transform.localPosition;
}