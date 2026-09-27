using System;
using System.Collections.Generic;

[Serializable]
public class GameSettings
{
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;
    public float mouseSensitivity = 1f;
    public float cameraFov = 100f;

    // Video
    public int resolutionIndex = -1;    // -1 = "not set yet, adopt current on first open"
    public bool fullscreen = true;

    public List<BindingOverride> bindingOverrides = new();

    public GameSettings() { }

    public GameSettings(GameSettings other)
    {
        masterVolume = other.masterVolume;
        musicVolume = other.musicVolume;
        sfxVolume = other.sfxVolume;
        mouseSensitivity = other.mouseSensitivity;
        cameraFov = other.cameraFov;

        resolutionIndex = other.resolutionIndex;
        fullscreen = other.fullscreen;

        bindingOverrides = new List<BindingOverride>(other.bindingOverrides);
    }
}

[Serializable]
public struct BindingOverride
{
    public string actionName;
    public string bindingId;
    public string path;
    public string processors;

    public BindingOverride(string action, string binding, string path, string proc)
    {
        actionName = action; bindingId = binding; this.path = path; processors = proc;
    }
}