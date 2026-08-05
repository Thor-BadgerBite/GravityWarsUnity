# Implementation Status — Next Stages (Code Level)

This document tracks the code-level implementation of the remaining stages from
`IMPLEMENTATION_PLAN.md` and the fixes from `Assets/Online/PRE_TESTING_REVIEW.md`.

---

## ✅ Completed in this pass

### 1. Player Data Unification (PRE_TESTING_REVIEW Issue #1, #7)
- `PlayerAccountData` is the single source of truth (compatibility aliases for the
  old `PlayerProfileData` field names already existed).
- **NEW:** `PlayerAccountData.UnlockById(UnlockType, id)` — routes string-id unlocks
  to the correct list, including perk **tier routing** via
  `ExtendedProgressionData.GetActiveTier()`. This fixes the silent no-op bug where
  `unlockedActives.Add(...)` wrote into a computed copy and unlocks were lost.
- `MatchHistoryManager` rewritten to use the real `MatchResultData` field names
  (was referencing non-existent fields → would not compile under the netcode define).

### 2. Starter Content Initialization (Issue #2)
- `PlayerAccountData.InitializeDefaultUnlocks()` now grants:
  starter ship (`starter_ship`), starter body (`Standard` + `body_allaround_standard`),
  starter missile (`standard_mk1` + `Standard`), standard move type.
- Custom slot #1 is level-based (level 1) — nothing to store.

### 3. Ship XP / Leveling (Issue #3)
- `MatchHistoryManager.CalculateRewards()` now awards ship XP to the loadout used.
- `GameManager.AwardMatchProgression()` resolves the equipped loadout
  (`currentEquippedShipId` → `selectedCasualLoadoutId` → first loadout) and passes it
  to `ProgressionManager.AwardMatchXP()` so ship XP accrues in local play too.

### 4. Battle Pass XP Source + Persistence (Issue #4)
- Online: `MatchHistoryManager` awards battle pass XP per match (100 win / 50 loss).
- Local: `ProgressionManager.AwardMatchXP()` already fed `battlePassXP` (unchanged).
- Netcode: `NetworkGameManager.AwardRewardsClientRpc` now actually applies
  currency/XP/battle-pass XP to the local account (was commented-out stubs).
- **NEW:** `BattlePassSystem` persists progress on `PlayerAccountData`
  (`battlePassTier`, `battlePassXP`, `hasPremiumBattlePass`,
  `claimedFreeBattlePassTiers`, `claimedPremiumBattlePassTiers`) with season reset,
  and `ApplyReward` now writes every reward type into the proper unlock list.
  `PurchasePremiumPass(gemCost)` actually charges gems.

### 5. Missile Selection Before Matches (Issue #5)
- **NEW:** `Assets/UI/MissileSelectionUI.cs` — pre-match screen listing unlocked
  missiles compatible with the selected body (`ShipBodySO.CanUseMissileType`).
  Stores the choice in `CustomShipLoadout.equippedMissileName`; changing missiles
  never resets ship XP (missile is excluded from the progression key).

### 6. Match Stats + Results Screen (Phase 3.3)
- **NEW:** `Assets/MatchStatsTracker.cs` — per-player damage, missiles fired/hit,
  accuracy, rounds, perks used. Hooked into `PlayerShip.TakeDamage` and
  `PlayerShip.FireMissile`.
- **NEW:** `Assets/UI/MatchResultsUI.cs` — winner banner, per-player stats,
  XP/credits gained, level-up + battle pass banners, Play Again / Return to Menu.
- `GameManager` now: starts/ends match tracking, calls the
  quest/achievement/leaderboard/analytics integrations at match start, round end
  and match end (previously **never called**), uses **real tracked damage** instead
  of the `rounds × 5000` placeholder, and hands off to the results screen when
  one exists in the scene.

### 7. Settings Screen (Phase 3.6)
- **NEW:** `Assets/UI/SettingsUI.cs` — audio (master/music/sfx), graphics
  (quality/vsync/fullscreen), controls (sensitivity/invert-Y), account (logout).
  Persists to `PlayerAccountData.preferences` (cloud-synced) + PlayerPrefs mirror,
  live-applies via `AudioListener`/`QualitySettings`/`AudioManager`.
- `MainMenuController` opens the in-scene panel when present (scene fallback kept).

### 8. Content Generation (Phase 4)
- **NEW:** `Assets/Editor/GameContentGenerator.cs`
  (**Tools → Gravity Wars → Generate Game Content**) creates into
  `Assets/Resources/GeneratedContent/` (auto-loaded by `ProgressionManager`):
  - 10 ship bodies (4 archetypes, level-gated, missile restrictions)
  - 17 passives (mapped to implemented `PassiveType`s, archetype-restricted)
  - 18 active perks (6 families × 3 tiers, scaled stats)
  - 9 missiles (Light/Medium/Heavy × Mk I–III, ids match the retrofit schedule)
  - 3 move types (Heavy Thrusters / Precision Drift / Warp Jump)
  - 10 prebuilt ships (`ShipPresetSO`) incl. the level-0 `starter_ship`
  - Season 1 Battle Pass (`BattlePassData`, 30 tiers, free + premium)
  - Asset names deliberately match the progression ids so `UnlockById` resolves
    directly to assets.
- Quests & achievements generators already existed
  (Tools → Gravity Wars → Generate Quest/Achievement Templates).

### 9. Bug Fixes
- `ExtendedProgressionData.SHIP_BODY_UNLOCKS` had **duplicate key 21 three times**
  → static initializer would throw `ArgumentException` at runtime. Re-staggered to
  levels 21 / 25 / 31.
- `MatchHistoryManager`: field mismatches, read-only property assignment,
  `unlock.username` → `unlock.displayName`.

---

## 🔧 Remaining Unity-Editor work (not code)

1. **Run the generators** (Tools → Gravity Wars):
   Generate Game Content, Quest Templates, Achievement Templates.
2. **Scene setup:**
   - Add `GameManagerQuestIntegration`, `GameManagerAchievementIntegration`,
     `GameManagerLeaderboardIntegration`, `GameManagerAnalytics` components to the
     GameManager GameObject (they are found via `GetComponent`).
   - Build the `MatchResultsUI`, `SettingsUI`, `MissileSelectionUI` panels and wire
     the serialized references (all scripts tolerate missing references).
   - Assign `freeBattlePass` / `seasonalBattlePass` on `ProgressionManager`
     (use the generated `Season1_BattlePass`).
3. **Enable online multiplayer:** all packages are already in `Packages/manifest.json`.
   Add the `UNITY_NETCODE_GAMEOBJECTS` scripting define
   (Project Settings → Player → Scripting Define Symbols) to activate the
   `Assets/Multiplayer/*` netcode layer and `MatchHistoryManager`, then fix any
   remaining compile errors inside the previously-dormant guarded code.
4. **Prefabs/visuals:** ship models for the generated bodies (`visualPrefab`),
   icons, VFX (Phase 5) — art tasks per `IMPLEMENTATION_PLAN.md`.

---

## 🎮 Engagement & Retention Features (second pass — all implemented)

### First session (D1 retention)
- **Bot opponent** (`Assets/Bot/BotController.cs`): simulates trajectories with
  the real game physics (mirrors `PredictMissileTrajectory`, incl. the 0.5x
  launch factor), coarse+refine search over angle/velocity, difficulty-based
  aiming error, human-like think time. Enable via `GameManager.player2IsBot`
  (+ `botDifficulty`) — auto-attached in `PlaceShips`.

### Session loop ("one more match")
- **Killshot replay** (`KillshotRecorder.cs` + `UI/KillshotReplayUI.cs`):
  every missile's flight path is sampled; the fatal shot is kept and replayed
  slow-mo on the results screen (auto-plays once + re-watch button).
- **Trickshot / Gravity Assist detection**: cumulative path curvature ≥ 100°
  on a landed hit → "GRAVITY ASSIST!" banner, per-player trickshot stat,
  +25 XP per trickshot at match end.
- **Instant requeue**: `MatchResultsUI.OnRequeueRequested` event +
  requeue button (wire to matchmaking UI when netcode is enabled).
- **Close-match consolation**: losing one round short of victory grants
  +50 XP (local + online paths) with a "SO CLOSE!" banner.

### Daily loop
- **First win of the day**: 2x battle pass XP, tracked via
  `PlayerAccountData.lastFirstWinDate` (local + online paths), banner on results.
- **Win streaks**: tracked for ALL matches now (not only ranked); milestone
  credit bonuses at 3/5/10 wins; streak banner with bonus on results screen.
- **Next-unlock widget** (`UI/NextUnlockWidget.cs`): "Level 7 → 🚀 Phoenix Mk-I"
  + XP progress bar, scans all unlock schedules for the nearest reward.

### Long-term
- **Ship mastery prestige**: titles (Veteran/Ace/Master/Legend at ship level
  5/10/15/20) via `ShipProgressionEntry.GetMasteryTitle()`, mastery skins
  auto-unlocked at levels 10/20, `UI/ShipMasteryBadgeUI.cs` display component.
- **Seasonal ranked reset** (`Online/RankedSeasonSystem.cs`): soft ELO reset
  (halfway to 1200), gems by peak rank, exclusive skins for Diamond+;
  rolls over together with the battle pass season.
- **Weekly mutators** (`MutatorSystem.cs`): deterministic weekly rotation
  (None / Low Gravity / Giant Planets / Overdrive +2 AP), applied to planet
  mass/size and action points; toggle `GameManager.enableWeeklyMutators`.
- **Rivalry records**: `PlayerAccountData.GetHeadToHeadRecord()` from match
  history; "vs X: 3-1" line on the results screen.

### Fairness
- **Comeback mechanic**: previous round's loser gets +1 action point
  (toggle `GameManager.comebackBonusActionPoint`, on by default; applied in
  `ApplyTurnBonuses` after ship presets, so it is never overwritten).
- **Tournament mode** (`TournamentMode.cs`): normalizes every ship to a fixed
  reference level (default 10) for skill-only matches; ship XP still accrues.

### Ship building — unified (single rule set)
- `ProgressionManager.ValidateLoadoutBuild` / `CreateCustomLoadout` is now the
  **canonical** ship building path: body + move type required, exactly 1
  passive, 3 perks (one per tier) required, **missile optional** (retrofitted
  pre-match via MissileSelectionUI), custom-slot limit by level, per-error
  validation messages.
- `CustomShipBuilder` (id-based online API) is now a thin wrapper that resolves
  ids to the ScriptableObjects and delegates — no duplicated rules; also
  resolves a default move type (fixes the old TODO).
- `ShipBuilderUI` dropped its own third rule set and now displays the canonical
  validation errors; missile selection is optional in the builder.

### Full rebalance around Star Sparrow (measured, not guessed)

**The problem, in numbers.** The generated content was tuned in a vacuum and
never calibrated against the one hand-tuned ship in the project. Worst
matchup (Tank Colossus vs DD Reaper at L20) was **2.3 hits vs 14.7 hits =
6.5x**. Root causes:
1. Armor is multiplicative (`eHP = HP*(armor+400)/400`), so a wide armor
   spread (50-260) multiplied an already-wide HP spread.
2. Missile payload spread (1600-5000 = 3.1x) swamped the ship damage
   multiplier spread (2x), so *missile access* decided damage - and tanks had
   the big missiles. The "glass cannon" dealt **26% LESS** damage per hit than
   the tank while having 4.8x less effective HP. The archetype was inverted.
3. The leveling formulas diverged (Tank +4% HP/+4 armor vs DD +2%/+1), so the
   gap *widened* with level: 5x at L1 -> 6.5x at L20.
4. Generated missiles left `maxLaunchVelocity` at the class default (10) while
   the hand-tuned Standard uses 20, and `maxVelocity` at 6.5-14 vs its 50 -
   every generated missile launched at half power and flew 4-7x slower.

**The reference.** Star Sparrow: 15000 HP / 80 armor / 1.20 damage, firing the
Standard missile (2500 payload, launch 0-20, maxVel 50). Mirror match = **6.0
hits to kill**, which is now the target feel for every matchup.

**The method.** Each body is tuned so `POWER = effectiveHP x damagePerHit`
lands near Star Sparrow's 5.40e7. Equal power => equal time-to-kill, while
archetypes still differ in *how* they win.

**Constraint discovered:** `ShipBodySO.OnValidate` hard-clamps
`baseHealth` (Tank >= 11000, DD <= 10000, Controller <= 10000) and forces
`actionPointsPerTurn` + warns on `rotationSpeed`. Values outside those get
silently auto-corrected by Unity, so every generated body now stays inside
them (and sets the recommended rotation speed, silencing those warnings).

**Result** (verified against the real formulas, L1 and L20):

| | before | after |
|---|---|---|
| Worst matchup asymmetry | **6.5x** | **1.18x** (L1) / **1.17x** (L20) |
| Power spread across bodies | ~3x | ~1.18x |
| Missile payload spread | 3.1x | 1.32x |

Archetype feel is preserved and now emergent rather than accidental:
Tank mirror = 10-12 hits (attrition), AllAround mirror = 6-7 (the benchmark),
DD mirror = 2.2-3.1 (fast, swingy trades).

Missiles now differentiate on **feel, not raw damage**: light = low mass so it
bends hard through gravity wells (high skill ceiling, can arc around planets),
fast, long fuel; heavy = flies straight, slow, big knockback, short fuel.

**Open item - Star Sparrow's own leveling formula.** Its body is
`Tank.asset` (archetype Tank), so it uses `TankLevelingMain` - i.e. tank
damage growth (+0.018/level) on an all-rounder stat line. It is exactly on
reference at L1 but drifts ~8% under an equivalent all-rounder by L20.
Optional one-line fix: point its `levelingFormula` at
`AllAroundLevelingMain` (guid `6efa3446e5e318c45bfa1f731b5dfdf0`). Left
untouched pending a decision, since the body's Tank archetype also gates
which perks/passives it may equip - a separate concern from leveling.

### Star Sparrow re-archetyped to All-Around + sustain passives cut (balance session continued)

Two follow-ups from building the interactive balance sheet, both changing
actual game behavior (not just the generator - hand-authored assets edited
directly):

- **Star Sparrow is now genuinely All-Around.** Its body asset was
  `Tank.asset` (archetype Tank) despite carrying pure all-rounder numbers
  (15000 HP / 80 armor / ×1.20 / rotation 50 - literally the All-Around
  recommendation). Renamed to `Star Sparrow Frame.asset` (via `git mv`, so the
  `.meta`/guid travelled with it - no reference breaks), archetype flipped to
  `AllAround`, `canUseLightMissiles` enabled (All-Around can mount every
  class), and `Star Sparrow.asset`'s `levelingFormula` repointed from
  `TankLevelingMain` to `AllAroundLevelingMain`. It had been ~8% under an
  equivalent all-rounder by level 20 before this; now it sits exactly on the
  reference line at every level. Verified every attached component (passive,
  move type, all 3 perks) already allows All-Around, so nothing else needed
  to change. The generated starter ship was renamed `starter_ship` →
  "Sparrow Trainer" to free up the "Star Sparrow" name for this canonical
  reference ship (id unchanged, so existing unlock logic is unaffected).

- **Lifesteal and Regeneration were the real imbalance, not chassis stats.**
  Building a sustain-aware duel model (net damage per exchange = raw × armor
  factor − healing − regen, run across all 10 prebuilt ships) found a 1.69×
  worst-case asymmetry driven almost entirely by two passives:
  - `Lifesteal` healed off **raw** damage (applied before armor reduction in
    `Missile3D`), so 20% returned ~1280 HP per hit on a 15700 HP hull -
    roughly +50% effective health. That one passive won it 9 duels out of 9
    against the full roster.
  - `Shield Regeneration`'s tick rate is 20/sec (every 0.05s) while it's the
    opponent's turn, so 1.5/tick was ~540 HP recovered per exchange - a free
    extra hit of health every turn.
  Cut to 8% and 0.5 respectively (both in `GameContentGenerator.cs`); worst-
  case asymmetry with sustain included drops to 1.31×. Also tested
  re-pairing passives onto different chassis (strong hull + neutral passive,
  weak hull + sustain passive) - it reshuffles who wins but doesn't change
  the 1.31× worst case, so passive assignments were left as-is rather than
  reshuffling ship identities for no measurable gain.

**Action needed:** re-run Tools → Gravity Wars → Generate Game Content
(regenerates the starter ship name/passive values); the Star Sparrow/Star
Sparrow Frame asset edits are already live since they were hand-edited
directly.

### Round-transition race conditions (found during live hotseat playtesting - full round to a kill)
Confirmed live: two `MissingReferenceException`s right after a ship-killing
hit ("PlayerShip has been destroyed", "Planet has been destroyed"). Both stem
from the SAME root cause: a lethal missile hit fires two independent
turn-advance flows that race each other.

- **Duplicate turn-advance flow:** `PlayerShip.TakeDamage` → `ShipDestroyed`
  starts the AUTHORITATIVE round-reset sequence (`destructionDelay` + fade +
  `StartNextRound` → all-new ships/planets + a further 4s wait before its own
  `StartPreparationPhase`). But the SAME hit's `Missile3D.DestroyMissile` also
  fires `OnMissileDestroyed` → `EndTurn("Missile destroyed!")` → the NORMAL
  (non-lethal-hit) `DelayedNextTurn` → `StartPreparationPhase`, gated only by
  the much shorter `infoFadeDuration`. That shorter delay fires first, reads
  `player1Ship`/`player2Ship` before the round-reset sequence has replaced
  them, and ends up calling `StartPlayerTurn()` on a ship that the round-reset
  sequence destroys moments later - crash.
  Fixed with a `roundEndPending` flag: set the instant `ShipDestroyed` runs
  (synchronously, before the missile-destroy event even fires), checked by
  `OnMissileDestroyed` before it starts the normal turn-advance flow, cleared
  when `PlaceShips()` spawns the next round's ships. Added defensive
  null-guards in `StartPreparationPhase`/`StartPlayerTurn` as a second layer.
- **Planet cache poisoned by deferred `Destroy()`:** `ClearExistingPlanetsAndShips`
  destroys the old planets, then `SpawnPlanets` calls `FindObjectsOfType<Planet>()`
  in the same frame to rebuild the cache - since `Destroy()` doesn't remove
  objects until end-of-frame, the rebuilt cache could still contain the
  about-to-be-gone old planets for one frame, crashing trajectory prediction
  and the bot's shot simulation on the next frame.
  Fixed `GameManager.GetCachedPlanets()` to self-heal: strip null (destroyed)
  entries in place before returning, so both consumers never see a stale
  reference regardless of exactly when the race lands.

### Console log flood fix (found during live hotseat playtesting)
- The trajectory-prediction log in `PlayerShip.PredictMissileTrajectory`
  fires every ~60 frames while a player is aiming (holding Fire mode) -
  over an extended playtest session this floods the Console and buries real
  findings. Gated behind a new `DebugSettings.verboseTrajectoryLogging`
  switch (`Assets/DebugSettings.cs`), OFF by default. Flip it on only when
  actually debugging missile trajectory physics.
- Audited the rest of `PlayerShip.cs`/`Missile3D.cs` for similar per-frame
  logging - everything else is event-driven (fires once per shot/hit/mode
  switch), not a flood source.

### Scene data fix (found during live hotseat playtesting)
- Confirmed live: `"Player 1 Name: Player 1, Player 2 Name: Player 1"` -
  both players showed as "Player 1" even with nothing typed. Not a script
  bug: the Player 2 name `TMP_InputField`'s default `m_Text` was saved as
  `"Player 1"` in both `HotSeat.unity` and `HotSeat 1.unity` (copy-paste
  authoring mistake when the field was duplicated from Player 1's). Field
  wiring on `HotSeatSetup` was correct (two distinct objects) - only the
  default text value was wrong. Fixed directly in both scene files.

### Bot ghost-clone fix (found during live hotseat playtesting)
- Confirmed live: `"Can't remove PlayerShip (Script) because BotController
  (Script) depends on it"`. `PlayerShip.Start()` clones the whole ship
  GameObject to make a movement-preview "ghost", then tries to strip the
  `PlayerShip` script off the clone. Since `BotController` has
  `[RequireComponent(typeof(PlayerShip))]`, when the source ship is
  bot-controlled the clone also carries a `BotController`, and Unity refuses
  to destroy `PlayerShip` while something still requires it - so the ghost
  kept a live (if `isGhost`-guarded, inert) `PlayerShip` + `BotController`
  pair instead of being a stripped-down visual-only clone. Fixed by
  destroying the cloned `BotController` first.
- Practical impact was low (the ghost's `BotController.Update()` already
  early-outs on `isGhost`), but it left stray components and doubled the
  ship-preset-applied console logs during ship spawn - both gone now.

### Quest/Achievement pipeline fixes (found during live hotseat playtesting)
- **Templates were never reaching the runtime services.** `AchievementService`
  and `QuestService` are created lazily at runtime via `AddComponent` (no
  scene GameObject, so nothing to drag assets onto in the Inspector) - their
  `achievementTemplates` / `questTemplates` lists stayed empty forever, so
  achievements/quests could never unlock even though the generators reported
  success (confirmed live: "Created 0 achievements from templates").
  Fix: both template generators now save under a `Resources/` subfolder
  (`Assets/Resources/Achievements/Templates/`, `Assets/Resources/Quests/Templates/`)
  and both services auto-populate via `Resources.LoadAll` when the list is empty.
  **Action needed:** re-run "Generate Achievement Templates" and "Generate
  Quest Templates" (old assets at the previous paths are inert and can be deleted).
- **`QuestService.InitializeQuests()` was never called anywhere** in the
  codebase - the quest system could never start, regardless of templates.
  Now called from `ProgressionManager.Initialize()` once player data is ready
  (matches the method's own "will be called by ProgressionManager" comment),
  AND from `GameManagerQuestIntegration.Start()` (now idempotent-guarded) so
  quests also work when testing a match scene directly without going through
  MainMenu/ProgressionManager first - the same self-bootstrapping pattern
  Achievement/Leaderboard/Analytics integrations already used.
- Misleading log fixed: `LoadAchievementsFromCloud()` always returns false
  (cloud path is a stub) but logged "loaded from cloud" - now logs that cloud
  loading isn't implemented yet, to avoid confusing future debugging.

### Gameplay audit fixes (third pass)
- **Loadout → match bridge** (`MatchLoadoutBridge.cs`): the selected ship
  actually reaches the match now. `GameManager.PlaceShips` applies either an
  explicit `ShipPresetSO` per player (`player1Preset`/`player2Preset`) or the
  local player's equipped custom loadout (resolved to SOs, applied via a
  runtime preset incl. ship XP and retrofitted missile). `PerkManager` gained
  `ReloadSlotsFromPreset()` because its Awake runs before the bridge.
- **HotSeatSetup sliders fixed**: winning score / turn duration / prep time now
  actually write back to GameManager (previously only the label text changed).
- **Perk tier limits enforced**: `_usedThisTurn` was written but never read.
  Now: Tier 1 unlimited, Tier 2 once per turn, Tier 3 once per round.
- **Manual detonation exploit closed**: Space mid-flight only works for the
  acting shooter and never on bot missiles (hotseat shared-keyboard case is
  inherently unattributable and stays social-contract).
- **NRE guards** in `Missile3D` (attacker/shooter null checks in collision,
  SelfDestruct, AvoidPlanetsPredictively).
- **MissileFlightPhase** now tracks a missile fired by the current player
  (was `FindObjectOfType` grabbing an arbitrary one).
- **AP sync + cap**: `ApplyTurnBonuses` re-syncs `movesRemainingThisRound`
  after presets change AP (Controller = 4), and caps stacked bonuses at +2.
- **DamageBoost cap proportional**: cap = base ×2 (+100% for every archetype)
  instead of absolute 2.0 (which gave tanks +167% but DDs only +38%).
- **Regen comment corrected** (20 ticks/sec is the play-tested behavior).
- **Bot think time clamped** to 30% of the turn duration; killshot recorder
  resets per round.
- Deferred intentionally: GameManager's nested `PassiveType` enum duplicate
  (removing it would shift the passive icon array indices wired in the scene).

### Extra editor wiring for the new features
- Results screen: assign the new banner objects (first win, streak, close
  match, trickshot, rivalry) + requeue/replay buttons on `MatchResultsUI`.
- Replay: add a world-space LineRenderer + optional marker to `KillshotReplayUI`.
- Main menu: place `NextUnlockWidget` and (optionally) `ShipMasteryBadgeUI`.
- Practice mode: expose `player2IsBot`/`botDifficulty` in the setup screen UI.

### Prebuilt ships missing a Tier 3 perk (not intentional, fixed)
Found while reviewing `Viper_assault`'s preset in the Inspector: 6 of the 10
generated prebuilt ships had `t3: null` in `GeneratePrebuiltShips()`, with no
level-based pattern explaining which ones (e.g. Eclipse Striker at level 10
had a full 3-tier loadout, but Bastion Class at 12 and Viper Assault at 16
didn't) - just gaps left over from earlier generator passes.
- **`starter_ship` ("Sparrow Trainer", level 0) is the one ship that stays
  Tier-1-only, now on purpose and documented in code**: it's the account's
  very first ship, so fewer buttons to learn beats a full loadout. This is
  a deliberate exception, not a gap - custom-built ships are still required
  to have exactly one perk per tier (`ProgressionManager.ValidateLoadoutBuild`).
- **The other 5 now have a real Tier 3 perk each**, chosen from a perk family
  not already used in that ship's own Tier 1/2 (same pattern the already-complete
  ships like Eclipse Striker/Juggernaut/Reaper Class/Nexus Command follow):
  - `nova_class` -> `overcharged_cannon_t3`
  - `titan_defender` -> `explosive_missile_t3`
  - `phoenix_mk1` -> `pusher_missile_t3`
  - `bastion_class` -> `overcharged_cannon_t3`
  - `viper_assault` -> `cluster_missile_t3`
- **Action needed:** re-run Tools -> Gravity Wars -> Generate Game Content to
  apply the new Tier 3 assignments to the existing `ShipPresetSO` assets.
