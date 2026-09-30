# 🚀 Gravity Wars

> **Turn-based tactical space combat with realistic gravity physics**

A 2D tactical space combat game rendered with beautiful 3D models, featuring deep strategic gameplay, customizable ships, and intense physics-based missile warfare around planets with N-body gravitational simulation.

![Genre](https://img.shields.io/badge/Genre-Turn--Based%20Tactics-blue)
![Platform](https://img.shields.io/badge/Platform-Unity-black)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)

---

## 🎮 Game Overview

**Gravity Wars** is a competitive turn-based space combat game where two players battle by firing missiles around procedurally-placed planets. Master gravity wells, trajectory prediction, ship positioning, and tactical abilities to destroy your opponent!

### Core Gameplay Loop

1. **Plan Your Move** - Aim your ship, adjust power, or reposition using slingshot/warp mechanics
2. **Fire Your Missile** - Launch projectiles that curve through realistic gravity fields
3. **Watch the Chaos** - Dynamic camera follows missiles with cinematic slow-motion on near-misses
4. **Adapt & Survive** - Use ship parts strategically (wings absorb damage, core hits are critical!)
5. **Win the Round** - First to destroy enemy ship wins the round
6. **Claim Victory** - Best of 3/5/7 rounds (configurable) wins the match

---

## ✨ Key Features

### 🌌 Realistic N-Body Gravity Physics
- **Newton's Law of Universal Gravitation**: `F = G × (M₁ × M₂) / r²`
- Planets exert gravitational pull on missiles and ships
- Procedural planet spawning with overlap prevention
- Mass-based physics for authentic orbital mechanics

### 🚀 Deep Ship Customization
Choose from **4 Archetypes**, each with unique playstyle:

| Archetype | HP | Armor | Damage | Action Points | Special Trait |
|-----------|-----|-------|--------|---------------|---------------|
| **Tank** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | 3 | Can't use Light missiles, Slow rotation |
| **Damage Dealer** | ⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | 3 | Fast rotation, All missile types |
| **Controller** | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | **4** | Extra action point, Can't use Heavy missiles |
| **All-Around** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 3 | Balanced stats, All missile types |

### 📈 Dual Progression Systems

**1. Ship Progression (Level 1-20)**
- Quadratic XP formula: `XP = 200 + 75 × Level²`
- Stats scale with level (HP, Armor, Damage multiplier)
- Unlock passive at **Level 10**
- Unlock perks at **Level 5/15/20**

**2. Account Progression** *(Planned for Multiplayer)*
- Global account XP unlocks ships, perks, passives, move types
- Free Battle Pass (permanent unlocks)
- Seasonal Premium Battle Pass (exclusive cosmetics + content)

### 💥 3 Missile Classes

| Type | Speed | Damage | Fuel | Push Force | Restrictions |
|------|-------|--------|------|------------|--------------|
| **Light** | Fast | Low | Low | Weak | Not usable by Tank |
| **Medium** | Balanced | Medium | Medium | Medium | All archetypes |
| **Heavy** | Slow → **Devastating** | High | High | **STRONG** | Not usable by Controller |

**Advanced Missile Features:**
- **Fuel System**: Missiles die when fuel runs out (lbs/second burn rate)
- **Max Velocity**: Terminal speed cap (Heavy missiles can reach 500 m/s through gravity!)
- **Launch Velocity Range**: Configurable min/max throw speed per missile
- **Part-Based Damage**: Hitting wings = 85%, core = 130% damage
- **Speed-Based Damage**: Faster impacts = bigger explosions
- **Self-Destruct**: Manual detonation (Space key) with blast radius
- **Collision Avoidance** (Passive): RCS thrusters steer around planets

### 🎯 15 Passive Abilities (Unlock at Level 10)

<details>
<summary><b>Click to view all passives</b></summary>

1. **Sniper Mode** - Full trajectory preview (normally half-length)
2. **Unmovable** - Immune to missile knockback
3. **Enhanced Regeneration** - Passive HP regen (1% per second)
4. **Damage Resistance** - Take 15% less damage
5. **Critical Immunity** - Core hits don't deal bonus damage
6. **Critical Enhancement** - Your core hits deal 1.5× damage
7. **Damage Boost** - Damage ramps up 1.0× → 2.0× over 120s of inactivity
8. **Last Chance** - Survive at 1 HP once per round instead of dying
9. **Adaptive Armor** - Armor increases 10% each time hit
10. **Adaptive Damage** - Damage increases 10% each hit on enemy
11. **Precision Engineering** - No damage variation (always exact)
12. **Collision Avoidance** - Missiles auto-steer around planets
13. **Lifesteal** - Heal for 20% of damage dealt
14. **Reduce High-Speed Damage** - 20% less damage from fast missiles
15. **Increase High-Speed Damage** - Deal 20% more with fast missiles

*Passives have archetype restrictions to prevent overpowered combinations*

</details>

### ⚡ Active Perks (3-Tier System)

**Tier 1 (Unlock Level 5)** - Cost: 1 Action Point
- Multi-Missile (3 missiles in spread pattern)
- Explosive Missile (area-of-effect blast)
- Overcharged Cannon (extra damage)

**Tier 2 (Unlock Level 15)** - Cost: 2 Action Points
- Cluster Missile (splits into 3 mid-flight)
- Pusher Missile (massive knockback)
- Boost Jets (extra move action)

**Tier 3 (Unlock Level 20)** - Cost: 3 Action Points
- Missile Barrage (4 rapid-fire missiles)
- *More coming soon...*

*Perks validate against ship archetype automatically!*

### 🎮 3 Movement Types

| Move Type | Description | Archetype Restrictions |
|-----------|-------------|------------------------|
| **Normal** | Slingshot physics with deceleration | All archetypes |
| **Precision** | Shows ghost preview of landing spot | ❌ NOT for Tank (too bulky!) |
| **Warp** | Instant teleport with zoom animation | ✅ **Controller EXCLUSIVE** |

### 🎨 Polish & Visual Features

- **Dynamic Camera System**:
  - Auto-frames both ships between turns
  - Follows missiles with smooth panning/zooming
  - Proximity zoom when near enemy ship
  - Slow-motion effect on close calls
  - Holds on explosion point for 3 seconds

- **3D Models in 2D Space**:
  - All objects are 3D models for visual fidelity
  - Orthographic camera for consistent gameplay
  - Banking/tilting animations on missiles and ships
  - Particle effects for explosions and trails

- **Ship Destruction**:
  - Parts break off dynamically with physics
  - Explosion force radiates outward
  - Cinematic camera hold on impact point
  - 3-second dramatic pause before next round

- **Audio Design**:
  - 3D spatial audio for fly-by effects
  - Engine loop with velocity-based pitch
  - Impact sounds, explosions, destruction SFX
  - Volume controls for SFX/Music

---

## 🕹️ Controls

### Ship Controls (Your Turn)
| Key | Action |
|-----|--------|
| **A / D** | Rotate ship left/right |
| **Hold Shift** | Fine-tune rotation (slower) |
| **W / S** | Adjust launch power |
| **Space** | Fire missile OR Execute move OR Self-destruct missile (in flight) |
| **E** | Toggle Fire/Move mode |
| **Tab** | Cycle move types (Normal → Precision → Warp) |
| **1 / 2 / 3** | Arm Tier 1/2/3 perk (press again to disarm) |

### Missile Flight Controls
| Key | Action |
|-----|--------|
| **A / D** | Steer missile (if fuel remaining) |
| **Space** | Self-destruct missile (manual detonation) |

---

## 🎯 Game Modes

### Hot Seat (Local Multiplayer)
- 2 players on same device
- Configurable settings:
  - Player names
  - Winning score (best of 3/5/7/etc.)
  - Turn duration (seconds)
  - Preparation time
  - Number of planets to spawn

### *(Planned) Online Multiplayer*
- Matchmaking with skill-based ratings
- Custom ship loadouts
- Account progression & battle pass
- Seasonal content updates

---

## 🏗️ Technical Architecture

### Modular ScriptableObject System

All game content is data-driven using Unity ScriptableObjects for maximum flexibility:

```
Ship System/
├── ShipBodySO.cs          # Chassis with base stats (HP, armor, damage, etc.)
├── ShipLevelingFormulaSO.cs # Stat scaling formulas per archetype
├── ShipPresetSO.cs        # Complete ship (Body + Perks + Passives + Move Type)
├── ActivePerkSO.cs        # Active ability definitions
├── PassiveAbilitySO.cs    # Passive ability definitions
├── MoveTypeSO.cs          # Movement type definitions
└── MissilePresetSO.cs     # Missile configurations

Core Systems/
├── GameManager.cs         # Turn management, round/match flow
├── PlayerShip.cs          # Ship behavior, controls, stats
├── Missile3D.cs           # Missile physics, guidance, collision
├── Planet.cs              # Gravitational pull calculations
├── PerkManager.cs         # Perk activation and validation
└── CameraController.cs    # Dynamic camera following and framing
```

### Validation System

**Every component validates compatibility automatically:**

```csharp
// Example: Creating a custom ship
ShipPresetSO customShip = CreateInstance<ShipPresetSO>();
customShip.shipBody = tankChassis;           // Tank archetype
customShip.tier1Perk = warpPerk;             // ❌ INVALID!
customShip.moveType = warpMove;              // ❌ INVALID!

if (!customShip.Validate())
{
    // Validation fails with helpful error messages:
    // "ERROR: Tier 1 perk 'Warp' cannot be used by Tank!"
    // "ERROR: Move type 'Warp Move' cannot be used by Tank!"
}
```

**Design Rules Enforced:**
- Tanks can't use Light missiles or Precision/Warp moves
- Controllers can't use Heavy missiles
- Warp move is Controller-exclusive
- Perks/Passives check archetype compatibility
- Overpowered combinations trigger warnings (e.g., AdaptiveArmor on Tank)

---

## 📊 Damage System

### Damage Calculation Pipeline

```
1. Base Damage = Missile Payload × Random Variation (±10%)
2. Part Multiplier Applied (Wing: 0.85×, Core: 1.30×, etc.)
3. Attacker Damage Multiplier Applied (scales with ship level)
4. Armor Reduction = Armor / (Armor + 400)
5. Damage Resistance Applied (if passive active)
6. Speed Bonus/Penalty Applied (if passive active)
7. Final Damage → Reduce Ship HP
```

### Armor Formula
```
Damage Reduction % = Armor / (Armor + 400)

Examples:
- 100 Armor = 20% reduction
- 200 Armor = 33% reduction
- 400 Armor = 50% reduction (diminishing returns!)
```

### Effective HP
```
Effective HP = HP × (1 + Armor / 400)

Example (Tank at Level 1):
- HP: 11,000
- Armor: 100
- Effective HP: 11,000 × 1.25 = 13,750
```

---

## 🌍 Physics Simulation

### Gravity System
```csharp
// Gravitational force calculation (executed every FixedUpdate)
Vector3 CalculateGravitationalPull(Vector3 objectPos, float objectMass)
{
    Vector3 direction = planetPos - objectPos;
    float distance = direction.magnitude;
    float forceMagnitude = G × (planetMass × objectMass) / (distance²);
    return direction.normalized × forceMagnitude;
}
```

**Optimization:**
- Planet cache to avoid `FindObjectsOfType()` spam
- Distance checks to skip negligible forces
- FixedUpdate for physics consistency

### Missile Flight
```csharp
// Missile physics loop (simplified)
foreach (Planet planet in allPlanets)
{
    Vector3 gravityPull = planet.CalculateGravitationalPull(position, mass);
    velocity += gravityPull * Time.fixedDeltaTime;
}

// Apply drag
velocity *= (1 - drag);

// Clamp to max velocity
if (velocity.magnitude > maxVelocity)
    velocity = velocity.normalized × maxVelocity;

// Update position
position += velocity * Time.fixedDeltaTime;
```

---

## 🎨 Art & Visuals

### 3D Models in 2D Space

- **Rendering:** Orthographic camera for 2D gameplay with 3D depth
- **Ships:** Detailed 3D models with multiple destructible parts
- **Missiles:** 3D models with dynamic trails and banking animations
- **Planets:** Spheres with procedural textures and glow effects
- **VFX:** Particle systems for explosions, trails, engine flames

### UI/UX Design

**Fighting Game Style HUD:**
- Player panels on left/right sides
- Health bars with numeric display (e.g., "8500 / 10000")
- Action point bars with visual dividers
- Ship info: Model name, level, archetype
- Passive ability icon (active/inactive states)
- 3 perk icon slots with color coding:
  - **Black** = Locked (level too low)
  - **Gray** = Can't activate (wrong mode/used)
  - **White** = Ready to use
  - **Yellow** = Currently armed
- Move type icon with active/inactive states
- Bubble timer for turn countdown / missile fuel

---

## 🔧 Development Roadmap

### ✅ Completed (Current Build)
- [x] Core turn-based gameplay loop
- [x] N-body gravity physics simulation
- [x] 4 ship archetypes with full customization
- [x] 3 missile types with fuel system
- [x] 15 passive abilities
- [x] Active perk system (Tier 1/2/3)
- [x] 3 movement types (Normal/Precision/Warp)
- [x] Part-based ship destruction
- [x] Dynamic camera system with slow-motion
- [x] Trajectory prediction (full/half based on Sniper Mode)
- [x] Hot Seat local multiplayer
- [x] Modular ScriptableObject architecture
- [x] Comprehensive validation system
- [x] Ship progression (Level 1-20)

### 🚧 In Progress
- [ ] Bug fixes for perk activation timing
- [ ] Balance tuning for archetypes
- [ ] Audio implementation (SFX, music, spatial audio)
- [ ] Visual polish (VFX, particles, animations)

### 📅 Planned Features

**Phase 1: Core Content**
- [ ] More ship bodies per archetype (3-5 variants)
- [ ] Additional perks (fill out all tier slots)
- [ ] More passive abilities (target: 20+)
- [ ] Visual customization (skins, colors, decals)
- [ ] Tutorial system with guided gameplay

**Phase 2: Progression & Monetization**
- [ ] Account progression system (global XP)
- [ ] Free Battle Pass (permanent unlocks)
- [ ] Seasonal Battle Pass (premium content)
- [ ] Ship customization UI (loadout builder)
- [ ] Unlock system with progression gates
- [ ] In-game currency (soft + hard)
- [ ] Cosmetic shop

**Phase 3: Multiplayer**
- [ ] Online matchmaking (skill-based)
- [ ] Ranked ladder with seasons
- [ ] Friend system & private lobbies
- [ ] Replay system
- [ ] Leaderboards
- [ ] Spectator mode

**Phase 4: Advanced Features**
- [ ] 3v3 or 4v4 team modes
- [ ] Map variants (different planet layouts)
- [ ] Environmental hazards (asteroid fields, black holes)
- [ ] Ship abilities (shields, dashes, etc.)
- [ ] Clan/Guild system
- [ ] Tournaments & events

---

## 🎓 Design Philosophy

### Balance Pillars

1. **Archetype Identity**
   - Each archetype has clear strengths and weaknesses
   - No "best" archetype - all viable in different scenarios
   - Restrictions prevent homogenization (e.g., Tank can't use Warp)

2. **Skill-Based Gameplay**
   - Physics mastery > stat advantages
   - Trajectory prediction rewards planning
   - Positioning matters (slingshot around planets)

3. **Tactical Depth**
   - Action point economy (when to fire vs. move vs. perk)
   - Part-targeting (aim for core vs. chip away at wings)
   - Perk timing (save Tier 3 for finishing blow?)

4. **Fair Monetization**
   - Premium content = cosmetics + XP boosts, NOT power
   - All gameplay-affecting items unlockable for free
   - Battle Pass earns premium currency back (like Fortnite)

### Accessibility

- **Easy to Learn:** Core loop is simple (aim, fire, watch)
- **Hard to Master:** Physics, timing, perk combos create skill ceiling
- **No Pay-to-Win:** Matchmaking uses skill rating, not wallet size
- **Colorblind Support:** *(Planned)* UI color options
- **Configurable Match Length:** Quick 3-round matches or epic 7-rounders

---

## 🛠️ For Developers

### Project Setup

**Requirements:**
- Unity 2021.3 LTS or newer
- .NET Framework 4.x
- TextMeshPro package

**Cloning & Running:**
```bash
git clone <repository-url>
cd GravityWarsUnity
# Open project in Unity
# Load scene: Assets/Scenes/MainScene.unity
# Press Play
```

### Creating Custom Content

**New Ship Archetype:**
1. Right-click in Project → Create → GravityWars → Ship System → Ship Body
2. Configure base stats (HP, armor, damage, action points, rotation speeds)
3. Set archetype type (Tank/DD/Controller/AllAround)
4. Set missile restrictions (checkboxes)
5. Create matching leveling formula (Ship Leveling Formula)
6. Create ship preset combining body + formula + perks + passives

**New Passive Ability:**
1. Right-click → Create → GravityWars → Ship System → Passive Ability
2. Set passive type (from enum)
3. Configure archetype restrictions (checkboxes)
4. Set unlock level (default: 10)
5. If needed, implement logic in `PassiveAbilitySO.ApplyToShip()`

**New Active Perk:**
1. Create new `ActivePerkSO` subclass (e.g., `TeleportPerkSO`)
2. Create corresponding `IActivePerk` implementation
3. Add archetype restrictions (checkboxes in SO)
4. Set tier (1/2/3) and cost
5. Implement `CanActivate()` and `Activate()` logic

**New Missile Type:**
1. Right-click → Create → GravityWars → Missile Preset
2. Configure physics (mass, velocity, drag, fuel)
3. Configure damage (payload, push strength, variation)
4. Set missile type (Light/Medium/Heavy)
5. Assign visual model and trail color

### Debugging

**Console Logs:**
- `[PerkManager]` - Perk activation, validation
- `[ExplosiveMissilePerk]` etc. - Specific perk activations
- `[PlayerShip]` - Ship actions, damage, stat updates
- `[GameManager]` - Turn flow, round/match state

**Inspector Tools:**
- Select PlayerShip in Hierarchy → View real-time stats
- Select Planet → See gravitational constant, mass
- Select Missile → Monitor fuel, velocity, payload

---

## 📜 License

*(Add your license here - MIT, Apache, Proprietary, etc.)*

---

## 🤝 Contributing

*(If open-source, add contribution guidelines)*

---

## 📞 Contact & Support

*(Add contact info, Discord server, bug report email, etc.)*

---

## 🎮 Credits

**Game Design & Development:** *(Your name/studio)*
**Engine:** Unity
**Programming:** C#
**Inspiration:** Scorched Earth, Artillery, Worms, Brawl Stars

---

## 📸 Screenshots

*(Add gameplay screenshots, ship customization UI, battle pass mockups, etc.)*

---

## 🔥 Why Play Gravity Wars?

- ✅ **Easy to Learn, Hard to Master** - Intuitive controls, deep strategy
- ✅ **Fair Progression** - No pay-to-win, skill-based matchmaking
- ✅ **Endless Variety** - 4 archetypes × 15 passives × perks × missiles = thousands of loadouts
- ✅ **Tactical Depth** - Action points, positioning, part-targeting, physics mastery
- ✅ **Satisfying Combat** - Cinematic camera, slow-motion, explosive destruction
- ✅ **Competitive Scene** - Ranked seasons, tournaments, leaderboards
- ✅ **Regular Updates** - Seasonal content, balance patches, new ships

**Join the battle. Master gravity. Destroy your enemies. 🚀**

---

*Last Updated: November 2025*
