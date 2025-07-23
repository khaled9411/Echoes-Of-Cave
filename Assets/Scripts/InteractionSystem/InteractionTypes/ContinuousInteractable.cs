using UnityEngine;
using UnityEngine.Events;


public class ContinuousInteractable : BaseInteractable, IContinuousInteractable
{
    [Header("Continuous Interaction")]
    [SerializeField] private float interactionDuration = 3f;
    [SerializeField] UnityEvent onInteractionComplete;
    [SerializeField] UnityEvent<float> onInteractionProgress;

    private bool isInteracting = false;
    private float currentInteractionTime = 0f;
    private GameObject currentInteractor;

    public override void Interact(GameObject interactor)
    {
        if (!isInteracting)
        {
            StartInteraction(interactor);
        }
    }

    public void StartInteraction(GameObject interactor)
    {
        isInteracting = true;
        currentInteractor = interactor;
        currentInteractionTime = 0f;
        PlayInteractionSound();
    }

    public void UpdateInteraction(GameObject interactor)
    {
        if (isInteracting && interactor == currentInteractor)
        {
            currentInteractionTime += Time.deltaTime;
            float progress = currentInteractionTime / interactionDuration;

            onInteractionProgress?.Invoke(progress);

            if (currentInteractionTime >= interactionDuration)
            {
                CompleteInteraction();
            }
        }
    }

    public void StopInteraction(GameObject interactor)
    {
        if (isInteracting && interactor == currentInteractor)
        {
            isInteracting = false;
            currentInteractor = null;
            currentInteractionTime = 0f;
            onInteractionProgress?.Invoke(0f);
        }
    }

    private void CompleteInteraction()
    {
        isInteracting = false;
        currentInteractor = null;
        onInteractionComplete?.Invoke();
    }

    public bool IsInteracting => isInteracting;
    public float Progress => currentInteractionTime / interactionDuration;
}
