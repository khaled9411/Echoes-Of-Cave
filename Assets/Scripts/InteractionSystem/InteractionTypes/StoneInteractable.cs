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
    [SerializeField] private string placementPrompt = "Press E to place stone";
    [SerializeField] private UnityEvent onStonePlacedCorrectly;
    [SerializeField] private UnityEvent onStonePlacedIncorrectly;
    [SerializeField] private AudioClip incorrectPlacementSound;
    [SerializeField] private float incorrectPlacementShakeDuration = 0.2f;
    [SerializeField] private float incorrectPlacementShakeAmount = 0.1f;

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
        base.Interact(interactor);
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
        isInteractable = false;

        PlayInteractionSound();

        onStonePlacedCorrectly?.Invoke();
        HideHighlight();
        HideInteractionUI();
        Debug.Log($"Stone {name} placed correctly in slot {slotTransform.name}!");

        interactor.GetComponent<InteractionManager>()?.ClearHeldObject();
    }

    public void PlaceStoneIncorrectly(GameObject interactor)
    {
        if (!IsPickedUp) return;

        Drop(interactor);

        isInteractable = true;

        if (audioSource != null && incorrectPlacementSound != null)
        {
            audioSource.PlayOneShot(incorrectPlacementSound);
        }
        StartCoroutine(ShakeObject(incorrectPlacementShakeDuration, incorrectPlacementShakeAmount));

        onStonePlacedIncorrectly?.Invoke();
        Debug.Log($"Stone {name} placed incorrectly!");

        interactor.GetComponent<InteractionManager>()?.ClearHeldObject();
    }

    private System.Collections.IEnumerator ShakeObject(float duration, float amount)
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * amount;
            float y = Random.Range(-1f, 1f) * amount;

            transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
    }
}
