using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PushPullObject : BaseInteractable, IPushable, IPullable
{
    [Header("Push/Pull Settings")]
    [SerializeField] private float pushSpeed = 1f;
    [SerializeField] private float pullSpeed = 1f;
    [SerializeField] private float pushDetectionDistance = 0.3f;
    [SerializeField] private float maxPullDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayerMask = -1;

    [Header("Audio")]
    [SerializeField] private AudioClip pushSound;
    [SerializeField] private AudioClip pullSound;
    [SerializeField] private AudioClip stopSound;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private GameObject pushHighlight;
    [SerializeField] private GameObject pullHighlight;

    private bool isBeingPushed = false;
    private bool isBeingPulled = false;
    private GameObject currentPusher;
    private GameObject currentPuller;
    private Vector3 lastValidPosition;
    private AudioSource moveAudioSource;

    protected override void Awake()
    {
        base.Awake();
        lastValidPosition = transform.position;

        GameObject audioObj = new GameObject("MoveAudioSource");
        audioObj.transform.SetParent(transform);
        audioObj.transform.localPosition = Vector3.zero;
        moveAudioSource = audioObj.AddComponent<AudioSource>();
        moveAudioSource.loop = true;
        moveAudioSource.volume = 0.3f;

        interactionPrompt = "Press E to pull or move towards to push";
    }

    #region IPushable Implementation
    public bool CanPush(GameObject pusher, Vector3 pushDirection)
    {
        if (!isInteractable || isBeingPulled || isBeingPushed) return false;

        Vector3 checkPosition = transform.position + pushDirection.normalized * pushDetectionDistance;
        return !Physics.CheckSphere(checkPosition, 0.2f, obstacleLayerMask);
    }

    public void StartPush(GameObject pusher, Vector3 pushDirection)
    {
        isBeingPushed = true;
        currentPusher = pusher;
        lastValidPosition = transform.position;

        interactionPrompt = "Pushing...";

        if (pushHighlight != null)
            pushHighlight.SetActive(true);

        if (pushSound != null && moveAudioSource != null)
        {
            moveAudioSource.clip = pushSound;
            moveAudioSource.Play();
        }

        if (moveParticles != null)
            moveParticles.Play();

        PlayInteractionSound();
    }

    public void UpdatePush(GameObject pusher, Vector3 pushDirection, float pushForce)
    {
        if (!isBeingPushed || currentPusher != pusher) return;

        Vector3 targetPosition = transform.position + pushDirection.normalized * pushSpeed * Time.deltaTime;

        if (!Physics.CheckSphere(targetPosition, 0.2f, obstacleLayerMask))
        {
            transform.position = targetPosition;
            lastValidPosition = transform.position;
        }
        else
        {
            StopPush(pusher);
        }
    }

    public void StopPush(GameObject pusher)
    {
        if (currentPusher != pusher) return;

        isBeingPushed = false;
        currentPusher = null;

        interactionPrompt = "Press E to pull or move towards to push";

        if (pushHighlight != null)
            pushHighlight.SetActive(false);

        if (moveAudioSource != null && moveAudioSource.isPlaying)
            moveAudioSource.Stop();

        if (moveParticles != null)
            moveParticles.Stop();

        if (stopSound != null && audioSource != null)
            audioSource.PlayOneShot(stopSound);
    }
    #endregion

    #region IPullable Implementation
    public bool CanPull(GameObject puller)
    {
        return isInteractable && !isBeingPushed && !isBeingPulled;
    }

    public void StartPull(GameObject puller)
    {
        isBeingPulled = true;
        currentPuller = puller;
        lastValidPosition = transform.position;

        interactionPrompt = "Press E to stop pulling";

        if (pullHighlight != null)
            pullHighlight.SetActive(true);

        if (pullSound != null && moveAudioSource != null)
        {
            moveAudioSource.clip = pullSound;
            moveAudioSource.Play();
        }

        if (moveParticles != null)
            moveParticles.Play();

        PlayInteractionSound();
    }

    public void UpdatePull(GameObject puller, Vector3 pullDirection, float pullForce)
    {
        if (!isBeingPulled || currentPuller != puller) return;

        float distanceToPuller = Vector3.Distance(transform.position, puller.transform.position);

        if (distanceToPuller > maxPullDistance)
        {
            StopPull(puller);
            return;
        }

        Vector3 targetPosition = transform.position + pullDirection.normalized * pullSpeed * Time.deltaTime;

        if (!Physics.CheckSphere(targetPosition, 0.2f, obstacleLayerMask))
        {
            transform.position = targetPosition;
            lastValidPosition = transform.position;
        }
    }

    public void StopPull(GameObject puller)
    {
        if (currentPuller != puller) return;

        isBeingPulled = false;
        currentPuller = null;

        interactionPrompt = "Press E to pull or move towards to push";

        if (pullHighlight != null)
            pullHighlight.SetActive(false);

        if (moveAudioSource != null && moveAudioSource.isPlaying)
            moveAudioSource.Stop();

        if (moveParticles != null)
            moveParticles.Stop();

        if (stopSound != null && audioSource != null)
            audioSource.PlayOneShot(stopSound);
    }
    #endregion

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

    public override void ShowHighlight()
    {
        base.ShowHighlight();

        if (!isBeingPushed && !isBeingPulled)
        {
            if (pullHighlight != null)
                pullHighlight.SetActive(true);
        }
    }

    public override void HideHighlight()
    {
        base.HideHighlight();

        if (!isBeingPushed && pullHighlight != null)
            pullHighlight.SetActive(false);

        if (!isBeingPulled && pushHighlight != null)
            pushHighlight.SetActive(false);
    }

    public Vector3 GetCurrentVelocity()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetAccumulatedMovement()
    {
        throw new System.NotImplementedException();
    }
}