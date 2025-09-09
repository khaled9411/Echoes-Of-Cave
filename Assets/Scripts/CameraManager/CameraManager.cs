using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera[] openAreaCameras;
    public CinemachineCamera[] closedRoomCameras;

    [Header("Settings")]
    public float smoothTransitionTime = 1f;

    [Header("Debug")]
    public int currentCameraIndex = 0;
    public bool isInOpenArea = true;

    private CinemachineCamera currentCamera;
    private CinemachineBrain cinemachineBrain;

    public static CameraManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        if (openAreaCameras.Length > 0)
        {
            SwitchToOpenAreaCamera(0, false);
        }
    }

    void DeactivateAllCameras()
    {
        foreach (var cam in openAreaCameras)
        {
            if (cam != null)
                cam.Priority = 0;
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
        currentCameraIndex = cameraIndex;
        isInOpenArea = isOpenArea;

        Debug.Log($"Switched to {(isOpenArea ? "open area" : "closed room")} - Camera {cameraIndex}");
    }

    public void SwitchToNextCamera(bool smoothTransition = true)
    {
        if (isInOpenArea)
        {
            int nextIndex = (currentCameraIndex + 1) % openAreaCameras.Length;
            SwitchToOpenAreaCamera(nextIndex, smoothTransition);
        }
        else
        {
            int nextIndex = (currentCameraIndex + 1) % closedRoomCameras.Length;
            SwitchToClosedRoomCamera(nextIndex, smoothTransition);
        }
    }

    public void SwitchToPreviousCamera(bool smoothTransition = true)
    {
        if (isInOpenArea)
        {
            int prevIndex = currentCameraIndex - 1;
            if (prevIndex < 0) prevIndex = openAreaCameras.Length - 1;
            SwitchToOpenAreaCamera(prevIndex, smoothTransition);
        }
        else
        {
            int prevIndex = currentCameraIndex - 1;
            if (prevIndex < 0) prevIndex = closedRoomCameras.Length - 1;
            SwitchToClosedRoomCamera(prevIndex, smoothTransition);
        }
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