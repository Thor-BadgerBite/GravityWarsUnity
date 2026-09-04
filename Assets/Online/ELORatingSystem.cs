using UnityEngine;

/// <summary>
/// ELO Rating System implementation (based on chess ELO formula).
/// Used for competitive matchmaking and ranking in Gravity Wars.
///
/// Military/Naval Rank ELO Ranges (16 Ranks Total):
/// - Starting ELO: 800 (Ensign Rank)
/// - Rank 16 (Lowest): Cadet 0-699
/// - Rank 15 (Starting): Ensign 700-1049 ⭐
/// - Rank 14: Lieutenant 1050-1199
/// - Rank 13: Lieutenant Commander 1200-1349
/// - Rank 12: Commander 1350-1499
/// - Rank 11: Captain 1500-1649
/// - Rank 10: Senior Captain 1650-1799
/// - Rank 9: Commodore 1800-1949
/// - Rank 8: Rear Admiral 1950-2099
/// - Rank 7: Rear Admiral Upper Half 2100-2249
/// - Rank 6: Vice Admiral 2250-2399
/// - Rank 5: Admiral 2400-2549
/// - Rank 4: High Admiral 2550-2699
/// - Rank 3: Fleet Admiral 2700-2849
/// - Rank 2: Supreme Admiral 2850-2999
/// - Rank 1 (Highest): Grand Admiral 3000+
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
    /// Starting ELO for all new players (Ensign rank)
    /// </summary>
    public const int STARTING_ELO = 800;

    /// <summary>
    /// Minimum ELO (prevents going below this)
    /// </summary>
    public const int MINIMUM_ELO = 100;

    /// <summary>
    /// Maximum ELO (theoretical cap)
    /// </summary>
    public const int MAXIMUM_ELO = 4000;

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
    /// 16-rank system with starting rank at Ensign (700-1049 ELO).
    /// </summary>
    public static CompetitiveRank GetRankFromELO(int elo)
    {
        if (elo < 700) return CompetitiveRank.Cadet;               // 0-699
        if (elo < 1050) return CompetitiveRank.Ensign;             // 700-1049 (Starting)
        if (elo < 1200) return CompetitiveRank.Lieutenant;         // 1050-1199
        if (elo < 1350) return CompetitiveRank.LieutenantCommander; // 1200-1349
        if (elo < 1500) return CompetitiveRank.Commander;          // 1350-1499
        if (elo < 1650) return CompetitiveRank.Captain;            // 1500-1649
        if (elo < 1800) return CompetitiveRank.SeniorCaptain;      // 1650-1799
        if (elo < 1950) return CompetitiveRank.Commodore;          // 1800-1949
        if (elo < 2100) return CompetitiveRank.RearAdmiral;        // 1950-2099
        if (elo < 2250) return CompetitiveRank.RearAdmiralUpperHalf; // 2100-2249
        if (elo < 2400) return CompetitiveRank.ViceAdmiral;        // 2250-2399
        if (elo < 2550) return CompetitiveRank.Admiral;            // 2400-2549
        if (elo < 2700) return CompetitiveRank.HighAdmiral;        // 2550-2699
        if (elo < 2850) return CompetitiveRank.FleetAdmiral;       // 2700-2849
        if (elo < 3000) return CompetitiveRank.SupremeAdmiral;     // 2850-2999
        return CompetitiveRank.GrandAdmiral;                       // 3000+
    }

    /// <summary>
    /// Get ELO range for a specific rank.
    /// Returns tuple of (minELO, maxELO).
    /// </summary>
    public static (int min, int max) GetELORangeForRank(CompetitiveRank rank)
    {
        switch (rank)
        {
            case CompetitiveRank.Cadet:
                return (MINIMUM_ELO, 699);           // Rank 16
            case CompetitiveRank.Ensign:
                return (700, 1049);                   // Rank 15 (Starting)
            case CompetitiveRank.Lieutenant:
                return (1050, 1199);                  // Rank 14
            case CompetitiveRank.LieutenantCommander:
                return (1200, 1349);                  // Rank 13
            case CompetitiveRank.Commander:
                return (1350, 1499);                  // Rank 12
            case CompetitiveRank.Captain:
                return (1500, 1649);                  // Rank 11
            case CompetitiveRank.SeniorCaptain:
                return (1650, 1799);                  // Rank 10
            case CompetitiveRank.Commodore:
                return (1800, 1949);                  // Rank 9
            case CompetitiveRank.RearAdmiral:
                return (1950, 2099);                  // Rank 8
            case CompetitiveRank.RearAdmiralUpperHalf:
                return (2100, 2249);                  // Rank 7
            case CompetitiveRank.ViceAdmiral:
                return (2250, 2399);                  // Rank 6
            case CompetitiveRank.Admiral:
                return (2400, 2549);                  // Rank 5
            case CompetitiveRank.HighAdmiral:
                return (2550, 2699);                  // Rank 4
            case CompetitiveRank.FleetAdmiral:
                return (2700, 2849);                  // Rank 3
            case CompetitiveRank.SupremeAdmiral:
                return (2850, 2999);                  // Rank 2
            case CompetitiveRank.GrandAdmiral:
                return (3000, MAXIMUM_ELO);           // Rank 1 (Highest)
            default:
                return (STARTING_ELO, STARTING_ELO);
        }
    }

    /// <summary>
    /// Get rank display name formatted for UI display.
    /// Converts enum names to properly spaced titles.
    /// </summary>
    public static string GetRankDisplayName(CompetitiveRank rank)
    {
        switch (rank)
        {
            case CompetitiveRank.Cadet:
                return "Cadet";
            case CompetitiveRank.Ensign:
                return "Ensign";
            case CompetitiveRank.Lieutenant:
                return "Lieutenant";
            case CompetitiveRank.LieutenantCommander:
                return "Lieutenant Commander";
            case CompetitiveRank.Commander:
                return "Commander";
            case CompetitiveRank.Captain:
                return "Captain";
            case CompetitiveRank.SeniorCaptain:
                return "Senior Captain";
            case CompetitiveRank.Commodore:
                return "Commodore";
            case CompetitiveRank.RearAdmiral:
                return "Rear Admiral";
            case CompetitiveRank.RearAdmiralUpperHalf:
                return "Rear Admiral (Upper Half)";
            case CompetitiveRank.ViceAdmiral:
                return "Vice Admiral";
            case CompetitiveRank.Admiral:
                return "Admiral";
            case CompetitiveRank.HighAdmiral:
                return "High Admiral";
            case CompetitiveRank.FleetAdmiral:
                return "Fleet Admiral";
            case CompetitiveRank.SupremeAdmiral:
                return "Supreme Admiral";
            case CompetitiveRank.GrandAdmiral:
                return "Grand Admiral";
            default:
                return rank.ToString();
        }
    }

    /// <summary>
    /// Get rank color for UI display.
    /// Colors progress from bronze (low ranks) through silver, gold, blue, purple, to red (high ranks).
    /// </summary>
    public static Color GetRankColor(CompetitiveRank rank)
    {
        switch (rank)
        {
            case CompetitiveRank.Cadet:
                return new Color(0.5f, 0.35f, 0.15f); // Dark Bronze
            case CompetitiveRank.Ensign:
                return new Color(0.8f, 0.5f, 0.2f); // Bronze
            case CompetitiveRank.Lieutenant:
                return new Color(0.7f, 0.7f, 0.7f); // Light Silver
            case CompetitiveRank.LieutenantCommander:
                return new Color(0.9f, 0.9f, 0.95f); // Bright Silver
            case CompetitiveRank.Commander:
                return new Color(1f, 0.84f, 0f); // Gold
            case CompetitiveRank.Captain:
                return new Color(1f, 0.92f, 0.3f); // Bright Gold
            case CompetitiveRank.SeniorCaptain:
                return new Color(0.5f, 0.8f, 1f); // Sky Blue
            case CompetitiveRank.Commodore:
                return new Color(0.7f, 0.9f, 1f); // Light Blue
            case CompetitiveRank.RearAdmiral:
                return new Color(0.4f, 0.7f, 1f); // Blue
            case CompetitiveRank.RearAdmiralUpperHalf:
                return new Color(0.3f, 0.5f, 0.9f); // Deep Blue
            case CompetitiveRank.ViceAdmiral:
                return new Color(0.6f, 0.2f, 0.8f); // Purple
            case CompetitiveRank.Admiral:
                return new Color(0.8f, 0.3f, 0.9f); // Bright Purple
            case CompetitiveRank.HighAdmiral:
                return new Color(0.9f, 0.4f, 1f); // Brilliant Purple
            case CompetitiveRank.FleetAdmiral:
                return new Color(1f, 0.3f, 0.3f); // Red
            case CompetitiveRank.SupremeAdmiral:
                return new Color(1f, 0.25f, 0.25f); // Bright Red
            case CompetitiveRank.GrandAdmiral:
                return new Color(1f, 0.2f, 0.2f); // Crimson Red
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
