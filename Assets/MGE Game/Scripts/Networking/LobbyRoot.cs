using UnityEngine;

public class LobbyRoot : MonoBehaviour
{
    private void Awake()
    {
        GlobalEventsManager.Instance.OnPlayerSpawned_Client += PlayerSpawned;
    }

    private void OnDestroy()
    {
        if (GlobalEventsManager.Instance != null)
            GlobalEventsManager.Instance.OnPlayerSpawned_Client -= PlayerSpawned;
    }

    private void Start()
    {
        MenusManager.Instance.AddMenu(this);
    }

    private void PlayerSpawned(PlayerSpawnedEvent e)
    {
        MenusManager.Instance.RemoveMenu(this);
        gameObject.SetActive(false);
    }
}
