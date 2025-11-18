# Online Multiplayer Implementation Guide
## Complete Step-by-Step Implementation

**Purpose:** Transform the multiplayer foundation into actual networked gameplay
**Estimated Time:** 20-30 hours
**Difficulty:** Advanced

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Phase 1: Unity Gaming Services Setup](#phase-1-unity-gaming-services-setup)
3. [Phase 2: Netcode for GameObjects Setup](#phase-2-netcode-for-gameobjects-setup)
4. [Phase 3: Integrate UGS APIs](#phase-3-integrate-ugs-apis)
5. [Phase 4: Network Game State](#phase-4-network-game-state)
6. [Phase 5: Sync Ships and Combat](#phase-5-sync-ships-and-combat)
7. [Phase 6: Testing](#phase-6-testing)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Knowledge
- C# intermediate level
- Unity basics
- Async/await programming
- Understanding of client-server architecture

### Current Game Architecture Understanding
You need to understand:
- How your GameManager controls matches
- How PlayerShip movement works
- How missiles are spawned and move
- How damage is dealt

---

## Phase 1: Unity Gaming Services Setup

### Step 1.1: Create Unity Project in Dashboard

1. Go to [Unity Dashboard](https://dashboard.unity3d.com/)
2. Click "Create Project"
3. Note your **Project ID** and **Organization ID**

### Step 1.2: Link Unity Project

1. In Unity Editor: **Edit → Project Settings → Services**
2. Click "Create Unity ID" or "Link to existing"
3. Select your organization
4. Select or create project
5. Verify "Project Settings → Services" shows your project ID

### Step 1.3: Install Required Packages

Open **Window → Package Manager**:

1. **Unity Registry** tab:
   - Install: **Netcode for GameObjects** (latest)
   - Install: **Unity Transport** (comes with Netcode)

2. **My Registries** → **Unity Registry**:
   - Install: **Authentication** (com.unity.services.authentication)
   - Install: **Cloud Save** (com.unity.services.cloudsave)
   - Install: **Lobby** (com.unity.services.lobby)
   - Install: **Relay** (com.unity.services.relay)
   - Install: **Matchmaker** (com.unity.services.matchmaker)
   - Install: **Analytics** (com.unity.analytics)

**Verify Installation:**
```csharp
// Add to any script temporarily to verify:
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Netcode;
// If no errors, packages installed correctly
```

### Step 1.4: Initialize Unity Services

1. **In Unity Dashboard → Relay:**
   - Enable Relay service
   - Note the region (e.g., "us-east")

2. **In Unity Dashboard → Lobby:**
   - Enable Lobby service

3. **In Unity Dashboard → Matchmaker:**
   - Enable Matchmaker service
   - Create a queue named "default-queue"
   - Min players: 2, Max players: 2

4. **In Unity Dashboard → Cloud Save:**
   - Enable Cloud Save

---

## Phase 2: Netcode for GameObjects Setup

### Step 2.1: Add Network Manager

1. **Create GameObject:**
   - Hierarchy → Create Empty
   - Name: `NetworkManager`
   - Add Component: **NetworkManager** (from Netcode)

2. **Configure NetworkManager:**
   - **Network Transport:**
     - Select: **Unity Transport**
   - **Player Prefab:**
     - Leave empty for now (we'll set this later)
   - **Network Prefabs List:**
     - Leave empty for now

### Step 2.2: Configure Unity Transport

1. **On NetworkManager GameObject:**
   - Find **UnityTransport** component
   - **Protocol Type:** Relay Unity Transport (we'll use Relay)
   - Leave other settings default

### Step 2.3: Create Network Scene Manager

Create `Assets/Networking/NetworkSceneManager.cs`:

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkSceneManager : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to network events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("[NetworkSceneManager] Server started");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkSceneManager] Client {clientId} connected");

        if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            // Both players connected - start match
            Debug.Log("[NetworkSceneManager] Both players connected - starting match");
            StartMatch();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkSceneManager] Client {clientId} disconnected");
    }

    private void StartMatch()
    {
        // Find GameManager and start match
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Call your existing match start logic
            // gameManager.StartMatch();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}
```

2. **Add to NetworkManager GameObject:**
   - Add Component: `NetworkSceneManager`

---

## Phase 3: Integrate UGS APIs

### Step 3.1: Initialize Services in AuthenticationService

**Update `Assets/Networking/Services/AuthenticationService.cs`:**

Replace the TODO sections with actual implementation:

```csharp
using Unity.Services.Core;
using Unity.Services.Authentication;

// In Start() method:
private async void Start()
{
    await InitializeUnityServices();

    if (autoLoginOnStart)
    {
        await SignInAnonymouslyAsync();
    }
}

// Replace TODO section:
private async Task InitializeUnityServices()
{
    try
    {
        await UnityServices.InitializeAsync();
        Log("Unity Services initialized");
    }
    catch (Exception ex)
    {
        Debug.LogError($"[AuthenticationService] Failed to initialize Unity Services: {ex.Message}");
    }
}

// Replace SignInAnonymouslyAsync TODO:
public async Task<bool> SignInAnonymouslyAsync()
{
    if (_isSignedIn)
        return true;

    try
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
        _playerName = $"Player_{_playerId.Substring(0, 6)}";
        _isSignedIn = true;

        Log($"Signed in anonymously - Player ID: {_playerId}");
        OnSignedInEvent?.Invoke(_playerId);
        return true;
    }
    catch (Exception ex)
    {
        Debug.LogError($"[AuthenticationService] Sign in failed: {ex.Message}");
        return false;
    }
}
```

### Step 3.2: Implement Relay Integration

**Update `Assets/Networking/Services/RelayService.cs`:**

```csharp
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

// Replace CreateRelay TODO:
public async Task<string> CreateRelay()
{
    try
    {
        // Create allocation for 2 players (1 host + 1 client)
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

        // Get join code
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Configure Unity Transport with relay
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

        Log($"Relay created - Join Code: {joinCode}");
        return joinCode;
    }
    catch (Exception ex)
    {
        Debug.LogError($"[RelayService] Failed to create relay: {ex.Message}");
        return null;
    }
}

// Replace JoinRelay TODO:
public async Task<bool> JoinRelay(string joinCode)
{
    try
    {
        // Join allocation with join code
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        // Configure Unity Transport with relay
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

        Log($"Joined relay with code: {joinCode}");
        return true;
    }
    catch (Exception ex)
    {
        Debug.LogError($"[RelayService] Failed to join relay: {ex.Message}");
        return false;
    }
}
```

### Step 3.3: Implement Lobby Integration

**Update `Assets/Networking/Services/LobbyService.cs`:**

```csharp
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

// Replace CreateLobby TODO:
public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false)
{
    try
    {
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = GetPlayer(),
            Data = new Dictionary<string, DataObject>
            {
                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "1v1")}
            }
        };

        Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        _currentLobby = lobby;

        // Start heartbeat
        StartLobbyHeartbeat(lobby.Id);

        Log($"Lobby created: {lobby.Name} ({lobby.Id})");
        OnLobbyCreatedEvent?.Invoke(lobby);
        return lobby;
    }
    catch (Exception ex)
    {
        Debug.LogError($"[LobbyService] Failed to create lobby: {ex.Message}");
        return null;
    }
}

// Replace JoinLobby TODO:
public async Task<Lobby> JoinLobbyById(string lobbyId)
{
    try
    {
        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
        {
            Player = GetPlayer()
        };

        Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);
        _currentLobby = lobby;

        Log($"Joined lobby: {lobby.Name}");
        OnLobbyJoinedEvent?.Invoke(lobby);
        return lobby;
    }
    catch (Exception ex)
    {
        Debug.LogError($"[LobbyService] Failed to join lobby: {ex.Message}");
        return null;
    }
}

// Helper method:
private Player GetPlayer()
{
    return new Player
    {
        Data = new Dictionary<string, PlayerDataObject>
        {
            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "Player")}
        }
    };
}

// Heartbeat to keep lobby alive:
private async void StartLobbyHeartbeat(string lobbyId)
{
    while (_currentLobby != null && _currentLobby.Id == lobbyId)
    {
        await Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
        await Task.Delay((int)(lobbyHeartbeatInterval * 1000));
    }
}
```

### Step 3.4: Connect Services Together

Create `Assets/Networking/MultiplayerFlow.cs`:

```csharp
using UnityEngine;
using Unity.Netcode;
using GravityWars.Networking;

public class MultiplayerFlow : MonoBehaviour
{
    [Header("Services")]
    private AuthenticationService _authService;
    private RelayService _relayService;
    private LobbyService _lobbyService;

    [Header("Settings")]
    public bool autoStart = false;

    private async void Start()
    {
        // Get services
        _authService = FindObjectOfType<AuthenticationService>();
        _relayService = FindObjectOfType<RelayService>();
        _lobbyService = FindObjectOfType<LobbyService>();

        if (autoStart)
        {
            await StartAsHost();
        }
    }

    // Call this to host a game
    public async System.Threading.Tasks.Task StartAsHost()
    {
        Debug.Log("[MultiplayerFlow] Starting as host...");

        // 1. Sign in
        bool signedIn = await _authService.SignInAnonymouslyAsync();
        if (!signedIn)
        {
            Debug.LogError("[MultiplayerFlow] Sign in failed");
            return;
        }

        // 2. Create relay
        string joinCode = await _relayService.CreateRelay();
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("[MultiplayerFlow] Relay creation failed");
            return;
        }

        // 3. Start network as host
        NetworkManager.Singleton.StartHost();

        // 4. Create lobby with join code
        var lobby = await _lobbyService.CreateLobby("MyLobby", 2, false);
        if (lobby != null)
        {
            // Update lobby with join code
            Debug.Log($"[MultiplayerFlow] Host started! Join Code: {joinCode}");
            Debug.Log($"[MultiplayerFlow] Share this code with other player");
        }
    }

    // Call this to join a game
    public async System.Threading.Tasks.Task StartAsClient(string joinCode)
    {
        Debug.Log($"[MultiplayerFlow] Joining with code: {joinCode}");

        // 1. Sign in
        bool signedIn = await _authService.SignInAnonymouslyAsync();
        if (!signedIn)
        {
            Debug.LogError("[MultiplayerFlow] Sign in failed");
            return;
        }

        // 2. Join relay
        bool joined = await _relayService.JoinRelay(joinCode);
        if (!joined)
        {
            Debug.LogError("[MultiplayerFlow] Relay join failed");
            return;
        }

        // 3. Start network as client
        NetworkManager.Singleton.StartClient();

        Debug.Log("[MultiplayerFlow] Client started!");
    }

    // Simple UI for testing
    private string _joinCodeInput = "";
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            if (GUILayout.Button("Host Game", GUILayout.Height(50)))
            {
                _ = StartAsHost();
            }

            GUILayout.Space(10);

            GUILayout.Label("Join Code:");
            _joinCodeInput = GUILayout.TextField(_joinCodeInput);

            if (GUILayout.Button("Join Game", GUILayout.Height(50)))
            {
                if (!string.IsNullOrEmpty(_joinCodeInput))
                {
                    _ = StartAsClient(_joinCodeInput);
                }
            }
        }
        else
        {
            GUILayout.Label($"Network Status: {(NetworkManager.Singleton.IsServer ? "Host" : "Client")}");
            GUILayout.Label($"Connected Clients: {NetworkManager.Singleton.ConnectedClients.Count}");

            if (GUILayout.Button("Disconnect"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}
```

**Add to scene:**
- Create empty GameObject: `MultiplayerFlow`
- Add component: `MultiplayerFlow`
- Test in Play Mode!

---

## Phase 4: Network Game State

### Step 4.1: Make PlayerShip Networked

**Update your PlayerShip script (or create wrapper):**

Create `Assets/Scripts/Multiplayer/NetworkedPlayerShip.cs`:

```csharp
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Networked wrapper for PlayerShip.
/// Add this component to your ship prefab alongside existing PlayerShip component.
/// </summary>
public class NetworkedPlayerShip : NetworkBehaviour
{
    [Header("Components")]
    private PlayerShip _playerShip;
    private Rigidbody2D _rb;

    [Header("Network Variables")]
    private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>();
    private NetworkVariable<float> _networkRotation = new NetworkVariable<float>();
    private NetworkVariable<int> _networkHealth = new NetworkVariable<int>(100);

    private void Awake()
    {
        _playerShip = GetComponent<PlayerShip>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to health changes
        _networkHealth.OnValueChanged += OnHealthChanged;

        if (IsOwner)
        {
            // This is the local player's ship
            // Enable player input
            _playerShip.enabled = true;
        }
        else
        {
            // This is the remote player's ship
            // Disable player input (will be controlled by network updates)
            _playerShip.enabled = false;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Send position to server
            UpdateServerPosition();
        }
        else
        {
            // Interpolate to network position
            InterpolatePosition();
        }
    }

    private void UpdateServerPosition()
    {
        if (_rb != null)
        {
            UpdatePositionServerRpc(_rb.position, transform.rotation.eulerAngles.z);
        }
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector2 position, float rotation)
    {
        _networkPosition.Value = position;
        _networkRotation.Value = rotation;
    }

    private void InterpolatePosition()
    {
        // Smooth interpolation to network position
        transform.position = Vector2.Lerp(transform.position, _networkPosition.Value, Time.deltaTime * 10f);

        Quaternion targetRotation = Quaternion.Euler(0, 0, _networkRotation.Value);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    /// <summary>
    /// Call this when ship takes damage.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!IsServer)
            return; // Only server can modify health

        _networkHealth.Value = Mathf.Max(0, _networkHealth.Value - damage);

        if (_networkHealth.Value <= 0)
        {
            OnShipDestroyed();
        }
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        // Update local ship health display
        if (_playerShip != null)
        {
            // _playerShip.UpdateHealthDisplay(newHealth);
        }

        Debug.Log($"[NetworkedPlayerShip] Health changed: {oldHealth} → {newHealth}");
    }

    private void OnShipDestroyed()
    {
        Debug.Log($"[NetworkedPlayerShip] Ship destroyed!");
        // Handle ship destruction
    }
}
```

### Step 4.2: Make Missiles Networked

Create `Assets/Scripts/Multiplayer/NetworkedMissile.cs`:

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkedMissile : NetworkBehaviour
{
    [Header("Components")]
    private Rigidbody2D _rb;

    [Header("Missile Properties")]
    private NetworkVariable<ulong> _ownerClientId = new NetworkVariable<ulong>();
    private NetworkVariable<int> _damage = new NetworkVariable<int>(25);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Initialize missile (called by whoever spawns it).
    /// </summary>
    public void Initialize(ulong ownerClientId, Vector2 direction, float speed, int damage)
    {
        if (!IsServer)
            return;

        _ownerClientId.Value = ownerClientId;
        _damage.Value = damage;

        // Apply velocity
        if (_rb != null)
        {
            _rb.velocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return; // Only server handles collisions

        // Check if hit a ship
        var ship = collision.GetComponent<NetworkedPlayerShip>();
        if (ship != null)
        {
            // Don't hit own ship
            if (ship.OwnerClientId != _ownerClientId.Value)
            {
                // Deal damage
                ship.TakeDamage(_damage.Value);

                // Destroy missile
                GetComponent<NetworkObject>().Despawn();
                Destroy(gameObject);
            }
        }
    }
}
```

### Step 4.3: Network Spawn Missiles

**In your missile spawning code (GameManager or PlayerShip):**

```csharp
// OLD CODE (local):
// Instantiate(missilePrefab, position, rotation);

// NEW CODE (networked):
public void FireMissile(Vector2 position, Vector2 direction, float speed, int damage)
{
    if (!NetworkManager.Singleton.IsServer)
    {
        // Client requests server to spawn missile
        FireMissileServerRpc(position, direction, speed, damage);
    }
    else
    {
        // Server spawns missile
        SpawnMissile(position, direction, speed, damage);
    }
}

[ServerRpc(RequireOwnership = false)]
private void FireMissileServerRpc(Vector2 position, Vector2 direction, float speed, int damage, ServerRpcParams rpcParams = default)
{
    // Get which client sent this
    ulong clientId = rpcParams.Receive.SenderClientId;
    SpawnMissile(position, direction, speed, damage, clientId);
}

private void SpawnMissile(Vector2 position, Vector2 direction, float speed, int damage, ulong ownerClientId)
{
    // Instantiate missile
    GameObject missileObj = Instantiate(networkedMissilePrefab, position, Quaternion.identity);

    // Get NetworkObject and spawn it
    NetworkObject networkObject = missileObj.GetComponent<NetworkObject>();
    networkObject.Spawn();

    // Initialize missile
    NetworkedMissile missile = missileObj.GetComponent<NetworkedMissile>();
    missile.Initialize(ownerClientId, direction, speed, damage);
}
```

---

## Phase 5: Sync Ships and Combat

### Step 5.1: Setup Networked Prefabs

1. **Create networked ship prefab:**
   - Duplicate your existing ship prefab
   - Rename: `NetworkedPlayerShip`
   - Add component: `NetworkObject`
   - Add component: `NetworkedPlayerShip`
   - Add component: `NetworkTransform` (for backup position sync)

2. **Create networked missile prefab:**
   - Duplicate your existing missile prefab
   - Rename: `NetworkedMissile`
   - Add component: `NetworkObject`
   - Add component: `NetworkedMissile`

3. **Register prefabs in NetworkManager:**
   - Select `NetworkManager` GameObject
   - In **NetworkManager** component:
     - **Network Prefabs List:** Add `NetworkedMissile` prefab
     - (Ships will be spawned manually, but you can add them too)

### Step 5.2: Spawn Ships When Players Connect

**Update `NetworkSceneManager.cs`:**

```csharp
public GameObject playerShipPrefab; // Assign in Inspector

private void OnClientConnected(ulong clientId)
{
    Debug.Log($"[NetworkSceneManager] Client {clientId} connected");

    if (NetworkManager.Singleton.IsServer)
    {
        // Spawn ship for this client
        SpawnPlayerShip(clientId);

        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            StartMatch();
        }
    }
}

private void SpawnPlayerShip(ulong clientId)
{
    // Determine spawn position (player 1 left, player 2 right)
    Vector3 spawnPosition = clientId == 0
        ? new Vector3(-5, 0, 0)  // Player 1 (host)
        : new Vector3(5, 0, 0);   // Player 2 (client)

    // Spawn ship
    GameObject shipObj = Instantiate(playerShipPrefab, spawnPosition, Quaternion.identity);
    NetworkObject networkObject = shipObj.GetComponent<NetworkObject>();
    networkObject.SpawnAsPlayerObject(clientId);

    Debug.Log($"[NetworkSceneManager] Spawned ship for client {clientId}");
}
```

### Step 5.3: Sync Game State

**Create `Assets/Scripts/Multiplayer/NetworkedGameManager.cs`:**

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkedGameManager : NetworkBehaviour
{
    private NetworkVariable<int> _player1Score = new NetworkVariable<int>(0);
    private NetworkVariable<int> _player2Score = new NetworkVariable<int>(0);
    private NetworkVariable<GameState> _gameState = new NetworkVariable<GameState>(GameState.WaitingForPlayers);

    public enum GameState
    {
        WaitingForPlayers,
        MatchStarting,
        RoundInProgress,
        RoundEnd,
        MatchEnd
    }

    public override void OnNetworkSpawn()
    {
        _gameState.OnValueChanged += OnGameStateChanged;
        _player1Score.OnValueChanged += OnScoreChanged;
        _player2Score.OnValueChanged += OnScoreChanged;
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"[NetworkedGameManager] Game state: {oldState} → {newState}");

        // Update UI, notify players, etc.
    }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        // Update score display
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportRoundWinnerServerRpc(ulong winnerClientId)
    {
        if (!IsServer)
            return;

        // Update scores
        if (winnerClientId == 0)
            _player1Score.Value++;
        else
            _player2Score.Value++;

        // Check if match is over
        if (_player1Score.Value >= 3 || _player2Score.Value >= 3)
        {
            _gameState.Value = GameState.MatchEnd;
            EndMatch(winnerClientId);
        }
        else
        {
            _gameState.Value = GameState.RoundEnd;
            Invoke(nameof(StartNextRound), 3f);
        }
    }

    private void StartNextRound()
    {
        _gameState.Value = GameState.RoundInProgress;
        // Reset ships, etc.
    }

    private void EndMatch(ulong winnerClientId)
    {
        Debug.Log($"[NetworkedGameManager] Match won by client {winnerClientId}");
        // Show match results, disconnect, etc.
    }
}
```

---

## Phase 6: Testing

### Step 6.1: Local Testing (Same Machine)

1. **Build Settings:**
   - File → Build Settings
   - Add your game scene
   - **Check:** "Development Build"
   - Click "Build and Run"
   - This opens a standalone build

2. **Test Setup:**
   - Run the standalone build → Click "Host Game"
   - Note the Join Code shown in console
   - In Unity Editor → Click Play → Click "Join Game"
   - Enter the join code
   - You should connect!

### Step 6.2: Network Testing (Different Machines)

1. **Build the game** for both test machines
2. **On Host Machine:**
   - Run game → "Host Game"
   - Note join code (will appear in debug UI)
3. **On Client Machine:**
   - Run game → Enter join code → "Join Game"
4. **Verify:**
   - Both clients see each other
   - Ships move on both screens
   - Missiles spawn and hit

### Step 6.3: Cloud Deployment Testing

Once local testing works:
1. **Deploy to Unity Cloud** (or your own backend)
2. **Test with real internet connection**
3. **Test with friends**

---

## Troubleshooting

### "Unity Services not initialized"
**Solution:** Ensure `UnityServices.InitializeAsync()` is called and completes before using any service

### "Relay allocation failed"
**Solution:**
- Check Unity Dashboard → Relay is enabled
- Verify internet connection
- Check you're signed in (`AuthenticationService.Instance.IsSignedIn`)

### "NetworkManager is null"
**Solution:** Ensure NetworkManager GameObject exists in scene and has NetworkManager component

### Ships don't spawn
**Solution:**
- Verify prefab has NetworkObject component
- Check `SpawnAsPlayerObject()` is called on server
- Ensure prefab is valid and not null

### Missiles don't sync
**Solution:**
- Verify missile prefab has NetworkObject
- Check missile is in NetworkManager's Network Prefabs List
- Ensure `NetworkObject.Spawn()` is called on server

### High latency/lag
**Solution:**
- Use NetworkTransform for smooth interpolation
- Implement client-side prediction
- Use server authoritative model (current implementation)

### Players can't connect
**Solution:**
- Verify both players signed in
- Check relay join code is correct (case-sensitive)
- Ensure Unity Relay is enabled in dashboard
- Check firewall/NAT settings

---

## Next Steps After Implementation

1. **Add Client Prediction** - Make movement feel responsive
2. **Add Lag Compensation** - Hit detection with lag
3. **Add Reconnection Logic** - Handle disconnects gracefully
4. **Add Matchmaking** - Use the MatchmakingService
5. **Add Voice Chat** - Unity Vivox integration
6. **Optimize Network Traffic** - Reduce bandwidth usage
7. **Add Anti-Cheat** - Server-authoritative everything
8. **Add Spectator Mode** - Watch matches
9. **Add Replays** - Record and playback matches
10. **Stress Test** - Test with many concurrent matches

---

## Code Examples Summary

### To Host a Match:
```csharp
// 1. Sign in
await AuthenticationService.Instance.SignInAnonymouslyAsync();

// 2. Create relay
string joinCode = await RelayService.Instance.CreateRelay();

// 3. Start as host
NetworkManager.Singleton.StartHost();

// 4. Share join code with other player
Debug.Log($"Join Code: {joinCode}");
```

### To Join a Match:
```csharp
// 1. Sign in
await AuthenticationService.Instance.SignInAnonymouslyAsync();

// 2. Join relay
await RelayService.Instance.JoinRelay(joinCode);

// 3. Start as client
NetworkManager.Singleton.StartClient();
```

### To Spawn Networked Object (Server Only):
```csharp
GameObject obj = Instantiate(prefab, position, rotation);
NetworkObject networkObject = obj.GetComponent<NetworkObject>();
networkObject.Spawn(); // Spawns on all clients
```

### To Call Server Function:
```csharp
[ServerRpc(RequireOwnership = false)]
public void DoSomethingServerRpc(int value, ServerRpcParams rpcParams = default)
{
    // This runs on server
    ulong senderClientId = rpcParams.Receive.SenderClientId;
}
```

### To Call Client Function:
```csharp
[ClientRpc]
public void DoSomethingClientRpc(int value)
{
    // This runs on all clients
}
```

---

## Resources

- [Netcode for GameObjects Docs](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Gaming Services](https://unity.com/solutions/gaming-services)
- [Unity Relay Documentation](https://docs.unity.com/relay/)
- [Unity Lobby Documentation](https://docs.unity.com/lobby/)
- [Multiplayer Networking Best Practices](https://docs-multiplayer.unity3d.com/netcode/current/learn/bestpractices/)

---

## Estimated Timeline

- **Phase 1 (UGS Setup):** 2-3 hours
- **Phase 2 (Netcode Setup):** 2-3 hours
- **Phase 3 (UGS Integration):** 3-4 hours
- **Phase 4 (Network Game State):** 4-6 hours
- **Phase 5 (Ship & Combat Sync):** 6-8 hours
- **Phase 6 (Testing & Debugging):** 4-6 hours

**Total:** 20-30 hours for experienced developer

---

**Good luck! 🚀 You're about to have a fully online multiplayer game!**
