# 🛠️ Custom Ship Builder - Complete Implementation Guide

**Purpose:** Build custom ships from unlocked components  
**Access:** Main Menu → Loadout → Custom Ship Builder button  
**Status:** Ready for Unity implementation

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [UX Flow (Step-by-Step)](#ux-flow-step-by-step)
3. [Screen Layout](#screen-layout)
4. [Validation Rules](#validation-rules)
5. [Stats Preview System](#stats-preview-system)
6. [Custom Slots Management](#custom-slots-management)
7. [Technical Implementation](#technical-implementation)

---

## 🎯 Overview

### What is Custom Ship Building?

Players can create personalized ships by combining **unlocked components**:
- **1 Ship Body** (defines archetype: Tank/DD/Controller/All-Around)
- **1 Passive Ability** (always-on effect, must be compatible with archetype)
- **3 Active Perks** (one from each tier: T1, T2, T3)
- **Custom Name** (player-defined, 30 char max)

### Key Features
- ✅ Ships start at Level 1, progress independently
- ✅ Stats preview during building (live updates)
- ✅ Archetype restrictions enforced automatically
- ✅ Limited custom slots (start with 1, unlock more)
- ✅ Delete ships to free slots (XP lost permanently)

### **CRITICAL:** Missiles NOT Selected Here
- Missiles chosen separately via **Missile Loadout** screen
- This keeps missile swapping flexible (no XP reset)

---

## 🚶 UX Flow (Step-by-Step)

### **Entry Point**
```
Main Menu → Bottom-Right Corner → Loadout Frame → [Custom Ship Builder Button]
```

### **Step 1: Check Custom Slots**

**If player has free slot:**
→ Proceed to Step 2

**If all slots full:**
```
┌─────────────────────────────────────────┐
│   ⚠️ NO FREE CUSTOM SLOTS             │
│                                         │
│   You have 2/2 custom slots filled.    │
│                                         │
│   OPTIONS:                              │
│   [Delete Existing Ship]                │
│   [Store Ship (7-day lockout)]          │
│   [Unlock More Slots - 500 Gems]        │
│   [Cancel]                              │
└─────────────────────────────────────────┘
```

**Delete Option:**
- Shows list of custom ships with levels
- Warning: "Deleting [Ship Name] (Level 8) will lose all progress. Continue?"
- Confirm → Ship deleted, slot freed

**Store Option (Alternative):**
- Ship stored for 7 days (can't use during lockout)
- After 7 days, can restore to active slot
- Slot freed immediately

---

### **Step 2: Choose Ship Body**

**Screen Layout:**
```
┌─────────────────────────────────────────────────────────┐
│  CUSTOM SHIP BUILDER - Step 1 of 6                      │
│  Choose Ship Body                                        │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  UNLOCKED BODIES:                                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │ 🛡️ TANK  │  │ âš"DD     │  │ 🎯 CTRL  │             │
│  │ Iron     │  │ Viper    │  │ Phantom  │             │
│  │ Fortress │  │ MK-III   │  │ Recon    │             │
│  │          │  │          │  │          │             │
│  │ [SELECT] │  │ [SELECT] │  │ [SELECT] │             │
│  └──────────┘  └──────────┘  └──────────┘             │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │ ⚖️ ALL   │  │ 🛡️ TANK  │  │ âš" DD     │             │
│  │ Balanced │  │ Heavy    │  │ Striker  │             │
│  │ Cruiser  │  │ Dread.   │  │ Hawk     │             │
│  │ [LOCKED] │  │ [LOCKED] │  │ [LOCKED] │             │
│  └──────────┘  └──────────┘  └──────────┘             │
│                                                          │
│  [Back to Main Menu]                                    │
└─────────────────────────────────────────────────────────┘
```

**On Click Ship Card:**
- Highlight selected body
- Show archetype badge (Tank/DD/Controller/All-Around)
- Display base stats:
  - HP: 11,000
  - Armor: 100
  - Damage: 1.0×
  - Action Points: 3
- **[Next Step]** button appears

---

### **Step 3: Choose Passive Ability**

**Screen Layout:**
```
┌─────────────────────────────────────────────────────────┐
│  CUSTOM SHIP BUILDER - Step 2 of 6                      │
│  Choose Passive Ability                                  │
│  Selected Body: Iron Fortress (Tank)                     │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  COMPATIBLE PASSIVES (Tank Only):                        │
│  ┌────────────────┐  ┌────────────────┐               │
│  │ 🛡️ DAMAGE      │  │ ❤️ LAST         │               │
│  │ RESISTANCE     │  │ CHANCE         │               │
│  │                │  │                │               │
│  │ Take 15% less  │  │ Survive at 1HP │               │
│  │ damage         │  │ once per round │               │
│  │                │  │                │               │
│  │ [SELECT]       │  │ [SELECT]       │               │
│  └────────────────┘  └────────────────┘               │
│                                                          │
│  INCOMPATIBLE (Locked for Tank):                         │
│  ┌────────────────┐  ┌────────────────┐               │
│  │ 🩹 REGEN       │  │ ðŸ'Š LIFESTEAL  │               │
│  │ [LOCKED]       │  │ [LOCKED]       │               │
│  │ (DD only)      │  │ (Not Tank)     │               │
│  └────────────────┘  └────────────────┘               │
│                                                          │
│  [Back] [Next Step]                                     │
└─────────────────────────────────────────────────────────┘
```

**Filtering:**
- Only show passives compatible with chosen archetype
- Grayed-out incompatible passives with reason ("Tank can't use Regen")

---

### **Step 4: Choose Tier 1 Perk**

**Screen Layout:**
```
┌─────────────────────────────────────────────────────────┐
│  CUSTOM SHIP BUILDER - Step 3 of 6                      │
│  Choose Tier 1 Active Perk                               │
│  Selected Body: Iron Fortress (Tank)                     │
│  Selected Passive: Damage Resistance                     │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  TIER 1 PERKS (Cost: 1 AP):                             │
│  ┌────────────────┐  ┌────────────────┐               │
│  │ 🚀 MULTI       │  │ 💥 EXPLOSIVE   │               │
│  │ MISSILE        │  │ MISSILE        │               │
│  │                │  │                │               │
│  │ Fire 3 missiles│  │ Area blast     │               │
│  │ in spread      │  │ 12-unit radius │               │
│  │                │  │                │               │
│  │ [SELECT]       │  │ [SELECT]       │               │
│  └────────────────┘  └────────────────┘               │
│                                                          │
│  ┌────────────────┐  ┌────────────────┐               │
│  │ âš¡ OVERCHARGED│  │ 🔒 BOOST JETS  │               │
│  │ CANNON         │  │ [LOCKED]       │               │
│  │ +50% damage    │  │ Unlock Lv 12   │               │
│  │ [SELECT]       │  └────────────────┘               │
│  └────────────────┘                                     │
│                                                          │
│  [Back] [Next Step]                                     │
└─────────────────────────────────────────────────────────┘
```

**Filtering:**
- Only show unlocked Tier 1 perks
- Show locked perks with unlock requirement

---

### **Step 5: Choose Tier 2 Perk** (Same as Step 4, T2 perks)
### **Step 6: Choose Tier 3 Perk** (Same as Step 4, T3 perks)

---

### **Step 7: Name Your Ship**

**Screen Layout:**
```
┌─────────────────────────────────────────────────────────┐
│  CUSTOM SHIP BUILDER - Step 6 of 6                      │
│  Name Your Ship                                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  SHIP CONFIGURATION:                                     │
│  Body: Iron Fortress (Tank)                              │
│  Passive: Damage Resistance                              │
│  Tier 1 Perk: Multi Missile                              │
│  Tier 2 Perk: Cluster Missile                            │
│  Tier 3 Perk: Missile Barrage                            │
│                                                          │
│  ENTER SHIP NAME:                                        │
│  ┌───────────────────────────────────────┐             │
│  │ [____________________________]         │             │
│  │ (30 characters max)                    │             │
│  └───────────────────────────────────────┘             │
│                                                          │
│  SHIP STATS AT LEVEL 1:                                  │
│  HP: 11,000 | Armor: 100 | Damage: 1.0× | AP: 3         │
│                                                          │
│  [Back] [Create Ship]                                   │
└─────────────────────────────────────────────────────────┘
```

**Validation:**
- Name cannot be empty
- Max 30 characters
- Must be unique to player's ships (check existing names)
- Profanity filter (optional, future feature)

**On [Create Ship]:**
- Validate all components
- Create `CustomShipLoadout` entry in `PlayerAccountData`
- Ship starts at Level 1, 0 XP
- Success message: "Ship '[Name]' created! Ready for battle!"
- Return to Main Menu (ship now appears in Ships Garage)

---

## 🎨 Screen Layout (Full Builder)

### **Layout Structure**

```
┌────────────────────────────────────────────────────────────────┐
│ TOP BAR: Step Progress Indicator                               │
│ [1.Body] → [2.Passive] → [3.T1] → [4.T2] → [5.T3] → [6.Name]  │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│ LEFT PANEL: Component Grid                                      │
│ ┌───────────────────────────────────────┐                      │
│ │ Component Cards (3-4 per row)         │                      │
│ │ - Icon, Name, Description             │                      │
│ │ - [SELECT] or [LOCKED] button         │                      │
│ │ - Archetype restrictions visible      │                      │
│ └───────────────────────────────────────┘                      │
│                                                                 │
│ RIGHT PANEL: Live Stats Preview                                 │
│ ┌───────────────────────────────────────┐                      │
│ │ SHIP PREVIEW                          │                      │
│ │                                       │                      │
│ │ Body: Iron Fortress (Tank)            │                      │
│ │ Passive: Damage Resistance            │                      │
│ │ T1 Perk: Multi Missile                │                      │
│ │ T2 Perk: (Not selected)               │                      │
│ │ T3 Perk: (Not selected)               │                      │
│ │                                       │                      │
│ │ STATS AT LEVEL 1:                     │                      │
│ │ ❤️ HP: 11,000                         │                      │
│ │ 🛡️ Armor: 100                          │                      │
│ │ âš" Damage: 1.0×                       │                      │
│ │ âš¡ Action Points: 3                   │                      │
│ │                                       │                      │
│ │ [3D Model Preview - Future]           │                      │
│ └───────────────────────────────────────┘                      │
│                                                                 │
├────────────────────────────────────────────────────────────────┤
│ BOTTOM BAR: Navigation                                          │
│ [Back to Previous Step]  [Cancel]  [Next Step / Create Ship]   │
└────────────────────────────────────────────────────────────────┘
```

---

## ✅ Validation Rules

### **Archetype Compatibility**

```csharp
// From CustomShipBuilder.cs
public static bool ValidateShipBuild(
    PlayerAccountData profile,
    string bodyId,
    string passiveId,
    string tier1ActiveId,
    string tier2ActiveId,
    string tier3ActiveId)
{
    // 1. Check body is unlocked
    if (!profile.unlockedShipBodies.Contains(bodyId))
        return false;
    
    // 2. Get body archetype
    ShipArchetype archetype = GetBodyArchetype(bodyId);
    
    // 3. Check passive compatibility
    PassiveAbilitySO passive = GetPassive(passiveId);
    if (!passive.CanBeUsedBy(archetype))
        return false;
    
    // 4. Check all 3 active perks
    // - Must be unlocked
    // - Must be correct tier (T1/T2/T3)
    // - Must be compatible with archetype
    
    // 5. Check custom slot availability
    if (profile.customShipLoadouts.Count >= GetMaxCustomSlots(profile.level))
        return false;
    
    return true;
}
```

### **Error Messages**

| Error | Message |
|-------|---------|
| Body locked | "Ship body 'Iron Fortress' is not yet unlocked. Unlock at Level 5." |
| Passive incompatible | "Passive 'Regeneration' cannot be used by Tank archetype." |
| Perk wrong tier | "Selected perk is Tier 2, but Tier 1 is required." |
| Perk locked | "Perk 'Cluster Missile' unlocks at Level 12." |
| No free slots | "No available custom slots. Delete a ship or unlock more slots." |
| Name taken | "Ship name already exists. Choose a different name." |
| Name empty | "Ship name cannot be empty." |
| Name too long | "Ship name must be 30 characters or less." |

---

## 📊 Stats Preview System

### **Live Stats Calculation**

As player selects components, stats update in real-time:

```csharp
// Calculate ship stats based on body + level
public static ShipStats CalculateShipStats(ShipBodySO body, int level)
{
    // Get base stats from body
    float baseHP = body.baseHealth;
    float baseArmor = body.baseArmor;
    float baseDamage = body.baseDamageMultiplier;
    int actionPoints = body.actionPoints;
    
    // Apply level scaling (from ShipLevelingFormulaSO)
    ShipLevelingFormulaSO formula = GetFormulaForArchetype(body.archetype);
    
    float levelOffset = level - 1;
    float scaledHP = baseHP * (1.0f + formula.healthMultPerLevel * levelOffset);
    float scaledArmor = baseArmor + (formula.armorAddPerLevel * levelOffset);
    float scaledDamage = baseDamage + (formula.damageAddPerLevel * levelOffset);
    
    return new ShipStats {
        hp = scaledHP,
        armor = scaledArmor,
        damageMultiplier = scaledDamage,
        actionPoints = actionPoints
    };
}
```

### **Preview Display**

**Right Panel shows:**
- Selected components (name + icon)
- Stats at Level 1 (calculated)
- **Future:** 3D model preview rotating

**Example:**
```
STATS AT LEVEL 1:
❤️ HP: 11,000
🛡️ Armor: 100 (20% damage reduction)
âš" Damage: 1.0× (base: 2,500 per missile)
âš¡ Action Points: 3
```

---

## 🔓 Custom Slots Management

### **Unlocking Slots**

| Account Level | Custom Slots |
|---------------|--------------|
| 1-9 | 1 slot |
| 10-24 | 2 slots |
| 25-49 | 3 slots |
| 50-74 | 4 slots |
| 75+ | 5 slots |

**Alternative:** Purchase extra slot with 500 Gems (skip level requirement)

### **Deleting Ships**

**Process:**
1. Builder detects no free slots
2. Show "Manage Custom Ships" popup
3. List all custom ships with:
   - Name
   - Level
   - Archetype icon
   - [Delete] button
4. Click [Delete] → Warning popup:
   ```
   ⚠️ DELETE SHIP?
   
   Deleting "Iron Fortress" (Level 8) will:
   - Permanently remove this ship
   - Lose all XP and progress
   - Free 1 custom slot
   
   This action cannot be undone!
   
   [Cancel] [Confirm Delete]
   ```
5. Confirm → Ship deleted, slot freed

### **Storing Ships (Alternative)**

**7-Day Lockout System:**
1. Player chooses "Store Ship"
2. Ship moved to "Storage" (can't use)
3. Slot freed immediately
4. After 7 days, ship can be restored to active slot
5. **Purpose:** Prevents rapid slot cycling, encourages commitment

---

## 🛠️ Technical Implementation

### **Data Structure**

```csharp
// From PlayerAccountData.cs
[System.Serializable]
public class CustomShipLoadout
{
    public string loadoutID;          // Unique ID (Guid)
    public string loadoutName;        // Player-defined name
    
    // Core Components
    public string shipBodyName;       // ShipBodySO.name
    public string moveTypeName;       // NOT selected here! Set by body archetype
    public string equippedMissileName; // NOT selected here! Set via Missile Loadout screen
    
    // Abilities
    public string tier1PerkName;      // ActivePerkSO.name
    public string tier2PerkName;
    public string tier3PerkName;
    public List<string> passiveNames; // PassiveAbilitySO.name (1 passive)
    
    // Cosmetics (optional, future)
    public string skinID;
    public string colorSchemeID;
    public string decalID;
    
    // Progression Key (excludes missile!)
    public string GetProgressionKey()
    {
        return $"{shipBodyName}|{tier1PerkName}|{tier2PerkName}|{tier3PerkName}|{moveTypeName}|{passiveNames[0]}";
    }
}
```

### **Creation Flow (Code)**

```csharp
// From CustomShipBuilder.cs
public static CustomShipLoadout CreateCustomShip(
    PlayerAccountData profile,
    string bodyId,
    string passiveId,
    string tier1ActiveId,
    string tier2ActiveId,
    string tier3ActiveId,
    string customName)
{
    // 1. Validate build
    var validation = ValidateShipBuild(profile, bodyId, passiveId, 
                                       tier1ActiveId, tier2ActiveId, tier3ActiveId);
    if (!validation.isValid)
    {
        Debug.LogError($"Build validation failed: {validation.errors}");
        return null;
    }
    
    // 2. Validate name
    if (string.IsNullOrWhiteSpace(customName) || customName.Length > 30)
    {
        Debug.LogError("Invalid ship name");
        return null;
    }
    
    // 3. Create loadout
    var loadout = new CustomShipLoadout
    {
        loadoutID = Guid.NewGuid().ToString(),
        loadoutName = customName,
        shipBodyName = bodyId,
        passiveNames = new List<string> { passiveId },
        tier1PerkName = tier1ActiveId,
        tier2PerkName = tier2ActiveId,
        tier3PerkName = tier3ActiveId,
        // Move type set by body archetype (not selectable)
        moveTypeName = GetDefaultMoveType(bodyId),
        // Missile equipped separately (default: Medium)
        equippedMissileName = "Medium_Missile_Default"
    };
    
    // 4. Add to profile
    profile.customShipLoadouts.Add(loadout);
    
    // 5. Save
    SaveSystem.SavePlayerData(profile);
    
    Debug.Log($"Created custom ship '{customName}' (ID: {loadout.loadoutID})");
    return loadout;
}
```

### **Integration with Ships Garage**

After creation:
1. New ship appears in Ships Garage grid
2. Card shows: Custom icon, name, "Level 1", archetype badge
3. Can be equipped like any other ship
4. Gains XP from matches
5. Levels up 1→20 like prebuild ships

---

## 🎯 UI Assets Needed

### **Buttons**
- `CustomBuilderButton_normal.png`
- `CustomBuilderButton_hover.png`
- `CustomBuilderButton_pressed.png`
- `CustomBuilderButton_inactive.png`

### **Component Cards**
- `ComponentCard_background.png`
- `ComponentCard_selected.png` (highlighted border)
- `ComponentCard_locked.png` (grayed out)

### **Icons**
- `Icon_Tank.png`, `Icon_DamageDealer.png`, `Icon_Controller.png`, `Icon_AllAround.png`
- `Icon_Tier1.png`, `Icon_Tier2.png`, `Icon_Tier3.png`
- `Icon_Passive.png`

### **Progress Indicator**
- `StepIndicator_complete.png` (green checkmark)
- `StepIndicator_current.png` (orange highlight)
- `StepIndicator_future.png` (gray)

---

**END OF DOCUMENT**

Custom Ship Builder fully documented! Ready for Unity implementation! 🚀
