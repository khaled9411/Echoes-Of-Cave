using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask interactionLayerMask = -1;
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private Transform handInteractionPoint;

    private StarterAssets.StarterAssetsInputs input;
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable currentTargetInteractable;
    private IInteractable lastTargetInteractable;

    public IPickable heldObject { get; private set; } = null;
    private IContinuousInteractable currentContinuousInteractable = null;

    private void Start()
    {
        input = GetComponent<StarterAssets.StarterAssetsInputs>();
        if (detectionPoint == null)
            detectionPoint = transform;
        if (handInteractionPoint == null)
        {
            GameObject handPoint = new GameObject("HandInteractionPoint");
            handPoint.transform.SetParent(transform);
            handPoint.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
            handInteractionPoint = handPoint.transform;
        }
    }

    private void Update()
    {
        DetectInteractables();
        HandleInteractionInput();
        UpdateContinuousInteraction();
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

    private void HandleInteractionInput()
    {
        if (input.interact)
        {
            input.interact = false;

            if (heldObject != null)
            {
                if (currentTargetInteractable != null && currentTargetInteractable != heldObject)
                {
                    if (currentTargetInteractable.CanInteract(gameObject))
                    {
                        currentTargetInteractable.Interact(gameObject);
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
                if (currentTargetInteractable is IPickable pickableObject)
                {
                    heldObject = pickableObject;
                    pickableObject.PickUp(gameObject, handInteractionPoint);
                    currentTargetInteractable.Interact(gameObject);
                }
                else
                {
                    currentTargetInteractable.Interact(gameObject);
                }
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
                    if (currentContinuousInteractable == null || currentContinuousInteractable != fireSource)
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

    private void OnDrawGizmosSelected()
    {
        if (detectionPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);
        }
        if (handInteractionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(handInteractionPoint.position, 0.1f);
        }
    }
}