# 🚢 Ship System Analysis & Balance Report

## 📊 Current System Overview

### **Ship Components**
- **Modular Design:** Engine, wings, fins detach on impact
- **Archetypes:** Tank, DamageDealer, AllAround, Controller
- **Leveling:** XP-based (max level 20)
- **Passives:** 1 per ship (unlocks at level 10), fixed to ship model
- **Active Perks:** 3 slots (Tier 1/2/3), unlock at specific levels
- **Move Types:** Normal, Precision, Warp (1 per ship, fixed)
- **Action Points:** 3 per turn (can use for perks OR moves)

---

## 🔢 XP & Leveling Formula Analysis

### **XP Formula:** `200 + 75 × L²`

| Level | XP for Next Level | Total XP Needed |
|-------|-------------------|-----------------|
| 1 → 2 | 275 | 0 |
| 2 → 3 | 500 | 275 |
| 5 → 6 | 2,075 | 3,050 |
| 9 → 10 | 6,275 | 16,900 |
| 10 → 11 | 7,700 | **23,175** ← Passive unlocks |
| 15 → 16 | 17,075 | 78,925 |
| 19 → 20 | 27,275 | 161,775 |
| **MAX** | - | **189,050** |

**Assessment:** ✅ **Good quadratic curve** - exponential growth feels rewarding
- Early levels are achievable
- Late levels require significant investment
- Level 10 (passive unlock) requires 23k XP - good gate

---

## ⚖️ Archetype Balance Analysis

### **Stat Scaling Formulas**

**Tank:**
```
Health:  baseHealth × (1 + 0.04 × Loffset)
Armor:   baseArmor + (4.0 × Loffset)
Damage:  baseDmg + (0.02 × Loffset)
```

**DamageDealer:**
```
Health:  baseHealth × (1 + 0.02 × Loffset)
Armor:   baseArmor + (1.0 × Loffset)
Damage:  baseDmg + (0.04 × Loffset)
```

**AllAround:**
```
Health:  baseHealth × (1 + 0.03 × Loffset)
Armor:   baseArmor + (3.0 × Loffset)
Damage:  baseDmg + (0.03 × Loffset)
```

**Controller:**
```
Health:  baseHealth × (1 + 0.02 × Loffset)
Armor:   baseArmor + (2.0 × Loffset)
Damage:  baseDmg + (0.025 × Loffset)
```

---

## 📈 Level 20 Comparison (baseHealth=10k, baseArmor=100, baseDmg=1.0)

| Archetype | Raw HP | Armor | Effective HP* | Dmg Mult | Missile Dmg (2500 payload) |
|-----------|--------|-------|---------------|----------|----------------------------|
| **Tank** | 17,600 | 176 | **25,344** | 1.38 | 3,450 |
| **DamageDealer** | 13,800 | 119 | 17,905 | **1.76** | **4,400** |
| **AllAround** | 15,700 | 157 | 21,862 | 1.57 | 3,925 |
| **Controller** | 13,800 | 138 | 18,561 | 1.48 | 3,688 |

*Effective HP = Raw HP / (1 - ArmorReduction), where ArmorReduction = Armor / (Armor + 400)

---

## ⚠️ BALANCE ISSUES IDENTIFIED

### 🔴 **CRITICAL: Tank is Overpowered**

**Problem:**
- Highest effective HP (25,344 - **41% more** than DamageDealer!)
- Still has decent damage (3,450 - only 22% less than DamageDealer)
- Tank has **both** survivability AND respectable damage

**Evidence:**
```
Tank vs DamageDealer at Level 20:
- Tank survives 41% more damage
- DamageDealer only deals 28% more damage
- Tank can also use Heavy missiles (high payload)
- Result: Tank dominates in most scenarios
```

**Recommendation:** Reduce Tank damage scaling from `0.02` → `0.015` or reduce health scaling

---

### 🟡 **MODERATE: Controller is Underpowered**

**Problem:**
- Very similar to AllAround but slightly worse in all stats
- Same HP as DamageDealer (13,800) but less damage
- No clear role identity

**Evidence:**
```
Controller vs AllAround at Level 20:
- 12% less health
- 12% less armor
- 6% less damage
- What's the tradeoff? Controller gets Light/Medium missiles, but so does AllAround
```

**Recommendation:**
- Buff Controller's utility (e.g., +1 movesAllowedPerTurn, or better passive)
- OR give Controller unique missile access (e.g., exclusive Controller-only missiles)

---

### 🟢 **GOOD: DamageDealer & AllAround are Balanced**

**DamageDealer:**
- Clear "glass cannon" identity
- High risk, high reward
- ✅ Balanced

**AllAround:**
- True middle ground
- No obvious weaknesses
- ✅ Balanced

---

## 🎮 Passive System Review

### **Current Passives:**

| Passive | Effect | Balance |
|---------|--------|---------|
| SniperMode | Double trajectory prediction steps | ✅ Strong but fair |
| Unmovable | Immune to push forces | ✅ Good |
| EnhancedRegeneration | HP regen when enemy's turn | ⚠️ Strong on Tanks |
| DamageResistance | 15% damage reduction | ⚠️ Very strong on Tanks |
| CriticalImmunity | No crit damage from core hits | ✅ Good |
| CriticalEnhancement | Deal 1.5× dmg on core hits | ✅ Good |
| DamageBoost | Damage increases over time | ⚠️ Strong late-game |
| LastChance | Survive at 1 HP once | ✅ Clutch potential |
| AdaptiveArmor | +10% armor per hit taken | 🔴 **OVERPOWERED on Tanks** |
| AdaptiveDamage | +10% dmg per hit landed | ✅ Good |
| PrecisionEngineering | No damage variation | ✅ Skill-based |
| CollisionAvoidance | Missiles avoid planets | ✅ Good |
| Lifesteal | 20% damage → healing | ⚠️ Strong |
| ReduceDamageFromHighSpeed | 20% less damage from fast missiles | ✅ Good |
| IncreaseDamageOnHighSpeed | 20% more damage on fast missiles | ✅ Good |

**Balance Concerns:**
1. **AdaptiveArmor on Tank** = Unkillable fortress (already has highest armor)
2. **EnhancedRegeneration on Tank** = Never dies
3. **DamageResistance stacks with armor** = 15% reduction AFTER armor calculation

**Recommendation:** Passives should be assigned based on archetype to prevent OP combinations

---

## 🎯 Active Perk System Review

**Current Setup:**
- 3 slots: Tier 1 (Level 5+), Tier 2 (Level 15+), Tier 3 (Level 20+)
- Cost: Action points (1-3 per perk)
- Usage: Tier 1 unlimited, Tier 2/3 once per turn

**Issues:**
1. ✅ No issues found - system is well-designed
2. ✅ Cost system prevents spam
3. ✅ Level gates feel rewarding

---

## 🔧 RECOMMENDED FIXES

### **1. Tank Rebalance (CRITICAL)**

**Option A: Reduce Damage Scaling**
```csharp
// OLD:
damageMultiplier = baseDamageMultiplier + (0.02f * Loffset);

// NEW:
damageMultiplier = baseDamageMultiplier + (0.015f * Loffset);
```
Result: Level 20 Tank does 3,188 damage instead of 3,450 (-7.6%)

**Option B: Reduce Health Scaling**
```csharp
// OLD:
maxHealth = baseHealth * (1f + 0.04f * Loffset);

// NEW:
maxHealth = baseHealth * (1f + 0.035f * Loffset);
```
Result: Level 20 Tank has 16,650 HP instead of 17,600 (-5.4%)

**Recommendation:** Use **Option A** - preserves Tank's identity as "high HP" but reduces damage output

---

### **2. Controller Buff**

**Option A: Unique Advantage**
```csharp
// Give Controller +1 action point
public int movesAllowedPerTurn = 4;  // vs 3 for others
```

**Option B: Better Damage Scaling**
```csharp
// OLD:
damageMultiplier = baseDamageMultiplier + (0.025f * Loffset);

// NEW:
damageMultiplier = baseDamageMultiplier + (0.035f * Loffset);
```

**Recommendation:** Use **Option A** - gives Controller a unique "control" playstyle with more actions

---

### **3. Passive Assignment Rules**

**Prevent OP Combinations:**

```
Tank-only passives:
- Unmovable
- CriticalImmunity
- (NO AdaptiveArmor, DamageResistance, or Regen - too strong)

DamageDealer-only passives:
- CriticalEnhancement
- AdaptiveDamage
- IncreaseDamageOnHighSpeed

Controller-only passives:
- CollisionAvoidance
- PrecisionEngineering
- SniperMode

Universal passives:
- LastChance
- Lifesteal (balanced risk/reward)
```

---

## 🎨 ShipPresetSO System Design

### **Why ScriptableObject System?**

**Current Problems:**
1. All formulas hardcoded in `UpdateStatsFromLevel()`
2. Must edit C# code to change balance
3. Can't A/B test different scaling curves
4. No way to create "premium" ships with custom formulas

**Solution:** Create `ShipPresetSO` (like `MissilePresetSO`)

---

### **Proposed ShipPresetSO Structure**

```csharp
[CreateAssetMenu(fileName = "NewShip", menuName = "GravityWars/Ship Preset")]
public class ShipPresetSO : ScriptableObject
{
    [Header("Ship Identity")]
    public string shipName = "Star Sparrow";
    public ShipArchetype archetype = ShipArchetype.AllAround;
    public GameObject visualPrefab;  // The modular ship model
    public Sprite icon;

    [Header("Base Stats (Level 1)")]
    public float baseHealth = 10000f;
    public float baseArmor = 100f;
    public float baseDamageMultiplier = 1.0f;

    [Header("Scaling Formulas")]
    [Tooltip("Health multiplier per level (e.g., 0.03 = +3% per level)")]
    public float healthScalingPerLevel = 0.03f;

    [Tooltip("Armor added per level (e.g., 3.0 = +3 armor per level)")]
    public float armorScalingPerLevel = 3.0f;

    [Tooltip("Damage multiplier added per level (e.g., 0.03 = +0.03 per level)")]
    public float damageScalingPerLevel = 0.03f;

    [Header("Action Points")]
    public int movesAllowedPerTurn = 3;

    [Header("Passive Ability (Unlocks at Level 10)")]
    public PassiveAbilitySO passive;

    [Header("Active Perks")]
    public ActivePerkSO tier1Perk;
    public ActivePerkSO tier2Perk;
    public ActivePerkSO tier3Perk;

    [Header("Move Type")]
    public MoveType moveType = MoveType.Normal;

    [Header("Missile Restrictions")]
    public bool canUseLightMissiles = true;
    public bool canUseMediumMissiles = true;
    public bool canUseHeavyMissiles = true;

    // Methods to apply stats to PlayerShip
    public void ApplyToShip(PlayerShip ship)
    {
        ship.shipModelName = shipName;
        ship.shipArchetype = archetype;
        ship.baseHealth = baseHealth;
        ship.armor = baseArmor;
        ship.damageMultiplier = baseDamageMultiplier;
        ship.movesAllowedPerTurn = movesAllowedPerTurn;

        // Apply scaling (will be read by UpdateStatsFromLevel)
        // ... etc
    }
}
```

**Benefits:**
1. ✅ Create ships in Unity Inspector (no code changes)
2. ✅ Easy balance tweaking
3. ✅ A/B test different formulas
4. ✅ Create "premium" ships with unique stats
5. ✅ Ships become data assets (can be modded!)

---

## 📝 Implementation Plan

### **Phase 1: Create ShipPresetSO**
1. Create `ShipPresetSO.cs`
2. Create `PassiveAbilitySO.cs` (for passive abilities)
3. Modify `PlayerShip.cs` to read from preset

### **Phase 2: Refactor Scaling System**
1. Move formulas from code → ScriptableObject
2. `UpdateStatsFromLevel()` reads from preset
3. Test existing ships still work

### **Phase 3: Balance Pass**
1. Create presets for all 4 archetypes
2. Apply recommended fixes (Tank nerf, Controller buff)
3. Test in-game balance

### **Phase 4: Create Ship Variants**
1. Create multiple ships per archetype
2. Different passive/perk combinations
3. Unique visuals per ship

---

## 🎮 Example Ship Presets

### **"Iron Fortress" (Tank)**
```
Base Health: 12000
Health Scaling: 0.035/level
Armor Scaling: 4.0/level
Damage Scaling: 0.015/level  ← NERFED
Moves: 3
Passive: Unmovable
Tier1: Explosive Missile
Tier2: Shield Boost
Tier3: Ramming Speed
Move: Normal
Missiles: Medium, Heavy only
```

### **"Glass Cannon" (DamageDealer)**
```
Base Health: 8000  ← LOWER
Health Scaling: 0.02/level
Armor Scaling: 1.0/level
Damage Scaling: 0.045/level  ← BUFFED
Moves: 3
Passive: CriticalEnhancement
Tier1: Multi-Missile
Tier2: Damage Boost
Tier3: Assassinate
Move: Normal
Missiles: All types
```

### **"Tactician" (Controller)**
```
Base Health: 10000
Health Scaling: 0.02/level
Armor Scaling: 2.0/level
Damage Scaling: 0.03/level  ← BUFFED
Moves: 4  ← UNIQUE ADVANTAGE
Passive: CollisionAvoidance
Tier1: Cluster Missile
Tier2: Precision Strike
Tier3: Tactical Warp
Move: Precision
Missiles: Light, Medium only
```

---

## 🚀 Next Steps

**What would you like to implement first?**

1. **Quick Fix:** Apply balance changes to existing code (Tank nerf, Controller buff)
2. **Full System:** Create ShipPresetSO system (takes longer but future-proof)
3. **Hybrid:** Fix balance now, implement SO system later

Let me know your preference and I'll start implementation!
