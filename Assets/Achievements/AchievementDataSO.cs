using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject definition for achievements.
///
/// Achievement Types:
/// - Single: Unlock once (e.g., "Win your first match")
/// - Incremental: Progress over time (e.g., "Win 100 matches")
/// - Tiered: Multiple tiers of same achievement (e.g., Bronze/Silver/Gold/Platinum)
///
/// Categories:
/// - Combat: Combat-related achievements
/// - Progression: Level/XP achievements
/// - Collection: Unlock all items
/// - Skill: Skill-based achievements (accuracy, streaks)
/// - Social: Multiplayer achievements
/// - Secret: Hidden achievements
///
/// Usage:
///   Right-click in Project → Create → GravityWars/Achievement
///   Configure achievement properties in Inspector
///   Add to AchievementService's achievement list
/// </summary>
[CreateAssetMenu(fileName = "NewAchievement", menuName = "GravityWars/Achievement", order = 1)]
public class AchievementDataSO : ScriptableObject
{
    #region Core Properties

    [Header("Core Info")]
    [Tooltip("Unique achievement ID (e.g., 'first_blood')")]
    public string achievementID = "new_achievement";

    [Tooltip("Display name shown to player")]
    public string displayName = "First Blood";

    [Tooltip("Achievement description")]
    [TextArea(2, 4)]
    public string description = "Win your first match!";

    [Tooltip("Icon for this achievement")]
    public Sprite icon;

    #endregion

    #region Achievement Type

    [Header("Type & Category")]
    [Tooltip("Achievement type")]
    public AchievementType achievementType = AchievementType.Single;

    [Tooltip("Achievement category")]
    public AchievementCategory category = AchievementCategory.Combat;

    [Tooltip("Is this a secret achievement? (hidden until unlocked)")]
    public bool isSecret = false;

    [Tooltip("Tier level (for tiered achievements)")]
    public AchievementTier tier = AchievementTier.None;

    #endregion

    #region Unlock Conditions

    [Header("Unlock Conditions")]
    [Tooltip("What triggers this achievement")]
    public AchievementConditionType conditionType = AchievementConditionType.WinMatches;

    [Tooltip("Target value to unlock (e.g., 100 for 'Win 100 matches')")]
    public int targetValue = 1;

    [Tooltip("Required context (e.g., 'Tank' for archetype-specific achievements)")]
    public string requiredContext = "";

    [Tooltip("Requires all conditions to be met simultaneously")]
    public bool requiresSimultaneousConditions = false;

    [Tooltip("Additional conditions (for complex achievements)")]
    public List<AchievementCondition> additionalConditions = new List<AchievementCondition>();

    #endregion

    #region Rewards

    [Header("Rewards")]
    [Tooltip("Soft currency reward")]
    public int softCurrencyReward = 100;

    [Tooltip("Hard currency reward")]
    public int hardCurrencyReward = 0;

    [Tooltip("XP reward")]
    public int accountXPReward = 50;

    [Tooltip("Exclusive cosmetic item ID")]
    public string exclusiveItemReward = "";

    [Tooltip("Player title reward")]
    public string titleReward = "";

    [Tooltip("Profile icon reward")]
    public Sprite profileIconReward;

    #endregion

    #region Display Info

    [Header("Display")]
    [Tooltip("Sort order (lower = shown first)")]
    public int sortOrder = 0;

    [Tooltip("Points awarded for this achievement")]
    public int achievementPoints = 10;

    [Tooltip("Required account level to see this achievement")]
    public int requiredLevelToView = 1;

    #endregion

    #region Platform Integration

    [Header("Platform Integration")]
    [Tooltip("Steam achievement ID")]
    public string steamAchievementID = "";

    [Tooltip("PlayStation trophy ID")]
    public string playstationTrophyID = "";

    [Tooltip("Xbox achievement ID")]
    public string xboxAchievementID = "";

    #endregion

    #region Runtime Instance Creation

    /// <summary>
    /// Creates a runtime instance of this achievement.
    /// </summary>
    public AchievementInstance CreateInstance()
    {
        return new AchievementInstance(this);
    }

    #endregion
}

#region Enums

/// <summary>
/// Achievement type.
/// </summary>
public enum AchievementType
{
    Single,        // Unlock once (0 → 1)
    Incremental,   // Progress over time (0 → target)
    Tiered         // Multiple tiers (bronze → silver → gold → platinum)
}

/// <summary>
/// Achievement category.
/// </summary>
public enum AchievementCategory
{
    Combat,        // Combat-related
    Progression,   // Level/XP
    Collection,    // Unlock all items
    Skill,         // Skill-based (accuracy, streaks)
    Social,        // Multiplayer
    Secret,        // Hidden achievements
    Special        // Special events
}

/// <summary>
/// Achievement tier (for tiered achievements).
/// </summary>
public enum AchievementTier
{
    None,
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond
}

/// <summary>
/// Achievement unlock condition types.
/// </summary>
public enum AchievementConditionType
{
    // Match-based
    WinMatches,
    PlayMatches,
    WinRounds,
    WinMatchesInRow,        // Win streak
    WinWithoutTakingDamage, // Flawless victory

    // Combat-based
    DealDamage,
    DealDamageInOneMatch,
    FireMissiles,
    HitMissiles,
    AchieveAccuracy,        // % accuracy
    DestroyShipsWithMissileType,

    // Perk-based
    UsePerkNTimes,
    WinWithPerk,
    UseAllPerks,

    // Ship-based
    WinWithArchetype,
    WinWithAllArchetypes,
    UnlockAllShips,

    // Progression
    ReachAccountLevel,
    EarnTotalCurrency,
    SpendTotalCurrency,
    UnlockAllItems,

    // Collection
    UnlockAllMissiles,
    UnlockAllPerks,
    UnlockAllCosmetics,

    // Skill-based
    WinWithPerfectAccuracy,
    WinIn60Seconds,
    DealDamageWithSingleShot,

    // Social
    PlayWithFriend,
    WinAgainstFriend,
    CompleteMatchAgainstPlayer,

    // Special
    PlayOnAllMaps,
    WinOnAllMaps,
    CompleteDailyQuest,
    CompleteWeeklyQuest,
    ReachBattlePassMaxTier
}

#endregion

#region Runtime Achievement Instance

/// <summary>
/// Runtime instance of an achievement.
/// Tracks progress and unlock state.
/// </summary>
[Serializable]
public class AchievementInstance
{
    // Reference to template
    [NonSerialized]
    public AchievementDataSO template;

    // Core info (cached from template)
    public string achievementID;
    public string displayName;
    public string description;
    public AchievementType achievementType;
    public AchievementCategory category;
    public AchievementConditionType conditionType;
    public int targetValue;
    public bool isSecret;
    public AchievementTier tier;

    // Progress tracking
    public int currentProgress = 0;
    public bool isUnlocked = false;
    public DateTime unlockedAt;

    // Rewards
    public int softCurrencyReward;
    public int hardCurrencyReward;
    public int accountXPReward;
    public string exclusiveItemReward;
    public string titleReward;
    public int achievementPoints;

    // Required context
    public string requiredContext;

    #region Constructors

    public AchievementInstance()
    {
    }

    public AchievementInstance(AchievementDataSO template)
    {
        this.template = template;

        // Cache template data
        achievementID = template.achievementID;
        displayName = template.username;
        description = template.description;
        achievementType = template.achievementType;
        category = template.category;
        conditionType = template.conditionType;
        targetValue = template.targetValue;
        isSecret = template.isSecret;
        tier = template.tier;
        requiredContext = template.requiredContext;

        // Cache rewards
        softCurrencyReward = template.creditsReward;
        hardCurrencyReward = template.gemsReward;
        accountXPReward = template.accountXPReward;
        exclusiveItemReward = template.exclusiveItemReward;
        titleReward = template.titleReward;
        achievementPoints = template.achievementPoints;

        // Initialize progress
        currentProgress = 0;
        isUnlocked = false;
    }

    #endregion

    #region Progress Management

    /// <summary>
    /// Adds progress to this achievement.
    /// </summary>
    public void AddProgress(int amount)
    {
        if (isUnlocked)
            return;

        currentProgress += amount;

        // Check if unlocked
        if (currentProgress >= targetValue && !isUnlocked)
        {
            Unlock();
        }
    }

    /// <summary>
    /// Sets absolute progress value.
    /// </summary>
    public void SetProgress(int value)
    {
        if (isUnlocked)
            return;

        currentProgress = value;

        // Check if unlocked
        if (currentProgress >= targetValue && !isUnlocked)
        {
            Unlock();
        }
    }

    /// <summary>
    /// Unlocks this achievement.
    /// </summary>
    public void Unlock()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;
        currentProgress = targetValue;
        unlockedAt = DateTime.UtcNow;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public float ProgressPercentage
    {
        get
        {
            if (targetValue <= 0)
                return 0f;

            return Mathf.Clamp01((float)currentProgress / targetValue) * 100f;
        }
    }

    /// <summary>
    /// Is this achievement completed but not yet claimed?
    /// (For achievements that require manual claiming)
    /// </summary>
    public bool IsClaimable
    {
        get { return isUnlocked && !isUnlocked; } // Can expand later for manual claiming
    }

    /// <summary>
    /// Display name (hides secret achievement names until unlocked).
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (isSecret && !isUnlocked)
                return "???";
            return displayName;
        }
    }

    /// <summary>
    /// Description (hides secret achievement descriptions until unlocked).
    /// </summary>
    public string Description
    {
        get
        {
            if (isSecret && !isUnlocked)
                return "This is a secret achievement.";
            return description;
        }
    }

    #endregion
}

#endregion

#region Additional Condition

/// <summary>
/// Additional condition for complex achievements.
/// </summary>
[Serializable]
public class AchievementCondition
{
    public AchievementConditionType conditionType;
    public int targetValue;
    public string requiredContext;
}

#endregion
