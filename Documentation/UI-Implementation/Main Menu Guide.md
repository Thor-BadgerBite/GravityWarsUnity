# Gravity Wars - Main Menu Hub Guide
**Complete Main Menu Implementation Guide**

---

## 📋 Overview

This guide provides a comprehensive overview of the **Main Menu Hub** - the primary screen players see when they launch Gravity Wars. This is the central hub that connects to all other game systems and screens.

**Purpose:** Understand the complete layout, navigation flow, and all subsystems before building.

**Companion Guide:** See `MAIN_MENU_HUB_BUILD_GUIDE.md` for step-by-step implementation instructions.

---

## 🎨 Main Menu Hub Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ┌──────────────┐                                      ┌──────────────────┐ │
│  │  [Profile]   │      [🔔]  [GRAVITY WARS]  [📅]     │ 💰 12,450  [+]  │ │
│  │   LVL 42     │                LOGO                  │ 💎 250     [+]  │ │
│  │ ⚡8500/10000 │                                       │                  │ │
│  │ [Rank Icon]  │                                       │ Battle Pass      │ │
│  └──────────────┘                                       │ Tier 12/50  ▓▓░░ │ │
│                                                          └──────────────────┘ │
│   ┌────────┐              ┌─────────────────┐                  ┌────────┐   │
│   │ SHIPS  │              │                 │                  │  SHOP  │   │
│   │GARAGE  │              │   3D SHIP       │                  │        │   │
│   └────────┘              │   MODEL         │                  └────────┘   │
│                           │   (Rotating)    │                                │
│   ┌────────┐              │                 │                  ┌────────┐   │
│   │INVENT- │              │                 │                  │ACHIEVE-│   │
│   │  ORY   │              └─────────────────┘                  │ MENTS  │   │
│   └────────┘                                                    └────────┘   │
│                           W/L: 84/43 | ELO: 1450                             │
│   ┌────────┐         ┌──────────────────────────────┐          ┌────────┐   │
│   │ LEADER-│         │         [PLAY NOW]           │          │ACCOUNT │   │
│   │ BOARDS │         │      ▶ ────────────           │          │PROGRESS│   │
│   └────────┘         └──────────────────────────────┘          └────────┘   │
│                                                                               │
│   ┌────────┐         ┌────────┐ ┌──────┐ ┌────────┐           ┌────────┐   │
│   │ FRIENDS│         │ QUESTS │ │BATTLE│ │ RANKED │           │  CLAN  │   │
│   └────────┘         │        │ │ PASS │ │        │           └────────┘   │
│                      └────────┘ └──────┘ └────────┘                         │
│                                                                               │
│                    ┌───────────────────────────────┐                         │
│  [⚙️]              │  LOCAL  │ ONLINE │  RANKED   │         ┌──────────┐    │
│ SETTINGS           └───────────────────────────────┘         │ LOADOUT  │    │
│                                                               │ [🚀][🔧] │    │
│                                                               └──────────┘    │
│  Latest Update: New 'Void Stalker' Ship Released! Triple XP Weekend!        │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🗺️ Screen Sections Breakdown

### **1. TOP LEFT - Player Profile Section**

**Components:**
- Player profile icon (clickable)
- Current level display
- Current rank icon (rank1.png to rank18.png)
- XP progress bar with text (e.g., "8500/10000")

**Functionality:**
- **Click Profile Icon** → Opens **Player Profile Screen** (see section below)
- Shows visual progression at a glance

---

### **2. TOP CENTER - Logo and Notifications**

**Components:**
- Game logo ("GRAVITY WARS - INTERSTELLAR CONQUEST")
- **Reminder/Notification Button** 🔔
- **Calendar Button** 📅

**Functionality:**
- **Notification Button** → Opens **Notifications Screen** with:
  - Daily quest reminders
  - Friend requests
  - Season announcements
  - Achievement unlocks

- **Calendar Button** → Opens **Events Calendar Screen** with:
  - Upcoming seasons
  - Special events (Double XP Weekend, etc.)
  - Season start/end dates
  - Holiday events

---

### **3. TOP RIGHT - Currencies and Battle Pass**

**Components:**
- **Coins Display** with + button
- **Gems Display** with + button
- **Battle Pass Progress Bar** (Tier X/50)

**Functionality:**
- **Coins + Button** → Opens **Shop Screen** (Coins section)
- **Gems + Button** → Opens **Shop Screen** (Gems section)
- **Battle Pass Bar (clickable)** → Opens **Battle Pass Screen**

---

### **4. CENTER - 3D Ship Viewer**

**Components:**
- Real-time 3D model of currently equipped ship
- Interactive (player can rotate by dragging)
- Rendered using separate camera to RenderTexture

**Functionality:**
- Displays the player's active ship
- Updates when ship is changed in Ships Garage
- Shows visual customization (skins, colors if implemented)

---

### **5. LEFT SIDE - Navigation Frame (4 Buttons)**

#### **Button 1: Ships Garage** 🚀
**Leads to:** Ships Garage Screen
**Purpose:** Ship selection and management
**Screen Details:**
- Shows all ships player has unlocked
- Displays ship info: Health, Armor, Speed, Special Abilities
- Player can select and equip a ship for next game
- Shows locked ships (greyed out with lock icon)
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 2: Inventory** 📦
**Leads to:** Inventory Screen
**Purpose:** Informational screen showing all unlocked items
**Screen Details:**
- Ship bodies unlocked
- Perks unlocked
- Passives unlocked
- Missiles unlocked
- Shows stats and info for each item
- Used for custom ship building reference
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 3: Leaderboards** 🏆
**Leads to:** Leaderboards Screen
**Purpose:** Rankings and competitive info
**Screen Details:**
- **Local Leaderboard:** Top players in region
- **World Leaderboard:** Top players globally
- Shows: Rank, Username, ELO, Rank Icon
- Highlights current player's position
- Shows player's current rank and progress to next rank
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 4: Friends** 👥
**Leads to:** Friends Screen
**Purpose:** Friend management (to be implemented later)
**Screen Details:**
- Friends list
- Add/remove friends
- See friends online status
- Invite friends to party
- **States:** Normal, Hover, Pressed, Disabled

---

### **6. RIGHT SIDE - Navigation Frame (4 Buttons)**

#### **Button 1: Shop** 🛒
**Leads to:** Shop Screen
**Purpose:** In-game purchases
**Screen Details:**
- **Gems Section:** Purchase gems with real money
- **Skins Section:** Purchase ship skins (real money or coins)
- **Ship Bodies Section:** Purchase ship bodies with coins or real money
- **Special Offers:** Limited-time bundles
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 2: Achievements** 🎖️
**Leads to:** Achievements Screen (to be implemented later)
**Purpose:** Achievement tracking
**Screen Details:**
- Shows all achievements (locked/unlocked)
- Achievement progress bars
- Achievement points total
- Rewards for completing achievements
- Categories: Combat, Exploration, Social, Mastery
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 3: Account Progress** 📊
**Leads to:** Account Progress Screen
**Purpose:** Level progression visualization
**Screen Details:**
- Timeline showing all levels (1 to max level)
- Shows current level (highlighted)
- Displays items unlocked at each level
- Shows items yet to be unlocked
- Visual progression path
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 4: Clan** 🛡️
**Leads to:** Clan Screen (to be implemented later)
**Purpose:** Clan/guild system
**Screen Details:**
- Current clan info
- Members list and online status
- Clan wars/events
- Clan chat
- Join/leave clan options
- **States:** Normal, Hover, Pressed, Disabled

---

### **7. CENTER TOP - Player Stats Display**

**Components:**
- Win/Loss ratio (e.g., "W/L: 84/43")
- Current ELO rating (e.g., "ELO: 1450")

**Functionality:**
- Quick reference for player performance
- Updates in real-time as matches complete

---

### **8. CENTER - Main Action Buttons (3 Buttons)**

#### **Button 1: Quests** 📜
**Leads to:** Quests Screen
**Purpose:** Quest tracking and progression
**Screen Details:**
- **Daily Quests:** Refresh every 24 hours
- **Weekly Quests:** Refresh every 7 days
- **Monthly Quests:** Refresh every 30 days
- **Battle Pass Quests:** Season-specific
- Shows quest progress (e.g., "Win 3 matches: 2/3")
- Shows quest rewards (XP, coins, items)
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 2: Play Now** ▶️
**Leads to:** Matchmaking System
**Purpose:** PRIMARY GAME BUTTON - Start a match
**Screen Details:**
- Starts matchmaking based on selected game mode (Local/Online/Ranked)
- Shows matchmaking progress
- Displays estimated wait time
- Option to cancel matchmaking
- **This is the most prominent button on screen**
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 3: Battle Pass** 🎖️
**Leads to:** Battle Pass Screen
**Purpose:** Battle Pass progression and rewards
**Screen Details:**
- Shows current battle pass tier (e.g., "Tier 12/50")
- Displays all tiers with rewards
- **Free Track:** Available to all players
- **Premium Track:** Purchased with gems
- Shows unlocked items and upcoming items
- XP required for next tier
- "Upgrade to Premium" button if not owned
- **States:** Normal, Hover, Pressed, Disabled

---

### **9. CENTER BOTTOM - Game Mode Selection**

**Components:**
- **3 Toggle Buttons** in a frame/container

#### **Mode 1: Local** 🏠
**Purpose:** Play against local AI or practice
**States:** Normal, Hover, Toggled (3 states only, no Disabled)

#### **Mode 2: Online** 🌐
**Purpose:** Play against other players (casual)
**States:** Normal, Hover, Toggled (3 states only, no Disabled)

#### **Mode 3: Ranked** 🏆
**Purpose:** Competitive matches that affect ELO
**States:** Normal, Hover, Toggled (3 states only, no Disabled)

**Functionality:**
- **Exactly ONE mode is selected at all times** (toggled state)
- Clicking a mode toggles it on and untoggles the others
- Selected mode determines behavior of "Play Now" button

---

### **10. BOTTOM LEFT - Settings**

**Components:**
- **Settings Button** ⚙️

**Leads to:** Settings Screen
**Purpose:** Game configuration
**Screen Details:**
- **Graphics Settings:** Resolution, quality, VSync
- **Audio Settings:** Master volume, music, SFX
- **Controls Settings:** Mouse sensitivity, keybindings
- **Account Settings:** Change username, logout
- **Game Settings:** Language, tutorials, notifications
- **States:** Normal, Hover, Pressed, Disabled

---

### **11. BOTTOM RIGHT - Loadout Frame**

**Components:**
- **2 Separate Buttons** in a container

#### **Button 1: Missile Loadout** 🚀
**Leads to:** Missile Loadout Screen
**Purpose:** Select missiles for equipped ship
**Button Icon:** 3D model of currently equipped missile
**Screen Details:**
- Shows all unlocked missiles
- Player selects missile to equip on active ship
- Shows missile stats (Mass, Speed, Damage, Special Effects)
- Only missiles compatible with equipped ship are available
- **States:** Normal, Hover, Pressed, Disabled

#### **Button 2: Custom Ship Builder** 🔧
**Leads to:** Custom Ship Builder Screen
**Purpose:** Build custom ships from unlocked components
**Screen Details:**
- Choose ship body type from unlocked bodies
- Add perks to ship (from unlocked perks)
- Add passives to ship (from unlocked passives)
- Add missile loadout
- Save custom builds
- Load saved builds
- **States:** Normal, Hover, Pressed, Disabled

---

### **12. BOTTOM - Latest News Bar**

**Components:**
- Text frame with scrolling or static news text
- **Clickable**

**Leads to:** Latest News/Patch Notes Screen
**Purpose:** Inform players of updates
**Screen Details:**
- Latest patch notes
- Season announcements
- Bug fixes
- New content releases
- Developer messages
- Event announcements

---

## 📊 Complete Screen Flow Map

```
Main Menu Hub (THIS SCREEN)
├── Player Profile Screen
│   └── Statistics, Match History, etc.
├── Notifications Screen
│   └── Alerts, Reminders, Announcements
├── Events Calendar Screen
│   └── Seasons, Special Events, Schedule
├── Shop Screen
│   ├── Gems Purchase
│   ├── Skins
│   ├── Ship Bodies
│   └── Special Offers
├── Battle Pass Screen (2 ways to access)
│   ├── Free Track Rewards
│   ├── Premium Track Rewards
│   └── Upgrade Option
├── Ships Garage Screen
│   ├── Ship Selection
│   ├── Ship Info
│   └── Equip Ship
├── Inventory Screen
│   ├── Ship Bodies
│   ├── Perks
│   ├── Passives
│   └── Missiles
├── Leaderboards Screen
│   ├── Local Rankings
│   ├── World Rankings
│   └── Player Position
├── Friends Screen (Future)
│   ├── Friends List
│   ├── Add/Remove Friends
│   └── Friend Status
├── Achievements Screen (Future)
│   ├── Achievement List
│   ├── Progress Tracking
│   └── Rewards
├── Account Progress Screen
│   ├── Level Timeline
│   ├── Unlocked Items
│   └── Future Unlocks
├── Clan Screen (Future)
│   ├── Clan Info
│   ├── Members
│   └── Clan Events
├── Quests Screen
│   ├── Daily Quests
│   ├── Weekly Quests
│   ├── Monthly Quests
│   └── Battle Pass Quests
├── Matchmaking / Play Game
│   └── Leads to actual gameplay
├── Missile Loadout Screen
│   ├── Missile Selection
│   └── Missile Info
├── Custom Ship Builder Screen
│   ├── Body Selection
│   ├── Perk Selection
│   ├── Passive Selection
│   └── Save/Load Builds
├── Settings Screen
│   ├── Graphics
│   ├── Audio
│   ├── Controls
│   └── Account
└── Latest News Screen
    └── Patch Notes, Updates, Announcements
```

---

## 🎮 Button States Reference

### **Standard Navigation Buttons (Most Buttons)**
All buttons in Left/Right navigation frames, main buttons, and loadout buttons have **4 states**:

1. **Normal** (`_Normal.png`)
   - Default appearance when not interacted with
   - Base color scheme

2. **Hover** (`_Hover.png`)
   - Appears when mouse is over button
   - Usually brighter, with glow effect

3. **Pressed** (`_Pressed.png`)
   - Appears when button is clicked/held down
   - Usually darker, with inner shadow

4. **Disabled** (`_Disabled.png`)
   - Appears when button is not available
   - Greyed out, low opacity

**Examples:**
- `icon_ShipGarage_Normal.png`
- `icon_ShipGarage_Hover.png`
- `icon_ShipGarage_Pressed.png`
- `icon_ShipGarage_Disabled.png`

### **Game Mode Toggle Buttons (Special Case)**
The 3 game mode buttons (Local, Online, Ranked) have **only 3 states**:

1. **Normal** (`_Normal.png`)
   - Default appearance when not selected

2. **Hover** (`_Hover.png`)
   - Appears when mouse is over button

3. **Toggled** (`_Toggled.png`)
   - Appears when this mode is selected
   - Highlighted, usually with accent color
   - **Only ONE mode can be toggled at a time**

**Examples:**
- `icon_Local_Normal.png`
- `icon_Local_Hover.png`
- `icon_Local_Toggled.png`
- `icon_Online_Normal.png`
- `icon_Online_Hover.png`
- `icon_Online_Toggled.png`

**No Disabled state** - These buttons are always available.

---

## 🔄 Interaction Flow Examples

### **Example 1: Changing Equipped Ship**
1. Player clicks **Ships Garage** button
2. **Ships Garage Screen** opens
3. Player selects a different ship
4. Player clicks **Equip** button
5. Screen returns to **Main Menu Hub**
6. **3D Ship Viewer** updates to show new ship

### **Example 2: Starting a Match**
1. Player clicks game mode (e.g., **Online**)
2. **Online** button enters **Toggled** state
3. Previous mode (e.g., Local) returns to **Normal** state
4. Player clicks **Play Now** button
5. **Matchmaking Screen** appears
6. When match is found, game starts

### **Example 3: Customizing Ship Loadout**
1. Player clicks **Custom Ship Builder** (bottom-right)
2. **Custom Ship Builder Screen** opens
3. Player selects body, perks, passives
4. Player clicks **Save Build**
5. Returns to **Main Menu Hub**
6. Player clicks **Missile Loadout** button
7. **Missile Loadout Screen** opens
8. Player selects missile
9. Returns to **Main Menu Hub**
10. Ship is ready with custom loadout

### **Example 4: Viewing Profile Stats**
1. Player clicks **Profile Icon** (top-left)
2. **Player Profile Screen** opens
3. Shows statistics:
   - Total matches played
   - Wins/Losses
   - Win rate %
   - Missiles fired
   - Accuracy %
   - Ships destroyed
   - Favorite ship
   - Play time
   - ELO history graph
4. Player clicks **Back** button
5. Returns to **Main Menu Hub**

---

## 🎨 UI Asset Requirements Summary

### **Icons Needed (with states)**

**Left Frame Buttons (4 states each):**
- Ships Garage: `icon_ShipGarage_Normal/Hover/Pressed/Disabled.png`
- Inventory: `icon_Inventory_Normal/Hover/Pressed/Disabled.png`
- Leaderboards: `icon_LeaderBoards_Normal/Hover/Pressed/Disabled.png`
- Friends: `icon_Friends_Normal/Hover/Pressed/Disabled.png`

**Right Frame Buttons (4 states each):**
- Shop: `icon_Shop_Normal/Hover/Pressed/Disabled.png`
- Achievements: `icon_Achievements_Normal/Hover/Pressed/Disabled.png`
- Account Progress: `icon_Progress_Normal/Hover/Pressed/Disabled.png`
- Clan: `icon_Clan_Normal/Hover/Pressed/Disabled.png`

**Main Center Buttons (4 states each):**
- Quests: `icon_Quests_Normal/Hover/Pressed/Disabled.png`
- Play Now: `icon_Play_Normal/Hover/Pressed/Disabled.png`
- Battle Pass: `icon_BattlePass_Normal/Hover/Pressed/Disabled.png`

**Game Mode Buttons (3 states each):**
- Local: `icon_Local_Normal/Hover/Toggled.png`
- Online: `icon_Online_Normal/Hover/Toggled.png`
- Ranked: `icon_Ranked_Normal/Hover/Toggled.png`

**Loadout Buttons (4 states each):**
- Missile Loadout: Dynamic icon (3D model)
- Custom Builder: `icon_Progress_Normal/Hover/Pressed/Disabled.png`

**Other Icons:**
- Reminder/Notification: `icon_Reminder.png`
- Calendar: `icon_Calendar.png`
- Settings: 4 states (Normal/Hover/Pressed/Disabled)
- Coins: `icon_Coins_Frame.png`
- Gems: `icon_Gems_Frame.png`
- Plus buttons: `icon_CoinsPlus_*`, `icon_GemsPlus_*`
- Battle Pass progress frame: `icon_BattlePassProgress_Frame.png`

**Rank Icons (18 total, single versions):**
- `rank1.png` to `rank18.png` (highest to lowest)
- Based on ELO ranges

**Frames:**
- Player profile frame: `frame_PlayerFrame.png`
- Level frame: `frame_LevelFrame.png`
- XP bar frame: `frame_XPFrame.png`
- Logo frame: `frame_Logo.png`
- Game mode frame: `frame_GameMode.png`

---

## 🔧 Technical Integration Points

### **Data Sources**
- **PlayerAccountData:** Username, level, XP, ELO, rank, credits, gems
- **ProgressionManager:** Unlocked ships, items, perks, passives
- **MatchHistoryData:** Win/loss stats, recent matches
- **QuestSystem:** Active quests, quest progress
- **BattlePassSystem:** Current tier, XP progress
- **SaveSystem:** Load/save all player data

### **Systems to Connect**
1. **ProgressionManager** - Core player data
2. **SaveSystem** - Persist player choices
3. **MatchmakingSystem** - Start games
4. **QuestSystem** - Track and display quests
5. **BattlePassSystem** - Track season progress
6. **ShopSystem** - Handle purchases
7. **FriendsSystem** - Friend management (future)
8. **ClanSystem** - Clan features (future)
9. **AchievementSystem** - Achievements (future)
10. **AudioManager** - UI sounds and music

---

## ✅ Implementation Checklist

### **Phase 1: Main Hub Screen**
- [ ] Create main canvas with proper scaling
- [ ] Build top bar (profile, logo, currencies)
- [ ] Build left navigation frame (4 buttons)
- [ ] Build right navigation frame (4 buttons)
- [ ] Build center 3D ship viewer
- [ ] Build main action buttons (Quests, Play Now, Battle Pass)
- [ ] Build game mode selector
- [ ] Build bottom loadout frame
- [ ] Build settings button
- [ ] Build news bar

### **Phase 2: Core Screens (Priority)**
- [ ] Ships Garage Screen
- [ ] Missile Loadout Screen
- [ ] Shop Screen
- [ ] Quests Screen
- [ ] Battle Pass Screen
- [ ] Settings Screen

### **Phase 3: Secondary Screens**
- [ ] Player Profile Screen
- [ ] Inventory Screen
- [ ] Leaderboards Screen
- [ ] Account Progress Screen
- [ ] Notifications Screen
- [ ] Events Calendar Screen
- [ ] Latest News Screen

### **Phase 4: Future Features**
- [ ] Friends Screen
- [ ] Achievements Screen
- [ ] Clan Screen

---

## 📝 Notes for Implementation

1. **Start with Main Hub** - This is the foundation. Get this right first.

2. **3D Ship Viewer is Critical** - This is the visual centerpiece. Needs proper lighting and rotation.

3. **Button States are Essential** - Make sure all buttons have proper state transitions for good UX.

4. **Game Mode Selection Logic** - Ensure only one mode can be toggled at a time. Save last selected mode.

5. **Responsive Design** - UI must scale properly for different resolutions (1920x1080 is reference).

6. **Performance** - 3D ship viewer should run at 60 FPS. Optimize render texture size if needed.

7. **Data Persistence** - Save player's last selected game mode, last equipped ship, etc.

8. **Accessibility** - Make sure buttons are large enough and have clear visual feedback.

9. **Future-Proofing** - Leave placeholders for future features (Friends, Achievements, Clan).

10. **Consistency** - Use same fonts, colors, and spacing throughout all screens.

---

## 🚀 Next Steps

After reading this guide:

1. Review the `MAIN_MENU_HUB_BUILD_GUIDE.md` for step-by-step implementation
2. Create/gather all required icon assets
3. Build the Main Menu Hub canvas in Unity
4. Implement individual screen panels one by one
5. Connect to existing systems (ProgressionManager, SaveSystem, etc.)
6. Test navigation flow between all screens
7. Polish with animations and sound effects

---

**This is your roadmap. Build the main hub first, then branch out to individual screens.** 🎮
