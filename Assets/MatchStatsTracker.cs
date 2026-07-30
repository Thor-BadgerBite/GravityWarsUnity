using UnityEngine;

/// <summary>
/// Tracks per-match combat statistics for both players (damage, missiles fired/hit).
/// Used by the post-match results screen, progression rewards, quests and achievements.
///
/// Lives on the GameManager GameObject (auto-added if missing).
/// PlayerShip reports damage taken and missiles fired; the tracker attributes
/// damage to the opponent (2-player game).
/// </summary>
public class MatchStatsTracker : MonoBehaviour
{
    public static MatchStatsTracker Instance { get; private set; }

    /// <summary>
    /// Combat statistics for one player over the whole match.
    /// </summary>
    [System.Serializable]
    public class PlayerStats
    {
        public int damageDealt;
        public int damageReceived;
        public int missilesFired;
        public int missilesHit;
        public int roundsWon;
        public int perksUsed;

        public float Accuracy => missilesFired > 0 ? (float)missilesHit / missilesFired : 0f;

        public void Reset()
        {
            damageDealt = 0;
            damageReceived = 0;
            missilesFired = 0;
            missilesHit = 0;
            roundsWon = 0;
            perksUsed = 0;
        }
    }

    public PlayerStats player1Stats = new PlayerStats();
    public PlayerStats player2Stats = new PlayerStats();

    private float _matchStartTime;

    public float MatchDurationSeconds => Time.time - _matchStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Ensures a tracker exists in the scene (attached to the GameManager if possible).
    /// </summary>
    public static MatchStatsTracker GetOrCreate()
    {
        if (Instance != null) return Instance;

        var host = GameManager.Instance != null ? GameManager.Instance.gameObject : new GameObject("MatchStatsTracker");
        return host.AddComponent<MatchStatsTracker>();
    }

    /// <summary>
    /// Call at the start of a new match.
    /// </summary>
    public void ResetMatch()
    {
        player1Stats.Reset();
        player2Stats.Reset();
        _matchStartTime = Time.time;
    }

    private PlayerStats StatsFor(PlayerShip ship)
    {
        if (ship == null) return null;
        return ship.isLeftPlayer ? player1Stats : player2Stats;
    }

    private PlayerStats OpponentStatsFor(PlayerShip ship)
    {
        if (ship == null) return null;
        return ship.isLeftPlayer ? player2Stats : player1Stats;
    }

    /// <summary>
    /// Called by PlayerShip when it fires a missile (any type).
    /// </summary>
    public void RecordMissileFired(PlayerShip shooter)
    {
        var stats = StatsFor(shooter);
        if (stats != null) stats.missilesFired++;
    }

    /// <summary>
    /// Called by PlayerShip when it takes damage from a missile.
    /// Damage dealt is attributed to the opponent, and the hit counts
    /// toward the opponent's accuracy.
    /// </summary>
    public void RecordDamageTaken(PlayerShip victim, float effectiveDamage)
    {
        int dmg = Mathf.RoundToInt(effectiveDamage);

        var victimStats = StatsFor(victim);
        if (victimStats != null) victimStats.damageReceived += dmg;

        var attackerStats = OpponentStatsFor(victim);
        if (attackerStats != null)
        {
            attackerStats.damageDealt += dmg;
            attackerStats.missilesHit++;
        }
    }

    /// <summary>
    /// Called when a round is won.
    /// </summary>
    public void RecordRoundWon(PlayerShip winner)
    {
        var stats = StatsFor(winner);
        if (stats != null) stats.roundsWon++;
    }

    /// <summary>
    /// Called when a player activates a perk.
    /// </summary>
    public void RecordPerkUsed(PlayerShip user)
    {
        var stats = StatsFor(user);
        if (stats != null) stats.perksUsed++;
    }

    /// <summary>
    /// Gets the stats for a specific ship.
    /// </summary>
    public PlayerStats GetStats(PlayerShip ship) => StatsFor(ship);

    public PlayerStats GetStats(bool isLeftPlayer) => isLeftPlayer ? player1Stats : player2Stats;
}
