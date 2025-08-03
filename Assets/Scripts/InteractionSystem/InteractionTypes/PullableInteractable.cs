using UnityEngine;

public abstract class PullableInteractable : BaseInteractable, IPullable
{
    [Header("Pull Settings")]
    [SerializeField] protected float pullSpeedMultiplier = 1f;
    [SerializeField] protected float maxPullDistance = 1.5f;
    [SerializeField] protected bool enableObstacleCheck = true;
    [SerializeField] protected LayerMask obstacleLayerMask = -1;
    [SerializeField] protected float obstacleCheckRadius = 0.3f;
    [SerializeField] protected bool canPullThroughSlopes = false;
    [SerializeField] protected float maxPullAngle = 45f;

    protected bool isBeingPulled = false;
    protected GameObject currentPuller;
    protected Vector3 lastValidPosition;
    protected Vector3 pullStartPosition;

    protected override void Awake()
    {
        base.Awake();
        lastValidPosition = transform.position;
    }

    public virtual bool CanPull(GameObject puller)
    {
        return isInteractable && !isBeingPulled;
    }

    public virtual void StartPull(GameObject puller)
    {
        isBeingPulled = true;
        currentPuller = puller;
        lastValidPosition = transform.position;
        pullStartPosition = puller.transform.position;

        interactionPrompt = "Press E to stop pulling";

        if (highlightObject != null)
            highlightObject.SetActive(true);

        PlayInteractionSound();
    }

    public virtual void UpdatePull(GameObject puller, Vector3 pullDirection, float playerSpeed)
    {
        if (!isBeingPulled || currentPuller != puller) return;

        float distanceToPuller = Vector3.Distance(transform.position, puller.transform.position);

        if (distanceToPuller > maxPullDistance)
        {
            StopPull(puller);
            return;
        }

        Vector3 directionToPlayer = (puller.transform.position - transform.position).normalized;


        float directionDot = Vector3.Dot(pullDirection.normalized, directionToPlayer);

        if (directionDot > 0.3f)
        {
            float moveSpeed = playerSpeed * pullSpeedMultiplier;
            Vector3 targetPosition = transform.position + pullDirection.normalized * moveSpeed * Time.deltaTime;

            bool canMove = true;

            if (enableObstacleCheck)
            {
                canMove = !Physics.CheckSphere(targetPosition, obstacleCheckRadius, obstacleLayerMask);

                if (canMove && !canPullThroughSlopes)
                {
                    if (Physics.Raycast(transform.position, pullDirection, out RaycastHit hit, 0.5f))
                    {
                        float angle = Vector3.Angle(Vector3.up, hit.normal);
                        if (angle > maxPullAngle)
                            canMove = false;
                    }
                }
            }

            if (canMove)
            {
                transform.position = targetPosition;
                lastValidPosition = transform.position;
            }
        }
    }

    public virtual void StopPull(GameObject puller)
    {
        if (currentPuller != puller) return;

        isBeingPulled = false;
        currentPuller = null;

        interactionPrompt = "Press E to pull";

        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    public override void Interact(GameObject interactor)
    {
        if (isBeingPulled)
        {
            StopPull(interactor);
        }
        else if (CanPull(interactor))
        {
            StartPull(interactor);
        }
    }
}