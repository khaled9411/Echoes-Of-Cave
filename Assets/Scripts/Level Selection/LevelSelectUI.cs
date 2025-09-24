using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform levelButtonsContainer;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI levelDescriptionText;
    public Button playButton;

    [Header("Level Button Prefab")]
    public GameObject levelButtonPrefab;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlaySelectedLevel);
        }
    }

    private void SetupLevelButtons()
    {
        if (LevelManager.Instance.levelData == null) return;


        for (int i = 0; i < LevelManager.Instance.levelData.GetTotalLevels(); i++)
        {
            int levelNumber = i + 1;
            Transform buttonObj = levelButtonsContainer.GetChild(i);

            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + levelNumber.ToString();
            Button button = buttonObj.GetComponent<Button>();

            bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(levelNumber);
            button.interactable = isUnlocked;
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().color = isUnlocked ? Color.white : Color.gray;

            button.onClick.AddListener(() => SelectLevel(levelNumber));

            if (levelNumber == LevelManager.Instance.GetSelectedLevel())
            {
                SelectLevel(levelNumber);
            }
        }
    }

    private void CreateLevelButton(int levelNumber)
    {
        GameObject buttonObj;

        if (levelButtonPrefab != null)
        {
            buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + levelNumber.ToString();
        }
        else
        {
            buttonObj = new GameObject($"Level {levelNumber} Button");
            buttonObj.transform.SetParent(levelButtonsContainer);

            buttonObj.AddComponent<Button>();
            buttonObj.AddComponent<Image>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = levelNumber.ToString();
            text.alignment = TextAlignmentOptions.Center;

            var rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        Button button = buttonObj.GetComponent<Button>();

        bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(levelNumber);
        button.interactable = isUnlocked;

        //change color of locked buttons
        buttonObj.GetComponentInChildren<TextMeshProUGUI>().color = isUnlocked ? Color.white : Color.gray;
        //if (!isUnlocked)
        //{
        //    var image = buttonObj.GetComponent<Image>();
        //    if (image != null)
        //    {
        //        image.color = Color.gray;
        //    }
        //}

        button.onClick.AddListener(() => SelectLevel(levelNumber));

        if (levelNumber == LevelManager.Instance.GetSelectedLevel())
        {
            SelectLevel(levelNumber);
        }
    }

    private void SelectLevel(int levelNumber)
    {
        if (!LevelManager.Instance.IsLevelUnlocked(levelNumber)) return;

        LevelManager.Instance.SetSelectedLevel(levelNumber);
        UpdateLevelInfo();
        UpdateButtonSelection();
    }

    private void UpdateLevelInfo()
    {
        LevelInfo currentLevel = LevelManager.Instance.GetCurrentLevelInfo();

        if (currentLevel != null)
        {
            if (levelNumberText != null)
            {
                levelNumberText.text = $"Level {currentLevel.levelNumber}";
            }

            if (levelDescriptionText != null)
            {
                levelDescriptionText.text = currentLevel.description;
            }
        }
    }

    private void UpdateButtonSelection()
    {
        int selectedLevel = LevelManager.Instance.GetSelectedLevel();
        for (int i = 0; i < levelButtonsContainer.childCount; i++)
        {
            Transform buttonTransform = levelButtonsContainer.GetChild(i);
            Image highlight = buttonTransform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>();

            if (highlight != null)
            {
                int levelNumber = i + 1;

                if (highlight != null && LevelManager.Instance.IsLevelUnlocked(levelNumber))
                {
                    if (levelNumber == selectedLevel)
                    {
                        highlight.gameObject.SetActive(true);
                    }
                    else
                    {
                        highlight.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void PlaySelectedLevel()
    {
        LevelManager.Instance.LoadSelectedLevel();
    }

    private void OnEnable()
    {
        if (levelButtonsContainer != null)
        {
            SetupLevelButtons();
            UpdateLevelInfo();
        }
    }
}