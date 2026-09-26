using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishNet.Example
{
    /// <summary>
    /// New Input System compatible replacement for NetworkHudCanvases.
    /// Wire up your own Canvas buttons and indicators in the Inspector.
    /// </summary>
    public class ConnectionUI : MonoBehaviour
    {
        private enum AutoStartType { Disabled, Host, Server, Client }

        [Header("Auto Start")]
        [Tooltip("Automatically start a connection on scene load.")]
        [SerializeField] private AutoStartType _autoStartType = AutoStartType.Disabled;

        [Header("Indicators")]
        [SerializeField] private Color _stoppedColor = new Color(0.65f, 0.15f, 0.15f);
        [SerializeField] private Color _changingColor = new Color(0.85f, 0.75f, 0.15f);
        [SerializeField] private Color _startedColor = new Color(0.15f, 0.65f, 0.15f);

        [SerializeField] private Image _serverIndicator;
        [SerializeField] private Image _clientIndicator;

        [Header("Buttons (optional — assign if you want text auto-updated)")]
        [SerializeField] private Button _serverButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private TMP_Text _serverButtonLabel;
        [SerializeField] private TMP_Text _clientButtonLabel;

        private NetworkManager _networkManager;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;

        private void Start()
        {
            _networkManager = FindObjectOfType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found, ConnectionUI will not function.");
                return;
            }

            // Hook button clicks (new Input System routes clicks via InputSystemUIInputModule).
            if (_serverButton != null) _serverButton.onClick.AddListener(OnClick_Server);
            if (_clientButton != null) _clientButton.onClick.AddListener(OnClick_Client);

            // Subscribe to connection state changes.
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

            // Initial visual state.
            UpdateIndicator(_serverState, _serverIndicator);
            UpdateIndicator(_clientState, _clientIndicator);
            UpdateButtonLabels();

            // Auto-start if configured.
            if (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Server)
                OnClick_Server();
            if (!Application.isBatchMode &&
                (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Client))
                OnClick_Client();
        }

        private void OnDestroy()
        {
            if (_networkManager == null) return;

            _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;

            if (_serverButton != null) _serverButton.onClick.RemoveListener(OnClick_Server);
            if (_clientButton != null) _clientButton.onClick.RemoveListener(OnClick_Client);
        }

        // --- Button handlers -------------------------------------------------

        public void OnClick_Server()
        {
            if (_networkManager == null) return;

            if (_serverState != LocalConnectionState.Stopped)
                _networkManager.ServerManager.StopConnection(true);
            else
                _networkManager.ServerManager.StartConnection();

            Deselect();
        }

        public void OnClick_Client()
        {
            if (_networkManager == null) return;

            if (_clientState != LocalConnectionState.Stopped)
                _networkManager.ClientManager.StopConnection();
            else
                _networkManager.ClientManager.StartConnection();

            Deselect();
        }

        // --- State callbacks ------------------------------------------------

        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
        {
            _serverState = args.ConnectionState;
            UpdateIndicator(_serverState, _serverIndicator);
            UpdateButtonLabels();
        }

        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
        {
            _clientState = args.ConnectionState;
            UpdateIndicator(_clientState, _clientIndicator);
            UpdateButtonLabels();
        }

        // --- Helpers --------------------------------------------------------

        private void UpdateIndicator(LocalConnectionState state, Image img)
        {
            if (img == null) return;

            img.color = state switch
            {
                LocalConnectionState.Started => _startedColor,
                LocalConnectionState.Stopped => _stoppedColor,
                _ => _changingColor
            };
        }

        private static string GetNextStateText(LocalConnectionState state) => state switch
        {
            LocalConnectionState.Stopped => "Start",
            LocalConnectionState.Starting => "Starting",
            LocalConnectionState.Stopping => "Stopping",
            LocalConnectionState.Started => "Stop",
            _ => "Invalid"
        };

        private void UpdateButtonLabels()
        {
            if (_serverButtonLabel != null)
                _serverButtonLabel.text = $"{GetNextStateText(_serverState)} Server";
            if (_clientButtonLabel != null)
                _clientButtonLabel.text = $"{GetNextStateText(_clientState)} Client";
        }

        /// <summary>
        /// Clears UI selection so keyboard/controller focus doesn't stay on a button.
        /// </summary>
        private void Deselect()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}