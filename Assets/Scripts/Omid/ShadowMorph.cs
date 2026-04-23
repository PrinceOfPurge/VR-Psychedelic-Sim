using UnityEngine;

public class ShadowMorph : MonoBehaviour
{
    public Material shadowMaterial;

    void Awake()
    {
        // Find every mesh part on the kid (skin, shirt, pants, etc.)
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer smr in renderers)
        {
            // Create an array of the shadow material matching the original material count
            Material[] shadowMats = new Material[smr.sharedMaterials.Length];
            for (int i = 0; i < shadowMats.Length; i++)
            {
                shadowMats[i] = shadowMaterial;
            }

            // Override the materials
            smr.materials = shadowMats;
        }
    }
}