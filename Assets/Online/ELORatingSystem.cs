using UnityEngine;

/// <summary>
/// ELO Rating System implementation (based on chess ELO formula).
/// Used for competitive matchmaking and ranking in Gravity Wars.
///
/// Standard ELO ranges:
/// - Starting: 1200
/// - Bronze: <1000
/// - Silver: 1000-1199
/// - Gold: 1200-1399
/// - Platinum: 1400-1599
/// - Diamond: 1600-1799
/// - Master: 1800-1999
/// - Grandmaster: 2000+
/// </summary>
public static class ELORatingSystem
{
    #region Constants

    /// <summary>
    /// K-factor determines how much ELO changes per match.
    /// Higher K = more volatile ratings (faster climb/fall).
    /// Standard chess uses K=32 for beginners, K=24 for intermediate, K=16 for masters.
    /// </summary>
    private const int K_FACTOR_BEGINNER = 40;      // <10 matches played
    private const int K_FACTOR_INTERMEDIATE = 32;  // 10-50 matches
    private const int K_FACTOR_VETERAN = 24;       // 50+ matches
    private const int K_FACTOR_MASTER = 16;        // 1800+ ELO

    /// <summary>
    /// Starting ELO for all new players
    /// </summary>
    public const int STARTING_ELO = 1200;

    /// <summary>
    /// Minimum ELO (prevents going below this)
    /// </summary>
    public const int MINIMUM_ELO = 100;

    /// <summary>
    /// Maximum ELO (theoretical cap)
    /// </summary>
    public const int MAXIMUM_ELO = 3000;

    #endregion

    #region ELO Calculation

    /// <summary>
    /// Calculate new ELO ratings after a match.
    /// Returns tuple of (newELO, eloChange) for the player.
    /// </summary>
    /// <param name="playerELO">Player's current ELO</param>
    /// <param name="opponentELO">Opponent's current ELO</param>
    /// <param name="playerWon">Did player win?</param>
    /// <param name="matchesPlayed">Number of ranked matches player has played</param>
    /// <returns>Tuple of (newELO, eloChange)</returns>
    public static (int newELO, int eloChange) CalculateNewELO(
        int playerELO,
        int opponentELO,
        bool playerWon,
        int matchesPlayed)
    {
        // Get appropriate K-factor based on experience and skill
        int kFactor = GetKFactor(playerELO, matchesPlayed);

        // Calculate expected score (probability of winning)
        float expectedScore = CalculateExpectedScore(playerELO, opponentELO);

        // Actual score: 1 for win, 0 for loss (no draws in Gravity Wars)
        float actualScore = playerWon ? 1.0f : 0.0f;

        // Calculate ELO change
        float eloChangeFloat = kFactor * (actualScore - expectedScore);
        int eloChange = Mathf.RoundToInt(eloChangeFloat);

        // Apply change
        int newELO = playerELO + eloChange;

        // Clamp to min/max
        newELO = Mathf.Clamp(newELO, MINIMUM_ELO, MAXIMUM_ELO);

        // Recalculate actual change after clamping
        eloChange = newELO - playerELO;

        return (newELO, eloChange);
    }

    /// <summary>
    /// Calculate expected score (win probability) using ELO formula.
    /// Returns value between 0 and 1.
    /// </summary>
    private static float CalculateExpectedScore(int playerELO, int opponentELO)
    {
        // Standard ELO expected score formula
        // E = 1 / (1 + 10^((opponentELO - playerELO) / 400))
        float exponent = (opponentELO - playerELO) / 400f;
        float expected = 1f / (1f + Mathf.Pow(10f, exponent));
        return expected;
    }

    /// <summary>
    /// Get K-factor based on player experience and skill level.
    /// Beginners have higher K (faster rating changes).
    /// Veterans and masters have lower K (more stable ratings).
    /// </summary>
    private static int GetKFactor(int playerELO, int matchesPlayed)
    {
        // Masters (1800+) have lowest K-factor for stability
        if (playerELO >= 1800)
        {
            return K_FACTOR_MASTER;
        }

        // Veterans (50+ matches) have medium K-factor
        if (matchesPlayed >= 50)
        {
            return K_FACTOR_VETERAN;
        }

        // Intermediates (10-49 matches) have higher K-factor
        if (matchesPlayed >= 10)
        {
            return K_FACTOR_INTERMEDIATE;
        }

        // Beginners (<10 matches) have highest K-factor
        return K_FACTOR_BEGINNER;
    }

    #endregion

    #region Win Probability

    /// <summary>
    /// Calculate win probability percentage based on ELO difference.
    /// Useful for matchmaking to show predicted match outcome.
    /// </summary>
    /// <param name="playerELO">Player's ELO</param>
    /// <param name="opponentELO">Opponent's ELO</param>
    /// <returns>Win probability as percentage (0-100)</returns>
    public static float GetWinProbability(int playerELO, int opponentELO)
    {
        return CalculateExpectedScore(playerELO, opponentELO) * 100f;
    }

    /// <summary>
    /// Get ELO difference between two players.
    /// Positive = player has higher ELO, Negative = opponent has higher ELO.
    /// </summary>
    public static int GetELODifference(int playerELO, int opponentELO)
    {
        return playerELO - opponentELO;
    }

    /// <summary>
    /// Check if two players are considered a "fair match" for matchmaking.
    /// Fair match = ELO difference within acceptable range.
    /// </summary>
    /// <param name="elo1">First player's ELO</param>
    /// <param name="elo2">Second player's ELO</param>
    /// <param name="maxDifference">Maximum acceptable ELO difference (default 150)</param>
    /// <returns>True if match is fair</returns>
    public static bool IsFairMatch(int elo1, int elo2, int maxDifference = 150)
    {
        return Mathf.Abs(elo1 - elo2) <= maxDifference;
    }

    #endregion

    #region Rank Helpers

    /// <summary>
    /// Get competitive rank based on ELO rating.
    /// </summary>
    public static CompetitiveRank GetRankFromELO(int elo)
    {
        if (elo < 1000) return CompetitiveRank.Bronze;
        if (elo < 1200) return CompetitiveRank.Silver;
        if (elo < 1400) return CompetitiveRank.Gold;
        if (elo < 1600) return CompetitiveRank.Platinum;
        if (elo < 1800) return CompetitiveRank.Diamond;
        if (elo < 2000) return CompetitiveRank.Master;
        return CompetitiveRank.Grandmaster;
    }

    /// <summary>
    /// Get ELO range for a specific rank.
    /// Returns tuple of (minELO, maxELO).
    /// </summary>
    public static (int min, int max) GetELORangeForRank(CompetitiveRank rank)
    {
        switch (rank)
        {
            case CompetitiveRank.Bronze:
                return (MINIMUM_ELO, 999);
            case CompetitiveRank.Silver:
                return (1000, 1199);
            case CompetitiveRank.Gold:
                return (1200, 1399);
            case CompetitiveRank.Platinum:
                return (1400, 1599);
            case CompetitiveRank.Diamond:
                return (1600, 1799);
            case CompetitiveRank.Master:
                return (1800, 1999);
            case CompetitiveRank.Grandmaster:
                return (2000, MAXIMUM_ELO);
            default:
                return (STARTING_ELO, STARTING_ELO);
        }
    }

    /// <summary>
    /// Get rank display name with color.
    /// </summary>
    public static string GetRankDisplayName(CompetitiveRank rank)
    {
        return rank.ToString();
    }

    /// <summary>
    /// Get rank color for UI display.
    /// </summary>
    public static Color GetRankColor(CompetitiveRank rank)
    {
        switch (rank)
        {
            case CompetitiveRank.Bronze:
                return new Color(0.8f, 0.5f, 0.2f); // Bronze color
            case CompetitiveRank.Silver:
                return new Color(0.75f, 0.75f, 0.75f); // Silver color
            case CompetitiveRank.Gold:
                return new Color(1f, 0.84f, 0f); // Gold color
            case CompetitiveRank.Platinum:
                return new Color(0.9f, 1f, 1f); // Platinum color
            case CompetitiveRank.Diamond:
                return new Color(0.7f, 0.9f, 1f); // Diamond blue
            case CompetitiveRank.Master:
                return new Color(0.6f, 0.2f, 0.8f); // Purple
            case CompetitiveRank.Grandmaster:
                return new Color(1f, 0.3f, 0.3f); // Red/Crimson
            default:
                return Color.white;
        }
    }

    #endregion

    #region Simulation & Testing

    /// <summary>
    /// Simulate ELO change for testing/preview.
    /// Shows what would happen without actually changing ratings.
    /// </summary>
    public static MatchELOPreview PreviewMatchELO(int playerELO, int opponentELO, bool playerWins, int matchesPlayed)
    {
        var (newELOWin, eloChangeWin) = CalculateNewELO(playerELO, opponentELO, true, matchesPlayed);
        var (newELOLoss, eloChangeLoss) = CalculateNewELO(playerELO, opponentELO, false, matchesPlayed);

        float winProbability = GetWinProbability(playerELO, opponentELO);

        return new MatchELOPreview
        {
            playerELO = playerELO,
            opponentELO = opponentELO,
            eloDifference = playerELO - opponentELO,
            winProbability = winProbability,
            eloIfWin = newELOWin,
            eloChangeIfWin = eloChangeWin,
            eloIfLose = newELOLoss,
            eloChangeIfLose = eloChangeLoss
        };
    }

    #endregion
}

/// <summary>
/// Preview of potential ELO changes before a match.
/// </summary>
public struct MatchELOPreview
{
    public int playerELO;
    public int opponentELO;
    public int eloDifference;
    public float winProbability;
    public int eloIfWin;
    public int eloChangeIfWin;
    public int eloIfLose;
    public int eloChangeIfLose;

    public override string ToString()
    {
        return $"Player: {playerELO} vs Opponent: {opponentELO}\n" +
               $"Win Probability: {winProbability:F1}%\n" +
               $"If Win: {eloIfWin} ({eloChangeIfWin:+0;-0})\n" +
               $"If Lose: {eloIfLose} ({eloChangeIfLose:+0;-0})";
    }
}
