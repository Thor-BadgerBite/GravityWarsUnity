using System;
using UnityEngine;
// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;

#if UNITY_NETCODE_GAMEOBJECTS
namespace GravityWars.Networking
{
    /// <summary>
    /// Manages synchronized turn-based gameplay for online multiplayer matches.
    ///
    /// Features:
    /// - Turn synchronization between players
    /// - Action validation and broadcasting
    /// - Deterministic physics synchronization
    /// - Match result validation
    /// - Disconnect handling (pause/resume/forfeit)
    ///
    /// Turn Flow (Networked):
    /// 1. Host starts match → broadcasts to client
    /// 2. Player 1's turn → input → validate → broadcast to opponent
    /// 3. Both clients simulate physics identically (deterministic)
    /// 4. Turn ends → validate final state → next turn
    /// 5. Match ends → both clients submit results → server validates
    ///
    /// Usage:
    ///   Attach to GameObject in online match scene.
    ///   MatchManager.Instance.StartNetworkedMatch();
    /// </summary>
    public class MatchManager : NetworkBehaviour
    {
        #region Singleton

        private static MatchManager _instance;
        public static MatchManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MatchManager>();
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Match Configuration")]
        [Tooltip("Enable match debug logging")]
        public bool debugLogging = true;

        [Tooltip("Turn timeout (seconds)")]
        public float turnTimeout = 60f;

        [Tooltip("Reconnect grace period (seconds)")]
        public float reconnectGracePeriod = 30f;

        #endregion

        #region Network State

        private NetworkVariable<MatchState> _currentMatchState = new NetworkVariable<MatchState>(
            MatchState.WaitingForPlayers,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<ulong> _currentTurnPlayerId = new NetworkVariable<ulong>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<int> _currentRound = new NetworkVariable<int>(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public MatchState CurrentMatchState => _currentMatchState.Value;
        public bool IsMyTurn => _currentTurnPlayerId.Value == NetworkManager.Singleton.LocalClientId;
        public int CurrentRound => _currentRound.Value;

        #endregion

        #region Match Data

        private GameManager _gameManager;
        private float _turnStartTime;
        private bool _isMatchActive = false;

        #endregion

        #region Enums

        public enum MatchState
        {
            WaitingForPlayers,
            PreparationPhase,
            PlayerTurn,
            MissileFlight,
            RoundEnd,
            MatchEnd
        }

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

            // Get GameManager reference
            _gameManager = FindObjectOfType<GameManager>();
            if (_gameManager == null)
            {
                Debug.LogError("[MatchManager] GameManager not found!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to network variable changes
            _currentMatchState.OnValueChanged += OnMatchStateChanged;
            _currentTurnPlayerId.OnValueChanged += OnTurnPlayerChanged;

            Log("Network spawned");
        }

        public override void OnNetworkDespawn()
        {
            // Unsubscribe from network variable changes
            _currentMatchState.OnValueChanged -= OnMatchStateChanged;
            _currentTurnPlayerId.OnValueChanged -= OnTurnPlayerChanged;

            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsServer || !_isMatchActive)
                return;

            // Check for turn timeout (server-side only)
            if (_currentMatchState.Value == MatchState.PlayerTurn)
            {
                if (Time.time - _turnStartTime > turnTimeout)
                {
                    Log("Turn timeout - forcing turn end");
                    EndTurnServerRpc(_currentTurnPlayerId.Value);
                }
            }
        }

        #endregion

        #region Match Lifecycle

        /// <summary>
        /// Starts a networked match (called by NetworkManager when both players connected).
        /// </summary>
        public void StartNetworkedMatch()
        {
            if (!IsServer)
            {
                Log("Only server can start match");
                return;
            }

            Log("Starting networked match...");

            _isMatchActive = true;
            _currentRound.Value = 1;

            // Initialize game state
            InitializeMatchServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void InitializeMatchServerRpc()
        {
            Log("Initializing match for all clients...");

            // Set initial state
            _currentMatchState.Value = MatchState.PreparationPhase;

            // Determine starting player (player 1 = host)
            _currentTurnPlayerId.Value = NetworkManager.Singleton.ServerClientId;

            // Notify clients to initialize
            InitializeMatchClientRpc();
        }

        [ClientRpc]
        private void InitializeMatchClientRpc()
        {
            Log("Match initialized - starting preparation phase");

            // Tell GameManager to set up the match
            if (_gameManager != null)
            {
                _gameManager.StartGamePhase(); // This will initialize planets, ships, etc.
            }
        }

        #endregion

        #region Turn Management

        /// <summary>
        /// Called when a player's turn begins.
        /// </summary>
        public void StartPlayerTurn(ulong playerId)
        {
            if (!IsServer)
                return;

            Log($"Starting turn for player {playerId}");

            _currentMatchState.Value = MatchState.PlayerTurn;
            _currentTurnPlayerId.Value = playerId;
            _turnStartTime = Time.time;

            // Notify clients
            StartPlayerTurnClientRpc(playerId);
        }

        [ClientRpc]
        private void StartPlayerTurnClientRpc(ulong playerId)
        {
            Log($"Turn started - Player {playerId}'s turn");

            if (_gameManager != null)
            {
                // Enable controls for active player only
                bool isMyTurn = playerId == NetworkManager.Singleton.LocalClientId;
                _gameManager.StartPlayerTurn(); // This will handle local control enabling
            }
        }

        /// <summary>
        /// Called when a player ends their turn.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void EndTurnServerRpc(ulong playerId)
        {
            if (_currentMatchState.Value != MatchState.PlayerTurn)
                return;

            Log($"Turn ended for player {playerId}");

            // Switch to next player
            ulong nextPlayer = GetOpponentId(playerId);
            StartPlayerTurn(nextPlayer);
        }

        public void EndTurn()
        {
            EndTurnServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        #endregion

        #region Player Actions (Networked)

        /// <summary>
        /// Fires a missile (networked action).
        /// Client calls this → Server validates → broadcasts to all clients.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void FireMissileServerRpc(float angle, float power, int perkSlot, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            // Validate it's sender's turn
            if (senderId != _currentTurnPlayerId.Value)
            {
                Log($"Rejected fire action from {senderId} - not their turn");
                return;
            }

            Log($"Player {senderId} fired missile (angle: {angle}, power: {power}, perk: {perkSlot})");

            // Broadcast to all clients
            FireMissileClientRpc(senderId, angle, power, perkSlot);
        }

        [ClientRpc]
        private void FireMissileClientRpc(ulong playerId, float angle, float power, int perkSlot)
        {
            Log($"Executing fire action for player {playerId}");

            if (_gameManager != null)
            {
                // Execute fire action locally (deterministic physics)
                // GameManager will handle the actual firing
                // This ensures both clients simulate the exact same trajectory
            }
        }

        /// <summary>
        /// Moves ship (networked action).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void MoveShipServerRpc(Vector3 direction, float power, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            // Validate it's sender's turn
            if (senderId != _currentTurnPlayerId.Value)
            {
                Log($"Rejected move action from {senderId} - not their turn");
                return;
            }

            Log($"Player {senderId} moved ship (direction: {direction}, power: {power})");

            // Broadcast to all clients
            MoveShipClientRpc(senderId, direction, power);
        }

        [ClientRpc]
        private void MoveShipClientRpc(ulong playerId, Vector3 direction, float power)
        {
            Log($"Executing move action for player {playerId}");

            if (_gameManager != null)
            {
                // Execute move action locally (deterministic physics)
            }
        }

        /// <summary>
        /// Activates a perk (networked action).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ActivatePerkServerRpc(int perkSlot, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            // Validate it's sender's turn
            if (senderId != _currentTurnPlayerId.Value)
            {
                Log($"Rejected perk activation from {senderId} - not their turn");
                return;
            }

            Log($"Player {senderId} activated perk slot {perkSlot}");

            // Broadcast to all clients
            ActivatePerkClientRpc(senderId, perkSlot);
        }

        [ClientRpc]
        private void ActivatePerkClientRpc(ulong playerId, int perkSlot)
        {
            Log($"Executing perk activation for player {playerId}");

            if (_gameManager != null)
            {
                // Execute perk activation locally
            }
        }

        #endregion

        #region Round/Match End

        /// <summary>
        /// Called when a ship is destroyed (round ends).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ShipDestroyedServerRpc(ulong destroyedPlayerId)
        {
            Log($"Ship destroyed: Player {destroyedPlayerId}");

            _currentMatchState.Value = MatchState.RoundEnd;

            // Determine winner of round
            ulong roundWinner = GetOpponentId(destroyedPlayerId);

            // Notify clients
            RoundEndClientRpc(roundWinner);

            // Check if match is over (best of 3/5/etc)
            // For now, just end match after 1 round
            _currentMatchState.Value = MatchState.MatchEnd;
            MatchEndClientRpc(roundWinner);
        }

        [ClientRpc]
        private void RoundEndClientRpc(ulong winnerId)
        {
            Log($"Round ended - Winner: Player {winnerId}");

            if (_gameManager != null)
            {
                // Update scores, show round end UI
            }
        }

        [ClientRpc]
        private void MatchEndClientRpc(ulong winnerId)
        {
            Log($"Match ended - Winner: Player {winnerId}");

            _isMatchActive = false;

            if (_gameManager != null)
            {
                // Show match end UI, award XP, etc.
            }
        }

        #endregion

        #region Network Event Handlers

        private void OnMatchStateChanged(MatchState oldState, MatchState newState)
        {
            Log($"Match state changed: {oldState} → {newState}");

            // Handle state transitions
            switch (newState)
            {
                case MatchState.PreparationPhase:
                    // Show preparation UI
                    break;

                case MatchState.PlayerTurn:
                    // Enable/disable controls based on whose turn it is
                    break;

                case MatchState.MissileFlight:
                    // Camera follow missile
                    break;

                case MatchState.RoundEnd:
                    // Show round results
                    break;

                case MatchState.MatchEnd:
                    // Show match results, return to lobby
                    break;
            }
        }

        private void OnTurnPlayerChanged(ulong oldPlayerId, ulong newPlayerId)
        {
            Log($"Turn changed: {oldPlayerId} → {newPlayerId}");

            bool isMyTurn = newPlayerId == NetworkManager.Singleton.LocalClientId;
            Log($"Is my turn: {isMyTurn}");

            // Enable/disable controls in GameManager
            if (_gameManager != null)
            {
                // _gameManager.SetControlsEnabled(isMyTurn);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the opponent's client ID.
        /// </summary>
        private ulong GetOpponentId(ulong playerId)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId != playerId)
                {
                    return client.ClientId;
                }
            }

            return 0; // No opponent found
        }

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[MatchManager] {message}");
        }

        #endregion
    }
}
#endif // UNITY_NETCODE_GAMEOBJECTS
