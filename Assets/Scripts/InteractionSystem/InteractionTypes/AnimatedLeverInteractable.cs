using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AnimatedLeverInteractable : BaseInteractable
{
    [Header("Lever Settings")]
    [SerializeField] private string activatePrompt = "Press E to pull lever";
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float interactionDelay = 0.7f;
    [SerializeField] private float leverAnimationSpeed = 2f;

    [Header("Lever States")]
    [SerializeField] private bool startActivated = false; // Initial state as developer
    [SerializeField] private Vector3 activatedRotation = new Vector3(0, 0, -45f); // Rotation when activated
    [SerializeField] private Vector3 deactivatedRotation = new Vector3(0, 0, 45f); // Rotation when deactivated

    [Header("Lever Events")]
    [SerializeField] private UnityEvent onActivate;
    [SerializeField] private UnityEvent onDeactivate;
    [SerializeField] private UnityEvent onLeverMoved; // Called every time lever moves

    [Header("Animation Settings")]
    [SerializeField] private string leverAnimationTrigger = "PullLever";
    [SerializeField] private Vector3 leverIKPosition = new Vector3(0.2f, 0.1f, 0.3f);
    [SerializeField] private Vector3 leverIKRotation = new Vector3(-90f, 0f, 0f);

    [Header("Visual Settings")]
    [SerializeField] private Transform leverVisual; // The visual part that rotates

    private bool isActivated;
    private bool isInteracting = false;
    private bool isMoving = false;
    private Quaternion targetRotation;

    public override string InteractionPrompt
    {
        get
        {
            return activatePrompt;
        }
    }

    public bool IsActivated => isActivated;

    protected override void Awake()
    {
        base.Awake();

        // Set initial state
        isActivated = startActivated;

        // If no visual is assigned, use this transform
        if (leverVisual == null)
            leverVisual = transform;

        // Set initial rotation based on starting state
        SetLeverVisualRotation(isActivated);
    }

    private void Start()
    {
        // Ensure lever starts in correct position
        SetLeverVisualRotation(isActivated);
    }

    public override bool CanInteract(GameObject interactor)
    {
        return isInteractable && !isInteracting && !isMoving;
    }

    public override void Interact(GameObject interactor)
    {
        if (isInteracting || isMoving) return;

        StartCoroutine(HandleLeverInteraction(interactor));
    }

    private IEnumerator HandleLeverInteraction(GameObject player)
    {
        isInteracting = true;

        // Disable player movement
        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        if (controller != null)
            controller.disableMovement = true;

        Debug.Log($"Starting lever interaction with {player.name}");

        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.LogError("Player GameObject does not have an Animator component!");
            isInteracting = false;
            if (controller != null)
                controller.disableMovement = false;
            yield break;
        }

        // Calculate direction to face the lever
        Vector3 direction = (transform.position - player.transform.position).normalized;
        direction.y = 0; // Keep only horizontal rotation

        Quaternion targetPlayerRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate player towards the lever
        bool isRotating = true;
        while (isRotating)
        {
            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                targetPlayerRotation,
                Time.deltaTime * rotationSpeed
            );

            if (Quaternion.Angle(player.transform.rotation, targetPlayerRotation) < 0.3f)
            {
                isRotating = false;
            }

            yield return null;
        }

        // Set up IK for lever interaction
        InteractionManager interactionManager = player.GetComponent<InteractionManager>();
        if (interactionManager != null)
        {
            // Position hand near the lever
            interactionManager.rightHandTarget.position = transform.position + transform.TransformDirection(leverIKPosition);
            interactionManager.rightHandTarget.rotation = Quaternion.Euler(leverIKRotation);

            // Smoothly blend IK weight
            interactionManager.LerpRightHandWeight(0.9f, 1f);
        }

        // Play lever interaction animation
        if (!string.IsNullOrEmpty(leverAnimationTrigger))
        {
            playerAnimator.SetTrigger(leverAnimationTrigger);
        }

        // Wait for interaction to complete
        yield return new WaitForSeconds(interactionDelay);

        // Start lever movement
        StartCoroutine(MoveLever());

        // Reset IK weight
        if (interactionManager != null)
        {
            interactionManager.LerpRightHandWeight(0f, 0.5f);
        }

        // Play interaction sound
        PlayInteractionSound();

        Debug.Log($"Lever {(isActivated ? "activated" : "deactivated")} by {player.name}");

        // Hide UI and re-enable movement
        HideInteractionUI();

        if (controller != null)
            controller.disableMovement = false;

        isInteracting = false;
    }

    private IEnumerator MoveLever()
    {
        isMoving = true;

        // Toggle state
        isActivated = !isActivated;

        // Set target rotation based on new state
        Vector3 targetEuler = isActivated ? activatedRotation : deactivatedRotation;
        targetRotation = Quaternion.Euler(targetEuler);

        Quaternion startRotation = leverVisual.localRotation;
        float elapsed = 0f;
        float duration = 1f / leverAnimationSpeed;

        // Smoothly rotate the lever
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Use smooth curve for natural movement
            progress = Mathf.SmoothStep(0f, 1f, progress);

            leverVisual.localRotation = Quaternion.Lerp(startRotation, targetRotation, progress);

            yield return null;
        }

        // Ensure exact final rotation
        leverVisual.localRotation = targetRotation;

        // Trigger appropriate events
        if (isActivated)
        {
            onActivate?.Invoke();
            Debug.Log($"{name} lever activated!");
        }
        else
        {
            onDeactivate?.Invoke();
            Debug.Log($"{name} lever deactivated!");
        }

        // Always call movement event
        onLeverMoved?.Invoke();

        isMoving = false;
    }

    private void SetLeverVisualRotation(bool activated)
    {
        if (leverVisual == null) return;

        Vector3 targetEuler = activated ? activatedRotation : deactivatedRotation;
        leverVisual.localRotation = Quaternion.Euler(targetEuler);
    }

    // Public methods for external control
    public void SetLeverState(bool activated, bool animate = false)
    {
        if (isActivated == activated) return;

        isActivated = activated;

        if (animate && !isMoving)
        {
            StartCoroutine(MoveLever());
        }
        else
        {
            SetLeverVisualRotation(isActivated);
        }
    }

    public void ActivateLever(bool animate = false)
    {
        SetLeverState(true, animate);
    }

    public void DeactivateLever(bool animate = false)
    {
        SetLeverState(false, animate);
    }

    // Force immediate lever movement without player interaction
    public void ToggleLeverImmediate()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveLever());
        }
    }

    // Editor helper - call this in inspector to test rotations
    [ContextMenu("Test Activated Rotation")]
    private void TestActivatedRotation()
    {
        if (leverVisual == null) leverVisual = transform;
        leverVisual.localRotation = Quaternion.Euler(activatedRotation);
    }

    [ContextMenu("Test Deactivated Rotation")]
    private void TestDeactivatedRotation()
    {
        if (leverVisual == null) leverVisual = transform;
        leverVisual.localRotation = Quaternion.Euler(deactivatedRotation);
    }

    [ContextMenu("Reset To Start State")]
    private void ResetToStartState()
    {
        isActivated = startActivated;
        SetLeverVisualRotation(isActivated);
    }

    // Optional: Visual feedback methods
    private void OnDrawGizmosSelected()
    {
        // Draw IK position gizmo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(leverIKPosition), 0.1f);

        // Draw rotation visualization
        if (leverVisual != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(leverVisual.position, leverVisual.position + leverVisual.forward * 0.5f);
        }
    }
}