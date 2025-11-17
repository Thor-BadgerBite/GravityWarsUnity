# System Transition Analysis: Inspector → ScriptableObject System

## Critical Issues Identified

### 🚨 Issue #1: Ship Preset Never Actually Applied
**Problem**: In `PlayerShip.Start()`, we never call `shipPreset.ApplyToShip(this)`, so the preset is completely ignored!

**Current Code** (PlayerShip.cs:162-169):
```csharp
void Start()
{
    // Uses INSPECTOR values, not preset values!
    currentHealth    = baseHealth;     // ← Inspector value
    maxHealth        = baseHealth;     // ← Inspector value
    baseArmorValue   = armor;          // ← Inspector value
    baseDamageMultiplier = damageMultiplier;  // ← Inspector value
    // ...never calls shipPreset.ApplyToShip(this)!
}
```

**Fix Required**: Apply ship preset BEFORE using the values.

---

### 🚨 Issue #2: Passive System Broken
**Problem**: `PassiveAbilitySO.ApplyToShip()` resets ALL passives to false, then sets only ONE to true. But inspector checkboxes override everything anyway.

**Current Code** (PassiveAbilitySO.cs):
```csharp
public void ApplyToShip(PlayerShip ship)
{
    ResetAllPassives(ship);  // ← Sets ALL 15 passives to FALSE!

    switch (passiveType)
    {
        case PassiveType.SniperMode:
            ship.sniperMode = true;  // ← Sets only THIS passive to true
            break;
        // ...
    }
}
```

**Problem**: Ship can only have 1 passive, but we designed for multiple!

---

### 🚨 Issue #3: Active Perks Managed Separately
**Problem**: Active perks are loaded from `PerkManager` component's inspector fields (tier1Perk, tier2Perk, tier3Perk), NOT from ship preset!

**Current System**:
- PerkManager.cs lines 7-9 has inspector fields for perks
- ShipPresetSO also has fields for perks
- They're disconnected!

---

## PlayerShip Field Categorization

### ✅ **Category A: Move to ShipBodySO** (Ship-specific stats)
```csharp
[ALREADY MOVED]
✓ shipArchetype
✓ baseHealth
✓ armor (baseArmorValue)
✓ damageMultiplier (baseDamageMultiplier)
✓ actionPointsPerTurn (movesAllowedPerTurn)

[SHOULD MOVE]
→ shipModelName              // Name of the ship
→ rotationSpeed              // Ship handling characteristic
→ maxTiltAngle               // Ship handling characteristic
→ tiltSpeed                  // Ship handling characteristic
→ fineRotationSpeedMultiplier  // Ship handling characteristic
→ fineTiltSpeedMultiplier    // Ship handling characteristic
```

**Rationale**: Different ships should rotate differently (Tank slow, DamageDealer nimble, etc.)

---

### ✅ **Category B: Move to MoveTypeSO** (Move type-specific)
```csharp
[SHOULD MOVE]
→ minMoveSpeed       // Different for Normal/Precision/Warp
→ maxMoveSpeed       // Different for Normal/Precision/Warp
→ moveDeceleration   // Different for Normal/Precision/Warp
→ moveDuration       // Different for Normal/Precision/Warp

[WARP-SPECIFIC - SHOULD MOVE]
→ warpZoomDuration   // Warp animation timing
→ minScaleFactor     // Warp zoom effect
→ postWarpShakeTime  // Warp animation timing
→ postWarpShakeAngle // Warp animation effect
```

**Rationale**: Move behavior should be defined by move type, not hardcoded per ship.

---

### ✅ **Category C: Keep in PlayerShip** (Global/Runtime/Common settings)
```csharp
[GLOBAL GAME SETTINGS - Keep in PlayerShip]
✓ fireKey             // KeyCode.Space - same for all ships
✓ missileSpawnDistance  // 2f - same for all ships
✓ cooldownTime        // 1f - same for all ships
✓ predictionSteps     // 100 - same for all ships
✓ missilePrefab       // GameObject reference (should be in MissilePresetSO though)

[RUNTIME STATE - Keep in PlayerShip]
✓ shipXP              // Player progression (NOT ship config)
✓ shipLevel           // Player progression (NOT ship config)
✓ currentHealth       // Runtime state
✓ controlsEnabled     // Runtime state
✓ isDestroyed         // Runtime state
✓ shotsThisRound      // Runtime state
✓ movesRemainingThisRound  // Runtime state
✓ isLeftPlayer        // Player assignment
✓ playerName          // Player assignment
✓ score               // Runtime state
✓ isPassiveUnlocked   // Derived from shipLevel >= 10

[PERK/MISSILE FLAGS - Keep in PlayerShip]
✓ nextExplosiveEnabled, nextMultiEnabled, etc.  // Runtime perk effects
✓ equippedMissile     // Match-specific choice

[REFERENCES - Keep in PlayerShip]
✓ shipPreset          // Reference to ScriptableObject configuration
✓ playerUI            // UI reference
✓ ghostShipInstance   // Runtime ghost ship
✓ trajectoryLine      // Line renderer component
✓ rb                  // Rigidbody component
```

---

### ❌ **Category D: DELETE from PlayerShip** (Replaced by ScriptableObjects)
```csharp
[PASSIVE FLAGS - DELETE - Now in PassiveAbilitySO]
✗ precisionMove              // Now in PassiveAbilitySO
✗ warpMove                   // Now in PassiveAbilitySO
✗ sniperMode                 // Now in PassiveAbilitySO
✗ unmovable                  // Now in PassiveAbilitySO
✗ enhancedRegeneration       // Now in PassiveAbilitySO
✗ regenRate                  // Now in PassiveAbilitySO
✗ damageResistancePassive    // Now in PassiveAbilitySO
✗ damageResistancePercentage // Now in PassiveAbilitySO
✗ criticalImmunity           // Now in PassiveAbilitySO
✗ CriticalEnhancement        // Now in PassiveAbilitySO
✗ damageBoostPassive         // Now in PassiveAbilitySO
✗ hasLastChancePassive       // Now in PassiveAbilitySO
✗ adaptiveArmorPassive       // Now in PassiveAbilitySO
✗ adaptiveDamagePassive      // Now in PassiveAbilitySO
✗ precisionEngineering       // Now in PassiveAbilitySO
✗ collisionAvoidancePassive  // Now in PassiveAbilitySO
✗ lifestealPassive           // Now in PassiveAbilitySO
✗ lifestealPercent           // Now in PassiveAbilitySO
✗ reduceDamageFromHighSpeedMissiles     // Now in PassiveAbilitySO
✗ highSpeedDamageReductionPercent       // Now in PassiveAbilitySO
✗ increaseDamageOnHighSpeedMissiles     // Now in PassiveAbilitySO
✗ highSpeedDamageAmplifyPercent         // Now in PassiveAbilitySO

[MOVE TYPE FLAGS - DELETE - Now in MoveTypeSO]
✗ minLaunchVelocity         // Now in MissilePresetSO
✗ maxLaunchVelocity         // Now in MissilePresetSO
```

---

## Active Perk System Analysis

### Current PerkManager System (ALREADY WORKS!)
```csharp
// PerkManager.cs loads perks from inspector:
[Header("Perk Slots (Tier 1 / 2 / 3)")]
public ActivePerkSO tier1Perk;   // ← Inspector field
public ActivePerkSO tier2Perk;   // ← Inspector field
public ActivePerkSO tier3Perk;   // ← Inspector field
```

### ShipPresetSO Also Has Perk Fields (NOT USED!)
```csharp
// ShipPresetSO.cs:
[Header("Active Perks")]
public ActivePerkSO tier1Perk;   // ← ScriptableObject field (not used!)
public ActivePerkSO tier2Perk;
public ActivePerkSO tier3Perk;
```

### Decision Required:
**Option A**: Keep PerkManager loading from inspector (current system)
- Pro: Already works
- Con: Not part of ship preset system

**Option B**: Load perks from ShipPresetSO
- Pro: All ship config in one place
- Con: Need to refactor PerkManager to read from ship.shipPreset.tier1Perk

**Recommendation**: Option B - Apply perks from ship preset in PerkManager.Awake()

---

## XP/Level System (CORRECT AS-IS!)

XP and level are **player progression**, not ship configuration. They should stay in PlayerShip:
```csharp
✓ public float shipXP = 6250f;        // Player's progress with this ship
✓ public int shipLevel = 1;           // Derived from shipXP
✓ public float xpNeededForNextLevel;  // Calculated value
```

**Why?**:
- Ship preset defines base stats (what a level 1 Tank has)
- Player's XP determines current level (this player's Tank is level 15)
- Leveling formula (from ShipLevelingFormulaSO) calculates stats at current level

---

## Proposed Fix Implementation Plan

### Phase 1: Fix Ship Preset Application ⚠️ CRITICAL
1. Modify `PlayerShip.Start()` to apply ship preset FIRST
2. Remove inspector fields that are now in ship preset
3. Make passive system support multiple passives (not just 1)

### Phase 2: Expand ShipBodySO
1. Add rotation settings fields
2. Update ShipBodySO.cs with new fields
3. Update validation logic

### Phase 3: Expand MoveTypeSO
1. Add move speed/deceleration fields
2. Add warp-specific animation fields
3. Update MoveTypeSO.cs

### Phase 4: Clean Up PlayerShip
1. Remove all passive boolean flags
2. Add property accessors that read from ship preset
3. Maintain backward compatibility during transition

### Phase 5: Integrate PerkManager with Ship Preset
1. Load perks from shipPreset, not inspector
2. Update PerkManager.Awake() logic

---

## Backward Compatibility Strategy

To avoid breaking existing scenes, use this pattern:
```csharp
void Start()
{
    // NEW SYSTEM: Apply ship preset if assigned
    if (shipPreset != null)
    {
        shipPreset.ApplyToShip(this);
        Debug.Log($"Applied ship preset: {shipPreset.name}");
    }
    else
    {
        // OLD SYSTEM: Use inspector values (backward compatible)
        Debug.LogWarning($"{playerName}: No ship preset assigned, using inspector values");
        baseArmorValue = armor;
        baseDamageMultiplier = damageMultiplier;
    }

    // Rest of Start() logic...
}
```

---

## Example: What Final System Should Look Like

### Ship Configuration (All in ShipPresetSO)
```
Iron Fortress (ShipPresetSO)
├── Ship Body (ShipBodySO)
│   ├── Archetype: Tank
│   ├── Base Stats: HP=12000, Armor=120, Damage=1.0
│   ├── Rotation: rotationSpeed=30 (slower than others)
│   └── Missile Restrictions: No Light missiles
├── Leveling Formula (ShipLevelingFormulaSO)
│   └── Tank Formula (slower damage scaling)
├── Passives (PassiveAbilitySO[])
│   ├── [0] Adaptive Armor
│   └── [1] (empty - only 1 passive unlocked initially)
├── Active Perks (ActivePerkSO x3)
│   ├── Tier 1: Explosive Missile
│   ├── Tier 2: Emergency Shield
│   └── Tier 3: Artillery Barrage
├── Move Type (MoveTypeSO)
│   └── Normal Move (slower max speed for balance)
└── Starting Missile (MissilePresetSO)
    └── Heavy Missile
```

### Player-Specific Runtime State (Stays in PlayerShip)
```
Player 1's Ship Instance (PlayerShip component)
├── shipPreset = Iron Fortress (reference)
├── shipXP = 15,000 (player progression)
├── shipLevel = 12 (calculated from XP)
├── currentHealth = 8,432 (runtime state)
├── movesRemainingThisRound = 2 (runtime state)
├── isLeftPlayer = true
└── playerName = "Thor-BadgerBite"
```

---

## Next Steps

Would you like me to:
1. ✅ **Implement Phase 1** (Fix ship preset application - CRITICAL)
2. ⏳ **Implement Phase 2-3** (Expand ShipBodySO and MoveTypeSO)
3. ⏳ **Implement Phase 4** (Clean up PlayerShip passive flags)
4. ⏳ **Implement Phase 5** (Integrate PerkManager)

Or would you prefer a different approach?
