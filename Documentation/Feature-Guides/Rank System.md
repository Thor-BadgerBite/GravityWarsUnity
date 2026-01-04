# Gravity Wars - Player Rank System

## Overview

The Gravity Wars competitive ranking system uses **military/naval-themed ranks** based on ELO ratings. Players progress through 12 distinct rank tiers, from **Cadet** to **Grand Admiral**, earning prestige and demonstrating their mastery of gravitational warfare.

---

## Rank Progression Chart

| Rank | ELO Range | Abbreviation | Stars | Color | Description |
|------|-----------|--------------|-------|-------|-------------|
| **Cadet** | 0-599 | CDT | ☆ | Dark Bronze | Training Officer - Begin your journey |
| **Ensign** | 600-799 | ENS | ★ | Bronze | Junior Officer - Proving your worth |
| **Lieutenant** | 800-999 | LT | ★★ | Silver | Commissioned Officer - Skilled tactician |
| **Lieutenant Commander** | 1000-1199 | LCDR | ★★ | Bright Silver | Senior Officer - Exceptional prowess |
| **Commander** | 1200-1399 | CDR | ★★★ | Gold | Command Officer - Strategic excellence |
| **Captain** | 1400-1599 | CAPT | ★★★ | Bright Gold | Ship Captain - Master of warfare |
| **Commodore** | 1600-1799 | CDRE | ★★★★ | Light Blue | Fleet Officer - Respected commander |
| **Rear Admiral** | 1800-1999 | RADM | ★★★★ | Blue | Lower Admiral - Elite tactician |
| **Vice Admiral** | 2000-2199 | VADM | ★★★★ | Purple | High Admiral - Galaxy's finest |
| **Admiral** | 2200-2499 | ADM | ★★★★★ | Bright Purple | Admiral - Supreme authority |
| **Fleet Admiral** | 2500-2799 | FADM | ★★★★★ | Red | Supreme Commander - Legendary |
| **Grand Admiral** | 2800+ | GADM | ★★★★★ | Crimson | Pinnacle of excellence |

---

## Starting Rank

**All new players start at 1200 ELO**, which places them in the **Commander** rank tier. This provides:
- A balanced starting point for matchmaking
- Room to climb or fall based on performance
- Immediate sense of prestige (not starting at the bottom)

---

## Rank Colors

Ranks use a color progression that reflects increasing prestige:

1. **Bronze Tier** (Cadet, Ensign) - Training ranks
2. **Silver Tier** (Lieutenant, Lieutenant Commander) - Intermediate ranks
3. **Gold Tier** (Commander, Captain) - Advanced ranks
4. **Blue Tier** (Commodore, Rear Admiral) - Expert ranks
5. **Purple Tier** (Vice Admiral, Admiral) - Master ranks
6. **Red Tier** (Fleet Admiral, Grand Admiral) - Legendary ranks

### RGB Color Values

```
Cadet:                RGB(0.6, 0.4, 0.2)   - Dark Bronze
Ensign:               RGB(0.8, 0.5, 0.2)   - Bronze
Lieutenant:           RGB(0.75, 0.75, 0.75) - Silver
Lieutenant Commander: RGB(0.9, 0.9, 0.95)  - Bright Silver
Commander:            RGB(1.0, 0.84, 0.0)  - Gold
Captain:              RGB(1.0, 0.92, 0.3)  - Bright Gold
Commodore:            RGB(0.7, 0.9, 1.0)   - Light Blue
Rear Admiral:         RGB(0.4, 0.7, 1.0)   - Blue
Vice Admiral:         RGB(0.6, 0.2, 0.8)   - Purple
Admiral:              RGB(0.8, 0.3, 0.9)   - Bright Purple
Fleet Admiral:        RGB(1.0, 0.3, 0.3)   - Red
Grand Admiral:        RGB(1.0, 0.2, 0.2)   - Crimson Red
```

---

## Rank Insignia (Stars)

Each rank displays a number of stars to indicate progression:
- **0 Stars**: Cadet (training only)
- **1 Star**: Ensign
- **2 Stars**: Lieutenant, Lieutenant Commander
- **3 Stars**: Commander, Captain
- **4 Stars**: Commodore, Rear Admiral, Vice Admiral
- **5 Stars**: Admiral, Fleet Admiral, Grand Admiral

---

## ELO System Details

### Starting ELO
- **Default**: 1200 ELO (Commander rank)

### ELO Boundaries
- **Minimum**: 100 ELO (cannot drop below)
- **Maximum**: 3000 ELO (theoretical cap)

### K-Factor (Rating Volatility)
The K-factor determines how much ELO changes per match:

- **Beginners** (<10 matches): K=40 (volatile, fast progression)
- **Intermediate** (10-50 matches): K=32 (moderate changes)
- **Veterans** (50+ matches): K=24 (stable ratings)
- **Masters** (1800+ ELO): K=16 (very stable, prevents rating inflation)

### Rank Distribution Goals
Approximate target distribution for a healthy player base:

```
Cadet:                5%  (New/struggling players)
Ensign:               8%
Lieutenant:           12%
Lieutenant Commander: 15%
Commander:            20% (Starting rank - largest group)
Captain:              15%
Commodore:            12%
Rear Admiral:         8%
Vice Admiral:         3%
Admiral:              1.5%
Fleet Admiral:        0.4%
Grand Admiral:        0.1% (Top 0.1% of players)
```

---

## Rank Progression Examples

### From Commander to Captain
- Current: 1200 ELO (Commander)
- Target: 1400 ELO (Captain)
- **Need**: +200 ELO (~8-12 wins against equal opponents)

### From Rear Admiral to Vice Admiral
- Current: 1800 ELO (Rear Admiral)
- Target: 2000 ELO (Vice Admiral)
- **Need**: +200 ELO (~12-16 wins, K-factor is lower at this level)

### From Admiral to Fleet Admiral
- Current: 2200 ELO (Admiral)
- Target: 2500 ELO (Fleet Admiral)
- **Need**: +300 ELO (~18-25 wins, very challenging)

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
