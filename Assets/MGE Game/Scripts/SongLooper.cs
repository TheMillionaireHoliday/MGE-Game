using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongLooper : MonoBehaviour
{
    [SerializeField] private double loopStartTime;
    [SerializeField] private double loopEndTime;

    private int loopStartSamples;
    private int loopEndSamples;
    private int loopLengthSamples;

    private AudioSource audioSource;

    private void Awake()
    {
        GlobalEventsManager.Instance.OnPlayerSpawned_Client += StartMusic;
    }

    private void OnDestroy()
    {
        if (GlobalEventsManager.Instance != null)
            GlobalEventsManager.Instance.OnPlayerSpawned_Client -= StartMusic;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        loopStartSamples = (int)(loopStartTime * audioSource.clip.frequency);
        loopEndSamples = (int)(loopEndTime * audioSource.clip.frequency);
        loopLengthSamples = loopEndSamples - loopStartSamples;
    }

    private void StartMusic(PlayerSpawnedEvent _)
    {
        audioSource.Play();
    }

    private void Update()
    {
        if (audioSource.timeSamples >= loopEndSamples) { audioSource.timeSamples -= loopLengthSamples; }
    }
}