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

    /// <summary>
    /// Ship progression data (XP tracking per loadout configuration)
    /// Key = Unique loadout identifier (NOT including missile!)
    /// Value = ShipProgressionEntry (XP, level, stats)
    /// </summary>
    public List<ShipProgressionEntry> shipProgressionData = new List<ShipProgressionEntry>();

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

    /// <summary>
    /// Checks if a specific ScriptableObject is unlocked
    /// </summary>
    public bool IsUnlocked(ScriptableObject item)
    {
        if (item == null) return false;

        string itemName = item.name;

        // Check by type (requires ShipBodySO, ActivePerkSO, etc. to be defined)
        // This uses Unity's type checking for ScriptableObjects
        if (item.GetType().Name == "ShipBodySO")
            return unlockedShipBodies.Contains(itemName);
        else if (item.GetType().Name == "ActivePerkSO")
            return unlockedActives.Contains(itemName);
        else if (item.GetType().Name == "PassiveAbilitySO")
            return unlockedPassives.Contains(itemName);
        else if (item.GetType().Name == "MoveTypeSO")
            return unlockedShipBodies.Contains(itemName); // Move types may share same list or need separate check
        else if (item.GetType().Name == "MissilePresetSO")
            return unlockedMissiles.Contains(itemName);

        return false;
    }

    /// <summary>
    /// Unlocks a specific ScriptableObject
    /// </summary>
    public void UnlockItem(ScriptableObject item)
    {
        if (item == null || IsUnlocked(item)) return;

        string itemName = item.name;

        // Add to appropriate list
        if (item.GetType().Name == "ShipBodySO")
            unlockedShipBodies.Add(itemName);
        else if (item.GetType().Name == "ActivePerkSO")
            unlockedActives.Add(itemName);
        else if (item.GetType().Name == "PassiveAbilitySO")
            unlockedPassives.Add(itemName);
        else if (item.GetType().Name == "MoveTypeSO")
            unlockedShipBodies.Add(itemName); // Move types may share same list
        else if (item.GetType().Name == "MissilePresetSO")
            unlockedMissiles.Add(itemName);

        Debug.Log($"[PlayerProfileData] Unlocked: {itemName}");
    }

    /// <summary>
    /// Gets progression data for a specific ship loadout (excluding missile!)
    /// </summary>
    public ShipProgressionEntry GetShipProgression(CustomShipLoadout loadout)
    {
        if (loadout == null) return null;

        // Generate unique ID based on body + perks + passive + move type (NOT missile!)
        string loadoutKey = loadout.GetProgressionKey();

        // Find existing entry
        var existing = shipProgressionData.Find(e => e.loadoutKey == loadoutKey);
        if (existing != null)
            return existing;

        // Create new entry
        var newEntry = new ShipProgressionEntry(loadoutKey, loadout.loadoutName);
        shipProgressionData.Add(newEntry);
        return newEntry;
    }

    /// <summary>
    /// Adds XP to a specific ship loadout
    /// </summary>
    public void AddShipXP(CustomShipLoadout loadout, int amount)
    {
        var progression = GetShipProgression(loadout);
        if (progression != null)
        {
            progression.AddXP(amount);
        }
    }

    #endregion
}

// NOTE: Helper classes (CompetitiveRank, QuestProgressData, MatchResultData,
// PlayerPreferences, CustomShipLoadout, ShipProgressionEntry) are defined in
// PlayerAccountData.cs to avoid duplicate definitions.
