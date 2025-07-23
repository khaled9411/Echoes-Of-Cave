using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUIManager : MonoBehaviour
{
    public static InteractionUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Slider progressSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowPrompt(string prompt)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            if (promptText != null)
                promptText.text = prompt;
        }
    }

    public void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(progress > 0 && progress < 1);
            progressSlider.value = progress;
        }
    }
}
