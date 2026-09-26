using FishNet.Object;
using System.Collections;
using UnityEngine;

public class MuzzleFlash : NetworkBehaviour
{
    [SerializeField] private MeshRenderer flashRenderer;
    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private float flashScale = 0.5f;

    private Vector3 originalScale;
    [SerializeField] private AnimatorStateObserver stateObserver;

    private void Start()
    {
        if (flashRenderer == null)
            flashRenderer = GetComponent<MeshRenderer>();

        originalScale = transform.localScale;
        flashRenderer.enabled = false;
    }

    private void OnEnable()
    {
        stateObserver.OnStateEntered += OnStateEntered;
        stateObserver.OnStateRestarted += OnStateEntered;
    }
    private void OnDisable()
    {
        stateObserver.OnStateEntered -= OnStateEntered;
        stateObserver.OnStateRestarted -= OnStateEntered;
    }

    private void OnStateEntered(string statename)
    {
        if (statename == "Shot")
            PlayFlash();
    }
    public void PlayFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
        PlayFlashServer();
    }

    [ServerRpc]
    public void PlayFlashServer() => PlayFlashObserver();

    [ObserversRpc(ExcludeOwner = true)]
    public void PlayFlashObserver()
    {
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // Enable and scale up
        flashRenderer.enabled = true;
        transform.localScale = originalScale * flashScale;

        // Scale down over time
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            transform.localScale = originalScale * Mathf.Lerp(flashScale, 0.1f, t);

            yield return null;
        }

        // Disable
        flashRenderer.enabled = false;
        transform.localScale = originalScale;
    }
}