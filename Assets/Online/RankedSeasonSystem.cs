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

        // Graduated gem reward across the 16-tier military rank ladder
        // (merged in from the MainMenuHub rank rework - was a 7-tier
        // Bronze..Grandmaster scale before). Roughly doubles every couple
        // of tiers, same shape as the old curve, just more steps.
        int gems;
        switch (peakRank)
        {
            case CompetitiveRank.GrandAdmiral: gems = 500; break;
            case CompetitiveRank.SupremeAdmiral: gems = 420; break;
            case CompetitiveRank.FleetAdmiral: gems = 350; break;
            case CompetitiveRank.HighAdmiral: gems = 300; break;
            case CompetitiveRank.Admiral: gems = 250; break;
            case CompetitiveRank.ViceAdmiral: gems = 200; break;
            case CompetitiveRank.RearAdmiralUpperHalf: gems = 160; break;
            case CompetitiveRank.RearAdmiral: gems = 130; break;
            case CompetitiveRank.Commodore: gems = 100; break;
            case CompetitiveRank.SeniorCaptain: gems = 80; break;
            case CompetitiveRank.Captain: gems = 60; break;
            case CompetitiveRank.Commander: gems = 45; break;
            case CompetitiveRank.LieutenantCommander: gems = 35; break;
            case CompetitiveRank.Lieutenant: gems = 25; break;
            default: gems = 20; break; // Ensign, Cadet
        }

        profile.gems += gems;

        // Exclusive seasonal skin for Vice Admiral and above (top 6 of 16 -
        // same "clearly above average" cutoff the old Diamond+ threshold
        // used out of 7 tiers).
        if (peakRank >= CompetitiveRank.ViceAdmiral)
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
        if (elo < 700) return CompetitiveRank.Cadet;
        if (elo < 1050) return CompetitiveRank.Ensign;
        if (elo < 1200) return CompetitiveRank.Lieutenant;
        if (elo < 1350) return CompetitiveRank.LieutenantCommander;
        if (elo < 1500) return CompetitiveRank.Commander;
        if (elo < 1650) return CompetitiveRank.Captain;
        if (elo < 1800) return CompetitiveRank.SeniorCaptain;
        if (elo < 1950) return CompetitiveRank.Commodore;
        if (elo < 2100) return CompetitiveRank.RearAdmiral;
        if (elo < 2250) return CompetitiveRank.RearAdmiralUpperHalf;
        if (elo < 2400) return CompetitiveRank.ViceAdmiral;
        if (elo < 2550) return CompetitiveRank.Admiral;
        if (elo < 2700) return CompetitiveRank.HighAdmiral;
        if (elo < 2850) return CompetitiveRank.FleetAdmiral;
        if (elo < 3000) return CompetitiveRank.SupremeAdmiral;
        return CompetitiveRank.GrandAdmiral;
    }
}
