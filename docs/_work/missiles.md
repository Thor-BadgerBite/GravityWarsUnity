# Missiles & loadouts (working notes)

Scope: `Assets/MissilePresetSO.cs`, `Assets/Missile3D.cs` (physics side in gameplay-physics.md), `Assets/Missile.prefab`,
`Assets/Missile3D.prefab`, `Assets/missilePrefab.prefab`, `Assets/Online/MissileRetrofitSystem.cs`,
`Assets/UI/MissileSelectionUI.cs` (see ui-screens.md for the screen itself), generated `Resources/GeneratedContent/Missiles/*`.

## Data model (IMPLEMENTED)

`MissilePresetSO` (`GravityWars/Missile Preset`): name, `missileType` (global enum `MissileType { Light, Medium, Heavy }`), `visualModelPrefab`, `icon`, `requiredAccountLevel`, `physicsMass` (0.6–3.0, synced to a cosmetic `displayMass` 200–1000 lbs by `OnValidate`), launch range `minLaunchVelocity`/`maxLaunchVelocity`, `maxVelocity`, `drag`, `velocityApproachRate`, `fuel`, `fuelConsumptionRate`, `payload`, `pushStrength`, `damageVariation`, self-destruct radius/factor/push, trail/max-velocity colours, optional sounds (never used by `Missile3D`), tilt/banking.
`ApplyToMissile` copies everything onto the spawned `Missile3D` and sets `Rigidbody.mass`; `selfDestructPushStrength` is **not** applied (`pushStrength` is used for both impact and self-destruct — the code comments say so). Custom `launchSound/flyingSound/explosionSound` are never played.

## Missile roster

| id (asset name) | type | acct lvl | mass | payload | maxVel | fuel | push |
|---|---|---|---|---|---|---|---|
| Standard (hand-made, `Ship System/Standard.asset`) | Medium | 0 | 1.5 | 2500 | 50 | 100 | 2 |
| standard_mk1 | Medium | 1 | 1.5 | 2500 | 50 | 100 | 2.0 |
| standard_mk2 | Medium | 4 | 1.5 | 2600 | 51 | 105 | 2.1 |
| standard_mk3 | Medium | 11 | 1.5 | 2700 | 52 | 110 | 2.2 |
| light_swarm | Light | 8 | 1.0 | 2200 | 60 | 110 | 1.4 |
| light_vortex | Light | 14 | 1.0 | 2280 | 61 | 115 | 1.5 |
| light_phantom | Light | 22 | 0.95 | 2350 | 62 | 120 | 1.6 |
| tactical_emp | Light | 0 (BP premium 13) | 1.0 | 2240 | 60.5 | 112 | 1.45 |
| heavy_titan | Heavy | 9 | 2.2 | 2900 | 40 | 90 | 3.2 |
| heavy_crusher | Heavy | 17 | 2.3 | 3000 | 39 | 92 | 3.4 |
| heavy_apocalypse | Heavy | 27 | 2.4 | 3100 | 38 | 94 | 3.6 |

All generated missiles share launch 0.1–20, drag 0.01, approach 0.1, burn 2/s, variation ±10 %. Design intent (generator comments): classes differ on feel (light bends more in gravity because acceleration = F/mass), payload spread deliberately narrow (1.32×).
README's missile table ("Heavy → 500 m/s through gravity", "Tank can't use Light", "Controller can't use Heavy") only partly matches: the class caps are 38–62, not 500.

## Restriction rule sets (three, CONFLICT)

1. `ShipBodySO.canUseLight/Medium/HeavyMissiles` flags (data-driven; used by `ShipPresetSO.Validate`, `ProgressionManager.ValidateLoadoutBuild`, `MissileSelectionUI`).
2. `PlayerShip.CanUseMissile` / `GetAllowedMissileTypes`: hard-coded by archetype (Tank: Medium/Heavy; Controller: Light/Medium; others all). Callers: none found in project code → dead duplicate.
3. `GravityWars.Online.MissileRetrofitSystem.IsMissileCompatible`: id-based, uses a **different** `MissileType` enum (`Standard, Light, Heavy, Tactical, Piercing, Cluster, Ultimate`, same enum name in namespace `GravityWars.Online`) and `preferredClass`; its 19-entry unlock schedule contains 9 ids with no asset (`tactical_gravity`, `tactical_plasma`, `piercing_lance`, `piercing_railgun`, `cluster_nova`, `cluster_cascade`, `ultimate_singularity`, `ultimate_omega`) and lists `tactical_emp` as level 13 while the asset says 0. `GetMissileUnlock` is only called from `ProgressionSystem.GetUnlocksForLevel` (netcode-only path).

## Selection flow

- Prebuilt ship: `ShipPresetSO.defaultMissile` → `MatchLoadoutBridge.ApplyPreset` sets `PlayerShip.equippedMissile`.
- Custom loadout: `CustomShipLoadout.equippedMissileName` (optional at build time) → `MatchLoadoutBridge.ApplyLoadout`; `MissileSelectionUI` (not in any scene) writes it pre-match. Changing the missile does not change `GetProgressionKey` → ship XP is kept (design rule, implemented).
- Fallback: prefab `equippedMissile` (Gravity1 → `Standard.asset`).
- Unlocks: `PlayerAccountData.unlockedMissileIDs` seeded with `"standard_mk1"` and `"Standard"`; `ProgressionManager.GrantStarterContent` also unlocks the first Medium missile in the database (order bug: databases are empty at that moment on a fresh account). Battle pass grants `standard_mk2`, `light_vortex`, `tactical_emp`. No local code unlocks missiles by `requiredAccountLevel`.

## Bugs / risks (M = missiles)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| M1 | medium | `MissilePresetSO` vs `GravityWars.Online.MissileType` | Two enums named `MissileType`; the Online one describes classes that do not exist. | Delete the Online enum/table or regenerate it from assets. |
| M2 | low | `MissilePresetSO.ApplyToMissile` | `selfDestructPushStrength`, `launchSound`, `flyingSound`, `explosionSound`, `trailColor` (only applied if a child LineRenderer exists — the trail is created at runtime in `Missile3D.SetupTrajectoryLine`, unparented) are dead fields. | Apply or remove. |
| M3 | low | `Assets/missilePrefab.prefab` | Uses obsolete `Missile` (`Obsolete/MissileOLD.cs`). Not referenced by scenes. | Delete with Obsolete. |
| M4 | low | `Missile.prefab` vs `Missile3D.prefab` | Two prefabs with `Missile3D`; Gravity1 uses `Missile.prefab`; `Standard.asset.visualModelPrefab` points at `Missile3D.prefab`. | Keep one. |
| M5 | medium | `MissileBarragePerk.FireMissileBarrage` | Spawns missiles without `ApplyMissilePreset`, so barrage missiles use prefab defaults (mass 10, maxVel 10, payload 2500) instead of the equipped missile. | Route through `PlayerShip` spawn helper. |

## Tunables to carry into the GDD
Everything in the roster table; `physicsMass ↔ displayMass` factor 333.33 (`MissilePresetSO.OnValidate`); fuel → flight time = fuel / rate (50 s standard).
