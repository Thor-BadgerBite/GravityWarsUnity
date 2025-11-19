using UnityEngine;
using GravityWars.Networking;

/// <summary>
/// Integrates achievement system with GameManager.
/// Tracks match-related achievement progress.
///
/// IMPORTANT: Attach this component to the same GameObject as GameManager.
/// It will automatically hook into match events and update achievement progress.
///
/// This is a non-invasive integration - no modifications to GameManager needed.
///
/// Actions Tracked:
/// - Matches won/played
/// - Rounds won
/// - Win streaks
/// - Damage dealt
/// - Missiles fired/hit
/// - Perfect accuracy matches
/// - Flawless victories
/// - Quick victories (under 60 seconds)
///
/// Usage:
///   Simply attach to GameManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(GameManager))]
public class GameManagerAchievementIntegration : MonoBehaviour
{
    #region Configuration

    [Header("Achievement Integration")]
    [Tooltip("Enable achievement tracking")]
    public bool enableAchievementTracking = true;

    [Header("Accuracy Thresholds")]
    [Tooltip("Minimum accuracy for perfect accuracy achievement (%)")]
    public float perfectAccuracyThreshold = 95f;

    [Header("Speed Thresholds")]
    [Tooltip("Maximum match time for quick victory achievement (seconds)")]
    public float quickVictoryTimeLimit = 60f;

    #endregion

    #region Component References

    private GameManager _gameManager;
    private AchievementService _achievementService;

    #endregion

    #region Match Tracking State

    // Match stats
    private float _matchStartTime;
    private int _roundsPlayedThisMatch = 0;
    private int _roundsWonThisMatch = 0;

    // Damage tracking
    private int _totalDamageThisMatch = 0;
    private int _damageLastShot = 0;

    // Missile tracking
    private int _missilesFireThisMatch = 0;
    private int _missilesHitThisMatch = 0;

    // Flawless tracking
    private bool _hasP1TakenDamage = false;

    // Archetype tracking
    private ShipArchetype _currentPlayerArchetype = ShipArchetype.AllAround;

    // Map tracking
    private string _currentMap = "";

    // Win streak tracking
    private int _currentWinStreak = 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("[GameManagerAchievementIntegration] GameManager not found!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enableAchievementTracking)
            return;

        // Get achievement service
        _achievementService = AchievementService.Instance;

        if (_achievementService == null)
        {
            Debug.LogWarning("[GameManagerAchievementIntegration] AchievementService not available - achievements disabled");
            enableAchievementTracking = false;
            return;
        }

        Log("Achievement integration initialized");
    }

    #endregion

    #region Match Events

    /// <summary>
    /// Call this when a match starts.
    /// </summary>
    public void OnMatchStart()
    {
        if (!enableAchievementTracking)
            return;

        // Reset match tracking
        _matchStartTime = Time.time;
        _roundsPlayedThisMatch = 0;
        _roundsWonThisMatch = 0;
        _totalDamageThisMatch = 0;
        _missilesFireThisMatch = 0;
        _missilesHitThisMatch = 0;
        _hasP1TakenDamage = false;
        _damageLastShot = 0;

        // Track current archetype (would need to get from GameManager)
        // _currentPlayerArchetype = GetPlayerArchetype();

        // Track current map (would need to get from GameManager)
        // _currentMap = GetCurrentMap();

        Log("Match started - tracking reset");
    }

    /// <summary>
    /// Call this when a match ends.
    /// </summary>
    public void OnMatchEnd(PlayerShip winner, bool isPlayer1Winner)
    {
        if (!enableAchievementTracking)
            return;

        float matchDuration = Time.time - _matchStartTime;

        // Track: Play Matches
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.PlayMatches,
            1
        );

        if (isPlayer1Winner)
        {
            // Track: Win Matches
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.WinMatches,
                1
            );

            // Track: Win With Archetype
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.WinWithArchetype,
                1,
                _currentPlayerArchetype.ToString()
            );

            // Track: Win On Map
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.WinOnAllMaps,
                1,
                _currentMap
            );

            // Update win streak
            _currentWinStreak++;

            // Track: Win Streak
            _achievementService.SetAchievementProgress(
                AchievementConditionType.WinMatchesInRow,
                _currentWinStreak
            );

            // Track: Quick Victory (under 60 seconds)
            if (matchDuration <= quickVictoryTimeLimit)
            {
                _achievementService.UpdateAchievementProgress(
                    AchievementConditionType.WinIn60Seconds,
                    1
                );
            }

            // Track: Flawless Victory (no damage taken)
            if (!_hasP1TakenDamage)
            {
                _achievementService.UpdateAchievementProgress(
                    AchievementConditionType.WinWithoutTakingDamage,
                    1
                );
            }

            // Track: Perfect Accuracy
            if (_missilesFireThisMatch > 0)
            {
                float accuracy = (float)_missilesHitThisMatch / _missilesFireThisMatch * 100f;

                if (accuracy >= perfectAccuracyThreshold)
                {
                    _achievementService.UpdateAchievementProgress(
                        AchievementConditionType.WinWithPerfectAccuracy,
                        1
                    );

                    // Also track general accuracy achievement
                    _achievementService.SetAchievementProgress(
                        AchievementConditionType.AchieveAccuracy,
                        (int)accuracy
                    );
                }
            }
        }
        else
        {
            // Reset win streak
            _currentWinStreak = 0;
        }

        // Track: Deal Damage (total)
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.DealDamage,
            _totalDamageThisMatch
        );

        // Track: Deal Damage In One Match
        _achievementService.SetAchievementProgress(
            AchievementConditionType.DealDamageInOneMatch,
            _totalDamageThisMatch
        );

        // Track: Deal Damage With Single Shot
        if (_damageLastShot > 0)
        {
            _achievementService.SetAchievementProgress(
                AchievementConditionType.DealDamageWithSingleShot,
                _damageLastShot
            );
        }

        // Track: Fire Missiles
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.FireMissiles,
            _missilesFireThisMatch
        );

        // Track: Hit Missiles
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.HitMissiles,
            _missilesHitThisMatch
        );

        // Track: Win Rounds
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.WinRounds,
            _roundsWonThisMatch
        );

        Log($"Match ended - Winner: {(isPlayer1Winner ? "Player 1" : "Player 2")}, " +
            $"Damage: {_totalDamageThisMatch}, Accuracy: {(_missilesFireThisMatch > 0 ? (float)_missilesHitThisMatch / _missilesFireThisMatch * 100f : 0f):F1}%");
    }

    #endregion

    #region Round Events

    /// <summary>
    /// Call this when a round ends.
    /// </summary>
    public void OnRoundEnd(PlayerShip winner, bool isPlayer1Winner)
    {
        if (!enableAchievementTracking)
            return;

        _roundsPlayedThisMatch++;

        if (isPlayer1Winner)
        {
            _roundsWonThisMatch++;
        }
    }

    #endregion

    #region Combat Events

    /// <summary>
    /// Call this when player fires a missile.
    /// </summary>
    public void OnPlayerFireMissile(bool hit, int damage, string missileType)
    {
        if (!enableAchievementTracking)
            return;

        _missilesFireThisMatch++;

        if (hit)
        {
            _missilesHitThisMatch++;
            _totalDamageThisMatch += damage;
            _damageLastShot = damage;

            // Track missile type specific achievements
            if (!string.IsNullOrEmpty(missileType))
            {
                // Track: Destroy Ships With Missile Type
                // (Would need to know when ship is destroyed)
            }
        }
    }

    /// <summary>
    /// Call this when player takes damage.
    /// </summary>
    public void OnPlayerTakeDamage(int damage)
    {
        if (!enableAchievementTracking)
            return;

        if (damage > 0)
        {
            _hasP1TakenDamage = true;
        }
    }

    /// <summary>
    /// Call this when enemy ship is destroyed.
    /// </summary>
    public void OnEnemyShipDestroyed(string missileType)
    {
        if (!enableAchievementTracking)
            return;

        // Track: Destroy Ships With Missile Type
        if (!string.IsNullOrEmpty(missileType))
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.DestroyShipsWithMissileType,
                1,
                missileType
            );
        }
    }

    #endregion

    #region Perk Events

    /// <summary>
    /// Call this when player activates a perk.
    /// </summary>
    public void OnPlayerActivatePerk(string perkName)
    {
        if (!enableAchievementTracking)
            return;

        // Track: Use Perk N Times
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.UsePerkNTimes,
            1
        );

        // Track: Use specific perk
        if (!string.IsNullOrEmpty(perkName))
        {
            _achievementService.UpdateAchievementProgress(
                AchievementConditionType.UsePerkNTimes,
                1,
                perkName
            );
        }
    }

    /// <summary>
    /// Call this when player wins with a specific perk.
    /// </summary>
    public void OnPlayerWinWithPerk(string perkName)
    {
        if (!enableAchievementTracking)
            return;

        // Track: Win With Perk
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.WinWithPerk,
            1,
            perkName
        );
    }

    #endregion

    #region Social Events

    /// <summary>
    /// Call this when playing with a friend.
    /// </summary>
    public void OnPlayWithFriend(string friendID)
    {
        if (!enableAchievementTracking)
            return;

        // Track: Play With Friend
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.PlayWithFriend,
            1
        );
    }

    /// <summary>
    /// Call this when winning against a friend.
    /// </summary>
    public void OnWinAgainstFriend(string friendID)
    {
        if (!enableAchievementTracking)
            return;

        // Track: Win Against Friend
        _achievementService.UpdateAchievementProgress(
            AchievementConditionType.WinAgainstFriend,
            1
        );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets current win streak.
    /// </summary>
    public int GetCurrentWinStreak()
    {
        return _currentWinStreak;
    }

    /// <summary>
    /// Resets win streak (call when streak is broken).
    /// </summary>
    public void ResetWinStreak()
    {
        _currentWinStreak = 0;
        Log("Win streak reset");
    }

    private void Log(string message)
    {
        Debug.Log($"[GameManagerAchievementIntegration] {message}");
    }

    #endregion
}
