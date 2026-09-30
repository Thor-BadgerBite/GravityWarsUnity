# UI screens, scenes and navigation (working notes)

Scope: `Assets/Scenes/*.unity` (6 scenes), `ProjectSettings/EditorBuildSettings.asset`, `Assets/MainMenu.cs` (class `MainMenuManager`), `Assets/SplasScreenManager.cs`, `Assets/UI/**` (MainMenuController, MainMenuUI, ShipViewer3D, MatchResultsUI, MissileSelectionUI, NextUnlockWidget, SettingsUI, ShipMasteryBadgeUI, KillshotReplayUI, ShipsGarage/*), `Assets/PlayerUI.cs`, `Progression System/UI/*` (ProgressionUI, BattlePassUI, ShipBuilderUI — see account-progression.md / battlepass-ranked.md), `Quests/UI/QuestUI.cs`, `Achievements/UI/AchievementUI.cs`, `Leaderboards/UI/LeaderboardUI.cs`, `Debug/DebugSystemsUI.cs`, `Networking/UI/*` (see multiplayer.md). HUD fields owned by `GameManager` are covered in gameplay-physics.md.

## Scenes (verified in scene YAML by script guid)

| Scene | In Build Settings | Scripts referenced | Verdict |
|---|---|---|---|
| `SplashScreen.unity` | yes (0) | `SplasScreenManager` | works; loads `"MainMenu"` after its intro (`SplasScreenManager.cs:74`) |
| `MainMenu.unity` | yes (1) | `MainMenuManager` (`Assets/MainMenu.cs`) | **old placeholder menu**: 6 mode buttons + Login + Settings with hover descriptions; only "Hotseat Mode" does anything (`SceneManager.LoadScene("HotSeat")`, line 107); AI / Puzzle / Sandbox / PvP Online / Tournament / Login / Settings log "not yet implemented" (lines 101-112). `IMPLEMENTATION_STATUS.md` calls it "the near-empty skeleton". |
| `SampleScene.unity` | yes (2) | `GameManager`, `AudioManager`, `CameraController`, `ScrollingBackround`, **`Obsolete/GameSetup`** | legacy test scene; the only scene still referencing `Assets/Obsolete` code |
| `HotSeat 1.unity` | **no** | `GameManager` (full HUD fields, `player2IsBot`, `botDifficulty`, presets), `AudioManager`, `CameraController`, `HotSeatSetup`, `PlayerUI` | the maintained match scene (gameplay-physics.md G2) |
| `HotSeat.unity` | **no** | `GameManager` (older field set), `AudioManager`, `CameraController`, `HotSeatSetup`, `ScrollingBackround` | stale copy, but it is the scene every `LoadScene("HotSeat")` call targets |
| `MainMenuScene.unity` | **no** | `ShipViewer3D`, `ShipsGarageController` | the Brawl-Stars-style hub recovered from branch `claude/build-main-menu-hub-…` (IMPLEMENTATION_STATUS "Recovered and merged the lost MainMenu work"); **no `MainMenuController`, `MainMenuUI`, `ShipsGarageUI`, `ShipInventoryCard`, `NextUnlockWidget`, `SettingsUI` component in it** |

Prefab wiring: `Gravity1.prefab` = the ship (`PlayerShip` + `PerkManager`), `Gravity2.prefab` (`PlayerShip`, unused by scenes), `Missile.prefab` / `Missile3D.prefab` (`Missile3D`), `missilePrefab.prefab` (**`Obsolete/MissileOLD`**), `PlayerUIPrefab.prefab` (`PlayerUI`), `Obsolete/PlayerShip.prefab` (`PlayerShip`), `LeftPlayer/RightPlayer/Spacer/Sparrow 1/StarSparrow*/ShipsGarageButton/UI/MainMenuHub/Prefabs/NavigationButton` carry no project scripts.

Only **15 project scripts** are referenced by any scene or prefab (rg of script guids): PerkManager, AudioManager, CameraController, GameManager, HotSeatSetup, MainMenuManager, Missile3D, Obsolete/GameSetup, Obsolete/MissileOLD, PlayerShip, PlayerUI, ScrollingBackround, SplasScreenManager, ShipViewer3D, ShipsGarageController. Everything else is created at runtime (lazy singletons: ProgressionManager, BattlePassSystem, QuestService, AchievementService, LeaderboardService, ServiceLocator, CloudSaveService, AnalyticsService, SaveManager, EconomyValidator, RateLimiter, SuspiciousActivityDetector, ConnectionManager; `GameManager` adds BotController, MatchStatsTracker, KillshotRecorder) or is never instantiated at all (every UI class below except PlayerUI/ShipViewer3D/ShipsGarageController, all `GameManager*Integration`, `GameManagerAnalytics`, AccountSystem, MatchmakingService, MatchHistoryManager, networking).

## Navigation graph (as coded)

```
SplashScreen ──"MainMenu"──▶ MainMenu.unity (MainMenuManager)
                               └─ Hotseat ──"HotSeat"──▶ HotSeat.unity (stale, NOT in build → load fails in a build, works only in editor with the scene open/added)
MainMenuScene.unity (hub, not in build)
   MainMenuController (not in scene!) would load: "RankedMatchmaking", "CasualMatchmaking", "LocalHotseat", "Training",
     "ShipsGarage", "Achievements", "Settings", "Profile", "Leaderboard", "Quests"  ── none of these scenes exist
   ShipsGarageController.OpenGarage()/CloseGarage() toggles its own GameObject (in-scene panel model)
Match scene → MatchResultsUI: Play Again = reload active scene; Return = "MainMenu" if `Application.CanStreamedLevelBeLoaded`, else reload
LobbyUI (netcode) ──"HotSeat"──▶ ; OnlineGameAdapter ──"MainMenu"──▶
README.md: "Load scene MainScene.unity" (no such scene)
```

Two menu generations coexist: the placeholder `MainMenu.unity` (in build, functional path to hotseat) and the hub `MainMenuScene.unity` (intended, per IMPLEMENTATION_STATUS "open MainMenuScene.unity (not MainMenu.unity) to pick the build back up", but incomplete and not in build). Two navigation models coexist inside the hub code: scene-per-screen (`MainMenuController` scene-name fields) vs in-scene panels (`ShipsGarageController.OpenGarage`, `SettingsUI.Show`, `MissileSelectionUI.ShowForLoadout`, `MatchResultsUI.Show`). `Documentation/UI-Implementation/Screen Catalog.md` lists 18 screens, status "Main Menu ✅ | Others: Pending".

## Screen-by-screen

| Screen / class | Status | Notes (file:line) |
|---|---|---|
| Splash (`SplasScreenManager`) | IMPLEMENTED | adds its own AudioSource; coroutine named `LoadGame` loads "MainMenu" (74) |
| Old main menu (`MainMenuManager`, `MainMenu.cs`) | PARTIAL / OBSOLETE | see table above; hover text via runtime `EventTrigger`s |
| Hub controller (`MainMenuController`) | DESIGNED - NOT WIRED | singleton; `async void Start`; profile = `AccountSystem` if signed in else `ProgressionManager` (152-157, the fix described in IMPLEMENTATION_STATUS); ranked gate `ProgressionSystem.IsRankedUnlocked` (432); `GetNotificationCount` returns 0 (439-449); `ShowLockedMessage` TODO (454-458); all scene names point to missing scenes (31-40) |
| Hub UI (`MainMenuUI`) | DESIGNED - NOT WIRED | XP bar uses `profile.currentXP / profile.xpForNextLevel` (180) — one of the three XP formulas (account-progression.md); rank icons for 16 `CompetitiveRank`s; `SetPanelActive` uses `transform.Find(name)` (409); LeanTween fades |
| 3D ship viewer (`ShipViewer3D`) | IMPLEMENTED but **cannot find any ship** | `LoadShipPrefab` tries `Resources/Ships/{id}`, `Resources/Prefabs/Ships/{id}`, `Resources/PlayerShips/{id}`, `Resources/{id}` (174-197). `Assets/Resources` contains only `GeneratedContent/`, `Quests/`, `Achievements/` (find) — **no ship prefabs**, so `DisplayShip` always logs "Failed to load ship prefab". Ids passed are `PlayerAccountData.currentEquippedShipId` / `ShipBodySO.name`. `ShipBodySO.visualPrefab` exists but is never read (ships-archetypes.md). Per-frame `Input.GetMouseButton` polling; `Update` early-outs when no ship |
| Ships garage (`ShipsGarageController` + `ShipsGarageUI` + `ShipInventoryCard`) | PARTIAL (controller in scene, UI not) | `garageUI == null` → `LogError` + no cards (63-73); `GrantStarterShips` unlocks one body per archetype with `requiredAccountLevel == 0` "for testing" (134-152) — bypasses the starter rules in `ProgressionManager.GrantStarterContent`; equips `ShipBodySO.name` into `currentEquippedShipId` while `MissileSelectionUI`/`MatchLoadoutBridge` expect a **loadout id** there (see account-progression.md) → **id-space conflict**; `ShipsGarageUI.UpdateShipInfo` hard-codes `baseMissileDamage = 1000` (270) and `"0/275"` (303); `RefreshInventory()` is empty (229-233) so equipped markers on cards go stale; cards destroyed/re-instantiated on every filter change (157-224) |
| Next unlock widget (`NextUnlockWidget`) | DESIGNED - NOT WIRED | hard-codes `xpForNext = 1000 + level * 500` (35) "matches ProgressionManager level formula" — duplicate of formula in `ProgressionManager` (account-progression.md A2/A3); walks `ExtendedProgressionData` + `MissileRetrofitSystem` tables up to level 100 |
| Match HUD (`PlayerUI`, `GameManager` fields) | IMPLEMENTED | `PlayerUI` in `HotSeat 1` + prefab; bubble timer, health bars, perk icons on `GameManager` (gameplay-physics.md) |
| Match results (`MatchResultsUI`, `MatchResultsSummary`) | IMPLEMENTED in code, no panel in any scene | `GameManager` 1358-1360 looks it up (`Instance` or `FindObjectOfType(true)`) and skips when absent; banners for first win, streak, close match, trickshot, rivalry (`GetHeadToHeadRecord`), killshot replay; "Return to menu" scene name `"MainMenu"` (74) |
| Killshot replay (`KillshotReplayUI` + `KillshotRecorder`) | IMPLEMENTED in code, not in scene | unscaled-time playback of recorded points |
| Missile selection (`MissileSelectionUI`) | IMPLEMENTED in code, not in scene | filters `ProgressionManager.allMissiles` by `IsUnlocked` + `ShipBodySO.CanUseMissileType` (137-151); stores `equippedMissileName` on the loadout and saves; looks up loadout by `loadoutID == currentEquippedShipId` (78) |
| Settings (`SettingsUI`) | IMPLEMENTED in code, not in scene | preferences in `PlayerAccountData.preferences` mirrored to 8 PlayerPrefs keys; applies `AudioListener.volume`, `AudioManager.musicVolume/sfxVolume`, quality, vsync, fullscreen, `targetFrameRate` (default 60) on `Start`; logout via `AccountSystem` |
| Ship mastery badge (`ShipMasteryBadgeUI`) | IMPLEMENTED in code, not in scene | titles from `ShipProgressionEntry.GetMasteryTitle`; colour thresholds 10/15/20 hard-coded (65-71) |
| Progression / battle pass / ship builder UIs (`Progression System/UI`) | DESIGNED - NOT WIRED | see account-progression.md, battlepass-ranked.md, ships-archetypes.md |
| Quests / achievements / leaderboard UIs | DESIGNED - NOT WIRED | ~800-900 lines each, singleton with `Destroy` on duplicate, card prefabs instantiated per refresh, depend on the services' mock/local data (quests-achievements.md, leaderboards.md); `QuestUI.Update` (183) polls every frame |
| Debug (`DebugSystemsUI`) | IMPLEMENTED in code, not in scene | `~` toggles; builds buttons at runtime; reads/writes the unused `SaveData` model (services-cloudsave-analytics.md SV8) |
| Networking UIs | DESIGNED - NOT BUILT | multiplayer.md |

## Bugs / risks (U)

| ID | Sev | Where | Problem | Suggested fix |
|---|---|---|---|---|
| U1 | critical | `EditorBuildSettings.asset`, `MainMenu.cs:107`, `LobbyUI.cs:351` | The playable match scene (`HotSeat 1`) is not in Build Settings and nothing loads it by name; the menu loads the stale `HotSeat`, also not in the build. A player build cannot reach a match. | Decide the canonical scene set (recommend: `SplashScreen`, `MainMenuScene`, `HotSeat 1` renamed `Match`), add to Build Settings, remove `SampleScene`/`HotSeat`/`MainMenu.unity` from the build, put scene names in one `SceneNames` static class. |
| U2 | high | `MainMenuScene.unity` | Hub scene lacks `MainMenuController`, `MainMenuUI`, `ShipsGarageUI`, `ShipInventoryCard` prefab wiring; `ShipsGarageController` errors on start. | Editor task: build the hub per `Documentation/UI-Implementation/Main Menu Hub Build Guide.md`, or archive the hub until the slice needs it. |
| U3 | high | `MainMenuController` 31-40 | Ten scene names for screens that do not exist; every hub button except Settings (when `SettingsUI` present) fails. | Switch the hub to in-scene panels (the model `ShipsGarageController`/`SettingsUI`/`MissileSelectionUI` already use) and keep scene loads only for the match. |
| U4 | high | `ShipViewer3D.LoadShipPrefab` | No ship prefabs under `Resources/`; viewer can never show a ship. `ShipBodySO.visualPrefab` is the intended source. | Load `ShipBodySO.visualPrefab` (via `ProgressionManager.allShipBodies`) instead of `Resources.Load` by name. |
| U5 | high | `ShipsGarageController.EquipSelectedShip` (343) vs `MissileSelectionUI.ShowForEquippedLoadout` (78), `MatchLoadoutBridge` | `currentEquippedShipId` holds a `ShipBodySO.name` after the garage, a `loadoutID`/`ShipPresetSO` id elsewhere. | One definition in the GDD (recommend: equipped *loadout id*; the garage equips the loadout that uses the body, creating a default loadout if none). |
| U6 | medium | `NextUnlockWidget.cs:35`, `MainMenuUI.cs:180`, `ShipsGarageUI.cs:270,303` | Duplicated/hard-coded progression numbers in UI. | Read from `PlayerAccountData`/`ShipLevelingFormulaSO` accessors only. |
| U7 | medium | `ShipsGarageController.GrantStarterShips` | "For testing" grant of one body per archetype at first open — changes the economy silently. | Delete; rely on `ProgressionManager` starter content. |
| U8 | medium | `MainMenu.cs` | Placeholder menu advertises AI / Puzzle / Sandbox / Tournament modes that do not exist; the bot mode is reachable only by scene fields (`player2IsBot`). | Replace with the hub or wire "VS. AI" to load the match scene with `player2IsBot = true` via a static match-config object. |
| U9 | low | `MainMenuUI.SetPanelActive` (409), `ShipsGarageUI.DisplayShips` | String `transform.Find` and destroy/instantiate churn. | Serialized references; pooled cards. |
| U10 | low | `QuestUI.Update` (183), `LobbyUI.Update` | Per-frame polling of service state. | Event-driven refresh. |
| U11 | low | `MatchResultsUI.OnPlayAgainClicked` | Reloads the scene; `GameManager` is `DontDestroyOnLoad` (un-parented in `Awake` per IMPLEMENTATION_STATUS) → after reload the scene's new `GameManager` and the surviving one both exist unless the singleton guard destroys one; needs editor verification. | Verify in editor; prefer an in-scene restart over reload. |

## Verified statically / to check in editor
- Verified: scene → script guid map, Build Settings list, `Resources` folder tree, all `LoadScene` call sites.
- To check in editor: whether `MainMenuScene` opens without errors; the visual state of the hub prefabs from `Sci-Fi UI Collection` (third-party, not read); Play Again after a full match.
