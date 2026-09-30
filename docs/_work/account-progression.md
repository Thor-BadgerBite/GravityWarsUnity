# Account progression, player data model & online account (working notes)

Scope: `Assets/Progression System/PlayerAccountData.cs`, `ProgressionManager.cs`, `SaveSystem.cs`,
`Assets/Online/ProgressionSystem.cs`, `ExtendedProgressionData.cs`, `AccountSystem.cs`, `MatchHistoryManager.cs`,
`Assets/Progression System/UI/ProgressionUI.cs`, `Assets/UI/NextUnlockWidget.cs` (ui-screens.md).

## Player data models (CONFLICT: two)

1. `PlayerAccountData` (`Progression System/PlayerAccountData.cs`) — the unified model used by everything runtime: level/XP, credits/gems, ELO/rank/streaks, battle pass fields, unlock id lists (ship bodies, ship models, perk tiers 1/2/3, passives, move types, missiles, skins/colour schemes/decals, achievements), `customShipLoadouts`, `shipProgressionData`, equipped/selected loadout ids, recent matches, quests, `lastFirstWinDate`, statistics, `preferences`. Serialized with `JsonUtility` (`DateTime` fields will not survive JsonUtility — see A9).
2. `GravityWars.CloudSave.SaveData` (`CloudSave/SaveData.cs`) — a second, richer model (profile, currency with transactions, progression, quests, achievements, statistics, settings, unlockables, leaderboard stats, analytics queue, metadata) used only by `CloudSave/SaveManager.cs`. To be confirmed in cloudsave.md; from the class list it duplicates the above.

## Local progression path (ProgressionManager, IMPLEMENTED with bugs)

- Lazy singleton (`ProgressionManager.Instance` creates `[ProgressionManager]` on demand), `DontDestroyOnLoad`.
- `Initialize`: load local save (`SaveSystem.LoadPlayerData`) or `CreateNewAccount("Player", guid)` → `GrantStarterContent` → `Save`; then `PopulateContentDatabases` (`Resources.LoadAll` for bodies, presets, perks, passives, move types, missiles); `UnlockShipsForLevel(level)` catch-up; `QuestService.InitializeQuests()`.
- **Order bug (A1)**: on a brand-new account `GrantStarterContent` runs while `allShipBodies/allMoveTypes/allMissiles` are still empty (databases are populated afterwards), so nothing from the databases is unlocked; only the hard-coded ids in `PlayerAccountData.InitializeDefaultUnlocks` apply (`starter_ship`, bodies `"Standard"` (no such body) and `"body_allaround_standard"`, missiles `"standard_mk1"` and `"Standard"`, move `"Standard Move"`), plus 1000 credits / 50 gems.
- `AwardMatchXP(won, roundsWon, damageDealt, usedLoadout, closeMatch, trickshots)`: base 50 + win 100 + 25/round + damage/100 + 25/trickshot + 50 close-match consolation; win streak credit bonus 50/100/200 at 3/5/10; premium pass +50 % XP; account XP += total; `CheckAccountLevelUp`; ship XP (= same total) to `usedLoadout` (custom loadouts only); battle pass XP = total, ×2 first win of the day (`lastFirstWinDate`), pushed to `BattlePassSystem.AddBattlePassXP`; stats; save.
- `CheckAccountLevelUp`: `while (currentXP >= 1000 + level*500 && level < 50) level++` — **XP is cumulative and never reduced**, thresholds are per-level, so level 2 costs 1500 XP total but every later level costs only +500 (A2). `PlayerAccountData.xpForNextLevel` is never updated (stays 1000).
- `UnlockShipsForLevel`: unlocks non-premium `ShipPresetSO` whose `requiredAccountLevel <= level`. This is the **only** account-level unlock in the local path: bodies, perks, passives, move types and missiles are never unlocked by level locally (their `requiredAccountLevel` is only checked by the builder through `IsUnlocked`) (A3, CONFLICT with the whole "unlock by leveling" design).
- Currency helpers `CanAfford/SpendCurrency`; loadout CRUD; `Save/Load` via `SaveSystem` (local JSON in `persistentDataPath/Saves/player_data.json` + backup; async cloud push through `ServiceLocator.Instance.CloudSave` if present).

## Online / netcode progression path (DESIGNED, compiled out)

- `MatchHistoryManager` (whole file inside `#if UNITY_NETCODE_GAMEOBJECTS`, define not set in `ProjectSettings.asset`): server-side match record → stats, ELO (`ELORatingSystem.CalculateNewELO`), history (last 50), rewards (base 50/100 XP casual/ranked, ×2 win, damage/10, accuracy bonuses, streak, close-match), battle pass XP 50/100 (×2 first win), ship XP, `CheckLevelUp` using `profile.xpForNextLevel` and `ProgressionSystem.CalculateXPForLevel` (**exponential 1000·1.15^(L−1)**, subtracts XP per level) and `ProgressionSystem.GetLevelUpReward` (credits 100+50·L (+500 every 10), gems 10·(L/5) every 5 levels, unlocks from the id tables via `PlayerAccountData.UnlockById`). Blocks the main thread with `task.Wait()` on cloud loads.
- `ProgressionSystem` (static): feature unlock levels (ranked 10, custom match 5, achievements 3, quests 5, leaderboard 8, clan 30), custom slots (1/2/3 at 1/20/40), class unlock levels (Tank 5, DD 15, Controller 25 — enforced nowhere), `SHIP_UNLOCKS` (17 entries, 8 without assets; `GetShipUnlock/GetAllUnlockedShips` have no callers), the exponential XP curve (used only by MatchHistoryManager), display helpers.
- `ExtendedProgressionData` (static id tables for levels 1–100): 40 prebuilt ships (9 exist as assets + 8 BP ships), 16 bodies (12 exist; ids match), 30 passives (17 exist; e.g. `passive_speed_boost_1`, `passive_evasion`, `passive_god_mode` do not), 20 actives (none exist) + 1 bridging entry, `GetActiveTier` (defaults to 1 → tier-filing risk for any unknown T2/T3 id, noted in source).
- `AccountSystem` (Unity Authentication username/password): `Awake` singleton, **no lazy bootstrap and not in any scene** → `AccountSystem.Instance` is always null in the tested flow; `CreateNewPlayerProfile` gives 1000 credits / 0 gems and `currentRank = Lieutenant` while ELO 800 = Ensign (A5); `IsUsernameTaken` is a stub; password reset unimplemented.

## Three account XP formulas (CONFLICT)

| Where | Formula | Used by |
|---|---|---|
| `ProgressionManager.CheckAccountLevelUp` | threshold `1000 + level·500` against cumulative XP, cap 50 | local play (live) |
| `ProgressionSystem.CalculateXPForLevel` | `1000 · 1.15^(level−1)` per level, XP subtracted | `MatchHistoryManager` (netcode only) |
| `ProgressionUI.GetXPForLevel`, `GameManager.SumLevelUpXP` | `1000 + level·500` treated as cumulative "XP for level" / per-level cost | display only |

## Rank thresholds duplicated (3 copies + display)
`PlayerAccountData.UpdateRankFromELO`, `ELORatingSystem.GetRankFromELO`, `RankedSeasonSystem.RankFromELO` (identical 16-rank tables: Cadet <700, Ensign <1050, Lieutenant <1200, LtCdr <1350, Cdr <1500, Capt <1650, SrCapt <1800, Cdre <1950, RAdm <2100, RAdm(UH) <2250, VAdm <2400, Adm <2550, HAdm <2700, FAdm <2850, SAdm <3000, GAdm 3000+), plus `RankConfiguration.GetRankData` (colours/abbreviations). `ELORatingSystem.STARTING_ELO` = 800; K-factors 40/32/24/16.

## Bugs / risks (A = account)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| A1 | high | `ProgressionManager.Initialize/GrantStarterContent` | Starter unlock loop runs before `PopulateContentDatabases` → new accounts get no database-driven starter content (the hard-coded id list saves the day partially; body id `"Standard"` matches nothing). | Populate databases first. |
| A2 | high | `ProgressionManager.CheckAccountLevelUp` | Cumulative XP vs per-level threshold → levels get 500 XP cheap after level 2; `xpForNextLevel` stale. | Subtract XP per level (or use cumulative table) and store `xpForNextLevel`; one formula for all paths. |
| A3 | high | local path | Bodies/perks/passives/move types/missiles never unlock by account level locally; only prebuilt ships and battle-pass rewards. Builder is unusable for a fresh account. | Add `UnlockContentForLevel` driven by each SO's `requiredAccountLevel` (mirror of `UnlockShipsForLevel`). |
| A4 | high | `GameManager.AwardMatchProgression` (G1) | Both players' awards go to the one local account. | see G1 |
| A5 | medium | `AccountSystem.CreateNewPlayerProfile` vs `ProgressionManager.GrantStarterContent` | Different starter economies (1000/0 vs 1000/50 gems), rank mismatch (Lieutenant vs ELO 800). | One `NewAccountDefaults` config. |
| A6 | medium | `PlayerAccountData.saveVersion` | Declared "for migration" but no migration code exists. | Add versioned migration or remove. |
| A7 | medium | `AccountSystem` | No lazy bootstrap, never placed in a scene; all `AccountSystem.Instance` checks are dead paths. | Decide online scope (see multiplayer.md). |
| A8 | medium | `ExtendedProgressionData`, `ProgressionSystem.SHIP_UNLOCKS`, `MissileRetrofitSystem.MISSILE_UNLOCKS` | Three id tables that disagree with the generated assets and with each other (e.g. `nova_class` at level 3 in both tables but `oracle_class` level 28 vs 29). | Delete the tables; derive schedules from `requiredAccountLevel` on the SOs (single source), or generate the tables from assets. |
| A9 | medium | `PlayerAccountData` `DateTime` fields (`accountCreatedDate`, `lastLoginDate`, `ShipProgressionEntry.firstUsedDate/lastUsedDate`, `MatchResultData.matchDate`, `QuestProgressData` dates) | `JsonUtility` does not serialize `DateTime`; these reset to default on every load (`accountCreatedTimestamp` therefore = 1970). | Store Unix timestamps (longs) only. |
| A10 | low | `PlayerAccountData.unlockedActives` getter | Allocates a new list on every access. | Cache or expose per-tier lists. |
| A11 | low | `ProgressionManager.GetUnlocked*` | Linear scans each call; fine at this scale. | – |
| A12 | low | `SaveSystem.LoadPlayerDataWithCloudMergeAsync` | "Merge" = pick newest by `lastLoginTimestamp` (always 1970, see A9) and max currency → effectively random winner. Never called. | Implement a real merge or delete. |

## Values for the GDD tunables table
Match XP: base 50, win 100, round 25, damage/100, trickshot 25, close-match 50; streak credits 50/100/200 (3/5/10); premium +50 %; first-win BP ×2; level formula (three variants); level cap 50; starter 1000 credits + 50 gems (local) / 0 gems (online); custom slots 1/2/3 at 1/20/40; feature unlock levels (ranked 10, quests 5, achievements 3, leaderboard 8, custom match 5, clan 30); ELO start 800, min 100, max 4000, K 40/32/24/16, fair-match window 150; rank thresholds above.
