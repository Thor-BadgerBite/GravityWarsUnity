# Ship progression (per-ship XP / level) (working notes)

Scope: `PlayerShip.RecalcLevelFromXP/XPNeededForNext/UpdateStatsFromLevel`, `PlayerAccountData.ShipProgressionEntry`,
`PlayerAccountData.AddShipXP/GetShipProgression/UnlockMasterySkin`, `ProgressionManager.AwardMatchXP`,
`GameManager.ResolveEquippedLoadout`, `MatchLoadoutBridge`, `UI/ShipMasteryBadgeUI.cs`, `TournamentMode`.

## What exists

- Ship level 1–20 (`PlayerShip.MAX_LEVEL`, `ShipProgressionEntry.AddXP` cap 20). XP formula `200 + 75·L²` in both places, but interpreted differently (see S1).
- Stats scale per level via `ShipLevelingFormulaSO` (or hard-coded fallback) in `PlayerShip.UpdateStatsFromLevel`.
- Gates: passive active at ship level ≥ 10 (`PlayerShip.isPassiveUnlocked`), perk tiers at 5/15/20 (`ActivePerkSO.minLevel`, checked in `PerkManager.ToggleSlot`).
- XP source: `ProgressionManager.AwardMatchXP` gives the used **custom loadout** the same XP as the account (`totalShipXP = totalAccountXP`). Prebuilt `ShipPresetSO` ships have no `ShipProgressionEntry` and never gain XP; in a match they use the prefab's `shipXP` (189050 → level 20).
- Key: `CustomShipLoadout.GetProgressionKey()` = body|T1|T2|T3|move|passives (missile excluded) → changing perks or passive **resets** ship XP (new entry), changing the missile does not.
- Mastery: titles Veteran/Ace/Master/Legend at 5/10/15/20 (`ShipProgressionEntry.GetMasteryTitle`), skin ids `skin_mastery_gold_{body}` / `skin_mastery_legend_{body}` unlocked at 10/20 (string ids only, no skin assets). `ShipMasteryBadgeUI` displays the title (not in any scene).
- Tournament mode: `TournamentMode.GetEffectiveLevel` normalizes to level 10 when `Enabled` — never enabled anywhere.

## Bugs / conflicts (S = ship progression)

| ID | Sev | Where | Problem | Fix |
|---|---|---|---|---|
| S1 | high | `PlayerShip.RecalcLevelFromXP` vs `ShipProgressionEntry.AddXP/GetXPRequiredForLevel` | `PlayerShip` treats `200+75·L²` as the XP needed to go from L to L+1 and subtracts it from a pool; `ShipProgressionEntry` treats it as the cumulative total required for level L. Same `shipXP` gives different levels (e.g. 6250 XP → level 6 in-match, level 8 in the garage). | One static `ShipXPTable` used by both. |
| S2 | high | `GameManager.ResolveEquippedLoadout`, `MatchLoadoutBridge.ApplyPreset` | Prebuilt ships never progress and always fight at the prefab's level 20; only custom loadouts have XP. The Brawl-Stars "per-ship progression" therefore only exists for custom ships. | Give every ship (preset or custom) a progression entry keyed by preset name; set `ship.shipXP` in `ApplyPreset`. |
| S3 | medium | `GetProgressionKey` | Editing a loadout's perks/passive silently starts a new XP entry (old entry orphaned in `shipProgressionData`). Design intent unclear (docs say "changing missile never resets XP", silent on perks). | Decide: key by `loadoutID` (XP survives edits) or keep. Open question. |
| S4 | medium | `PlayerShip.shipXP` default 6250 / prefab 189050 | Test values baked into script/prefab. | 0 in prefab; bridge sets XP. |
| S5 | low | `ShipProgressionEntry.matchesPlayed/matchesWon/roundsWon/totalDamage/totalKills` | Never incremented anywhere (only `AddXP` runs). `ProgressionUI` displays them. | Update in `AwardMatchXP`. |
| S6 | low | `UnlockMasterySkin` | Skin ids have no backing assets (cosmetics system has none). | Cosmetics milestone. |

## Tunables
`200 + 75·L²` (two interpretations), max level 20, passive unlock 10, perk tiers 5/15/20, mastery 5/10/15/20, tournament reference level 10.
