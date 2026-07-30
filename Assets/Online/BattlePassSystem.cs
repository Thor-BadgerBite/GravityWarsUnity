using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Battle Pass system - seasonal progression with free and premium tracks.
/// 25 levels total, resets each season (typically 3 months).
///
/// FREE TRACK: Basic rewards (lower-tier ships, skins, currencies)
/// PREMIUM TRACK: Premium rewards (exclusive ships, rare skins, more currencies)
///
/// Unlocks same categories as account XP plus SKINS!
/// </summary>
public class BattlePassSystem : MonoBehaviour
{
    #region Singleton

    public static BattlePassSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Restore battle pass state when a profile becomes available
        if (AccountSystem.Instance != null)
        {
            AccountSystem.Instance.OnLoginSuccess += LoadFromProfile;
            if (AccountSystem.Instance.CurrentPlayerProfile != null)
                LoadFromProfile(AccountSystem.Instance.CurrentPlayerProfile);
        }
        else if (ProgressionManager.Instance != null && ProgressionManager.Instance.currentPlayerData != null)
        {
            // Offline / local play - restore from local save
            LoadFromProfile(ProgressionManager.Instance.currentPlayerData);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this && AccountSystem.Instance != null)
        {
            AccountSystem.Instance.OnLoginSuccess -= LoadFromProfile;
        }
    }

    #endregion

    #region Configuration

    [Header("Battle Pass Settings")]
    [SerializeField] private int maxLevel = 25;
    [SerializeField] private int xpPerLevel = 1000;  // XP needed per level
    [SerializeField] private bool isPremiumUnlocked = false;

    [Header("Season Info")]
    [SerializeField] private int currentSeason = 1;
    [SerializeField] private string seasonName = "Season 1: Cosmic Dawn";
    [SerializeField] private long seasonEndTimestamp;

    #endregion

    #region State

    private int _currentLevel = 0;
    private int _currentXP = 0;
    private List<int> _claimedFreeRewards = new List<int>();
    private List<int> _claimedPremiumRewards = new List<int>();

    #endregion

    #region Battle Pass Rewards Data

    /// <summary>
    /// Free track rewards (available to all players).
    /// </summary>
    private static readonly Dictionary<int, BattlePassReward> FREE_TRACK_REWARDS = new Dictionary<int, BattlePassReward>
    {
        // Early Levels (1-5)
        { 1, new BattlePassReward(RewardType.Credits, 500, "Credits x500") },
        { 2, new BattlePassReward(RewardType.Skin, 0, "Starter Skin: Blue Wave", "skin_starter_blue") },
        { 3, new BattlePassReward(RewardType.Passive, 0, "Passive: Speed Boost I", "passive_speed_boost_1") },
        { 4, new BattlePassReward(RewardType.Credits, 750, "Credits x750") },
        { 5, new BattlePassReward(RewardType.Missile, 0, "Missile: Standard Mk-II", "standard_mk2") },

        // Mid Levels (6-15)
        { 6, new BattlePassReward(RewardType.Credits, 1000, "Credits x1000") },
        { 7, new BattlePassReward(RewardType.Skin, 0, "Tank Skin: Iron Fortress", "skin_tank_iron") },
        { 8, new BattlePassReward(RewardType.Active, 0, "Active: Emergency Repair", "active_repair") },
        { 9, new BattlePassReward(RewardType.Credits, 1250, "Credits x1250") },
        { 10, new BattlePassReward(RewardType.PrebuildShip, 0, "Ship: Seasonal Scout", "seasonal_scout_free", ShipClass.AllAround) },
        { 11, new BattlePassReward(RewardType.Credits, 1500, "Credits x1500") },
        { 12, new BattlePassReward(RewardType.Skin, 0, "DD Skin: Crimson Strike", "skin_dd_crimson") },
        { 13, new BattlePassReward(RewardType.Passive, 0, "Passive: Armor Boost II", "passive_armor_boost_2") },
        { 14, new BattlePassReward(RewardType.Credits, 1750, "Credits x1750") },
        { 15, new BattlePassReward(RewardType.Missile, 0, "Missile: Light Vortex", "light_vortex") },

        // Late Levels (16-25)
        { 16, new BattlePassReward(RewardType.Credits, 2000, "Credits x2000") },
        { 17, new BattlePassReward(RewardType.Skin, 0, "Controller Skin: Shadow Ops", "skin_ctrl_shadow") },
        { 18, new BattlePassReward(RewardType.Active, 0, "Active: EMP Pulse", "active_emp_pulse") },
        { 19, new BattlePassReward(RewardType.Credits, 2500, "Credits x2500") },
        { 20, new BattlePassReward(RewardType.PrebuildShip, 0, "Ship: Seasonal Defender", "seasonal_defender_free", ShipClass.Tank) },
        { 21, new BattlePassReward(RewardType.Credits, 3000, "Credits x3000") },
        { 22, new BattlePassReward(RewardType.Skin, 0, "All-Around Skin: Golden Eagle", "skin_allaround_gold") },
        { 23, new BattlePassReward(RewardType.Gems, 50, "Gems x50") },
        { 24, new BattlePassReward(RewardType.Credits, 5000, "Credits x5000") },
        { 25, new BattlePassReward(RewardType.ShipBody, 0, "Ship Body: Seasonal Frame", "body_seasonal_standard", ShipClass.AllAround) }
    };

    /// <summary>
    /// Premium track rewards (requires premium battle pass purchase).
    /// Higher quality rewards including exclusive ships and skins.
    /// </summary>
    private static readonly Dictionary<int, BattlePassReward> PREMIUM_TRACK_REWARDS = new Dictionary<int, BattlePassReward>
    {
        // Early Levels (1-5)
        { 1, new BattlePassReward(RewardType.Credits, 1000, "Credits x1000") },
        { 2, new BattlePassReward(RewardType.Skin, 0, "PREMIUM Skin: Platinum Storm", "skin_premium_platinum") },
        { 3, new BattlePassReward(RewardType.Gems, 25, "Gems x25") },
        { 4, new BattlePassReward(RewardType.Passive, 0, "Passive: Critical Strike", "passive_critical_strike") },
        { 5, new BattlePassReward(RewardType.PrebuildShip, 0, "PREMIUM Ship: Nebula Hunter", "premium_nebula_hunter", ShipClass.DamageDealer) },

        // Mid Levels (6-15)
        { 6, new BattlePassReward(RewardType.Gems, 30, "Gems x30") },
        { 7, new BattlePassReward(RewardType.Skin, 0, "PREMIUM Skin: Cosmic Void", "skin_premium_cosmic") },
        { 8, new BattlePassReward(RewardType.Active, 0, "Active: Stealth Cloak", "active_cloak") },
        { 9, new BattlePassReward(RewardType.Gems, 35, "Gems x35") },
        { 10, new BattlePassReward(RewardType.PrebuildShip, 0, "EXCLUSIVE Ship: Stellar Dominator", "exclusive_stellar_dom", ShipClass.AllAround) },
        { 11, new BattlePassReward(RewardType.Gems, 40, "Gems x40") },
        { 12, new BattlePassReward(RewardType.Skin, 0, "PREMIUM Skin: Diamond Fury", "skin_premium_diamond") },
        { 13, new BattlePassReward(RewardType.Missile, 0, "Missile: Tactical EMP", "tactical_emp") },
        { 14, new BattlePassReward(RewardType.Gems, 45, "Gems x45") },
        { 15, new BattlePassReward(RewardType.PrebuildShip, 0, "PREMIUM Ship: Quantum Fortress", "premium_quantum_fortress", ShipClass.Tank) },

        // Late Levels (16-25)
        { 16, new BattlePassReward(RewardType.Gems, 50, "Gems x50") },
        { 17, new BattlePassReward(RewardType.Skin, 0, "PREMIUM Skin: Royal Prestige", "skin_premium_royal") },
        { 18, new BattlePassReward(RewardType.Active, 0, "Active: Time Dilation", "active_time_dilation") },
        { 19, new BattlePassReward(RewardType.Gems, 60, "Gems x60") },
        { 20, new BattlePassReward(RewardType.PrebuildShip, 0, "PREMIUM Ship: Ethereal Phantom", "premium_ethereal_phantom", ShipClass.Controller) },
        { 21, new BattlePassReward(RewardType.Gems, 75, "Gems x75") },
        { 22, new BattlePassReward(RewardType.Skin, 0, "LEGENDARY Skin: Celestial Radiance", "skin_legendary_celestial") },
        { 23, new BattlePassReward(RewardType.ShipBody, 0, "Ship Body: Premium Elite Frame", "body_premium_elite", ShipClass.AllAround) },
        { 24, new BattlePassReward(RewardType.Gems, 100, "Gems x100") },
        { 25, new BattlePassReward(RewardType.PrebuildShip, 0, "ULTIMATE EXCLUSIVE: Season Monarch", "ultimate_season_monarch", ShipClass.Controller) }
    };

    #endregion

    #region Persistence

    /// <summary>
    /// Restore battle pass progress from the player profile.
    /// Resets progress if the stored season doesn't match the current one.
    /// </summary>
    public void LoadFromProfile(PlayerAccountData profile)
    {
        if (profile == null) return;

        string seasonId = $"season_{currentSeason}";

        if (profile.currentSeasonID != seasonId)
        {
            // New season - reset progress (premium must be re-purchased per season)
            profile.currentSeasonID = seasonId;
            profile.battlePassTier = 0;
            profile.battlePassXP = 0;
            profile.hasPremiumBattlePass = false;
            profile.claimedFreeBattlePassTiers.Clear();
            profile.claimedPremiumBattlePassTiers.Clear();
            Debug.Log($"[BattlePass] New season started: {seasonName}");
        }

        _currentLevel = Mathf.Clamp(profile.battlePassTier, 0, maxLevel);
        _currentXP = Mathf.Max(0, profile.battlePassXP - (_currentLevel * xpPerLevel));
        isPremiumUnlocked = profile.hasPremiumBattlePass;
        _claimedFreeRewards = new List<int>(profile.claimedFreeBattlePassTiers);
        _claimedPremiumRewards = new List<int>(profile.claimedPremiumBattlePassTiers);

        Debug.Log($"[BattlePass] Restored progress - Level {_currentLevel}, XP {_currentXP}, Premium: {isPremiumUnlocked}");
    }

    /// <summary>
    /// Write current battle pass state back to the player profile (and save).
    /// </summary>
    private void SaveToProfile()
    {
        var profile = GetActiveProfile();
        if (profile == null) return;

        profile.battlePassTier = _currentLevel;
        profile.battlePassXP = (_currentLevel * xpPerLevel) + _currentXP;
        profile.hasPremiumBattlePass = isPremiumUnlocked;
        profile.currentSeasonID = $"season_{currentSeason}";
        profile.claimedFreeBattlePassTiers = new List<int>(_claimedFreeRewards);
        profile.claimedPremiumBattlePassTiers = new List<int>(_claimedPremiumRewards);

        // Persist: online profile if signed in, local save otherwise
        if (AccountSystem.Instance != null && AccountSystem.Instance.IsSignedIn)
        {
            _ = AccountSystem.Instance.UpdateProfileAsync(profile);
        }
        else if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.Save();
        }
    }

    /// <summary>
    /// The profile battle pass progress is stored on:
    /// the online account profile when signed in, the local save otherwise.
    /// </summary>
    private PlayerAccountData GetActiveProfile()
    {
        if (AccountSystem.Instance != null && AccountSystem.Instance.CurrentPlayerProfile != null)
            return AccountSystem.Instance.CurrentPlayerProfile;

        return ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
    }

    #endregion

    #region Battle Pass Progression

    /// <summary>
    /// Add XP to battle pass and check for level ups.
    /// </summary>
    public void AddBattlePassXP(int xp)
    {
        _currentXP += xp;

        // Check for level ups
        while (_currentXP >= xpPerLevel && _currentLevel < maxLevel)
        {
            _currentXP -= xpPerLevel;
            _currentLevel++;

            Debug.Log($"[BattlePass] Level up! Now level {_currentLevel}");
            OnLevelUp(_currentLevel);
        }

        // Cap at max level
        if (_currentLevel >= maxLevel)
        {
            _currentLevel = maxLevel;
            _currentXP = 0;
        }

        SaveToProfile();
    }

    /// <summary>
    /// Called when player levels up in battle pass.
    /// </summary>
    private void OnLevelUp(int newLevel)
    {
        // Notify player of available rewards
        Debug.Log($"[BattlePass] Rewards available at level {newLevel}!");

        // Check what rewards are available
        bool hasFreeReward = FREE_TRACK_REWARDS.ContainsKey(newLevel);
        bool hasPremiumReward = PREMIUM_TRACK_REWARDS.ContainsKey(newLevel) && isPremiumUnlocked;

        if (hasFreeReward)
        {
            var reward = FREE_TRACK_REWARDS[newLevel];
            Debug.Log($"[BattlePass] Free reward: {reward.displayName}");
        }

        if (hasPremiumReward)
        {
            var reward = PREMIUM_TRACK_REWARDS[newLevel];
            Debug.Log($"[BattlePass] Premium reward: {reward.displayName}");
        }
    }

    #endregion

    #region Reward Claiming

    /// <summary>
    /// Claim free track reward for a specific level.
    /// </summary>
    public bool ClaimFreeReward(int level)
    {
        if (level > _currentLevel) return false;
        if (_claimedFreeRewards.Contains(level)) return false;
        if (!FREE_TRACK_REWARDS.ContainsKey(level)) return false;

        var reward = FREE_TRACK_REWARDS[level];
        ApplyReward(reward);
        _claimedFreeRewards.Add(level);
        SaveToProfile();

        Debug.Log($"[BattlePass] Claimed free reward: {reward.displayName}");
        return true;
    }

    /// <summary>
    /// Claim premium track reward for a specific level.
    /// </summary>
    public bool ClaimPremiumReward(int level)
    {
        if (!isPremiumUnlocked) return false;
        if (level > _currentLevel) return false;
        if (_claimedPremiumRewards.Contains(level)) return false;
        if (!PREMIUM_TRACK_REWARDS.ContainsKey(level)) return false;

        var reward = PREMIUM_TRACK_REWARDS[level];
        ApplyReward(reward);
        _claimedPremiumRewards.Add(level);
        SaveToProfile();

        Debug.Log($"[BattlePass] Claimed premium reward: {reward.displayName}");
        return true;
    }

    /// <summary>
    /// Apply reward to player profile.
    /// </summary>
    private void ApplyReward(BattlePassReward reward)
    {
        var profile = GetActiveProfile();
        if (profile == null) return;

        switch (reward.type)
        {
            case RewardType.Credits:
                profile.credits += reward.amount;
                Debug.Log($"[BattlePass] +{reward.amount} credits");
                break;

            case RewardType.Gems:
                profile.gems += reward.amount;
                Debug.Log($"[BattlePass] +{reward.amount} gems");
                break;

            case RewardType.PrebuildShip:
                profile.UnlockById(UnlockType.PrebuildShip, reward.rewardId);
                Debug.Log($"[BattlePass] Unlocked ship: {reward.displayName}");
                break;

            case RewardType.ShipBody:
                profile.UnlockById(UnlockType.ShipBody, reward.rewardId);
                Debug.Log($"[BattlePass] Unlocked ship body: {reward.displayName}");
                break;

            case RewardType.Skin:
                profile.UnlockById(UnlockType.Skin, reward.rewardId);
                Debug.Log($"[BattlePass] Unlocked skin: {reward.displayName}");
                break;

            case RewardType.Passive:
                profile.UnlockById(UnlockType.Passive, reward.rewardId);
                Debug.Log($"[BattlePass] Unlocked passive: {reward.displayName}");
                break;

            case RewardType.Active:
                profile.UnlockById(UnlockType.Active, reward.rewardId);
                Debug.Log($"[BattlePass] Unlocked active: {reward.displayName}");
                break;

            case RewardType.Missile:
                profile.UnlockById(UnlockType.Missile, reward.rewardId);
                Debug.Log($"[BattlePass] Unlocked missile: {reward.displayName}");
                break;

            default:
                Debug.Log($"[BattlePass] Unlocked {reward.type}: {reward.displayName}");
                break;
        }
    }

    #endregion

    #region Premium Purchase

    /// <summary>
    /// Purchase premium battle pass with gems.
    /// </summary>
    public bool PurchasePremiumPass(int gemCost = 1000)
    {
        if (isPremiumUnlocked)
        {
            Debug.LogWarning("[BattlePass] Premium pass already owned!");
            return false;
        }

        var profile = GetActiveProfile();
        if (profile == null)
        {
            Debug.LogWarning("[BattlePass] No player profile - cannot purchase");
            return false;
        }

        if (profile.gems < gemCost)
        {
            Debug.LogWarning($"[BattlePass] Not enough gems ({profile.gems}/{gemCost})");
            return false;
        }

        profile.gems -= gemCost;
        isPremiumUnlocked = true;
        SaveToProfile();

        Debug.Log($"[BattlePass] Premium pass purchased for {gemCost} gems!");
        return true;
    }

    #endregion

    #region Public API

    public int GetCurrentLevel() => _currentLevel;
    public int GetCurrentXP() => _currentXP;
    public int GetXPForNextLevel() => xpPerLevel;
    public bool IsPremiumUnlocked() => isPremiumUnlocked;
    public string GetSeasonName() => seasonName;
    public int GetCurrentSeason() => currentSeason;

    public BattlePassReward GetFreeReward(int level)
    {
        return FREE_TRACK_REWARDS.ContainsKey(level) ? FREE_TRACK_REWARDS[level] : null;
    }

    public BattlePassReward GetPremiumReward(int level)
    {
        return PREMIUM_TRACK_REWARDS.ContainsKey(level) ? PREMIUM_TRACK_REWARDS[level] : null;
    }

    public bool IsFreeRewardClaimed(int level) => _claimedFreeRewards.Contains(level);
    public bool IsPremiumRewardClaimed(int level) => _claimedPremiumRewards.Contains(level);

    #endregion
}

#region Data Structures

// Note: RewardType enum is defined in BattlePassData.cs

[Serializable]
public class BattlePassReward
{
    public RewardType type;
    public int amount;              // For credits/gems
    public string displayName;
    public string rewardId;         // For unlockables
    public ShipClass shipClass;     // For ships/bodies

    public BattlePassReward(RewardType type, int amount, string displayName, string rewardId = "", ShipClass shipClass = ShipClass.AllAround)
    {
        this.type = type;
        this.amount = amount;
        this.displayName = displayName;
        this.rewardId = rewardId;
        this.shipClass = shipClass;
    }
}

#endregion
