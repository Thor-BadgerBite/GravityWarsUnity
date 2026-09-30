# Bot / AI opponent (working notes)

Scope: `Assets/Bot/BotController.cs` (246 lines) and the hooks it uses in `GameManager.cs` / `PlayerShip.cs`.

## Status: IMPLEMENTED (fire-only bot), no move / perk / missile-choice behaviour

- **Attachment**: `GameManager` adds a `BotController` to player 2's ship at runtime when `player2IsBot` is true and calls `Configure(botDifficulty)` (`GameManager.cs`, see gameplay-physics.md for the spawn path). `HotSeat 1.unity` serialises `player2IsBot = 1`, `botDifficulty = 1` (verified in scene YAML); the stale `HotSeat.unity` has no bot fields.
- **Turn detection** (`BotController.Update`): every frame checks `GameManager.Instance.CurrentPlayer == _ship && _ship.controlsEnabled`; on its turn it sets `controlsEnabled = false` (blocks human input) and starts `PlayTurn()`.
- **Think time** (`PlayTurn`): `Random.Range(minThink, maxThink)` where `maxThink = min(maxThinkTime 3 s, turnDuration × 0.3)` and `minThink = min(1.2 s, maxThink)`. This is the only place the bot respects the turn timer.
- **Aiming** (`FindBestShot`): two-pass grid search. Pass 1: 60 angles (6° steps) × 3 launch velocities (40 %, 70 %, 100 % of `PlayerShip.GetLaunchVelocityRange()`); pass 2: ±6° in 1.5° steps × velocity ±15 % in 7.5 % steps around the best. Each candidate runs `SimulateShot`, up to `simulationSteps = 600` fixed steps.
- **Simulation** (`SimulateShot`): re-implements the missile flight model of `PlayerShip.PredictMissileTrajectory` / `Missile3D`: gravity from `GameManager.GetCachedPlanets()` via `Planet.CalculateGravitationalPull`, drag `vel *= 1 - drag·dt`, soft clamp to `maxVelocity` with `velocityApproachRate`, and the **`× 0.5` launch factor** (comment: "matches Missile3D.Launch"). Fallback constants when no missile preset is equipped: mass 1.5, drag 0.01, maxVel 10, approachRate 0.1. Planet collision approximated as `localScale.x × 0.5` radius. A candidate scores 0 (perfect) when it passes within `hitRadius = 1.6` units of the enemy transform.
- **Error model**: `angleError = gaussian × (1 − difficulty) × 10°`, `velocity × (1 + gaussian × (1 − difficulty) × 0.12)`; gaussian = sum of 3 uniforms − 1.5 (approx. N(0, 0.5)). At difficulty 1 the bot is exact to the search resolution.
- **Firing**: `PlayerShip.BotSetAim(angle, velocity)` then after 0.4 s `PlayerShip.BotFire()`.
- **What the bot never does**: move (any move type), activate perks, choose a missile, self-destruct a missile, or react to its own damaged parts. It targets `enemy.transform.position` only (fine while ships are static during the shot).

## Bugs / risks (BOT)

| ID | Sev | Where | Problem | Suggested fix |
|---|---|---|---|---|
| BOT1 | medium (perf) | `BotController.FindBestShot` / `SimulateShot` | 180 + 45 candidate shots × up to 600 steps × N planets are simulated synchronously inside one coroutine resume, i.e. one frame (≈ 0.8 M gravity evaluations with 6 planets). Visible hitch on the bot's turn, worse on mobile. | Spread candidates over frames (`yield return null` every N candidates) or run the search in a job/thread on copied planet data. |
| BOT2 | medium | `BotController.SimulateShot` | Fourth copy of the missile flight model and of the hard-coded `0.5f` launch factor (also in `Missile3D.Launch`, `PlayerShip.PredictMissileTrajectory`, and the preview). Any physics tweak silently desyncs the bot. | Extract one static `MissileFlightModel.Step(...)` used by `Missile3D`, the preview and the bot; put the launch factor in `MissilePresetSO` or a physics config asset. |
| BOT3 | low | `BotController` fields `hitRadius`, `simulationSteps`, think times, error scale `10f`/`0.12f` | Balance numbers live on the component, not in a config asset; `hitRadius` ignores the target ship's actual collider size. | Move to a `BotDifficultySO` (per difficulty tier) referenced by `GameManager`; derive hit radius from the enemy collider bounds. |
| BOT4 | low | `BotController.HitsPlanet` | Planet radius from `localScale.x * 0.5` disagrees with `Planet` colliders if a planet prefab is not unit-sphere scaled. | Use `Planet` collider radius (already needed by `Missile3D`). |
| BOT5 | design gap | whole class | No move / perk / missile usage → bot cannot exercise the archetype system, so ranked-like "Training" against a bot teaches only aiming. | GDD open question: scope of bot behaviour for the vertical slice. |
| BOT6 | low | `BotController.Update` | Per-frame `GameManager.Instance` + property polling for every bot ship; harmless at 1 bot but the turn start should be event-driven (`GameManager` already knows when a turn begins). | Have `GameManager` call `bot.OnTurnStarted()`. |

## Hard-coded numbers
`difficulty 0.6` default (scene overrides to 1), `minThinkTime 1.2`, `maxThinkTime 3`, `turnBudget × 0.3`, `simulationSteps 600`, `hitRadius 1.6`, angle step 6° / 1.5°, velocity fractions 0.4 / 0.7 / 1.0, refinement ±15 %, error 10° / 12 %, post-aim pause 0.4 s, fallback physics 1.5 / 0.01 / 10 / 0.1, launch factor 0.5.

## Verified statically / to check in editor
- Verified: bot fields in `HotSeat 1.unity`; `GameManager.GetCachedPlanets()` exists; `PlayerShip.BotSetAim/BotFire/GetLaunchVelocityRange` exist.
- To check in editor: whether the bot's turn actually ends when the missile is destroyed (depends on `GameManager` missile-flight phase, not on the bot), and the size of the frame hitch on the bot's first turn.
