using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum StoneType
{
    s1,
    s2,
    s3,
    s4,
    s5,
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

    // Anti-Spam Protection
    private bool isBeingPlaced = false;
    private Coroutine placementCoroutine = null;

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

    public override bool CanInteract(GameObject interactor)
    {
        // Prevent interaction while being placed or animating
        if (isBeingPlaced || isAnimating) return false;

        return base.CanInteract(interactor);
    }

    public override void Interact(GameObject interactor)
    {
        var manager = interactor.GetComponent<InteractionManager>();
        if (manager == null) return;

        if (!isCurrentlyPickedUp && !isAnimating && !isBeingPlaced)
        {
            base.Interact(interactor);
        }
    }

    public new void Drop(GameObject interactor)
    {
        // Prevent drop while being placed
        if (isBeingPlaced)
        {
            Debug.LogWarning($"[SPAM PREVENTION] Drop blocked - stone is being placed");
            return;
        }

        base.Drop(interactor);
        isInteractable = true;
    }

    public void PlaceStoneInSlot(GameObject interactor, Transform slotTransform)
    {
        // Prevent multiple placement calls
        if (!IsPickedUp || isBeingPlaced || isAnimating)
        {
            Debug.LogWarning($"[SPAM PREVENTION] PlaceStoneInSlot blocked - IsPickedUp: {IsPickedUp}, isBeingPlaced: {isBeingPlaced}, isAnimating: {isAnimating}");
            return;
        }

        // Stop any existing placement coroutine
        if (placementCoroutine != null)
        {
            StopCoroutine(placementCoroutine);
        }

        placementCoroutine = StartCoroutine(SmoothRotateAndAnimateForPlacement(interactor, slotTransform));
    }

    private IEnumerator SmoothRotateAndAnimateForPlacement(GameObject player, Transform parent)
    {
        // Mark as being placed immediately
        isBeingPlaced = true;
        isAnimating = true;

        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        if (controller != null)
            controller.disableMovement = true;

        Debug.Log($"Starting stone placement animation for {name}.");
        Animator playerAnimator = player.GetComponent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogError("Player GameObject does not have an Animator component!");
            isBeingPlaced = false;
            isAnimating = false;
            if (controller != null)
                controller.disableMovement = false;

            // Unlock interaction
            InteractionManager manager = player.GetComponent<InteractionManager>();
            if (manager != null)
                manager.UnlockInteraction();

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

        // Place the stone
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

        // Unlock interaction after animation completes
        InteractionManager interactionManager = player.GetComponent<InteractionManager>();
        if (interactionManager != null)
        {
            interactionManager.UnlockInteraction();
        }

        // Mark placement as complete
        isBeingPlaced = false;
        isAnimating = false;
        placementCoroutine = null;

        Debug.Log($"{name} placed in slot successfully.");
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Clean up placement coroutine if object is disabled
        if (placementCoroutine != null)
        {
            StopCoroutine(placementCoroutine);
            placementCoroutine = null;
            isBeingPlaced = false;
        }
    }
}