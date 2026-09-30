# Online services, cloud save, analytics, anti-cheat (working notes)

Scope: `Assets/Networking/ServiceLocator.cs`, `Networking/Services/{CloudSaveService,AnalyticsService,EconomyValidator,RateLimiter,SuspiciousActivityDetector,NetworkService}.cs`, `Networking/Integration/ServiceIntegrationHelper.cs`, `CloudSave/{SaveManager,SaveData}.cs`, `Progression System/SaveSystem.cs`, `Analytics/{GameManagerAnalytics,ProgressionManagerAnalytics}.cs`, `Online/AccountSystem.cs`, `Online/MatchHistoryManager.cs`. Quest/achievement/leaderboard services are in their own notes.

## Status summary

| Area | Status | Evidence |
|---|---|---|
| UGS packages | installed | `Packages/manifest.json`: authentication 3.5.2, cloudsave 3.2.2, analytics 6.1.1, economy 3.5.3, leaderboards 2.3.3, lobby 1.3.0, relay 1.2.0, netcode 1.15.0 |
| Service bootstrap | PARTIAL, two competing flows | `ServiceLocator` (anonymous sign-in) vs `AccountSystem` (username/password) — see SV2 |
| Local save | IMPLEMENTED (one path used), duplicated | `SaveSystem` (JSON files) is what `ProgressionManager` uses; `SaveManager` + `SaveData` (PlayerPrefs) is a second, unused model — SV1 |
| Cloud save | DESIGNED - NOT VERIFIED | `CloudSaveService` has real `Unity.Services.CloudSave` calls (`Data.Player.SaveAsync/LoadAsync/DeleteAsync`) but nothing in a scene ever triggers a sync; cannot be verified without a UGS project id |
| Analytics | DESIGNED - NOT BUILT | `AnalyticsService` lines 78/96/124: `StartDataCollection`, `Flush`, `CustomData` are commented out ("TODO: Install Unity Analytics package and uncomment") although the package is installed → no event ever leaves the device |
| Anti-cheat | DESIGNED - NOT BUILT (dead code) | `EconomyValidator`, `RateLimiter`, `SuspiciousActivityDetector` are unreferenced by game code (rg), all client-side |
| Match history | DESIGNED - NOT BUILT | `MatchHistoryManager` is wholly inside `#if UNITY_NETCODE_GAMEOBJECTS` (define not set) |

## Bootstrap and identity

- `ServiceLocator` (`Networking/ServiceLocator.cs`): lazy singleton (`FindObjectOfType` → `new GameObject().AddComponent`, `DontDestroyOnLoad`). `Start()` is `async void`: `UnityServices.InitializeAsync()` then `AuthenticationService.SignInAnonymouslyAsync()`. Registers services with `gameObject.AddComponent<T>()` (line 163). `GetPlayerId()` (line 243) returns `null` unless initialised **and** signed in. `Network` property exists only under the netcode define (line 41).
- `AccountSystem` (`Online/AccountSystem.cs`, plain Awake singleton, not in any scene): `Start()` also `async void` → `InitializeAsync()` calls `UnityServices.InitializeAsync()` and subscribes to auth events; `RegisterAsync` / `LoginAsync` use `SignUpWithUsernamePasswordAsync` / `SignInWithUsernamePasswordAsync`; profile stored through `CloudSaveService` profile keys; `IsUsernameTaken` is a TODO returning false (line 494-499); password reset TODO (477). New accounts get `STARTER_SHIP_ID = "starter_ship"` and `STARTING_CREDITS = 1000` (line 215-216) — a second starter definition next to `ProgressionManager.CreateNewPlayer` (see account-progression.md).
- Consumers pick a profile with `AccountSystem.Instance.IsSignedIn ? CurrentPlayerProfile : ProgressionManager.currentPlayerData` (`MainMenuController.GetActiveProfile`, `BattlePassSystem.GetActiveProfile`). Because `ServiceLocator` signs in anonymously, `AuthenticationService.IsSignedIn` can be true while `AccountSystem.IsSignedIn` is false, and vice versa.

## Save paths

1. **`SaveSystem` (static, `Progression System/SaveSystem.cs`)** — used by `ProgressionManager.Save/Load`. JSON via `JsonUtility` to `Application.persistentDataPath + "/Saves/"`. `SavePlayerData` also fires `SavePlayerDataToCloudAsync` (`async void`, line 83) which calls `CloudSaveService` and logs a warning on failure; `LoadPlayerDataWithCloudMergeAsync` (line 165) exists but `ProgressionManager` uses the synchronous local load (see account-progression.md). `JsonUtility` cannot serialise `Dictionary`/`DateTime`; `PlayerAccountData` stores timestamps as strings/longs (checked in account-progression.md).
2. **`SaveManager` + `SaveData` (`CloudSave/`)** — lazy singleton, PlayerPrefs key `gravity_wars_save_data`, auto-save coroutine every `autoSaveIntervalSeconds = 300`, cloud key `player_save_data_v2`. `SaveData` is a *parallel* data model: `BasicProfileData`, `CurrencyData` (+transactions), `ProgressionData`, `QuestSaveData`, `AchievementSaveData`, `PlayerStatistics`, `PlayerSettings`, `UnlockablesData`, `LeaderboardStatsData`, `AnalyticsQueueData`, `SaveMetadata`. Nothing in the game writes to it except `DebugSystemsUI` and `ServiceIntegrationHelper` (both unreferenced by scenes). `SaveManager.CollectSaveData` has TODOs "Implement EconomyService or use ProgressionManager directly" (lines 292, 387).
3. **`CloudSaveService` (`Networking/Services/CloudSaveService.cs`, 1225 lines)** — lazy singleton. Keys: `player_save_data_v2`, `save_metadata_v2`, `save_data_hash_v2`, `player_save_backup_v2`, `last_sync_timestamp_v2` (SaveData model) and `player_profile_data_v1`, `player_profile_hash_v1`, `player_profile_backup_v1` (PlayerAccountData model). Rate limit `MIN_SAVE_INTERVAL = 5 s`, offline queue max 50, `autoSyncInterval = 300 s`, `Update()` drains the queue. Conflict resolution `LoadWithConflictResolution`: strategies with `TakeNewest` default; `AskUser` "not implemented, defaulting to TakeNewest" (line 370); analytics queues merged. Hash check on load warns "data may have been tampered with" (line 268) — the hash is computed on the client, so it detects corruption, not cheating.
4. **`ServiceIntegrationHelper`** — glue that would copy `ProgressionManager` ↔ `SaveData`, quests, achievements, leaderboard stats; most bodies are TODO (lines 116, 131, 208, 252, 270, 281, 293, 301). Unreferenced.

## Analytics

- `AnalyticsService` (`Networking/Services/AnalyticsService.cs`): lazy singleton; `TrackMatchComplete(MatchAnalytics)`, `TrackEvent(name, params)` etc. log and (per lines 78-124) never call the SDK. `MatchAnalytics` struct lives in `GravityWars.Networking`.
- `GameManagerAnalytics` / `ProgressionManagerAnalytics` (`Assets/Analytics/`): MonoBehaviour wrappers meant to sit next to `GameManager`/`ProgressionManager`; not in any scene; their `TrackRoundEnd/TrackPlayerFire/TrackPerkActivation` and progression hooks are never called (rg) — see gameplay-physics.md dead-code list.
- No consent / opt-out UI, no privacy flag in `PlayerPreferences` (`SettingsUI` has none).

## Anti-cheat (all client-side, all unreferenced)

- `EconomyValidator`: XP/hour cap, "minimum hours to reach level 10/50" checks, violation records per player id.
- `RateLimiter`: per-player/per-action sliding window from a config dictionary; missing config → allow (line 99-100).
- `SuspiciousActivityDetector`: behaviour profile per player id (win rate, damage, abandonment); `IsPlayerSuspicious/IsPlayerFlagged/ClearFlag`.
- Documentation claims "server-side validation prevents cheating" (`Documentation/Game-Design/... GDD.md` § Daily Quest System); there is no server code, no Cloud Code, no Economy service usage. Everything that awards currency runs on the client (`GameManager.AwardMatchProgression`, `NetworkGameManager.AwardRewardsClientRpc`).

## Bugs / risks (SV)

| ID | Sev | Where | Problem | Suggested fix |
|---|---|---|---|---|
| SV1 | high | `CloudSave/SaveData.cs`, `CloudSave/SaveManager.cs` vs `Progression System/PlayerAccountData.cs` + `SaveSystem.cs` | Two complete save models. The game plays and saves through `PlayerAccountData`; `SaveData` duplicates currency, progression, quests, achievements, stats and is only touched by debug/glue code. Any future cloud sync of `SaveData` would overwrite nothing the game reads. | Decide for `PlayerAccountData` (it is what every screen uses). Delete `SaveManager`/`SaveData`/`ServiceIntegrationHelper` or reduce `SaveData` to a thin cloud envelope around `PlayerAccountData`. |
| SV2 | high | `ServiceLocator.Start` and `AccountSystem.Start` | Two independent `UnityServices.InitializeAsync()` + auth flows (anonymous vs username/password). Order of `Awake/Start` decides which identity the session has; `ServiceLocator.GetPlayerId()` may return the anonymous id while `AccountSystem` thinks the user is logged out. | One bootstrap object (`ServiceLocator`) that initialises UGS once and exposes the auth mode; `AccountSystem` becomes a client of it. Anonymous-first with optional account upgrade is the usual UGS pattern. |
| SV3 | medium | `AnalyticsService` lines 78, 96, 124 | SDK calls commented out → analytics silently disabled while the package is installed and wrappers exist. | Either wire `Unity.Services.Analytics.AnalyticsService.Instance.CustomData` + consent handling, or delete the analytics layer until needed. |
| SV4 | medium | `EconomyValidator`, `RateLimiter`, `SuspiciousActivityDetector` | Dead, client-side "anti-cheat" gives false confidence; docs promise server validation. | Remove until there is a server (Cloud Code / dedicated). Record in GDD that all validation is currently client-side. |
| SV5 | medium | `SaveSystem.SavePlayerDataToCloudAsync` (`async void`), `ServiceLocator.Start`, `AccountSystem.Start`, `MainMenuController.Start`, `SettingsUI.OnLogoutClicked` (`async void`) | Fire-and-forget async: exceptions are lost, ordering with scene loads undefined. | Return `Task` and await from a single bootstrap coroutine; keep `async void` only for Unity event handlers with try/catch. |
| SV6 | medium | `MatchHistoryManager` lines 402-443 | "TEMPORARY: Synchronous wait for async operation" (blocking `.Wait()`-style) and "fire and forget" comments — would freeze the main thread when enabled. | Make `RecordMatchResult` async; cache history locally. |
| SV7 | low | `CloudSaveService` hash keys | Client-computed hash cannot detect tampering; log message misleads. | Rename to "integrity check", or move hashing to Cloud Code. |
| SV8 | low | `DebugSystemsUI.OnAddCurrency` | Edits `SaveData.currency` directly; the running game reads `PlayerAccountData.credits`, so the debug button appears to do nothing. | Point the debug UI at `ProgressionManager`. |
| SV9 | low | `AccountSystem` lines 215-216 | Starter ship id and starting credits duplicated from `ProgressionManager.CreateNewPlayer` (see account-progression.md A-series). | Single `NewPlayerDefaultsSO`. |

## Verified statically / to check in editor
- Verified: file-level define guards (rg), package versions, key names, unreferenced classes (rg over project code excluding third-party folders).
- Verified: `ProjectSettings/ProjectSettings.asset` lines 709-713 carry a `cloudProjectId`, `projectName: Gravity Wars` and an `organizationId`, so the project is linked to a UGS project.
- Cannot verify: whether the linked UGS project has Cloud Save / Lobby / Relay / Analytics enabled, whether cloud save calls succeed, whether anonymous sign-in works (editor + dashboard task).
