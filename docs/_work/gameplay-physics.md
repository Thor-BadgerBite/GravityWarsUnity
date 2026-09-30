# Gameplay & Physics (working notes)

Scope: `Assets/GameManager.cs`, `Assets/PlayerShip.cs`, `Assets/Missile3D.cs`, `Assets/Planet.cs`,
`Assets/PlanetRotation.cs`, `Assets/CameraController.cs`, `Assets/HotSeatSetup.cs`, `Assets/PlayerUI.cs`,
`Assets/MatchStatsTracker.cs`, `Assets/KillshotRecorder.cs`, `Assets/MutatorSystem.cs`,
`Assets/TournamentMode.cs`, `Assets/AudioManager.cs`, `Assets/ScoreKeeper.cs`, `Assets/ScrollingBackround.cs`,
`Assets/DebugSettings.cs`, `Assets/DebugExtensions.cs`, scenes `HotSeat 1.unity` / `HotSeat.unity`.
Everything below was read statically; nothing was run in Unity.

## Status summary

| Feature | Status | Evidence |
|---|---|---|
| Turn-based hotseat match loop (setup screen → rounds → game over) | IMPLEMENTED | `GameManager.StartGame/StartGamePhase/StartPreparationPhase/StartPlayerTurn/EndTurn/ShipDestroyed/HandleShipDestruction/GameOver` |
| N-body-style gravity on missiles (mass-independent force) | IMPLEMENTED | `Planet.CalculateGravitationalPull` (F = G·M/r², G static 0.5), `Missile3D.ApplyGravity` |
| Trajectory preview (full/half depending on Sniper Mode) | IMPLEMENTED | `PlayerShip.PredictMissileTrajectory` (predictionSteps=100, half if `!sniperMode`) |
| Procedural planet spawning with unit budget + overlap repositioning | IMPLEMENTED | `GameManager.SpawnPlanetsWithSeed/FindValidSpawnPosition/RepositionPlanets` |
| Ship placement with clearance search | IMPLEMENTED | `GameManager.GetValidShipPosition/ClearanceFromPlanets` |
| Slingshot move (velocity slider → decelerating push) | IMPLEMENTED | `PlayerShip.PerformSlingshotMove/SlingshotCoroutine` |
| Precision move (ghost preview) | IMPLEMENTED | `PlayerShip.PositionGhostShip`, ghost clone built in `PlayerShip.Start` |
| Warp move (teleport with zoom/shake) | PARTIAL | `PlayerShip.WarpShip/WarpSequence`; does not end the turn, see bug G4 |
| Part-based damage (wing/weapon/plasma/engine/core multipliers) + part detachment | IMPLEMENTED | `Missile3D.HandleCollision/FindPartRoot/GetPartMultiplier/DetachPart` |
| Armor formula, passives in damage pipeline | IMPLEMENTED | `PlayerShip.TakeDamage`, `Missile3D.HandleCollision` |
| Knockback on hit | PARTIAL/BUGGY | `Missile3D.HandleCollision` gates knockback on the *target's* `isPassiveUnlocked` (bug G5) |
| Manual detonation (Space mid-flight) with AoE | IMPLEMENTED | `Missile3D.Update/SelfDestruct/CanManuallyDetonate` |
| Fuel, lost-in-space detection | IMPLEMENTED | `Missile3D.Update` (fuel), `Missile3D.CheckIfMissileIsLost` |
| Dynamic camera (follow, zoom-out, proximity zoom, slow-mo) | IMPLEMENTED | `CameraController` |
| Best-of-N rounds, comeback +1 AP for previous loser | IMPLEMENTED | `GameManager.winningScore`, `GameManager.ApplyTurnBonuses` |
| Weekly mutators | IMPLEMENTED, OFF | `MutatorSystem`, `GameManager.enableWeeklyMutators` = false in `HotSeat 1.unity` |
| Tournament (normalized level) mode | IMPLEMENTED, UNREACHABLE | `TournamentMode.Enabled` is never set by any code |
| Bot opponent | IMPLEMENTED | `Bot/BotController` (see bot-ai.md) |
| Killshot replay + trickshot detection | IMPLEMENTED (recorder), UI needs scene wiring | `KillshotRecorder`, `UI/KillshotReplayUI` |
| Match stats | IMPLEMENTED | `MatchStatsTracker` |
| Online turn sync | DESIGNED - NOT BUILT (guarded by `UNITY_NETCODE_GAMEOBJECTS`, define not set) | `GameManager` `#region Network Multiplayer Support`, `PlayerShip` network branches commented "TEMPORARILY DISABLED" |

## Match flow (as coded)

1. `HotSeatSetup.Start` binds sliders to `GameManager.winningScore/turnDuration/preparationTime/unitsToSpawn`; `StartGame` copies names and calls `GameManager.StartGame`.
2. `GameManager.StartGame` → `InitializeGame` (`ClearExistingPlanetsAndShips`, `SpawnPlanets`, `PlaceShips`, `SetPlayerNames`, `UpdateFightingUI_AtRoundStart`, `SetupMoveDividers`) → `SetupPlayerUI` (old per-ship `PlayerUIPrefab`) → resets `MatchStatsTracker`/`KillshotRecorder` → calls `GameManagerQuestIntegration/AchievementIntegration/LeaderboardIntegration/Analytics` if attached (none are attached in any scene) → `StartGamePhase` (2 s wait, `StartPreparationPhase(player1Ship)`).
3. `StartPreparationPhase` stops the next player's over-time effects, starts the enemy's (regen/damage-boost run while it is the *opponent's* turn), then `PreparationPhase` counts down `preparationTime`, then `StartPlayerTurn` (controls on, perk `ResetPerTurn`, `TurnTimer` for `turnDuration`).
4. One action per turn: `PlayerShip.FireMissile` or `PerformSlingshotMove` → `GameManager.PlayerActionUsed`. Fire → `MissileFlightPhase` (fuel shown in the bubble timer) and the turn ends when `Missile3D.OnMissileDestroyed` fires (`OnMissileDestroyed` → `EndTurn` → `DelayedNextTurn` after `infoFadeDuration`). Move → `movesRemainingThisRound--` and `EndTurn` immediately. Timeout → `EndTurn("No action taken in time!")`.
5. Kill: `PlayerShip.TakeDamage` (HP ≤ 0) or `PlayerShip.OnCollisionEnter` with a `Planet` → `DestroyShipWithExplosion` + `GameManager.ShipDestroyed` (sets `roundEndPending`, stops timers, bumps `storedScore1/2`, calls round-end integrations) → `HandleShipDestruction` (`destructionDelay` realtime, fade) → `GameOver` or `StartNextRound` (`ResetForNewRound` re-runs `InitializeGame`; 4 s wait; the round loser starts).
6. `GameOver` → `AwardMatchProgression` → `MatchResultsUI.Show(summary)` if a results UI exists (none in any scene), else scene reload after `gameOverDuration`.

Note on "action points": `PlayerShip.movesAllowedPerTurn` (3, Controller 4) is really a **per-round** pool (`movesRemainingThisRound`) consumed by moves and perk costs; a shot always ends the turn regardless of remaining points. The naming (`PerTurn`) is misleading.

## Physics (numbers, where they live)

| Parameter | Value | Location |
|---|---|---|
| Gravitational constant G | 0.5 (static) | `Planet.gravitationalConstant` |
| Gravity force | G·M/r², independent of missile mass; acceleration = F/mass via `rb.AddForce` | `Planet.CalculateGravitationalForce`, `Missile3D.ApplyGravity` |
| Launch velocity factor | ×0.5 (hard-coded three times) | `Missile3D.Launch`, `PlayerShip.PredictMissileTrajectory`, `BotController.SimulateShot` |
| Drag | v *= 1 − drag·dt (drag from `MissilePresetSO.drag`, 0.01) | `Missile3D.MoveMissile` |
| Max velocity | soft clamp `Lerp(v, dir·maxVelocity, velocityApproachRate)` | `Missile3D.MoveMissile` |
| Fuel burn | `fuelConsumptionRate`·deltaTime in `Update` (frame-rate based, physics in FixedUpdate) | `Missile3D.Update` |
| Lost-in-space | force < 0.01, distance > 30, moving away, for 2 s | `Missile3D` fields |
| Planet masses (HotSeat 1 scene) | Mercury 300, Venus 500, Earth 600, Mars 400, Jupiter 1000 (2 units), Saturn 900 (2), Uranus 700 (2), Neptune 600 (2), Moon 150 (0 units) | `HotSeat 1.unity` GameManager.planetInfos |
| Playfield | width/height from `Camera.main.orthographicSize` and aspect | `GameManager.SetupSpawnArea` |
| Ship x-range | 29–31 from centre (HotSeat 1), 25–28 (HotSeat), topBottomOffset 5 | scene GameManager fields |
| Knockback impulse | missile momentum × 2 (hard-coded `forceScale`) | `Missile3D.HandleCollision` |
| Self-destruct | `detRadius` 4, `detDamageFactor` 0.5, push 2 (from missile preset) | `MissilePresetSO.ApplyToMissile` |
| Slingshot random factor | ×0.9–1.1 | `PlayerShip.PerformSlingshotMove` |
| Ship part explosion | force 5, radius 2 | `PlayerShip.ExplodeShip` |
| Impact detach force | Lerp(0.5, 6, speed/maxVelocity) | `Missile3D.HandleCollision` |
| Slow motion | timeScale 0.5 for 1 s, 0.3 s ramps, when missile within 10 u of a ship | `CameraController` |

## Damage pipeline (as coded)

`Missile3D.HandleCollision`:
`base = payload × Random(1±damageVariation)` (→ ×1 with attacker PrecisionEngineering) → `× partMult` (wing 0.85, weapon 0.90, plasma 1.10, engine 1.15, core 1.30; core with attacker CriticalEnhancement 1.5; core with target CriticalImmunity 1.0) → `× attackerDamageMultiplier` → min 1 → high-speed (≥ 0.8·maxVelocity) attacker +`highSpeedDamageAmplifyPercent`, target −`highSpeedDamageReductionPercent` → `ship.TakeDamage(raw)`; attacker Lifesteal heals `raw × lifestealPercent` (before armor); target `IncreaseAdaptiveArmor` (+10 %), attacker `IncreaseAdaptiveDamage` (+10 %).
`PlayerShip.TakeDamage`: `eff = raw × (1 − armor/(armor+400))` → `× (1 − damageResistancePercentage)` if DamageResistance → LastChance (once per round, survive at 1 HP) → HP −= eff → stats/killshot hooks → `StabilizeRotationOverTime`.
All passive effects require `isPassiveUnlocked` = `shipLevel >= 10` (hard-coded in `PlayerShip.Start` and `GameManager.PopulatePassivesUI`; `PassiveAbilitySO.unlockLevel` is never read at runtime).

## Controls (code vs README)

| Action | Code (`PlayerShip.Update`, `PerkManager.Update`, `Missile3D.Update`) | README.md |
|---|---|---|
| Rotate | Left/Right arrow | A / D |
| Power | Up/Down arrow | W / S |
| Fine tune | Shift | Shift |
| Toggle fire/move (warp instantly if warp move) | M | E |
| Fire / move / detonate | Space | Space |
| Perk arm | 1 / 2 / 3 | 1 / 2 / 3 |
| Cycle move types | (no such key; move type comes from the preset) | Tab |
| Steer missile in flight | (not implemented) | A / D |
CONFLICT: README documents a control scheme that does not exist in code.

## Scene wiring (read from YAML)

- `Assets/Scenes/HotSeat 1.unity` is the maintained match scene: `GameManager` has all current fields (bubble timer, health bars, perk icons, passives icons, `player2IsBot=1`, `botDifficulty=1`, `player1Preset`/`player2Preset` = `viper_assault`, `winningScore=1`, `unitsToSpawn=6`, `turnDuration=15`). `GameManager` is a child of the `GameScreen` object (hence the `SetParent(null)` in `Awake`). Contains `HotSeatSetup`, `CameraController`, `AudioManager`, `PlayerUI`.
- `Assets/Scenes/HotSeat.unity` is stale: its serialized `GameManager` predates the fighting-game HUD fields (`bubbleTimer`, `health1Bar`, `player1PerkIcons`, `passivesActiveIcons` … are absent) → `TurnTimer`/`UpdateFightingUI_AtRoundStart` would throw NullReferenceException. It is the scene `MainMenu.cs` tries to load by name.
- `Assets/Scenes/SampleScene.unity` uses `Obsolete/GameSetup.cs` + `GameManager` + `CameraController` + `AudioManager` + `ScrollingBackground` (legacy test scene) and **is in Build Settings** while neither HotSeat scene is.
- Ship prefab used by both HotSeat scenes: `Assets/Gravity1.prefab` (`PlayerShip` + `PerkManager`, preset `Ship System/Star Sparrow.asset`, `shipXP = 189050` → level 20, `missilePrefab` = `Assets/Missile.prefab`, `equippedMissile` = `Ship System/Standard.asset`, Rigidbody mass 10). Other ship prefabs in `Assets/` (`Gravity2`, `Sparrow 1`, `StarSparrow2 (1)`, `LeftPlayer`, `RightPlayer`) are not referenced by any scene.
- No scene contains `ProgressionManager`, `MatchResultsUI`, `SettingsUI`, `MissileSelectionUI`, `KillshotReplayUI`, the four `GameManager*Integration` components, or any networking component. All of those are code-only.
- `Missile.prefab` and `Missile3D.prefab` both carry `Missile3D`; `missilePrefab.prefab` carries the obsolete `Missile` class from `Obsolete/MissileOLD.cs`.

## Bugs / risks found (G = gameplay)

| ID | Sev | Where | Problem | Suggested fix |
|---|---|---|---|---|
| G1 | critical | `GameManager.AwardMatchProgression` | Calls `ProgressionManager.Instance.AwardMatchXP` twice (winner and loser) against the **same local account**. In hotseat/bot play the local player receives both awards: double XP/credits/battle-pass XP, `totalMatchesPlayed += 2`, and `currentWinStreak` is reset to 0 by the loser call right after the winner call increments it. | Award only the local player's result (`player1Won ? winnerResult : loserResult`), or give the bot/P2 no account. |
| G2 | critical | `MainMenu.cs` + Build Settings + `HotSeat.unity` | Menu loads scene `"HotSeat"`, which is not in Build Settings (`EditorBuildSettings.asset` lists SplashScreen, MainMenu, SampleScene only) and whose `GameManager` serialization is stale (missing HUD fields → NRE). The working scene is `HotSeat 1.unity`. | Decide the canonical match scene, add it to Build Settings, delete or re-serialize the other. |
| G3 | high | `GameManager.Awake` | `DontDestroyOnLoad` on an object holding scene references (UI texts, sliders, canvases, `setupScreen`). After any scene reload the surviving instance points at destroyed objects and the new scene's GameManager destroys itself. | Remove `DontDestroyOnLoad` from `GameManager` (keep only stateless managers persistent) and pass rematch data through a small persistent object. |
| G4 | medium | `PlayerShip.WarpShip` | Warp consumes `movesRemainingThisRound` *before* `FindWarpPosition` (a failed search still costs the point), never calls `GameManager.PlayerActionUsed` (turn continues, perks/stats not notified), and skips `MatchStatsTracker`. | Find position first, then route through `PlayerActionUsed` like slingshot. |
| G5 | medium | `Missile3D.HandleCollision` line ~910 | `if (!ship.unmovable && ship.isPassiveUnlocked)` gates knockback on the target being level ≥ 10; ships below level 10 are never pushed. | `if (!(ship.unmovable && ship.isPassiveUnlocked))`. |
| G6 | medium | `GameManager.MissileLostInSpace` | No `roundEndPending` guard and no `currentPlayer` null check (same race class as the fixed `OnMissileDestroyed`). | Mirror the guard from `OnMissileDestroyed`. |
| G7 | medium | `GameManager.OnMissileDestroyed` / `BeginMultiMissile` | Only Multi Missile registers 3 in-flight missiles. Cluster split (`Missile3D.SplitCluster`) and Missile Barrage spawn extra missiles that are not counted, so the first destroyed one ends the turn while others are still flying (and can hit during the opponent's preparation phase). | Count live missiles via a registry instead of a fixed counter. |
| G8 | medium | `Missile3D.Update` | Every live missile polls `Input.GetKeyDown(Space)`; with 3+ missiles in flight one key press detonates/splits all of them. | Let `GameManager` own the detonate input and forward to the newest missile. |
| G9 | medium | `MatchLoadoutBridge.ApplyLoadout` + `PlayerShip.UpdateStatsFromLevel` | Runtime preset for custom loadouts never sets `levelingFormula`, and `ShipPresetSO.GetLevelingFormula` falls back to `Resources.Load("{archetype}LevelingFormula")` which does not exist, so custom ships use the **hard-coded** formulas in `PlayerShip.UpdateStatsFromHardcodedFormulas` (AllAround +3 armor/lvl, +0.03 dmg) while prebuilt ships use the SO formulas (+2.5 armor, +0.025 dmg). | Assign the archetype formula in the bridge, or move the four formula assets into `Resources` with the expected names, and delete the hard-coded fallback. |
| G10 | medium | `GameManager.SumLevelUpXP` | Duplicates the account level formula (`1000 + lvl*500`) and assumes XP resets per level; `ProgressionManager.CheckAccountLevelUp` never subtracts XP, so the results screen over-reports XP. | Single source for the formula; report `result.accountXP` directly. |
| G11 | medium | `GameManager.PopulatePassivesUI`, `PlayerShip.Start` | Passive unlock level 10 hard-coded twice; `PassiveAbilitySO.unlockLevel` ignored. | Read the SO value. |
| G12 | low | `GameManager.PassiveType` (nested) vs global `PassiveType` (`PassiveAbilitySO.cs`) | Two enums with the same name and different ordering (global has `None` first); the HUD icon arrays are indexed by the nested one. | Keep one enum; map icons by `PassiveType` explicitly. |
| G13 | medium | `CameraController.FollowMissile/FocusAllShips` | `FindObjectsOfType<PlayerShip>()` twice per frame. | Cache the two ships from `GameManager`. |
| G14 | medium | `Missile3D.AddTrajectoryPoint` | `SetPositions(trajectoryPoints.ToArray())` every `trailPointInterval` (0.01 s) → O(n²) copying and an allocation per physics step per missile. | Append with `SetPosition(count-1)` and grow capacity. |
| G15 | medium | `Missile3D.CreateExplosionEffect`, `PlayerShip.CreateExplosionEffect`, `Missile3D.SetupTrajectoryLine`, ghost material instancing in `PlayerShip.Start` | New `Material`/`Texture2D` per missile/explosion, never destroyed → leak over a long session. Duplicated explosion code in two files. | One pooled explosion prefab; shared material. |
| G16 | low | `Missile3D.RotateMissile/VisualizeThresholds/UpdateTrajectory` | Debug draws every FixedUpdate. | Guard with `DebugSettings`. |
| G17 | low | `PlayerShip.Update` (netcode define) | `GameManager.Instance?.GetComponent<GameManagerNetworkAdapter>()` every frame. | Cache. |
| G18 | low | `PlayerShip.RegenerationCoroutine` | `new WaitForSeconds(0.05f)` allocated 20×/s. | Cache the yield instruction or tick in `Update`. |
| G19 | low | `MatchStatsTracker.RecordDamageTaken` | Every damage event counts as a hit, so multi/cluster/barrage yield accuracy > 100 %. | Count hits per missile, not per damage event. |
| G20 | low | `PlayerShip.CanUseMissile`, `GetAllowedMissileTypes` | Second, hard-coded missile-restriction rule set next to `ShipBodySO.canUse*` flags (and a third in `MissileRetrofitSystem.IsMissileCompatible`). | Keep body flags only. |
| G21 | low | `PlayerShip.FindWarpPosition/ShipOverlapsWithPlanet` | Duplicates `GameManager.ClearanceFromPlanets`. | Call the GameManager helper. |
| G22 | low | `Assets/Gravity1.prefab` | `shipXP = 189050` → every hotseat ship spawns at ship level 20 with all perk tiers unlocked (prebuilt presets never override `shipXP`), which hides the level gating during playtests. | Set prefab XP to 0; have the bridge set XP for prebuilt ships too. |
| G23 | low | `ScoreKeeper`, `ScrollingBackground`(only in stale scenes), `DebugExtensions` (only used by `Missile3D` debug circle), `TournamentMode` | Dead or unreachable code. | Delete or wire. |
| G24 | low | `GameManager.CountdownTimer`/`TurnTimer` | Timers advance by fixed decrements after `WaitForSeconds`, so they drift with frame time; string allocations every tick. | Use elapsed `Time.time`. |
| G25 | low | `Missile3D.SetupAudio` / `AudioManager.Setup3DAudioSource` | `spatialBlend = 0` "force 2D temporarily" → 3D audio is effectively disabled. | Decide 2D vs 3D. |
| G26 | low | `PlayerShip.OnCollisionEnter` | Any contact with a planet destroys the ship instantly (also after knockback). Confirmed live in IMPLEMENTATION_STATUS. Design decision needed (damage vs instant death). | Open question. |

## Hard-coded numbers that should be config

`Planet.gravitationalConstant`; the ×0.5 launch factor (3 places); armor constant 400 (`PlayerShip.TakeDamage`, `ShipLevelingFormulaSO.CalculateEffectiveHP`, generator comments); part multipliers; knockback ×2; passive unlock level 10; `PlayerShip.MAX_LEVEL` 20 and XP formula `200 + 75·L²`; comeback +1 / bonus cap 2 (`GameManager.ApplyTurnBonuses`); regen tick 0.05 s; damage-boost 120 s and ×2 cap; adaptive +10 %; `ExplodeShip` force/radius; `CameraController` slow-mo values (serialized but per-scene); bot search grid (`BotController`).
