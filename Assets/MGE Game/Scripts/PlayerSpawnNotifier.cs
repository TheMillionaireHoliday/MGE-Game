using FishNet.Object;

public class PlayerSpawnNotifier : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        GlobalEventsManager.Instance.PlayerSpawned_Client(new PlayerSpawnedEvent(this.NetworkObject));
    }
    public override void OnStartServer()
    {
        base.OnStartClient();

        GlobalEventsManager.Instance.PlayerSpawned_Server(new PlayerSpawnedEvent(this.NetworkObject));
    }
}
