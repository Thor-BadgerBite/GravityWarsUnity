# Gravity Wars - Complete Implementation Plan

## 📋 Current State

### ✅ What's Already Working
- **Hotseat Local Multiplayer** - 2-player local PVP works perfectly
- **Unified Data System** - Single PlayerAccountData structure for all save data
- **Cloud Save Infrastructure** - Code ready, needs Unity Gaming Services setup
- **Progression System** - ProgressionManager handles XP, levels, currency, unlocks
- **Save System** - Local JSON saves working, cloud sync ready
- **Lobby System** - LobbyManager code ready, needs Unity Netcode package
- **Analytics System** - AnalyticsService ready for tracking

### 🔧 What Needs Implementation
1. **Unity Gaming Services Setup** (for cloud save & online multiplayer)
2. **UI Screens** (Main Menu, Ship Selection, Progression, Battle Pass, etc.)
3. **Content Creation** (Ships, Perks, Passives, Quests, Achievements)
4. **Visual Polish** (Ship 3D models, VFX, UI styling)
5. **Online Multiplayer Integration** (netcode setup)
6. **Testing & Balancing**

---

## 🎯 Implementation Phases

### **Phase 1: Unity Project Setup** (1-2 hours)
**Goal:** Install required packages and configure services

**Tasks:**
1. Install Unity Gaming Services packages
2. Configure Unity Cloud Save
3. Install Unity Netcode for GameObjects (optional for now)
4. Configure project settings
5. Test authentication

**See:** `UNITY_SETUP_GUIDE.md`

---

### **Phase 2: Core UI Screens** (4-6 hours)
**Goal:** Create all main menu and game UI

**Screens to Create:**
1. **Main Menu Screen**
   - Player profile display (username, level, XP bar)
   - Currency display (credits, gems)
   - Navigation buttons (Play, Ships, Battle Pass, Achievements, Settings)

2. **Ship Selection Screen**
   - 3D ship viewer (rotate, zoom)
   - Ship stats panel
   - Loadout editor
   - Ship unlock requirements

3. **Battle Pass Screen**
   - Tier progress bar
   - Reward grid (free & premium tracks)
   - XP display
   - Purchase premium button

4. **Achievements Screen**
   - Achievement list with progress bars
   - Filter by type (Single, Incremental, Tiered)
   - Claim rewards button

5. **Quest Screen**
   - Daily quests section
   - Weekly quests section
   - Season quests section
   - Progress tracking

6. **Settings Screen**
   - Audio sliders
   - Graphics options
   - Controls settings
   - Account management

**See:** `UI_CREATION_GUIDE.md`

---

### **Phase 3: Content Creation** (8-12 hours)
**Goal:** Create all game content as ScriptableObjects

**Content Types:**

#### **A. Ships (10-15 ships)**
Create `ShipBodySO` ScriptableObjects:
- **Starter Ships** (3): Basic ships players start with
- **Common Ships** (4): Unlocked early through progression
- **Rare Ships** (4): Unlocked mid-game
- **Epic Ships** (3): High-tier unlocks
- **Legendary Ships** (1-2): Premium/endgame ships

**For Each Ship:**
- Name, description, tier
- Base stats (health, speed, damage)
- Archetype (Tank, DamageDealer, AllAround, Controller)
- 3D model/sprite
- Unlock requirements (level, currency, quest)

#### **B. Perks (24-30 perks)**
Create `ActivePerkSO` ScriptableObjects:
- **Tier 1 Perks** (8-10): Weak but cheap
- **Tier 2 Perks** (8-10): Moderate power/cost
- **Tier 3 Perks** (8-10): Powerful but expensive

**For Each Perk:**
- Name, description, tier
- Cooldown, energy cost
- Effect implementation
- VFX/SFX
- Unlock requirements

#### **C. Passive Abilities (12-16)**
Create `PassiveAbilitySO` ScriptableObjects:
- Stat boosts (damage +10%, health +15%, etc.)
- Special effects (shield regeneration, lifesteal, etc.)
- Archetype restrictions (some only work on certain ship types)

#### **D. Quests (30-40 quests)**
Create `QuestDataSO` ScriptableObjects:
- **Daily Quests** (10): Reset every 24h, quick objectives
- **Weekly Quests** (10): Reset weekly, more challenging
- **Season Quests** (10-20): One-time, progression milestones

**Quest Types:**
- Win X matches
- Deal X damage
- Use specific ship/perk X times
- Win without taking damage
- Hit X missiles in a row

#### **E. Achievements (40-60 achievements)**
Create `AchievementTemplateSO` ScriptableObjects:
- **Single Achievements**: One-time accomplishments
- **Incremental Achievements**: Track progress (win 10/50/100 matches)
- **Tiered Achievements**: Bronze/Silver/Gold tiers

**Categories:**
- Combat achievements
- Progression achievements
- Skill achievements
- Special achievements

#### **F. Battle Pass (30 tiers)**
Create `BattlePassData` ScriptableObject:
- Define 30 tiers with rewards each
- Free track rewards (currency, common ships)
- Premium track rewards (exclusive ships, gems, cosmetics)
- XP required per tier

**See:** `CONTENT_CREATION_GUIDE.md`

---

### **Phase 4: Ship 3D Models & VFX** (Varies based on art style)
**Goal:** Add visual polish

**Options:**
1. **Placeholder Art**: Use Unity primitives + materials (fast, functional)
2. **2D Sprites**: Create/buy 2D ship sprites (medium time)
3. **3D Models**: Model ships in Blender (time-consuming but polished)
4. **Asset Store**: Buy ready-made space ship packs (fastest, costs money)

**VFX Needed:**
- Missile trails
- Hit effects
- Explosion effects
- Shield effects
- Perk activation effects
- Level-up effects
- Achievement unlock effects

**See:** `ART_PIPELINE_GUIDE.md`

---

### **Phase 5: Testing & Balancing** (Ongoing)
**Goal:** Ensure everything works and is fun

**Testing Checklist:**
- [ ] All ships balanced (no overpowered/underpowered ships)
- [ ] All perks functional and useful
- [ ] Quests completable and fair
- [ ] Achievements tracking correctly
- [ ] Progression feels rewarding
- [ ] Economy balanced (currency earning/spending)
- [ ] Cloud save working across devices
- [ ] No game-breaking bugs

---

### **Phase 6: Online Multiplayer** (8-12 hours)
**Goal:** Enable online PVP

**Tasks:**
1. Install Unity Netcode for GameObjects
2. Setup Relay/Lobby services
3. Implement network synchronization
4. Test matchmaking
5. Implement ELO rating system
6. Add ranked/casual modes

**See:** `ONLINE_MULTIPLAYER_GUIDE.md`

---

## 📦 What to Create in Unity

### **Prefabs Needed:**

#### **UI Prefabs:**
```
Assets/UI/Prefabs/
├── MainMenuUI.prefab
├── ShipSelectionUI.prefab
├── BattlePassUI.prefab
├── AchievementsPanelUI.prefab
├── QuestsPanelUI.prefab
├── SettingsUI.prefab
├── LoadoutEditorUI.prefab
├── MatchResultsUI.prefab
└── Components/
    ├── CurrencyDisplay.prefab
    ├── XPBar.prefab
    ├── ShipCard.prefab
    ├── QuestEntry.prefab
    ├── AchievementEntry.prefab
    └── BattlePassTier.prefab
```

#### **Game Prefabs:**
```
Assets/Prefabs/
├── Ships/
│   ├── Ship_Starter_01.prefab
│   ├── Ship_Starter_02.prefab
│   └── ... (one per ship)
├── Missiles/
│   ├── Missile_Standard.prefab
│   ├── Missile_Heavy.prefab
│   └── ... (one per missile type)
├── VFX/
│   ├── VFX_MissileTrail.prefab
│   ├── VFX_Explosion.prefab
│   ├── VFX_HitImpact.prefab
│   └── VFX_LevelUp.prefab
└── Managers/
    ├── GameManager.prefab
    ├── ProgressionManager.prefab
    └── UIManager.prefab
```

### **ScriptableObject Assets:**

```
Assets/Data/
├── Ships/
│   ├── Starter/
│   │   ├── Ship_Starter_Scout.asset
│   │   ├── Ship_Starter_Fighter.asset
│   │   └── Ship_Starter_Tank.asset
│   ├── Common/
│   ├── Rare/
│   ├── Epic/
│   └── Legendary/
├── Perks/
│   ├── Tier1/
│   ├── Tier2/
│   └── Tier3/
├── Passives/
│   ├── Passive_ShieldRegen.asset
│   ├── Passive_DamageBoost.asset
│   └── ...
├── Quests/
│   ├── Daily/
│   ├── Weekly/
│   └── Season/
├── Achievements/
│   ├── Combat/
│   ├── Progression/
│   └── Special/
├── BattlePass/
│   ├── Season1_BattlePass.asset
│   └── Season2_BattlePass.asset
└── MoveTypes/
    ├── MoveType_Linear.asset
    ├── MoveType_Curved.asset
    └── ...
```

---

## 🎮 Gameplay Loop

### **Session Flow:**
1. Player opens game → Main Menu loads
2. ProgressionManager loads PlayerAccountData from save
3. Player sees their profile, currency, level
4. Player navigates to:
   - **Play** → Ship Selection → Match → Results → XP/Currency gained
   - **Ships** → Browse ships → Unlock new ship → Customize loadout
   - **Battle Pass** → View rewards → Claim tier → Purchase premium
   - **Quests** → View progress → Claim rewards
   - **Achievements** → View progress → Claim rewards
5. On exit → Auto-save to local + cloud

### **Progression Loop:**
```
Play Match
    ↓
Earn XP + Currency
    ↓
Level Up / Unlock Content
    ↓
Customize Ships / Loadouts
    ↓
Play Match (stronger/different build)
    ↓
Complete Quests
    ↓
Progress Battle Pass
    ↓
Unlock More Content
    ↓
Repeat
```

---

## 📊 Economy Design

### **Currency Types:**
1. **Credits** (Soft Currency)
   - Earned from: Matches, quests, achievements, battle pass
   - Used for: Unlocking common/rare ships, perks, passives
   - Typical earning: 50-200 per match

2. **Gems** (Hard Currency)
   - Earned from: Battle pass, achievements, daily rewards
   - Purchased with: Real money (optional)
   - Used for: Epic/legendary ships, premium battle pass, cosmetics
   - Typical earning: 5-20 per day (free)

### **Unlock Costs Example:**
```
Starter Ships:     Free (unlocked by default)
Common Ships:      500-1000 credits
Rare Ships:        2000-5000 credits
Epic Ships:        10000 credits OR 100 gems
Legendary Ships:   50000 credits OR 500 gems

Tier 1 Perks:      200-500 credits
Tier 2 Perks:      1000-2000 credits
Tier 3 Perks:      5000 credits OR 50 gems

Premium Battle Pass: 1000 gems (~$9.99)
```

---

## 🗂️ File Organization

### **Recommended Folder Structure:**
```
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   ├── ShipSelection.unity
│   ├── GameArena.unity
│   └── Testing.unity
├── Scripts/
│   ├── Core/
│   ├── UI/
│   ├── Gameplay/
│   ├── Networking/
│   └── Progression/
├── Data/
│   └── (ScriptableObjects as shown above)
├── Prefabs/
│   └── (Prefabs as shown above)
├── Art/
│   ├── Models/
│   ├── Textures/
│   ├── Materials/
│   └── Sprites/
├── Audio/
│   ├── Music/
│   ├── SFX/
│   └── Mixers/
├── VFX/
│   └── Particles/
└── Resources/
    └── (Runtime loaded assets)
```

---

## 📝 Next Steps

### **Immediate Actions:**
1. ✅ Read `UNITY_SETUP_GUIDE.md` - Setup Unity Gaming Services
2. ✅ Read `UI_CREATION_GUIDE.md` - Create main menu UI
3. ✅ Read `CONTENT_CREATION_GUIDE.md` - Create starter content
4. ✅ Test hotseat mode with new content
5. ✅ Iterate based on feedback

### **Before Launch Checklist:**
- [ ] All core systems implemented
- [ ] At least 10 ships created
- [ ] At least 20 perks created
- [ ] At least 30 quests created
- [ ] Battle pass with 30 tiers
- [ ] Main menu polished
- [ ] Cloud save tested on multiple devices
- [ ] Tutorial/onboarding created
- [ ] Game balanced and fun
- [ ] No critical bugs

---

## 🆘 Common Issues & Solutions

### **Issue: "Cloud save not working"**
**Solution:** Check Unity Gaming Services setup in `UNITY_SETUP_GUIDE.md` section 3

### **Issue: "LobbyManager errors"**
**Solution:** Install Unity Netcode package or keep `#if UNITY_NETCODE_GAMEOBJECTS` directives

### **Issue: "ProgressionManager is null"**
**Solution:** Ensure ProgressionManager prefab is in scene and has DontDestroyOnLoad

### **Issue: "ScriptableObject data not showing in game"**
**Solution:** Assign ScriptableObjects to ProgressionManager's public lists in Inspector

### **Issue: "Saves not persisting"**
**Solution:** Check `SaveSystem.autoSave` is enabled and `SaveSystem.SAVE_FOLDER` path is writable

---

## 📚 Additional Documentation

- `UNITY_SETUP_GUIDE.md` - Complete Unity project setup
- `UI_CREATION_GUIDE.md` - Step-by-step UI creation
- `CONTENT_CREATION_GUIDE.md` - Creating ships, perks, quests, etc.
- `ART_PIPELINE_GUIDE.md` - Adding 3D models and VFX
- `ONLINE_MULTIPLAYER_GUIDE.md` - Implementing netcode
- `TESTING_GUIDE.md` - QA and balancing
- `MONETIZATION_GUIDE.md` - Optional: IAP and ads

---

**Total Estimated Time:**
- **Minimum Viable Product:** 20-30 hours
- **Polished Release:** 60-100 hours
- **With Online Multiplayer:** +20-30 hours

**Good luck! Start with UNITY_SETUP_GUIDE.md** 🚀
