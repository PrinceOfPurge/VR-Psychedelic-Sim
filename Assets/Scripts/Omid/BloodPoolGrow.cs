using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloodPoolGrow : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthDuration = 3.0f;
    public float maxXZSize = 8.0f; 
    public float maxYSize = 5.0f; 

    [Header("Ease Settings")]
    [SerializeField] private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("URP VR Blood Screen Settings")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Color bloodTintColor = Color.red;
    [SerializeField] private AnimationCurve visionBloodCurve = AnimationCurve.EaseInOut(0, 0, 1, 0.5f);
    [SerializeField] private bool useColorTint = true;
    
    private ColorAdjustments colorAdjustments;
    private bool isGrowing = false;
    public bool IsGrowing => isGrowing;
    private float timeElapsed = 0.0f;
    
    private Vector3 startScale;
    private Vector3 targetScale;
    
    // FIX: Flags to lock the system from being re-triggered by overlapping clone attacks
    private bool hasStartedGrowing = false;
    private bool hasFinishedGrowing = false; 

    void Start()
    {
        startScale = new Vector3(0.01f, 0.01f, 0.01f);
        transform.localScale = startScale;
        
        targetScale = new Vector3(maxXZSize, maxYSize, maxXZSize);
        
        if (postProcessVolume == null)
            postProcessVolume = FindFirstObjectByType<Volume>();

        if (postProcessVolume != null && postProcessVolume.profile != null)
            postProcessVolume.profile.TryGet(out colorAdjustments);
    }

    public void StartPool()
    {
        // FIX: If the pool has already been kicked off by a clone, completely ignore subsequent hits
        if (hasStartedGrowing || hasFinishedGrowing) return;

        isGrowing = true;
        hasStartedGrowing = true;
        timeElapsed = 0.0f;
        
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.overrideState = true;
        }
    }

    void Update()
    {
        if (isGrowing)
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timeElapsed / growthDuration);
            
            float poolProgress = growthCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.Lerp(startScale, targetScale, poolProgress);

            if (colorAdjustments != null && useColorTint)
            {
                float tintIntensity = visionBloodCurve.Evaluate(normalizedTime);
                colorAdjustments.colorFilter.value = Color.Lerp(Color.white, bloodTintColor, tintIntensity);
            }

            if (normalizedTime >= 1.0f)
            {
                isGrowing = false;
                hasFinishedGrowing = true;
                transform.localScale = targetScale; // Hard-lock the pool at its absolute max scale
            }
        }
    }
    
    private void OnDestroy()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.value = Color.white;
            colorAdjustments.colorFilter.overrideState = false;
        }
    }
}