# Gravity Wars – Code Audit 0.1

Companion to `docs/GRAVITY_WARS_GDD.md` (GDD). Everything below was found by reading code, scene and prefab YAML statically on 2026-09-30; nothing was compiled or run. Line numbers are approximate to the commit assessed. Detailed evidence per system: `docs/_work/*.md`.

**Severity:** *critical* = blocks a playable build or corrupts player data · *high* = wrong behaviour the player will hit, or a design rule not enforced · *medium* = bug, duplicate or risk to fix within the milestone that touches the system · *low* = cleanup, perf or hygiene.

**Counts:** 4 critical · 28 high · 54 medium · 58 low · 1 design decision (145 entries).

Rule for maintaining this file (see `CLAUDE.md`): remove an entry when fixed and note the id in the GDD Changelog; add new findings with the same format.

---

## 1. Scenes, build settings and navigation

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| G2 | critical | `ProjectSettings/EditorBuildSettings.asset`; `Assets/MainMenu.cs:107` (`MainMenuManager.StartHotseatMode`); `Assets/Scenes/HotSeat.unity` | Build contains SplashScreen, MainMenu, SampleScene only. The menu loads `"HotSeat"` (not in build; stale `GameManager` serialization lacks `bubbleTimer`, health bars, perk icons → NRE in `TurnTimer`/`UpdateFightingUI_AtRoundStart`). The working scene is `HotSeat 1.unity`. | Decide OQ1; add the match scene and hub to the build; introduce `SceneNames` constants; delete `HotSeat.unity`. |
| U1 | critical | same as G2 plus `Networking/UI/LobbyUI.cs:351`, `UI/MatchResultsUI.cs:74` | No path from a player build to a match; results screen returns to `"MainMenu"` only if loadable. | Same as G2. |
| G3 | high | `GameManager.Awake` | `DontDestroyOnLoad` (with `SetParent(null)`) on an object holding scene references (texts, sliders, canvases, `setupScreen`). After a scene reload the survivor points at destroyed objects and the new scene's `GameManager` destroys itself or duplicates. | Remove `DontDestroyOnLoad` from `GameManager`; carry rematch data in a small persistent object or static. |
| U2 | high | `Assets/Scenes/MainMenuScene.unity` | Hub scene contains only `ShipViewer3D` and `ShipsGarageController`; `MainMenuController`, `MainMenuUI`, `ShipsGarageUI`, `ShipInventoryCard` prefab are absent → `ShipsGarageController.ValidateReferences` errors, nothing displays. | Editor task per archived `Main Menu Hub Build Guide.md` (M3). |
| U3 | high | `UI/MainMenu/MainMenuController.cs:31-40`, `LoadScene` | Ten scene names (`RankedMatchmaking`, `LocalHotseat`, `ShipsGarage`, …) for scenes that do not exist. | In-scene panels for hub screens; scene load only for the match (OQ1). |
| U4 | high | `UI/MainMenu/ShipViewer3D.LoadShipPrefab` (174-197) | Looks for `Resources/Ships/{id}` etc.; no ship prefabs exist under `Resources` → viewer never shows a ship. | Load `ShipBodySO.visualPrefab` via `ProgressionManager.allShipBodies`. |
| U5 | high | `UI/ShipsGarage/ShipsGarageController.EquipSelectedShip` (343) vs `UI/MissileSelectionUI.ShowForEquippedLoadout` (78), `MatchLoadoutBridge`, `GameManager.ResolveEquippedLoadout` | `currentEquippedShipId` is written with a `ShipBodySO.name` by the garage and read as a loadout/preset id elsewhere. | One id space (OQ5: loadout id); garage creates/equips a loadout. |
| U7 | medium | `ShipsGarageController.GrantStarterShips` (134-152) | "For testing" grant of one body per archetype on first open. | Delete; starter content comes from `ProgressionManager` only. |
| U8 | medium | `Assets/MainMenu.cs` (`MainMenuManager`) | Advertises AI/Puzzle/Sandbox/PvP/Tournament/Login/Settings that only log "not yet implemented"; bot mode reachable only via scene fields. | Replace with the hub; until then wire "VS. AI" to load the match with `player2IsBot = true` through a static match-config. |
| U6 | medium | `UI/NextUnlockWidget.cs:35`, `UI/MainMenu/MainMenuUI.cs:180`, `UI/ShipsGarage/ShipsGarageUI.cs:270,303` | Duplicated/hard-coded progression numbers (`1000 + level*500`, `baseMissileDamage = 1000`, `"0/275"`). | Read from the single XP tables / equipped missile. |
| U11 | medium | `UI/MatchResultsUI.OnPlayAgainClicked` | Reloads the scene while `GameManager` is persistent (see G3). | Fix with G3; prefer in-scene restart. |
| U9 | low | `MainMenuUI.SetPanelActive` (409), `ShipsGarageUI.DisplayShips`/`ClearInventory` | `transform.Find(string)`; card destroy/instantiate on every filter. | Serialized references; pooled cards. |
| U10 | low | `Quests/UI/QuestUI.Update` (183), `Networking/UI/LobbyUI.Update` | Per-frame polling of services / `Time.frameCount % 120`. | Event-driven refresh. |
| H6 | medium | `Assets/Scenes/SampleScene.unity` → `Obsolete/GameSetup.cs`; `Assets/missilePrefab.prefab` → `Obsolete/MissileOLD.cs` | Only references keeping `Assets/Obsolete` alive; SampleScene is in the build. | Remove SampleScene from the build, delete `missilePrefab.prefab`, delete `Assets/Obsolete`. |

## 2. Gameplay and physics

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| G1 | critical | `GameManager.AwardMatchProgression` | Calls `ProgressionManager.AwardMatchXP` for winner **and** loser against the single local account → double XP/credits/BP XP, `totalMatchesPlayed += 2`, win streak reset by the loser call right after the winner call. | Award only the local player's result (`player1Won ? winner : loser`); the bot/P2 has no account. |
| G4 | medium | `PlayerShip.WarpShip`/`FindWarpPosition` | AP consumed before a spot is found; never calls `GameManager.PlayerActionUsed`; turn does not end; stats not recorded. | Find position first, then route through `PlayerActionUsed` like slingshot. |
| G5 | medium | `Missile3D.HandleCollision` (~910) | `if (!ship.unmovable && ship.isPassiveUnlocked)` gates knockback on the target being level ≥ 10. | `if (!(ship.unmovable && ship.isPassiveUnlocked))`. |
| G6 | medium | `GameManager.MissileLostInSpace` | No `roundEndPending` / null guard (same race class as the fixed `OnMissileDestroyed`). | Mirror the guard. |
| G7 | medium | `GameManager.OnMissileDestroyed`/`BeginMultiMissile`; `Missile3D.SplitCluster`; `MissileBarragePerk` | Only Multi Missile registers 3 missiles; cluster/barrage extras are uncounted → turn ends while missiles fly. | Live-missile registry (register on spawn, unregister on destroy). |
| G8 | medium | `Missile3D.Update` | Every live missile polls `Input.GetKeyDown(Space)` → one press detonates/splits all. | `GameManager` owns the detonate input, forwards to the newest missile. |
| G9 | medium | `MatchLoadoutBridge.ApplyLoadout`; `ShipPresetSO.GetLevelingFormula`; `PlayerShip.UpdateStatsFromHardcodedFormulas` | Custom loadouts get no `levelingFormula`; `Resources.Load("{archetype}LevelingFormula")` finds nothing; hard-coded fallback numbers differ from the SO assets. | Bridge assigns the archetype formula (move the four formula assets under `Resources/…` or reference them from a config SO); delete the fallback. |
| G10 | medium | `GameManager.SumLevelUpXP` | Duplicates the account XP formula and assumes per-level reset; over-reports on the results screen. | Use `AccountXPTable`; report `result.accountXP`. |
| G11 | medium | `GameManager.PopulatePassivesUI`, `PlayerShip.Start` | Passive unlock level 10 hard-coded twice; `PassiveAbilitySO.unlockLevel` ignored. | Read the SO value. |
| G13 | medium | `CameraController.FollowMissile`/`FocusAllShips` | `FindObjectsOfType<PlayerShip>()` twice per frame. | Cache from `GameManager`. |
| G14 | medium | `Missile3D.AddTrajectoryPoint` | `SetPositions(list.ToArray())` every 0.01 s → O(n²) copies and allocations per missile. | `positionCount++`, `SetPosition(count-1)`. |
| G15 | medium | `Missile3D.CreateExplosionEffect`, `PlayerShip.CreateExplosionEffect`, `Missile3D.SetupTrajectoryLine`, ghost material in `PlayerShip.Start` | New `Material`/`Texture2D` per missile/explosion, never destroyed; explosion code duplicated. | One pooled explosion prefab; shared materials; `MaterialPropertyBlock`. |
| G19 | low | `MatchStatsTracker.RecordDamageTaken` | Every damage event counts as a hit → accuracy > 100 % with multi/cluster. | Count hits per missile. |
| G20 | low | `PlayerShip.CanUseMissile`, `GetAllowedMissileTypes` | Dead hard-coded missile-restriction duplicate. | Delete; body flags only. |
| G21 | low | `PlayerShip.FindWarpPosition`/`ShipOverlapsWithPlanet` | Duplicates `GameManager.ClearanceFromPlanets`. | Call the helper. |
| G22 | low | `Assets/Gravity1.prefab` (`shipXP = 189050`), `PlayerShip.shipXP` default 6250 | Every hotseat ship spawns at ship level 20; gating never seen in playtests. | Prefab XP 0; bridge sets XP for prebuilt ships too (S2). |
| G23 | low | `ScoreKeeper`, `ScrollingBackground` (stale scenes only), `TournamentMode` (`Enabled` never set), `MutatorSystem.Override`, `PerkManager.HandleShotFired` | Dead or unreachable code. | Delete or wire through config. |
| G24 | low | `GameManager.CountdownTimer`/`TurnTimer` | Fixed decrements after `WaitForSeconds` drift; string allocations per tick. | Elapsed `Time.time`; update text only when the integer changes. |
| G25 | low | `Missile3D.SetupAudio`, `AudioManager.Setup3DAudioSource` | `spatialBlend = 0` "temporarily" → 3D audio disabled. | OQ19. |
| G26 | design | `PlayerShip.OnCollisionEnter` | Any planet contact (including after knockback) destroys the ship. | OQ13. |
| G12 | low | `GameManager.PassiveType` (nested) vs global `PassiveType` in `PassiveAbilitySO.cs` | Two enums, different order; HUD icon arrays indexed by the nested one. | One enum; explicit icon map. |
| G16 | low | `Missile3D.RotateMissile/VisualizeThresholds/UpdateTrajectory` | Debug draws every FixedUpdate. | Guard with `DebugSettings`. |
| G17 | low | `PlayerShip.Update` (netcode branch) | `GameManager.Instance?.GetComponent<GameManagerNetworkAdapter>()` per frame. | Cache. |
| G18 | low | `PlayerShip.RegenerationCoroutine` | `new WaitForSeconds(0.05f)` 20×/s. | Cache the yield or tick in `Update`. |
| BOT1 | medium | `Bot/BotController.FindBestShot`/`SimulateShot` | ≈ 225 shots × 600 steps × planets simulated in one frame → hitch on the bot's turn. | Spread over frames or run as a job. |
| BOT2 | medium | `BotController.SimulateShot`; `PlayerShip.PredictMissileTrajectory`; `Missile3D.Launch/MoveMissile` | Third/fourth copy of the flight model and the `0.5f` launch factor. | Extract `MissileFlightModel.Step()`; `PhysicsConfigSO` for G, launch factor, armor constant. |
| BOT3 | low | `BotController` fields | Difficulty numbers, hit radius, think times on the component. | `BotDifficultySO` tiers (OQ14). |
| BOT4 | low | `BotController.HitsPlanet` | Radius from `localScale.x * 0.5`. | Use the planet collider radius. |
| BOT6 | low | `BotController.Update` | Per-frame turn polling. | `GameManager` notifies the bot on turn start. |

## 3. Ships, archetypes and content

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| SH1 | high | `Ship System/ShipBodySO.cs` `[Range]` attributes (HP 8000–15000, armor 80–120, dmg 0.8–1.2) vs generated bodies (Tank 16500–17500 HP, DD armor 52, dmg 2.2) | Touching any slider in the Inspector clamps a generated body to wrong values. | Widen or remove the ranges; keep `OnValidate` archetype rules only. |
| SH2 | high | `Ship System/MoveTypeSO.OnValidate` vs `Editor/GameContentGenerator.cs` (Warp `allowDamageDealer/allowAllAround = true`) | Validator flips the generated Warp flags back to Controller-only the next time the asset is touched. | OQ6, then align generator and validator. |
| SH3 | medium | `Assets/Ship System/*.asset` (Star Sparrow, Frame, Standard, Standard Move, 4 formulas), `Assets/Gravity1.prefab` | Reference assets outside `Resources` are invisible to `ProgressionManager.PopulateContentDatabases`; the ship prefab still references the Star Sparrow preset; `PlayerAccountData.InitializeDefaultUnlocks` unlocks body id `"Standard"` which matches nothing. | Move reference assets under `Resources/GeneratedContent` (or make the generator emit them); prefab preset = `starter_ship`; fix the default id list. |
| SH4 | medium | `Editor/GameContentGenerator.cs` levels 46–98 (8 passives, 7 bodies, 9 perks) vs `ProgressionManager.CheckAccountLevelUp` cap 50 | Unreachable content. | OQ3; re-schedule in the generator. |
| SH5 | medium | `ShipPresetSO.passives[]` + `ApplyToShip` (stacks every passive) vs builder rule "exactly one" | Prebuilt data model allows multiple passives; UI/validator do not. | Single `passive` field. |
| SH6 | medium | `ShipBodySO.visualPrefab` | Never read; every ship uses the prefab mesh; the hub viewer cannot show bodies (U4). | Instantiate `visualPrefab` under the ship root in `ShipPresetSO.ApplyToShip` and in `ShipViewer3D`. |
| SH7 | low | `Ship System/ArchetypeRestrictionChecker.cs` | Dead duplicate of `ShipPresetSO.Validate`. | Delete. |
| SH8 | low | `Online/ProgressionSystem.cs` enum `ShipClass` | Duplicate of `ShipArchetype`. | Delete; tables use `ShipArchetype`. |
| SH9 | low | `MoveTypeSO.ApplyToShip` (warp block commented out) | Warp parameters on the SO are not applied; `PlayerShip` keeps its own copies. | Apply or remove the SO fields. |
| SH10 | low | `ShipLevelingFormulaSO.OnValidate` warnings | Stale against the deliberate rebalance. | Update thresholds or remove. |
| SH11 | low | `seasonal_scout_free`/`seasonal_defender_free` (`isPremiumShip = false`, levels 10/20) | Granted both by level (`UnlockShipsForLevel`) and by the battle pass. | Mark BP-only (`isPremiumShip` or a new `unlockSource`). |
| SH12 | low | generated passives/ships `icon = null`; `MissileBarrage` reuses the Cluster icon | Missing art. | Art task; placeholder icons. |

## 4. Missiles

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| M1 | medium | `Online/MissileRetrofitSystem.cs` (second enum `MissileType`, 19-entry table with 9 non-existent ids, `tactical_emp` level 13 vs asset 0) | Third restriction/unlock rule set contradicting the assets. | Delete; derive from `MissilePresetSO.requiredAccountLevel` and body flags. |
| M5 | medium | `+Active Perks+/MissileBarragePerk.FireMissileBarrage` | Spawns missiles without `ApplyMissilePreset` (prefab mass 10, maxVel 10). | Route through `PlayerShip`'s spawn helper (P1). |
| M2 | low | `MissilePresetSO.ApplyToMissile` | `selfDestructPushStrength`, `launchSound`, `flyingSound`, `explosionSound`, `trailColor` never used. | Apply or remove. |
| M3 | low | `Assets/missilePrefab.prefab` | Uses obsolete `Missile` class. | Delete (H6). |
| M4 | low | `Assets/Missile.prefab` vs `Assets/Missile3D.prefab` | Two prefabs with `Missile3D`; prefab and `Standard.asset.visualModelPrefab` disagree. | Keep one. |

## 5. Perks

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| P1 | high | `MissileBarragePerk.Activate/FireMissileBarrage` + `PlayerShip.FireMissile` | 5 missiles instead of 4; extras use prefab stats; turn-end race (G7). | Make barrage a `next*` flag handled in `FireMissile`; register missile count. |
| P2 | high | `Assets/+Active Perks+/Overcharged Cannon SO.asset` (`damageMultiplier: 100`) | 100× damage if any designer picks it. | Set to 1.5 (generator value) or delete the hand asset. |
| P3 | medium | every `*Perk.cs` `used` flag; `BoostJetsPerk._usedThisTurn` | Tier rules not as intended (all perks once per round). | OQ7; single limiter in `PerkManager`. |
| P4 | medium | `BoostJetsPerk.CanActivate` (needs Move mode) vs `PerkManager.ActivateToggledPerk` (only from `FireMissile`) | Boost Jets never activatable. | Call `ActivateToggledPerk` from the move path, or delete the perk. |
| P5 | low | `OverchargedCannonPerk.ResetAfterShot` | Fragile reset on `shotsThisRound`. | Reset in `ConsumeToggledPerk`. |
| P6 | low | `Assets/+Active Perks+/PusherMissile SO.asset` | `perkName` empty. | Fill in. |
| P7 | low | `ActivePerkSO.OnValidate` | `cost = tier`, `minLevel` 5/15/20 hard-coded. | `PerkRulesSO`. |
| P8 | low | `PerkManager.HandleShotFired` | Dead. | Delete. |
| P9 | low | `ExtendedProgressionData.ACTIVE_UNLOCKS` (20 fictional actives) | Aspirational table with no assets; misfiles tiers (B3). | Delete with A8. |

## 6. Account progression and player data

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| A1 | high | `ProgressionManager.Initialize` → `GrantStarterContent` before `PopulateContentDatabases` | New accounts get no database-driven starter content. | Populate first. |
| A2 | high | `ProgressionManager.CheckAccountLevelUp` | Cumulative XP vs per-level threshold → every level after 2 costs 500 XP; `xpForNextLevel` never updated. | `AccountXPTable` (OQ3); subtract or use cumulative thresholds consistently; update `xpForNextLevel`. |
| A3 | high | local unlock path (`ProgressionManager.UnlockShipsForLevel` only) | Bodies, passives, perks, move types, missiles never unlock by account level → builder unusable for a fresh account. | `UnlockContentForLevel` over every SO list using `requiredAccountLevel`. |
| A4 | critical | see G1 | Double award. | see G1. |
| A8 | high | `Online/ExtendedProgressionData.cs`, `ProgressionSystem.SHIP_UNLOCKS`, `MissileRetrofitSystem.MISSILE_UNLOCKS` | Three id tables disagreeing with assets and each other. | Delete; SOs are the schedule. `NextUnlockWidget` walks `ProgressionManager.all*`. |
| A9 | high | `PlayerAccountData` `DateTime` fields (`accountCreatedDate`, `lastLoginDate`, `ShipProgressionEntry.firstUsedDate/lastUsedDate`, `MatchResultData.matchDate`, quest dates) | `JsonUtility` drops them → reset on every load; `lastLoginTimestamp` = 1970 breaks cloud merge (A12). | Unix `long` only; add a `saveVersion` migration (A6). |
| A5 | medium | `Online/AccountSystem.CreateNewPlayerProfile` vs `ProgressionManager.GrantStarterContent` | Different starter economies (gems 0 vs 50), rank Lieutenant vs ELO 800 (Ensign). | `NewAccountDefaultsSO`. |
| A6 | medium | `PlayerAccountData.saveVersion` | No migration code. | Versioned migration or remove the field. |
| A7 | medium | `Online/AccountSystem` | No lazy bootstrap, never in a scene → every `AccountSystem.Instance` branch is dead in the tested flow. | Decide online scope (M7/M8); until then treat as absent. |
| A10 | low | `PlayerAccountData.unlockedActives` getter | Allocates on every access. | Cache. |
| A12 | low | `SaveSystem.LoadPlayerDataWithCloudMergeAsync` | "Merge" picks by a timestamp that is always 1970 and max currency; never called. | Real merge or delete. |
| SV9 | low | `AccountSystem` lines 215-216 | Starter ship id / credits duplicated. | Same SO as A5. |

## 7. Ship progression

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| S1 | high | `PlayerShip.RecalcLevelFromXP`/`XPNeededForNext` vs `ShipProgressionEntry.AddXP`/`GetXPRequiredForLevel` | Same `200 + 75·L²` read as per-level cost vs cumulative → different levels in match and garage. | `ShipXPTable` (OQ4) used by both. |
| S2 | high | `GameManager.ResolveEquippedLoadout`, `MatchLoadoutBridge.ApplyPreset`, `ProgressionManager.AwardMatchXP` | Prebuilt ships never gain XP and always fight at the prefab's level 20. | Progression entry for every ship (preset id or loadout id); bridge sets `shipXP`. |
| S3 | medium | `CustomShipLoadout.GetProgressionKey` | Editing perks/passive orphans the XP entry. | OQ4 (key by `loadoutID`). |
| S4 | medium | `PlayerShip.shipXP` default 6250; `Gravity1.prefab` 189050 | Test values in code/prefab. | 0; bridge sets XP. |
| S5 | low | `ShipProgressionEntry.matchesPlayed/…/totalKills` | Never incremented; `ProgressionUI` shows them. | Update in `AwardMatchXP`. |
| S6 | low | `PlayerAccountData.UnlockMasterySkin` | Skin ids without assets. | Cosmetics milestone (OQ15). |

## 8. Economy and cosmetics

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| E1 | high | design (`ProgressionManager.SpendCurrency` has no callers) | Credits have no sink; premium pass is the only gem sink; no store. | OQ8. |
| E2 | medium | `BattlePassUI.OnPurchasePremium` / `BattlePassSystem.PurchasePremiumPass` | Price 1000 hard-coded twice. | One config value. |
| E5 | medium | `ProgressionManager.SpendCurrency`, `EconomyValidator` | Validation client-side only. | Server-side in M8; document as client-trusted until then. |
| E3 | low | `CosmeticsSystem.CosmeticsApplier.GetUnlockedColorSchemes` | Reads `unlockedSkins` instead of `unlockedColorSchemeIDs`. | Fix list. |
| E4 | low | `ColorSchemeSO.ApplyToShip`, `CosmeticsApplier.ApplyDecal` | New `Material` per renderer per apply. | `MaterialPropertyBlock`. |

## 9. Battle pass and ranked

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| B1 | high | `Online/BattlePassSystem` (`seasonEndTimestamp` unused) | No season clock; seasons change only by editing `currentSeason`. | `SeasonConfigSO` with start/end; check in `LoadFromProfile` (OQ9). |
| B2 | medium | `BattlePassSystem.FREE_TRACK_REWARDS/PREMIUM_TRACK_REWARDS` | Hard-coded dictionaries; skin ids without content. | `BattlePassSeasonSO`. |
| B3 | medium | `PlayerAccountData.UnlockById` + `ExtendedProgressionData.GetActiveTier` | Perk tier resolved from a fictional table (any T2 reward would file as T1). | Resolve via `ProgressionManager.allPerks[].tier`. |
| B4 | medium | `Progression System/BattlePassData.cs`, `Resources/GeneratedContent/BattlePass/Season1_BattlePass.asset` | Orphaned second implementation. | Delete asset + classes; keep `RewardType`. |
| B5 | medium | `RankedSeasonSystem.SOFT_RESET_ANCHOR` 1200 vs `ELORatingSystem.STARTING_ELO` 800 | Rollover raises low ratings. | OQ10. |
| B6 | medium | `Online/MatchHistoryManager` (402-443) | Blocking waits; only ELO writer; compiled out. | Rewrite async in M8. |
| B7 | low | `BattlePassSystem.maxLevel/xpPerLevel` `[SerializeField]` on a runtime-created object | Not editable. | Config SO. |
| B8 | low | `BattlePassUI.UpdateHeader` | Tier shown as level+1. | Align wording. |
| A-rank | medium | `PlayerAccountData.UpdateRankFromELO`, `ELORatingSystem.GetRankFromELO`, `RankedSeasonSystem.RankFromELO`, `Online/RankConfiguration.cs` | Rank table in three places plus an unreferenced fourth. | One table in `ELORatingSystem`; delete `RankConfiguration`. |

## 10. Quests and achievements

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| Q2 | high | `Networking/Services/QuestService.LoadQuestsFromCloud/SaveQuestsToCloud` | No persistence; fresh quests every launch. | Serialize into `PlayerAccountData.activeQuests` (long timestamps). |
| Q3 | high | `Quests/GameManagerQuestIntegration` (not in scene; `OnPlayerFireMissile`/`OnShipDestroyedWithMissile` never called); `Quests/ProgressionManagerQuestIntegration` (no callers) | 13 of 24 templates cannot progress. | Feed from `MatchStatsTracker`/`ProgressionManager` events; auto-create the integration. |
| Q1 | medium | `QuestService.OnQuestCompleted` + `ClaimQuest` | Double reward. | Reward on claim only. |
| Q4 | low | `QuestService.Update` | `Time.frameCount % 3600` timer. | Elapsed time. |
| Q5 | low | `QuestDataSO.requiredArchetype` | AllAround doubles as "any". | Explicit `Any`. |
| AC1 | high | `Achievements/AchievementService.AwardAchievementRewards` | Every reward line commented out. | Route through `ProgressionManager`. |
| AC2 | high | `AchievementService.SaveAchievementsToCloud/LoadAchievementsFromCloud` | No persistence. | Persist instances in `PlayerAccountData`. |
| AC5 | medium | `Achievements/GameManagerAchievementIntegration` | Archetype/flawless/accuracy/map contexts never populated → wrong unlocks. | Feed from `MatchStatsTracker`; set archetype from `player1Ship`. |
| AC3 | low | `AchievementSaveData.lifetimeStats` (`Dictionary`) | Not serializable with `JsonUtility`. | Parallel lists. |
| AC4 | low | `AchievementInstance.IsClaimable` (`isUnlocked && !isUnlocked`) | Always false. | Fix or delete. |
| AC6 | low | `ProgressionManagerAchievementIntegration.CheckAll*` | Hard-coded content counts. | Count from `ProgressionManager.all*`. |
| AC7 | low | `[RequireComponent(typeof(ProgressionManager))]` on both ProgressionManager integrations | Cannot attach to the runtime singleton object. | Auto-add in `ProgressionManager.Awake` or drop the attribute. |

## 11. Leaderboards

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| L1 | high (M8) | `Leaderboards/LeaderboardService.SubmitToLeaderboard/FetchLeaderboardFromServer`; `leaderboardDefinitions` empty at runtime | UGS calls TODO; mock data; zero definitions → `SubmitScore` always false. | Implement with `Unity.Services.Leaderboards`; load definitions from Resources; hide the screen until then. |
| L2 | medium | `LeaderboardService.ValidateScore` (damage cap 10 000) | Below real per-match damage. | Caps from balance config. |
| L3 | medium | `GameManagerLeaderboardIntegration.SaveStats/LoadStats` (`Leaderboard_*` PlayerPrefs) | Third copy of lifetime stats. | Read `PlayerAccountData`. |
| L4 | low | `LeaderboardData.LeaderboardShipFilter` (Tank/Sniper/AllAround/Glass) | Stale archetype names (also quest template names "Sniper"). | Use `ShipArchetype`. |
| L5 | low | `LeaderboardService.GetPlayerID/GetPlayerName` | Hard-coded ids. | `ServiceLocator.GetPlayerId()`. |

## 12. Services, save, analytics, anti-cheat

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| SV1 | high | `CloudSave/SaveData.cs`, `CloudSave/SaveManager.cs`, `Networking/Integration/ServiceIntegrationHelper.cs` | Second complete save model used by nothing the game reads. | OQ16: delete; cloud-save `PlayerAccountData` only. |
| SV2 | high | `Networking/ServiceLocator.Start` (anonymous sign-in) vs `Online/AccountSystem.Start` (username/password) | Two UGS init/auth flows; identity depends on Awake order. | One bootstrap; anonymous-first with optional account link. |
| SV3 | medium | `Networking/Services/AnalyticsService.cs:78,96,124` | SDK calls commented out. | Wire with consent or delete the layer. |
| SV4 | medium | `EconomyValidator`, `RateLimiter`, `SuspiciousActivityDetector` | Dead client-side "anti-cheat". | Delete until server-side exists. |
| SV5 | medium | `SaveSystem.SavePlayerDataToCloudAsync`, `ServiceLocator.Start`, `AccountSystem.Start`, `MainMenuController.Start`, `SettingsUI.OnLogoutClicked` | `async void` fire-and-forget. | Return `Task`; await from a bootstrap coroutine. |
| SV6 | medium | `MatchHistoryManager` 402-443 | Blocking `.Wait()`-style. | Async. |
| SV7 | low | `CloudSaveService` hash keys | Client-computed "tamper" check. | Rename to integrity; server-side later. |
| SV8 | low | `Debug/DebugSystemsUI.OnAddCurrency` | Edits the dead `SaveData`. | Point at `ProgressionManager`. |

## 13. Multiplayer (all under `#if UNITY_NETCODE_GAMEOBJECTS`, define not set)

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| N1 | high | `Networking/LobbyManager.cs`, `MatchManager.cs`, `NetworkManager.cs`, `NetworkedPlayerShip.cs`, `OnlineGameAdapter.cs`, `Multiplayer/*.cs` (`using Unity.Netcode` / `Unity.Services.Lobbies` commented out); `MatchManager.cs:214,249` (`GameManager.StartGamePhase`/`StartPlayerTurn` do not exist as public methods); `NetworkGameManager.cs` (`NetworkedPlayerShip.IsDead/Respawn/GetHealth/OnDeath` missing) | Enabling the define fails to compile in ~10 files. | OQ12; delete the losing stack; restore usings; compile in a branch. |
| N2 | high | `Assets/Networking` vs `Assets/Multiplayer` (+ real-time `NetworkGameManager`) | Two incompatible architectures, three rule sets. | Keep stack B turn machine + lobby/relay from stack A (OQ12). |
| N3 | high (design) | stack B + `Missile3D` `Rigidbody` | Lockstep without determinism guarantees or resync. | Host-authoritative with the shared `MissileFlightModel` (BOT2). |
| N4 | medium | `Multiplayer/NetworkGameManager.AwardMatchRewards/AwardRewardsClientRpc` | Fourth reward table, applied on the client. | One reward path (`AwardMatchProgression`) fed by the server result. |
| N5 | medium | `LobbyUI.StartMatch` (351), `OnlineGameAdapter.ReturnToLobbyCoroutine` (343) | Load `"HotSeat"` / `"MainMenu"`. | `SceneNames`. |
| N6 | medium | `LobbyUI.OnReadyButtonClicked`, `CheckIfBothPlayersReady`, `Update` | Ready never synced; frame-count polling. | Lobby player data + timed polling/events. |
| N7 | medium | `NetworkTurnCoordinator._currentTick`, `NetworkInputManager._currentTick` | Per-peer ticks not comparable. | Server-issued sequence number. |
| N8 | medium | `NetworkInputManager.ValidateFireAction/ValidateMoveAction` (`return true`) | No validation. | Use `NetworkTurnCoordinator.ValidatePlayerAction(senderId)`. |
| N9 | medium | `GameManagerNetworkAdapter.ExecuteNetworkMoveAction` | Teleports the ship. | Use the local move code. |
| N13 | medium | `NetworkTurnCoordinator.InitializeArenaClientRpc` | Respawns ships from the prefab without loadouts. | Apply loadouts as `GameManager` 822-827 does. |
| N10 | low | `NetworkTurnCoordinator.OnGUI`, `NetworkInputManager.OnGUI`, `NetworkedPlayerShip.OnGUI` | Debug overlays. | Development-build guard. |
| N11 | low | `ConnectionManager.maxConnections = 4` | Inconsistent player limit; Relay path under never-defined `UNITY_SERVICES_RELAY`. | Constant 2; drop the extra define (no asmdef version defines exist). |
| N12 | low | `OnlineMatchmakingUI.cs:259,273` | Unguarded `Unity.Services.Lobbies.Models.Lobby` signatures. | Guard. |

## 14. Code hygiene and conventions

| ID | Sev | File / class / method | Problem | Suggested fix |
|---|---|---|---|---|
| H1 | low | `Assets/MainMenu.cs` (`MainMenuManager`), `Assets/SplasScreenManager.cs` (`SplashScreenManager`), `Assets/ScrollingBackround.cs` (`ScrollingBackground`), `Networking/NetworkManager.cs` (`GravityWarsNetworkManager`) | File name ≠ class name. Existing scene/prefab references keep working (`MainMenu.unity` references `MainMenu.cs`), but Unity refuses to add such a `MonoBehaviour` from the Inspector and shows a warning on the script asset (verify in editor). | Rename files when touched. |
| H2 | low | two `MissileType` enums, two `PassiveType` enums, `ShipClass`/`ShipArchetype`, `LeaderboardShipFilter` | Parallel enums for one concept. | One enum each (M1, G12, SH8, L4). |
| H3 | low | namespaces: most code global, `GravityWars.Networking/Multiplayer/CloudSave/DebugUI/Online` for the rest | Inconsistent; name collisions (`NetworkManager`, `MissileType`). | Adopt `GravityWars.<System>` for new code; migrate when touched. |
| H4 | low | no `.asmdef`, no tests, no CI | Full recompiles; no regression net. | Optional: assembly per system after M2; edit-mode tests for XP tables and validators. |
| H5 | low | `Assets/Resources/BillingMode.json`, `UnityPlayerAccountSettings.asset`, `Assets/DefaultNetworkPrefabs.asset` | Package artifacts committed. | Keep; document. |
| H7 | low | `README.md` controls/versions | Documented controls (A/D, W/S, E, Tab) and Unity 2021.3 do not match code. | Fixed in this pass (README rewritten). |

---

## Prioritized refactor plan (one session each)

Order follows the GDD milestones; each step is small enough to review in one sitting and ends with a commit.

| # | Step | Closes | Milestone |
|---|---|---|---|
| 1 | Add `SceneNames` static class; replace every `LoadScene("…")`; decide OQ1; Thomas adds `MainMenuScene` + `HotSeat 1` (renamed `Match`) to Build Settings and removes `SampleScene`/`MainMenu` | G2, U1, U3 (scene part), N5 | M1 |
| 2 | `GameManager.AwardMatchProgression`: award the local player only; remove `DontDestroyOnLoad` from `GameManager`; set `Gravity1.prefab` `shipXP` to 0 and `PlayerShip.shipXP` default to 0 | G1/A4, G3, U11, G22, S4 | M1 |
| 3 | Editor session (Thomas): place `MatchResultsUI`, `SettingsUI`, `MissileSelectionUI`, `KillshotReplayUI` panels in the match/hub scenes per their header comments; verify the loop | U-panels | M1 |
| 4 | Delete `Assets/Obsolete`, `SampleScene.unity`, `HotSeat.unity`, `MainMenu.unity`, `missilePrefab.prefab`, `Missile3D.prefab` (keep `Missile.prefab`), `ScoreKeeper`, `TournamentMode`, `ArchetypeRestrictionChecker`, `RankConfiguration`, `BattlePassData` + orphan asset | H6, M3, M4, G23, SH7, B4, A-rank (part) | M1 |
| 5 | `AccountXPTable` (OQ3): one formula, XP handling, `xpForNextLevel` maintained; `GameManager.SumLevelUpXP`, `ProgressionUI`, `NextUnlockWidget` read it; fix starter-content order | A1, A2, G10, U6 | M2 |
| 6 | `ProgressionManager.UnlockContentForLevel` over bodies/passives/perks/moves/missiles by `requiredAccountLevel`; delete `ExtendedProgressionData`, `ProgressionSystem.SHIP_UNLOCKS`, `MissileRetrofitSystem`, `ShipClass`; `UnlockById` resolves perk tier from the SO; `NextUnlockWidget` walks SOs | A3, A8, M1, SH8, B3, P9 | M2 |
| 7 | `ShipXPTable` (OQ4); every ship gets a progression entry; bridge sets XP and leveling formula; delete hard-coded formulas | S1, S2, S3, S5, G9 | M2 |
| 8 | Equipped id = loadout id everywhere; garage creates/equips loadouts; remove `GrantStarterShips` | U5, U7 | M2 |
| 9 | Timestamps to `long`, `saveVersion` migration, delete the cloud "merge" | A9, A6, A12 | M2 |
| 10 | Delete `CloudSave/SaveData`, `SaveManager`, `ServiceIntegrationHelper`, `EconomyValidator`, `RateLimiter`, `SuspiciousActivityDetector`; `DebugSystemsUI` → `ProgressionManager`; one UGS bootstrap | SV1, SV4, SV8, SV2 | M2 |
| 11 | Generator fixes: widen `ShipBodySO` ranges, OQ6 flags, single `passive`, reference assets emitted into Resources, re-schedule levels > cap; Thomas re-runs the generator | SH1–SH5, SH11, A5 | M2 |
| 12 | Hub wiring (editor, Thomas) + `ShipViewer3D` from `visualPrefab` + `visualPrefab` instantiated on ships | U2, U4, SH6 | M3 |
| 13 | `MissileFlightModel` + `PhysicsConfigSO` shared by `Missile3D`, preview, bot | BOT2, G-hard-coded physics numbers | M4 |
| 14 | Perk rules (OQ7): single limiter in `PerkManager`; barrage as `next*` flag; boost jets from the move path; fix hand assets | P1–P4, P6, M5 | M4 |
| 15 | Live-missile registry, `GameManager`-owned detonate input, warp through `PlayerActionUsed`, knockback gate, lost-in-space guard, `MatchRulesSO` (OQ17) | G4–G8 | M4 |
| 16 | Quest persistence + feeds from `MatchStatsTracker` events; auto-create integrations; reward on claim only | Q1–Q3, AC7 | M5 |
| 17 | Achievement rewards + persistence + correct contexts | AC1, AC2, AC5, AC3, AC4 | M5 |
| 18 | `SeasonConfigSO` + `BattlePassSeasonSO`; soft-reset anchor; BP XP source (OQ9–OQ11) | B1, B2, B5, B7, E2 | M5 |
| 19 | Credit sink v0 + one cosmetic path (OQ8, OQ15); fix cosmetics bugs | E1, E3, E4, S6 | M5 |
| 20 | Perf pass: trail line, explosion pooling, camera caches, timers, regen tick, bot spread | G13–G18, G24, BOT1, BOT6 | M6 |
| 21 | Networking decision (OQ12): delete the losing stack, enable define in a branch, compile clean, fix N-series | N1–N13 | M7 |
| 22 | Leaderboards, analytics, match history async, server-side validation | L1–L5, SV3, SV6, B6, E5 | M8 |

## Docs triage

All documents listed were moved with `git mv` into `docs/_archive/` (folder structure kept). Nothing was deleted.

| Document (new location under `docs/_archive/`) | Verdict | Where it went |
|---|---|---|
| `README_2025-11.md` (former root `README.md`) | merged into GDD | pitch → GDD §1; physics/damage formulas → §3.4/§3.6; controls → §3.11 (corrected); roadmap → Implementation Plan. New short `README.md` written. |
| `IMPLEMENTATION_STATUS.md` | kept as history | changelog of the 2025-11 → 2026-09 sessions; its "Remaining Unity-Editor work" is now M1/M3 editor tasks; live-playtest findings feed CODE_AUDIT. |
| `🛠️ Custom Ship Builder - Complete Implementation Guide.md` (root copy) | obsolete (duplicate) | identical to the Feature-Guides copy. |
| `Feature-Guides/Custom Ship Builder - Complete Implementation Guide.md` | merged into GDD | rules → §7; screen spec kept as the reference for M3 builder UI. |
| `Feature-Guides/Custom Ship Building Guide.md` | merged into GDD | §7 (slots, validation); "delete prebuilt ships" claim dropped. |
| `Feature-Guides/Complete Progression Guide.md` | merged into GDD with conflicts | AP model → OQ2; BP XP source → OQ11; 100-level schedule → superseded by SO `requiredAccountLevel` (OQ3); ranks → §12.2. |
| `Feature-Guides/Progression System Guide.md` | merged into GDD | feature unlock levels → §8.1; level-up rewards → §8.2 (not built). |
| `Feature-Guides/Rank System.md` | merged into GDD | §12.2 (16 ranks, thresholds, K-factors). |
| `Feature-Guides/Missile Presets Guide.md` | merged into GDD with conflicts | mass factor → §5.1; recommended launch/max ranges do not match assets (noted in `docs/_work/missiles.md`). |
| `Feature-Guides/Ships Garage Implementation Guide.md` | kept as editor guide | referenced by M3 (U2). |
| `Game-Design/Gravity Wars - Complete Game Design Document.md` | superseded by GDD | pillars, glossary, monetization principles → GDD §1/§13; conflicting numbers listed in OQ2/6/10/18 and `docs/_work/inventory.md` §3. |
| `Implementation Plan.md` | superseded by GDD Implementation Plan | online-first ordering replaced by M1–M9; economy numbers not adopted (no code). |
| `Pre-Testing Review.md` | obsolete (historical) | issues resolved per IMPLEMENTATION_STATUS; recurrence noted as SV1. |
| `Setup-Guides/Achievement System Setup.md` | kept as editor guide (with caveats AC1/AC2) | M5. |
| `Setup-Guides/Leaderboard System Setup.md` | kept as editor guide (with caveat L1) | M8. |
| `Setup-Guides/Quest System Setup.md` | kept as editor guide (with caveats Q2/Q3) | M5. |
| `Setup-Guides/Unity Setup Guide.md` | partly obsolete | package list done; Physics2D layer section wrong for the 3D physics used; define instructions → M7 only. |
| `UI-Implementation/Main Menu Guide.md` | merged into GDD | §15 (hub layout intent). |
| `UI-Implementation/Main Menu Hub Build Guide.md` | kept as editor guide | the M3 instruction set. |
| `UI-Implementation/Main Menu Setup Guide.md` | obsolete | old 7-tier ranks, ELO 1200; superseded by the Hub Build Guide. |
| `UI-Implementation/Screen Catalog.md` | merged into GDD | §15.2 screen list; detailed specs stay as reference for M3+. |
