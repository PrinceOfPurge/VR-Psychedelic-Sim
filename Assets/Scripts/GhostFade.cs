using UnityEngine;

public class GhostFade : MonoBehaviour
{
    public float lifetime = 0.5f;

    float timer;
    Material mat;
    Color startColor;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        startColor = mat.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float alpha = Mathf.Lerp(startColor.a, 0f, timer / lifetime);
        mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}