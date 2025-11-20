// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;
using UnityEngine;
using GravityWars.Multiplayer;
using System.Collections;

/// <summary>
/// Coordinates turn-based gameplay in deterministic peer-to-peer multiplayer.
/// Server manages turn state machine, timing, and validation.
/// Clients execute game logic locally with synchronized inputs.
/// </summary>
public class NetworkTurnCoordinator : NetworkBehaviour
{
    #region Singleton

    public static NetworkTurnCoordinator Instance { get; private set; }

    #endregion

    #region Configuration

    [Header("Turn Timing")]
    [SerializeField] private float preparationTime = 3f;
    [SerializeField] private float turnDuration = 15f;
    [SerializeField] private float maxMissileFlightTime = 30f;

    [Header("Match Settings")]
    [SerializeField] private int winningScore = 3;  // Best of 5

    #endregion

    #region Network Variables

    private NetworkVariable<TurnStateChange.Phase> _currentPhase = new NetworkVariable<TurnStateChange.Phase>(
        TurnStateChange.Phase.GameStart,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<ulong> _currentPlayerId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<int> _roundNumber = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> _phaseTimeRemaining = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<int> _player1Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<int> _player2Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    #endregion

    #region State

    private ulong _player1Id;
    private ulong _player2Id;
    private uint _currentTick = 0;
    private Coroutine _phaseTimerCoroutine;
    private bool _isMatchActive = false;

    #endregion

    #region Properties

    public TurnStateChange.Phase CurrentPhase => _currentPhase.Value;
    public ulong CurrentPlayerId => _currentPlayerId.Value;
    public int RoundNumber => _roundNumber.Value;
    public float PhaseTimeRemaining => _phaseTimeRemaining.Value;
    public int Player1Score => _player1Score.Value;
    public int Player2Score => _player2Score.Value;
    public bool IsMatchActive => _isMatchActive;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            // Subscribe to phase changes
            _currentPhase.OnValueChanged += OnPhaseChanged;
            _currentPlayerId.OnValueChanged += OnCurrentPlayerChanged;
            _roundNumber.OnValueChanged += OnRoundChanged;
        }

        Debug.Log($"[TurnCoordinator] Spawned - IsServer: {IsServer}");
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _currentPhase.OnValueChanged -= OnPhaseChanged;
            _currentPlayerId.OnValueChanged -= OnCurrentPlayerChanged;
            _roundNumber.OnValueChanged -= OnRoundChanged;
        }

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer || !_isMatchActive) return;

        // Server updates phase timer
        if (_phaseTimeRemaining.Value > 0f)
        {
            _phaseTimeRemaining.Value -= Time.deltaTime;

            if (_phaseTimeRemaining.Value <= 0f)
            {
                OnPhaseTimerExpired();
            }
        }

        _currentTick++;
    }

    #endregion

    #region Match Control - Server Only

    /// <summary>
    /// Server: Initialize match with player IDs
    /// </summary>
    public void InitializeMatch(ulong player1Id, ulong player2Id)
    {
        if (!IsServer)
        {
            Debug.LogError("[TurnCoordinator] Only server can initialize match!");
            return;
        }

        _player1Id = player1Id;
        _player2Id = player2Id;
        _roundNumber.Value = 1;
        _player1Score.Value = 0;
        _player2Score.Value = 0;
        _isMatchActive = true;

        Debug.Log($"[TurnCoordinator] Match initialized - P1: {player1Id}, P2: {player2Id}");

        // Tell clients about player assignments
        NotifyPlayerAssignmentsClientRpc(player1Id, player2Id);

        // Start match initialization sequence
        StartCoroutine(StartMatchSequence());
    }

    private IEnumerator StartMatchSequence()
    {
        // Brief delay for clients to initialize
        yield return new WaitForSeconds(0.5f);

        // Generate deterministic arena
        InitializeArena();

        // Another brief delay for arena setup
        yield return new WaitForSeconds(1f);

        // Start with player 1
        StartPreparationPhase(_player1Id);
    }

    /// <summary>
    /// Server: Generate and broadcast arena setup (planets, ship spawns)
    /// </summary>
    private void InitializeArena()
    {
        if (!IsServer) return;

        // Generate random seed from current time
        int randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        Debug.Log($"[TurnCoordinator] Generating arena with seed: {randomSeed}");

        // Get adapter for planet generation
        var adapter = GameManager.Instance?.GetComponent<GameManagerNetworkAdapter>();
        if (adapter == null)
        {
            Debug.LogError("[TurnCoordinator] GameManagerNetworkAdapter not found!");
            return;
        }

        // Server generates planets using seed
        GravityWars.Multiplayer.PlanetSpawnData[] planetData = adapter.GenerateDeterministicPlanets(randomSeed);

        Debug.Log($"[TurnCoordinator] Generated {planetData.Length} planets");

        // Calculate ship spawn positions (opposite sides of arena)
        Vector3 player1SpawnPos = new Vector3(-25f, 0f, 0f);  // Left side
        Vector3 player2SpawnPos = new Vector3(25f, 0f, 0f);   // Right side

        Quaternion player1SpawnRot = Quaternion.Euler(0, 90, -90);   // Facing right
        Quaternion player2SpawnRot = Quaternion.Euler(0, -90, -90);  // Facing left

        // Create match init data
        var matchData = new GravityWars.Multiplayer.MatchInitData
        {
            randomSeed = randomSeed,
            player1Id = _player1Id,
            player2Id = _player2Id,
            player1SpawnPos = player1SpawnPos,
            player2SpawnPos = player2SpawnPos,
            player1SpawnRot = player1SpawnRot,
            player2SpawnRot = player2SpawnRot,
            winningScore = winningScore
        };

        // Broadcast match initialization to all clients
        InitializeArenaClientRpc(matchData, planetData);

        Debug.Log($"[TurnCoordinator] Arena initialization broadcast complete");
    }

    /// <summary>
    /// Server: Start preparation phase for a player
    /// </summary>
    private void StartPreparationPhase(ulong playerId)
    {
        if (!IsServer) return;

        _currentPlayerId.Value = playerId;
        _currentPhase.Value = TurnStateChange.Phase.PreparationPhase;
        _phaseTimeRemaining.Value = preparationTime;

        var state = new TurnStateChange
        {
            phase = TurnStateChange.Phase.PreparationPhase,
            currentPlayerId = playerId,
            phaseDuration = preparationTime,
            roundNumber = _roundNumber.Value,
            tick = _currentTick
        };

        BroadcastTurnStateClientRpc(state);

        Debug.Log($"[TurnCoordinator] Preparation phase started for player {playerId}");
    }

    /// <summary>
    /// Server: Start player's turn
    /// </summary>
    private void StartPlayerTurn()
    {
        if (!IsServer) return;

        _currentPhase.Value = TurnStateChange.Phase.PlayerTurn;
        _phaseTimeRemaining.Value = turnDuration;

        var state = new TurnStateChange
        {
            phase = TurnStateChange.Phase.PlayerTurn,
            currentPlayerId = _currentPlayerId.Value,
            phaseDuration = turnDuration,
            roundNumber = _roundNumber.Value,
            tick = _currentTick
        };

        BroadcastTurnStateClientRpc(state);

        // Reset turn flags in NetworkInputManager
        if (NetworkInputManager.Instance != null)
        {
            NetworkInputManager.Instance.ResetTurnFlags();
        }

        Debug.Log($"[TurnCoordinator] Turn started for player {_currentPlayerId.Value}");
    }

    /// <summary>
    /// Server: Start missile flight phase
    /// </summary>
    public void StartMissileFlightPhase()
    {
        if (!IsServer) return;

        _currentPhase.Value = TurnStateChange.Phase.MissileFlight;
        _phaseTimeRemaining.Value = maxMissileFlightTime;

        var state = new TurnStateChange
        {
            phase = TurnStateChange.Phase.MissileFlight,
            currentPlayerId = _currentPlayerId.Value,
            phaseDuration = maxMissileFlightTime,
            roundNumber = _roundNumber.Value,
            tick = _currentTick
        };

        BroadcastTurnStateClientRpc(state);

        Debug.Log($"[TurnCoordinator] Missile flight phase started");
    }

    /// <summary>
    /// Server: Called when missile is destroyed or phase ends
    /// </summary>
    public void OnMissileFlightComplete()
    {
        if (!IsServer) return;
        if (_currentPhase.Value != TurnStateChange.Phase.MissileFlight) return;

        // Switch to next player
        SwitchToNextPlayer();
    }

    /// <summary>
    /// Server: Switch to next player's turn
    /// </summary>
    private void SwitchToNextPlayer()
    {
        if (!IsServer) return;

        // Alternate between players
        ulong nextPlayer = (_currentPlayerId.Value == _player1Id) ? _player2Id : _player1Id;

        StartPreparationPhase(nextPlayer);
    }

    /// <summary>
    /// Server: Called when phase timer expires
    /// </summary>
    private void OnPhaseTimerExpired()
    {
        if (!IsServer) return;

        switch (_currentPhase.Value)
        {
            case TurnStateChange.Phase.PreparationPhase:
                // Prep time over, start turn
                StartPlayerTurn();
                break;

            case TurnStateChange.Phase.PlayerTurn:
                // Turn time expired, player took no action
                // Switch to next player
                SwitchToNextPlayer();
                break;

            case TurnStateChange.Phase.MissileFlight:
                // Missile took too long (lost in space)
                OnMissileFlightComplete();
                break;
        }
    }

    /// <summary>
    /// Server: Called when a ship is destroyed
    /// </summary>
    public void OnShipDestroyed(ulong destroyedPlayerId)
    {
        if (!IsServer) return;

        // Award point to winner
        if (destroyedPlayerId == _player1Id)
        {
            _player2Score.Value++;
        }
        else
        {
            _player1Score.Value++;
        }

        // Broadcast score update
        BroadcastScoreUpdateClientRpc(_player1Score.Value, _player2Score.Value);

        // Check for match winner
        if (_player1Score.Value >= winningScore || _player2Score.Value >= winningScore)
        {
            EndMatch();
        }
        else
        {
            // Start next round
            StartNextRound(destroyedPlayerId);
        }
    }

    /// <summary>
    /// Server: Start next round (loser goes first)
    /// </summary>
    private void StartNextRound(ulong loserPlayerId)
    {
        if (!IsServer) return;

        _roundNumber.Value++;

        StartCoroutine(RoundTransitionSequence(loserPlayerId));
    }

    private IEnumerator RoundTransitionSequence(ulong firstPlayerId)
    {
        // Notify clients to reset arena
        NotifyRoundEndClientRpc(_roundNumber.Value);

        // Wait for arena reset
        yield return new WaitForSeconds(3f);

        // Start prep phase for loser (fairness)
        StartPreparationPhase(firstPlayerId);
    }

    /// <summary>
    /// Server: End match
    /// </summary>
    private void EndMatch()
    {
        if (!IsServer) return;

        _isMatchActive = false;

        ulong winnerId = (_player1Score.Value >= winningScore) ? _player1Id : _player2Id;

        _currentPhase.Value = TurnStateChange.Phase.GameOver;

        NotifyMatchEndClientRpc(winnerId, _player1Score.Value, _player2Score.Value);

        Debug.Log($"[TurnCoordinator] Match ended - Winner: {winnerId}");
    }

    #endregion

    #region Client RPCs

    [ClientRpc]
    private void BroadcastTurnStateClientRpc(TurnStateChange state)
    {
        // Clients receive turn state and update their local GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNetworkTurnStateChanged(state);
        }
    }

    [ClientRpc]
    private void NotifyPlayerAssignmentsClientRpc(ulong player1Id, ulong player2Id)
    {
        Debug.Log($"[TurnCoordinator] Client received player assignments - P1: {player1Id}, P2: {player2Id}");

        if (NetworkInputManager.Instance != null)
        {
            NetworkInputManager.Instance.InitializePlayers(player1Id, player2Id);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNetworkPlayersAssigned(player1Id, player2Id);
        }
    }

    [ClientRpc]
    private void InitializeArenaClientRpc(GravityWars.Multiplayer.MatchInitData matchData, GravityWars.Multiplayer.PlanetSpawnData[] planetData)
    {
        Debug.Log($"[TurnCoordinator] Client received arena initialization - Seed: {matchData.randomSeed}, Planets: {planetData.Length}");

        // Get network adapter
        var adapter = GameManager.Instance?.GetComponent<GameManagerNetworkAdapter>();
        if (adapter == null)
        {
            Debug.LogError("[TurnCoordinator] GameManagerNetworkAdapter not found on client!");
            return;
        }

        // Spawn planets from network data
        adapter.SpawnPlanetsFromNetworkData(planetData);

        // Spawn ships at exact positions
        if (GameManager.Instance != null)
        {
            // Clear any existing ships first
            PlayerShip[] existingShips = FindObjectsOfType<PlayerShip>();
            foreach (var ship in existingShips)
            {
                Destroy(ship.gameObject);
            }

            // Spawn Player 1 ship
            GameObject player1Obj = Instantiate(
                GameManager.Instance.playerShipPrefab,
                matchData.player1SpawnPos,
                matchData.player1SpawnRot
            );
            player1Obj.name = "Player1Ship";
            GameManager.Instance.player1Ship = player1Obj.GetComponent<PlayerShip>();
            GameManager.Instance.player1Ship.playerName = "Player 1";
            GameManager.Instance.player1Ship.isLeftPlayer = true;

            // Spawn Player 2 ship
            GameObject player2Obj = Instantiate(
                GameManager.Instance.playerShipPrefab,
                matchData.player2SpawnPos,
                matchData.player2SpawnRot
            );
            player2Obj.name = "Player2Ship";
            GameManager.Instance.player2Ship = player2Obj.GetComponent<PlayerShip>();
            GameManager.Instance.player2Ship.playerName = "Player 2";
            GameManager.Instance.player2Ship.isLeftPlayer = false;

            // Initialize UI
            GameManager.Instance.UpdateFightingUI_AtRoundStart();

            Debug.Log($"[TurnCoordinator] Client arena setup complete - Ships spawned");
        }
    }

    [ClientRpc]
    private void BroadcastScoreUpdateClientRpc(int player1Score, int player2Score)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNetworkScoreUpdated(player1Score, player2Score);
        }
    }

    [ClientRpc]
    private void NotifyRoundEndClientRpc(int newRoundNumber)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNetworkRoundEnd(newRoundNumber);
        }
    }

    [ClientRpc]
    private void NotifyMatchEndClientRpc(ulong winnerId, int player1Score, int player2Score)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNetworkMatchEnd(winnerId, player1Score, player2Score);
        }
    }

    #endregion

    #region Network Variable Callbacks

    private void OnPhaseChanged(TurnStateChange.Phase oldPhase, TurnStateChange.Phase newPhase)
    {
        Debug.Log($"[TurnCoordinator] Phase changed: {oldPhase} → {newPhase}");
    }

    private void OnCurrentPlayerChanged(ulong oldId, ulong newId)
    {
        Debug.Log($"[TurnCoordinator] Current player changed: {oldId} → {newId}");
    }

    private void OnRoundChanged(int oldRound, int newRound)
    {
        Debug.Log($"[TurnCoordinator] Round changed: {oldRound} → {newRound}");
    }

    #endregion

    #region Validation

    /// <summary>
    /// Server: Validate if player can perform action
    /// </summary>
    public bool ValidatePlayerAction(ulong playerId)
    {
        if (!IsServer) return false;

        // Check if it's this player's turn
        if (playerId != _currentPlayerId.Value)
        {
            Debug.LogWarning($"[TurnCoordinator] Player {playerId} tried to act but it's {_currentPlayerId.Value}'s turn");
            return false;
        }

        // Check if in correct phase
        if (_currentPhase.Value != TurnStateChange.Phase.PlayerTurn)
        {
            Debug.LogWarning($"[TurnCoordinator] Player tried to act during {_currentPhase.Value} phase");
            return false;
        }

        return true;
    }

    #endregion

    #region Debug UI

    private void OnGUI()
    {
        if (!IsSpawned) return;

        GUILayout.BeginArea(new Rect(10, 300, 300, 300));
        GUILayout.Label("=== Turn Coordinator ===");
        GUILayout.Label($"Phase: {_currentPhase.Value}");
        GUILayout.Label($"Current Player: {_currentPlayerId.Value}");
        GUILayout.Label($"Round: {_roundNumber.Value}");
        GUILayout.Label($"Time Left: {_phaseTimeRemaining.Value:F1}s");
        GUILayout.Label($"Score: {_player1Score.Value} - {_player2Score.Value}");
        GUILayout.Label($"Match Active: {_isMatchActive}");

        if (IsServer && GUILayout.Button("Force Switch Player"))
        {
            SwitchToNextPlayer();
        }

        GUILayout.EndArea();
    }

    #endregion
}
