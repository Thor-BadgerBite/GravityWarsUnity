# Leaderboards (working notes)

Scope: `Assets/Leaderboards/LeaderboardData.cs`, `LeaderboardService.cs`, `GameManagerLeaderboardIntegration.cs`, `UI/LeaderboardUI.cs` (900 lines, not read line by line — scene-less UI, see ui-screens.md), `Editor/LeaderboardConfigGenerator.cs`.

## Status: DESIGNED - NOT BUILT (mock data only)

- Data model: `LeaderboardScope` (Global/Friends/Regional), `LeaderboardStatType` (18 stats), `LeaderboardTimeFrame` (AllTime…Daily), `LeaderboardShipFilter` (**All/Tank/Sniper/AllAround/Glass** — old archetype names, CONFLICT with `ShipArchetype`), `LeaderboardEntry`, `LeaderboardDefinition` (id, scope, stat, timeframe, filter, format, paging, reset), `LeaderboardData`, `PlayerLeaderboardStats`.
- `LeaderboardService` (lazy singleton): `leaderboardDefinitions` list is empty on the runtime-created object (definitions come only from the editor generator into a serialized field nobody assigns) → `SubmitScore` loops over zero definitions and returns false; `SubmitToLeaderboard` and `FetchLeaderboardFromServer` are TODO stubs — fetch returns **`GenerateMockLeaderboardData`** (fake players, self = rank 50). Player id/name hard-coded `"player_12345"` / `"TestPlayer"`. Rate limit 10 submissions/min; `ValidateScore` rejects `HighestDamageInMatch > 10000` (real matches deal 15 000+ per kill → every real value rejected, L2); the `com.unity.services.leaderboards` package is installed but unused.
- `GameManagerLeaderboardIntegration` (not in any scene): keeps its **own** lifetime stats in `PlayerPrefs` (`Leaderboard_*` keys) — a third copy of wins/matches/streak next to `PlayerAccountData` and `MatchStatsTracker`; `OnPlayerFireMissile` never called → damage/accuracy always 0; `GetTotalMissilesHit` returns 0 (TODO).
- `LeaderboardUI`: tabs/filters/pagination against the mock service.

## Bugs / risks (L)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| L1 | high (for online) | `LeaderboardService.SubmitToLeaderboard/FetchLeaderboardFromServer` | UGS calls are TODO; mock data shown as real. | Implement with `Unity.Services.Leaderboards` or hide the screen until then. |
| L2 | medium | `LeaderboardService.ValidateScore` | Damage cap 10 000 below real per-match damage. | Derive caps from balance config. |
| L3 | medium | `GameManagerLeaderboardIntegration.SaveStats/LoadStats` | Separate PlayerPrefs stat store diverges from `PlayerAccountData`. | Read from `PlayerAccountData`. |
| L4 | low | `LeaderboardShipFilter` | Stale archetype names. | Use `ShipArchetype`. |
| L5 | low | `LeaderboardService.GetPlayerID/GetPlayerName` | Hard-coded ids. | Use `ServiceLocator.GetPlayerId()` / profile. |
