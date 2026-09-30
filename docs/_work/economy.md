# Economy, currencies & cosmetics (working notes)

Scope: currency fields in `PlayerAccountData`, `ProgressionManager` currency API, `BattlePassSystem` rewards,
`RankedSeasonSystem`, `MatchHistoryManager` (netcode), `ProgressionSystem.GetLevelUpReward`, `Progression System/CosmeticsSystem.cs`,
`Networking/Services/EconomyValidator.cs` (read in services.md), `Unity Economy` package (in manifest, no code uses it).

## Currencies
- `credits` (soft) and `gems` (hard) on `PlayerAccountData`. Aliases in old docs: coins / softCurrency, hardCurrency.
- Older `RewardType.Credits/Gems` + `UnlockableReward.softCurrencyAmount/hardCurrencyAmount` in `BattlePassData.cs` (dead SO path).

## Sources (as coded)

| Source | Amount | Path | Status |
|---|---|---|---|
| New account | 1000 credits + 50 gems (`ProgressionManager.GrantStarterContent`) / 1000 + 0 (`AccountSystem.CreateNewPlayerProfile`) | local / online | CONFLICT |
| Win streak milestones | 50 / 100 / 200 credits at 3 / 5 / 10 wins (`ProgressionManager.AwardMatchXP`) | local | IMPLEMENTED (but streak is reset every match by bug G1) |
| Battle pass free track | credits 500…5000 at 11 tiers, 50 gems at tier 23 (`BattlePassSystem.FREE_TRACK_REWARDS`) | both | IMPLEMENTED |
| Battle pass premium track | 1000 credits at 1; gems 25/30/35/40/45/50/60/75/100 (total 460) | both | IMPLEMENTED |
| Ranked season peak-rank reward | 20…500 gems (`RankedSeasonSystem.GrantPeakRankRewards`) | both | IMPLEMENTED (rollover trigger only on `currentSeason` constant change) |
| Match credits | 25/50 base ×2 win + damage/20 + accuracy bonuses (`MatchHistoryManager.CalculateRewards`) | netcode only | DESIGNED |
| Level-up rewards | credits 100+50·L (+500 at ×10), gems 10·(L/5) at ×5 (`ProgressionSystem.GetLevelUpReward`) | netcode only | DESIGNED |
| Achievements/quests | see quests-achievements.md (services grant rewards?) | – | to verify |

## Sinks
- Premium battle pass: 1000 gems (`BattlePassSystem.PurchasePremiumPass(gemCost = 1000)`, `BattlePassUI` hard-codes "1000 Gems").
- Nothing else. `ProgressionManager.SpendCurrency` has no callers besides the removed premium purchase. **Credits have no sink at all** (no shop, no unlock purchases). [CONFLICT - needs decision]

## Monetization rules found in docs/code
- IMPLEMENTATION_STATUS: premium content power-neutral; exclusive perks are sidegrades; cosmetics-only monetization; F2P must finish pass.
- README: "Premium content = cosmetics + XP boosts, NOT power"; "Battle Pass earns premium currency back".
- Code: premium pass grants +50 % account/ship XP (`ProgressionManager.AwardMatchXP`) and 460 gems back over the track (pass costs 1000 → not self-funding). Premium ships/perks are stat-budget-neutral per generator comments.
- No IAP / store code exists anywhere (`UnityPlayerAccountSettings.asset` and `BillingMode.json` in Resources are package artifacts).

## F2P battle pass feasibility (static estimate)
Pass = 25 levels × 1000 XP = 25 000 BP XP (`BattlePassSystem.maxLevel/xpPerLevel`). Local BP XP per match ≈ account XP (50 + 100 win + 25·rounds + damage/100 ≈ 200–350 for a win, ~100–200 for a loss), first win of the day ×2. ≈ 80–150 matches per season. Season length is undefined in code (`seasonEndTimestamp` never set, rollover only when `currentSeason` changes) → cannot judge "within a season" until a season length is decided. [Open question]

## Cosmetics (`CosmeticsSystem.cs`)
- SO types `ShipSkinSO`, `ColorSchemeSO`, `DecalSO` and a `CosmeticsApplier` MonoBehaviour (instantiates skin prefab, tints materials by renderer name, sets decal texture). No assets of any of these types exist; `CosmeticsApplier` has no callers and is in no scene; `CustomShipLoadout.skinID/colorSchemeID/decalID` are never set by any UI. `GetUnlockedColorSchemes` checks `unlockedSkins` instead of `unlockedColorSchemeIDs` (bug E3). Battle pass and ranked/mastery rewards add skin **ids** to `unlockedSkinIDs` with nothing to display → DESIGNED - NOT BUILT.

## Bugs / risks (E = economy)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| E1 | high | design | No credit sink; premium pass is the only gem sink; no store. | Decide the shop/cosmetic economy (Open Question). |
| E2 | medium | `BattlePassUI.OnPurchasePremium` / `BattlePassSystem.PurchasePremiumPass` | Price hard-coded in two places. | One config value. |
| E3 | low | `CosmeticsApplier.GetUnlockedColorSchemes` | Reads `unlockedSkins` instead of `unlockedColorSchemeIDs`. | Fix list. |
| E4 | low | `ColorSchemeSO.ApplyToShip`, `CosmeticsApplier.ApplyDecal` | Create new `Material` per renderer per apply, never released. | Use `MaterialPropertyBlock`. |
| E5 | medium | `ProgressionManager.SpendCurrency`, `EconomyValidator` | Server-side validation exists in name only (client-authoritative save). | Out of scope until online. |

## Tunables
All amounts above.
