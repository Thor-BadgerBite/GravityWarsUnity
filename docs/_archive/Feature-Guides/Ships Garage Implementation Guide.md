# Ships Garage Implementation Guide

This guide explains how to set up the Ships Garage panel in Unity using a **simplified toggle-based approach**.

## Overview

The Ships Garage panel displays:
- **Left Side**: Ship info panel with stats, 3D model, and equip button
- **Right Side**: Scrollable inventory with ship cards and archetype filter tabs
- **Top**: Title bar with close button (X) and cancel button

## Design Approach: Simple Toggle Cards (RECOMMENDED)

This implementation uses **minimal ship cards** with just an icon and Unity's Toggle component:

**Ship Card Structure:**
```
ShipCardPrefab
├── ShipIcon (Image)
└── Toggle (on root GameObject)
```

**How it works:**
1. Click a ship card → Card's Toggle becomes selected
2. The controller receives the selection event
3. Stats panel updates with selected ship's information:
   - NameText = ship name
   - TypeText = ship type (Tank, Damage Dealer, etc.)
   - LevelText = "Level: ##"
   - DamageText = numerical damage with equipped missile
   - HealthText = current health
   - ArmorText = current armor
   - XpProgressBar = XP progress filled image
   - XPText = currentXP/totalXP
4. Model viewer displays the selected ship's 3D model
5. Equip button becomes enabled (unless ship is already equipped)
6. If the selected ship is already equipped, the Equip button is disabled
7. Clicking Equip equips the ship and closes the garage window
8. Cancel button closes the window without making changes

**Benefits:**
- Minimal UI complexity
- Built-in toggle group ensures only one ship is selected at a time
- Clear visual feedback via Unity's Toggle component
- Easy to set up in Unity Editor

## Created Scripts

### 1. ShipsGarageController.cs
**Location**: `Assets/UI/ShipsGarage/ShipsGarageController.cs`

Main controller that manages:
- Ship selection and filtering
- Integration with ProgressionManager
- Equipping ships as active
- Communication between UI and 3D viewer

### 2. ShipsGarageUI.cs
**Location**: `Assets/UI/ShipsGarage/ShipsGarageUI.cs`

UI manager that handles:
- Displaying ship info panel
- Managing scrollable inventory
- Archetype tab switching
- Fade in/out animations
- Button interactions

### 3. ShipInventoryCard.cs
**Location**: `Assets/UI/ShipsGarage/ShipInventoryCard.cs`

Minimal ship card component that displays:
- Ship icon only
- Uses Unity Toggle for selection (requires Toggle component)
- Part of a ToggleGroup for mutual exclusivity
- Notifies controller when selected

## Unity Editor Setup

### Step 1: Create the Ships Garage Panel

1. In your MainMenu scene, create a new Canvas child object:
   - **Name**: `ShipsGaragePanel`
   - Add `CanvasGroup` component
   - Set to initially inactive (will be shown when player clicks Ships button)

2. Add the main panel structure:
   ```
   ShipsGaragePanel (Canvas)
   ├── Background (Image - semi-transparent dark overlay)
   ├── MainPanel (Image - your sci-fi frame)
   │   ├── TopBar
   │   │   ├── Title (TextMeshProUGUI - "SHIPS GARAGE")
   │   │   ├── CloseButton (Button with "X" icon)
   │   │   └── CancelButton (Button - closes window)
   │   ├── ShipInfoPanel (Left side)
   │   │   ├── ShipModelContainer (for 3D view)
   │   │   ├── StatsPanel
   │   │   │   ├── NameText (TextMeshProUGUI - ship name)
   │   │   │   ├── TypeText (TextMeshProUGUI - ship type/archetype)
   │   │   │   ├── LevelText (TextMeshProUGUI - "Level: ##")
   │   │   │   ├── StatsGroup
   │   │   │   │   ├── DamageText (TextMeshProUGUI - numerical damage)
   │   │   │   │   ├── HealthText (TextMeshProUGUI - health value)
   │   │   │   │   └── ArmorText (TextMeshProUGUI - armor value)
   │   │   │   └── XPGroup
   │   │   │       ├── XPProgressBar (Image with Fill - XP progress)
   │   │   │       └── XPText (TextMeshProUGUI - "currentXP/totalXP")
   │   │   └── EquipButton (Button - large, bottom of panel)
   │   │       ├── ButtonText ("EQUIP" - you handle sprite swap)
   │   │       └── EquippedIndicator (optional checkmark icon)
   │   └── InventoryPanel (Right side)
   │       ├── TabsGroup
   │       │   ├── AllTab (Button)
   │       │   ├── TankTab (Button)
   │       │   ├── DDTab (Button)
   │       │   ├── ControllerTab (Button)
   │       │   └── AllAroundTab (Button)
   │       └── ScrollView (ScrollRect)
   │           ├── Viewport
   │           │   └── Content (Grid Layout Group + Toggle Group)
   │           │       └── [Ship toggle cards spawn here]
   │           └── Scrollbar Vertical
   ```

   **Important**: Add a `ToggleGroup` component to the `Content` object. This ensures only one ship card can be selected at a time.

### Step 2: Create Ship Card Prefab (SIMPLIFIED TOGGLE APPROACH)

1. Create a new GameObject in the Project:
   - **Name**: `ShipCardPrefab`
   - **Location**: `Assets/Prefabs/UI/`

2. Add components to the prefab (MINIMAL DESIGN):
   ```
   ShipCardPrefab (with Toggle component)
   └── ShipIcon (Image - ship thumbnail)
   ```

3. Add `Toggle` component to the prefab root
   - Set Transition: Color Tint (or your preferred visual feedback)
   - Target Graphic: Set to ShipIcon or a background image
   - Normal Color: Default color
   - Highlighted Color: Slightly brighter
   - Pressed Color: Darker
   - Selected Color: Highlighted (e.g., gold tint)

4. Add `ShipInventoryCard` script to the prefab root

5. Assign references in the ShipInventoryCard component:
   - Ship Icon → ShipIcon Image

**That's it!** The card is now minimal and uses Unity's Toggle component for selection.

### Step 3: Setup 3D Ship Viewer

You already have `ShipViewer3D.cs` in the project, which is perfect!

1. In the `ShipsGaragePanel/MainPanel/ShipInfoPanel`:
   - Create a child object: `ShipModelContainer`
   - Add a new Camera (or reuse existing ship viewer camera):
     - Name: `ShipGarageCamera`
     - Clear Flags: Solid Color (or Depth Only if layered)
     - Culling Mask: Set to "Ships" layer
     - Depth: Higher than main camera
     - Target Display: Same as UI

2. Add `ShipViewer3D` component to the ShipModelContainer
   - Assign the camera reference
   - Configure lighting (can reuse main menu lights or create new ones)

### Step 4: Wire Up the Controller

1. Select the `ShipsGaragePanel` root object

2. Add these components:
   - `ShipsGarageController`
   - Assign references:
     - Garage UI → ShipsGarageUI component (add to MainPanel)
     - Ship Viewer → ShipViewer3D component

3. Select the `MainPanel` child object

4. Add `ShipsGarageUI` component:
   - Assign ALL references in the inspector:
     - **Main Panel:**
       - Main Canvas Group → CanvasGroup on root panel
       - Garage Panel → MainPanel object
     - **Top Bar:**
       - Title Text → "SHIPS GARAGE" text
       - Close Button → X button
       - Cancel Button → Cancel button (closes window)
     - **Ship Info Panel (Stats Display):**
       - Ship Info Panel → ShipInfoPanel object
       - Name Text → Ship name display
       - Type Text → Ship type/archetype display
       - Level Text → Ship level display (shows "Level: ##")
       - Damage Text → Damage value display (numerical)
       - Health Text → Health value display
       - Armor Text → Armor value display
       - XP Progress Bar → Image with Fill (fillAmount shows XP progress)
       - XP Text → XP text display (shows "currentXP/totalXP")
     - **Equip Button:**
       - Equip Button → Button (becomes disabled when ship is already equipped)
       - Equip Button Text → "EQUIP" text (you handle sprite swap)
       - Equipped Indicator → checkmark GameObject (optional)
     - **Inventory Panel:**
       - Inventory Panel → InventoryPanel object
       - Inventory Scroll Rect → ScrollRect component
       - Inventory Content → Content transform (inside ScrollView)
       - Ship Card Prefab → Your ShipCardPrefab
       - Ship Toggle Group → ToggleGroup component (add to Content or InventoryPanel)
     - **Archetype Tabs:**
       - All tab buttons (All, Tank, DD, Controller, All-Around)

### Step 5: Configure ScrollRect

1. Select the `ScrollView` object
2. Configure ScrollRect component:
   - Content → Content Transform
   - Viewport → Viewport Transform
   - Horizontal: OFF
   - Vertical: ON
   - Movement Type: Clamped
   - Scrollbar Vertical → Scrollbar (if you have one)

3. On the `Content` object:
   - Add `Grid Layout Group` component:
     - Cell Size: 200x250 (adjust based on your card size)
     - Spacing: 10x10 (horizontal and vertical gap between cards)
     - Start Corner: Upper Left
     - Start Axis: Horizontal (fills left to right first)
     - Child Alignment: Upper Left
     - Constraint: Fixed Column Count
     - Constraint Count: 4 (this creates 4 columns)
   - Add `Content Size Fitter` component:
     - Vertical Fit: Preferred Size
     - Horizontal Fit: Unconstrained

### Step 6: Connect to Main Menu

To open the Ships Garage from the main menu:

1. Open `MainMenuController.cs`
2. Find the `HandleShipsClicked()` method (around line 292)
3. Instead of loading a scene, activate the Ships Garage panel:

```csharp
private void HandleShipsClicked()
{
    Debug.Log("[MainMenuController] Ships garage selected");

    // Find and open the Ships Garage panel
    var garagePanel = FindObjectOfType<ShipsGarageController>(true);
    if (garagePanel != null)
    {
        garagePanel.OpenGarage();
    }
    else
    {
        Debug.LogError("[MainMenuController] ShipsGarageController not found in scene!");
    }
}
```

## Layout Specifications (Based on Mockup)

### Panel Dimensions
- **Main Panel**: Use your existing sci-fi frame sprite
- **Aspect**: Landscape (similar to mockup - roughly 16:9 or 16:10)
- **Anchor**: Center of screen

### Left Side - Ship Info Panel (~40% width)
- Ship Name: Large, bold text at top
- Type: Medium text below name
- Stats Layout:
  ```
  NAME:        VOYAGER MK-II
  TYPE:        BATTLECRUISER

  DAMAGE:      6500
  ARMOR:       6000
  HEALTH:      12000
  SPEED:       450
  RANGE:       1200

  XP:          [████████░░] 80%
  ```
- 3D Model: Center of panel, takes most space
- Equip Button: Large orange button at bottom ("EQUIP" text)
- Active Ship Icon: Small crossed swords icon next to Equip button when equipped

### Right Side - Inventory Panel (~60% width)
- **Tabs** at top (horizontal row):
  - Tab icons or text: [ALL] [TANK] [DD] [CTRL] [ALL]
  - Active tab: Gold/orange highlight
  - Inactive tabs: White/gray

- **Scrollable Grid** (4 columns):
  - Ships fill from left to right, then create new rows
  - Each card shows:
    - Ship thumbnail/icon (top)
    - Ship name (center)
    - Archetype badge (color-coded)
    - "ACTIVE" badge if equipped (green, top-right corner)
  - Cards have subtle hover/select states
  - Selected card has gold border

### Colors
- **Background**: Dark blue-gray (#1A1A25)
- **Panel Frame**: Metallic sci-fi border (from your asset pack)
- **Tank**: Red (#CC3333)
- **Damage Dealer**: Orange (#FF8833)
- **Controller**: Blue (#5599FF)
- **All-Around**: Green (#77CC77)
- **Equipped/Active**: Gold/Orange (#FFCC33)
- **Selection Highlight**: Gold (#FFD700)

### Fonts
Use TextMeshPro fonts:
- **Title**: Large, bold (32-40pt)
- **Ship Name**: Medium-large, bold (24-28pt)
- **Stats Labels**: Medium (16-20pt)
- **Card Text**: Small-medium (14-18pt)

## Testing Checklist

Once set up in Unity:

- [ ] Ships Garage opens when clicking Ships button in main menu
- [ ] All unlocked ships appear in inventory as toggle cards
- [ ] Ship cards display correctly with ship icons
- [ ] Toggle group ensures only one ship card is selected at a time
- [ ] Archetype tabs filter ships correctly
- [ ] Clicking a ship card selects it (Toggle visual feedback)
- [ ] Stats panel updates with selected ship's information:
  - [ ] NameText shows ship name
  - [ ] TypeText shows ship type (Tank, Damage Dealer, etc.)
  - [ ] LevelText shows "Level: ##"
  - [ ] DamageText shows numerical damage value
  - [ ] HealthText shows health value
  - [ ] ArmorText shows armor value
  - [ ] XpProgressBar fill amount reflects XP progress
  - [ ] XPText shows "currentXP/totalXP" format
- [ ] 3D ship model displays and rotates when ship is selected
- [ ] Equip button becomes enabled when ship is selected (and not already equipped)
- [ ] Equip button is disabled when the selected ship is already equipped
- [ ] Equipping a ship:
  - [ ] Updates the active ship in MainMenuController
  - [ ] Closes the Ships Garage window
- [ ] Cancel button closes the panel without making changes
- [ ] Close button (X) closes the panel
- [ ] Fade in/out animations work smoothly

## Common Issues & Solutions

### Issue: Ships not appearing in inventory
**Solution**: Check that ProgressionManager has ships unlocked. Run the game and check the console for "Granted starter ships" message.

### Issue: 3D ship model not visible
**Solution**:
1. Check camera culling mask includes Ships layer
2. Verify ship prefabs are in Resources folder (or correct path)
3. Check lighting setup in ShipViewer3D

### Issue: Stats not calculating correctly
**Solution**: Ensure ShipBodySO assets have proper base stats configured. Check that leveling formulas are assigned.

### Issue: Equip button not responding
**Solution**: Verify that all button OnClick events are properly wired up in ShipsGarageUI.Setup().

## Next Steps

After basic setup:

1. **Polish Animations**: Add more tweening effects with LeanTween
2. **Sound Effects**: Add UI sounds for clicks, equip, etc.
3. **Particle Effects**: Add sparkles when equipping a ship
4. **Ship Previews**: Generate ship thumbnail icons if not already created
5. **Search/Filter**: Add search bar if you have many ships
6. **Sort Options**: Add sorting by level, name, archetype, etc.

## Integration with Existing Systems

The Ships Garage automatically integrates with:

- **ProgressionManager**: Reads unlocked ships and progression data
- **PlayerAccountData**: Reads/writes currentEquippedShipId
- **ShipViewer3D**: Reuses existing 3D viewer (same as main menu)
- **MainMenuController**: Updates active ship when equipped

## File Structure

```
Assets/
├── UI/
│   └── ShipsGarage/
│       ├── ShipsGarageController.cs      (Created ✓)
│       ├── ShipsGarageUI.cs              (Created ✓)
│       └── ShipInventoryCard.cs          (Created ✓)
├── Prefabs/
│   └── UI/
│       └── ShipCardPrefab.prefab         (Create in Unity)
└── Scenes/
    └── MainMenuScene.unity               (Add ShipsGaragePanel here)
```

## Script References Summary

### ShipsGarageController
- **Responsibilities**: Ship selection logic, equipping, filtering
- **Key Methods**:
  - `SelectShip(ShipBodySO)`: Select a ship to preview
  - `EquipSelectedShip()`: Set selected ship as active
  - `ApplyFilter()`: Filter ships by archetype
  - `OpenGarage()` / `CloseGarage()`: Show/hide panel

### ShipsGarageUI
- **Responsibilities**: UI updates, animations, user input
- **Key Methods**:
  - `DisplayShips(List<ShipBodySO>)`: Populate inventory
  - `UpdateShipInfo(ShipBodySO, ShipProgressionEntry, bool)`: Update stats panel
  - `FadeIn()` / `FadeOut()`: Transition animations

### ShipInventoryCard
- **Responsibilities**: Minimal card display using Toggle component
- **Key Methods**:
  - `Setup(ShipBodySO, bool)`: Initialize card with ship data and icon
  - `SetToggleGroup(ToggleGroup)`: Assign the card to a toggle group
  - `SetSelected(bool)`: Programmatically select/deselect the toggle
  - `HandleToggleChanged(bool)`: Internal - fires OnCardClicked event when toggle is selected

---

## Quick Setup Summary

**5-Minute Setup** (if you have UI assets ready):

1. Create ShipsGaragePanel GameObject in MainMenu scene
2. Add ShipsGarageController and ShipsGarageUI components
3. Create minimal ShipCardPrefab with Toggle + ShipInventoryCard components
4. Add ToggleGroup component to inventory Content object
5. Wire up all references in Inspector (including the new Ship Toggle Group)
6. Connect MainMenuController's HandleShipsClicked() to OpenGarage()
7. Test in Play mode!

**Key Points for the Simplified Approach:**
- Ship cards only need an icon and a Toggle component
- The ToggleGroup ensures mutual exclusivity (only one ship selected at a time)
- Stats panel updates when a ship card is clicked
- Equip button automatically closes the window after equipping
- Cancel button closes the window without changes

**Need Help?**
- Check Unity Console for detailed error messages
- All scripts include Debug.Log statements for troubleshooting
- Ensure ProgressionManager is initialized before opening garage

---

**Implementation Status**: Scripts created ✓ | Unity setup required ⚙️

Good luck! 🚀
