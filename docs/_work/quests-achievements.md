# Quests & achievements (working notes)

Scope: `Assets/Quests/*` (QuestDataSO, GameManagerQuestIntegration, ProgressionManagerQuestIntegration, UI/QuestUI, Editor/QuestTemplateGenerator),
`Assets/Networking/Services/QuestService.cs`, `Assets/Achievements/*` (AchievementDataSO, AchievementService, two integrations, UI/AchievementUI,
Editor/AchievementTemplateGenerator), `Resources/Quests/Templates/*` (24), `Resources/Achievements/Templates/*` (43).

## Quests — status: PARTIAL (runs in memory, no persistence, half the objectives never fed)

- `QuestDataSO` (`GravityWars/Quest`): id, type Daily/Weekly/Season, `QuestObjectiveType` (16 kinds), `targetValue`, `requiredAccountLevel`, `requiredArchetype` (AllAround = "no restriction", so an AllAround-only quest is impossible), `requiredMissileType`, rewards (credits, gems, account XP, item ids), difficulty. `CreateInstance()` → `QuestInstance` (expiry 24 h / 7 d / 90 d from assignment; `DateTime` fields).
- `QuestService` (lazy singleton, DontDestroyOnLoad): slots 3 daily / 3 weekly / 5 season; templates auto-loaded from `Resources/Quests/Templates`; `InitializeQuests()` called from `ProgressionManager.Initialize` and `GameManagerQuestIntegration.Start`; expired quests refreshed every 3600 frames (`Time.frameCount % 3600`, i.e. once a minute at 60 fps, less often at other frame rates); `UpdateQuestProgress(type, amount, context)`; completion auto-awards credits/gems/XP through `ProgressionManager.currentPlayerData` and calls `CheckAccountLevelUp`; item rewards are only logged; `ClaimQuest` exists (manual path, used by `QuestUI`/`DebugSystemsUI`) → **double reward**: `OnQuestCompleted` already awarded, `ClaimQuest` awards again (Q1).
- **Persistence: none.** `LoadQuestsFromCloud` always creates an empty list; `SaveQuestsToCloud` only logs. `PlayerAccountData.activeQuests/completedQuests` exist but are never written. Every app start rolls a fresh set of quests (Q2).
- Progress feeds (only if `GameManagerQuestIntegration` is attached to the GameManager object — it is not in any scene): `OnMatchStart` (PlayMatches, PlayMatchesWithArchetype using P1's archetype), `OnMatchEnd` (WinMatches, WinWithArchetype, DealDamage, HitMissiles — the last two from counters that only `OnPlayerFireMissile` increments, and **nothing calls `OnPlayerFireMissile`**), `OnRoundEnd` (WinRounds), `OnPlayerActivatePerk` (from `PerkManager`). `OnShipDestroyedWithMissile` has no caller. `ProgressionManagerQuestIntegration` (ReachAccountLevel, EarnCurrency, UnlockItem, LevelUpShip) has **no callers at all** and requires a `ProgressionManager` component on the same object (the lazy singleton object has none).
  → Objectives that can actually progress today: PlayMatches, WinMatches, WinRounds, UsePerkNTimes, PlayMatchesWithArchetype, WinWithArchetype (with `context.Contains(archetype)` matching). Never: DealDamage, FireMissiles, HitMissiles, DestroyShipsWithMissileType, WinWithPerk, ReachWinStreak, UnlockItem, ReachAccountLevel, EarnCurrency, LevelUpShip. 13 of the 24 templates are therefore uncompletable (Q3).
- `QuestUI` (813 lines, not in any scene): tabs, cards, claim buttons; reads `QuestService`. `QuestTemplateGenerator` (editor) writes the 24 templates.

### Quest templates (Resources/Quests/Templates)

| id | type | objective | target | credits | gems | XP | difficulty |
|---|---|---|---|---|---|---|---|
| Daily_DealDamage_Easy | Daily | DealDamage | 1000 | 100 | 0 | 50 | Easy |
| Daily_DealDamage_Hard | Daily | DealDamage | 2500 | 250 | 5 | 125 | Hard |
| Daily_FireMissiles | Daily | FireMissiles | 20 | 80 | 0 | 40 | Easy |
| Daily_HitMissiles | Daily | HitMissiles | 15 | 150 | 0 | 75 | Medium |
| Daily_PlayMatches | Daily | PlayMatches | 5 | 75 | 0 | 40 | Easy |
| Daily_PlaySniper | Daily | PlayMatchesWithArchetype (DamageDealer) | 3 | 120 | 0 | 60 | Easy |
| Daily_PlayTank | Daily | PlayMatchesWithArchetype (Tank) | 3 | 120 | 0 | 60 | Easy |
| Daily_UsePerk | Daily | UsePerkNTimes | 10 | 100 | 0 | 50 | Easy |
| Daily_WinMatches_Easy | Daily | WinMatches | 3 | 100 | 0 | 50 | Easy |
| Daily_WinMatches_Medium | Daily | WinMatches | 5 | 200 | 0 | 100 | Medium |
| Daily_WinRounds | Daily | WinRounds | 10 | 150 | 0 | 75 | Medium |
| Weekly_DealDamage | Weekly | DealDamage | 10000 | 750 | 20 | 400 | Hard |
| Weekly_HitMissiles | Weekly | HitMissiles | 100 | 700 | 15 | 350 | Hard |
| Weekly_WinMatches | Weekly | WinMatches | 20 | 500 | 10 | 250 | Medium |
| Weekly_WinRounds | Weekly | WinRounds | 50 | 600 | 15 | 300 | Hard |
| Weekly_WinStreak | Weekly | ReachWinStreak | 5 | 1000 | 30 | 500 | VeryHard |
| Weekly_WinWithSniper | Weekly | WinWithArchetype (DamageDealer) | 10 | 400 | 10 | 200 | Medium |
| Weekly_WinWithTank | Weekly | WinWithArchetype (Tank) | 10 | 400 | 10 | 200 | Medium |
| Season_DealDamage | Season | DealDamage | 50000 | 3000 | 150 | 1500 | VeryHard |
| Season_EarnCurrency | Season | EarnCurrency | 10000 | 2000 | 75 | 750 | Hard |
| Season_MasterAllShips | Season | WinWithArchetype (all) | 4 | 1500 | 50 | 600 | Hard |
| Season_ReachLevel20 | Season | ReachAccountLevel | 20 | 1500 | 50 | 0 | Hard |
| Season_ReachLevel50 | Season | ReachAccountLevel | 50 | 5000 | 250 | 0 | VeryHard |
| Season_WinMatches_100 | Season | WinMatches | 100 | 2500 | 100 | 1000 | VeryHard |
| Season_WinMatches_50 | Season | WinMatches | 50 | 1000 | 25 | 500 | Medium |

"Sniper" in the template names maps to `ShipArchetype.DamageDealer` (index 1) — leftover of an older archetype naming (Tank/Sniper/AllAround/Glass also survives in `LeaderboardShipFilter`). CONFLICT in naming.

## Achievements — status: PARTIAL (tracks in memory, rewards are stubs, no persistence)

- `AchievementDataSO` (`GravityWars/Achievement`): id, type Single/Incremental/Tiered, category, secret flag, tier, `AchievementConditionType` (36 kinds), target, `requiredContext`, rewards (credits, gems, XP, exclusive item, title, profile icon), points, platform ids (Steam/PS/Xbox).
- `AchievementService` (lazy singleton; `Start()` auto-initialises; templates from `Resources/Achievements/Templates`): `UpdateAchievementProgress` / `SetAchievementProgress` by condition type + context; `OnAchievementUnlocked` → `AwardAchievementRewards` — **every reward line is commented out** (currency, XP, items, titles are only logged) (AC1); platform sync stubs; analytics hook; `SaveAchievementsToCloud` serialises but never sends; `LoadAchievementsFromCloud` returns false → progress resets every session (AC2). `AchievementSaveData` holds a `Dictionary<string,int>` (`JsonUtility` cannot serialise it) (AC3). `AchievementInstance.IsClaimable => isUnlocked && !isUnlocked` is always false (AC4).
- Feeds (only if `GameManagerAchievementIntegration` is attached — it is not): `OnMatchEnd` (PlayMatches, WinMatches, WinWithArchetype with `_currentPlayerArchetype` never set → always "AllAround", WinOnAllMaps with empty map, WinMatchesInRow from a private counter, WinIn60Seconds, WinWithoutTakingDamage — `_hasP1TakenDamage` only set by the never-called `OnPlayerTakeDamage`, so **every win is flawless**; WinWithPerfectAccuracy/AchieveAccuracy need `OnPlayerFireMissile` which nobody calls; DealDamage/DealDamageInOneMatch/DealDamageWithSingleShot/FireMissiles/HitMissiles from the same dead counters; WinRounds). `OnPlayerActivatePerk` works (PerkManager). `ProgressionManagerAchievementIntegration` (ReachAccountLevel, EarnTotalCurrency, SpendTotalCurrency, UnlockAll*, ReachBattlePassMaxTier, CompleteDaily/WeeklyQuest, WinWithAllArchetypes) has no callers, and its "all items" thresholds are hard-coded guesses (4 ships, 10 missiles, 15 perks, 50 cosmetics).
- `AchievementUI` (812 lines) and `AchievementTemplateGenerator` (editor) exist; UI not in any scene.

### Achievement templates (43; credits/gems/XP/points)
Combat: FirstBlood (1 win; 100/0/50/10), WinMatches Bronze 10 (250/0/100/15), Silver 50 (500/10/250/30), Gold 100 (1000/25/500/50), Platinum 500 (5000/100/2000/100); WinRounds Bronze 50 (200/0/100/15), Silver 250 (500/10/250/30); DealDamage Bronze 10k (200/0/100/15), Silver 50k (500/10/250/30), Gold 100k (1000/25/500/50); HitMissiles Bronze 100 (200/0/100/15), Silver 500 (500/10/250/30), Gold 1000 (1000/25/500/50).
Progression: ReachLevel 10/25/50/100 (250/0/0/10, 500/10/0/25, 1000/25/0/50, 5000/100/0/100 — level 100 unreachable with cap 50); EarnCurrency 10k/50k/100k (500/0/100/20, 1500/25/250/40, 3000/50/500/60).
Collection: UnlockAllShips (1000/20/250/40), UnlockAllMissiles (1500/30/300/50), UnlockAllPerks (1500/30/300/50), UnlockAllCosmetics (2000/50/400/75), UnlockAllItems (10000/200/1000/150).
Skill: FlawlessVictory (500/10/200/30), PerfectAccuracy (500/10/200/30), QuickVictory ≤ 60 s (400/8/150/25), HighDamageSingleShot 500 (300/0/100/20 — trivially met, base payload is 2500), WinStreak 3/5/10 (300/0/100/20, 750/15/250/40, 2000/50/500/75), WinWithAllArchetypes (1000/20/300/50).
Social: PlayMatches Bronze 25 (250/0/100/15), Silver 100 (500/10/250/30), PlayWithFriend (200/0/50/10), WinAgainstFriend (300/0/75/15).
Secret: MapMaster (1500/30/400/55), MassiveDestruction 1000 dmg in one match (1000/25/300/50), PerkAddict 100 perks (800/15/250/40), QuestGrinder 50 daily quests (1500/30/400/50), SniperMaster / TankMaster 100 wins with archetype (2000/40/500/60 each).

## Bugs / risks (Q = quests, AC = achievements)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| Q1 | medium | `QuestService.OnQuestCompleted` + `ClaimQuest` | Rewards granted on completion **and** on claim. | Pick one (recommend claim). |
| Q2 | high | `QuestService.LoadQuestsFromCloud/SaveQuestsToCloud` | No persistence; quests regenerate every launch, progress lost. | Serialize `_activeQuests` into `PlayerAccountData.activeQuests` (timestamps as longs). |
| Q3 | high | `GameManager` ↔ `GameManagerQuestIntegration` | `OnPlayerFireMissile`/`OnShipDestroyedWithMissile` never called; 13/24 templates cannot progress; integration component absent from scenes. | Drive integrations from `MatchStatsTracker` events (already has fired/hit/damage) and attach or auto-create the component. |
| Q4 | low | `QuestService.Update` | Frame-count modulo timer. | Use time. |
| Q5 | low | `QuestDataSO.requiredArchetype` | AllAround doubles as "any". | Nullable/enum with `Any`. |
| AC1 | high | `AchievementService.AwardAchievementRewards` | All rewards commented out. | Route through `ProgressionManager` like `QuestService` does. |
| AC2 | high | `AchievementService` | No local/cloud persistence; resets each session. | Persist `AchievementInstance` list in `PlayerAccountData` (it already has `unlockedAchievements`). |
| AC3 | low | `AchievementSaveData.lifetimeStats` | Dictionary not serialisable with JsonUtility. | Parallel lists. |
| AC4 | low | `AchievementInstance.IsClaimable` | Always false. | Fix or delete. |
| AC5 | medium | `GameManagerAchievementIntegration` | Flawless/accuracy/damage/archetype/map contexts never populated → wrong unlocks (every win = flawless). | Feed from `MatchStatsTracker`; set archetype from `GameManager.player1Ship`. |
| AC6 | low | `ProgressionManagerAchievementIntegration.CheckAll*` | Hard-coded content counts. | Count from `ProgressionManager.all*`. |
| AC7 | low | `[RequireComponent(typeof(ProgressionManager))]` on both ProgressionManager integrations | Can never be attached to the runtime-created singleton object. | Auto-add in `ProgressionManager.Awake` or drop the attribute. |

## Design facts for the GDD
Quest slots 3/3/5, expiry 24 h/7 d/90 d; quest and achievement reward tables above; achievement points; quest/achievement feature unlock levels 5/3 (`ProgressionSystem`) not enforced.
