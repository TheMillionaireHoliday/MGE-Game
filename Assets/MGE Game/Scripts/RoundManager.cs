using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    #region "Singleton"

    public static RoundManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    public NetworkConnection player1_connection = null;
    public NetworkConnection player2_connection = null;

    private bool HasBothPlayers => player1_connection != null && player1_connection.IsValid() &&
                                   player2_connection != null && player2_connection.IsValid();

    public readonly SyncVar<bool> GameActive = new SyncVar<bool>();

    public readonly SyncVar<bool> IntroFinished = new SyncVar<bool>();

    public readonly SyncVar<bool> IsOnVictoryScreen = new SyncVar<bool>();

    private Coroutine startedGame;

    [SerializeField] private UIEventChannelSO UIChannel;

    [SerializeField] private float intervalBeforeStart = 1.0f;
    [SerializeField] private float threeTwoOneInterval = 1.0f;
    [SerializeField] private float holdFightText = 7.0f;
    [SerializeField] private float holdVictoryScreen = 5.0f;

    public override void OnStartServer()
    {
        base.OnStartServer();

        base.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        base.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }

    // =====================  Connect/Disconnect
    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {

            if (player1_connection == null)
                player1_connection = conn;

            else if (player2_connection == null)
                player2_connection = conn;

            StartGameCheck();
        }

        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {

            if (args.ConnectionId == player1_connection.ClientId)
            { player1_connection = null; }

            if (args.ConnectionId == player2_connection.ClientId)
            { player2_connection = null; }

            StopGameCheck();
        }
    }

    private void StartGameCheck()
    {
        if (!HasBothPlayers)
            return;

        StartGame();
    }

    private void StartGame()
    {
        startedGame = StartCoroutine(DelayBeforeGameStart());
    }

    private IEnumerator DelayBeforeGameStart()
    {
        var interval = IsOnVictoryScreen.Value ? holdVictoryScreen : intervalBeforeStart;

        yield return new WaitForSeconds(interval);

        GameActive.Value = true;
        IntroFinished.Value = false;

        GlobalEventsManager.Instance.GameStarted_Server();
        GameStartObserver();
        EnableRoundTextObserver(true);

        IsOnVictoryScreen.Value = false;

        startedGame = StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        UpdateUI("THREE", true);

        yield return new WaitForSeconds(threeTwoOneInterval);

        UpdateUI("TWO", true);

        yield return new WaitForSeconds(threeTwoOneInterval);

        UpdateUI("ONE", true);

        yield return new WaitForSeconds(threeTwoOneInterval);

        // ========================== 

        GameActive.Value = true;
        IntroFinished.Value = true;

        GlobalEventsManager.Instance.IntroFinished_Server();
        IntroFinishedObserver();

        // ==========================

        UpdateUI("FIGHT", true);

        yield return new WaitForSeconds(holdFightText);

        UpdateUI("", false);
    }


    private void StopGameCheck()
    {
        if (HasBothPlayers)
            return;

        StopGame(false);
    }

    private void StopGame(bool isRestart)
    {
        GameActive.Value = false;
        IntroFinished.Value = false;
        if (!isRestart)
            IsOnVictoryScreen.Value = false;

        if (startedGame != null)
            StopCoroutine(startedGame);

        GlobalEventsManager.Instance.GameStopped_Server();
        GameStoppedObserver();

        if (!isRestart)
        {
            EnableRoundTextObserver(false);
        }

        if (!isRestart)
            UpdateUI("", false);
    }

    public void RestartGame()
    {
        IsOnVictoryScreen.Value = true;
        StopGame(true);
        StartGame();
    }

    [ObserversRpc]
    void GameStartObserver() => GlobalEventsManager.Instance.GameStarted_Client();

    [ObserversRpc]
    void IntroFinishedObserver() => GlobalEventsManager.Instance.IntroFinished_Client();

    [ObserversRpc]
    void GameStoppedObserver() => GlobalEventsManager.Instance.GameStopped_Client();

    [ObserversRpc]
    void UpdateUI(string text, bool isEnabled)
    {
        UIChannel.GameStartTextChanged(text, isEnabled);
    }

    [ObserversRpc]
    void EnableRoundTextObserver(bool enabled)
    {
        UIChannel.EnableScoreText(enabled);
    }
}
