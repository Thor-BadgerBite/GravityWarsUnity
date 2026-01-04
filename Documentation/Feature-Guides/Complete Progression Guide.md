# Complete Progression System Guide

**Gravity Wars - Account Progression, Battle Pass & Unlock Systems**

This guide documents the complete progression system (Levels 1-100), Battle Pass system (25 levels), and all unlock types.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [XP Sources & Progression Types](#xp-sources--progression-types)
3. [Action Points System](#action-points-system)
4. [Rank System](#rank-system)
5. [Account Level Progression (1-100)](#account-level-progression)
6. [Battle Pass System](#battle-pass-system)
7. [Missile Retrofit System](#missile-retrofit-system)
8. [Integration Guide](#integration-guide)
9. [Complete Unlock Schedule](#complete-unlock-schedule)

---

## System Overview

### Philosophy

The progression system uses **placeholder names** for all content. When you're ready, replace these placeholder IDs and names with your actual game data.

### Three Parallel Systems

1. **Account XP Progression**: Levels 1-100 with permanent unlocks
2. **Battle Pass**: Seasonal 25-level progression (free + premium tracks)
3. **Missile Retrofit**: Separate missile system with compatibility restrictions

### Key Design Principles

- **Missiles are SEPARATE from ships** - Players select missiles before matches, not during ship building
- **Custom ship building** uses: Body + Passive + Active (missiles selected separately)
- **Prebuild ships** are ready-to-use ships with pre-configured stats
- **Placeholder names** throughout - easy to find and replace later

---

## XP Sources & Progression Types

### Three Types of XP

**1. Account XP**
- **Source**: Battles (PvP matches, custom matches)
- **Progression**: Levels 1-100 with permanent unlocks
- **Awards**: Ships, abilities, features, game modes
- **Note**: Account XP and Ship XP are awarded **simultaneously** from the same source

**2. Ship XP**
- **Source**: Battles (same as Account XP)
- **Progression**: Individual ship mastery system
- **Awards**: Ship-specific upgrades and cosmetics
- **Note**: When you earn 200 XP from a battle, you receive **both** 200 Account XP **and** 200 Ship XP

**3. Battle Pass XP**
- **Source**: **Daily Quests ONLY**
- **Progression**: Seasonal 25-level system
- **Awards**: Seasonal rewards (skins, ships, gems)
- **Note**: Battle Pass XP is **separate** from battle rewards

### XP Award Summary

**After Each Battle:**
- ✅ Account XP: 200 XP (example amount)
- ✅ Ship XP: 200 XP (same amount, awarded simultaneously)
- ❌ Battle Pass XP: 0 XP (not awarded from battles)

**After Completing Daily Quest:**
- ❌ Account XP: 0 XP (not awarded from quests)
- ❌ Ship XP: 0 XP (not awarded from quests)
- ✅ Battle Pass XP: 100 XP (example amount, quests only)

---

## Action Points System

### Overview

Action Points (AP) determine what actions players can perform during their turn in battle.

### AP Pool

- **Default**: 3 Action Points per turn
- **Controller Archetype**: 4 Action Points per turn (special advantage)

### AP Costs

**Missile Fire:**
- Cost: **0 AP** (always available, free action)

**Movement:**
- Cost: **1 AP per move**
- **Repeatable**: Can move multiple times if you have AP remaining
- Example: With 3 AP, you can move 3 times in one turn

**Active Perks (Abilities):**
- **Tier 1 Perks**: 1 AP
- **Tier 2 Perks**: 2 AP
- **Tier 3 Perks**: 3 AP
- **One-time use**: Perks can only be used **once per turn**

### Action Rules

**Move Actions:**
- ✅ Can be repeated multiple times (if you have AP)
- ✅ Example: Move → Move → Move (costs 3 AP total)

**Perk Actions:**
- ❌ Can only be used **once per turn** (regardless of AP remaining)
- ❌ Example: Cannot use "Afterburner" twice in one turn, even with 3+ AP

**Turn Ending:**
- Moving **ends your turn** by default
- **Exception**: Passive abilities that allow "Fire After Move" enable firing after movement
- Without this passive, moving is your final action

### Example Turn Sequences

**Example 1: All-Around Ship (3 AP)**
- Fire Missile (0 AP) → Move (1 AP) → Move (1 AP) → Move (1 AP) = Turn ends
- Total AP used: 3 AP

**Example 2: Controller Ship (4 AP)**
- Move (1 AP) → Move (1 AP) → Use Tier 2 Perk (2 AP) → Fire Missile (0 AP) = Turn ends
- Total AP used: 4 AP

**Example 3: Tank Ship with Tier 3 Perk (3 AP)**
- Use Tier 3 Perk (3 AP) → Fire Missile (0 AP) = Turn ends
- Total AP used: 3 AP

---

## Rank System

### Overview

The competitive ranking system uses **16 ranks** from Ensign to Grand Admiral, based on ELO rating.

### Starting Rank

- **Starting Rank**: Ensign
- **Starting ELO**: 800
- **Ensign Range**: 700-1049 ELO

### Complete Rank List (16 Ranks)

| Rank | ELO Range | Description |
|------|-----------|-------------|
| 1. Ensign | 700-1049 | Starting rank for all new players |
| 2. Lieutenant | 1050-1199 | Beginner competitive players |
| 3. Captain | 1200-1349 | Competent tactical awareness |
| 4. Major | 1350-1499 | Above-average skill level |
| 5. Commander | 1500-1649 | Strong strategic understanding |
| 6. Colonel | 1650-1799 | Advanced competitive player |
| 7. Brigadier | 1800-1949 | Expert-level performance |
| 8. General | 1950-2099 | Elite tier player |
| 9. Field Marshal | 2100-2249 | Top 5% of players |
| 10. Admiral | 2250-2399 | Top 2% of players |
| 11. Fleet Admiral | 2400-2549 | Top 1% of players |
| 12. Supreme Admiral | 2550-2699 | Top 0.5% of players |
| 13. High Admiral | 2700-2849 | Top 0.25% of players |
| 14. Grand Admiral | 2850-2999 | Top 0.1% of players |
| 15. Legendary Admiral | 3000-3149 | Top 0.05% of players |
| 16. Apex Legend | 3150+ | Top 0.01% - The absolute best |

### Rank Progression

- **ELO Gain/Loss**: ±20-30 ELO per match (varies by opponent rating)
- **Promotion**: Automatically promoted when reaching next rank's ELO threshold
- **Demotion**: Can drop ranks if ELO falls below current rank minimum
- **Season Resets**: Soft reset at start of each competitive season

---

## Account Level Progression

### XP Requirements

XP required per level increases exponentially:
```
Formula: 1000 × (1.15 ^ (level - 1))
```

**Examples:**
- Level 1→2: 1,000 XP
- Level 10→11: 3,518 XP
- Level 25→26: 17,623 XP
- Level 50→51: 677,847 XP
- Level 100: Several million XP

### Level-Up Rewards

**Credits:**
```
Formula: 100 + (level × 50) + milestone_bonus
Milestone bonus: +500 credits every 10 levels
```

**Gems (premium currency):**
```
Formula: (level / 5) × 10 gems
Only at milestone levels (5, 10, 15, 20, etc.)
```

### Unlock Types

1. **Prebuild Ships** - 36 total (9 per archetype, distributed across levels 1-100)
2. **Ship Bodies** - 16 total (4 per archetype)
3. **Passive Abilities** - 30 total
4. **Active Abilities** - 20 total
5. **Missiles** - 17 total (separate retrofit system, starter missile included)
6. **Ship Classes** - 4 archetypes (All-Around, Tank, DD, Controller)
7. **Game Modes** - Ranked, Custom Match
8. **Features** - Achievements, Quests, Leaderboards
9. **Custom Slots** - 3 total (for saving loadouts)

---

## Complete Unlock Schedule

### Levels 1-10 (Early Game)

**Level 1** (Starting)
- Ship Class: All-Around (starter)
- Custom Slot #1 (starter)
- Ship Body: "StarSparrow Class" (starter body - All-Around)
- Prebuild Ship: "StarSparrow" (starter ship, given on account creation)
- Missile: "Default Starter Medium Missile" (basic missile, all players start with this)

**Level 3**
- Prebuild Ship: "Nova Class" (All-Around)
- Feature: Achievements

**Level 4**
- Missile: "Standard Mk-II" (improved standard)

**Level 5**
- Ship Class: Tank (unlock heavy armor ships)
- Feature: Daily Quests
- Game Mode: Custom Match
- Passive: "Armor Boost I" (+10% armor)
- Gems: **10 gems** 💎

**Level 6**
- Prebuild Ship: "Titan Defender" (Tank)

**Level 7**
- Passive: "Engine Tuning I" (+5% speed)
- Prebuild Ship: "Phoenix Mk-I" (All-Around)

**Level 8**
- Feature: Leaderboards
- Missile: "Light Swarm" (fast, low damage)

**Level 9**
- Active: "Afterburner" (temporary speed boost, 10s cooldown)

**Level 10**
- Game Mode: **Ranked PVP** (competitive mode unlocked!)
- Prebuild Ship: "Eclipse Striker" (All-Around)
- Gems: **20 gems** 💎

### Levels 11-20 (Mid-Early Game)

**Level 11**
- Passive: "Armor Boost II" (+20% armor)

**Level 12**
- Prebuild Ship: "Bastion Class" (Tank)
- Missile: "Heavy Impact" (slow, high damage)

**Level 13**
- Active: "Emergency Repair" (restore 30% HP, 45s cooldown)

**Level 14**
- Passive: "Critical Strike I" (+10% crit chance)

**Level 15**
- Ship Class: Damage Dealer (unlock glass cannon ships!)
- Gems: **30 gems** 💎

**Level 16**
- Prebuild Ship: "Viper Assault" (Damage Dealer)
- Missile: "Tactical EMP" (disables enemy briefly)

**Level 17**
- Passive: "Speed Boost II" (+15% speed)

**Level 18**
- Prebuild Ship: "Juggernaut" (Tank)
- Active: "Shield Overcharge" (temporary +50% armor, 60s cooldown)

**Level 19**
- Prebuild Ship: "Reaper Class" (Damage Dealer)

**Level 20**
- **Custom Slot #2** (second loadout slot!)
- Prebuild Ship: "Horizon Vanguard" (All-Around)
- Gems: **40 gems** 💎

### Levels 21-30 (Mid Game)

**Level 21**
- Ship Body: "Standard Frame" (All-Around)
- Ship Body: "Reinforced Hull" (Tank)
- Ship Body: "Striker Chassis" (Damage Dealer)

**Level 22**
- Passive: "Firepower Boost I" (+10% damage)
- Missile: "Piercing Arrow" (ignores some armor)

**Level 23**
- Prebuild Ship: "Spectre Hunter" (Damage Dealer)
- Active: "Stealth Cloak" (brief invisibility, 90s cooldown)

**Level 24**
- Passive: "Shield Regeneration" (slowly restore HP)

**Level 25**
- Ship Class: Controller (unlock tactical ships!)
- Gems: **50 gems** 💎

**Level 26**
- Prebuild Ship: "Nexus Command" (Controller)
- Missile: "Cluster Bomb" (splits into multiple)

**Level 27**
- Ship Body: "Tactician Frame" (Controller)
- Passive: "Energy Efficiency" (reduce cooldowns 10%)

**Level 28**
- Prebuild Ship: "Oracle Class" (Controller)

**Level 29**
- Active: "Gravity Well" (pull enemies, 120s cooldown)

**Level 30**
- Feature: Clans (future feature)
- Prebuild Ship: "Crimson Tempest" (Damage Dealer)
- Gems: **60 gems** 💎

### Levels 31-40 (Mid-Late Game)

**Level 31**
- Passive: "Armor Boost III" (+30% armor)

**Level 32**
- Prebuild Ship: "Phantom Ops" (Controller)
- Missile: "Ultimate Devastator" (massive damage, slow)

**Level 33**
- Active: "Missile Barrage" (rapid fire, 150s cooldown)

**Level 34**
- Passive: "Critical Strike II" (+20% crit chance)

**Level 35**
- Prebuild Ship: "Sovereign Elite" (Tank)
- Gems: **70 gems** 💎

**Level 36**
- Ship Body: "Advanced Frame" (All-Around)

**Level 37**
- Passive: "Speed Boost III" (+25% speed)

**Level 38**
- Active: "Time Dilation" (slow time briefly, 180s cooldown)

**Level 39**
- Passive: "Reactive Armor" (reduce incoming damage 15%)

**Level 40**
- **Custom Slot #3** (third loadout slot!)
- Prebuild Ship: "Infinity Class" (Controller)
- Gems: **80 gems** 💎

### Levels 41-50 (Late Game)

**Level 41**
- Passive: "Firepower Boost II" (+20% damage)

**Level 42**
- Ship Body: "Elite Hull" (Tank)
- Missile: "Vortex Spiral" (homing)

**Level 43**
- Active: "Phase Shift" (teleport short distance, 120s cooldown)

**Level 44**
- Passive: "Energy Shield" (absorb first hit)

**Level 45**
- Prebuild Ship: "Omega Apex" (All-Around)
- Missile: "Ultimate Singularity" (creates mini black hole)
- Gems: **90 gems** 💎

**Level 46**
- Passive: "Vampiric Strikes" (heal on hit)

**Level 47**
- Active: "Reflect Shield" (return damage to sender, 180s cooldown)

**Level 48**
- Ship Body: "Assault Chassis" (Damage Dealer)

**Level 49**
- Active: "EMP Blast" (disable all enemies briefly, 240s cooldown)

**Level 50**
- Prebuild Ship: "Celestial Monarch" (Controller)
- Gems: **100 gems** 💎

### Levels 51-60 (Veteran)

**Level 51**
- Passive: "Critical Strike III" (+30% crit chance)

**Level 52**
- Prebuild Ship: "Astral Wraith" (Controller)

**Level 53**
- Active: "Overdrive" (double fire rate, 180s cooldown)

**Level 54**
- Passive: "Armor Boost IV" (+40% armor)
- Prebuild Ship: "Annihilator Prime" (Damage Dealer)

**Level 55**
- Missile: "Ultimate Oblivion" (one-shot capability)
- Gems: **110 gems** 💎

**Level 56**
- Ship Body: "Tactical Frame" (Controller)

**Level 57**
- Passive: "Speed Boost IV" (+35% speed)
- Prebuild Ship: "Fortress Apex" (Tank)

**Level 58**
- Active: "Black Hole Generator" (massive gravity well, 300s cooldown)

**Level 59**
- Prebuild Ship: "Stellar Vanguard" (All-Around)

**Level 60**
- Gems: **120 gems** 💎

### Levels 61-70 (Elite)

**Level 61**
- Passive: "Firepower Boost III" (+30% damage)

**Level 62**
- Prebuild Ship: "Nebula Striker" (Damage Dealer)

**Level 63**
- Passive: "Critical Damage" (crits deal +50% damage)

**Level 64**
- Prebuild Ship: "Warlord Supreme" (Tank)

**Level 65**
- Gems: **130 gems** 💎

**Level 66**
- Ship Body: "Master Frame" (All-Around)
- Prebuild Ship: "Cosmic Dreadnought" (All-Around)

**Level 67**
- Active: "Temporal Freeze" (freeze enemies, 240s cooldown)

**Level 68**
- Prebuild Ship: "Void Harbinger" (Controller)

**Level 69**
- Passive: "Phoenix Rebirth" (revive once per match at 50% HP)

**Level 70**
- Gems: **140 gems** 💎

### Levels 71-80 (Master)

**Level 71**
- Ship Body: "Apex Frame" (All-Around - ultimate versatile)

**Level 72**
- Passive: "Armor Boost V" (+50% armor)
- Prebuild Ship: "Titan Colossus" (Tank)

**Level 73**
- Active: "Weapon Systems Override" (unlimited ammo brief, 300s cooldown)

**Level 74**
- Prebuild Ship: "Executioner Omega" (Damage Dealer)

**Level 75**
- Gems: **150 gems** 💎

**Level 76**
- Ship Body: "Ultimate Hull" (Tank - maximum armor)
- Prebuild Ship: "Oracle Sovereign" (Controller)

**Level 77**
- Passive: "Speed Boost V" (+45% speed)

**Level 78**
- Prebuild Ship: "Celestial Guardian" (All-Around)

**Level 79**
- Active: "Quantum Leap" (instant repositioning, 150s cooldown)

**Level 80**
- Gems: **160 gems** 💎

### Levels 81-90 (Grandmaster)

**Level 81**
- Passive: "Firepower Boost IV" (+40% damage)

**Level 82**
- Prebuild Ship: "Shadow Reaper" (Damage Dealer)

**Level 83**
- Ship Body: "Legendary Chassis" (Damage Dealer - ultimate offense)

**Level 84**
- Active: "Supernova" (massive AoE explosion, 360s cooldown)
- Prebuild Ship: "Horizon Apex" (All-Around)

**Level 85**
- Gems: **170 gems** 💎

**Level 86**
- Prebuild Ship: "Phantom Overlord" (Controller)

**Level 87**
- Passive: "Ultimate Reflexes" (dodge chance 25%)

**Level 88**
- Active: "Reality Distortion" (confuse enemy targeting, 240s cooldown)

**Level 89**
- Passive: "Ascension" (near-invincible legendary passive)

**Level 90**
- Gems: **180 gems** 💎

### Levels 91-100 (Legend)

**Level 91**
- Ship Body: "Divine Frame" (Controller - ultimate tactical)

**Level 92**
- Passive: "Armor Boost VI" (+60% armor - legendary)

**Level 93**
- Active: "Dimension Shift" (invulnerability brief, 420s cooldown)

**Level 94**
- Passive: "Combat Mastery" (enhanced combat effectiveness)

**Level 95**
- Gems: **190 gems** 💎

**Level 96**
- Passive: "Divine Protection" (auto-revive with full HP, once per match)

**Level 97**
- Active: "Omega Strike" (one-shot kill ability, 600s cooldown)

**Level 98**
- Passive: "Legendary Mastery" (ultimate combat effectiveness)

**Level 99**
- Prebuild Ship: "Immortal Juggernaut" (Tank)

**Level 100**
- Prebuild Ship: **"APEX LEGEND"** (Ultimate ship - All-Around)
- Gems: **200 gems** 💎
- **Special Reward**: Exclusive title "Legendary Commander"

---

## Battle Pass System

### Overview

- **Levels**: 25 total
- **Duration**: ~3 months per season
- **Tracks**: Free (all players) + Premium (purchased)
- **XP**: Separate from account XP (earned ONLY from daily quests)
- **Resets**: Every season

### XP Requirements

- **Per Level**: 1,000 XP (flat, not exponential)
- **Total for Level 25**: 25,000 XP
- **XP Source**: Daily quests only (NOT from battles)
- **Estimated Time**: Complete daily quests consistently over ~3 months

### Free Track Rewards (Levels 1-25)

| Level | Reward |
|-------|--------|
| 1 | Credits x500 |
| 2 | Skin: "Blue Wave" (starter skin) |
| 3 | Passive: "Speed Boost I" |
| 4 | Credits x750 |
| 5 | Missile: "Standard Mk-II" |
| 6 | Credits x1000 |
| 7 | Skin: "Iron Fortress" (tank skin) |
| 8 | Active: "Emergency Repair" |
| 9 | Credits x1250 |
| 10 | **Prebuild Ship: "Seasonal Scout"** (All-Around) |
| 11 | Credits x1500 |
| 12 | Skin: "Crimson Strike" (DD skin) |
| 13 | Passive: "Armor Boost II" |
| 14 | Credits x1750 |
| 15 | Missile: "Light Vortex" |
| 16 | Credits x2000 |
| 17 | Skin: "Shadow Ops" (controller skin) |
| 18 | Active: "EMP Pulse" |
| 19 | Credits x2500 |
| 20 | **Prebuild Ship: "Seasonal Defender"** (Tank) |
| 21 | Credits x3000 |
| 22 | Skin: "Golden Eagle" (All-Around skin) |
| 23 | Gems x50 |
| 24 | Credits x5000 |
| 25 | **Ship Body: "Seasonal Frame"** (All-Around) |

### Premium Track Rewards (Levels 1-25)

**Note**: Premium track requires purchase (~$10 USD)

| Level | Reward |
|-------|--------|
| 1 | Credits x1000 (2x free track) |
| 2 | **PREMIUM Skin: "Platinum Storm"** |
| 3 | Gems x25 |
| 4 | Passive: "Critical Strike" |
| 5 | **PREMIUM Ship: "Nebula Hunter"** (Damage Dealer) |
| 6 | Gems x30 |
| 7 | **PREMIUM Skin: "Cosmic Void"** |
| 8 | Active: "Stealth Cloak" |
| 9 | Gems x35 |
| 10 | **EXCLUSIVE Ship: "Stellar Dominator"** (All-Around) |
| 11 | Gems x40 |
| 12 | **PREMIUM Skin: "Diamond Fury"** |
| 13 | Missile: "Tactical EMP" |
| 14 | Gems x45 |
| 15 | **PREMIUM Ship: "Quantum Fortress"** (Tank) |
| 16 | Gems x50 |
| 17 | **PREMIUM Skin: "Royal Prestige"** |
| 18 | Active: "Time Dilation" |
| 19 | Gems x60 |
| 20 | **PREMIUM Ship: "Ethereal Phantom"** (Controller) |
| 21 | Gems x75 |
| 22 | **LEGENDARY Skin: "Celestial Radiance"** |
| 23 | Ship Body: "Premium Elite Frame" (All-Around) |
| 24 | Gems x100 |
| 25 | **ULTIMATE EXCLUSIVE: "Season Monarch"** (Controller) |

**Premium Track Totals:**
- Total Gems: 485 gems
- Exclusive Ships: 5
- Premium Skins: 5
- Ship Bodies: 1

---

## Missile Retrofit System

### Key Concept

**Missiles are SEPARATE from ships!** Players select missiles before matches, not during ship building.

**IMPORTANT**: This Complete Progression Guide is the **authoritative source** for all missile unlock information in Gravity Wars.

### Starting Missile

All players start with:
- **Missile**: "Default Starter Medium Missile"
- **Type**: Standard
- **Compatible With**: All ships
- **Given at**: Account creation (Level 1)

### Missile Unlock Schedule

**Note**: Missiles are unlocked through player progression at specific account levels.

| Level | Missile ID | Name | Type | Compatible With |
|-------|-----------|------|------|----------------|
| 1 | default_starter | Default Starter Medium Missile | Standard | All ships |
| 4 | standard_mk2 | Standard Mk-II | Standard | All ships |
| 8 | light_swarm | Swarm Light | Light | DD, All-Around |
| 12 | heavy_impact | Heavy Impact | Heavy | Tank, All-Around |
| 16 | tactical_emp | Tactical EMP | Tactical | Controller, All-Around |
| 22 | piercing_arrow | Piercing Arrow | Piercing | DD, All-Around |
| 26 | cluster_bomb | Cluster Bomb | Cluster | Tank, Controller |
| 32 | ultimate_devastator | Ultimate Devastator | Ultimate | All ships |
| 35 | light_vortex | Light Vortex | Light | DD, All-Around |
| 38 | heavy_crusher | Heavy Crusher | Heavy | Tank, All-Around |
| 42 | vortex_spiral | Vortex Spiral | Tactical | Controller, All-Around |
| 45 | ultimate_singularity | Singularity | Ultimate | All ships |
| 48 | piercing_lance | Piercing Lance | Piercing | DD, All-Around |
| 52 | cluster_nova | Cluster Nova | Cluster | Tank, Controller |
| 55 | ultimate_oblivion | Oblivion | Ultimate | All ships |
| 60 | light_photon | Photon Burst | Light | DD, All-Around |
| 65 | heavy_annihilator | Annihilator | Heavy | Tank, All-Around |

### Missile Type Compatibility

**Standard Missiles:**
- Work on ALL ship types
- Example: Standard Mk-I, Standard Mk-II

**Light Missiles:**
- Fast, low damage
- Work on: Damage Dealer, All-Around
- NOT compatible with: Tank, Controller

**Heavy Missiles:**
- Slow, high damage
- Work on: Tank, All-Around
- NOT compatible with: Damage Dealer, Controller

**Tactical Missiles:**
- Special effects (EMP, slow, etc.)
- Work on: Controller, All-Around
- NOT compatible with: Tank, Damage Dealer

**Piercing Missiles:**
- Ignore armor
- Work on: Damage Dealer, All-Around
- NOT compatible with: Tank, Controller

**Cluster Missiles:**
- Split into multiple
- Work on: Tank, Controller
- NOT compatible with: Damage Dealer, All-Around

**Ultimate Missiles:**
- Extremely powerful, late-game
- Work on ALL ship types

### All-Around Ships Special Note

**All-Around ships can use ANY missile type!** This is their unique advantage - maximum versatility.

---

## Integration Guide

### Replacing Placeholder Names

All content uses placeholder names that are easy to find and replace. Here's how:

#### 1. Find Placeholder Data

**Prebuild Ships:**
```csharp
// In ExtendedProgressionData.cs
{ 3, new PrebuildShipUnlock("nova_class", "Nova Class", ShipClass.AllAround, "...") }
```

**Replace with:**
```csharp
{ 3, new PrebuildShipUnlock("fighter_mk1", "Fighter Mk-1", ShipClass.AllAround, "Fast interceptor") }
```

#### 2. Update All References

After changing IDs, update references in:
- `PlayerProfileData.cs` (unlocked lists)
- UI display code
- Ship spawning code
- Asset bundle references

#### 3. Bulk Replace Strategy

Use find-and-replace in your IDE:

**Example: Replace "Nova Class" ship**
1. Find: `"nova_class"`
2. Replace with: `"your_real_ship_id"`
3. Find: `"Nova Class"`
4. Replace with: `"Your Real Ship Name"`

#### 4. Testing Unlocks

Add debug commands to test:
```csharp
// Debug menu
ProgressionSystem.DEBUG_SetLevel(profile, 50); // Jump to level 50
```

### Adding New Content

**To add a new prebuild ship:**

1. Add to `ExtendedProgressionData.cs`:
```csharp
{ YOUR_LEVEL, new PrebuildShipUnlock("your_ship_id", "Your Ship Name", ShipClass.Tank, "Description") }
```

2. The unlock system handles it automatically!

**To add a new missile:**

1. Add to `MissileRetrofitSystem.cs`:
```csharp
{ YOUR_LEVEL, new MissileUnlockData("missile_id", "Missile Name", MissileType.Heavy, ShipClass.Tank, "Description") }
```

2. Check compatibility in `IsMissileCompatible()` method

### Battle Pass Seasonal Updates

**To create a new season:**

1. Update `BattlePassSystem.cs`:
```csharp
[SerializeField] private int currentSeason = 2; // Increment season
[SerializeField] private string seasonName = "Season 2: Stellar Conquest"; // New name
```

2. Update reward tables with new season rewards

3. Reset all player battle pass progress (server-side operation)

---

## File Structure

```
Assets/Online/
├── ProgressionSystem.cs           // Core progression logic & unlock levels
├── ExtendedProgressionData.cs     // All unlock data (40 ships, 16 bodies, etc.)
├── MissileRetrofitSystem.cs       // Missile unlocks & compatibility
├── BattlePassSystem.cs            // Seasonal battle pass (25 levels)
├── MatchHistoryManager.cs         // XP rewards & unlock application
├── PlayerProfileData.cs           // Player save data structure
├── AccountSystem.cs               // Account management
└── COMPLETE_PROGRESSION_GUIDE.md  // This file!
```

---

## Quick Reference Tables

### Ship Class Unlock Levels

| Ship Class | Unlock Level | Description |
|-----------|--------------|-------------|
| All-Around | 1 (starter) | Balanced, versatile |
| Tank | 5 | High armor, slow |
| Damage Dealer | 15 | High damage, fragile |
| Controller | 25 | Tactical, crowd control |

### Custom Slot Unlocks

| Slot | Unlock Level |
|------|--------------|
| Slot #1 | 1 (starter) |
| Slot #2 | 20 |
| Slot #3 | 40 |

### Feature Unlocks

| Feature | Unlock Level |
|---------|--------------|
| Achievements | 3 |
| Custom Match | 5 |
| Daily Quests | 5 |
| Leaderboards | 8 |
| Ranked PVP | 10 |
| Clans | 30 (future) |

### Prebuild Ship Counts

| Archetype | Total Ships | Levels Unlocked |
|-----------|-------------|----------------|
| All-Around | 11 | 1, 3, 7, 10, 20, 45, 59, 66, 78, 84, 100 |
| Tank | 8 | 6, 12, 18, 35, 57, 64, 72, 99 |
| Damage Dealer | 8 | 16, 19, 23, 30, 54, 62, 74, 82 |
| Controller | 9 | 26, 28, 32, 40, 50, 52, 68, 76, 86 |

### Ship Body Counts

| Archetype | Total Bodies | Quality Tiers |
|-----------|--------------|---------------|
| All-Around | 4 | Standard (21), Advanced (36), Master (66), Apex (71) |
| Tank | 4 | Reinforced (21), Elite (42), Ultimate (76) |
| Damage Dealer | 4 | Striker (21), Assault (48), Legendary (83) |
| Controller | 4 | Tactician (27), Tactical (56), Divine (91) |

---

## Summary

✅ **Account Progression**: 100 levels with 36 ships, 16 bodies, 30 passives, 20 actives
✅ **Battle Pass**: 25 levels with free + premium rewards (XP from daily quests only)
✅ **Missile System**: 17 missiles with class-based compatibility (starter missile included)
✅ **Action Points System**: 3 AP per turn (4 for Controller), missile fire costs 0 AP
✅ **Rank System**: 16 ranks from Ensign (starting) to Apex Legend
✅ **XP Sources**: Account XP & Ship XP from battles (simultaneous), Battle Pass XP from daily quests
✅ **Starter Ship**: StarSparrow with StarSparrow Class body
✅ **Placeholder Names**: Easy to find and replace
✅ **Skeleton System**: Ready to fill with real data

**All systems are integrated and ready to use!**
