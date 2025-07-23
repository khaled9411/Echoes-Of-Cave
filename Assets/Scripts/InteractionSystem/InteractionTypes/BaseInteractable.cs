using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable, IInteractionFeedback
{
    [Header("Basic Interaction Settings")]
    [SerializeField] protected string interactionPrompt = "Press E to interact";
    [SerializeField] protected bool isInteractable = true;

    [Header("Visual Feedback")]
    [SerializeField] protected GameObject highlightObject;
    [SerializeField] protected Material highlightMaterial;
    [SerializeField] protected Color highlightColor = Color.yellow;

    [Header("Audio Feedback")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip interactionSound;

    protected Renderer objectRenderer;
    protected Material originalMaterial;
    protected bool isHighlighted = false;

    public virtual string InteractionPrompt => interactionPrompt;

    protected virtual void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public virtual bool CanInteract(GameObject interactor)
    {
        return isInteractable;
    }

    public abstract void Interact(GameObject interactor);

    public virtual void ShowHighlight()
    {
        if (isHighlighted) return;

        isHighlighted = true;

        if (highlightObject != null)
        {
            highlightObject.SetActive(true);
        }
        else if (objectRenderer != null && highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.material.color = highlightColor;
        }
    }

    public virtual void HideHighlight()
    {
        if (!isHighlighted) return;

        isHighlighted = false;

        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
        else if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
    }

    public virtual void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }

    public virtual void ShowInteractionUI(string prompt)
    {
        InteractionUIManager.Instance?.ShowPrompt(prompt);
    }

    public virtual void HideInteractionUI()
    {
        InteractionUIManager.Instance?.HidePrompt();
    }
}
