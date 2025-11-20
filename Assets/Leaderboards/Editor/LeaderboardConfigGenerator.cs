using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GravityWars.Networking;

/// <summary>
/// Editor utility to generate default leaderboard configurations.
///
/// Usage:
///   In Unity Editor: Tools → Gravity Wars → Generate Leaderboard Configurations
///
/// This will create a LeaderboardConfig ScriptableObject with default leaderboards.
/// </summary>
public class LeaderboardConfigGenerator : EditorWindow
{
    [MenuItem("Tools/Gravity Wars/Generate Leaderboard Configurations")]
    public static void ShowWindow()
    {
        GetWindow<LeaderboardConfigGenerator>("Leaderboard Config Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Leaderboard Configuration Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will create default leaderboard configurations.");
        GUILayout.Space(10);

        if (GUILayout.Button("Generate Default Leaderboards", GUILayout.Height(40)))
        {
            GenerateDefaultLeaderboards();
        }
    }

    private void GenerateDefaultLeaderboards()
    {
        var leaderboards = new List<LeaderboardDefinition>
        {
            // Total Wins (Global, All-Time)
            new LeaderboardDefinition
            {
                leaderboardID = "global_total_wins_alltime",
                displayName = "Total Wins",
                description = "All-time match wins leaderboard",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.TotalWins,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0} wins",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Win Streak (Global, All-Time)
            new LeaderboardDefinition
            {
                leaderboardID = "global_longest_streak_alltime",
                displayName = "Longest Win Streak",
                description = "Longest win streak ever achieved",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.LongestWinStreak,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0} streak",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Total Damage (Global, All-Time)
            new LeaderboardDefinition
            {
                leaderboardID = "global_total_damage_alltime",
                displayName = "Total Damage Dealt",
                description = "Lifetime damage dealt leaderboard",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.TotalDamageDealt,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0:N0} dmg",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Highest Damage In Match (Global, All-Time)
            new LeaderboardDefinition
            {
                leaderboardID = "global_highest_damage_match_alltime",
                displayName = "Highest Damage (Single Match)",
                description = "Highest damage dealt in a single match",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.HighestDamageInMatch,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0:N0} dmg",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Best Accuracy (Global, All-Time)
            new LeaderboardDefinition
            {
                leaderboardID = "global_best_accuracy_alltime",
                displayName = "Best Accuracy",
                description = "Best accuracy percentage achieved",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.BestAccuracy,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0:F1}%",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Fastest Win (Global, All-Time)
            new LeaderboardDefinition
            {
                leaderboardID = "global_fastest_win_alltime",
                displayName = "Fastest Win",
                description = "Fastest match victory time",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.FastestWin,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0:F2}s",
                descending = false, // Lower is better
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Win Rate (Global, All-Time, min 10 matches)
            new LeaderboardDefinition
            {
                leaderboardID = "global_win_rate_alltime",
                displayName = "Win Rate",
                description = "Win percentage (minimum 10 matches)",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.WinRate,
                timeFrame = LeaderboardTimeFrame.AllTime,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0:F1}%",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = false
            },

            // Weekly Wins (Global, Weekly)
            new LeaderboardDefinition
            {
                leaderboardID = "global_wins_weekly",
                displayName = "Weekly Wins",
                description = "Most wins this week",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.TotalWins,
                timeFrame = LeaderboardTimeFrame.Weekly,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0} wins",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = true,
                nextResetTime = GetNextMonday()
            },

            // Monthly Wins (Global, Monthly)
            new LeaderboardDefinition
            {
                leaderboardID = "global_wins_monthly",
                displayName = "Monthly Wins",
                description = "Most wins this month",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.TotalWins,
                timeFrame = LeaderboardTimeFrame.Monthly,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0} wins",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = true,
                nextResetTime = GetFirstDayOfNextMonth()
            },

            // MMR Rating (Global, Season)
            new LeaderboardDefinition
            {
                leaderboardID = "global_mmr_season",
                displayName = "Ranked MMR",
                description = "Ranked matchmaking rating this season",
                scope = LeaderboardScope.Global,
                statType = LeaderboardStatType.MMRRating,
                timeFrame = LeaderboardTimeFrame.Season,
                shipFilter = LeaderboardShipFilter.All,
                scoreFormat = "{0} rating",
                descending = true,
                maxEntries = 1000,
                entriesPerPage = 20,
                autoReset = true,
                nextResetTime = GetNextSeasonStart()
            },
        };

        Debug.Log($"[LeaderboardConfigGenerator] Generated {leaderboards.Count} default leaderboards");

        EditorUtility.DisplayDialog("Success",
            $"Generated {leaderboards.Count} default leaderboard configurations!\n\nAdd these to your LeaderboardService component.",
            "OK");

        // Log configurations for easy setup
        foreach (var lb in leaderboards)
        {
            Debug.Log($"  - {lb.username} ({lb.leaderboardID})");
        }
    }

    private static System.DateTime GetNextMonday()
    {
        var today = System.DateTime.UtcNow.Date;
        int daysUntilMonday = ((int)System.DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0)
            daysUntilMonday = 7;
        return today.AddDays(daysUntilMonday);
    }

    private static System.DateTime GetFirstDayOfNextMonth()
    {
        var today = System.DateTime.UtcNow.Date;
        return new System.DateTime(today.Year, today.Month, 1).AddMonths(1);
    }

    private static System.DateTime GetNextSeasonStart()
    {
        // Assume seasons start on March 1, June 1, September 1, December 1
        var today = System.DateTime.UtcNow.Date;
        int currentMonth = today.Month;

        int nextSeasonMonth = currentMonth switch
        {
            < 3 => 3,
            < 6 => 6,
            < 9 => 9,
            < 12 => 12,
            _ => 3 // Next year
        };

        int year = nextSeasonMonth < currentMonth ? today.Year + 1 : today.Year;
        return new System.DateTime(year, nextSeasonMonth, 1);
    }
}
