using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(BoxCollider))]
public class PushDetectionTrigger : MonoBehaviour
{
    [Header("Detection Box Settings")]
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] private Vector3 detectionBoxOffset = new Vector3(0f, 1f, 0.5f);

    [Header("References")]
    [SerializeField] private ProceduralPushController pushController;

    private BoxCollider detectionCollider;

    void Start()
    {
        detectionCollider = GetComponent<BoxCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.size = detectionBoxSize;
        detectionCollider.center = detectionBoxOffset;

        if (!pushController)
        {
            pushController = GetComponentInParent<ProceduralPushController>();
            if (!pushController)
            {
                pushController = transform.root.GetComponent<ProceduralPushController>();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (pushController != null)
        {
            pushController.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        pushController?.SendMessage("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerExit(Collider other)
    {
        if (pushController != null)
        {
            pushController.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 boxSize = detectionCollider ? detectionCollider.size : detectionBoxSize;
        Vector3 boxCenter = detectionCollider ? detectionCollider.center : detectionBoxOffset;

        Gizmos.DrawCube(boxCenter, boxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(boxCenter, boxSize);

        Gizmos.matrix = oldMatrix;
    }
}