using UnityEngine;
using UnityEngine.SceneManagement;

public class Reload : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        LevelManager.Instance.LoadSelectedLevel();
    }
}
