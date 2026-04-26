using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct DistortedUVsInfoContainer
{
    public Material mat;
    public string shaderEffectParamName;
}

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
        // Ensure the scene starts without any lingering fade effects
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
        
        // Start the local wall-melting distortions, and call the trippy effects from SceneTransitionManager
        StartCoroutine(HutMaterialDistortionRoutine());
        StartCoroutine(TrippyEffects());
    }

    private IEnumerator HutMaterialDistortionRoutine()
    {
        // Wait for Phase 1 (Fog) to complete before walls start melting (matching your original sequence)
        float fogElapsed = 0f;

        while (fogElapsed < fogDuration)
        {
            fogElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(fogElapsed / fogDuration);
            
            // Change to the new volume
            descentVolume.weight = t;
        }
        
        yield return new WaitForSeconds(fogDuration);
        
        float elapsed = 0f;
        while (elapsed < hutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hutDuration);

            // Apply UV distortions to the Hogan interior materials
            foreach (var matInfo in hutMaterials)
            {
                if (matInfo.mat != null)
                    matInfo.mat.SetFloat(matInfo.shaderEffectParamName, t);
            }
            
            // Hue Shift
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

        // Instantly reset all environmental materials to their base state
        foreach (var matInfo in hutMaterials)
        {
            if (matInfo.mat != null)
                matInfo.mat.SetFloat(matInfo.shaderEffectParamName, 0f);
        }
    }

    private void OnApplicationQuit() => ResetEffects();
}