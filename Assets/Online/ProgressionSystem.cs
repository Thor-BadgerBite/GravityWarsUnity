using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all progression unlocks and rewards for player leveling.
/// Controls access to features, ship classes, and custom slots based on player level.
///
/// Philosophy: Gradual feature unlocking to increase player engagement.
/// Not everything is available at level 1!
/// </summary>
public static class ProgressionSystem
{
    #region Feature Unlock Levels

    // Game Modes
    public const int RANKED_UNLOCK_LEVEL = 10;
    public const int CUSTOM_MATCH_UNLOCK_LEVEL = 5;

    // Custom Ship Slots (3 total)
    public const int CUSTOM_SLOT_1_UNLOCK_LEVEL = 1;   // Available from start
    public const int CUSTOM_SLOT_2_UNLOCK_LEVEL = 20;
    public const int CUSTOM_SLOT_3_UNLOCK_LEVEL = 40;

    // Ship Classes
    public const int CLASS_ALLAROUND_UNLOCK_LEVEL = 1;    // Starter class
    public const int CLASS_TANK_UNLOCK_LEVEL = 5;
    public const int CLASS_DD_UNLOCK_LEVEL = 15;          // DD = Damage Dealer
    public const int CLASS_CONTROLLER_UNLOCK_LEVEL = 25;

    // Other Features
    public const int ACHIEVEMENTS_UNLOCK_LEVEL = 3;
    public const int LEADERBOARD_UNLOCK_LEVEL = 8;
    public const int QUESTS_UNLOCK_LEVEL = 5;
    public const int CLAN_UNLOCK_LEVEL = 30;              // Future feature

    #endregion

    #region Level-Up Rewards Configuration

    /// <summary>
    /// Get rewards for reaching a specific level.
    /// Returns credits, gems, and any special unlocks.
    /// </summary>
    public static LevelUpReward GetLevelUpReward(int level)
    {
        var reward = new LevelUpReward
        {
            level = level,
            credits = CalculateCreditsReward(level),
            gems = CalculateGemsReward(level),
            unlocks = GetUnlocksForLevel(level)
        };

        return reward;
    }

    /// <summary>
    /// Calculate credits reward for leveling up.
    /// Scales with level (higher levels = more credits).
    /// </summary>
    private static int CalculateCreditsReward(int level)
    {
        // Base: 100 credits
        // Scaling: +50 per level
        // Special bonus every 10 levels: +500
        int baseCredits = 100 + (level * 50);
        int bonusCredits = (level % 10 == 0) ? 500 : 0;

        return baseCredits + bonusCredits;
    }

    /// <summary>
    /// Calculate gems reward for leveling up.
    /// Only given at milestone levels (every 5 levels).
    /// </summary>
    private static int CalculateGemsReward(int level)
    {
        // Gems only at milestone levels
        if (level % 5 == 0)
        {
            // Level 5: 10 gems
            // Level 10: 20 gems
            // Level 15: 30 gems, etc.
            return (level / 5) * 10;
        }

        return 0;
    }

    /// <summary>
    /// Get list of unlocks for a specific level.
    /// </summary>
    private static List<UnlockData> GetUnlocksForLevel(int level)
    {
        var unlocks = new List<UnlockData>();

        // Game Mode Unlocks
        if (level == RANKED_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.GameMode,
                id = "ranked",
                displayName = "Ranked Mode",
                description = "Compete in ranked matches to climb the ladder!"
            });
        }

        if (level == CUSTOM_MATCH_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.GameMode,
                id = "custom_match",
                displayName = "Custom Match",
                description = "Create custom matches with your own rules!"
            });
        }

        // Custom Ship Slot Unlocks
        if (level == CUSTOM_SLOT_2_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.CustomSlot,
                id = "custom_slot_2",
                displayName = "Custom Ship Slot #2",
                description = "Create and save a second custom ship loadout!"
            });
        }

        if (level == CUSTOM_SLOT_3_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.CustomSlot,
                id = "custom_slot_3",
                displayName = "Custom Ship Slot #3",
                description = "Create and save a third custom ship loadout!"
            });
        }

        // Ship Class Unlocks
        if (level == CLASS_TANK_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.ShipClass,
                id = ShipClass.Tank.ToString(),
                displayName = "Tank Class Ships",
                description = "High armor, slow movement. Perfect for absorbing damage!"
            });
        }

        if (level == CLASS_DD_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.ShipClass,
                id = ShipClass.DamageDealer.ToString(),
                displayName = "Damage Dealer Class Ships",
                description = "High firepower, low armor. Strike hard and fast!"
            });
        }

        if (level == CLASS_CONTROLLER_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.ShipClass,
                id = ShipClass.Controller.ToString(),
                displayName = "Controller Class Ships",
                description = "Special abilities and crowd control. Outsmart your opponent!"
            });
        }

        // Feature Unlocks
        if (level == ACHIEVEMENTS_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.Feature,
                id = "achievements",
                displayName = "Achievements",
                description = "Track your progress and earn special rewards!"
            });
        }

        if (level == QUESTS_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.Feature,
                id = "quests",
                displayName = "Daily Quests",
                description = "Complete daily challenges for extra rewards!"
            });
        }

        if (level == LEADERBOARD_UNLOCK_LEVEL)
        {
            unlocks.Add(new UnlockData
            {
                type = UnlockType.Feature,
                id = "leaderboard",
                displayName = "Leaderboards",
                description = "Compete for the top spot on the global leaderboard!"
            });
        }

        return unlocks;
    }

    #endregion

    #region Unlock Checks

    /// <summary>
    /// Check if ranked mode is unlocked for player.
    /// </summary>
    public static bool IsRankedUnlocked(int playerLevel)
    {
        return playerLevel >= RANKED_UNLOCK_LEVEL;
    }

    /// <summary>
    /// Check if a ship class is unlocked for player.
    /// </summary>
    public static bool IsShipClassUnlocked(ShipClass shipClass, int playerLevel)
    {
        switch (shipClass)
        {
            case ShipClass.AllAround:
                return playerLevel >= CLASS_ALLAROUND_UNLOCK_LEVEL;
            case ShipClass.Tank:
                return playerLevel >= CLASS_TANK_UNLOCK_LEVEL;
            case ShipClass.DamageDealer:
                return playerLevel >= CLASS_DD_UNLOCK_LEVEL;
            case ShipClass.Controller:
                return playerLevel >= CLASS_CONTROLLER_UNLOCK_LEVEL;
            default:
                return false;
        }
    }

    /// <summary>
    /// Get number of custom ship slots unlocked for player.
    /// Returns 1, 2, or 3.
    /// </summary>
    public static int GetUnlockedCustomSlots(int playerLevel)
    {
        if (playerLevel >= CUSTOM_SLOT_3_UNLOCK_LEVEL)
            return 3;
        else if (playerLevel >= CUSTOM_SLOT_2_UNLOCK_LEVEL)
            return 2;
        else
            return 1; // First slot always available
    }

    /// <summary>
    /// Check if a feature is unlocked for player.
    /// </summary>
    public static bool IsFeatureUnlocked(string featureId, int playerLevel)
    {
        switch (featureId.ToLower())
        {
            case "ranked":
                return playerLevel >= RANKED_UNLOCK_LEVEL;
            case "achievements":
                return playerLevel >= ACHIEVEMENTS_UNLOCK_LEVEL;
            case "leaderboard":
                return playerLevel >= LEADERBOARD_UNLOCK_LEVEL;
            case "quests":
                return playerLevel >= QUESTS_UNLOCK_LEVEL;
            case "custom_match":
                return playerLevel >= CUSTOM_MATCH_UNLOCK_LEVEL;
            case "clan":
                return playerLevel >= CLAN_UNLOCK_LEVEL;
            default:
                return true; // Unknown features are unlocked by default
        }
    }

    /// <summary>
    /// Get level requirement for a feature.
    /// Returns -1 if feature doesn't exist.
    /// </summary>
    public static int GetUnlockLevel(string featureId)
    {
        switch (featureId.ToLower())
        {
            case "ranked": return RANKED_UNLOCK_LEVEL;
            case "achievements": return ACHIEVEMENTS_UNLOCK_LEVEL;
            case "leaderboard": return LEADERBOARD_UNLOCK_LEVEL;
            case "quests": return QUESTS_UNLOCK_LEVEL;
            case "custom_match": return CUSTOM_MATCH_UNLOCK_LEVEL;
            case "clan": return CLAN_UNLOCK_LEVEL;
            case "tank": return CLASS_TANK_UNLOCK_LEVEL;
            case "dd": return CLASS_DD_UNLOCK_LEVEL;
            case "controller": return CLASS_CONTROLLER_UNLOCK_LEVEL;
            case "custom_slot_2": return CUSTOM_SLOT_2_UNLOCK_LEVEL;
            case "custom_slot_3": return CUSTOM_SLOT_3_UNLOCK_LEVEL;
            default: return -1;
        }
    }

    /// <summary>
    /// Get user-friendly message for locked feature.
    /// </summary>
    public static string GetLockedMessage(string featureId)
    {
        int unlockLevel = GetUnlockLevel(featureId);
        if (unlockLevel == -1)
        {
            return "This feature is currently unavailable.";
        }

        return $"Unlocks at Level {unlockLevel}";
    }

    #endregion

    #region Ship Class Helpers

    /// <summary>
    /// Get list of all unlocked ship classes for player.
    /// </summary>
    public static List<ShipClass> GetUnlockedShipClasses(int playerLevel)
    {
        var unlockedClasses = new List<ShipClass>();

        if (playerLevel >= CLASS_ALLAROUND_UNLOCK_LEVEL)
            unlockedClasses.Add(ShipClass.AllAround);

        if (playerLevel >= CLASS_TANK_UNLOCK_LEVEL)
            unlockedClasses.Add(ShipClass.Tank);

        if (playerLevel >= CLASS_DD_UNLOCK_LEVEL)
            unlockedClasses.Add(ShipClass.DamageDealer);

        if (playerLevel >= CLASS_CONTROLLER_UNLOCK_LEVEL)
            unlockedClasses.Add(ShipClass.Controller);

        return unlockedClasses;
    }

    /// <summary>
    /// Get ship class name for display.
    /// </summary>
    public static string GetShipClassName(ShipClass shipClass)
    {
        switch (shipClass)
        {
            case ShipClass.AllAround: return "All-Around";
            case ShipClass.Tank: return "Tank";
            case ShipClass.DamageDealer: return "Damage Dealer";
            case ShipClass.Controller: return "Controller";
            default: return "Unknown";
        }
    }

    /// <summary>
    /// Get ship class description.
    /// </summary>
    public static string GetShipClassDescription(ShipClass shipClass)
    {
        switch (shipClass)
        {
            case ShipClass.AllAround:
                return "Balanced stats. Good for beginners and versatile playstyles.";
            case ShipClass.Tank:
                return "High armor, slow movement. Absorbs damage and protects allies.";
            case ShipClass.DamageDealer:
                return "High firepower, low armor. Deals massive damage but fragile.";
            case ShipClass.Controller:
                return "Special abilities and crowd control. Disrupts enemy strategies.";
            default:
                return "Unknown ship class.";
        }
    }

    #endregion

    #region XP Calculation (Reference)

    /// <summary>
    /// Calculate XP required for next level.
    /// Exponential curve for increasing difficulty.
    /// </summary>
    public static int CalculateXPForLevel(int level)
    {
        // Base: 1000 XP for level 2
        // Exponential scaling: 1.15x per level
        return Mathf.RoundToInt(1000 * Mathf.Pow(1.15f, level - 1));
    }

    /// <summary>
    /// Calculate total XP required to reach a specific level.
    /// </summary>
    public static int CalculateTotalXPForLevel(int targetLevel)
    {
        int totalXP = 0;
        for (int level = 1; level < targetLevel; level++)
        {
            totalXP += CalculateXPForLevel(level);
        }
        return totalXP;
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Ship classes with different playstyles.
/// </summary>
public enum ShipClass
{
    AllAround,      // Balanced, starter class
    Tank,           // High armor, slow
    DamageDealer,   // High damage, low armor
    Controller      // Special abilities, crowd control
}

/// <summary>
/// Types of unlockables.
/// </summary>
public enum UnlockType
{
    GameMode,       // Ranked, Custom Match, etc.
    ShipClass,      // Tank, DD, Controller
    CustomSlot,     // Custom ship loadout slots
    Feature,        // Achievements, Quests, Leaderboard
    Ship,           // Specific ship models
    Cosmetic        // Skins, trails, etc.
}

/// <summary>
/// Reward data for leveling up.
/// </summary>
[Serializable]
public class LevelUpReward
{
    public int level;
    public int credits;
    public int gems;
    public List<UnlockData> unlocks;
}

/// <summary>
/// Data for a single unlock.
/// </summary>
[Serializable]
public class UnlockData
{
    public UnlockType type;
    public string id;
    public string displayName;
    public string description;
}

#endregion
