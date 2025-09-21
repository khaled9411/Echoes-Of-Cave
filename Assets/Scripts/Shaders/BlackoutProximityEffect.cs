using UnityEngine;

public class BlackoutProximityEffect : MonoBehaviour
{
    [Tooltip("Radius of nearby object detection area")]
    public float proximityRadius = 5f;

    [Tooltip("Layers that should be affected by the fade effect")]
    public LayerMask targetLayer;

    [Tooltip("Fade transition speed")]
    public float fadeSpeed = 2f;

    private Renderer objectRenderer;
    private Material material;

    private float currentFadeValue;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            material = objectRenderer.material;
            currentFadeValue = material.GetFloat("_ProximityFade");
        }
        else
        {
            Debug.LogError("Renderer component not found on this GameObject. Please add a Renderer to use this script.");
            this.enabled = false;
        }
    }

    void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, proximityRadius, targetLayer);

        if (hitColliders.Length > 0)
        {
            currentFadeValue = Mathf.MoveTowards(currentFadeValue, 0f, fadeSpeed * Time.deltaTime);
        }
        else
        {
            currentFadeValue = Mathf.MoveTowards(currentFadeValue, 1f, fadeSpeed * Time.deltaTime);
        }

        material.SetFloat("_ProximityFade", currentFadeValue);
    }
}