using System.Collections.Generic;
using UnityEngine;

public class MenusManager : MonoBehaviour
{
    public static MenusManager Instance { get; private set; }

    [SerializeField] List<MonoBehaviour> activeMenus = new List<MonoBehaviour>();
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddMenu(MonoBehaviour menu)
    {
        if (activeMenus.Contains(menu))
            return;

        activeMenus.Add(menu);
        OnMenusUpdate();
    }

    public void RemoveMenu(MonoBehaviour menu)
    {
        if (!activeMenus.Contains(menu))
            return;

        activeMenus.Remove(menu);
        OnMenusUpdate();
    }

    private void OnMenusUpdate()
    {
        RemoveNullValues();

        if (activeMenus.Count > 0)
        {
            InputModeController.Instance.SetMode(InputMode.Menu);
        }
        else
        {
            InputModeController.Instance.SetMode(InputMode.FirstPersonController);
        }
    }

    private void RemoveNullValues()
    {
        activeMenus.RemoveAll(item => item is null);
    }
}
