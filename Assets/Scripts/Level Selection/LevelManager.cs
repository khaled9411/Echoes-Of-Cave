using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData levelData;

    public Action levelWinAction;

    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LevelManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("LevelManager");
                    _instance = go.AddComponent<LevelManager>();
                    DontDestroyOnLoad(go);
                }
                else
                {
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
            return _instance;
        }
    }

    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";
    private const string SELECTED_LEVEL_KEY = "SelectedLevel";
    private const string STEAM_CLOUD_FILENAME = "game_progress.dat";

    private GameObject currentLevelInstance;
    private bool useSteamCloud = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSaveSystem()
    {
        // Check if Steam is initialized and Cloud is enabled
        try
        {
            if (SteamClient.IsValid && SteamRemoteStorage.IsCloudEnabled)
            {
                useSteamCloud = true;
                Debug.Log("Steam Cloud enabled - using Steam Cloud for save data");
                LoadFromSteamCloud();
            }
            else
            {
                useSteamCloud = false;
                Debug.Log("Steam Cloud not available - using PlayerPrefs");
            }
        }
        catch (Exception e)
        {
            useSteamCloud = false;
            Debug.LogWarning($"Steam Cloud initialization failed: {e.Message}. Using PlayerPrefs.");
        }
    }

    #region Steam Cloud Methods

    private void LoadFromSteamCloud()
    {
        try
        {
            if (SteamRemoteStorage.FileExists(STEAM_CLOUD_FILENAME))
            {
                byte[] data = SteamRemoteStorage.FileRead(STEAM_CLOUD_FILENAME);
                if (data != null && data.Length > 0)
                {
                    string json = System.Text.Encoding.UTF8.GetString(data);
                    GameProgress progress = JsonUtility.FromJson<GameProgress>(json);

                    // Resolve conflicts - take the higher progress
                    int localUnlocked = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
                    int localSelected = PlayerPrefs.GetInt(SELECTED_LEVEL_KEY, 1);

                    int finalUnlocked = Mathf.Max(localUnlocked, progress.unlockedLevel);
                    int finalSelected = Mathf.Max(localSelected, progress.selectedLevel);

                    // Update PlayerPrefs with merged data
                    PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, finalUnlocked);
                    PlayerPrefs.SetInt(SELECTED_LEVEL_KEY, finalSelected);
                    PlayerPrefs.Save();

                    Debug.Log($"Steam Cloud data loaded and merged. Unlocked: {finalUnlocked}, Selected: {finalSelected}");
                }
            }
            else
            {
                Debug.Log("No Steam Cloud save file found. Starting fresh.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load from Steam Cloud: {e.Message}. Using local data.");
            useSteamCloud = false;
        }
    }

    private void SaveToSteamCloud()
    {
        if (!useSteamCloud)
            return;

        try
        {
            GameProgress progress = new GameProgress
            {
                unlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1),
                selectedLevel = PlayerPrefs.GetInt(SELECTED_LEVEL_KEY, 1)
            };

            string json = JsonUtility.ToJson(progress);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

            bool success = SteamRemoteStorage.FileWrite(STEAM_CLOUD_FILENAME, data);

            if (success)
            {
                Debug.Log("Progress saved to Steam Cloud successfully");
            }
            else
            {
                Debug.LogWarning("Failed to write to Steam Cloud. Data saved locally only.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to save to Steam Cloud: {e.Message}. Data saved locally only.");
        }
    }

    #endregion

    #region Level Progress Management

    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }

    public int GetSelectedLevel()
    {
        return PlayerPrefs.GetInt(SELECTED_LEVEL_KEY, 1);
    }

    public void SetSelectedLevel(int levelNumber)
    {
        if (IsLevelUnlocked(levelNumber))
        {
            PlayerPrefs.SetInt(SELECTED_LEVEL_KEY, levelNumber);
            PlayerPrefs.Save();
            SaveToSteamCloud();
        }
    }

    public void UnlockNextLevel(int completedLevel)
    {
        int currentUnlockedLevel = GetUnlockedLevel();

        if (completedLevel == currentUnlockedLevel && completedLevel < levelData.GetTotalLevels())
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, completedLevel + 1);
            PlayerPrefs.Save();
            SaveToSteamCloud();
            Debug.Log($"Level {completedLevel + 1} unlocked!");
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= GetUnlockedLevel() && levelNumber >= 1;
    }

    public bool IsLastLevel(int levelNumber)
    {
        return levelNumber >= levelData.GetTotalLevels();
    }

    #endregion

    #region Level Loading

    public void LoadSelectedLevel()
    {
        int selectedLevel = GetSelectedLevel();
        LoadLevel(selectedLevel);
    }

    public void LoadLevel(int levelNumber)
    {
        if (!IsLevelUnlocked(levelNumber))
        {
            Debug.LogWarning($"Level {levelNumber} is not unlocked yet!");
            return;
        }

        SetSelectedLevel(levelNumber);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main")
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            InstantiateCurrentLevel();
        }
    }

    private void InstantiateCurrentLevel()
    {
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        int selectedLevel = GetSelectedLevel();
        LevelInfo levelInfo = levelData.GetLevelInfo(selectedLevel);

        if (levelInfo != null)
        {
            GameObject levelPrefab = Resources.Load<GameObject>($"Levels/{levelInfo.prefabName}");

            if (levelPrefab != null)
            {
                currentLevelInstance = Instantiate(levelPrefab, new Vector3(0, 7, -20), Quaternion.identity);
                Debug.Log($"Level {selectedLevel} loaded successfully!");
            }
            else
            {
                Debug.LogError($"Level prefab not found: Levels/{levelInfo.prefabName}");
            }
        }
    }

    #endregion

    #region Win System

    [ContextMenu("Win Level")]
    public void OnLevelWin()
    {
        levelWinAction?.Invoke();
        int completedLevel = GetSelectedLevel();
        Debug.Log($"Level {completedLevel} completed!");

        if (IsLastLevel(completedLevel))
        {
            Debug.Log("Game completed! Showing credits...");
            ShowCredits();
            return;
        }

        UnlockNextLevel(completedLevel);

        int nextLevel = completedLevel + 1;
        if (IsLevelUnlocked(nextLevel))
        {
            SetSelectedLevel(nextLevel);
        }
    }

    private void ShowCredits()
    {
        if (CreditsManager.Instance != null)
        {
            CreditsManager.Instance.ShowCredits();
        }
        else
        {
            Debug.LogError("CreditsManager not found in scene!");
            SceneManager.LoadScene(0);
        }
    }

    #endregion

    #region Helper Methods

    public LevelInfo GetCurrentLevelInfo()
    {
        return levelData.GetLevelInfo(GetSelectedLevel());
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
        PlayerPrefs.DeleteKey(SELECTED_LEVEL_KEY);
        PlayerPrefs.Save();

        // Also delete from Steam Cloud
        if (useSteamCloud)
        {
            try
            {
                if (SteamRemoteStorage.FileExists(STEAM_CLOUD_FILENAME))
                {
                    SteamRemoteStorage.FileDelete(STEAM_CLOUD_FILENAME);
                    Debug.Log("Progress reset from Steam Cloud!");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to delete Steam Cloud save: {e.Message}");
            }
        }

        Debug.Log("Progress reset!");
    }

    // Method to manually sync with Steam Cloud
    [ContextMenu("Force Sync with Steam Cloud")]
    public void ForceSyncWithSteam()
    {
        if (useSteamCloud)
        {
            LoadFromSteamCloud();
            SaveToSteamCloud();
            Debug.Log("Manual sync with Steam Cloud completed");
        }
        else
        {
            Debug.Log("Steam Cloud not available");
        }
    }

    #endregion
}

[Serializable]
public class GameProgress
{
    public int unlockedLevel = 1;
    public int selectedLevel = 1;
}