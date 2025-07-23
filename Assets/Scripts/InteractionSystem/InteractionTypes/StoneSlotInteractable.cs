using UnityEngine;
using UnityEngine.Events;

public class StoneSlotInteractable : BaseInteractable
{
    [Header("Stone Slot Settings")]
    [SerializeField] private StoneType requiredStoneType;
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private UnityEvent onStonePlacedSuccessfullyInThisSlot;

    public override string InteractionPrompt
    {
        get
        {
            if (isOccupied) return "";
            return base.InteractionPrompt;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"StoneSlotInteractable on {gameObject.name} requires a Collider component to function.", this);
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} should be set to 'Is Trigger' for StoneSlotInteractable.", this);
        }
        UpdateHighlightEffect();
    }

    public override bool CanInteract(GameObject interactor)
    {
        if (isOccupied) return false;

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager != null && manager.heldObject is StoneInteractable heldStone)
        {
            return heldStone.StoneType == requiredStoneType;
        }
        return false;
    }

    public override void Interact(GameObject interactor)
    {
        if (isOccupied) return;

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager != null && manager.heldObject is StoneInteractable heldStone)
        {
            if (heldStone.StoneType == requiredStoneType)
            {
                heldStone.PlaceStoneInSlot(interactor, transform);
                isOccupied = true;
                onStonePlacedSuccessfullyInThisSlot?.Invoke();
                UpdateHighlightEffect();
                PlayInteractionSound();
                HideInteractionUI();
                HideHighlight();
            }
            else
            {
                Debug.Log($"Incorrect stone type for this slot! Expected {requiredStoneType}, got {heldStone.StoneType}.");
                heldStone.PlaceStoneIncorrectly(interactor);
                HideInteractionUI();
            }
        }
        else
        {
            Debug.Log("You need a stone to place here.");
        }
    }

    private void UpdateHighlightEffect()
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(!isOccupied);
        }
    }

    public override void ShowHighlight()
    {
        if (!isOccupied)
        {
            base.ShowHighlight();
        }
    }

    public override void HideHighlight()
    {
        base.HideHighlight();
    }
}
