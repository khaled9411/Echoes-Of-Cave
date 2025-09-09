using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class AncientGateSystem : MonoBehaviour
{
    [Header("Gate Components")]
    public Transform gateMovingPart;
    public Transform closedPosition;
    public Transform openPosition;

    [Header("Animation Settings")]
    [Range(1f, 10f)]
    public float openDuration = 3f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool autoClose = false;
    [Range(1f, 30f)]
    public float autoCloseDelay = 5f;

    [Header("Camera Shake")]
    public CinemachineImpulseDefinition impulseDefinition;
    [Range(0.1f, 5f)]
    public float shakeIntensity = 1f;
    [Range(0.5f, 5f)]
    public float shakeDuration = 2f;

    [Header("Particle Effects")]
    public ParticleSystem dustEffect;
    public ParticleSystem debrisEffect;
    public ParticleSystem magicLightEffect;

    [Header("Audio")]
    public AudioClip gateStartSound;
    public AudioClip gateMovingSound;
    public AudioClip gateCompleteSound;
    public AudioSource audioSource;

    [Header("Visual Effects")]
    public Light gateLight;
    public Color lightColor = Color.yellow;
    public float maxLightIntensity = 3f;

    [Header("Status")]
    [SerializeField]
    private bool isOpen = false;
    [SerializeField]
    private bool isAnimating = false;

    private CinemachineImpulseSource impulseSource;
    private Vector3 initialGatePosition;
    private Coroutine currentAnimation;
    private Coroutine autoCloseCoroutine;

    public System.Action OnGateStartOpening;
    public System.Action OnGateFullyOpened;
    public System.Action OnGateStartClosing;
    public System.Action OnGateFullyClosed;

    void Start()
    {
        InitializeGate();
    }

    void InitializeGate()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        if (gateMovingPart != null)
        {
            initialGatePosition = gateMovingPart.position;

            if (closedPosition == null)
            {
                GameObject closedPosObj = new GameObject("ClosedPosition");
                closedPosObj.transform.SetParent(transform);
                closedPosObj.transform.position = initialGatePosition;
                closedPosition = closedPosObj.transform;
            }

            gateMovingPart.position = closedPosition.position;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (gateLight != null)
        {
            gateLight.color = lightColor;
            gateLight.intensity = 0f;
            gateLight.enabled = true;
        }

        StopAllEffects();

        Debug.Log($"Ancient Gate initialized: {gameObject.name}");
    }

    [ContextMenu("Open Gate")]
    public void OpenGate()
    {
        if (isAnimating || isOpen) return;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(OpenGateCoroutine());
    }

    public void CloseGate()
    {
        if (isAnimating || !isOpen) return;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        currentAnimation = StartCoroutine(CloseGateCoroutine());
    }

    public void ToggleGate()
    {
        if (isOpen)
        {
            CloseGate();
        }
        else
        {
            OpenGate();
        }
    }

    IEnumerator OpenGateCoroutine()
    {
        isAnimating = true;
        OnGateStartOpening?.Invoke();

        PlayOpeningEffects();
        StartCameraShake();

        float elapsedTime = 0f;
        Vector3 startPos = closedPosition.position;
        Vector3 targetPos = openPosition.position;

        while (elapsedTime < openDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / openDuration;
            float curveValue = movementCurve.Evaluate(normalizedTime);

            gateMovingPart.position = Vector3.Lerp(startPos, targetPos, curveValue);

            if (gateLight != null)
            {
                gateLight.intensity = Mathf.Lerp(0f, maxLightIntensity, curveValue);
            }

            yield return null;
        }

        gateMovingPart.position = targetPos;
        if (gateLight != null)
        {
            gateLight.intensity = maxLightIntensity;
        }

        if (gateCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gateCompleteSound);
        }

        if (dustEffect != null && dustEffect.isPlaying)
        {
            dustEffect.Stop();
        }

        isAnimating = false;
        isOpen = true;
        OnGateFullyOpened?.Invoke();

        if (autoClose)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseCountdown());
        }

        Debug.Log("Gate fully opened!");
    }

    IEnumerator CloseGateCoroutine()
    {
        isAnimating = true;
        OnGateStartClosing?.Invoke();

        PlayClosingEffects();

        float elapsedTime = 0f;
        Vector3 startPos = openPosition.position;
        Vector3 targetPos = closedPosition.position;

        while (elapsedTime < openDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / openDuration;
            float curveValue = movementCurve.Evaluate(normalizedTime);

            gateMovingPart.position = Vector3.Lerp(startPos, targetPos, curveValue);

            if (gateLight != null)
            {
                gateLight.intensity = Mathf.Lerp(maxLightIntensity, 0f, curveValue);
            }

            yield return null;
        }

        gateMovingPart.position = targetPos;
        if (gateLight != null)
        {
            gateLight.intensity = 0f;
        }

        StopAllEffects();

        isAnimating = false;
        isOpen = false;
        OnGateFullyClosed?.Invoke();

        Debug.Log("Gate fully closed!");
    }

    void PlayOpeningEffects()
    {
        if (gateStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gateStartSound);
        }

        if (gateMovingSound != null && audioSource != null)
        {
            audioSource.clip = gateMovingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (dustEffect != null)
        {
            dustEffect.Play();
        }

        if (debrisEffect != null)
        {
            debrisEffect.Play();
        }

        if (magicLightEffect != null)
        {
            magicLightEffect.Play();
        }
    }

    void PlayClosingEffects()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (dustEffect != null)
        {
            dustEffect.Play();
        }
    }

    void StartCameraShake()
    {
        if (impulseSource != null)
        {
            // Now you can directly use the impulseDefinition asset
            impulseSource.GenerateImpulse(shakeIntensity);

            Debug.Log("Camera shake activated!");
        }
    }

    void StopAllEffects()
    {
        if (dustEffect != null)
            dustEffect.Stop();

        if (debrisEffect != null)
            debrisEffect.Stop();

        if (magicLightEffect != null)
            magicLightEffect.Stop();

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    IEnumerator AutoCloseCountdown()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isOpen && !isAnimating)
        {
            CloseGate();
        }

        autoCloseCoroutine = null;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    public void SetOpenDuration(float duration)
    {
        openDuration = Mathf.Clamp(duration, 0.5f, 20f);
    }

    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = Mathf.Clamp(intensity, 0f, 10f);
    }

    void OnDrawGizmosSelected()
    {
        if (closedPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(closedPosition.position, 0.5f);
            Gizmos.DrawLine(transform.position, closedPosition.position);
        }

        if (openPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(openPosition.position, 0.5f);
            Gizmos.DrawLine(transform.position, openPosition.position);
        }

        if (closedPosition != null && openPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(closedPosition.position, openPosition.position);
        }
    }
}