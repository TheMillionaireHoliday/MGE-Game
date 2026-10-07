using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsListener : MonoBehaviour
{
    [SerializeField] private SettingsUI settingsUI;
    private void OnEnable()
    {
        GameInputHolder.Input.Menu.EscapeButton.performed += OnEscapePressed;
    }

    private void OnDisable()
    {
        if (GameInputHolder.Input != null)
            GameInputHolder.Input.Menu.EscapeButton.performed -= OnEscapePressed;
    }

    private void OnEscapePressed(InputAction.CallbackContext _)
    {
        if (settingsUI.IsOpen)
            settingsUI.CloseWindow();
        else
            settingsUI.OpenWindow();
    }
}
