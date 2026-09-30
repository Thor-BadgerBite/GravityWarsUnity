# Gravity Wars - Complete Screen Catalog
**Brawl Stars-Style UI Screen System**

---

## 📋 Overview

This document catalogs **all UI screens** for Gravity Wars' main menu hub, organized by priority and implementation complexity.

**Total Screens:** 18
**Implementation Status:** Main Menu ✅ | Others: Pending

---

## 🎯 Screen Categories

### **Core Hub Screens** (Priority 1 - Essential)
- Main Menu Hub
- Ship Building/Garage
- Inventory
- Settings

### **Progression Screens** (Priority 2 - High Value)
- Battle Pass
- Quests
- Account Progress/Leveling
- Achievements

### **Social/Competitive Screens** (Priority 3 - Engagement)
- Leaderboards
- Profile
- Friends/Social
- Clan/Teams

### **Monetization Screens** (Priority 4 - Revenue)
- Shop/Store
- Premium Currency Store
- Offers/Deals

### **Game Mode Screens** (Priority 5 - Gameplay)
- Matchmaking Lobby
- Training/Practice
- Custom Game Setup

### **Miscellaneous Screens** (Priority 6 - Polish)
- News/Events
- Notifications
- Tutorial
- Credits/About

---

## 📱 Detailed Screen Specifications

### **CORE HUB SCREENS**

---

#### **1. Main Menu Hub** ✅
**Status:** IMPLEMENTED
**Priority:** 1 (Essential)
**Complexity:** HIGH

**Purpose:**
Central hub for all navigation. Players return here after every match.

**Key Features:**
- 3D rotating ship viewer in center
- Navigation buttons to all other screens
- Currency display (Credits, Gems)
- Player info (Username, Level, XP, ELO, Rank)
- Quick Play button (main CTA)
- Notification badge
- Background music

**Dependencies:**
- AccountSystem
- ShipViewer3D
- ProgressionSystem
- SaveSystem

**Implementation Files:**
- `MainMenuController.cs` ✅
- `MainMenuUI.cs` ✅
- `ShipViewer3D.cs` ✅
- `MainMenuScene.unity` ✅

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│ [Profile] [Notifications]              [Settings]  │
│                                                      │
│           ┌───────────────────┐                     │
│ [Ships]   │                   │   [Battle Pass]    │
│           │   3D SHIP         │                     │
│ [Quests]  │   VIEWER          │   [Inventory]      │
│           │   (Rotating)      │                     │
│ [Social]  │                   │   [Leaderboard]    │
│           └───────────────────┘                     │
│                                                      │
│               [QUICK PLAY]                          │
│                                                      │
│  💰 Credits: 1,250  |  💎 Gems: 45  |  ⭐ Lvl: 12  │
└─────────────────────────────────────────────────────┘
```

---

#### **2. Ship Building / Garage Screen** 🔨
**Status:** PENDING
**Priority:** 1 (Essential)
**Complexity:** HIGH

**Purpose:**
Customize ships with unlocked bodies, weapons, perks, and cosmetics.

**Key Features:**
- 3D ship preview with real-time customization
- Ship body selection (show unlocked + locked ships)
- Weapon loadout customization (2-4 weapon slots)
- Perk selection (passive abilities)
- Ship cosmetics (paint jobs, decals, trails)
- Loadout presets (save 3-5 configurations)
- Stats comparison (before/after)
- "Test Drive" button → Training mode
- "Equip" button → Set as active ship

**Dependencies:**
- Inventory system (unlocked items)
- ShipBodySO database
- WeaponSO database
- PerkSO database
- ShipViewer3D

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              SHIP GARAGE                  │
│                                                      │
│  ┌──────────────┐  ╔═════════════════════════════╗ │
│  │              │  ║ Ship Name: Viper            ║ │
│  │   3D SHIP    │  ║ Health: 1000  Armor: 50     ║ │
│  │   PREVIEW    │  ║ Speed: High   Agility: Med  ║ │
│  │              │  ╚═════════════════════════════╝ │
│  │ [← Rotate →] │                                  │
│  └──────────────┘  LOADOUT SLOTS:                  │
│                    Weapon 1: [Hellfire MK-2    ▼] │
│  SHIP BODIES:      Weapon 2: [EMP Disruptor    ▼] │
│  [Viper] [Phoenix] Perk 1:   [Shield Regen     ▼] │
│  [Titan] [🔒 Nova] Perk 2:   [Speed Boost      ▼] │
│                                                      │
│  COSMETICS:        [Test Drive]  [Equip & Close]   │
│  Paint: [Red ▼]                                     │
│  Trail: [Flame ▼]                                   │
└─────────────────────────────────────────────────────┘
```

**Implementation Notes:**
- Integrate with existing ship loading system
- Use same ShipViewer3D as main menu
- Real-time stat calculations when swapping loadout
- Save loadout to PlayerAccountData

---

#### **3. Inventory Screen** 📦
**Status:** PENDING
**Priority:** 1 (Essential)
**Complexity:** MEDIUM

**Purpose:**
View all unlocked items (ships, weapons, perks, cosmetics).

**Key Features:**
- Tabs for different item categories:
  - Ship Bodies
  - Weapons
  - Perks
  - Cosmetics (paint, decals, trails)
  - Consumables (boosters, XP boosts)
- Item cards showing:
  - Icon/thumbnail
  - Name & rarity
  - Stats
  - "Equipped" badge (if active)
  - "New!" badge (recently unlocked)
- Filter options:
  - By rarity (Common, Rare, Epic, Legendary)
  - By type
  - Owned / Locked
- Search bar
- Grid view with scrolling
- Click item → Show detailed stats popup

**Dependencies:**
- PlayerAccountData (owned items list)
- Item database (ShipBodySO, WeaponSO, PerkSO, CosmeticSO)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              INVENTORY                    │
│                                                      │
│  [Ships] [Weapons] [Perks] [Cosmetics] [Items]     │
│  ────────                                           │
│  Filter: [All ▼]  Rarity: [All ▼]  🔍 [Search...]  │
│                                                      │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐        │
│  │    │ │    │ │    │ │    │ │    │ │    │        │
│  │ S1 │ │ S2 │ │ S3 │ │ S4 │ │🔒 │ │🔒 │        │
│  │ ⭐ │ │ ⭐⭐│ │⭐⭐⭐│ │ ⭐ │ │    │ │    │        │
│  └────┘ └────┘ └────┘ └────┘ └────┘ └────┘        │
│  Viper  Phoenix Titan  Wraith  Nova   Apex         │
│  [✓]    [ ]     [ ]    [✓]    [🔒]  [🔒]          │
│                                                      │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐        │
│  │ ... more items scrolling ...                    │
│  └────┘ └────┘ └────┘ └────┘ └────┘ └────┘        │
│                                                      │
│  Total Items: 47/120    Completion: 39%             │
└─────────────────────────────────────────────────────┘
```

**Implementation Notes:**
- Use ScrollRect with Grid Layout Group
- Lazy loading for large inventories (100+ items)
- Cache item cards for performance

---

#### **4. Settings Screen** ⚙️
**Status:** PENDING
**Priority:** 1 (Essential)
**Complexity:** LOW

**Purpose:**
Configure game settings, audio, graphics, controls, and account.

**Key Features:**
- **Audio Settings:**
  - Master volume slider
  - Music volume slider
  - SFX volume slider
  - Mute toggle
- **Graphics Settings:**
  - Quality preset dropdown (Low, Medium, High, Ultra)
  - Resolution dropdown
  - Fullscreen toggle
  - VSync toggle
  - FPS limit dropdown
- **Controls Settings:**
  - Mouse sensitivity slider
  - Invert Y-axis toggle
  - Key bindings (if keyboard controls)
- **Gameplay Settings:**
  - Colorblind mode
  - Show FPS counter
  - Camera shake intensity
- **Account Settings:**
  - Change username
  - Link social accounts
  - Privacy settings
  - Logout button
- **About:**
  - Version number
  - Credits button
  - Support/Help button

**Dependencies:**
- PlayerPrefs (save settings)
- QualitySettings API
- Screen API

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              SETTINGS                     │
│                                                      │
│  [Audio] [Graphics] [Controls] [Gameplay] [Account] │
│  ──────                                             │
│                                                      │
│  AUDIO                                              │
│  Master Volume:  ████████░░ 80%                     │
│  Music Volume:   ██████░░░░ 60%                     │
│  SFX Volume:     ██████████ 100%                    │
│  [✓] Mute All                                       │
│                                                      │
│  GRAPHICS                                           │
│  Quality Preset: [High ▼]                           │
│  Resolution:     [1920x1080 ▼]                      │
│  [✓] Fullscreen                                     │
│  [✓] VSync Enabled                                  │
│  FPS Limit:      [60 ▼]                             │
│                                                      │
│  CONTROLS                                           │
│  Mouse Sensitivity: ███████░░░ 70%                  │
│  [ ] Invert Y-Axis                                  │
│                                                      │
│  [Apply Settings]  [Restore Defaults]               │
└─────────────────────────────────────────────────────┘
```

---

### **PROGRESSION SCREENS**

---

#### **5. Battle Pass Screen** 🎖️
**Status:** PENDING
**Priority:** 2 (High Value)
**Complexity:** MEDIUM

**Purpose:**
Show seasonal Battle Pass progression with free and premium reward tracks.

**Key Features:**
- Season info (name, time remaining)
- Progress bar (current level / max level)
- XP to next level
- Dual track system:
  - **Free Track:** Rewards for all players
  - **Premium Track:** Exclusive rewards (unlocked with gems)
- Reward tiers displayed horizontally (scrollable)
- Visual indicators:
  - ✅ Claimed rewards
  - 🎁 Available to claim
  - 🔒 Locked (future tiers)
- "Claim All" button for multiple rewards
- "Upgrade to Premium" button (1000 gems)
- Daily/weekly challenges for BP XP

**Dependencies:**
- BattlePassSystem
- QuestSystem (BP challenges)
- RewardSystem
- PlayerAccountData (BP level, premium status)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]            BATTLE PASS                    │
│                                                      │
│  SEASON 1: COSMIC WARFARE         Ends in: 23 days  │
│  ════════════════════════════════ 45%               │
│  Level 12 / 100                   XP: 4,500/10,000  │
│                                                      │
│  FREE TRACK:                                        │
│  [✓] [✓] [✓] [🎁] [○] [○] [○] [○] → [scroll]      │
│   1   2   3   4    5   6   7   8                    │
│  100  50  Ship Gem 200 Perk XP  Ship                │
│   XP  Cr  Skin     Cr       Boost Body              │
│                                                      │
│  PREMIUM TRACK: 🔒                                  │
│  [✓] [✓] [✓] [🎁] [○] [○] [○] [○] → [scroll]      │
│   1   2   3   4    5   6   7   8                    │
│  500  100 Rare Excl. 5  Epic Leg. Excl.             │
│   Cr  Gem Perk Ship Gems Weap. Skin Ship            │
│                                                      │
│  [🔒 Unlock Premium - 1,000 Gems]  [Claim All (3)]  │
│                                                      │
│  CHALLENGES:                                        │
│  Win 5 matches: [3/5] ███░░                         │
│  Deal 10K damage: [✓] █████ [+500 BP XP]            │
└─────────────────────────────────────────────────────┘
```

**Implementation Notes:**
- Use horizontal scroll view for tier display
- Real-time sync with server for premium purchases
- Animate rewards when claimed
- Push notifications when rewards are ready

---

#### **6. Quests Screen** 📜
**Status:** PENDING
**Priority:** 2 (High Value)
**Complexity:** MEDIUM

**Purpose:**
Display daily, weekly, and seasonal quests for earning rewards and Battle Pass XP.

**Key Features:**
- Three quest categories:
  - **Daily Quests:** Reset every 24 hours (3-5 quests)
  - **Weekly Quests:** Reset every 7 days (5-7 quests)
  - **Seasonal Quests:** Season-long challenges
- Quest card showing:
  - Quest name & description
  - Progress bar (e.g., "Win 3 matches: 2/3")
  - Rewards (Credits, Gems, BP XP)
  - Time remaining (countdown timer)
  - Claim button (when complete)
- Quest types:
  - Win X matches
  - Deal X damage
  - Fire X missiles
  - Reach X ELO
  - Complete X matches with specific ship
  - Destroy X enemies
- "Claim All" button
- Refresh countdown timers
- Notifications when quests are complete

**Dependencies:**
- QuestSystem
- ProgressionSystem
- BattlePassSystem (BP XP rewards)
- PlayerAccountData

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              QUESTS                       │
│                                                      │
│  [Daily] [Weekly] [Seasonal]                        │
│  ──────                                             │
│  Refresh in: 8h 42m                                 │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ Win 3 Matches                        [2/3] │   │
│  │ ██████████░░░░░ 67%                         │   │
│  │ Rewards: 💰 50 Credits, ⭐ 25 BP XP          │   │
│  │                              [In Progress]  │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ Deal 5,000 Damage                   [✓]    │   │
│  │ ████████████████ 100%                       │   │
│  │ Rewards: 💰 100 Credits, ⭐ 50 BP XP         │   │
│  │                              [CLAIM] ← Click│   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ Fire 50 Missiles                   [12/50] │   │
│  │ ████░░░░░░░░░░░░ 24%                        │   │
│  │ Rewards: 💎 10 Gems                          │   │
│  │                              [In Progress]  │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  [Claim All (1)]                                    │
└─────────────────────────────────────────────────────┘
```

**Implementation Notes:**
- Quest progress tracking in MatchEndHandler
- Server-side validation for quest completion
- Auto-claim option in settings
- Daily reset at specific time (e.g., 00:00 UTC)

---

#### **7. Account Progress / Leveling Screen** 📊
**Status:** PENDING
**Priority:** 2 (High Value)
**Complexity:** MEDIUM

**Purpose:**
Display account level progression and milestone rewards (separate from Battle Pass).

**Key Features:**
- Account level display (1-100+)
- XP progress bar to next level
- **Milestone Rewards Track:**
  - Shows rewards unlocked at specific levels
  - Level 5: Unlock Ranked mode
  - Level 10: Unlock 2nd ship slot
  - Level 15: Unlock premium currency
  - Level 20: Unlock customization
  - etc.
- Progression statistics:
  - Total matches played
  - Total wins
  - Total XP earned
  - Hours played
  - Favorite ship
- **Prestige System** (if level > 100):
  - Reset to level 1 with special badge
  - Keep all unlocks
  - Gain exclusive cosmetic rewards
- "Boost XP" button (consumable item or premium)

**Dependencies:**
- ProgressionSystem
- PlayerAccountData
- RewardSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]         ACCOUNT PROGRESS                  │
│                                                      │
│  LEVEL 12                          Next Level: 13   │
│  ════════════════════════════════ 45%               │
│  4,500 / 10,000 XP                                  │
│                                                      │
│  MILESTONE REWARDS:                                 │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐        │
│  │ ✓  │ │ ✓  │ │ ✓  │ │ 🎁 │ │ ○  │ │ ○  │        │
│  │ L5 │ │ L10│ │ L12│ │ L15│ │ L20│ │ L25│        │
│  └────┘ └────┘ └────┘ └────┘ └────┘ └────┘        │
│  Ranked Ship   500Cr  Ship   2nd    Perk           │
│  Unlock Slot          Skin   Loadout Slot          │
│                                                      │
│  PROGRESSION STATS:                                 │
│  Total Matches:  127                                │
│  Total Wins:     84 (66% win rate)                  │
│  Total XP Earned: 125,400                           │
│  Hours Played:   42h 15m                            │
│  Favorite Ship:  Viper (87 matches)                 │
│                                                      │
│  [Buy XP Boost - 2x XP for 24h - 100 Gems]          │
└─────────────────────────────────────────────────────┘
```

**Implementation Notes:**
- XP gain from matches, quests, achievements
- Milestone rewards auto-claimed or manual claim
- Track stats in PlayerAccountData
- XP boost tracked with expiration timestamp

---

#### **8. Achievements Screen** 🏆
**Status:** PENDING
**Priority:** 3 (Engagement)
**Complexity:** MEDIUM

**Purpose:**
Display unlockable achievements with rewards.

**Key Features:**
- Achievement categories:
  - Combat (damage, kills, streaks)
  - Wins (total wins, ranked wins)
  - Ships (use all ships, master ships)
  - Weapons (use all weapons, accuracy)
  - Milestones (play X matches, reach level X)
  - Special (hidden achievements, easter eggs)
- Achievement card showing:
  - Icon
  - Name & description
  - Progress bar (e.g., "Win 100 matches: 47/100")
  - Rewards (Credits, Gems, Titles, Cosmetics)
  - Claim button (when unlocked)
  - Rarity (Common, Rare, Epic, Legendary)
- Filter by:
  - Completed / In Progress / Locked
  - Category
  - Rarity
- Total achievement points display
- Showcase slot (pin favorite achievement to profile)

**Dependencies:**
- AchievementSystem
- PlayerAccountData
- RewardSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]           ACHIEVEMENTS                    │
│                                                      │
│  Achievement Points: 1,240 / 5,000                  │
│  Completed: 23/75                                   │
│                                                      │
│  [All] [Combat] [Wins] [Ships] [Special]           │
│  Filter: [In Progress ▼]  Rarity: [All ▼]          │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🏆 FIRST BLOOD              [✓] UNLOCKED   │   │
│  │ Win your first match                        │   │
│  │ Reward: 💰 100 Credits                       │   │
│  │                                   [Claimed] │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🎯 SHARPSHOOTER             [47%] ░░░░░░   │   │
│  │ Hit 1,000 missiles             470/1,000    │   │
│  │ Rewards: 💰 500 Cr, 💎 50 Gems, 🎨 Gold Trail│   │
│  │                             [In Progress]   │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ 👑 UNTOUCHABLE              [🔒] LOCKED     │   │
│  │ Win 10 matches without taking damage        │   │
│  │ Reward: ??? (Hidden)                        │   │
│  │                               [0/10]        │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

### **SOCIAL / COMPETITIVE SCREENS**

---

#### **9. Leaderboards Screen** 🏅
**Status:** PENDING
**Priority:** 3 (Engagement)
**Complexity:** MEDIUM

**Purpose:**
Display competitive rankings (local, friends, regional, global).

**Key Features:**
- Multiple leaderboard tabs:
  - **Global:** Top 100 worldwide
  - **Regional:** Top players in your region
  - **Friends:** Your friends' rankings
  - **Local:** Players near you
- Leaderboard entry showing:
  - Rank (#1, #2, etc.)
  - Username
  - Level
  - ELO rating
  - Rank icon (Bronze, Silver, Gold, etc.)
  - Win rate
  - "You" highlight (your position)
- Time period filter:
  - All Time
  - This Season
  - This Week
- Search player by username
- Click player → View profile
- "Challenge" button (send match invite)
- Pagination (load more on scroll)
- Auto-refresh (update rankings live)

**Dependencies:**
- LeaderboardSystem
- ELORatingSystem
- AccountSystem
- Server API (fetch rankings)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]           LEADERBOARDS                    │
│                                                      │
│  [Global] [Regional] [Friends] [Local]              │
│  ────────                                           │
│  Season 1    [This Season ▼]    🔍 [Search...]      │
│                                                      │
│  Rank  Player           Level  ELO    W/L   Rate    │
│  ───────────────────────────────────────────────────│
│  🥇 #1  Ace_Pilot_99      48   2450  520/80  87%   │
│  🥈 #2  StarDestroyer     45   2398  498/102 83%   │
│  🥉 #3  CosmicWarrior     43   2345  475/125 79%   │
│     #4  GravityKing       42   2290  461/139 77%   │
│     #5  NovaBlast         41   2245  447/153 74%   │
│     ...                                              │
│  ╔═ #47 YOU - Ace_Pilot_42  12  1450  84/43  66% ═╗│
│  ║     [View My Profile]          [Challenge]     ║│
│  ╚══════════════════════════════════════════════════╝│
│     #48  NextPlayer        11   1442  82/44  65%   │
│     #49  AnotherPlayer     12   1438  81/45  64%   │
│     ...                                              │
│                                                      │
│  [Load More ↓]                                      │
└─────────────────────────────────────────────────────┘
```

**Implementation Notes:**
- Cache leaderboard data (refresh every 5 minutes)
- Fetch player's position separately (efficient query)
- Highlight player's row with different color
- Use virtual scrolling for large lists (1000+ entries)

---

#### **10. Profile Screen** 👤
**Status:** PENDING
**Priority:** 3 (Engagement)
**Complexity:** MEDIUM

**Purpose:**
Display detailed player profile (yours or other players').

**Key Features:**
- **Player Info:**
  - Username (editable if yours)
  - Player ID
  - Level & XP progress
  - ELO rating & rank
  - Join date
  - Last online (if friend)
- **Statistics:**
  - Total matches, wins, losses
  - Win rate %
  - Average damage per match
  - Missiles fired / hit (accuracy %)
  - Favorite ship (most played)
  - Total playtime
- **Showcased Achievements:**
  - 3-5 pinned achievements
  - Title/banner (if unlocked)
- **Match History:**
  - Recent 10 matches (win/loss)
  - Date, opponent, result, ELO change
- **Equipped Ship Display:**
  - 3D preview of current ship
- **Social Actions** (if viewing other player):
  - Add Friend
  - Challenge to Match
  - Block/Report
- **Edit Profile** (if yours):
  - Change username
  - Select title/banner
  - Pin achievements
  - Privacy settings

**Dependencies:**
- PlayerAccountData
- MatchHistorySystem
- AchievementSystem
- FriendSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              PROFILE                      │
│                                                      │
│  ┌──────────┐  Ace_Pilot_42         #GW-1234567    │
│  │          │  Level 12  |  Gold III  |  1450 ELO  │
│  │  SHIP    │  Joined: Jan 2025  |  Last: Online   │
│  │  3D      │                                       │
│  │  PREVIEW │  [Edit Profile] [Change Username]    │
│  └──────────┘                                       │
│                                                      │
│  STATISTICS:                                        │
│  Total Matches: 127    Wins: 84    Losses: 43      │
│  Win Rate: 66%         Playtime: 42h 15m            │
│  Missiles Fired: 1,247   Accuracy: 62%              │
│  Avg Damage/Match: 4,250                            │
│  Favorite Ship: Viper (87 matches)                  │
│                                                      │
│  SHOWCASED ACHIEVEMENTS:                            │
│  [🏆 First Blood] [🎯 Sharpshooter] [⚡ Speed Demon]│
│                                                      │
│  RECENT MATCHES:                                    │
│  ┌────────────────────────────────────────────┐    │
│  │ 2025-01-20  vs GravityKing   WIN  +15 ELO │    │
│  │ 2025-01-20  vs NovaBlast     LOSS -12 ELO │    │
│  │ 2025-01-19  vs StarDestroyer WIN  +18 ELO │    │
│  └────────────────────────────────────────────┘    │
│  [View Full Match History]                          │
└─────────────────────────────────────────────────────┘
```

---

#### **11. Friends / Social Screen** 👥
**Status:** PENDING
**Priority:** 4 (Social Features)
**Complexity:** MEDIUM

**Purpose:**
Manage friends list, send invites, view friend activity.

**Key Features:**
- **Friends List:**
  - Friend's username, level, rank
  - Online status (Online, Offline, In Match)
  - "Invite to Match" button (if online)
  - "View Profile" button
  - "Remove Friend" button
- **Friend Requests:**
  - Pending requests (incoming/outgoing)
  - Accept/Decline buttons
- **Add Friend:**
  - Search by username or Player ID
  - Send friend request
- **Recent Players:**
  - List of players from recent matches
  - "Add Friend" button
- **Friend Activity Feed:**
  - "X reached Gold rank!"
  - "Y unlocked new ship!"
  - "Z is on a 5-win streak!"
- **Clan/Team Integration** (future):
  - Create/join clans
  - Clan chat
  - Clan wars

**Dependencies:**
- FriendSystem
- AccountSystem
- Server API (friend requests, online status)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              FRIENDS                      │
│                                                      │
│  [Friends (12)] [Requests (2)] [Recent Players]     │
│  ─────────────                                      │
│  🔍 [Add Friend - Enter Username or ID...]          │
│                                                      │
│  ONLINE (4):                                        │
│  ┌────────────────────────────────────────────┐    │
│  │ 🟢 StarDestroyer      Lvl 45  Gold III     │    │
│  │    [Invite to Match] [View Profile]        │    │
│  └────────────────────────────────────────────┘    │
│  ┌────────────────────────────────────────────┐    │
│  │ 🟢 CosmicWarrior      Lvl 43  Silver I      │    │
│  │    [Invite to Match] [View Profile]        │    │
│  └────────────────────────────────────────────┘    │
│                                                      │
│  OFFLINE (8):                                       │
│  ┌────────────────────────────────────────────┐    │
│  │ ⚫ GravityKing        Lvl 42  Gold II       │    │
│  │    Last online: 2h ago  [View Profile]     │    │
│  └────────────────────────────────────────────┘    │
│  ┌────────────────────────────────────────────┐    │
│  │ ⚫ NovaBlast          Lvl 41  Silver III    │    │
│  │    Last online: 1 day ago  [View Profile]  │    │
│  └────────────────────────────────────────────┘    │
│  ...                                                │
└─────────────────────────────────────────────────────┘
```

---

#### **12. Clan / Team Screen** 🛡️
**Status:** PENDING
**Priority:** 5 (Advanced Social)
**Complexity:** HIGH

**Purpose:**
Create/join clans for team play and clan wars.

**Key Features:**
- **Clan Info:**
  - Clan name, tag, emblem
  - Member count (e.g., 25/50)
  - Clan level & XP
  - Clan rank (leaderboard position)
  - Required ELO to join
- **Members List:**
  - Role badges (Leader, Officer, Member)
  - Online status
  - Contribution stats
- **Clan Chat:**
  - Text chat for clan members
  - Announcements from leaders
- **Clan Wars:**
  - Weekly clan vs clan battles
  - Clan war leaderboard
  - Rewards for participation
- **Clan Perks:**
  - Bonus XP/Credits for clan members
  - Exclusive clan shop
- **Create Clan:**
  - Cost: 1,000 gems
  - Customize name, tag, emblem
- **Clan Requests:**
  - View/approve join requests (leaders only)

**Dependencies:**
- ClanSystem (complex server-side)
- AccountSystem
- MatchmakingSystem (clan wars)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              CLAN                         │
│                                                      │
│  ┌────┐  GRAVITY WARRIORS [GW]      Rank: #47      │
│  │    │  Level 8  |  Members: 25/50                 │
│  │ 🛡 │  Clan Leader: Ace_Pilot_99                  │
│  │    │  Required ELO: 1,000+                       │
│  └────┘  [Leave Clan]  [Clan Settings]              │
│                                                      │
│  [Members (25)] [Chat] [Clan Wars] [Perks]          │
│  ─────────────                                      │
│                                                      │
│  ONLINE MEMBERS (8):                                │
│  👑 Ace_Pilot_99 (Leader)      Lvl 48  2450 ELO    │
│  ⚜️  StarDestroyer (Officer)    Lvl 45  2398 ELO    │
│  👤 CosmicWarrior              Lvl 43  2345 ELO    │
│  ...                                                │
│                                                      │
│  OFFLINE MEMBERS (17):                              │
│  👤 GravityKing                Lvl 42  2290 ELO    │
│  ...                                                │
│                                                      │
│  CLAN STATS:                                        │
│  Total Clan Wars Won: 12                            │
│  Clan XP This Week: 45,200                          │
│  Top Contributor: StarDestroyer (8,900 XP)          │
└─────────────────────────────────────────────────────┘
```

---

### **MONETIZATION SCREENS**

---

#### **13. Shop / Store Screen** 🛒
**Status:** PENDING
**Priority:** 4 (Monetization)
**Complexity:** MEDIUM

**Purpose:**
In-game store for buying items with Credits or Gems.

**Key Features:**
- **Store Tabs:**
  - Featured (daily rotating deals)
  - Ships
  - Weapons
  - Perks
  - Cosmetics
  - Bundles
- **Item Card:**
  - Icon/preview
  - Name & rarity
  - Price (Credits or Gems)
  - "Owned" badge (if already purchased)
  - "Sale!" badge (discounted)
  - "New!" badge
  - "Buy" button
- **Featured Deals:**
  - Daily rotating shop (resets every 24h)
  - Limited-time offers
  - Countdown timer
- **Bundles:**
  - Ship + Weapons + Cosmetics bundles
  - Starter pack (one-time purchase)
  - Season pass bundle
- **Currency Purchase:**
  - Button to buy Gems with real money
  - Links to Premium Currency Store

**Dependencies:**
- PlayerAccountData (currency, owned items)
- ShopSystem
- TransactionSystem
- Server API (microtransactions)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]              SHOP                         │
│                                                      │
│  💰 Credits: 1,250  |  💎 Gems: 45  [+ Buy Gems]    │
│                                                      │
│  [Featured] [Ships] [Weapons] [Cosmetics] [Bundles] │
│  ──────────                                         │
│  Daily Shop - Resets in: 8h 42m                     │
│                                                      │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐      │
│  │            │ │            │ │            │      │
│  │  VIPER     │ │ HELLFIRE   │ │  RED FLAME │      │
│  │  SHIP      │ │ WEAPON     │ │  TRAIL     │      │
│  │            │ │            │ │            │      │
│  │ ⭐⭐⭐     │ │ ⭐⭐       │ │ ⭐         │      │
│  │            │ │            │ │            │      │
│  │ 💰 5,000   │ │ 💎 150     │ │ 💰 500     │      │
│  │ [BUY]      │ │ [BUY]      │ │ [OWNED]    │      │
│  └────────────┘ └────────────┘ └────────────┘      │
│                                                      │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐      │
│  │  STARTER   │ │  SEASON 1  │ │  MEGA      │      │
│  │  BUNDLE    │ │  PASS      │ │  BUNDLE    │      │
│  │            │ │            │ │            │      │
│  │ 🔥 -50%    │ │  NEW!      │ │  SALE!     │      │
│  │ 💎 500     │ │ 💎 1,000   │ │ 💎 2,500   │      │
│  │ [BUY]      │ │ [BUY]      │ │ [BUY]      │      │
│  └────────────┘ └────────────┘ └────────────┘      │
└─────────────────────────────────────────────────────┘
```

---

#### **14. Premium Currency Store** 💎
**Status:** PENDING
**Priority:** 4 (Monetization)
**Complexity:** LOW

**Purpose:**
Purchase Gems with real money (microtransactions).

**Key Features:**
- Gem packs with increasing value:
  - 100 Gems - $0.99
  - 500 Gems (+50 bonus) - $4.99
  - 1,200 Gems (+200 bonus) - $9.99
  - 2,500 Gems (+500 bonus) - $19.99
  - 6,500 Gems (+1,500 bonus) - $49.99
  - 14,000 Gems (+4,000 bonus) - $99.99 (Best Value!)
- "Best Value" badge on largest pack
- "First Purchase Bonus" (double gems on first buy)
- Payment methods:
  - Credit card
  - PayPal
  - Google Play / App Store
- Transaction history
- Secure payment disclaimer
- Restore purchases button (for mobile)

**Dependencies:**
- UnityIAP (In-App Purchases)
- Server API (validate purchases)
- TransactionSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]          BUY GEMS                         │
│                                                      │
│  Current Balance: 💎 45 Gems                        │
│                                                      │
│  ┌──────────────┐ ┌──────────────┐                  │
│  │  100 GEMS    │ │  500 GEMS    │                  │
│  │              │ │  + 50 BONUS  │                  │
│  │              │ │              │                  │
│  │   $0.99      │ │   $4.99      │                  │
│  │   [BUY]      │ │   [BUY]      │                  │
│  └──────────────┘ └──────────────┘                  │
│                                                      │
│  ┌──────────────┐ ┌──────────────┐                  │
│  │  1,200 GEMS  │ │  2,500 GEMS  │                  │
│  │  + 200 BONUS │ │  + 500 BONUS │                  │
│  │              │ │              │                  │
│  │   $9.99      │ │  $19.99      │                  │
│  │   [BUY]      │ │   [BUY]      │                  │
│  └──────────────┘ └──────────────┘                  │
│                                                      │
│  ┌──────────────┐ ┌──────────────┐                  │
│  │  6,500 GEMS  │ │ 14,000 GEMS  │                  │
│  │+1,500 BONUS  │ │+4,000 BONUS  │                  │
│  │              │ │  🌟 BEST!    │                  │
│  │  $49.99      │ │  $99.99      │                  │
│  │   [BUY]      │ │   [BUY]      │                  │
│  └──────────────┘ └──────────────┘                  │
│                                                      │
│  🔒 Secure Payment  |  [Restore Purchases]          │
└─────────────────────────────────────────────────────┘
```

---

#### **15. Offers / Deals Screen** 🎁
**Status:** PENDING
**Priority:** 5 (Monetization)
**Complexity:** LOW

**Purpose:**
Special limited-time offers and promotions.

**Key Features:**
- **Offer Types:**
  - Welcome offer (for new players)
  - Daily login bonus
  - Weekend sale
  - Season launch offer
  - Level-up rewards
  - Returning player bonus
- **Offer Card:**
  - Title & description
  - Discount % (e.g., "70% OFF!")
  - Original price (strikethrough)
  - Sale price
  - Countdown timer (expires in X)
  - "Claim" or "Buy" button
- **Free Offers:**
  - Daily login reward (Credits, XP boost)
  - Watch ad for Gems
- **Push Notifications:**
  - Alert when new offers available

**Dependencies:**
- OfferSystem
- Server API (fetch active offers)
- TransactionSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]         SPECIAL OFFERS                    │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🎁 DAILY LOGIN REWARD               DAY 3   │   │
│  │ Claim your daily bonus!                     │   │
│  │ Rewards: 💰 500 Credits, ⭐ 100 XP           │   │
│  │                              [CLAIM] ← FREE │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ 🔥 WEEKEND SALE - 70% OFF!  Ends in: 23h   │   │
│  │ Epic Ship Bundle                            │   │
│  │ ̶$̶2̶9̶.̶9̶9̶  → $8.99                            │   │
│  │ Includes: Phoenix Ship, 3 Weapons, Cosmetics│   │
│  │                                      [BUY]  │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ ⭐ LEVEL UP REWARD!             LEVEL 12    │   │
│  │ Congratulations on reaching Level 12!       │   │
│  │ Rewards: 💎 25 Gems, 💰 1,000 Credits        │   │
│  │                              [CLAIM] ← FREE │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ 📺 WATCH AD FOR GEMS                        │   │
│  │ Watch a short ad to earn 10 gems!           │   │
│  │ Available: 3/3 today                        │   │
│  │                          [WATCH AD] ← FREE  │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

### **GAME MODE SCREENS**

---

#### **16. Matchmaking Lobby Screen** 🎮
**Status:** PENDING
**Priority:** 2 (Core Gameplay)
**Complexity:** MEDIUM

**Purpose:**
Queue for online matches (Ranked or Casual).

**Key Features:**
- **Game Mode Selection:**
  - Ranked (ELO-based matchmaking)
  - Casual (unranked matchmaking)
  - Private Match (custom lobbies)
- **Ship & Loadout Selection:**
  - Show equipped ship
  - "Change Ship" button → Ship Garage
- **Matchmaking Queue:**
  - "Find Match" button
  - Estimated wait time
  - Cancel button
  - Queue animation (searching for opponent)
- **Match Found:**
  - Opponent preview (username, level, rank)
  - Map preview
  - "Accept" button (30s timer)
  - Decline penalty warning (for ranked)
- **Private Match Lobby:**
  - Room code display
  - Invite friend button
  - Ready/Not Ready status
  - Kick player (host only)
  - Start match (host only)

**Dependencies:**
- MatchmakingSystem
- LobbySystem
- Server API (Unity Lobby/Relay)

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]          MATCHMAKING                      │
│                                                      │
│  [Ranked] [Casual] [Private Match]                  │
│  ────────                                           │
│                                                      │
│  YOUR SHIP:                                         │
│  ┌────────────┐  Viper                              │
│  │            │  Health: 1000  |  Speed: High       │
│  │  3D SHIP   │  Weapons: Hellfire MK-2, EMP        │
│  │  PREVIEW   │  Perks: Shield Regen, Speed Boost   │
│  │            │                                      │
│  └────────────┘  [Change Ship & Loadout]            │
│                                                      │
│  ESTIMATED WAIT: ~30 seconds                        │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │                                             │   │
│  │     🔍 SEARCHING FOR OPPONENT...            │   │
│  │                                             │   │
│  │         ⚪⚪⚪ (animated)                     │   │
│  │                                             │   │
│  │              [CANCEL QUEUE]                 │   │
│  │                                             │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  RANKED STATS:                                      │
│  Current ELO: 1,450  |  Rank: Gold III              │
│  Wins Today: 5  |  Losses: 2                        │
└─────────────────────────────────────────────────────┘
```

**Match Found State:**
```
┌─────────────────────────────────────────────────────┐
│  MATCH FOUND!                        Accept in: 28s │
│                                                      │
│  OPPONENT:                                          │
│  ┌────────────┐  GravityKing                        │
│  │            │  Level 42  |  Gold II  |  2290 ELO  │
│  │  OPPONENT  │  Win Rate: 77%                      │
│  │  SHIP      │                                      │
│  │  PREVIEW   │  Map: Asteroid Belt                 │
│  └────────────┘                                      │
│                                                      │
│              [ACCEPT]  [DECLINE]                    │
│                                                      │
│  Note: Declining ranked matches may result in       │
│  queue cooldown penalties.                          │
└─────────────────────────────────────────────────────┘
```

---

#### **17. Training / Practice Screen** 🎯
**Status:** PENDING
**Priority:** 3 (Player Retention)
**Complexity:** LOW

**Purpose:**
Practice mode for learning mechanics without ELO risk.

**Key Features:**
- **Training Modes:**
  - Tutorial (guided lessons)
  - Free Practice (vs AI or solo)
  - Target Practice (missile accuracy)
  - Advanced Mechanics (gravity slingshot, etc.)
- **AI Difficulty:**
  - Easy, Medium, Hard, Expert
- **Practice Stats:**
  - Missiles fired
  - Accuracy %
  - Best slingshot speed
  - Damage dealt
- **Rewards:**
  - First-time completion bonuses
  - Daily practice XP bonus
- **Skip Tutorial** (for returning players)

**Dependencies:**
- TrainingSystem
- AI opponent
- TutorialSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]             TRAINING                      │
│                                                      │
│  [Tutorial] [Free Practice] [Target Practice]       │
│  ──────────                                         │
│                                                      │
│  TUTORIAL MISSIONS:                                 │
│  ┌────────────────────────────────────────────┐    │
│  │ ✓ 1. Basic Controls         [Completed]   │    │
│  │   Learn movement and aiming                │    │
│  │   Reward: 💰 100 Credits (Claimed)          │    │
│  └────────────────────────────────────────────┘    │
│                                                      │
│  ┌────────────────────────────────────────────┐    │
│  │ ✓ 2. Firing Missiles        [Completed]   │    │
│  │   Practice missile launching               │    │
│  │   Reward: 💰 100 Credits (Claimed)          │    │
│  └────────────────────────────────────────────┘    │
│                                                      │
│  ┌────────────────────────────────────────────┐    │
│  │ ○ 3. Gravity Mechanics      [START]        │    │
│  │   Master gravity slingshots                │    │
│  │   Reward: 💰 150 Credits, ⭐ 50 XP          │    │
│  └────────────────────────────────────────────┘    │
│                                                      │
│  ┌────────────────────────────────────────────┐    │
│  │ 🔒 4. Advanced Tactics      [Locked]       │    │
│  │   Complete Mission 3 to unlock             │    │
│  │   Reward: 💎 25 Gems, ⭐ 100 XP             │    │
│  └────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

---

### **MISCELLANEOUS SCREENS**

---

#### **18. News / Events Screen** 📰
**Status:** PENDING
**Priority:** 5 (Engagement)
**Complexity:** LOW

**Purpose:**
Display game news, patch notes, events, and announcements.

**Key Features:**
- **News Feed:**
  - Latest patch notes
  - New features
  - Balance changes
  - Server maintenance notices
  - Community events
- **Event Calendar:**
  - Upcoming events (double XP weekend, etc.)
  - Season start/end dates
  - Special tournaments
- **News Card:**
  - Title & thumbnail image
  - Date published
  - Short description
  - "Read More" button → Full article
- **Push Notifications:**
  - Alert for important news

**Dependencies:**
- Server API (fetch news)
- NewsSystem

**Layout:**
```
┌─────────────────────────────────────────────────────┐
│  [← Back]          NEWS & EVENTS                    │
│                                                      │
│  [Latest News] [Patch Notes] [Events] [Community]   │
│  ──────────────                                     │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ [Image]  SEASON 2 LAUNCHES FEBRUARY 1ST!   │   │
│  │          Posted: Jan 22, 2025               │   │
│  │ Get ready for new ships, maps, and rewards! │   │
│  │                          [Read More →]      │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ [Image]  DOUBLE XP WEEKEND - THIS SATURDAY! │   │
│  │          Posted: Jan 20, 2025               │   │
│  │ Earn 2x XP on all matches this weekend!     │   │
│  │                          [Read More →]      │   │
│  └─────────────────────────────────────────────┘   │
│                                                      │
│  ┌─────────────────────────────────────────────┐   │
│  │ [Image]  PATCH 1.2.0 - BALANCE CHANGES      │   │
│  │          Posted: Jan 15, 2025               │   │
│  │ Hellfire missile damage reduced, EMP...     │   │
│  │                          [Read More →]      │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

## 🗂️ Implementation Priority Order

### **Phase 1: Core Hub (Week 1-2)**
Must-have screens to make the game functional.

1. **Main Menu Hub** ✅ (Already done)
2. **Ship Building/Garage** (Customize ships before matches)
3. **Inventory** (View unlocked items)
4. **Settings** (Configure game options)

### **Phase 2: Progression (Week 3-4)**
Add progression systems to keep players engaged.

5. **Battle Pass** (Seasonal rewards)
6. **Quests** (Daily/weekly challenges)
7. **Account Progress** (Level-up rewards)
8. **Achievements** (Long-term goals)

### **Phase 3: Social/Competitive (Week 5-6)**
Build community and competition.

9. **Leaderboards** (Rankings)
10. **Profile** (Player stats)
11. **Matchmaking Lobby** (Queue for matches)
12. **Training** (Practice mode)

### **Phase 4: Monetization (Week 7)**
Add revenue streams.

13. **Shop/Store** (Buy items)
14. **Premium Currency Store** (IAP)
15. **Offers/Deals** (Special promotions)

### **Phase 5: Social Features (Week 8+)**
Advanced social systems.

16. **Friends/Social** (Friend list)
17. **Clan/Team** (Group play)

### **Phase 6: Polish (Week 9+)**
Nice-to-have extras.

18. **News/Events** (Announcements)

---

## 📊 Screen Dependencies Matrix

| Screen | Depends On |
|--------|------------|
| Main Menu Hub | AccountSystem, ShipViewer3D, ProgressionSystem |
| Ship Garage | Inventory, ShipBodySO, WeaponSO, PerkSO |
| Inventory | PlayerAccountData, Item databases |
| Settings | PlayerPrefs, QualitySettings API |
| Battle Pass | BattlePassSystem, QuestSystem, RewardSystem |
| Quests | QuestSystem, ProgressionSystem |
| Account Progress | ProgressionSystem, RewardSystem |
| Achievements | AchievementSystem, RewardSystem |
| Leaderboards | LeaderboardSystem, ELORatingSystem, Server API |
| Profile | PlayerAccountData, MatchHistorySystem |
| Matchmaking | MatchmakingSystem, LobbySystem, Server API |
| Training | TrainingSystem, AI |
| Shop | ShopSystem, TransactionSystem, Server API |
| Premium Store | UnityIAP, TransactionSystem, Server API |
| Friends | FriendSystem, Server API |
| Clan | ClanSystem (complex), Server API |
| News | NewsSystem, Server API |

---

## ✅ Implementation Checklist

Use this checklist to track your progress:

### Core Hub Screens
- [x] Main Menu Hub
- [ ] Ship Building/Garage
- [ ] Inventory
- [ ] Settings

### Progression Screens
- [ ] Battle Pass
- [ ] Quests
- [ ] Account Progress
- [ ] Achievements

### Social/Competitive Screens
- [ ] Leaderboards
- [ ] Profile
- [ ] Friends/Social
- [ ] Clan/Team

### Monetization Screens
- [ ] Shop/Store
- [ ] Premium Currency Store
- [ ] Offers/Deals

### Game Mode Screens
- [ ] Matchmaking Lobby
- [ ] Training/Practice

### Miscellaneous Screens
- [ ] News/Events

---

## 🎨 UI Design Resources

**Fonts:**
- Orbitron (sci-fi)
- Rajdhani (modern)
- Exo 2 (futuristic)

**Color Palette:**
- Primary: `#3498DB` (Blue)
- Secondary: `#9B59B6` (Purple)
- Success: `#2ECC71` (Green)
- Warning: `#F39C12` (Orange)
- Danger: `#E74C3C` (Red)
- Dark: `#2C3E50` (Navy)
- Light: `#ECF0F1` (White-Gray)

**Icons:**
- FontAwesome
- Unity Asset Store: "Game Icon Pack"

**3D Models:**
- Unity Asset Store: "Sci-Fi Spaceships"
- Sketchfab (free models)

---

## 📝 Next Steps

1. **Review this catalog** - Make sure you agree with all screens
2. **Prioritize** - Decide which screens to build first
3. **Start implementing Phase 1** - Ship Garage, Inventory, Settings
4. **Create UI mockups** - Sketch designs before coding
5. **Build reusable components** - Button styles, panels, transitions
6. **Test on target resolution** - 1920x1080 (PC) or 16:9 (mobile)

---

**Ready to start building? Let's begin with the highest priority screen!** 🚀
