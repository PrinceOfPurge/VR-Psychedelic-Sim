using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
// Weird PP Namespaces
using FronkonGames.Weird.Crystal;
using FronkonGames.Weird.Kaleidoscope;
using FronkonGames.Weird.Extruder;
using FronkonGames.Weird.Spiral;

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
    [Header("Phase 1: The Fog")]
    [SerializeField] private float fogDuration = 5f;
    [SerializeField] private float targetFogDensity = 30f;

    [Header("Phase 2: The Hut (Shader Distortions)")]
    [SerializeField] private DistortedUVsInfoContainer[] hutMaterials;
    [SerializeField] private float hutDuration = 15f;
    [Range(0, 1)] [SerializeField] private float ppStartThreshold = 0.8f;

    [Header("Phase 3: Weird Post-Processing")]
    [SerializeField] private Volume descentVolume;
    [SerializeField] private float ppSequenceDuration = 30f;
    [SerializeField] private List<PPSequenceStep> ppSteps;

    private enum TripState { Idle, FogRising, HutDistorting, WeirdSequence, Complete }
    private TripState currentState = TripState.Idle;

    private float timer;
    private bool ppStarted = false;

    
    private void Start()
    {
        CacheComponents();
        ResetEffects();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) StartTrip();
        if (Input.GetKeyDown(KeyCode.G)) ResetEffects();

        HandleTransition();
    }

    private void OnTriggerEnter(Collider other)
    {
        StartTrip();
    }

    private void OnApplicationQuit()
    {
        ResetEffects();
    }

    private void StartTrip()
    {
        if (currentState != TripState.Idle) return;
        
        if (descentVolume != null) descentVolume.weight = 1f;
        
        currentState = TripState.FogRising;
        timer = 0;
    }

    private void HandleTransition()
    {
        switch (currentState)
        {
            case TripState.FogRising:
                UpdateFog();
                break;

            case TripState.HutDistorting:
                UpdateHut();
                break;

            case TripState.WeirdSequence:
                UpdateHut(); 
                UpdatePPSequence();
                break;
        }
    }

    private void UpdateFog()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fogDuration);
        
        SetFogDensity(t * targetFogDensity);

        if (t >= 1f)
        {
            currentState = TripState.HutDistorting;
            timer = 0;
        }
    }

    private void UpdateHut()
    {
        timer += Time.deltaTime;
        float hutT = Mathf.Clamp01(timer / hutDuration);

        foreach (var matInfo in hutMaterials)
        {
            matInfo.mat.SetFloat(matInfo.shaderEffectParamName, hutT);
        }

        if (!ppStarted && hutT >= ppStartThreshold)
        {
            ppStarted = true;
        }

        if (ppStarted)
        {
            UpdatePPSequence();
        }
    }

    private void UpdatePPSequence()
    {
        float ppTimer = timer - (hutDuration * ppStartThreshold);
        float globalPPT = Mathf.Clamp01(ppTimer / ppSequenceDuration);

        foreach (var step in ppSteps)
        {
            if (step.component == null) continue;

            float localT = Mathf.InverseLerp(step.startAtNormalized, step.endAtNormalized, globalPPT);
            float intensity = step.intensityCurve.Evaluate(localT);
            
            ApplyWeirdIntensity(step.component, intensity);
        }

        if (globalPPT >= 1f) currentState = TripState.Complete;
    }

    #region Component Logic

    private void CacheComponents()
    {
        if (descentVolume == null || descentVolume.profile == null) return;

        foreach (var step in ppSteps)
        {
            foreach (var comp in descentVolume.profile.components)
            {
                if (comp.name.Contains(step.componentName)) step.component = comp;
            }
        }
    }

    private void SetFogDensity(float value)
    {
        foreach (var comp in descentVolume.profile.components)
        {
            // Haze is likely a different custom component, so we still use reflection for its density field
            if (comp.name.Contains("Haze"))
            {
                var field = comp.GetType().GetField("globalDensityMultiplier");
                if (field != null)
                {
                    var param = field.GetValue(comp) as FloatParameter;
                    if (param != null)
                    {
                        param.overrideState = true;
                        param.value = value;
                    }
                }
            }
        }
    }

    private void ApplyWeirdIntensity(VolumeComponent comp, float value)
    {
        // Pattern-matching based on the docs for each specific Weird effect
        if (comp is KaleidoscopeVolume k)
        {
            k.intensity.overrideState = true;
            k.intensity.value = value;
        }
        else if (comp is ExtruderVolume e)
        {
            e.intensity.overrideState = true;
            e.intensity.value = value;
        }
        else if (comp is CrystalVolume c)
        {
            c.intensity.overrideState = true;
            c.intensity.value = value;
        }
        else if (comp is SpiralVolume s)
        {
            s.wrap.overrideState = true;
            s.wrap.value = value;
        }
    }

    public void ResetEffects()
    {
        currentState = TripState.Idle;
        timer = 0;
        ppStarted = false;

        if (descentVolume != null) descentVolume.weight = 0f;

        SetFogDensity(0);
        foreach (var matInfo in hutMaterials) matInfo.mat.SetFloat(matInfo.shaderEffectParamName, 0f);
        
        foreach (var step in ppSteps)
        {
            if (step.component != null) ApplyWeirdIntensity(step.component, 0f);
        }
    }
    #endregion
}