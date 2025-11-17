# 🚀 Gravity Wars - Progression System Setup Guide

## Overview

This document explains the complete progression system implementation for Gravity Wars, including account progression, battle passes, ship customization, unlocks, and currency management.

---

## 📁 File Structure

```
Assets/
├── Progression System/
│   ├── PlayerAccountData.cs         # Core account data structure
│   ├── BattlePassData.cs            # Battle pass definitions (tiers, rewards)
│   ├── ProgressionManager.cs        # Central progression manager (singleton)
│   ├── SaveSystem.cs                # JSON save/load system
│   ├── CosmeticsSystem.cs           # Skins, colors, decals
│   └── UI/
│       ├── ShipBuilderUI.cs         # Custom ship loadout builder
│       ├── ProgressionUI.cs         # Account level, XP, stats display
│       └── BattlePassUI.cs          # Battle pass tier/reward display
│
├── GameManager.cs                   # *** MODIFIED: Integrated XP awarding
└── Ship System/                     # (Existing) All ship components
    ├── ShipBodySO.cs
    ├── ShipPresetSO.cs
    ├── ActivePerkSO.cs
    ├── PassiveAbilitySO.cs
    ├── MoveTypeSO.cs
    └── MissilePresetSO.cs
```

---

## 🎯 Core Concepts

### 1. **Dual Progression Systems**

#### A. Account Progression (Global)
- **AccountLevel**: 1-50 (linear XP formula: 1000 + Level × 500)
- **AccountXP**: Earned from all matches
- **Unlocks**: Ships, perks, passives, move types unlocked at specific levels
- **Free Battle Pass**: Permanent progression with rewards at each level

#### B. Ship Progression (Per Loadout)
- **ShipLevel**: 1-20 (quadratic XP formula: 200 + 75 × Level²)
- **ShipXP**: Earned per-match, tied to specific loadout configuration
- **Key Feature**: Changing **missile only** does NOT reset ship XP!
  - XP is tracked by `ShipBody + Perks + Passive + MoveType` (missile excluded)
  - This allows players to swap missiles freely without losing progress

### 2. **Battle Pass System**

#### Free Battle Pass (Permanent)
- Tied to account level
- Never resets
- Unlocks core content (ships, perks, passives)
- Example rewards:
  - Level 5: Tier 1 Perk
  - Level 10: Passive Ability
  - Level 15: New Ship Body

#### Seasonal Battle Pass (Premium)
- Time-limited (e.g., 2-3 months)
- Two tracks: Free + Premium
- Free track: Available to all
- Premium track: Requires purchase (1000 gems example)
- Retroactive rewards: Buying premium grants all previous tier rewards
- Resets each season (progress starts fresh)

### 3. **Currency System**

#### Soft Currency (Coins)
- Earned from matches
- Used for: Ship customization, cosmetics, minor unlocks
- Free-to-play friendly

#### Hard Currency (Gems)
- Premium currency (IAP or battle pass rewards)
- Used for: Premium battle pass, exclusive skins, accelerators
- Can be earned slowly for free (battle pass tiers, achievements)

### 4. **Custom Ship Loadouts**

Players can create custom ships by combining:
- **Ship Body** (archetype: Tank/DD/Controller/AllAround)
- **3 Active Perks** (Tier 1/2/3, optional)
- **1-2 Passives** (optional, future expansion)
- **1 Move Type** (Normal/Precision/Warp)
- **1 Missile** (Light/Medium/Heavy)
- **Cosmetics** (skin, color scheme, decal - optional)

**Validation**: ProgressionManager automatically validates compatibility (archetype restrictions, unlocks, etc.)

---

## 🛠️ Setup Instructions

### Step 1: Create ProgressionManager GameObject

1. Create empty GameObject in your main scene: `ProgressionManager`
2. Add component: `ProgressionManager`
3. Set `DontDestroyOnLoad` (already handled in code)

### Step 2: Configure Content Databases

In ProgressionManager Inspector:

#### Battle Pass References:
- **freeBattlePass**: Create new `BattlePassData` asset (Right-click → Create → GravityWars → Progression → Battle Pass)
  - Set `isSeasonal = false`
  - Configure 50 tiers with rewards at key levels

- **seasonalBattlePass**: Create seasonal battle pass
  - Set `isSeasonal = true`
  - Set season dates (start/end)
  - Configure tiers (50-70 recommended)

#### Content Databases:
- **allShipBodies**: Drag all ShipBodySO assets
- **allPerks**: Drag all ActivePerkSO assets
- **allPassives**: Drag all PassiveAbilitySO assets
- **allMoveTypes**: Drag all MoveTypeSO assets
- **allMissiles**: Drag all MissilePresetSO assets

**Alternative**: Leave empty and they'll auto-populate from Resources folder

### Step 3: Create Battle Pass Tiers

Example free battle pass structure:

```csharp
// Tier 1 (Level 1)
xpRequired: 0
freeReward: 500 Coins

// Tier 5 (Level 5)
xpRequired: 3500
freeReward: Tier 1 Perk (Multi-Missile)

// Tier 10 (Level 10)
xpRequired: 6000
freeReward: Passive Ability (Sniper Mode)

// Tier 15 (Level 15)
xpRequired: 8500
freeReward: Tier 2 Perk (Cluster Missile) + 1000 Coins

// Tier 20 (Level 20)
xpRequired: 11000
freeReward: New Ship Body (Assault Frigate)
```

Example seasonal battle pass (Premium):

```csharp
// Each tier has FREE + PREMIUM rewards
Tier 10:
  freeReward: 200 Coins
  premiumReward: Exclusive Skin + 500 Gems

Tier 25:
  freeReward: Tier 1 Perk
  premiumReward: Premium Ship Body + Color Scheme

Tier 50 (Final):
  freeReward: 1000 Coins
  premiumReward: Legendary Skin + Title + 2000 Gems
```

### Step 4: Create UI Scenes

#### A. Ship Builder UI

1. Create UI Canvas: `ShipBuilderCanvas`
2. Add component: `ShipBuilderUI`
3. Setup panels:
   - Loadout Name Input (TMP_InputField)
   - Component selection panels (ship body, perks, passives, etc.)
   - Selected components display (TextMeshPro labels)
   - Preview panel (stats, validation errors)
   - Buttons (Save, Clear, Back)
4. Create **componentButtonPrefab**:
   - Button with:
     - Icon (Image)
     - Label (TextMeshProUGUI)
5. Assign all references in Inspector

#### B. Progression UI

1. Add to main menu canvas
2. Add component: `ProgressionUI`
3. Setup display elements:
   - Account level (TextMeshProUGUI)
   - XP bar (Slider with fill)
   - Currency display (soft/hard)
   - Stats (matches, win rate, damage)
   - Ship progression panel (optional, shows selected ship XP)
   - Next unlock preview
4. Assign references in Inspector
5. Call `RefreshUI()` when returning to menu

#### C. Battle Pass UI

1. Create new scene or panel: `BattlePassScreen`
2. Add component: `BattlePassUI`
3. Setup:
   - Header (title, current tier, XP bar)
   - Scrollable tier list (ScrollRect)
   - **tierItemPrefab**: Shows tier number + free/premium rewards
   - Purchase premium panel (price, button)
   - Rewards popup (for retroactive unlocks)
4. Assign references
5. Set `battlePass` to seasonal battle pass asset

### Step 5: Integration with GameManager

**Already done!** GameManager now calls `AwardMatchProgression()` after each match.

#### How it works:
1. Match ends (`GameOver()` coroutine)
2. Calls `AwardMatchProgression(winner)`
3. Calculates XP based on:
   - Win/loss (win = +100 bonus)
   - Rounds won (×25 per round)
   - Damage dealt (÷100)
   - Premium pass bonus (+50%)
4. Awards XP to account
5. Awards XP to ship (if loadout tracked)
6. Awards battle pass XP
7. Checks for level-ups
8. Grants rewards (unlocks, currency)
9. Auto-saves

---

## 💾 Save System

### How Saving Works

- **Auto-save**: Enabled by default (`ProgressionManager.autoSave = true`)
- **Save location**: `Application.persistentDataPath/Saves/player_data.json`
- **Format**: JSON (human-readable)
- **Backup**: Automatic backup created on each save

### Save File Contents

```json
{
  "playerID": "player_abc123",
  "displayName": "ProGamer",
  "accountLevel": 15,
  "accountXP": 8500,
  "softCurrency": 5000,
  "hardCurrency": 250,
  "battlePassTier": 12,
  "battlePassXP": 6400,
  "hasPremiumBattlePass": true,
  "unlockedShipBodyIDs": ["TankBody1", "DDBody1", "AllAroundBody1"],
  "unlockedTier1PerkIDs": ["MultiMissile", "ExplosiveMissile"],
  "customShipLoadouts": [
    {
      "loadoutID": "guid-12345",
      "loadoutName": "My Tank Build",
      "shipBodyName": "TankBody1",
      "tier1PerkName": "MultiMissile",
      "moveTypeName": "NormalMove",
      "equippedMissileName": "HeavyMissile1"
    }
  ],
  "shipProgressionData": [
    {
      "loadoutKey": "TankBody1|MultiMissile|||NormalMove",
      "shipLevel": 8,
      "shipXP": 3200
    }
  ]
}
```

### Manual Save/Load

```csharp
// Save
ProgressionManager.Instance.Save();

// Load
ProgressionManager.Instance.Load();

// Delete (reset account)
SaveSystem.DeleteSaveData();

// Export for sharing/debugging
SaveSystem.ExportSaveData("/path/to/export.json");

// Import from file
SaveSystem.ImportSaveData("/path/to/import.json");
```

---

## 🎨 Cosmetics System

### Creating Cosmetic Assets

#### Ship Skin:
```
Right-click → Create → GravityWars → Cosmetics → Ship Skin

Fields:
- skinID: Unique identifier
- skinName: Display name
- modelPrefab: 3D model prefab to replace default
- archetype restrictions (optional)
- isPremiumExclusive, isSeasonalExclusive
```

#### Color Scheme:
```
Right-click → Create → GravityWars → Cosmetics → Color Scheme

Fields:
- primaryColor, secondaryColor, accentColor, emissionColor
- unlockLevel (0 = starter, 10 = level 10 unlock)
```

#### Decal:
```
Right-click → Create → GravityWars → Cosmetics → Decal

Fields:
- decalTexture: PNG/JPEG texture
- placement: Body/Wings/Engine/Weapon
- isPremiumExclusive, isAchievementReward
```

### Applying Cosmetics

```csharp
// In ship spawning code:
CosmeticsApplier applier = GetComponent<CosmeticsApplier>();
applier.ApplyCosmeticsToShip(shipGameObject, customLoadout);
```

---

## 🔧 Advanced Configuration

### XP Formulas

#### Account Level XP (Linear):
```csharp
xpForLevel = 1000 + (level × 500)

Example:
Level 1: 0 XP
Level 2: 1500 XP
Level 10: 6000 XP
Level 50: 26000 XP
```

#### Ship Level XP (Quadratic):
```csharp
xpForLevel = 200 + (75 × level²)

Example:
Level 1: 0 XP
Level 2: 275 XP
Level 10: 7700 XP
Level 20: 30200 XP
```

### Match XP Calculation

```csharp
baseXP = 50
winBonus = won ? 100 : 0
roundBonus = roundsWon × 25
damageBonus = damageDealt ÷ 100

totalXP = baseXP + winBonus + roundBonus + damageBonus

if (hasPremiumPass)
    totalXP × 1.5  // 50% bonus
```

**Example:**
- Won match: 3-1 (won 3 rounds, lost 1)
- Damage dealt: 15,000
- Premium pass: Yes

```
baseXP = 50
winBonus = 100
roundBonus = 3 × 25 = 75
damageBonus = 15000 ÷ 100 = 150

total = 50 + 100 + 75 + 150 = 375
premium bonus = 375 × 0.5 = 188
final = 375 + 188 = 563 XP
```

### Unlock Gates

You can gate content by:
1. **Account Level**: Set `requiredAccountLevel` on assets
2. **Battle Pass Tier**: Rewards in `BattlePassData` tiers
3. **Currency**: Check `CanAfford()` before unlock
4. **Premium Status**: `isPremiumExclusive` flag on cosmetics
5. **Season**: `isSeasonalExclusive` + `seasonNumber`

---

## 🐛 Debugging & Testing

### Console Logs

All systems log with prefixes:
- `[ProgressionManager]` - Core progression events
- `[SaveSystem]` - Save/load operations
- `[PlayerAccountData]` - Account changes
- `[ShipBuilderUI]` - Loadout validation
- `[GameManager]` - Match XP awards

### Testing Commands (Add to Debug Menu)

```csharp
// Grant XP
ProgressionManager.Instance.currentPlayerData.AddAccountXP(1000);

// Unlock item
ProgressionManager.Instance.UnlockItem(somePerkSO);

// Add currency
ProgressionManager.Instance.currentPlayerData.AddCurrency(5000, 100);

// Level up ship
CustomShipLoadout loadout = /* get loadout */;
ProgressionManager.Instance.currentPlayerData.AddShipXP(loadout, 5000);

// Purchase premium pass
ProgressionManager.Instance.PurchasePremiumBattlePass(1000);

// Reset save
SaveSystem.DeleteSaveData();
ProgressionManager.Instance.CreateNewAccount("TestPlayer", "test_123");
```

### Save File Location

```
Windows: C:/Users/[Username]/AppData/LocalLow/[CompanyName]/GravityWars/Saves/
Mac: ~/Library/Application Support/[CompanyName]/GravityWars/Saves/
Linux: ~/.config/unity3d/[CompanyName]/GravityWars/Saves/
```

Use `Debug.Log(SaveSystem.GetSavePath())` to find exact path.

---

## 📊 Balance Recommendations

### XP Rates
- Average match: 3-5 minutes
- XP per match: 200-600 (depending on performance)
- Matches to level up (account):
  - Early levels (1-10): 3-5 matches
  - Mid levels (10-30): 5-10 matches
  - Late levels (30-50): 10-20 matches

### Battle Pass Completion
- Season duration: 60-90 days
- Total XP needed: ~50,000 for max tier
- Daily play required: ~30 minutes (2-3 matches)
- Casual players should reach tier 30-40
- Dedicated players reach tier 50

### Currency Pricing
- Premium Battle Pass: 1000 gems (~$10 USD equivalent)
- Exclusive Skins: 500-1500 gems
- Color Schemes: 200-500 coins (soft currency)
- Decals: 300-800 coins

---

## 🚀 Future Enhancements

### Phase 4+ (Post-Launch)

1. **Achievements System**
   - Track milestones (100 wins, 1M damage, etc.)
   - Grant exclusive rewards (titles, decals, currency)

2. **Daily/Weekly Quests**
   - "Win 3 matches" → 500 coins
   - "Deal 50,000 damage" → 200 gems

3. **Leaderboards**
   - Global ranking by account level
   - Seasonal rankings

4. **Clan/Guild System**
   - Shared progression
   - Clan wars

5. **Prestige System**
   - Reset to level 1 at max level
   - Earn prestige currency for exclusive items

6. **Events**
   - Limited-time challenges
   - Event-exclusive rewards

---

## 📞 Support & Troubleshooting

### Common Issues

**Q: Save file not loading?**
A: Check console for errors. Try loading backup: `SaveSystem.LoadBackup()`

**Q: XP not awarding after matches?**
A: Ensure ProgressionManager GameObject exists in scene with correct references.

**Q: Invalid loadout validation errors?**
A: Check archetype compatibility. Some perks/moves are archetype-restricted.

**Q: Premium pass not granting retroactive rewards?**
A: This should happen automatically. Check logs for errors. Verify `PurchasePremiumBattlePass()` completed.

**Q: Ship XP resetting when changing missile?**
A: This is a bug if happening. Ship XP should be tied to `Body+Perks+Passive+MoveType` only. Check `CustomShipLoadout.GetProgressionKey()`.

---

## ✅ Checklist for Launch

- [ ] Configure free battle pass (50 tiers minimum)
- [ ] Configure seasonal battle pass (50-70 tiers)
- [ ] Create starter content (1-2 ships, 1 perk, 1 passive, all move types)
- [ ] Balance XP formulas (playtest 10+ matches)
- [ ] Set currency prices
- [ ] Create premium cosmetics
- [ ] Test save/load extensively
- [ ] Test ship builder validation
- [ ] Test battle pass purchase flow
- [ ] Add analytics tracking (XP gained, purchases, etc.)
- [ ] Implement server-side save backup (for multiplayer)

---

**Congratulations! Your progression system is now fully implemented and ready for testing!** 🎉

For questions or issues, check the code comments or debug logs. All systems are heavily documented.
