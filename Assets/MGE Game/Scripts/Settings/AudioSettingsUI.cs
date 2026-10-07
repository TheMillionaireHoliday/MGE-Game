using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour, ISettingsPanel
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // The working copy — edits live here until Apply/Cancel
    private GameSettings _draft;
    private bool _wired;

    private void Awake()
    {
        if (_wired) return;
        _wired = true;

        masterSlider.minValue = 0f; masterSlider.maxValue = 1f;
        musicSlider.minValue = 0f; musicSlider.maxValue = 1f;
        sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f;

        masterSlider.onValueChanged.AddListener(v => { _draft.masterVolume = v; });
        musicSlider.onValueChanged.AddListener(v => { _draft.musicVolume = v; });
        sfxSlider.onValueChanged.AddListener(v => { _draft.sfxVolume = v; });
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
        mgr.SetMasterVolume(_draft.masterVolume);
        mgr.SetMusicVolume(_draft.musicVolume);
        mgr.SetSfxVolume(_draft.sfxVolume);
        mgr.SaveAndApply();
    }

    public void Cancel()
    {
        LoadIntoUI(SettingsManager.Instance.Current);
    }


    private void LoadIntoUI(GameSettings s)
    {
        _draft = new GameSettings(s);

        masterSlider.SetValueWithoutNotify(s.masterVolume);
        musicSlider.SetValueWithoutNotify(s.musicVolume);
        sfxSlider.SetValueWithoutNotify(s.sfxVolume);
    }
}