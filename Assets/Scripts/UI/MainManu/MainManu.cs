using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainManu : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;

        if (SceneManager.GetActiveScene().buildIndex == 0)
            GetComponent<Animator>().SetTrigger("Start");
    }
    //On PlayButton
    public void StartGame(int scene)
    {
        StartCoroutine(TransactionEffect(scene));
    }

    IEnumerator TransactionEffect(int scene)
    {
        GetComponent<Animator>().SetTrigger("End");
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadSceneAsync(scene);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
