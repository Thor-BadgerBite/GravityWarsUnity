using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityWars.CloudSave
{
    /// <summary>
    /// Complete save data structure containing all player progress and state.
    /// This is the root data structure that gets serialized to cloud storage.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Meta Information
        public string playerID;
        public string saveVersion = "1.0.0";
        public long lastSaveTimestamp; // Unix timestamp
        public string deviceID;
        public int saveCount = 0;

        // Player Profile
        public PlayerAccountData profile = new PlayerAccountData();

        // Currency & Economy
        public CurrencyData currency = new CurrencyData();

        // Progression
        public ProgressionData progression = new ProgressionData();

        // Quests
        public QuestSaveData quests = new QuestSaveData();

        // Achievements
        public AchievementSaveData achievements = new AchievementSaveData();

        // Statistics
        public PlayerStatistics statistics = new PlayerStatistics();

        // Settings
        public PlayerSettings settings = new PlayerSettings();

        // Unlockables
        public UnlockablesData unlockables = new UnlockablesData();

        // Leaderboard Stats (for offline tracking)
        public LeaderboardStatsData leaderboardStats = new LeaderboardStatsData();

        // Analytics Data (for offline queuing)
        public AnalyticsQueueData analyticsQueue = new AnalyticsQueueData();

        public SaveData()
        {
            lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Creates a deep copy of this save data
        /// </summary>
        public SaveData Clone()
        {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<SaveData>(json);
        }

        /// <summary>
        /// Validates the integrity of save data
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(playerID)) return false;
            if (currency == null || progression == null) return false;
            if (currency.credits < 0 || currency.gems < 0) return false;
            if (progression.level < 1 || progression.experience < 0) return false;
            return true;
        }
    }

    #region Player Profile

    [Serializable]
    public class PlayerAccountData
    {
        public string displayName = "Player";
        public int avatarID = 0;
        public string customTitle = "";
        public long accountCreatedTimestamp;
        public long lastLoginTimestamp;
        public int totalPlaytimeSeconds = 0;
        public int loginStreak = 0;
        public long lastLoginStreakTimestamp;

        public PlayerAccountData()
        {
            accountCreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastLoginTimestamp = accountCreatedTimestamp;
            lastLoginStreakTimestamp = accountCreatedTimestamp;
        }
    }

    #endregion

    #region Currency & Economy

    [Serializable]
    public class CurrencyData
    {
        public int softCurrency = 0; // Credits
        public int hardCurrency = 0; // Gems
        public int premiumCurrency = 0; // Special currency

        // Transaction history (last 100 transactions)
        public List<CurrencyTransaction> recentTransactions = new List<CurrencyTransaction>();

        // Spending analytics
        public int lifetimeSoftCurrencyEarned = 0;
        public int lifetimeSoftCurrencySpent = 0;
        public int lifetimeHardCurrencyEarned = 0;
        public int lifetimeHardCurrencySpent = 0;
    }

    [Serializable]
    public class CurrencyTransaction
    {
        public string transactionID;
        public CurrencyType currencyType;
        public int amount; // Positive for gain, negative for spend
        public string reason;
        public long timestamp;
    }

    public enum CurrencyType
    {
        Soft,
        Hard,
        Premium
    }

    #endregion

    #region Progression

    [Serializable]
    public class ProgressionData
    {
        public int level = 1;
        public int experience = 0;
        public int prestigeLevel = 0;
        public int prestigePoints = 0;

        // Skill tree (using dictionary-like parallel arrays for serialization)
        public List<string> unlockedSkillIDs = new List<string>();
        public List<int> skillLevels = new List<int>();

        // Tutorial progress
        public List<string> completedTutorials = new List<string>();
        public bool hasCompletedOnboarding = false;

        // Milestones
        public List<string> reachedMilestones = new List<string>();
    }

    #endregion

    #region Quests

    [Serializable]
    public class QuestSaveData
    {
        // Active quests
        public List<ActiveQuestData> activeQuests = new List<ActiveQuestData>();

        // Completed quest IDs (for tracking)
        public List<string> completedQuestIDs = new List<string>();

        // Claimed quest IDs (for rewards)
        public List<string> claimedQuestIDs = new List<string>();

        // Last refresh timestamps
        public long lastDailyRefreshTimestamp;
        public long lastWeeklyRefreshTimestamp;
        public long lastSeasonRefreshTimestamp;

        // Quest statistics
        public int totalQuestsCompleted = 0;
        public int dailyQuestsCompleted = 0;
        public int weeklyQuestsCompleted = 0;
        public int seasonQuestsCompleted = 0;
    }

    [Serializable]
    public class ActiveQuestData
    {
        public string questID;
        public int currentProgress;
        public long acceptedTimestamp;
        public long expirationTimestamp;
        public bool isCompleted;
        public bool isClaimed;
    }

    #endregion

    #region Achievements

    [Serializable]
    public class AchievementSaveData
    {
        // Achievement progress (parallel arrays for serialization)
        public List<string> achievementIDs = new List<string>();
        public List<int> achievementProgress = new List<int>();
        public List<int> currentTiers = new List<int>();
        public List<bool> isUnlocked = new List<bool>();
        public List<long> unlockTimestamps = new List<long>();

        // Lifetime stats for achievements (separate from leaderboard stats)
        public Dictionary<string, int> lifetimeStats = new Dictionary<string, int>();

        // Achievement points
        public int totalAchievementPoints = 0;

        // Statistics
        public int totalAchievementsUnlocked = 0;
        public int totalSecretAchievementsUnlocked = 0;
    }

    #endregion

    #region Statistics

    [Serializable]
    public class PlayerStatistics
    {
        // Match Statistics
        public int totalMatchesPlayed = 0;
        public int totalMatchesWon = 0;
        public int totalMatchesLost = 0;
        public int totalRoundsPlayed = 0;
        public int totalRoundsWon = 0;

        // Win Streaks
        public int currentWinStreak = 0;
        public int longestWinStreak = 0;
        public int currentLossStreak = 0;

        // Combat Statistics
        public int totalDamageDealt = 0;
        public int totalDamageTaken = 0;
        public int totalMissilesFired = 0;
        public int totalMissilesHit = 0;
        public int totalKills = 0;
        public int totalDeaths = 0;

        // Perfection Statistics
        public int perfectWins = 0; // Won without taking damage
        public int flawlessRounds = 0;
        public int comebackWins = 0; // Won from behind

        // Weapon Statistics
        public Dictionary<string, WeaponStats> weaponStats = new Dictionary<string, WeaponStats>();

        // Time Statistics
        public int totalPlaytimeSeconds = 0;
        public long fastestWinSeconds = long.MaxValue;
        public long longestMatchSeconds = 0;

        // Map Statistics
        public Dictionary<string, MapStats> mapStats = new Dictionary<string, MapStats>();

        // Special Achievements
        public int trickshotHits = 0; // Missiles that bounced before hitting
        public int selfDestructs = 0;
        public int environmentalKills = 0;

        // Calculate derived stats
        public float GetWinRate()
        {
            if (totalMatchesPlayed == 0) return 0f;
            return (float)totalMatchesWon / totalMatchesPlayed * 100f;
        }

        public float GetAccuracy()
        {
            if (totalMissilesFired == 0) return 0f;
            return (float)totalMissilesHit / totalMissilesFired * 100f;
        }

        public float GetAverageDamagePerMatch()
        {
            if (totalMatchesPlayed == 0) return 0f;
            return (float)totalDamageDealt / totalMatchesPlayed;
        }

        public float GetKDRatio()
        {
            if (totalDeaths == 0) return totalKills;
            return (float)totalKills / totalDeaths;
        }
    }

    [Serializable]
    public class WeaponStats
    {
        public string weaponID;
        public int shotsFired = 0;
        public int shotsHit = 0;
        public int kills = 0;
        public int damageDealt = 0;
    }

    [Serializable]
    public class MapStats
    {
        public string mapID;
        public int matchesPlayed = 0;
        public int matchesWon = 0;
        public int favoriteCount = 0; // How many times player selected this map
    }

    #endregion

    #region Settings

    [Serializable]
    public class PlayerSettings
    {
        // Audio Settings
        public float masterVolume = 1.0f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public bool muteOnFocusLoss = false;

        // Graphics Settings
        public int qualityLevel = 2; // 0=Low, 1=Medium, 2=High, 3=Ultra
        public bool vsyncEnabled = true;
        public int targetFramerate = 60;
        public bool fullscreen = true;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;

        // Gameplay Settings
        public bool showTutorialHints = true;
        public bool autoSaveEnabled = true;
        public float aimSensitivity = 1.0f;
        public bool invertY = false;
        public bool showDamageNumbers = true;
        public bool showHealthBars = true;

        // UI Settings
        public bool showFPS = false;
        public bool showPing = true;
        public float uiScale = 1.0f;
        public string language = "en";

        // Notification Settings
        public bool questNotificationsEnabled = true;
        public bool achievementNotificationsEnabled = true;
        public bool friendNotificationsEnabled = true;
        public bool matchmakingNotificationsEnabled = true;

        // Privacy Settings
        public bool showOnlineStatus = true;
        public bool allowFriendRequests = true;
        public bool showInLeaderboards = true;
    }

    #endregion

    #region Unlockables

    [Serializable]
    public class UnlockablesData
    {
        // Cosmetics
        public List<string> unlockedSkins = new List<string>();
        public List<string> unlockedTrails = new List<string>();
        public List<string> unlockedEmotes = new List<string>();
        public List<string> unlockedTitles = new List<string>();
        public List<string> unlockedAvatars = new List<string>();

        // Active cosmetics
        public string activeSkin = "";
        public string activeTrail = "";
        public string activeTitle = "";
        public int activeAvatar = 0;

        // Ships & Weapons
        public List<string> unlockedShips = new List<string>();
        public List<string> unlockedWeapons = new List<string>();
        public string activeShip = "default";
        public List<string> activeWeaponLoadout = new List<string>();

        // Perks
        public List<string> unlockedPerks = new List<string>();
        public List<string> activePerks = new List<string>();

        // Maps
        public List<string> unlockedMaps = new List<string>();

        // Game Modes
        public List<string> unlockedGameModes = new List<string>();
    }

    #endregion

    #region Leaderboard Stats

    [Serializable]
    public class LeaderboardStatsData
    {
        // Cached stats for offline leaderboard submission
        public long bestScore = 0;
        public long bestWinStreak = 0;
        public double bestAccuracy = 0.0;
        public long fastestWinSeconds = long.MaxValue;
        public int highestDamageInMatch = 0;
        public int currentMMR = 1000;
        public int peakMMR = 1000;
        public int rankedPoints = 0;

        // Seasonal stats
        public int currentSeasonWins = 0;
        public int currentSeasonMatches = 0;
        public long currentSeasonStartTimestamp;

        // Last submission timestamps (for rate limiting)
        public Dictionary<string, long> lastSubmissionTimestamps = new Dictionary<string, long>();
    }

    #endregion

    #region Analytics Queue

    [Serializable]
    public class AnalyticsQueueData
    {
        // Queued events (for offline analytics)
        public List<QueuedAnalyticsEvent> queuedEvents = new List<QueuedAnalyticsEvent>();

        // Last successful batch upload
        public long lastUploadTimestamp;

        // Failed upload count (for backoff)
        public int failedUploadCount = 0;
    }

    [Serializable]
    public class QueuedAnalyticsEvent
    {
        public string eventName;
        public string eventDataJson; // Serialized Dictionary<string, object>
        public long timestamp;
        public int retryCount = 0;
    }

    #endregion

    #region Save Metadata

    /// <summary>
    /// Metadata about a save file, used for conflict resolution
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public string playerID;
        public long timestamp;
        public string deviceID;
        public int saveCount;
        public string saveVersion;
        public int dataHash; // For quick comparison

        public SaveMetadata() { }

        public SaveMetadata(SaveData saveData)
        {
            playerID = saveData.playerID;
            timestamp = saveData.lastSaveTimestamp;
            deviceID = saveData.deviceID;
            saveCount = saveData.saveCount;
            saveVersion = saveData.saveVersion;
            dataHash = saveData.GetHashCode();
        }
    }

    #endregion

    #region Conflict Resolution

    public enum ConflictResolutionStrategy
    {
        TakeNewest,      // Use the save with the latest timestamp
        TakeHighestProgress, // Use the save with higher level/progression
        Merge,           // Attempt to merge both saves (advanced)
        AskUser          // Let user decide
    }

    /// <summary>
    /// Result of a save conflict resolution
    /// </summary>
    public class ConflictResolutionResult
    {
        public bool hadConflict;
        public SaveData resolvedData;
        public ConflictResolutionStrategy strategyUsed;
        public string conflictDetails;
    }

    #endregion
}
