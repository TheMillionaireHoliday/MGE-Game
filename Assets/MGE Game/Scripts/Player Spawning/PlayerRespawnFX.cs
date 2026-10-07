using FishNet.Object;
using System.Collections;
using UnityEngine;

public class PlayerRespawnFX : NetworkBehaviour, IResettableObserver
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip respawnClip;
    [SerializeField] private AudioClip gibsClip;

    [SerializeField] private float respawnSoundWaitTime = 0.2f;

    void OnEnable()
    {
        GlobalEventsManager.Instance.OnPlayerDeath_Observer += OnDeath;
    }

    void OnDisable()
    {
        if (GlobalEventsManager.Instance != null)
            GlobalEventsManager.Instance.OnPlayerDeath_Observer -= OnDeath;
    }

    private void Start()
    {
        StartCoroutine(PlayRespawnWithDelay());
    }

    public void OnDeath(PlayerDiedEvent _e)
    {
        if (_e.playerNO.Owner != Owner)
            return;

        PlayFX(true);
    }
    private void PlayFX(bool playGibs)
    {
        if (playGibs)
            audioSource.PlayOneShot(gibsClip);

        StartCoroutine(PlayRespawnWithDelay());
    }
    private IEnumerator PlayRespawnWithDelay()
    {
        yield return new WaitForSeconds(respawnSoundWaitTime);
        audioSource.PlayOneShot(respawnClip);
    }

    public void ResetStateObserver()
    {
        PlayFX(false);
    }
}
