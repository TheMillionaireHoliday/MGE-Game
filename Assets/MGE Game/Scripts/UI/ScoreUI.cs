using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private UIEventChannelSO scoreChannel;
    [SerializeField] private TMP_Text scoreText;

    private void OnEnable()
    {
        if (scoreText != null)
        {
            scoreChannel.OnScoreChanged += UpdateScore;
            scoreChannel.OnEnableScoreText += EnableText;
        }
    }

    private void OnDisable()
    {
        if (scoreChannel != null)
        {
            scoreChannel.OnScoreChanged -= UpdateScore;
            scoreChannel.OnEnableScoreText -= EnableText;
        }
    }

    private void UpdateScore(int player1_score, int player2_score)
    {
        scoreText.text = $"Player 1 - {player1_score}\nPlayer 2 - {player2_score}";
    }

    private void EnableText(bool enabled)
    {
        scoreText.enabled = enabled;
    }
}
