// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using GravityWars.Networking;

namespace GravityWars.Multiplayer
{
    /// <summary>
    /// Server-authoritative game manager for online multiplayer matches.
    ///
    /// Features:
    /// - Match lifecycle management (lobby → playing → ended)
    /// - Player spawning and respawning
    /// - Round-based gameplay with scoring
    /// - Server-authoritative win conditions
    /// - Match statistics tracking
    /// - Anti-cheat integration
    ///
    /// Architecture:
    /// - Server controls all game state
    /// - Clients receive state updates via NetworkVariables and RPCs
    /// - Server validates all player actions
    /// - Server determines winners and applies rewards
    ///
    /// Usage:
    /// - Place in multiplayer scene
    /// - Assign spawn points
    /// - Configure match settings
    /// - Server starts match when all players ready
    /// </summary>
    public class NetworkGameManager : NetworkBehaviour
    {
        #region Singleton

        private static NetworkGameManager _instance;
        public static NetworkGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NetworkGameManager>();
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
        }

        #endregion

        #region Serialized Fields

        [Header("Match Configuration")]
        [SerializeField] private int roundsToWin = 3;
        [SerializeField] private float roundDuration = 180f; // 3 minutes
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private int maxPlayers = 2;

        [Header("Spawn Configuration")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject playerShipPrefab;

        [Header("UI References")]
        [SerializeField] private GameObject matchUICanvas;

        #endregion

        #region Network Variables

        /// <summary>
        /// Current match state.
        /// </summary>
        private NetworkVariable<MatchState> _matchState = new NetworkVariable<MatchState>(
            MatchState.Lobby,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Current round number (1-indexed).
        /// </summary>
        private NetworkVariable<int> _currentRound = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Time remaining in current round.
        /// </summary>
        private NetworkVariable<float> _roundTimeRemaining = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Serialized player scores (NetworkVariable doesn't support Dictionary directly).
        /// Format: "clientId:score,clientId:score,..."
        /// </summary>
        private NetworkVariable<NetworkString> _playerScores = new NetworkVariable<NetworkString>(
            new NetworkString(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        #endregion

        #region Match State

        public enum MatchState
        {
            Lobby,          // Waiting for players
            Starting,       // Countdown to start
            Playing,        // Round in progress
            RoundEnd,       // Round ended, showing results
            MatchEnd        // Match completed, showing final results
        }

        #endregion

        #region Player Tracking

        private Dictionary<ulong, PlayerMatchData> _players = new Dictionary<ulong, PlayerMatchData>();

        private class PlayerMatchData
        {
            public ulong clientId;
            public string playerName;
            public int roundsWon;
            public int totalKills;
            public int totalDeaths;
            public int totalDamageDealt;
            public float totalPlaytime;
            public NetworkedPlayerShip shipInstance;
            public bool isReady;
        }

        #endregion

        #region Events

        public event Action<MatchState> OnMatchStateChanged;
        public event Action<int> OnRoundStarted;
        public event Action<ulong> OnRoundWon;
        public event Action<ulong> OnMatchWon;
        public event Action<float> OnRoundTimeUpdated;

        #endregion

        #region Unity Lifecycle

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Initialize match
                _matchState.Value = MatchState.Lobby;
                _currentRound.Value = 0;

                // Listen for player connections/disconnections
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            // Subscribe to state changes
            _matchState.OnValueChanged += OnMatchStateChangedCallback;
            _roundTimeRemaining.OnValueChanged += OnRoundTimeChangedCallback;

            Debug.Log($"[NetworkGameManager] Spawned - IsServer: {IsServer}");
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            _matchState.OnValueChanged -= OnMatchStateChangedCallback;
            _roundTimeRemaining.OnValueChanged -= OnRoundTimeChangedCallback;
        }

        private void Update()
        {
            if (!IsServer) return;

            if (_matchState.Value == MatchState.Playing)
            {
                // Update round timer
                _roundTimeRemaining.Value -= Time.deltaTime;

                if (_roundTimeRemaining.Value <= 0)
                {
                    // Round timeout - check for winner
                    CheckRoundWinCondition(timeExpired: true);
                }
            }
        }

        #endregion

        #region Player Connection

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkGameManager] Client {clientId} connected");

            // Add player data
            _players[clientId] = new PlayerMatchData
            {
                clientId = clientId,
                playerName = $"Player {clientId}",
                roundsWon = 0,
                totalKills = 0,
                totalDeaths = 0,
                totalDamageDealt = 0,
                totalPlaytime = 0f,
                shipInstance = null,
                isReady = false
            };

            // Notify clients
            PlayerConnectedClientRpc(clientId, $"Player {clientId}");

            // Check if we can start (all players connected)
            if (_matchState.Value == MatchState.Lobby && _players.Count >= maxPlayers)
            {
                StartMatchCountdown();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkGameManager] Client {clientId} disconnected");

            // Remove player
            if (_players.ContainsKey(clientId))
            {
                // Despawn their ship
                if (_players[clientId].shipInstance != null)
                {
                    _players[clientId].shipInstance.NetworkObject.Despawn(destroy: true);
                }

                _players.Remove(clientId);
            }

            // Notify clients
            PlayerDisconnectedClientRpc(clientId);

            // End match if not enough players
            if (_matchState.Value == MatchState.Playing && _players.Count < 2)
            {
                EndMatch(winnerClientId: 0, reason: "Player disconnected");
            }
        }

        [ClientRpc]
        private void PlayerConnectedClientRpc(ulong clientId, string playerName)
        {
            Debug.Log($"[NetworkGameManager] Player {playerName} joined ({clientId})");
            // Update UI: show player in lobby
        }

        [ClientRpc]
        private void PlayerDisconnectedClientRpc(ulong clientId)
        {
            Debug.Log($"[NetworkGameManager] Player {clientId} left");
            // Update UI: remove player from lobby
        }

        #endregion

        #region Match Lifecycle

        private void StartMatchCountdown()
        {
            if (!IsServer) return;
            if (_matchState.Value != MatchState.Lobby) return;

            Debug.Log("[NetworkGameManager] Starting match countdown...");
            _matchState.Value = MatchState.Starting;

            StartCoroutine(MatchCountdownCoroutine());
        }

        private IEnumerator MatchCountdownCoroutine()
        {
            // 5 second countdown
            for (int i = 5; i > 0; i--)
            {
                CountdownClientRpc(i);
                yield return new WaitForSeconds(1f);
            }

            StartMatch();
        }

        [ClientRpc]
        private void CountdownClientRpc(int seconds)
        {
            Debug.Log($"[NetworkGameManager] Match starting in {seconds}...");
            // Update UI: show countdown
        }

        private void StartMatch()
        {
            if (!IsServer) return;

            Debug.Log("[NetworkGameManager] Match started!");

            _matchState.Value = MatchState.Playing;
            _currentRound.Value = 1;

            // Spawn all players
            SpawnAllPlayers();

            // Start first round
            StartRound();
        }

        private void StartRound()
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkGameManager] Round {_currentRound.Value} started");

            _roundTimeRemaining.Value = roundDuration;

            // Respawn all dead players
            foreach (var playerData in _players.Values)
            {
                if (playerData.shipInstance != null && playerData.shipInstance.IsDead())
                {
                    Vector2 spawnPos = GetSpawnPosition(playerData.clientId);
                    playerData.shipInstance.Respawn(spawnPos, UnityEngine.Random.Range(0f, 360f));
                }
            }

            // Notify clients
            RoundStartedClientRpc(_currentRound.Value);
        }

        [ClientRpc]
        private void RoundStartedClientRpc(int roundNumber)
        {
            OnRoundStarted?.Invoke(roundNumber);
            Debug.Log($"[NetworkGameManager] Round {roundNumber} started");
            // Update UI: show "Round X" banner
        }

        #endregion

        #region Player Spawning

        private void SpawnAllPlayers()
        {
            if (!IsServer) return;

            int spawnIndex = 0;
            foreach (var playerData in _players.Values)
            {
                SpawnPlayer(playerData.clientId, spawnIndex);
                spawnIndex++;
            }
        }

        private void SpawnPlayer(ulong clientId, int spawnIndex)
        {
            if (!IsServer) return;

            Vector2 spawnPos = GetSpawnPosition(spawnIndex);
            float spawnRot = UnityEngine.Random.Range(0f, 360f);

            GameObject shipObj = Instantiate(playerShipPrefab, spawnPos, Quaternion.Euler(0, 0, spawnRot));
            NetworkObject shipNetObj = shipObj.GetComponent<NetworkObject>();

            if (shipNetObj != null)
            {
                shipNetObj.SpawnWithOwnership(clientId);

                NetworkedPlayerShip ship = shipObj.GetComponent<NetworkedPlayerShip>();
                if (ship != null)
                {
                    _players[clientId].shipInstance = ship;

                    // Subscribe to death event
                    ship.OnDeath += () => OnPlayerDeath(clientId);
                }
            }

            Debug.Log($"[NetworkGameManager] Spawned ship for client {clientId} at {spawnPos}");
        }

        private Vector2 GetSpawnPosition(int index)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int spawnIndex = index % spawnPoints.Length;
                return spawnPoints[spawnIndex].position;
            }

            // Fallback: circular spawn
            float angle = (index * 360f / maxPlayers) * Mathf.Deg2Rad;
            float radius = 10f;
            return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        #endregion

        #region Win Conditions

        private void OnPlayerDeath(ulong deadClientId)
        {
            if (!IsServer) return;
            if (_matchState.Value != MatchState.Playing) return;

            Debug.Log($"[NetworkGameManager] Player {deadClientId} died");

            _players[deadClientId].totalDeaths++;

            // Find killer (whoever did last damage - would need to track this in NetworkedPlayerShip)
            // For now, just check if only one player alive

            CheckRoundWinCondition(timeExpired: false);
        }

        private void CheckRoundWinCondition(bool timeExpired)
        {
            if (!IsServer) return;
            if (_matchState.Value != MatchState.Playing) return;

            // Count alive players
            int aliveCount = 0;
            ulong lastAliveClientId = 0;

            foreach (var playerData in _players.Values)
            {
                if (playerData.shipInstance != null && !playerData.shipInstance.IsDead())
                {
                    aliveCount++;
                    lastAliveClientId = playerData.clientId;
                }
            }

            // Round win condition: only one player alive OR time expired
            if (aliveCount <= 1 || timeExpired)
            {
                ulong winnerClientId = 0;

                if (aliveCount == 1)
                {
                    // Last player alive wins
                    winnerClientId = lastAliveClientId;
                }
                else if (timeExpired)
                {
                    // Time expired - player with most health wins
                    float maxHealth = 0f;
                    foreach (var playerData in _players.Values)
                    {
                        if (playerData.shipInstance != null)
                        {
                            float health = playerData.shipInstance.GetHealth();
                            if (health > maxHealth)
                            {
                                maxHealth = health;
                                winnerClientId = playerData.clientId;
                            }
                        }
                    }
                }

                EndRound(winnerClientId);
            }
        }

        private void EndRound(ulong winnerClientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkGameManager] Round {_currentRound.Value} won by client {winnerClientId}");

            _matchState.Value = MatchState.RoundEnd;

            // Update scores
            if (_players.ContainsKey(winnerClientId))
            {
                _players[winnerClientId].roundsWon++;
                _players[winnerClientId].totalKills++;
            }

            // Serialize scores
            SerializePlayerScores();

            // Notify clients
            RoundEndedClientRpc(winnerClientId, _currentRound.Value);

            // Check for match win
            bool matchWon = false;
            foreach (var playerData in _players.Values)
            {
                if (playerData.roundsWon >= roundsToWin)
                {
                    matchWon = true;
                    EndMatch(playerData.clientId, "Rounds won");
                    break;
                }
            }

            // Start next round if match not over
            if (!matchWon)
            {
                StartCoroutine(NextRoundDelayCoroutine());
            }
        }

        [ClientRpc]
        private void RoundEndedClientRpc(ulong winnerClientId, int roundNumber)
        {
            OnRoundWon?.Invoke(winnerClientId);
            Debug.Log($"[NetworkGameManager] Round {roundNumber} won by {winnerClientId}");
            // Update UI: show round result
        }

        private IEnumerator NextRoundDelayCoroutine()
        {
            yield return new WaitForSeconds(5f);

            _currentRound.Value++;
            _matchState.Value = MatchState.Playing;
            StartRound();
        }

        private void EndMatch(ulong winnerClientId, string reason)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkGameManager] Match ended - Winner: {winnerClientId}, Reason: {reason}");

            _matchState.Value = MatchState.MatchEnd;

            // Calculate final stats
            MatchResult result = new MatchResult
            {
                winnerClientId = winnerClientId,
                totalRounds = _currentRound.Value,
                matchDuration = Time.time, // Would track actual match start time
                reason = reason
            };

            // Notify clients
            MatchEndedClientRpc(winnerClientId, reason);

            // Award rewards (economy, XP, achievements, etc.)
            AwardMatchRewards(result);
        }

        [ClientRpc]
        private void MatchEndedClientRpc(ulong winnerClientId, string reason)
        {
            OnMatchWon?.Invoke(winnerClientId);
            Debug.Log($"[NetworkGameManager] Match won by {winnerClientId} ({reason})");
            // Update UI: show victory/defeat screen
        }

        private struct MatchResult
        {
            public ulong winnerClientId;
            public int totalRounds;
            public float matchDuration;
            public string reason;
        }

        #endregion

        #region Rewards & Stats

        private void AwardMatchRewards(MatchResult result)
        {
            if (!IsServer) return;

            foreach (var playerData in _players.Values)
            {
                bool isWinner = playerData.clientId == result.winnerClientId;

                // Calculate rewards
                int softCurrency = isWinner ? 500 : 200;
                int hardCurrency = isWinner ? 10 : 0;
                int xp = isWinner ? 1000 : 500;

                // Bonus for kills
                softCurrency += playerData.totalKills * 50;
                xp += playerData.totalKills * 100;

                // Send rewards to client
                AwardRewardsClientRpc(
                    new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { playerData.clientId }
                        }
                    },
                    softCurrency,
                    hardCurrency,
                    xp,
                    isWinner
                );

                // Track analytics
                // AnalyticsService.Instance.TrackMatchCompleted(
                //     isWin: isWinner,
                //     duration: (int)result.matchDuration,
                //     damageDealt: playerData.totalDamageDealt,
                //     missilesHit: 0, // Would need to track
                //     missilesFired: 0 // Would need to track
                // );
            }
        }

        [ClientRpc]
        private void AwardRewardsClientRpc(ClientRpcParams clientRpcParams, int softCurrency, int hardCurrency, int xp, bool isWinner)
        {
            Debug.Log($"[NetworkGameManager] Rewards: {softCurrency}c, {hardCurrency}g, {xp}xp (Win: {isWinner})");

            // Update local economy
            // EconomyService.Instance.AddSoftCurrency(softCurrency);
            // EconomyService.Instance.AddHardCurrency(hardCurrency);
            // ProgressionManager.Instance.AddXP(xp);

            // Update save
            // SaveManager.Instance.SaveGameAsync();
        }

        #endregion

        #region Score Serialization

        private void SerializePlayerScores()
        {
            if (!IsServer) return;

            string serialized = "";
            foreach (var playerData in _players.Values)
            {
                if (serialized.Length > 0) serialized += ",";
                serialized += $"{playerData.clientId}:{playerData.roundsWon}";
            }

            _playerScores.Value = new NetworkString(serialized);
        }

        public Dictionary<ulong, int> GetPlayerScores()
        {
            Dictionary<ulong, int> scores = new Dictionary<ulong, int>();

            string serialized = _playerScores.Value.ToString();
            if (string.IsNullOrEmpty(serialized)) return scores;

            string[] entries = serialized.Split(',');
            foreach (string entry in entries)
            {
                string[] parts = entry.Split(':');
                if (parts.Length == 2)
                {
                    ulong clientId = ulong.Parse(parts[0]);
                    int score = int.Parse(parts[1]);
                    scores[clientId] = score;
                }
            }

            return scores;
        }

        #endregion

        #region Network Variable Callbacks

        private void OnMatchStateChangedCallback(MatchState previousValue, MatchState newValue)
        {
            OnMatchStateChanged?.Invoke(newValue);
            Debug.Log($"[NetworkGameManager] Match state changed: {previousValue} → {newValue}");
        }

        private void OnRoundTimeChangedCallback(float previousValue, float newValue)
        {
            OnRoundTimeUpdated?.Invoke(newValue);
        }

        #endregion

        #region Public API

        public MatchState GetMatchState()
        {
            return _matchState.Value;
        }

        public int GetCurrentRound()
        {
            return _currentRound.Value;
        }

        public float GetRoundTimeRemaining()
        {
            return _roundTimeRemaining.Value;
        }

        #endregion
    }

    /// <summary>
    /// NetworkString for serializing strings in NetworkVariables.
    /// </summary>
    public struct NetworkString : INetworkSerializable
    {
        private string _value;

        public NetworkString(string value)
        {
            _value = value ?? "";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _value);
        }

        public override string ToString()
        {
            return _value;
        }

        public static implicit operator string(NetworkString s) => s.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString(s);
    }
}
