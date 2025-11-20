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

    [Header("Account Progression")]
    public int accountLevel = 1;
    public int accountXP = 0;

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

    [Header("Settings & Preferences")]
    public PlayerPreferences preferences = new PlayerPreferences();

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
        accountXP += amount;
        Debug.Log($"[PlayerAccountData] +{amount} Account XP (Total: {accountXP})");
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
}
