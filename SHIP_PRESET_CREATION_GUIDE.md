# 🚢 Ship Preset System - Creation Guide

## 📋 System Overview

The new Ship Preset System uses **ScriptableObjects** to define ships in a modular, data-driven way. This allows you to:

✅ Create ships without touching code
✅ Easily balance and tweak stats
✅ Enforce archetype restrictions automatically
✅ Prepare for multiplayer progression system
✅ Enable community modding (future)

---

## 🏗️ Architecture

```
ShipPresetSO (Complete Ship)
├── ShipBodySO (Base stats + archetype)
├── ShipLevelingFormulaSO (Scaling formulas)
├── PassiveAbilitySO[] (Passive abilities)
├── ActivePerkSO × 3 (Tier 1/2/3 perks)
├── MoveTypeSO (Normal/Precision/Warp)
└── MissilePresetSO (Default missile - optional)
```

---

## 📝 Step-by-Step Creation

### **Step 1: Create Leveling Formulas (One-Time Setup)**

You only need **4 leveling formulas** - one per archetype.

1. Right-click in Project → **Create → GravityWars → Ship System → Leveling Formula**
2. Name it: `TankLevelingFormula`
3. Configure values:

```
Archetype: Tank
Health Scaling Per Level: 0.04      (Tank gets +4% HP per level)
Armor Scaling Per Level: 4.0        (Tank gets +4 armor per level)
Damage Scaling Per Level: 0.015     (Tank gets +0.015 dmg per level - NERFED for balance!)
```

4. Repeat for `DamageDealer`, `AllAround`, `Controller` with their respective values

**Recommended Values:**

| Archetype | Health Scaling | Armor Scaling | Damage Scaling |
|-----------|----------------|---------------|----------------|
| **Tank** | 0.04 | 4.0 | **0.015** ← NERFED! |
| **DamageDealer** | 0.02 | 1.0 | 0.04 |
| **AllAround** | 0.03 | 3.0 | 0.03 |
| **Controller** | 0.02 | 2.0 | **0.03** ← BUFFED! |

---

### **Step 2: Create Ship Bodies**

1. Right-click → **Create → GravityWars → Ship System → Ship Body**
2. Name it: `HeavyChassisMK1`
3. Configure:

```
Body Name: Heavy Chassis MK-I
Archetype: Tank
Visual Prefab: (Assign your 3D model)
Icon: (Assign sprite)

Base Stats (Level 1):
├─ Base Health: 12000          (Tank should be 11000+)
├─ Base Armor: 100
├─ Base Damage Multiplier: 1.0
└─ Action Points Per Turn: 3   (Controller gets 4!)

Missile Restrictions:
├─ Can Use Light: ☐ (Tank can't use Light!)
├─ Can Use Medium: ☑
└─ Can Use Heavy: ☑

Description: "A heavily armored chassis designed for frontline combat."
```

**Validation will auto-correct mistakes!**
- Tank with Light missiles → auto-disabled
- Controller without 4 action points → warning shown

---

### **Step 3: Create Passive Abilities**

1. Right-click → **Create → GravityWars → Ship System → Passive Ability**
2. Name it: `Passive_Unmovable`
3. Configure:

```
Passive Name: Unmovable
Passive Type: Unmovable (select from dropdown)
Icon: (Assign sprite)
Unlock Level: 10

Archetype Restrictions:
├─ Allow Tank: ☑
├─ Allow DamageDealer: ☐
├─ Allow Controller: ☐
└─ Allow AllAround: ☐

Description: "Ship is immune to push forces from explosions."
```

**Balance Warnings:**
- AdaptiveArmor + Tank → Warning: OVERPOWERED!
- EnhancedRegen + Tank → Warning: Very strong!

---

### **Step 4: Create Move Types (One-Time Setup)**

You only need **3 move types** total:

**Normal Move:**
```
Move Type Name: Normal Move
Category: Normal
Allow Tank: ☑
Allow DamageDealer: ☑
Allow Controller: ☑
Allow AllAround: ☑
```

**Precision Move:**
```
Move Type Name: Precision Move
Category: Precision
Allow Tank: ☐   ← Tank too bulky!
Allow DamageDealer: ☑
Allow Controller: ☑
Allow AllAround: ☑
```

**Warp Move:**
```
Move Type Name: Warp Move
Category: Warp
Allow Tank: ☐
Allow DamageDealer: ☐
Allow Controller: ☑   ← EXCLUSIVE to Controller!
Allow AllAround: ☐
```

---

### **Step 5: Configure Active Perks**

Active perks already exist in your project. Just add archetype restrictions:

1. Find existing perk (e.g., `ExplosiveMissile`)
2. Enable **Archetype Restrictions** section:

```
Allow Tank: ☑
Allow DamageDealer: ☑
Allow Controller: ☐     (If it doesn't fit Controller playstyle)
Allow AllAround: ☑
```

---

### **Step 6: Create Complete Ship Preset**

1. Right-click → **Create → GravityWars → Ship System → Ship Preset**
2. Name it: `Ship_IronFortress`
3. Assemble components:

```
Ship Name: Iron Fortress
Ship Icon: (Assign sprite)
Description: "A heavily armored tank that excels in prolonged combat."

Core Components:
├─ Ship Body: HeavyChassisMK1
└─ Leveling Formula: TankLevelingFormula

Passive Abilities:
└─ Passives[0]: Passive_Unmovable

Active Perks:
├─ Tier 1 Perk: ExplosiveMissile
├─ Tier 2 Perk: ShieldBoost
└─ Tier 3 Perk: RammingSpeed

Movement:
└─ Move Type: NormalMove

Starting Missile (Optional):
└─ Default Missile: Sledgehammer_Heavy

Unlock Requirements:
├─ Required Account Level: 5
└─ Is Premium Ship: ☐
```

4. **Validation Status** field shows errors/warnings!

---

## ✅ Validation System

The system automatically checks for:

**Errors (Won't Work):**
- ❌ Passive not compatible with archetype
- ❌ Perk not compatible with archetype
- ❌ Move type not compatible with archetype
- ❌ Default missile incompatible with ship body

**Warnings (May Be Unbalanced):**
- ⚠️ Tank + AdaptiveArmor = OVERPOWERED!
- ⚠️ Controller without 4 action points
- ⚠️ Warp move on non-Controller ship

---

## 🎮 Using Ship Presets In-Game

### **Option A: Assign to PlayerShip in Inspector**

1. Select `Player1Ship` GameObject
2. Find `PlayerShip` component
3. Under **Ship Preset System**:
   - Assign `Ship_IronFortress` to **Ship Preset** field
4. Stats will auto-apply from preset!

### **Option B: Keep Using Old System (Backward Compatible)**

1. Don't assign a Ship Preset
2. Ship uses hardcoded formulas (with BALANCE FIXES applied!)
3. No errors, everything works as before

---

## 🔧 Example Ship Presets

### **Iron Fortress (Tank)**

```
Ship Body: HeavyChassisMK1
- Base HP: 12000
- Base Armor: 100
- Action Points: 3

Leveling: TankLevelingFormula
- Health: +4%/level
- Armor: +4/level
- Damage: +0.015/level (NERFED!)

Passive: Unmovable
Perks: ExplosiveMissile, ShieldBoost, RammingSpeed
Move: Normal
Missile: Sledgehammer (Heavy)
```

**Level 20 Stats:**
- HP: 21,120 (Effective: 30,379)
- Armor: 176
- Damage: 1.285
- Role: Unkillable fortress

---

### **Glass Cannon (DamageDealer)**

```
Ship Body: LightChassisMK1
- Base HP: 9000
- Base Armor: 100
- Action Points: 3

Leveling: DamageDealerLevelingFormula
- Health: +2%/level
- Armor: +1/level
- Damage: +0.04/level

Passive: CriticalEnhancement
Perks: MultiMissile, DamageBoost, Assassinate
Move: Precision
Missile: Dart (Light)
```

**Level 20 Stats:**
- HP: 12,420 (Effective: 16,116)
- Armor: 119
- Damage: 1.76
- Role: High-risk, high-reward sniper

---

### **Tactician (Controller)**

```
Ship Body: ControlChassisMK1
- Base HP: 10000
- Base Armor: 100
- Action Points: 4   ← UNIQUE ADVANTAGE!

Leveling: ControllerLevelingFormula
- Health: +2%/level
- Armor: +2/level
- Damage: +0.03/level (BUFFED!)

Passive: CollisionAvoidance
Perks: ClusterMissile, PrecisionStrike, TacticalWarp
Move: Warp   ← EXCLUSIVE!
Missile: Standard (Medium)
```

**Level 20 Stats:**
- HP: 13,800 (Effective: 18,561)
- Armor: 138
- Damage: 1.57
- Role: Tactical mastermind with extra actions

---

## 📊 Balance Fixes Applied

### **Tank Nerf:**
```diff
- Damage Scaling: 0.02/level
+ Damage Scaling: 0.015/level

Result at Level 20:
- Old: 3,450 missile damage
+ New: 3,188 missile damage (-7.6%)
```

### **Controller Buff:**
```diff
- Damage Scaling: 0.025/level
+ Damage Scaling: 0.03/level

- Action Points: 3
+ Action Points: 4

Result at Level 20:
- Old: 3,125 missile damage, 3 actions
+ New: 3,750 missile damage (+20%), 4 actions (+33% more actions!)
```

---

## 🚀 Next Steps

### **Phase 1 (Current):**
- ✅ Create 4 leveling formulas
- ✅ Create 4 ship bodies (one per archetype)
- ✅ Create passives with restrictions
- ✅ Create 3 move types
- ✅ Create 4 complete ship presets

### **Phase 2 (Future):**
- Create 10+ ship variants per archetype
- Create "premium" ships with unique stats
- Implement missile loadout selection UI
- Create stat preview tool

### **Phase 3 (Multiplayer):**
- Account XP tracking system
- Unlock manager (track unlocked parts)
- Battle pass framework (free + premium)
- Seasonal content system
- Custom ship builder UI

---

## 🐛 Troubleshooting

**Q: Validation says "Passive cannot be used by Tank"**
A: Check passive's **Archetype Restrictions** - make sure `Allow Tank` is checked

**Q: Ship stats don't change in-game**
A: Make sure you assigned the Ship Preset to PlayerShip's `Ship Preset` field

**Q: Old ships still work but use old formulas**
A: That's intentional! Ships without presets use hardcoded formulas (with balance fixes)

**Q: How do I create a custom Tank with different stats?**
A: Create a new ShipBodySO with different base values, but keep archetype = Tank

**Q: Can I have 2 passives on one ship?**
A: System supports it (passives array), but currently only 1 slot is enabled. Future feature!

---

## 📝 Quick Reference

### **File Locations:**
```
Assets/
├── Ship System/
│   ├── ShipBodySO.cs
│   ├── ShipLevelingFormulaSO.cs
│   ├── PassiveAbilitySO.cs
│   ├── MoveTypeSO.cs
│   ├── ShipPresetSO.cs
│   └── ArchetypeRestrictionChecker.cs
└── +Active Perks+/
    └── ActivePerkSO.cs (updated with restrictions)
```

### **Menu Paths:**
```
Create → GravityWars → Ship System →
├── Ship Preset
├── Ship Body
├── Leveling Formula
├── Passive Ability
└── Move Type
```

---

Generated: 2025
