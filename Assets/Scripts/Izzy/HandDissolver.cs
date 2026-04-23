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
    [SerializeField] private float dissolveRate = 0.0125f;
    [SerializeField] private float refreshRate = 0.025f;


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
            float counter = 0;

            while (handMaterial.GetFloat(dissolveAmountParam) < 1)
            {
                counter += dissolveRate;
                handMaterial.SetFloat(dissolveAmountParam, counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }

        /*
        // This graph doesn't really look that good
        if (vfxGraph != null)
        {
            vfxGraph.Play();
        }
        */
    }

    private void OnApplicationQuit()
    {
        if (handMaterial != null) handMaterial.SetFloat(dissolveAmountParam, 0f);
    }
}