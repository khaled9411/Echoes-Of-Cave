using DG.Tweening;
using StarterAssets;
using System.Collections;
using UnityEngine;

public class FireSourceInteractable : BaseInteractable, IContinuousInteractable
{
    [Header("Fire Source Settings")]
    [SerializeField] private float relightDuration = 2f;
    [SerializeField] private string defaultPrompt = "Hold E to interact";
    [SerializeField] private string relightHoldingPrompt = "Relighting torch...";
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 holdIKConstrintPosition;
    [SerializeField] private Vector3 holdIKConstrintRotation;

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

        ThirdPersonController controller = interactor.GetComponent<ThirdPersonController>();
        if (controller != null)
            controller.disableMovement = true;

        StartCoroutine(SmoothRotateAndAnimate(interactor));

        var manager = interactor.GetComponent<InteractionManager>();
        if (manager != null && manager.heldObject is TorchInteractable torch && !torch.IsLit)
        {
            isInteractingContinuously = true;
            currentInteractor = interactor;
            currentInteractionTime = 0f;
            Debug.Log("Started relighting interaction with fire source.");
            ShowInteractionUI(relightHoldingPrompt);
            InteractionManager interactionManager = interactor.GetComponent<InteractionManager>();
            if (interactionManager != null)
            {
                interactionManager.rightHandTarget.DOKill();
                interactionManager.rightHandTarget.DOLocalMove(holdIKConstrintPosition, 0.5f)
                    .SetEase(Ease.InOutSine);
                interactionManager.rightHandTarget.DOLocalRotate(holdIKConstrintRotation, 0.5f)
                    .SetEase(Ease.InOutSine);

                //interactionManager.rightHandTarget.localPosition = holdIKConstrintPosition;
                //interactionManager.rightHandTarget.localRotation = Quaternion.Euler(holdIKConstrintRotation);
                //interactionManager.LerpRightHandWeight(1, 1);
            }

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

        var manager = interactor.GetComponent<InteractionManager>();
        InteractionManager interactionManager = interactor.GetComponent<InteractionManager>();
        if (manager.heldObject is TorchInteractable torch && interactionManager != null)
        {
            interactionManager.rightHandTarget.DOKill();
            interactionManager.rightHandTarget.DOLocalMove(torch.holdIKConstrintPosition, 0.5f)
                .SetEase(Ease.InOutSine);
            interactionManager.rightHandTarget.DOLocalRotate(torch.holdIKConstrintRotation, 0.5f)
                .SetEase(Ease.InOutSine);

            //interactionManager.rightHandTarget.localPosition = torch.holdIKConstrintPosition;
            //interactionManager.rightHandTarget.localRotation = Quaternion.Euler(torch.holdIKConstrintRotation);
        } 
        

        //if (interactionManager != null)
        //    interactionManager.LerpRightHandWeight(0.9f, 1f);


        isInteractingContinuously = false;
        currentInteractionTime = 0f;
        currentInteractor = null;
        InteractionUIManager.Instance?.UpdateProgress(0f);
        Debug.Log("Stopped relighting interaction.");
        HideInteractionUI();

        ThirdPersonController controller = interactor.GetComponent<ThirdPersonController>();
        if (controller != null)
            controller.disableMovement = false;
    }

    private IEnumerator SmoothRotateAndAnimate(GameObject interactor)
    {
        Debug.Log($"Starting rotation and animation coroutine in {interactor}.");
        Animator playerAnimator = interactor.GetComponent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogError("Player GameObject does not have an Animator component!");
            yield break;
        }

        Vector3 direction = (transform.position - interactor.transform.position).normalized;

        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        bool isRotating = true;

        while (isRotating)
        {
            interactor.transform.rotation = Quaternion.Slerp(interactor.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(interactor.transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }

            yield return null;
        }

        //yield return new WaitForSeconds(0.5f);

        //// relight logic can start here

        //var manager = interactor.GetComponent<InteractionManager>();
        //if (manager == null || !(manager.heldObject is TorchInteractable torch) || torch.IsLit)
        //{
        //    StopInteraction(interactor);
        //    yield break;
        //}

        //currentInteractionTime += Time.deltaTime;
        //float progress = currentInteractionTime / relightDuration;

        //InteractionUIManager.Instance?.UpdateProgress(progress);

        //if (currentInteractionTime >= relightDuration)
        //{
        //    torch.RelightTorch();
        //    StopInteraction(interactor);
        //}

    }
}
