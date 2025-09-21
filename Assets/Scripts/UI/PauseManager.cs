using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using StarterAssets;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas pauseMenuCanvas;
    [SerializeField] private Button resumeButton;
    [SerializeField] private StarterAssetsInputs starterInputs;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    // Private variables
    private bool isPaused = false;
    private bool wasGamePausedBefore = false;

    private void Start()
    {
        // Initialize
        InitializePauseMenu();

        // Save the initial time scale state
        wasGamePausedBefore = Time.timeScale == 0f;

        // Setup button events
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // Find StarterAssetsInputs if not assigned
        if (starterInputs == null)
        {
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();
        }
    }

    private void Update()
    {
        // Check for ESC input
        if (starterInputs != null && starterInputs.esc)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }

            // Reset the ESC input
            starterInputs.esc = false;
        }
    }

    private void InitializePauseMenu()
    {
        if (pauseMenuCanvas != null)
        {
            // Hide the pause menu initially
            pauseMenuCanvas.gameObject.SetActive(false);

            // Set initial scale for animation
            pauseMenuCanvas.transform.localScale = Vector3.zero;
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // Show pause menu
        ShowPauseMenu();

        // Pause the game
        Time.timeScale = 0f;

        // Show and unlock cursor
        SetCursorState(false);

        // Disable player input for look (to prevent camera movement in pause)
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // Hide pause menu with animation
        HidePauseMenu();

        // Resume the game (but only if it wasn't paused before)
        if (!wasGamePausedBefore)
        {
            Time.timeScale = 1f;
        }

        // Hide and lock cursor
        SetCursorState(true);

        // Enable player input for look
        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = true;
        }
    }

    private void ShowPauseMenu()
    {
        if (pauseMenuCanvas == null) return;

        // Activate the canvas
        pauseMenuCanvas.gameObject.SetActive(true);

        // Animate the menu appearance
        pauseMenuCanvas.transform.localScale = Vector3.zero;
        pauseMenuCanvas.transform.DOScale(Vector3.one, animationDuration)
            .SetEase(showEase)
            .SetUpdate(true); // Use unscaled time for animation during pause
    }

    private void HidePauseMenu()
    {
        if (pauseMenuCanvas == null) return;

        // Animate the menu disappearance
        pauseMenuCanvas.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(hideEase)
            .SetUpdate(true) // Use unscaled time for animation during pause
            .OnComplete(() => {
                pauseMenuCanvas.gameObject.SetActive(false);
            });
    }

    private void SetCursorState(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Update the StarterAssetsInputs cursor state
        if (starterInputs != null)
        {
            starterInputs.cursorLocked = locked;
        }
    }

    // Public methods for external use
    public bool IsPaused()
    {
        return isPaused;
    }

    public void ForceResume()
    {
        if (isPaused)
        {
            ResumeGame();
        }
    }

    public void ForcePause()
    {
        if (!isPaused)
        {
            PauseGame();
        }
    }

    // Handle scene changes and cleanup
    private void OnDestroy()
    {
        // Ensure time scale is reset when this object is destroyed
        if (isPaused && !wasGamePausedBefore)
        {
            Time.timeScale = 1f;
        }

        // Kill any running DOTween animations
        //pauseMenuCanvas?.transform.DOKill();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Handle application pause (useful for mobile builds)
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Handle application focus loss
        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
    }
}