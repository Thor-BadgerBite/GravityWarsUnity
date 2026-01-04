# Gravity Wars - Player Rank System

## Overview

The Gravity Wars competitive ranking system uses **military/naval-themed ranks** based on ELO ratings. Players progress through **16 distinct rank tiers**, from **Cadet** to **Grand Admiral**, earning prestige and demonstrating their mastery of gravitational warfare.

**Starting Rank**: ⭐ Ensign (800 ELO) - All new players begin their journey here!

---

## Rank Progression Chart (16 Ranks Total)

| # | Rank | ELO Range | Abbreviation | Stars | Color | Description |
|---|------|-----------|--------------|-------|-------|-------------|
| 16 | **Cadet** | 0-699 | CDT | ☆ | Dark Bronze | Training Officer - Begin your journey |
| 15 | **⭐ Ensign** | 700-1049 | ENS | ★ | Bronze | **STARTING RANK** - Proving your worth |
| 14 | **Lieutenant** | 1050-1199 | LT | ★ | Light Silver | Commissioned Officer - Skilled tactician |
| 13 | **Lieutenant Commander** | 1200-1349 | LCDR | ★★ | Bright Silver | Senior Officer - Exceptional prowess |
| 12 | **Commander** | 1350-1499 | CDR | ★★★ | Gold | Command Officer - Strategic excellence |
| 11 | **Captain** | 1500-1649 | CAPT | ★★★ | Bright Gold | Ship Captain - Master of warfare |
| 10 | **Senior Captain** | 1650-1799 | SCPT | ★★★ | Sky Blue | Distinguished Captain - Veteran commander |
| 9 | **Commodore** | 1800-1949 | CDRE | ★★★★ | Light Blue | Fleet Officer - Commands respect |
| 8 | **Rear Admiral** | 1950-2099 | RADM | ★★★★ | Blue | Lower Admiral - Elite tactician |
| 7 | **Rear Admiral (Upper Half)** | 2100-2249 | RADM(UH) | ★★★★ | Deep Blue | Senior Rear Admiral - Elite command |
| 6 | **Vice Admiral** | 2250-2399 | VADM | ★★★★ | Purple | High Admiral - Galaxy's finest |
| 5 | **Admiral** | 2400-2549 | ADM | ★★★★★ | Bright Purple | Admiral - Supreme authority |
| 4 | **High Admiral** | 2550-2699 | HADM | ★★★★★ | Brilliant Purple | Distinguished Admiral - Renowned |
| 3 | **Fleet Admiral** | 2700-2849 | FADM | ★★★★★ | Red | Supreme Commander - Legendary |
| 2 | **Supreme Admiral** | 2850-2999 | SADM | ★★★★★ | Bright Red | Elite Commander - Greatest legends |
| 1 | **Grand Admiral** | 3000+ | GADM | ★★★★★ | Crimson | **HIGHEST RANK** - Pinnacle of excellence |

---

## Starting Rank

**All new players start at 800 ELO**, which places them in the **⭐ Ensign** rank tier (Rank 15). This provides:
- A balanced starting point for matchmaking
- Room to climb or fall based on performance (can drop to Cadet or climb to Grand Admiral)
- Clear progression path with 14 ranks above to achieve
- Starting rank is marked with ⭐ to indicate it's the entry point for new players

---

## Rank Colors

Ranks use a color progression that reflects increasing prestige:

1. **Bronze Tier** (Cadet, Ensign) - Training/Starting ranks
2. **Silver Tier** (Lieutenant, Lieutenant Commander) - Intermediate ranks
3. **Gold Tier** (Commander, Captain, Senior Captain) - Advanced ranks
4. **Blue Tier** (Commodore, Rear Admiral, Rear Admiral Upper Half) - Expert ranks
5. **Purple Tier** (Vice Admiral, Admiral, High Admiral) - Master ranks
6. **Red Tier** (Fleet Admiral, Supreme Admiral, Grand Admiral) - Legendary ranks

### RGB Color Values

```
Cadet:                     RGB(0.5, 0.35, 0.15) - Dark Bronze
⭐ Ensign (Starting):      RGB(0.8, 0.5, 0.2)   - Bronze
Lieutenant:                RGB(0.7, 0.7, 0.7)   - Light Silver
Lieutenant Commander:      RGB(0.9, 0.9, 0.95)  - Bright Silver
Commander:                 RGB(1.0, 0.84, 0.0)  - Gold
Captain:                   RGB(1.0, 0.92, 0.3)  - Bright Gold
Senior Captain:            RGB(0.5, 0.8, 1.0)   - Sky Blue
Commodore:                 RGB(0.7, 0.9, 1.0)   - Light Blue
Rear Admiral:              RGB(0.4, 0.7, 1.0)   - Blue
Rear Admiral Upper Half:   RGB(0.3, 0.5, 0.9)   - Deep Blue
Vice Admiral:              RGB(0.6, 0.2, 0.8)   - Purple
Admiral:                   RGB(0.8, 0.3, 0.9)   - Bright Purple
High Admiral:              RGB(0.9, 0.4, 1.0)   - Brilliant Purple
Fleet Admiral:             RGB(1.0, 0.3, 0.3)   - Red
Supreme Admiral:           RGB(1.0, 0.25, 0.25) - Bright Red
Grand Admiral:             RGB(1.0, 0.2, 0.2)   - Crimson Red
```

---

## Rank Insignia (Stars)

Each rank displays a number of stars to indicate progression:
- **0 Stars**: Cadet (training only)
- **1 Star**: Ensign (⭐ starting rank), Lieutenant
- **2 Stars**: Lieutenant Commander
- **3 Stars**: Commander, Captain, Senior Captain
- **4 Stars**: Commodore, Rear Admiral, Rear Admiral Upper Half, Vice Admiral
- **5 Stars**: Admiral, High Admiral, Fleet Admiral, Supreme Admiral, Grand Admiral

---

## ELO System Details

### Starting ELO
- **Default**: 800 ELO (⭐ Ensign rank - the starting rank for all new players)

### ELO Boundaries
- **Minimum**: 100 ELO (cannot drop below)
- **Maximum**: 4000 ELO (theoretical cap)
- **Grand Admiral Threshold**: 3000 ELO (highest rank)

### K-Factor (Rating Volatility)
The K-factor determines how much ELO changes per match:

- **Beginners** (<10 matches): K=40 (volatile, fast progression)
- **Intermediate** (10-50 matches): K=32 (moderate changes)
- **Veterans** (50+ matches): K=24 (stable ratings)
- **Masters** (1800+ ELO): K=16 (very stable, prevents rating inflation)

### Rank Distribution Goals
Approximate target distribution for a healthy player base:

```
Cadet:                     5%  (Below starting rank)
⭐ Ensign:                 20% (Starting rank - largest group)
Lieutenant:                15%
Lieutenant Commander:      12%
Commander:                 10%
Captain:                   8%
Senior Captain:            7%
Commodore:                 6%
Rear Admiral:              5%
Rear Admiral Upper Half:   4%
Vice Admiral:              3%
Admiral:                   2%
High Admiral:              1.5%
Fleet Admiral:             1%
Supreme Admiral:           0.4%
Grand Admiral:             0.1% (Top 0.1% of players)
```

---

## Rank Progression Examples

### From Ensign to Lieutenant
- Current: 800 ELO (⭐ Ensign - Starting)
- Target: 1050 ELO (Lieutenant)
- **Need**: +250 ELO (~10-14 wins against equal opponents)

### From Captain to Senior Captain
- Current: 1500 ELO (Captain)
- Target: 1650 ELO (Senior Captain)
- **Need**: +150 ELO (~6-10 wins against equal opponents)

### From Rear Admiral to Rear Admiral Upper Half
- Current: 1950 ELO (Rear Admiral)
- Target: 2100 ELO (Rear Admiral Upper Half)
- **Need**: +150 ELO (~9-13 wins, K-factor is lower at this level)

### From Supreme Admiral to Grand Admiral
- Current: 2850 ELO (Supreme Admiral)
- Target: 3000 ELO (Grand Admiral - Highest Rank!)
- **Need**: +150 ELO (~12-18 wins, very challenging at this level)

---

## Matchmaking Considerations

### Fair Match Definition
Two players are considered a "fair match" if their ELO difference is within **150 points**.

### Win Probability Examples
- **Equal ELO (1500 vs 1500)**: 50% win chance
- **100 ELO difference (1500 vs 1600)**: 36% vs 64% win chance
- **200 ELO difference (1500 vs 1700)**: 24% vs 76% win chance
- **400 ELO difference (1500 vs 1900)**: 9% vs 91% win chance

---

## UI Integration

### Display Formats

**Simple**: "Commander"
**With ELO**: "Commander (1250 ELO)"
**Full**: "Commander (1200-1399 ELO)"
**Colored**: `<color=#FFD700>Commander</color>`
**Abbreviated**: "CDR"

### Progress Indicators
- Show progress bar through current rank
- Display ELO needed for next rank
- Highlight rank-up achievements

### Rank Badges
The rank badge icon should:
- Display the appropriate color for the rank
- Show the number of stars (insignia)
- Include the rank abbreviation
- Have a distinctive border/frame that matches the rank tier

---

## Code Usage Examples

### Get Player Rank
```csharp
int playerELO = 1450;
CompetitiveRank rank = ELORatingSystem.GetRankFromELO(playerELO);
// Returns: CompetitiveRank.Captain
```

### Get Rank Display Name
```csharp
string displayName = ELORatingSystem.GetRankDisplayName(CompetitiveRank.RearAdmiral);
// Returns: "Rear Admiral"
```

### Get Rank Data
```csharp
var rankData = RankConfiguration.GetRankData(CompetitiveRank.Admiral);
// Access: rankData.displayName, rankData.description, rankData.color, etc.
```

### Get Colored Rank Text
```csharp
string coloredText = RankConfiguration.GetRankColoredText(CompetitiveRank.FleetAdmiral);
// Returns: "<color=#FF4D4D>Fleet Admiral</color>"
```

### Calculate Progress to Next Rank
```csharp
int currentELO = 1650;
int eloNeeded = RankConfiguration.GetELOToNextRank(currentELO);
// Returns: 150 (need 1800 ELO for Rear Admiral)

float progress = RankConfiguration.GetRankProgress(currentELO);
// Returns: 0.25 (25% through Commodore rank)
```

---

## Achievement Ideas

Consider creating achievements for:
- Reaching each major rank tier (Commander, Captain, Admiral, etc.)
- Promoting ranks in a single session
- Maintaining a rank for X matches
- Reaching Grand Admiral (ultimate achievement)
- Fastest climb from Cadet to Commander
- Win streaks at different rank tiers

---

## Future Enhancements

### Seasonal Ranks
- Reset ranks periodically (e.g., every 3 months)
- Award special badges for peak seasonal rank
- Seasonal leaderboards

### Rank Decay
- Prevent inactive high-rank players from camping
- Small ELO loss after 30 days of inactivity (only above 1600 ELO)
- Encourages active participation

### Division System
- Split each rank into 3 divisions (I, II, III)
- Example: "Captain II" or "Admiral I"
- Provides more granular progression

### Special Titles
- Top 100 players: "Elite" prefix
- Top 10 players: "Legendary" prefix
- #1 player: "Supreme" prefix

---

## Design Philosophy

The rank system is designed to:

1. **Inspire**: Military ranks evoke authority and achievement
2. **Motivate**: Clear progression path with visible goals
3. **Reward**: Each rank feels meaningful and earned
4. **Differentiate**: Wide range of tiers prevents clustering
5. **Balance**: Starting at Commander prevents new player stigma
6. **Persist**: ELO-based system rewards skill over time played

---

**Remember**: Ranks are a reflection of skill, not time invested. Every match is an opportunity to prove your worth and climb the ranks!

🎖️ **Good luck on your journey to Grand Admiral!** 🎖️
