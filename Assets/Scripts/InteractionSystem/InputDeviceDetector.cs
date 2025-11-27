using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputDeviceDetector : MonoBehaviour
{
    public UnityEvent OnGamepadUsed;
    public UnityEvent OnKeyboardMouseUsed;

    private void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(UnityEngine.InputSystem.LowLevel.InputEventPtr eventPtr, InputDevice device)
    {
        if (device == null) return;

        if (device is Gamepad)
        {
            OnGamepadUsed?.Invoke();
        }

        else if (device is Keyboard || device is Mouse)
        {
            OnKeyboardMouseUsed?.Invoke();
        }
    }
}
