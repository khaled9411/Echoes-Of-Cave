using UnityEngine;

public class TriggerTo : MonoBehaviour
{
    MovingPlatformSystem Controller;
    private void Start()
    {
        Controller = GetComponentInParent<MovingPlatformSystem>();
    }

    void OnTriggerExit(Collider other)
    {
        if (Controller != null)
        {
            Controller.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Controller != null)
        {
            Controller.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        }
    }
}
