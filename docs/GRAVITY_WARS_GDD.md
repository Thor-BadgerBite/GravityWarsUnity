# Gravity Wars – Game Design Document

**Version:** 0.3
**Genre:** Turn-based tactical space artillery (2D play on a 3D-rendered field) with a free-to-play, cosmetics-only live-service meta-game (Brawl-Stars-style hub, custom ships, ranked seasons, battle pass)
**Engine:** Unity 2022.3.34f1 (C# 9), built-in render pipeline, PhysX 3D physics on a Z-locked plane; Netcode for GameObjects 1.15 and Unity Gaming Services packages installed but inactive
**Status:** M1 in progress (2026-10-01). The build is the three canonical scenes `SplashScreen` → `MainMenu` (hub) → `Match` (OQ1 decided, §15.1); every scene load goes through `SceneNames`; the hub's PLAY NOW button loads the match (wired and tested by Thomas); a match awards XP, credits and battle-pass progress to the one local account exactly once (G1 fixed) and `GameManager` is scene-bound (G3 fixed). Hub panels are popups owned by `HubPopupHost` (dimmer behind, click outside closes, §15.3); the garage, settings and missile panels route through it in code. `Assets/Obsolete`, the dead menu scripts and the orphan battle-pass data are deleted (refactor step 4). Still open in M1, all editor work: placing `HubPopupHost` and wiring SHIPS GARAGE / Cancel, placing the results/settings/missile panels (refactor step 3). G22/S4 (prefab ship XP) is deferred to M2 refactor step 7 (decision 2026-10-01). The meta-game (hub, garage, ship builder, quests, achievements, leaderboards, battle pass, ranked, cloud save, online play) exists as code but is not wired into any scene, is partly stubbed, and contains competing implementations (three account-XP formulas, two ship-XP readings, two save models, two network stacks). Current milestone: **M1 – Playable build baseline** (see Implementation Plan).

This document is the single source of truth. When code and this document disagree, update one of them deliberately - never let them drift.

Tags used below: **[IMPLEMENTED]** works as described in code · **[PARTIAL]** exists but incomplete or buggy · **[DESIGNED - NOT BUILT]** intent only (docs or stub code) · **[CONFLICT - needs decision]** code, assets and/or docs disagree; see Open Questions. Every claim cites the file and, where useful, the class/method. `docs/_work/*.md` hold the detailed evidence; `docs/CODE_AUDIT.md` lists the defects by id (G, M, P, A, S, E, B, Q, AC, L, N, SV, BOT, U).

---

## 1. Pitch and design pillars

**Pitch.** Two ships face each other across a field of planets. Each turn you aim, set launch power and fire a missile that bends around the planets' gravity, or you reposition, or you arm a perk. First to strip the opponent's hull wins the round; best-of-N wins the match. Outside the match you level an account and individual ships, build custom loadouts from unlocked bodies, passives, perks and move types, and chase a battle pass and a ranked ladder. Nothing that affects the match can be bought.

**Design pillars** (from `README.md` "Design Philosophy", the archived GDD "Balance Philosophy", and `IMPLEMENTATION_STATUS.md`; all reaffirmed by this assessment as the intent to build against):

1. **Physics mastery beats stats.** Trajectory prediction, gravity slingshots and positioning decide matches (`PlayerShip.PredictMissileTrajectory`, `Planet.CalculateGravitationalPull`). [IMPLEMENTED]
2. **Archetype identity is enforced, not suggested.** Tank / Damage Dealer / Controller / All-Around differ in stats, missile access, move types and passive pools, and the data model rejects invalid combinations (`ShipBodySO.OnValidate`, `ShipPresetSO.Validate`, `ProgressionManager.ValidateLoadoutBuild`). [IMPLEMENTED, rules in conflict, §4]
3. **Tactical depth through the action economy.** Action points, part targeting (core 130 %), perk timing and the one-action turn make every turn a decision (`GameManager.PlayerActionUsed`, `Missile3D.GetPartMultiplier`, `PerkManager`). [IMPLEMENTED, AP model in conflict, §3.3]
4. **Fair free-to-play.** Everything that changes match outcome is earnable by playing; money buys cosmetics and convenience only; a free player can complete the battle pass within a season (§13). [DESIGNED - NOT BUILT as an economy; code partially contradicts, §10/§13]
5. **Always a next goal.** The hub shows the equipped ship, the next unlock and live progress after every match (`MainMenuController`, `NextUnlockWidget`, `MatchResultsUI` banners). [DESIGNED - NOT BUILT in scenes]

## 2. Glossary

| Term | Meaning (and where it lives) |
|---|---|
| Archetype | One of Tank, DamageDealer, AllAround, Controller (`ShipArchetype`, `Assets/Ship System/ShipEnums.cs`). `ShipClass` in `Online/ProgressionSystem.cs` is a duplicate to be removed. |
| Body | Chassis asset with base stats and missile permissions (`ShipBodySO`). |
| Prebuilt ship / preset | Complete ship asset: body + leveling formula + passive + 3 perks + move type + default missile (`ShipPresetSO`, 17 generated in `Resources/GeneratedContent/Ships`). |
| Custom loadout | Player-built ship stored by component names (`CustomShipLoadout` in `PlayerAccountData.cs`). |
| Passive | Always-on ability, active from ship level 10 (`PassiveAbilitySO`, flags on `PlayerShip`). |
| Active perk | Armed with keys 1/2/3, tier 1–3, costs tier AP, unlocks at ship level 5/15/20 (`ActivePerkSO`, `PerkManager`). |
| Move type | Normal (slingshot), Precision (ghost preview), Warp (teleport) (`MoveTypeSO`, `PlayerShip`). |
| Missile class | Light / Medium / Heavy (`MissileType` in `MissilePresetSO.cs`). |
| Action points (AP) | Per-round pool of moves/perk costs (`PlayerShip.movesAllowedPerTurn`, `movesRemainingThisRound`). |
| Turn / round / match | Turn = one player's action; round ends when a ship dies; match = best-of `winningScore` rounds (`GameManager`). |
| Preparation phase | Countdown before each turn (`GameManager.PreparationPhase`). |
| Account XP / level | Global progression, level cap 50 locally (`ProgressionManager.CheckAccountLevelUp`). |
| Ship XP / level | Per-loadout progression 1–20 (`ShipProgressionEntry`, `PlayerShip.RecalcLevelFromXP`). |
| BP XP | Battle-pass progression, 25 levels (`BattlePassSystem`). |
| Credits / Gems | Soft / hard currency (`PlayerAccountData.credits/gems`). |
| ELO / Rank | Rating 100–4000 mapped to 16 `CompetitiveRank`s (`ELORatingSystem`). |
| Season | Battle-pass and ranked period; identity `season_N` (`BattlePassSystem.currentSeason`). |
| Mutator | Weekly rule modifier, disabled (`MutatorSystem`). |
| Killshot / trickshot | Recorded final trajectory; gravity-assist bonus (`KillshotRecorder`). |
| Hotseat | Two players on one device (`HotSeatSetup`). Bot = AI player 2 (`BotController`). |

## 3. Core gameplay and physics

### 3.1 Match structure [IMPLEMENTED]
- Setup panel (`HotSeatSetup`) binds player names, `winningScore`, `turnDuration`, `preparationTime`, `unitsToSpawn` and calls `GameManager.StartGame`.
- `GameManager.InitializeGame` clears and respawns planets (`SpawnPlanetsWithSeed`, unit budget, overlap repositioning) and ships (`PlaceShips`, clearance search), applies loadouts (`MatchLoadoutBridge.ApplyPreset/ApplyEquippedLoadout`), adds the bot when `player2IsBot`.
- A round ends when a ship's HP reaches 0 or it touches a planet (`PlayerShip.TakeDamage`, `PlayerShip.OnCollisionEnter`); `GameManager.ShipDestroyed → HandleShipDestruction → StartNextRound/GameOver`. The round loser starts the next round. Best-of `winningScore` (scene default 1 → first kill wins).
- Comeback rule: the previous round's loser gets +1 AP (`GameManager.ApplyTurnBonuses`, cap 2 bonus).

### 3.2 Turn flow [IMPLEMENTED]
Preparation countdown → controls on for the active ship, perks reset (`PerkManager.ResetPerTurn`), `TurnTimer` runs `turnDuration` → exactly one action: **fire** (`PlayerShip.FireMissile`) or **move** (`PerformSlingshotMove` / precision / warp) → firing enters the missile-flight phase and the turn ends when the missile dies (`GameManager.OnMissileDestroyed`); moving ends the turn immediately; timeout ends the turn with no action.

### 3.3 Action points [CONFLICT - needs decision]
Code: `movesAllowedPerTurn` is a **per-round pool** (3, Controller 4) decremented by moves (1) and armed perks (tier cost) (`GameManager.PlayerActionUsed`, `PerkManager.ConsumeToggledPerk`); firing always ends the turn regardless of remaining AP. Archived docs (`Complete Progression Guide.md`, archived GDD) describe fire = 0 AP and repeatable within a turn, moves repeatable, and "moving ends the turn unless a passive allows fire-after-move". → **OQ2**.

### 3.4 Physics [IMPLEMENTED]
- Gravity: `F = G·M / r²` per planet, **independent of missile mass** (`Planet.CalculateGravitationalPull`, `Missile3D.ApplyGravity` applies acceleration `F/mass` so lighter missiles bend more). `G = 0.5` static. Old docs write `G·M₁·M₂/r²` — superseded by the code.
- Launch: `velocity = direction × launchVelocity × 0.5` (`Missile3D.Launch`; duplicated in `PlayerShip.PredictMissileTrajectory` and `BotController.SimulateShot`).
- Flight: drag `v *= 1 − drag·dt`, soft cap `Lerp(v, dir·maxVelocity, velocityApproachRate)` (`Missile3D.MoveMissile`); fuel burns per frame (`Missile3D.Update`); lost-in-space after 2 s of drifting away with negligible force (`Missile3D.CheckIfMissileIsLost`).
- Trajectory preview: 100 steps, full with Sniper Mode passive, half otherwise (`PlayerShip.PredictMissileTrajectory`).
- Missiles use a `Rigidbody` for motion and PhysX for collisions; ships have `Rigidbody` mass 10 (`Gravity1.prefab`).

### 3.5 Moves
- Normal slingshot: velocity slider → decelerating push, random ×0.9–1.1 (`PlayerShip.PerformSlingshotMove`). [IMPLEMENTED]
- Precision: ghost-ship preview of the landing spot (`PlayerShip.PositionGhostShip`). [IMPLEMENTED]
- Warp: instant teleport with zoom/shake (`PlayerShip.WarpShip`); costs AP before a valid spot is found, does not notify `GameManager.PlayerActionUsed`, does not end the turn. [PARTIAL, G4]
- Boost Jets perk (move without ending the turn) can never trigger. [PARTIAL, P4]

### 3.6 Damage [IMPLEMENTED]
`Missile3D.HandleCollision`: `payload × random(1 ± damageVariation)` (exact with Precision Engineering) × part multiplier (wing 0.85, weapon 0.90, plasma 1.10, engine 1.15, core 1.30; core 1.50 with Critical Enhancement; core 1.00 against Critical Immunity) × attacker damage multiplier; ≥ 0.8·maxVelocity impacts add attacker Momentum bonus and target Bulwark reduction. `PlayerShip.TakeDamage`: `× (1 − armor/(armor+400))`, × Damage Resistance, Last Chance (survive once per round at 1 HP). Lifesteal heals attacker on raw damage; Adaptive Armor/Damage +10 % per hit. Parts detach with impulse; knockback impulse = missile momentum × 2 — currently only applied to targets of ship level ≥ 10 [G5].
Planet contact destroys a ship instantly, including after knockback (`PlayerShip.OnCollisionEnter`). [CONFLICT - needs decision, OQ13]

### 3.7 Presentation [IMPLEMENTED]
Camera follows the missile, zooms to frame both ships, proximity zoom and 0.5× slow-motion within 10 units of a ship (`CameraController`). Part-based ship break-up and explosion particles (`PlayerShip.ExplodeShip`, `Missile3D.CreateExplosionEffect`). Audio via `AudioManager` (3D audio forced to 2D, G25).

### 3.8 Modifiers
- Weekly mutators (planet mass/size, AP bonus) exist and are switched off in the scene (`MutatorSystem`, `GameManager.enableWeeklyMutators = false`). [IMPLEMENTED, disabled]
- Tournament level normalisation (every ship fights at one reference level) is design intent only; the unreachable `TournamentMode` stub (`Enabled` was never set) was deleted in 0.3 (G23). If wanted, it becomes a flag on the match rules asset (OQ17). [DESIGNED - NOT BUILT]

### 3.9 Bot opponent [PARTIAL]
`BotController` (attached by `GameManager` when `player2IsBot`) simulates ~225 candidate shots with the game's own flight model, picks the closest approach and adds Gaussian aiming error scaled by `1 − difficulty`. It never moves, never uses perks, never changes missiles. Scene default difficulty 1.0. Details: `docs/_work/bot-ai.md`.

### 3.10 Match results and stats [PARTIAL]
`MatchStatsTracker` records damage, shots, hits, rounds; `KillshotRecorder` records the killing trajectory and detects gravity-assist trickshots; `GameManager.AwardMatchProgression` builds a `MatchResultsSummary` (XP, credits, ELO change 0 offline, level-up, BP tiers, first-win, streak, close-match, trickshot, rivalry) for `MatchResultsUI`. **Rule:** exactly one account is awarded per match, the local player's (always Player 1); Player 2 is a hotseat guest or the bot and owns no account (0.2, closes G1). No results panel exists in any scene yet; without one `GameManager.GameOver` returns to the hub after `gameOverDuration`.

### 3.11 Controls (as coded; `README.md` documents a different scheme, see audit)
| Action | Key |
|---|---|
| Rotate ship | ← / → |
| Launch power | ↑ / ↓ (Shift = fine) |
| Toggle fire / move mode (warp immediately if warp move) | M |
| Fire, execute move, detonate missile in flight, split cluster | Space |
| Arm perk slot | 1 / 2 / 3 |

## 4. Ships and archetypes

### 4.1 Archetype identity [IMPLEMENTED, rules in conflict]
| Archetype | Stat profile (generated bodies) | AP | Rotation | Missiles (body flags) | Move types | Passive pool (generator) |
|---|---|---|---|---|---|---|
| Tank | 16 500–17 500 HP, armor 115–130, dmg ×0.80–0.86 | 3 | 30 | Medium + Heavy (colossus: Heavy only) | Normal only | armor boosts, fortified, critical immunity, unmovable, last stand, adaptive armor, bulwark |
| Damage Dealer | 9 500–10 000 HP, armor 52–60, dmg ×2.05–2.20 | 3 | 70 | Light + Medium (**no Heavy** in generator; README/archived GDD say all) [CONFLICT] | Normal, Precision, Warp (generator) | damage boost, critical strike, lifesteal, adaptive damage, momentum |
| Controller | 9 700–10 000 HP, armor 72–78, dmg ×1.70–1.78 | **4** | 60 | Light + Medium | Normal, Precision, Warp | sniper mode, precision engineering, collision avoidance |
| All-Around | 15 000–15 900 HP, armor 80–89, dmg ×1.20–1.25 | 3 | 50 | all | Normal, Precision, Warp (generator) | every passive |

Sources: `Assets/Editor/GameContentGenerator.cs` and the generated `ShipBodySO`/`PassiveAbilitySO`/`MoveTypeSO` assets (`docs/_work/ships-archetypes.md` has the full tables). `ShipBodySO.OnValidate` hard-enforces Tank HP ≥ 11 000, DD/Controller ≤ 10 000, Controller AP 4, Tank no Light, Controller no Heavy. `MoveTypeSO.OnValidate` enforces Precision ≠ Tank and **Warp = Controller only**, contradicting the generator (Warp for DD/AllAround) → **OQ6**. Regeneration and Lifesteal are "NOT Tank" in the archived GDD but `passive_shield_regen` is generated for all archetypes → **OQ6**.

### 4.2 Ship composition [IMPLEMENTED]
A ship = body + leveling formula + **exactly one** passive + one perk per tier + move type; the missile is chosen separately (§5). Prebuilt ships (`ShipPresetSO`) and custom loadouts (`CustomShipLoadout`) both resolve to a runtime preset applied by `MatchLoadoutBridge` → `ShipPresetSO.ApplyToShip` → `PlayerShip.UpdateStatsFromLevel`.

### 4.3 Content inventory [IMPLEMENTED as data]
12 bodies, 17 prebuilt ships, 17 passives (+ hand-made `Unmovable`), 3 generated move types (+ hand-made `Standard Move`), 11 missiles, 20 perks — all produced by `GameContentGenerator` into `Resources/GeneratedContent`. Hand-made reference assets (`Ship System/Star Sparrow.asset`, `Star Sparrow Frame`, `Standard`, `Standard Move`, four `*LevelingMain` formulas) live outside `Resources` and are invisible to `ProgressionManager.PopulateContentDatabases`; `Gravity1.prefab` (the only ship prefab used by scenes) still references the Star Sparrow preset. Ship 3D models: `ShipBodySO.visualPrefab` is never read; every ship uses the prefab mesh. [PARTIAL]

### 4.4 Leveling formulas [CONFLICT - needs decision]
`ShipLevelingFormulaSO` assets (HP +3 %/lvl, armor +2.5/lvl, damage +0.018–0.045/lvl by archetype) are used by prebuilt ships; custom loadouts fall back to different hard-coded numbers in `PlayerShip.UpdateStatsFromHardcodedFormulas` because the bridge never assigns a formula and the `Resources.Load("{archetype}LevelingFormula")` lookup finds nothing [G9]. Rule: one formula source per archetype (the SO).

## 5. Missiles and loadouts

### 5.1 Classes and roster [IMPLEMENTED]
Light (mass 0.95–1.0, payload 2 200–2 350, max 60–62), Medium (1.5, 2 500–2 700, 50–52), Heavy (2.2–2.4, 2 900–3 100, 38–40); launch 0.1–20 for all, drag 0.01, burn 2/s, ±10 % variation (`docs/_work/missiles.md` table, `MissilePresetSO`). Design intent: classes differ in feel (light bends more), payload spread deliberately narrow. Display mass = physics mass × 333.33 (`MissilePresetSO.OnValidate`).

### 5.2 Rules
- The missile is **not** part of the ship's progression key: swapping missiles keeps ship XP (`CustomShipLoadout.GetProgressionKey`). [IMPLEMENTED]
- Missile access is decided by the body flags `canUseLight/Medium/HeavyMissiles` (`ShipBodySO`), checked by `ShipPresetSO.Validate`, `ProgressionManager.ValidateLoadoutBuild`, `MissileSelectionUI`. Two other rule sets exist and must go: `PlayerShip.CanUseMissile` (hard-coded) and `MissileRetrofitSystem.IsMissileCompatible` (id-based, different `MissileType` enum, 9 missiles that do not exist). [CONFLICT, M1/G20]
- Pre-match selection through `MissileSelectionUI` (not in any scene) writes `CustomShipLoadout.equippedMissileName`; prebuilt ships use `ShipPresetSO.defaultMissile`; fallback is the prefab's `Standard`. [PARTIAL]
- Unlocks: `requiredAccountLevel` on each missile is not applied by the local progression path (only the battle pass grants `standard_mk2`, `light_vortex`, `tactical_emp`). [PARTIAL, A3]

## 6. Perks

### 6.1 Structure [IMPLEMENTED]
Seven families (`Assets/+Active Perks+`): Multi Missile (3-shot spread), Cluster Missile (split on Space), Explosive Missile (AoE, replaces push), Pusher Missile (knockback), Overcharged Cannon (damage multiplier for the next shot), Missile Barrage (4 sequential missiles), Boost Jets (move without ending the turn). Each family has a generated T1/T2/T3 asset with account-level requirements 9–98 plus two "exclusive" premium sidegrades (`cluster_missile_exclusive_t1`, `explosive_missile_exclusive_t3`).
Rules enforced by `ActivePerkSO.OnValidate`: `cost = tier`, `minLevel` = 5 / 15 / 20 by tier. `PerkManager.ToggleSlot` refuses slots below ship level; arming happens with keys 1/2/3, the perk fires inside `PlayerShip.FireMissile`, AP are deducted in `GameManager.PlayerActionUsed`.

### 6.2 Usage limits [CONFLICT - needs decision]
Intended (comments in `PerkManager.ActivateToggledPerk`, `IMPLEMENTATION_STATUS.md`): T1 unlimited per round, T2 once per turn, T3 once per round. Actual: every runtime perk except Overcharged Cannon sets its own `used` flag on first activation and never resets → all perks once per round [P3]. → **OQ7**.

### 6.3 Known defects
Missile Barrage fires 5 missiles with prefab stats and races the turn end [P1]; hand-made `Overcharged Cannon SO.asset` has `damageMultiplier = 100` [P2]; Boost Jets unreachable [P4]. Twenty further "actives" in `ExtendedProgressionData.ACTIVE_UNLOCKS` (Afterburner, Energy Shield, Nova Bomb, …) have no assets or code. [DESIGNED - NOT BUILT]

## 7. Ship builder

Rules (canonical, `ProgressionManager.ValidateLoadoutBuild`; also `Documentation` guides now archived): name 1–30 chars; body owned; move type owned and archetype-compatible; exactly one owned, compatible passive; exactly one owned, compatible perk per tier; missile optional; a free custom slot. Slots: 1 at start, 2 at account level 20, 3 at level 40 (`ProgressionSystem.GetUnlockedCustomSlots`). Editing perks or passive of a loadout starts a new progression entry (§9). Deleting a loadout forfeits its XP (docs). [IMPLEMENTED as logic]
Two facades (`CustomShipBuilder` id-based, `ShipBuilderUI` SO-based) delegate to the same validator; the UI is not in any scene and needs a prefab that does not exist. A fresh local account cannot satisfy the rules (no perks or passives unlocked, A3). [PARTIAL]

## 8. Account progression

### 8.1 Local path (the one that runs) [PARTIAL]
- Account is created by `ProgressionManager.CreateNewAccount` → `GrantStarterContent` (1000 credits, 50 gems, `starter_ship`, `body_allaround_standard`, `standard_mk1`, `Standard Move`) — runs before the content databases are populated [A1].
- Match award (`ProgressionManager.AwardMatchXP`): 50 base + 100 win + 25 per round won + damage/100 + 25 per trickshot + 50 close-match consolation; win-streak credits 50/100/200 at 3/5/10; premium pass +50 % XP; the same total goes to the used custom loadout as ship XP and to the battle pass (×2 first win of the day).
- Level-up: `while currentXP ≥ 1000 + level·500 && level < 50` with cumulative XP never reduced [A2]; `xpForNextLevel` never updated. Two other formulas exist (`ProgressionSystem.CalculateXPForLevel` exponential 1000·1.15^(L−1), display formula in `ProgressionUI`/`GameManager.SumLevelUpXP`). [CONFLICT - needs decision, OQ3]
- Unlocks by level: only non-premium `ShipPresetSO`s (`UnlockShipsForLevel`). Bodies, passives, perks, move types and missiles carry `requiredAccountLevel` but nothing grants them locally [A3]. The id tables `ExtendedProgressionData` (1–100), `ProgressionSystem.SHIP_UNLOCKS`, `MissileRetrofitSystem.MISSILE_UNLOCKS` are a third, partly fictional schedule. [CONFLICT]
- Feature gates (`ProgressionSystem` constants): achievements 3, quests 5, custom match 5, leaderboard 8, **ranked 10**, custom slot 2/3 at 20/40, clan 30. Only the ranked gate is read (`MainMenuController.CheckRankedUnlocked`).
- Save: `SaveSystem` JSON in `persistentDataPath/Saves/`; `DateTime` fields do not survive `JsonUtility` [A9].

### 8.2 Online path [DESIGNED - NOT BUILT]
`MatchHistoryManager` (compiled out) awards casual/ranked XP, credits, ELO and level-up rewards with its own formulas; `AccountSystem` (Unity Authentication username/password, not in any scene) creates profiles with 1000 credits / 0 gems and rank Lieutenant at ELO 800 (Ensign) [A5].

## 9. Ship progression

- Levels 1–20, `XP(L) = 200 + 75·L²`. `PlayerShip.RecalcLevelFromXP` treats it as the cost of the next level (subtractive); `ShipProgressionEntry.GetXPRequiredForLevel` treats it as cumulative; the same XP yields different levels in match vs garage [S1]. [CONFLICT - needs decision, OQ4]
- Gates: passive active from level 10 (`PlayerShip.isPassiveUnlocked`, hard-coded; `PassiveAbilitySO.unlockLevel` ignored [G11]); perk tiers 5/15/20.
- Mastery titles Veteran/Ace/Master/Legend at 5/10/15/20 and mastery skin ids at 10/20 (`ShipProgressionEntry.GetMasteryTitle`, `PlayerAccountData.UnlockMasterySkin`; no skin assets). [PARTIAL]
- XP source: only custom loadouts gain XP (`ProgressionManager.AwardMatchXP`); prebuilt ships have no entry and fight at the prefab's `shipXP = 189050` → level 20 with every tier unlocked [S2, G22]. Setting the prefab XP to 0 (G22/S4) is deferred to refactor step 7 together with S2 (decision 2026-10-01): until prebuilt ships get XP of their own, prefab XP 0 would put every hotseat/bot ship at level 1 without perks or passive, so playtests keep level-20 ships for now. [CONFLICT - needs decision, OQ4]
- Progression key = body|T1|T2|T3|move|passives (`CustomShipLoadout.GetProgressionKey`): missile swaps keep XP, perk/passive edits reset it. [IMPLEMENTED; OQ4 asks whether edits should reset]

## 10. Economy and currencies

| Flow | Amount | Code | Status |
|---|---|---|---|
| New account | 1000 credits + 50 gems (local) / 1000 + 0 (online) | `ProgressionManager.GrantStarterContent`, `AccountSystem.CreateNewPlayerProfile` | [CONFLICT] |
| Win streak | 50 / 100 / 200 credits at 3 / 5 / 10 | `ProgressionManager.AwardMatchXP` | [IMPLEMENTED] |
| Battle pass free track | 500…5000 credits at 11 levels, 50 gems at 23 | `BattlePassSystem.FREE_TRACK_REWARDS` | [IMPLEMENTED] |
| Battle pass premium | 1000 credits at 1; 460 gems over the track | `BattlePassSystem.PREMIUM_TRACK_REWARDS` | [IMPLEMENTED] |
| Season peak-rank | 20…500 gems | `RankedSeasonSystem.GrantPeakRankRewards` | [IMPLEMENTED, never triggered] |
| Quest rewards | 75–5000 credits, 0–250 gems | `QuestService` + templates | [PARTIAL] |
| Achievement rewards | 100–10 000 credits | `AchievementService.AwardAchievementRewards` (commented out) | [DESIGNED - NOT BUILT] |
| Match credits, level-up rewards | online formulas | `MatchHistoryManager`, `ProgressionSystem.GetLevelUpReward` | [DESIGNED - NOT BUILT] |
| **Sinks** | Premium pass 1000 gems | `BattlePassSystem.PurchasePremiumPass` | [IMPLEMENTED] |
| | **Credits have no sink** (no shop, no purchasable unlocks) | – | [CONFLICT - needs decision, OQ8] |

All validation is client-side; `EconomyValidator`, `RateLimiter`, `SuspiciousActivityDetector` are unreferenced and the Unity Economy package is unused [SV4]. Cosmetics: SO types and an applier exist, zero assets, no UI [E-series]. [DESIGNED - NOT BUILT]

## 11. Quests and achievements

### 11.1 Quests [PARTIAL]
`QuestService`: 3 daily (24 h) / 3 weekly (7 d) / 5 season (90 d) slots filled from 24 `QuestDataSO` templates in `Resources/Quests/Templates` (table in `docs/_work/quests-achievements.md`); rewards credits/gems/account XP on completion **and** again on claim [Q1]; no persistence (fresh quests every launch) [Q2]; only PlayMatches, WinMatches, WinRounds, UsePerkNTimes, PlayMatchesWithArchetype, WinWithArchetype can progress because `GameManagerQuestIntegration` is not in any scene and its fire/hit/damage hooks are never called [Q3]. "Sniper" in template names means DamageDealer.

### 11.2 Achievements [PARTIAL]
`AchievementService`: 43 templates (combat, progression, collection, skill, social, secret), points, tiers; progress tracked in memory only [AC2]; **all rewards commented out** [AC1]; feeds mostly dead, so every win counts as flawless [AC5]. Platform (Steam/PS/Xbox) ids exist as data only.

## 12. Battle pass and ranked seasons

### 12.1 Battle pass [PARTIAL]
- 25 levels × 1000 BP XP; free and premium tracks with auto-grant on level-up (`BattlePassSystem.OnLevelUp → ApplyReward → PlayerAccountData.UnlockById`); premium purchase 1000 gems with retro-grant (`PurchasePremiumPass`). Reward tables are hard-coded dictionaries; all skin ids resolve to nothing. The orphan ScriptableObject battle pass (`BattlePassData`, `Resources/GeneratedContent/BattlePass/Season1_BattlePass.asset`) was deleted in 0.3 (B4); the `RewardType` enum it declared now lives in `Online/RewardType.cs`. [IMPLEMENTED as logic]
- BP XP source: every match (local path, = account XP, ×2 first win). Archived docs: "daily quests only". [CONFLICT - needs decision, OQ11]
- Season: `currentSeason = 1`, no start/end time (`seasonEndTimestamp` unused); a season changes only when the constant changes [B1]. [PARTIAL]
- F2P feasibility: 25 000 BP XP at ≈ 150–350 per match ≈ 80–150 matches; cannot be judged until season length exists (OQ9).

### 12.2 Ranked [DESIGNED - NOT BUILT for live play]
- ELO (`ELORatingSystem`): start 800, clamp 100–4000, K 40 (<10 games) / 32 (<50) / 24 / 16 (≥1800), expected-score formula, fair-match window ±150.
- 16 ranks (`CompetitiveRank`): Cadet <700, Ensign <1050, Lieutenant <1200, Lt Commander <1350, Commander <1500, Captain <1650, Senior Captain <1800, Commodore <1950, Rear Admiral <2100, Rear Admiral (UH) <2250, Vice Admiral <2400, Admiral <2550, High Admiral <2700, Fleet Admiral <2850, Supreme Admiral <3000, Grand Admiral 3000+. Duplicated in three classes [A-series]. The archived GDD lists different names/thresholds (Ensign 800 … Eternal Admiral 2200). [CONFLICT, OQ18]
- Season rollover (`RankedSeasonSystem.ApplySeasonRollover`, triggered by the battle-pass season check): peak-rank gems, exclusive skin id for Vice Admiral+, soft reset `(elo + 1200)/2` — pulls new players **up** [B5]; streak reset.
- Only `MatchHistoryManager` (compiled out) writes ELO; hotseat/bot matches never touch it. `MatchmakingService` (ELO window ±100 → ±400, 120 s timeout, casual FIFO) has no subscriber and no transport.
- Ranked unlock: account level 10 in code, level 5 + 3 ships in the archived GDD. [CONFLICT, OQ10]

## 13. Monetization rules

Rules stated by Thomas for this assessment and found in `IMPLEMENTATION_STATUS.md` / `README.md` / archived GDD; they are the design law for every future change:

1. **Cosmetic-only monetization.** Nothing purchasable changes stats, unlock speed of power, or matchmaking. [DESIGNED - NOT BUILT: there is no store or IAP code]
2. **Premium content is power-neutral.** Premium ships/bodies/perks are sidegrades with the same stat budget (`GameContentGenerator` POWER ≈ 5.40e7 for every body). [IMPLEMENTED for generated content]
3. **A free player can complete the battle pass within a season.** Requires a season length (OQ9) and the BP XP source decision (OQ11). [DESIGNED - NOT BUILT]
4. **Premium pass price** 1000 gems (`BattlePassSystem.PurchasePremiumPass`, `BattlePassUI`); the premium track returns 460 gems, so the pass is **not** self-funding as the archived docs promise ("earns back the next pass"). [CONFLICT - needs decision, OQ8]
5. **Premium pass grants +50 % account and ship XP** (`ProgressionManager.AwardMatchXP`). This is a convenience boost, not stat power; whether it is allowed under rule 1 is a decision. [CONFLICT - needs decision, OQ8]
6. No loot boxes, no energy/timers, no obscured pricing (archived GDD "No Dark Patterns"). [DESIGNED]
7. Gems are earnable in-game (battle pass, ranked seasons, quests) and purchasable with money. [PARTIAL: earnable paths exist; no purchase path]

## 14. Multiplayer and online services

| Service | Package | Code | Status |
|---|---|---|---|
| Authentication | `com.unity.services.authentication` | `ServiceLocator` (anonymous) **and** `AccountSystem` (username/password) | two competing bootstraps [SV2] |
| Cloud Save | `cloudsave` | `CloudSaveService` (real SDK calls, hash/backup/queue/conflict resolution) for two data models (`SaveData` v2 keys, `PlayerAccountData` v1 keys) | [DESIGNED - NOT VERIFIED]; second model is dead [SV1] |
| Lobby / Relay / Netcode | `lobby`, `relay`, `netcode.gameobjects` | `Assets/Networking` (stack A: action relay) and `Assets/Multiplayer` (stack B: deterministic lockstep with a turn state machine, plus a real-time round manager); all under `#if UNITY_NETCODE_GAMEOBJECTS` (define not set); enabling it fails to compile (missing `using`s, missing members) | [DESIGNED - NOT BUILT] [CONFLICT - needs decision, OQ12] |
| Leaderboards | `leaderboards` | `LeaderboardService` mock data, UGS calls TODO | [DESIGNED - NOT BUILT] |
| Analytics | `analytics` | `AnalyticsService` SDK calls commented out | [DESIGNED - NOT BUILT] |
| Economy | `economy` | unused | – |
| Match history / ELO | – | `MatchHistoryManager` (compiled out, blocking waits) | [DESIGNED - NOT BUILT] |

`GameManager` and `PlayerShip` already contain the hooks stack B needs (`GameManager.OnNetworkTurnStateChanged`, `SpawnPlanetsWithSeed`, `GetSpawnedPlanetData`, `PlayerShip.ExecuteNetworkFire`), so stack B is the closer fit; stack A is never called by `GameManager`. Details and defects N1–N13: `docs/_work/multiplayer.md`; services SV1–SV9: `docs/_work/services-cloudsave-analytics.md`.

## 15. UI screens and navigation

### 15.1 Scenes (`Assets/Scenes`) [IMPLEMENTED] (OQ1 decided 2026-09-30, option b)
The build is exactly three scenes, in this Build Settings order (`ProjectSettings/EditorBuildSettings.asset`):

| Scene | File | Role |
|---|---|---|
| `SplashScreen` | `SplashScreen.unity` | Boot: intro, any key → hub (`SplashScreenManager`). |
| `MainMenu` | `MainMenu.unity` (the former hub `MainMenuScene.unity`, same GUID) | The hub. Garage, settings, missile selection, results-return, quests, achievements, profile, leaderboard are **panels inside this scene**, never scenes of their own. |
| `Match` | `Match.unity` (the former `HotSeat 1.unity`) | The one match scene: hotseat and bot now, online in M7. Its setup panel (`HotSeatSetup`) decides the opponent (`GameManager.player2IsBot`, scene value: bot). |

Deleted with the decision: `HotSeat.unity` (stale copy), the placeholder `MainMenu.unity` test menu, `SampleScene.unity`. Scene names exist in code only as the constants of `SceneNames` (`Assets/UI/SceneNames.cs`); `LoadScene("literal")` and serialized scene-name fields are forbidden (CLAUDE.md). Scene transitions: Splash → `SceneNames.MainMenu`; hub → `SceneNames.Match` (`MainMenuController.PlayNow`, hotseat and training buttons); match → `SceneNames.Match` (Play Again) or `SceneNames.MainMenu` (Return, and the no-results fallback in `GameManager.GameOver`); online stack (compiled out) uses the same constants. The scripts that only served the deleted scenes (`MainMenuManager` in `MainMenu.cs`, `ScoreKeeper`, `ScrollingBackground`) were deleted in 0.3 (refactor step 4; G23, U8).

### 15.2 Screens
| Screen | Class | Status |
|---|---|---|
| Splash | `SplashScreenManager` (`SplashScreenManager.cs`, renamed in 0.2) | [IMPLEMENTED] |
| Hub (3D ship, player info, mode buttons) | `MainMenuController`, `MainMenuUI`, `ShipViewer3D`, `HubPopupHost` | [PARTIAL] `MainMenuController` placed, PLAY NOW → `PlayNow()` wired and tested (Thomas, 2026-10-01); `MainMenuUI`/`ShipViewer3D` references empty and `HubPopupHost` not placed yet; viewer finds no prefab (U2–U4). Ranked/casual say "M7", the other panel buttons say "M3" until wired |
| Ships garage | `ShipsGarageController/UI`, `ShipInventoryCard` | [PARTIAL] controller placed on the inactive `ShipsGaragePanel`; opens/closes as a hub popup through `HubPopupHost` (0.3); UI and card prefab not placed; equips body ids (U5) |
| Ship builder | `ShipBuilderUI` | [DESIGNED - NOT BUILT] |
| Missile selection | `MissileSelectionUI` | [PARTIAL] code complete, opens as a hub popup (0.3); no panel |
| Match setup | `HotSeatSetup` | [IMPLEMENTED] |
| Match HUD | `GameManager` fields, `PlayerUI` | [IMPLEMENTED] |
| Match results (+ killshot replay) | `MatchResultsUI`, `KillshotReplayUI` | [PARTIAL] code complete, no panel |
| Settings | `SettingsUI` | [PARTIAL] code complete, opens as a hub popup (0.3); no panel |
| Account progression, battle pass | `ProgressionUI`, `BattlePassUI` | [DESIGNED - NOT BUILT] |
| Quests, achievements, leaderboards | `QuestUI`, `AchievementUI`, `LeaderboardUI` | [DESIGNED - NOT BUILT] |
| Next-unlock widget, mastery badge | `NextUnlockWidget`, `ShipMasteryBadgeUI` | [DESIGNED - NOT BUILT] |
| Online matchmaking, lobby, connection status | `OnlineMatchmakingUI`, `LobbyUI`, `ConnectionStatusUI` | [DESIGNED - NOT BUILT] |
| Debug panel | `DebugSystemsUI` | [PARTIAL] targets the dead save model |
| Inventory, profile, friends, clan, shop, gem store, offers, training, news | – | [DESIGNED - NOT BUILT] (archived `Screen Catalog.md`, 18 screens) |

### 15.3 Navigation model [IMPLEMENTED in code, panels pending in editor]
Decided with OQ1: **three scenes, panels inside the hub.** The hub loads exactly one other scene, the match; every other hub screen is an in-scene panel (`ShipsGarageController.OpenGarage`, `SettingsUI.Show`, `MissileSelectionUI.ShowForLoadout`; results via `MatchResultsUI.Show` inside the match scene). `MainMenuController` no longer carries scene-name fields; it holds optional references to the hub panels (garage today, the rest in M3). Loop for M1: hub PLAY NOW → `Match` → setup panel → match → results panel (Play Again reloads `Match`, Return loads `MainMenu`) → hub.

**Hub popups (0.3).** Every panel that opens on top of the hub is a popup owned by `HubPopupHost` (`Assets/UI/MainMenu/HubPopupHost.cs`): one per hub scene, on `MainMenuCanvas/PanelsContainer`, holding the full-screen `BackgroundDimmer` image. Rules: while a popup is open the dimmer is active and drawn directly under it (draw order hub < dimmer < popup; popups are siblings of the dimmer under `PanelsContainer`); closing the popup hides the dimmer; clicking the dimmer closes the popup; one popup at a time (opening another closes the first). `Open(GameObject)` / `Close(GameObject)` are wireable from a Button OnClick for panels without a controller; panel controllers call the static `ShowPanel` / `HidePanel`, which fall back to plain `SetActive` when the scene has no host. The garage (`OpenGarage` / `CloseGarage`), `SettingsUI.Show/Hide` and `MissileSelectionUI.ShowForLoadout/Hide` already route through it. The dimmer stays disabled in the scene (enabled, it blocked every hub button); the host switches it on only while a popup is open and gives it a transition-less Button at runtime if it has none.

Hub scene state (editor, Thomas, 2026-10-01): `MainMenuController` object placed (Ship Viewer / Menu UI empty), PLAY NOW → `MainMenuController.PlayNow` wired and tested, `ShipsGaragePanel` starts inactive, `BackgroundDimmer` disabled. Pending editor work (M1): place `HubPopupHost`; wire SHIPS GARAGE → `ShipsGarageController.OpenGarage` and the garage's Cancel → `ShipsGarageController.CloseGarage` (the committed `MainMenu.unity` carries only the PLAY NOW OnClick); place the panels of refactor step 3.

---

## Tunable Parameters

"Where" marks **(hard-coded)** literals that must move to config, **(SO)** ScriptableObject data, **(scene)** serialized scene/prefab fields, **(const)** C# constants in a static rule class (acceptable until a config asset exists).

| Parameter | Current value | Where it lives in code |
|---|---|---|
| Gravitational constant G | 0.5 | `Planet.gravitationalConstant` (static, hard-coded) |
| Launch velocity factor | 0.5 | `Missile3D.Launch`, `PlayerShip.PredictMissileTrajectory`, `BotController.SimulateShot` (hard-coded ×3) |
| Trajectory preview steps | 100 (half without Sniper Mode) | `PlayerShip.predictionSteps` (scene/prefab) |
| Missile drag / approach rate / burn | 0.01 / 0.1 / 2 per s | `MissilePresetSO` (SO) |
| Missile launch range | 0.1–20 | `MissilePresetSO` (SO) |
| Missile class stats (mass, payload, maxVel, fuel, push) | see §5.1 | `MissilePresetSO` (SO) |
| Display-mass factor | 333.33 | `MissilePresetSO.OnValidate` (hard-coded) |
| Lost-in-space | force < 0.01, distance > 30, 2 s | `Missile3D` fields (prefab) |
| Self-destruct radius / factor / push | 4 / 0.5 / 2 | `MissilePresetSO` (SO) |
| Knockback impulse scale | 2 | `Missile3D.HandleCollision` (hard-coded) |
| Part multipliers | wing 0.85, weapon 0.90, plasma 1.10, engine 1.15, core 1.30, crit 1.50 | `Missile3D.GetPartMultiplier` (hard-coded) |
| Armor constant | 400 | `PlayerShip.TakeDamage`, `ShipLevelingFormulaSO.CalculateEffectiveHP` (hard-coded ×2) |
| High-speed threshold | 0.8 × maxVelocity | `Missile3D.HandleCollision` (hard-coded) |
| Adaptive armor/damage step | +10 % per hit | `PlayerShip` (hard-coded) |
| Damage boost ramp | ×2 over 120 s | `PlayerShip` (hard-coded) |
| Regeneration tick | 0.05 s | `PlayerShip.RegenerationCoroutine` (hard-coded) |
| Ship explosion force / radius | 5 / 2 | `PlayerShip.ExplodeShip` (hard-coded) |
| Slingshot randomness | ×0.9–1.1 | `PlayerShip.PerformSlingshotMove` (hard-coded) |
| Slow-motion | timeScale 0.5, 1 s, within 10 u | `CameraController` (scene) |
| Action points | 3 (Controller 4), comeback +1, bonus cap 2 | `ShipBodySO.actionPointsPerTurn` (SO); `GameManager.ApplyTurnBonuses` (hard-coded) |
| Match format (hotseat scene) | best-of 1, turn 15 s, prep 3 s, 6 planet units, bot on at difficulty 1.0 | `Match.unity` GameManager (scene) |
| Match format (online design) | 3 rounds, turn 60 s / 15 s, prep 3 s, flight 30 s | `MatchmakingService`, `MatchManager`, `NetworkTurnCoordinator` (hard-coded, conflicting) |
| Planet masses / units | Mercury 300, Venus 500, Earth 600, Mars 400, Jupiter 1000 (2), Saturn 900 (2), Uranus 700 (2), Neptune 600 (2), Moon 150 (0) | `Match.unity` `GameManager.planetInfos` (scene) |
| Ship spawn x-range | 29–31 (Match) | `GameManager` (scene) |
| Body base stats | §4.1 | `ShipBodySO` (SO, generated) |
| Body validation clamps | Tank ≥ 11 000 HP, DD/Ctrl ≤ 10 000, Ctrl AP 4 | `ShipBodySO.OnValidate` (hard-coded) |
| Leveling per level | HP +3 %, armor +2.5, dmg +0.018/0.025/0.036/0.045 | `ShipLevelingFormulaSO` assets (SO); fallback in `PlayerShip.UpdateStatsFromHardcodedFormulas` (hard-coded, different) |
| Ship XP curve | 200 + 75·L², cap 20 | `PlayerShip.XPNeededForNext`, `ShipProgressionEntry.GetXPRequiredForLevel` (hard-coded ×2, different semantics) |
| Passive unlock ship level | 10 | `PlayerShip.Start`, `GameManager.PopulatePassivesUI` (hard-coded); `PassiveAbilitySO.unlockLevel` (SO, ignored) |
| Perk tier unlock ship levels / costs | 5 / 15 / 20; 1 / 2 / 3 | `ActivePerkSO.OnValidate` (hard-coded) |
| Perk family values | §6, `docs/_work/perks.md` | perk SO assets (SO) |
| Mastery titles | 5 / 10 / 15 / 20 | `ShipProgressionEntry.GetMasteryTitle`, `ShipMasteryBadgeUI` (hard-coded ×2) |
| Match XP | base 50, win 100, round 25, damage/100, trickshot 25, close-match 50 | `ProgressionManager.AwardMatchXP` (hard-coded) |
| Win-streak credits | 50 / 100 / 200 at 3 / 5 / 10 | `ProgressionManager.AwardMatchXP` (hard-coded) |
| Premium XP bonus | +50 % | `ProgressionManager.AwardMatchXP` (hard-coded) |
| First-win BP multiplier | ×2 | `ProgressionManager.AwardMatchXP` (hard-coded) |
| Account XP threshold | 1000 + level·500, cap 50 | `ProgressionManager.CheckAccountLevelUp` (hard-coded); alt. 1000·1.15^(L−1) in `ProgressionSystem.CalculateXPForLevel` (const) |
| Starter grant | 1000 credits, 50 gems, starter ids | `ProgressionManager.GrantStarterContent`, `PlayerAccountData.InitializeDefaultUnlocks` (hard-coded); `AccountSystem` 1000/0 (hard-coded) |
| Custom slots | 1 / 2 / 3 at 1 / 20 / 40 | `ProgressionSystem` (const) |
| Feature unlock levels | achievements 3, quests 5, custom match 5, leaderboard 8, ranked 10, clan 30 | `ProgressionSystem` (const) |
| Content unlock levels | per asset (bodies 0–57, passives 5–82, perks 9–98, missiles 0–27, ships 0–26) | `requiredAccountLevel` on SOs (SO, generated); duplicate tables in `ExtendedProgressionData`, `ProgressionSystem.SHIP_UNLOCKS`, `MissileRetrofitSystem` (hard-coded) |
| Battle pass | 25 levels × 1000 XP, premium 1000 gems | `BattlePassSystem` serialized-on-runtime-object (effectively hard-coded); `BattlePassUI` "1000 Gems" (hard-coded) |
| Battle pass rewards | free/premium tables | `BattlePassSystem.FREE_TRACK_REWARDS/PREMIUM_TRACK_REWARDS` (hard-coded) |
| Season length | undefined | `BattlePassSystem.seasonEndTimestamp` (unused) |
| ELO | start 800, clamp 100–4000, K 40/32/24/16, fair ±150 | `ELORatingSystem` (const) |
| Rank thresholds | §12.2 | `ELORatingSystem.GetRankFromELO`, `PlayerAccountData.UpdateRankFromELO`, `RankedSeasonSystem.RankFromELO` (const ×3) |
| Season soft-reset anchor | 1200 | `RankedSeasonSystem.SOFT_RESET_ANCHOR` (const) |
| Peak-rank season gems | 20 … 500 | `RankedSeasonSystem` (const) |
| Matchmaking window | ±100 → ±400 (+50 / 5 s), 120 s timeout | `MatchmakingService` (hard-coded) |
| Quest slots / expiry | 3 / 3 / 5; 24 h / 7 d / 90 d | `QuestService`, `QuestInstance` (hard-coded) |
| Quest & achievement rewards | per template | `QuestDataSO`, `AchievementDataSO` assets (SO, generated) |
| Leaderboard submit limit / damage cap | 10 per min / 10 000 | `LeaderboardService` (hard-coded) |
| Cloud save rate / queue / sync | 5 s / 50 / 300 s | `CloudSaveService` (const) |
| Bot | difficulty 0.6 (scene 1.0), think 1.2–3 s, 600 steps, hit radius 1.6, error 10° / 12 % | `BotController` fields (prefab-less, hard-coded defaults) |
| Online rewards (stack B) | win 500 cr / 10 gems / 1000 XP, loss 200 / 0 / 500, +50 cr & +100 XP per kill | `NetworkGameManager.AwardMatchRewards` (hard-coded) |
| Scene names | `SplashScreen`, `MainMenu`, `Match` | `SceneNames` (const, `Assets/UI/SceneNames.cs`) – the only place; used by `SplashScreenManager`, `MainMenuController`, `MatchResultsUI`, `GameManager`, `LobbyUI`, `OnlineGameAdapter` |

## Open Questions

Each item: options, then the recommendation (R). Answers go into the section named and the Changelog.

1. **Canonical scenes and menu (§15).** ✅ **Decided (Thomas, 2026-09-30): option (b).** Three scenes in the build, in order: `SplashScreen`, `MainMenu` (the hub, `MainMenuScene.unity` renamed with its GUID kept), `Match` (`HotSeat 1.unity` renamed; hotseat/bot now, online later). `HotSeat.unity`, the old test `MainMenu.unity` and `SampleScene.unity` deleted (commit 959fd108). Hub screens are panels inside `MainMenu`; the hub loads only `Match`. Recorded in §15.1–15.3; code side (`SceneNames`, call sites) in 0.2. Options considered: (a) keep placeholder `MainMenu.unity` + `HotSeat 1.unity`; (c) scene per screen as `MainMenuController` assumed.
2. **Action-point model (§3.3).** (a) Keep code: one action per turn, AP pool per round for moves/perks; (b) docs: fire free and repeatable, moves repeatable, perks once per turn. **R: (a)** for the vertical slice — it is what is built and balanced ("Star Sparrow mirror = 6 hits"); revisit after playtests. Rename `movesAllowedPerTurn` → `actionPointsPerRound`.
3. **Account XP formula and cap (§8).** (a) Linear threshold `1000 + L·500` with XP subtracted per level, cap 50; (b) exponential `1000·1.15^(L−1)`, cap 100 to match the 100-level content schedule; (c) linear, cap 100. **R: (a)** and re-schedule the 15 assets above level 50 (or raise the cap to 100 with (c) if the long tail is wanted). Whatever is chosen becomes a single `AccountXPTable`.
4. **Ship XP semantics (§9).** (a) `200 + 75·L²` cumulative (garage reading); (b) per-level cost (in-match reading). Also: (c) do prebuilt ships progress? (d) does editing perks/passive reset XP? **R:** (a) cumulative — the archived docs' milestone table ("Level 10→11: 7 700 XP") reads it that way and `ShipProgressionEntry` already does; (c) yes, every ship (preset or custom) gets an entry keyed by preset id or loadout id; (d) no reset on edit — key by `loadoutID`, keep the missile rule.
5. **Equipped-ship identity (§7, §15).** `currentEquippedShipId` holds a body name (garage), a loadout id (missile selection, bridge) or a preset id. **R:** loadout id only; the garage equips or creates a loadout for the chosen body/preset.
6. **Archetype restrictions (§4.1).** DD heavy missiles: generator no / README yes. Warp: Controller-only (`MoveTypeSO.OnValidate`, docs) vs Controller/DD/AllAround (generator). Regen/Lifesteal for Tank: docs no / generator yes. **R:** DD no Heavy (keeps Tank's identity as the heavy-hitter), Warp Controller-only (archived GDD "EXCLUSIVE"), Regen and Lifesteal not for Tank. Fix the generator, delete the other rule sets.
7. **Perk usage limits (§6.2).** (a) T1 unlimited/round, T2 once/turn, T3 once/round (intent); (b) all once per round (actual). **R: (a)**, enforced only in `PerkManager`.
8. **Economy (§10, §13).** Credit sink: (a) cosmetics shop for credits; (b) buy content unlocks early with credits; (c) remove credits. Premium pass: gem-back 460 vs "self-funding" 1000; +50 % XP boost allowed? **R:** (a) credits buy cosmetics only; keep gems for the pass; raise gem-back to ≥ 1000 only if seasons are long enough for F2P to finish (rule 3); drop the XP boost from the premium pass (convenience boosts blur rule 1) or keep it explicitly as "convenience, not power".
9. **Season length and clock (§12).** (a) 90 days (docs), (b) 60, (c) 30. Ranked seasons = battle-pass seasons? **R:** 90 days, one season clock for both, driven by a `SeasonConfigSO` with start/end timestamps; then verify F2P completion ≈ 1–2 matches/day.
10. **Ranked unlock and reset (§12.2).** Unlock at level 10 (code) or 5 + 3 ships (docs); soft reset `(elo+1200)/2` or `−20 %, min 800`. **R:** level 10; soft reset toward `STARTING_ELO` (800): `elo = 800 + (elo − 800)/2`.
11. **Battle-pass XP source (§12.1).** (a) every match (code); (b) quests only (docs); (c) both, quests larger. **R: (c)** — matches keep the pass moving for people who ignore quests; quests give the big chunks that make dailies matter.
12. **Online architecture (§14).** (a) stack A action relay; (b) stack B lockstep with Unity physics; (c) stack B turn machine + host-authoritative simulation with a deterministic custom integrator (the one the preview and bot already use). **R: (c)**; delete stack A's match code, keep `LobbyManager`/`NetworkService` for UGS lobby and relay.
13. **Planet collision (§3.6).** (a) instant death (code); (b) heavy damage + bounce. **R: (a)** for the slice (already tuned around it), but knockback-into-planet should be visible in the results as cause of death.
14. **Bot scope (§3.9).** (a) fire-only with difficulty tiers; (b) also moves and perks. **R: (a)** for the slice with three named tiers (Easy 0.3, Normal 0.6, Hard 0.9) in a `BotDifficultySO`.
15. **Cosmetics for the slice (§10).** (a) none, remove skin ids from reward tables; (b) one colour-scheme cosmetic path end-to-end. **R: (b)** — the pass and mastery already promise skins; one working colour scheme proves the pipeline.
16. **Cloud data model (§14).** `PlayerAccountData` (used by the game) vs `SaveData` (used by nothing). **R:** `PlayerAccountData`; delete `SaveManager`/`SaveData`/`ServiceIntegrationHelper`.
17. **Match format defaults.** Hotseat scene best-of-1 / 15 s vs online 3 rounds / 60 s. **R:** best-of-3, 30 s turns, 3 s prep for both, in one `MatchRulesSO` with a hotseat override in the setup panel.
18. **Rank names/thresholds (§12.2).** Code (Cadet…Grand Admiral, 700–3000) vs archived GDD (Ensign…Eternal Admiral, 800–2200). **R:** code (also in `Rank System.md`); archived GDD is superseded.
19. **Audio.** 3D spatial audio forced to 2D (`AudioManager.Setup3DAudioSource`). **R:** 2D for a top-down game; remove the 3D setup.

## Implementation Plan

### Architecture principles actually used
- **Data-driven content** via ScriptableObjects (`ShipBodySO`, `ShipPresetSO`, `PassiveAbilitySO`, `MoveTypeSO`, `MissilePresetSO`, `ActivePerkSO` subclasses, `QuestDataSO`, `AchievementDataSO`), generated by editor windows (`GameContentGenerator`, `QuestTemplateGenerator`, `AchievementTemplateGenerator`) into `Resources/…` and loaded with `Resources.LoadAll` (`ProgressionManager.PopulateContentDatabases`). Rule going forward: generator is the source, assets are output.
- **One match orchestrator** (`GameManager`, 1 930 lines) owning scene references, timers, HUD and progression hand-off; `PlayerShip` (2 041 lines) owning input, stats and damage; `Missile3D` owning flight. Rule: no new responsibilities in these three; extract (flight model, HUD, results) as milestones require.
- **Lazy singletons** for persistent managers (`ProgressionManager`, `BattlePassSystem`, `QuestService`, `AchievementService`, `LeaderboardService`, `ServiceLocator`, `CloudSaveService`, `AnalyticsService`): `Instance` → `FindObjectOfType` → `AddComponent`, `DontDestroyOnLoad`. Scene-bound classes (`GameManager`, UI panels) are plain scene singletons: `Instance` set in `Awake`, cleared in `OnDestroy`, never `DontDestroyOnLoad` (G3 fixed in 0.2; a rematch is a fresh scene load).
- **Static rule classes** for pure logic (`ELORatingSystem`, `RankedSeasonSystem`, `ProgressionSystem`, `MutatorSystem`, `MatchLoadoutBridge`). Fine while their numbers are constants; any tuned number moves to a config SO.
- **Player state** = one `[Serializable]` graph (`PlayerAccountData`) saved with `JsonUtility` (`SaveSystem`). Timestamps as `long`.
- **Networking behind a define** (`UNITY_NETCODE_GAMEOBJECTS`), off until M7.

### Folder layout (current, with the target rule per folder)
```
Assets/
  GameManager.cs, PlayerShip.cs, Missile3D.cs, Planet.cs, CameraController.cs,
  HotSeatSetup.cs, MatchLoadoutBridge.cs, MatchStatsTracker.cs, KillshotRecorder.cs,
  AudioManager.cs, MutatorSystem.cs, BotController (Bot/)     ← match runtime (no new root files; new match code → Assets/Match/)
  SplashScreenManager.cs                                      ← boot scene (root today; moves to UI/ when next touched)
  Ship System/            ← ship data types + hand-made reference assets
  +Active Perks+/         ← perk SO types, runtime perks, PerkManager
  MissilePresetSO.cs      ← missile data type
  Progression System/     ← PlayerAccountData, ProgressionManager, SaveSystem, (UI/)
  Online/                 ← rules: ELO, ranked season, battle pass (+ RewardType), builder facade, account
  Quests/, Achievements/, Leaderboards/ ← data types, services (QuestService lives in Networking/Services), integrations, UI, Editor generators
  Networking/, Multiplayer/ ← UGS services and the two network stacks (define-guarded)
  CloudSave/              ← dead second save model (delete in M2)
  UI/                     ← hub (MainMenu/: controller, UI, viewer, HubPopupHost), garage, results, settings, missile selection, widgets, SceneNames (navigation constants)
  Debug/                  ← debug panel
  Editor/                 ← content generator
  Resources/GeneratedContent, Resources/Quests, Resources/Achievements ← generator output only
  Scenes/                 ← SplashScreen, MainMenu (hub), Match — the whole build (OQ1)
docs/GRAVITY_WARS_GDD.md, docs/CODE_AUDIT.md, docs/_work/, docs/_archive/
```
`Assets/Obsolete` was deleted in 0.3 (refactor step 4).

### Key classes and data flow
1. Boot: `SplashScreenManager` → `SceneNames.MainMenu`. `ProgressionManager.Instance` self-creates on first use, loads `PlayerAccountData` (`SaveSystem`), fills content databases, calls `QuestService.InitializeQuests`.
2. Hub: `MainMenuController` reads the profile (`AccountSystem` if signed in, else `ProgressionManager`), `ShipViewer3D` shows the equipped ship, `ShipsGarageController` equips, `MissileSelectionUI` sets the missile, `SettingsUI` writes `PlayerPreferences`; those three open as popups through `HubPopupHost` (owns `BackgroundDimmer`, one open popup at a time, §15.3); `MainMenuController.PlayNow` → `SceneNames.Match`.
3. Match: `HotSeatSetup` → `GameManager.StartGame` → `MatchLoadoutBridge` applies the preset/loadout to `PlayerShip` (`ShipPresetSO.ApplyToShip`, `PerkManager.ReloadSlotsFromPreset`) → turns (`PlayerShip` input, `Missile3D` flight, `Planet` gravity, `PerkManager` perks, `BotController` for AI) → `MatchStatsTracker`/`KillshotRecorder` collect.
4. End: `GameManager.AwardMatchProgression` → `ProgressionManager.AwardMatchXP` → account/ship/BP XP (`BattlePassSystem.AddBattlePassXP` → rewards via `PlayerAccountData.UnlockById`), quests (`QuestService.UpdateQuestProgress` when the integration is attached) → `SaveSystem` (+ async `CloudSaveService`) → `MatchResultsUI.Show(MatchResultsSummary)`.

### Milestones

| # | Milestone | Done when |
|---|---|---|
| M0 | Assessment 0.1 (this document, CODE_AUDIT, CLAUDE.md, archive) | Merged to `main`. ✅ |
| **M1 (current)** | **Playable build baseline** – ✅ canonical scene set (OQ1), ✅ Build Settings, ✅ `SceneNames` class, ✅ fix G1 (double award), ✅ G2 (scene), ✅ G3 (`DontDestroyOnLoad`), ✅ `SampleScene`/stale scenes removed; ✅ hub PLAY NOW wired to `MainMenuController.PlayNow` (editor, Thomas 2026-10-01, hub → Match tested); ✅ `MainMenuController` placed, `ShipsGaragePanel` inactive, `BackgroundDimmer` disabled (editor); ✅ `Obsolete` + dead menu scripts deleted (refactor step 4); ✅ `HubPopupHost` code, garage/settings/missile panels routed through it; ☐ `HubPopupHost` placed and SHIPS GARAGE / Cancel wired (editor); ☐ results/settings/missile panels placed in scenes (editor, refactor step 3); G22/S4 (prefab XP) moved to M2 step 7 (decision 2026-10-01) | A player build starts at the hub, plays a bot match from a hub button, shows the results panel with correct single-account XP, returns to the hub; console shows no exceptions across two consecutive matches. **Progress:** code loop hub → match → hub is in place and hub → match is tested in the editor; the results panel, the popup host and the garage buttons are editor work. |
| M2 | Progression foundation – OQ3/4/5 decided and implemented as single tables (`AccountXPTable`, `ShipXPTable`), `UnlockContentForLevel` from `requiredAccountLevel`, starter-content order (A1), timestamps (A9), delete duplicate tables/enums (`ExtendedProgressionData`, `ProgressionSystem.SHIP_UNLOCKS`, `MissileRetrofitSystem`, `ShipClass`, `SaveData`/`SaveManager`; `RankConfiguration` already gone in 0.3), prebuilt ships progress (S2) together with prefab ship XP 0 (G22/S4, deferred from M1 on 2026-10-01) | A fresh account reaches level 5 in the editor, sees the promised unlocks, can build a valid custom ship, and a save/load round-trip preserves everything including dates. |
| M3 | Hub vertical slice – hub wired per archived build guide (controller, UI, garage UI, cards), `ShipViewer3D` from `ShipBodySO.visualPrefab`, panel navigation, next-unlock widget, mastery badge, id-space fix (U5), starter grant removed from the garage (U7) | Every hub button does something; equip → play → results reflects the equipped loadout; no missing-reference errors. |
| M4 | Combat correctness – one `MissileFlightModel` shared by `Missile3D`, preview and bot (BOT2/N3 groundwork), physics constants in a `PhysicsConfigSO`, perk rules (OQ7, P1–P4), warp (G4), knockback gate (G5), live-missile registry (G7, G8), match rules SO (OQ17) | All seven perk families behave per §6 in a checklist playtest; bot shots match the preview; no per-frame allocations in missile flight (profiler check by Thomas). |
| M5 | Meta loop – quests persisted and fed from `MatchStatsTracker` events (Q1–Q3), achievement rewards and persistence (AC1, AC2, AC5), season clock (OQ9, B1), BP XP source (OQ11), credit sink v0 and cosmetics v0 (OQ8, OQ15), rewards tables into SOs (B2) | A simulated 90-day season (editor time offset) shows a free player completing the pass at the intended match rate; quests survive restarts; one cosmetic applies in hub and match. |
| M6 | Slice polish – bot tiers (OQ14), training mode entry, VFX/material pooling (G14, G15), timers (G24), audio decision (OQ19), controls page, tutorial hints | 30-minute playtest without hitches or leaks; README quick-start matches the build. **= playable vertical slice** |
| M7 | Online foundation – OQ12 executed: delete the losing stack, enable the define in a branch, compile clean, lobby + relay handshake, one full match between two editor instances with host-authoritative turns | Two clients finish a best-of-3 with identical results; disconnect ends the match cleanly. |
| M8 | Ranked and services – ELO written from online results, one cloud-save model (OQ16), UGS leaderboards, analytics with consent, server-side reward validation (Cloud Code) replacing the client-side validators | Ranked match changes ELO on both clients and the leaderboard; cloud save survives reinstall. |
| M9 | Live ops – season config remote, shop and gem purchase (IAP), premium pass purchase flow, events/mutators on schedule | First season runs end-to-end on the dashboard without a code change. |

## Changelog

| Version | Changes |
|---|---|
| 0.1 | Initial assessment (2026-09-30): full static read of the project code, scenes and prefabs; this GDD written from code as source of truth with archived documents as evidence of intent; `docs/CODE_AUDIT.md` (defects G/M/P/A/S/E/B/Q/AC/L/N/SV/BOT/U with severities and fixes, refactor plan, docs triage); `CLAUDE.md` working rules; old documents moved to `docs/_archive/`; backing notes in `docs/_work/`. No code, scene, prefab or asset was changed. |
| 0.2 | M1, refactor steps 1–2 (2026-09-30). **OQ1 decided** (option b): scenes `SplashScreen`, `MainMenu` (hub), `Match`; stale scenes deleted and Build Settings set by Thomas (commit 959fd108); §15.1–15.3 rewritten. New `SceneNames` constants class (`Assets/UI/SceneNames.cs`) replaces every scene literal (`SplashScreenManager`, `MainMenu.cs`, `LobbyUI`, `OnlineGameAdapter`, `MatchResultsUI`, `GameManager`) and the serialized scene-name fields of `MatchResultsUI` and `MainMenuController` (ten scene-per-screen fields removed; hub buttons now open in-scene panels or say M3/M7; new `MainMenuController.PlayNow()` for the hub button). `GameManager.AwardMatchProgression` awards the local player (Player 1) once instead of winner and loser. `GameManager` no longer `DontDestroyOnLoad` (scene singleton; `Instance` cleared in `OnDestroy`); `GameOver` without a results panel returns to the hub instead of reloading. `SplasScreenManager.cs` renamed `SplashScreenManager.cs` (GUID kept). **Audit closed:** G1/A4, G2, G3, U1, U3, U11, N5; H1 (splash part), H6 (scene part). **Audit added/updated:** U8 (`MainMenuManager` dead), G23 (`ScoreKeeper`, `ScrollingBackground` dead). Static verification only; editor checks listed in the session notes. |
| 0.3 | M1, refactor step 4 + hub popups (2026-10-01). **Editor (Thomas, on main):** PLAY NOW → `MainMenuController.PlayNow` wired and tested (hub → Match plays), `MainMenuController` object added to `MainMenu.unity` (Ship Viewer / Menu UI empty), `ShipsGaragePanel` starts inactive, `BackgroundDimmer` disabled (a full-screen raycast blocker that stopped every hub button). **Decision:** G22/S4 (prefab `shipXP` → 0) deferred to refactor step 7 (`ShipXPTable`, M2) so hotseat/bot ships keep perks and passive until prebuilt ships earn XP (S2). **Code:** `ShipsGarageController.Initialize` guarded with `_isInitialized` (ran from `OpenGarage` and again from `Start` when the panel starts inactive → double event subscriptions); new `HubPopupHost` (§15.3) owns the dimmer and the one open popup, garage/settings/missile panels open through it, `CloseGarage` public for the Cancel button; refactor step 4 deleted `Assets/Obsolete`, `missilePrefab.prefab`, `MainMenu.cs`, `ScoreKeeper.cs`, `ScrollingBackround.cs`, `TournamentMode.cs` (`PlayerShip` uses `shipLevel` directly), `ArchetypeRestrictionChecker.cs`, `RankConfiguration.cs`, `BattlePassData.cs` + `Season1_BattlePass.asset` (`RewardType` moved to `Online/RewardType.cs`; the generator no longer recreates the empty folder). `Missile3D.prefab` kept: `Standard.asset.visualModelPrefab` (live on `Gravity1.prefab`) points at it (M4). **Audit closed:** H6, M3, U8, SH7, B4; G23, H1 and A-rank reduced to their remaining parts. **Audit updated:** U2 (hub state), M4 (editor step), G22/S4 (deferred). **Audit added:** U12 (dead root prefabs, two with missing references after step 4). Static verification only (no compiler in the session); editor steps in the session notes. |
