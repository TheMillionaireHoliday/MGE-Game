using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSettingsUI : MonoBehaviour, ISettingsPanel
{
    [Header("Sliders")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;

    // The working copy — edits live here until Apply/Cancel
    private GameSettings _draft;
    private bool _wired;

    private void Awake()
    {
        if (_wired) return;
        _wired = true;

        sensitivitySlider.minValue = 0.1f; sensitivitySlider.maxValue = 6f;

        sensitivitySlider.onValueChanged.AddListener(v => { _draft.mouseSensitivity = v; sensitivityText.text = _draft.mouseSensitivity.ToString("0.0"); });
    }

    // Called when the panel opens — snapshot the persisted settings into a draft
    public void Open()
    {
        LoadIntoUI(SettingsManager.Instance.Current);
    }

    public void Apply()
    {
        print("Applied audio");

        var mgr = SettingsManager.Instance;
        mgr.SetMouseSensitivity(_draft.mouseSensitivity);
        mgr.SaveAndApply();
    }

    public void Cancel()
    {
        LoadIntoUI(SettingsManager.Instance.Current);
    }


    private void LoadIntoUI(GameSettings s)
    {
        _draft = new GameSettings(s);

        sensitivitySlider.SetValueWithoutNotify(s.mouseSensitivity);
        sensitivityText.text = s.mouseSensitivity.ToString("0.0");
    }
}
