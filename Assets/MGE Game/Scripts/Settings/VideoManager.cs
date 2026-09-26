using System;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance { get; private set; }

    // ------------------ FOV ---------------------

    private int _fovValue;
    public int FovValue
    {
        get => _fovValue;
        set { _fovValue = value; OnFovChanged?.Invoke(value); }
    }

    public event Action<int> OnFovChanged;

    // ------------------ Resolution ---------------------

    public event Action<int> OnResolutionChanged;

    public void SetCameraFov(float value)
    {
        // Property setter already fires OnFovChanged; don't fire twice.
        FovValue = (int)value;
    }

    // ------------------ Unity ---------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ------------------ Resolution ---------------------

    public Resolution? GetResolution(int index)
    {
        var res = Screen.resolutions;
        if (index < 0 || index >= res.Length) return null;
        return res[index];
    }

    public int GetCurrentResolutionIndex()
    {
        var res = Screen.resolutions;
        var current = Screen.currentResolution;

        // Scan from the end: Screen.resolutions is ascending, and the last
        // match is the highest refresh rate for that width/height.
        for (int i = res.Length - 1; i >= 0; i--)
        {
            if (res[i].width == current.width &&
                res[i].height == current.height &&
                res[i].refreshRateRatio.Equals(current.refreshRateRatio))
                return i;
        }

        // Fall back to width/height only, in case the exact refresh rate
        // isn't in the list (e.g. driver reports a slightly different value).
        for (int i = res.Length - 1; i >= 0; i--)
        {
            if (res[i].width == current.width && res[i].height == current.height)
                return i;
        }

        return -1;
    }

    public void ApplyResolution(int index, bool fullscreen)
    {
        var r = GetResolution(index);
        if (r == null)
        {
            // Unset or out of range: keep the current resolution,
            // but still honor the fullscreen toggle.
            Screen.fullScreen = fullscreen;
            return;
        }

        var mode = fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        var rate = r.Value.refreshRateRatio;   // Unity 2022.2+

        Screen.SetResolution(r.Value.width, r.Value.height, mode, rate);
        OnResolutionChanged?.Invoke(index);
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
}