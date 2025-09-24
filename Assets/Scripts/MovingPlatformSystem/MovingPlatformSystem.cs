using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatformSystem : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform platformTransform;
    public Transform startPoint;
    public Transform endPoint;

    [Header("Speed Settings")]
    public float maxSpeed = 3f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Operation Settings")]
    public bool startAutomatically = true;
    public bool isActive = false;

    [Header("Rope Settings")]
    public Transform ropeAnchorPoint;
    public Transform mainRopeTopPoint;
    public Material ropeMaterial;
    public float ropeThickness = 0.1f;
    public float mainRopeThickness = 0.15f;
    public int ropeSegments = 10;
    public int mainRopeSegments = 8;

    [Header("Sound Settings")]
    public AudioClip ropeMovementSound;
    public float soundVolume = 0.5f;

    [Header("Player Movement Settings")]
    public MovementMethod playerMovementMethod = MovementMethod.Parenting;

    public enum MovementMethod
    {
        Parenting,
        VelocityAddition,
        PositionOffset
    }

    // System variables
    private float journeyLength;
    private float journeyTime = 0f;
    private bool movingToEnd = true;
    private bool isMoving = false;

    // Rope components
    private List<LineRenderer> ropes = new List<LineRenderer>();
    private LineRenderer mainRope;
    private Vector3[] platformCorners = new Vector3[4];

    // Sound components
    public AudioSource audioSource;

    // Enhanced player variables
    private Vector3 lastPlatformPosition;
    private Vector3 platformVelocity;
    private Dictionary<Transform, PlayerPlatformData> playersOnPlatform = new Dictionary<Transform, PlayerPlatformData>();

    // Class to store player data
    [System.Serializable]
    public class PlayerPlatformData
    {
        public CharacterController controller;
        public Transform originalParent;
        public Vector3 localPosition;
        public bool wasGrounded;

        public PlayerPlatformData(Transform player)
        {
            controller = player.GetComponent<CharacterController>();
            originalParent = player.parent;
            wasGrounded = true;
        }
    }

    void Start()
    {
        InitializeSystem();

        if (startAutomatically)
        {
            StartPlatform();
        }
    }

    void InitializeSystem()
    {
        journeyLength = Vector3.Distance(startPoint.position, endPoint.position);
        platformTransform.position = startPoint.position;
        lastPlatformPosition = platformTransform.position;

        //audioSource = gameObject.AddComponent<AudioSource>();
        //audioSource.clip = ropeMovementSound;
        //audioSource.volume = soundVolume;
        //audioSource.loop = true;

        CreateRopes();
        CreateMainRope();
        UpdateRopes();
        UpdateMainRope();
    }

    void CreateMainRope()
    {
        GameObject mainRopeObj = new GameObject("MainRope");
        mainRopeObj.transform.parent = transform;

        mainRope = mainRopeObj.AddComponent<LineRenderer>();
        mainRope.material = ropeMaterial;
        mainRope.startWidth = mainRopeThickness;
        mainRope.endWidth = mainRopeThickness;
        mainRope.positionCount = mainRopeSegments;
        mainRope.useWorldSpace = true;
    }

    void CreateRopes()
    {
        CalculatePlatformCorners();

        for (int i = 0; i < 4; i++)
        {
            GameObject ropeObj = new GameObject($"Rope_{i}");
            ropeObj.transform.parent = transform;

            LineRenderer rope = ropeObj.AddComponent<LineRenderer>();
            rope.material = ropeMaterial;
            rope.startWidth = ropeThickness;
            rope.endWidth = ropeThickness;
            rope.positionCount = ropeSegments;
            rope.useWorldSpace = true;

            ropes.Add(rope);
        }
    }

    void CalculatePlatformCorners()
    {
        Bounds bounds = GetPlatformBounds();

        platformCorners[0] = new Vector3(bounds.min.x, bounds.center.y, bounds.min.z);
        platformCorners[1] = new Vector3(bounds.max.x, bounds.center.y, bounds.min.z);
        platformCorners[2] = new Vector3(bounds.max.x, bounds.center.y, bounds.max.z);
        platformCorners[3] = new Vector3(bounds.min.x, bounds.center.y, bounds.max.z);
    }

    Bounds GetPlatformBounds()
    {
        Renderer renderer = platformTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        return new Bounds(platformTransform.position, Vector3.one * 2);
    }

    void Update()
    {
        if (isActive && isMoving)
        {
            Vector3 oldPosition = platformTransform.position;

            MovePlatform();
            UpdateRopes();
            UpdateMainRope();

            platformVelocity = (platformTransform.position - oldPosition) / Time.deltaTime;

            MovePlayersWithPlatform();
        }
    }

    void MovePlatform()
    {
        float distanceCovered = journeyTime * GetCurrentSpeed();
        float fractionOfJourney = distanceCovered / journeyLength;

        Vector3 targetPosition;

        if (movingToEnd)
        {
            targetPosition = Vector3.Lerp(startPoint.position, endPoint.position, fractionOfJourney);
        }
        else
        {
            targetPosition = Vector3.Lerp(endPoint.position, startPoint.position, fractionOfJourney);
        }

        lastPlatformPosition = platformTransform.position;
        platformTransform.position = targetPosition;

        journeyTime += Time.deltaTime;

        if (fractionOfJourney >= 1f)
        {
            journeyTime = 0f;
            movingToEnd = !movingToEnd;

            if (movingToEnd)
            {
                platformTransform.position = startPoint.position;
            }
            else
            {
                platformTransform.position = endPoint.position;
            }
        }
    }

    float GetCurrentSpeed()
    {
        float progress = (journeyTime * maxSpeed) / journeyLength;
        return maxSpeed * speedCurve.Evaluate(progress);
    }

    void UpdateMainRope()
    {
        if (mainRope == null || ropeAnchorPoint == null || mainRopeTopPoint == null) return;

        Vector3[] mainRopePositions = new Vector3[mainRopeSegments];
        Vector3 bottomPoint = ropeAnchorPoint.position;
        Vector3 topPoint = mainRopeTopPoint.position;

        for (int i = 0; i < mainRopeSegments; i++)
        {
            float t = (float)i / (mainRopeSegments - 1);
            Vector3 straightLine = Vector3.Lerp(bottomPoint, topPoint, t);
            float sag = Mathf.Sin(t * Mathf.PI) * 0.2f;
            mainRopePositions[i] = straightLine + Vector3.down * sag;
        }

        mainRope.SetPositions(mainRopePositions);
    }

    void UpdateRopes()
    {
        if (ropeAnchorPoint == null) return;

        CalculatePlatformCorners();

        for (int i = 0; i < ropes.Count && i < platformCorners.Length; i++)
        {
            UpdateSingleRope(ropes[i], platformCorners[i]);
        }
    }

    void UpdateSingleRope(LineRenderer rope, Vector3 platformCorner)
    {
        Vector3[] ropePositions = new Vector3[ropeSegments];
        Vector3 topPoint = ropeAnchorPoint.position;
        Vector3 bottomPoint = platformCorner;

        for (int j = 0; j < ropeSegments; j++)
        {
            float t = (float)j / (ropeSegments - 1);
            Vector3 straightLine = Vector3.Lerp(topPoint, bottomPoint, t);
            float sag = Mathf.Sin(t * Mathf.PI) * 0.5f;
            ropePositions[j] = straightLine + Vector3.down * sag;
        }

        rope.SetPositions(ropePositions);
    }

    void MovePlayersWithPlatform()
    {
        foreach (var kvp in playersOnPlatform)
        {
            Transform player = kvp.Key;
            PlayerPlatformData data = kvp.Value;

            if (player == null) continue;

            switch (playerMovementMethod)
            {
                case MovementMethod.Parenting:
                    break;

                case MovementMethod.VelocityAddition:
                    if (data.controller != null)
                    {
                        Vector3 platformMovement = platformTransform.position - lastPlatformPosition;
                        data.controller.Move(platformMovement);
                    }
                    break;

                case MovementMethod.PositionOffset:
                    Vector3 movement = platformTransform.position - lastPlatformPosition;
                    player.position += movement;
                    break;
            }
        }
    }

    // Control methods
    public void StartPlatform()
    {
        isActive = true;
        isMoving = true;

        if (audioSource != null && ropeMovementSound != null)
        {
            audioSource.Play();
        }
    }

    public void StopPlatform()
    {
        isActive = false;
        isMoving = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void TogglePlatform()
    {
        if (isActive)
        {
            StopPlatform();
        }
        else
        {
            StartPlatform();
        }
    }

    // Player interaction
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Transform playerTransform = other.transform;

            if (other.transform.position.y >= platformTransform.position.y - 0.5f)
            {
                if (!playersOnPlatform.ContainsKey(playerTransform))
                {
                    PlayerPlatformData playerData = new PlayerPlatformData(playerTransform);
                    playersOnPlatform.Add(playerTransform, playerData);

                    if (playerMovementMethod == MovementMethod.Parenting)
                    {
                        playerTransform.SetParent(platformTransform);
                    }

                    Debug.Log("Player stepped on platform");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Transform playerTransform = other.transform;

            if (playersOnPlatform.ContainsKey(playerTransform))
            {
                PlayerPlatformData playerData = playersOnPlatform[playerTransform];

                if (playerMovementMethod == MovementMethod.Parenting)
                {
                    //playerTransform.SetParent(playerData.originalParent);
                    playerTransform.SetParent(null);
                }

                playersOnPlatform.Remove(playerTransform);
                Debug.Log("Player left platform");
            }
        }
    }

    // Helper methods
    public bool IsMoving() => isMoving;
    public bool IsActive() => isActive;
    public bool IsMovingToEnd() => movingToEnd;
    public float GetProgress() => (journeyTime * GetCurrentSpeed()) / journeyLength;
    public Vector3 GetPlatformVelocity() => platformVelocity;

    void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }

        if (ropeAnchorPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(ropeAnchorPoint.position, 0.3f);
        }

        if (mainRopeTopPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mainRopeTopPoint.position, 0.3f);

            if (ropeAnchorPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(ropeAnchorPoint.position, mainRopeTopPoint.position);
            }
        }
    }
}