using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PullableCrate : PullableInteractable
{
    [Header("Crate Specific Settings")]
    [SerializeField] private AudioClip pullStartSound;
    [SerializeField] private AudioClip pullLoopSound;
    [SerializeField] private AudioClip pullStopSound;
    [SerializeField] private ParticleSystem scrapeParticles;
    [SerializeField] private LineRenderer ropeRenderer;

    private AudioSource pullAudioSource;
    private bool isPullSoundPlaying = false;

    protected override void Awake()
    {
        base.Awake();

        GameObject audioObj = new GameObject("PullAudioSource");
        audioObj.transform.SetParent(transform);
        audioObj.transform.localPosition = Vector3.zero;
        pullAudioSource = audioObj.AddComponent<AudioSource>();
        pullAudioSource.loop = true;
        pullAudioSource.volume = 0.5f;

        interactionPrompt = "Press E to pull";

        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
            ropeRenderer.startWidth = 0.05f;
            ropeRenderer.endWidth = 0.05f;
            ropeRenderer.positionCount = 2;
            ropeRenderer.useWorldSpace = true;

            if (ropeRenderer.material == null)
            {
                Material ropeMaterial = new Material(Shader.Find("Sprites/Default"));
                ropeMaterial.color = new Color(0.6f, 0.4f, 0.2f);
                ropeRenderer.material = ropeMaterial;
            }
        }
    }

    public override void StartPull(GameObject puller)
    {
        base.StartPull(puller);

        if (pullStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pullStartSound);
        }

        if (pullLoopSound != null && pullAudioSource != null)
        {
            pullAudioSource.clip = pullLoopSound;
            pullAudioSource.Play();
            isPullSoundPlaying = true;
        }

        if (scrapeParticles != null)
        {
            scrapeParticles.Play();
        }

        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = true;
        }
    }

    public override void UpdatePull(GameObject puller, Vector3 pullDirection, float playerSpeed)
    {
        base.UpdatePull(puller, pullDirection, playerSpeed);

        if (ropeRenderer != null && currentPuller != null)
        {
            Vector3 cratePosition = transform.position + Vector3.up * 0.5f;
            Vector3 playerPosition = currentPuller.transform.position + Vector3.up * 1f;

            ropeRenderer.SetPosition(0, cratePosition);
            ropeRenderer.SetPosition(1, playerPosition);
        }

        if (!isPullSoundPlaying && pullLoopSound != null && pullAudioSource != null)
        {
            pullAudioSource.clip = pullLoopSound;
            pullAudioSource.Play();
            isPullSoundPlaying = true;
        }
    }

    public override void StopPull(GameObject puller)
    {
        base.StopPull(puller);

        if (isPullSoundPlaying && pullAudioSource != null)
        {
            pullAudioSource.Stop();
            isPullSoundPlaying = false;
        }

        if (pullStopSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pullStopSound);
        }

        if (scrapeParticles != null)
        {
            scrapeParticles.Stop();
        }

        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
        }
    }
}