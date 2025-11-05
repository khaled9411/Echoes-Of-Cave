using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video; 

public class CreditsManager : MonoBehaviour
{
    [Header("Credits UI")]
    public GameObject creditsCanvas;
    public Image backgroundImage;
    public Image logoImage;
    public TextMeshProUGUI theEndText;
    public TextMeshProUGUI thanksText;

    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject videoDisplayObject;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float displayDuration = 2f;
    public float moveUpDuration = 2f;

    private static CreditsManager _instance;
    public static CreditsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CreditsManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        InitializeCredits();
    }

    private void InitializeCredits()
    {
        if (creditsCanvas != null)
        {
            creditsCanvas.SetActive(false);
        }

      
        if (videoDisplayObject != null)
        {
            videoDisplayObject.SetActive(false);
        }

        // Set initial alpha values
        if (backgroundImage != null)
            backgroundImage.color = new Color(0, 0, 0, 0);
        if (logoImage != null)
            logoImage.color = new Color(1, 1, 1, 0);
        if (theEndText != null)
            theEndText.color = new Color(1, 1, 1, 0);
        if (thanksText != null)
            thanksText.color = new Color(1, 1, 1, 0);
    }

    public void ShowCredits()
    {
        
        if (creditsCanvas == null || videoPlayer == null || videoDisplayObject == null)
        {
            Debug.LogError("Credits Canvas, VideoPlayer, or VideoDisplayObject is not assigned!");

           
            if (videoPlayer == null || videoDisplayObject == null)
            {
                Debug.LogWarning("Video components not set. Skipping video and showing credits.");
                creditsCanvas.SetActive(true);
                StartCreditsSequence();
            }
            return;
        }

        videoDisplayObject.SetActive(true);

       
        backgroundImage.color = new Color(0, 0, 0, 0);
        logoImage.color = new Color(1, 1, 1, 0);
        theEndText.color = new Color(1, 1, 1, 0);
        thanksText.color = new Color(1, 1, 1, 0);

        
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {

        vp.loopPointReached -= OnVideoFinished;


        if (videoDisplayObject != null)
        {
            videoDisplayObject.SetActive(false);
        }

        creditsCanvas.SetActive(true);

        StartCreditsSequence();
    }



    private void StartCreditsSequence()
    {
        Sequence creditsSequence = DOTween.Sequence();
        // Step 1: Fade in black background
        creditsSequence.Append(backgroundImage.DOFade(1f, fadeDuration));
        // Step 2: Show logo with fade in
        creditsSequence.Append(logoImage.DOFade(1f, fadeDuration));
        creditsSequence.AppendInterval(displayDuration);
        // Step 3: Show "The End" text
        creditsSequence.Append(theEndText.DOFade(1f, fadeDuration));
        creditsSequence.AppendInterval(displayDuration);
        // Step 4: Show "Thanks for Playing" text
        creditsSequence.Append(thanksText.DOFade(1f, fadeDuration));
        creditsSequence.AppendInterval(displayDuration);
        // Step 5: Move everything up and fade out
        creditsSequence.Append(MoveAllElementsUp());
        // Step 6: Return to main menu
        creditsSequence.AppendCallback(() => ReturnToMainMenu());
        creditsSequence.Play();
    }

    private Tween MoveAllElementsUp()
    {
        Sequence moveSequence = DOTween.Sequence();
        // Move all elements up
        if (logoImage != null)
            moveSequence.Join(logoImage.transform.DOMoveY(logoImage.transform.position.y + 1000, moveUpDuration));
        if (theEndText != null)
            moveSequence.Join(theEndText.transform.DOMoveY(theEndText.transform.position.y + 1000, moveUpDuration));
        if (thanksText != null)
            moveSequence.Join(thanksText.transform.DOMoveY(thanksText.transform.position.y + 1000, moveUpDuration));
        // Fade out background at the end
        moveSequence.Append(backgroundImage.DOFade(0f, fadeDuration));
        return moveSequence;
    }

    private void ReturnToMainMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0); // Load main menu scene (index 0)
    }

    [ContextMenu("Test Credits")]
    public void TestCredits()
    {

        if (!Application.isPlaying)
        {
            Debug.LogWarning("Video playback testing only works in Play Mode.");
            return;
        }
        ShowCredits();
    }
}