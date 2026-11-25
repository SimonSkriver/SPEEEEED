using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CloseMenu : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) gameObject.SetActive(false);
        if (Keyboard.current.spaceKey.wasPressedThisFrame) gameObject.SetActive(false);
        if (Keyboard.current.enterKey.wasPressedThisFrame) gameObject.SetActive(false);
        if (Gamepad.current.buttonNorth.wasPressedThisFrame) gameObject.SetActive(false);
        if (Gamepad.current.buttonEast.wasPressedThisFrame) gameObject.SetActive(false);
        if (Gamepad.current.buttonSouth.wasPressedThisFrame) gameObject.SetActive(false);
        if (Gamepad.current.buttonWest.wasPressedThisFrame) gameObject.SetActive(false);
    }

    void OnDisable()
    {
        eventSystem.enabled = true;
    }

    void OnEnable()
    {
        eventSystem.enabled = false;
    }
}