using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Leaderboard data structures and enums.
///
/// Leaderboard Types:
/// - Global: All players worldwide
/// - Friends: Friends-only leaderboard
/// - Seasonal: Resets weekly/monthly/season
/// - ShipSpecific: Per ship archetype
///
/// Stat Types:
/// - Wins: Total match wins
/// - WinStreak: Longest win streak
/// - Damage: Highest damage dealt
/// - Accuracy: Best accuracy %
/// - QuickWins: Fastest match wins
///
/// Time Frames:
/// - AllTime: Lifetime stats
/// - Season: Current season only
/// - Monthly: Current month
/// - Weekly: Current week
/// - Daily: Today only
/// </summary>
namespace GravityWars.Networking
{
    #region Enums

    /// <summary>
    /// Leaderboard scope type.
    /// </summary>
    public enum LeaderboardScope
    {
        Global,        // All players worldwide
        Friends,       // Friends-only
        Regional       // Region-specific (future)
    }

    /// <summary>
    /// Leaderboard stat type.
    /// </summary>
    public enum LeaderboardStatType
    {
        // Match-based
        TotalWins,              // Total match wins
        TotalMatches,           // Total matches played
        WinRate,                // Win percentage

        // Streak-based
        LongestWinStreak,       // Longest win streak
        CurrentWinStreak,       // Current win streak

        // Damage-based
        TotalDamageDealt,       // Total lifetime damage
        HighestDamageInMatch,   // Highest damage in single match
        AverageDamagePerMatch,  // Average damage per match

        // Accuracy-based
        BestAccuracy,           // Best accuracy %
        AverageAccuracy,        // Average accuracy %
        TotalMissilesHit,       // Total missiles hit

        // Speed-based
        FastestWin,             // Fastest match win (seconds)
        AverageMatchDuration,   // Average match length

        // Score-based
        TotalScore,             // Total score/points earned
        HighestScore,           // Highest score in match

        // Ranking
        MMRRating,              // Matchmaking rating
        RankedPoints,           // Ranked mode points

        // Other
        TotalPlayTime,          // Total hours played
        AccountLevel            // Account level
    }

    /// <summary>
    /// Leaderboard time frame.
    /// </summary>
    public enum LeaderboardTimeFrame
    {
        AllTime,    // Lifetime stats
        Season,     // Current season
        Monthly,    // Current month
        Weekly,     // Current week
        Daily       // Today
    }

    /// <summary>
    /// Ship archetype filter for leaderboards.
    /// </summary>
    public enum LeaderboardShipFilter
    {
        All,        // All ship types
        Tank,       // Tank archetype only
        Sniper,     // Sniper archetype only
        AllAround,  // All-Around archetype only
        Glass       // Glass Cannon archetype only
    }

    #endregion

    #region Leaderboard Entry

    /// <summary>
    /// Single entry in a leaderboard.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        // Player info
        public string playerID;
        public string playerName;
        public string playerAvatarURL;
        public int accountLevel;

        // Ranking
        public int rank;
        public int previousRank;

        // Score/stat
        public long score;          // Can be wins, damage, accuracy, etc.
        public double decimalScore; // For accuracy, win rate, etc.
        public string formattedScore; // Human-readable score

        // Metadata
        public DateTime lastUpdated;
        public string country;      // For regional leaderboards
        public bool isFriend;       // Is this player a friend?
        public bool isSelf;         // Is this the current player?

        // Ship-specific (if applicable)
        public ShipArchetype shipArchetype;

        #region Helper Methods

        /// <summary>
        /// Gets rank change indicator.
        /// </summary>
        public string GetRankChange()
        {
            if (previousRank == 0)
                return "NEW";

            int change = previousRank - rank;

            if (change > 0)
                return $"+{change}"; // Improved
            else if (change < 0)
                return $"{change}";  // Dropped
            else
                return "-";          // No change
        }

        /// <summary>
        /// Gets formatted rank string.
        /// </summary>
        public string GetFormattedRank()
        {
            if (rank == 1)
                return "1st";
            else if (rank == 2)
                return "2nd";
            else if (rank == 3)
                return "3rd";
            else
                return $"{rank}th";
        }

        #endregion
    }

    #endregion

    #region Leaderboard Definition

    /// <summary>
    /// Leaderboard definition/configuration.
    /// </summary>
    [Serializable]
    public class LeaderboardDefinition
    {
        // Identity
        public string leaderboardID;
        public string displayName;
        public string description;

        // Legacy alias for display name
        public string username
        {
            get => displayName;
            set => displayName = value;
        }

        // Configuration
        public LeaderboardScope scope;
        public LeaderboardStatType statType;
        public LeaderboardTimeFrame timeFrame;
        public LeaderboardShipFilter shipFilter;

        // Display
        public string scoreFormat;      // e.g., "{0} wins", "{0:F1}%", "{0:F2}s"
        public string icon;             // Icon sprite name
        public bool descending;         // Higher is better?

        // Limits
        public int maxEntries = 100;    // Max entries to track
        public int entriesPerPage = 20; // Entries per page

        // Reset schedule
        public bool autoReset;          // Auto-reset on schedule?
        public DateTime nextResetTime;  // When to reset

        #region Helper Methods

        /// <summary>
        /// Formats score based on stat type.
        /// </summary>
        public string FormatScore(long score, double decimalScore = 0)
        {
            switch (statType)
            {
                case LeaderboardStatType.TotalWins:
                    return $"{score} wins";

                case LeaderboardStatType.LongestWinStreak:
                    return $"{score} streak";

                case LeaderboardStatType.TotalDamageDealt:
                    return $"{score:N0} dmg";

                case LeaderboardStatType.HighestDamageInMatch:
                    return $"{score:N0} dmg";

                case LeaderboardStatType.BestAccuracy:
                case LeaderboardStatType.AverageAccuracy:
                case LeaderboardStatType.WinRate:
                    return $"{decimalScore:F1}%";

                case LeaderboardStatType.FastestWin:
                    return $"{decimalScore:F2}s";

                case LeaderboardStatType.TotalScore:
                case LeaderboardStatType.HighestScore:
                    return $"{score:N0} pts";

                case LeaderboardStatType.MMRRating:
                case LeaderboardStatType.RankedPoints:
                    return $"{score} rating";

                case LeaderboardStatType.AccountLevel:
                    return $"Lv. {score}";

                case LeaderboardStatType.TotalPlayTime:
                    float hours = score / 3600f;
                    return $"{hours:F1} hrs";

                default:
                    return score.ToString();
            }
        }

        /// <summary>
        /// Gets unique leaderboard key.
        /// </summary>
        public string GetLeaderboardKey()
        {
            return $"{scope}_{statType}_{timeFrame}_{shipFilter}";
        }

        #endregion
    }

    #endregion

    #region Leaderboard Data

    /// <summary>
    /// Full leaderboard data with entries.
    /// </summary>
    [Serializable]
    public class LeaderboardData
    {
        // Definition
        public LeaderboardDefinition definition;

        // Entries
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        // Metadata
        public int totalEntries;        // Total entries (can be > entries.Count if paginated)
        public DateTime lastUpdated;
        public DateTime nextReset;

        // Player's position (if not in top entries)
        public LeaderboardEntry playerEntry;
        public bool playerInTopList;

        #region Query Methods

        /// <summary>
        /// Gets top N entries.
        /// </summary>
        public List<LeaderboardEntry> GetTopEntries(int count)
        {
            return entries.GetRange(0, Mathf.Min(count, entries.Count));
        }

        /// <summary>
        /// Gets entry by rank.
        /// </summary>
        public LeaderboardEntry GetEntryByRank(int rank)
        {
            return entries.Find(e => e.rank == rank);
        }

        /// <summary>
        /// Gets entry by player ID.
        /// </summary>
        public LeaderboardEntry GetEntryByPlayerID(string playerID)
        {
            return entries.Find(e => e.playerID == playerID);
        }

        /// <summary>
        /// Gets entries on page.
        /// </summary>
        public List<LeaderboardEntry> GetPage(int pageNumber, int pageSize)
        {
            int startIndex = pageNumber * pageSize;
            int count = Mathf.Min(pageSize, entries.Count - startIndex);

            if (startIndex >= entries.Count)
                return new List<LeaderboardEntry>();

            return entries.GetRange(startIndex, count);
        }

        #endregion
    }

    #endregion

    #region Player Leaderboard Stats

    /// <summary>
    /// Player's stats for leaderboard submission.
    /// </summary>
    [Serializable]
    public class PlayerLeaderboardStats
    {
        // Player info
        public string playerID;
        public string playerName;

        // Match stats
        public int totalWins;
        public int totalMatches;
        public float winRate;

        // Streak stats
        public int longestWinStreak;
        public int currentWinStreak;

        // Damage stats
        public long totalDamageDealt;
        public int highestDamageInMatch;
        public float averageDamagePerMatch;

        // Accuracy stats
        public float bestAccuracy;
        public float averageAccuracy;
        public int totalMissilesHit;

        // Speed stats
        public float fastestWin; // seconds
        public float averageMatchDuration;

        // Score stats
        public long totalScore;
        public int highestScore;

        // Ranking
        public int mmrRating;
        public int rankedPoints;

        // Other
        public int totalPlayTime; // seconds
        public int accountLevel;

        // Timestamp
        public DateTime lastUpdated;

        #region Helper Methods

        /// <summary>
        /// Gets stat value by type.
        /// </summary>
        public long GetStatValue(LeaderboardStatType statType, out double decimalValue)
        {
            decimalValue = 0;

            switch (statType)
            {
                case LeaderboardStatType.TotalWins:
                    return totalWins;

                case LeaderboardStatType.TotalMatches:
                    return totalMatches;

                case LeaderboardStatType.WinRate:
                    decimalValue = winRate;
                    return (long)(winRate * 100);

                case LeaderboardStatType.LongestWinStreak:
                    return longestWinStreak;

                case LeaderboardStatType.CurrentWinStreak:
                    return currentWinStreak;

                case LeaderboardStatType.TotalDamageDealt:
                    return totalDamageDealt;

                case LeaderboardStatType.HighestDamageInMatch:
                    return highestDamageInMatch;

                case LeaderboardStatType.AverageDamagePerMatch:
                    decimalValue = averageDamagePerMatch;
                    return (long)averageDamagePerMatch;

                case LeaderboardStatType.BestAccuracy:
                    decimalValue = bestAccuracy;
                    return (long)(bestAccuracy * 100);

                case LeaderboardStatType.AverageAccuracy:
                    decimalValue = averageAccuracy;
                    return (long)(averageAccuracy * 100);

                case LeaderboardStatType.TotalMissilesHit:
                    return totalMissilesHit;

                case LeaderboardStatType.FastestWin:
                    decimalValue = fastestWin;
                    return (long)(fastestWin * 100); // Store as centiseconds

                case LeaderboardStatType.AverageMatchDuration:
                    decimalValue = averageMatchDuration;
                    return (long)averageMatchDuration;

                case LeaderboardStatType.TotalScore:
                    return totalScore;

                case LeaderboardStatType.HighestScore:
                    return highestScore;

                case LeaderboardStatType.MMRRating:
                    return mmrRating;

                case LeaderboardStatType.RankedPoints:
                    return rankedPoints;

                case LeaderboardStatType.TotalPlayTime:
                    return totalPlayTime;

                case LeaderboardStatType.AccountLevel:
                    return accountLevel;

                default:
                    return 0;
            }
        }

        #endregion
    }

    #endregion
}
