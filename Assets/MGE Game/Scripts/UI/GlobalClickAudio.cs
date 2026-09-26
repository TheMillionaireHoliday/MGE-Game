using UnityEngine;
using UnityEngine.UI;

public class GlobalClickAudio : MonoBehaviour
{
    public static GlobalClickAudio Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;   // ONE shared, 2D

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        // Hook buttons that already exist
        foreach (var btn in FindObjectsByType<Button>(FindObjectsSortMode.None))
            Hook(btn);

        // Hook buttons created later (new panels, spawned UI, etc.)
        //SceneManager.sceneLoaded += (_, __) => HookAll();
    }

    private void HookAll()
    {
        foreach (var btn in FindObjectsByType<Button>(FindObjectsSortMode.None))
            Hook(btn);
    }

    private void Hook(Button btn)
    {
        // RemoveListener first so re-hooking never double-fires
        btn.onClick.RemoveListener(PlayClick);
        btn.onClick.AddListener(PlayClick);
    }

    public void PlayClick()
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);

        audioSource.Play();
    }
}