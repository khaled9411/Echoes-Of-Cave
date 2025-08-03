using UnityEngine;

public interface IPullable : IInteractable
{
    bool CanPull(GameObject puller);
    void StartPull(GameObject puller);
    void UpdatePull(GameObject puller, Vector3 pullDirection, float playerSpeed);
    void StopPull(GameObject puller);
}
