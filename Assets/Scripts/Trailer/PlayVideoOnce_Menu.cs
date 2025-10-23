using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class PlayVideoOnce_Menu : MonoBehaviour
{
    public GameObject[] disableGameobjects;
    private VideoPlayer videoPlayer;
    private const string VideoPlayedKey = "Video1Played";
    private GameObject Level;
    IEnumerator Start()
    {

        yield return null; // Wait a frame to ensure all Start methods have run
        Level = GameObject.Find($"{LevelManager.Instance.GetCurrentLevelInfo().prefabName}(Clone)");
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null && videoPlayer.targetCamera == null)
        {
            videoPlayer.targetCamera = Camera.main;
        }

        if (PlayerPrefs.GetInt(VideoPlayedKey, 0) == 0)
        {
            if (Level != null)
            {
                Debug.Log("In Level Scene, disabling video player.");
                Level.SetActive(false);
            }

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
        if (Level != null)
        {
            Debug.Log("In Level Scene, disabling video player.");
            Level.SetActive(true);
        }
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