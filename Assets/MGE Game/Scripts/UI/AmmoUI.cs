using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private UIEventChannelSO ammoChannel;
    [SerializeField] private TMP_Text ammoText;

    private void OnEnable()
    {
        if (ammoChannel != null)
            ammoChannel.OnAmmoChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (ammoChannel != null)
            ammoChannel.OnAmmoChanged -= UpdateUI;
    }

    private void UpdateUI(int current, int reserve)
    {
        ammoText.text = $"{current}/{reserve}";
    }
}