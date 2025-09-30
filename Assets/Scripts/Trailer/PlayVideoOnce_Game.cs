using UnityEngine;
using UnityEngine.Video;

public class PlayVideoOnce_Game : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private const string VideoPlayedKey = "Video2Played";

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (PlayerPrefs.GetInt(VideoPlayedKey, 0) == 0)
        {
            videoPlayer.Play();

            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.Log("Video 2 already played. Disabling.");
            gameObject.SetActive(false);
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        PlayerPrefs.SetInt(VideoPlayedKey, 1);
        PlayerPrefs.Save();

        gameObject.SetActive(false);
    }

    [ContextMenu("ResetVideo")]
    public void ResetVideoPlayStatus()
    {
        PlayerPrefs.DeleteKey(VideoPlayedKey);
        PlayerPrefs.Save();
        Debug.Log("Video 1 play status reset.");
    }
}