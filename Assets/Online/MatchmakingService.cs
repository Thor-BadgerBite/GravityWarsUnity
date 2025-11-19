using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

/// <summary>
/// Manages competitive and casual matchmaking for Gravity Wars.
/// Handles queue management, ELO-based pairing for rated matches,
/// and quick match for unrated casual games.
/// </summary>
public class MatchmakingService : MonoBehaviour
{
    #region Singleton

    public static MatchmakingService Instance { get; private set; }

    #endregion

    #region Configuration

    [Header("Matchmaking Settings")]
    [SerializeField] private int initialELORange = 100;      // Start with ±100 ELO
    [SerializeField] private int maxELORange = 400;          // Max ±400 ELO
    [SerializeField] private float rangeExpansionRate = 50f; // Expand by 50 ELO every interval
    [SerializeField] private float rangeExpansionInterval = 5f; // Expand every 5 seconds

    [Header("Queue Settings")]
    [SerializeField] private float queueUpdateRate = 1f;     // Check queue every second
    [SerializeField] private int maxQueueTime = 120;         // Max 2 minutes in queue

    [Header("Match Settings")]
    [SerializeField] private int roundsToWin = 3;            // Best of 5 (first to 3 wins)
    [SerializeField] private float turnTimeLimit = 60f;      // 60 seconds per turn

    #endregion

    #region State

    // Queue management
    private List<MatchmakingQueueEntry> _rankedQueue = new List<MatchmakingQueueEntry>();
    private List<MatchmakingQueueEntry> _casualQueue = new List<MatchmakingQueueEntry>();

    // Active matchmaking tickets
    private Dictionary<string, MatchmakingTicket> _activeTickets = new Dictionary<string, MatchmakingTicket>();

    // Timers
    private float _queueUpdateTimer = 0f;

    #endregion

    #region Events

    public event Action<MatchFoundData> OnMatchFound;
    public event Action<string> OnMatchmakingCanceled;
    public event Action<float> OnQueueTimeUpdated;

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

    private void Update()
    {
        _queueUpdateTimer += Time.deltaTime;
        if (_queueUpdateTimer >= queueUpdateRate)
        {
            _queueUpdateTimer = 0f;
            ProcessMatchmakingQueues();
        }
    }

    #endregion

    #region Public API - Queue Management

    /// <summary>
    /// Join ranked matchmaking queue with ELO-based pairing.
    /// </summary>
    public void JoinRankedQueue(string playerId, PlayerProfileData profile)
    {
        if (IsPlayerInQueue(playerId))
        {
            Debug.LogWarning($"[Matchmaking] Player {playerId} already in queue!");
            return;
        }

        var entry = new MatchmakingQueueEntry
        {
            playerId = playerId,
            playerProfile = profile,
            eloRating = profile.eloRating,
            isRanked = true,
            joinTime = Time.time,
            currentELORange = initialELORange
        };

        _rankedQueue.Add(entry);

        Debug.Log($"[Matchmaking] {profile.username} joined RANKED queue (ELO: {profile.eloRating})");
    }

    /// <summary>
    /// Join casual matchmaking queue (no ELO restrictions).
    /// </summary>
    public void JoinCasualQueue(string playerId, PlayerProfileData profile)
    {
        if (IsPlayerInQueue(playerId))
        {
            Debug.LogWarning($"[Matchmaking] Player {playerId} already in queue!");
            return;
        }

        var entry = new MatchmakingQueueEntry
        {
            playerId = playerId,
            playerProfile = profile,
            eloRating = profile.eloRating,
            isRanked = false,
            joinTime = Time.time,
            currentELORange = int.MaxValue  // No ELO restriction for casual
        };

        _casualQueue.Add(entry);

        Debug.Log($"[Matchmaking] {profile.username} joined CASUAL queue");
    }

    /// <summary>
    /// Leave matchmaking queue.
    /// </summary>
    public void LeaveQueue(string playerId)
    {
        bool removed = false;

        // Remove from ranked queue
        var rankedEntry = _rankedQueue.Find(e => e.playerId == playerId);
        if (rankedEntry != null)
        {
            _rankedQueue.Remove(rankedEntry);
            removed = true;
        }

        // Remove from casual queue
        var casualEntry = _casualQueue.Find(e => e.playerId == playerId);
        if (casualEntry != null)
        {
            _casualQueue.Remove(casualEntry);
            removed = true;
        }

        if (removed)
        {
            Debug.Log($"[Matchmaking] Player {playerId} left queue");
            OnMatchmakingCanceled?.Invoke(playerId);
        }
    }

    /// <summary>
    /// Check if player is currently in any queue.
    /// </summary>
    public bool IsPlayerInQueue(string playerId)
    {
        return _rankedQueue.Any(e => e.playerId == playerId) ||
               _casualQueue.Any(e => e.playerId == playerId);
    }

    /// <summary>
    /// Get current queue time for player.
    /// </summary>
    public float GetQueueTime(string playerId)
    {
        var entry = _rankedQueue.Find(e => e.playerId == playerId) ??
                    _casualQueue.Find(e => e.playerId == playerId);

        if (entry != null)
        {
            return Time.time - entry.joinTime;
        }

        return 0f;
    }

    /// <summary>
    /// Get estimated wait time based on queue size and ELO.
    /// </summary>
    public int GetEstimatedWaitTime(bool isRanked, int playerELO)
    {
        var queue = isRanked ? _rankedQueue : _casualQueue;

        if (queue.Count == 0)
        {
            return 30; // Estimate 30 seconds if queue is empty
        }

        if (!isRanked)
        {
            return 10; // Casual matches pair quickly
        }

        // For ranked, estimate based on similar ELO players in queue
        int similarELOCount = queue.Count(e => Math.Abs(e.eloRating - playerELO) <= initialELORange);

        if (similarELOCount > 0)
        {
            return 15; // Quick match with similar ELO
        }
        else
        {
            return 45; // Longer wait for ELO range expansion
        }
    }

    #endregion

    #region Matchmaking Processing

    /// <summary>
    /// Process both ranked and casual queues to find matches.
    /// </summary>
    private void ProcessMatchmakingQueues()
    {
        // Process ranked queue (ELO-based pairing)
        ProcessRankedQueue();

        // Process casual queue (first-come-first-served)
        ProcessCasualQueue();

        // Update queue times and expand ELO ranges
        UpdateQueueEntries();
    }

    /// <summary>
    /// Process ranked queue with ELO-based matchmaking.
    /// </summary>
    private void ProcessRankedQueue()
    {
        if (_rankedQueue.Count < 2) return;

        // Sort by queue time (prioritize longest waiting)
        _rankedQueue.Sort((a, b) => a.joinTime.CompareTo(b.joinTime));

        var matchedPairs = new List<(MatchmakingQueueEntry, MatchmakingQueueEntry)>();

        // Try to pair players
        for (int i = 0; i < _rankedQueue.Count; i++)
        {
            var player1 = _rankedQueue[i];
            if (player1.isMatched) continue;

            // Find best opponent within ELO range
            MatchmakingQueueEntry bestOpponent = null;
            int bestEloDifference = int.MaxValue;

            for (int j = i + 1; j < _rankedQueue.Count; j++)
            {
                var player2 = _rankedQueue[j];
                if (player2.isMatched) continue;

                int eloDifference = Math.Abs(player1.eloRating - player2.eloRating);

                // Check if within both players' current ELO ranges
                if (eloDifference <= player1.currentELORange &&
                    eloDifference <= player2.currentELORange)
                {
                    if (eloDifference < bestEloDifference)
                    {
                        bestEloDifference = eloDifference;
                        bestOpponent = player2;
                    }
                }
            }

            // If found suitable opponent, create match
            if (bestOpponent != null)
            {
                player1.isMatched = true;
                bestOpponent.isMatched = true;
                matchedPairs.Add((player1, bestOpponent));

                Debug.Log($"[Matchmaking] RANKED match found - {player1.playerProfile.username} (ELO: {player1.eloRating}) vs {bestOpponent.playerProfile.username} (ELO: {bestOpponent.eloRating}) - Difference: {bestEloDifference}");
            }
        }

        // Create matches and remove from queue
        foreach (var (player1, player2) in matchedPairs)
        {
            CreateMatch(player1, player2, true);
            _rankedQueue.Remove(player1);
            _rankedQueue.Remove(player2);
        }
    }

    /// <summary>
    /// Process casual queue (no ELO restrictions).
    /// </summary>
    private void ProcessCasualQueue()
    {
        if (_casualQueue.Count < 2) return;

        // Sort by queue time (first-come-first-served)
        _casualQueue.Sort((a, b) => a.joinTime.CompareTo(b.joinTime));

        // Pair first two players
        var player1 = _casualQueue[0];
        var player2 = _casualQueue[1];

        Debug.Log($"[Matchmaking] CASUAL match found - {player1.playerProfile.username} vs {player2.playerProfile.username}");

        CreateMatch(player1, player2, false);
        _casualQueue.RemoveRange(0, 2);
    }

    /// <summary>
    /// Update queue entries (expand ELO ranges, check timeouts).
    /// </summary>
    private void UpdateQueueEntries()
    {
        float currentTime = Time.time;

        // Update ranked queue entries
        for (int i = _rankedQueue.Count - 1; i >= 0; i--)
        {
            var entry = _rankedQueue[i];
            float queueTime = currentTime - entry.joinTime;

            // Expand ELO range over time
            int expansions = Mathf.FloorToInt(queueTime / rangeExpansionInterval);
            entry.currentELORange = Mathf.Min(
                initialELORange + (int)(expansions * rangeExpansionRate),
                maxELORange
            );

            // Notify player of queue time update
            OnQueueTimeUpdated?.Invoke(queueTime);

            // Check for timeout
            if (queueTime > maxQueueTime)
            {
                Debug.LogWarning($"[Matchmaking] Queue timeout for {entry.playerProfile.username}");
                _rankedQueue.RemoveAt(i);
                OnMatchmakingCanceled?.Invoke(entry.playerId);
            }
        }

        // Update casual queue entries (check timeouts only)
        for (int i = _casualQueue.Count - 1; i >= 0; i--)
        {
            var entry = _casualQueue[i];
            float queueTime = currentTime - entry.joinTime;

            // Notify player of queue time update
            OnQueueTimeUpdated?.Invoke(queueTime);

            // Check for timeout
            if (queueTime > maxQueueTime)
            {
                Debug.LogWarning($"[Matchmaking] Queue timeout for {entry.playerProfile.username}");
                _casualQueue.RemoveAt(i);
                OnMatchmakingCanceled?.Invoke(entry.playerId);
            }
        }
    }

    #endregion

    #region Match Creation

    /// <summary>
    /// Create a match between two players.
    /// </summary>
    private void CreateMatch(MatchmakingQueueEntry player1, MatchmakingQueueEntry player2, bool isRanked)
    {
        // Generate unique match ID
        string matchId = Guid.NewGuid().ToString();

        // Generate random seed for deterministic arena
        int randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        // Create match data
        var matchData = new MatchFoundData
        {
            matchId = matchId,
            player1Id = player1.playerId,
            player2Id = player2.playerId,
            player1Profile = player1.playerProfile,
            player2Profile = player2.playerProfile,
            isRanked = isRanked,
            randomSeed = randomSeed,
            roundsToWin = roundsToWin,
            turnTimeLimit = turnTimeLimit,
            serverAddress = GetAvailableServerAddress()
        };

        // Calculate match quality (for ranked matches)
        if (isRanked)
        {
            int eloDifference = Math.Abs(player1.eloRating - player2.eloRating);
            float winProbability = ELORatingSystem.GetWinProbability(player1.eloRating, player2.eloRating);

            matchData.eloDifference = eloDifference;
            matchData.player1WinProbability = winProbability;
            matchData.matchQuality = CalculateMatchQuality(eloDifference);

            Debug.Log($"[Matchmaking] Match quality: {matchData.matchQuality:F1}% (ELO diff: {eloDifference}, P1 win prob: {winProbability:F1}%)");
        }
        else
        {
            matchData.matchQuality = 100f; // Casual matches are always "fair"
        }

        // Notify both players
        OnMatchFound?.Invoke(matchData);

        Debug.Log($"[Matchmaking] Match created - ID: {matchId}, Type: {(isRanked ? "RANKED" : "CASUAL")}");
    }

    /// <summary>
    /// Calculate match quality based on ELO difference (0-100%).
    /// Perfect match (same ELO) = 100%, max range = 0%.
    /// </summary>
    private float CalculateMatchQuality(int eloDifference)
    {
        float quality = 100f - (eloDifference / (float)maxELORange * 100f);
        return Mathf.Clamp(quality, 0f, 100f);
    }

    /// <summary>
    /// Get available server address for match.
    /// TODO: Implement proper server allocation (Unity Multiplay)
    /// </summary>
    private string GetAvailableServerAddress()
    {
        // Placeholder - will use Unity Multiplay or Relay
        // For now, use host-client model (player 1 hosts)
        return "HOST";
    }

    #endregion

    #region Queue Statistics

    /// <summary>
    /// Get current queue statistics.
    /// </summary>
    public QueueStatistics GetQueueStatistics()
    {
        return new QueueStatistics
        {
            rankedQueueSize = _rankedQueue.Count,
            casualQueueSize = _casualQueue.Count,
            averageRankedWaitTime = CalculateAverageWaitTime(_rankedQueue),
            averageCasualWaitTime = CalculateAverageWaitTime(_casualQueue),
            averageRankedELO = CalculateAverageELO(_rankedQueue)
        };
    }

    private float CalculateAverageWaitTime(List<MatchmakingQueueEntry> queue)
    {
        if (queue.Count == 0) return 0f;

        float totalWaitTime = 0f;
        float currentTime = Time.time;

        foreach (var entry in queue)
        {
            totalWaitTime += currentTime - entry.joinTime;
        }

        return totalWaitTime / queue.Count;
    }

    private int CalculateAverageELO(List<MatchmakingQueueEntry> queue)
    {
        if (queue.Count == 0) return 1200;

        int totalELO = 0;
        foreach (var entry in queue)
        {
            totalELO += entry.eloRating;
        }

        return totalELO / queue.Count;
    }

    #endregion
}

/// <summary>
/// Matchmaking queue entry.
/// </summary>
[Serializable]
public class MatchmakingQueueEntry
{
    public string playerId;
    public PlayerProfileData playerProfile;
    public int eloRating;
    public bool isRanked;
    public float joinTime;
    public int currentELORange;  // Expands over time
    public bool isMatched;
}

/// <summary>
/// Match found data sent to both players.
/// </summary>
[Serializable]
public class MatchFoundData
{
    public string matchId;
    public string player1Id;
    public string player2Id;
    public PlayerProfileData player1Profile;
    public PlayerProfileData player2Profile;
    public bool isRanked;
    public int randomSeed;
    public int roundsToWin;
    public float turnTimeLimit;
    public string serverAddress;

    // Match quality info (ranked only)
    public int eloDifference;
    public float player1WinProbability;
    public float matchQuality;  // 0-100%
}

/// <summary>
/// Matchmaking ticket (for Unity Matchmaker integration).
/// </summary>
[Serializable]
public class MatchmakingTicket
{
    public string ticketId;
    public string playerId;
    public bool isRanked;
    public float creationTime;
}

/// <summary>
/// Queue statistics for UI display.
/// </summary>
[Serializable]
public class QueueStatistics
{
    public int rankedQueueSize;
    public int casualQueueSize;
    public float averageRankedWaitTime;
    public float averageCasualWaitTime;
    public int averageRankedELO;
}
