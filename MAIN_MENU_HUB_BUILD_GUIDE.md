# Main Menu Hub - Complete Build Guide
**Everything You Need to Build the First Screen**

---

## 📋 Table of Contents

1. [Icon Requirements & Specifications](#icon-requirements--specifications)
2. [UI Elements Breakdown](#ui-elements-breakdown)
3. [Text & Font Specifications](#text--font-specifications)
4. [Prefab Structure](#prefab-structure)
5. [Step-by-Step Building Process](#step-by-step-building-process)
6. [Unity Component Settings](#unity-component-settings)
7. [Testing Checklist](#testing-checklist)

---

## 🎨 Icon Requirements & Specifications

### **Image Format & Settings**
- **Format**: PNG (with transparency)
- **Color Mode**: RGBA (32-bit)
- **Size**: 512x512 pixels (will be scaled down in Unity)
- **Background**: Transparent
- **Export**: No compression for source, Unity will optimize

### **Icon Categories & Variations Needed**

Each icon needs **4 variations** for button states:

#### **1. Navigation Icons (Main Hub)**

##### **Ships Icon** 🚀
- **Normal**: Gray/white color (#B0B0B0), no glow
- **Hover**: Bright white (#FFFFFF), subtle outer glow (blue #3498DB)
- **Pressed**: Slightly darker (#808080), inner shadow effect
- **Disabled**: Very dark gray (#404040), 50% opacity

**Design**: Spaceship silhouette, side view, simple geometric shape

##### **Quests Icon** 📜
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), gold glow (#FFD700)
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Scroll or quest paper with "!" symbol

##### **Missiles Icon** 🚀
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), red glow (#E74C3C)
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Missile/rocket, diagonal orientation

##### **Battle Pass Icon** 🎖️
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), purple glow (#9B59B6)
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Trophy or medal with star

##### **Profile Icon** 👤
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), cyan glow (#3498DB)
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Person silhouette or helmet

##### **Settings Icon** ⚙️
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), white glow
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Gear/cog wheel

##### **Leaderboard Icon** 📊
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), gold glow (#FFD700)
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Podium with ranks 1, 2, 3 OR bar chart

##### **Inventory Icon** 📦
- **Normal**: Gray/white (#B0B0B0)
- **Hover**: Bright white (#FFFFFF), orange glow (#FF9800)
- **Pressed**: Darker (#808080)
- **Disabled**: Dark gray (#404040), 50% opacity

**Design**: Chest or backpack

#### **2. Currency Icons**

##### **Credits Icon** 💰
- **Single version** (no states needed)
- **Color**: Gold (#FFD700)
- **Size**: 256x256 pixels
- **Design**: Coin with "C" symbol or credit symbol

##### **Gems Icon** 💎
- **Single version** (no states needed)
- **Color**: Cyan/diamond blue (#00FFFF)
- **Size**: 256x256 pixels
- **Design**: Gem/diamond crystal

#### **3. Rank Icons** 🏆 **UPDATED: Military/Naval Rank System**

**Single version each** (no button states):
- **Size**: 512x512 pixels
- **Background**: Transparent
- **Design**: Military-style badges/insignia with stripes, stars, or naval symbols

**NEW RANK SYSTEM** - 18 Tiers Based on ELO:

1. **Cadet** (0-499) - Color: Gray (#808080) - Design: Single stripe
2. **Midshipman** (500-699) - Color: Light gray (#A0A0A0) - Design: Two stripes
3. **Ensign** (700-899) - Color: Bronze (#CD7F32) - Design: Single bar
4. **Sub-Lieutenant** (900-1099) - Color: Copper (#B87333) - Design: Single bar with stripe
5. **Lieutenant** (1100-1299) - Color: Silver (#C0C0C0) - Design: Two bars ⭐ **Starting Rank**
6. **Lieutenant Commander** (1300-1499) - Color: Silver-blue (#B0C4DE) - Design: Two bars with stripe
7. **Commander** (1500-1699) - Color: Gold (#FFD700) - Design: Three bars
8. **Captain** (1700-1899) - Color: Gold-white (#FFF8DC) - Design: Four bars
9. **Senior Captain** (1900-2099) - Color: Platinum (#E5E4E2) - Design: Four bars with stripe
10. **Commodore** (2100-2299) - Color: Blue-white (#E0F0FF) - Design: Single star
11. **Rear Admiral** (2300-2499) - Color: Light blue (#ADD8E6) - Design: Two stars
12. **Rear Admiral (Upper Half)** (2500-2699) - Color: Sky blue (#87CEEB) - Design: Two stars (upper)
13. **Vice Admiral** (2700-2899) - Color: Royal blue (#4169E1) - Design: Three stars
14. **Admiral** (2900-3099) - Color: Navy blue (#000080) - Design: Four stars
15. **High Admiral** (3100-3299) - Color: Purple-blue (#7B68EE) - Design: Five stars
16. **Fleet Admiral** (3300-3499) - Color: Purple (#9B59B6) - Design: Five stars with wreath
17. **Supreme Admiral** (3500-3699) - Color: Purple-red gradient - Design: Crown with stars
18. **Grand Admiral** (3700+) - Color: Rainbow/prismatic - Design: Supreme insignia with crown

**Total Rank Icons Needed**: 18
- One icon per rank (no sub-tiers)

#### **4. Notification Icon** 🔔

- **Normal**: Gray (#B0B0B0)
- **Active**: Orange (#FF9800) with small red dot badge
- **Size**: 256x256 pixels

#### **5. Special Icons**

##### **Quick Play Button Icon** ▶️
- **Single large icon**
- **Size**: 256x256 pixels
- **Color**: Green (#2ECC71)
- **Design**: Play button triangle or "GO" text

---

### **Complete Icon List Summary**

| Icon Name | Variations | Size | Total Files |
|-----------|-----------|------|-------------|
| Ships | 4 (N, H, P, D) | 512x512 | 4 |
| Quests | 4 | 512x512 | 4 |
| Missiles | 4 | 512x512 | 4 |
| Battle Pass | 4 | 512x512 | 4 |
| Profile | 4 | 512x512 | 4 |
| Settings | 4 | 512x512 | 4 |
| Leaderboard | 4 | 512x512 | 4 |
| Inventory | 4 | 512x512 | 4 |
| Credits | 1 | 256x256 | 1 |
| Gems | 1 | 256x256 | 1 |
| Notification | 2 (N, A) | 256x256 | 2 |
| Quick Play | 1 | 256x256 | 1 |
| Rank Badges | 18 | 512x512 | 18 |
| **TOTAL** | - | - | **55 files** |

### **File Naming Convention**

Use this naming format:
```
icon_[name]_[state].png

Examples:
- icon_ships_normal.png
- icon_ships_hover.png
- icon_ships_pressed.png
- icon_ships_disabled.png
- icon_credits.png
- icon_gems.png
- icon_rank_cadet.png
- icon_rank_midshipman.png
- icon_rank_lieutenant.png
- icon_rank_admiral.png
- icon_rank_grand_admiral.png
- icon_notification_normal.png
- icon_notification_active.png
```

---

## 🖼️ UI Elements Breakdown

### **Main Components**

#### **1. Background**
- **Type**: Panel (UI Image)
- **Color**: Dark gradient (#0D0D14 to #1A1A26)
- **Size**: Full screen (1920x1080)
- **Alpha**: 100%

#### **2. Ship Viewer Display**
- **Type**: Raw Image
- **Render Texture**: 1024x1024
- **Position**: Center (0, 50, 0)
- **Size**: 600x600 pixels
- **Aspect Ratio**: 1:1

#### **3. Player Info Panel** (Top-Left)
- **Type**: Panel
- **Size**: 400x220 pixels
- **Position**: (20, -20) from top-left anchor
- **Color**: Semi-transparent dark (#1A1A26, alpha 180)
- **Rounded Corners**: 10px radius (optional, requires custom shader)

**Contains**:
- Username Text (TMP)
- Level Text (TMP)
- XP Bar (Image + Fill)
- ELO Text (TMP)
- Rank Icon (Image)
- Rank Text (TMP)

#### **4. Currency Panel** (Top-Right)
- **Type**: Panel
- **Size**: 250x120 pixels
- **Position**: (-20, -20) from top-right anchor
- **Color**: Semi-transparent dark (#1A1A26, alpha 180)

**Contains**:
- Credits Icon (32x32)
- Credits Text (TMP)
- Gems Icon (32x32)
- Gems Text (TMP)

#### **5. Navigation Buttons**

**Left Side Buttons**:
- Ships Button: Position (-700, 150)
- Quests Button: Position (-700, -50)

**Right Side Buttons**:
- Missiles Button: Position (700, 150)
- Battle Pass Button: Position (700, -50)

**Top Buttons**:
- Profile Button: Position (-850, 480)
- Leaderboard Button: Position (-650, 480)
- Settings Button: Position (850, 480)

**Button Specs**:
- **Size**: 180x80 pixels
- **Icon Size**: 48x48 pixels
- **Text Size**: 20pt
- **Spacing**: Icon left, text right
- **Background**: Semi-transparent (#2C3E50, alpha 200)

#### **6. Quick Play Button** (Bottom-Center)
- **Type**: Button
- **Size**: 400x120 pixels
- **Position**: (0, -380) from center
- **Colors**:
  - Normal: Green (#27AE60)
  - Hover: Bright green (#2ECC71)
  - Pressed: Dark green (#229954)
  - Disabled: Gray (#95A5A6)
- **Icon**: 64x64 play icon
- **Text**: "QUICK PLAY" 36pt bold

#### **7. Bottom Navigation Bar**
- **Type**: Panel
- **Size**: 1920x80 pixels (full width)
- **Position**: Bottom (0, 0)
- **Color**: Very dark (#0D0D14, alpha 220)

**Contains** (evenly spaced):
- Inventory Button (small, 80x60)
- (Future buttons as needed)

---

## 📝 Text & Font Specifications

### **Recommended Fonts**

**Primary Font** (Headers, Buttons):
- **Name**: Orbitron Bold OR Rajdhani Bold
- **Source**: Google Fonts (free)
- **Fallback**: Arial Bold

**Secondary Font** (Body Text):
- **Name**: Roboto Regular OR Open Sans
- **Source**: Google Fonts (free)
- **Fallback**: Arial

**Monospace Font** (Numbers, Stats):
- **Name**: Roboto Mono OR Courier New
- **For**: Credits, Gems, XP, ELO

### **Text Elements Specifications**

| Element | Font | Size | Color | Weight | Alignment |
|---------|------|------|-------|--------|-----------|
| Username | Orbitron | 36pt | White (#FFFFFF) | Bold | Left |
| Level | Orbitron | 24pt | Gold (#FFD700) | Bold | Left |
| XP Text | Roboto | 16pt | White (#FFFFFF) | Regular | Center |
| ELO | Orbitron | 28pt | Gold (#FFD700) | Bold | Left |
| Rank Name | Orbitron | 20pt | Gold (#FFD700) | Bold | Left |
| Credits | Roboto Mono | 24pt | Gold (#FFD700) | Bold | Right |
| Gems | Roboto Mono | 24pt | Cyan (#00FFFF) | Bold | Right |
| Button Text | Rajdhani | 20pt | White (#FFFFFF) | Bold | Center |
| Quick Play | Orbitron | 36pt | White (#FFFFFF) | Bold | Center |

### **TextMeshPro Settings**

**For all text elements**:
- **Material Preset**: LiberationSans SDF - Outline
- **Outline Thickness**: 0.2
- **Outline Color**: Black (#000000, alpha 200)
- **Enable Word Wrapping**: Where needed
- **Auto Size**: Disabled (use fixed sizes)

---

## 🏗️ Prefab Structure

### **Folder Organization**

```
Assets/
├── UI/
│   ├── MainMenu/
│   │   ├── Icons/
│   │   │   ├── Navigation/
│   │   │   │   ├── icon_ships_normal.png
│   │   │   │   ├── icon_ships_hover.png
│   │   │   │   ├── icon_ships_pressed.png
│   │   │   │   ├── icon_ships_disabled.png
│   │   │   │   └── [... all other navigation icons]
│   │   │   ├── Currency/
│   │   │   │   ├── icon_credits.png
│   │   │   │   └── icon_gems.png
│   │   │   ├── Ranks/
│   │   │   │   ├── icon_rank_bronze_1.png
│   │   │   │   ├── icon_rank_bronze_2.png
│   │   │   │   └── [... all rank icons]
│   │   │   └── Misc/
│   │   │       ├── icon_notification_normal.png
│   │   │       ├── icon_notification_active.png
│   │   │       └── icon_quickplay.png
│   │   ├── Prefabs/
│   │   │   ├── MainMenuCanvas.prefab
│   │   │   ├── NavigationButton.prefab
│   │   │   ├── PlayerInfoPanel.prefab
│   │   │   ├── CurrencyPanel.prefab
│   │   │   └── ShipViewerCamera.prefab
│   │   ├── Scripts/
│   │   │   ├── MainMenuController.cs
│   │   │   ├── MainMenuUI.cs
│   │   │   ├── ShipViewer3D.cs
│   │   │   ├── NavigationSystem.cs
│   │   │   ├── CurrencyDisplay.cs
│   │   │   └── PanelManager.cs
│   │   └── Fonts/
│   │       ├── Orbitron-Bold SDF.asset
│   │       └── Roboto-Regular SDF.asset
```

### **Prefab Hierarchy**

#### **MainMenuCanvas.prefab**
```
MainMenuCanvas
├── BackgroundPanel
├── PanelsContainer
│   ├── HomePanel
│   │   ├── ShipViewerDisplay (Raw Image)
│   │   ├── PlayerInfoPanel
│   │   │   ├── UsernameText (TMP)
│   │   │   ├── LevelText (TMP)
│   │   │   ├── XPBarBackground (Image)
│   │   │   │   └── XPFillBar (Image - Filled)
│   │   │   ├── XPText (TMP)
│   │   │   ├── ELOText (TMP)
│   │   │   ├── RankIcon (Image)
│   │   │   └── RankText (TMP)
│   │   ├── CurrencyPanel
│   │   │   ├── CreditsGroup
│   │   │   │   ├── CreditsIcon (Image)
│   │   │   │   └── CreditsText (TMP)
│   │   │   └── GemsGroup
│   │   │       ├── GemsIcon (Image)
│   │   │       └── GemsText (TMP)
│   │   ├── NotificationButton
│   │   │   ├── NotificationIcon (Image)
│   │   │   └── BadgeDot (Image - conditional)
│   │   ├── LeftNavigation
│   │   │   ├── ShipsButton (NavigationButton prefab)
│   │   │   └── QuestsButton (NavigationButton prefab)
│   │   ├── RightNavigation
│   │   │   ├── MissilesButton (NavigationButton prefab)
│   │   │   └── BattlePassButton (NavigationButton prefab)
│   │   ├── TopNavigation
│   │   │   ├── ProfileButton (NavigationButton prefab)
│   │   │   ├── LeaderboardButton (NavigationButton prefab)
│   │   │   └── SettingsButton (NavigationButton prefab)
│   │   └── QuickPlayButton
│   │       ├── QuickPlayIcon (Image)
│   │       └── QuickPlayText (TMP)
│   ├── ShipSelectionPanel (empty for now)
│   ├── MissileLoadoutPanel (empty for now)
│   ├── BattlePassPanel (empty for now)
│   ├── QuestsPanel (empty for now)
│   ├── ProfilePanel (empty for now)
│   └── SettingsPanel (empty for now)
└── EventSystem
```

#### **NavigationButton.prefab** (Reusable)
```
NavigationButton (Button component)
├── Background (Image)
├── IconImage (Image) ← Swap sprite for different buttons
└── ButtonText (TMP)
```

---

## 🔧 Step-by-Step Building Process

### **Phase 1: Import Assets**

#### **Step 1: Import Icons to Unity**

1. Create folder structure in Unity:
   - `Assets/UI/MainMenu/Icons/Navigation/`
   - `Assets/UI/MainMenu/Icons/Currency/`
   - `Assets/UI/MainMenu/Icons/Ranks/`
   - `Assets/UI/MainMenu/Icons/Misc/`

2. Import all 56 icon PNG files to respective folders

3. Configure import settings for each icon:
   - Select all icons in Unity
   - **Inspector** → Texture Type: **Sprite (2D and UI)**
   - Max Size: **2048** (for 512px icons) or **1024** (for 256px)
   - Format: **RGBA Compressed**
   - Compression: **High Quality**
   - **Apply**

#### **Step 2: Import Fonts**

1. Download fonts from Google Fonts:
   - Orbitron Bold (or Rajdhani Bold)
   - Roboto Regular
   - Roboto Mono

2. Import to `Assets/UI/MainMenu/Fonts/`

3. **Create TextMeshPro Font Assets**:
   - Window → TextMeshPro → Font Asset Creator
   - Source Font File: Select Orbitron-Bold.ttf
   - Sampling Point Size: **Auto Sizing**
   - Atlas Resolution: **2048x2048**
   - Character Set: **ASCII**
   - Render Mode: **SDFAA**
   - Padding: **5**
   - **Generate Font Atlas**
   - Save as: `Orbitron-Bold SDF.asset`

4. Repeat for Roboto fonts

#### **Step 3: Setup LeanTween Animation System** ⚠️ **ESSENTIAL**

**LeanTween** is a lightweight animation library that provides smooth UI transitions and animations. It's **required** for the main menu to function properly.

**What LeanTween Does:**
- Smooth fade-in/fade-out transitions for panels
- Button press scale animations (visual feedback)
- Menu transition effects
- Optimized performance (better than Unity's built-in Animation system for UI)

**Installation Check:**

1. **Verify LeanTween is in your project**:
   - Check if folder exists: `Assets/LeanTween/`
   - If missing, download from: [LeanTween on Unity Asset Store](https://assetstore.unity.com/packages/tools/animation/leantween-3595) (FREE)
   - Or download from: [GitHub - LeanTween](https://github.com/dentedpixel/LeanTween)

2. **Import LeanTween** (if not present):
   - **From Asset Store**:
     - Window → Package Manager → My Assets
     - Search "LeanTween"
     - Download and Import

   - **From GitHub**:
     - Download `LeanTween.cs` from repository
     - Place in: `Assets/LeanTween/` or `Assets/Plugins/`

3. **Verify Installation**:
   - Open Unity console
   - Create new C# script temporarily
   - Add line: `LeanTween.init();`
   - If no errors → LeanTween is ready ✓
   - Delete test script

**Used Animations in Main Menu:**
- **Fade In**: Menu appears smoothly when loaded (0.5s ease-out)
- **Fade Out**: Menu disappears smoothly when transitioning (0.5s ease-in)
- **Button Press**: Buttons scale to 95% then back to 100% when clicked (0.2s total)

**No Configuration Needed** - LeanTween works out-of-the-box once imported. The MainMenuUI script already has all animation code ready.

---

### **Phase 2: Scene Setup**

#### **Step 4: Create Main Menu Scene**

1. Create new scene: **MainMenuScene.unity**
2. Save to: `Assets/Scenes/MainMenuScene.unity`

#### **Step 5: Setup Ship Viewer System**

1. Create empty GameObject: **"ShipViewerSystem"**
   - Position: (0, 0, 0)

2. Add child: **"ShipContainer"**
   - Position: (0, 0, 0)

3. Add child: **"ShipCamera"**
   - Add **Camera** component
   - Position: (0, 2, -10)
   - Clear Flags: **Solid Color**
   - Background: #1A1A26
   - Culling Mask: **Default**

4. Create **Render Texture**:
   - Assets → Create → Render Texture
   - Name: **"ShipViewerRT"**
   - Size: 1024x1024
   - Depth Buffer: 16 bit
   - Anti-aliasing: 4x

5. Assign Render Texture:
   - Select **ShipCamera**
   - Target Texture: **ShipViewerRT**

6. Add lighting:
   - Create **Directional Light** as child of ShipViewerSystem
   - Name: **"ShipLight"**
   - Rotation: (50, -30, 0)
   - Intensity: 1.2

7. Add **ShipViewer3D** component to ShipViewerSystem:
   - Ship Container: **ShipContainer**
   - Rotation Speed: **30**
   - Auto Rotate: **✓**

---

### **Phase 3: Build UI Canvas**

#### **Step 6: Create Main Canvas**

1. GameObject → UI → Canvas
2. Name: **"MainMenuCanvas"**
3. Canvas settings:
   - Render Mode: **Screen Space - Overlay**
   - Canvas Scaler:
     - UI Scale Mode: **Scale With Screen Size**
     - Reference Resolution: **1920 x 1080**
     - Match: **0.5**

4. Add **Canvas Group** component (for fade animations)

#### **Step 7: Create Background**

1. Right-click **MainMenuCanvas** → UI → Panel
2. Name: **"BackgroundPanel"**
3. Stretch to full screen (Alt+Shift while clicking stretch anchor)
4. **Image** component:
   - Color: #0D0D14
   - Or use gradient texture

#### **Step 8: Create Panels Container**

1. Right-click **MainMenuCanvas** → Create Empty
2. Name: **"PanelsContainer"**
3. Stretch to full screen

#### **Step 9: Create Home Panel**

1. Right-click **PanelsContainer** → Create Empty
2. Name: **"HomePanel"**
3. This will hold all main menu elements

---

### **Phase 4: Build Player Info Panel**

#### **Step 10: Create Player Info Panel**

1. Right-click **HomePanel** → UI → Panel
2. Name: **"PlayerInfoPanel"**
3. Settings:
   - Anchor: **Top-Left**
   - Position: (220, -120)
   - Size: (400, 220)
   - Color: #1A1A26, alpha 180

4. Add **Vertical Layout Group** (optional, for auto-spacing):
   - Padding: (15, 15, 15, 15)
   - Spacing: 10
   - Child Alignment: Upper Left

#### **Step 11: Add Username Text**

1. Right-click **PlayerInfoPanel** → UI → Text - TextMeshPro
2. Name: **"UsernameText"**
3. Settings:
   - Text: "PlayerName"
   - Font Asset: **Orbitron-Bold SDF**
   - Font Size: **36**
   - Color: #FFFFFF
   - Alignment: Left, Top
   - Position: (10, -10)

#### **Step 12: Add Level Text**

1. Right-click **PlayerInfoPanel** → UI → Text - TextMeshPro
2. Name: **"LevelText"**
3. Settings:
   - Text: "Level 1"
   - Font Asset: **Orbitron-Bold SDF**
   - Font Size: **24**
   - Color: #FFD700 (gold)
   - Position: (10, -50)

#### **Step 13: Add XP Bar**

1. **Background**:
   - Right-click **PlayerInfoPanel** → UI → Image
   - Name: **"XPBarBackground"**
   - Size: (380, 20)
   - Position: (0, -90) relative to panel
   - Color: #2A2A3A

2. **Fill**:
   - Right-click **XPBarBackground** → UI → Image
   - Name: **"XPFillBar"**
   - Stretch to fill parent
   - Color: #4CAF50 (green)
   - Image Type: **Filled**
   - Fill Method: **Horizontal**
   - Fill Origin: **Left**
   - Fill Amount: **0.5** (50% - will be dynamic)

3. **XP Text**:
   - Right-click **XPBarBackground** → UI → Text - TextMeshPro
   - Name: **"XPText"**
   - Text: "500 / 1000 XP"
   - Font Size: **16**
   - Color: #FFFFFF
   - Alignment: Center

#### **Step 14: Add ELO and Rank**

1. **ELO Text**:
   - Right-click **PlayerInfoPanel** → UI → Text - TextMeshPro
   - Name: **"ELOText"**
   - Text: "1200 ELO"
   - Font: **Orbitron-Bold SDF**
   - Font Size: **28**
   - Color: #FFD700
   - Position: (10, -130)

2. **Rank Icon**:
   - Right-click **PlayerInfoPanel** → UI → Image
   - Name: **"RankIcon"**
   - Size: (48, 48)
   - Position: (10, -175)
   - Sprite: **icon_rank_gold_3** (default)

3. **Rank Text**:
   - Right-click **PlayerInfoPanel** → UI → Text - TextMeshPro
   - Name: **"RankText"**
   - Text: "Gold III"
   - Font Size: **20**
   - Color: #FFD700
   - Position: (70, -185)

---

### **Phase 5: Build Currency Panel**

#### **Step 15: Create Currency Panel**

1. Right-click **HomePanel** → UI → Panel
2. Name: **"CurrencyPanel"**
3. Settings:
   - Anchor: **Top-Right**
   - Position: (-145, -90)
   - Size: (250, 120)
   - Color: #1A1A26, alpha 180

#### **Step 16: Add Credits Display**

1. **Credits Group** (for organization):
   - Right-click **CurrencyPanel** → Create Empty
   - Name: **"CreditsGroup"**
   - Horizontal Layout Group (optional)

2. **Credits Icon**:
   - Right-click **CreditsGroup** → UI → Image
   - Name: **"CreditsIcon"**
   - Size: (32, 32)
   - Sprite: **icon_credits**
   - Position: (10, -20)

3. **Credits Text**:
   - Right-click **CreditsGroup** → UI → Text - TextMeshPro
   - Name: **"CreditsText"**
   - Text: "1,250"
   - Font: **Roboto Mono SDF**
   - Font Size: **24**
   - Color: #FFD700
   - Alignment: Right
   - Position: (50, -20)

#### **Step 17: Add Gems Display**

Repeat same process as Credits:
- **GemsGroup**
- **GemsIcon** (sprite: icon_gems)
- **GemsText** (color: #00FFFF cyan)
- Position below credits (Y: -70)

---

### **Phase 6: Build Navigation Buttons**

#### **Step 18: Create Navigation Button Prefab**

1. Right-click **HomePanel** → UI → Button
2. Name: **"NavigationButton"**
3. **Button** component settings:
   - Size: (180, 80)
   - Colors:
     - Normal: #2C3E50
     - Highlighted: #34495E
     - Pressed: #1ABC9C
     - Disabled: #95A5A6
   - Transition: **Sprite Swap** (more versatile than Color Tint)

4. **Add Icon**:
   - Right-click **NavigationButton** → UI → Image
   - Name: **"IconImage"**
   - Size: (48, 48)
   - Position: (-50, 0) (left side of button)

5. **Update Button Text**:
   - Select **Text (TMP)** child
   - Font: **Rajdhani Bold SDF**
   - Font Size: **20**
   - Color: #FFFFFF
   - Alignment: Right
   - Position: (20, 0)

6. **Make Prefab**:
   - Drag **NavigationButton** to `Assets/UI/MainMenu/Prefabs/`
   - Delete from scene (we'll create instances next)

#### **Step 19: Create Left Navigation**

1. Right-click **HomePanel** → Create Empty
2. Name: **"LeftNavigation"**
3. Position: (-700, 0)

4. **Ships Button**:
   - Drag **NavigationButton** prefab into **LeftNavigation**
   - Name: **"ShipsButton"**
   - Position: (0, 150)
   - Icon: **icon_ships_normal**
   - Text: "SHIPS"
   - **Button Transition → Sprite Swap**:
     - Highlighted Sprite: icon_ships_hover
     - Pressed Sprite: icon_ships_pressed
     - Disabled Sprite: icon_ships_disabled

5. **Quests Button**:
   - Duplicate **ShipsButton**
   - Name: **"QuestsButton"**
   - Position: (0, -50)
   - Icon: icon_quests_normal (+ hover, pressed, disabled)
   - Text: "QUESTS"

#### **Step 20: Create Right Navigation**

Same process:
1. Create **"RightNavigation"** at (700, 0)
2. **MissilesButton** at (0, 150)
3. **BattlePassButton** at (0, -50)

#### **Step 21: Create Top Navigation**

1. Create **"TopNavigation"** at (0, 480)
2. **ProfileButton** at (-850, 0) - size (150, 70)
3. **LeaderboardButton** at (-650, 0)
4. **SettingsButton** at (850, 0)

---

### **Phase 7: Build Ship Viewer Display**

#### **Step 22: Add Ship Viewer to Canvas**

1. Right-click **HomePanel** → UI → Raw Image
2. Name: **"ShipViewerDisplay"**
3. Settings:
   - Position: (0, 50) (center)
   - Size: (600, 600)
   - Texture: **ShipViewerRT**

---

### **Phase 8: Build Quick Play Button**

#### **Step 23: Create Quick Play Button**

1. Right-click **HomePanel** → UI → Button
2. Name: **"QuickPlayButton"**
3. Settings:
   - Position: (0, -380)
   - Size: (400, 120)
   - Colors:
     - Normal: #27AE60 (green)
     - Highlighted: #2ECC71
     - Pressed: #229954
     - Disabled: #95A5A6

4. **Add Icon**:
   - Right-click **QuickPlayButton** → UI → Image
   - Name: **"QuickPlayIcon"**
   - Size: (64, 64)
   - Sprite: **icon_quickplay**
   - Position: (-120, 0)

5. **Update Text**:
   - Select **Text (TMP)**
   - Text: "QUICK PLAY"
   - Font: **Orbitron Bold SDF**
   - Font Size: **36**
   - Bold: ✓
   - Position: (30, 0)

---

### **Phase 9: Wire Up Scripts**

#### **Step 24: Add MainMenuUI Component**

1. Select **MainMenuCanvas**
2. Add Component → **MainMenuUI**
3. Assign references in Inspector:
   - **Player Info Panel**:
     - Username Text: UsernameText
     - Level Text: LevelText
     - XP Fill Bar: XPFillBar
     - XP Text: XPText
     - ELO Text: ELOText
     - Rank Icon: RankIcon
     - Rank Text: RankText
   - **Currency Panel**:
     - Credits Text: CreditsText
     - Gems Text: GemsText
   - **Rank Icons** array (18 elements - Military/Naval Ranks):
     - [0] icon_rank_cadet
     - [1] icon_rank_midshipman
     - [2] icon_rank_ensign
     - ... (all 18 rank icons through Grand Admiral)
   - **Navigation Buttons**:
     - Ships Button: ShipsButton
     - Quests Button: QuestsButton
     - Missiles Button: MissilesButton
     - Battle Pass Button: BattlePassButton
     - Profile Button: ProfileButton
     - Leaderboard Button: LeaderboardButton
     - Settings Button: SettingsButton
   - **Quick Play Button**: QuickPlayButton

#### **Step 25: Add PanelManager Component**

1. Select **MainMenuCanvas**
2. Add Component → **PanelManager**
3. Assign panel references:
   - Home Panel: HomePanel
   - (Others will be empty for now)

#### **Step 26: Add MainMenuController**

1. Create empty GameObject in scene: **"MainMenuController"**
2. Add Component → **MainMenuController**
3. Assign:
   - Ship Viewer: ShipViewerSystem
   - Menu UI: MainMenuCanvas

#### **Step 27: Wire Button OnClick Events**

For each navigation button:
1. **ShipsButton**:
   - OnClick() → PanelManager.ShowPanel("ships")

2. **QuestsButton**:
   - OnClick() → PanelManager.ShowPanel("quests")

3. **MissilesButton**:
   - OnClick() → PanelManager.ShowPanel("missiles")

4. **BattlePassButton**:
   - OnClick() → PanelManager.ShowPanel("battlepass")

5. **ProfileButton**:
   - OnClick() → PanelManager.ShowPanel("profile")

6. **SettingsButton**:
   - OnClick() → PanelManager.ShowPanel("settings")

7. **LeaderboardButton**:
   - OnClick() → PanelManager.ShowPanel("leaderboard")

8. **QuickPlayButton**:
   - OnClick() → MainMenuController.OnQuickPlayClicked()

---

### **Phase 10: Create Empty Panels**

#### **Step 28: Create Placeholder Panels**

For each screen, create a basic placeholder:

1. Right-click **PanelsContainer** → UI → Panel
2. Name: **"ShipSelectionPanel"** (repeat for each)
3. Stretch to full screen
4. Add **Back Button**:
   - Position: Top-Left (50, -50)
   - Text: "← BACK"
   - OnClick() → PanelManager.ShowPanel("home")
5. Add **Title Text**:
   - Position: Top-Center (0, -50)
   - Text: "SHIPS" (or screen name)
   - Font Size: 48

Repeat for:
- MissileLoadoutPanel
- BattlePassPanel
- QuestsPanel
- ProfilePanel
- SettingsPanel
- LeaderboardPanel (future: InventoryPanel)

---

### **Phase 11: Create Prefabs**

#### **Step 29: Save Reusable Prefabs**

1. **PlayerInfoPanel**:
   - Drag to `Assets/UI/MainMenu/Prefabs/PlayerInfoPanel.prefab`

2. **CurrencyPanel**:
   - Drag to `Assets/UI/MainMenu/Prefabs/CurrencyPanel.prefab`

3. **NavigationButton**:
   - Already saved

4. **MainMenuCanvas**:
   - Drag entire canvas to `Assets/UI/MainMenu/Prefabs/MainMenuCanvas.prefab`

---

## 🧪 Unity Component Settings

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

### **Button Component (Navigation Buttons)**
```
Button
├── Transition: Sprite Swap
├── Target Graphic: Background Image
├── Normal Sprite: icon_[name]_normal
├── Highlighted Sprite: icon_[name]_hover
├── Pressed Sprite: icon_[name]_pressed
├── Disabled Sprite: icon_[name]_disabled
└── Navigation: Automatic
```

### **Image Component (Filled - XP Bar)**
```
Image
├── Image Type: Filled
├── Fill Method: Horizontal
├── Fill Origin: Left
├── Fill Amount: 0.0 - 1.0 (dynamic)
└── Preserve Aspect: ✗
```

### **TextMeshPro Settings**
```
TextMeshPro - Text
├── Font Asset: Orbitron-Bold SDF
├── Font Size: [see table above]
├── Auto Size: ✗
├── Color: [see table above]
├── Extra Settings
│   ├── Enable Outline: ✓
│   ├── Outline Thickness: 0.2
│   └── Outline Color: #000000 (alpha 200)
└── Overflow: Ellipsis or Overflow
```

---

## ✅ Testing Checklist

### **Visual Checks**
- [ ] All icons display correctly in all 4 states
- [ ] Text is readable and properly sized
- [ ] Colors match specifications
- [ ] Layout looks correct at 1920x1080
- [ ] Ship viewer displays in center (even if empty)
- [ ] No overlapping UI elements
- [ ] Currency panel aligned to top-right
- [ ] Player info panel aligned to top-left
- [ ] Quick Play button is prominent at bottom

### **Functionality Checks**
- [ ] Navigation buttons switch panels
- [ ] Back buttons return to home panel
- [ ] Buttons show hover state when mouse over
- [ ] Buttons show pressed state when clicked
- [ ] Quick Play button triggers correct function
- [ ] Ship viewer camera renders to texture

### **Animation Checks** (LeanTween)
- [ ] Main menu fades in smoothly when scene loads (0.5s)
- [ ] Main menu fades out smoothly when transitioning (0.5s)
- [ ] Buttons scale down to 95% then back when clicked
- [ ] No LeanTween errors in console
- [ ] Animations feel smooth and responsive

### **Integration Checks**
- [ ] ProgressionManager loads player data
- [ ] Username displays from PlayerAccountData
- [ ] Level displays correctly
- [ ] XP bar fills based on actual XP
- [ ] ELO displays correctly
- [ ] Rank icon changes based on ELO
- [ ] Credits display with comma formatting (1,250)
- [ ] Gems display correctly
- [ ] Ship loads in viewer (when equipped)

### **Performance Checks**
- [ ] No console errors
- [ ] Smooth 60 FPS
- [ ] No memory leaks (check Profiler)
- [ ] UI scales properly on different resolutions

---

## 📊 Resolution Testing

Test these resolutions:
- **1920x1080** (Primary)
- **1280x720** (HD)
- **2560x1440** (2K)
- **3840x2160** (4K)

UI should scale proportionally on all.

---

## 🎨 PaintShop Pro Tips

### **Creating Icons**

1. **New Image**:
   - Size: 512x512 (or 256x256 for small icons)
   - Background: Transparent
   - Color Depth: 16 million colors (24-bit)

2. **Design Icon**:
   - Use vector tools for clean edges
   - Keep design centered
   - Leave 20-30px padding from edges

3. **Create Variations**:
   - Save master as .psp file
   - For **Normal**: Use base gray color (#B0B0B0)
   - For **Hover**: Duplicate, adjust to white (#FFFFFF), add glow:
     - Effects → 3D Effects → Outer Glow
     - Color: Match theme (blue, gold, etc.)
     - Opacity: 80%
     - Blur: 10-15
   - For **Pressed**: Darken (#808080), add inner shadow:
     - Effects → 3D Effects → Inner Bevel (slight)
   - For **Disabled**: Very dark (#404040), reduce opacity to 50%

4. **Export**:
   - File → Export → PNG Optimizer
   - Format: PNG-24 (with transparency)
   - Compression: 9 (max)
   - Interlacing: None

---

## 🚀 Next Steps After First Screen

Once the main hub is complete:

1. **Implement Ship Selection Panel**
   - Display ship cards
   - Allow equipping ships
   - Show ship stats

2. **Implement Missile Loadout Panel**
   - Display missile presets
   - Allow equipping missiles
   - Show missile stats

3. **Add More Animations** (Basic animations already included via LeanTween)
   - Panel slide transitions (in/out from sides)
   - Additional button hover effects
   - Ship rotation smoothing/easing

4. **Polish**
   - Add sound effects
   - Add particle effects
   - Improve visual feedback

5. **Test Online Integration**
   - Wire Quick Play to matchmaking
   - Test with real player data

---

## 📝 Quick Reference

### **Key Shortcuts**
- **Alt + Shift + Anchor Preset** = Stretch to fill parent
- **Ctrl + D** = Duplicate selected object
- **Ctrl + Shift + N** = Create new GameObject
- **F** = Frame selected object in Scene view
- **Shift + F** = Frame and lock camera to object

### **Common Colors**
```
Dark Background: #0D0D14
Panel Background: #1A1A26 (alpha 180)
Button Normal: #2C3E50
Button Hover: #34495E
Button Pressed: #1ABC9C
White Text: #FFFFFF
Gold Text: #FFD700
Cyan: #00FFFF
Green (Play): #27AE60
```

---

## 🎯 Summary

**Total Assets to Create:**
- **55 icon files** (PNG) - Updated to 18 military/naval rank icons
- **2 fonts** (imported from Google Fonts, converted to TMP)
- **7 prefabs** (NavigationButton, PlayerInfoPanel, CurrencyPanel, etc.)
- **1 scene** (MainMenuScene.unity)
- **1 render texture** (ShipViewerRT)
- **LeanTween** ✓ Already included (animation system)

**Estimated Time:**
- Icon creation: **8-12 hours** (for all 55 icons with variations)
- Unity setup: **4-6 hours**
- Testing & polish: **2-3 hours**
- **Total: 14-21 hours**

**You are now ready to build the first screen!** 🚀

Start with creating the icons in PaintShop Pro, then import to Unity and follow the step-by-step build process. Good luck!
