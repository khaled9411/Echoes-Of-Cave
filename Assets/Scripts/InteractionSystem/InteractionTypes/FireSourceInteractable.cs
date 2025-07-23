using UnityEngine;

public class FireSourceInteractable : BaseInteractable, IContinuousInteractable
{
    [Header("Fire Source Settings")]
    [SerializeField] private float relightDuration = 2f;
    [SerializeField] private string defaultPrompt = "Hold E to interact";
    [SerializeField] private string relightHoldingPrompt = "Relighting torch...";

    private bool isInteractingContinuously = false;
    private float currentInteractionTime = 0f;
    private GameObject currentInteractor; 
    public override string InteractionPrompt
    {
        get
        {
            if (currentInteractor != null)
            {
                var manager = currentInteractor.GetComponent<InteractionManager>();
                if (manager != null && manager.heldObject is TorchInteractable torch && !torch.IsLit)
                {
                    return relightHoldingPrompt;
                }
            }
            return defaultPrompt;
        }
    }

    public override bool CanInteract(GameObject interactor)
    {
        var manager = interactor.GetComponent<InteractionManager>();
        return base.CanInteract(interactor) && manager != null && manager.heldObject is TorchInteractable torch && !torch.IsLit;
    }

    public override void Interact(GameObject interactor)
    {
        PlayInteractionSound();
    }

    public void StartInteraction(GameObject interactor)
    {
        if (isInteractingContinuously) return;

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager != null && manager.heldObject is TorchInteractable torch && !torch.IsLit)
        {
            isInteractingContinuously = true;
            currentInteractor = interactor;
            currentInteractionTime = 0f;
            Debug.Log("Started relighting interaction with fire source.");
            ShowInteractionUI(relightHoldingPrompt);
        }
    }

    public void UpdateInteraction(GameObject interactor)
    {
        if (!isInteractingContinuously || interactor != currentInteractor) return;

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager == null || !(manager.heldObject is TorchInteractable torch) || torch.IsLit)
        {
            StopInteraction(interactor);
            return;
        }

        currentInteractionTime += Time.deltaTime;
        float progress = currentInteractionTime / relightDuration;

        InteractionUIManager.Instance?.UpdateProgress(progress);

        if (currentInteractionTime >= relightDuration)
        {
            torch.RelightTorch();
            StopInteraction(interactor);
        }
    }

    public void StopInteraction(GameObject interactor)
    {
        if (!isInteractingContinuously) return;

        isInteractingContinuously = false;
        currentInteractionTime = 0f;
        currentInteractor = null;
        InteractionUIManager.Instance?.UpdateProgress(0f);
        Debug.Log("Stopped relighting interaction.");
        HideInteractionUI();
    }
}
