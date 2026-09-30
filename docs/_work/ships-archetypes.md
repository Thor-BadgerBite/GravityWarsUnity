# Ships, archetypes, ship builder & content generator (working notes)

Scope: `Assets/Ship System/*` (ShipEnums, ShipBodySO, ShipPresetSO, ShipLevelingFormulaSO, PassiveAbilitySO,
MoveTypeSO, ArchetypeRestrictionChecker + hand-made assets), `Assets/Editor/GameContentGenerator.cs`,
`Assets/Resources/GeneratedContent/*`, `Assets/MatchLoadoutBridge.cs`, `Assets/Online/CustomShipBuilder.cs`,
`Assets/Progression System/UI/ShipBuilderUI.cs`, `ProgressionManager.CreateCustomLoadout/ValidateLoadoutBuild`.

## Data model (IMPLEMENTED)

- `ShipArchetype` enum: Tank, DamageDealer, AllAround, Controller (`Ship System/ShipEnums.cs`). A second, unrelated enum `ShipClass` with the same four values lives in `Online/ProgressionSystem.cs` and is used by all the id-based tables (CONFLICT: two enums for one concept).
- `ShipBodySO`: identity, `visualPrefab` (never read by runtime code — `PlayerShip` always uses the prefab's mesh), `icon`, base HP/armor/damage multiplier, `actionPointsPerTurn`, rotation/tilt, missile flags `canUseLight/Medium/HeavyMissiles`, `requiredAccountLevel`. `OnValidate` hard-clamps: Tank HP ≥ 11000, DD ≤ 10000, Controller ≤ 10000, Controller AP = 4 else 3, Tank no light, Controller no heavy. Inspector `[Range]` attributes (HP 8000–15000, armor 80–120, dmg 0.8–1.2) are **narrower than the generated values** (tanks 16500–17500 HP, DD armor 52, dmg 2.2): touching a slider in the Inspector will clamp a generated body. (CONFLICT, low)
- `ShipPresetSO`: body, leveling formula (auto-lookup `Resources.Load("{archetype}LevelingFormula")` if null — no such asset exists), `passives[]` (ApplyToShip stacks all, builder allows exactly one), tier1/2/3 perks, move type, `defaultMissile`, `requiredAccountLevel`, `isPremiumShip`. `ApplyToShip` copies body stats, applies passives/move type, `UpdateStatsFromLevel`. `Validate` checks archetype compatibility.
- `ShipLevelingFormulaSO`: linear per-level scaling. Four hand-made assets in `Assets/Ship System/` (not in Resources):

| Asset | archetype | HP %/lvl | armor/lvl | dmg/lvl |
|---|---|---|---|---|
| AllAroundLevelingMain | AllAround | 0.03 | 2.5 | 0.025 |
| TankLevelingMain | Tank | 0.03 | 2.5 | 0.018 |
| DDLevelingMain | DamageDealer | 0.03 | 2.5 | 0.045 |
| ControllerLevelingMain | Controller | 0.03 | 2.5 | 0.036 |

  `OnValidate` warnings in this SO ("Tank health scaling seems low, recommended 0.035+") are stale relative to the deliberate rebalance (IMPLEMENTATION_STATUS says so too). The hard-coded fallback in `PlayerShip.UpdateStatsFromHardcodedFormulas` uses different numbers (Tank 0.04/4/0.015, DD 0.02/1/0.04, AllAround 0.03/3/0.03, Controller 0.02/2/0.03) → CONFLICT, and custom loadouts hit that fallback (bug G9 in gameplay-physics.md).
- `PassiveAbilitySO`: `passiveType` (15 real types + None), `value1/value2/flag1`, `unlockLevel` (default 10, ignored at runtime), `requiredAccountLevel`, archetype flags. `ResetAllPassives` + `ApplyToShip` set bool flags on `PlayerShip`.
- `MoveTypeSO`: Normal / Precision / Warp with slingshot params; warp params (`warpZoomDuration`, …) are **not** applied to `PlayerShip` (commented out in `ApplyToShip`; `PlayerShip` has its own serialized copies). `OnValidate` enforces Precision ≠ Tank and Warp = Controller-only, but the generator sets Warp `allowDamageDealer = allowAllAround = true` → `OnValidate` will flip them back to false the next time the asset is touched in the Inspector (CONFLICT between generator and validator).
- `ArchetypeRestrictionChecker`: static duplicate of `ShipPresetSO.Validate` plus OP-combo warnings. Zero callers → dead.

## Hand-made reference assets (`Assets/Ship System/`, outside Resources)

| Asset | Type | Key values |
|---|---|---|
| Star Sparrow.asset | ShipPresetSO | body = Star Sparrow Frame, formula AllAroundLevelingMain, passive Unmovable, perks Multi Missile SO / Cluster_missile / Explosive_Missile, move Standard Move, missile Standard |
| Star Sparrow Frame.asset | ShipBodySO | AllAround, 15000 HP, 80 armor, ×1.20, 3 AP, rot 50, all missiles |
| Standard.asset | MissilePresetSO | Medium, payload 2500, launch 0.1–20, maxVel 50, drag 0.01, fuel 100 @ 2/s, push 2 (file still carries obsolete `overridePhysicsMass/customPhysicsMass` fields; `physicsMass` falls back to the script default 1.5) |
| Standard Move.asset | MoveTypeSO | Normal, 2–10 speed, decel 4, 2.5 s |
| Unmovable.asset | PassiveAbilitySO | Unmovable, unlock 10 |
| 4× *LevelingMain.asset | ShipLevelingFormulaSO | see table above |

Because these are not under `Resources/`, `ProgressionManager.PopulateContentDatabases` (which uses `Resources.LoadAll`) never sees them: the "Star Sparrow" reference ship is **not** in the garage/builder databases, and `PlayerAccountData.InitializeDefaultUnlocks` unlocking `"Standard"` / `"Standard Move"` by name only matters for the hand-made `Standard Move` (the generator's prebuilt ships reference it directly) — the body id `"Standard"` matches no `ShipBodySO` at all (the body is named "Star Sparrow Frame").

## Generated content (`Assets/Editor/GameContentGenerator.cs` → `Assets/Resources/GeneratedContent/`)

Spot-checked assets match the generator source (icons assigned, exclusive perks present, `starter_ship` deliberately Tier-1 only). The orphan `BattlePass/Season1_BattlePass.asset` (30 tiers, `BattlePassData`) still exists although its generator and consumer were removed.

### Ship bodies (12)

| id | archetype | HP | armor | dmg× | AP | rot | acct lvl | L/M/H |
|---|---|---|---|---|---|---|---|---|
| body_allaround_standard | AllAround | 15000 | 80 | 1.20 | 3 | 50 | 0 | Y/Y/Y |
| body_allaround_tactical | AllAround | 15400 | 85 | 1.22 | 3 | 50 | 37 | Y/Y/Y |
| body_allaround_elite | AllAround | 15800 | 88 | 1.24 | 3 | 50 | 53 | Y/Y/Y |
| body_tank_reinforced | Tank | 16500 | 115 | 0.86 | 3 | 30 | 25 | N/Y/Y |
| body_tank_fortress | Tank | 17000 | 125 | 0.83 | 3 | 30 | 39 | N/Y/Y |
| body_tank_colossus | Tank | 17500 | 130 | 0.80 | 3 | 30 | 57 | N/N/Y |
| body_dd_striker | DamageDealer | 10000 | 60 | 2.05 | 3 | 70 | 31 | Y/Y/N |
| body_dd_reaper | DamageDealer | 9500 | 52 | 2.20 | 3 | 70 | 41 | Y/Y/N |
| body_ctrl_tactician | Controller | 10000 | 78 | 1.70 | 4 | 60 | 27 | Y/Y/N |
| body_ctrl_phantom | Controller | 9700 | 72 | 1.78 | 4 | 60 | 43 | Y/Y/N |
| body_seasonal_standard | AllAround | 15600 | 86 | 1.23 | 3 | 50 | 25 | Y/Y/Y |
| body_premium_elite | AllAround | 15900 | 89 | 1.25 | 3 | 50 | 23 | Y/Y/Y |

Balance method (generator comments): POWER = effectiveHP × damagePerHit ≈ 5.40e7 for every body; Star Sparrow mirror = 6 hits to kill.
DD/Controller missile flags: generator sets `heavy = false` for DD (README says DD can use all types) — CONFLICT with README/design table.

### Passives (17 generated + hand-made Unmovable)

`passive_shield_regen` (Regen 0.5/tick, acct 8, all), `passive_armor_boost_1/2` (DamageResistance 10 %/20 %, Tank, 5/22), `passive_fortified` (15 %, Tank, 35), `passive_damage_boost_1` (DamageBoost, DD, 11), `passive_critical_strike` (CriticalEnhancement, DD, 29), `passive_critical_immunity` (Tank, 46), `passive_lifesteal` (8 %, DD, 33), `passive_sniper_mode` (Controller, 17), `passive_unmovable` (Tank, 58), `passive_last_stand` (LastChance, Tank, 82), `passive_adaptive_armor` (Tank, 66), `passive_adaptive_damage` (DD, 54), `passive_precision_engineering` (Controller, 47), `passive_collision_avoidance` (Controller, 63), `passive_momentum` (+25 % high-speed, DD, 64), `passive_bulwark` (−25 % high-speed, Tank, 70). Archetype flags: target archetype + AllAround. No icons exist for passives or ships (`icon = null`).
Account levels 46–82 exceed the local level cap of 50 (`ProgressionManager.CheckAccountLevelUp` stops at 50) → 8 passives and 7 bodies are unreachable by leveling. (CONFLICT)

### Move types (3 generated + Standard Move)

`move_heavy_thrusters` (Normal, 1.5–7, acct 8, Tank/AllAround), `move_precision_drift` (Precision, 2–9, acct 15, not Tank), `move_warp_jump` (Warp, acct 30, Controller/DD/AllAround — see OnValidate conflict above).

### Prebuilt ships (17 `ShipPresetSO`)

| id | name | body | acct lvl | premium | passive | T1 / T2 / T3 | missile | move |
|---|---|---|---|---|---|---|---|---|
| starter_ship | Sparrow Trainer | allaround_standard | 0 | no | shield_regen | multi_t1 / – / – | standard_mk1 | Standard Move |
| nova_class | Nova Class | allaround_standard | 3 | no | shield_regen | multi_t1 / cluster_t2 / overcharged_t3 | standard_mk1 | Standard |
| titan_defender | Titan Defender | tank_reinforced | 6 | no | armor_boost_1 | pusher_t1 / cluster_t2 / explosive_t3 | heavy_titan | heavy_thrusters |
| phoenix_mk1 | Phoenix Mk-I | allaround_standard | 7 | no | damage_boost_1 | multi_t1 / explosive_t2 / pusher_t3 | standard_mk2 | Standard |
| eclipse_striker | Eclipse Striker | allaround_tactical | 10 | no | shield_regen | cluster_t1 / overcharged_t2 / explosive_t3 | standard_mk2 | Standard |
| bastion_class | Bastion Class | tank_fortress | 12 | no | fortified | pusher_t1 / barrage_t2 / overcharged_t3 | heavy_titan | heavy_thrusters |
| viper_assault | Viper Assault | dd_striker | 16 | no | lifesteal | multi_t1 / overcharged_t2 / cluster_t3 | light_swarm | Standard |
| juggernaut | Juggernaut | tank_colossus | 18 | no | armor_boost_2 | pusher_t1 / cluster_t2 / explosive_t3 | heavy_crusher | heavy_thrusters |
| reaper_class | Reaper Class | dd_reaper | 19 | no | critical_strike | overcharged_t1 / explosive_t2 / barrage_t3 | light_vortex | Standard |
| nexus_command | Nexus Command | ctrl_tactician | 26 | no | sniper_mode | cluster_t1 / pusher_t2 / barrage_t3 | standard_mk3 | precision_drift |
| seasonal_scout_free | Seasonal Scout | seasonal_standard | 10 | no (BP free 10) | momentum | multi_t1 / cluster_t2 / pusher_t3 | standard_mk1 | Standard |
| seasonal_defender_free | Seasonal Defender | tank_reinforced | 20 | no (BP free 20) | adaptive_armor | pusher_t1 / explosive_t2 / barrage_t3 | heavy_crusher | heavy_thrusters |
| premium_nebula_hunter | Nebula Hunter | dd_striker | 5 | yes | adaptive_damage | cluster_exclusive_t1 / overcharged_t2 / explosive_t3 | light_swarm | Standard |
| exclusive_stellar_dom | Stellar Dominator | premium_elite | 10 | yes | precision_engineering | multi_t1 / cluster_t2 / explosive_exclusive_t3 | standard_mk3 | Standard |
| premium_quantum_fortress | Quantum Fortress | tank_fortress | 15 | yes | unmovable | pusher_t1 / cluster_t2 / overcharged_t3 | heavy_titan | heavy_thrusters |
| premium_ethereal_phantom | Ethereal Phantom | ctrl_phantom | 20 | yes | collision_avoidance | cluster_t1 / pusher_t2 / barrage_t3 | light_vortex | precision_drift |
| ultimate_season_monarch | Season Monarch | ctrl_tactician | 25 | yes | last_stand | multi_t1 / overcharged_t2 / barrage_t3 | standard_mk3 | precision_drift |

Note: the two seasonal "free" ships have `requiredAccountLevel` 10/20 and `isPremiumShip=false`, so `ProgressionManager.UnlockShipsForLevel` also grants them by account level, not only via the battle pass (double path, probably unintended).
`nexus_command` (acct 26) and the DD ships are reachable by level; `ProgressionSystem.CLASS_*_UNLOCK_LEVEL` (Tank 5, DD 15, Controller 25) is a separate, unused gate.

## Ship builder (custom loadouts)

- Canonical rules: `ProgressionManager.ValidateLoadoutBuild` — name ≤ 30 chars; body required+unlocked; move type required+unlocked+archetype-ok; missile optional; exactly one perk per tier, unlocked, archetype-ok; exactly one passive; slot limit `ProgressionSystem.GetUnlockedCustomSlots` (1/2/3 at levels 1/20/40).
- `CustomShipBuilder` (id-based) and `ShipBuilderUI` (SO-based) both delegate to it. `ShipBuilderUI` is not in any scene and depends on a `componentButtonPrefab` that does not exist in the repo.
- `CustomShipLoadout` stores component **asset names**; `GetProgressionKey` = body|perks|move|passives (missile excluded so retrofitting keeps XP).
- `MatchLoadoutBridge.ApplyEquippedLoadout` resolves names against `ProgressionManager.all*` lists and builds a runtime `ShipPresetSO` (no `levelingFormula`, bug G9), sets `ship.shipXP` from `ShipProgressionEntry`.
- A brand-new local account has only `body_allaround_standard`, `Standard Move`, `standard_mk1` and **no perks or passives unlocked** (see account-progression.md), and the local code path never unlocks perks/passives/bodies by account level → the builder cannot produce a valid loadout (needs 3 perks + 1 passive) until the battle pass hands some out. [CONFLICT - needs decision]

## Open questions for the GDD
1. Which missile classes may DD use (README: all; generator: no heavy)?
2. Keep `ShipClass` and `ShipArchetype` both? (recommend: delete `ShipClass`, map tables to `ShipArchetype`.)
3. Should account level cap at 50 (then bodies/passives above 50 must be re-scheduled) or go to 100 (ExtendedProgressionData assumes 100)?
4. Warp move availability: Controller-only (`MoveTypeSO.OnValidate`, README) vs Controller/DD/AllAround (generator)?
5. Move hand-made reference assets into Resources or treat them as editor-only references?
