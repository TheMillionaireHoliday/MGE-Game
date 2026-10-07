// AmmoEventChannelSO.cs
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/UI Event Channel")]
public class UIEventChannelSO : ScriptableObject
{
    public UnityAction<int, int> OnAmmoChanged; // current, reserve
    public UnityAction<int, int> OnHealthChanged; // current, max

    public UnityAction<int, int> OnScoreChanged; // player1, player2

    public UnityAction<string, bool> OnGameStartTextChanged; // text, isEnabled
    public UnityAction<int> OnPlayerWon; // playerNum
    public UnityAction<bool> OnEnableScoreText; // playerNum

    public void AmmoChanged(int current, int reserve)
    {
        OnAmmoChanged?.Invoke(current, reserve);
    }

    public void HealthChanged(int current, int max)
    {
        OnHealthChanged?.Invoke(current, max);
    }

    public void ScoreChanged(int score1, int score2)
    {
        OnScoreChanged?.Invoke(score1, score2);
    }

    public void GameStartTextChanged(string text, bool isEnabled)
    {
        OnGameStartTextChanged?.Invoke(text, isEnabled);
    }

    public void EnableScoreText(bool enabled)
    {
        OnEnableScoreText?.Invoke(enabled);
    }

    public void PlayerWon(int playerNum)
    {
        OnPlayerWon?.Invoke(playerNum);
    }
}