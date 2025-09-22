using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public MovingPlatformSystem targetPlatform;
    public TriggerType triggerType = TriggerType.OnPlayerEnter;
    public KeyCode activationKey = KeyCode.E;
    public bool requireInteraction = false;

    [Header("Interaction Messages")]
    public string interactionMessage = "Press E to activate platform";
    public GameObject interactionUI;

    private bool playerInRange = false;
    private bool hasTriggered = false;

    public enum TriggerType
    {
        OnPlayerEnter,
        OnKeyPress,
        OnInteraction,
        Automatic,
        OneTime
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (targetPlatform == null) return;

        switch (triggerType)
        {
            case TriggerType.OnKeyPress:
                if (Input.GetKeyDown(activationKey))
                {
                    targetPlatform.TogglePlatform();
                }
                break;

            case TriggerType.OnInteraction:
                if (playerInRange && Input.GetKeyDown(activationKey))
                {
                    targetPlatform.TogglePlatform();
                    HideInteractionUI();
                }
                break;

            case TriggerType.Automatic:
                if (!hasTriggered)
                {
                    targetPlatform.StartPlatform();
                    hasTriggered = true;
                }
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            switch (triggerType)
            {
                case TriggerType.OnPlayerEnter:
                    targetPlatform.StartPlatform();
                    break;

                case TriggerType.OneTime:
                    if (!hasTriggered)
                    {
                        targetPlatform.StartPlatform();
                        hasTriggered = true;
                    }
                    break;

                case TriggerType.OnInteraction:
                    ShowInteractionUI();
                    break;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionUI();
        }
    }

    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
        }

        Debug.Log(interactionMessage);
    }

    void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    // Public methods for external calls
    public void ActivatePlatform()
    {
        if (targetPlatform != null)
        {
            targetPlatform.StartPlatform();
        }
    }

    public void DeactivatePlatform()
    {
        if (targetPlatform != null)
        {
            targetPlatform.StopPlatform();
        }
    }

    public void TogglePlatform()
    {
        if (targetPlatform != null)
        {
            targetPlatform.TogglePlatform();
        }
    }
}