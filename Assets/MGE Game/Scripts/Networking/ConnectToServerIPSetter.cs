using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

public class ConnectToServerIPSetter : MonoBehaviour
{
    private NetworkManager networkManager;
    [SerializeField] private GameObject mainInputField;

    private TMP_InputField field;

    [SerializeField] private ConnectionErrorDisplayer connectionDisplayer;

    private void OnEnable()
    {
        networkManager = InstanceFinder.NetworkManager;

        field = mainInputField.GetComponent<TMP_InputField>();
        field.onEndEdit.AddListener(SetIPAddress);

        networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }

    private void OnDisable()
    {
        networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        field.onEndEdit.RemoveListener(SetIPAddress);
    }

    public void SetIPAddress(string address)
    {
        string[] parts = address.Split(':');

        if (parts.Length == 2)
        {
            string ip = parts[0];                   // "192.168.1.100"
            string portString = parts[1];           // "7777"
            ushort port = ushort.Parse(portString);

            networkManager.TransportManager.Transport.SetClientAddress(ip);
            networkManager.TransportManager.Transport.SetPort(port);

            connectionDisplayer.ClearErrorStatus();
        }
        else
        {
            var errorText = "Invalid address format. Use ip:port";
            connectionDisplayer.UpdateErrorStatus(errorText, Color.red);
        }
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs state)
    {
        HideShowUI(state);
    }

    private void HideShowUI(ClientConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Started)
        {
            mainInputField.SetActive(false);
        }
        else if (state.ConnectionState == LocalConnectionState.Stopped)
        {
            mainInputField.SetActive(true);
        }
    }
}
