using StarterAssets;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public int startCameraIndex = 0;
    public bool isInOpenArea = true;
    public CinemachineCamera[] openAreaCameras;
    public CinemachineCamera[] closedRoomCameras;

    [Header("Settings")]
    public float smoothTransitionTime = 1f;


    private CinemachineCamera currentCamera;
    private CinemachineBrain cinemachineBrain;

    public static CameraManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogError("Cinemachine Brain not found on main camera!");
        }
    }

    void Start()
    {
        InitializeCameras();
    }

    void InitializeCameras()
    {
        DeactivateAllCameras();

        if (openAreaCameras.Length > 0 && isInOpenArea)
        {
            SwitchToOpenAreaCamera(startCameraIndex, false);
        }
        else if (closedRoomCameras.Length > 0 && !isInOpenArea)
        {
            SwitchToClosedRoomCamera(startCameraIndex, false);
        }
    }

    void DeactivateAllCameras()
    {
        foreach (var cam in openAreaCameras)
        {
            if (cam != null)
            {
                cam.Priority = 0;
                cam.Follow = FindFirstObjectByType<ThirdPersonController>().transform;
            }
        }

        foreach (var cam in closedRoomCameras)
        {
            if (cam != null)
                cam.Priority = 0;
        }
    }

    public void SwitchToOpenAreaCamera(int cameraIndex, bool smoothTransition = true)
    {
        if (cameraIndex < 0 || cameraIndex >= openAreaCameras.Length)
        {
            Debug.LogWarning($"Camera index {cameraIndex} is invalid for open areas!");
            return;
        }

        SwitchCamera(openAreaCameras[cameraIndex], cameraIndex, true, smoothTransition);
    }

    public void SwitchToClosedRoomCamera(int cameraIndex, bool smoothTransition = true)
    {
        if (cameraIndex < 0 || cameraIndex >= closedRoomCameras.Length)
        {
            Debug.LogWarning($"Camera index {cameraIndex} is invalid for closed rooms!");
            return;
        }

        SwitchCamera(closedRoomCameras[cameraIndex], cameraIndex, false, smoothTransition);
    }

    void SwitchCamera(CinemachineCamera targetCamera, int cameraIndex, bool isOpenArea, bool smoothTransition)
    {
        if (targetCamera == null)
        {
            Debug.LogError("Target camera is null!");
            return;
        }

        // Set blend time directly on CinemachineBrain for smooth transitions
        if (cinemachineBrain != null)
        {
            cinemachineBrain.DefaultBlend.Time = smoothTransition ? smoothTransitionTime : 0f;
        }

        // Deactivate old camera
        if (currentCamera != null)
        {
            currentCamera.Priority = 0;
        }

        // Activate new camera
        targetCamera.Priority = 10;

        // Update state
        currentCamera = targetCamera;
        isInOpenArea = isOpenArea;

        Debug.Log($"Switched to {(isOpenArea ? "open area" : "closed room")} - Camera {cameraIndex}");
    }

    public CinemachineCamera GetCurrentCamera()
    {
        return currentCamera;
    }

    public bool IsInOpenArea()
    {
        return isInOpenArea;
    }
}