using UnityEngine;

/// <summary>
/// Configuration and metadata for player ranks.
/// Provides display names, descriptions, colors, and requirements for each rank tier.
/// </summary>
public static class RankConfiguration
{
    /// <summary>
    /// Data structure containing all metadata for a specific rank.
    /// </summary>
    public struct RankData
    {
        public CompetitiveRank rank;
        public string displayName;
        public string description;
        public Color color;
        public int minELO;
        public int maxELO;
        public string abbreviation;
        public int starCount; // Number of stars/insignia to display (1-5)
    }

    /// <summary>
    /// Get comprehensive rank data for a specific rank.
    /// </summary>
    public static RankData GetRankData(CompetitiveRank rank)
    {
        var (minELO, maxELO) = ELORatingSystem.GetELORangeForRank(rank);

        switch (rank)
        {
            case CompetitiveRank.Cadet:
                return new RankData
                {
                    rank = rank,
                    displayName = "Cadet",
                    description = "Training Officer - Begin your journey to become a fleet commander",
                    color = new Color(0.5f, 0.35f, 0.15f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "CDT",
                    starCount = 0
                };

            case CompetitiveRank.Midshipman:
                return new RankData
                {
                    rank = rank,
                    displayName = "Midshipman",
                    description = "Junior Trainee - Advancing through basic fleet operations",
                    color = new Color(0.6f, 0.4f, 0.2f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "MIDN",
                    starCount = 0
                };

            case CompetitiveRank.Ensign:
                return new RankData
                {
                    rank = rank,
                    displayName = "Ensign",
                    description = "Junior Officer - Proving your worth in the fleet",
                    color = new Color(0.8f, 0.5f, 0.2f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "ENS",
                    starCount = 1
                };

            case CompetitiveRank.SubLieutenant:
                return new RankData
                {
                    rank = rank,
                    displayName = "Sub-Lieutenant",
                    description = "Junior Commissioned Officer - Developing tactical competence",
                    color = new Color(0.7f, 0.7f, 0.7f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "SBLT",
                    starCount = 1
                };

            case CompetitiveRank.Lieutenant:
                return new RankData
                {
                    rank = rank,
                    displayName = "Lieutenant",
                    description = "Commissioned Officer - A skilled tactical operator",
                    color = new Color(0.75f, 0.75f, 0.75f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "LT",
                    starCount = 2
                };

            case CompetitiveRank.LieutenantCommander:
                return new RankData
                {
                    rank = rank,
                    displayName = "Lieutenant Commander",
                    description = "Senior Officer - Demonstrates exceptional tactical prowess",
                    color = new Color(0.9f, 0.9f, 0.95f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "LCDR",
                    starCount = 2
                };

            case CompetitiveRank.Commander:
                return new RankData
                {
                    rank = rank,
                    displayName = "Commander",
                    description = "Command Officer - Leads with strategic excellence",
                    color = new Color(1f, 0.84f, 0f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "CDR",
                    starCount = 3
                };

            case CompetitiveRank.Captain:
                return new RankData
                {
                    rank = rank,
                    displayName = "Captain",
                    description = "Ship Captain - Master of gravitational warfare",
                    color = new Color(1f, 0.92f, 0.3f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "CAPT",
                    starCount = 3
                };

            case CompetitiveRank.SeniorCaptain:
                return new RankData
                {
                    rank = rank,
                    displayName = "Senior Captain",
                    description = "Distinguished Captain - Veteran of countless engagements",
                    color = new Color(0.5f, 0.8f, 1f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "SCPT",
                    starCount = 3
                };

            case CompetitiveRank.Commodore:
                return new RankData
                {
                    rank = rank,
                    displayName = "Commodore",
                    description = "Fleet Officer - Commands respect across the galaxy",
                    color = new Color(0.7f, 0.9f, 1f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "CDRE",
                    starCount = 4
                };

            case CompetitiveRank.RearAdmiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "Rear Admiral",
                    description = "Lower Admiral - An elite tactician of the fleet",
                    color = new Color(0.4f, 0.7f, 1f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "RADM",
                    starCount = 4
                };

            case CompetitiveRank.RearAdmiralUpperHalf:
                return new RankData
                {
                    rank = rank,
                    displayName = "Rear Admiral (Upper Half)",
                    description = "Senior Rear Admiral - Elite command authority",
                    color = new Color(0.3f, 0.5f, 0.9f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "RADM(UH)",
                    starCount = 4
                };

            case CompetitiveRank.ViceAdmiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "Vice Admiral",
                    description = "High Admiral - Among the galaxy's finest commanders",
                    color = new Color(0.6f, 0.2f, 0.8f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "VADM",
                    starCount = 4
                };

            case CompetitiveRank.Admiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "Admiral",
                    description = "Admiral - Supreme tactical authority",
                    color = new Color(0.8f, 0.3f, 0.9f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "ADM",
                    starCount = 5
                };

            case CompetitiveRank.HighAdmiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "High Admiral",
                    description = "Distinguished Admiral - Renowned across all sectors",
                    color = new Color(0.9f, 0.4f, 1f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "HADM",
                    starCount = 5
                };

            case CompetitiveRank.FleetAdmiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "Fleet Admiral",
                    description = "Supreme Commander - Legendary tactical mastery",
                    color = new Color(1f, 0.3f, 0.3f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "FADM",
                    starCount = 5
                };

            case CompetitiveRank.SupremeAdmiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "Supreme Admiral",
                    description = "Elite Commander - Among the galaxy's greatest legends",
                    color = new Color(1f, 0.25f, 0.25f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "SADM",
                    starCount = 5
                };

            case CompetitiveRank.GrandAdmiral:
                return new RankData
                {
                    rank = rank,
                    displayName = "Grand Admiral",
                    description = "Grand Admiral - The pinnacle of military excellence",
                    color = new Color(1f, 0.2f, 0.2f),
                    minELO = minELO,
                    maxELO = maxELO,
                    abbreviation = "GADM",
                    starCount = 5
                };

            default:
                return new RankData
                {
                    rank = rank,
                    displayName = "Unranked",
                    description = "No rank assigned",
                    color = Color.white,
                    minELO = 0,
                    maxELO = 0,
                    abbreviation = "???",
                    starCount = 0
                };
        }
    }

    /// <summary>
    /// Get a formatted string showing rank name and ELO range.
    /// Example: "Commander (1200-1399 ELO)"
    /// </summary>
    public static string GetRankDisplayString(CompetitiveRank rank)
    {
        var data = GetRankData(rank);
        if (data.maxELO >= ELORatingSystem.MAXIMUM_ELO)
        {
            return $"{data.displayName} ({data.minELO}+ ELO)";
        }
        return $"{data.displayName} ({data.minELO}-{data.maxELO} ELO)";
    }

    /// <summary>
    /// Get colored rich text for rank display (uses Unity's Rich Text format).
    /// Example: "<color=#FFD700>Commander</color>"
    /// </summary>
    public static string GetRankColoredText(CompetitiveRank rank)
    {
        var data = GetRankData(rank);
        string hexColor = ColorUtility.ToHtmlStringRGB(data.color);
        return $"<color=#{hexColor}>{data.displayName}</color>";
    }

    /// <summary>
    /// Get the next rank after the current rank (or null if at max rank).
    /// </summary>
    public static CompetitiveRank? GetNextRank(CompetitiveRank currentRank)
    {
        if (currentRank == CompetitiveRank.GrandAdmiral)
        {
            return null; // Already at max rank
        }

        return currentRank + 1;
    }

    /// <summary>
    /// Get the previous rank before the current rank (or null if at minimum rank).
    /// </summary>
    public static CompetitiveRank? GetPreviousRank(CompetitiveRank currentRank)
    {
        if (currentRank == CompetitiveRank.Cadet)
        {
            return null; // Already at minimum rank
        }

        return currentRank - 1;
    }

    /// <summary>
    /// Get how many ELO points needed to reach the next rank.
    /// Returns 0 if already at max rank.
    /// </summary>
    public static int GetELOToNextRank(int currentELO)
    {
        var currentRank = ELORatingSystem.GetRankFromELO(currentELO);
        var nextRank = GetNextRank(currentRank);

        if (nextRank == null)
        {
            return 0; // Already at max rank
        }

        var (nextMin, _) = ELORatingSystem.GetELORangeForRank(nextRank.Value);
        int eloNeeded = nextMin - currentELO;

        return Mathf.Max(0, eloNeeded);
    }

    /// <summary>
    /// Get progress percentage through current rank (0.0 to 1.0).
    /// </summary>
    public static float GetRankProgress(int currentELO)
    {
        var currentRank = ELORatingSystem.GetRankFromELO(currentELO);
        var (minELO, maxELO) = ELORatingSystem.GetELORangeForRank(currentRank);

        // If at max rank, always return 1.0
        if (currentRank == CompetitiveRank.GrandAdmiral)
        {
            return 1.0f;
        }

        int rangeSize = maxELO - minELO + 1;
        int eloIntoRank = currentELO - minELO;

        return Mathf.Clamp01((float)eloIntoRank / rangeSize);
    }

    /// <summary>
    /// Get all ranks in ascending order.
    /// </summary>
    public static CompetitiveRank[] GetAllRanks()
    {
        return new CompetitiveRank[]
        {
            CompetitiveRank.Cadet,
            CompetitiveRank.Midshipman,
            CompetitiveRank.Ensign,
            CompetitiveRank.SubLieutenant,
            CompetitiveRank.Lieutenant,
            CompetitiveRank.LieutenantCommander,
            CompetitiveRank.Commander,
            CompetitiveRank.Captain,
            CompetitiveRank.SeniorCaptain,
            CompetitiveRank.Commodore,
            CompetitiveRank.RearAdmiral,
            CompetitiveRank.RearAdmiralUpperHalf,
            CompetitiveRank.ViceAdmiral,
            CompetitiveRank.Admiral,
            CompetitiveRank.HighAdmiral,
            CompetitiveRank.FleetAdmiral,
            CompetitiveRank.SupremeAdmiral,
            CompetitiveRank.GrandAdmiral
        };
    }
}
