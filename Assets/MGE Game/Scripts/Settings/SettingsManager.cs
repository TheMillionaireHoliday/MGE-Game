using System;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string FileName = "GameSettings.json";

    public GameSettings Current { get; private set; }

    public event Action<GameSettings> OnSettingsApplied;

    private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Start()
    {
        ApplyAll();
    }

    public void Load()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                string json = File.ReadAllText(FilePath);
                Current = JsonUtility.FromJson<GameSettings>(json) ?? new GameSettings();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load settings, using defaults: {e.Message}");
                Current = new GameSettings();
            }
        }
        else
        {
            Current = new GameSettings();
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Current, prettyPrint: true);
        File.WriteAllText(FilePath, json);
    }

    // ---------- Audio ----------
    public void SetMasterVolume(float v) { Current.masterVolume = Mathf.Clamp01(v); }
    public void SetMusicVolume(float v) { Current.musicVolume = Mathf.Clamp01(v); }
    public void SetSfxVolume(float v) { Current.sfxVolume = Mathf.Clamp01(v); }

    // ---------- Camera / Input ----------
    public void SetMouseSensitivity(float v) { Current.mouseSensitivity = Mathf.Max(0.01f, v); }
    public void SetCameraFov(float v) { Current.cameraFov = Mathf.Clamp(v, 40f, 120f); }

    // ---------- Video ----------
    public void SetResolutionIndex(int index) { Current.resolutionIndex = index; }
    public void SetFullscreen(bool value) { Current.fullscreen = value; }
    public void SetVSync(bool value) { Current.vSync = value; }

    // ---------- Apply ----------
    public void ApplyAll()
    {
        var a = AudioManager.Instance;
        var v = VideoManager.Instance;
        var i = InputManager.Instance;

        if (a != null)
        {
            a.SetMasterVolume(Current.masterVolume);
            a.SetMusicVolume(Current.musicVolume);
            a.SetSfxVolume(Current.sfxVolume);
        }
        else
        {
            Debug.LogError("[SettingsManager] AudioManager.Instance is null — audio settings not applied.");
        }

        if (v != null)
        {
            v.SetCameraFov(Current.cameraFov);
            v.SetVSync(Current.vSync);
            v.ApplyResolution(Current.resolutionIndex, Current.fullscreen);
        }
        else
        {
            Debug.LogError("[SettingsManager] VideoManager.Instance is null — video settings not applied.");
        }

        if (i != null)
        {
            i.SetSensitivity(Current.mouseSensitivity);
        }
        else
        {
            Debug.LogError("[SettingsManager] InputManager.Instance is null — input settings not applied.");
        }

        OnSettingsApplied?.Invoke(Current);
    }

    public void SaveAndApply()
    {
        ApplyAll();
        Save();
    }
}