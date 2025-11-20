using UnityEngine;
// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;

#if UNITY_NETCODE_GAMEOBJECTS
namespace GravityWars.Networking
{
    /// <summary>
    /// Central network manager for Gravity Wars multiplayer.
    /// Orchestrates connection, lobby management, and match synchronization.
    ///
    /// This extends Unity's NetworkManager to add game-specific logic.
    ///
    /// Usage:
    ///   - Attach to a GameObject in the scene
    ///   - Configure NetworkManager settings in Inspector
    ///   - Use LobbyManager to start/join matches
    /// </summary>
    [RequireComponent(typeof(Unity.Netcode.NetworkManager))]
    public class GravityWarsNetworkManager : MonoBehaviour
    {
        #region Singleton

        private static GravityWarsNetworkManager _instance;
        public static GravityWarsNetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GravityWarsNetworkManager>();
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Network Configuration")]
        [Tooltip("Enable network debug logging")]
        public bool debugLogging = true;

        [Tooltip("Max players per match (always 2 for Gravity Wars)")]
        public int maxPlayersPerMatch = 2;

        #endregion

        #region State

        private Unity.Netcode.NetworkManager _networkManager;
        private bool _isMatchInProgress = false;

        public bool IsMatchInProgress => _isMatchInProgress;
        public bool IsConnected => _networkManager != null && _networkManager.IsConnectedClient;
        public bool IsHost => _networkManager != null && _networkManager.IsHost;
        public bool IsClient => _networkManager != null && _networkManager.IsClient && !_networkManager.IsHost;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Enforce singleton
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Get Unity NetworkManager component
            _networkManager = GetComponent<Unity.Netcode.NetworkManager>();

            if (_networkManager == null)
            {
                Debug.LogError("[NetworkManager] Unity.Netcode.NetworkManager component not found!");
                return;
            }

            // Subscribe to network events
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            _networkManager.OnServerStarted += OnServerStarted;

            Log("Initialized");
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
                _networkManager.OnServerStarted -= OnServerStarted;
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Network Events

        private void OnClientConnected(ulong clientId)
        {
            Log($"Client connected: {clientId}");

            if (IsHost)
            {
                // Host perspective: client joined
                Log($"Player {clientId} joined the match");

                // Check if match is ready to start (2 players connected)
                if (_networkManager.ConnectedClientsList.Count >= maxPlayersPerMatch)
                {
                    Log("Match ready - both players connected!");
                    OnMatchReady();
                }
            }
            else if (clientId == _networkManager.LocalClientId)
            {
                // Client perspective: we connected successfully
                Log("Successfully connected to host!");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Log($"Client disconnected: {clientId}");

            if (clientId == _networkManager.LocalClientId)
            {
                // We disconnected
                Log("Disconnected from match");
                OnLocalPlayerDisconnected();
            }
            else if (IsHost)
            {
                // Host perspective: opponent left
                Log("Opponent disconnected");
                OnOpponentDisconnected(clientId);
            }
        }

        private void OnServerStarted()
        {
            Log("Server started - waiting for players...");
        }

        #endregion

        #region Match Lifecycle

        /// <summary>
        /// Called when both players are connected and match is ready to start.
        /// </summary>
        private void OnMatchReady()
        {
            _isMatchInProgress = true;

            // Notify MatchManager to start the game
            var matchManager = FindObjectOfType<MatchManager>();
            if (matchManager != null)
            {
                matchManager.StartNetworkedMatch();
            }
            else
            {
                Debug.LogWarning("[NetworkManager] MatchManager not found - cannot start match");
            }
        }

        /// <summary>
        /// Called when local player disconnects from match.
        /// </summary>
        private void OnLocalPlayerDisconnected()
        {
            _isMatchInProgress = false;

            // Return to main menu
            // (Implement menu navigation here)
            Debug.Log("[NetworkManager] Returning to main menu...");
        }

        /// <summary>
        /// Called when opponent disconnects during match.
        /// </summary>
        private void OnOpponentDisconnected(ulong clientId)
        {
            if (_isMatchInProgress)
            {
                // Pause game and show disconnect UI
                Time.timeScale = 0f;

                // Give opponent 30 seconds to reconnect
                // (Implement reconnection logic here)
                Debug.LogWarning($"[NetworkManager] Opponent disconnected - waiting for reconnect...");
            }
        }

        /// <summary>
        /// Ends the current match and returns players to lobby.
        /// </summary>
        public void EndMatch()
        {
            _isMatchInProgress = false;

            Log("Match ended");

            // Disconnect from network
            if (IsHost)
            {
                _networkManager.Shutdown();
            }
            else if (IsClient)
            {
                _networkManager.Shutdown();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the opponent's client ID.
        /// </summary>
        public ulong GetOpponentClientId()
        {
            foreach (var client in _networkManager.ConnectedClientsList)
            {
                if (client.ClientId != _networkManager.LocalClientId)
                {
                    return client.ClientId;
                }
            }

            return ulong.MaxValue; // No opponent found
        }

        /// <summary>
        /// Returns true if both players are connected.
        /// </summary>
        public bool AreBothPlayersConnected()
        {
            return _networkManager != null && _networkManager.ConnectedClientsList.Count >= maxPlayersPerMatch;
        }

        #endregion

        #region Helper Methods

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[GravityWarsNetworkManager] {message}");
        }

        #endregion
    }
}
#endif // UNITY_NETCODE_GAMEOBJECTS
