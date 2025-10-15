using StarterAssets;
using System.Collections;
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
    [SerializeField] protected float rotationSpeed = 5f;
    public Vector3 holdIKConstrintPosition;
    public Vector3 holdIKConstrintRotation;
    public string animationTriggerName = "Is Grabing Item";
    public bool isCarryingObject = false;

    protected Rigidbody rb;
    protected Collider objectCollider;
    protected bool isCurrentlyPickedUp = false;
    protected Transform originalParent;

    // Anti-Spam Protection
    protected bool isAnimating = false;
    protected Coroutine currentAnimationCoroutine = null;

    public bool IsCurrentlyPickedUp
    {
        get
        {
            return isCurrentlyPickedUp;
        }
    }

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
        // Prevent interaction while animating
        if (isAnimating) return false;

        return isInteractable && (!isCurrentlyPickedUp || (isCurrentlyPickedUp && transform.parent == interactor.transform.Find("InteractionPoint")));
    }

    public override void Interact(GameObject interactor)
    {
        PlayInteractionSound();
    }

    public void PickUp(GameObject interactor, Transform parent)
    {
        // Prevent multiple pickup calls
        if (isCurrentlyPickedUp || isAnimating)
        {
            Debug.LogWarning($"[SPAM PREVENTION] PickUp blocked - isPickedUp: {isCurrentlyPickedUp}, isAnimating: {isAnimating}");
            return;
        }

        // Stop any existing coroutine
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        currentAnimationCoroutine = StartCoroutine(SmoothRotateAndAnimate(interactor, parent));
    }

    public void Drop(GameObject interactor)
    {
        // Prevent drop while animating or not picked up
        if (!isCurrentlyPickedUp || isAnimating)
        {
            Debug.LogWarning($"[SPAM PREVENTION] Drop blocked - isPickedUp: {isCurrentlyPickedUp}, isAnimating: {isAnimating}");
            return;
        }

        isCurrentlyPickedUp = false;
        isInteractable = true;

        Animator playerAnimator = interactor.GetComponent<Animator>();
        if (isCarryingObject && playerAnimator != null)
            playerAnimator.SetTrigger("StopCarrying");

        transform.SetParent(originalParent);

        Vector3 dropPosition = interactor.transform.position + interactor.transform.forward * 1.0f + new Vector3(0, 1, 0);
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
        InteractionManager interactionManager = interactor.GetComponent<InteractionManager>();
        if (interactionManager != null)
        {
            //IKConstraint from CURRENT to 0 
            interactionManager.LerpRightHandWeight(0, 0.5f);
        }
        Debug.Log($"{name} dropped by {interactor.name}");
    }

    private IEnumerator SmoothRotateAndAnimate(GameObject player, Transform parent)
    {
        // Mark as animating immediately
        isAnimating = true;

        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        if (controller != null)
            controller.disableMovement = true;

        Debug.Log($"Starting rotation and animation coroutine in {player}.");
        Animator playerAnimator = player.GetComponent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogError("Player GameObject does not have an Animator component!");
            isAnimating = false;
            if (controller != null)
                controller.disableMovement = false;
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

        //pick up animation
        playerAnimator.SetTrigger(animationTriggerName);
        InteractionManager interactionManager = player.GetComponent<InteractionManager>();
        if (interactionManager != null && !isCarryingObject)
        {
            interactionManager.rightHandTarget.position = transform.position;
            interactionManager.rightHandTarget.rotation = Quaternion.Euler(-260f, -60, 0);

            //IKConstraint from CURRENT to 0.9
            interactionManager.LerpRightHandWeight(0.9f, 1);
        }

        yield return new WaitForSeconds(0.7f);

        //pick up the object
        isCurrentlyPickedUp = true;
        isInteractable = false;

        transform.SetParent(parent);
        transform.localPosition = pickedUpLocalPosition;
        transform.localRotation = Quaternion.Euler(pickedUpLocalRotation);

        if (interactionManager != null)
        {
            interactionManager.rightHandTarget.localPosition = holdIKConstrintPosition;
            interactionManager.rightHandTarget.localRotation = Quaternion.Euler(holdIKConstrintRotation);
        }

        if (disablePhysicsOnPickUp && rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        if (isCarryingObject)
            playerAnimator.SetTrigger("Carrying");

        Debug.Log($"{name} picked up by {player.name}");
        HideInteractionUI();

        if (controller != null)
            controller.disableMovement = false;

        // Unlock interaction after animation completes
        if (interactionManager != null)
        {
            interactionManager.UnlockInteraction();
        }

        // Mark animation as complete
        isAnimating = false;
        currentAnimationCoroutine = null;
    }

    protected virtual void OnDisable()
    {
        // Clean up coroutine if object is disabled
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
            isAnimating = false;
        }
    }
}