# Leaderboard System Setup Guide

## Overview

The leaderboard system provides competitive rankings across multiple stat types and time frames.

**Features:**
- 10+ default leaderboard types
- Multiple scopes (Global, Friends, Regional)
- Multiple time frames (All-Time, Season, Monthly, Weekly, Daily)
- Ship-specific leaderboards
- Real-time rank updates
- Pagination support
- Player highlight and jump-to-player
- Anti-cheat integration
- Caching for performance
- Auto-refresh

---

## Installation Steps

### Step 1: Setup LeaderboardService

1. Create a new GameObject in your main scene: `[LeaderboardService]`
2. Add the **LeaderboardService** component to it
3. In Unity Editor, go to **Tools → Gravity Wars → Generate Leaderboard Configurations**
4. This generates 10 default leaderboard definitions (logged to Console)
5. In LeaderboardService Inspector, create leaderboard definitions manually:
   - Expand **Leaderboard Definitions** list
   - Add entries for each leaderboard type you want
   - Configure: ID, display name, stat type, scope, time frame, etc.

**Default Leaderboards:**
- Total Wins (All-Time)
- Longest Win Streak
- Total Damage Dealt
- Highest Damage (Single Match)
- Best Accuracy
- Fastest Win
- Win Rate
- Weekly Wins
- Monthly Wins
- Ranked MMR

**Configuration Options:**
- **Debug Logging**: Enable for testing
- **Cache Expiration**: How long to cache leaderboards (seconds)
- **Auto Refresh Cache**: Enable background refresh
- **Auto Refresh Interval**: Refresh interval (seconds)
- **Max Submissions Per Minute**: Rate limit for score submissions

### Step 2: Integrate with GameManager

1. Select your **GameManager** GameObject
2. Add the **GameManagerLeaderboardIntegration** component
3. Enable **Leaderboard Tracking** in Inspector

This component automatically tracks:
- Total wins/matches
- Win rate
- Win streaks (current and longest)
- Total damage dealt
- Highest damage in match
- Best accuracy
- Fastest win

**Important:** Add these method calls to your GameManager code:

```csharp
// In your match start method:
GetComponent<GameManagerLeaderboardIntegration>()?.OnMatchStart();

// In your match end method:
GetComponent<GameManagerLeaderboardIntegration>()?.OnMatchEnd(winner, isPlayer1Winner);

// When player fires missile:
GetComponent<GameManagerLeaderboardIntegration>()?.OnPlayerFireMissile(hit, damage);
```

**Configuration:**
- **Submit After Match**: Submit scores immediately after match (recommended)
- **Batch Submit Interval**: If not submitting after match, submit on interval

### Step 3: Setup Leaderboard UI

1. Create a Canvas GameObject if you don't have one
2. Add the **LeaderboardUI** component to the Canvas
3. Setup UI references in Inspector:

**Panel References:**
- **Leaderboard Panel**: Main leaderboard panel GameObject
- **Background Overlay**: Semi-transparent background

**Leaderboard Selection:**
- **Leaderboard Dropdown**: TMP_Dropdown for selecting leaderboard type

**Scope Tabs:**
- **Global Tab Button**: Show global leaderboard
- **Friends Tab Button**: Show friends-only leaderboard
- **Seasonal Tab Button**: Show seasonal leaderboard

**Entry Display:**
- **Entry Container**: Transform to hold entries (use VerticalLayoutGroup)
- **Entry Prefab**: Prefab for individual leaderboard entries
- **Scroll Rect**: ScrollRect for scrolling entries

**Pagination:**
- **Previous Page Button**: Go to previous page
- **Next Page Button**: Go to next page
- **Page Number Text**: Current page display
- **Jump To Player Button**: Jump to player's rank

**Header:**
- **Leaderboard Title Text**: Leaderboard name
- **Last Updated Text**: When last refreshed
- **Refresh Button**: Manual refresh button

**Loading:**
- **Loading Indicator**: Loading spinner/animation

**Player Info:**
- **Player Rank Text**: Player's current rank
- **Player Score Text**: Player's current score

**Panel Toggle:**
- **Toggle Panel Button**: Button to show/hide leaderboard panel

**Settings:**
- **Entries Per Page**: 20 (default)
- **Auto Refresh Interval**: 60 seconds
- **Enable Auto Refresh**: true

### Step 4: Create Leaderboard Entry Prefab

Create a prefab for leaderboard entries:

**Structure:**
```
LeaderboardEntry (Prefab)
├── Background (Image - colored for self/friend)
├── RankText (TextMeshProUGUI - e.g., "1", "2", "3")
├── RankMedal (Image - gold/silver/bronze for top 3)
├── PlayerAvatar (Image - player profile picture)
├── PlayerNameText (TextMeshProUGUI - player name)
├── ScoreText (TextMeshProUGUI - formatted score)
└── RankChangeText (TextMeshProUGUI - "+5", "-2", etc.)
```

**Components:**
- Add **LeaderboardEntryUI** component to the root
- Assign all UI references in Inspector
- Assign medal sprites (gold, silver, bronze)
- Configure colors:
  - Self Color: Yellow highlight
  - Friend Color: Blue highlight
  - Normal Color: White/gray

---

## Usage

### Submitting Scores

Scores are submitted automatically through the GameManager integration. For manual submission:

```csharp
// Submit single score:
await LeaderboardService.Instance.SubmitScore(
    LeaderboardStatType.TotalWins,
    score: 100
);

// Submit with decimal value (for accuracy, win rate, etc.):
await LeaderboardService.Instance.SubmitScore(
    LeaderboardStatType.BestAccuracy,
    score: 9500, // Stored as integer (95.00%)
    decimalScore: 95.0 // Display value
);

// Batch submit:
var stats = new Dictionary<LeaderboardStatType, (long, double)>
{
    { LeaderboardStatType.TotalWins, (100, 0) },
    { LeaderboardStatType.BestAccuracy, (9500, 95.0) }
};
await LeaderboardService.Instance.SubmitBatch(stats);
```

### Fetching Leaderboards

```csharp
// Fetch leaderboard:
var leaderboard = await LeaderboardService.Instance.FetchLeaderboard(
    "global_total_wins_alltime",
    pageNumber: 0,
    pageSize: 20
);

// Fetch friend leaderboard:
var friendLeaderboard = await LeaderboardService.Instance.FetchFriendLeaderboard(
    "global_total_wins_alltime"
);

// Fetch leaderboard around player:
var aroundPlayer = await LeaderboardService.Instance.FetchLeaderboardAroundPlayer(
    "global_total_wins_alltime",
    range: 10 // Show 10 above and 10 below player
);

// Fetch player's rank:
int rank = await LeaderboardService.Instance.FetchPlayerRank(
    "global_total_wins_alltime"
);
```

### Displaying Leaderboards

```csharp
// Show leaderboard panel:
LeaderboardUI.Instance.ShowLeaderboardPanel();

// Hide leaderboard panel:
LeaderboardUI.Instance.HideLeaderboardPanel();

// Select specific leaderboard:
LeaderboardUI.Instance.SelectLeaderboard("global_total_wins_alltime");
```

---

## Leaderboard Types

### Stat Types

- **TotalWins**: Total match wins
- **TotalMatches**: Total matches played
- **WinRate**: Win percentage
- **LongestWinStreak**: Longest win streak
- **CurrentWinStreak**: Current win streak
- **TotalDamageDealt**: Lifetime damage
- **HighestDamageInMatch**: Best damage in single match
- **AverageDamagePerMatch**: Average damage per match
- **BestAccuracy**: Best accuracy %
- **AverageAccuracy**: Average accuracy %
- **TotalMissilesHit**: Total missiles hit
- **FastestWin**: Fastest match win (seconds)
- **MMRRating**: Matchmaking rating
- **RankedPoints**: Ranked points

### Scopes

- **Global**: All players worldwide
- **Friends**: Friends-only leaderboard
- **Regional**: Region-specific (future)

### Time Frames

- **AllTime**: Lifetime stats (never resets)
- **Season**: Current season (resets quarterly)
- **Monthly**: Current month (resets monthly)
- **Weekly**: Current week (resets Monday)
- **Daily**: Today only (resets daily)

---

## Unity Gaming Services Integration

To integrate with Unity Gaming Services Leaderboards:

1. Install Unity Gaming Services SDK
2. Enable Leaderboards in Unity Dashboard
3. Create leaderboards in Unity Dashboard with matching IDs
4. Uncomment UGS code in LeaderboardService:

```csharp
// In SubmitToLeaderboard():
var leaderboardsService = LeaderboardsService.Instance;
await leaderboardsService.AddPlayerScoreAsync(
    definition.leaderboardID,
    score
);

// In FetchLeaderboardFromServer():
var scoresResponse = await leaderboardsService.GetScoresAsync(
    definition.leaderboardID,
    new GetScoresOptions
    {
        Offset = pageNumber * pageSize,
        Limit = pageSize
    }
);
```

---

## Anti-Cheat Integration

The leaderboard service includes built-in validation:

- Score range validation (e.g., accuracy 0-100%)
- Rate limiting (max submissions per minute)
- Stat-specific validation (e.g., fastest win >= 10 seconds)
- Integration hooks for SuspiciousActivityDetector

To enable:
1. Ensure SuspiciousActivityDetector is running
2. Leaderboard service automatically checks flagged accounts
3. Flagged accounts' scores are rejected

---

## Performance & Caching

- **Caching**: Leaderboards are cached for 5 minutes by default
- **Auto-Refresh**: Background refresh every 60 seconds
- **Rate Limiting**: Max 10 submissions per minute per player
- **Pagination**: Load 20 entries at a time (configurable)

---

## Testing

### Mock Data

LeaderboardService includes mock data generator for testing without server:

```csharp
// Mock data is automatically generated when server is unavailable
// Shows 1000 fake entries for testing UI
```

### Debug Commands

```csharp
// Clear cache:
LeaderboardService.Instance.ClearCache();

// Force refresh:
var leaderboard = await LeaderboardService.Instance.FetchLeaderboard(
    "global_total_wins_alltime",
    forceRefresh: true
);

// Get player stats:
var stats = LeaderboardService.Instance.GetPlayerStats();
Debug.Log($"Wins: {stats.totalWins}, Streak: {stats.longestWinStreak}");
```

---

## Troubleshooting

**Scores not submitting:**
- Check LeaderboardService is initialized
- Verify leaderboard definitions are configured
- Check rate limit (max 10/minute)
- Enable debug logging in LeaderboardService

**Leaderboard not displaying:**
- Verify UI references are assigned
- Check leaderboard ID matches definition
- Enable debug logging in LeaderboardUI
- Check Console for fetch errors

**Player rank not showing:**
- Ensure player has submitted scores
- Check leaderboard entry count
- Verify player ID is correct

---

## Next Steps

1. Configure Unity Gaming Services Leaderboards
2. Create custom leaderboard definitions
3. Design leaderboard UI
4. Setup leaderboard rewards
5. Add seasonal reset automation
6. Implement regional leaderboards
7. Add ship-specific leaderboards

---

## Support

For issues:
- Check Unity Console (enable debug logging)
- Verify LeaderboardService configuration
- Check GameManagerLeaderboardIntegration is tracking
- Test with mock data first
