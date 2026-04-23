using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class HandDissolver : MonoBehaviour
{
    public static HandDissolver Instance;
    
    [SerializeField] private Material handMaterial;
    [SerializeField] private VisualEffect vfxGraph;
    [SerializeField] private string dissolveAmountParam = "_DissolveAmount";
    
    // Updated these to follow duration-based logic
    [SerializeField] private float dissolveDuration = 2.0f; // Total seconds for the dissolve

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        if (handMaterial != null) handMaterial.SetFloat(dissolveAmountParam, 0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartHandDissolve();
        }
    }

    public void StartHandDissolve()
    {
        StartCoroutine(DissolveCo());
    }

    private IEnumerator DissolveCo()
    {
        if (handMaterial != null)
        {
            float elapsedTime = 0f;

            // Using a while loop with a timer for a smooth transition
            while (elapsedTime < dissolveDuration)
            {
                elapsedTime += Time.deltaTime;
                
                // Calculate progress (0 to 1) based on time
                float lerpValue = Mathf.Clamp01(elapsedTime / dissolveDuration);
                
                handMaterial.SetFloat(dissolveAmountParam, lerpValue);
                
                // yield return null tells Unity to wait until the next frame
                yield return null; 
            }

            // Ensure it's fully dissolved at the end
            handMaterial.SetFloat(dissolveAmountParam, 1f);
        }
    }

    private void OnApplicationQuit()
    {
        if (handMaterial != null) handMaterial.SetFloat(dissolveAmountParam, 0f);
    }
}