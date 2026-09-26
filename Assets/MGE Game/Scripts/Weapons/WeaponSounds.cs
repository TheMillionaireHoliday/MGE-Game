using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSounds : NetworkBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AnimatorStateObserver animatorStateObserver;

    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitchVariation = 0.05f;

    public List<AudioClipSettings> animationAssociatedAudio;

    private string lastState = "";
    private Coroutine delayedSound;

    private PlayerHealth playerHealth;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        animatorStateObserver.OnStateEntered += AnimationPlayed;
        animatorStateObserver.OnStateRestarted += AnimationPlayed;

        playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnDeathObserver += Stop;
    }
    private void OnDisable()
    {
        animatorStateObserver.OnStateEntered += AnimationPlayed;
        animatorStateObserver.OnStateRestarted += AnimationPlayed;

        if (playerHealth != null)
            playerHealth.OnDeathObserver -= Stop;

        StopAllCoroutines();
    }


    public void AnimationPlayed(string state)
    {
        if (delayedSound != null)
        {
            StopCoroutine(delayedSound);
            delayedSound = null;
        }

        lastState = state;
        delayedSound = StartCoroutine(PlaySoundNextFrame(state));
    }


    private IEnumerator PlaySoundNextFrame(string state) // Used to block sounds that are played from interrupted animations (that last only one frame).
    {
        yield return null;

        if (state == lastState && meshRenderer.enabled)
        {
            PlaySoundOwner(state);
        }
        delayedSound = null;
    }
    private void PlaySoundOwner(string state)
    {
        foreach (AudioClipSettings clip in animationAssociatedAudio)
        {
            if (clip.stateName == state)
            {
                StartCoroutine(PlayClip(clip));
                PlaySoundServer(state);
            }
        }
    }

    [ServerRpc]
    private void PlaySoundServer(string state)
    {
        PlaySoundObserver(state);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void PlaySoundObserver(string state)
    {
        foreach (AudioClipSettings clip in animationAssociatedAudio)
        {
            if (clip.stateName == state)
            {

                StartCoroutine(PlayClip(clip));

            }
        }
    }

    private IEnumerator PlayClip(AudioClipSettings clip)
    {
        yield return new WaitForSeconds(clip.delay);

        if (clip.interruptAllOtherSounds)
            Stop();

        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip.clip, clip.volume * this.volume);
    }

    public void Stop() => audioSource.Stop();
}