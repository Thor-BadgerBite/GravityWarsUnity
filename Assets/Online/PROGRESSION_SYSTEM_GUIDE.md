# Progression System Guide

Complete guide to the player progression, XP, rewards, and unlock systems in Gravity Wars.

---

## Table of Contents

1. [Overview](#overview)
2. [XP and Leveling](#xp-and-leveling)
3. [Level-Up Rewards](#level-up-rewards)
4. [Ship Classes](#ship-classes)
5. [Custom Ship Slots](#custom-ship-slots)
6. [Feature Unlocks](#feature-unlocks)
7. [Unlock Levels Reference](#unlock-levels-reference)
8. [Customization](#customization)

---

## Overview

The progression system is designed to **gradually unlock features** and keep players engaged. New players don't have access to everything at Level 1 - they must play matches, gain XP, and level up to unlock new content.

### Philosophy

- **Engagement**: Players work towards unlocks to stay motivated
- **Learning Curve**: Features unlock as players gain experience
- **Fairness**: Everyone follows the same progression path
- **Rewards**: Players feel rewarded for time invested

---

## XP and Leveling

### XP Gain (Per Match)

#### Base XP

**Ranked Matches:**
- **Win**: 200 XP (100 base × 2 win multiplier)
- **Loss**: 100 XP

**Casual Matches:**
- **Win**: 100 XP (50 base × 2 win multiplier)
- **Loss**: 50 XP

> **Note**: Players get XP even on loss (smaller amount) to maintain engagement!

#### Performance Bonuses

Players earn **additional XP** based on performance:

1. **Damage Dealt Bonus**
   - Formula: `+1 XP per 10 damage`
   - Example: 500 damage = +50 XP

2. **Accuracy Bonus**
   - >50% accuracy: **+25 XP**
   - >75% accuracy: **+50 XP** (additional +25)
   - Maximum: +50 XP

3. **Win Streak Bonus** (Ranked only)
   - Formula: `+10 XP per win in streak`
   - Maximum: **+100 XP** (10+ win streak)
   - Resets on loss

#### Example Calculation

**Ranked Win with Good Performance:**
```
Base XP:              200
Damage (600):         +60
Accuracy (80%):       +50
Win Streak (5):       +50
─────────────────────────
Total:                360 XP
```

**Casual Loss with Average Performance:**
```
Base XP:              50
Damage (300):         +30
Accuracy (45%):       +0
─────────────────────────
Total:                80 XP
```

### XP Requirements

XP required for each level increases **exponentially**:

```
Formula: 1000 × (1.15 ^ (level - 1))
```

**Examples:**
- Level 1 → 2: **1,000 XP**
- Level 2 → 3: **1,150 XP**
- Level 3 → 4: **1,322 XP**
- Level 5 → 6: **1,749 XP**
- Level 10 → 11: **3,518 XP**
- Level 20 → 21: **12,918 XP**
- Level 50 → 51: **677,847 XP**

### Estimated Time to Level

**Early Levels (1-10):**
- ~5-10 matches per level
- Takes ~1-2 hours

**Mid Levels (10-20):**
- ~15-25 matches per level
- Takes ~3-5 hours

**High Levels (20-40):**
- ~50-100 matches per level
- Takes ~10-20 hours

**Late Levels (40+):**
- ~200+ matches per level
- Takes many hours (prestige content)

---

## Level-Up Rewards

Every time a player levels up, they receive **automatic rewards**:

### Credits Reward

```
Formula: 100 + (level × 50) + milestone_bonus
```

- **Base**: 100 credits
- **Scaling**: +50 credits per level
- **Milestone Bonus**: +500 credits every 10 levels

**Examples:**
- Level 2: **150** credits
- Level 5: **350** credits
- Level 10: **600 + 500 = 1,100** credits (milestone!)
- Level 20: **1,100 + 500 = 1,600** credits (milestone!)

### Gems Reward

Gems (premium currency) are only given at **milestone levels** (every 5 levels):

```
Formula: (level / 5) × 10 gems
```

**Examples:**
- Level 5: **10 gems** 💎
- Level 10: **20 gems** 💎
- Level 15: **30 gems** 💎
- Level 20: **40 gems** 💎

### Unlocks

Special unlocks at specific levels (see sections below).

---

## Ship Classes

Ships are divided into **4 classes**, each with unique playstyles:

### 1. All-Around (Level 1)
- **Unlock Level**: 1 (starter class)
- **Stats**: Balanced
- **Playstyle**: Versatile, good for beginners
- **Description**: "Balanced stats. Good for beginners and versatile playstyles."

**Ships**:
- `starter_ship` (starter)
- More All-Around ships unlock via gameplay

### 2. Tank (Level 5)
- **Unlock Level**: 5 🛡️
- **Stats**: High armor, slow movement
- **Playstyle**: Absorbs damage, protects allies
- **Description**: "High armor, slow movement. Absorbs damage and protects allies."

**Example Stats**:
- Armor: 150%
- Speed: 70%
- Firepower: 90%

### 3. Damage Dealer (Level 15)
- **Unlock Level**: 15 💥
- **Stats**: High firepower, low armor
- **Playstyle**: Glass cannon, high risk/reward
- **Description**: "High firepower, low armor. Deals massive damage but fragile."

**Example Stats**:
- Armor: 60%
- Speed: 110%
- Firepower: 140%

### 4. Controller (Level 25)
- **Unlock Level**: 25 🎯
- **Stats**: Special abilities, crowd control
- **Playstyle**: Disrupt enemy strategies, utility
- **Description**: "Special abilities and crowd control. Disrupts enemy strategies."

**Example Abilities**:
- Slow enemy movement
- Disable enemy weapons temporarily
- Deploy shields or traps

---

## Custom Ship Slots

Players can create **custom ship loadouts** with different configurations (weapons, abilities, stats).

### Slot Unlock Progression

**Total Slots**: 3

1. **Slot 1**: Unlocked at **Level 1** (starter slot)
2. **Slot 2**: Unlocked at **Level 20** 🔓
3. **Slot 3**: Unlocked at **Level 40** 🔓

### What are Custom Slots?

Each slot allows players to:
- Choose a ship model
- Customize weapons loadout
- Select abilities/perks
- Adjust ship configuration
- Save for quick access

**Example Use Cases:**
- Slot 1: Ranked competitive loadout
- Slot 2: Casual fun loadout
- Slot 3: Experimental/testing loadout

---

## Feature Unlocks

Various game features unlock at different levels:

### Game Modes

**Ranked PVP** - **Level 10** 🏆
- Competitive matches with ELO ranking
- Win/loss affects rank
- Matchmaking by skill level
- Reason: Requires understanding of core mechanics first

**Custom Match** - **Level 5** ⚙️
- Create custom games with own rules
- Invite specific players
- Adjust match settings

### Progression Features

**Achievements** - **Level 3** 🏅
- Track accomplishments
- Earn special rewards
- Show off to other players

**Daily Quests** - **Level 5** 📜
- Complete daily challenges
- Earn bonus XP and credits
- Refreshes every 24 hours

**Leaderboards** - **Level 8** 📊
- View global rankings
- Compare stats with friends
- Climb the ladder

**Clans** - **Level 30** 🛡️
- Join or create clan
- Clan wars and events
- Clan chat and coordination
- (Future feature)

---

## Unlock Levels Reference

Quick reference table for all unlocks:

| Level | Unlocks |
|-------|---------|
| 1 | All-Around ships, Custom Slot #1 |
| 3 | Achievements |
| 5 | Tank ships, Custom Match, Daily Quests |
| 8 | Leaderboards |
| 10 | **Ranked PVP** |
| 15 | Damage Dealer ships |
| 20 | **Custom Slot #2** |
| 25 | Controller ships |
| 30 | Clans (future) |
| 40 | **Custom Slot #3** |

---

## Customization

### Adjusting Unlock Levels

To change unlock levels, edit **ProgressionSystem.cs**:

```csharp
// In ProgressionSystem.cs

// Game Modes
public const int RANKED_UNLOCK_LEVEL = 10;  // Change this!

// Custom Ship Slots
public const int CUSTOM_SLOT_2_UNLOCK_LEVEL = 20;  // Change this!
public const int CUSTOM_SLOT_3_UNLOCK_LEVEL = 40;  // Change this!

// Ship Classes
public const int CLASS_TANK_UNLOCK_LEVEL = 5;        // Change this!
public const int CLASS_DD_UNLOCK_LEVEL = 15;         // Change this!
public const int CLASS_CONTROLLER_UNLOCK_LEVEL = 25; // Change this!
```

### Adjusting XP Gains

To change XP rewards, edit **MatchHistoryManager.cs**:

```csharp
// In MatchHistoryManager.cs > CalculateRewards()

// Base rewards
int baseXP = result.isRanked ? 100 : 50;  // Change these!

// Win multiplier
if (isWinner)
{
    baseXP *= 2;  // Change win multiplier!
}

// Performance bonuses
performanceXP += playerStats.damageDealt / 10;  // Change damage scaling!
```

### Adjusting Level-Up Rewards

To change level-up rewards, edit **ProgressionSystem.cs**:

```csharp
// In ProgressionSystem.cs > CalculateCreditsReward()

int baseCredits = 100 + (level * 50);  // Change formula!
int bonusCredits = (level % 10 == 0) ? 500 : 0;  // Change milestone bonus!

// In CalculateGemsReward()
return (level / 5) * 10;  // Change gem formula!
```

---

## Implementation Details

### Code Structure

**ProgressionSystem.cs** (Static utility class)
- Defines all unlock levels
- Calculates level-up rewards
- Checks if features/classes are unlocked
- Provides helper methods

**MatchHistoryManager.cs** (Match results processing)
- Calculates XP and credits from match performance
- Applies level-up rewards
- Processes unlocks when leveling up

**MainMenuController.cs** (UI integration)
- Checks unlock status for buttons
- Shows locked messages with unlock levels
- Disables locked features

**PlayerProfileData.cs** (Data storage)
- Stores player level, XP, credits, gems
- Stores unlocked ships
- Stores custom loadouts

### Checking Unlocks

**Example: Check if Ranked is unlocked**
```csharp
bool canPlayRanked = ProgressionSystem.IsRankedUnlocked(playerProfile.level);

if (!canPlayRanked)
{
    int unlockLevel = ProgressionSystem.RANKED_UNLOCK_LEVEL;
    Debug.Log($"Ranked unlocks at level {unlockLevel}");
}
```

**Example: Check ship class**
```csharp
bool canUseTanks = ProgressionSystem.IsShipClassUnlocked(ShipClass.Tank, playerProfile.level);
```

**Example: Get custom slots count**
```csharp
int availableSlots = ProgressionSystem.GetUnlockedCustomSlots(playerProfile.level);
// Returns 1, 2, or 3
```

---

## Balancing Tips

### Unlock Timing

**Too Early** (everything unlocked by level 5):
- ❌ Players get overwhelmed
- ❌ No sense of progression
- ❌ Less engagement

**Too Late** (ranked at level 50):
- ❌ Players get bored
- ❌ Feels grindy
- ❌ Higher churn rate

**Balanced** (current system):
- ✅ Gradual feature introduction
- ✅ Always something to unlock
- ✅ Keeps players engaged
- ✅ Milestone moments (level 10 ranked, level 20 slot, etc.)

### XP Tuning

**Too Fast** (level 50 in 10 hours):
- ❌ Runs out of content quickly
- ❌ Less rewarding

**Too Slow** (level 10 takes 20 hours):
- ❌ Feels grindy
- ❌ Frustrating
- ❌ High churn

**Balanced** (current):
- ✅ Level 10 (ranked unlock) in ~2-3 hours
- ✅ Level 20 (2nd slot) in ~10-15 hours
- ✅ Steady progression feel

### Reward Scaling

**Credits** should scale with level:
- Early levels: Enough to buy basic items
- Mid levels: Can afford ship upgrades
- Late levels: Can afford premium items

**Gems** are scarce:
- Milestone rewards only
- Encourages in-app purchases (if monetizing)
- Or purely through gameplay progression

---

## Future Expansion Ideas

### Prestige System
- Reset to level 1 at level 50
- Keep ships/unlocks
- Get special prestige rewards
- Prestige level badge

### Season Pass
- Seasonal XP track with exclusive rewards
- Free track + Premium track
- Limited-time cosmetics
- Resets every season (3 months)

### Battle Pass
- Additional parallel progression
- Complete challenges for tiers
- Exclusive ships/cosmetics at high tiers

### Ship Mastery
- Per-ship progression
- Unlock ship-specific cosmetics
- Show dedication to favorite ships

---

## Testing Progression

### Debug Commands (Add these for testing)

```csharp
// In ProgressionSystem.cs or a debug menu

public static void DEBUG_AddXP(PlayerProfileData profile, int xp)
{
    profile.currentXP += xp;
    // Trigger level up check
}

public static void DEBUG_SetLevel(PlayerProfileData profile, int level)
{
    profile.level = level;
    profile.xpForNextLevel = CalculateXPForLevel(level + 1);
}

public static void DEBUG_UnlockAll(PlayerProfileData profile)
{
    profile.level = 50; // Max level
    // Unlocks everything
}
```

### Test Cases

1. **Level 1 → 10**: Verify ranked unlocks correctly
2. **Level 19 → 20**: Verify custom slot 2 unlocks and shows message
3. **Level 4 → 5**: Verify Tank ships become available
4. **Loss Scenario**: Verify players still gain XP on loss
5. **Performance Bonuses**: Test accuracy/damage bonuses calculate correctly

---

## Summary

The progression system creates **engagement through gradual unlocking**:

✅ **Players earn XP** from matches (more on win, some on loss)
✅ **Performance matters** (bonus XP for damage, accuracy, streaks)
✅ **Leveling up feels rewarding** (credits, gems, unlocks)
✅ **Features unlock gradually** (ranked at 10, slots at 20/40)
✅ **Ship classes provide variety** (Tank, DD, Controller at 5/15/25)
✅ **Customizable** (easy to adjust unlock levels and rewards)

Not everything is available at level 1 - players must **earn their progress**! 🚀
