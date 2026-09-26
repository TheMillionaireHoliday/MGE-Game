using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private UIEventChannelSO healthChannel;
    [SerializeField] private TMP_Text healthText;

    [SerializeField] private Color normalHealthColor = Color.white;
    [SerializeField] private Color overhealColor = new Color(0.68f, 1f, 1f);

    private void OnEnable()
    {
        if (healthChannel != null)
            healthChannel.OnHealthChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (healthChannel != null)
            healthChannel.OnHealthChanged -= UpdateUI;
    }

    private void UpdateUI(int currentHealth, int maxHealth)
    {
        var color = currentHealth > maxHealth ? overhealColor : normalHealthColor;

        healthText.color = color;

        healthText.text = $"{currentHealth}";
    }
}
