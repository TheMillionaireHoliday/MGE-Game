using UnityEngine;

public class EmptySettingsUI : MonoBehaviour, ISettingsPanel
{
    public void Apply()
    {

    }

    public void Open()
    {
        LoadIntoUI(SettingsManager.Instance.Current);
    }

    public void Cancel()
    {
        LoadIntoUI(SettingsManager.Instance.Current);
    }


    private void LoadIntoUI(GameSettings s)
    {

    }
}
