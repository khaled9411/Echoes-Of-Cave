using UnityEngine;

public interface IContinuousInteractable : IInteractable
{
    void StartInteraction(GameObject interactor);
    void UpdateInteraction(GameObject interactor);
    void StopInteraction(GameObject interactor);
}
