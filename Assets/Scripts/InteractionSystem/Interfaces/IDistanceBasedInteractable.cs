using UnityEngine;

public interface IDistanceBasedInteractable : IInteractable
{
    float InteractionDistance { get; }
}
