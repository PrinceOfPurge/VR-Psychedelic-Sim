using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
struct DistortedUVsInfoContainer
{
    public Material mat;
    public string shaderEffectParamName;
}

public class StartMedicine : MonoBehaviour
{
    [Header("Materials")] 
    [SerializeField] private DistortedUVsInfoContainer[] envPsy;
    
    [Header("Volumes")]
    [SerializeField] private Volume descentVolume;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Make environment trippy
            foreach (DistortedUVsInfoContainer matInfo in envPsy)
            {
                matInfo.mat.SetFloat(matInfo.shaderEffectParamName, 1f);
            }
            
            // Blend in trippy volume
            descentVolume.weight = 1f;
        }
    }
}