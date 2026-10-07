using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

public class ConnectionErrorDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorOutputField;
    private LocalConnectionState lastState = LocalConnectionState.Stopped;

    private NetworkManager networkManager;

    private void OnEnable()
    {
        networkManager = InstanceFinder.NetworkManager;
        networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }

    private void OnDisable()
    {
        networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args) => ConnectionErrorCheck(args);

    private void ConnectionErrorCheck(ClientConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case LocalConnectionState.Starting:
                UpdateErrorStatus("Attempting to connect...", Color.yellow);
                lastState = LocalConnectionState.Starting;
                break;

            case LocalConnectionState.Started:
                UpdateErrorStatus("Successfully connected!", Color.green);
                lastState = LocalConnectionState.Started;
                break;

            case LocalConnectionState.Stopping:
                if (lastState == LocalConnectionState.Starting)
                {
                    UpdateErrorStatus("Connection failed! Server might be down or IP/Port is incorrect.", Color.red);
                }
                else
                {
                    UpdateErrorStatus("Disconnected.", Color.white);
                }
                lastState = LocalConnectionState.Stopping;
                break;
        }
    }
    public void UpdateErrorStatus(string message, Color color)
    {
        errorOutputField.text = message;
        errorOutputField.color = color;
    }

    public void ClearErrorStatus()
    {
        errorOutputField.text = string.Empty;
    }
}
