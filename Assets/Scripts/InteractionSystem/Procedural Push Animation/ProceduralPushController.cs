using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class ProceduralPushController : MonoBehaviour
{
    [Header("IK Setup")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Vector3 leftHandRotation;
    [SerializeField] private Vector3 rightHandRotation;

    [Header("Detection Settings")]
    [SerializeField] private Transform leftShoulderReference;
    [SerializeField] private Transform rightShoulderReference;
    [SerializeField] private float maxPushAngle = 45f;
    [SerializeField] private float pushHeight = 1.2f;
    [SerializeField] private float handOffset = 0.1f;
    [SerializeField] private float revalidateDelay = 0.25f;


    [Header("Animation Settings")]
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float lastExitTime;
    private PushableInteractable currentPushable;
    private bool isPushing = false;
    private Coroutine pushCoroutine;

    private float originalLeftWeight;
    private float originalRightWeight;

    void Start()
    {
        if (leftHandIK) originalLeftWeight = leftHandIK.weight;
        if (rightHandIK) originalRightWeight = rightHandIK.weight;

        if (!leftHandTarget)
        {
            GameObject leftTarget = new GameObject("LeftHandPushTarget");
            leftTarget.transform.parent = transform;
            leftHandTarget = leftTarget.transform;
        }

        if (!rightHandTarget)
        {
            GameObject rightTarget = new GameObject("RightHandPushTarget");
            rightTarget.transform.parent = transform;
            rightHandTarget = rightTarget.transform;
        }

        SetIKWeight(0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if(TryGetComponent<InteractionManager>(out InteractionManager interactor) && interactor.heldObject != null)
            return;

        PushableInteractable pushable = other.GetComponent<PushableInteractable>();
        if (pushable != null && !isPushing)
        {
            StartCoroutine(DelayedValidation(pushable, other));
        }
    }

    IEnumerator DelayedValidation(PushableInteractable pushable, Collider other)
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(i * 0.2f);

            Vector3 leftClosestPoint = other.ClosestPoint(leftShoulderReference.position);
            Vector3 rightClosestPoint = other.ClosestPoint(rightShoulderReference.position);

            if (IsValidPushPosition(leftClosestPoint, rightClosestPoint, other))
            {
                currentPushable = pushable;
                StartPushing(leftClosestPoint, rightClosestPoint, other);
                yield break;
            }
        }
    }


    void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exited: " + other.name);
        PushableInteractable pushable = other.GetComponent<PushableInteractable>();
        if (pushable != null && pushable == currentPushable)
        {
            StopPushing();
        }
    }

    void UpdateHandTargets(Vector3 leftPoint, Vector3 rightPoint, Collider objectCollider)
    {
        Vector3 finalLeftPoint = GetEdgePoint(leftPoint, objectCollider, true);
        Vector3 finalRightPoint = GetEdgePoint(rightPoint, objectCollider, false);

        Vector3 normal = (transform.position - finalLeftPoint).normalized;
        normal.y = 0;
        finalLeftPoint += normal * handOffset;
        finalRightPoint += normal * handOffset;

        leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, finalLeftPoint, Time.deltaTime * transitionSpeed);
        rightHandTarget.position = Vector3.Lerp(rightHandTarget.position, finalRightPoint, Time.deltaTime * transitionSpeed);
    }

    void OnTriggerStay(Collider other)
    {
        if (!isPushing || currentPushable == null) return;

        if (other.GetComponent<PushableInteractable>() == currentPushable)
        {
            Vector3 leftClosestPoint = other.ClosestPoint(leftShoulderReference.position);
            Vector3 rightClosestPoint = other.ClosestPoint(rightShoulderReference.position);

            if (IsValidPushPosition(leftClosestPoint, rightClosestPoint, other))
            {
                UpdateHandTargets(leftClosestPoint, rightClosestPoint, other);
            }
            else
            {
                StopPushing();
            }
        }
    }


    bool IsValidPushPosition(Vector3 leftPoint, Vector3 rightPoint, Collider col)
    {
        if (Time.time - lastExitTime < revalidateDelay)
            return false;

        float avgHeight = (leftPoint.y + rightPoint.y) / 2f;
        float playerHeight = transform.position.y;

        if (Mathf.Abs(avgHeight - playerHeight) > pushHeight)
            return false;

        Vector3 playerShoulderMid = (leftShoulderReference.position + rightShoulderReference.position) / 2f;
        Vector3 objectMid = (leftPoint + rightPoint) / 2f;

        Vector3 rawDirection = (objectMid - playerShoulderMid).normalized;
        rawDirection.y = 0;

        float angle = Vector3.Angle(transform.forward, rawDirection);

        return angle <= maxPushAngle + 5f;
    }



    void StartPushing(Vector3 leftPoint, Vector3 rightPoint, Collider objectCollider)
    {
        isPushing = true;

        if (pushCoroutine != null)
            StopCoroutine(pushCoroutine);

        pushCoroutine = StartCoroutine(AnimateHandsToPushPosition(leftPoint, rightPoint, objectCollider));
    }

    void StopPushing()
    {
        isPushing = false;
        currentPushable = null;
        lastExitTime = Time.time;

        if (pushCoroutine != null)
            StopCoroutine(pushCoroutine);

        pushCoroutine = StartCoroutine(AnimateHandsToDefault());
    }


    IEnumerator AnimateHandsToPushPosition(Vector3 leftPoint, Vector3 rightPoint, Collider objectCollider)
    {
        float elapsedTime = 0f;
        float duration = 1f / transitionSpeed;

        Vector3 finalLeftPoint = GetEdgePoint(leftPoint, objectCollider, true);
        Vector3 finalRightPoint = GetEdgePoint(rightPoint, objectCollider, false);

        Vector3 normal = (transform.position - finalLeftPoint).normalized;
        normal.y = 0;
        finalLeftPoint += normal * handOffset;
        finalRightPoint += normal * handOffset;

        Vector3 startLeftPos = leftHandTarget.position;
        Vector3 startRightPos = rightHandTarget.position;

        leftHandTarget.localRotation = Quaternion.Euler(leftHandRotation);
        rightHandTarget.localRotation = Quaternion.Euler(rightHandRotation);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsedTime / duration);

            leftHandTarget.position = Vector3.Lerp(startLeftPos, finalLeftPoint, t);
            rightHandTarget.position = Vector3.Lerp(startRightPos, finalRightPoint, t);

            SetIKWeight(t);

            yield return null;
        }

        leftHandTarget.position = finalLeftPoint;
        rightHandTarget.position = finalRightPoint;
        SetIKWeight(1f);
    }

    IEnumerator AnimateHandsToDefault()
    {
        float elapsedTime = 0f;
        float duration = 0.5f / transitionSpeed;
        float startWeight = leftHandIK.weight;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsedTime / duration);

            SetIKWeight(Mathf.Lerp(startWeight, 0f, t));

            yield return null;
        }

        SetIKWeight(0f);
    }

    Vector3 GetEdgePoint(Vector3 closestPoint, Collider col, bool isLeft)
    {
        if (col is BoxCollider boxCol)
        {
            Bounds bounds = boxCol.bounds;
            Vector3 localPoint = col.transform.InverseTransformPoint(closestPoint);
            Vector3 size = boxCol.size / 2f;

            float distToLeft = Mathf.Abs(localPoint.x + size.x);
            float distToRight = Mathf.Abs(localPoint.x - size.x);
            float distToFront = Mathf.Abs(localPoint.z + size.z);
            float distToBack = Mathf.Abs(localPoint.z - size.z);

            float minDist = Mathf.Min(distToLeft, distToRight, distToFront, distToBack);

            if (minDist == distToLeft)
                localPoint.x = -size.x;
            else if (minDist == distToRight)
                localPoint.x = size.x;
            else if (minDist == distToFront)
                localPoint.z = -size.z;
            else
                localPoint.z = size.z;

            return col.transform.TransformPoint(localPoint);
        }

        return closestPoint;
    }

    void SetIKWeight(float weight)
    {
        if (leftHandIK) leftHandIK.weight = weight;
        if (rightHandIK) rightHandIK.weight = weight;
    }

    void OnDrawGizmosSelected()
    {
        if (!isPushing) return;

        if (leftHandTarget && rightHandTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(leftHandTarget.position, 0.05f);
            Gizmos.DrawWireSphere(rightHandTarget.position, 0.05f);

            if (leftShoulderReference && rightShoulderReference)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(leftShoulderReference.position, leftHandTarget.position);
                Gizmos.DrawLine(rightShoulderReference.position, rightHandTarget.position);
            }
        }
    }
}