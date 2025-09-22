using UnityEngine;

public class AnimatedRopeTexture : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0, -0.5f);
    private Material material;
    private Vector2 offset;

    void Start()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            material = lineRenderer.material;
        }
    }

    void Update()
    {
        if (material != null)
        {
            offset += scrollSpeed * Time.deltaTime;
            material.mainTextureOffset = offset;
        }
    }
}