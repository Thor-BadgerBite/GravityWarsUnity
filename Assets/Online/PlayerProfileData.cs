using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete player profile data stored on server (Unity Cloud Save).
/// This is the master data structure containing everything about a player's account.
/// </summary>
[Serializable]
public class PlayerProfileData
{
    #region Account Information

    /// <summary>
    /// Unique player ID (from Unity Authentication)
    /// </summary>
    public string playerId;

    /// <summary>
    /// Player's chosen username (must be unique)
    /// </summary>
    public string username;

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public long accountCreatedTimestamp;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public long lastLoginTimestamp;

    /// <summary>
    /// Total playtime in seconds
    /// </summary>
    public long totalPlaytimeSeconds;

    #endregion

    #region Competitive Stats

    /// <summary>
    /// Current ELO rating for ranked matches
    /// Starting ELO: 1200 (standard chess starting rating)
    /// </summary>
    public int eloRating = 1200;

    /// <summary>
    /// Peak ELO rating ever achieved
    /// </summary>
    public int peakEloRating = 1200;

    /// <summary>
    /// Current competitive rank based on ELO
    /// </summary>
    public CompetitiveRank currentRank = CompetitiveRank.Bronze;

    /// <summary>
    /// Total ranked matches played
    /// </summary>
    public int rankedMatchesPlayed;

    /// <summary>
    /// Total ranked matches won
    /// </summary>
    public int rankedMatchesWon;

    /// <summary>
    /// Current ranked win streak
    /// </summary>
    public int currentWinStreak;

    /// <summary>
    /// Best win streak ever
    /// </summary>
    public int bestWinStreak;

    #endregion

    #region Casual Stats

    /// <summary>
    /// Total unranked (casual) matches played
    /// </summary>
    public int casualMatchesPlayed;

    /// <summary>
    /// Total unranked matches won
    /// </summary>
    public int casualMatchesWon;

    #endregion

    #region Progression

    /// <summary>
    /// Player account level
    /// </summary>
    public int level = 1;

    /// <summary>
    /// Current XP
    /// </summary>
    public int currentXP;

    /// <summary>
    /// XP required for next level
    /// </summary>
    public int xpForNextLevel = 1000;

    /// <summary>
    /// Soft currency (earned through gameplay)
    /// </summary>
    public int credits;

    /// <summary>
    /// Premium currency (purchased with real money)
    /// </summary>
    public int gems;

    #endregion

    #region Ships & Loadouts

    /// <summary>
    /// List of unlocked prebuild ship model IDs
    /// </summary>
    public List<string> unlockedShipModels = new List<string>();

    /// <summary>
    /// List of unlocked ship bodies (for custom building)
    /// </summary>
    public List<string> unlockedShipBodies = new List<string>();

    /// <summary>
    /// List of unlocked passive abilities
    /// </summary>
    public List<string> unlockedPassives = new List<string>();

    /// <summary>
    /// List of unlocked active abilities
    /// </summary>
    public List<string> unlockedActives = new List<string>();

    /// <summary>
    /// List of unlocked missiles (retrofit system)
    /// </summary>
    public List<string> unlockedMissiles = new List<string>();

    /// <summary>
    /// List of unlocked skins (cosmetic variants)
    /// </summary>
    public List<string> unlockedSkins = new List<string>();

    /// <summary>
    /// Currently equipped ship ID (displayed in main menu)
    /// </summary>
    public string currentEquippedShipId = "starter_ship";

    /// <summary>
    /// List of custom ship loadouts
    /// </summary>
    public List<CustomShipLoadout> customLoadouts = new List<CustomShipLoadout>();

    /// <summary>
    /// Currently selected loadout ID for ranked matches
    /// </summary>
    public string selectedRankedLoadoutId;

    /// <summary>
    /// Currently selected loadout ID for casual matches
    /// </summary>
    public string selectedCasualLoadoutId;

    #endregion

    #region Achievements & Quests

    /// <summary>
    /// List of unlocked achievement IDs
    /// </summary>
    public List<string> unlockedAchievements = new List<string>();

    /// <summary>
    /// Active quest progress
    /// </summary>
    public List<QuestProgressData> activeQuests = new List<QuestProgressData>();

    /// <summary>
    /// Completed quest IDs
    /// </summary>
    public List<string> completedQuests = new List<string>();

    #endregion

    #region Match History

    /// <summary>
    /// Recent match history (last 50 matches)
    /// </summary>
    public List<MatchResultData> recentMatches = new List<MatchResultData>();

    #endregion

    #region Statistics

    /// <summary>
    /// Total missiles fired across all matches
    /// </summary>
    public int totalMissilesFired;

    /// <summary>
    /// Total missiles hit
    /// </summary>
    public int totalMissilesHit;

    /// <summary>
    /// Total damage dealt
    /// </summary>
    public int totalDamageDealt;

    /// <summary>
    /// Total damage received
    /// </summary>
    public int totalDamageReceived;

    /// <summary>
    /// Total planets hit
    /// </summary>
    public int totalPlanetsHit;

    /// <summary>
    /// Favorite ship model ID (most used)
    /// </summary>
    public string favoriteShipModel;

    #endregion

    #region Settings & Preferences

    /// <summary>
    /// Player preferences (audio, graphics, controls)
    /// </summary>
    public PlayerPreferences preferences = new PlayerPreferences();

    #endregion

    #region Data Version

    /// <summary>
    /// Data structure version (for migration/compatibility)
    /// </summary>
    public int dataVersion = 1;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate win rate percentage for ranked matches
    /// </summary>
    public float GetRankedWinRate()
    {
        if (rankedMatchesPlayed == 0) return 0f;
        return (float)rankedMatchesWon / rankedMatchesPlayed * 100f;
    }

    /// <summary>
    /// Calculate win rate percentage for casual matches
    /// </summary>
    public float GetCasualWinRate()
    {
        if (casualMatchesPlayed == 0) return 0f;
        return (float)casualMatchesWon / casualMatchesPlayed * 100f;
    }

    /// <summary>
    /// Calculate overall win rate (ranked + casual)
    /// </summary>
    public float GetOverallWinRate()
    {
        int totalMatches = rankedMatchesPlayed + casualMatchesPlayed;
        if (totalMatches == 0) return 0f;
        int totalWins = rankedMatchesWon + casualMatchesWon;
        return (float)totalWins / totalMatches * 100f;
    }

    /// <summary>
    /// Calculate missile accuracy percentage
    /// </summary>
    public float GetMissileAccuracy()
    {
        if (totalMissilesFired == 0) return 0f;
        return (float)totalMissilesHit / totalMissilesFired * 100f;
    }

    /// <summary>
    /// Update rank based on current ELO
    /// </summary>
    public void UpdateRankFromELO()
    {
        if (eloRating < 1000) currentRank = CompetitiveRank.Bronze;
        else if (eloRating < 1200) currentRank = CompetitiveRank.Silver;
        else if (eloRating < 1400) currentRank = CompetitiveRank.Gold;
        else if (eloRating < 1600) currentRank = CompetitiveRank.Platinum;
        else if (eloRating < 1800) currentRank = CompetitiveRank.Diamond;
        else if (eloRating < 2000) currentRank = CompetitiveRank.Master;
        else currentRank = CompetitiveRank.Grandmaster;
    }

    #endregion
}

/// <summary>
/// Competitive ranks based on ELO rating
/// </summary>
public enum CompetitiveRank
{
    Bronze,      // < 1000 ELO
    Silver,      // 1000-1199
    Gold,        // 1200-1399
    Platinum,    // 1400-1599
    Diamond,     // 1600-1799
    Master,      // 1800-1999
    Grandmaster  // 2000+
}

/// <summary>
/// Quest progress data
/// </summary>
[Serializable]
public class QuestProgressData
{
    public string questId;
    public int currentProgress;
    public int targetProgress;
    public long startedTimestamp;
    public bool isCompleted;
}

/// <summary>
/// Match result data
/// </summary>
[Serializable]
public class MatchResultData
{
    public string matchId;
    public long timestamp;
    public bool isRanked;
    public bool didWin;
    public int roundsWon;
    public int roundsLost;
    public int damageDealt;
    public int damageReceived;
    public int missileFired;
    public int missileHits;
    public string opponentUsername;
    public int opponentEloRating;
    public int eloChange; // +/- ELO gained/lost
    public int xpGained;
    public int creditsGained;
    public string shipModelUsed;
}

/// <summary>
/// Player preferences
/// </summary>
[Serializable]
public class PlayerPreferences
{
    // Audio
    public float masterVolume = 1.0f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1.0f;

    // Graphics
    public int graphicsQuality = 2; // 0=Low, 1=Medium, 2=High
    public bool vSyncEnabled = true;
    public int targetFramerate = 60;

    // Controls
    public float mouseSensitivity = 1.0f;
    public bool invertYAxis = false;

    // Gameplay
    public bool showTrajectoryLine = true;
    public bool showDamageNumbers = true;
    public bool screenShakeEnabled = true;

    // Privacy
    public bool showProfilePublicly = true;
    public bool allowFriendRequests = true;
}
