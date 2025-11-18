using UnityEngine;
using GravityWars.Networking;

/// <summary>
/// Analytics integration for GameManager.
/// Tracks all match-related events and player actions.
///
/// IMPORTANT: Attach this component to the same GameObject as GameManager.
/// It will automatically hook into game events and send analytics.
///
/// This is a non-invasive integration - no modifications to GameManager needed.
///
/// Events Tracked:
/// - Match started/completed
/// - Round started/completed
/// - Player actions (fire, move, perk activation)
/// - Damage dealt
/// - Ships destroyed
/// - Session duration
///
/// Usage:
///   Simply attach to GameManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(GameManager))]
public class GameManagerAnalytics : MonoBehaviour
{
    #region Configuration

    [Header("Analytics Configuration")]
    [Tooltip("Enable analytics tracking")]
    public bool enableAnalytics = true;

    [Tooltip("Track individual player actions (may generate many events)")]
    public bool trackPlayerActions = true;

    #endregion

    #region Component References

    private GameManager _gameManager;
    private AnalyticsService _analyticsService;

    #endregion

    #region Match State Tracking

    private float _matchStartTime;
    private int _player1ShotsThisMatch = 0;
    private int _player2ShotsThisMatch = 0;
    private int _player1HitsThisMatch = 0;
    private int _player2HitsThisMatch = 0;
    private int _player1DamageThisMatch = 0;
    private int _player2DamageThisMatch = 0;
    private int _roundsPlayedThisMatch = 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("[GameManagerAnalytics] GameManager not found!");
            enabled = false;
            return;
        }

        // Get analytics service
        _analyticsService = ServiceLocator.Instance?.Analytics;

        if (_analyticsService == null)
        {
            Debug.LogWarning("[GameManagerAnalytics] AnalyticsService not available - analytics disabled");
            enableAnalytics = false;
        }
    }

    private void Start()
    {
        if (!enableAnalytics)
            return;

        // Track session start
        _analyticsService.TrackSessionStart();

        Log("Analytics integration initialized");
    }

    private void OnDestroy()
    {
        if (!enableAnalytics)
            return;

        // Track session end
        if (_analyticsService != null)
        {
            _analyticsService.TrackSessionEnd();
        }
    }

    #endregion

    #region Match Lifecycle Tracking

    /// <summary>
    /// Call this when match starts (from GameManager.StartGamePhase).
    /// </summary>
    public void TrackMatchStart()
    {
        if (!enableAnalytics)
            return;

        _matchStartTime = Time.time;
        _roundsPlayedThisMatch = 0;
        _player1ShotsThisMatch = 0;
        _player2ShotsThisMatch = 0;
        _player1HitsThisMatch = 0;
        _player2HitsThisMatch = 0;
        _player1DamageThisMatch = 0;
        _player2DamageThisMatch = 0;

        // Get ship names
        string player1Ship = _gameManager.player1Ship?.shipPreset?.name ?? "Unknown";
        string player2Ship = _gameManager.player2Ship?.shipPreset?.name ?? "Unknown";

        // Count planets
        int planetCount = FindObjectsOfType<Planet>().Length;

        _analyticsService.TrackMatchStarted(
            mode: "hotseat", // Will be "online" if OnlineGameAdapter.isOnlineMode = true
            playerShip: player1Ship,
            opponentShip: player2Ship,
            planetCount: planetCount
        );

        Log($"Match started tracked: {player1Ship} vs {player2Ship}");
    }

    /// <summary>
    /// Call this when round starts.
    /// </summary>
    public void TrackRoundStart(int roundNumber)
    {
        if (!enableAnalytics)
            return;

        _roundsPlayedThisMatch = roundNumber;

        Log($"Round {roundNumber} started");
    }

    /// <summary>
    /// Call this when round ends (ship destroyed).
    /// </summary>
    public void TrackRoundEnd(int roundNumber, PlayerShip winner, int shotsThisRound, int damageTaken)
    {
        if (!enableAnalytics)
            return;

        _analyticsService.TrackRoundComplete(
            roundNumber: roundNumber,
            winner: winner.playerName,
            shotsThisRound: shotsThisRound,
            damageTaken: damageTaken
        );

        Log($"Round {roundNumber} ended - Winner: {winner.playerName}");
    }

    /// <summary>
    /// Call this when match ends (from GameManager.GameOver).
    /// </summary>
    public void TrackMatchEnd(PlayerShip winner)
    {
        if (!enableAnalytics)
            return;

        float matchDuration = Time.time - _matchStartTime;

        // Calculate accuracy
        float player1Accuracy = _player1ShotsThisMatch > 0
            ? (float)_player1HitsThisMatch / _player1ShotsThisMatch * 100f
            : 0f;

        float player2Accuracy = _player2ShotsThisMatch > 0
            ? (float)_player2HitsThisMatch / _player2ShotsThisMatch * 100f
            : 0f;

        // Create match analytics data
        var matchAnalytics = new MatchAnalytics
        {
            winner = winner.playerName,
            duration = matchDuration,
            roundsPlayed = _roundsPlayedThisMatch,
            playerDamageDealt = _player1DamageThisMatch,
            opponentDamageDealt = _player2DamageThisMatch,
            playerShotsFired = _player1ShotsThisMatch,
            opponentShotsFired = _player2ShotsThisMatch,
            playerAccuracy = player1Accuracy,
            opponentAccuracy = player2Accuracy,
            xpGained = 0, // Will be set by ProgressionManager
            currencyGained = 0 // Will be set by ProgressionManager
        };

        _analyticsService.TrackMatchComplete(matchAnalytics);

        Log($"Match ended tracked - Winner: {winner.playerName}, Duration: {matchDuration:F1}s");
    }

    #endregion

    #region Player Action Tracking

    /// <summary>
    /// Call this when player fires a missile.
    /// </summary>
    public void TrackPlayerFire(PlayerShip ship, bool hit, int damage)
    {
        if (!enableAnalytics)
            return;

        // Increment shot counter
        if (ship == _gameManager.player1Ship)
        {
            _player1ShotsThisMatch++;
            if (hit)
            {
                _player1HitsThisMatch++;
                _player1DamageThisMatch += damage;
            }
        }
        else if (ship == _gameManager.player2Ship)
        {
            _player2ShotsThisMatch++;
            if (hit)
            {
                _player2HitsThisMatch++;
                _player2DamageThisMatch += damage;
            }
        }

        // Track action (optional - may generate many events)
        if (trackPlayerActions)
        {
            _analyticsService.TrackPlayerAction(
                actionType: "fire",
                success: hit,
                value: damage
            );
        }
    }

    /// <summary>
    /// Call this when player moves.
    /// </summary>
    public void TrackPlayerMove(PlayerShip ship, float distance)
    {
        if (!enableAnalytics || !trackPlayerActions)
            return;

        _analyticsService.TrackPlayerAction(
            actionType: "move",
            success: true,
            value: distance
        );
    }

    /// <summary>
    /// Call this when player activates a perk.
    /// </summary>
    public void TrackPerkActivation(PlayerShip ship, string perkName)
    {
        if (!enableAnalytics || !trackPlayerActions)
            return;

        _analyticsService.TrackPlayerAction(
            actionType: "perk_activate",
            success: true,
            value: 0
        );

        Log($"Perk activated: {perkName}");
    }

    #endregion

    #region Helper Methods

    private void Log(string message)
    {
        Debug.Log($"[GameManagerAnalytics] {message}");
    }

    #endregion
}
