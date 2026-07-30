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
