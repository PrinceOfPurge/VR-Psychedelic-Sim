using UnityEngine;

[ExecuteAlways]
public class HospitalOrbController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Light childLight;
    [SerializeField] private Transform playerCamera;

    [Header("Shader Integration")]
    [SerializeField] private string colorPropertyName = "_AuraGlowColor";
    [SerializeField] private string intensityPropertyName = "_EffectStrength";

    [Header("Global Intensity Control")]
    [Range(0f, 1f)] [SerializeField] private float sceneIntensity = 0.1f;
    [SerializeField] private Gradient orbColorGradient;
    [SerializeField] private float hdrBoostMin = 5f;
    [SerializeField] private float hdrBoostMax = 25f;

    [Header("Light Settings")]
    [SerializeField] private float minLightIntensity = 1.0f;
    [SerializeField] private float maxLightIntensity = 8.0f;
    [SerializeField] private float minLightRange = 5.0f;
    [SerializeField] private float maxLightRange = 20.0f;

    [Header("Other Settings")]
    [SerializeField] private float maxJitterAmount = 0.05f;
    [SerializeField] private float lookAwayMultiplier = 2.5f;

    private Vector3 basePosition;
    private Material orbMaterial;

    void Start()
    {
        basePosition = transform.localPosition;
        
        // Initialize Material properly for Play vs Edit mode
        if (orbRenderer != null)
        {
            if (Application.isPlaying)
                orbMaterial = orbRenderer.material;
            else
                orbMaterial = orbRenderer.sharedMaterial;
        }
    }

    void Update()
    {
        if (orbRenderer == null || childLight == null) return;

        // --- Suggestion 2: Audio & Look-Away Logic ---
        float finalIntensity = sceneIntensity;

        // Audio Placeholder for Omid:
        // finalIntensity = FMODHelper.GetEventIntensity(); 

        if (playerCamera != null)
        {
            // Calculate how much the player is looking at the orb
            Vector3 directionToOrb = (transform.position - playerCamera.position).normalized;
            float dot = Vector3.Dot(playerCamera.forward, directionToOrb);

            // If dot is low (looking away), boost intensity
            if (dot < 0.7f) // Adjust threshold as needed
            {
                float lookAwayFactor = Mathf.Clamp01(1f - dot);
                finalIntensity += lookAwayFactor * lookAwayMultiplier * sceneIntensity;
            }
        }

        // --- Color & Lighting ---
        Color currentColor = orbColorGradient.Evaluate(sceneIntensity);
        childLight.color = currentColor;
        childLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, finalIntensity);
        childLight.range = Mathf.Lerp(minLightRange, maxLightRange, finalIntensity);

        // Update Shader
        if (orbMaterial != null)
        {
            float currentBoost = Mathf.Lerp(hdrBoostMin, hdrBoostMax, finalIntensity);
            Color hdrColor = currentColor * currentBoost;
            
            orbMaterial.SetColor(colorPropertyName, hdrColor);
            orbMaterial.SetFloat(intensityPropertyName, finalIntensity);
        }

        // --- Suggestion 1: Jitter & Floating ---
        float floatOffset = Mathf.Sin(Time.time * 0.5f) * 0.1f;
        Vector3 jitterOffset = Vector3.zero;

        if (finalIntensity > 0.5f)
        {
            jitterOffset = Random.insideUnitSphere * (maxJitterAmount * finalIntensity);
        }

        transform.localPosition = basePosition + new Vector3(0, floatOffset, 0) + jitterOffset;
    }
}