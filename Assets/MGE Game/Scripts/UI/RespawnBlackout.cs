using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RespawnBlackout : NetworkBehaviour
{
    private Image image;
    [SerializeField] float blackOutTime = 0.1f;
    private void Awake()
    {
        GlobalEventsManager.Instance.OnPlayerDeath_Observer += Blackout;
        image = GetComponent<Image>();
    }
    private void OnDestroy()
    {
        if(GlobalEventsManager.Instance != null)
            GlobalEventsManager.Instance.OnPlayerDeath_Observer -= Blackout;
    }

    private void Blackout(PlayerDiedEvent playerEvent)
    {
        if (LocalConnection.ClientId != playerEvent.playerNO.OwnerId)
            return;

        StopCoroutine(BlackoutCoroutine());
        StartCoroutine(BlackoutCoroutine());
    }

    private IEnumerator BlackoutCoroutine()
    {
        image.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(blackOutTime);
        image.color = new Color(0, 0, 0, 0);
    }
}
