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

### Extra editor wiring for the new features
- Results screen: assign the new banner objects (first win, streak, close
  match, trickshot, rivalry) + requeue/replay buttons on `MatchResultsUI`.
- Replay: add a world-space LineRenderer + optional marker to `KillshotReplayUI`.
- Main menu: place `NextUnlockWidget` and (optionally) `ShipMasteryBadgeUI`.
- Practice mode: expose `player2IsBot`/`botDifficulty` in the setup screen UI.
