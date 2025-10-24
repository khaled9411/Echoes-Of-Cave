using StarterAssets;
using UnityEngine;

public class DisableOnEnable : MonoBehaviour
{
    private GameObject[] disableGameobjects = new GameObject[2];
    private void OnEnable()
    {
        disableGameobjects[0] = FindFirstObjectByType<StarterAssetsInputs>().gameObject;
        disableGameobjects[1] = GameObject.Find($"{LevelManager.Instance.GetCurrentLevelInfo().prefabName}(Clone)");
        foreach (var obj in disableGameobjects)
        {
            obj?.SetActive(false);
        }
    }
}
