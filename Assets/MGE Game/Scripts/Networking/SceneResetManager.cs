using FishNet;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneResetManager : MonoBehaviour
{
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientState;
    }

    private void OnDisable()
    {
        if (InstanceFinder.ClientManager != null)
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientState;
    }

    private void OnClientState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            print("Disconnected");

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            Cursor.lockState = CursorLockMode.None;
        }
    }
}