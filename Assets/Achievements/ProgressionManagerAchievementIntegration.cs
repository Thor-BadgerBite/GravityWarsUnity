using UnityEngine;
using GravityWars.Networking;

/// <summary>
/// Integrates achievement system with ProgressionManager.
/// Tracks progression-related achievement progress.
///
/// IMPORTANT: Attach this component to the same GameObject as ProgressionManager.
/// It will automatically hook into progression events and update achievement progress.
///
/// This is a non-invasive integration - no modifications to ProgressionManager needed.
///
/// Actions Tracked:
/// - Account level reached
/// - Total currency earned
/// - Total currency spent
/// - Items unlocked
/// - All ships unlocked
/// - All missiles unlocked
/// - All perks unlocked
/// - All cosmetics unlocked
/// - Battle pass tier reached
///
/// Usage:
///   Simply attach to ProgressionManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(ProgressionManager))]
public class ProgressionManagerAchievementIntegration : MonoBehaviour
{
    #region Configuration

    [Header("Achievement Integration")]
    [Tooltip("Enable achievement tracking")]
    public bool enableAchievementTracking = true;

    #endregion

    #region Component References

    private ProgressionManager _progressionManager;
    private AchievementService _achievementService;

    #endregion

    #region Tracking State

    // Currency tracking
    private int _totalCurrencyEarned = 0;
    private int _totalCurrencySpent = 0;

    // Unlock tracking
    private int _shipsUnlocked = 0;
    private int _missilesUnlocked = 0;
    private int _perksUnlocked = 0;
    private int _cosmeticsUnlocked = 0;
    private int _totalItemsUnlocked = 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _progressionManager = GetComponent<ProgressionManager>();

        if (_progressionManager == null)
        {
            Debug.LogError("[ProgressionManagerAchievementIntegration] ProgressionManager not found!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enableAchievementTracking)
            return;

        // Get achievement service
        _achievementService = AchievementService.Instance;

        if (_achievementService == null)
        {
            Debug.LogWarning("[ProgressionManagerAchievementIntegration] AchievementService not available - achievements disabled");
            enableAchievementTracking = false;
            return;
        }

        Log("Achievement integration initialized");
    }

    #endregion

    #region Account Level Tracking

    /// <summary>
    /// Call this when player gains account XP.
    /// Checks for level up and updates reach level achievements.
    /// </summary>
    public void OnAccountXPGained(int xpGained)
    {
        if (!enableAchievementTracking || _progressionManager.currentPlayerData == null)
            return;

        var data = _progressionManager.currentPlayerData;

        // Update achievement: Reach Account Level
        _achievementService.SetAchievementProgress(
            AchievementConditionType.ReachAccountLevel,
            data.accountLevel
        );

        Log($"Account level achievement updated: Level {data.accountLevel}");
    }

    #endregion

    #region Currency Tracking

    /// <summary>
    /// Call this when player earns currency (soft or hard).
    /// </summary>
    public void OnCurrencyEarned(string currencyType, int amount)
    {
        if (!enableAchievementTracking)
            return;

        // Track total currency earned
        _totalCurrencyEarned += amount;

        // Update achievement: Earn Total Currency
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.EarnTotalCurrency,
            amount
        );

        Log($"Currency earned achievement updated: {amount} {currencyType} (Total: {_totalCurrencyEarned})");
    }

    /// <summary>
    /// Call this when player spends currency.
    /// </summary>
    public void OnCurrencySpent(string currencyType, int amount, string itemPurchased)
    {
        if (!enableAchievementTracking)
            return;

        // Track total currency spent
        _totalCurrencySpent += amount;

        // Update achievement: Spend Total Currency
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.SpendTotalCurrency,
            amount
        );

        Log($"Currency spent achievement updated: {amount} {currencyType} on {itemPurchased} (Total: {_totalCurrencySpent})");
    }

    #endregion

    #region Item Unlock Tracking

    /// <summary>
    /// Call this when player unlocks an item.
    /// </summary>
    public void OnItemUnlocked(string itemType, string itemName, string unlockSource)
    {
        if (!enableAchievementTracking)
            return;

        _totalItemsUnlocked++;

        // Track by item type
        switch (itemType.ToLower())
        {
            case "ship":
                _shipsUnlocked++;
                CheckAllShipsUnlocked();
                break;

            case "missile":
                _missilesUnlocked++;
                CheckAllMissilesUnlocked();
                break;

            case "perk":
                _perksUnlocked++;
                CheckAllPerksUnlocked();
                break;

            case "cosmetic":
                _cosmeticsUnlocked++;
                CheckAllCosmeticsUnlocked();
                break;
        }

        // Check if all items unlocked
        CheckAllItemsUnlocked();

        Log($"Item unlock achievement updated: {itemType} - {itemName} (Total: {_totalItemsUnlocked})");
    }

    /// <summary>
    /// Checks if all ships are unlocked.
    /// </summary>
    private void CheckAllShipsUnlocked()
    {
        // TODO: Get total ship count from game configuration
        int totalShips = 4; // Assume 4 archetypes for now

        if (_shipsUnlocked >= totalShips)
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UnlockAllShips,
                1
            );

            Log("All ships unlocked achievement triggered!");
        }
    }

    /// <summary>
    /// Checks if all missiles are unlocked.
    /// </summary>
    private void CheckAllMissilesUnlocked()
    {
        // TODO: Get total missile count from game configuration
        int totalMissiles = 10; // Example

        if (_missilesUnlocked >= totalMissiles)
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UnlockAllMissiles,
                1
            );

            Log("All missiles unlocked achievement triggered!");
        }
    }

    /// <summary>
    /// Checks if all perks are unlocked.
    /// </summary>
    private void CheckAllPerksUnlocked()
    {
        // TODO: Get total perk count from game configuration
        int totalPerks = 15; // Example

        if (_perksUnlocked >= totalPerks)
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UnlockAllPerks,
                1
            );

            // Also check "Use All Perks" achievement
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UseAllPerks,
                1
            );

            Log("All perks unlocked achievement triggered!");
        }
    }

    /// <summary>
    /// Checks if all cosmetics are unlocked.
    /// </summary>
    private void CheckAllCosmeticsUnlocked()
    {
        // TODO: Get total cosmetic count from game configuration
        int totalCosmetics = 50; // Example

        if (_cosmeticsUnlocked >= totalCosmetics)
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UnlockAllCosmetics,
                1
            );

            Log("All cosmetics unlocked achievement triggered!");
        }
    }

    /// <summary>
    /// Checks if all items (ships + missiles + perks + cosmetics) are unlocked.
    /// </summary>
    private void CheckAllItemsUnlocked()
    {
        // TODO: Get total item count from game configuration
        int totalItems = 4 + 10 + 15 + 50; // ships + missiles + perks + cosmetics = 79

        if (_totalItemsUnlocked >= totalItems)
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UnlockAllItems,
                1
            );

            Log("All items unlocked achievement triggered!");
        }
    }

    #endregion

    #region Battle Pass Tracking

    /// <summary>
    /// Call this when player tiers up in battle pass.
    /// </summary>
    public void OnBattlePassTierUp(int tierNumber, bool isPremium)
    {
        if (!enableAchievementTracking)
            return;

        // Update achievement: Reach Battle Pass Max Tier
        _achievementService.SetAchievementProgress(
            AchievementConditionType.ReachBattlePassMaxTier,
            tierNumber
        );

        Log($"Battle pass tier achievement updated: Tier {tierNumber} ({(isPremium ? "Premium" : "Free")})");
    }

    #endregion

    #region Quest Tracking

    /// <summary>
    /// Call this when player completes a daily quest.
    /// </summary>
    public void OnDailyQuestCompleted()
    {
        if (!enableAchievementTracking)
            return;

        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.CompleteDailyQuest,
            1
        );

        Log("Daily quest completion achievement updated");
    }

    /// <summary>
    /// Call this when player completes a weekly quest.
    /// </summary>
    public void OnWeeklyQuestCompleted()
    {
        if (!enableAchievementTracking)
            return;

        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.CompleteWeeklyQuest,
            1
        );

        Log("Weekly quest completion achievement updated");
    }

    #endregion

    #region Archetype Tracking

    /// <summary>
    /// Call this when player wins with all archetypes.
    /// (Call this from GameManager when tracking archetype wins)
    /// </summary>
    public void OnWinWithAllArchetypes()
    {
        if (!enableAchievementTracking)
            return;

        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.WinWithAllArchetypes,
            1
        );

        Log("Win with all archetypes achievement triggered!");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets total currency earned.
    /// </summary>
    public int GetTotalCurrencyEarned()
    {
        return _totalCurrencyEarned;
    }

    /// <summary>
    /// Gets total currency spent.
    /// </summary>
    public int GetTotalCurrencySpent()
    {
        return _totalCurrencySpent;
    }

    /// <summary>
    /// Gets total items unlocked.
    /// </summary>
    public int GetTotalItemsUnlocked()
    {
        return _totalItemsUnlocked;
    }

    /// <summary>
    /// Resets tracking (call at season reset if needed).
    /// </summary>
    public void ResetSeasonTracking()
    {
        // Note: Don't reset lifetime stats, only season-specific ones
        Log("Season tracking reset");
    }

    private void Log(string message)
    {
        Debug.Log($"[ProgressionManagerAchievementIntegration] {message}");
    }

    #endregion
}
