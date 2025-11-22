using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all account-wide progression data for a player.
/// This includes global XP, unlocked content, currency, ship progression, and online features.
/// UNIFIED DATA STRUCTURE - Used for both local and online play.
/// </summary>
[System.Serializable]
public class PlayerAccountData
{
    [Header("Save Version")]
    public int saveVersion = 3; // Incremented when data structure changes (for migration)

    [Header("Account Info")]
    public string playerID;
    public string username;  // Renamed from displayName for consistency
    public DateTime accountCreatedDate;
    public DateTime lastLoginDate;
    public long totalPlaytimeSeconds = 0; // Total time played in seconds

    [Header("Account Progression")]
    public int level = 1;
    public int currentXP = 0;
    public int xpForNextLevel = 1000; // XP required for next level

    [Header("Currency")]
    public int credits = 0;      // Renamed from softCurrency - Earned through gameplay
    public int gems = 0;         // Renamed from hardCurrency - Premium currency

    [Header("Competitive Stats - Online Play")]
    public int eloRating = 1200;
    public int peakEloRating = 1200;
    public CompetitiveRank currentRank = CompetitiveRank.Bronze;
    public int rankedMatchesPlayed = 0;
    public int rankedMatchesWon = 0;
    public int casualMatchesPlayed = 0;
    public int casualMatchesWon = 0;
    public int currentWinStreak = 0;
    public int bestWinStreak = 0;

    [Header("Battle Pass")]
    public int battlePassTier = 0;
    public int battlePassXP = 0;
    public bool hasPremiumBattlePass = false;
    public string currentSeasonID = "";

    [Header("Unlocked Content - Ship Bodies")]
    public List<string> unlockedShipBodyIDs = new List<string>();
    public List<string> unlockedShipModels = new List<string>(); // Prebuilt ship models

    [Header("Unlocked Content - Perks")]
    public List<string> unlockedTier1PerkIDs = new List<string>();
    public List<string> unlockedTier2PerkIDs = new List<string>();
    public List<string> unlockedTier3PerkIDs = new List<string>();

    [Header("Unlocked Content - Passives")]
    public List<string> unlockedPassiveIDs = new List<string>();

    [Header("Unlocked Content - Move Types")]
    public List<string> unlockedMoveTypeIDs = new List<string>();

    [Header("Unlocked Content - Missiles")]
    public List<string> unlockedMissileIDs = new List<string>();

    [Header("Unlocked Content - Cosmetics")]
    public List<string> unlockedSkinIDs = new List<string>();
    public List<string> unlockedColorSchemeIDs = new List<string>();
    public List<string> unlockedDecalIDs = new List<string>();

    [Header("Unlocked Content - Achievements")]
    public List<string> unlockedAchievements = new List<string>();

    [Header("Ship Progression - Custom Loadouts")]
    public List<CustomShipLoadout> customShipLoadouts = new List<CustomShipLoadout>();

    [Header("Ship Progression - Ship XP Tracking")]
    // Key = Unique loadout identifier (NOT including missile!)
    // Value = ShipProgressionEntry (XP, level, stats)
    public List<ShipProgressionEntry> shipProgressionData = new List<ShipProgressionEntry>();

    [Header("Online Features")]
    public string currentEquippedShipId = "";
    public string selectedRankedLoadoutId = "";
    public string selectedCasualLoadoutId = "";
    public List<MatchResultData> recentMatches = new List<MatchResultData>();
    public List<QuestProgressData> activeQuests = new List<QuestProgressData>();
    public List<string> completedQuests = new List<string>();

    [Header("Statistics")]
    public int totalMatchesPlayed = 0;
    public int totalMatchesWon = 0;
    public int totalRoundsWon = 0;
    public int totalDamageDealt = 0;
    public int totalMissilesFired = 0;
    public int totalMissilesHit = 0;
    public int totalDamageReceived = 0;
    public int totalPlanetsHit = 0;
    public string favoriteShipModel = ""; // Most used ship model

    [Header("Settings & Preferences")]
    public PlayerPreferences preferences = new PlayerPreferences();

    // Timestamp properties for cloud save compatibility (convert DateTime to Unix timestamps)
    public long accountCreatedTimestamp => (long)(accountCreatedDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    public long lastLoginTimestamp => (long)(lastLoginDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

    // Compatibility aliases for PlayerProfileData field names
    public List<string> unlockedShipBodies => unlockedShipBodyIDs;
    public List<string> unlockedPassives => unlockedPassiveIDs;
    public List<string> unlockedSkins => unlockedSkinIDs;
    public List<CustomShipLoadout> customLoadouts => customShipLoadouts;
    public List<string> unlockedActives
    {
        get
        {
            var all = new List<string>();
            all.AddRange(unlockedTier1PerkIDs);
            all.AddRange(unlockedTier2PerkIDs);
            all.AddRange(unlockedTier3PerkIDs);
            return all;
        }
    }

    /// <summary>
    /// Constructor for new accounts
    /// </summary>
    public PlayerAccountData(string id, string name)
    {
        playerID = id;
        username = name;
        accountCreatedDate = DateTime.Now;
        lastLoginDate = DateTime.Now;

        // Start with default unlocks (everything available for testing)
        // In production, you'd unlock only starter items
        InitializeDefaultUnlocks();
    }

    /// <summary>
    /// Unlocks default content for new accounts
    /// </summary>
    private void InitializeDefaultUnlocks()
    {
        // Starter content - minimal unlocks
        // (In production, you'd only unlock 1-2 ships, 1 perk, etc.)

        // For development, we'll unlock common items
        // Customize this based on your game design
    }

    /// <summary>
    /// Checks if a specific ScriptableObject is unlocked
    /// </summary>
    public bool IsUnlocked(ScriptableObject item)
    {
        if (item == null) return false;

        string itemName = item.name;

        // Check by type
        if (item is ShipBodySO)
            return unlockedShipBodyIDs.Contains(itemName);
        else if (item is ActivePerkSO perk)
        {
            return perk.tier switch
            {
                1 => unlockedTier1PerkIDs.Contains(itemName),
                2 => unlockedTier2PerkIDs.Contains(itemName),
                3 => unlockedTier3PerkIDs.Contains(itemName),
                _ => false
            };
        }
        else if (item is PassiveAbilitySO)
            return unlockedPassiveIDs.Contains(itemName);
        else if (item is MoveTypeSO)
            return unlockedMoveTypeIDs.Contains(itemName);
        else if (item is MissilePresetSO)
            return unlockedMissileIDs.Contains(itemName);

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
        if (item is ShipBodySO)
            unlockedShipBodyIDs.Add(itemName);
        else if (item is ActivePerkSO perk)
        {
            switch (perk.tier)
            {
                case 1: unlockedTier1PerkIDs.Add(itemName); break;
                case 2: unlockedTier2PerkIDs.Add(itemName); break;
                case 3: unlockedTier3PerkIDs.Add(itemName); break;
            }
        }
        else if (item is PassiveAbilitySO)
            unlockedPassiveIDs.Add(itemName);
        else if (item is MoveTypeSO)
            unlockedMoveTypeIDs.Add(itemName);
        else if (item is MissilePresetSO)
            unlockedMissileIDs.Add(itemName);

        Debug.Log($"[PlayerAccountData] Unlocked: {itemName}");
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
    /// Adds XP to account level
    /// </summary>
    public void AddAccountXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"[PlayerAccountData] +{amount} Account XP (Total: {currentXP})");
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

    /// <summary>
    /// Adds currency
    /// </summary>
    public void AddCurrency(int creditsAmount, int gemsAmount)
    {
        credits += creditsAmount;
        gems += gemsAmount;
        Debug.Log($"[PlayerAccountData] Currency: +{creditsAmount} credits, +{gemsAmount} gems");
    }

    /// <summary>
    /// Updates last login time
    /// </summary>
    public void UpdateLastLogin()
    {
        lastLoginDate = DateTime.Now;
    }

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

    public DateTime firstUsedDate;
    public DateTime lastUsedDate;

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
        firstUsedDate = DateTime.Now;
        lastUsedDate = DateTime.Now;
    }

    /// <summary>
    /// Adds XP and checks for level-up
    /// </summary>
    public void AddXP(int amount)
    {
        shipXP += amount;
        lastUsedDate = DateTime.Now;

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

/// <summary>
/// Competitive ranking tiers for matchmaking.
/// Military/Naval-themed ranks progressing from Cadet to Grand Admiral.
/// </summary>
[System.Serializable]
public enum CompetitiveRank
{
    Cadet,                  // 0-599 ELO - Training/Beginner
    Ensign,                 // 600-799 ELO - Junior Officer
    Lieutenant,             // 800-999 ELO - Officer
    LieutenantCommander,    // 1000-1199 ELO - Senior Officer
    Commander,              // 1200-1399 ELO - Command Officer (Starting ELO range)
    Captain,                // 1400-1599 ELO - Ship Captain
    Commodore,              // 1600-1799 ELO - Fleet Officer
    RearAdmiral,            // 1800-1999 ELO - Lower Admiral
    ViceAdmiral,            // 2000-2199 ELO - High Admiral
    Admiral,                // 2200-2499 ELO - Admiral
    FleetAdmiral,           // 2500-2799 ELO - Supreme Commander
    GrandAdmiral            // 2800+ ELO - Legendary Rank
}

/// <summary>
/// Stores data for a completed match
/// </summary>
[System.Serializable]
public class MatchResultData
{
    public string matchID;
    public DateTime matchDate;
    public bool isRanked;
    public bool won;
    public string opponentUsername;
    public int opponentELO;
    public int eloChange;
    public int roundsWon;
    public int roundsLost;
    public int damageDealt;
    public int damageReceived;
    public string shipUsed;
}

/// <summary>
/// Stores quest progress data
/// </summary>
[System.Serializable]
public class QuestProgressData
{
    public string questID;
    public int currentProgress;
    public int requiredProgress;
    public bool isCompleted;
    public bool isClaimed;
    public DateTime acceptedDate;
    public DateTime expirationDate;
}

/// <summary>
/// Player preferences and settings
/// </summary>
[System.Serializable]
public class PlayerPreferences
{
    [Header("Audio")]
    public float masterVolume = 1.0f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public bool muteWhenUnfocused = true;

    [Header("Graphics")]
    public int qualityLevel = 2;
    public bool vSync = true;
    public bool fullscreen = true;
    public int targetFrameRate = 60;

    [Header("Gameplay")]
    public bool showTutorialHints = true;
    public bool autoEquipBestShip = false;
    public bool confirmBeforeDelete = true;

    [Header("Controls")]
    public float mouseSensitivity = 1.0f;
    public bool invertYAxis = false;
}
