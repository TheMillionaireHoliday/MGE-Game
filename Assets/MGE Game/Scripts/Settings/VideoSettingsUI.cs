using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsUI : MonoBehaviour, ISettingsPanel
{
    [Header("FOV")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TextMeshProUGUI fovSliderText;
    [SerializeField] private int minFov = 70;
    [SerializeField] private int maxFov = 120;

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("VSync")]
    [SerializeField] private Toggle vSyncToggle;

    private GameSettings _draft;
    private bool _wired;
    private bool _suppressCallbacks;

    // Maps dropdown option index -> raw Screen.resolutions index.
    // Rebuilt every time the dropdown is rebuilt.
    private readonly List<int> _dropdownToRaw = new();

    public bool IsDirty { get; set; }

    private void Awake()
    {
        if (_wired) return;
        _wired = true;

        fovSlider.minValue = minFov;
        fovSlider.maxValue = maxFov;

        fovSlider.onValueChanged.AddListener(OnFovChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    // ---- ISettingsPanel ----

    public void Open()
    {
        BuildResolutionDropdown();
        LoadIntoUI(SettingsManager.Instance.Current);
    }

    public void Apply()
    {
        IsDirty = false;

        var mgr = SettingsManager.Instance;
        mgr.SetCameraFov(_draft.cameraFov);
        mgr.SetResolutionIndex(_draft.resolutionIndex);  // raw index — see OnResolutionChanged
        mgr.SetFullscreen(_draft.fullscreen);
        mgr.SetVSync(_draft.vSync);
        mgr.SaveAndApply();
    }

    public void Cancel()
    {
        LoadIntoUI(SettingsManager.Instance.Current);
    }

    // ---- Resolution dropdown ----

    private void BuildResolutionDropdown()
    {
        var res = Screen.resolutions;
        _dropdownToRaw.Clear();

        _suppressCallbacks = true;

        // Deduplicate by (width, height): keep the highest refresh rate
        // for each size, and remember the raw index for that entry.
        var bestBySize = new Dictionary<(int w, int h), int>();   // -> raw index

        for (int i = 0; i < res.Length; i++)
        {
            var r = res[i];
            var key = (r.width, r.height);

            if (!bestBySize.TryGetValue(key, out int existingRaw))
            {
                bestBySize[key] = i;
                continue;
            }

            var existing = res[existingRaw];
            if (r.refreshRateRatio.value > existing.refreshRateRatio.value)
                bestBySize[key] = i;
        }

        // Sort: highest pixel count first, then highest refresh rate.
        var rawIndices = new List<int>(bestBySize.Values);
        rawIndices.Sort((a, b) =>
        {
            var ra = res[a];
            var rb = res[b];

            int areaA = ra.width * ra.height;
            int areaB = rb.width * rb.height;
            if (areaA != areaB) return areaB.CompareTo(areaA);

            return rb.refreshRateRatio.value.CompareTo(ra.refreshRateRatio.value);
        });

        var options = new List<string>(rawIndices.Count);
        foreach (int raw in rawIndices)
        {
            var r = res[raw];
            int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);
            options.Add($"{r.width} x {r.height}");
            _dropdownToRaw.Add(raw);
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        _suppressCallbacks = false;
    }

    private int DropdownIndexForRaw(int rawIndex)
    {
        if (rawIndex < 0) return -1;

        // Prefer an exact match...
        int idx = _dropdownToRaw.IndexOf(rawIndex);
        if (idx >= 0) return idx;

        // ...otherwise fall back to a same-size entry (the raw index might
        // have been a duplicate that dedup collapsed into a different raw).
        var res = Screen.resolutions;
        if (rawIndex >= res.Length) return -1;

        var target = res[rawIndex];
        for (int i = 0; i < _dropdownToRaw.Count; i++)
        {
            var r = res[_dropdownToRaw[i]];
            if (r.width == target.width && r.height == target.height)
                return i;
        }

        return -1;
    }

    // ---- Load ----

    private void LoadIntoUI(GameSettings s)
    {
        _draft = new GameSettings(s);

        // -1 means "never chosen": adopt the current display resolution.
        if (_draft.resolutionIndex < 0)
        {
            _draft.resolutionIndex = VideoManager.Instance != null
                ? VideoManager.Instance.GetCurrentResolutionIndex()
                : -1;
        }

        _suppressCallbacks = true;

        // FOV
        fovSlider.SetValueWithoutNotify(_draft.cameraFov);
        fovSliderText.text = ((int)_draft.cameraFov).ToString();

        // Resolution
        if (resolutionDropdown.options.Count > 0)
        {
            int dropdownIndex = DropdownIndexForRaw(_draft.resolutionIndex);
            if (dropdownIndex < 0) dropdownIndex = 0;   // safest fallback: first (largest) option

            resolutionDropdown.value = dropdownIndex;
            resolutionDropdown.RefreshShownValue();

            // Keep the draft consistent with what's actually selected.
            _draft.resolutionIndex = _dropdownToRaw[dropdownIndex];
        }

        // Fullscreen / VSync
        fullscreenToggle.SetIsOnWithoutNotify(_draft.fullscreen);
        vSyncToggle.SetIsOnWithoutNotify(_draft.vSync);

        _suppressCallbacks = false;
        IsDirty = false;
    }

    // ---- Callbacks ----

    private void OnFovChanged(float v)
    {
        if (_suppressCallbacks || _draft == null) return;
        _draft.cameraFov = v;
        fovSliderText.text = ((int)v).ToString();
        IsDirty = true;
    }

    private void OnResolutionChanged(int dropdownIndex)
    {
        if (_suppressCallbacks || _draft == null) return;
        if (dropdownIndex < 0 || dropdownIndex >= _dropdownToRaw.Count) return;

        // Store the RAW resolution index, not the dropdown index.
        _draft.resolutionIndex = _dropdownToRaw[dropdownIndex];
        IsDirty = true;
    }

    private void OnFullscreenChanged(bool value)
    {
        if (_suppressCallbacks || _draft == null) return;
        _draft.fullscreen = value;
        IsDirty = true;
    }

    private void OnVSyncChanged(bool value)
    {
        if (_suppressCallbacks || _draft == null) return;
        _draft.vSync = value;
        IsDirty = true;
    }
}