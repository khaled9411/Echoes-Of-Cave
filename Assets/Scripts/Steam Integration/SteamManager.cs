using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    private const uint AppId = 480;

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSteamworks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSteamworks()
    {
        try
        {
            SteamClient.Init(AppId);
            isInitialized = true;
            Debug.Log("Steamworks Initialized Successfully!");

            SteamStatsAndAchievements.Initialize();
        }
        catch (System.Exception e)
        {
            isInitialized = false;
            Debug.LogError($"Steamworks failed to initialize! Error: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        ShutdownSteamworks();
    }

    private void ShutdownSteamworks()
    {
        if (isInitialized)
        {
            Steamworks.SteamUserStats.StoreStats();

            SteamClient.Shutdown();
            isInitialized = false;
            Debug.Log("Steamworks Shutdown Complete.");
        }
    }

    private void Update()
    {
        if (isInitialized)
        {
            SteamClient.RunCallbacks();
        }
    }

    public ulong GetClientSteamID()
    {
        return isInitialized ? SteamClient.SteamId : 0;
    }
}