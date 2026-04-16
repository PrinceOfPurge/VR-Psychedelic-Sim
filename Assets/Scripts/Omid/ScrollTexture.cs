using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    public float scrollSpeedX = 0f;
    public float scrollSpeedY = 0.5f; // Adjust this to match the belt's direction
    
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Calculate how far the texture should have moved by now
        float offsetX = Time.time * scrollSpeedX;
        float offsetY = Time.time * scrollSpeedY;

        // Apply the offset to the material
        _renderer.material.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}