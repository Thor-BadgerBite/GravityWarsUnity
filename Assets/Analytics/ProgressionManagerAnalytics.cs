using UnityEngine;
using GravityWars.Networking;

/// <summary>
/// Analytics integration for ProgressionManager.
/// Tracks all progression-related events and economy transactions.
///
/// IMPORTANT: Attach this component to the same GameObject as ProgressionManager.
/// It will automatically hook into progression events and send analytics.
///
/// Events Tracked:
/// - Account level up
/// - Ship level up
/// - Item unlocks
/// - Currency earned/spent
/// - Battle pass tier up
/// - Loadout created/equipped
///
/// Usage:
///   Simply attach to ProgressionManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(ProgressionManager))]
public class ProgressionManagerAnalytics : MonoBehaviour
{
    #region Configuration

    [Header("Analytics Configuration")]
    [Tooltip("Enable analytics tracking")]
    public bool enableAnalytics = true;

    #endregion

    #region Component References

    private ProgressionManager _progressionManager;
    private AnalyticsService _analyticsService;

    #endregion

    #region State Tracking

    private int _lastAccountLevel = 1;
    private float _lastLevelUpTime = 0f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _progressionManager = GetComponent<ProgressionManager>();

        if (_progressionManager == null)
        {
            Debug.LogError("[ProgressionManagerAnalytics] ProgressionManager not found!");
            enabled = false;
            return;
        }

        // Get analytics service
        _analyticsService = ServiceLocator.Instance?.Analytics;

        if (_analyticsService == null)
        {
            Debug.LogWarning("[ProgressionManagerAnalytics] AnalyticsService not available - analytics disabled");
            enableAnalytics = false;
        }
    }

    private void Start()
    {
        if (!enableAnalytics)
            return;

        // Initialize tracking state
        if (_progressionManager.currentPlayerData != null)
        {
            _lastAccountLevel = _progressionManager.currentPlayerData.level;
            _lastLevelUpTime = Time.time;
        }

        Log("Analytics integration initialized");
    }

    #endregion

    #region Account Progression Tracking

    /// <summary>
    /// Call this when player gains account XP.
    /// Checks for level up and tracks it.
    /// </summary>
    public void TrackAccountXPGain(int xpGained, string source)
    {
        if (!enableAnalytics || _progressionManager.currentPlayerData == null)
            return;

        var data = _progressionManager.currentPlayerData;

        // Check if leveled up
        if (data.level > _lastAccountLevel)
        {
            // Calculate time since last level
            float timeSinceLastLevel = Time.time - _lastLevelUpTime;

            _analyticsService.TrackAccountLevelUp(
                newLevel: data.level,
                xpSource: source,
                timeSinceLastLevel: timeSinceLastLevel
            );

            Log($"Account level up tracked: Level {data.level} (from {source})");

            // Update tracking state
            _lastAccountLevel = data.level;
            _lastLevelUpTime = Time.time;
        }
    }

    /// <summary>
    /// Call this when a ship gains XP and levels up.
    /// </summary>
    public void TrackShipLevelUp(string loadoutKey, int newLevel, int matchesPlayed)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackShipLevelUp(
            loadoutKey: loadoutKey,
            newLevel: newLevel,
            matchesPlayed: matchesPlayed
        );

        Log($"Ship level up tracked: {loadoutKey} -> Level {newLevel}");
    }

    #endregion

    #region Item Unlock Tracking

    /// <summary>
    /// Call this when player unlocks an item.
    /// </summary>
    public void TrackItemUnlock(string itemType, string itemName, string unlockSource)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackItemUnlocked(
            itemType: itemType,
            itemName: itemName,
            unlockSource: unlockSource
        );

        Log($"Item unlock tracked: {itemType} - {itemName} (from {unlockSource})");
    }

    #endregion

    #region Currency Tracking

    /// <summary>
    /// Call this when player earns currency.
    /// </summary>
    public void TrackCurrencyEarned(string currencyType, int amount, string source)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackCurrencyEarned(
            currencyType: currencyType,
            amount: amount,
            source: source
        );

        Log($"Currency earned tracked: {amount} {currencyType} (from {source})");
    }

    /// <summary>
    /// Call this when player spends currency.
    /// </summary>
    public void TrackCurrencySpent(string currencyType, int amount, string itemPurchased)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackCurrencySpent(
            currencyType: currencyType,
            amount: amount,
            itemPurchased: itemPurchased
        );

        Log($"Currency spent tracked: {amount} {currencyType} (on {itemPurchased})");
    }

    #endregion

    #region Battle Pass Tracking

    /// <summary>
    /// Call this when player tiers up in battle pass.
    /// </summary>
    public void TrackBattlePassTierUp(int tierNumber, bool isPremium, string reward)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackBattlePassTierUp(
            tierNumber: tierNumber,
            isPremium: isPremium,
            reward: reward
        );

        Log($"Battle pass tier up tracked: Tier {tierNumber} ({(isPremium ? "Premium" : "Free")})");
    }

    #endregion

    #region Loadout Tracking

    /// <summary>
    /// Call this when player creates a custom loadout.
    /// </summary>
    public void TrackLoadoutCreated(CustomShipLoadout loadout)
    {
        if (!enableAnalytics || loadout == null)
            return;

        _analyticsService.TrackLoadoutCreated(
            loadoutID: loadout.loadoutID,
            shipBody: loadout.shipBodyName,
            tier1Perk: loadout.tier1PerkName,
            tier2Perk: loadout.tier2PerkName,
            tier3Perk: loadout.tier3PerkName
        );

        Log($"Loadout created tracked: {loadout.loadoutName}");
    }

    /// <summary>
    /// Call this when player equips a loadout.
    /// </summary>
    public void TrackLoadoutEquipped(string loadoutID, string previousLoadout)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackLoadoutEquipped(
            loadoutID: loadoutID,
            previousLoadout: previousLoadout
        );

        Log($"Loadout equipped tracked: {loadoutID}");
    }

    /// <summary>
    /// Call this when player changes missile on a loadout.
    /// </summary>
    public void TrackMissileChanged(string loadoutID, string oldMissile, string newMissile)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackMissileChanged(
            loadoutID: loadoutID,
            oldMissile: oldMissile,
            newMissile: newMissile
        );

        Log($"Missile changed tracked: {oldMissile} -> {newMissile}");
    }

    #endregion

    #region Helper Methods

    private void Log(string message)
    {
        Debug.Log($"[ProgressionManagerAnalytics] {message}");
    }

    #endregion
}
