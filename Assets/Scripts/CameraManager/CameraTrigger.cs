using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public int targetCameraIndex = 0;
    public bool isOpenAreaCamera = true;
    public bool useSmoothTransition = true;

    [Header("Trigger Settings")]
    public bool triggerOnEnter = true;
    public bool triggerOnExit = false;

    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool showDebugMessages = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter) return;

        if (other.CompareTag(playerTag))
        {
            TriggerCameraSwitch();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!triggerOnExit) return;

        if (other.CompareTag(playerTag))
        {
            TriggerCameraSwitch();
        }
    }

    private void TriggerCameraSwitch()
    {
        if (CameraManager.Instance == null)
        {
            Debug.LogError("Camera Manager not found in scene!");
            return;
        }

        if (showDebugMessages)
        {
            Debug.Log($"Camera Trigger: {gameObject.name} - Switching to camera {targetCameraIndex} - " +
                     $"{(isOpenAreaCamera ? "Open Area" : "Closed Room")}");
        }

        if (isOpenAreaCamera)
        {
            CameraManager.Instance.SwitchToOpenAreaCamera(targetCameraIndex, useSmoothTransition);
        }
        else
        {
            CameraManager.Instance.SwitchToClosedRoomCamera(targetCameraIndex, useSmoothTransition);
        }
    }

    public void ManualTrigger()
    {
        TriggerCameraSwitch();
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = isOpenAreaCamera ? Color.green : Color.red;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}