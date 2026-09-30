# Main Menu Hub - Complete Build Guide
**Step-by-Step Implementation Instructions**

---

## 📋 Table of Contents

1. [Asset Requirements](#asset-requirements)
2. [Icon Specifications](#icon-specifications)
3. [Unity Setup](#unity-setup)
4. [Step-by-Step Building Process](#step-by-step-building-process)
5. [Component Settings Reference](#component-settings-reference)
6. [Testing Checklist](#testing-checklist)

---

## 🎨 Asset Requirements

### **Required Icons - Complete List**

Based on the provided file list screenshot, here are ALL the icons you need:

#### **Frame Assets**
- `frame_GameMode.png` - Container frame for game mode buttons
- `frame_LevelFrame.png` - Frame around level display
- `frame_Logo.png` - Frame for game logo
- `frame_XPFrame.png` - Frame for XP progress bar

#### **Left Side Navigation Icons (4 states each)**

**Ships Garage:**
- `icon_ShipGarage_Disabled.png`
- `icon_ShipGarage_Hover.png`
- `icon_ShipGarage_Normal.png`
- `icon_ShipGarage_Pressed.png`

**Inventory:**
- `icon_Inventory_Disabled.png`
- `icon_Inventory_Hover.png`
- `icon_Inventory_Normal.png`
- `icon_Inventory_Pressed.png`

**Leaderboards:**
- `icon_LeaderBoards_Disabled.png`
- `icon_LeaderBoards_Hover.png`
- `icon_LeaderBoards_Normal.png`
- `icon_LeaderBoards_Pressed.png`

**Friends:**
- `icon_Friends_Disabled.png`
- `icon_Friends_Hover.png`
- `icon_Friends_Normal.png`
- `icon_Friends_Pressed.png`

#### **Right Side Navigation Icons (4 states each)**

**Shop:**
- `icon_Shop_Disabled.png`
- `icon_Shop_Hover.png`
- `icon_Shop_Normal.png`
- `icon_Shop_Pressed.png`

**Achievements:**
- `icon_Achievements_Disabled.png`
- `icon_Achievements_Hover.png`
- `icon_Achievements_Normal.png`
- `icon_Achievements_Pressed.png`

**Account Progress:**
- `icon_Progress_Disabled.png`
- `icon_Progress_Hover.png`
- `icon_Progress_Normal.png`
- `icon_Progress_Pressed.png`

**Clan:**
- `icon_Clan_Disabled.png`
- `icon_Clan_Hover.png`
- `icon_Clan_Normal.png`
- `icon_Clan_Pressed.png`

#### **Main Center Buttons (4 states each)**

**Quests:**
- `icon_Quests_Disabled.png`
- `icon_Quests_Hover.png`
- `icon_Quests_Normal.png`
- `icon_Quests_Pressed.png`

**Play Now:**
- `icon_Play_Disabled.png`
- `icon_Play_Hover.png`
- `icon_Play_Normal.png`
- `icon_Play_Pressed.png`

**Battle Pass:**
- `icon_BattlePass_Disabled.png`
- `icon_BattlePass_Hover.png`
- `icon_BattlePass_Normal.png`
- `icon_BattlePass_Pressed.png`

#### **Game Mode Buttons (3 states each - NO DISABLED STATE)**

**Local:**
- `icon_Local_Hover.png`
- `icon_Local_Normal.png`
- `icon_Local_Toggled.png`

**Online:**
- `icon_Online_Hover.png`
- `icon_Online_Normal.png`
- `icon_Online_Toggled.png`

**Ranked:**
- `icon_Ranked_Hover.png`
- `icon_Ranked_Normal.png`
- `icon_Ranked_Toggled.png`

#### **Currency and Special Icons**

**Coins:**
- `icon_Coins_Frame.png`
- `icon_CoinsPlus_Disabled.png`
- `icon_CoinsPlus_Hover.png`
- `icon_CoinsPlus_Normal.png`
- `icon_CoinsPlus_Pressed.png`

**Gems:**
- `icon_Gems_Frame.png`
- `icon_GemsPlus_Disabled.png`
- `icon_GemsPlus_Hover.png`
- `icon_GemsPlus_Normal.png`
- `icon_GemsPlus_Pressed.png`

**Notifications:**
- `icon_Reminder.png`

**Calendar:**
- `icon_Calendar.png`

**Battle Pass Progress:**
- `icon_BattlePassProgress_Frame.png`
- `icon_PlayerFrame.png`

#### **Rank Icons (16 total - Single version each, NO states)**
- `rank1.png` (Highest rank - Grand Admiral 3000+)
- `rank2.png` (Supreme Admiral 2850-2999)
- `rank3.png` (Fleet Admiral 2700-2849)
- `rank4.png` (High Admiral 2550-2699)
- `rank5.png` (Admiral 2400-2549)
- `rank6.png` (Vice Admiral 2250-2399)
- `rank7.png` (Rear Admiral Upper Half 2100-2249)
- `rank8.png` (Rear Admiral 1950-2099)
- `rank9.png` (Commodore 1800-1949)
- `rank10.png` (Senior Captain 1650-1799)
- `rank11.png` (Captain 1500-1649)
- `rank12.png` (Commander 1350-1499)
- `rank13.png` (Lieutenant Commander 1200-1349)
- `rank14.png` (Lieutenant 1050-1199)
- `rank15.png` ⭐ (Ensign 700-1049) **STARTING RANK (800)**
- `rank16.png` (Cadet 0-699 - Lowest rank)

**Note:** Missing icons to be created:
- `icon_MissileLoadout_*.png` (4 states)
- `icon_CustomBuilder_*.png` (4 states)
- `icon_Settings_*.png` (4 states)
- `frame_LoadoutFrame.png`

---

## 📐 Icon Specifications

### **Format and Size**
- **Format:** PNG with transparency (RGBA 32-bit)
- **Navigation Icons:** 512x512 pixels
- **Currency Icons:** 256x256 pixels
- **Rank Icons:** 512x512 pixels
- **Frames:** Variable size depending on use

### **Button State Visual Guidelines**

#### **4-State Buttons (Normal, Hover, Pressed, Disabled)**

**Normal State:**
- Base color: Medium gray/white (#B0B0B0)
- No glow or effects
- Clean, simple design

**Hover State:**
- Brighter: Pure white (#FFFFFF)
- Outer glow effect (blue, gold, or thematic color)
- Glow opacity: 70-80%
- Glow blur: 10-15px

**Pressed State:**
- Darker: Dark gray (#808080)
- Inner shadow or bevel effect
- Slightly "pushed in" appearance

**Disabled State:**
- Very dark gray (#404040)
- Opacity: 50%
- No glow or effects

#### **3-State Toggle Buttons (Normal, Hover, Toggled)**

**Normal State:**
- Same as above

**Hover State:**
- Same as above

**Toggled State:**
- Bright accent color (based on mode)
  - Local: Green glow
  - Online: Blue glow
  - Ranked: Gold/Orange glow
- Indicates active selection
- Strongest visual presence of the 3 states

---

## 🔧 Unity Setup

### **Phase 1: Import Assets**

#### **Step 1: Create Folder Structure**

Create this exact folder structure in your Unity project:

```
Assets/
├── UI/
│   └── MainMenuHub/
│       ├── Icons/
│       │   ├── Navigation/
│       │   ├── GameModes/
│       │   ├── Currency/
│       │   ├── Ranks/
│       │   └── Frames/
│       ├── Prefabs/
│       ├── Scripts/
│       ├── Fonts/
│       └── Animations/
```

#### **Step 2: Import All Icons**

1. **Import all PNG files** from your creation software to Unity
2. **Organize by folder:**
   - **Navigation:** All navigation button icons (Ships, Inventory, Leaderboards, Friends, Shop, Achievements, Progress, Clan, Quests, Play, BattlePass)
   - **GameModes:** Local, Online, Ranked icons
   - **Currency:** Coins, Gems, Plus button icons
   - **Ranks:** rank1.png through rank16.png
   - **Frames:** frame_*.png files

3. **Configure Import Settings** for ALL icons:
   - Select all icons in Unity Project window
   - In **Inspector:**
     - Texture Type: **Sprite (2D and UI)**
     - Sprite Mode: **Single**
     - Pixels Per Unit: **100**
     - Max Size: **2048** (for 512px icons) or **1024** (for 256px icons)
     - Format: **RGBA Compressed**
     - Compression: **High Quality**
     - Filter Mode: **Bilinear**
     - Click **Apply**

#### **Step 3: Import and Setup Fonts**

**Recommended Fonts:**
- **Primary (Headers/Buttons):** Orbitron Bold or Rajdhani Bold
- **Secondary (Body Text):** Roboto Regular
- **Monospace (Numbers):** Roboto Mono

**Download from Google Fonts** (free):
1. Go to https://fonts.google.com
2. Download Orbitron, Roboto, Roboto Mono
3. Extract TTF files

**Import to Unity:**
1. Place TTF files in `Assets/UI/MainMenuHub/Fonts/`
2. **Create TextMeshPro Font Assets:**
   - Window → TextMeshPro → Font Asset Creator
   - Source Font File: Select **Orbitron-Bold.ttf**
   - Sampling Point Size: **Auto Sizing**
   - Atlas Resolution: **2048 x 2048**
   - Character Set: **ASCII** (or custom if you need special characters)
   - Render Mode: **SDFAA**
   - Padding: **5**
   - Click **Generate Font Atlas**
   - Save as: `Orbitron-Bold SDF.asset`
3. Repeat for **Roboto-Regular** and **Roboto-Mono**

#### **Step 4: Verify LeanTween Installation** ⚠️ **CRITICAL**

LeanTween is **REQUIRED** for menu animations.

**Check if installed:**
1. Look for folder: `Assets/LeanTween/` or search for "LeanTween.cs"
2. If **NOT found**, install it:

**Installation:**
- **Option A:** Unity Asset Store
  - Window → Package Manager → My Assets
  - Search "LeanTween" (FREE)
  - Import

- **Option B:** GitHub
  - Download from: https://github.com/dentedpixel/LeanTween
  - Place `LeanTween.cs` in `Assets/Plugins/` or `Assets/LeanTween/`

**Verify:**
- Create temporary C# script
- Add line: `LeanTween.init();`
- Check for errors in Console
- If no errors → ✅ Ready
- Delete test script

---

## 🏗️ Step-by-Step Building Process

### **Phase 2: Create Main Menu Scene**

#### **Step 5: Create New Scene**

1. **File → New Scene**
2. Name: `MainMenuScene`
3. Save to: `Assets/Scenes/MainMenuScene.unity`
4. **Delete** default Main Camera (we'll use UI camera)

---

### **Phase 3: Setup 3D Ship Viewer System**

The 3D ship viewer is the visual centerpiece. Set this up first.

#### **Step 6: Create Ship Viewer GameObject**

1. **Create empty GameObject in scene:**
   - Right-click in Hierarchy → Create Empty
   - Name: `ShipViewerSystem`
   - Position: `(0, 0, 0)`

2. **Add child GameObject:**
   - Right-click `ShipViewerSystem` → Create Empty
   - Name: `ShipContainer`
   - Position: `(0, 0, 0)`
   - **This will hold the actual ship model**

#### **Step 7: Create Ship Viewer Camera**

1. **Add Camera:**
   - Right-click `ShipViewerSystem` → Camera
   - Name: `ShipViewerCamera`
   - Position: `(0, 2, -10)` (adjust based on your ship size)
   - Rotation: `(0, 0, 0)`

2. **Camera Component Settings:**
   - Clear Flags: **Solid Color**
   - Background: **Black** `#000000` or dark blue `#0A0A14`
   - Culling Mask: **Everything** (we'll use layer later if needed)
   - Projection: **Perspective**
   - Field of View: **40** (narrower FOV = less distortion)
   - Clipping Planes: Near **0.3**, Far **100**
   - Depth: **-2** (renders before UI camera)
   - Target Display: **Display 1**

#### **Step 8: Create Render Texture**

1. **Create Render Texture:**
   - In Project window: `Assets/UI/MainMenuHub/`
   - Right-click → Create → Render Texture
   - Name: `ShipViewerRT`

2. **Render Texture Settings:**
   - Size: **1024 x 1024** (square for clean rendering)
   - Depth Buffer: **16 bit** (needed for 3D rendering)
   - Anti-aliasing: **4x** (smooth edges)
   - Filter Mode: **Bilinear**
   - Wrap Mode: **Clamp**

3. **Assign to Camera:**
   - Select `ShipViewerCamera`
   - **Target Texture:** Drag `ShipViewerRT` here

#### **Step 9: Add Lighting for Ship**

1. **Create Directional Light:**
   - Right-click `ShipViewerSystem` → Light → Directional Light
   - Name: `ShipViewerLight`
   - Rotation: `(50, -30, 0)` (angle from top-left)

2. **Light Settings:**
   - Type: **Directional**
   - Color: **White** `#FFFFFF`
   - Intensity: **1.2**
   - Shadow Type: **No Shadows** (optional - shadows can be expensive for UI)
   - Culling Mask: **Everything** (or specific layer if you create one)

3. **Optional - Add Fill Light:**
   - Right-click `ShipViewerSystem` → Light → Directional Light
   - Name: `ShipViewerFillLight`
   - Rotation: `(-30, 150, 0)` (from opposite side)
   - Intensity: **0.4** (subtle fill)
   - Color: Slightly blue `#E0E0FF`

---

### **Phase 4: Build Main UI Canvas**

#### **Step 10: Create Main Canvas**

1. **Create Canvas:**
   - Hierarchy → Right-click → UI → Canvas
   - Name: `MainMenuCanvas`
   - **EventSystem** should be created automatically

2. **Canvas Component Settings:**
   - Render Mode: **Screen Space - Overlay**
   - Pixel Perfect: **✓** (checked)
   - Sort Order: **0**

3. **Canvas Scaler Settings:**
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Screen Match Mode: **Match Width Or Height**
   - Match: **0.5** (balanced scaling)
   - Reference Pixels Per Unit: **100**

4. **Add Canvas Group** (for fade animations):
   - Select `MainMenuCanvas`
   - Add Component → **Canvas Group**
   - Alpha: **1**
   - Interactable: **✓**
   - Block Raycasts: **✓**

#### **Step 11: Create Background**

1. **Create Panel:**
   - Right-click `MainMenuCanvas` → UI → Panel
   - Name: `BackgroundPanel`

2. **RectTransform:**
   - Anchor Preset: **Stretch/Stretch** (hold Alt+Shift, click bottom-right preset)
   - Left: **0**, Right: **0**, Top: **0**, Bottom: **0**

3. **Image Component:**
   - Color: Dark navy blue `#0D0D14`
   - Material: **None**
   - Raycast Target: **✓** (blocks clicks from going through)

---

### **Phase 5: Build Top Bar**

#### **Step 12: Create Top Bar Container**

1. **Create Empty GameObject:**
   - Right-click `MainMenuCanvas` → Create Empty
   - Name: `TopBar`

2. **RectTransform:**
   - Anchor: **Top Stretch**
   - Height: **180**
   - Left: **0**, Right: **0**, Top: **0**

---

#### **Step 13: Build Player Profile Panel (Top-Left)**

1. **Create Panel:**
   - Right-click `TopBar` → UI → Panel
   - Name: `PlayerProfilePanel`

2. **RectTransform:**
   - Anchor: **Top-Left**
   - Pivot: `(0, 1)`
   - Position: `(20, -20)` (20px from top-left corner)
   - Width: **280**
   - Height: **160**

3. **Image Component:**
   - Source Image: `frame_PlayerFrame.png` (if you have a frame)
   - Color: `#1A1A26` (dark blue-gray)
   - Material: **None**
   - Image Type: **Sliced** (if using 9-slice frame)

4. **Add Profile Icon:**
   - Right-click `PlayerProfilePanel` → UI → Image
   - Name: `ProfileIcon`
   - RectTransform:
     - Anchor: **Top-Left**
     - Position: `(15, -15)`
     - Width: **80**
     - Height: **80**
   - Image: Player avatar icon (placeholder for now)
   - **Button Component:**
     - Add Component → Button
     - Target Graphic: ProfileIcon Image
     - Transition: **Color Tint**
     - OnClick: Will wire later

5. **Add Level Display:**
   - Right-click `PlayerProfilePanel` → UI → Image
   - Name: `LevelFrame`
   - RectTransform:
     - Anchor: **Top-Left**
     - Position: `(105, -15)`
     - Width: **160**
     - Height: **40**
   - Source Image: `frame_LevelFrame.png`
   - Color: `#2C3E50`

6. **Add Level Text:**
   - Right-click `LevelFrame` → UI → Text - TextMeshPro
   - Name: `LevelText`
   - RectTransform: Stretch to fill parent
   - TextMeshPro Settings:
     - Text: "LVL 42"
     - Font Asset: `Orbitron-Bold SDF`
     - Font Size: **28**
     - Color: `#3498DB` (bright blue)
     - Alignment: **Center, Middle**
     - Extra Settings → Outline: **✓**
     - Outline Thickness: **0.2**
     - Outline Color: Black `#000000`

7. **Add Rank Icon:**
   - Right-click `PlayerProfilePanel` → UI → Image
   - Name: `RankIcon`
   - RectTransform:
     - Anchor: **Top-Right**
     - Position: `(-15, -15)`
     - Width: **50**
     - Height: **50**
   - Image: `rank15.png` (Ensign - starting rank)
   - Preserve Aspect: **✓**

8. **Add XP Progress Bar Background:**
   - Right-click `PlayerProfilePanel` → UI → Image
   - Name: `XPBarBackground`
   - RectTransform:
     - Anchor: **Bottom Stretch**
     - Position Y: **30**
     - Height: **25**
     - Left: **15**, Right: **15**
   - Source Image: `frame_XPFrame.png` (or solid color)
   - Color: `#2A2A3A` (dark purple-gray)
   - Image Type: **Sliced**

9. **Add XP Fill Bar:**
   - Right-click `XPBarBackground` → UI → Image
   - Name: `XPFillBar`
   - RectTransform: Stretch to fill parent (all zeros)
   - Image Component:
     - Color: `#4CAF50` (green)
     - Image Type: **Filled**
     - Fill Method: **Horizontal**
     - Fill Origin: **Left**
     - Fill Amount: **0.5** (50% - will be dynamic)

10. **Add XP Text:**
    - Right-click `XPBarBackground` → UI → Text - TextMeshPro
    - Name: `XPText`
    - RectTransform: Stretch to fill parent
    - TextMeshPro Settings:
      - Text: "8500 / 10000"
      - Font Asset: `Roboto-Regular SDF`
      - Font Size: **14**
      - Color: `#FFFFFF`
      - Alignment: **Center, Middle**
      - Outline: **✓**, Thickness **0.3**, Color Black

---

#### **Step 14: Build Logo and Notification Area (Top-Center)**

1. **Create Container:**
   - Right-click `TopBar` → Create Empty
   - Name: `LogoArea`
   - RectTransform:
     - Anchor: **Top Center**
     - Pivot: `(0.5, 1)`
     - Position: `(0, -20)`
     - Width: **600**
     - Height: **120**

2. **Add Game Logo:**
   - Right-click `LogoArea` → UI → Image
   - Name: `GameLogo`
   - RectTransform:
     - Anchor: **Middle Center**
     - Width: **500**
     - Height: **80**
   - Image: `frame_Logo.png` or your game logo
   - Preserve Aspect: **✓**

3. **Add Notification Button:**
   - Right-click `LogoArea` → UI → Button
   - Name: `NotificationButton`
   - RectTransform:
     - Anchor: **Middle Left**
     - Position: `(-280, 0)`
     - Width: **60**
     - Height: **60**

4. **Add Notification Icon:**
   - Right-click `NotificationButton` → UI → Image
   - Name: `NotificationIcon`
   - RectTransform: Stretch to fill parent
   - Image: `icon_Reminder.png`
   - Button Component:
     - Transition: **Sprite Swap**
     - Normal: icon_Reminder (normal state)
     - Highlighted: icon_Reminder (slightly brighter)
     - Pressed: icon_Reminder (darker)

5. **Add Calendar Button:**
   - Right-click `LogoArea` → UI → Button
   - Name: `CalendarButton`
   - RectTransform:
     - Anchor: **Middle Right**
     - Position: `(280, 0)`
     - Width: **60**
     - Height: **60**
   - Image: `icon_Calendar.png`
   - Button setup: Same as Notification

---

#### **Step 15: Build Currency Panel (Top-Right)**

1. **Create Panel:**
   - Right-click `TopBar` → UI → Panel
   - Name: `CurrencyPanel`
   - RectTransform:
     - Anchor: **Top-Right**
     - Pivot: `(1, 1)`
     - Position: `(-20, -20)`
     - Width: **320**
     - Height: **160**
   - Image:
     - Color: `#1A1A26`
     - Image Type: **Sliced**

2. **Create Coins Row:**
   - Right-click `CurrencyPanel` → Create Empty
   - Name: `CoinsRow`
   - RectTransform:
     - Anchor: **Top Stretch**
     - Height: **45**
     - Top: **15**
     - Left: **15**, Right: **15**

3. **Add Coins Icon:**
   - Right-click `CoinsRow` → UI → Image
   - Name: `CoinsIcon`
   - RectTransform:
     - Anchor: **Middle Left**
     - Width: **40**
     - Height: **40**
   - Image: `icon_Coins_Frame.png`
   - Preserve Aspect: **✓**

4. **Add Coins Text:**
   - Right-click `CoinsRow` → UI → Text - TextMeshPro
   - Name: `CoinsText`
   - RectTransform:
     - Anchor: **Middle Stretch**
     - Left: **50**
     - Right: **60**
   - TextMeshPro:
     - Text: "12,450"
     - Font: `Roboto-Mono SDF`
     - Font Size: **28**
     - Color: `#FFD700` (gold)
     - Alignment: **Right, Middle**
     - Outline: **✓**

5. **Add Coins Plus Button:**
   - Right-click `CoinsRow` → UI → Button
   - Name: `CoinsPlusButton`
   - RectTransform:
     - Anchor: **Middle Right**
     - Width: **45**
     - Height: **45**
   - Image Component:
     - Source Image: `icon_CoinsPlus_Normal.png`
   - Button Component:
     - Transition: **Sprite Swap**
     - Highlighted Sprite: `icon_CoinsPlus_Hover.png`
     - Pressed Sprite: `icon_CoinsPlus_Pressed.png`
     - Disabled Sprite: `icon_CoinsPlus_Disabled.png`

6. **Repeat for Gems Row:**
   - Duplicate `CoinsRow` (Ctrl+D)
   - Rename: `GemsRow`
   - Position: `Y = -65`
   - Update icons to Gems versions:
     - `icon_Gems_Frame.png`
     - `icon_GemsPlus_*.png`
   - Gems text color: `#00FFFF` (cyan)

7. **Add Battle Pass Progress:**
   - Right-click `CurrencyPanel` → UI → Panel
   - Name: `BattlePassProgress`
   - RectTransform:
     - Anchor: **Bottom Stretch**
     - Height: **50**
     - Bottom: **15**
     - Left: **15**, Right: **15**
   - Image: `icon_BattlePassProgress_Frame.png`

8. **Add Battle Pass Fill Bar:**
   - Right-click `BattlePassProgress` → UI → Image
   - Name: `BattlePassFillBar`
   - RectTransform: Stretch to fill
   - Image Type: **Filled**
   - Fill Method: **Horizontal**
   - Fill Amount: **0.24** (Tier 12/50 = 24%)
   - Color: `#9B59B6` (purple)

9. **Add Battle Pass Text:**
   - Right-click `BattlePassProgress` → UI → Text - TextMeshPro
   - Name: `BattlePassText`
   - Text: "Battle Pass - Tier 12/50"
   - Font Size: **14**
   - Alignment: **Center, Middle**
   - Outline: **✓**

10. **Make Progress Clickable:**
    - Select `BattlePassProgress`
    - Add Component → **Button**
    - Target Graphic: BattlePassProgress Image
    - Transition: **Color Tint**
    - OnClick: Will wire later to open Battle Pass screen

---

### **Phase 6: Build Center Ship Viewer Display**

#### **Step 16: Add Ship Viewer to Canvas**

1. **Create Raw Image:**
   - Right-click `MainMenuCanvas` → UI → Raw Image
   - Name: `ShipViewerDisplay`

2. **RectTransform:**
   - Anchor: **Middle Center**
   - Position: `(0, 50)` (slightly above center)
   - Width: **600**
   - Height: **600**

3. **Raw Image Component:**
   - Texture: Drag `ShipViewerRT` here
   - Color: **White** (full opacity)
   - Material: **None**
   - UV Rect: `(0, 0, 1, 1)` (default)

**The ship viewer is now connected! Any 3D model in ShipContainer will appear here.**

---

### **Phase 7: Build Left Navigation Frame**

#### **Step 17: Create Left Frame Container**

1. **Create Panel:**
   - Right-click `MainMenuCanvas` → UI → Panel
   - Name: `LeftNavigationFrame`

2. **RectTransform:**
   - Anchor: **Middle Left**
   - Pivot: `(0, 0.5)`
   - Position: `(20, 0)`
   - Width: **180**
   - Height: **500**

3. **Image:**
   - Color: `#1A1A26` (semi-transparent)
   - Alpha: **180**

4. **Add Vertical Layout Group** (auto-spacing):
   - Add Component → Vertical Layout Group
   - Padding: Top **20**, Bottom **20**, Left **15**, Right **15**
   - Spacing: **15**
   - Child Alignment: **Upper Center**
   - Child Force Expand: Width **✓**, Height **✗**

---

#### **Step 18: Create Navigation Button Prefab**

**Create a reusable button prefab:**

1. **Create Button:**
   - Right-click `LeftNavigationFrame` → UI → Button
   - Name: `NavButton_Template`

2. **RectTransform:**
   - Width: **150**
   - Height: **100**

3. **Button Component:**
   - **Transition: Sprite Swap** (IMPORTANT - not Color Tint)
   - Target Graphic: The Image component
   - Navigation: **Automatic**

4. **Add Icon Image:**
   - Right-click `NavButton_Template` → UI → Image
   - Name: `Icon`
   - RectTransform:
     - Anchor: **Top Center**
     - Position: `(0, -10)`
     - Width: **64**
     - Height: **64**
   - Raycast Target: **✗** (unchecked - let button handle clicks)

5. **Add Text Label:**
   - Right-click `NavButton_Template` → UI → Text - TextMeshPro
   - Name: `Label`
   - RectTransform:
     - Anchor: **Bottom Stretch**
     - Height: **30**
     - Bottom: **5**
     - Left: **0**, Right: **0**
   - TextMeshPro:
     - Text: "BUTTON"
     - Font: `Rajdhani-Bold SDF`
     - Font Size: **16**
     - Color: `#FFFFFF`
     - Alignment: **Center, Middle**
     - Outline: **✓**
     - Outline Thickness: **0.2**

6. **Make Prefab:**
   - Drag `NavButton_Template` to `Assets/UI/MainMenuHub/Prefabs/`
   - Name it: `NavigationButton.prefab`
   - **Delete from scene** (we'll create instances next)

---

#### **Step 19: Create All Left Navigation Buttons**

**Now create the 4 buttons using the prefab:**

1. **Ships Garage Button:**
   - Drag `NavigationButton.prefab` into `LeftNavigationFrame`
   - Rename: `ShipsGarageButton`
   - Icon sprite: `icon_ShipGarage_Normal.png`
   - Label text: "SHIPS\nGARAGE"
   - Button Component → Sprite Swap:
     - Highlighted Sprite: `icon_ShipGarage_Hover.png`
     - Pressed Sprite: `icon_ShipGarage_Pressed.png`
     - Disabled Sprite: `icon_ShipGarage_Disabled.png`

2. **Inventory Button:**
   - Duplicate button or drag prefab
   - Rename: `InventoryButton`
   - Icon: `icon_Inventory_Normal.png` (+ hover, pressed, disabled)
   - Label: "INVENTORY"

3. **Leaderboards Button:**
   - Rename: `LeaderboardsButton`
   - Icon: `icon_LeaderBoards_Normal.png` (+ hover, pressed, disabled)
   - Label: "LEADER-\nBOARDS"

4. **Friends Button:**
   - Rename: `FriendsButton`
   - Icon: `icon_Friends_Normal.png` (+ hover, pressed, disabled)
   - Label: "FRIENDS"

**Vertical Layout Group should auto-space them!**

---

### **Phase 8: Build Right Navigation Frame**

#### **Step 20: Create Right Frame**

Repeat the same process for right side:

1. **Duplicate `LeftNavigationFrame`:**
   - Select `LeftNavigationFrame`
   - Ctrl+D (duplicate)
   - Rename: `RightNavigationFrame`

2. **RectTransform:**
   - Anchor: **Middle Right**
   - Pivot: `(1, 0.5)`
   - Position: `(-20, 0)`

3. **Delete all buttons inside it**

4. **Create 4 buttons:**

   **Shop Button:**
   - Icon: `icon_Shop_Normal.png` (+ states)
   - Label: "SHOP"

   **Achievements Button:**
   - Icon: `icon_Achievements_Normal.png` (+ states)
   - Label: "ACHIEVE-\nMENTS"

   **Account Progress Button:**
   - Icon: `icon_Progress_Normal.png` (+ states)
   - Label: "ACCOUNT\nPROGRESS"

   **Clan Button:**
   - Icon: `icon_Clan_Normal.png` (+ states)
   - Label: "CLAN"

---

### **Phase 9: Build Center Action Buttons**

#### **Step 21: Create Stats Display (Above Play Button)**

1. **Create Panel:**
   - Right-click `MainMenuCanvas` → UI → Panel
   - Name: `StatsDisplay`
   - RectTransform:
     - Anchor: **Middle Center**
     - Position: `(0, -150)`
     - Width: **400**
     - Height: **40**
   - Image Color: `#2A2A3A` (dark, semi-transparent)

2. **Add Stats Text:**
   - Right-click `StatsDisplay` → UI → Text - TextMeshPro
   - Name: `StatsText`
   - RectTransform: Stretch to fill
   - Text: "W/L: 84/43  |  ELO: 1450"
   - Font: `Roboto-Mono SDF`
   - Font Size: **20**
   - Color: `#FFD700` (gold)
   - Alignment: **Center, Middle**
   - Outline: **✓**

---

#### **Step 22: Create Center Button Container**

1. **Create Empty:**
   - Right-click `MainMenuCanvas` → Create Empty
   - Name: `CenterButtonsContainer`
   - RectTransform:
     - Anchor: **Middle Center**
     - Position: `(0, -220)`
     - Width: **850**
     - Height: **120**

2. **Add Horizontal Layout Group:**
   - Add Component → Horizontal Layout Group
   - Spacing: **25**
   - Child Alignment: **Middle Center**
   - Child Force Expand: Both **✓**

---

#### **Step 23: Create Main Buttons**

1. **Quests Button:**
   - Drag `NavigationButton.prefab` into `CenterButtonsContainer`
   - Rename: `QuestsButton`
   - Width: **200**, Height: **120**
   - Icon: `icon_Quests_Normal.png` (+ states)
   - Icon size: **80x80**
   - Label: "QUESTS"
   - Label font size: **24**
   - Button sprite swap states configured

2. **Play Now Button:** (LARGEST BUTTON)
   - Create new Button (not from prefab)
   - Right-click `CenterButtonsContainer` → UI → Button
   - Name: `PlayNowButton`
   - Width: **300**, Height: **120**

   **Button Image:**
   - Source Image: `icon_Play_Normal.png`
   - Image Type: **Simple**

   **Button Component:**
   - Transition: **Sprite Swap**
   - Highlighted: `icon_Play_Hover.png`
   - Pressed: `icon_Play_Pressed.png`
   - Disabled: `icon_Play_Disabled.png`
   - Colors (if using Color Tint instead):
     - Normal: `#27AE60` (green)
     - Highlighted: `#2ECC71` (bright green)
     - Pressed: `#229954` (dark green)
     - Disabled: `#95A5A6` (gray)

   **Add Play Icon:**
   - Right-click `PlayNowButton` → UI → Image
   - Name: `PlayIcon`
   - Position: Left side of button
   - Width: **80**, Height: **80**
   - Image: Play triangle icon

   **Add Text:**
   - Right-click `PlayNowButton` → UI → Text - TextMeshPro
   - Name: `PlayText`
   - Text: "PLAY NOW"
   - Font: `Orbitron-Bold SDF`
   - Font Size: **36**
   - Color: `#FFFFFF`
   - Alignment: **Center, Middle**
   - Bold: **✓**

3. **Battle Pass Button:**
   - Drag `NavigationButton.prefab`
   - Rename: `BattlePassButton`
   - Width: **200**, Height: **120**
   - Icon: `icon_BattlePass_Normal.png` (+ states)
   - Icon size: **80x80**
   - Label: "BATTLE\nPASS"
   - Label font size: **24**

---

### **Phase 10: Build Game Mode Selector**

#### **Step 24: Create Game Mode Frame**

1. **Create Panel:**
   - Right-click `MainMenuCanvas` → UI → Panel
   - Name: `GameModeFrame`
   - RectTransform:
     - Anchor: **Middle Center**
     - Position: `(0, -370)`
     - Width: **600**
     - Height: **80**
   - Image:
     - Source Image: `frame_GameMode.png`
     - Color: `#2C3E50`
     - Image Type: **Sliced**

2. **Add Horizontal Layout Group:**
   - Spacing: **10**
   - Padding: **10** on all sides
   - Child Alignment: **Middle Center**
   - Child Force Expand: Both **✓**

---

#### **Step 25: Create Game Mode Toggle Buttons**

**IMPORTANT:** These have only **3 states** (Normal, Hover, Toggled) - NO DISABLED STATE

**Create Toggle Button Prefab:**

1. **Create Toggle:**
   - Right-click `GameModeFrame` → UI → Toggle
   - Name: `GameModeToggle_Template`

2. **RectTransform:**
   - Width: **180**
   - Height: **60**

3. **Remove default checkmark:**
   - Delete the "Background" and "Checkmark" child objects
   - We'll use sprite swap instead

4. **Configure Toggle Component:**
   - Is On: **✗** (unchecked by default)
   - Transition: **Sprite Swap**
   - Toggle Transition: **None**

5. **Configure Image (Background):**
   - Source Image: Will be set per mode

6. **Add Icon and Label** (same as nav buttons)

**Make Prefab:**
- Save as `GameModeToggle.prefab`
- Delete from scene

**Create 3 Toggles:**

1. **Local Toggle:**
   - Drag prefab into `GameModeFrame`
   - Rename: `LocalToggle`
   - Image sprite: `icon_Local_Normal.png`
   - Toggle Component → Sprite Swap:
     - Highlighted Sprite: `icon_Local_Hover.png`
     - Selected Sprite: `icon_Local_Toggled.png`
   - Label: "LOCAL"
   - Is On: **✓** (default selected)

2. **Online Toggle:**
   - Sprite: `icon_Online_Normal.png` (+ hover, toggled)
   - Label: "ONLINE"
   - Is On: **✗**

3. **Ranked Toggle:**
   - Sprite: `icon_Ranked_Normal.png` (+ hover, toggled)
   - Label: "RANKED"
   - Is On: **✗**

4. **Create Toggle Group:**
   - Select `GameModeFrame`
   - Add Component → **Toggle Group**
   - Allow Switch Off: **✗** (unchecked - one must always be selected)

5. **Assign Toggles to Group:**
   - Select `LocalToggle`
   - Toggle Component → Group: Drag `GameModeFrame` here
   - Repeat for Online and Ranked toggles

**Now only ONE mode can be selected at a time!**

---

### **Phase 11: Build Bottom Elements**

#### **Step 26: Create Settings Button (Bottom-Left)**

1. **Create Button:**
   - Right-click `MainMenuCanvas` → UI → Button
   - Name: `SettingsButton`
   - RectTransform:
     - Anchor: **Bottom-Left**
     - Pivot: `(0, 0)`
     - Position: `(20, 80)`
     - Width: **80**
     - Height: **80**

2. **Button Component:**
   - Transition: **Sprite Swap**
   - Image: `icon_Settings_Normal.png` (if you have it) or gear icon
   - Highlighted: `icon_Settings_Hover.png`
   - Pressed: `icon_Settings_Pressed.png`
   - Disabled: `icon_Settings_Disabled.png`

---

#### **Step 27: Create Loadout Frame (Bottom-Right)**

1. **Create Panel:**
   - Right-click `MainMenuCanvas` → UI → Panel
   - Name: `LoadoutFrame`
   - RectTransform:
     - Anchor: **Bottom-Right**
     - Pivot: `(1, 0)`
     - Position: `(-20, 80)`
     - Width: **200**
     - Height: **100**
   - Image Color: `#1A1A26`

2. **Add Horizontal Layout Group:**
   - Spacing: **15**
   - Padding: **10**
   - Child Force Expand: Both **✓**

3. **Create Missile Loadout Button:**
   - Right-click `LoadoutFrame` → UI → Button
   - Name: `MissileLoadoutButton`
   - Width: **85**
   - Height: **80**
   - **Special:** Icon will be 3D model of equipped missile (rendered to texture)
   - For now, use placeholder icon
   - Button sprite swap configured

4. **Create Custom Builder Button:**
   - Right-click `LoadoutFrame` → UI → Button
   - Name: `CustomBuilderButton`
   - Width: **85**
   - Height: **80**
   - Icon: Wrench/tools icon (or use Progress icon temporarily)
   - Button sprite swap configured

---

#### **Step 28: Create Latest News Bar (Bottom)**

1. **Create Panel:**
   - Right-click `MainMenuCanvas` → UI → Panel
   - Name: `NewsBar`
   - RectTransform:
     - Anchor: **Bottom Stretch**
     - Height: **60**
     - Bottom: **0**
     - Left: **0**, Right: **0**
   - Image Color: `#0D0D14` (very dark)

2. **Add News Text:**
   - Right-click `NewsBar` → UI → Text - TextMeshPro
   - Name: `NewsText`
   - RectTransform:
     - Stretch to fill
     - Padding Left: **20**, Right: **20**
   - Text: "LATEST UPDATE: New 'Void Stalker' Ship Released! Triple XP Weekend!"
   - Font: `Roboto-Regular SDF`
   - Font Size: **18**
   - Color: `#FFD700` (gold)
   - Alignment: **Left, Middle**
   - Overflow: **Ellipsis**

3. **Make Clickable:**
   - Select `NewsBar`
   - Add Component → **Button**
   - Transition: **Color Tint**
   - Highlighted Color: Slightly brighter
   - OnClick: Will wire later

---

### **Phase 12: Create Panel System (Semi-Transparent Overlay)**

**DESIGN PHILOSOPHY:** All panels appear as semi-transparent overlays on top of the main menu. The ship viewer, planet background, and main menu elements remain visible underneath, creating a layered, immersive experience.

#### **Step 29: Create Panels Container**

1. **Create Empty:**
   - Right-click `MainMenuCanvas` → Create Empty
   - Name: `PanelsContainer`
   - RectTransform: Stretch to fill entire canvas
   - **Keep this ABOVE all main menu elements in hierarchy** (so panels render on top)

2. **Add Background Dimmer (Shared Overlay):**
   - Right-click `PanelsContainer` → UI → Panel
   - Name: `BackgroundDimmer`
   - RectTransform: Stretch to fill entire canvas
   - Image Component:
     - Color: `#000000` (Black)
     - Alpha: **128** (50% transparency - adjustable)
     - Material: **None**
   - Set Active: **✗** (unchecked - only shown when panels are open)
   - **This creates the darkened overlay effect behind panels**

3. **Create Semi-Transparent Panel Template:**

   For each screen, create a semi-transparent panel that sits on top:

   **Template for each panel:**
   - Right-click `PanelsContainer` → UI → Panel
   - Name: e.g., `ShipsGaragePanel`
   - RectTransform:
     - Anchor: **Middle Center**
     - Position: `(0, 0)`
     - Width: **1400** (doesn't fill entire screen)
     - Height: **900**
   - Image Component:
     - Color: `#1A1A26` (Dark blue-gray)
     - Alpha: **220** (86% opacity - semi-transparent)
     - Image Type: **Sliced** (if using frame)
     - Material: **None**
   - Add Component → **Canvas Group**:
     - Alpha: **1**
     - Interactable: **✓**
     - Block Raycasts: **✓**
   - Set Active: **✗** (unchecked - hidden by default)

   **Add Optional Decorative Frame:**
   - Right-click panel → UI → Image
   - Name: `PanelFrame`
   - RectTransform: Stretch to fill parent (all zeros)
   - Source Image: Frame asset (if available)
   - Color: `#3A4A5A` (Lighter accent color)
   - Alpha: **180**
   - Image Type: **Sliced**
   - Raycast Target: **✗**

   **Add Back Button:**
   - Right-click panel → UI → Button
   - Name: `BackButton`
   - RectTransform:
     - Anchor: **Top-Left**
     - Position: `(30, -30)`
     - Width: **150**, Height: **60**
   - Image Color: `#2C3E50`
   - Button Component:
     - Transition: **Color Tint**
     - Highlighted: `#34495E`
     - Pressed: `#1ABC9C`
   - **Add Text:**
     - Right-click `BackButton` → UI → Text - TextMeshPro
     - Text: "← BACK"
     - Font: `Orbitron-Bold SDF`
     - Font Size: **20**
     - Color: `#FFFFFF`
     - Alignment: **Center, Middle**
   - OnClick: Will wire to PanelManager later

   **Add Title:**
   - Right-click panel → UI → Text - TextMeshPro
   - Name: `PanelTitle`
   - RectTransform:
     - Anchor: **Top Center**
     - Position: `(0, -40)`
     - Width: **800**, Height: **80**
   - Text: Panel name (e.g., "SHIPS GARAGE")
   - Font: `Orbitron-Bold SDF`
   - Font Size: **56**
   - Color: `#FFFFFF`
   - Alignment: **Center, Middle**
   - Outline: **✓**, Thickness **0.3**, Color Black
   - Shadow: **✓** (optional for depth)

   **Add Content Container:**
   - Right-click panel → Create Empty
   - Name: `ContentContainer`
   - RectTransform:
     - Anchor: **Stretch/Stretch**
     - Top: **120** (below title)
     - Bottom: **30**
     - Left: **30**, Right: **30**
   - **This is where panel-specific content goes**

   **Create these semi-transparent overlay panels:**
   - ShipsGaragePanel
   - InventoryPanel
   - LeaderboardsPanel
   - FriendsPanel
   - ShopPanel
   - AchievementsPanel
   - AccountProgressPanel
   - ClanPanel
   - QuestsPanel
   - BattlePassPanel
   - MissileLoadoutPanel
   - CustomBuilderPanel
   - SettingsPanel
   - PlayerProfilePanel
   - NotificationsPanel
   - EventsCalendarPanel
   - NewsPanel

   **All panels should be inactive initially. Only main menu elements are visible.**

4. **Organization Tips:**
   - Keep `BackgroundDimmer` as first child of `PanelsContainer` (renders first)
   - Order panels in hierarchy by usage frequency
   - Use consistent sizing for similar panel types:
     - **Full-screen content:** 1400x900
     - **Medium popups:** 1000x700
     - **Small dialogs:** 600x400

---

## 🔌 Wiring Up Scripts and Functionality

### **Phase 13: Add Scripts to Scene**

You should already have these scripts from previous development. If not, you'll need to create them based on your architecture.

#### **Step 30: Add MainMenuManager**

1. **Create Empty GameObject:**
   - Hierarchy → Create Empty
   - Name: `MainMenuManager`
   - Position: `(0, 0, 0)`

2. **Add MainMenuController Component:**
   - Add Component → Search "MainMenuController" (your script)
   - If script doesn't exist, you'll need to create it

3. **Assign References:**
   - Ship Viewer System: Drag `ShipViewerSystem`
   - Menu Canvas: Drag `MainMenuCanvas`

---

#### **Step 31: Add PanelManager Component**

1. **Select `MainMenuCanvas`**

2. **Add PanelManager Component:**
   - Add Component → Search "PanelManager"

3. **Assign Panel References:**
   - Background Dimmer: `BackgroundDimmer` (IMPORTANT - needed for overlay effect)
   - Ships Garage Panel: `ShipsGaragePanel`
   - Inventory Panel: `InventoryPanel`
   - Leaderboards Panel: `LeaderboardsPanel`
   - Friends Panel: `FriendsPanel`
   - Shop Panel: `ShopPanel`
   - Achievements Panel: `AchievementsPanel`
   - Account Progress Panel: `AccountProgressPanel`
   - Clan Panel: `ClanPanel`
   - Quests Panel: `QuestsPanel`
   - Battle Pass Panel: `BattlePassPanel`
   - Missile Loadout Panel: `MissileLoadoutPanel`
   - Custom Builder Panel: `CustomBuilderPanel`
   - Settings Panel: `SettingsPanel`
   - Player Profile Panel: `PlayerProfilePanel`
   - Notifications Panel: `NotificationsPanel`
   - Events Calendar Panel: `EventsCalendarPanel`
   - News Panel: `NewsPanel`

**Note on PanelManager Behavior:**
- When showing a panel: Enable `BackgroundDimmer` + Enable target panel
- When hiding a panel: Disable panel + Disable `BackgroundDimmer`
- Main menu elements (ship viewer, navigation, etc.) remain always active
- Use LeanTween for smooth fade-in/fade-out animations on panels

---

#### **Step 32: Wire Button OnClick Events**

For each navigation button:

**Left Navigation:**
1. **ShipsGarageButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String parameter: "ships"

2. **InventoryButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "inventory"

3. **LeaderboardsButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "leaderboards"

4. **FriendsButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "friends"

**Right Navigation:**
5. **ShopButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "shop"

6. **AchievementsButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "achievements"

7. **AccountProgressButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "accountprogress"

8. **ClanButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "clan"

**Center Buttons:**
9. **QuestsButton:**
   - OnClick() → `PanelManager.ShowPanel`
   - String: "quests"

10. **PlayNowButton:**
    - OnClick() → `MainMenuController.OnPlayNowClicked`

11. **BattlePassButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "battlepass"

**Loadout:**
12. **MissileLoadoutButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "missileloadout"

13. **CustomBuilderButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "custombuilder"

**Other:**
14. **SettingsButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "settings"

15. **ProfileIcon (in PlayerProfilePanel):**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "profile"

16. **NotificationButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "notifications"

17. **CalendarButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "events"

18. **NewsBar:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "news"

19. **BattlePassProgress (top-right):**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "battlepass"

20. **CoinsPlusButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "shop"

21. **GemsPlusButton:**
    - OnClick() → `PanelManager.ShowPanel`
    - String: "shop"

**Back Buttons (on all panels):**
- OnClick() → `PanelManager.HideCurrentPanel` (or `ClosePanel`)
- **No parameter needed** - closes current overlay and returns to main menu view

**Alternative Back Button Setup (if using ShowPanel method):**
- OnClick() → `PanelManager.ShowPanel`
- String: "" (empty string closes panels and shows main menu)

**ESC Key Handling:**
- Should close current panel overlay when pressed
- Returns to main menu view (no panel active)

---

## 📱 Component Settings Reference

### **Canvas Settings**
```
Canvas
├── Render Mode: Screen Space - Overlay
├── Pixel Perfect: ✓
└── Canvas Scaler
    ├── UI Scale Mode: Scale With Screen Size
    ├── Reference Resolution: 1920 x 1080
    ├── Screen Match Mode: Match Width Or Height
    └── Match: 0.5
```

### **Button with Sprite Swap (4 States)**
```
Button Component
├── Interactable: ✓
├── Transition: Sprite Swap
├── Target Graphic: Button's Image component
├── Highlighted Sprite: [name]_Hover.png
├── Pressed Sprite: [name]_Pressed.png
├── Disabled Sprite: [name]_Disabled.png
└── Navigation: Automatic
```

### **Toggle with Sprite Swap (3 States)**
```
Toggle Component
├── Is On: ✓ (for default mode only)
├── Transition: Sprite Swap
├── Target Graphic: Toggle's Image component
├── Highlighted Sprite: [name]_Hover.png
├── Selected Sprite: [name]_Toggled.png
├── Group: GameModeFrame (Toggle Group)
└── Navigation: Automatic
```

### **Image - Filled (Progress Bars)**
```
Image Component
├── Image Type: Filled
├── Fill Method: Horizontal
├── Fill Origin: Left
├── Fill Amount: 0.0 - 1.0 (dynamic)
├── Preserve Aspect: ✗
└── Raycast Target: ✗ (if not clickable)
```

### **TextMeshPro Settings**
```
TextMeshPro - Text
├── Font Asset: [font] SDF
├── Font Size: [see specifications]
├── Auto Size: ✗
├── Color: [see specifications]
├── Alignment: Center, Middle (or as specified)
├── Wrapping: Enabled (if multi-line)
├── Overflow: Ellipsis or Overflow
├── Extra Settings
│   ├── Outline: ✓
│   ├── Thickness: 0.2 - 0.3
│   └── Color: #000000 (black, alpha 200)
└── Raycast Target: ✗ (if not clickable)
```

### **Raw Image (Ship Viewer)**
```
Raw Image Component
├── Texture: ShipViewerRT
├── Color: White (1, 1, 1, 1)
├── Material: None
├── UV Rect: (0, 0, 1, 1)
└── Raycast Target: ✗
```

### **Render Texture (Ship Viewer)**
```
Render Texture
├── Size: 1024 x 1024
├── Depth Buffer: 16 bit
├── Anti-aliasing: 4x
├── Color Format: Default
├── Filter Mode: Bilinear
└── Wrap Mode: Clamp
```

### **Camera (Ship Viewer)**
```
Camera Component
├── Clear Flags: Solid Color
├── Background: #000000 or #0A0A14
├── Culling Mask: Everything
├── Projection: Perspective
├── Field of View: 40
├── Clipping Planes: Near 0.3, Far 100
├── Depth: -2
├── Target Texture: ShipViewerRT
└── Target Display: Display 1
```

---

## ✅ Testing Checklist

### **Visual Tests**
- [ ] All icons display correctly
- [ ] All 4-state buttons show correct sprites on hover/press
- [ ] Game mode toggles work (only one selected at a time)
- [ ] Text is readable and properly sized
- [ ] Colors match specifications
- [ ] Layout looks correct at 1920x1080
- [ ] Ship viewer renders in center (even if empty)
- [ ] No overlapping UI elements
- [ ] All frames and panels aligned correctly
- [ ] **Overlay Tests:**
  - [ ] Panels appear semi-transparent (can see ship/background behind)
  - [ ] Background dimmer creates proper darkening effect
  - [ ] Text remains readable on semi-transparent panels
  - [ ] Panel shadows/frames provide depth separation
  - [ ] Overlay panels don't completely obscure main menu
  - [ ] Layering looks correct (dimmer → panel → content)

### **Functionality Tests**
- [ ] All navigation buttons open correct overlay panels
- [ ] Back buttons close overlay and return to main menu
- [ ] Background dimmer appears when panel opens
- [ ] Background dimmer disappears when panel closes
- [ ] Ship viewer remains visible behind panels
- [ ] Main menu elements remain visible behind semi-transparent panels
- [ ] Play Now button triggers correctly
- [ ] Game mode selection works (one at a time)
- [ ] Currency +buttons clickable and open shop overlay
- [ ] Battle Pass progress bar clickable and opens battle pass overlay
- [ ] Profile icon clickable and opens profile overlay
- [ ] Notification/Calendar buttons open respective overlays
- [ ] Settings button opens settings overlay
- [ ] Loadout buttons open loadout overlays
- [ ] News bar clickable and opens news overlay
- [ ] ESC key closes current overlay panel

### **State Tests**
- [ ] Buttons show hover state on mouse over
- [ ] Buttons show pressed state on click
- [ ] Disabled buttons appear greyed out
- [ ] Toggles show toggled state when selected
- [ ] Only one game mode toggled at a time

### **Animation Tests** (if LeanTween implemented)
- [ ] Menu fades in smoothly on load
- [ ] Overlay panels fade in smoothly when opened
- [ ] Overlay panels fade out smoothly when closed
- [ ] Background dimmer fades in/out with panels
- [ ] Panel animations don't interfere with ship viewer
- [ ] Buttons have subtle press animation
- [ ] No animation errors in console
- [ ] Smooth transitions maintain semi-transparency effect

### **Integration Tests**
- [ ] ProgressionManager loads player data
- [ ] Username displays correctly
- [ ] Level displays correctly
- [ ] XP bar fills based on actual XP
- [ ] ELO displays correctly
- [ ] Rank icon changes based on ELO
- [ ] Win/Loss ratio displays correctly
- [ ] Coins/Gems display with formatting
- [ ] Battle Pass tier displays correctly
- [ ] Ship loads in 3D viewer (when equipped)

### **Performance Tests**
- [ ] No console errors
- [ ] Smooth 60 FPS
- [ ] No memory leaks
- [ ] UI scales properly at different resolutions:
  - [ ] 1920x1080 (primary)
  - [ ] 1280x720
  - [ ] 2560x1440
  - [ ] 3840x2160

### **Accessibility Tests**
- [ ] All buttons large enough to click easily
- [ ] Text readable at all sizes
- [ ] Sufficient contrast between text and background
- [ ] Color-blind friendly (if applicable)

---

## 🎨 Design Tips

### **Creating Icons in PaintShop Pro**

**For 4-State Buttons:**

1. **Create Base (Normal State):**
   - New image: 512x512, transparent background
   - Use vector tools for clean edges
   - Base color: Medium gray `#B0B0B0`
   - Keep 30-40px padding from edges
   - Save master as .pspimage

2. **Create Hover State:**
   - Duplicate layer
   - Adjust → Brightness/Contrast → Brightness +30
   - Change color to white `#FFFFFF`
   - Add outer glow:
     - Effects → 3D Effects → Drop Shadow
     - Modify for glow effect
     - Color: Thematic (blue, gold, etc.)
     - Opacity: 70-80%
     - Blur: 12px
   - Export as `[name]_Hover.png`

3. **Create Pressed State:**
   - Duplicate base
   - Adjust → Brightness/Contrast → Brightness -20
   - Color: Dark gray `#808080`
   - Add inner bevel:
     - Effects → 3D Effects → Inner Bevel
     - Depth: 5
     - Smoothness: 10
   - Export as `[name]_Pressed.png`

4. **Create Disabled State:**
   - Duplicate base
   - Adjust → Brightness/Contrast → Brightness -40
   - Color: Very dark `#404040`
   - Layers → Opacity: 50%
   - Export as `[name]_Disabled.png`

**Export Settings:**
- File → Export → PNG Optimizer
- Format: PNG-24
- Transparency: Yes
- Compression: 9 (maximum)
- Interlacing: None

---

## 💡 Semi-Transparent Overlay Implementation Tips

### **Adjusting Transparency Levels**

**Background Dimmer:**
- **Light dimming:** Alpha 80-100 (subtle darkening)
- **Medium dimming:** Alpha 128-150 (balanced - recommended)
- **Heavy dimming:** Alpha 180-200 (dramatic focus on panel)

**Panel Background:**
- **Subtle transparency:** Alpha 200-230 (barely see-through)
- **Medium transparency:** Alpha 180-220 (recommended - good balance)
- **High transparency:** Alpha 150-180 (very see-through, requires careful contrast)

### **Ensuring Text Readability**

1. **Use text outlines/shadows:**
   - Outline Thickness: 0.2-0.3 minimum
   - Outline Color: Black with alpha 200+
   - This prevents background from interfering with text

2. **Add text background panels:**
   - For critical text, add a darker sub-panel behind it
   - Color: `#000000`, Alpha: 100-150
   - Provides contrast while maintaining overlay aesthetic

3. **Choose high-contrast text colors:**
   - Prefer: White, Bright Gold, Bright Cyan
   - Avoid: Gray tones, dim colors

### **Panel Depth and Visual Hierarchy**

**Layer Order (bottom to top):**
1. Background Panel (solid dark background)
2. Ship Viewer (3D rendered texture)
3. Main Menu Elements (navigation, buttons, top bar)
4. Background Dimmer (semi-transparent black overlay)
5. Overlay Panels (semi-transparent content panels)
6. Panel Content (buttons, text, interactive elements)

**Creating Depth:**
- Add subtle drop shadows to panels (offset 5-10px)
- Use decorative frames with lighter colors
- Stack multiple transparency layers for richness
- Consider blur effect on background dimmer (requires shader)

### **Performance Considerations**

**Optimization:**
- Don't stack too many transparent layers (max 2-3)
- Disable unused panels completely (SetActive false)
- Use Canvas Groups for batch alpha changes
- Minimize overdraw by sizing panels appropriately

**Avoid:**
- Full-screen transparent images (expensive)
- Complex transparency with many UI elements underneath
- Animated transparency on large panels (use fade on Canvas Group instead)

### **Alternative Design Variations**

**Frosted Glass Effect:**
- Requires UI Blur shader
- Background Dimmer uses blur material instead of solid black
- Creates premium, modern look
- More expensive performance-wise

**Slide-in Panels:**
- Panels slide from edges instead of fade in center
- Main menu slides to side when panel opens
- More dynamic, less overlay-focused

**Hybrid Approach:**
- Some panels are overlays (quick access: shop, settings)
- Others replace view entirely (major screens: ships garage)
- Best of both worlds

### **Code Example - Panel Manager Logic**

```csharp
public void ShowPanel(string panelName) {
    // Hide current panel if any
    if (currentPanel != null) {
        LeanTween.alphaCanvas(currentPanel, 0f, 0.2f).setOnComplete(() => {
            currentPanel.gameObject.SetActive(false);
        });
    }

    // Show background dimmer
    backgroundDimmer.SetActive(true);
    LeanTween.alpha(backgroundDimmer.GetComponent<RectTransform>(), 0.5f, 0.2f);

    // Show new panel
    GameObject panel = GetPanel(panelName);
    panel.SetActive(true);
    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
    cg.alpha = 0f;
    LeanTween.alphaCanvas(cg, 1f, 0.3f);

    currentPanel = cg;
}

public void HideCurrentPanel() {
    if (currentPanel == null) return;

    // Fade out panel
    LeanTween.alphaCanvas(currentPanel, 0f, 0.2f).setOnComplete(() => {
        currentPanel.gameObject.SetActive(false);
        currentPanel = null;
    });

    // Fade out background dimmer
    Image dimmer = backgroundDimmer.GetComponent<Image>();
    LeanTween.alpha(dimmer.rectTransform, 0f, 0.2f).setOnComplete(() => {
        backgroundDimmer.SetActive(false);
    });
}
```

---

## 🚀 Next Steps

### **After Main Hub is Complete:**

1. **Implement Individual Screens:**
   - Start with Ships Garage Screen (most important)
   - Then Missile Loadout
   - Then Quests and Battle Pass
   - Settings Screen
   - Profile Screen
   - etc.

2. **Add Ship Models:**
   - Import 3D ship models
   - Place in Resources folder or assign to ship data
   - Test loading in ShipViewer

3. **Polish Animations:**
   - Add LeanTween fade animations
   - Button press scale effects
   - Panel slide transitions

4. **Add Sound Effects:**
   - Button click sounds
   - Panel transition sounds
   - UI feedback sounds

5. **Test with Real Data:**
   - Load actual player data
   - Test with different ELO ranges
   - Test with different levels
   - Test unlocked/locked states

6. **Optimize:**
   - Reduce texture sizes if needed
   - Pool UI elements
   - Optimize render texture

---

## 📊 Asset Summary

**Total Assets Required:**

| Category | Count | Notes |
|----------|-------|-------|
| Navigation Icons (4 states) | 44 files | 11 buttons × 4 states |
| Game Mode Icons (3 states) | 9 files | 3 modes × 3 states |
| Currency Icons | 10 files | Coins, Gems frames + buttons |
| Rank Icons | 16 files | rank1.png to rank16.png |
| Frame Assets | 4+ files | GameMode, Level, XP, Logo |
| Special Icons | 5+ files | Notification, Calendar, Settings |
| **TOTAL** | **~88 files** | PNG format |

**Fonts Required:**
- Orbitron Bold (or Rajdhani Bold)
- Roboto Regular
- Roboto Mono

**Scripts Required:**
- MainMenuController
- PanelManager
- ShipViewer3D
- CurrencyDisplay
- StatsDisplay
- GameModeSelector
- (Plus all screen-specific scripts)

---

## 📝 Quick Reference

### **Common Unity Shortcuts**
- **F** - Frame selected object
- **Ctrl+D** - Duplicate
- **Ctrl+Shift+N** - New GameObject
- **Alt+Shift+Click Anchor** - Stretch to fill parent

### **Common Colors**
```
Dark Background: #0D0D14
Panel Background: #1A1A26
Button Normal: #2C3E50
Button Hover: #34495E
Button Pressed: #1ABC9C
Gold: #FFD700
Cyan: #00FFFF
Green (Play): #27AE60
White: #FFFFFF
```

### **Common Positions**
- Top-Left: `(20, -20)`
- Top-Right: `(-20, -20)`
- Bottom-Left: `(20, 20)`
- Bottom-Right: `(-20, 20)`
- Center: `(0, 0)`

---

## 🎯 Final Notes

1. **Test frequently** - After each section, press Play and verify everything works

2. **Save often** - Save scene and project frequently (Ctrl+S)

3. **Use prefabs** - Reuse button prefabs to maintain consistency

4. **Organize hierarchy** - Keep hierarchy clean and organized with empty containers

5. **Name consistently** - Use clear, descriptive names for all objects

6. **Comment your work** - Add comments in Inspector for complex setups

7. **Back up your project** - Use version control (Git) or manual backups

8. **Optimize as you go** - Don't wait until the end to optimize

9. **Test on target hardware** - Test on actual devices, not just editor

10. **Get feedback** - Show to others and iterate based on feedback

---

**You now have a complete, step-by-step guide to build the Main Menu Hub!** 🚀

Follow each phase carefully, test as you go, and you'll have a professional main menu that rivals commercial games.

Good luck! 🎮
