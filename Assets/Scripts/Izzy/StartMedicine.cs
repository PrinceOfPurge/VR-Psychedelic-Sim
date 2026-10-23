using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class PPSequenceStep
{
    public string componentName; 
    [Range(0, 1)] public float startAtNormalized; 
    [Range(0, 1)] public float endAtNormalized;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [HideInInspector] public VolumeComponent component;
}

public class StartMedicine : MonoBehaviour
{
    [Header("Phase 1: The Fog (Durations must match Manager)")]
    [SerializeField] private float fogDuration = 5f;

    [Header("Phase 2: The Hut (Local Shader Distortions)")]
    [SerializeField] private DistortedUVsInfoContainer[] hutMaterials;
    [SerializeField] private float hutDuration = 15f;
    
    // FIX: Reference the hand scripts here so they delay until Phase 2 starts
    [Header("Phase 2: Hand After-Effects Integration")]
    [Tooltip("Drag your hand after-image / VR hand effect scripts here to delay them until Phase 2.")]
    [SerializeField] private MonoBehaviour[] handVisualEffects; 

    [Header("Phase 3: Weird Post-Processing")] 
    [SerializeField] private float trippyEffectsDelay = 10f;
    [SerializeField] private Volume descentVolume;
    
    [Header("Ethereal Lighting")]
    [SerializeField] private Light fireLight;
    [SerializeField] private Gradient tripColorGradient; // Set this in Inspector (Orange -> Teal/Purple)
    
    [Header("Transition Settings")]
    [SerializeField] private string starweaverSceneName = "Level5_Starweaver";
    [SerializeField] private float peakWaitDuration = 2f;

    private bool isTripActive = false;

    private void Start()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.ResetFadeEffect();
        
        ResetEffects();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) StartTrip();
        if (Input.GetKeyDown(KeyCode.G)) ResetEffects();
    }

    private void OnTriggerEnter(Collider other)
    {
        StartTrip();
    }

    public void StartTrip()
    {
        if (isTripActive) return;
        isTripActive = true;
        
        StartCoroutine(HutMaterialDistortionRoutine());
        StartCoroutine(TrippyEffects());
    }

    private IEnumerator HutMaterialDistortionRoutine()
    {
        float fogElapsed = 0f;

        while (fogElapsed < fogDuration)
        {
            fogElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(fogElapsed / fogDuration);
            descentVolume.weight = t;
            yield return null; // Added missing yield fix to prevent locking frame loops
        }
        
        yield return new WaitForSeconds(fogDuration);

        // FIX: The fog has ended! Turn on the hand after-images now as Phase 2 starts
        if (handVisualEffects != null)
        {
            foreach (var effect in handVisualEffects)
            {
                if (effect != null) effect.enabled = true;
            }
        }
        
        float elapsed = 0f;
        while (elapsed < hutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hutDuration);

            foreach (var matInfo in hutMaterials)
            {
                if (matInfo.mat != null)
                    matInfo.mat.SetFloat(matInfo.shaderEffectParamName, t);
            }
            
            if (fireLight != null)
            {
                fireLight.color = tripColorGradient.Evaluate(t);
            }

            yield return null;
        }
    }

    private IEnumerator TrippyEffects()
    {
        yield return new WaitForSeconds(trippyEffectsDelay);
        
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.StartTrippyEffects(descentVolume, starweaverSceneName, peakWaitDuration);
    }

    public void ResetEffects()
    {
        isTripActive = false;
        StopAllCoroutines();

        if (descentVolume != null) descentVolume.weight = 0f;

        foreach (var matInfo in hutMaterials)
        {
            if (matInfo.mat != null)
                matInfo.mat.SetFloat(matInfo.shaderEffectParamName, 0f);
        }

        // Reset hand visuals state safely
        if (handVisualEffects != null)
        {
            foreach (var effect in handVisualEffects) if (effect != null) effect.enabled = false;
        }
    }

    private void OnApplicationQuit() => ResetEffects();
}