using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Button selectButtonOnDisable;

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
        selectButtonOnDisable.Select();
    }
}