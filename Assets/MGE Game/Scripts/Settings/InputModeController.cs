using System;
using UnityEngine;

public enum InputMode { FirstPersonController, Menu }

public class InputModeController : MonoBehaviour
{
    public static InputModeController Instance { get; private set; }

    public static event Action<InputMode> ModeChanged;

    private GameInput gameInput;
    public InputMode CurrentInputMode { get; private set; } = InputMode.FirstPersonController;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        gameInput = new GameInput();
        SetMode(InputMode.FirstPersonController);
    }

    public void SetMode(InputMode mode)
    {
        if (CurrentInputMode == mode && gameInput != null) return;

        gameInput = GameInputHolder.Input;

        gameInput.FirstPersonController.Disable();
        gameInput.UI.Disable();
        gameInput.Menu.Disable();

        switch (mode)
        {
            case InputMode.FirstPersonController:
                gameInput.FirstPersonController.Enable();
                gameInput.Menu.Enable();
                Cursor.lockState = CursorLockMode.Locked;

                break;

            case InputMode.Menu:
                gameInput.UI.Enable();
                gameInput.Menu.Enable();
                Cursor.lockState = CursorLockMode.None;

                break;
        }

        CurrentInputMode = mode;
        ModeChanged?.Invoke(mode);
    }

    private void OnDestroy()
    {
        gameInput?.Dispose();
    }
}