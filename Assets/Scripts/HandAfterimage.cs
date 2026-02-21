using UnityEngine;
using System.Collections;

public class HandAfterimage : MonoBehaviour
{
    [Header("References")]
    public SkinnedMeshRenderer handRenderer;
    public Material ghostMaterial;

    [Header("Afterimage Settings")]
    [Range(0.005f, 0.03f)]
    public float distanceThreshold = 0.01f;

    [Range(0.1f, 0.25f)]
    public float ghostLifetime = 0.18f;

    [Range(0.05f, 0.15f)]
    public float initialAlpha = 0.08f;

    [Range(0f, 0.03f)]
    public float scaleGrowth = 0.01f;

    private Vector3 lastSpawnPosition;

    void Start()
    {
        lastSpawnPosition = transform.position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, lastSpawnPosition);

        if (distance > distanceThreshold)
        {
            SpawnGhost();
            lastSpawnPosition = transform.position;
        }
    }

    void SpawnGhost()
    {
        Mesh mesh = new Mesh();
        handRenderer.BakeMesh(mesh);

        GameObject ghost = new GameObject("HandGhost");
        ghost.transform.position = handRenderer.transform.position;
        ghost.transform.rotation = handRenderer.transform.rotation;
        ghost.transform.localScale = handRenderer.transform.lossyScale;

        ghost.layer = LayerMask.NameToLayer("Ignore Raycast");

        MeshFilter mf = ghost.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = ghost.AddComponent<MeshRenderer>();

        Material matInstance = new Material(ghostMaterial);

        Color c = matInstance.color;
        c.a = initialAlpha;
        matInstance.color = c;

        mr.material = matInstance;

        StartCoroutine(FadeAndDestroy(ghost, matInstance));
    }

    IEnumerator FadeAndDestroy(GameObject ghost, Material mat)
    {
        float timer = 0f;
        Color startColor = mat.color;

        while (timer < ghostLifetime)
        {
            timer += Time.deltaTime;
            float t = timer / ghostLifetime;

            // Ease-out fade
            float eased = 1f - (t * t);

            mat.color = new Color(
                startColor.r,
                startColor.g,
                startColor.b,
                initialAlpha * eased
            );

            // subtle expansion
            ghost.transform.localScale += Vector3.one * scaleGrowth * Time.deltaTime;

            yield return null;
        }

        Destroy(ghost);
    }
}