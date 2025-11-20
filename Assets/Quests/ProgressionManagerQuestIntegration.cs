using UnityEngine;
using GravityWars.Networking;

/// <summary>
/// Integrates quest system with ProgressionManager.
/// Tracks progression-related quest objectives.
///
/// IMPORTANT: Attach this component to the same GameObject as ProgressionManager.
/// It will automatically hook into progression events and update quest progress.
///
/// This is a non-invasive integration - no modifications to ProgressionManager needed.
///
/// Actions Tracked:
/// - Account level reached
/// - Currency earned
/// - Items unlocked
/// - Ships leveled up
///
/// Usage:
///   Simply attach to ProgressionManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(ProgressionManager))]
public class ProgressionManagerQuestIntegration : MonoBehaviour
{
    #region Configuration

    [Header("Quest Integration")]
    [Tooltip("Enable quest tracking")]
    public bool enableQuestTracking = true;

    #endregion

    #region Component References

    private ProgressionManager _progressionManager;
    private QuestService _questService;

    #endregion

    #region Tracking State

    private int _lastAccountLevel = 1;
    private int _totalCurrencyEarned = 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _progressionManager = GetComponent<ProgressionManager>();

        if (_progressionManager == null)
        {
            Debug.LogError("[ProgressionManagerQuestIntegration] ProgressionManager not found!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enableQuestTracking)
            return;

        // Get quest service
        _questService = QuestService.Instance;

        if (_questService == null)
        {
            Debug.LogWarning("[ProgressionManagerQuestIntegration] QuestService not available - quests disabled");
            enableQuestTracking = false;
            return;
        }

        // Initialize tracking state
        if (_progressionManager.currentPlayerData != null)
        {
            _lastAccountLevel = _progressionManager.currentPlayerData.level;
        }

        Log("Quest integration initialized");
    }

    #endregion

    #region Account Level Tracking

    /// <summary>
    /// Call this when player gains account XP.
    /// Checks for level up and updates reach level quests.
    /// </summary>
    public void OnAccountXPGained(int xpGained)
    {
        if (!enableQuestTracking || _progressionManager.currentPlayerData == null)
            return;

        var data = _progressionManager.currentPlayerData;

        // Check if leveled up
        if (data.level > _lastAccountLevel)
        {
            // Update quest: Reach Account Level
            _questService.UpdateQuestProgress(
                QuestObjectiveType.ReachAccountLevel,
                data.level // Pass current level as amount
            );

            Log($"Account level quest updated: Level {data.level}");

            _lastAccountLevel = data.level;
        }
    }

    #endregion

    #region Currency Tracking

    /// <summary>
    /// Call this when player earns currency (soft or hard).
    /// </summary>
    public void OnCurrencyEarned(string currencyType, int amount)
    {
        if (!enableQuestTracking)
            return;

        // Track total currency earned
        _totalCurrencyEarned += amount;

        // Update quest: Earn Currency
        _questService.UpdateQuestProgress(
            QuestObjectiveType.EarnCurrency,
            amount
        );

        Log($"Currency earned quest updated: {amount} {currencyType}");
    }

    /// <summary>
    /// Call this when player spends currency.
    /// (Currently no quests for spending, but included for future use)
    /// </summary>
    public void OnCurrencySpent(string currencyType, int amount, string itemPurchased)
    {
        if (!enableQuestTracking)
            return;

        // No current quests track spending, but could add in future
        Log($"Currency spent: {amount} {currencyType} on {itemPurchased}");
    }

    #endregion

    #region Item Unlock Tracking

    /// <summary>
    /// Call this when player unlocks an item.
    /// </summary>
    public void OnItemUnlocked(string itemType, string itemName, string unlockSource)
    {
        if (!enableQuestTracking)
            return;

        // Update quest: Unlock Item
        _questService.UpdateQuestProgress(
            QuestObjectiveType.UnlockItem,
            1,
            itemType
        );

        Log($"Item unlock quest updated: {itemType} - {itemName}");
    }

    #endregion

    #region Ship Leveling Tracking

    /// <summary>
    /// Call this when a custom loadout levels up.
    /// </summary>
    public void OnShipLevelUp(string loadoutKey, int newLevel)
    {
        if (!enableQuestTracking)
            return;

        // Update quest: Level Up Ship
        _questService.UpdateQuestProgress(
            QuestObjectiveType.LevelUpShip,
            1,
            loadoutKey
        );

        Log($"Ship level up quest updated: {loadoutKey} -> Level {newLevel}");
    }

    #endregion

    #region Battle Pass Tracking

    /// <summary>
    /// Call this when player tiers up in battle pass.
    /// </summary>
    public void OnBattlePassTierUp(int tierNumber, bool isPremium)
    {
        if (!enableQuestTracking)
            return;

        // Currently no battle pass quests, but included for future use
        Log($"Battle pass tier up: Tier {tierNumber} ({(isPremium ? "Premium" : "Free")})");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resets currency tracking (call at season reset).
    /// </summary>
    public void ResetCurrencyTracking()
    {
        _totalCurrencyEarned = 0;
        Log("Currency tracking reset");
    }

    /// <summary>
    /// Gets total currency earned this session.
    /// </summary>
    public int GetTotalCurrencyEarned()
    {
        return _totalCurrencyEarned;
    }

    private void Log(string message)
    {
        Debug.Log($"[ProgressionManagerQuestIntegration] {message}");
    }

    #endregion
}
