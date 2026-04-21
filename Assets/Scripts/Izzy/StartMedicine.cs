using System;
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
public class VolumeEffectStep
{
    public string effectName; // Label for your own sanity (e.g., "Extruder")
    [Range(0, 1)] public float startAtNormalizedTime; // 0.2 = starts 20% into the duration
    [Range(0, 1)] public float endAtNormalizedTime;   // 0.8 = reaches max at 80%
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    // We will cache the actual volume component here
    [HideInInspector] public VolumeComponent component;
}

public class StartMedicine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DistortedUVsInfoContainer[] environmentalPsychedelicMaterials;
    [SerializeField] private Volume descentVolume;

    [Header("Sequencing")]
    [SerializeField] private float transitionDuration = 30f;
    [SerializeField] private List<VolumeEffectStep> sequencedEffects;

    private bool transitioning;
    private float elapsedTransitionTime;

    private void Start()
    {
        InitializeVolumeComponents();
        ResetEffects();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) transitioning = true;
        if (Input.GetKeyDown(KeyCode.G)) ResetEffects();

        if (transitioning)
        {
            UpdateTransition();
        }
    }

    private void InitializeVolumeComponents()
    {
        if (descentVolume == null || descentVolume.profile == null) return;

        // Matches the effect steps to the actual components in the Volume Profile
        // Note: This assumes your custom effects have classes like "CrystalVolume", etc.
        foreach (var step in sequencedEffects)
        {
            foreach (var comp in descentVolume.profile.components)
            {
                if (comp.name.Contains(step.effectName))
                {
                    step.component = comp;
                }
            }
        }
    }

    private void UpdateTransition()
    {
        elapsedTransitionTime += Time.deltaTime;
        float globalT = Mathf.Clamp01(elapsedTransitionTime / transitionDuration);

        // 1. Update Environment Materials (Linear 0-1)
        foreach (var matInfo in environmentalPsychedelicMaterials)
        {
            matInfo.mat.SetFloat(matInfo.shaderEffectParamName, globalT);
        }

        // 2. Update Global Volume Weight
        if (descentVolume != null) descentVolume.weight = globalT;

        // 3. Update Sequenced Post-Processing Effects
        foreach (var step in sequencedEffects)
        {
            if (step.component == null) continue;

            // Calculate local T for this specific effect's window
            float localT = Mathf.InverseLerp(step.startAtNormalizedTime, step.endAtNormalizedTime, globalT);
            float intensity = step.intensityCurve.Evaluate(localT);

            // Dynamically find the intensity/weight property and set it
            SetComponentIntensity(step.component, intensity);
        }

        if (globalT >= 1f) transitioning = false;
    }

    private void SetComponentIntensity(VolumeComponent comp, float value)
    {
        // This uses reflection to find a field named "intensity" or "weight" 
        // since I don't have your specific class definitions.
        var field = comp.GetType().GetField("intensity") ?? comp.GetType().GetField("weight");
        if (field != null)
        {
            // Assuming the field is a VolumeParameter (e.g., ClampedFloatParameter)
            var param = field.GetValue(comp) as VolumeParameter;
            if (param != null) param.SetValue(param); // This is a placeholder logic
            
            // For standard URP parameters, we access 'value'
            var floatParam = field.GetValue(comp) as ClampedFloatParameter;
            if (floatParam != null) floatParam.value = value;
        }
    }

    public void ResetEffects()
    {
        transitioning = false;
        elapsedTransitionTime = 0f;

        foreach (var matInfo in environmentalPsychedelicMaterials)
            matInfo.mat.SetFloat(matInfo.shaderEffectParamName, 0f);

        if (descentVolume != null) descentVolume.weight = 0f;

        foreach (var step in sequencedEffects)
            if (step.component != null) SetComponentIntensity(step.component, 0f);
    }
}