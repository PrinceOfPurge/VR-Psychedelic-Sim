using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[Serializable]
struct DistortedUVsInfoContainer
{
    public Material mat;
    public string shaderEffectParamName;
}


public class StartMedicine : MonoBehaviour
{
    [FormerlySerializedAs("envPsy"),Header("References")] 
    [SerializeField] private DistortedUVsInfoContainer[] environmentalPsychedelicMaterials;
    [SerializeField] private Volume descentVolume;

    [Header("Parameters")]
    [SerializeField] private float transitionDuration = 15f;

    private bool transitioning;
    private float elapsedTransitionTime;

    private void Start()
    {
        ResetEffects();
    }

    private void Update()
    {
        // Start transition
        if (Input.GetKeyDown(KeyCode.F))
            transitioning = true;
        
        if (transitioning)
            StartMedicineEffects();
        else
            elapsedTransitionTime = 0f;

        // Reset effects
        if (Input.GetKeyDown(KeyCode.G))
            ResetEffects();
    }

    private void StartMedicineEffects()
    {
        // Transition
        elapsedTransitionTime += Time.deltaTime;
        float t = elapsedTransitionTime / transitionDuration;
            
        // Make environment trippy
        if (environmentalPsychedelicMaterials.Length > 0)
        {
            foreach (DistortedUVsInfoContainer matInfo in environmentalPsychedelicMaterials)
            {
                matInfo.mat.SetFloat(matInfo.shaderEffectParamName, t);
            }
        }
            
        // Blend in trippy volume
        if (descentVolume != null) descentVolume.weight = t;
    }

    private void ResetEffects()
    {
        // Reset transition
        transitioning = false;

        // Make environment normal
        if (environmentalPsychedelicMaterials.Length > 0)
        {
            foreach (DistortedUVsInfoContainer matInfo in environmentalPsychedelicMaterials)
            {
                matInfo.mat.SetFloat(matInfo.shaderEffectParamName, 0f);
            }
        }

        // Remove trippy volume
        if (descentVolume != null) descentVolume.weight = 0f;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        StartMedicineEffects();
    }

    private void OnApplicationQuit()
    {
        ResetEffects();
    }
}