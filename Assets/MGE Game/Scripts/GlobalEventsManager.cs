using FishNet.Object;
using System;

public class GlobalEventsManager : NetworkBehaviour
{
    #region "Singleton"

    private static GlobalEventsManager _instance;
    public static GlobalEventsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #endregion

    // =========================== Events

    public event Action OnGameStarted_Server;
    public event Action OnGameStarted_Client;

    public event Action OnIntroFinished_Server;
    public event Action OnIntroFinished_Client;

    public event Action OnGameStopped_Server;
    public event Action OnGameStopped_Client;

    public event Action<PlayerDiedEvent> OnPlayerDeath_Server;
    public event Action<PlayerDiedEvent> OnPlayerDeath_Observer;

    public event Action<PlayerSpawnedEvent> OnPlayerSpawned_Client;
    public event Action<PlayerSpawnedEvent> OnPlayerSpawned_Server;

    [Server] public void GameStarted_Server() => OnGameStarted_Server?.Invoke();
    [Client] public void GameStarted_Client() => OnGameStarted_Client?.Invoke();

    [Server] public void IntroFinished_Server() => OnIntroFinished_Server?.Invoke();
    [Client] public void IntroFinished_Client() => OnIntroFinished_Client?.Invoke();

    [Server] public void GameStopped_Server() => OnGameStopped_Server?.Invoke();
    [Client] public void GameStopped_Client() => OnGameStopped_Client?.Invoke();

    [Server] public void PlayerDied_Server(PlayerDiedEvent _event) => OnPlayerDeath_Server?.Invoke(_event);
    [Client] public void PlayerDied_Observer(PlayerDiedEvent _event) => OnPlayerDeath_Observer?.Invoke(_event);

    [Server] public void PlayerSpawned_Server(PlayerSpawnedEvent _event) => OnPlayerSpawned_Server?.Invoke(_event);
    [Client] public void PlayerSpawned_Client(PlayerSpawnedEvent _event) => OnPlayerSpawned_Client?.Invoke(_event);
}

public class PlayerDiedEvent
{
    public DamageInfo takeDamageInfo;
    public NetworkObject playerNO;

    public PlayerDiedEvent(DamageInfo takeDamageInfo, NetworkObject playerNO)
    {
        this.takeDamageInfo = takeDamageInfo;
        this.playerNO = playerNO;
    }

    public PlayerDiedEvent() { } // Needed for network serialization
}

public class PlayerSpawnedEvent
{
    public NetworkObject playerNO;

    public PlayerSpawnedEvent(NetworkObject playerNO)
    {
        this.playerNO = playerNO;
    }

    public PlayerSpawnedEvent() { } // Needed for network serialization
}