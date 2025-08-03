using UnityEngine;

public abstract class PushableInteractable : BaseInteractable, IPushable
{
    [Header("Push Settings")]
    [SerializeField] protected float pushSpeedMultiplier = 1f;
    [SerializeField] protected float pushDetectionDistance = 0.3f;
    [SerializeField] protected bool enableObstacleCheck = true;
    [SerializeField] protected LayerMask obstacleLayerMask = -1;
    [SerializeField] protected float obstacleCheckRadius = 0.3f;
    [SerializeField] protected bool canPushThroughSlopes = false;
    [SerializeField] protected float maxPushAngle = 45f;

    [Header("Frame Rate Independence")]
    [SerializeField] protected float minMoveThreshold = 0.001f;
    [SerializeField] protected float maxMoveStep = 0.1f;
    [SerializeField] protected float smoothingFactor = 5f;

    protected bool isBeingPushed = false;
    protected GameObject currentPusher;
    protected Vector3 lastValidPosition;

    protected Vector3 accumulatedMovement = Vector3.zero;
    protected Vector3 lastFramePosition;
    protected float lastUpdateTime;
    protected Vector3 targetVelocity = Vector3.zero;
    protected Vector3 currentVelocity = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        lastValidPosition = transform.position;
        lastFramePosition = transform.position;
        lastUpdateTime = Time.time;
    }

    public virtual bool CanPush(GameObject pusher, Vector3 pushDirection)
    {
        if (!isInteractable || isBeingPushed) return false;

        if (enableObstacleCheck)
        {
            Vector3 checkPosition = transform.position + pushDirection.normalized * pushDetectionDistance;

            if (Physics.CheckSphere(checkPosition, obstacleCheckRadius, obstacleLayerMask))
                return false;
        }

        if (!canPushThroughSlopes)
        {
            if (Physics.Raycast(transform.position, pushDirection, out RaycastHit hit, pushDetectionDistance * 2))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle > maxPushAngle)
                    return false;
            }
        }

        return true;
    }

    public virtual void StartPush(GameObject pusher, Vector3 pushDirection)
    {
        isBeingPushed = true;
        currentPusher = pusher;
        lastValidPosition = transform.position;
        lastFramePosition = transform.position;
        lastUpdateTime = Time.time;

        accumulatedMovement = Vector3.zero;
        targetVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;

        if (highlightObject != null)
            highlightObject.SetActive(true);

        PlayInteractionSound();
    }

    public virtual void UpdatePush(GameObject pusher, Vector3 pushDirection, float playerSpeed)
    {
        if (!isBeingPushed || currentPusher != pusher) return;

        float currentTime = Time.time;
        float deltaTime = currentTime - lastUpdateTime;

        deltaTime = Mathf.Clamp(deltaTime, 0.001f, 0.05f);

        float moveSpeed = playerSpeed * pushSpeedMultiplier;

        targetVelocity = pushDirection.normalized * moveSpeed;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smoothingFactor * deltaTime);

        Vector3 desiredMovement = currentVelocity * deltaTime;

        accumulatedMovement += desiredMovement;

        if (accumulatedMovement.magnitude >= minMoveThreshold)
        {
            Vector3 actualMovement = Vector3.ClampMagnitude(accumulatedMovement, maxMoveStep);
            Vector3 targetPosition = transform.position + actualMovement;

            bool canMove = true;

            if (enableObstacleCheck)
            {
                canMove = !Physics.CheckSphere(targetPosition, obstacleCheckRadius, obstacleLayerMask);
            }

            if (canMove)
            {
                transform.position = targetPosition;
                lastValidPosition = transform.position;

                accumulatedMovement -= actualMovement;
            }
            else if (enableObstacleCheck)
            {
                accumulatedMovement = Vector3.zero;
                currentVelocity = Vector3.zero;
                StopPush(pusher);
                return;
            }
        }

        lastFramePosition = transform.position;
        lastUpdateTime = currentTime;
    }

    public virtual void StopPush(GameObject pusher)
    {
        if (currentPusher != pusher) return;

        isBeingPushed = false;
        currentPusher = null;

        accumulatedMovement = Vector3.zero;
        targetVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;

        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    public override void Interact(GameObject interactor)
    {
        // Implementation specific to subclasses
    }

    protected virtual void FixedUpdate()
    {
        if (!isBeingPushed)
        {
            if (currentVelocity.magnitude > 0.01f)
            {
                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, smoothingFactor * Time.fixedDeltaTime);
                accumulatedMovement = Vector3.zero;
            }
        }
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public Vector3 GetAccumulatedMovement()
    {
        return accumulatedMovement;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && isBeingPushed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, currentVelocity);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, accumulatedMovement * 10f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, obstacleCheckRadius);
    }
}