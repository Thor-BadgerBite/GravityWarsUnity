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
    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    public PlayerProfileData() { }

    /// <summary>
    /// Convenience constructor used by offline/online systems when creating a new account.
    /// </summary>
    public PlayerProfileData(string playerId, string username)
    {
        this.playerId = playerId;
        this.username = username;

        accountCreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        lastLoginTimestamp = accountCreatedTimestamp;

        accountLevel = level = 1;
        accountXP = currentXP = 0;
        xpForNextLevel = ProgressionSystem.CalculateXPForLevel(accountLevel + 1);

        currentEquippedShipId = ProgressionSystem.STARTER_SHIP;
    }

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
    /// Player account level (legacy field kept for UI systems)
    /// Mirrors <see cref="accountLevel"/>.
    /// </summary>
    public int level = 1;

    /// <summary>
    /// Current XP toward the next level (legacy remainder value for UI).
    /// Mirrors <see cref="accountXP"/> progression calculations.
    /// </summary>
    public int currentXP;

    /// <summary>
    /// XP required for next level (legacy UI helper for currentXP remainder).
    /// </summary>
    public int xpForNextLevel = 1000;

    /// <summary>
    /// Total cumulative account XP (authoritative progression value).
    /// </summary>
    public int accountXP = 0;

    /// <summary>
    /// Account level used by progression/battle pass systems (authoritative).
    /// </summary>
    public int accountLevel = 1;

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
    /// Adds account XP and updates both cumulative and legacy UI values.
    /// </summary>
    public void AddAccountXP(int amount)
    {
        accountXP += amount;
        currentXP += amount;

        // Level-up loop uses the shared progression curve.
        while (currentXP >= xpForNextLevel)
        {
            currentXP -= xpForNextLevel;
            accountLevel++;
            level = accountLevel;
            xpForNextLevel = ProgressionSystem.CalculateXPForLevel(accountLevel + 1);
        }

        Debug.Log($"[PlayerProfileData] +{amount} Account XP (Total: {accountXP})");
    }

    /// <summary>
    /// Adds soft/hard currency.
    /// </summary>
    public void AddCurrency(int creditsAmount, int gemsAmount)
    {
        credits += creditsAmount;
        gems += gemsAmount;
        Debug.Log($"[PlayerProfileData] Currency: +{creditsAmount} credits, +{gemsAmount} gems");
    }

    /// <summary>
    /// Updates the last login timestamp to now.
    /// </summary>
    public void UpdateLastLogin()
    {
        lastLoginTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

/// <summary>
/// Represents a player-created ship loadout (configuration of components)
/// </summary>
[System.Serializable]
public class CustomShipLoadout
{
    public string loadoutID;          // Unique identifier
    public string loadoutName;        // Display name

    // Core Components (required)
    public string shipBodyName;       // ShipBodySO.name
    public string moveTypeName;       // MoveTypeSO.name
    public string equippedMissileName; // MissilePresetSO.name (can change without resetting XP!)

    // Perks (optional)
    public string tier1PerkName;      // ActivePerkSO.name
    public string tier2PerkName;
    public string tier3PerkName;

    // Passives (1-2 slots, future expansion)
    public List<string> passiveNames = new List<string>();

    // Cosmetics (optional)
    public string skinID;
    public string colorSchemeID;
    public string decalID;

    /// <summary>
    /// Generates a unique key for progression tracking (excludes missile and cosmetics!)
    /// This ensures changing missile doesn't reset ship XP, as per user requirement.
    /// </summary>
    public string GetProgressionKey()
    {
        // Combine body + perks + passives + move type (NOT missile or cosmetics)
        string key = $"{shipBodyName}|{tier1PerkName}|{tier2PerkName}|{tier3PerkName}|{moveTypeName}";
        foreach (var passive in passiveNames)
            key += $"|{passive}";
        return key;
    }

    /// <summary>
    /// Creates a unique loadout ID
    /// </summary>
    public static string GenerateLoadoutID()
    {
        return Guid.NewGuid().ToString();
    }
}

/// <summary>
/// Tracks progression (XP, level, stats) for a specific ship configuration
/// </summary>
[System.Serializable]
public class ShipProgressionEntry
{
    public string loadoutKey;         // Unique key (see CustomShipLoadout.GetProgressionKey())
    public string displayName;        // Display name for UI

    public int shipLevel = 1;
    public int shipXP = 0;

    public long firstUsedTimestamp;   // Changed from DateTime for serialization
    public long lastUsedTimestamp;

    // Stats for this ship
    public int matchesPlayed = 0;
    public int matchesWon = 0;
    public int roundsWon = 0;
    public int totalDamage = 0;
    public int totalKills = 0;

    public ShipProgressionEntry(string key, string name)
    {
        loadoutKey = key;
        displayName = name;
        firstUsedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        lastUsedTimestamp = firstUsedTimestamp;
    }

    /// <summary>
    /// Adds XP and checks for level-up
    /// </summary>
    public void AddXP(int amount)
    {
        shipXP += amount;
        lastUsedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Check for level-up (quadratic formula: XP = 200 + 75 × Level²)
        while (shipLevel < 20 && shipXP >= GetXPRequiredForLevel(shipLevel + 1))
        {
            shipLevel++;
            Debug.Log($"[ShipProgression] {displayName} leveled up to {shipLevel}!");
        }
    }

    /// <summary>
    /// Calculates XP required for a specific level
    /// </summary>
    public static int GetXPRequiredForLevel(int level)
    {
        return 200 + (75 * level * level);
    }

    /// <summary>
    /// Gets XP progress toward next level (0.0 to 1.0)
    /// </summary>
    public float GetLevelProgress()
    {
        if (shipLevel >= 20) return 1.0f;

        int currentLevelXP = GetXPRequiredForLevel(shipLevel);
        int nextLevelXP = GetXPRequiredForLevel(shipLevel + 1);
        int xpIntoLevel = shipXP - currentLevelXP;
        int xpNeededForLevel = nextLevelXP - currentLevelXP;

        return Mathf.Clamp01((float)xpIntoLevel / xpNeededForLevel);
    }
}
