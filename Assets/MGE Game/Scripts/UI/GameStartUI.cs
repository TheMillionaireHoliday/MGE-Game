using TMPro;
using UnityEngine;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private UIEventChannelSO UIChannel;
    [SerializeField] private TMP_Text startText;

    private void OnEnable()
    {
        if (UIChannel != null)
        {
            UIChannel.OnGameStartTextChanged += UpdateUI;
            UIChannel.OnPlayerWon += PlayerWon;
        }
    }

    private void OnDisable()
    {
        if (UIChannel != null)
        {
            UIChannel.OnGameStartTextChanged -= UpdateUI;
            UIChannel.OnPlayerWon -= PlayerWon;
        }
    }

    private void UpdateUI(string text, bool isEnabled)
    {
        startText.text = text;
        startText.enabled = isEnabled;
    }

    private void PlayerWon(int playerNum)
    {
        startText.text = $"Player {playerNum + 1} won!";
        startText.enabled = true;
    }
}
