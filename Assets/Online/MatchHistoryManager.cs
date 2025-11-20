using System;
using System.Collections.Generic;
using UnityEngine;
using GravityWars.Multiplayer;
using GravityWars.Networking;

/// <summary>
/// Manages match history tracking and statistics updates.
/// Records match results, updates player profiles, and calculates ELO changes.
/// SERVER-SIDE ONLY - Authoritative match result processing.
/// </summary>
public class MatchHistoryManager : MonoBehaviour
{
    #region Singleton

    public static MatchHistoryManager Instance { get; private set; }

    #endregion

    #region Configuration

    [Header("Match History Settings")]
    [SerializeField] private int maxRecentMatches = 50;  // Keep last 50 matches
    [SerializeField] private bool recordCasualMatches = true;

    #endregion

    #region Unity Lifecycle

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

    #endregion

    #region Match Recording

    /// <summary>
    /// Record a completed match and update both players' profiles.
    /// SERVER-ONLY - Called when match ends.
    /// </summary>
    public void RecordMatchResult(MatchResult result)
    {
        Debug.Log($"[MatchHistory] Recording match result - Winner: {result.winnerId}, Ranked: {result.isRanked}");

        // Load both players' profiles
        PlayerProfileData winnerProfile = LoadPlayerProfile(result.winnerId);
        PlayerProfileData loserProfile = LoadPlayerProfile(result.loserId);

        if (winnerProfile == null || loserProfile == null)
        {
            Debug.LogError("[MatchHistory] Failed to load player profiles!");
            return;
        }

        // Update statistics for both players
        UpdatePlayerStatistics(winnerProfile, result, true);
        UpdatePlayerStatistics(loserProfile, result, false);

        // Handle ELO changes if ranked
        if (result.isRanked)
        {
            ProcessELOChanges(winnerProfile, loserProfile, result);
        }

        // Add to match history
        AddToMatchHistory(winnerProfile, result, true);
        AddToMatchHistory(loserProfile, result, false);

        // Calculate XP and credits rewards
        CalculateRewards(winnerProfile, result, true);
        CalculateRewards(loserProfile, result, false);

        // Save both profiles
        SavePlayerProfile(winnerProfile);
        SavePlayerProfile(loserProfile);

        Debug.Log($"[MatchHistory] Match recorded successfully - Match ID: {result.matchId}");
    }

    #endregion

    #region Statistics Updates

    /// <summary>
    /// Update player statistics from match result.
    /// </summary>
    private void UpdatePlayerStatistics(PlayerProfileData profile, MatchResult result, bool isWinner)
    {
        var playerStats = isWinner ? result.winnerStats : result.loserStats;

        // Update match counts
        if (result.isRanked)
        {
            profile.rankedMatchesPlayed++;
            if (isWinner)
            {
                profile.rankedMatchesWon++;
                profile.currentWinStreak++;
                if (profile.currentWinStreak > profile.bestWinStreak)
                {
                    profile.bestWinStreak = profile.currentWinStreak;
                }
            }
            else
            {
                profile.currentWinStreak = 0;
            }
        }
        else
        {
            profile.casualMatchesPlayed++;
            if (isWinner)
            {
                profile.casualMatchesWon++;
            }
        }

        // Update combat statistics
        profile.totalMissilesFired += playerStats.missilesFired;
        profile.totalMissilesHit += playerStats.missilesHit;
        profile.totalDamageDealt += playerStats.damageDealt;
        profile.totalDamageReceived += playerStats.damageReceived;
        profile.totalPlanetsHit += playerStats.planetsHit;

        // Update playtime
        profile.totalPlaytimeSeconds += (long)result.matchDurationSeconds;
        profile.lastLoginTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Debug.Log($"[MatchHistory] Updated stats for {profile.username} - Matches: {profile.rankedMatchesPlayed + profile.casualMatchesPlayed}, Win Rate: {profile.GetOverallWinRate():F1}%");
    }

    /// <summary>
    /// Process ELO rating changes for ranked match.
    /// </summary>
    private void ProcessELOChanges(PlayerProfileData winnerProfile, PlayerProfileData loserProfile, MatchResult result)
    {
        // Calculate winner's ELO change
        var (winnerNewELO, winnerChange) = ELORatingSystem.CalculateNewELO(
            winnerProfile.eloRating,
            loserProfile.eloRating,
            true,
            winnerProfile.rankedMatchesPlayed
        );

        // Calculate loser's ELO change
        var (loserNewELO, loserChange) = ELORatingSystem.CalculateNewELO(
            loserProfile.eloRating,
            winnerProfile.eloRating,
            false,
            loserProfile.rankedMatchesPlayed
        );

        // Apply changes
        winnerProfile.eloRating = winnerNewELO;
        loserProfile.eloRating = loserNewELO;

        // Update peak ELO
        if (winnerNewELO > winnerProfile.peakEloRating)
        {
            winnerProfile.peakEloRating = winnerNewELO;
        }
        if (loserNewELO > loserProfile.peakEloRating)
        {
            loserProfile.peakEloRating = loserNewELO;
        }

        // Update ranks based on new ELO
        winnerProfile.UpdateRankFromELO();
        loserProfile.UpdateRankFromELO();

        // Store ELO changes in match result
        result.winnerStats.eloChange = winnerChange;
        result.loserStats.eloChange = loserChange;

        Debug.Log($"[MatchHistory] ELO Changes - Winner: {winnerProfile.eloRating} ({winnerChange:+0;-0}), Loser: {loserProfile.eloRating} ({loserChange:+0;-0})");
    }

    /// <summary>
    /// Add match to player's match history (limited to maxRecentMatches).
    /// </summary>
    private void AddToMatchHistory(PlayerProfileData profile, MatchResult result, bool isWinner)
    {
        var opponentId = isWinner ? result.loserId : result.winnerId;
        var opponentProfile = LoadPlayerProfile(opponentId);
        var playerStats = isWinner ? result.winnerStats : result.loserStats;

        var matchData = new MatchResultData
        {
            matchId = result.matchId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            isRanked = result.isRanked,
            didWin = isWinner,
            roundsWon = playerStats.roundsWon,
            roundsLost = playerStats.roundsLost,
            damageDealt = playerStats.damageDealt,
            damageReceived = playerStats.damageReceived,
            missileFired = playerStats.missilesFired,
            missileHits = playerStats.missilesHit,
            opponentUsername = opponentProfile?.username ?? "Unknown",
            opponentEloRating = opponentProfile?.eloRating ?? 1200,
            eloChange = playerStats.eloChange,
            xpGained = playerStats.xpGained,
            creditsGained = playerStats.creditsGained,
            shipModelUsed = playerStats.shipModelUsed
        };

        // Add to beginning of list
        profile.recentMatches.Insert(0, matchData);

        // Trim to max size
        if (profile.recentMatches.Count > maxRecentMatches)
        {
            profile.recentMatches.RemoveRange(maxRecentMatches, profile.recentMatches.Count - maxRecentMatches);
        }
    }

    /// <summary>
    /// Calculate XP and credits rewards based on match performance.
    /// </summary>
    private void CalculateRewards(PlayerProfileData profile, MatchResult result, bool isWinner)
    {
        var playerStats = isWinner ? result.winnerStats : result.loserStats;

        // Base rewards
        int baseXP = result.isRanked ? 100 : 50;
        int baseCredits = result.isRanked ? 50 : 25;

        // Win bonus (2x multiplier)
        if (isWinner)
        {
            baseXP *= 2;
            baseCredits *= 2;
        }

        // Performance bonuses
        int performanceXP = 0;
        int performanceCredits = 0;

        // Damage dealt bonus
        performanceXP += playerStats.damageDealt / 10;
        performanceCredits += playerStats.damageDealt / 20;

        // Accuracy bonus
        float accuracy = playerStats.missilesFired > 0
            ? (float)playerStats.missilesHit / playerStats.missilesFired
            : 0f;

        if (accuracy > 0.5f)
        {
            performanceXP += 25;
            performanceCredits += 10;
        }
        if (accuracy > 0.75f)
        {
            performanceXP += 25;
            performanceCredits += 10;
        }

        // Win streak bonus (ranked only)
        if (result.isRanked && isWinner)
        {
            int streakBonus = Mathf.Min(profile.currentWinStreak * 10, 100);
            performanceXP += streakBonus;
            performanceCredits += streakBonus / 2;
        }

        // Total rewards
        int totalXP = baseXP + performanceXP;
        int totalCredits = baseCredits + performanceCredits;

        // Apply to profile
        profile.currentXP += totalXP;
        profile.credits += totalCredits;

        // Store in stats
        playerStats.xpGained = totalXP;
        playerStats.creditsGained = totalCredits;

        // Check for level up
        CheckLevelUp(profile);

        Debug.Log($"[MatchHistory] Rewards for {profile.username} - XP: +{totalXP}, Credits: +{totalCredits}");
    }

    /// <summary>
    /// Check if player leveled up and apply level progression with rewards.
    /// Uses ProgressionSystem for unlock management and rewards.
    /// </summary>
    private void CheckLevelUp(PlayerProfileData profile)
    {
        while (profile.currentXP >= profile.xpForNextLevel)
        {
            profile.currentXP -= profile.xpForNextLevel;
            profile.level++;

            // Get level-up rewards from ProgressionSystem
            LevelUpReward reward = ProgressionSystem.GetLevelUpReward(profile.level);

            // Apply rewards
            profile.credits += reward.credits;
            profile.gems += reward.gems;

            // Calculate XP requirement for next level
            profile.xpForNextLevel = ProgressionSystem.CalculateXPForLevel(profile.level + 1);

            // Log level up with rewards
            Debug.Log($"[MatchHistory] 🎉 {profile.username} leveled up to level {profile.level}!");
            Debug.Log($"[MatchHistory]   Rewards: +{reward.credits} credits, +{reward.gems} gems");

            // Log unlocks
            if (reward.unlocks != null && reward.unlocks.Count > 0)
            {
                Debug.Log($"[MatchHistory]   🔓 New unlocks:");
                foreach (var unlock in reward.unlocks)
                {
                    Debug.Log($"[MatchHistory]      - {unlock.displayName}: {unlock.description}");

                    // Add unlock to player profile based on type
                    ApplyUnlock(profile, unlock);
                }
            }
        }
    }

    /// <summary>
    /// Apply an unlock to the player profile.
    /// </summary>
    private void ApplyUnlock(PlayerProfileData profile, UnlockData unlock)
    {
        switch (unlock.type)
        {
            case UnlockType.ShipClass:
                // Ship class unlocks are level-based, no need to store
                Debug.Log($"[MatchHistory] Ship class unlocked: {unlock.displayName}");
                break;

            case UnlockType.CustomSlot:
                // Custom slot unlocks are level-based, no need to store
                Debug.Log($"[MatchHistory] Custom slot unlocked: {unlock.displayName}");
                break;

            case UnlockType.Ship:
                // Legacy ship unlock (kept for backwards compatibility)
                if (!profile.unlockedShipModels.Contains(unlock.id))
                {
                    profile.unlockedShipModels.Add(unlock.id);
                    Debug.Log($"[MatchHistory] Ship unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.PrebuildShip:
                // Add prebuild ship to unlocked ships
                if (!profile.unlockedShipModels.Contains(unlock.id))
                {
                    profile.unlockedShipModels.Add(unlock.id);
                    Debug.Log($"[MatchHistory] 🚀 Prebuild ship unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.ShipBody:
                // Add ship body to unlocked bodies (for custom building)
                if (!profile.unlockedShipBodies.Contains(unlock.id))
                {
                    profile.unlockedShipBodies.Add(unlock.id);
                    Debug.Log($"[MatchHistory] 🔧 Ship body unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.Passive:
                // Add passive ability to unlocked passives
                if (!profile.unlockedPassives.Contains(unlock.id))
                {
                    profile.unlockedPassives.Add(unlock.id);
                    Debug.Log($"[MatchHistory] ⚡ Passive ability unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.Active:
                // Add active ability to unlocked actives
                if (!profile.unlockedActives.Contains(unlock.id))
                {
                    profile.unlockedActives.Add(unlock.id);
                    Debug.Log($"[MatchHistory] 💫 Active ability unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.Missile:
                // Add missile to unlocked missiles (retrofit system)
                if (!profile.unlockedMissiles.Contains(unlock.id))
                {
                    profile.unlockedMissiles.Add(unlock.id);
                    Debug.Log($"[MatchHistory] 🚀 Missile unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.Skin:
                // Add skin to unlocked skins
                if (!profile.unlockedSkins.Contains(unlock.id))
                {
                    profile.unlockedSkins.Add(unlock.id);
                    Debug.Log($"[MatchHistory] 🎨 Skin unlocked: {unlock.displayName}");
                }
                break;

            case UnlockType.Feature:
            case UnlockType.GameMode:
                // Feature unlocks are level-based, no need to store
                Debug.Log($"[MatchHistory] Feature unlocked: {unlock.displayName}");
                break;

            case UnlockType.Cosmetic:
                // Legacy cosmetic (kept for backwards compatibility)
                Debug.Log($"[MatchHistory] Cosmetic unlocked: {unlock.displayName}");
                break;
        }
    }

    #endregion

    #region Profile Storage (Placeholder - Will integrate with CloudSaveService)

    /// <summary>
    /// Load player profile from storage.
    /// Uses CloudSaveService for server-side profile retrieval.
    /// </summary>
    private PlayerProfileData LoadPlayerProfile(string playerId)
    {
        Debug.Log($"[MatchHistory] Loading profile for player {playerId}");

        // Note: This is a synchronous method but CloudSaveService is async
        // In production, this should be refactored to async or use a cache
        // For now, we'll use a temporary workaround with Task.Run

        if (CloudSaveService.Instance == null)
        {
            Debug.LogError("[MatchHistory] CloudSaveService not available!");
            return null;
        }

        try
        {
            // TEMPORARY: Synchronous wait for async operation
            // In production, refactor RecordMatchResult to be async
            var task = CloudSaveService.Instance.LoadPlayerProfile();
            task.Wait();
            return task.Result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MatchHistory] Failed to load profile: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save player profile to storage.
    /// Uses CloudSaveService for server-side profile storage.
    /// </summary>
    private void SavePlayerProfile(PlayerProfileData profile)
    {
        Debug.Log($"[MatchHistory] Saving profile for {profile.username} (ELO: {profile.eloRating})");

        if (CloudSaveService.Instance == null)
        {
            Debug.LogError("[MatchHistory] CloudSaveService not available!");
            return;
        }

        try
        {
            // TEMPORARY: Fire and forget async operation
            // In production, refactor RecordMatchResult to be async and await this
            _ = CloudSaveService.Instance.SavePlayerProfile(profile);
        }
        catch (Exception e)
        {
            Debug.LogError($"[MatchHistory] Failed to save profile: {e.Message}");
        }
    }

    #endregion

    #region Public Query Methods

    /// <summary>
    /// Get player's match history.
    /// </summary>
    public List<MatchResultData> GetPlayerMatchHistory(string playerId, int count = 10)
    {
        var profile = LoadPlayerProfile(playerId);
        if (profile == null) return new List<MatchResultData>();

        int resultCount = Mathf.Min(count, profile.recentMatches.Count);
        return profile.recentMatches.GetRange(0, resultCount);
    }

    /// <summary>
    /// Get player's statistics summary.
    /// </summary>
    public PlayerStatsSummary GetPlayerStats(string playerId)
    {
        var profile = LoadPlayerProfile(playerId);
        if (profile == null) return new PlayerStatsSummary();

        return new PlayerStatsSummary
        {
            username = profile.username,
            level = profile.level,
            eloRating = profile.eloRating,
            peakEloRating = profile.peakEloRating,
            currentRank = profile.currentRank,
            rankedWinRate = profile.GetRankedWinRate(),
            casualWinRate = profile.GetCasualWinRate(),
            overallWinRate = profile.GetOverallWinRate(),
            totalMatches = profile.rankedMatchesPlayed + profile.casualMatchesPlayed,
            currentWinStreak = profile.currentWinStreak,
            bestWinStreak = profile.bestWinStreak,
            missileAccuracy = profile.GetMissileAccuracy()
        };
    }

    #endregion
}

/// <summary>
/// Complete match result data (server-side).
/// </summary>
[Serializable]
public class MatchResult
{
    public string matchId;
    public string winnerId;
    public string loserId;
    public bool isRanked;
    public int matchDurationSeconds;
    public PlayerMatchStats winnerStats;
    public PlayerMatchStats loserStats;
}

/// <summary>
/// Individual player statistics for a match.
/// </summary>
[Serializable]
public class PlayerMatchStats
{
    public int roundsWon;
    public int roundsLost;
    public int damageDealt;
    public int damageReceived;
    public int missilesFired;
    public int missilesHit;
    public int planetsHit;
    public int eloChange;  // +/- for ranked matches
    public int xpGained;
    public int creditsGained;
    public string shipModelUsed;
}

/// <summary>
/// Player statistics summary for UI display.
/// </summary>
[Serializable]
public class PlayerStatsSummary
{
    public string username;
    public int level;
    public int eloRating;
    public int peakEloRating;
    public CompetitiveRank currentRank;
    public float rankedWinRate;
    public float casualWinRate;
    public float overallWinRate;
    public int totalMatches;
    public int currentWinStreak;
    public int bestWinStreak;
    public float missileAccuracy;
}
