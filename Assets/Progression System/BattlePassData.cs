using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a battle pass with tiers and rewards (free + premium tracks).
/// Create instances via: Right-click -> Create -> GravityWars -> Progression -> Battle Pass
/// </summary>
[CreateAssetMenu(fileName = "BattlePass", menuName = "GravityWars/Progression/Battle Pass")]
public class BattlePassData : ScriptableObject
{
    [Header("Battle Pass Info")]
    [Tooltip("Unique identifier for this battle pass")]
    public string battlePassID;

    [Tooltip("Display name (e.g., 'Season 1: Nebula Storm')")]
    public string displayName;

    // Legacy alias for display name
    public string username => displayName;

    [Tooltip("Is this a seasonal pass (resets) or permanent free pass?")]
    public bool isSeasonal = true;

    [Tooltip("Season number (for seasonal passes)")]
    public int seasonNumber = 1;

    [Header("Duration (Seasonal Only)")]
    [Tooltip("Season start date (leave default for permanent passes)")]
    public string seasonStartDate = "2025-01-01";

    [Tooltip("Season end date (leave default for permanent passes)")]
    public string seasonEndDate = "2025-03-01";

    [Header("Tiers")]
    [Tooltip("All tiers in this battle pass (50-70 recommended)")]
    public BattlePassTier[] tiers;

    [Header("Preview")]
    [Tooltip("Icon for this battle pass")]
    public Sprite icon;

    [TextArea(3, 5)]
    public string description;

    /// <summary>
    /// Checks if this seasonal pass is currently active
    /// </summary>
    public bool IsActive()
    {
        if (!isSeasonal) return true; // Permanent passes always active

        try
        {
            DateTime start = DateTime.Parse(seasonStartDate);
            DateTime end = DateTime.Parse(seasonEndDate);
            DateTime now = DateTime.Now;

            return now >= start && now <= end;
        }
        catch
        {
            Debug.LogWarning($"[BattlePassData] Invalid date format for {displayName}");
            return false;
        }
    }

    /// <summary>
    /// Gets the tier at a specific index (0-based)
    /// </summary>
    public BattlePassTier GetTier(int tierIndex)
    {
        if (tierIndex < 0 || tierIndex >= tiers.Length)
            return null;

        return tiers[tierIndex];
    }

    /// <summary>
    /// Gets total number of tiers
    /// </summary>
    public int GetTierCount()
    {
        return tiers != null ? tiers.Length : 0;
    }

    /// <summary>
    /// Calculates which tier the player is on based on XP
    /// </summary>
    public int GetTierFromXP(int xp)
    {
        for (int i = 0; i < tiers.Length; i++)
        {
            if (xp < tiers[i].xpRequired)
                return Mathf.Max(0, i - 1); // Previous tier
        }

        return tiers.Length - 1; // Max tier
    }

    /// <summary>
    /// Gets XP required for next tier
    /// </summary>
    public int GetXPForNextTier(int currentTier)
    {
        if (currentTier >= tiers.Length - 1)
            return tiers[tiers.Length - 1].xpRequired; // Max tier

        return tiers[currentTier + 1].xpRequired;
    }

    /// <summary>
    /// Validates all tiers on Inspector changes
    /// </summary>
    void OnValidate()
    {
        if (string.IsNullOrEmpty(battlePassID))
            battlePassID = name;

        ValidateTiers();
    }

    private void ValidateTiers()
    {
        if (tiers == null || tiers.Length == 0)
        {
            Debug.LogWarning($"[{name}] No tiers defined!");
            return;
        }

        // Check tier numbering
        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].tierNumber != i + 1)
            {
                Debug.LogWarning($"[{name}] Tier {i} has incorrect tierNumber: {tiers[i].tierNumber} (should be {i + 1})");
                tiers[i].tierNumber = i + 1;
            }
        }

        // Check XP progression
        for (int i = 1; i < tiers.Length; i++)
        {
            if (tiers[i].xpRequired <= tiers[i - 1].xpRequired)
            {
                Debug.LogWarning($"[{name}] Tier {i + 1} XP ({tiers[i].xpRequired}) must be greater than Tier {i} XP ({tiers[i - 1].xpRequired})!");
            }
        }
    }
}

/// <summary>
/// Represents a single tier in a battle pass with free and premium rewards
/// </summary>
[System.Serializable]
public class BattlePassTier
{
    [Header("Tier Info")]
    [Tooltip("Tier number (1, 2, 3, etc.)")]
    public int tierNumber = 1;

    [Tooltip("Total XP required to reach this tier")]
    public int xpRequired = 100;

    [Header("Rewards")]
    [Tooltip("Free reward (everyone gets this)")]
    public UnlockableReward freeReward;

    [Tooltip("Premium reward (only premium pass holders get this)")]
    public UnlockableReward premiumReward;

    /// <summary>
    /// Checks if this tier has any rewards
    /// </summary>
    public bool HasRewards()
    {
        return freeReward.HasReward() || premiumReward.HasReward();
    }
}

/// <summary>
/// Represents a reward that can be unlocked (item + currency)
/// </summary>
[System.Serializable]
public class UnlockableReward
{
    [Header("Item Reward")]
    [Tooltip("Type of item to unlock")]
    public RewardType rewardType = RewardType.None;

    [Tooltip("The actual ScriptableObject to unlock (ShipBodySO, ActivePerkSO, etc.)")]
    public ScriptableObject rewardItem;

    [Header("Currency Rewards")]
    [Tooltip("Soft currency (coins) to award")]
    public int softCurrencyAmount = 0;

    [Tooltip("Hard currency (gems) to award")]
    public int hardCurrencyAmount = 0;

    [Header("XP Rewards")]
    [Tooltip("Account XP to award")]
    public int accountXP = 0;

    [Tooltip("Ship XP to award (applies to equipped ship)")]
    public int shipXP = 0;

    [Header("Cosmetic Rewards")]
    [Tooltip("Skin ID to unlock (if rewardType is Skin)")]
    public string skinID;

    [Tooltip("Color scheme ID (if rewardType is ColorScheme)")]
    public string colorSchemeID;

    [Tooltip("Decal ID (if rewardType is Decal)")]
    public string decalID;

    /// <summary>
    /// Checks if this reward has anything to give
    /// </summary>
    public bool HasReward()
    {
        return rewardType != RewardType.None ||
               softCurrencyAmount > 0 ||
               hardCurrencyAmount > 0 ||
               accountXP > 0 ||
               shipXP > 0;
    }

    /// <summary>
    /// Gets a display string for UI
    /// </summary>
    public string GetDisplayText()
    {
        List<string> parts = new List<string>();

        // Item
        if (rewardItem != null)
        {
            string itemName = rewardItem.name;
            parts.Add($"{rewardType}: {itemName}");
        }

        // Currency
        if (softCurrencyAmount > 0)
            parts.Add($"{softCurrencyAmount} Coins");
        if (hardCurrencyAmount > 0)
            parts.Add($"{hardCurrencyAmount} Gems");

        // XP
        if (accountXP > 0)
            parts.Add($"{accountXP} Account XP");
        if (shipXP > 0)
            parts.Add($"{shipXP} Ship XP");

        // Cosmetics
        if (!string.IsNullOrEmpty(skinID))
            parts.Add($"Skin: {skinID}");
        if (!string.IsNullOrEmpty(colorSchemeID))
            parts.Add($"Color: {colorSchemeID}");
        if (!string.IsNullOrEmpty(decalID))
            parts.Add($"Decal: {decalID}");

        return parts.Count > 0 ? string.Join(", ", parts) : "No Reward";
    }
}

/// <summary>
/// Types of rewards that can be unlocked
/// </summary>
public enum RewardType
{
    None,
    ShipBody,
    Tier1Perk,
    Tier2Perk,
    Tier3Perk,
    Passive,
    MoveType,
    Missile,
    Skin,
    ColorScheme,
    Decal,
    Credits,
    Gems,
    PrebuildShip,
    Active
}
