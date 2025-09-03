using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PushableBox : PushableInteractable
{
    [Header("Box Specific Settings")]
    [SerializeField] private AudioClip pushStartSound;
    [SerializeField] private AudioClip pushLoopSound;
    [SerializeField] private AudioClip pushStopSound;
    [SerializeField] private ParticleSystem dustParticles;

    private AudioSource pushAudioSource;
    private bool isPushSoundPlaying = false;

    protected override void Awake()
    {
        base.Awake();

        GameObject audioObj = new GameObject("PushAudioSource");
        audioObj.transform.SetParent(transform);
        audioObj.transform.localPosition = Vector3.zero;
        pushAudioSource = audioObj.AddComponent<AudioSource>();
        pushAudioSource.loop = true;
        pushAudioSource.volume = 0.5f;

        interactionPrompt = "Move towards to push";
    }

    public override void StartPush(GameObject pusher, Vector3 pushDirection)
    {
        Animator playerAnimator = pusher.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("StartPush");
        }
        base.StartPush(pusher, pushDirection);

        if (pushStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pushStartSound);
        }

        if (pushLoopSound != null && pushAudioSource != null)
        {
            pushAudioSource.clip = pushLoopSound;
            pushAudioSource.Play();
            isPushSoundPlaying = true;
        }

        if (dustParticles != null)
        {
            dustParticles.Play();
        }
    }

    public override void UpdatePush(GameObject pusher, Vector3 pushDirection, float playerSpeed)
    {
        Animator playerAnimator = pusher.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Pushing");
        }

        base.UpdatePush(pusher, pushDirection, playerSpeed);

        if (!isPushSoundPlaying && pushLoopSound != null && pushAudioSource != null)
        {
            pushAudioSource.clip = pushLoopSound;
            pushAudioSource.Play();
            isPushSoundPlaying = true;
        }
    }

    public override void StopPush(GameObject pusher)
    {
        Animator playerAnimator = pusher.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("StopPush");
        }

        base.StopPush(pusher);

        if (isPushSoundPlaying && pushAudioSource != null)
        {
            pushAudioSource.Stop();
            isPushSoundPlaying = false;
        }

        if (pushStopSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pushStopSound);
        }

        if (dustParticles != null)
        {
            dustParticles.Stop();
        }
    }
}