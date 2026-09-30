# Phase 1 – Inventory

Snapshot of the repository at the start of the assessment (branch `claude/lucid-rubin-xc5aye`, base commit on `main` 2026-09-30). Unity `2022.3.34f1` (`ProjectSettings/ProjectVersion.txt`), so C# 9 is the ceiling. Everything here was determined statically (rg / find / git log / scene & prefab YAML); nothing was compiled or run.

Third-party folders were **not read** and are excluded from all counts: `Sci-Fi UI Collection`, `Sci-Fi UI`, `TextMesh Pro`, `LeanTween`, `StarSparrow`, `Planets of the Solar System 3D`, `Nebula Skyboxes`, `2D Space Kit`, `3Skyboxes`, `Honeti`. Asset-only folders (`+3d Objects+`, `+Audio+`, `+Graphics+`, `Resources/GeneratedContent`, `Resources/Quests`, `Resources/Achievements`) were only listed.

Project code: **130 C# files, 46 094 lines** (`find … | xargs wc -l`, third-party excluded, `Assets/Obsolete` included).

Status legend: **working** = reachable and believed to behave as intended; **partial** = reachable but incomplete or buggy (see the system note); **stub** = compiles but the real behaviour is TODO/commented out; **dead** = no caller / not in any scene / compiled out; **duplicate** = second implementation of something that exists elsewhere; **legacy** = `Assets/Obsolete`.

"In scene/prefab" = referenced by a scene or prefab (script-guid scan). Lazy singletons create themselves at runtime and are marked *(runtime)*.

## 1. Scripts

### Assets/ (root)

| File | Lines | Main class | Purpose | Status | In scene/prefab | Note |
|---|---|---|---|---|---|---|
| GameManager.cs | 1930 | `GameManager` | Match orchestrator: setup screen, planets, ships, turns, timers, HUD, round/match end, progression award, network hooks | working (with bugs G1–G26) | HotSeat 1, HotSeat, SampleScene | gameplay-physics.md |
| PlayerShip.cs | 2041 | `PlayerShip` | Ship controls, aiming, trajectory preview, moves, damage, parts, passives, level stats, bot/network entry points | working | Gravity1, Gravity2, Obsolete/PlayerShip prefabs | gameplay-physics.md, ship-progression.md |
| Missile3D.cs | 1362 | `Missile3D` | Missile flight (gravity, drag, fuel), collisions, part damage, self-destruct, cluster split, trail | working | Missile.prefab, Missile3D.prefab | gameplay-physics.md, missiles.md |
| MissilePresetSO.cs | 293 | `MissilePresetSO`, enum `MissileType` | Missile data asset | working | – (assets) | missiles.md |
| Planet.cs | 80 | `Planet` | Gravity source, collider setup | working | added at runtime | |
| PlanetRotation.cs | 20 | `PlanetRotation` | Cosmetic spin | working | planet prefabs (`+3d Objects+`) | |
| CameraController.cs | 286 | `CameraController` | Follow missile, zoom, slow-mo | working | HotSeat 1, HotSeat, SampleScene | G13 |
| HotSeatSetup.cs | 77 | `HotSeatSetup` | Pre-match setup panel → GameManager | working | HotSeat 1, HotSeat | |
| PlayerUI.cs | 69 | `PlayerUI` | Legacy per-ship name/score panel | partial (superseded by GameManager HUD) | HotSeat 1, PlayerUIPrefab | |
| AudioManager.cs | 222 | `AudioManager` | SFX/music, engine loop | working | HotSeat 1, HotSeat, SampleScene | G25 |
| MainMenu.cs | 112 | `MainMenuManager` | Old placeholder menu; only "Hotseat" loads a scene | partial / obsolete | MainMenu.unity | ui-screens.md U8 |
| SplasScreenManager.cs | 76 | `SplashScreenManager` | Splash → "MainMenu" | working | SplashScreen.unity | |
| MatchLoadoutBridge.cs | 116 | `MatchLoadoutBridge` (static) | Applies `ShipPresetSO` / custom loadout to a spawned ship | partial (G9) | – | ships-archetypes.md |
| MatchStatsTracker.cs | 162 | `MatchStatsTracker` | Per-match damage/shots/hits/accuracy | working | runtime (`GetOrCreate`) | G19 |
| KillshotRecorder.cs | 219 | `KillshotRecorder` | Records last killing trajectory, trickshot detection | working | runtime | |
| MutatorSystem.cs | 101 | `MutatorSystem` (static) | Weekly mutators (planet mass/size, AP) | working, disabled | – | `enableWeeklyMutators = false`; `Override` unused |
| TournamentMode.cs | 28 | `TournamentMode` (static) | Level normalisation | dead (`Enabled` never set) | – | |
| ScoreKeeper.cs | 21 | `ScoreKeeper` | Old score singleton | dead | – | |
| ScrollingBackround.cs | 30 | `ScrollingBackground` | Scrolling texture | working (stale scenes only) | HotSeat, SampleScene | |
| DebugSettings.cs | 17 | `DebugSettings` (static) | Verbose logging switches | working | – | |
| DebugExtensions.cs | 22 | `DebugExtensions` | `DrawCircle` gizmo helper | working (debug only) | – | |

### Assets/Bot

| File | Lines | Main class | Purpose | Status | In scene/prefab | Note |
|---|---|---|---|---|---|---|
| BotController.cs | 246 | `BotController` | Fire-only AI: simulated shot search + difficulty error | working | runtime (added by GameManager) | bot-ai.md |

### Assets/Ship System

| File | Lines | Main class | Purpose | Status | Note |
|---|---|---|---|---|---|
| ShipEnums.cs | 10 | enum `ShipArchetype` | Tank / DamageDealer / AllAround / Controller | working | duplicate concept: `ShipClass` in Online/ProgressionSystem |
| ShipBodySO.cs | 254 | `ShipBodySO` | Body stats, missile flags, `visualPrefab` (never read) | working | ships-archetypes.md |
| ShipPresetSO.cs | 285 | `ShipPresetSO` | Prebuilt ship = body + formula + passives + perks + move + missile | working | |
| ShipLevelingFormulaSO.cs | 168 | `ShipLevelingFormulaSO` | Per-level stat scaling | working | fallback formulas duplicated in PlayerShip (G9) |
| PassiveAbilitySO.cs | 272 | `PassiveAbilitySO`, enum `PassiveType` | Passive definitions applied as flags on PlayerShip | working | second `PassiveType` nested in GameManager (G12) |
| MoveTypeSO.cs | 223 | `MoveTypeSO` | Normal / Precision / Warp parameters | partial (warp params not applied) | |
| ArchetypeRestrictionChecker.cs | 217 | `ArchetypeRestrictionChecker` (static) | Validation duplicate | dead / duplicate | |

### Assets/+Active Perks+

| File | Lines | Main class | Purpose | Status | Note |
|---|---|---|---|---|---|
| ActivePerkSO.cs | 84 | `ActivePerkSO` (abstract) | Perk asset base (tier, cost, minLevel, archetypes) | working | perks.md |
| IActivePerk.cs | 8 | `IActivePerk` | Runtime perk interface | working | |
| PerkManager.cs | 269 | `PerkManager` | Three perk slots, keys 1/2/3, AP cost, HUD colours | working | Gravity1.prefab; `HandleShotFired` dead (P8) |
| MultiMissileSO.cs / MultiMissilePerk.cs | 21 / 33 | `MultiMissileSO`, `MultiMissilePerk` | 3-missile spread | working | |
| ClusterMissileSO.cs / ClusterMissilePerk.cs | 21 / 32 | … | Split on Space | working | |
| ExplosiveMissileSO.cs / ExplosiveMissilePerk.cs | 18 / 31 | … | AoE detonation | working | |
| PusherMissileSO.cs / PusherMissilePerk.cs | 21 / 31 | … | Knockback missile | working | hand asset `perkName` empty (P6) |
| OverchargedCannonSO.cs / OverchargedCannonPerk.cs | 10 / 42 | … | Damage multiplier for next shot | working | hand asset ×100 (P2) |
| MissileBarrageSO.cs / MissileBarragePerk.cs | 21 / 59 | … | 4 sequential missiles | partial (P1) | |
| BoostJetsSO.cs / BoostJetsPerk.cs | 9 / 32 | … | Move without ending turn | dead (never activatable, P4) | |

### Assets/Progression System

| File | Lines | Main class | Purpose | Status | Note |
|---|---|---|---|---|---|
| PlayerAccountData.cs | 673 | `PlayerAccountData`, `CustomShipLoadout`, `ShipProgressionEntry`, `PlayerPreferences`, … | The runtime player model | working (A2, A9, S1) | account-progression.md |
| ProgressionManager.cs | 695 | `ProgressionManager` | Lazy singleton: load/save, content databases, match XP, unlocks, loadouts | working with bugs (A1–A3) | runtime |
| SaveSystem.cs | 339 | `SaveSystem` (static) | Local JSON save + async cloud push | working (local) | services-cloudsave-analytics.md |
| BattlePassData.cs | 285 | `BattlePassData` (SO), `BattlePassTier`, `UnlockableReward`, enum `RewardType` | Old SO-based battle pass | dead / duplicate (only `RewardType` used) | B4 |
| CosmeticsSystem.cs | 335 | `ShipSkinSO`, `ColorSchemeSO`, `DecalSO`, `CosmeticsApplier` | Cosmetics model | dead (no assets, no callers) | economy.md |
| UI/ProgressionUI.cs | 308 | `ProgressionUI` | Account/ship progression screen | dead (not in scene; own XP formula) | |
| UI/BattlePassUI.cs | 316 | `BattlePassUI` | Battle pass screen | dead (not in scene; prefab missing) | |
| UI/ShipBuilderUI.cs | 470 | `ShipBuilderUI` | Custom ship builder screen | dead (not in scene; prefab missing) | |

### Assets/Online

| File | Lines | Main class | Purpose | Status | Note |
|---|---|---|---|---|---|
| ProgressionSystem.cs | 653 | `ProgressionSystem` (static), enum `ShipClass` | Feature unlock levels, custom slots, exponential XP curve, id-based ship unlock table | partial (constants used; tables/curve netcode-only or dead) | account-progression.md |
| ExtendedProgressionData.cs | 390 | `ExtendedProgressionData` (static) | Level 1–100 id tables (ships, bodies, passives, actives) | partial / aspirational (many ids have no asset) | A8 |
| BattlePassSystem.cs | 539 | `BattlePassSystem` | Canonical battle pass (25 levels, hard-coded reward dictionaries) | working (B1–B3) | runtime |
| RankedSeasonSystem.cs | 112 | `RankedSeasonSystem` (static) | Season rollover rewards + soft reset | working (B5) | |
| ELORatingSystem.cs | 394 | `ELORatingSystem` (static), enum `CompetitiveRank` | ELO maths, 16 ranks | working (no live writer of ELO) | |
| RankConfiguration.cs | 372 | `RankConfiguration` (static) | Rank colours/abbreviations | dead (no callers) | duplicate of ELORatingSystem tables |
| MatchmakingService.cs | 564 | `MatchmakingService` | In-memory ELO queue; Unity Matchmaker commented out | stub / dead (no subscriber, not in scene) | multiplayer.md |
| MatchHistoryManager.cs | 549 | `MatchHistoryManager` | Online match record → stats/ELO/rewards | compiled out (`#if UNITY_NETCODE_GAMEOBJECTS`) | B6 |
| MissileRetrofitSystem.cs | 256 | `MissileRetrofitSystem` (static), second enum `MissileType` | Id-based missile unlock table & compatibility | dead / duplicate (M1) | |
| CustomShipBuilder.cs | 251 | `CustomShipBuilder` (static) | Id-based builder facade over ProgressionManager | working (no UI caller) | |
| AccountSystem.cs | 673 | `AccountSystem`, `AccountResult` | Unity Authentication username/password, profile in cloud save | stub-ish (not in scene, no lazy bootstrap; username check/password reset TODO) | A5, A7, SV2 |

### Assets/Quests, Assets/Achievements, Assets/Leaderboards, Assets/Analytics

| File | Lines | Main class | Purpose | Status | Note |
|---|---|---|---|---|---|
| Quests/QuestDataSO.cs | 327 | `QuestDataSO`, `QuestInstance`, enums | Quest template + instance | working | quests-achievements.md |
| Networking/Services/QuestService.cs | 616 | `QuestService` | Daily/weekly/season slots, progress, rewards | partial (no persistence, Q1–Q3) | runtime |
| Quests/GameManagerQuestIntegration.cs | 235 | `GameManagerQuestIntegration` | Match events → quests | dead (not in scene; half its hooks never called) | |
| Quests/ProgressionManagerQuestIntegration.cs | 239 | `ProgressionManagerQuestIntegration` | Progression events → quests | dead (no callers; RequireComponent on runtime object) | AC7 |
| Quests/UI/QuestUI.cs | 813 | `QuestUI`, `QuestCardUI` | Quest screen | dead (not in scene) | |
| Quests/Editor/QuestTemplateGenerator.cs | 566 | `QuestTemplateGenerator` (EditorWindow) | Writes 24 templates to Resources | working (editor) | |
| Achievements/AchievementDataSO.cs | 451 | `AchievementDataSO`, `AchievementInstance`, enums | Achievement template + instance | working (AC4) | |
| Achievements/AchievementService.cs | 733 | `AchievementService` | Progress tracking; rewards commented out | partial / stub (AC1, AC2) | runtime |
| Achievements/GameManagerAchievementIntegration.cs | 466 | `GameManagerAchievementIntegration` | Match events → achievements | dead (not in scene; AC5) | |
| Achievements/ProgressionManagerAchievementIntegration.cs | 426 | `ProgressionManagerAchievementIntegration` | Progression events → achievements | dead | |
| Achievements/UI/AchievementUI.cs | 812 | `AchievementUI`, `AchievementCardUI` | Achievement screen | dead (not in scene) | |
| Achievements/Editor/AchievementTemplateGenerator.cs | 925 | `AchievementTemplateGenerator` | Writes 43 templates | working (editor) | |
| Leaderboards/LeaderboardData.cs | 476 | `LeaderboardDefinition`, `LeaderboardEntry`, enums | Leaderboard model | working (stale `LeaderboardShipFilter`) | leaderboards.md |
| Leaderboards/LeaderboardService.cs | 700 | `LeaderboardService` | Submit/fetch; UGS calls TODO, mock data | stub | runtime |
| Leaderboards/GameManagerLeaderboardIntegration.cs | 355 | `GameManagerLeaderboardIntegration` | Own PlayerPrefs stat store → leaderboard | dead / duplicate stats (L3) | |
| Leaderboards/UI/LeaderboardUI.cs | 900 | `LeaderboardUI`, `LeaderboardEntryUI` | Leaderboard screen | dead (not in scene) | |
| Leaderboards/Editor/LeaderboardConfigGenerator.cs | 260 | `LeaderboardConfigGenerator` | Generates definitions (never assigned at runtime) | working (editor) | L1 |
| Analytics/GameManagerAnalytics.cs | 298 | `GameManagerAnalytics` | Match analytics wrapper | dead (not in scene; hooks never called) | |
| Analytics/ProgressionManagerAnalytics.cs | 283 | `ProgressionManagerAnalytics` | Progression analytics wrapper | dead | |

### Assets/Networking, Assets/Multiplayer, Assets/CloudSave

| File | Lines | Main class | Purpose | Status | Note |
|---|---|---|---|---|---|
| Networking/ServiceLocator.cs | 253 | `ServiceLocator` | UGS init + anonymous auth, service registry | working (SV2 conflict with AccountSystem) | runtime |
| Networking/Services/CloudSaveService.cs | 1225 | `CloudSaveService` | UGS Cloud Save for `SaveData` and `PlayerAccountData`, queue, backup, conflict resolution | designed, unverified | runtime |
| Networking/Services/AnalyticsService.cs | 489 | `AnalyticsService`, `MatchAnalytics` | Analytics facade; SDK calls commented out | stub (SV3) | runtime |
| Networking/Services/NetworkService.cs | 320 | `NetworkService` | Relay host/join via UTP | compiled out | multiplayer.md |
| Networking/Services/EconomyValidator.cs | 455 | `EconomyValidator` | Client-side XP/level sanity checks | dead | SV4 |
| Networking/Services/RateLimiter.cs | 302 | `RateLimiter` | Sliding-window limiter | dead | |
| Networking/Services/SuspiciousActivityDetector.cs | 450 | `SuspiciousActivityDetector`, `PlayerBehaviorProfile` | Behaviour heuristics | dead | |
| Networking/Integration/ServiceIntegrationHelper.cs | 379 | `ServiceIntegrationHelper` | Glue SaveManager ↔ managers; mostly TODO | dead / stub | SV1 |
| Networking/LobbyManager.cs | 541 | `LobbyManager` | UGS Lobby quick match / create / join, relay code | compiled out; usings commented → would not compile | N1 |
| Networking/MatchManager.cs | 503 | `MatchManager` | Netcode turn relay (stack A) | compiled out; calls non-existent GameManager methods | N1, N2 |
| Networking/NetworkManager.cs | 270 | `GravityWarsNetworkManager` | Wrapper over Unity NetworkManager | compiled out | |
| Networking/NetworkedPlayerShip.cs | 386 | `NetworkedPlayerShip` | NetworkVariables for ship state (stack A) | compiled out; members missing for stack B | |
| Networking/OnlineGameAdapter.cs | 412 | `OnlineGameAdapter` | GameManager ↔ stack A bridge | compiled out; GameManager never calls it | |
| Networking/UI/ConnectionStatusUI.cs | 274 | `ConnectionStatusUI` | Ping/quality HUD | compiled out | |
| Networking/UI/LobbyUI.cs | 391 | `LobbyUI` | Lobby waiting room | compiled out; loads "HotSeat" | N5, N6 |
| Networking/UI/OnlineMatchmakingUI.cs | 395 | `OnlineMatchmakingUI` | Quick match / create / join UI | partial (compiles; shows "requires Netcode" error) | N12 |
| Multiplayer/NetworkMessages.cs | 292 | 12 `INetworkSerializable` structs | Lockstep message types | compiled out | |
| Multiplayer/NetworkTurnCoordinator.cs | 644 | `NetworkTurnCoordinator` | Server turn state machine (stack B) | compiled out | |
| Multiplayer/NetworkInputManager.cs | 430 | `NetworkInputManager` | Input relay (stack B); validation TODO | compiled out | N8 |
| Multiplayer/GameManagerNetworkAdapter.cs | 330 | `GameManagerNetworkAdapter` | GameManager ↔ stack B bridge | compiled out | N9 |
| Multiplayer/NetworkGameManager.cs | 777 | `NetworkGameManager`, `NetworkString` | Server-authoritative real-time rounds (third rule set) | compiled out; would not compile | N1, N4 |
| Multiplayer/ConnectionManager.cs | 512 | `ConnectionManager` | Relay/local connection (relay path under never-defined `UNITY_SERVICES_RELAY`) | compiled out | N11 |
| CloudSave/SaveData.cs | 513 | `SaveData` + 15 nested data classes | Second save model | dead / duplicate | SV1 |
| CloudSave/SaveManager.cs | 697 | `SaveManager` | PlayerPrefs + cloud save of `SaveData`, autosave | dead / duplicate (only debug/glue callers) | SV1 |

### Assets/UI, Assets/Debug, Assets/Editor

| File | Lines | Main class | Purpose | Status | In scene | Note |
|---|---|---|---|---|---|---|
| UI/MainMenu/MainMenuController.cs | 496 | `MainMenuController` | Hub controller (profile, ship viewer, navigation) | designed, not wired | no | U3 |
| UI/MainMenu/MainMenuUI.cs | 421 | `MainMenuUI` | Hub widgets, rank icons, fades | designed, not wired | no | |
| UI/MainMenu/ShipViewer3D.cs | 465 | `ShipViewer3D` | Rotating 3D ship preview | partial (never finds a prefab) | MainMenuScene | U4 |
| UI/ShipsGarage/ShipsGarageController.cs | 511 | `ShipsGarageController` | Garage logic, equip | partial (UI reference missing) | MainMenuScene | U5, U7 |
| UI/ShipsGarage/ShipsGarageUI.cs | 499 | `ShipsGarageUI` | Garage panel | designed, not wired | no | |
| UI/ShipsGarage/ShipInventoryCard.cs | 148 | `ShipInventoryCard` | Toggle card | designed, not wired | no | |
| UI/MatchResultsUI.cs | 281 | `MatchResultsUI`, `MatchResultsSummary` | Post-match results | working in code, no panel | no | |
| UI/MissileSelectionUI.cs | 246 | `MissileSelectionUI` | Pre-match missile pick | working in code, no panel | no | |
| UI/SettingsUI.cs | 267 | `SettingsUI` | Audio/graphics/controls settings | working in code, no panel | no | |
| UI/NextUnlockWidget.cs | 86 | `NextUnlockWidget` | "Next unlock" widget | designed, not wired | no | U6 |
| UI/ShipMasteryBadgeUI.cs | 72 | `ShipMasteryBadgeUI` | Mastery title badge | designed, not wired | no | |
| UI/KillshotReplayUI.cs | 123 | `KillshotReplayUI` | Slow-mo replay line | working in code, no object | no | |
| Debug/DebugSystemsUI.cs | 603 | `DebugSystemsUI` | `~` debug panel for services | working in code, no object | no | SV8 |
| Editor/GameContentGenerator.cs | 841 | `GameContentGenerator` (EditorWindow) | Generates bodies, passives, moves, perks, missiles, presets into Resources | working (editor) | – | ships-archetypes.md |

### Assets/Obsolete (legacy — only referenced-ness was checked)

| File | Lines | Class | Still referenced by |
|---|---|---|---|
| GameEngine.cs | 319 | `GameEngine` | nothing |
| GameModeBase.cs | 129 | `GameModeBase` | nothing |
| GameSetup.cs | 368 | `GameSetup` | **`SampleScene.unity`** (which is in Build Settings) |
| HotseatGameManager.cs | 353 | `HotseatGameManager` | nothing |
| MissileOLD.cs | 600 | `Missile` | **`Assets/missilePrefab.prefab`** (prefab itself unreferenced) |
| PlanetSpawner.cs | 169 | `PlanetSpawner` | nothing |
| ShipPlacer.cs | 101 | `ShipPlacer` | nothing |
| PlayerShip.prefab, Ship.png, protractor.png | – | – | nothing |

Conclusion: `Assets/Obsolete` can be deleted together with `SampleScene.unity` and `missilePrefab.prefab` once `SampleScene` is removed from Build Settings.

### C# language level check
No C# 10+ features found in project code (rg for `record `, `global using`, `file-scoped namespace ;`, `init;`, `is not null` patterns, `new()` target-typed, `with {`): only C# 8/9 constructs are used (switch expressions in `MoveTypeSO`, `PlayerAccountData`, `CosmeticsSystem`, `QuestInstance`, `AchievementService`, `ConnectionStatusUI`; tuples in `BotController`, `PlayerAccountData.GetHeadToHeadRecord`; `async`/`await` throughout). Compatible with Unity 2022.3.

## 2. Scenes (`Assets/Scenes`)

| Scene | Build index | Purpose | Verdict |
|---|---|---|---|
| SplashScreen.unity | 0 | Splash → MainMenu | works |
| MainMenu.unity | 1 | Placeholder menu (`MainMenuManager`) | obsolete; only Hotseat button works and targets a scene outside the build |
| SampleScene.unity | 2 | Legacy test scene using `Obsolete/GameSetup` | should leave the build |
| HotSeat 1.unity | – | **The maintained match scene** (bot, presets, full HUD) | must be added to the build and renamed |
| HotSeat.unity | – | Stale copy of the match scene (missing HUD fields → NRE) | delete after moving references |
| MainMenuScene.unity | – | Brawl-Stars hub (recovered branch), incomplete wiring | the intended hub; editor work required |

`Assets/LeanTween/**` contains 33 example/test scenes (third-party, ignored).

## 3. Existing markdown documents

Dates are the last commit touching the file (`git log -1 --date=short`). Verdicts compare the document with the code as read in Phase 2.

| Document | Lines | Last change | Summary | Matches code? |
|---|---|---|---|---|
| `README.md` | 559 | 2025-11-17 ("Last Updated: November 2025") | Public-facing overview: pitch, features, controls, modes, architecture, damage/physics formulas, roadmap | **Partly.** Physics and damage formulas match (G 0.5, armor/400, part multipliers). Wrong: controls (A/D, W/S, E, Tab, missile steering do not exist — code uses arrows, M, Space, 1-3), "Heavy missile 500 m/s" (caps 38-62), DD missile access, missile masses "7/10/15 units", "15 passives unlock at level 10" (17 + Unmovable), "Load MainScene.unity", roadmap claims ship progression 1-20 complete (only custom loadouts). Roadmap "planned" items (account progression, battle pass, ranked, leaderboards) exist in code since. |
| `IMPLEMENTATION_STATUS.md` | 827 | 2026-09-04 | Session log of code passes: data unification, starter content, ship XP, battle pass, results/settings/missile UIs, content generation, rebalance around Star Sparrow, live playtest fixes (race conditions, bot clone, ProgressionManager bootstrap), MainMenu branch merge, remaining editor work | **Mostly accurate as a changelog** (all described code exists). Its "Remaining Unity-Editor work" list is still open (integrations not attached, UI panels not built, `freeBattlePass` fields no longer exist after the battle-pass merge, define not set). Claims "engagement features all implemented" — true in code, none reachable without the panels. Does not mention the double-award bug (G1) or the scene/build mismatch (G2). |
| `🛠️ Custom Ship Builder - Complete Implementation Guide.md` (root) | 580 | 2026-01-04 | Byte-identical copy of the Feature-Guides file below (`diff -q`) | duplicate |
| `Documentation/Feature-Guides/Custom Ship Builder - Complete Implementation Guide.md` | 580 | 2026-01-04 | UX flow, layout, validation rules and stats preview for the builder screen; "Status: Ready for Unity implementation" | **Rules match** `ProgressionManager.ValidateLoadoutBuild` (1 passive, 3 tiered perks, slots 1/20/40, name ≤ 30). Screen never built (`ShipBuilderUI` not in scene). |
| `Documentation/Feature-Guides/Custom Ship Building Guide.md` | 435 | 2026-01-04 | Designer-level description of custom ships, slots, validation, code reference | Matches builder rules. Claims "delete prebuilt ships to free slots" — no such code (prebuilt ships are not loadouts). |
| `Documentation/Feature-Guides/Complete Progression Guide.md` | 917 | 2026-01-04 | Account XP, ship XP, AP system, 16 ranks, 100-level unlock schedule, 25-level battle pass, missile retrofit | **Conflicts:** AP model (fire = 0 AP, multiple fires per turn, moves repeatable) vs code (one action per turn, AP pool per round); battle-pass XP "daily quests only" vs code (every match); level 100 schedule vs local cap 50; the schedule is the aspirational `ExtendedProgressionData` table (many ids without assets). Rank list matches code. |
| `Documentation/Feature-Guides/Progression System Guide.md` | 540 | 2026-01-04 | Older account progression guide (level-up rewards, ship classes, feature unlock levels) | Feature unlock levels (ranked 10, slot 20/40) match `ProgressionSystem`; level-up rewards/exponential XP exist only in the netcode path; "ship classes unlock at 5/15/25" enforced nowhere. |
| `Documentation/Feature-Guides/Rank System.md` | 283 | 2026-01-04 | 16-rank ladder Cadet → Grand Admiral with ELO thresholds, colours, K-factors | Matches `ELORatingSystem` (thresholds 700…3000, start 800). Mentions old Bronze/Silver names in passing. |
| `Documentation/Feature-Guides/Missile Presets Guide.md` | 439 | 2026-01-04 | Display vs physics mass (÷333.33), launch vs flight velocity, recommended variants, archetype restrictions | Mass conversion matches `MissilePresetSO.OnValidate`. Recommended launch ranges (heavy 1-8, light 1-25) and max velocities (heavy 50-150) do **not** match the generated assets (all 0.1-20 launch, max 38-62). |
| `Documentation/Feature-Guides/Ships Garage Implementation Guide.md` | 462 | 2026-01-04 | Editor setup for the garage (toggle cards), layout, testing checklist; "Scripts created ✓ | Unity setup required" | Accurate: scripts exist, scene wiring not done. |
| `Documentation/Game-Design/Gravity Wars - Complete Game Design Document.md` | 918 | 2026-01-04 (header says v1.0, Dec 19 2024) | Previous GDD: overview, loop, AP, physics, archetypes, combat numbers, progression, modes, UI hub, monetization, architecture, balance | **Mixed.** Matches: pitch, pillars, physics, damage pipeline, part multipliers, custom-ship rules, monetization principles. Conflicts: AP model (fire 0 AP), missile numbers (masses 7/10/15, payload 1800/3500, fuel "lbs"), archetype base stats (Tank 11000 HP vs generated 16500-17500), passive restrictions (Regen/Lifesteal "NOT Tank" vs generator), perk tiers (Explosive/Overcharged as T1, Barrage the only T3) vs assets, rank names/thresholds (Ensign 800…Eternal Admiral 2200 vs code Cadet <700…Grand Admiral 3000), ranked unlock (level 5 + 3 ships vs code level 10), "ELO soft reset −20 %" vs code `(elo+1200)/2`, `PlayerProfileData.cs` (does not exist), "server validates" (no server). Status flags ("Online PARTIALLY IMPLEMENTED") overstate. |
| `Documentation/Implementation Plan.md` | 676 | 2026-01-04 | Online-first phased plan (UGS setup → online match → UI → content → polish), economy numbers, launch checklist | Phase 1 items ("install packages") done; "Lobby/Matchmaking/Analytics ready" overstated (see multiplayer.md, services notes). Economy numbers (credits 50-150/match, gems 10-20/day) match nothing in code. |
| `Documentation/Pre-Testing Review.md` | 418 | 2026-01-04 | Review that found the dual data model, starter unlock, ship XP, BP XP, missile selection, move type and field-name issues | Historical: issues #1-#7 were addressed per IMPLEMENTATION_STATUS; the dual-model problem re-appeared as `CloudSave/SaveData` (SV1). |
| `Documentation/Setup-Guides/Achievement System Setup.md` | 637 | 2026-01-04 | Editor setup for AchievementService/UI, condition types, cloud/platform integration | Code exists; rewards and persistence described as working are stubs (AC1, AC2). Uses old Bronze/Silver naming for achievement tiers (fine, that is tier naming). |
| `Documentation/Setup-Guides/Leaderboard System Setup.md` | 391 | 2026-01-04 | Editor setup, UGS Leaderboards integration, anti-cheat | Describes UGS calls that are TODO; mock data not mentioned. |
| `Documentation/Setup-Guides/Quest System Setup.md` | 412 | 2026-01-04 | Editor setup, objective types, cloud save integration | Cloud save described, not implemented (Q2); half the objectives unfed (Q3). |
| `Documentation/Setup-Guides/Unity Setup Guide.md` | 537 | 2026-01-04 | Package install, UGS dashboard, project settings (incl. `UNITY_NETCODE_GAMEOBJECTS` define), Physics2D layers, test script | Packages installed; define not set; the game uses 3D physics (Rigidbody, SphereCollider) so the Physics2D layer setup is wrong; layers/tags not verified in `TagManager.asset` (editor check). |
| `Documentation/UI-Implementation/Main Menu Guide.md` | 686 | 2026-01-04 | Hub layout, screen flow map, button states, asset list | Design intent for the hub; nothing in it contradicts code, nothing in it is built beyond `MainMenuScene`. |
| `Documentation/UI-Implementation/Main Menu Hub Build Guide.md` | 1995 | 2026-01-04 | Step-by-step editor build of the hub with the Sci-Fi UI pack, wiring, testing | The concrete instruction set for U2; not executed yet. |
| `Documentation/UI-Implementation/Main Menu Setup Guide.md` | 641 | 2026-01-04 | Older hub setup (3D viewer, canvas, rank icons) with test expectations "Rank shows Gold", "1200 ELO" | Superseded (old 7-tier ranks, ELO 1200 start vs 800). |
| `Documentation/UI-Implementation/Screen Catalog.md` | 1498 | 2026-01-04 | 18-screen catalogue with specs, priority order, dependency matrix; "Main Menu ✅, others pending" | Accurate as a wish-list; "Main Menu implemented" overstates (scene exists, wiring not). |

Documents with no counterpart: none found for the bot, mutators, killshot/trickshot, match results banners, settings — those exist only in `IMPLEMENTATION_STATUS.md` and code.

## 4. Other repository facts
- Git history: 195 commits, 2025-08-20 → 2026-09-30; bulk of the meta-game code landed 2025-11; documentation reorganised 2026-01-04; last code session 2026-09-04.
- `Packages/manifest.json`: netcode 1.15.0, UGS authentication/cloudsave/analytics/economy/leaderboards/lobby/relay, TextMeshPro, LeanTween (Asset Store copy in `Assets/`).
- `ProjectSettings/ProjectSettings.asset`: `scriptingDefineSymbols: {}`, `cloudProjectId` set (UGS-linked project).
- No `.asmdef`, no tests (`Assets/Tests` absent), no CI configuration.
