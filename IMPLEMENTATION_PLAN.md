# Gravity Wars - Complete Implementation Plan
**Focus: Online Multiplayer as Core Feature**

---

## 📋 Current State

### ✅ What's Already Working
- **Hotseat Local Multiplayer** - 2-player local PVP works perfectly
- **Unified Data System** - Single PlayerAccountData structure for all save data
- **Cloud Save Infrastructure** - Code ready, needs Unity Gaming Services setup
- **Progression System** - ProgressionManager handles XP, levels, currency, unlocks
- **Save System** - Local JSON saves working, cloud sync ready
- **Lobby System** - LobbyManager code ready, needs Unity Netcode package
- **Matchmaking System** - MatchmakingService ready for online play
- **ELO Rating System** - Competitive ranking system implemented
- **Analytics System** - AnalyticsService ready for tracking

### 🔧 What Needs Implementation
1. **Unity Gaming Services Setup** (REQUIRED for online multiplayer)
2. **Unity Netcode Integration** (REQUIRED for online multiplayer)
3. **Online Multiplayer Testing** (matchmaking, lobbies, netcode synchronization)
4. **UI Screens** (Main Menu, Matchmaking, Ship Selection, etc.)
5. **Content Creation** (Ships, Perks, Passives, Quests, Achievements)
6. **Visual Polish** (Ship 3D models, VFX, UI styling)
7. **Testing & Balancing**

---

## 🎯 Implementation Phases (Online-First Approach)

### **Phase 1: Unity Gaming Services + Netcode Setup** (2-3 hours)
**Goal:** Get online multiplayer infrastructure running

**CRITICAL - DO THIS FIRST:**

**Tasks:**
1. ✅ Install Unity Gaming Services packages (Authentication, Cloud Save, Lobby, Relay)
2. ✅ **Install Unity Netcode for GameObjects** (REQUIRED)
3. ✅ Configure Unity Cloud Save
4. ✅ Configure Unity Lobby Service
5. ✅ Configure Unity Relay Service (for NAT punchthrough)
6. ✅ Set up scripting define symbols
7. ✅ Test authentication & cloud save
8. ✅ Test basic netcode connection

**Critical Packages (ALL REQUIRED):**
- `com.unity.services.core`
- `com.unity.services.authentication`
- `com.unity.services.cloudsave`
- `com.unity.services.lobby`
- `com.unity.services.relay`
- `com.unity.netcode.gameobjects`
- `com.unity.transport`

**See:** `UNITY_SETUP_GUIDE.md` (Section: Online Multiplayer Setup)

**Success Criteria:**
- [ ] All packages installed
- [ ] Unity Gaming Services authenticated
- [ ] Test lobby created successfully
- [ ] Two clients can connect via Relay
- [ ] Network variables sync between clients

---

### **Phase 2: Online Match Flow** (6-8 hours)
**Goal:** Complete end-to-end online match experience

**Tasks:**

#### **A. Matchmaking UI** (2 hours)
1. Create Online Matchmaking screen:
   - Quick Match button (finds opponent automatically)
   - Create Private Lobby button
   - Join by Code input field
   - Region selection dropdown
   - Ranked/Casual toggle

2. Create Lobby UI:
   - Show lobby code
   - Player list (host + opponent)
   - Ready/Not Ready indicators
   - Ship selection preview
   - Chat (optional)
   - Start Match button (host only)

3. Create Searching UI:
   - Animated "Searching..." indicator
   - Cancel button
   - Estimated wait time
   - Player count in queue

#### **B. Network Synchronization** (3-4 hours)
1. Convert PlayerShip to NetworkBehaviour:
   - Position/rotation synchronization
   - Velocity synchronization
   - Health synchronization
   - Missile firing synchronization

2. Convert Missile to NetworkBehaviour:
   - Trajectory synchronization
   - Hit detection (server-authoritative)
   - Destroy synchronization

3. Implement NetworkGameManager:
   - Match state synchronization (countdown, playing, ended)
   - Score synchronization
   - Round transitions
   - Victory/defeat conditions

4. Add lag compensation:
   - Client-side prediction
   - Server reconciliation
   - Interpolation for smooth movement

#### **C. Matchmaking Integration** (1-2 hours)
1. Connect UI to LobbyManager
2. Implement Quick Match flow:
   - Search for available lobbies
   - Auto-join if found
   - Create new lobby if none available
   - Start match when 2 players ready

3. Implement ELO matchmaking:
   - Match players within ±200 ELO range
   - Expand range after 30 seconds
   - Record match results
   - Update ELO ratings post-match

**See:** `ONLINE_MULTIPLAYER_GUIDE.md`

**Success Criteria:**
- [ ] Can find opponent via Quick Match
- [ ] Can create private lobby and share code
- [ ] Two players see each other's ships move in real-time
- [ ] Missiles hit and deal damage correctly
- [ ] Match ends properly, winner declared
- [ ] ELO ratings update after match

---

### **Phase 3: Core UI Screens** (4-6 hours)
**Goal:** Polish main menu and core game UI

**Screens to Create:**

#### **1. Main Menu Screen** (1 hour)
- Player profile display (username, level, XP bar, ELO rating)
- Currency display (credits, gems)
- **PLAY Button** (most prominent - goes to Online Matchmaking)
- Ships button
- Battle Pass button
- Achievements button
- Settings button
- Friends/Social button (optional)

#### **2. Ship Selection Screen** (2 hours)
- 3D ship viewer (rotate, zoom)
- Ship stats panel (health, speed, damage, special abilities)
- Loadout editor:
  - Select ship body
  - Select 3 perks (tier 1, 2, 3)
  - Select passive
  - Select move type
  - Select missile
- Unlock requirements display
- "Equip for Ranked" button
- "Equip for Casual" button

#### **3. Post-Match Results Screen** (1 hour)
- Winner/Loser banner
- Match statistics:
  - Damage dealt
  - Missiles fired/hit
  - Accuracy percentage
  - Kill/death ratio
- XP gained
- Currency gained
- ELO change (+15, -10, etc.)
- Quest progress updates
- Achievement unlocks
- "Play Again" button
- "Return to Menu" button

#### **4. Battle Pass Screen** (1 hour)
- Tier progress bar (1-30)
- Reward grid:
  - Free track rewards
  - Premium track rewards
  - "Locked" overlay for premium
- XP display (current/needed for next tier)
- "Purchase Premium" button (1000 gems)
- "Claim Tier" buttons

#### **5. Achievements Screen** (1 hour)
- Achievement grid
- Filter by category (Combat, Progression, Social, etc.)
- Search bar
- Progress bars for incomplete achievements
- "Claim Reward" buttons

#### **6. Settings Screen** (30 min)
- Audio: Master, Music, SFX sliders
- Graphics: Quality dropdown, VSync toggle
- Controls: Sensitivity slider
- Account: Username, Change Password, Log Out

**See:** `UI_CREATION_GUIDE.md`

---

### **Phase 4: Content Creation** (8-12 hours)
**Goal:** Create all game content

**Priority Order (for online play):**

#### **A. Ships** (3-4 hours) - **START HERE**
Create at least **8-10 ships** for variety:

**Starter Ships** (Free, unlocked by default):
1. **Scout** - Fast, low health (Tank archetype)
2. **Fighter** - Balanced stats (AllAround archetype)
3. **Interceptor** - High speed, medium health (DamageDealer archetype)

**Unlockable Ships** (Level/Currency requirements):
4. **Heavy** - Slow, high health (Tank)
5. **Sniper** - Long-range damage (DamageDealer)
6. **Support** - Utility-focused (Controller)
7. **Assassin** - High burst damage (DamageDealer)
8. **Fortress** - Extreme tankiness (Tank)

**Premium Ships** (Gem-only or high progression):
9. **Phantom** - Stealth mechanics (DamageDealer)
10. **Titan** - Massive ship with unique abilities (Tank)

**For Each Ship:**
- Name, description
- Base stats: Health (100-300), Speed (5-15), Damage multiplier (0.8-1.5)
- Archetype
- 3D model or sprite
- Unlock requirement (e.g., Level 5, 2000 credits, or 100 gems)

#### **B. Perks** (3-4 hours)
Create **24-30 perks** (8-10 per tier):

**Tier 1 Perks** (Low cost, low cooldown):
- Speed Boost (15s cooldown): +50% speed for 3s
- Shield (20s cooldown): Block next 50 damage
- Quick Fire (10s cooldown): Fire 3 missiles rapidly
- Dash (12s cooldown): Instant movement in direction
- Heal (30s cooldown): Restore 50 HP
- Decoy (25s cooldown): Create fake ship
- etc.

**Tier 2 Perks** (Medium cost/cooldown):
- Cloak (40s cooldown): Invisible for 5s
- Reflect Shield (45s cooldown): Reflect projectiles for 4s
- Missile Barrage (35s cooldown): Fire 5 homing missiles
- Time Slow (60s cooldown): Slow enemy for 3s
- etc.

**Tier 3 Perks** (High cost/cooldown):
- Black Hole (90s cooldown): Pull enemy toward point
- Nuke (120s cooldown): Massive AoE damage
- Resurrection (One-time): Revive on death
- Ultimate Weapon (90s cooldown): Super powerful attack
- etc.

#### **C. Passives** (1-2 hours)
Create **12-16 passives**:
- +10% Max Health
- +15% Speed
- +20% Damage
- Shield Regeneration (5 HP/s)
- Missile Accuracy +15%
- Cooldown Reduction -10%
- Lifesteal 20%
- Damage Reduction 10%
- etc.

#### **D. Quests** (1-2 hours)
Create **30-40 quests** (focused on online play):

**Daily Quests** (10):
- Win 3 matches
- Deal 500 damage
- Win using [specific ship]
- Fire 20 missiles
- Achieve 70% accuracy
- Win without taking damage
- etc.

**Weekly Quests** (10):
- Win 15 matches
- Reach ELO 1300
- Play 5 different ships
- Complete 10 daily quests
- etc.

**Season Quests** (10-20):
- Reach Level 10
- Unlock 5 ships
- Win 50 matches
- Complete Battle Pass tier 20
- etc.

#### **E. Achievements** (2-3 hours)
Create **40-60 achievements**:

**Combat**:
- First Blood: Win your first match
- Sharpshooter: 90% accuracy in a match
- Untouchable: Win without taking damage
- Comeback King: Win from 50+ HP deficit
- etc.

**Progression**:
- Level Up: Reach Level 5/10/20/50
- Ship Collector: Unlock 5/10/15 ships
- Currency Hoarder: Accumulate 10K credits
- etc.

**Competitive**:
- Ranked Warrior: Play 10/50/100 ranked matches
- ELO Climber: Reach 1400/1600/1800 ELO
- Win Streak: Win 3/5/10 matches in a row
- etc.

#### **F. Battle Pass** (1 hour)
Create **Battle Pass with 30 tiers**:

Example tier rewards:
- Tier 1: 100 credits (free) / 50 gems (premium)
- Tier 5: Common ship (free) / Rare ship (premium)
- Tier 10: 500 credits (free) / Epic ship (premium)
- Tier 15: Rare perk (free) / Cosmetic skin (premium)
- Tier 20: 1000 credits (free) / Legendary ship (premium)
- Tier 30: Epic ship (free) / Exclusive legendary ship (premium)

**See:** `CONTENT_CREATION_GUIDE.md`

---

### **Phase 5: Visual Polish** (4-8 hours, can be done in parallel)
**Goal:** Make the game look good

**Priority Items:**

#### **A. Ship Visuals** (2-3 hours)
**Options:**
1. **Placeholder** (fastest): Unity primitives + materials
2. **2D Sprites** (fast): Create/buy ship sprites
3. **3D Models** (best): Create in Blender or buy Asset Store packs
4. **Recommendation:** Start with **Asset Store** ship packs for speed

**Suggested Asset Packs:**
- "Space Ship Collection" (free)
- "Sci-Fi Ships Pack" ($15-30)
- Various free spaceship models on Sketchfab

#### **B. VFX** (2-3 hours)
Required effects:
- **Missile Trail** - Particle system with glow
- **Hit Impact** - Spark particles
- **Explosion** - Fire/smoke particles
- **Shield Effect** - Transparent dome with shimmer
- **Speed Boost** - Motion blur trails
- **Level Up** - Golden sparkle burst
- **Achievement Unlock** - Confetti/celebration

Use Unity's Particle System or buy VFX packs.

#### **C. UI Styling** (2 hours)
- Color scheme (dark space theme with neon accents)
- Custom buttons (9-slice sprites)
- Health bars (gradient fill)
- XP bars (animated fill)
- Background patterns

**See:** `ART_PIPELINE_GUIDE.md`

---

### **Phase 6: Testing & Balancing** (Ongoing)
**Goal:** Ensure everything works and is fun

#### **A. Online Multiplayer Testing** (PRIORITY)
**Test with a friend or two devices:**
- [ ] Both players can connect
- [ ] Movement syncs properly (no jittering)
- [ ] Missiles hit correctly
- [ ] Health updates in real-time
- [ ] Winner declared correctly
- [ ] No game-breaking bugs
- [ ] Acceptable latency (<150ms)
- [ ] Reconnection works after disconnect

#### **B. Balance Testing**
- [ ] No ship is overpowered
- [ ] All perks are useful
- [ ] Matches last 2-5 minutes (good length)
- [ ] Economy feels fair (not too grindy)
- [ ] ELO system works (fair matches)

#### **C. Bug Fixing**
- [ ] No crashes
- [ ] No soft-locks
- [ ] No exploits
- [ ] UI works on all resolutions
- [ ] Save/load works reliably

**See:** `TESTING_GUIDE.md`

---

## 📦 What to Create in Unity

### **Prefabs (Priority Order)**

#### **1. Network Prefabs (CRITICAL)**
```
Assets/Prefabs/Network/
├── NetworkedPlayerShip.prefab (has NetworkObject + NetworkTransform)
├── NetworkedMissile.prefab (has NetworkObject + NetworkTransform)
└── NetworkGameManager.prefab (handles match state)
```

#### **2. UI Prefabs**
```
Assets/Prefabs/UI/
├── MainMenuUI.prefab
├── MatchmakingUI.prefab
├── LobbyUI.prefab
├── ShipSelectionUI.prefab
├── MatchResultsUI.prefab
├── BattlePassUI.prefab
├── AchievementsUI.prefab
└── Components/
    ├── PlayerCard.prefab (shows player info in lobby)
    ├── ShipCard.prefab (ship selection grid)
    ├── QuestCard.prefab
    └── AchievementCard.prefab
```

#### **3. Game Prefabs**
```
Assets/Prefabs/Ships/
├── Ship_Scout.prefab
├── Ship_Fighter.prefab
└── ... (one per ship)

Assets/Prefabs/VFX/
├── VFX_MissileTrail.prefab
├── VFX_Explosion.prefab
└── VFX_HitImpact.prefab
```

### **ScriptableObject Assets**

```
Assets/Data/
├── Ships/
│   ├── Ship_Scout.asset
│   ├── Ship_Fighter.asset
│   └── ... (10-15 ships)
├── Perks/
│   ├── Tier1/
│   │   ├── Perk_SpeedBoost.asset
│   │   └── ... (8-10 tier 1 perks)
│   ├── Tier2/
│   └── Tier3/
├── Passives/
│   ├── Passive_HealthBoost.asset
│   └── ... (12-16 passives)
├── Quests/
│   ├── Daily/
│   ├── Weekly/
│   └── Season/
├── Achievements/
│   ├── Combat/
│   ├── Progression/
│   └── Competitive/
└── BattlePass/
    └── Season1_BattlePass.asset
```

---

## 🎮 Online Multiplayer Flow

### **Complete Match Flow:**

1. **Player opens game**
   - Auto-login with Unity Authentication
   - Load PlayerAccountData from cloud
   - Display main menu

2. **Player clicks "PLAY"**
   - Show Online Matchmaking screen
   - Options: Quick Match, Create Lobby, Join by Code

3. **Player clicks "Quick Match"**
   - Search for lobbies with similar ELO (±200)
   - If found: Join lobby
   - If not found: Create new lobby and wait

4. **Lobby screen**
   - Show lobby code
   - Show both players (host + guest)
   - Players select ships
   - Players click "Ready"
   - Host clicks "Start Match" when both ready

5. **Loading screen**
   - Initialize Unity Relay connection
   - Spawn networked PlayerShips
   - Spawn networked planets/obstacles

6. **Match countdown**
   - "3... 2... 1... FIGHT!"
   - Enable player controls

7. **Match gameplay**
   - Players control ships (movement synced via NetworkTransform)
   - Players fire missiles (spawned via NetworkObject)
   - Server handles hit detection
   - Health updates sync to all clients
   - First player to 0 HP loses

8. **Match ends**
   - Show results screen (winner/loser, stats, XP, ELO change)
   - Update PlayerAccountData (level, currency, quests, achievements)
   - Save to cloud
   - Submit stats to leaderboards
   - "Play Again" or "Return to Menu"

---

## 📊 Economy Design (Online-Focused)

### **Currency Types:**

1. **Credits** (Soft Currency)
   - Earned: 50-150 per match (based on performance)
   - Used for: Common/Rare ships, perks, passives

2. **Gems** (Hard Currency)
   - Earned: 10-20 per day (daily login, quests, battle pass)
   - Purchased: Real money (optional)
   - Used for: Premium ships, Battle Pass, cosmetics

### **Progression Curve:**

- **Level 1-5**: Tutorial/starter phase (unlock 2-3 common ships)
- **Level 5-10**: Early game (unlock perks, learn matchmaking)
- **Level 10-20**: Mid game (unlock rare ships, start ranked)
- **Level 20+**: Endgame (competitive, legendary ships, high ELO)

### **Time to Unlock:**

- Common ship: 10-15 matches (~2-3 hours)
- Rare ship: 30-40 matches (~6-8 hours)
- Epic ship: 100+ matches or 100 gems
- Legendary ship: 300+ matches or 500 gems
- Battle Pass premium: 1000 gems (~$10)

---

## 🗂️ File Organization

```
Assets/
├── Scenes/
│   ├── 00_Bootstrap.unity (initializes services)
│   ├── 01_MainMenu.unity
│   ├── 02_ShipSelection.unity
│   ├── 03_OnlineMatch.unity (networked game arena)
│   └── Testing/
│       ├── NetworkTest.unity
│       └── ServicesTest.unity
├── Scripts/
│   ├── Network/
│   │   ├── NetworkGameManager.cs
│   │   ├── NetworkPlayerShip.cs
│   │   ├── NetworkMissile.cs
│   │   └── NetworkMatchState.cs
│   ├── UI/
│   │   ├── MatchmakingUI.cs
│   │   ├── LobbyUI.cs
│   │   └── MatchResultsUI.cs
│   ├── Managers/
│   └── Gameplay/
├── Prefabs/
│   ├── Network/ (networked objects)
│   ├── UI/ (UI panels)
│   └── Ships/ (ship prefabs)
└── Data/ (ScriptableObjects)
```

---

## 📝 Recommended Order of Implementation

### **Week 1: Get Online Working**
1. Day 1-2: Unity Gaming Services + Netcode setup
2. Day 3-4: Basic online match (two players can connect and move)
3. Day 5: Missiles work online
4. Day 6-7: Matchmaking UI + lobby system

### **Week 2: Content + Polish**
1. Day 1-2: Create 8-10 ships
2. Day 3: Create 24 perks
3. Day 4: Create passives, quests, achievements
4. Day 5-6: UI polish (ship selection, results screen)
5. Day 7: Testing + bug fixes

### **Week 3: Battle Pass + Launch Prep**
1. Day 1: Battle Pass system
2. Day 2-3: Visual polish (VFX, ship models)
3. Day 4-5: Extensive testing
4. Day 6-7: Bug fixes, balancing

---

## ✅ Launch Checklist

Before launching:
- [ ] Online matchmaking works reliably
- [ ] At least 8 ships created
- [ ] At least 20 perks created
- [ ] Quests functional
- [ ] Achievements functional
- [ ] Battle Pass functional
- [ ] ELO system working
- [ ] Cloud save works
- [ ] No critical bugs
- [ ] Tested on multiple devices
- [ ] Tested with multiple players
- [ ] Performance is acceptable (60 FPS+)
- [ ] Latency is acceptable (<150ms)

---

## 🆘 Common Issues

### **Network Issues:**
- Players can't connect → Check Unity Relay configuration
- Ships jitter/lag → Adjust NetworkTransform settings
- Missiles don't hit → Implement server-authoritative hit detection
- Desync issues → Use NetworkVariables for all shared state

### **Matchmaking Issues:**
- Can't find opponents → Lower ELO range requirement
- Lobbies don't close → Implement proper cleanup
- Players stuck in lobby → Add timeout/force start

---

## 📚 Additional Documentation

- `UNITY_SETUP_GUIDE.md` - Complete project setup
- `ONLINE_MULTIPLAYER_GUIDE.md` - Detailed netcode implementation
- `UI_CREATION_GUIDE.md` - Building all UI screens
- `CONTENT_CREATION_GUIDE.md` - Creating ships, perks, etc.
- `TESTING_GUIDE.md` - QA procedures

---

**Total Estimated Time for MVP:**
- **With online multiplayer:** 30-40 hours
- **Polished release:** 80-120 hours

**Start with:** `UNITY_SETUP_GUIDE.md` → Focus on Netcode section

**Online multiplayer is the CORE of your game. Everything else supports it!** 🚀
