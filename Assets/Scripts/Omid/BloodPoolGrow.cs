using UnityEngine;

public class BloodPoolGrow : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growSpeed = 0.05f;
    public float maxSize = 2.0f; 
    private bool isGrowing = false;

    void Start()
    {
        // Start as a tiny dot
        transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    public void StartPool()
    {
        isGrowing = true;
    }

    void Update()
    {
        // Purely handle scale growth
        if (isGrowing && transform.localScale.x < maxSize)
        {
            float growth = growSpeed * Time.deltaTime;
            
            // Uniform growth on all axes for a perfect circle
            transform.localScale += new Vector3(growth, growth, growth); 
        }
    }
}