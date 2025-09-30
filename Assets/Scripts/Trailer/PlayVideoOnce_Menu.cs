using UnityEngine;
using UnityEngine.Video;

public class PlayVideoOnce_Menu : MonoBehaviour
{
    public GameObject[] disableGameobjects;
    private VideoPlayer videoPlayer;
    private const string VideoPlayedKey = "Video1Played";

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (PlayerPrefs.GetInt(VideoPlayedKey, 0) == 0)
        {

            foreach (var obj in disableGameobjects)
            {
                obj.SetActive(false);
            }

            videoPlayer.Play();

            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.Log("Video 1 already played. Disabling.");
            gameObject.SetActive(false);
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        PlayerPrefs.SetInt(VideoPlayedKey, 1);
        PlayerPrefs.Save();
        foreach (var obj in disableGameobjects)
        {
            obj.SetActive(true);
        }
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