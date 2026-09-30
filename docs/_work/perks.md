# Active perks (working notes)

Scope: `Assets/+Active Perks+/*` (17 scripts + 7 hand-made assets + icons), `Resources/GeneratedContent/Perks/*` (20),
`PerkManager` on the ship prefab, `ExtendedProgressionData.ACTIVE_UNLOCKS`.

## Architecture (IMPLEMENTED)

- `ActivePerkSO` (abstract SO): `perkName`, `tier` 1–3, `cost` (forced `= tier` by `OnValidate`), `icon`, `minLevel` (forced 5/15/20 by tier in `OnValidate`), archetype flags, `requiredAccountLevel`. `CreatePerk()` returns a runtime `IActivePerk`.
- `IActivePerk`: `Name/Tier/Cost`, `CanActivate(ship)`, `Activate(ship)`.
- `PerkManager` (MonoBehaviour on `Gravity1.prefab`): reads the three slots from `PlayerShip.shipPreset` (or its own inspector fields), instantiates perks; keys 1/2/3 toggle a slot (`ToggleSlot`: requires `shipLevel >= minLevel`, slot not used this turn, `CanActivate`); `PlayerShip.FireMissile` calls `ActivateToggledPerk()` **before** spawning (sets `next*` flags on the ship), `GameManager.PlayerActionUsed` → `OnActionExecuted` → `ConsumeToggledPerk()` deducts `Cost` from `movesRemainingThisRound`. `ResetPerTurn` clears tier-2 usage; tier-3 usage lasts the round (ships are re-instantiated each round). UI colours: black locked, yellow armed, gray can't, white ready. `ReloadSlotsFromPreset` exists for the loadout bridge.

## Families

| Family | SO | Runtime | Effect | Hand-made asset | Generated (T1 / T2 / T3, acct lvl) |
|---|---|---|---|---|---|
| Multi Missile | `MultiMissileSO` | `MultiMissilePerk` | next shot = 3 missiles at ±spread, payload × factor | Multi Missile SO (T1, 0.75, 5°) | 0.6/5° L9, 0.75/6° L36, 0.9/7° L75 |
| Cluster Missile | `ClusterMissileSO` | `ClusterMissilePerk` | next missile splits into 3 on Space (`Missile3D.SplitCluster`) | Cluster_missile (T2, 0.75, 5°) | 0.6/5° L13, 0.75/6° L40, 0.9/8° L83; exclusive T1 0.75/3° |
| Explosive Missile | `ExplosiveMissileSO` | `ExplosiveMissilePerk` | next missile gets `detRadius`, `detDamageFactor`, and **replaces** `pushStrength` | Explosive_Missile (T3, radius 15, ×2, push 15) | 8/×2/20 L16, 12/×3/30 L48, 16/×4/45 L88; exclusive T3 22/×3/60 |
| Pusher Missile | `PusherMissileSO` | `PusherMissilePerk` | next missile payload × factor, push × knockback | PusherMissile SO (T1, ×2 kb, 0.8 dmg, `perkName` empty) | 1.5/0.7 L19, 2/0.8 L55, 3/0.9 L93 |
| Overcharged Cannon | `OverchargedCannonSO` | `OverchargedCannonPerk` | ship `damageMultiplier *= x` until the shot is fired | Overcharged Cannon SO (T1, **damageMultiplier = 100**) | 1.3 L27, 1.5 L60, 1.8 L98 |
| Missile Barrage | `MissileBarrageSO` | `MissileBarragePerk` | coroutine fires 4 missiles (payload × factor, ±spread, interval) | MissileBarrage SO (T2, 0.4, 5°, 1 s) | 0.3/5°/1s L30, 0.4/5°/0.8s L67, 0.5/6°/0.6s L95 |
| Boost Jets | `BoostJetsSO` | `BoostJetsPerk` | sets `skipNextMoveEndsTurn` (move without ending the turn) | Boost_Jets (T2) | not generated (unreachable, see P4) |

`ExtendedProgressionData.ACTIVE_UNLOCKS` lists 20 further "abilities" (Afterburner, Energy Shield, Nova Bomb, Ultima, Omega Strike, …, levels 9–98) with no SO, no code → DESIGNED - NOT BUILT (aspirational table, flagged as such in the source).

## Rules: documented vs actual (CONFLICT)

| Rule | Where documented | Actual behaviour |
|---|---|---|
| Tier 1 unlimited per round, T2 once per turn, T3 once per round | `PerkManager.ActivateToggledPerk` comments, IMPLEMENTATION_STATUS | Every runtime perk except `OverchargedCannonPerk` sets its own `used = true` on first `Activate` and never resets → **every perk (T1 included) is once per round**. `BoostJetsPerk._usedThisTurn` likewise never resets. |
| Cost = tier action points | `ActivePerkSO.OnValidate` | Enforced on validate; generator also passes cost = tier. Consistent. |
| Unlock at ship level 5/15/20 | `ActivePerkSO.OnValidate`, README | Enforced by `PerkManager.ToggleSlot`; but every hotseat ship spawns at level 20 (prefab XP), so never observed. |
| Perks also gated by account level (`requiredAccountLevel` 9–98) | generator | Only matters for the builder (`IsUnlocked`); no local code ever unlocks perks by level (see account-progression.md). |
| Missile Barrage = "4 rapid-fire missiles" | README | `Activate` fires 4 missiles from a coroutine **and** `FireMissile` continues to spawn the normal single missile → 5 missiles; the extra 4 skip `ApplyMissilePreset` (prefab stats) and are not registered with `GameManager.BeginMultiMissile`, so the turn ends when the first of them dies. |
| Explosive push "base impulse" | `ExplosiveMissileSO` tooltip | `PlayerShip.FireMissile` assigns `missile.pushStrength = nextExplPushStrength` (replace, not multiply); default SO value 3× payload vs hand asset 2× vs generated 2–4×. |

## Bugs / risks (P = perks)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| P1 | high | `MissileBarragePerk.Activate/FireMissileBarrage` | 5 missiles instead of 4, prefab stats, turn-end race (see G7). | Make barrage a `next*` flag handled inside `PlayerShip.FireMissile` like the other families; register count with GameManager. |
| P2 | high | `Assets/+Active Perks+/Overcharged Cannon SO.asset` | `damageMultiplier: 100` (100× damage). Not referenced by any preset/prefab today, but any designer picking it gets a one-shot perk. | Set to 1.5. |
| P3 | medium | all `*Perk.cs` `used` flags | Tier rules not as documented (see table). | Remove per-instance `used`; let `PerkManager._usedThisTurn` be the only limiter. |
| P4 | medium | `BoostJetsPerk.CanActivate` requires Move mode but `PerkManager.ActivateToggledPerk` is only invoked from `PlayerShip.FireMissile` | Boost Jets can never fire (documented in IMPLEMENTATION_STATUS, still true). Hand asset Boost_Jets exists. | Call `ActivateToggledPerk` from `PerformSlingshotMove` too, or delete the perk. |
| P5 | low | `OverchargedCannonPerk.ResetAfterShot` | Waits for `ship.shotsThisRound > 0`; if the ship already shot this round the boost is removed on the next frame — fine because `FireMissile` spawns synchronously, but fragile. | Reset in `ConsumeToggledPerk`. |
| P6 | low | `PusherMissile SO.asset` | `perkName` empty → blank in UI. | Fill in. |
| P7 | low | `ActivePerkSO.OnValidate` | Hard-codes `cost = tier` and `minLevel` 5/15/20; not configurable. | Move to a config SO. |
| P8 | low | `PerkManager.HandleShotFired` | Dead method. | Delete. |
| P9 | low | `MissileBarrage` icon | Reuses Cluster icon (no art). | Art task. |

## Tunables
All family values above; tier costs 1/2/3; tier unlock ship levels 5/15/20; `GameManager.ApplyTurnBonuses` AP bonus cap 2.
