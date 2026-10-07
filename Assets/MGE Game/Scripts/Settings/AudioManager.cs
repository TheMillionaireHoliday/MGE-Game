using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetMasterVolume(float linear01) => SetParam("Master", linear01);
    public void SetMusicVolume(float linear01) => SetParam("Music", linear01);
    public void SetSfxVolume(float linear01) => SetParam("SFX", linear01);

    private void SetParam(string name, float linear01)
    {
        float db = linear01 <= 0.0001f ? -80f : Mathf.Log10(linear01) * 20f;
        mixer.SetFloat(name, db);
    }
}