using UnityEngine;
using UnityEngine.Video;

public class Win : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private void OnTriggerEnter(Collider other)
    {
        // Check if this is the player
        if (other.CompareTag("Player"))
        {
            videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.Play();

            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        WinLevel();
    }

    private void WinLevel()
    {
        int currentLevel = LevelManager.Instance.GetSelectedLevel();

        // Call OnLevelWin first - it handles last level check internally
        LevelManager.Instance.OnLevelWin();

        // Only continue to next level if it's not the last level
        if (!LevelManager.Instance.IsLastLevel(currentLevel))
        {
            LevelManager.Instance.LoadSelectedLevel();
            if (MainManu.Instance != null)
            {
                MainManu.Instance.StartGame(1);
            }
        }
        // If it's the last level, OnLevelWin() will handle showing credits
    }
}