using System;
using System.Threading.Tasks;
using UnityEngine;
// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;
// using Unity.Services.Relay;
// using Unity.Services.Relay.Models;

#if UNITY_NETCODE_GAMEOBJECTS
namespace GravityWars.Networking
{
    /// <summary>
    /// Network service for online multiplayer using Unity Relay and Netcode for GameObjects.
    ///
    /// Features:
    /// - Relay server allocation and join codes
    /// - Host/Client connection management
    /// - Network state tracking
    /// - Disconnect/reconnect handling
    /// - Latency monitoring
    ///
    /// Usage:
    ///   await NetworkService.Instance.StartHost();
    ///   await NetworkService.Instance.JoinAsClient(joinCode);
    /// </summary>
    public class NetworkService : MonoBehaviour
    {
        #region Network State

        public enum NetworkState
        {
            Disconnected,
            Connecting,
            Connected,
            Hosting,
            Disconnecting
        }

        private NetworkState _currentState = NetworkState.Disconnected;
        public NetworkState CurrentState => _currentState;

        public bool IsConnected => _currentState == NetworkState.Connected || _currentState == NetworkState.Hosting;
        public bool IsHost => _currentState == NetworkState.Hosting;

        #endregion

        #region Configuration

        [Header("Network Configuration")]
        [Tooltip("Maximum number of players per match")]
        public int maxPlayers = 2;

        [Tooltip("Enable network debug logging")]
        public bool debugLogging = true;

        #endregion

        #region Relay Data

        private string _currentJoinCode;
        private Allocation _hostAllocation;
        private JoinAllocation _clientAllocation;

        public string CurrentJoinCode => _currentJoinCode;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Subscribe to network events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from network events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            }

            // Clean up network connection
            if (IsConnected)
            {
                Disconnect();
            }
        }

        #endregion

        #region Host API

        /// <summary>
        /// Starts the game as host using Unity Relay.
        /// Returns the join code that clients can use to connect.
        /// </summary>
        public async Task<string> StartHost()
        {
            if (IsConnected)
            {
                Debug.LogWarning("[Network] Already connected");
                return null;
            }

            try
            {
                _currentState = NetworkState.Connecting;
                Log("Starting host...");

                // Allocate relay server
                _hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

                // Get join code
                _currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(_hostAllocation.AllocationId);

                Log($"Relay server allocated. Join code: {_currentJoinCode}");

                // Set up Unity Transport with Relay
                var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                transport.SetRelayServerData(new Unity.Services.Relay.Models.RelayServerData(_hostAllocation, "dtls"));

                // Start as host
                bool started = NetworkManager.Singleton.StartHost();

                if (started)
                {
                    _currentState = NetworkState.Hosting;
                    Log("Host started successfully");
                    return _currentJoinCode;
                }
                else
                {
                    _currentState = NetworkState.Disconnected;
                    LogError("Failed to start host");
                    return null;
                }
            }
            catch (Exception e)
            {
                _currentState = NetworkState.Disconnected;
                LogError($"Failed to start host: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Client API

        /// <summary>
        /// Joins a game as client using a join code.
        /// </summary>
        public async Task<bool> JoinAsClient(string joinCode)
        {
            if (IsConnected)
            {
                Debug.LogWarning("[Network] Already connected");
                return false;
            }

            try
            {
                _currentState = NetworkState.Connecting;
                Log($"Joining with code: {joinCode}");

                // Join relay server
                _clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                _currentJoinCode = joinCode;

                Log("Relay server joined successfully");

                // Set up Unity Transport with Relay
                var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                transport.SetRelayServerData(new Unity.Services.Relay.Models.RelayServerData(_clientAllocation, "dtls"));

                // Start as client
                bool started = NetworkManager.Singleton.StartClient();

                if (started)
                {
                    Log("Client started successfully");
                    return true;
                }
                else
                {
                    _currentState = NetworkState.Disconnected;
                    LogError("Failed to start client");
                    return false;
                }
            }
            catch (Exception e)
            {
                _currentState = NetworkState.Disconnected;
                LogError($"Failed to join as client: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Disconnect

        /// <summary>
        /// Disconnects from the current network session.
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
                return;

            _currentState = NetworkState.Disconnecting;
            Log("Disconnecting...");

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            _currentState = NetworkState.Disconnected;
            _currentJoinCode = null;
            _hostAllocation = null;
            _clientAllocation = null;

            Log("Disconnected");
        }

        #endregion

        #region Network Events

        private void OnClientConnected(ulong clientId)
        {
            Log($"Client connected: {clientId}");

            if (NetworkManager.Singleton.IsHost)
            {
                // Host perspective: client joined
                Log($"Player {clientId} joined the match");
            }
            else if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // Client perspective: we connected successfully
                _currentState = NetworkState.Connected;
                Log("Successfully connected to host");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Log($"Client disconnected: {clientId}");

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // We disconnected
                _currentState = NetworkState.Disconnected;
                Log("Disconnected from server");
            }
            else if (NetworkManager.Singleton.IsHost)
            {
                // Host perspective: client left
                Log($"Player {clientId} left the match");
            }
        }

        private void OnServerStarted()
        {
            Log("Server started");
        }

        #endregion

        #region Latency Monitoring

        /// <summary>
        /// Gets the current network round-trip time (ping) in milliseconds.
        /// </summary>
        public float GetRTT()
        {
            if (!IsConnected || NetworkManager.Singleton == null)
                return 0;

            return NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId);
        }

        /// <summary>
        /// Returns true if latency is acceptable for gameplay (<150ms).
        /// </summary>
        public bool IsLatencyGood()
        {
            return GetRTT() < 150f;
        }

        #endregion

        #region Helper Methods

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[Network] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[Network] {message}");
        }

        #endregion
    }
}
#endif // UNITY_NETCODE_GAMEOBJECTS
