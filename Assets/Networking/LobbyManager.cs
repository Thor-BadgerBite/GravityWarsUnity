using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Services.Lobbies;
// using Unity.Services.Lobbies.Models;
// using Unity.Services.Relay;

namespace GravityWars.Networking
{
    /// <summary>
    /// Manages lobby creation, matchmaking, and player joining for online multiplayer.
    ///
    /// Features:
    /// - Quick Match (auto-matchmaking)
    /// - Custom Lobby Creation
    /// - Lobby Browse/Join
    /// - Lobby Heartbeat (keep-alive)
    /// - Relay Integration
    ///
    /// Usage:
    ///   await LobbyManager.Instance.QuickMatch();
    ///   await LobbyManager.Instance.CreateLobby("My Lobby");
    ///   await LobbyManager.Instance.JoinLobby(lobbyCode);
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        #region Singleton

        private static LobbyManager _instance;
        public static LobbyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LobbyManager>();
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Lobby Configuration")]
        [Tooltip("Maximum players per lobby (always 2 for Gravity Wars)")]
        public int maxPlayers = 2;

        [Tooltip("Lobby name for quick match")]
        public string quickMatchLobbyName = "Gravity Wars Match";

        [Tooltip("Lobby heartbeat interval (seconds)")]
        public float lobbyHeartbeatInterval = 15f;

        [Tooltip("Enable lobby debug logging")]
        public bool debugLogging = true;

        #endregion

        #region State

        private Lobby _currentLobby;
        private float _lastHeartbeatTime;
        private bool _isInLobby = false;

        public bool IsInLobby => _isInLobby;
        public Lobby CurrentLobby => _currentLobby;
        public bool IsLobbyHost => _currentLobby != null && _currentLobby.HostId == GetPlayerId();

        #endregion

        #region Events

        public event Action<Lobby> OnLobbyCreated;
        public event Action<Lobby> OnLobbyJoined;
        public event Action OnLobbyLeft;
        public event Action<Player> OnPlayerJoined;
        public event Action<Player> OnPlayerLeft;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Log("Initialized");
        }

        private void Update()
        {
            // Send lobby heartbeat to keep lobby alive
            if (_isInLobby && IsLobbyHost)
            {
                if (Time.time - _lastHeartbeatTime > lobbyHeartbeatInterval)
                {
                    SendLobbyHeartbeat();
                    _lastHeartbeatTime = Time.time;
                }
            }
        }

        private void OnDestroy()
        {
            // Leave lobby when destroyed
            if (_isInLobby)
            {
                LeaveLobby();
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            // Clean up lobby on quit
            if (_isInLobby)
            {
                LeaveLobby();
            }
        }

        #endregion

        #region Quick Match

        /// <summary>
        /// Quick match: Finds an existing lobby or creates a new one.
        /// This is the primary matchmaking method for online PvP.
        /// </summary>
        public async Task<bool> QuickMatch()
        {
            Log("Quick Match: Searching for available lobbies...");

            try
            {
                // Try to find existing lobbies with space
                var lobbies = await FindAvailableLobbies();

                if (lobbies != null && lobbies.Count > 0)
                {
                    // Join first available lobby
                    Log($"Found {lobbies.Count} available lobbies - joining first one");
                    return await JoinLobby(lobbies[0].Id);
                }
                else
                {
                    // No lobbies found - create new one
                    Log("No available lobbies - creating new lobby");
                    return await CreateLobby(quickMatchLobbyName, isPrivate: false);
                }
            }
            catch (Exception e)
            {
                LogError($"Quick Match failed: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Create Lobby

        /// <summary>
        /// Creates a new lobby and starts as host.
        /// </summary>
        /// <param name="lobbyName">Display name for the lobby</param>
        /// <param name="isPrivate">If true, lobby is invite-only (not shown in browse)</param>
        /// <returns>True if lobby created successfully</returns>
        public async Task<bool> CreateLobby(string lobbyName, bool isPrivate = false)
        {
            Log($"Creating lobby: {lobbyName} (Private: {isPrivate})");

            try
            {
                // Create lobby options
                var options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = GetLocalPlayerData()
                };

                // Create lobby
                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                _isInLobby = true;

                Log($"Lobby created successfully! Lobby Code: {_currentLobby.LobbyCode}");

                // Invoke event
                OnLobbyCreated?.Invoke(_currentLobby);

                // Start as network host
                await StartAsHost();

                return true;
            }
            catch (Exception e)
            {
                LogError($"Failed to create lobby: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Join Lobby

        /// <summary>
        /// Joins an existing lobby by lobby ID.
        /// </summary>
        public async Task<bool> JoinLobby(string lobbyId)
        {
            Log($"Joining lobby: {lobbyId}");

            try
            {
                var options = new JoinLobbyByIdOptions
                {
                    Player = GetLocalPlayerData()
                };

                _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
                _isInLobby = true;

                Log($"Joined lobby successfully! Lobby: {_currentLobby.Name}");

                // Invoke event
                OnLobbyJoined?.Invoke(_currentLobby);

                // Start as network client
                await StartAsClient();

                return true;
            }
            catch (Exception e)
            {
                LogError($"Failed to join lobby: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Joins an existing lobby by lobby code.
        /// </summary>
        public async Task<bool> JoinLobbyByCode(string lobbyCode)
        {
            Log($"Joining lobby by code: {lobbyCode}");

            try
            {
                var options = new JoinLobbyByCodeOptions
                {
                    Player = GetLocalPlayerData()
                };

                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
                _isInLobby = true;

                Log($"Joined lobby successfully! Lobby: {_currentLobby.Name}");

                // Invoke event
                OnLobbyJoined?.Invoke(_currentLobby);

                // Start as network client
                await StartAsClient();

                return true;
            }
            catch (Exception e)
            {
                LogError($"Failed to join lobby by code: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Leave Lobby

        /// <summary>
        /// Leaves the current lobby.
        /// </summary>
        public async void LeaveLobby()
        {
            if (!_isInLobby || _currentLobby == null)
                return;

            Log("Leaving lobby...");

            try
            {
                string playerId = GetPlayerId();

                if (IsLobbyHost)
                {
                    // Host: Delete lobby
                    await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                    Log("Lobby deleted (host left)");
                }
                else
                {
                    // Client: Remove self from lobby
                    await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, playerId);
                    Log("Left lobby");
                }

                _currentLobby = null;
                _isInLobby = false;

                // Invoke event
                OnLobbyLeft?.Invoke();

                // Disconnect from network
                ServiceLocator.Instance.Network.Disconnect();
            }
            catch (Exception e)
            {
                LogError($"Failed to leave lobby: {e.Message}");
            }
        }

        #endregion

        #region Lobby Heartbeat

        /// <summary>
        /// Sends heartbeat to keep lobby alive (host only).
        /// Lobbies auto-delete after 30 seconds without heartbeat.
        /// </summary>
        private async void SendLobbyHeartbeat()
        {
            if (!_isInLobby || _currentLobby == null || !IsLobbyHost)
                return;

            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
            catch (Exception e)
            {
                LogError($"Heartbeat failed: {e.Message}");
            }
        }

        #endregion

        #region Lobby Browse

        /// <summary>
        /// Finds available public lobbies with space.
        /// </summary>
        private async Task<List<Lobby>> FindAvailableLobbies()
        {
            try
            {
                var options = new QueryLobbiesOptions
                {
                    Count = 10, // Get up to 10 lobbies
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) // Has available slots
                    }
                };

                var response = await LobbyService.Instance.QueryLobbiesAsync(options);
                return response.Results;
            }
            catch (Exception e)
            {
                LogError($"Failed to query lobbies: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Network Integration

        /// <summary>
        /// Starts as network host using Unity Relay.
        /// </summary>
        private async Task StartAsHost()
        {
            Log("Starting as host...");

            try
            {
                // Start host via NetworkService
                string joinCode = await ServiceLocator.Instance.Network.StartHost();

                if (joinCode != null)
                {
                    // Update lobby with relay join code
                    await UpdateLobbyRelayCode(joinCode);
                    Log($"Host started with join code: {joinCode}");
                }
                else
                {
                    LogError("Failed to start host");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to start as host: {e.Message}");
            }
        }

        /// <summary>
        /// Starts as network client and connects to host using relay code from lobby.
        /// </summary>
        private async Task StartAsClient()
        {
            Log("Starting as client...");

            try
            {
                // Get relay join code from lobby data
                string relayCode = GetRelayCodeFromLobby();

                if (relayCode != null)
                {
                    // Connect to host via NetworkService
                    bool success = await ServiceLocator.Instance.Network.JoinAsClient(relayCode);

                    if (success)
                    {
                        Log("Client connected successfully");
                    }
                    else
                    {
                        LogError("Failed to connect as client");
                    }
                }
                else
                {
                    LogError("Relay code not found in lobby data");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to start as client: {e.Message}");
            }
        }

        /// <summary>
        /// Updates lobby with relay join code (for clients to connect).
        /// </summary>
        private async Task UpdateLobbyRelayCode(string relayCode)
        {
            try
            {
                var options = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
                    }
                };

                _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, options);
                Log("Lobby updated with relay code");
            }
            catch (Exception e)
            {
                LogError($"Failed to update lobby with relay code: {e.Message}");
            }
        }

        /// <summary>
        /// Gets relay join code from current lobby data.
        /// </summary>
        private string GetRelayCodeFromLobby()
        {
            if (_currentLobby?.Data != null && _currentLobby.Data.ContainsKey("RelayCode"))
            {
                return _currentLobby.Data["RelayCode"].Value;
            }

            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets local player data for lobby.
        /// </summary>
        private Player GetLocalPlayerData()
        {
            var data = new Dictionary<string, PlayerDataObject>();

            // Add player display name
            var playerData = ProgressionManager.Instance?.currentPlayerData;
            if (playerData != null)
            {
                data["DisplayName"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerData.username);
                data["AccountLevel"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerData.accountLevel.ToString());
            }

            return new Player(GetPlayerId(), null, data);
        }

        /// <summary>
        /// Gets current player ID from authentication service.
        /// </summary>
        private string GetPlayerId()
        {
            return ServiceLocator.Instance.GetPlayerId() ?? "Unknown";
        }

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[LobbyManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[LobbyManager] {message}");
        }

        #endregion
    }
}
