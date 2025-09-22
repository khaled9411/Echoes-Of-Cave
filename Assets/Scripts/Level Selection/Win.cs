using UnityEngine;

public class Win : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        LevelManager.Instance.OnLevelWin();
        LevelManager.Instance.LoadSelectedLevel();
        MainManu.Instance.StartGame(1);
    }
}
