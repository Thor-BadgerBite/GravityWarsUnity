# 🚀 Missile Preset Creation Guide

## How to Create Missile Presets in Unity

1. **Right-click** in the Project window (Assets folder)
2. Select **Create → GravityWars → Missile Preset**
3. Name the asset (e.g., "Wasp_Light", "Hellfire_Medium", "Sledgehammer_Heavy")
4. Select the asset and configure values in the Inspector
5. Assign the preset to ships via the **PlayerShip** component's `Equipped Missile` field

---

## 📊 Recommended Missile Variants

### **LIGHT CLASS** (Fast, Low Damage, Weak Push)

#### 1. **Wasp-I** - Ultra Fast Scout
```
Missile Name: Wasp-I
Missile Type: Light
---
Mass: 5
Max Velocity: 14 m/s
Drag: 0.012
Velocity Approach Rate: 0.15
---
Fuel: 60
Fuel Consumption Rate: 2
---
Payload: 1500
Push Strength: 0.8
Damage Variation: 0.1
---
Self-Destruct Radius: 3
Self-Destruct Damage Factor: 0.4
---
Trail Color: Cyan (R:0, G:1, B:1)
Max Velocity Color: White
```
**Strategy:** Best for quick pot-shots and harassment. Low fuel means short flight time.

---

#### 2. **Dart** - Speed Demon
```
Missile Name: Dart
Missile Type: Light
---
Mass: 4
Max Velocity: 16 m/s
Drag: 0.015
Velocity Approach Rate: 0.2
---
Fuel: 50
Fuel Consumption Rate: 2.5
---
Payload: 1200
Push Strength: 0.6
Damage Variation: 0.12
---
Self-Destruct Radius: 2.5
Self-Destruct Damage Factor: 0.3
---
Trail Color: Light Blue (R:0.5, G:0.8, B:1)
Max Velocity Color: Light Cyan
```
**Strategy:** Fastest missile in the game but weakest damage. Hit-and-run tactics.

---

#### 3. **Viper** - Armor Piercer
```
Missile Name: Viper
Missile Type: Light
---
Mass: 6
Max Velocity: 13 m/s
Drag: 0.01
Velocity Approach Rate: 0.12
---
Fuel: 70
Fuel Consumption Rate: 1.8
---
Payload: 1800
Push Strength: 1.0
Damage Variation: 0.08
---
Self-Destruct Radius: 3.5
Self-Destruct Damage Factor: 0.45
---
Trail Color: Green (R:0, G:1, B:0.3)
Max Velocity Color: Yellow-Green
```
**Strategy:** Balanced light missile with better fuel economy. Good for precise shots.

---

### **MEDIUM CLASS** (Balanced)

#### 4. **Standard** - Reliable Workhorse
```
Missile Name: Standard
Missile Type: Medium
---
Mass: 10
Max Velocity: 10 m/s
Drag: 0.01
Velocity Approach Rate: 0.1
---
Fuel: 100
Fuel Consumption Rate: 2
---
Payload: 2500
Push Strength: 2.0
Damage Variation: 0.1
---
Self-Destruct Radius: 4
Self-Destruct Damage Factor: 0.5
---
Trail Color: Orange (R:1, G:0.5, B:0)
Max Velocity Color: Red
```
**Strategy:** Default missile. No weaknesses, no strengths. Good all-rounder.

---

#### 5. **Hellfire** - High Explosive
```
Missile Name: Hellfire
Missile Type: Medium
---
Mass: 12
Max Velocity: 9 m/s
Drag: 0.009
Velocity Approach Rate: 0.08
---
Fuel: 120
Fuel Consumption Rate: 2.2
---
Payload: 3000
Push Strength: 2.5
Damage Variation: 0.12
---
Self-Destruct Radius: 5
Self-Destruct Damage Factor: 0.6
---
Trail Color: Red-Orange (R:1, G:0.3, B:0)
Max Velocity Color: Bright Red
```
**Strategy:** Higher damage and better blast radius. Slightly slower.

---

#### 6. **Sparrow** - Precision Missile
```
Missile Name: Sparrow
Missile Type: Medium
---
Mass: 8
Max Velocity: 11 m/s
Drag: 0.011
Velocity Approach Rate: 0.12
---
Fuel: 90
Fuel Consumption Rate: 1.8
---
Payload: 2200
Push Strength: 1.7
Damage Variation: 0.05
---
Self-Destruct Radius: 3.5
Self-Destruct Damage Factor: 0.45
---
Trail Color: Yellow (R:1, G:1, B:0)
Max Velocity Color: Gold
```
**Strategy:** Low damage variation makes it more predictable. Good for skill-based players.

---

### **HEAVY CLASS** (Slow, High Damage, Strong Push)

#### 7. **Sledgehammer** - Knockback King
```
Missile Name: Sledgehammer
Missile Type: Heavy
---
Mass: 20
Max Velocity: 7 m/s
Drag: 0.008
Velocity Approach Rate: 0.07
---
Fuel: 150
Fuel Consumption Rate: 2
---
Payload: 5000
Push Strength: 6.0
Damage Variation: 0.15
---
Self-Destruct Radius: 6
Self-Destruct Damage Factor: 0.7
---
Trail Color: Purple (R:0.8, G:0, B:1)
Max Velocity Color: Magenta
```
**Strategy:** Massive knockback force. Can push ships into planets. Devastating repositioning tool.

---

#### 8. **Bunker Buster** - Maximum Damage
```
Missile Name: Bunker Buster
Missile Type: Heavy
---
Mass: 25
Max Velocity: 6 m/s
Drag: 0.007
Velocity Approach Rate: 0.05
---
Fuel: 180
Fuel Consumption Rate: 2.5
---
Payload: 6000
Push Strength: 4.0
Damage Variation: 0.18
---
Self-Destruct Radius: 5.5
Self-Destruct Damage Factor: 0.75
---
Trail Color: Dark Red (R:0.8, G:0, B:0)
Max Velocity Color: Deep Crimson
```
**Strategy:** Highest raw damage. Slow but devastating. One-shot potential against light ships.

---

#### 9. **Devastator** - All-Around Heavy
```
Missile Name: Devastator
Missile Type: Heavy
---
Mass: 22
Max Velocity: 7.5 m/s
Drag: 0.0085
Velocity Approach Rate: 0.08
---
Fuel: 160
Fuel Consumption Rate: 2.2
---
Payload: 5500
Push Strength: 5.0
Damage Variation: 0.12
---
Self-Destruct Radius: 7
Self-Destruct Damage Factor: 0.65
---
Trail Color: Orange-Red (R:1, G:0.2, B:0)
Max Velocity Color: Fiery Orange
```
**Strategy:** Balanced heavy missile. Best blast radius for self-destruct plays.

---

## 📐 Ship Archetype Restrictions

| Ship Type | Allowed Missiles | Reasoning |
|-----------|------------------|-----------|
| **Tank** | Medium, Heavy | Tanks can't mount light missile launchers (too fragile) |
| **Damage Dealer** | Light, Medium, Heavy | Pure offense - can use anything |
| **All-Around** | Light, Medium, Heavy | Versatile - no restrictions |
| **Controller** | Light, Medium | Controllers focus on precision, not brute force |

---

## 🎮 Gameplay Balance Notes

### **Flight Time Comparison**
```
Light missiles:   25-40 seconds
Medium missiles:  45-50 seconds
Heavy missiles:   60-75 seconds
```

### **Effective Damage Per Fuel** (Efficiency)
```
Wasp-I:      1500 / 60  = 25 damage/fuel  ✓ EFFICIENT
Standard:    2500 / 100 = 25 damage/fuel
Sledgehammer: 5000 / 150 = 33 damage/fuel  ✓ MOST EFFICIENT
```

### **Speed vs. Damage Tradeoff**
```
Fastest: Dart (16 m/s,   1200 dmg) = 75  dmg/speed
Balanced: Standard (10 m/s, 2500 dmg) = 250 dmg/speed  ✓ BALANCED
Slowest: Bunker Buster (6 m/s, 6000 dmg) = 1000 dmg/speed ✓ BEST RATIO
```

---

## 🔧 Testing Checklist

After creating all 9 presets:

- [ ] Create all 9 assets in Unity
- [ ] Assign unique trail colors to each
- [ ] Test Tank ship can only equip Medium/Heavy
- [ ] Test Controller ship can only equip Light/Medium
- [ ] Verify trajectory prediction matches actual flight
- [ ] Confirm push forces differ between types
- [ ] Test that perks (Cluster, Explosive, etc.) still work
- [ ] Verify fuel consumption differences are noticeable
- [ ] Check that damage scales correctly
- [ ] Ensure visual differentiation is clear

---

## 🎨 Future Enhancements

1. **Custom Visual Models** - Assign unique 3D models per missile type
2. **Custom Sounds** - Different launch/fly/explosion sounds
3. **Particle Effects** - Unique thrust particles per type
4. **Unlock System** - Require player level to access certain missiles
5. **Economy** - Different purchase costs (currently unused but in ScriptableObject)

---

## 📝 Usage Example

```csharp
// In Unity Inspector:
// 1. Select Player1Ship GameObject
// 2. Find "PlayerShip" component
// 3. Under "Missile Type Selection":
//    - Equipped Missile: Drag "Hellfire_Medium" asset here
//    - Ignore Restrictions: Unchecked (for normal gameplay)

// For testing all missiles on one ship:
// - Set "Ignore Restrictions" to TRUE
// - Swap "Equipped Missile" during playtesting
```

---

## 🐛 Troubleshooting

**Q: Trajectory line doesn't match missile flight**
A: Ensure `equippedMissile` is assigned in PlayerShip Inspector

**Q: Tank can use light missiles**
A: Check `ignoreRestrictions` is FALSE in PlayerShip component

**Q: All missiles behave the same**
A: Verify `ApplyMissilePreset()` is being called in `FireMissile()`

**Q: No visual difference between missiles**
A: Trail colors are set. For model differences, assign `visualModelPrefab` in preset

---

Generated: 2025
