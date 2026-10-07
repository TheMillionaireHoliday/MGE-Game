using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public readonly SyncVar<int> player1_score = new SyncVar<int>(0);
    public readonly SyncVar<int> player2_score = new SyncVar<int>(0);

    public int winLimit = 20;

    [SerializeField] private UIEventChannelSO UIChannel;
    public bool CanChangeScore => (RoundManager.Instance.GameActive.Value && RoundManager.Instance.IntroFinished.Value) &&
                                    (player1_score.Value < winLimit) && (player2_score.Value < winLimit) &&
                                    !roundManager.IsOnVictoryScreen.Value;

    private RoundManager roundManager;

    // ===================== Events

    public override void OnStartServer()
    {

        base.OnStartServer();

        player1_score.OnChange += OnPlayer1Score;
        player2_score.OnChange += OnPlayer2Score;

        GlobalEventsManager.Instance.OnPlayerDeath_Server += OnPlayerDeath;
        GlobalEventsManager.Instance.OnGameStarted_Server += ResetScore;

        roundManager = RoundManager.Instance;
    }
    public override void OnStopServer()
    {

        base.OnStopServer();

        player1_score.OnChange -= OnPlayer1Score;
        player2_score.OnChange -= OnPlayer2Score;

        if (GlobalEventsManager.Instance != null)
        {
            GlobalEventsManager.Instance.OnPlayerDeath_Server -= OnPlayerDeath;
            GlobalEventsManager.Instance.OnGameStarted_Server -= ResetScore;
        }
    }

    private void OnPlayer1Score(int prev, int next, bool asServer)
    {
        if (asServer)
            OnScoreChanged(next, player2_score.Value);
    }

    private void OnPlayer2Score(int prev, int next, bool asServer)
    {
        if (asServer)
            OnScoreChanged(player1_score.Value, next);
    }

    private void OnScoreChanged(int _scorePlayer1, int _scorePlayer2)
    {
        if (_scorePlayer1 >= winLimit)
            PlayerWon(0);
        else if (_scorePlayer2 >= winLimit)
            PlayerWon(1);

        BroadcastOnScoreChanged(_scorePlayer1, _scorePlayer2);
    }

    [ObserversRpc]
    private void BroadcastOnScoreChanged(int _scorePlayer1, int _scorePlayer2)
    {
        UIChannel.ScoreChanged(_scorePlayer1, _scorePlayer2);
    }

    private void PlayerWon(int playerNum)
    {
        roundManager.RestartGame();
        PlayerWonObserver(playerNum);
    }


    private void OnPlayerDeath(PlayerDiedEvent playerDiedEvent)
    {

        if (!CanChangeScore) return;

        if (playerDiedEvent.playerNO.Owner == roundManager.player1_connection)
            player2_score.Value++;

        if (playerDiedEvent.playerNO.Owner == roundManager.player2_connection)
            player1_score.Value++;
    }

    // =====================  Public methods

    private void ResetScore()
    {
        player1_score.Value = 0;
        player2_score.Value = 0;
    }

    [ObserversRpc]
    void PlayerWonObserver(int playerNum) => UIChannel.PlayerWon(playerNum);
}