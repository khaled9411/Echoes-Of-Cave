using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI qualityText;
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI frameRateText;
    public TextMeshProUGUI displayModeText;

    [Header("Buttons")]
    public Button applyButton;

    // ================== Data ================== //
    private string[] qualityLevels = { "Low", "Medium", "High" };
    private int qualityIndex = 2;

    private Resolution[] resolutions;
    private int resolutionIndex = 0;

    private int frameRate = 60;

    private string[] displayModes = { "Fullscreen", "Windowed", "Borderless" };
    private int displayModeIndex = 2;

    // ================== Unity ================== //
    void Start()
    {
        resolutions = new Resolution[]
        {
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1366, height = 768 },
            new Resolution { width = 1280, height = 720 }
        };

        LoadSettings();
        UpdateUI();

        applyButton.onClick.AddListener(ApplySettings);

        Cursor.lockState = CursorLockMode.None;
    }

    // ================== Quality ================== //
    public void ChangeQuality(int direction)
    {
        Debug.Log("Changing quality setting." + direction);
        qualityIndex += direction;
        if (qualityIndex < 0) qualityIndex = qualityLevels.Length - 1;
        if (qualityIndex >= qualityLevels.Length) qualityIndex = 0;
        UpdateUI();
    }

    // ================== Resolution ================== //
    public void ChangeResolution(int direction)
    {
        Debug.Log("Changing Resolution Setting " + direction);
        resolutionIndex += direction;
        if (resolutionIndex < 0) resolutionIndex = resolutions.Length - 1;
        if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
        UpdateUI();
    }

    // ================== Frame Rate ================== //
    public void ChangeFrameRate(int direction)
    {
        Debug.Log("Changing FrameRate Setting " + direction);
        frameRate += direction * 10;
        if (frameRate < 30) frameRate = 30;
        if (frameRate > 240) frameRate = 240;
        UpdateUI();
    }

    // ================== Display Mode ================== //
    public void ChangeDisplayMode(int direction)
    {
        Debug.Log("Changing DisplayMode Setting " + direction);
        displayModeIndex += direction;
        if (displayModeIndex < 0) displayModeIndex = displayModes.Length - 1;
        if (displayModeIndex >= displayModes.Length) displayModeIndex = 0;
        UpdateUI();
    }

    // ================== Apply ================== //
    public void ApplySettings()
    {
        // Quality
        QualitySettings.SetQualityLevel(qualityIndex);

        // Resolution + Display Mode
        FullScreenMode mode = FullScreenMode.Windowed;
        if (displayModes[displayModeIndex] == "Fullscreen") mode = FullScreenMode.ExclusiveFullScreen;
        else if (displayModes[displayModeIndex] == "Borderless") mode = FullScreenMode.FullScreenWindow;

        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, mode);

        // Frame Rate
        Application.targetFrameRate = frameRate;

        SaveSettings();
    }

    // ================== Save & Load ================== //
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
        PlayerPrefs.SetInt("FrameRate", frameRate);
        PlayerPrefs.SetInt("DisplayMode", displayModeIndex);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        qualityIndex = PlayerPrefs.GetInt("Quality", 2);
        resolutionIndex = PlayerPrefs.GetInt("Resolution", 0);
        frameRate = PlayerPrefs.GetInt("FrameRate", 60);
        displayModeIndex = PlayerPrefs.GetInt("DisplayMode", 2);

        ApplySettings();
    }

    // ================== Update UI ================== //
    private void UpdateUI()
    {
        qualityText.text = qualityLevels[qualityIndex];
        resolutionText.text = resolutions[resolutionIndex].width + " x " + resolutions[resolutionIndex].height;
        frameRateText.text = frameRate.ToString();
        displayModeText.text = displayModes[displayModeIndex];
    }
}
