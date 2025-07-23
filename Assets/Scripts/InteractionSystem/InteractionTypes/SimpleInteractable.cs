using UnityEngine;
using UnityEngine.Events;

public class SimpleInteractable : BaseInteractable
{
    [Header("Interaction Events")]
    [SerializeField] private UnityEvent onInteract;

    public override void Interact(GameObject interactor)
    {
        PlayInteractionSound();
        onInteract?.Invoke();
    }
}
