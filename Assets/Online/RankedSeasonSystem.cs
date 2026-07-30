using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Seasonal ranked ladder handling:
/// - Soft ELO reset at season rollover (pulled halfway back to 1200)
/// - Rewards granted based on the PEAK rank reached last season
///
/// Called from BattlePassSystem.LoadFromProfile when a new season is detected,
/// so battle pass season and ranked season roll over together.
/// </summary>
public static class RankedSeasonSystem
{
    public const int SOFT_RESET_ANCHOR = 1200;

    /// <summary>
    /// Applies the ranked season rollover to a profile:
    /// grants peak-rank rewards for the finished season, then soft-resets ELO.
    /// Safe to call once per season change (tracked via lastRankedSeasonID).
    /// </summary>
    public static void ApplySeasonRollover(PlayerAccountData profile, string newSeasonId)
    {
        if (profile == null || profile.lastRankedSeasonID == newSeasonId) return;

        bool isFirstSeason = string.IsNullOrEmpty(profile.lastRankedSeasonID);
        profile.lastRankedSeasonID = newSeasonId;

        // Nothing to reward or reset before the player's first season
        if (isFirstSeason) return;

        // 1. Grant rewards for last season's PEAK rank
        GrantPeakRankRewards(profile, newSeasonId);

        // 2. Soft-reset ELO: halfway back toward the anchor
        int oldElo = profile.eloRating;
        profile.eloRating = (profile.eloRating + SOFT_RESET_ANCHOR) / 2;
        profile.peakEloRating = profile.eloRating;
        profile.currentWinStreak = 0;
        profile.UpdateRankFromELO();

        Debug.Log($"[RankedSeason] Season rollover ({newSeasonId}) - ELO soft reset {oldElo} → {profile.eloRating}");
    }

    /// <summary>
    /// Rewards by peak rank: gems + an exclusive seasonal skin for high ranks.
    /// Skins are id-based so each season's set is unique.
    /// </summary>
    private static void GrantPeakRankRewards(PlayerAccountData profile, string seasonId)
    {
        CompetitiveRank peakRank = RankFromELO(profile.peakEloRating);

        int gems;
        switch (peakRank)
        {
            case CompetitiveRank.Grandmaster: gems = 500; break;
            case CompetitiveRank.Master: gems = 300; break;
            case CompetitiveRank.Diamond: gems = 200; break;
            case CompetitiveRank.Platinum: gems = 120; break;
            case CompetitiveRank.Gold: gems = 80; break;
            case CompetitiveRank.Silver: gems = 40; break;
            default: gems = 20; break;
        }

        profile.gems += gems;

        // Exclusive seasonal skin for Diamond and above
        if (peakRank >= CompetitiveRank.Diamond)
        {
            string skinId = $"skin_ranked_{seasonId}_{peakRank.ToString().ToLower()}";
            if (!profile.unlockedSkinIDs.Contains(skinId))
                profile.unlockedSkinIDs.Add(skinId);
            Debug.Log($"[RankedSeason] Exclusive skin unlocked: {skinId}");
        }

        Debug.Log($"[RankedSeason] Peak rank {peakRank} rewards: +{gems} gems");
    }

    /// <summary>Rank thresholds (mirror of PlayerAccountData.UpdateRankFromELO).</summary>
    public static CompetitiveRank RankFromELO(int elo)
    {
        if (elo < 1000) return CompetitiveRank.Bronze;
        if (elo < 1200) return CompetitiveRank.Silver;
        if (elo < 1400) return CompetitiveRank.Gold;
        if (elo < 1600) return CompetitiveRank.Platinum;
        if (elo < 1800) return CompetitiveRank.Diamond;
        if (elo < 2000) return CompetitiveRank.Master;
        return CompetitiveRank.Grandmaster;
    }
}
