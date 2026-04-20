using UnityEngine;

public class TracerSliceLogic : MonoBehaviour
{
    public float lifetime = 1.0f; 
    
    // Add this line so you can tweak the size in the Unity Inspector!
    public float maxSmokeStretch = 0.5f; 
    
    private float timer;
    private Material mat;

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    public void ActivateSlice()
    {
        timer = lifetime;
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;

        if (mat != null)
        {
            mat.SetFloat("_FadeProgress", 1f); 
            mat.SetFloat("_WarpStrength", 0f);
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            float normalizedLife = timer / lifetime; 

            if (mat != null)
            {
                // Multiply by your new Inspector variable instead of 2.5f
                float meltIntensity = (1f - normalizedLife) * maxSmokeStretch;
                mat.SetFloat("_WarpStrength", meltIntensity);

                mat.SetFloat("_FadeProgress", normalizedLife);
            }

            if (timer <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}