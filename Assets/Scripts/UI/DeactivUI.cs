using UnityEngine;
using System.Collections;

public class DeactivUI : MonoBehaviour
{
    private void OnEnable()
    {
        StopAllCoroutines();
        GetComponent<Animator>().SetBool("Active", true);
        StartCoroutine(DisableGameObject());
    }

    private IEnumerator DisableGameObject()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<Animator>().SetBool("Active", false);
    }
}
