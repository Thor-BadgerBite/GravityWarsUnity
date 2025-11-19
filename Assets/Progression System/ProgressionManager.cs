using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for all progression systems (unlocks, XP, battle pass, currency).
/// This is a singleton that persists across scenes.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Player Data")]
    public PlayerAccountData currentPlayerData;

    [Header("Battle Pass References")]
    [Tooltip("The permanent free battle pass (account progression)")]
    public BattlePassData freeBattlePass;

    [Tooltip("The current seasonal premium battle pass")]
    public BattlePassData seasonalBattlePass;

    [Header("Content Databases")]
    [Tooltip("All available ship bodies in the game")]
    public List<ShipBodySO> allShipBodies = new List<ShipBodySO>();

    [Tooltip("All available perks (Tier 1/2/3)")]
    public List<ActivePerkSO> allPerks = new List<ActivePerkSO>();

    [Tooltip("All available passives")]
    public List<PassiveAbilitySO> allPassives = new List<PassiveAbilitySO>();

    [Tooltip("All available move types")]
    public List<MoveTypeSO> allMoveTypes = new List<MoveTypeSO>();

    [Tooltip("All available missiles")]
    public List<MissilePresetSO> allMissiles = new List<MissilePresetSO>();

    [Header("Save/Load")]
    [Tooltip("Auto-save after every change?")]
    public bool autoSave = true;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes the progression system
    /// </summary>
    private void Initialize()
    {
        // Try to load existing player data
        currentPlayerData = SaveSystem.LoadPlayerData();

        if (currentPlayerData == null)
        {
            // Create new account
            Debug.Log("[ProgressionManager] No save data found, creating new account");
            CreateNewAccount("Player", "player_" + System.Guid.NewGuid().ToString());
        }
        else
        {
            Debug.Log($"[ProgressionManager] Loaded account: {currentPlayerData.displayName} (Level {currentPlayerData.accountLevel})");
            currentPlayerData.UpdateLastLogin();
        }

        // Initialize content databases if empty (auto-populate from Resources)
        if (allShipBodies.Count == 0)
            PopulateContentDatabases();
    }

    /// <summary>
    /// Creates a new player account
    /// </summary>
    public void CreateNewAccount(string displayName, string playerID)
    {
        currentPlayerData = new PlayerAccountData(playerID, displayName);

        // Grant starter unlocks
        GrantStarterContent();

        Save();
    }

    /// <summary>
    /// Grants default starter content to new players
    /// </summary>
    private void GrantStarterContent()
    {
        // Unlock starter ship bodies (1 of each archetype for testing)
        // In production, you'd unlock only 1-2 starter ships
        foreach (var body in allShipBodies)
        {
            if (body.name.Contains("Starter") || body.requiredAccountLevel == 0)
            {
                UnlockItem(body);
            }
        }

        // Unlock all move types (Normal is always available, others unlocked later)
        foreach (var moveType in allMoveTypes)
        {
            if (moveType.category == MoveTypeCategory.Normal)
            {
                UnlockItem(moveType);
            }
        }

        // Unlock starter missiles
        foreach (var missile in allMissiles)
        {
            if (missile.missileType == MissileType.Medium) // Start with Medium missiles
            {
                UnlockItem(missile);
                break;
            }
        }

        // Grant starting currency
        currentPlayerData.AddCurrency(1000, 50); // 1000 coins, 50 gems

        Debug.Log("[ProgressionManager] Granted starter content to new account");
    }

    #region UNLOCKS

    /// <summary>
    /// Checks if an item is unlocked
    /// </summary>
    public bool IsUnlocked(ScriptableObject item)
    {
        return currentPlayerData.IsUnlocked(item);
    }

    /// <summary>
    /// Unlocks an item and saves
    /// </summary>
    public void UnlockItem(ScriptableObject item)
    {
        if (item == null) return;

        currentPlayerData.UnlockItem(item);

        if (autoSave)
            Save();
    }

    /// <summary>
    /// Gets all unlocked ship bodies
    /// </summary>
    public List<ShipBodySO> GetUnlockedShipBodies()
    {
        List<ShipBodySO> unlocked = new List<ShipBodySO>();
        foreach (var body in allShipBodies)
        {
            if (IsUnlocked(body))
                unlocked.Add(body);
        }
        return unlocked;
    }

    /// <summary>
    /// Gets all unlocked perks of a specific tier
    /// </summary>
    public List<ActivePerkSO> GetUnlockedPerks(int tier)
    {
        List<ActivePerkSO> unlocked = new List<ActivePerkSO>();
        foreach (var perk in allPerks)
        {
            if (perk.tier == tier && IsUnlocked(perk))
                unlocked.Add(perk);
        }
        return unlocked;
    }

    /// <summary>
    /// Gets all unlocked passives
    /// </summary>
    public List<PassiveAbilitySO> GetUnlockedPassives()
    {
        List<PassiveAbilitySO> unlocked = new List<PassiveAbilitySO>();
        foreach (var passive in allPassives)
        {
            if (IsUnlocked(passive))
                unlocked.Add(passive);
        }
        return unlocked;
    }

    #endregion

    #region XP & LEVELING

    /// <summary>
    /// Awards XP after a match
    /// </summary>
    public void AwardMatchXP(bool won, int roundsWon, int damageDealt, CustomShipLoadout usedLoadout)
    {
        // Calculate XP amounts
        int baseXP = 50;
        int winBonus = won ? 100 : 0;
        int roundBonus = roundsWon * 25;
        int damageBonus = Mathf.FloorToInt(damageDealt / 100f); // 1 XP per 100 damage

        int totalAccountXP = baseXP + winBonus + roundBonus + damageBonus;
        int totalShipXP = totalAccountXP; // Same for now, can be different

        // Apply premium pass bonus (e.g., +50% XP)
        if (currentPlayerData.hasPremiumBattlePass)
        {
            int bonus = Mathf.FloorToInt(totalAccountXP * 0.5f);
            totalAccountXP += bonus;
            totalShipXP += bonus;
            Debug.Log($"[ProgressionManager] Premium Pass Bonus: +{bonus} XP");
        }

        // Award account XP
        currentPlayerData.AddAccountXP(totalAccountXP);
        CheckAccountLevelUp();

        // Award ship XP
        if (usedLoadout != null)
        {
            currentPlayerData.AddShipXP(usedLoadout, totalShipXP);
        }

        // Award battle pass XP
        currentPlayerData.battlePassXP += totalAccountXP;
        CheckBattlePassTierUp();

        // Update stats
        currentPlayerData.totalMatchesPlayed++;
        if (won) currentPlayerData.totalMatchesWon++;
        currentPlayerData.totalRoundsWon += roundsWon;
        currentPlayerData.totalDamageDealt += damageDealt;

        Debug.Log($"[ProgressionManager] Match XP: Account +{totalAccountXP}, Ship +{totalShipXP}");

        if (autoSave)
            Save();
    }

    /// <summary>
    /// Checks if account leveled up and grants rewards
    /// </summary>
    private void CheckAccountLevelUp()
    {
        // Account level formula (simple linear for now, can be quadratic like ships)
        int xpForNextLevel = 1000 + (currentPlayerData.accountLevel * 500);

        while (currentPlayerData.accountXP >= xpForNextLevel && currentPlayerData.accountLevel < 50)
        {
            currentPlayerData.accountLevel++;
            Debug.Log($"[ProgressionManager] ACCOUNT LEVEL UP! Now Level {currentPlayerData.accountLevel}");

            // Grant level-up rewards (check free battle pass)
            GrantAccountLevelRewards(currentPlayerData.accountLevel);

            xpForNextLevel = 1000 + (currentPlayerData.accountLevel * 500);
        }
    }

    /// <summary>
    /// Grants rewards for reaching an account level (from free battle pass)
    /// </summary>
    private void GrantAccountLevelRewards(int level)
    {
        if (freeBattlePass == null) return;

        // Check if this level matches a battle pass tier
        var tier = freeBattlePass.GetTier(level - 1); // 0-indexed
        if (tier != null && tier.freeReward.HasReward())
        {
            GrantReward(tier.freeReward);
        }
    }

    /// <summary>
    /// Checks if player unlocked new battle pass tier
    /// </summary>
    private void CheckBattlePassTierUp()
    {
        if (seasonalBattlePass == null || !seasonalBattlePass.IsActive())
            return;

        int newTier = seasonalBattlePass.GetTierFromXP(currentPlayerData.battlePassXP);

        if (newTier > currentPlayerData.battlePassTier)
        {
            Debug.Log($"[ProgressionManager] BATTLE PASS TIER UP! Now Tier {newTier + 1}");

            // Grant rewards for all tiers between old and new
            for (int i = currentPlayerData.battlePassTier + 1; i <= newTier; i++)
            {
                GrantBattlePassTierRewards(i);
            }

            currentPlayerData.battlePassTier = newTier;
        }
    }

    /// <summary>
    /// Grants rewards for a battle pass tier
    /// </summary>
    private void GrantBattlePassTierRewards(int tierIndex)
    {
        if (seasonalBattlePass == null) return;

        var tier = seasonalBattlePass.GetTier(tierIndex);
        if (tier == null) return;

        // Always grant free reward
        if (tier.freeReward.HasReward())
        {
            Debug.Log($"[ProgressionManager] Battle Pass Tier {tierIndex + 1} - Free Reward");
            GrantReward(tier.freeReward);
        }

        // Grant premium reward if player has premium pass
        if (currentPlayerData.hasPremiumBattlePass && tier.premiumReward.HasReward())
        {
            Debug.Log($"[ProgressionManager] Battle Pass Tier {tierIndex + 1} - Premium Reward");
            GrantReward(tier.premiumReward);
        }
    }

    /// <summary>
    /// Grants a reward to the player
    /// </summary>
    private void GrantReward(UnlockableReward reward)
    {
        // Unlock item
        if (reward.rewardItem != null)
        {
            UnlockItem(reward.rewardItem);
        }

        // Grant currency
        if (reward.softCurrencyAmount > 0 || reward.hardCurrencyAmount > 0)
        {
            currentPlayerData.AddCurrency(reward.softCurrencyAmount, reward.hardCurrencyAmount);
        }

        // Grant XP
        if (reward.accountXP > 0)
        {
            currentPlayerData.AddAccountXP(reward.accountXP);
        }

        // Unlock cosmetics
        if (!string.IsNullOrEmpty(reward.skinID))
            currentPlayerData.unlockedSkinIDs.Add(reward.skinID);
        if (!string.IsNullOrEmpty(reward.colorSchemeID))
            currentPlayerData.unlockedColorSchemeIDs.Add(reward.colorSchemeID);
        if (!string.IsNullOrEmpty(reward.decalID))
            currentPlayerData.unlockedDecalIDs.Add(reward.decalID);

        Debug.Log($"[ProgressionManager] Granted reward: {reward.GetDisplayText()}");
    }

    #endregion

    #region CURRENCY

    /// <summary>
    /// Checks if player can afford a purchase
    /// </summary>
    public bool CanAfford(int softCost, int hardCost)
    {
        return currentPlayerData.softCurrency >= softCost &&
               currentPlayerData.hardCurrency >= hardCost;
    }

    /// <summary>
    /// Spends currency
    /// </summary>
    public bool SpendCurrency(int softCost, int hardCost)
    {
        if (!CanAfford(softCost, hardCost))
        {
            Debug.LogWarning("[ProgressionManager] Insufficient funds!");
            return false;
        }

        currentPlayerData.softCurrency -= softCost;
        currentPlayerData.hardCurrency -= hardCost;

        Debug.Log($"[ProgressionManager] Spent: {softCost} coins, {hardCost} gems");

        if (autoSave)
            Save();

        return true;
    }

    /// <summary>
    /// Purchases premium battle pass
    /// </summary>
    public bool PurchasePremiumBattlePass(int gemCost)
    {
        if (currentPlayerData.hasPremiumBattlePass)
        {
            Debug.LogWarning("[ProgressionManager] Already owns premium battle pass!");
            return false;
        }

        if (SpendCurrency(0, gemCost))
        {
            currentPlayerData.hasPremiumBattlePass = true;
            Debug.Log("[ProgressionManager] Premium Battle Pass purchased!");

            // Grant all premium rewards for already-unlocked tiers
            for (int i = 0; i <= currentPlayerData.battlePassTier; i++)
            {
                var tier = seasonalBattlePass.GetTier(i);
                if (tier != null && tier.premiumReward.HasReward())
                {
                    GrantReward(tier.premiumReward);
                }
            }

            if (autoSave)
                Save();

            return true;
        }

        return false;
    }

    #endregion

    #region CUSTOM LOADOUTS

    /// <summary>
    /// Creates and saves a custom ship loadout
    /// </summary>
    public CustomShipLoadout CreateCustomLoadout(
        string loadoutName,
        ShipBodySO body,
        MoveTypeSO moveType,
        MissilePresetSO missile,
        ActivePerkSO tier1Perk = null,
        ActivePerkSO tier2Perk = null,
        ActivePerkSO tier3Perk = null,
        List<PassiveAbilitySO> passives = null)
    {
        // Validate all components
        if (!ValidateLoadout(body, moveType, missile, tier1Perk, tier2Perk, tier3Perk, passives))
        {
            Debug.LogError("[ProgressionManager] Invalid loadout configuration!");
            return null;
        }

        // Create loadout
        CustomShipLoadout loadout = new CustomShipLoadout
        {
            loadoutID = CustomShipLoadout.GenerateLoadoutID(),
            loadoutName = loadoutName,
            shipBodyName = body.name,
            moveTypeName = moveType.name,
            equippedMissileName = missile.name,
            tier1PerkName = tier1Perk?.name ?? "",
            tier2PerkName = tier2Perk?.name ?? "",
            tier3PerkName = tier3Perk?.name ?? "",
            passiveNames = new List<string>()
        };

        if (passives != null)
        {
            foreach (var passive in passives)
            {
                if (passive != null)
                    loadout.passiveNames.Add(passive.name);
            }
        }

        // Add to player's loadouts
        currentPlayerData.customShipLoadouts.Add(loadout);

        Debug.Log($"[ProgressionManager] Created loadout: {loadoutName}");

        if (autoSave)
            Save();

        return loadout;
    }

    /// <summary>
    /// Validates a loadout configuration
    /// </summary>
    private bool ValidateLoadout(
        ShipBodySO body,
        MoveTypeSO moveType,
        MissilePresetSO missile,
        ActivePerkSO tier1Perk,
        ActivePerkSO tier2Perk,
        ActivePerkSO tier3Perk,
        List<PassiveAbilitySO> passives)
    {
        if (body == null || moveType == null || missile == null)
        {
            Debug.LogError("[ProgressionManager] Body, move type, and missile are required!");
            return false;
        }

        // Check if unlocked
        if (!IsUnlocked(body) || !IsUnlocked(moveType) || !IsUnlocked(missile))
        {
            Debug.LogError("[ProgressionManager] Some components are locked!");
            return false;
        }

        // Check archetype compatibility
        ShipArchetype archetype = body.archetype;

        if (!moveType.CanBeUsedBy(archetype))
        {
            Debug.LogError($"[ProgressionManager] {moveType.name} cannot be used by {archetype}!");
            return false;
        }

        if (!body.CanUseMissileType(missile.missileType))
        {
            Debug.LogError($"[ProgressionManager] {body.name} cannot use {missile.missileType} missiles!");
            return false;
        }

        // Check perks
        if (tier1Perk != null && (!tier1Perk.CanBeUsedBy(archetype) || !IsUnlocked(tier1Perk)))
            return false;
        if (tier2Perk != null && (!tier2Perk.CanBeUsedBy(archetype) || !IsUnlocked(tier2Perk)))
            return false;
        if (tier3Perk != null && (!tier3Perk.CanBeUsedBy(archetype) || !IsUnlocked(tier3Perk)))
            return false;

        // Check passives
        if (passives != null)
        {
            foreach (var passive in passives)
            {
                if (passive != null && (!passive.CanBeUsedBy(archetype) || !IsUnlocked(passive)))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Deletes a custom loadout
    /// </summary>
    public void DeleteLoadout(string loadoutID)
    {
        currentPlayerData.customShipLoadouts.RemoveAll(l => l.loadoutID == loadoutID);

        if (autoSave)
            Save();
    }

    #endregion

    #region SAVE/LOAD

    /// <summary>
    /// Saves current player data
    /// </summary>
    public void Save()
    {
        SaveSystem.SavePlayerData(currentPlayerData);
    }

    /// <summary>
    /// Loads player data
    /// </summary>
    public void Load()
    {
        currentPlayerData = SaveSystem.LoadPlayerData();
        if (currentPlayerData == null)
        {
            Debug.LogWarning("[ProgressionManager] No save data found!");
        }
    }

    #endregion

    #region CONTENT DATABASE

    /// <summary>
    /// Auto-populates content databases from Resources folder
    /// </summary>
    private void PopulateContentDatabases()
    {
        // Load all ScriptableObjects from Resources (or you can manually assign in Inspector)
        allShipBodies.AddRange(Resources.LoadAll<ShipBodySO>(""));
        allPerks.AddRange(Resources.LoadAll<ActivePerkSO>(""));
        allPassives.AddRange(Resources.LoadAll<PassiveAbilitySO>(""));
        allMoveTypes.AddRange(Resources.LoadAll<MoveTypeSO>(""));
        allMissiles.AddRange(Resources.LoadAll<MissilePresetSO>(""));

        Debug.Log($"[ProgressionManager] Loaded content: {allShipBodies.Count} bodies, {allPerks.Count} perks, {allPassives.Count} passives");
    }

    #endregion
}
