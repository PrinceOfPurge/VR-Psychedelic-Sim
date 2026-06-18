using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloodPoolGrow : MonoBehaviour
{
    [Header("Growth Settings")]
    [Tooltip("Total duration of the pool's growth in seconds.")]
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

    void Start()
    {
        // Start as a tiny dot
        startScale = new Vector3(0.01f, 0.01f, 0.01f);
        transform.localScale = startScale;
        
        // Define the target destination for the scale
        targetScale = new Vector3(maxXZSize, maxYSize, maxXZSize);
        
        // Initialize Volume Profile components
        if (postProcessVolume == null)
            postProcessVolume = FindFirstObjectByType<Volume>();

        if (postProcessVolume != null && postProcessVolume.profile != null)
            postProcessVolume.profile.TryGet(out colorAdjustments);
    }

    public void StartPool()
    {
        isGrowing = true;
        timeElapsed = 0.0f;
        
        // Ensure the post-processing override is active when starting
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.overrideState = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            StartPool();
        
        if (isGrowing)
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timeElapsed / growthDuration);
            
            // 1. Handle physical pool growth
            float poolProgress = growthCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.Lerp(startScale, targetScale, poolProgress);

            // 2. Handle VR screen blood tinting
            if (colorAdjustments != null && useColorTint)
            {
                float tintIntensity = visionBloodCurve.Evaluate(normalizedTime);
                
                // Interpolate from a clean white (no tint) to the target blood color
                colorAdjustments.colorFilter.value = Color.Lerp(Color.white, bloodTintColor, tintIntensity);
            }

            if (normalizedTime >= 1.0f)
            {
                isGrowing = false;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up: Reset the color filter back to normal when scene changes or object is destroyed
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.value = Color.white;
            colorAdjustments.colorFilter.overrideState = false;
        }
    }
}