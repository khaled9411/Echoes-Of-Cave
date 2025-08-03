using UnityEngine;

public interface IPushable : IInteractable
{
    bool CanPush(GameObject pusher, Vector3 pushDirection);
    void StartPush(GameObject pusher, Vector3 pushDirection);
    void UpdatePush(GameObject pusher, Vector3 pushDirection, float playerSpeed);
    void StopPush(GameObject pusher);
    Vector3 GetCurrentVelocity();
    Vector3 GetAccumulatedMovement();
}