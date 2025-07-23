using UnityEngine;

public class PickableObject : BaseInteractable, IPickable
{
    [Header("Pickable Settings")]
    [SerializeField] protected string pickUpPrompt = "Press E to pick up";
    [SerializeField] protected string dropPrompt = "Press E to drop";
    [SerializeField] protected Vector3 pickedUpLocalPosition = new Vector3(0, 0, 0);
    [SerializeField] protected Vector3 pickedUpLocalRotation = new Vector3(0, 0, 0);
    [SerializeField] protected bool disablePhysicsOnPickUp = true;
    [SerializeField] protected bool reEnablePhysicsOnDrop = true;

    protected Rigidbody rb;
    protected Collider objectCollider;
    protected bool isCurrentlyPickedUp = false;
    protected Transform originalParent;

    public override string InteractionPrompt
    {
        get
        {
            return isCurrentlyPickedUp ? dropPrompt : pickUpPrompt;
        }
    }

    public bool IsPickedUp => isCurrentlyPickedUp;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        originalParent = transform.parent;
    }

    public override bool CanInteract(GameObject interactor)
    {
        return isInteractable && (!isCurrentlyPickedUp || (isCurrentlyPickedUp && transform.parent == interactor.transform.Find("InteractionPoint")));
    }

    public override void Interact(GameObject interactor)
    {
        PlayInteractionSound();
    }

    public void PickUp(GameObject interactor, Transform parent)
    {
        if (isCurrentlyPickedUp) return;

        isCurrentlyPickedUp = true;
        isInteractable = false;

        transform.SetParent(parent);
        transform.localPosition = pickedUpLocalPosition;
        transform.localRotation = Quaternion.Euler(pickedUpLocalRotation);

        if (disablePhysicsOnPickUp && rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        Debug.Log($"{name} picked up by {interactor.name}");
        HideInteractionUI();
    }

    public void Drop(GameObject interactor)
    {
        if (!isCurrentlyPickedUp) return;

        isCurrentlyPickedUp = false;
        isInteractable = true;

        transform.SetParent(originalParent);

        Vector3 dropPosition = interactor.transform.position + interactor.transform.forward * 1.0f;
        transform.position = dropPosition;

        if (disablePhysicsOnPickUp && rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = reEnablePhysicsOnDrop;
        }
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        Debug.Log($"{name} dropped by {interactor.name}");
    }
}
