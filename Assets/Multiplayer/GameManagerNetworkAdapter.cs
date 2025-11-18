using UnityEngine;
using GravityWars.Multiplayer;

/// <summary>
/// Extends GameManager with network multiplayer support.
/// Provides bridge between deterministic network input and existing game logic.
/// Allows GameManager to work in both local (hotseat) and networked modes.
/// </summary>
public class GameManagerNetworkAdapter : MonoBehaviour
{
    #region State

    private bool _isNetworkedMode = false;
    private ulong _localPlayerId;
    private ulong _remotePlayerId;
    private bool _isPlayer1;

    #endregion

    #region Properties

    public bool IsNetworkedMode => _isNetworkedMode;
    public bool IsLocalPlayerTurn => _isNetworkedMode && GameManager.Instance.currentPlayer != null;

    #endregion

    #region Initialization

    /// <summary>
    /// Enable networked mode (called when match starts)
    /// </summary>
    public void EnableNetworkedMode(ulong player1Id, ulong player2Id, ulong localClientId)
    {
        _isNetworkedMode = true;
        _localPlayerId = localClientId;
        _isPlayer1 = (localClientId == player1Id);
        _remotePlayerId = _isPlayer1 ? player2Id : player1Id;

        Debug.Log($"[NetworkAdapter] Networked mode enabled - IsPlayer1: {_isPlayer1}");
    }

    /// <summary>
    /// Disable networked mode (return to local)
    /// </summary>
    public void DisableNetworkedMode()
    {
        _isNetworkedMode = false;
        Debug.Log("[NetworkAdapter] Returned to local mode");
    }

    #endregion

    #region Deterministic Planet Spawning

    /// <summary>
    /// Spawn planets using a seed for deterministic generation.
    /// Both clients will generate identical planet layouts.
    /// </summary>
    public PlanetSpawnData[] GenerateDeterministicPlanets(int seed, int unitsToSpawn)
    {
        // Set random seed for determinism
        Random.InitState(seed);

        var planetDataList = new System.Collections.Generic.List<PlanetSpawnData>();

        // Use GameManager's existing planet spawning logic, but capture the results
        // This requires modifications to GameManager to support seeded spawning

        // For now, return empty array - this will be implemented when modifying GameManager
        return planetDataList.ToArray();
    }

    /// <summary>
    /// Spawn planets from network data (client receives from server)
    /// </summary>
    public void SpawnPlanetsFromNetworkData(PlanetSpawnData[] planetData)
    {
        if (GameManager.Instance == null) return;

        // Clear existing planets
        GameManager.Instance.ClearExistingPlanetsAndShips();

        // Spawn each planet with exact parameters
        foreach (var data in planetData)
        {
            SpawnPlanetAtPosition(data);
        }

        // Update planet cache for gravity calculations
        GameManager.UpdatePlanetCache();

        Debug.Log($"[NetworkAdapter] Spawned {planetData.Length} planets from network data");
    }

    private void SpawnPlanetAtPosition(PlanetSpawnData data)
    {
        // Get planet prefab from GameManager's planetInfos array
        if (GameManager.Instance.planetInfos == null ||
            data.prefabIndex < 0 ||
            data.prefabIndex >= GameManager.Instance.planetInfos.Length)
        {
            Debug.LogError($"[NetworkAdapter] Invalid planet prefab index: {data.prefabIndex}");
            return;
        }

        var planetInfo = GameManager.Instance.planetInfos[data.prefabIndex];
        GameObject planetObj = Instantiate(planetInfo.prefab, data.position, data.rotation);

        Planet planetComponent = planetObj.GetComponent<Planet>() ?? planetObj.AddComponent<Planet>();
        planetComponent.SetPlanetProperties(planetInfo.name, data.mass);
    }

    #endregion

    #region Input Routing

    /// <summary>
    /// Route player input - sends to network in networked mode, applies directly in local mode
    /// </summary>
    public void RoutePlayerRotation(PlayerShip ship, float rotationDelta)
    {
        if (_isNetworkedMode && IsLocalPlayer(ship))
        {
            // Send to network
            NetworkInputManager.Instance?.SendRotationInput(rotationDelta);
        }
        else if (!_isNetworkedMode)
        {
            // Apply directly (local mode)
            // Ship handles its own rotation in local mode
        }
    }

    public void RoutePlayerThrust(PlayerShip ship, float thrustAmount)
    {
        if (_isNetworkedMode && IsLocalPlayer(ship))
        {
            NetworkInputManager.Instance?.SendThrustInput(thrustAmount);
        }
    }

    public void RoutePlayerFire(PlayerShip ship, Vector3 spawnPos, Quaternion spawnRot, Vector3 velocity, float angle, float power)
    {
        if (_isNetworkedMode)
        {
            // Always send to network in networked mode (server validates and broadcasts)
            if (IsLocalPlayer(ship))
            {
                NetworkInputManager.Instance?.SendFireAction(spawnPos, spawnRot, velocity, angle, power);
            }
        }
        else
        {
            // Apply directly (local mode)
            // Ship's existing fire logic handles this
        }
    }

    public void RoutePlayerMove(PlayerShip ship, Vector3 targetPos, int moveType)
    {
        if (_isNetworkedMode && IsLocalPlayer(ship))
        {
            NetworkInputManager.Instance?.SendMoveAction(targetPos, moveType);
        }
    }

    /// <summary>
    /// Check if ship belongs to local player
    /// </summary>
    private bool IsLocalPlayer(PlayerShip ship)
    {
        if (ship == GameManager.Instance.player1Ship)
        {
            return _isPlayer1;
        }
        else if (ship == GameManager.Instance.player2Ship)
        {
            return !_isPlayer1;
        }
        return false;
    }

    #endregion

    #region Network Action Execution

    /// <summary>
    /// Execute fire action from network (both clients execute simultaneously)
    /// CRITICAL: This must be deterministic - same inputs produce same results
    /// </summary>
    public void ExecuteNetworkFireAction(PlayerFireAction action)
    {
        // Determine which ship fired
        PlayerShip firingShip = GetShipForPlayer(action.playerId);
        if (firingShip == null)
        {
            Debug.LogError($"[NetworkAdapter] Cannot find ship for player {action.playerId}");
            return;
        }

        // Spawn missile with exact parameters from network
        GameObject missileObj = Instantiate(
            firingShip.missilePrefab,
            action.spawnPosition,
            action.spawnRotation
        );

        Missile3D missile = missileObj.GetComponent<Missile3D>();
        if (missile != null)
        {
            // Set owner
            missile.ownerShip = firingShip;

            // Apply exact initial velocity from network
            Rigidbody missileRb = missile.GetComponent<Rigidbody>();
            if (missileRb != null)
            {
                missileRb.velocity = action.initialVelocity;
            }

            // Store last trail reference
            firingShip.StoreLastMissileTrail(missile);

            Debug.Log($"[NetworkAdapter] Executed fire action for player {action.playerId} at tick {action.tick}");
        }

        // Notify turn coordinator that missile flight has started
        if (NetworkTurnCoordinator.Instance != null && NetworkTurnCoordinator.Instance.IsServer)
        {
            NetworkTurnCoordinator.Instance.StartMissileFlightPhase();
        }
    }

    /// <summary>
    /// Execute move action from network
    /// </summary>
    public void ExecuteNetworkMoveAction(PlayerMoveAction action)
    {
        PlayerShip movingShip = GetShipForPlayer(action.playerId);
        if (movingShip == null) return;

        // Execute move using ship's existing move logic
        // The ship's position will be set deterministically
        movingShip.transform.position = action.targetPosition;

        Debug.Log($"[NetworkAdapter] Executed move action for player {action.playerId}");
    }

    private PlayerShip GetShipForPlayer(ulong playerId)
    {
        if (GameManager.Instance == null) return null;

        // Determine which ship based on player ID
        // (Requires player ID tracking in GameManager - will be added)

        // For now, simple logic: if local player, use their ship
        if (playerId == _localPlayerId)
        {
            return _isPlayer1 ? GameManager.Instance.player1Ship : GameManager.Instance.player2Ship;
        }
        else
        {
            return _isPlayer1 ? GameManager.Instance.player2Ship : GameManager.Instance.player1Ship;
        }
    }

    #endregion

    #region Network Event Handlers

    /// <summary>
    /// Called when turn state changes from server
    /// </summary>
    public void OnTurnStateChanged(TurnStateChange state)
    {
        Debug.Log($"[NetworkAdapter] Turn state changed to {state.phase}");

        switch (state.phase)
        {
            case TurnStateChange.Phase.PreparationPhase:
                HandlePreparationPhase(state);
                break;

            case TurnStateChange.Phase.PlayerTurn:
                HandlePlayerTurn(state);
                break;

            case TurnStateChange.Phase.MissileFlight:
                HandleMissileFlight(state);
                break;

            case TurnStateChange.Phase.RoundEnd:
                HandleRoundEnd(state);
                break;

            case TurnStateChange.Phase.GameOver:
                HandleGameOver(state);
                break;
        }
    }

    private void HandlePreparationPhase(TurnStateChange state)
    {
        // Show "Get ready" message
        PlayerShip currentShip = GetShipForPlayer(state.currentPlayerId);
        if (currentShip != null && GameManager.Instance != null)
        {
            // Use GameManager's existing prep phase logic
            // This requires GameManager modifications to accept network-driven phase changes
        }
    }

    private void HandlePlayerTurn(TurnStateChange state)
    {
        // Enable controls for current player
        PlayerShip currentShip = GetShipForPlayer(state.currentPlayerId);
        if (currentShip != null)
        {
            bool isLocalPlayerTurn = (state.currentPlayerId == _localPlayerId);
            currentShip.EnableControls(isLocalPlayerTurn);
        }
    }

    private void HandleMissileFlight(TurnStateChange state)
    {
        // Disable all controls during missile flight
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player1Ship?.EnableControls(false);
            GameManager.Instance.player2Ship?.EnableControls(false);
        }
    }

    private void HandleRoundEnd(TurnStateChange state)
    {
        // Show round end transition
    }

    private void HandleGameOver(TurnStateChange state)
    {
        // Show game over screen
    }

    /// <summary>
    /// Called when missile is destroyed (from network)
    /// </summary>
    public void OnNetworkMissileDestroyed(MissileDestroyedEvent evt)
    {
        Debug.Log($"[NetworkAdapter] Missile destroyed - Reason: {evt.reason}");

        // Notify turn coordinator (server only)
        if (NetworkTurnCoordinator.Instance != null && NetworkTurnCoordinator.Instance.IsServer)
        {
            NetworkTurnCoordinator.Instance.OnMissileFlightComplete();
        }
    }

    /// <summary>
    /// Called when score is updated from server
    /// </summary>
    public void OnScoreUpdated(int player1Score, int player2Score)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player1Ship.score = player1Score;
            GameManager.Instance.player2Ship.score = player2Score;
            GameManager.Instance.UpdateScoreDisplay();
        }
    }

    #endregion
}
