# 🚀 Gravity Wars - Complete Game Design Document

**Version:** 1.0  
**Last Updated:** December 19, 2024  
**Document Purpose:** Comprehensive reference for all game systems, mechanics, and design decisions

---

## 📋 Table of Contents

1. [Game Overview](#game-overview)
2. [Core Gameplay Mechanics](#core-gameplay-mechanics)
3. [Ship System](#ship-system)
4. [Combat System](#combat-system)
5. [Progression Systems](#progression-systems)
6. [Game Modes](#game-modes)
7. [UI/UX Design](#uiux-design)
8. [Monetization](#monetization)
9. [Technical Architecture](#technical-architecture)
10. [Balance Philosophy](#balance-philosophy)

---

## 🎮 Game Overview

### High-Level Concept
**Gravity Wars** is a **turn-based tactical space combat game** that combines:
- **Scorched Earth-style artillery gameplay** (aim, power, fire)
- **Realistic N-body gravity physics** (planets affect missile trajectories)
- **Brawl Stars-style progression** (account XP, battle passes, unlockables)
- **2D gameplay with 3D visual elements** (viewed from top-down, Z-axis locked)

### Inspirations
- **Scorched Earth / Worms** - Turn-based artillery gameplay
- **Brawl Stars** - Progression, UI/UX, main menu hub structure
- **Chess** - ELO ranking system for competitive play
- **Fortnite** - Battle Pass model (free + premium tracks)

### Target Audience
- **Primary:** Casual-to-midcore players who enjoy tactical gameplay
- **Secondary:** Competitive players seeking skill-based PvP
- **Age Range:** 13+
- **Platforms:** PC (Unity), Mobile (planned)

### Unique Selling Points
1. **Realistic gravity physics** - Missiles curve around planets using Newton's law
2. **Part-based ship destruction** - Wings, fins, engines break off realistically
3. **No pay-to-win** - All gameplay items unlockable for free
4. **Deep customization** - Build custom ships from unlocked parts
5. **Peer-to-peer netcode** - Both players see identical screens, minimal lag

---

## 🎯 Core Gameplay Mechanics

### Turn-Based Combat Loop

```
┌──────────────────────────────────────────────┐
│ 1. PLAN YOUR MOVE                            │
│    - Aim ship (360° rotation)                │
│    - Adjust launch power (min/max range)     │
│    - OR reposition using move action         │
│    - OR activate perk                        │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│ 2. FIRE MISSILE                              │
│    - Missile launches with initial velocity  │
│    - Gravity from planets affects trajectory │
│    - Fuel burns (lbs/second)                 │
│    - Player can self-destruct (Space key)    │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│ 3. WATCH THE CHAOS                           │
│    - Dynamic camera follows missile          │
│    - Slow-motion when near enemy (50% speed) │
│    - Proximity zoom effect                   │
│    - Cinematic camera angles                 │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│ 4. DAMAGE RESOLUTION                         │
│    - Part-based damage (wings 85%, core 130%)│
│    - Speed-based damage (faster = more dmg)  │
│    - Knockback/push force applied            │
│    - Ship parts break off realistically      │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│ 5. TURN ENDS                                 │
│    - Camera frames both ships                │
│    - Opponent's turn begins                  │
│    - Repeat until ship destroyed             │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│ 6. ROUND/MATCH VICTORY                       │
│    - First to destroy enemy wins round       │
│    - Best of 3/5/7 wins match (configurable) │
│    - XP awarded to account & equipped ship   │
└──────────────────────────────────────────────┘
```

### Action Points System

**Each turn, players have 3 action points** (Controller archetype has 4).

Action points can be spent on:
- **Fire Missile** - Costs 0 AP (always available, multiple times per turn)
- **Move Ship** - Costs 1 AP per move
- **Activate Perk** - Costs 1-3 AP depending on tier:
  - Tier 1 Perks: 1 AP
  - Tier 2 Perks: 2 AP
  - Tier 3 Perks: 3 AP

**Examples:**
- Player uses 3 moves in one turn (3 AP) → Can still fire missiles (0 AP)
- Player uses 1 move (1 AP) + Tier 2 perk (2 AP) → Total 3 AP, still can fire
- Controller uses 1 move (1 AP) + Tier 3 perk (3 AP) = 4 AP total (unique advantage!)

### Physics System

#### Gravity Calculation
The game uses **Newton's Law of Universal Gravitation**:

```
F = G × (M₁ × M₂) / r²
```

Where:
- **G** = 0.5 (gravitational constant, tuned for gameplay)
- **M₁** = Planet mass (varies per planet)
- **M₂** = Missile mass (typically 10 units)
- **r** = Distance between planet center and missile

**Implementation:**
- Custom physics (NOT Unity's built-in Rigidbody)
- Calculated in `FixedUpdate()` every physics tick
- All planets affect all missiles simultaneously (N-body simulation)

#### Why Custom Physics?
1. **Z-axis locked** - Game is fundamentally 2D (viewed top-down)
2. **Tuned for gameplay** - Gravity constant adjusted for fun, not realism
3. **Hybrid missiles** - Missiles have thrust + gravity (not pure ballistic)
4. **Fuel system** - Missiles die when fuel runs out (not in Unity physics)

#### Missile Physics Pipeline (Missile3D.cs)

```
FixedUpdate() {
    1. ApplyGravity()             → Calculate force from all planets
    2. AvoidPlanetsPredictively() → RCS collision avoidance (if passive active)
    3. MoveMissile()              → Apply drag, clamp velocity, consume fuel
    4. CheckCollisions()          → Detect hits on ships/planets
}
```

---

## 🚢 Ship System

### 4 Ship Archetypes

| Archetype | Role | HP | Armor | Damage | Action Points | Special Traits |
|-----------|------|-----|-------|--------|---------------|----------------|
| **Tank** | Frontline Absorber | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | 3 | • Can't use Light missiles<br>• Slow rotation speed<br>• High survivability |
| **Damage Dealer** | Glass Cannon | ⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | 3 | • Fast rotation speed<br>• All missile types<br>• High damage output |
| **Controller** | Tactical Manipulator | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | **4** | • Extra action point<br>• Can't use Heavy missiles<br>• Warp move exclusive |
| **All-Around** | Balanced Versatility | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 3 | • Balanced stats<br>• All missile types<br>• No major weaknesses |

### Ship Components

Every ship consists of:

1. **Ship Body (ShipBodySO)** - Core structure
   - Archetype type (Tank/DD/Controller/AllAround)
   - Base stats (HP, Armor, Damage Multiplier, Action Points)
   - 3D model reference
   - Missile type restrictions
   - Rotation speeds (coarse/fine adjustments)

2. **Leveling Formula (ShipLevelingFormulaSO)** - Stat scaling
   - HP scaling per level (%)
   - Armor scaling per level (flat)
   - Damage scaling per level (%)
   - **4 total formulas** (one per archetype, static)

3. **1 Passive Ability (PassiveAbilitySO)** - Always-on effect
   - Unlocks at **Level 10**
   - Cannot be changed (part of ship model)
   - Examples: Lifesteal, Damage Resistance, Sniper Mode

4. **3 Active Perks (ActivePerkSO)** - One-time-per-turn abilities
   - **Tier 1** unlocks at Level 5 (costs 1 AP)
   - **Tier 2** unlocks at Level 15 (costs 2 AP)
   - **Tier 3** unlocks at Level 20 (costs 3 AP)
   - Examples: Multi-Missile, Cluster Missile, Missile Barrage

5. **1 Move Type (MoveTypeSO)** - Movement mechanic
   - **Normal** - Slingshot physics with deceleration (all archetypes)
   - **Precision** - Ghost preview of landing spot (NOT Tank)
   - **Warp** - Instant teleport with zoom animation (Controller ONLY)

6. **Missile Type** - NOT part of ship model
   - Players can freely switch missiles before each match
   - Restrictions based on archetype (e.g., Tank can't use Light)

### Ship Progression (Levels 1-20)

#### XP Formula
```
XP Needed = 200 + 75 × Level²
```

**Key Milestones:**
- **Level 1 → 2:** 275 XP
- **Level 5 → 6:** 2,075 XP (unlock Tier 1 perk)
- **Level 10 → 11:** 7,700 XP (unlock Passive)
- **Level 15 → 16:** 17,075 XP (unlock Tier 2 perk)
- **Level 20:** MAX LEVEL (unlock Tier 3 perk)
- **Total XP 1→20:** 101,275 XP

#### Stat Scaling (Level 1 vs Level 20)

| Archetype | HP (L1) | HP (L20) | Armor (L1) | Armor (L20) | Damage (L1) | Damage (L20) |
|-----------|---------|----------|------------|-------------|-------------|--------------|
| Tank | 11,000 | 19,360 | 100 | 176 | 1.0× | 1.285× |
| Damage Dealer | 8,500 | 11,730 | 50 | 69 | 1.0× | 1.76× |
| Controller | 9,000 | 12,420 | 75 | 113 | 1.0× | 1.475× |
| All-Around | 10,000 | 15,700 | 100 | 157 | 1.0× | 1.57× |

### Ship XP Tracking

**CRITICAL DESIGN DECISION:**
- Ship XP is tied to **ship configuration** (Body + Perks + Passive + Move Type)
- **Changing missile type does NOT reset ship XP**
- This allows players to experiment with missiles without penalty

**Example:**
- Player equips "Iron Fortress" (Tank) with Multi-Missile perk
- Ship gains XP and levels up to Level 8
- Player switches missile from Heavy → Medium
- **Ship remains Level 8** (XP preserved)

### Archetype Restrictions

To maintain balance and archetype identity, restrictions are enforced:

| Item Type | Tank | Damage Dealer | Controller | All-Around |
|-----------|------|---------------|------------|------------|
| **Light Missiles** | ❌ | ✅ | ✅ | ✅ |
| **Medium Missiles** | ✅ | ✅ | ✅ | ✅ |
| **Heavy Missiles** | ✅ | ✅ | ❌ | ✅ |
| **Normal Move** | ✅ | ✅ | ✅ | ✅ |
| **Precision Move** | ❌ | ✅ | ✅ | ✅ |
| **Warp Move** | ❌ | ❌ | ✅ (EXCLUSIVE) | ❌ |

**Passive/Perk Restrictions:**
- Each passive/perk has checkboxes for allowed archetypes
- Example: "Regeneration" passive ❌ NOT for Tank (would be OP)
- Example: "Adaptive Armor" passive ❌ NOT for Tank (already high armor)

---

## ⚔️ Combat System

### 3 Missile Types

| Type | Mass | Speed | Damage | Fuel | Push Force | Use Case |
|------|------|-------|--------|------|------------|----------|
| **Light** | Low (7 units) | Fast | Low (1,800 payload) | Low (80 lbs) | Weak (15 N) | • Quick harassment<br>• Multiple shots<br>• Low fuel cost |
| **Medium** | Medium (10 units) | Balanced | Medium (2,500 payload) | Medium (100 lbs) | Medium (25 N) | • Standard combat<br>• Balanced stats<br>• Most versatile |
| **Heavy** | High (15 units) | Slow initially | High (3,500 payload) | High (150 lbs) | **STRONG** (50 N) | • Devastating impact<br>• Can reach 500 m/s via gravity!<br>• Knockback positioning |

### Damage Calculation

#### Base Damage Formula
```
Final Damage = 
    Missile Payload 
    × Random Variation (±10%)
    × Part Multiplier
    × Attacker Damage Multiplier (scales with ship level)
    × Armor Reduction
    × Damage Resistance (if passive active)
    × Speed Bonus/Penalty (if passive active)
```

#### Part-Based Damage

Ships have multiple destructible parts:
- **Wings** - 85% damage taken (ablative armor)
- **Fins** - 85% damage taken
- **Core (main body)** - **130% damage taken** (critical hit!)

**Strategy:** Aim for core to maximize damage, but wings are easier to hit.

#### Armor System

```
Damage Reduction % = Armor / (Armor + 400)
```

**Examples:**
- 100 Armor = 20% reduction
- 200 Armor = 33% reduction
- 400 Armor = 50% reduction (diminishing returns!)

#### Effective HP

```
Effective HP = HP × (1 + Armor / 400)
```

**Level 20 Tank Example:**
- HP: 19,360
- Armor: 176
- Effective HP: 19,360 × (1 + 176/400) = **27,900 EHP**

### 15 Passive Abilities

| Name | Effect | Best For | Restrictions |
|------|--------|----------|--------------|
| **Sniper Mode** | Full trajectory preview (normally half) | Precision players | All archetypes |
| **Unmovable** | Immune to missile knockback | Tank, positioning | All archetypes |
| **Enhanced Regeneration** | Passive HP regen (1%/sec) | Long matches | NOT Tank (OP) |
| **Damage Resistance** | Take 15% less damage | Tank | All archetypes |
| **Critical Immunity** | Core hits don't deal bonus damage | Defensive play | All archetypes |
| **Critical Enhancement** | Your core hits deal 1.5× damage | Aggressive play | Damage Dealer, All-Around |
| **Damage Boost** | Damage ramps 1.0× → 2.0× over 120s inactivity | Patience rewarded | Damage Dealer |
| **Last Chance** | Survive at 1 HP once per round | Clutch plays | All archetypes |
| **Adaptive Armor** | Armor increases 10% each hit taken | Tank (if balanced) | Monitor for OP combos |
| **Adaptive Damage** | Damage increases 10% each hit landed | Damage Dealer | NOT Tank |
| **Precision Engineering** | No damage variation (always exact) | Consistent damage | All archetypes |
| **Collision Avoidance** | Missiles auto-steer around planets (RCS) | Skill floor lowered | All archetypes |
| **Lifesteal** | Heal for 20% of damage dealt | Sustain | NOT Tank (OP) |
| **Reduce High-Speed Damage** | 20% less damage from fast missiles | Counter to slingshots | Tank, All-Around |
| **Increase High-Speed Damage** | Deal 20% more with fast missiles | Aggressive gravity use | Damage Dealer |

### Active Perks (By Tier)

#### Tier 1 (Level 5 unlock, 1 AP cost)
1. **Multi-Missile** - Fire 3 missiles in spread pattern (±5° angles)
2. **Explosive Missile** - 12-unit blast radius, 3× damage, 30 m/s push
3. **Overcharged Cannon** - Next missile deals +50% damage

#### Tier 2 (Level 15 unlock, 2 AP cost)
4. **Cluster Missile** - Splits into 3 missiles mid-flight
5. **Pusher Missile** - Massive knockback (100 N push force)
6. **Boost Jets** - Extra move action this turn

#### Tier 3 (Level 20 unlock, 3 AP cost)
7. **Missile Barrage** - Fire 4 rapid missiles in sequence

**Design Philosophy:**
- Higher tier = more powerful BUT more expensive
- One perk per turn maximum
- Perks create "big moment" plays
- Encourage strategic AP management

---

## 📈 Progression Systems

### Dual XP Systems

**1. Account XP (Global, Levels 1-100+)**
- Earned from: Battles (simultaneous with Ship XP)
- Unlocks: Ships, ship bodies, perks, passives, game modes
- Formula: `XP = 1000 + (Level × 500)`
- Example milestones:
  - **Level 5:** Unlock Ranked mode
  - **Level 10:** Unlock 2nd custom ship slot
  - **Level 20:** Unlock advanced missiles
  - **Level 50:** Unlock premium battle pass tier

**2. Ship XP (Per-ship, Levels 1-20)**
- Earned from: Battles while ship equipped (simultaneous with Account XP)
- Unlocks: Passive (L10), Perks (L5/15/20)
- Formula: `XP = 200 + 75 × Level²`
- **Tied to ship configuration** (NOT missile type)

**3. Battle Pass XP (Seasonal Progress)**
- Earned from: Daily quests ONLY (not from battles)
- Progresses: Free and premium track simultaneously
- Used for: Unlocking seasonal rewards and cosmetics

### Battle Pass System (Two Types)

#### Free Battle Pass (Non-Seasonal)
- **Available to:** ALL players permanently
- **Progress via:** Daily quest XP
- **Rewards:**
  - Basic ship bodies (Tank, DD, Controller, All-Around variants)
  - Common perks and passives
  - Soft currency (Credits)
  - Ship slots
- **Purpose:** Core progression system, no FOMO

#### Premium Battle Pass (Seasonal)
- **Available to:** Paid ($10-15 per season, ~3 months)
- **Progress via:** Daily quest XP (shared with free track)
- **Rewards:**
  - **Exclusive ship bodies** (unique 3D models, same stats as free equivalents)
  - **Premium skins** (visual customization)
  - **Hard currency** (Gems) - **enough to buy next season's pass**
  - **Cosmetics** (ship trails, explosion effects, UI themes)
- **Purpose:** Monetization without pay-to-win
- **Note:** Battle Pass XP progresses via daily quests ONLY, NOT from battles

**Key Design Decision:**
- Premium battle pass gives **zero gameplay advantage**
- All stats/perks/passives available in free track
- Premium = cosmetics + convenience (XP boosts)
- Model inspired by Fortnite (earns back premium currency)

### Daily Quest System

**Purpose:** Keep players engaged, reward regular play

**Quest Examples:**
- "Win 3 matches" → +500 Battle Pass XP
- "Hit opponent 5 times" → +300 Battle Pass XP
- "Deal 10,000 damage in one match" → +400 Battle Pass XP
- "Use a Tier 3 perk" → +200 Battle Pass XP
- "Win without taking damage" → +1000 Battle Pass XP (challenge)

**Quest Types:**
- **Daily Quests:** 3 per day, rotate at 00:00 UTC
- **Weekly Quests:** 5 per week, harder but more XP
- **Seasonal Challenges:** Special quests for events

**Technical:**
- Tracked via `GameManagerQuestIntegration.cs`
- Server-side validation prevents cheating
- Auto-claim option in settings

### Custom Ship Building System

**Overview:**
- Players build custom ships from unlocked parts
- Ship configuration = Body + 1 Passive + 3 Perks + Move Type
- **Limited custom slots** (2-3 initially, unlock more via account leveling)

**Process:**
1. Choose Ship Body (archetype determines base stats)
2. Choose Passive Ability (must be compatible with archetype)
3. Choose 3 Active Perks (Tier 1, 2, 3) - must match archetype
4. Choose Move Type (must match archetype)
5. Name the ship (user-defined)
6. Ship starts at Level 1, progresses independently

**Custom Ship Management:**
- Ships saved to account permanently (unless deleted)
- Can delete ships to free custom slot
- Ship XP preserved even when not equipped
- Can't edit existing ship (must delete + rebuild to free slot)

**Balance Enforcement:**
- ScriptableObject validation prevents invalid combinations
- Example: Can't put Warp move on Tank
- Example: Can't put Adaptive Armor passive on Tank (flagged as warning)

---

## 🎮 Game Modes

### 1. Hot Seat PvP (LOCAL)
**Status:** ✅ FULLY IMPLEMENTED

**Description:**
- 2 players on same device
- Couch co-op, same screen
- Turn-based, no time pressure option
- Perfect for testing, casual play

**Features:**
- Configurable settings:
  - Player names
  - Winning score (best of 3/5/7/etc.)
  - Turn duration (10-60 seconds, or unlimited)
  - Number of planets (0-10)
  - Starting positions
- Local stats tracking
- Best for: Learning, friends, offline play

### 2. Online Multiplayer - Unranked
**Status:** 🚧 PARTIALLY IMPLEMENTED

**Description:**
- Casual online PvP
- Matchmaking based on skill bracket (loose)
- No ELO/ranking changes
- Forgiving, experimental

**Features:**
- Quick match button
- Play with friends (invite codes)
- Leave without penalty
- XP rewards (account + ship)
- Best for: Practice, trying new ships, fun

### 3. Online Multiplayer - Ranked
**Status:** 🚧 PARTIALLY IMPLEMENTED

**Description:**
- Competitive online PvP
- ELO-based ranking system (like chess)
- Climb leaderboard tiers
- Seasonal rewards

**Features:**
- **ELO System:**
  - Starting ELO: 800 (Ensign rank)
  - Gain/lose ELO per match based on opponent strength
  - K-factor: 32 (volatile) → 16 (stable at high ELO)
- **16 Rank Tiers:**
  - Bronze: Ensign (800-899), Lieutenant (900-999)
  - Silver: Lt. Commander (1000-1099), Commander (1100-1199)
  - Gold: Captain (1200-1299), Commodore (1300-1399)
  - Blue: Rear Admiral (1400-1499), Vice Admiral (1500-1599)
  - Purple: Admiral (1600-1699), Fleet Admiral (1700-1799)
  - Red: Grand Admiral (1800-1899), Supreme Admiral (1900-1999)
  - Elite: Imperial Admiral (2000-2099), Galactic Admiral (2100-2199)
  - Legendary: Eternal Admiral (2200+)
- **Season System:**
  - 3-month seasons
  - Soft reset at season end (ELO reduced by 20%, min 800)
  - Seasonal rewards based on peak rank
- **Leaderboards:**
  - Global top 100
  - Regional top 50
  - Friends leaderboard
- **Ranked Restrictions:**
  - Must be account Level 5+
  - Must have 3+ ships unlocked
  - Leaving match = -50 ELO penalty + temp ban

**Best for:** Competitive players, bragging rights, esports potential

### 4. Training Mode (Planned)
**Description:**
- Solo practice against AI
- No XP rewards
- Adjustable AI difficulty
- Physics playground mode

### 5. Custom Matches (Planned)
**Description:**
- Create private lobbies
- Custom rules (gravity strength, planet count, turn time)
- Tournament support
- Spectator mode

---

## 🎨 UI/UX Design

### Main Menu Hub (Brawl Stars-Style)

**Layout Philosophy:**
- Central 3D ship viewer (primary focus)
- Framed navigation buttons (left/right sides)
- Top bar: Profile, currencies, notifications
- Bottom: Game modes, settings, news ticker

**Screen Breakdown:**

```
┌─────────────────────────────────────────────────────────────┐
│  [Profile]  [Logo + Notifs]  [Currencies + Battle Pass]     │
├─────┬───────────────────────────────────────────────┬───────┤
│     │                                               │       │
│ [🚢]│          ╔═══════════════════╗               │ [🛒] │
│ Ships         ║                   ║                │ Shop  │
│ Garage│        ║   3D SHIP VIEWER  ║               │       │
│     │          ║   (Rotating)      ║               │ [🏆] │
│ [📦]│          ╚═══════════════════╝               │Achiev.│
│ Inv. │                                               │       │
│     │          [🎮 PLAY NOW]                       │ [📊] │
│ [📈]│          [Local|Online|Ranked]               │Acct.  │
│ Lead.│                                               │Prog.  │
│     │                                               │       │
│ [👥]│                                               │ [🛡️] │
│Friends│                                              │ Clan  │
├─────┴───────────────────────────────────────────────┴───────┤
│ [⚙️Settings]  [Loadout]  [News: Season 2 starts Dec 20!]  │
└─────────────────────────────────────────────────────────────┘
```

**Navigation Buttons (Left Side):**
1. Ships Garage - Select/equip ships
2. Inventory - View unlocked items
3. Leaderboards - Rankings (local/global)
4. Friends - Social features

**Navigation Buttons (Right Side):**
5. Shop - Buy items with currency
6. Achievements - Long-term goals
7. Account Progress - View level/unlocks
8. Clan - Guild system (future)

**Top Bar:**
- Avatar, username, account level
- Soft currency (Credits), Hard currency (Gems)
- Battle pass mini-progress bar
- Notification bell
- Daily reward timer

**Center:**
- Large 3D ship rotating on platform
- "PLAY NOW" button (primary CTA)
- Game mode toggle (Local / Online / Ranked)
- Quick stats (W/L ratio, ELO rank)

**Bottom:**
- Settings (gear icon)
- Loadout quick access
- News ticker (announcements, events)

### 18 UI Screens Catalog

Documented in `SCREEN_CATALOG.md`:

**Priority 1 (Essential):**
- ✅ Main Menu Hub
- ✅ Ships Garage
- ⏳ Inventory
- ⏳ Settings

**Priority 2 (High Value):**
- ⏳ Battle Pass
- ⏳ Quests
- ⏳ Account Progress
- ⏳ Achievements

**Priority 3-6:** Leaderboards, Profile, Shop, Matchmaking, Friends, Clan, etc.

### Visual Style

**Aesthetic:** Sci-fi military, futuristic, high-tech
**Color Palette:**
- Primary: Deep blue (#1a237e), Cyan accent (#00e5ff)
- Secondary: Orange highlight (#ff6f00), Gold (#ffd700)
- Background: Dark space (#0d0d1a), Nebula purple (#3d1a4d)
- UI: Metallic chrome, brushed steel textures

**Inspirations:**
- Brawl Stars (clarity, layout)
- Apex Legends (sci-fi HUD)
- Star Citizen (space theme)
- Destiny 2 (menu design)

**Typography:**
- Headers: Orbitron, Exo 2 (futuristic sans-serif)
- Body: Roboto, Inter (clean, readable)

**VFX:**
- Holographic effects
- Glowing borders (neon)
- Particle systems (floating tech icons, energy wisps)
- Animated backgrounds (starfield, nebula parallax)

---

## 💰 Monetization

### Currency System

**Soft Currency: Credits**
- Earned from: Matches, quests, battle pass, achievements
- Used for: Upgrading ships, buying cosmetics, re-rolls
- Economy balance: F2P players should earn ~5,000 credits/week

**Hard Currency: Gems**
- Earned from: Battle pass (free + premium), special events, daily login
- Purchased with: Real money ($5 = 500 gems, $20 = 2,500 gems + bonus)
- Used for: Premium battle pass, cosmetics, XP boosts, convenience

### Monetization Strategy (Fair & Ethical)

1. **NO Pay-to-Win**
   - All ships, perks, passives unlockable for free
   - Premium battle pass = cosmetics only
   - No "loot boxes" or gambling mechanics

2. **Battle Pass Model** (Primary Revenue)
   - Free track: Core content (ships, perks)
   - Premium track: Exclusive cosmetics, XP boosts
   - Seasonal ($10-15 per 3 months)
   - **Earns back premium currency** if completed (à la Fortnite)

3. **Direct Purchases** (Secondary Revenue)
   - Premium ship skins ($5-10)
   - Cosmetic bundles ($15-20)
   - XP boosts (temporary, convenience)

4. **No Dark Patterns**
   - Clear pricing (no obfuscated currency)
   - No limited-time FOMO (free battle pass is permanent)
   - No predatory tactics (timers, energy systems)

### Pricing Philosophy

**Target:** $5-15/month from engaged players
**Comparison:**
- Free players: 100% of gameplay content accessible
- Premium players: Support development, get cosmetic rewards
- Competitive integrity: ELO matchmaking = skill, not wallet

---

## 🔧 Technical Architecture

### Data Architecture

**ONE Unified Player Data System: `PlayerAccountData`**
- Stores: Display name, currencies, account XP, unlocks, battle pass progress
- Used by: All local systems (SaveSystem, ProgressionManager, UI)
- Serialized to: JSON for local save files

**Separate for Online: `PlayerProfileData`**
- Stores: Username, ELO, rank, match history, online stats
- Used by: Multiplayer services, matchmaking, leaderboards
- Synced with: Server (authoritative)

**Key File Locations:**
- `Assets/Progression System/PlayerAccountData.cs` ⭐ (local data)
- `Assets/Online/PlayerProfileData.cs` (online only)
- `Assets/Progression System/SaveSystem.cs` (save/load logic)

### ScriptableObject System

**All game content is data-driven:**

```
Ship System/
├── ShipBodySO.cs         # Chassis with base stats
├── ShipLevelingFormulaSO.cs # Stat scaling formulas
├── ShipPresetSO.cs       # Complete ship (Body + Perks + Passive)
├── PassiveAbilitySO.cs   # Passive ability definitions
├── MoveTypeSO.cs         # Movement type definitions
└── ArchetypeRestrictionChecker.cs # Validation

Active Perks/
├── ActivePerkSO.cs       # Base perk class
├── MultiMissilePerkSO.cs # Specific perk implementations
└── ...

Missiles/
└── MissilePresetSO.cs    # Missile configurations
```

**Benefits:**
- No code changes for balance tweaks
- Create unlimited variants in Unity Inspector
- Easy A/B testing
- Community modding potential

### Network Architecture

**Peer-to-Peer Engine Synchronization:**
- Both clients run identical game engine
- Clients inform each other of moves
- **Both players see identical screens** (no lag compensation needed)
- Server validates moves (anti-cheat)

**Server Responsibilities:**
- Store player data (authoritative)
- Handle authentication
- Matchmaking
- Turn validation
- XP/ranking updates

**Unity Gaming Services:**
- Authentication (Unity Auth)
- Cloud Save (player data sync)
- Relay (P2P connection)
- Lobby (matchmaking queues)

---

## ⚖️ Balance Philosophy

### Design Principles

1. **Archetype Identity**
   - Each archetype has clear strengths and weaknesses
   - No "best" archetype - all viable in different scenarios
   - Restrictions enforce identity (e.g., Tank can't use Warp)

2. **Skill-Based Gameplay**
   - Physics mastery > stat advantages
   - Trajectory prediction rewards planning
   - Positioning matters (slingshot around planets)

3. **Tactical Depth**
   - Action point economy (when to fire vs. move vs. perk)
   - Part-targeting (aim for core vs. chip away at wings)
   - Perk timing (save Tier 3 for finishing blow?)

4. **Counterplay Exists**
   - Every strategy has counter-strategy
   - No unbeatable combinations
   - Passive abilities create rock-paper-scissors dynamics

### Balance Numbers (Level 20)

| Archetype | Effective HP | Damage Output | Role |
|-----------|--------------|---------------|------|
| Tank | 27,900 | 3,188 | Absorb damage, control space |
| Damage Dealer | 17,905 | 4,400 | High burst, glass cannon |
| Controller | 21,500 | 3,688 | Tactical plays, extra AP |
| All-Around | 24,800 | 3,925 | Versatile, no weaknesses |

**Philosophy:**
- Tank: 56% more EHP than DD, but 27% less damage → Balanced
- Controller: Extra AP compensates for lower stats → Unique
- All-Around: Best "average" stats → Beginner-friendly

### Balance Levers

**If archetype is too strong/weak:**
1. Adjust leveling formula (HP/Armor/Damage scaling)
2. Change base stats in ShipBodySO
3. Add/remove archetype restrictions
4. Modify perk costs or effects

**Example Balance Change:**
- Tank was OP (41% more EHP than DD, still decent damage)
- **Fix:** Reduced damage scaling 0.02 → 0.015 per level
- **Result:** Tank now 27% less damage at L20, more balanced

---

## 🚀 Future Development Roadmap

### Phase 1: Core Content (Weeks 1-2)
- ✅ Hot seat multiplayer
- ✅ Physics engine
- ✅ Ship archetypes
- ⏳ Inventory screen
- ⏳ Settings screen

### Phase 2: Progression (Weeks 3-4)
- ⏳ Battle pass UI
- ⏳ Quests system UI
- ⏳ Account progress screen
- ⏳ Achievements system

### Phase 3: Online Multiplayer (Weeks 5-6)
- ⏳ Matchmaking lobby
- ⏳ Ranked system integration
- ⏳ Leaderboards
- ⏳ Profile screen

### Phase 4: Monetization (Week 7)
- ⏳ Shop implementation
- ⏳ Premium currency store
- ⏳ Offer/deal system

### Phase 5: Social (Week 8+)
- ⏳ Friends system
- ⏳ Clan/guild system
- ⏳ Chat integration

### Phase 6: Polish (Week 9+)
- ⏳ Training mode
- ⏳ Tutorial system
- ⏳ News/events screen
- ⏳ Spectator mode

---

## 📝 Key Design Decisions Summary

1. **Custom physics over Unity's** - Tuned for gameplay, 2D locked
2. **Ship XP tied to configuration, NOT missile** - Encourages experimentation
3. **No pay-to-win** - All gameplay content free, premium = cosmetics
4. **Battle pass earns back currency** - Generous like Fortnite
5. **Peer-to-peer netcode** - Both clients identical, server validates
6. **ScriptableObject architecture** - Data-driven, balance without code changes
7. **4 archetypes with enforced identity** - Restrictions prevent homogenization
8. **Dual progression (account + ship)** - Long-term + short-term goals
9. **ELO ranking like chess** - Skill-based matchmaking
10. **18 UI screens for complete game** - Comprehensive menu hub

---

## 🎯 Success Metrics

**Player Retention:**
- Day 1: 60%+
- Day 7: 30%+
- Day 30: 15%+

**Monetization:**
- 5-10% conversion to paying
- $5-15 ARPU (Average Revenue Per User)
- Battle pass completion: 50%+

**Engagement:**
- Average session: 20-30 minutes
- Sessions per week: 10+
- Match completion rate: 90%+

**Competitive Health:**
- ELO distribution: Bell curve centered at 1200 (with starting point at 800)
- Match balance: 45-55% win rate for most players
- Ranked population: 30%+ of active players

---

**END OF DOCUMENT**

This document should be updated as design evolves. Version control recommended.
