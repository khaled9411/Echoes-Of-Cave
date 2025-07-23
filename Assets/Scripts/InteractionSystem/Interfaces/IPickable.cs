using UnityEngine;

public interface IPickable : IInteractable
{
    void PickUp(GameObject interactor, Transform parent);
    void Drop(GameObject interactor);
    bool IsPickedUp { get; }
}
