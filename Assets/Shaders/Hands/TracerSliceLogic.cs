using UnityEngine;

public class TracerSliceLogic : MonoBehaviour
{
    public float lifetime = 0.8f; // Increased slightly to see the effect better
    private float timer;
    private Material mat;

    void Awake()
    {
        // We use .material to ensure we are talking to a unique instance, 
        // otherwise all hands will melt at the exact same time.
        mat = GetComponent<MeshRenderer>().material;
    }

    public void ActivateSlice()
    {
        timer = lifetime;
        gameObject.SetActive(true);
        
        // Ensure scale is always full
        transform.localScale = Vector3.one;

        // Reset Shader properties
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
            float normalizedLife = timer / lifetime; // 1.0 (start) to 0.0 (end)

            if (mat != null)
            {
                // 1. THE MELT: Starts at 0 and goes up to 2.0 (High intensity)
                // We use (1 - normalizedLife) so it gets stronger as it dies
                float meltIntensity = (1f - normalizedLife) * 2.0f;
                mat.SetFloat("_WarpStrength", meltIntensity);

                // 2. THE DISSOLVE: Controls the Alpha/Transparency
                // Hand stays fully visible for the first half, then fades out
                float alphaFade = Mathf.Clamp01(normalizedLife * 2.0f);
                mat.SetFloat("_FadeProgress", alphaFade);
            }

            if (timer <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}