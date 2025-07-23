using UnityEngine;
using UnityEngine.Events;

public class ConditionalInteractable : BaseInteractable, IConditionalInteractable
{
    [Header("Conditional Interaction")]
    [SerializeField] private string requiredItem = "Key";
    [SerializeField] private string lockedPrompt = "You need a key to unlock this";
    [SerializeField] private UnityEvent onSuccessfulInteract;
    [SerializeField] private UnityEvent onFailedInteract;

    public override string InteractionPrompt
    {
        get
        {
            return MeetsInteractionCondition(null) ? interactionPrompt : lockedPrompt;
        }
    }

    public bool MeetsInteractionCondition(GameObject interactor)
    {
        if (interactor == null) return false;

        var inventory = interactor.GetComponent<PlayerInventory>();
        return inventory != null && inventory.HasItem(requiredItem);
    }

    public override void Interact(GameObject interactor)
    {
        if (MeetsInteractionCondition(interactor))
        {
            PlayInteractionSound();
            onSuccessfulInteract?.Invoke();
        }
        else
        {
            onFailedInteract?.Invoke();
        }
    }
}
