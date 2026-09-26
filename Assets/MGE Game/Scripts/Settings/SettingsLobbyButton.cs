using UnityEngine;
using UnityEngine.UI;

public class SettingsLobbyButton : MonoBehaviour
{
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private Button settingsButton;

    private void Awake()
    {
        settingsButton.onClick.AddListener(OnButtonPressed);
    }

    public void OnButtonPressed()
    {
        if (settingsUI.IsOpen)
            settingsUI.CloseWindow();
        else
            settingsUI.OpenWindow();
    }
}
