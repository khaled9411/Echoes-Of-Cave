using UnityEngine;
using UnityEngine.Events;

public enum StoneType
{
    Generic,
    RedStone,
    BlueStone,
    GreenStone,
}

public class StoneInteractable : PickableObject
{
    [Header("Stone Specific Settings")]
    [SerializeField] private StoneType stoneType;
    [SerializeField] private UnityEvent onStonePlacedCorrectly;
    [SerializeField] private UnityEvent onStonePlacedIncorrectly;
    [SerializeField] private AudioClip incorrectPlacementSound;

    private Transform originalLocalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    public StoneType StoneType => stoneType;

    protected override void Awake()
    {
        base.Awake();
        originalLocalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public override string InteractionPrompt
    {
        get
        {
            return base.InteractionPrompt;
        }
    }

    public override void Interact(GameObject interactor)
    {
        var manager = interactor.GetComponent<InteractionManager>();
        if (manager == null) return;

        if (!isCurrentlyPickedUp)
        {
            base.Interact(interactor);
        }
    }

    public new void Drop(GameObject interactor)
    {
        base.Drop(interactor);

        isInteractable = true;
    }

    public void PlaceStoneInSlot(GameObject interactor, Transform slotTransform)
    {
        if (!IsPickedUp) return;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        transform.SetParent(slotTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        isCurrentlyPickedUp = false;
        isInteractable = true;
        objectCollider.enabled = true;

        PlayInteractionSound();
        HideHighlight();
        HideInteractionUI();
    }
}
