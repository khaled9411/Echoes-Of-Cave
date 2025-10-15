using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class InteractionManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask interactionLayerMask = -1;
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private Transform handInteractionPoint;

    [Header("IK Settings")]
    public TwoBoneIKConstraint rightHandIK;
    public TwoBoneIKConstraint leftHandIK;
    public Transform rightHandTarget;
    public Transform leftHandTarget;

    [Header("Push Settings")]
    [SerializeField] private float pushDetectionDistance = 0.4f;
    [SerializeField] private float pushSpeedReduction = 0.5f;
    [SerializeField] private float pushActivationThreshold = 0.3f;
    [SerializeField] private float pushStickyTime = 0.1f;
    [SerializeField] private float directionChangeThreshold = 0.3f;
    [SerializeField] private float directionStabilityTime = 0.1f;

    [Header("Frame Rate Independence")]
    [SerializeField] private float movementThreshold = 0.001f;
    [SerializeField] private float pushUpdateRate = 60f;
    [SerializeField] private bool useFrameRateIndependentPush = true;

    [Header("Anti-Spam Settings")]
    [SerializeField] private float interactionCooldown = 0.3f;
    [SerializeField] private bool debugSpamPrevention = false;

    public Transform HandInteractionPoint => handInteractionPoint;

    private StarterAssets.StarterAssetsInputs input;
    private StarterAssets.ThirdPersonController playerController;
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable currentTargetInteractable;
    private IInteractable lastTargetInteractable;

    public IPickable heldObject { get; private set; } = null;
    private IContinuousInteractable currentContinuousInteractable = null;

    private IPushable currentPushable = null;
    private IPullable currentPullable = null;
    private Vector3 lastPlayerPosition;
    private Vector3 currentMovementDirection;
    private Vector3 smoothedMovementDirection;
    private Vector3 lastPushDirection;
    private bool wasMovingLastFrame = false;
    private float originalMoveSpeed = 0f;
    private float originalSprintSpeed = 0f;
    private bool speedsModified = false;

    private float lastPushUpdateTime = 0f;
    private float pushUpdateInterval;
    private Vector3 accumulatedPlayerMovement = Vector3.zero;
    private float pushStickyTimer = 0f;

    private Vector3 movementDirectionBuffer = Vector3.zero;
    private float movementSmoothingFactor = 8f;

    private Vector3 previousMovementDirection = Vector3.zero;
    private float currentDirectionTime = 0f;

    // Anti-Spam Variables
    private bool isInteractionLocked = false;
    private float lastInteractionTime = -999f;
    private bool isProcessingInteraction = false;

    private void Start()
    {
        input = GetComponent<StarterAssets.StarterAssetsInputs>();
        playerController = GetComponent<StarterAssets.ThirdPersonController>();

        if (detectionPoint == null)
            detectionPoint = transform;
        if (handInteractionPoint == null)
        {
            GameObject handPoint = new GameObject("HandInteractionPoint");
            handPoint.transform.SetParent(transform);
            handPoint.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            handInteractionPoint = handPoint.transform;
        }

        lastPlayerPosition = transform.position;
        pushUpdateInterval = 1f / pushUpdateRate;
        lastPushUpdateTime = Time.time;

        if (playerController != null)
        {
            originalMoveSpeed = playerController.MoveSpeed;
            originalSprintSpeed = playerController.SprintSpeed;
        }
    }

    private void Update()
    {
        UpdateMovementTracking();
        DetectInteractables();

        if (useFrameRateIndependentPush)
        {
            HandlePushSystemFrameIndependent();
        }
        else
        {
            HandlePushSystem();
        }

        HandleInteractionInput();
        UpdateContinuousInteraction();
        UpdatePullSystem();
    }

    private void UpdateMovementTracking()
    {
        Vector3 currentPosition = transform.position;
        Vector3 positionDelta = currentPosition - lastPlayerPosition;

        if (positionDelta.magnitude > movementThreshold)
        {
            Vector3 rawDirection = positionDelta.normalized;

            if (previousMovementDirection != Vector3.zero)
            {
                float directionSimilarity = Vector3.Dot(rawDirection, previousMovementDirection);

                if (directionSimilarity < directionChangeThreshold)
                {
                    currentDirectionTime = 0f;
                    if (currentPushable != null)
                    {
                        StopCurrentPush();
                    }
                }
                else
                {
                    currentDirectionTime += Time.deltaTime;
                }
            }
            else
            {
                currentDirectionTime = 0f;
            }

            if (currentDirectionTime >= directionStabilityTime)
            {
                movementDirectionBuffer = Vector3.Lerp(movementDirectionBuffer, rawDirection,
                    movementSmoothingFactor * Time.deltaTime);
            }
            else
            {
                movementDirectionBuffer = Vector3.Lerp(movementDirectionBuffer, rawDirection,
                    movementSmoothingFactor * 3f * Time.deltaTime);
            }

            currentMovementDirection = rawDirection;
            smoothedMovementDirection = movementDirectionBuffer;
            previousMovementDirection = rawDirection;
            wasMovingLastFrame = true;

            accumulatedPlayerMovement += positionDelta;
        }
        else
        {
            wasMovingLastFrame = false;
            currentDirectionTime = 0f;

            movementDirectionBuffer = Vector3.Lerp(movementDirectionBuffer, Vector3.zero,
                movementSmoothingFactor * 0.5f * Time.deltaTime);
        }

        lastPlayerPosition = currentPosition;
    }

    private void HandlePushSystemFrameIndependent()
    {
        if (currentPullable != null && heldObject != null) return;

        float currentTime = Time.time;
        bool shouldUpdatePush = (currentTime - lastPushUpdateTime) >= pushUpdateInterval;

        IPushable nearestPushable = GetNearestPushable();

        if (pushStickyTimer > 0f)
        {
            pushStickyTimer -= Time.deltaTime;
        }

        if (nearestPushable != null && wasMovingLastFrame)
        {
            Transform pushableTransform = (nearestPushable as MonoBehaviour).transform;
            Vector3 directionToPushable = (pushableTransform.position - transform.position).normalized;
            float distanceToPushable = Vector3.Distance(transform.position, pushableTransform.position);

            bool isCloseEnough = distanceToPushable <= pushDetectionDistance;
            bool isMovingTowardsPushable = Vector3.Dot(smoothedMovementDirection.normalized, directionToPushable) > pushActivationThreshold;
            bool hasEnoughMovement = accumulatedPlayerMovement.magnitude > movementThreshold;

            if (currentPushable == null)
            {
                if (isCloseEnough && isMovingTowardsPushable && hasEnoughMovement)
                {
                    if (nearestPushable.CanPush(gameObject, currentMovementDirection))
                    {
                        currentPushable = nearestPushable;
                        currentPushable.StartPush(gameObject, currentMovementDirection);
                        lastPushDirection = currentMovementDirection;
                        pushStickyTimer = pushStickyTime;

                        ModifyPlayerSpeed(pushSpeedReduction, false);
                    }
                }
            }
            else if (currentPushable == nearestPushable)
            {
                bool isSameDirection = Vector3.Dot(currentMovementDirection.normalized, lastPushDirection.normalized) > 0.5f;
                bool isStillMovingTowardsPushable = Vector3.Dot(currentMovementDirection.normalized, directionToPushable) > 0.3f;

                if (!isSameDirection || !isStillMovingTowardsPushable || !isCloseEnough)
                {
                    StopCurrentPush();
                }
                else if (shouldUpdatePush && hasEnoughMovement)
                {
                    float currentPlayerSpeed = GetCurrentPlayerSpeed();
                    currentPushable.UpdatePush(gameObject, currentMovementDirection, currentPlayerSpeed);
                    lastPushDirection = currentMovementDirection;
                    lastPushUpdateTime = currentTime;

                    accumulatedPlayerMovement = Vector3.zero;
                    pushStickyTimer = pushStickyTime;
                }
            }
            else if (currentPushable != nearestPushable)
            {
                StopCurrentPush();
            }
        }
        else if (currentPushable != null)
        {
            if (!wasMovingLastFrame && pushStickyTimer <= 0f)
            {
                StopCurrentPush();
            }
            else if (pushStickyTimer > 0f && nearestPushable == currentPushable)
            {
                Transform pushableTransform = (nearestPushable as MonoBehaviour).transform;
                float distanceToPushable = Vector3.Distance(transform.position, pushableTransform.position);

                if (distanceToPushable > pushDetectionDistance * 1.5f)
                {
                    StopCurrentPush();
                }
            }
            else
            {
                StopCurrentPush();
            }
        }
    }

    private void HandlePushSystem()
    {
        if (currentPullable != null && heldObject != null) return;

        IPushable nearestPushable = GetNearestPushable();

        if (nearestPushable != null && wasMovingLastFrame)
        {
            Transform pushableTransform = (nearestPushable as MonoBehaviour).transform;
            Vector3 directionToPushable = (pushableTransform.position - transform.position).normalized;
            float distanceToPushable = Vector3.Distance(transform.position, pushableTransform.position);

            bool isCloseEnough = distanceToPushable <= pushDetectionDistance;
            bool isMovingTowardsPushable = Vector3.Dot(currentMovementDirection, directionToPushable) > 0.7f;

            if (currentPushable == null)
            {
                if (isCloseEnough && isMovingTowardsPushable)
                {
                    if (nearestPushable.CanPush(gameObject, currentMovementDirection))
                    {
                        currentPushable = nearestPushable;
                        currentPushable.StartPush(gameObject, currentMovementDirection);
                        lastPushDirection = currentMovementDirection;

                        ModifyPlayerSpeed(pushSpeedReduction, false);
                    }
                }
            }
            else if (currentPushable == nearestPushable)
            {
                bool isSameDirection = Vector3.Dot(currentMovementDirection, lastPushDirection) > 0.5f;
                bool isStillMovingTowardsPushable = Vector3.Dot(currentMovementDirection, directionToPushable) > 0.3f;

                if (isSameDirection && isStillMovingTowardsPushable && isCloseEnough)
                {
                    float currentPlayerSpeed = GetCurrentPlayerSpeed();
                    currentPushable.UpdatePush(gameObject, currentMovementDirection, currentPlayerSpeed);
                    lastPushDirection = currentMovementDirection;
                }
                else
                {
                    StopCurrentPush();
                }
            }
            else
            {
                StopCurrentPush();
            }
        }
        else if (currentPushable != null)
        {
            StopCurrentPush();
        }
    }

    private IPushable GetNearestPushable()
    {
        IPushable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var interactable in nearbyInteractables)
        {
            if (interactable is IPushable pushable && interactable is MonoBehaviour mono)
            {
                float distance = Vector3.Distance(transform.position, mono.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = pushable;
                }
            }
        }

        return nearest;
    }

    private void StopCurrentPush()
    {
        if (currentPushable != null)
        {
            currentPushable.StopPush(gameObject);
            currentPushable = null;
            pushStickyTimer = 0f;
            accumulatedPlayerMovement = Vector3.zero;

            RestorePlayerSpeed();
        }
    }

    private void DetectInteractables()
    {
        nearbyInteractables.Clear();

        Collider[] colliders = Physics.OverlapSphere(detectionPoint.position, detectionRadius, interactionLayerMask);

        foreach (var collider in colliders)
        {
            var interactables = collider.GetComponents<IInteractable>();
            foreach (var interactable in interactables)
            {
                if (interactable.CanInteract(gameObject))
                {
                    if (interactable is IDistanceBasedInteractable distanceBased)
                    {
                        float distance = Vector3.Distance(transform.position, collider.transform.position);
                        if (distance <= distanceBased.InteractionDistance)
                        {
                            nearbyInteractables.Add(interactable);
                        }
                    }
                    else
                    {
                        nearbyInteractables.Add(interactable);
                    }
                }
            }
        }

        UpdateInteractionUI();
    }

    private void UpdatePullSystem()
    {
        if (currentPullable != null && wasMovingLastFrame)
        {
            float currentPlayerSpeed = GetCurrentPlayerSpeed();
            currentPullable.UpdatePull(gameObject, currentMovementDirection, currentPlayerSpeed);
        }
    }

    private void UpdateInteractionUI()
    {
        lastTargetInteractable = currentTargetInteractable;
        currentTargetInteractable = GetClosestInteractable();

        if (lastTargetInteractable != null && lastTargetInteractable != currentTargetInteractable)
        {
            if (lastTargetInteractable is IInteractionFeedback previousFeedback && previousFeedback != heldObject)
            {
                previousFeedback.HideHighlight();
                previousFeedback.HideInteractionUI();
            }
        }

        InteractionUIManager.Instance?.HidePrompt();

        if (currentTargetInteractable != null)
        {
            if (currentTargetInteractable is IInteractionFeedback currentFeedback)
            {
                currentFeedback.ShowHighlight();
                currentFeedback.ShowInteractionUI(currentTargetInteractable.InteractionPrompt);
            }
        }

        if (heldObject != null)
        {
            if (heldObject is IInteractionFeedback heldFeedback)
            {
                if (currentTargetInteractable != null && heldObject != currentTargetInteractable)
                {
                    InteractionUIManager.Instance?.ShowPrompt(currentTargetInteractable.InteractionPrompt);
                }
                else
                {
                    InteractionUIManager.Instance?.ShowPrompt(heldObject.InteractionPrompt);
                }
            }
        }
    }

    private IInteractable GetClosestInteractable()
    {
        if (nearbyInteractables.Count == 0) return null;

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (var interactable in nearbyInteractables)
        {
            if (interactable is IPushable) continue;

            if (interactable is MonoBehaviour mono)
            {
                float distance = Vector3.Distance(transform.position, mono.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
        }
        return closest;
    }

    public void HandleInteractionInput()
    {
        // Check cooldown
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            if (debugSpamPrevention && input.interact)
            {
                Debug.LogWarning($"[SPAM PREVENTION] Interaction blocked - cooldown active ({Time.time - lastInteractionTime:F2}s)");
            }
            input.interact = false;
            return;
        }

        // Check if already processing
        if (isProcessingInteraction)
        {
            if (debugSpamPrevention && input.interact)
            {
                Debug.LogWarning("[SPAM PREVENTION] Interaction blocked - already processing interaction");
            }
            input.interact = false;
            return;
        }

        // Check interaction lock
        if (isInteractionLocked)
        {
            if (debugSpamPrevention && input.interact)
            {
                Debug.LogWarning("[SPAM PREVENTION] Interaction blocked - interaction locked");
            }
            input.interact = false;
            return;
        }

        if (input.interact)
        {
            input.interact = false;
            lastInteractionTime = Time.time;
            isProcessingInteraction = true;

            ProcessInteraction();

            isProcessingInteraction = false;
        }
    }

    private void ProcessInteraction()
    {
        if (currentTargetInteractable is IPullable pullable)
        {
            if (currentPullable == pullable)
            {
                currentPullable.StopPull(gameObject);
                currentPullable = null;
                RestorePlayerSpeed();
            }
            else if (currentPullable == null)
            {
                currentPullable = pullable;
                currentPullable.StartPull(gameObject);
            }
            return;
        }

        if (heldObject != null)
        {
            if (currentTargetInteractable != null && currentTargetInteractable != heldObject)
            {
                if (currentTargetInteractable.CanInteract(gameObject))
                {
                    if (currentTargetInteractable is StoneSlotInteractable stoneSlot)
                    {
                        // Lock interaction during stone placement
                        Debug.Log("Placing stone in slot...");
                        LockInteraction();
                        stoneSlot.Interact(gameObject);
                        return;
                    }
                    else
                    {
                        currentTargetInteractable.Interact(gameObject);
                    }
                }
                else
                {
                    DropHeldObject();
                }
            }
            else
            {
                DropHeldObject();
            }
        }
        else if (currentTargetInteractable != null)
        {
            if (currentTargetInteractable is StoneSlotInteractable stoneSlot && stoneSlot.PlacedStone != null)
            {
                // Lock interaction during stone pickup
                Debug.Log("Picking up stone from slot...");
                LockInteraction();
                stoneSlot.Interact(gameObject);
            }
            else if (currentTargetInteractable is IPickable pickableObject)
            {
                // Lock interaction during pickup
                Debug.Log("Picking up object...");
                LockInteraction();
                heldObject = pickableObject;
                pickableObject.PickUp(gameObject, handInteractionPoint);
            }
            else
            {
                currentTargetInteractable.Interact(gameObject);
            }
        }
    }

    private void DropHeldObject()
    {
        if (heldObject == null) return;

        heldObject.Drop(gameObject);
        if (heldObject is IInteractionFeedback heldFeedback)
        {
            heldFeedback.HideInteractionUI();
            heldFeedback.HideHighlight();
        }
        heldObject = null;
        currentContinuousInteractable = null;
    }

    private void UpdateContinuousInteraction()
    {
        if (input.interactHold)
        {
            if (heldObject is TorchInteractable torch && !torch.IsLit)
            {
                FireSourceInteractable fireSource = GetClosestFireSource();
                if (fireSource != null && fireSource.CanInteract(gameObject))
                {
                    if (currentContinuousInteractable == null)
                    {
                        currentContinuousInteractable = fireSource;
                        fireSource.StartInteraction(gameObject);
                    }
                    fireSource.UpdateInteraction(gameObject);
                    if (InteractionUIManager.Instance != null && currentContinuousInteractable is ContinuousInteractable ci)
                    {
                        InteractionUIManager.Instance.UpdateProgress(ci.Progress);
                    }
                }
                else if (currentContinuousInteractable != null && currentContinuousInteractable is FireSourceInteractable)
                {
                    currentContinuousInteractable.StopInteraction(gameObject);
                    currentContinuousInteractable = null;
                    InteractionUIManager.Instance?.UpdateProgress(0f);
                }
            }
            else if (currentContinuousInteractable != null)
            {
                currentContinuousInteractable.UpdateInteraction(gameObject);
                if (InteractionUIManager.Instance != null && currentContinuousInteractable is ContinuousInteractable ci)
                {
                    InteractionUIManager.Instance.UpdateProgress(ci.Progress);
                }
            }
        }
        else
        {
            if (currentContinuousInteractable != null)
            {
                currentContinuousInteractable.StopInteraction(gameObject);
                currentContinuousInteractable = null;
                InteractionUIManager.Instance?.UpdateProgress(0f);
            }
        }
    }

    public void ClearHeldObject()
    {
        //if (isInteractionLocked)
        //{
        //    if (debugSpamPrevention)
        //    {
        //        Debug.LogWarning("[SPAM PREVENTION] ClearHeldObject called while locked - ignoring");
        //    }
        //    return;
        //}

        heldObject = null;
        InteractionUIManager.Instance?.HidePrompt();
    }

    private FireSourceInteractable GetClosestFireSource()
    {
        FireSourceInteractable closestSource = null;
        float closestDistance = float.MaxValue;

        foreach (var interactable in nearbyInteractables)
        {
            if (interactable is FireSourceInteractable fireSource)
            {
                if (fireSource is MonoBehaviour mono)
                {
                    float distance = Vector3.Distance(transform.position, mono.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestSource = fireSource;
                    }
                }
            }
        }
        return closestSource;
    }

    public void ForcePickup(IPickable itemToPickup)
    {
        if (itemToPickup == null) return;

        //if (isInteractionLocked)
        //{
        //    if (debugSpamPrevention)
        //    {
        //        Debug.LogWarning("[SPAM PREVENTION] ForcePickup called while locked - ignoring");
        //    }
        //    return;
        //}

        ClearHeldObject();

        heldObject = itemToPickup;
        itemToPickup.PickUp(gameObject, handInteractionPoint);
    }

    public void StopAllPushPull()
    {
        StopCurrentPush();

        if (currentPullable != null)
        {
            currentPullable.StopPull(gameObject);
            currentPullable = null;
            RestorePlayerSpeed();
        }
    }

    public void ModifyPlayerSpeed(float speedReduction, bool allowSprint)
    {
        if (playerController == null || speedsModified) return;

        speedsModified = true;
        playerController.MoveSpeed *= speedReduction;

        if (!allowSprint)
        {
            playerController.SprintSpeed = playerController.MoveSpeed;
        }
        else
        {
            playerController.SprintSpeed *= speedReduction;
        }
    }

    public void RestorePlayerSpeed()
    {
        if (playerController == null || !speedsModified) return;

        speedsModified = false;
        playerController.MoveSpeed = originalMoveSpeed;
        playerController.SprintSpeed = originalSprintSpeed;
    }

    private float GetCurrentPlayerSpeed()
    {
        if (playerController == null) return 1f;

        if (input.sprint && playerController.SprintSpeed > playerController.MoveSpeed)
        {
            return playerController.SprintSpeed;
        }

        return playerController.MoveSpeed;
    }

    // Anti-Spam Methods
    public void LockInteraction()
    {
        isInteractionLocked = true;
        if (debugSpamPrevention)
        {
            Debug.Log("[SPAM PREVENTION] Interaction LOCKED");
        }
    }

    public void UnlockInteraction()
    {
        isInteractionLocked = false;
        if (debugSpamPrevention)
        {
            Debug.Log("[SPAM PREVENTION] Interaction UNLOCKED");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detectionPoint.position, pushDetectionDistance);
        }
        if (handInteractionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(handInteractionPoint.position, 0.1f);
        }

        if (Application.isPlaying && currentPushable != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, currentMovementDirection * 2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.2f, smoothedMovementDirection * 2f);
        }
    }

    Tween cuurentRightTween = null;
    Tween cuurentLeftTween = null;
    public void LerpRightHandWeight(float target, float duration)
    {
        if (cuurentRightTween != null && cuurentRightTween.IsActive())
        {
            cuurentRightTween.Kill();
        }

        cuurentRightTween = DOTween.To(
                () => rightHandIK.weight,
                x => rightHandIK.weight = x,
                target,
                duration
            );
    }
    public void LerpLeftHandWeight(float target, float duration)
    {
        if (cuurentLeftTween != null && cuurentLeftTween.IsActive())
        {
            cuurentLeftTween.Kill();
        }

        cuurentLeftTween = DOTween.To(
                () => leftHandIK.weight,
                x => leftHandIK.weight = x,
                target,
                duration
            );
    }
}