// TODO: Install Unity Gaming Services packages and uncomment
// using Unity.Netcode;
using UnityEngine;
using GravityWars.Multiplayer;
using System.Collections.Generic;

/// <summary>
/// Manages deterministic input synchronization for peer-to-peer gameplay.
/// Captures local player input, sends to server, and broadcasts to all clients.
/// Both clients execute the same inputs on their local game engines.
/// </summary>
public class NetworkInputManager : NetworkBehaviour
{
    #region Singleton

    public static NetworkInputManager Instance { get; private set; }

    #endregion

    #region Configuration

    [Header("Network Settings")]
    [SerializeField] private float inputSendRate = 60f;  // Hz - sends per second
    [SerializeField] private bool enableClientPrediction = false;  // Not needed for turn-based

    #endregion

    #region State

    private uint _currentTick = 0;
    private float _tickTimer = 0f;
    private float _tickInterval;

    // Player ID assignments
    private ulong _localPlayerId;
    private ulong _remotePlayerId;
    private bool _isPlayer1;  // Which side am I?

    // Input buffering
    private Queue<PlayerRotationInput> _rotationInputQueue = new Queue<PlayerRotationInput>();
    private Queue<PlayerThrustInput> _thrustInputQueue = new Queue<PlayerThrustInput>();

    // Action tracking
    private bool _hasFiredThisTurn = false;
    private bool _hasMovedThisTurn = false;

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
            return;
        }

        _tickInterval = 1f / inputSendRate;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _localPlayerId = NetworkManager.Singleton.LocalClientId;

        Debug.Log($"[NetworkInput] Spawned - LocalClientId: {_localPlayerId}, IsServer: {IsServer}, IsHost: {IsHost}");
    }

    private void Update()
    {
        if (!IsSpawned) return;

        // Update tick timer
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= _tickInterval)
        {
            _tickTimer -= _tickInterval;
            _currentTick++;
        }
    }

    #endregion

    #region Public API - Called by GameManager/PlayerShip

    /// <summary>
    /// Initialize player assignments after match setup
    /// </summary>
    public void InitializePlayers(ulong player1Id, ulong player2Id)
    {
        _isPlayer1 = (_localPlayerId == player1Id);
        _remotePlayerId = _isPlayer1 ? player2Id : player1Id;

        Debug.Log($"[NetworkInput] Initialized - IsPlayer1: {_isPlayer1}, LocalId: {_localPlayerId}, RemoteId: {_remotePlayerId}");
    }

    /// <summary>
    /// Called when local player rotates their ship
    /// </summary>
    public void SendRotationInput(float rotationDelta)
    {
        if (!IsOwner) return;

        var input = new PlayerRotationInput
        {
            playerId = _localPlayerId,
            rotationDelta = rotationDelta,
            tick = _currentTick
        };

        // Send to server for broadcast
        SendRotationInputServerRpc(input);

        // Apply locally immediately (no lag for local player)
        ApplyRotationInput(input);
    }

    /// <summary>
    /// Called when local player applies thrust
    /// </summary>
    public void SendThrustInput(float thrustAmount)
    {
        if (!IsOwner) return;

        var input = new PlayerThrustInput
        {
            playerId = _localPlayerId,
            thrustAmount = thrustAmount,
            tick = _currentTick
        };

        SendThrustInputServerRpc(input);
        ApplyThrustInput(input);
    }

    /// <summary>
    /// Called when local player fires missile - CRITICAL for determinism
    /// </summary>
    public void SendFireAction(Vector3 spawnPos, Quaternion spawnRot, Vector3 velocity, float angle, float power)
    {
        if (!IsOwner) return;
        if (_hasFiredThisTurn)
        {
            Debug.LogWarning("[NetworkInput] Already fired this turn!");
            return;
        }

        var action = new PlayerFireAction
        {
            playerId = _localPlayerId,
            spawnPosition = spawnPos,
            spawnRotation = spawnRot,
            initialVelocity = velocity,
            fireAngle = angle,
            firePower = power,
            tick = _currentTick
        };

        SendFireActionServerRpc(action);

        // Don't apply locally yet - wait for server validation
        // This ensures both clients fire on the exact same tick
    }

    /// <summary>
    /// Called when local player uses move action
    /// </summary>
    public void SendMoveAction(Vector3 targetPos, int moveType)
    {
        if (!IsOwner) return;
        if (_hasMovedThisTurn)
        {
            Debug.LogWarning("[NetworkInput] Already moved this turn!");
            return;
        }

        var action = new PlayerMoveAction
        {
            playerId = _localPlayerId,
            targetPosition = targetPos,
            moveType = moveType,
            tick = _currentTick
        };

        SendMoveActionServerRpc(action);
    }

    /// <summary>
    /// Called when missile is destroyed - report to server
    /// </summary>
    public void ReportMissileDestroyed(int missileId, MissileDestroyedEvent.DestructionReason reason, Vector3 finalPos)
    {
        var evt = new MissileDestroyedEvent
        {
            ownerPlayerId = _localPlayerId,
            missileId = missileId,
            reason = reason,
            finalPosition = finalPos,
            tick = _currentTick
        };

        ReportMissileDestroyedServerRpc(evt);
    }

    /// <summary>
    /// Reset turn flags when new turn starts
    /// </summary>
    public void ResetTurnFlags()
    {
        _hasFiredThisTurn = false;
        _hasMovedThisTurn = false;
    }

    #endregion

    #region Server RPCs - Client to Server

    [ServerRpc(RequireOwnership = false)]
    private void SendRotationInputServerRpc(PlayerRotationInput input, ServerRpcParams rpcParams = default)
    {
        // Server broadcasts to all clients
        BroadcastRotationInputClientRpc(input);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendThrustInputServerRpc(PlayerThrustInput input, ServerRpcParams rpcParams = default)
    {
        BroadcastThrustInputClientRpc(input);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendFireActionServerRpc(PlayerFireAction action, ServerRpcParams rpcParams = default)
    {
        // Server validates the action
        if (ValidateFireAction(action))
        {
            // Broadcast to ALL clients (including sender for synchronization)
            BroadcastFireActionClientRpc(action);
        }
        else
        {
            Debug.LogWarning($"[NetworkInput] Server rejected fire action from player {action.playerId}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMoveActionServerRpc(PlayerMoveAction action, ServerRpcParams rpcParams = default)
    {
        if (ValidateMoveAction(action))
        {
            BroadcastMoveActionClientRpc(action);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReportMissileDestroyedServerRpc(MissileDestroyedEvent evt, ServerRpcParams rpcParams = default)
    {
        // Server validates and broadcasts
        // (In turn-based game, clients are authoritative for their own missiles)
        BroadcastMissileDestroyedClientRpc(evt);
    }

    #endregion

    #region Client RPCs - Server to Clients

    [ClientRpc]
    private void BroadcastRotationInputClientRpc(PlayerRotationInput input)
    {
        // Skip if this is our own input (already applied locally)
        if (input.playerId == _localPlayerId) return;

        ApplyRotationInput(input);
    }

    [ClientRpc]
    private void BroadcastThrustInputClientRpc(PlayerThrustInput input)
    {
        if (input.playerId == _localPlayerId) return;

        ApplyThrustInput(input);
    }

    [ClientRpc]
    private void BroadcastFireActionClientRpc(PlayerFireAction action)
    {
        // ALL clients apply fire action on same tick for perfect sync
        ApplyFireAction(action);
    }

    [ClientRpc]
    private void BroadcastMoveActionClientRpc(PlayerMoveAction action)
    {
        ApplyMoveAction(action);
    }

    [ClientRpc]
    private void BroadcastMissileDestroyedClientRpc(MissileDestroyedEvent evt)
    {
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNetworkMissileDestroyed(evt);
        }
    }

    #endregion

    #region Input Application - Routes to Game Logic

    private void ApplyRotationInput(PlayerRotationInput input)
    {
        // Find the ship for this player and apply rotation
        PlayerShip ship = GetShipForPlayer(input.playerId);
        if (ship != null)
        {
            ship.ApplyNetworkRotation(input.rotationDelta);
        }
    }

    private void ApplyThrustInput(PlayerThrustInput input)
    {
        PlayerShip ship = GetShipForPlayer(input.playerId);
        if (ship != null)
        {
            // TODO: Apply thrust if needed
            Debug.Log($"[NetworkInput] Thrust input from player {input.playerId}: {input.thrustAmount}");
        }
    }

    private void ApplyFireAction(PlayerFireAction action)
    {
        // This is CRITICAL - both clients must spawn missile with EXACT same parameters
        PlayerShip ship = GetShipForPlayer(action.playerId);
        if (ship != null)
        {
            ship.ExecuteNetworkFire(
                action.spawnPosition,
                action.spawnRotation,
                action.initialVelocity,
                action.fireAngle,
                action.firePower
            );
        }

        if (action.playerId == _localPlayerId)
        {
            _hasFiredThisTurn = true;
        }
    }

    private void ApplyMoveAction(PlayerMoveAction action)
    {
        PlayerShip ship = GetShipForPlayer(action.playerId);
        if (ship != null && GameManager.Instance != null)
        {
            // TODO: Execute move action on ship
            Debug.Log($"[NetworkInput] Move action from player {action.playerId} to {action.targetPosition}");
            GameManager.Instance.ExecuteNetworkMoveAction(action);
        }

        if (action.playerId == _localPlayerId)
        {
            _hasMovedThisTurn = true;
        }
    }

    #endregion

    #region Validation - Server Side

    private bool ValidateFireAction(PlayerFireAction action)
    {
        // Server checks:
        // 1. Is it this player's turn?
        // 2. Does player have missiles remaining?
        // 3. Is player in correct phase?

        // TODO: Add validation logic from NetworkGameManager
        return true;
    }

    private bool ValidateMoveAction(PlayerMoveAction action)
    {
        // Similar validation for moves
        return true;
    }

    #endregion

    #region Helper Methods

    private PlayerShip GetShipForPlayer(ulong playerId)
    {
        if (GameManager.Instance == null) return null;

        // Determine if this is player1 or player2
        bool isPlayer1 = (playerId == (_isPlayer1 ? _localPlayerId : _remotePlayerId));

        return isPlayer1 ? GameManager.Instance.player1Ship : GameManager.Instance.player2Ship;
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        if (!IsSpawned) return;

        GUILayout.BeginArea(new Rect(10, 100, 300, 200));
        GUILayout.Label($"Network Input Manager");
        GUILayout.Label($"Tick: {_currentTick}");
        GUILayout.Label($"Local Player: {(_isPlayer1 ? "Player 1" : "Player 2")}");
        GUILayout.Label($"Fired: {_hasFiredThisTurn}, Moved: {_hasMovedThisTurn}");
        GUILayout.EndArea();
    }

    #endregion
}
