using UnityEngine;

public interface IConditionalInteractable : IInteractable
{
    bool MeetsInteractionCondition(GameObject interactor);
}
