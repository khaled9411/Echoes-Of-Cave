using StarterAssets;
using System.Collections;
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
    [SerializeField] private Vector3 holeLocalPosition;
    [SerializeField] private Vector3 holeLocalRotation;

    public StoneType StoneType => stoneType;

    protected override void Awake()
    {
        base.Awake();
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

        StartCoroutine(SmoothRotateAndAnimate(interactor, slotTransform));
    }

    private IEnumerator SmoothRotateAndAnimate(GameObject player, Transform parent)
    {
        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        if (controller != null)
            controller.disableMovement = true;

        Debug.Log($"Starting rotation and animation coroutine in {player}.");
        Animator playerAnimator = player.GetComponent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogError("Player GameObject does not have an Animator component!");
            yield break;
        }

        Vector3 direction = (transform.position - player.transform.position).normalized;

        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        bool isRotating = true;

        while (isRotating)
        {
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(player.transform.rotation, targetRotation) < 0.3f)
            {
                isRotating = false;
            }

            yield return null;
        }

        //Put In The Hole animation
        playerAnimator.SetTrigger("PutInTheHole");


        yield return new WaitForSeconds(0.7f);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        transform.SetParent(parent);
        transform.localPosition = holeLocalPosition;
        transform.localRotation = Quaternion.Euler(holeLocalRotation);

        isCurrentlyPickedUp = false;
        isInteractable = true;
        objectCollider.enabled = true;

        PlayInteractionSound();
        HideHighlight();
        HideInteractionUI();

        if (controller != null)
            controller.disableMovement = false;
    }
}
