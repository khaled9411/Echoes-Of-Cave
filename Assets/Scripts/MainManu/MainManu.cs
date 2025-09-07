using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainManu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //On PlayButton
    public void StartGame()
    {
        StartCoroutine(TransactionEffect());
    }

    IEnumerator TransactionEffect()
    {
        GetComponent<Animator>().SetTrigger("End");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync("Main");
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
