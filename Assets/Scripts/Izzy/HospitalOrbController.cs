using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class HospitalOrbController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Material voidMaterial;
    [SerializeField] private Light childLight;
    [SerializeField] private Transform playerCamera;

    [Header("Shader Integration")]
    [SerializeField] private string orbGlowColorPropertyName = "_AuraGlowColor";
    [SerializeField] private string intensityPropertyName = "_EffectStrength";
    [SerializeField] private string voidColorPropertyName = "_Color";

    [Header("Global Intensity Control")]
    [Range(0f, 1f)] [SerializeField] private float sceneIntensity = 0.1f;
    [SerializeField] private Gradient orbColorGradient;
    [SerializeField] private float hdrBoostMin = 5f;
    [SerializeField] private float hdrBoostMax = 25f;

    [Header("Transform Scaling & Motion")]
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 4.0f;
    [SerializeField] private float minRotationSpeed = 10.0f;
    [SerializeField] private float maxRotationSpeed = 200.0f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatFrequency = 0.5f;

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

        // --- Intensity Logic ---
        float finalIntensity = sceneIntensity;

        if (playerCamera != null)
        {
            Vector3 directionToOrb = (transform.position - playerCamera.position).normalized;
            float dot = Vector3.Dot(playerCamera.forward, directionToOrb);

            if (dot < 0.7f)
            {
                float lookAwayFactor = Mathf.Clamp01(1f - dot);
                finalIntensity += lookAwayFactor * lookAwayMultiplier * sceneIntensity;
            }
        }

        // Clamp intensity to 1.0 for the non-shader calculations
        float clampedIntensity = Mathf.Clamp01(finalIntensity);

        // --- Scaling & Rotation ---
        // Increase size based on intensity
        float currentScale = Mathf.Lerp(minScale, maxScale, clampedIntensity);
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        // Aggressive rotation - rotates on multiple axes for a "trippier" look
        float currentRotationSpeed = Mathf.Lerp(minRotationSpeed, maxRotationSpeed, clampedIntensity);
        transform.Rotate(new Vector3(1, 1.5f, 0.5f) * currentRotationSpeed * Time.deltaTime);

        // --- Color & Lighting ---
        Color currentColor = orbColorGradient.Evaluate(clampedIntensity);
        childLight.color = currentColor;
        childLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, clampedIntensity);
        childLight.range = Mathf.Lerp(minLightRange, maxLightRange, clampedIntensity);

        // --- Shader Updates ---
        if (orbMaterial != null)
        {
            float currentBoost = Mathf.Lerp(hdrBoostMin, hdrBoostMax, clampedIntensity);
            Color hdrColor = currentColor * currentBoost;
            
            orbMaterial.SetColor(orbGlowColorPropertyName, hdrColor);
            orbMaterial.SetFloat(intensityPropertyName, finalIntensity);
        }

        if (voidMaterial != null)
        {
            voidMaterial.SetColor(voidColorPropertyName, currentColor);
        }

        // --- Jitter & Floating ---
        // Floating gets faster/higher as it gets more intense
        float currentFloatFreq = floatFrequency * (1f + clampedIntensity);
        float currentFloatAmp = floatAmplitude * (1f + (clampedIntensity * 0.5f));
        float floatOffset = Mathf.Sin(Time.time * currentFloatFreq) * currentFloatAmp;
        
        Vector3 jitterOffset = Vector3.zero;
        if (clampedIntensity > 0.5f)
        {
            jitterOffset = Random.insideUnitSphere * (maxJitterAmount * clampedIntensity);
        }

        transform.localPosition = basePosition + new Vector3(0, floatOffset, 0) + jitterOffset;
    }
}