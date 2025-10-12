using UnityEngine.Events;
using UnityEngine;

public class StoneSlotInteractable : BaseInteractable
{
    [Header("Stone Slot Settings")]
    [SerializeField] private StoneType requiredStoneType;
    [SerializeField] private GameObject highlightEffect;
    public UnityEvent onStonePlacedCorrectly;
    public UnityEvent onStoneRemoved;

    private ParticleSystem glowParticle;

    private StoneInteractable placedStone;
    private bool isOccupied => placedStone != null;

    public StoneType RequiredStoneType => requiredStoneType;
    public StoneInteractable PlacedStone => placedStone;

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

    private void Start()
    {
        highlightEffect.SetActive(false);
        highlightEffect = null;
        glowParticle = GetComponentInChildren<ParticleSystem>();
        if(glowParticle != null)
        {
            glowParticle.gameObject.SetActive(false);
        }
    }

    public override bool CanInteract(GameObject interactor)
    {
        var manager = interactor.GetComponent<InteractionManager>();

        if (!isOccupied && manager != null && manager.heldObject is StoneInteractable)
        {
            return true;
        }
        else if (isOccupied && manager != null && manager.heldObject == null)
        {
            return true;
        }

        return false;
    }

    public override void Interact(GameObject interactor)
    {
        var manager = interactor.GetComponent<InteractionManager>();
        if (manager == null) return;

        if (!isOccupied && manager.heldObject is StoneInteractable heldStone)
        {
            PlaceStone(heldStone, interactor);
        }
        else if (isOccupied && manager.heldObject == null)
        {
            PickUpStone(interactor);
        }
        else
        {
            Debug.Log("Invalid interaction with stone slot.");
        }
    }

    private void PlaceStone(StoneInteractable stone, GameObject interactor)
    {

        if (glowParticle != null)
        {
            glowParticle.gameObject.SetActive(true);
        }

        placedStone = stone;
        stone.PlaceStoneInSlot(interactor, transform);

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager != null)
        {
            manager.ClearHeldObject();
        }

        PlayInteractionSound();
        UpdateHighlightEffect();
        HideInteractionUI();
        HideHighlight();

        if (placedStone.StoneType == requiredStoneType)
        {
            onStonePlacedCorrectly?.Invoke();
            Debug.Log($"Stone {placedStone.name} placed correctly!");
        }
        else
        {
            Debug.Log($"Stone {placedStone.name} placed incorrectly! Required: {requiredStoneType}, Placed: {placedStone.StoneType}");
        }
    }

    private void PickUpStone(GameObject interactor)
    {

        if (glowParticle != null)
        {
            glowParticle.gameObject.SetActive(false);
        }

        if (placedStone == null) return;

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager == null) return;
        StoneInteractable stoneToPickUp = placedStone;

        placedStone = null;
        manager.ForcePickup(stoneToPickUp);

        PlayInteractionSound();
        UpdateHighlightEffect();
        HideInteractionUI();
        HideHighlight();
        onStoneRemoved?.Invoke();
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
        else
        {
            var manager = FindFirstObjectByType<InteractionManager>();
            if (manager != null && manager.heldObject == null)
            {
                base.ShowHighlight();
            }
        }
    }

    public override void HideHighlight()
    {
        base.HideHighlight();
    }
}