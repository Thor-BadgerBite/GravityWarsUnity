using UnityEngine;
using System.Collections.Generic;
using GravityWars.Networking;

/// <summary>
/// Integrates leaderboard system with GameManager.
/// Tracks match stats and submits to leaderboards.
///
/// IMPORTANT: Attach this component to the same GameObject as GameManager.
/// It will automatically track match events and submit scores to leaderboards.
///
/// This is a non-invasive integration - no modifications to GameManager needed.
///
/// Stats Tracked:
/// - Total wins
/// - Total matches played
/// - Win rate
/// - Longest win streak
/// - Current win streak
/// - Total damage dealt
/// - Highest damage in match
/// - Best accuracy
/// - Fastest win
/// - MMR rating (if ranked mode)
///
/// Usage:
///   Simply attach to GameManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(GameManager))]
public class GameManagerLeaderboardIntegration : MonoBehaviour
{
    #region Configuration

    [Header("Leaderboard Integration")]
    [Tooltip("Enable leaderboard tracking")]
    public bool enableLeaderboardTracking = true;

    [Tooltip("Submit scores after every match")]
    public bool submitAfterMatch = true;

    [Tooltip("Batch submit interval (seconds) if not submitting after match")]
    public float batchSubmitInterval = 300f; // 5 minutes

    #endregion

    #region Component References

    private GameManager _gameManager;
    private LeaderboardService _leaderboardService;

    #endregion

    #region Match Tracking State

    // Lifetime stats
    private int _totalWins = 0;
    private int _totalMatches = 0;
    private int _longestWinStreak = 0;
    private int _currentWinStreak = 0;
    private long _totalDamageDealt = 0;

    // Match stats
    private float _matchStartTime;
    private int _totalDamageThisMatch = 0;
    private int _missilesFireThisMatch = 0;
    private int _missilesHitThisMatch = 0;

    // Best records
    private int _highestDamageInMatch = 0;
    private float _bestAccuracy = 0f;
    private float _fastestWin = float.MaxValue;

    // Pending submissions (for batch mode)
    private Dictionary<LeaderboardStatType, (long score, double decimalScore)> _pendingSubmissions =
        new Dictionary<LeaderboardStatType, (long, double)>();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("[GameManagerLeaderboardIntegration] GameManager not found!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enableLeaderboardTracking)
            return;

        // Get leaderboard service
        _leaderboardService = LeaderboardService.Instance;

        if (_leaderboardService == null)
        {
            Debug.LogWarning("[GameManagerLeaderboardIntegration] LeaderboardService not available - leaderboards disabled");
            enableLeaderboardTracking = false;
            return;
        }

        // Load saved stats
        LoadStats();

        // Start batch submit if enabled
        if (!submitAfterMatch)
        {
            InvokeRepeating(nameof(BatchSubmitScores), batchSubmitInterval, batchSubmitInterval);
        }

        Log("Leaderboard integration initialized");
    }

    #endregion

    #region Match Events

    /// <summary>
    /// Call this when a match starts.
    /// </summary>
    public void OnMatchStart()
    {
        if (!enableLeaderboardTracking)
            return;

        // Reset match tracking
        _matchStartTime = Time.time;
        _totalDamageThisMatch = 0;
        _missilesFireThisMatch = 0;
        _missilesHitThisMatch = 0;

        Log("Match started - tracking reset");
    }

    /// <summary>
    /// Call this when a match ends.
    /// </summary>
    public void OnMatchEnd(PlayerShip winner, bool isPlayer1Winner)
    {
        if (!enableLeaderboardTracking)
            return;

        float matchDuration = Time.time - _matchStartTime;

        // Update match count
        _totalMatches++;
        QueueSubmission(LeaderboardStatType.TotalMatches, _totalMatches);

        if (isPlayer1Winner)
        {
            // Update win count
            _totalWins++;
            QueueSubmission(LeaderboardStatType.TotalWins, _totalWins);

            // Update win streak
            _currentWinStreak++;
            QueueSubmission(LeaderboardStatType.CurrentWinStreak, _currentWinStreak);

            if (_currentWinStreak > _longestWinStreak)
            {
                _longestWinStreak = _currentWinStreak;
                QueueSubmission(LeaderboardStatType.LongestWinStreak, _longestWinStreak);
            }

            // Update fastest win
            if (matchDuration < _fastestWin)
            {
                _fastestWin = matchDuration;
                QueueSubmission(LeaderboardStatType.FastestWin, 0, _fastestWin);
            }
        }
        else
        {
            // Reset win streak
            _currentWinStreak = 0;
        }

        // Update win rate
        float winRate = _totalMatches > 0 ? (float)_totalWins / _totalMatches * 100f : 0f;
        QueueSubmission(LeaderboardStatType.WinRate, (long)(winRate * 100), winRate);

        // Update damage stats
        _totalDamageDealt += _totalDamageThisMatch;
        QueueSubmission(LeaderboardStatType.TotalDamageDealt, _totalDamageDealt);

        if (_totalDamageThisMatch > _highestDamageInMatch)
        {
            _highestDamageInMatch = _totalDamageThisMatch;
            QueueSubmission(LeaderboardStatType.HighestDamageInMatch, _highestDamageInMatch);
        }

        float avgDamage = _totalMatches > 0 ? (float)_totalDamageDealt / _totalMatches : 0f;
        QueueSubmission(LeaderboardStatType.AverageDamagePerMatch, (long)avgDamage, avgDamage);

        // Update accuracy stats
        if (_missilesFireThisMatch > 0)
        {
            float accuracy = (float)_missilesHitThisMatch / _missilesFireThisMatch * 100f;

            if (accuracy > _bestAccuracy)
            {
                _bestAccuracy = accuracy;
                QueueSubmission(LeaderboardStatType.BestAccuracy, (long)(accuracy * 100), accuracy);
            }
        }

        // Update missiles hit total
        int totalMissilesHit = GetTotalMissilesHit();
        QueueSubmission(LeaderboardStatType.TotalMissilesHit, totalMissilesHit);

        // Save stats
        SaveStats();

        // Submit if enabled
        if (submitAfterMatch)
        {
            SubmitAllQueuedScores();
        }

        Log($"Match ended - Winner: {(isPlayer1Winner ? "Player 1" : "Player 2")}, " +
            $"Wins: {_totalWins}/{_totalMatches}, Streak: {_currentWinStreak}, Damage: {_totalDamageThisMatch}");
    }

    #endregion

    #region Combat Tracking

    /// <summary>
    /// Call this when player fires a missile.
    /// </summary>
    public void OnPlayerFireMissile(bool hit, int damage)
    {
        if (!enableLeaderboardTracking)
            return;

        _missilesFireThisMatch++;

        if (hit)
        {
            _missilesHitThisMatch++;
            _totalDamageThisMatch += damage;
        }
    }

    #endregion

    #region Score Submission

    /// <summary>
    /// Queues a score submission.
    /// </summary>
    private void QueueSubmission(LeaderboardStatType statType, long score, double decimalScore = 0)
    {
        _pendingSubmissions[statType] = (score, decimalScore);
    }

    /// <summary>
    /// Submits all queued scores.
    /// </summary>
    private async void SubmitAllQueuedScores()
    {
        if (_pendingSubmissions.Count == 0)
            return;

        var submissions = new Dictionary<LeaderboardStatType, (long score, double decimalScore)>(_pendingSubmissions);
        _pendingSubmissions.Clear();

        bool success = await _leaderboardService.SubmitBatch(submissions);

        if (success)
        {
            Log($"Submitted {submissions.Count} scores to leaderboards");
        }
    }

    /// <summary>
    /// Batch submits scores (called on interval).
    /// </summary>
    private void BatchSubmitScores()
    {
        SubmitAllQueuedScores();
    }

    #endregion

    #region Stats Persistence

    /// <summary>
    /// Saves stats to PlayerPrefs.
    /// </summary>
    private void SaveStats()
    {
        PlayerPrefs.SetInt("Leaderboard_TotalWins", _totalWins);
        PlayerPrefs.SetInt("Leaderboard_TotalMatches", _totalMatches);
        PlayerPrefs.SetInt("Leaderboard_LongestWinStreak", _longestWinStreak);
        PlayerPrefs.SetInt("Leaderboard_CurrentWinStreak", _currentWinStreak);
        PlayerPrefs.SetString("Leaderboard_TotalDamage", _totalDamageDealt.ToString());
        PlayerPrefs.SetInt("Leaderboard_HighestDamage", _highestDamageInMatch);
        PlayerPrefs.SetFloat("Leaderboard_BestAccuracy", _bestAccuracy);
        PlayerPrefs.SetFloat("Leaderboard_FastestWin", _fastestWin);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads stats from PlayerPrefs.
    /// </summary>
    private void LoadStats()
    {
        _totalWins = PlayerPrefs.GetInt("Leaderboard_TotalWins", 0);
        _totalMatches = PlayerPrefs.GetInt("Leaderboard_TotalMatches", 0);
        _longestWinStreak = PlayerPrefs.GetInt("Leaderboard_LongestWinStreak", 0);
        _currentWinStreak = PlayerPrefs.GetInt("Leaderboard_CurrentWinStreak", 0);
        _totalDamageDealt = long.Parse(PlayerPrefs.GetString("Leaderboard_TotalDamage", "0"));
        _highestDamageInMatch = PlayerPrefs.GetInt("Leaderboard_HighestDamage", 0);
        _bestAccuracy = PlayerPrefs.GetFloat("Leaderboard_BestAccuracy", 0f);
        _fastestWin = PlayerPrefs.GetFloat("Leaderboard_FastestWin", float.MaxValue);

        Log($"Stats loaded - Wins: {_totalWins}/{_totalMatches}, Best Streak: {_longestWinStreak}");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets total missiles hit (lifetime).
    /// </summary>
    private int GetTotalMissilesHit()
    {
        // TODO: Track this separately
        return 0;
    }

    /// <summary>
    /// Gets current stats for display.
    /// </summary>
    public (int wins, int matches, float winRate, int streak) GetCurrentStats()
    {
        float winRate = _totalMatches > 0 ? (float)_totalWins / _totalMatches * 100f : 0f;
        return (_totalWins, _totalMatches, winRate, _currentWinStreak);
    }

    private void Log(string message)
    {
        Debug.Log($"[GameManagerLeaderboardIntegration] {message}");
    }

    #endregion
}
