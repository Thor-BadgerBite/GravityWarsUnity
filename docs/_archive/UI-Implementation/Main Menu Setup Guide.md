# Main Menu Setup Guide (Brawl Stars Style)

Complete step-by-step guide to create a professional main menu with 3D ship viewer, similar to Brawl Stars.

---

## Table of Contents

1. [Overview](#overview)
2. [Scene Setup](#scene-setup)
3. [3D Ship Viewer Setup](#3d-ship-viewer-setup)
4. [UI Canvas Setup](#ui-canvas-setup)
5. [Player Info Panel](#player-info-panel)
6. [Game Mode Buttons](#game-mode-buttons)
7. [Navigation Buttons](#navigation-buttons)
8. [Rank Icons Setup](#rank-icons-setup)
9. [Testing](#testing)
10. [Troubleshooting](#troubleshooting)

---

## Overview

The main menu consists of:
- **3D Ship Viewer**: Displays player's equipped ship in the center with rotation
- **Player Info Panel**: Shows username, level, XP bar, ELO, rank
- **Currency Display**: Credits and gems
- **Game Mode Buttons**: Ranked, Casual, Local Hotseat, Training
- **Navigation**: Ships, Achievements, Settings, Profile, Leaderboard, Quests

---

## Scene Setup

### Step 1: Create New Scene

1. In Unity, create a new scene: **File > New Scene**
2. Name it **"MainMenu"**
3. Save to **Assets/Scenes/MainMenu.unity**

### Step 2: Lighting Setup

1. **Directional Light** (Main Light):
   - Create: **GameObject > Light > Directional Light**
   - Name: **"MainLight"**
   - Rotation: **(50, -30, 0)**
   - Color: **#FFFAF0** (warm white)
   - Intensity: **1.2**

2. **Directional Light** (Fill Light):
   - Create: **GameObject > Light > Directional Light**
   - Name: **"FillLight"**
   - Rotation: **(-20, 120, 0)**
   - Color: **#8099CC** (blue)
   - Intensity: **0.5**

### Step 3: Camera Setup

1. Delete the default **Main Camera**
2. Create new camera: **GameObject > Camera**
3. Name: **"UICamera"**
4. Settings:
   - Clear Flags: **Solid Color**
   - Background: **#1A1A26** (dark blue)
   - Culling Mask: **UI**

---

## 3D Ship Viewer Setup

### Step 1: Create Ship Viewer Container

1. Create empty GameObject: **GameObject > Create Empty**
2. Name: **"ShipViewerSystem"**
3. Position: **(0, 0, 0)**

### Step 2: Create Ship Container

1. Right-click **ShipViewerSystem** > **Create Empty**
2. Name: **"ShipContainer"**
3. Position: **(0, 0, 0)**
4. This will hold the instantiated ship

### Step 3: Create Ship Camera

1. Right-click **ShipViewerSystem** > **Camera**
2. Name: **"ShipCamera"**
3. Settings:
   - Position: **(0, 2, -10)**
   - Rotation: **(0, 0, 0)**
   - Clear Flags: **Solid Color**
   - Background: **#1A1A26**
   - Culling Mask: **Default** (or create "Ship" layer)
   - Depth: **0** (renders before UI)
   - Target Display: **Display 1**

4. Make camera look at ship container:
   - Select **ShipCamera**
   - In Inspector, you can manually adjust rotation or use a constraint

### Step 4: Add ShipViewer3D Component

1. Select **ShipViewerSystem**
2. **Add Component > ShipViewer3D**
3. Configure in Inspector:
   - **Ship Container**: Drag **ShipContainer** object
   - **Ship Camera**: Drag **ShipCamera** object
   - **Camera Distance**: **10**
   - **Camera Offset**: **(0, 2, -10)**
   - **Enable Auto Rotation**: **✓**
   - **Auto Rotation Speed**: **15**
   - **Enable Manual Rotation**: **✓**
   - **Ship Scale**: **1** (adjust based on your ship size)
   - **Main Light**: Drag **MainLight**
   - **Fill Light**: Drag **FillLight**

---

## UI Canvas Setup

### Step 1: Create Main Canvas

1. **GameObject > UI > Canvas**
2. Name: **"MainMenuCanvas"**
3. Canvas settings:
   - Render Mode: **Screen Space - Overlay**
   - Pixel Perfect: **✓**
   - Canvas Scaler:
     - UI Scale Mode: **Scale With Screen Size**
     - Reference Resolution: **1920 x 1080**
     - Screen Match Mode: **Match Width Or Height**
     - Match: **0.5** (balance between width and height)

### Step 2: Add Canvas Group for Fade Animations

1. Select **MainMenuCanvas**
2. **Add Component > Canvas Group**
3. This allows fade in/out animations

### Step 3: Create Background Panel (Optional)

1. Right-click **MainMenuCanvas** > **UI > Panel**
2. Name: **"BackgroundPanel"**
3. Color: **#0D0D14** (very dark blue, 90% alpha)
4. This adds a subtle overlay over the ship viewer

---

## Player Info Panel

### Step 1: Create Player Info Container

1. Right-click **MainMenuCanvas** > **UI > Panel**
2. Name: **"PlayerInfoPanel"**
3. Position:
   - **Anchor**: Top-Left
   - **Position**: **(20, -20, 0)**
   - **Width**: **400**
   - **Height**: **200**

### Step 2: Username Text

1. Right-click **PlayerInfoPanel** > **UI > Text - TextMeshPro**
2. Name: **"UsernameText"**
3. Settings:
   - Text: **"PlayerName"**
   - Font Size: **36**
   - Color: **#FFFFFF**
   - Alignment: **Left, Top**
   - Position: **(10, -10)**

### Step 3: Level Text

1. Right-click **PlayerInfoPanel** > **UI > Text - TextMeshPro**
2. Name: **"LevelText"**
3. Settings:
   - Text: **"Level 1"**
   - Font Size: **24**
   - Color: **#FFD700** (gold)
   - Alignment: **Left, Top**
   - Position: **(10, -50)**

### Step 4: XP Bar

1. **Background**:
   - Right-click **PlayerInfoPanel** > **UI > Image**
   - Name: **"XPBarBackground"**
   - Color: **#2A2A3A**
   - Width: **380**, Height: **20**
   - Position: **(10, -90)**

2. **Fill**:
   - Right-click **XPBarBackground** > **UI > Image**
   - Name: **"XPFillBar"**
   - Color: **#4CAF50** (green)
   - Image Type: **Filled**
   - Fill Method: **Horizontal**
   - Fill Amount: **0.5** (50%)
   - Stretch to fill parent

3. **XP Text**:
   - Right-click **XPBarBackground** > **UI > Text - TextMeshPro**
   - Name: **"XPText"**
   - Text: **"500 / 1000 XP"**
   - Font Size: **16**
   - Color: **#FFFFFF**
   - Alignment: **Center**

### Step 5: ELO and Rank Display

1. **ELO Text**:
   - Right-click **PlayerInfoPanel** > **UI > Text - TextMeshPro**
   - Name: **"ELOText"**
   - Text: **"1200 ELO"**
   - Font Size: **28**
   - Color: **#FFD700** (gold)
   - Position: **(10, -130)**

2. **Rank Icon**:
   - Right-click **PlayerInfoPanel** > **UI > Image**
   - Name: **"RankIcon"**
   - Width: **48**, Height: **48**
   - Position: **(10, -170)**

3. **Rank Text**:
   - Right-click **PlayerInfoPanel** > **UI > Text - TextMeshPro**
   - Name: **"RankText"**
   - Text: **"Gold"**
   - Font Size: **20**
   - Color: **#FFD700**
   - Position: **(70, -180)**

---

## Currency Display

### Step 1: Create Currency Panel

1. Right-click **MainMenuCanvas** > **UI > Panel**
2. Name: **"CurrencyPanel"**
3. Position:
   - **Anchor**: Top-Right
   - **Position**: **(-20, -20, 0)**
   - **Width**: **300**
   - **Height**: **100**

### Step 2: Credits Display

1. **Credits Icon** (optional):
   - Right-click **CurrencyPanel** > **UI > Image**
   - Name: **"CreditsIcon"**
   - Width: **32**, Height: **32**

2. **Credits Text**:
   - Right-click **CurrencyPanel** > **UI > Text - TextMeshPro**
   - Name: **"CreditsText"**
   - Text: **"1000"**
   - Font Size: **24**
   - Color: **#FFD700** (gold)

### Step 3: Gems Display

Similar to credits, but position below:

1. **Gems Icon**
2. **Gems Text** - Color: **#00FFFF** (cyan)

---

## Game Mode Buttons

### Step 1: Create Button Container

1. Right-click **MainMenuCanvas** > **UI > Panel**
2. Name: **"GameModesPanel"**
3. Position:
   - **Anchor**: Bottom-Center
   - **Position**: **(0, 100, 0)**
   - **Width**: **800**
   - **Height**: **120**

### Step 2: Create Ranked Button

1. Right-click **GameModesPanel** > **UI > Button - TextMeshPro**
2. Name: **"RankedButton"**
3. Settings:
   - Width: **180**, Height: **100**
   - Position: **(-300, 0, 0)**
   - Colors:
     - Normal: **#2196F3** (blue)
     - Highlighted: **#42A5F5**
     - Pressed: **#1976D2**
     - Selected: **#1E88E5**

4. **Button Text**:
   - Select **RankedButton > Text (TMP)**
   - Text: **"RANKED"**
   - Font Size: **24**
   - Color: **#FFFFFF**
   - Bold: **✓**

### Step 3: Create Other Game Mode Buttons

Repeat for:
- **CasualButton** - Position: **(-100, 0, 0)** - Color: **#4CAF50** (green)
- **LocalHotseatButton** - Position: **(100, 0, 0)** - Color: **#FF9800** (orange)
- **TrainingButton** - Position: **(300, 0, 0)** - Color: **#9C27B0** (purple)

---

## Navigation Buttons

### Step 1: Create Navigation Bar

1. Right-click **MainMenuCanvas** > **UI > Panel**
2. Name: **"NavigationBar"**
3. Position:
   - **Anchor**: Bottom
   - **Position**: **(0, 0, 0)**
   - **Width**: **1920** (full width)
   - **Height**: **80**
   - **Color**: **#1A1A26AA** (dark with transparency)

### Step 2: Create Navigation Buttons

Create small icon buttons for:
- **ShipsButton** - "🚀 Ships"
- **AchievementsButton** - "🏆 Achievements"
- **SettingsButton** - "⚙ Settings"
- **ProfileButton** - "👤 Profile"
- **LeaderboardButton** - "📊 Leaderboard"
- **QuestsButton** - "📜 Quests"

Position them evenly across the navigation bar.

---

## Rank Icons Setup

### Step 1: Import Rank Icons

1. Create rank icons (or download) for:
   - Bronze (brown/copper color)
   - Silver (silver/gray color)
   - Gold (gold/yellow color)
   - Platinum (platinum/white-blue color)
   - Diamond (diamond/light blue color)
   - Master (purple/red color)
   - Grandmaster (rainbow/prismatic color)

2. Import to **Assets/UI/Icons/Ranks/**

3. Set Texture Type to **Sprite (2D and UI)**

### Step 2: Assign to MainMenuUI

1. Select **MainMenuCanvas**
2. **Add Component > MainMenuUI**
3. In Inspector, assign all UI elements:
   - Player Info Panel references
   - Currency Panel references
   - Game Mode Buttons
   - Navigation Buttons
   - **Rank Icons**: Drag all 7 rank sprites

---

## Final Assembly

### Step 1: Add MainMenuController

1. Create empty GameObject: **GameObject > Create Empty**
2. Name: **"MainMenuController"**
3. **Add Component > MainMenuController**
4. Configure in Inspector:
   - **Ship Viewer**: Drag **ShipViewerSystem**
   - **Menu UI**: Drag **MainMenuCanvas**
   - **Scene Names**: Enter scene names for each mode
   - **Music Source**: Add AudioSource and assign music clip

### Step 2: Configure Ship Resources

1. Create folder: **Assets/Resources/Ships/**
2. Place your ship prefabs here
3. Name your starter ship prefab: **"starter_ship"**

**Important**: The ShipViewer3D loads ships from Resources folder, so ships MUST be in a Resources folder!

### Step 3: Test in Editor

1. Press **Play**
2. You should see:
   - Empty ship container (no ship yet - need to be logged in)
   - UI panels with default values
   - Buttons should respond to clicks (check console for logs)

---

## Testing with Account System

### Step 1: Create Login Scene (Temporary)

Since the main menu requires authentication, create a simple login scene:

1. Create new scene: **"Login"**
2. Add UI buttons for Register/Login
3. Use AccountSystem to authenticate
4. On success, load **MainMenu** scene

### Step 2: Test Flow

1. Start from **Login** scene
2. Register new account (username: "TestPlayer", password: "test123")
3. Should automatically load **MainMenu** scene
4. Verify:
   - ✅ Starter ship displays and rotates
   - ✅ Username shows "TestPlayer"
   - ✅ Level shows "Level 1"
   - ✅ ELO shows "1200 ELO"
   - ✅ Rank shows "Gold"
   - ✅ Credits show "1000"
   - ✅ Buttons respond to clicks

---

## Layout Recommendations (Brawl Stars Style)

### Screen Layout

```
┌─────────────────────────────────────────────────────┐
│  [Player Info]                    [Credits] [Gems]  │
│  Username                                            │
│  Level 5                                             │
│  [====XP Bar===]                                     │
│  🏆 1200 ELO - Gold                                  │
│                                                      │
│                  ┌──────────┐                        │
│                  │          │                        │
│                  │   SHIP   │   ← 3D Ship Here      │
│                  │  (3D)    │                        │
│                  │          │                        │
│                  └──────────┘                        │
│                                                      │
│      [RANKED] [CASUAL] [HOTSEAT] [TRAINING]         │
│                                                      │
│ ─────────────────────────────────────────────────── │
│  🚀 Ships | 🏆 Achievements | ⚙ Settings | 👤 Profile│
└─────────────────────────────────────────────────────┘
```

### Color Scheme (Brawl Stars Inspired)

- **Background**: Dark blue/purple gradient (#0D0D14 → #1A1A26)
- **Primary Buttons**: Vibrant colors (blue, green, orange, purple)
- **Text**: White (#FFFFFF) and Gold (#FFD700)
- **Panels**: Dark semi-transparent (#1A1A26AA)
- **Accent**: Gold highlights for important info

### Font Recommendations

- **Headers**: Bold, sans-serif (e.g., "Montserrat Bold", "Poppins Bold")
- **Body**: Regular sans-serif (e.g., "Roboto", "Open Sans")
- **Numbers**: Monospace for scores/stats

---

## Advanced Features (Optional)

### Animated Background

1. Add particle system behind ship
2. Slow-moving stars or energy particles
3. Color: Light blue (#42A5F5), low alpha

### Ship Spotlight

1. Add Spotlight pointing at ship
2. Color: White with blue tint
3. Intensity: 2
4. Angle: 30
5. Position: Above and in front of ship

### UI Animations

1. **Entry Animations**:
   - Panels slide in from edges
   - Buttons scale up with bounce
   - Ship "materializes" with particles

2. **Idle Animations**:
   - Subtle pulse on ranked button
   - Floating animation on ship
   - Glow effect on currency

3. **Hover Effects**:
   - Button scale increase (1.1x)
   - Color brightening
   - Shadow/glow effect

### Audio

1. **Background Music**: Epic orchestral or electronic
2. **Button Click**: Short "pop" sound
3. **Ship Rotation**: Subtle whoosh
4. **UI Transitions**: Swoosh sounds

---

## Troubleshooting

### Ship Not Displaying

**Problem**: Ship container is empty

**Solutions**:
1. Check ship prefab is in **Resources/Ships/** folder
2. Verify ship ID matches filename exactly (case-sensitive)
3. Check console for load errors
4. Verify player profile has `currentEquippedShipId` set

### UI Elements Not Showing

**Problem**: Canvas elements not visible

**Solutions**:
1. Check Canvas Render Mode is **Screen Space - Overlay**
2. Verify Canvas Scaler settings
3. Check if elements are outside camera view
4. Verify CanvasGroup alpha is 1

### Ship Too Small/Large

**Problem**: Ship doesn't fit viewport

**Solutions**:
1. Adjust **Ship Scale** in ShipViewer3D (try 0.5 - 2.0)
2. Adjust **Camera Distance** (try 5 - 15)
3. Adjust ship's local position offset

### Buttons Not Responding

**Problem**: Clicks not registering

**Solutions**:
1. Check if EventSystem exists in scene
2. Verify buttons have Graphic Raycaster
3. Check if canvas is blocking clicks
4. Verify MainMenuUI component is attached and configured

### Profile Not Loading

**Problem**: "Player not signed in" error

**Solutions**:
1. Ensure AccountSystem is initialized before MainMenu loads
2. Check if player logged in successfully
3. Verify AccountSystem singleton exists in scene
4. Add DontDestroyOnLoad to AccountSystem

---

## Performance Optimization

### For Mobile Devices

1. **Reduce Shadow Quality**:
   - Main Light: Shadows = No Shadows (or Soft Shadows on high-end)

2. **Optimize Ship Model**:
   - Use LOD (Level of Detail) system
   - Keep poly count under 5000 triangles

3. **UI Optimization**:
   - Disable raycast on non-interactive elements
   - Use sprite atlases for icons
   - Reduce overdraw with alpha

4. **Lighting**:
   - Bake lights if possible
   - Use Light Probes for dynamic objects

---

## Next Steps

1. **Create Ship Garage Scene**: Where players can view/select ships
2. **Implement Matchmaking UI**: Queue screen with countdown
3. **Add Notifications System**: Show achievement unlocks, messages
4. **Create Settings Menu**: Audio, graphics, controls
5. **Build Profile Screen**: Stats, match history, achievements

---

## Example Prefab Structure

For easy reuse, save as prefab:

```
MainMenuPrefab
├── ShipViewerSystem
│   ├── ShipContainer
│   ├── ShipCamera
│   ├── MainLight
│   └── FillLight
├── MainMenuCanvas
│   ├── PlayerInfoPanel
│   │   ├── UsernameText
│   │   ├── LevelText
│   │   ├── XPBar
│   │   ├── ELOText
│   │   ├── RankIcon
│   │   └── RankText
│   ├── CurrencyPanel
│   │   ├── CreditsText
│   │   └── GemsText
│   ├── GameModesPanel
│   │   ├── RankedButton
│   │   ├── CasualButton
│   │   ├── LocalHotseatButton
│   │   └── TrainingButton
│   └── NavigationBar
│       ├── ShipsButton
│       ├── AchievementsButton
│       ├── SettingsButton
│       └── [...more buttons]
└── MainMenuController
```

---

## Resources

- **TextMeshPro**: Import from Package Manager
- **LeanTween**: For smooth animations (optional, or use DOTween)
- **Free UI Assets**: Unity Asset Store
- **Rank Icons**: Create in Photoshop/GIMP or commission artist

---

**That's it! You now have a professional Brawl Stars-style main menu with 3D ship viewer!** 🚀
