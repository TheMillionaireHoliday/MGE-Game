using UnityEngine;

public class GameInputHolder : MonoBehaviour
{
    public static GameInputHolder Instance { get; private set; }
    public static GameInput Input { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Input = new GameInput();
        Input.Enable();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        Input.Disable();
        Input = null;
        Instance = null;
    }
}
