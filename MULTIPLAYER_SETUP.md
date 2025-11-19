# Gravity Wars - Multiplayer Setup Guide

## 🎮 Architecture Overview

Your multiplayer system uses **deterministic peer-to-peer lockstep architecture**:

- **Server**: Coordinates turns, timing, validation (NO physics simulation)
- **Both Clients**: Run IDENTICAL game engine (same as local hotseat mode)
- **Synchronization**: Server broadcasts inputs, both clients execute simultaneously
- **Result**: Both players see EXACT same screen at all times

## 📋 Prerequisites

1. **Unity Netcode for GameObjects** package installed
2. **Unity Transport** package installed
3. **Unity Services** packages:
   - Unity Authentication
   - Unity Relay
   - Unity Cloud Save (already configured)

### Install Required Packages

1. Open Unity Editor
2. Go to `Window → Package Manager`
3. Click `+` → `Add package by name`
4. Add these packages:
   ```
   com.unity.netcode.gameobjects
   com.unity.transport
   com.unity.services.authentication
   com.unity.services.relay
   ```

## 🏗️ Scene Setup

### Step 1: Create Multiplayer Scene

1. **Create New Scene**
   - `File → New Scene`
   - Save as `Assets/Scenes/MultiplayerMatch.unity`

2. **Or Duplicate Existing Scene**
   - Duplicate your main game scene
   - Rename to `MultiplayerMatch`
   - Remove local-only components

### Step 2: Network Manager Setup

1. **Create Network Manager GameObject**
   ```
   Hierarchy → Right Click → Create Empty
   Name: "NetworkManager"
   ```

2. **Add NetworkManager Component**
   - Select `NetworkManager` GameObject
   - `Add Component → NetworkManager`

3. **Configure NetworkManager**
   - **Transport**: Click "Select Transport"
   - Choose `UnityTransport`
   - **Connection Data**:
     - Address: `127.0.0.1` (for local testing)
     - Port: `7777`
     - Server Listen Address: `0.0.0.0`

4. **Network Prefabs List**
   - Leave empty for now (we'll add later if needed)

### Step 3: Network Coordinator Setup

1. **Create NetworkCoordinator GameObject**
   ```
   Hierarchy → Right Click → Create Empty
   Name: "NetworkCoordinator"
   ```

2. **Add Components** (in this order):
   - `Add Component → NetworkObject`
   - `Add Component → NetworkInputManager`
   - `Add Component → NetworkTurnCoordinator`
   - `Add Component → ConnectionManager`

3. **Configure NetworkObject**
   - ✅ Check "Don't Destroy With Owner"
   - ✅ Check "Always Replicate As Root"

4. **Configure NetworkTurnCoordinator**
   - **Preparation Time**: `3` seconds
   - **Turn Duration**: `15` seconds
   - **Max Missile Flight Time**: `30` seconds
   - **Winning Score**: `3` (best of 5)

5. **Configure ConnectionManager**
   - **Max Connections**: `2`
   - (Other fields will be set at runtime)

### Step 4: GameManager Setup (Modified for Network)

1. **Find Existing GameManager GameObject**
   - Should already exist in your scene

2. **Add NetworkObject Component**
   - Select `GameManager` GameObject
   - `Add Component → NetworkObject`
   - ✅ Check "Don't Destroy With Owner"

3. **Add GameManagerNetworkAdapter Component**
   - `Add Component → GameManagerNetworkAdapter`

4. **Verify GameManager Configuration**
   - Ensure all existing references are still connected:
     - `planetInfos` array
     - `playerShipPrefab`
     - UI references (bubbleTimer, player1NameText, etc.)
     - All other serialized fields

### Step 5: Camera Setup

1. **Configure Main Camera**
   - Should already exist in scene
   - No special changes needed
   - Camera shows entire arena (both ships visible)

### Step 6: UI Canvas Setup

1. **Existing UI**
   - Your existing UI (HUD, timer, health bars) works as-is
   - Both clients see same UI updates

2. **Add Network Status UI** (Optional but recommended)
   ```
   Hierarchy → Right Click → UI → Canvas
   Name: "NetworkStatusCanvas"
   ```

3. **Add Network Status Text**
   - Right Click `NetworkStatusCanvas → UI → Text - TextMeshPro`
   - Name: "NetworkStatusText"
   - Position: Top-left corner
   - Text: "Network: Disconnected"
   - This will show connection status, player role, etc.

## 🎯 Prefab Setup

### Option A: No Prefabs Needed (Recommended)

Since both clients spawn ships locally using deterministic positions from the server, you **don't need network prefabs** for ships or missiles!

The server just sends spawn positions and both clients use their existing local prefabs.

### Option B: Network Prefabs (If Needed Later)

If you want server-spawned objects:

1. **Create Network Ship Prefab** (Optional)
   - Duplicate `playerShipPrefab`
   - Add `NetworkObject` component
   - Save as new prefab in `Assets/Prefabs/NetworkPlayerShip.prefab`

2. **Register in NetworkManager**
   - Select `NetworkManager` GameObject
   - In Inspector, expand "Network Prefabs List"
   - Add your network prefab

**Note**: Current implementation doesn't require this. Ships are spawned locally.

## 🔧 Testing Setup - Local (2 Editor Instances)

### Step 1: ParrelSync Setup (Recommended)

1. **Install ParrelSync** (Unity Asset Store - FREE)
   - Allows running multiple Unity Editor instances
   - Essential for local multiplayer testing

2. **Create Clone Project**
   - `ParrelSync → Clones Manager`
   - Click "Create New Clone"
   - Wait for clone creation

### Step 2: Testing Flow

**Editor Instance 1 (Host):**

1. Open your main Unity project
2. Enter Play Mode
3. Run this in Console or create a test UI button:
   ```csharp
   // Start as Host
   NetworkManager.Singleton.StartHost();

   // OR use ConnectionManager with Relay
   await ConnectionManager.Instance.StartHostWithRelay();
   // This returns a join code - copy it!
   ```

**Editor Instance 2 (Client):**

1. Open cloned project (via ParrelSync)
2. Enter Play Mode
3. Connect to host:
   ```csharp
   // Local connection
   NetworkManager.Singleton.StartClient();

   // OR use Relay with join code
   await ConnectionManager.Instance.StartClientWithRelay("JOIN_CODE_HERE");
   ```

### Step 3: Verification Checklist

✅ Both editors show identical planet positions
✅ Both editors show both ships (Player 1 and Player 2)
✅ Each player can only control their own ship
✅ When one player rotates, both editors show the rotation
✅ When one player fires, both editors show identical missile trajectory
✅ Timer shows same value on both clients
✅ Turn switching works correctly
✅ Score updates on both clients when ship destroyed

## 🎮 In-Game Testing UI (Temporary)

Create a simple test UI for starting matches:

### Create TestNetworkUI.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class TestNetworkUI : MonoBehaviour
{
    [Header("UI References")]
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI statusText;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);

        UpdateStatus("Disconnected");
    }

    private async void OnHostClicked()
    {
        UpdateStatus("Starting Host...");

        // Start host with relay
        string joinCode = await ConnectionManager.Instance.StartHostWithRelay();

        if (!string.IsNullOrEmpty(joinCode))
        {
            UpdateStatus($"Hosting! Join Code: {joinCode}");

            // Initialize match when client connects
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            UpdateStatus("Failed to start host!");
        }

        // Hide buttons
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }

    private async void OnClientClicked()
    {
        string joinCode = joinCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(joinCode))
        {
            UpdateStatus("Enter join code!");
            return;
        }

        UpdateStatus("Connecting...");

        bool success = await ConnectionManager.Instance.StartClientWithRelay(joinCode);

        if (success)
        {
            UpdateStatus("Connected!");
        }
        else
        {
            UpdateStatus("Failed to connect!");
        }

        // Hide buttons
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Wait for 2 players
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            UpdateStatus("2 Players Connected! Starting Match...");

            // Get player IDs
            ulong player1Id = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
            ulong player2Id = NetworkManager.Singleton.ConnectedClientsList[1].ClientId;

            // Initialize match
            NetworkTurnCoordinator.Instance.InitializeMatch(player1Id, player2Id);
        }
    }

    private void UpdateStatus(string message)
    {
        statusText.text = message;
        Debug.Log($"[TestNetworkUI] {message}");
    }
}
```

### Add to Scene

1. Create UI Canvas with:
   - "Host Game" button
   - "Join Game" button
   - Input field for join code
   - Status text
2. Attach `TestNetworkUI.cs` to Canvas
3. Connect references in Inspector

## 🔄 Match Flow (What Happens Behind the Scenes)

### Match Initialization

```
1. Host starts → Creates NetworkManager
2. Client joins → Connects to host via Relay
3. Server detects 2 players connected
4. Server calls NetworkTurnCoordinator.InitializeMatch(player1Id, player2Id)
5. Server generates random seed for planets
6. Server → Both Clients: MatchInitData(seed, spawnPositions)
7. Both clients spawn identical planet arenas using seed
8. Both clients spawn ships at exact positions
9. Server starts turn sequence
```

### Turn Execution

```
10. Server → Both: "PreparationPhase - Player 1 - 3 seconds"
11. Both clients show countdown
12. Server → Both: "PlayerTurn - Player 1 - 15 seconds"
13. Both clients enable Player 1's controls (disable Player 2's)

14. Player 1 rotates ship:
    a. Client captures: rotationDelta = 5.2°
    b. Client applies locally (instant feedback)
    c. Client → Server: PlayerRotationInput(5.2°)
    d. Server → Player 2: PlayerRotationInput(5.2°)
    e. Player 2's client rotates Player 1's ship by 5.2°

15. Player 1 fires missile:
    a. Client calculates: pos=(10,5,0), vel=(3.2,1.8,0)
    b. Client → Server: PlayerFireAction(pos, vel, tick)
    c. Server validates
    d. Server → BOTH clients: PlayerFireAction
    e. BOTH clients spawn missile with exact same parameters
    f. BOTH clients run missile physics (Planet-based gravity)
    g. Result: Identical trajectories

16. Missile hits ship:
    a. Both clients detect collision (deterministic physics)
    b. Client → Server: MissileDestroyedEvent(HitShip)
    c. Server validates
    d. Server → Both: DamageEvent + ScoreUpdate
    e. Both clients apply damage, update score

17. Server → Both: "PreparationPhase - Player 2 - 3 seconds"
    (Repeat from step 10)
```

## 🐛 Troubleshooting

### Issue: Planets are different on each client

**Cause**: Random seed not synchronized
**Fix**: Ensure server sends same seed to both clients
**Check**: `GameManagerNetworkAdapter.SpawnPlanetsFromNetworkData()`

### Issue: Missile trajectories don't match

**Cause**:
- Different spawn parameters
- Non-deterministic physics
- Different Time.fixedDeltaTime

**Fix**:
- Verify both clients receive exact same `PlayerFireAction`
- Ensure `Time.fixedDeltaTime` is same on both (set in Project Settings)
- Check that both use `GameManager.GetCachedPlanets()` for gravity

### Issue: One player can't control ship

**Cause**: Input routing not set up
**Fix**: Player can only control ship during their turn (by design)
**Verify**: Check `NetworkTurnCoordinator.CurrentPlayerId` matches player

### Issue: Turn timer out of sync

**Cause**: Clients not receiving turn state updates
**Fix**:
- Check `NetworkTurnCoordinator` is spawned on network
- Verify `OnNetworkTurnStateChanged()` is being called
- Enable Debug UI to see current phase

### Issue: Score doesn't update

**Cause**: Server not broadcasting score updates
**Fix**:
- Verify `NetworkTurnCoordinator.OnShipDestroyed()` is called
- Check `OnNetworkScoreUpdated()` callback in GameManager

## 📊 Debug Information

### Enable Debug UI

All network components have `OnGUI()` debug displays:

**NetworkInputManager** (top-left):
- Current tick
- Player assignment (P1/P2)
- Action flags (Fired/Moved)

**NetworkTurnCoordinator** (bottom-left):
- Current phase
- Current player ID
- Round number
- Time remaining
- Score

### Console Logs

All network components log extensively:
- `[NetworkInput]` - Input capture and broadcast
- `[TurnCoordinator]` - Phase changes, player switches
- `[NetworkAdapter]` - Action execution
- `[GameManager]` - Network callbacks

Filter console by these tags to debug specific systems.

## 🚀 Production Build Setup

When building for release:

1. **Remove Test UI**
   - Delete `TestNetworkUI` components
   - Create proper matchmaking UI

2. **Configure Relay**
   - Set up Unity Gaming Services project
   - Enable Relay service
   - Get Project ID from Unity Dashboard

3. **Build Settings**
   - Add `MultiplayerMatch` scene to build
   - Set scripting backend to IL2CPP (recommended)
   - Build for target platforms

4. **Player Settings**
   - Set `Time.fixedDeltaTime = 0.02` (50 FPS physics)
   - Ensure deterministic physics settings

## ✅ Next Steps

1. **Implement Seed-Based Planet Spawning** (High Priority)
   - Modify `GameManager.SpawnPlanets()` to accept seed parameter
   - Ensure deterministic planet generation

2. **Modify PlayerShip Input Routing**
   - Route inputs through `GameManagerNetworkAdapter` in networked mode
   - Keep direct input for local mode

3. **Test with 2 Clients**
   - Use ParrelSync to verify synchronization
   - Test all game phases (prep, turn, missile flight, round end)

4. **Add UI Polish**
   - Show whose turn it is
   - Highlight current player's ship
   - Add "Waiting for opponent" messages

5. **Implement Reconnection Handling**
   - Handle disconnections gracefully
   - Option to forfeit if opponent disconnects

---

## 📞 Quick Reference

**Start Host:**
```csharp
string joinCode = await ConnectionManager.Instance.StartHostWithRelay();
```

**Join as Client:**
```csharp
await ConnectionManager.Instance.StartClientWithRelay(joinCode);
```

**Initialize Match (Server Only):**
```csharp
NetworkTurnCoordinator.Instance.InitializeMatch(player1Id, player2Id);
```

**Check Current Turn:**
```csharp
bool isMyTurn = (NetworkTurnCoordinator.Instance.CurrentPlayerId == NetworkManager.Singleton.LocalClientId);
```

---

**You're all set!** The multiplayer system uses the exact same game engine as local hotseat mode, so both players will see identical gameplay. 🎮
