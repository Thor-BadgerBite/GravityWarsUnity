using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject definition for a quest.
///
/// Quests are goals that reward players for playing the game.
/// There are three types:
/// - Daily: Refresh every 24 hours (3 active at a time)
/// - Weekly: Refresh every 7 days (3 active at a time)
/// - Season: Permanent for the entire season
///
/// Quest Objectives:
/// - Win matches
/// - Deal damage
/// - Fire missiles
/// - Use specific perks
/// - Reach win streaks
/// - Unlock items
/// - Earn currency
///
/// Rewards:
/// - Soft currency (coins)
/// - Hard currency (gems)
/// - Account XP
/// - Items (ships, perks, cosmetics)
///
/// Usage:
///   Right-click in Project → Create → GravityWars/Quest
///   Configure quest parameters in Inspector
///   Reference in QuestService.questTemplates
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "GravityWars/Quest", order = 0)]
public class QuestDataSO : ScriptableObject
{
    [Header("Quest Identity")]
    [Tooltip("Unique identifier for this quest (e.g., 'daily_win_3_matches')")]
    public string questID;

    [Tooltip("Display name shown to player")]
    public string displayName = "Win 3 Matches";

    [Tooltip("Description shown to player")]
    [TextArea(2, 4)]
    public string description = "Victory is yours! Win 3 matches to complete this quest.";

    [Header("Quest Type")]
    [Tooltip("Quest type determines refresh schedule")]
    public QuestType questType = QuestType.Daily;

    [Header("Objective")]
    [Tooltip("What the player needs to do")]
    public QuestObjectiveType objectiveType = QuestObjectiveType.WinMatches;

    [Tooltip("How many times the objective must be completed")]
    public int targetValue = 3;

    [Header("Requirements")]
    [Tooltip("Minimum account level to unlock this quest")]
    public int requiredAccountLevel = 1;

    [Tooltip("Specific ship archetype required (leave as AllAround for no restriction)")]
    public ShipArchetype requiredArchetype = ShipArchetype.AllAround;

    [Tooltip("Specific missile type required (leave empty for no restriction)")]
    public string requiredMissileType = "";

    [Header("Rewards")]
    [Tooltip("Soft currency reward (coins)")]
    public int softCurrencyReward = 100;

    [Tooltip("Hard currency reward (gems)")]
    public int hardCurrencyReward = 0;

    [Tooltip("Account XP reward")]
    public int accountXPReward = 50;

    [Tooltip("Item rewards (leave empty for no items)")]
    public List<string> itemRewards = new List<string>();

    [Header("Visual")]
    [Tooltip("Quest icon")]
    public Sprite icon;

    [Tooltip("Quest difficulty (affects UI display)")]
    public QuestDifficulty difficulty = QuestDifficulty.Easy;

    #region Runtime Data (Not in Inspector)

    /// <summary>Current progress toward completing this quest.</summary>
    [HideInInspector]
    public int currentProgress = 0;

    /// <summary>When this quest was assigned to the player.</summary>
    [HideInInspector]
    public DateTime assignedAt;

    /// <summary>When this quest expires.</summary>
    [HideInInspector]
    public DateTime expiresAt;

    /// <summary>Is this quest completed?</summary>
    public bool IsCompleted => currentProgress >= targetValue;

    /// <summary>Progress percentage (0.0 to 1.0)</summary>
    public float ProgressPercentage => targetValue > 0 ? (float)currentProgress / targetValue : 0f;

    /// <summary>Time remaining until expiration (for daily/weekly quests).</summary>
    public TimeSpan TimeRemaining => expiresAt - DateTime.UtcNow;

    /// <summary>Is this quest expired?</summary>
    public bool IsExpired => DateTime.UtcNow > expiresAt;

    #endregion

    #region Methods

    /// <summary>
    /// Increments progress by specified amount.
    /// </summary>
    public void AddProgress(int amount)
    {
        if (IsCompleted || IsExpired)
            return;

        currentProgress = Mathf.Min(currentProgress + amount, targetValue);

        Debug.Log($"[Quest] {displayName} progress: {currentProgress}/{targetValue}");
    }

    /// <summary>
    /// Resets quest progress.
    /// </summary>
    public void ResetProgress()
    {
        currentProgress = 0;
    }

    /// <summary>
    /// Creates a runtime instance of this quest.
    /// </summary>
    public QuestInstance CreateInstance()
    {
        return new QuestInstance(this);
    }

    #endregion

    #region Validation

    private void OnValidate()
    {
        // Ensure quest ID is set
        if (string.IsNullOrEmpty(questID))
        {
            questID = name.ToLower().Replace(" ", "_");
        }

        // Ensure target value is positive
        if (targetValue <= 0)
        {
            targetValue = 1;
        }

        // Ensure at least one reward
        if (softCurrencyReward == 0 && hardCurrencyReward == 0 && accountXPReward == 0 && itemRewards.Count == 0)
        {
            Debug.LogWarning($"[Quest] {displayName} has no rewards!");
        }
    }

    #endregion
}

#region Enums

/// <summary>
/// Quest type determines refresh schedule.
/// </summary>
public enum QuestType
{
    Daily,      // Refresh every 24 hours
    Weekly,     // Refresh every 7 days
    Season      // Permanent for entire season
}

/// <summary>
/// Quest objective types.
/// </summary>
public enum QuestObjectiveType
{
    // Match objectives
    WinMatches,
    PlayMatches,
    WinRounds,

    // Combat objectives
    DealDamage,
    FireMissiles,
    HitMissiles,
    DestroyShipsWithMissileType, // Requires specific missile type

    // Perk objectives
    UsePerkNTimes,
    WinWithPerk,

    // Ship objectives
    PlayMatchesWithArchetype,
    WinWithArchetype,

    // Streak objectives
    ReachWinStreak,

    // Progression objectives
    UnlockItem,
    ReachAccountLevel,
    EarnCurrency,
    LevelUpShip
}

/// <summary>
/// Quest difficulty (affects reward scaling and UI display).
/// </summary>
public enum QuestDifficulty
{
    Easy,       // 1-star
    Medium,     // 2-star
    Hard,       // 3-star
    VeryHard    // 4-star (epic quests)
}

#endregion

#region Runtime Quest Instance

/// <summary>
/// Runtime instance of a quest assigned to a player.
/// Serializable for saving to PlayerProfileData.
/// </summary>
[Serializable]
public class QuestInstance
{
    public string questID;
    public string displayName;
    public string description;
    public QuestType questType;
    public QuestObjectiveType objectiveType;
    public int targetValue;
    public int currentProgress;
    public DateTime assignedAt;
    public DateTime expiresAt;

    // Rewards
    public int softCurrencyReward;
    public int hardCurrencyReward;
    public int accountXPReward;
    public List<string> itemRewards;

    // Requirements
    public int requiredAccountLevel;
    public ShipArchetype requiredArchetype;
    public string requiredMissileType;

    // Visual
    public QuestDifficulty difficulty;

    // Computed properties
    public bool IsCompleted => currentProgress >= targetValue;
    public float ProgressPercentage => targetValue > 0 ? (float)currentProgress / targetValue : 0f;
    public TimeSpan TimeRemaining => expiresAt - DateTime.UtcNow;
    public bool IsExpired => DateTime.UtcNow > expiresAt;

    // Constructor from ScriptableObject
    public QuestInstance(QuestDataSO template)
    {
        questID = template.questID;
        displayName = template.displayName;
        description = template.description;
        questType = template.questType;
        objectiveType = template.objectiveType;
        targetValue = template.targetValue;
        currentProgress = 0;
        assignedAt = DateTime.UtcNow;

        // Calculate expiration based on quest type
        expiresAt = questType switch
        {
            QuestType.Daily => DateTime.UtcNow.AddHours(24),
            QuestType.Weekly => DateTime.UtcNow.AddDays(7),
            QuestType.Season => DateTime.UtcNow.AddDays(90), // 3 months
            _ => DateTime.UtcNow.AddHours(24)
        };

        softCurrencyReward = template.softCurrencyReward;
        hardCurrencyReward = template.hardCurrencyReward;
        accountXPReward = template.accountXPReward;
        itemRewards = new List<string>(template.itemRewards);

        requiredAccountLevel = template.requiredAccountLevel;
        requiredArchetype = template.requiredArchetype;
        requiredMissileType = template.requiredMissileType;

        difficulty = template.difficulty;
    }

    // Parameterless constructor for deserialization
    public QuestInstance() { }

    public void AddProgress(int amount)
    {
        if (IsCompleted || IsExpired)
            return;

        currentProgress = Mathf.Min(currentProgress + amount, targetValue);
    }
}

#endregion
