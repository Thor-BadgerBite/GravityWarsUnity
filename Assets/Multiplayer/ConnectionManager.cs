// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;
// using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Threading.Tasks;
using System;

#if UNITY_SERVICES_RELAY
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
#endif

#if UNITY_NETCODE_GAMEOBJECTS
namespace GravityWars.Multiplayer
{
    /// <summary>
    /// Manages network connections using Unity Relay for NAT punch-through.
    ///
    /// Features:
    /// - Unity Relay integration for peer-to-peer connections
    /// - Join code generation for easy matchmaking
    /// - Automatic transport configuration
    /// - Connection state management
    /// - Error handling and retries
    ///
    /// Architecture:
    /// - Host creates Relay allocation and gets join code
    /// - Clients join using the join code
    /// - Unity Transport automatically configured with Relay data
    /// - NetworkManager handles the actual Netcode connection
    ///
    /// Usage:
    /// - Host: await ConnectionManager.Instance.StartHostWithRelay()
    /// - Client: await ConnectionManager.Instance.StartClientWithRelay(joinCode)
    /// - Disconnect: ConnectionManager.Instance.Disconnect()
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        #region Singleton

        private static ConnectionManager _instance;
        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ConnectionManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ConnectionManager");
                        _instance = go.AddComponent<ConnectionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Configuration

        [Header("Relay Configuration")]
        [SerializeField] private int maxConnections = 4; // Max players - 1 (host doesn't count)
        [SerializeField] private string relayRegion = ""; // Empty = auto-select closest

        [Header("Retry Configuration")]
        [SerializeField] private int maxRetries = 3;
        [SerializeField] private float retryDelay = 2f;

        #endregion

        #region Events

        public event Action<string> OnJoinCodeGenerated;
        public event Action OnHostStarted;
        public event Action OnClientConnected;
        public event Action OnDisconnected;
        public event Action<string> OnConnectionError;

        #endregion

        #region State

        private string _currentJoinCode = "";
        private bool _isConnecting = false;

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Error
        }

        private ConnectionState _connectionState = ConnectionState.Disconnected;

        #endregion

        #region Public API - Host

        /// <summary>
        /// Starts a host (server + client) using Unity Relay.
        /// Returns the join code for other players to join.
        /// </summary>
        public async Task<string> StartHostWithRelay()
        {
            if (_isConnecting)
            {
                Debug.LogWarning("[ConnectionManager] Already connecting");
                return null;
            }

            _isConnecting = true;
            _connectionState = ConnectionState.Connecting;

            try
            {
                Debug.Log("[ConnectionManager] Starting host with Relay...");

#if UNITY_SERVICES_RELAY
                // Create Relay allocation
                Allocation allocation = await CreateRelayAllocation(maxConnections);

                // Get join code
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                _currentJoinCode = joinCode;

                Debug.Log($"[ConnectionManager] Relay join code: {joinCode}");
                OnJoinCodeGenerated?.Invoke(joinCode);

                // Configure Unity Transport with Relay data
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                // Start NetworkManager as host
                bool started = NetworkManager.Singleton.StartHost();

                if (started)
                {
                    _connectionState = ConnectionState.Connected;
                    _isConnecting = false;
                    OnHostStarted?.Invoke();
                    Debug.Log("[ConnectionManager] ✓ Host started successfully");
                    return joinCode;
                }
                else
                {
                    throw new Exception("NetworkManager.StartHost() failed");
                }
#else
                Debug.LogError("[ConnectionManager] Unity Relay package not installed!");
                OnConnectionError?.Invoke("Unity Relay package not installed");
                _connectionState = ConnectionState.Error;
                _isConnecting = false;
                return null;
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionManager] Failed to start host: {e.Message}");
                OnConnectionError?.Invoke(e.Message);
                _connectionState = ConnectionState.Error;
                _isConnecting = false;
                return null;
            }
        }

        /// <summary>
        /// Starts a host without Relay (local network only).
        /// Useful for testing.
        /// </summary>
        public bool StartHostLocal()
        {
            if (_isConnecting)
            {
                Debug.LogWarning("[ConnectionManager] Already connecting");
                return false;
            }

            _isConnecting = true;
            _connectionState = ConnectionState.Connecting;

            try
            {
                Debug.Log("[ConnectionManager] Starting local host...");

                // Configure Unity Transport for local connection
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetConnectionData("127.0.0.1", 7777);

                // Start NetworkManager as host
                bool started = NetworkManager.Singleton.StartHost();

                if (started)
                {
                    _connectionState = ConnectionState.Connected;
                    _isConnecting = false;
                    OnHostStarted?.Invoke();
                    Debug.Log("[ConnectionManager] ✓ Local host started successfully");
                    return true;
                }
                else
                {
                    throw new Exception("NetworkManager.StartHost() failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionManager] Failed to start local host: {e.Message}");
                OnConnectionError?.Invoke(e.Message);
                _connectionState = ConnectionState.Error;
                _isConnecting = false;
                return false;
            }
        }

        #endregion

        #region Public API - Client

        /// <summary>
        /// Joins a host using a Relay join code.
        /// </summary>
        public async Task<bool> StartClientWithRelay(string joinCode)
        {
            if (_isConnecting)
            {
                Debug.LogWarning("[ConnectionManager] Already connecting");
                return false;
            }

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("[ConnectionManager] Join code is empty");
                OnConnectionError?.Invoke("Join code is empty");
                return false;
            }

            _isConnecting = true;
            _connectionState = ConnectionState.Connecting;

            try
            {
                Debug.Log($"[ConnectionManager] Joining with code: {joinCode}");

#if UNITY_SERVICES_RELAY
                // Join Relay allocation using join code
                JoinAllocation joinAllocation = await JoinRelayAllocation(joinCode);

                // Configure Unity Transport with Relay data
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                // Start NetworkManager as client
                bool started = NetworkManager.Singleton.StartClient();

                if (started)
                {
                    _connectionState = ConnectionState.Connected;
                    _isConnecting = false;
                    _currentJoinCode = joinCode;
                    OnClientConnected?.Invoke();
                    Debug.Log("[ConnectionManager] ✓ Client connected successfully");
                    return true;
                }
                else
                {
                    throw new Exception("NetworkManager.StartClient() failed");
                }
#else
                Debug.LogError("[ConnectionManager] Unity Relay package not installed!");
                OnConnectionError?.Invoke("Unity Relay package not installed");
                _connectionState = ConnectionState.Error;
                _isConnecting = false;
                return false;
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionManager] Failed to join: {e.Message}");
                OnConnectionError?.Invoke(e.Message);
                _connectionState = ConnectionState.Error;
                _isConnecting = false;
                return false;
            }
        }

        /// <summary>
        /// Joins a host on local network.
        /// Useful for testing.
        /// </summary>
        public bool StartClientLocal(string ipAddress = "127.0.0.1", ushort port = 7777)
        {
            if (_isConnecting)
            {
                Debug.LogWarning("[ConnectionManager] Already connecting");
                return false;
            }

            _isConnecting = true;
            _connectionState = ConnectionState.Connecting;

            try
            {
                Debug.Log($"[ConnectionManager] Joining local host at {ipAddress}:{port}");

                // Configure Unity Transport for local connection
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetConnectionData(ipAddress, port);

                // Start NetworkManager as client
                bool started = NetworkManager.Singleton.StartClient();

                if (started)
                {
                    _connectionState = ConnectionState.Connected;
                    _isConnecting = false;
                    OnClientConnected?.Invoke();
                    Debug.Log("[ConnectionManager] ✓ Local client connected successfully");
                    return true;
                }
                else
                {
                    throw new Exception("NetworkManager.StartClient() failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionManager] Failed to join local: {e.Message}");
                OnConnectionError?.Invoke(e.Message);
                _connectionState = ConnectionState.Error;
                _isConnecting = false;
                return false;
            }
        }

        #endregion

        #region Public API - Disconnect

        /// <summary>
        /// Disconnects from current session.
        /// </summary>
        public void Disconnect()
        {
            if (NetworkManager.Singleton == null) return;

            Debug.Log("[ConnectionManager] Disconnecting...");

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }

            _connectionState = ConnectionState.Disconnected;
            _currentJoinCode = "";
            _isConnecting = false;

            OnDisconnected?.Invoke();
            Debug.Log("[ConnectionManager] Disconnected");
        }

        #endregion

        #region Relay Helpers

#if UNITY_SERVICES_RELAY
        private async Task<Allocation> CreateRelayAllocation(int maxConnections, int retryCount = 0)
        {
            try
            {
                // Create allocation with optional region
                Allocation allocation;
                if (string.IsNullOrEmpty(relayRegion))
                {
                    allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                }
                else
                {
                    allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, relayRegion);
                }

                Debug.Log($"[ConnectionManager] Relay allocation created: {allocation.AllocationId}");
                return allocation;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionManager] Failed to create allocation: {e.Message}");

                // Retry logic
                if (retryCount < maxRetries)
                {
                    Debug.Log($"[ConnectionManager] Retrying... ({retryCount + 1}/{maxRetries})");
                    await Task.Delay((int)(retryDelay * 1000));
                    return await CreateRelayAllocation(maxConnections, retryCount + 1);
                }

                throw;
            }
        }

        private async Task<JoinAllocation> JoinRelayAllocation(string joinCode, int retryCount = 0)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log($"[ConnectionManager] Joined Relay allocation: {joinAllocation.AllocationId}");
                return joinAllocation;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionManager] Failed to join allocation: {e.Message}");

                // Retry logic
                if (retryCount < maxRetries)
                {
                    Debug.Log($"[ConnectionManager] Retrying... ({retryCount + 1}/{maxRetries})");
                    await Task.Delay((int)(retryDelay * 1000));
                    return await JoinRelayAllocation(joinCode, retryCount + 1);
                }

                throw;
            }
        }
#endif

        #endregion

        #region Getters

        public string GetCurrentJoinCode()
        {
            return _currentJoinCode;
        }

        public ConnectionState GetConnectionState()
        {
            return _connectionState;
        }

        public bool IsConnected()
        {
            return _connectionState == ConnectionState.Connected && NetworkManager.Singleton != null &&
                   (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient);
        }

        public bool IsHost()
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        }

        public bool IsClient()
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
        }

        #endregion

        #region Debug

        [ContextMenu("Start Local Host")]
        private void DebugStartLocalHost()
        {
            StartHostLocal();
        }

        [ContextMenu("Start Local Client")]
        private void DebugStartLocalClient()
        {
            StartClientLocal();
        }

        [ContextMenu("Disconnect")]
        private void DebugDisconnect()
        {
            Disconnect();
        }

        #endregion
    }
}
#endif // UNITY_NETCODE_GAMEOBJECTS
