using UnityEngine;

public class HandAfterimage : MonoBehaviour
{
    public SkinnedMeshRenderer handRenderer;
    public Material ghostMaterial;

    public float spawnInterval = 0.05f;
    public float ghostLifetime = 0.5f;
    public float minVelocity = 0.1f;

    float timer;
    Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float velocity = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        timer += Time.deltaTime;

        if (velocity > minVelocity && timer >= spawnInterval)
        {
            SpawnGhost();
            timer = 0f;
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

        MeshFilter mf = ghost.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = ghost.AddComponent<MeshRenderer>();
        mr.material = ghostMaterial;

        GhostFade fade = ghost.AddComponent<GhostFade>();
        fade.lifetime = ghostLifetime;
    }
}