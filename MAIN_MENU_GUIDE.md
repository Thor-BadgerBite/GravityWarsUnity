# Gravity Wars - Main Menu Hub Guide
**Brawl Stars-Style Main Menu Implementation**

---

## 📋 Overview

This guide shows you how to create a **professional main menu hub** with:
- **3D Ship Viewer** in the center (rotating equipped ship)
- **Navigation buttons** around the edges (Brawl Stars layout)
- **Panel system** for Ships, Missiles, Battle Pass, Quests, Shop, Profile, Settings
- **Smooth transitions** between screens
- **Full integration** with all game systems

**Estimated Time:** 4-6 hours

---

## 🎨 UI Layout Design

```
┌─────────────────────────────────────────────────┐
│  [Profile]                        [Settings]    │
│                                                  │
│           ┌─────────────────┐                   │
│           │                 │                   │
│  [Ships]  │   3D SHIP       │   [Missiles]     │
│           │   VIEWER        │                   │
│           │   (Rotating)    │                   │
│           │                 │                   │
│  [Quests] └─────────────────┘   [Battle Pass]  │
│                                                  │
│           [PLAY ONLINE]                         │
│                                                  │
│  Credits: 1,250  |  Gems: 45  |  Level: 12      │
└─────────────────────────────────────────────────┘
```

**Key Features:**
- **Center:** 3D ship model with stats overlay
- **Left side:** Ships, Quests buttons
- **Right side:** Missiles, Battle Pass buttons
- **Top:** Profile, Settings buttons
- **Bottom:** Large PLAY button, currency display

---

## 📁 Project Structure

Create these folders in Unity:
```
Assets/
├── UI/
│   ├── MainMenu/
│   │   ├── MainMenuScene.unity
│   │   ├── Prefabs/
│   │   │   ├── MainMenuCanvas.prefab
│   │   │   ├── ShipViewerCamera.prefab
│   │   │   └── NavigationButton.prefab
│   │   ├── Scripts/
│   │   │   ├── MainMenuManager.cs
│   │   │   ├── ShipViewer3D.cs
│   │   │   ├── NavigationSystem.cs
│   │   │   ├── CurrencyDisplay.cs
│   │   │   └── PanelManager.cs
│   │   └── Panels/
│   │       ├── ShipSelectionPanel.prefab
│   │       ├── MissileLoadoutPanel.prefab
│   │       ├── BattlePassPanel.prefab
│   │       ├── QuestsPanel.prefab
│   │       ├── ShopPanel.prefab
│   │       ├── ProfilePanel.prefab
│   │       └── SettingsPanel.prefab
```

---

## 🚀 Step-by-Step Implementation

### **Part 1: Create the Main Menu Scene**

#### **Step 1: Create New Scene**
1. File → New Scene → Create "MainMenuScene"
2. Save to: `Assets/UI/MainMenu/MainMenuScene.unity`
3. Remove default Main Camera (we'll add custom cameras)

#### **Step 2: Add Main Canvas**
1. GameObject → UI → Canvas
2. Rename to "MainMenuCanvas"
3. Canvas settings:
   - Render Mode: **Screen Space - Camera**
   - Pixel Perfect: **Checked**
   - Sorting Layer: **UI**
4. Add **Canvas Scaler** component:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Screen Match Mode: **Match Width Or Height**
   - Match: **0.5** (balanced)

#### **Step 3: Add Event System**
- Should be created automatically with Canvas
- If not: GameObject → UI → Event System

---

### **Part 2: 3D Ship Viewer Setup**

This is the **centerpiece** - a rotating 3D ship model.

#### **Step 1: Create Ship Viewer Camera**

1. **Create new camera:**
   - GameObject → Camera
   - Name: "ShipViewerCamera"
   - Position: `(0, 1, -5)`
   - Rotation: `(0, 0, 0)`

2. **Camera settings:**
   - Clear Flags: **Solid Color**
   - Background: **Transparent** (Alpha 0)
   - Culling Mask: **ShipViewer** layer only (create new layer)
   - Depth: **-2** (renders before main UI)
   - Target Display: **Display 1**

3. **Create Render Texture:**
   - Assets → Create → Render Texture
   - Name: "ShipViewerRT"
   - Size: **1024 x 1024**
   - Depth Buffer: **16 bit**
   - Anti-aliasing: **4x**
   - Filter Mode: **Bilinear**

4. **Assign Render Texture to Camera:**
   - Select ShipViewerCamera
   - Target Texture: **ShipViewerRT**

#### **Step 2: Create Ship Display Container**

1. **Create empty GameObject:**
   - Name: "ShipDisplayContainer"
   - Position: `(0, 0, 0)`
   - Layer: **ShipViewer**

2. **Add rotation script** (we'll create this next)

#### **Step 3: Create ShipViewer3D Script**

Create: `Assets/UI/MainMenu/Scripts/ShipViewer3D.cs`

```csharp
using UnityEngine;

/// <summary>
/// Displays and rotates the player's equipped ship in the main menu
/// </summary>
public class ShipViewer3D : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Container that holds the ship model")]
    public Transform shipContainer;

    [Tooltip("Current ship model instance")]
    private GameObject currentShipModel;

    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 30f;

    [Tooltip("If true, rotates continuously")]
    public bool autoRotate = true;

    [Header("Scale Settings")]
    [Tooltip("Ship model scale multiplier")]
    public float shipScale = 1.5f;

    [Header("Lighting")]
    [Tooltip("Directional light for ship")]
    public Light shipLight;

    void Start()
    {
        SetupLighting();
        LoadEquippedShip();
    }

    void Update()
    {
        if (autoRotate && shipContainer != null)
        {
            shipContainer.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// Loads the player's currently equipped ship
    /// </summary>
    public void LoadEquippedShip()
    {
        // Get equipped ship from ProgressionManager
        if (ProgressionManager.Instance == null || ProgressionManager.Instance.currentPlayerData == null)
        {
            Debug.LogWarning("[ShipViewer3D] No player data found");
            return;
        }

        string equippedShipID = ProgressionManager.Instance.currentPlayerData.equippedShipBodyID;

        // Load ship preset
        // TODO: You'll need a ShipDatabase or Resources folder with ship prefabs
        // For now, this is a placeholder
        LoadShipModel(equippedShipID);
    }

    /// <summary>
    /// Loads a specific ship model by ID
    /// </summary>
    private void LoadShipModel(string shipID)
    {
        // Clear existing model
        if (currentShipModel != null)
        {
            Destroy(currentShipModel);
        }

        // Load ship prefab from Resources
        // Option 1: From Resources folder
        GameObject shipPrefab = Resources.Load<GameObject>($"Ships/{shipID}");

        // Option 2: From ShipBodySO (if you have ship prefabs in ScriptableObjects)
        // ShipBodySO shipData = GetShipDataByID(shipID);
        // GameObject shipPrefab = shipData.shipModelPrefab;

        if (shipPrefab == null)
        {
            Debug.LogError($"[ShipViewer3D] Ship model not found: {shipID}");
            return;
        }

        // Instantiate ship
        currentShipModel = Instantiate(shipPrefab, shipContainer);
        currentShipModel.transform.localPosition = Vector3.zero;
        currentShipModel.transform.localRotation = Quaternion.identity;
        currentShipModel.transform.localScale = Vector3.one * shipScale;

        // Set layer recursively
        SetLayerRecursively(currentShipModel, LayerMask.NameToLayer("ShipViewer"));

        Debug.Log($"[ShipViewer3D] Loaded ship: {shipID}");
    }

    /// <summary>
    /// Sets layer on object and all children
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Sets up lighting for ship viewer
    /// </summary>
    private void SetupLighting()
    {
        if (shipLight == null)
        {
            // Create directional light
            GameObject lightObj = new GameObject("ShipViewerLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            lightObj.layer = LayerMask.NameToLayer("ShipViewer");

            shipLight = lightObj.AddComponent<Light>();
            shipLight.type = LightType.Directional;
            shipLight.color = Color.white;
            shipLight.intensity = 1.0f;
            shipLight.cullingMask = 1 << LayerMask.NameToLayer("ShipViewer");
        }
    }

    /// <summary>
    /// Manually refresh the displayed ship
    /// </summary>
    public void RefreshShip()
    {
        LoadEquippedShip();
    }
}
```

#### **Step 4: Add Ship Viewer to Canvas**

1. **In MainMenuCanvas, create:**
   - GameObject → UI → Raw Image
   - Name: "ShipViewerDisplay"
   - Anchor: **Center**
   - Position: `(0, 50, 0)`
   - Width: **600**
   - Height: **600**

2. **Assign Render Texture:**
   - Texture: **ShipViewerRT**

3. **Add ShipViewer3D component:**
   - Select ShipDisplayContainer
   - Add Component → ShipViewer3D
   - Ship Container: **ShipDisplayContainer**
   - Rotation Speed: **30**
   - Auto Rotate: **True**

---

### **Part 3: Navigation Buttons**

Create the main navigation buttons around the ship viewer.

#### **Step 1: Create Navigation Button Prefab**

1. **Create button:**
   - In MainMenuCanvas: GameObject → UI → Button
   - Name: "NavigationButton"
   - Width: **180**
   - Height: **80**

2. **Style the button:**
   - Colors:
     - Normal: `#2C3E50` (dark blue-gray)
     - Highlighted: `#34495E` (lighter blue-gray)
     - Pressed: `#1ABC9C` (turquoise)
     - Disabled: `#95A5A6` (gray)

3. **Add icon (optional):**
   - Inside button: GameObject → UI → Image
   - Name: "Icon"
   - Anchor: **Left**
   - Width/Height: **50**

4. **Update text:**
   - Select Text child
   - Font Size: **24**
   - Alignment: **Center**
   - Color: **White**

5. **Make it a prefab:**
   - Drag to `Assets/UI/MainMenu/Prefabs/NavigationButton.prefab`

#### **Step 2: Position Navigation Buttons**

Create these buttons in the MainMenuCanvas:

**Left Side:**
1. **Ships Button**
   - Position: `(-700, 150, 0)`
   - Text: "SHIPS"

2. **Quests Button**
   - Position: `(-700, -50, 0)`
   - Text: "QUESTS"

**Right Side:**
3. **Missiles Button**
   - Position: `(700, 150, 0)`
   - Text: "MISSILES"

4. **Battle Pass Button**
   - Position: `(700, -50, 0)`
   - Text: "BATTLE PASS"

**Top:**
5. **Profile Button**
   - Position: `(-850, 480, 0)`
   - Width: **150**
   - Text: "PROFILE"

6. **Settings Button**
   - Position: `(850, 480, 0)`
   - Width: **150**
   - Text: "SETTINGS"

**Bottom Center:**
7. **Play Online Button** (BIG!)
   - Position: `(0, -400, 0)`
   - Width: **400**
   - Height: **120**
   - Text: "PLAY ONLINE"
   - Font Size: **36**
   - Color: `#27AE60` (green) for Normal
   - Color: `#2ECC71` (bright green) for Highlighted

---

### **Part 4: Panel System**

Create the panel switching system for different screens.

#### **Step 1: Create PanelManager Script**

Create: `Assets/UI/MainMenu/Scripts/PanelManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages switching between different UI panels in the main menu
/// </summary>
public class PanelManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject homePanel;           // The main hub view
    public GameObject shipSelectionPanel;
    public GameObject missileLoadoutPanel;
    public GameObject battlePassPanel;
    public GameObject questsPanel;
    public GameObject shopPanel;
    public GameObject profilePanel;
    public GameObject settingsPanel;

    [Header("Animation Settings")]
    public float transitionSpeed = 0.3f;

    private GameObject currentPanel;
    private Dictionary<string, GameObject> panels;

    void Awake()
    {
        // Initialize panel dictionary
        panels = new Dictionary<string, GameObject>
        {
            { "home", homePanel },
            { "ships", shipSelectionPanel },
            { "missiles", missileLoadoutPanel },
            { "battlepass", battlePassPanel },
            { "quests", questsPanel },
            { "shop", shopPanel },
            { "profile", profilePanel },
            { "settings", settingsPanel }
        };

        // Show home panel by default
        ShowPanel("home");
    }

    /// <summary>
    /// Shows a specific panel by name
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            Debug.LogError($"[PanelManager] Panel not found: {panelName}");
            return;
        }

        // Hide current panel
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        // Show new panel
        GameObject newPanel = panels[panelName];
        newPanel.SetActive(true);
        currentPanel = newPanel;

        Debug.Log($"[PanelManager] Switched to panel: {panelName}");
    }

    /// <summary>
    /// Returns to home panel
    /// </summary>
    public void ShowHome()
    {
        ShowPanel("home");
    }

    // Button callback methods
    public void ShowShipsPanel() => ShowPanel("ships");
    public void ShowMissilesPanel() => ShowPanel("missiles");
    public void ShowBattlePassPanel() => ShowPanel("battlepass");
    public void ShowQuestsPanel() => ShowPanel("quests");
    public void ShowShopPanel() => ShowPanel("shop");
    public void ShowProfilePanel() => ShowPanel("profile");
    public void ShowSettingsPanel() => ShowPanel("settings");
}
```

#### **Step 2: Create Panel Structure**

1. **In MainMenuCanvas, create:**
   - GameObject → Empty
   - Name: "PanelsContainer"
   - Anchor: **Stretch** (fill entire screen)

2. **Inside PanelsContainer, create panels:**

**Home Panel:**
   - GameObject → Empty
   - Name: "HomePanel"
   - This contains the 3D ship viewer and navigation buttons

**Other Panels** (create these as empty containers for now):
   - ShipSelectionPanel
   - MissileLoadoutPanel
   - BattlePassPanel
   - QuestsPanel
   - ShopPanel
   - ProfilePanel
   - SettingsPanel

3. **Add PanelManager to Canvas:**
   - Select MainMenuCanvas
   - Add Component → PanelManager
   - Assign all panel references

#### **Step 3: Wire Up Navigation Buttons**

For each navigation button, set the OnClick() event:

1. **Ships Button:**
   - OnClick() → PanelManager.ShowShipsPanel()

2. **Missiles Button:**
   - OnClick() → PanelManager.ShowMissilesPanel()

3. **Battle Pass Button:**
   - OnClick() → PanelManager.ShowBattlePassPanel()

4. **Quests Button:**
   - OnClick() → PanelManager.ShowQuestsPanel()

5. **Profile Button:**
   - OnClick() → PanelManager.ShowProfilePanel()

6. **Settings Button:**
   - OnClick() → PanelManager.ShowSettingsPanel()

7. **Play Online Button:**
   - This will open matchmaking (we'll implement later)

---

### **Part 5: Currency Display**

Show player's credits, gems, and level at the bottom.

#### **Step 1: Create Currency Display UI**

1. **In MainMenuCanvas, create:**
   - GameObject → UI → Panel
   - Name: "CurrencyBar"
   - Anchor: **Bottom Stretch**
   - Position Y: **0**
   - Height: **60**
   - Color: `#34495E` (dark gray, alpha 200)

2. **Inside CurrencyBar:**

**Credits Display:**
   - GameObject → UI → Text
   - Name: "CreditsText"
   - Anchor: **Left**
   - Position: `(150, 0, 0)`
   - Text: "Credits: 0"
   - Font Size: **20**
   - Color: **Gold** `#F1C40F`

**Gems Display:**
   - GameObject → UI → Text
   - Name: "GemsText"
   - Anchor: **Center**
   - Text: "Gems: 0"
   - Font Size: **20**
   - Color: **Cyan** `#3498DB`

**Level Display:**
   - GameObject → UI → Text
   - Name: "LevelText"
   - Anchor: **Right**
   - Position: `(-150, 0, 0)`
   - Text: "Level: 1"
   - Font Size: **20**
   - Color: **White**

#### **Step 2: Create CurrencyDisplay Script**

Create: `Assets/UI/MainMenu/Scripts/CurrencyDisplay.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays player's currency and level in the main menu
/// </summary>
public class CurrencyDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Text creditsText;
    public Text gemsText;
    public Text levelText;

    [Header("Format")]
    public string creditsFormat = "Credits: {0:N0}";
    public string gemsFormat = "Gems: {0:N0}";
    public string levelFormat = "Level: {0}";

    void Start()
    {
        UpdateDisplay();
    }

    void OnEnable()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the currency display from ProgressionManager
    /// </summary>
    public void UpdateDisplay()
    {
        if (ProgressionManager.Instance == null || ProgressionManager.Instance.currentPlayerData == null)
        {
            Debug.LogWarning("[CurrencyDisplay] No player data found");
            return;
        }

        PlayerAccountData data = ProgressionManager.Instance.currentPlayerData;

        // Update credits
        if (creditsText != null)
            creditsText.text = string.Format(creditsFormat, data.credits);

        // Update gems
        if (gemsText != null)
            gemsText.text = string.Format(gemsFormat, data.gems);

        // Update level
        if (levelText != null)
            levelText.text = string.Format(levelFormat, data.level);
    }

    /// <summary>
    /// Call this when currency changes
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
}
```

#### **Step 3: Wire Up Currency Display**

1. Select CurrencyBar
2. Add Component → CurrencyDisplay
3. Assign text references:
   - Credits Text: **CreditsText**
   - Gems Text: **GemsText**
   - Level Text: **LevelText**

---

### **Part 6: Main Menu Manager**

Create the central manager that initializes everything.

#### **Create MainMenuManager Script**

Create: `Assets/UI/MainMenu/Scripts/MainMenuManager.cs`

```csharp
using UnityEngine;

/// <summary>
/// Main manager for the menu hub - initializes all systems
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Managers")]
    public PanelManager panelManager;
    public ShipViewer3D shipViewer;
    public CurrencyDisplay currencyDisplay;

    [Header("Audio")]
    public AudioClip menuMusic;

    void Start()
    {
        InitializeMenu();
    }

    /// <summary>
    /// Initializes the main menu
    /// </summary>
    private void InitializeMenu()
    {
        // Initialize ProgressionManager if not already loaded
        if (ProgressionManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuManager] ProgressionManager not found - creating new instance");
            GameObject progManager = new GameObject("ProgressionManager");
            progManager.AddComponent<ProgressionManager>();
        }

        // Load player data
        LoadPlayerData();

        // Update displays
        if (currencyDisplay != null)
            currencyDisplay.UpdateDisplay();

        if (shipViewer != null)
            shipViewer.LoadEquippedShip();

        // Play menu music
        if (menuMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }

        Debug.Log("[MainMenuManager] Main menu initialized");
    }

    /// <summary>
    /// Loads player data from save system
    /// </summary>
    private void LoadPlayerData()
    {
        if (ProgressionManager.Instance != null)
        {
            // Load from SaveSystem
            PlayerAccountData data = SaveSystem.LoadPlayerDataLocal();

            if (data != null)
            {
                ProgressionManager.Instance.currentPlayerData = data;
                Debug.Log($"[MainMenuManager] Loaded player data: {data.username}, Level {data.level}");
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] No save data found - creating new player");
                CreateNewPlayer();
            }
        }
    }

    /// <summary>
    /// Creates a new player if no save exists
    /// </summary>
    private void CreateNewPlayer()
    {
        PlayerAccountData newPlayer = new PlayerAccountData("Player", "NewPlayer");
        newPlayer.level = 1;
        newPlayer.credits = 1000;
        newPlayer.gems = 50;

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.currentPlayerData = newPlayer;
        }

        SaveSystem.SavePlayerDataLocal(newPlayer);
        Debug.Log("[MainMenuManager] Created new player profile");
    }

    /// <summary>
    /// Called when user clicks "Play Online"
    /// </summary>
    public void OnPlayOnlineClicked()
    {
        Debug.Log("[MainMenuManager] Play Online clicked");
        // TODO: Open matchmaking panel or load online scene
        // For now, this is a placeholder
    }

    /// <summary>
    /// Refreshes all displays (call after shop purchase, etc.)
    /// </summary>
    public void RefreshAllDisplays()
    {
        if (currencyDisplay != null)
            currencyDisplay.Refresh();

        if (shipViewer != null)
            shipViewer.RefreshShip();
    }
}
```

---

### **Part 7: Assembly and Testing**

#### **Step 1: Organize Scene Hierarchy**

Your hierarchy should look like this:

```
MainMenuScene
├── MainMenuCanvas
│   ├── PanelsContainer
│   │   ├── HomePanel
│   │   │   ├── ShipViewerDisplay (Raw Image)
│   │   │   ├── ShipsButton
│   │   │   ├── QuestsButton
│   │   │   ├── MissilesButton
│   │   │   ├── BattlePassButton
│   │   │   ├── ProfileButton
│   │   │   ├── SettingsButton
│   │   │   └── PlayOnlineButton
│   │   ├── ShipSelectionPanel (empty for now)
│   │   ├── MissileLoadoutPanel (empty for now)
│   │   ├── BattlePassPanel (empty for now)
│   │   ├── QuestsPanel (empty for now)
│   │   ├── ShopPanel (empty for now)
│   │   ├── ProfilePanel (empty for now)
│   │   └── SettingsPanel (empty for now)
│   └── CurrencyBar
│       ├── CreditsText
│       ├── GemsText
│       └── LevelText
├── ShipViewerCamera
└── ShipDisplayContainer
    └── ShipViewerLight
```

#### **Step 2: Add MainMenuManager**

1. Create empty GameObject in scene: "MainMenuManager"
2. Add MainMenuManager component
3. Assign references:
   - Panel Manager: **MainMenuCanvas (PanelManager component)**
   - Ship Viewer: **ShipDisplayContainer (ShipViewer3D component)**
   - Currency Display: **CurrencyBar (CurrencyDisplay component)**

#### **Step 3: Create Layer**

1. Edit → Project Settings → Tags and Layers
2. Add new layer: **ShipViewer**
3. Assign ShipDisplayContainer and its children to this layer

#### **Step 4: Test the Menu**

1. Press Play
2. You should see:
   - Empty ship viewer in center (until you add ship models)
   - Navigation buttons around the edges
   - Currency display at bottom
   - Clicking buttons switches panels

---

## 🎨 Part 8: Creating Individual Panels

Now we'll create content for each panel.

### **Ship Selection Panel**

This panel lets players select and equip ships.

**Layout:**
```
┌─────────────────────────────────────────┐
│  [← Back]              SHIPS             │
│                                          │
│  ┌────┐  ┌────┐  ┌────┐  ┌────┐        │
│  │ S1 │  │ S2 │  │ S3 │  │ S4 │        │
│  │    │  │    │  │    │  │    │        │
│  └────┘  └────┘  └────┘  └────┘        │
│  [Equip] [Equip] [🔒]   [🔒]           │
│                                          │
│  ╔════════════════════════╗             │
│  ║  Ship Name: Viper      ║             │
│  ║  Health: 1000          ║             │
│  ║  Armor: 50             ║             │
│  ║  Speed: High           ║             │
│  ╚════════════════════════╝             │
└─────────────────────────────────────────┘
```

**Steps:**

1. **In ShipSelectionPanel, create:**
   - Back Button (top-left) → OnClick: PanelManager.ShowHome()
   - Title Text: "SHIPS"
   - Scroll View for ship cards
   - Stats display panel (bottom)

2. **Create ShipCard prefab:**
   - Button with ship icon
   - Ship name text
   - "Equip" button or Lock icon
   - Highlight when equipped

3. **Create script:** `ShipSelectionUI.cs`
   - Loads all unlocked ships from PlayerAccountData
   - Displays ship cards
   - Handles equipping ships
   - Updates ShipViewer3D when ship changes

### **Missile Loadout Panel**

Similar to Ship Selection, but for missiles.

**Layout:**
```
┌─────────────────────────────────────────┐
│  [← Back]           MISSILES             │
│                                          │
│  ┌────┐  ┌────┐  ┌────┐  ┌────┐        │
│  │ M1 │  │ M2 │  │ M3 │  │ M4 │        │
│  └────┘  └────┘  └────┘  └────┘        │
│  [Equip] [Equip] [🔒]   [🔒]           │
│                                          │
│  ╔════════════════════════╗             │
│  ║  Hellfire MK-2         ║             │
│  ║  Mass: 500 lbs         ║             │
│  ║  Speed: 250 m/s        ║             │
│  ║  Damage: 2500          ║             │
│  ╚════════════════════════╝             │
└─────────────────────────────────────────┘
```

### **Battle Pass Panel**

Shows current season progression.

**Layout:**
```
┌─────────────────────────────────────────┐
│  [← Back]         BATTLE PASS            │
│                                          │
│  Season 1: Cosmic Warfare                │
│  ════════════════════════ 45%            │
│  Level 12 / 100                          │
│                                          │
│  FREE TRACK:                             │
│  [✓] [✓] [✓] [○] [○] [○] ...           │
│                                          │
│  PREMIUM TRACK:                          │
│  [✓] [✓] [✓] [○] [○] [○] ...           │
│                                          │
│  [Upgrade to Premium - 1000 gems]        │
└─────────────────────────────────────────┘
```

### **Quests Panel**

Shows daily, weekly, and season quests.

**Layout:**
```
┌─────────────────────────────────────────┐
│  [← Back]            QUESTS              │
│                                          │
│  DAILY QUESTS              [Refresh: 8h] │
│  ┌──────────────────────────────────┐   │
│  │ Win 3 matches         [2/3] ○○○  │   │
│  │ Reward: 50 credits               │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ Deal 5000 damage      [✓] ████  │   │
│  │ Reward: 25 XP         [Claim]   │   │
│  └──────────────────────────────────┘   │
│                                          │
│  WEEKLY QUESTS           [Refresh: 3d]  │
│  ...                                     │
└─────────────────────────────────────────┘
```

### **Profile Panel**

Shows player stats and achievements.

**Layout:**
```
┌─────────────────────────────────────────┐
│  [← Back]           PROFILE              │
│                                          │
│  Player: Ace_Pilot_42                    │
│  Level: 12  |  Rank: Gold III            │
│  ELO Rating: 1450                        │
│                                          │
│  STATISTICS:                             │
│  Total Matches: 127                      │
│  Wins: 84  |  Losses: 43                 │
│  Win Rate: 66%                           │
│  Missiles Fired: 1,247                   │
│  Accuracy: 62%                           │
│                                          │
│  RECENT ACHIEVEMENTS:                    │
│  [🏆] First Blood                        │
│  [🏆] Sharpshooter                       │
│  [🔒] Untouchable                        │
└─────────────────────────────────────────┘
```

### **Settings Panel**

Game settings and options.

**Layout:**
```
┌─────────────────────────────────────────┐
│  [← Back]          SETTINGS              │
│                                          │
│  AUDIO                                   │
│  Master Volume:  ████████░░ 80%          │
│  Music Volume:   ██████░░░░ 60%          │
│  SFX Volume:     ██████████ 100%         │
│                                          │
│  GRAPHICS                                │
│  Quality: [High ▼]                       │
│  VSync: [✓] Enabled                      │
│                                          │
│  CONTROLS                                │
│  Mouse Sensitivity: ███████░░░ 70%       │
│                                          │
│  ACCOUNT                                 │
│  [Change Username]                       │
│  [Logout]                                │
└─────────────────────────────────────────┘
```

---

## 🎯 Part 9: Integration Checklist

Make sure these systems are connected:

- [ ] **ProgressionManager** - Loads player data on menu start
- [ ] **SaveSystem** - Auto-saves when equipping ships/missiles
- [ ] **ShipViewer3D** - Updates when ship changes
- [ ] **CurrencyDisplay** - Updates after shop purchases
- [ ] **QuestSystem** - Displays active quests
- [ ] **BattlePassSystem** - Shows season progress
- [ ] **AchievementSystem** - Displays unlocked achievements

---

## 🚀 Part 10: Next Steps

After completing this main menu:

1. **Create ship models** - Add 3D models for each ship to Resources/Ships/
2. **Populate data** - Create ShipBodySO and MissilePresetSO assets
3. **Add animations** - Panel transitions, button hover effects
4. **Polish UI** - Add icons, better fonts, gradients
5. **Implement panels** - Flesh out each panel with full functionality
6. **Connect to online** - Wire up "Play Online" button to matchmaking

---

## 📚 Helpful Resources

**Free UI Assets:**
- Unity Asset Store: Search "Sci-Fi UI"
- FontAwesome: Free icons
- Google Fonts: Free game fonts

**Ship Models:**
- Asset Store: Search "Spaceship"
- Sketchfab: Free 3D models

**References:**
- Brawl Stars main menu
- Clash Royale main menu
- Any modern mobile game hub

---

## ✅ Testing Checklist

- [ ] Ship viewer displays and rotates
- [ ] All navigation buttons work
- [ ] Panels switch correctly
- [ ] Back buttons return to home
- [ ] Currency displays correctly
- [ ] Ship selection works
- [ ] Missile selection works
- [ ] Data persists after restart
- [ ] No console errors
- [ ] Smooth transitions

---

**Estimated Total Time:** 4-6 hours for basic implementation, 10-15 hours for full polish

Once this is done, you'll have a professional main menu hub that rivals commercial games!
