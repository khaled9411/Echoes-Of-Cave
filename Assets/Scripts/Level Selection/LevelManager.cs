using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private GameObject currentLevelInstance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

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
        }
    }

    public void UnlockNextLevel(int completedLevel)
    {
        int currentUnlockedLevel = GetUnlockedLevel();

        if (completedLevel == currentUnlockedLevel && completedLevel < levelData.GetTotalLevels())
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, completedLevel + 1);
            PlayerPrefs.Save();
            Debug.Log($"Level {completedLevel + 1} unlocked!");
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= GetUnlockedLevel() && levelNumber >= 1;
    }

    // New method to check if this is the last level
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

        //SceneManager.LoadScene("Main");

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

        // Check if this is the last level
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

        //ReturnToMainMenu();
    }

    private void ShowCredits()
    {
        // Show credits using the CreditsManager
        if (CreditsManager.Instance != null)
        {
            CreditsManager.Instance.ShowCredits();
        }
        else
        {
            Debug.LogError("CreditsManager not found in scene!");
            // Fallback: return to main menu
            SceneManager.LoadScene(0);
        }
    }

    //public void ReturnToMainMenu()
    //{
    //    SceneManager.LoadSceneAsync("MainManu");
    //}

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
        Debug.Log("Progress reset!");
    }

    #endregion
}