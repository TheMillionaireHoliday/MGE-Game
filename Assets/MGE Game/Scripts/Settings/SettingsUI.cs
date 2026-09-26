using UnityEngine;
using UnityEngine.UI; // or your input system of choice

public class SettingsUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button audioButton;
    [SerializeField] private Button videoButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button infoButton;

    [SerializeField] private AudioSettingsUI audioPanel;
    [SerializeField] private VideoSettingsUI videoPanel;
    [SerializeField] private ControlSettingsUI controlPanel;
    [SerializeField] private EmptySettingsUI infoPanel;

    [Header("Buttons")]
    [SerializeField] private Button okayButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] MenuPanelFader fader;
    public bool IsTransitioning => fader.IsTransitioning;

    private ISettingsPanel _activePanel;
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        audioButton.onClick.AddListener(OpenAudio);
        videoButton.onClick.AddListener(OpenVideo);
        controlsButton.onClick.AddListener(OpenControl);
        infoButton.onClick.AddListener(OpenInfo);

        okayButton.onClick.AddListener(Okay);
        applyButton.onClick.AddListener(Apply);
        cancelButton.onClick.AddListener(Cancel);

        gameObject.SetActive(false);
    }

    public void OpenAudio() => OpenPanel(audioPanel);
    public void OpenVideo() => OpenPanel(videoPanel);
    public void OpenControl() => OpenPanel(controlPanel);
    public void OpenInfo() => OpenPanel(infoPanel);

    private void OpenPanel(ISettingsPanel panel)
    {
        if (_activePanel != null) Detach(_activePanel);

        _activePanel = panel;
        Attach(_activePanel);

        gameObject.SetActive(true);         // show master window
        _activePanel.Open();
    }

    private void Attach(ISettingsPanel p)
    {
        // If your panel is a MonoBehaviour, activate its GO here:
        if (p is MonoBehaviour mb) mb.gameObject.SetActive(true);
    }

    private void Detach(ISettingsPanel p)
    {
        if (p is MonoBehaviour mb) mb.gameObject.SetActive(false);
    }

    public void Okay()
    {
        if (_activePanel == null) return;

        _activePanel.Apply();
        CloseWindow();
    }

    public void Apply()
    {
        if (_activePanel == null) return;

        _activePanel.Apply();
    }

    public void Cancel()
    {
        if (_activePanel == null) return;

        _activePanel.Cancel();
    }

    public void OpenWindow()
    {
        MenusManager.Instance.AddMenu(this);

        IsOpen = true;
        gameObject.SetActive(true);

        OpenPanel(audioPanel);
        fader.Show();
    }

    public void CloseWindow()
    {
        fader.Hide(this.NotifyClosed);
    }

    public void NotifyClosed()
    {
        if (_activePanel != null) Detach(_activePanel);
        _activePanel = null;

        IsOpen = false;
        MenusManager.Instance.RemoveMenu(this);
    }
}