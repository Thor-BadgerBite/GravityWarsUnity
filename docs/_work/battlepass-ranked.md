# Battle pass, seasons & ranked (working notes)

Scope: `Assets/Online/BattlePassSystem.cs` (canonical), `Progression System/BattlePassData.cs` (dead SO type),
`Resources/GeneratedContent/BattlePass/Season1_BattlePass.asset` (orphan), `Progression System/UI/BattlePassUI.cs`,
`Online/RankedSeasonSystem.cs`, `Online/ELORatingSystem.cs`, `Online/RankConfiguration.cs`, `Online/MatchmakingService.cs`,
`PlayerAccountData` ranked fields.

## Battle pass (IMPLEMENTED, local + online)

- `BattlePassSystem`: lazy singleton (`FindObjectOfType` → `AddComponent`, DontDestroyOnLoad). `maxLevel` 25, `xpPerLevel` 1000 (serialized on a component that is created at runtime → effectively constants). Season identity `currentSeason` = 1 → `season_1`, `seasonName` "Season 1: Cosmic Dawn". `seasonEndTimestamp` is declared and never used → **no season clock**; a season only changes when a developer edits `currentSeason` and ships a build.
- `LoadFromProfile`: if `profile.currentSeasonID != season_N` → reset tier/XP/premium/claims and call `RankedSeasonSystem.ApplySeasonRollover`. Restores level/XP from `battlePassTier/battlePassXP` (XP stored cumulatively as tier·1000 + rest).
- `AddBattlePassXP` → level-ups → `OnLevelUp` auto-grants free reward (+ premium if owned) via `ApplyReward` → `PlayerAccountData.UnlockById` / credits / gems; `SaveToProfile` through `AccountSystem.UpdateProfileAsync` if signed in else `ProgressionManager.Save`.
- `PurchasePremiumPass(gemCost = 1000)` deducts gems, sets premium, retro-grants premium rewards ≤ current level. `ClaimFreeReward/ClaimPremiumReward` also exist (manual path, unused by UI since auto-grant).
- Reward tables: see economy.md for currency; content ids: free — `skin_starter_blue`(2), `passive_shield_regen`(3), `standard_mk2`(5), `skin_tank_iron`(7), `pusher_missile_t1`(8), `seasonal_scout_free`(10), `skin_dd_crimson`(12), `passive_armor_boost_2`(13), `light_vortex`(15), `skin_ctrl_shadow`(17), `missile_barrage_t1`(18), `seasonal_defender_free`(20), `skin_allaround_gold`(22), `body_seasonal_standard`(25); premium — `skin_premium_platinum`(2), `passive_critical_strike`(4), `premium_nebula_hunter`(5), `skin_premium_cosmic`(7), `cluster_missile_exclusive_t1`(8), `exclusive_stellar_dom`(10), `skin_premium_diamond`(12), `tactical_emp`(13), `premium_quantum_fortress`(15), `skin_premium_royal`(17), `explosive_missile_exclusive_t3`(18), `premium_ethereal_phantom`(20), `skin_legendary_celestial`(22), `body_premium_elite`(23), `ultimate_season_monarch`(25). All non-skin ids resolve to generated assets (verified by listing `Resources/GeneratedContent`); all skin ids resolve to nothing.
- Perk-tier filing: `UnlockById(Active, id)` uses `ExtendedProgressionData.GetActiveTier(id)` which knows only the 20 fictional ids plus `explosive_missile_exclusive_t3`; `pusher_missile_t1`, `missile_barrage_t1`, `cluster_missile_exclusive_t1` fall back to tier 1 correctly by luck. Any future T2 reward would be filed as T1 (B3).
- `BattlePassUI` (not in any scene; needs `tierItemPrefab` with children `TierNumber/FreeReward/PremiumReward/Icon/Name/ClaimedMark`; no such prefab in repo).
- Dead path: `BattlePassData` / `BattlePassTier` / `UnlockableReward` classes + the orphan `Season1_BattlePass.asset` (30 tiers, XP 1000·tier, currency-only rewards; loaded by nothing). `RewardType` enum from that file is still used.

## Ranked / ELO (IMPLEMENTED as pure logic, no live match path)

- `ELORatingSystem`: standard expected-score formula, K 40 (<10 games) / 32 (<50) / 24 / 16 (≥1800 ELO), start 800, clamp 100–4000, fair match ±150, rank mapping, colours, preview.
- `PlayerAccountData`: `eloRating`, `peakEloRating`, `currentRank` (16 `CompetitiveRank` values), ranked/casual match counts, streaks, `selectedRankedLoadoutId`.
- `MatchHistoryManager.ProcessELOChanges` is the only writer of ELO — netcode-only, never compiled in the current build. Local hotseat/bot matches never touch ELO or rank.
- `RankedSeasonSystem.ApplySeasonRollover`: on season change, grants gems by peak rank (20…500), exclusive skin id for Vice Admiral+, soft reset `elo = (elo + 1200)/2`, resets streak. Anchor 1200 vs start 800 → a new player below 1200 is pulled **up** at rollover (B5, CONFLICT). Rollover is triggered from the battle pass season check, so ranked seasons = battle pass seasons.
- `MatchmakingService`: in-memory queues, ELO window ±100 expanding +50/5 s to ±400, 120 s timeout, casual FIFO, creates `MatchFoundData` with `roundsToWin` 3, `turnTimeLimit` 60 s, `serverAddress = "HOST"`; Unity Matchmaker commented out ("TODO: Install package"). Not in any scene; `OnMatchFound` has no subscriber in project code (to confirm in multiplayer.md).
- Ranked unlock at account level 10 (`ProgressionSystem.RANKED_UNLOCK_LEVEL`) is not enforced by any UI or service.
- 16-rank ladder vs old 7-tier (Bronze…Grandmaster) — the old names survive only in `Documentation/Feature-Guides/Rank System.md` (to check) → docs conflict.

## Bugs / risks (B = battle pass/ranked)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| B1 | high | `BattlePassSystem` | No season duration/clock; `seasonEndTimestamp` unused; season rolls only when code changes. | Add season start/end config (SO or remote config) and check it in `LoadFromProfile`. |
| B2 | medium | `BattlePassSystem` reward tables | Hard-coded C# dictionaries; skins reference nonexistent content. | Move to a `BattlePassSeasonSO` (the deleted `BattlePassData` was that) once cosmetics exist. |
| B3 | medium | `PlayerAccountData.UnlockById` + `ExtendedProgressionData.GetActiveTier` | Tier resolution by a fictional table; should read `ActivePerkSO.tier` from the content database. | Resolve via `ProgressionManager.allPerks`. |
| B4 | medium | `Season1_BattlePass.asset`, `BattlePassData.cs` | Orphaned second implementation left in Resources (loaded into memory by `Resources.LoadAll<ScriptableObject>`? No — typed loads only; harmless but confusing). | Delete asset + classes, keep `RewardType`. |
| B5 | medium | `RankedSeasonSystem.SOFT_RESET_ANCHOR` 1200 vs `ELORatingSystem.STARTING_ELO` 800 | Rollover inflates low ratings. | Anchor = STARTING_ELO or a configured value. |
| B6 | medium | `MatchHistoryManager` | ELO/ranked only in netcode path with blocking `task.Wait()`. | Rewrite async when online is built. |
| B7 | low | `BattlePassSystem.Start` | Subscribes to `AccountSystem.OnLoginSuccess` but `OnDestroy` unsubscribes only when `AccountSystem.Instance != null` at that time; fine. `maxLevel/xpPerLevel` are `[SerializeField]` on a runtime-created component → not editable. | Config SO. |
| B8 | low | `BattlePassUI.UpdateHeader` | Shows `GetCurrentLevel()+1` as "Tier" (1-indexed) while reward tables are keyed by level reached; tier 1 shown at level 0 (cosmetic). | Align wording. |

## Tunables
25 levels × 1000 XP; premium 1000 gems; BP XP per match = account XP (local) or 50/100 (online), first-win ×2; peak-rank gems 20/25/35/45/60/80/100/130/160/200/250/300/350/420/500; skin threshold Vice Admiral; soft-reset anchor 1200; ELO constants; matchmaking windows; roundsToWin 3 / turn 60 s (online) vs `GameManager.winningScore` 1 / `turnDuration` 15 s (HotSeat 1 scene) → CONFLICT in match format.
