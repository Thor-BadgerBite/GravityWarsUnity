using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Leaderboard service for managing global, friend, and seasonal leaderboards.
    ///
    /// Features:
    /// - Multiple leaderboard types (global, friends, seasonal)
    /// - Multiple stat types (wins, damage, accuracy, streaks, etc.)
    /// - Time-based leaderboards (all-time, season, monthly, weekly, daily)
    /// - Ship-specific leaderboards
    /// - Real-time rank updates
    /// - Caching for performance
    /// - Pagination support
    /// - Friend leaderboards
    /// - Anti-cheat integration
    ///
    /// Leaderboard Flow:
    /// 1. Player completes match → SubmitScore()
    /// 2. Validate score (anti-cheat)
    /// 3. Submit to Unity Gaming Services Leaderboards
    /// 4. Cache locally for quick access
    /// 5. UI fetches leaderboard → FetchLeaderboard()
    /// 6. Display with player's rank
    ///
    /// Usage:
    ///   LeaderboardService.Instance.SubmitScore(statType, score);
    ///   LeaderboardService.Instance.FetchLeaderboard(leaderboardID);
    /// </summary>
    public class LeaderboardService : MonoBehaviour
    {
        #region Singleton

        private static LeaderboardService _instance;
        public static LeaderboardService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LeaderboardService>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[LeaderboardService]");
                        _instance = go.AddComponent<LeaderboardService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Leaderboard Configuration")]
        [Tooltip("All available leaderboard definitions")]
        public List<LeaderboardDefinition> leaderboardDefinitions = new List<LeaderboardDefinition>();

        [Tooltip("Enable debug logging")]
        public bool debugLogging = true;

        [Header("Caching")]
        [Tooltip("Cache leaderboard data for X seconds")]
        public float cacheExpiration = 300f; // 5 minutes

        [Tooltip("Auto-refresh cache in background")]
        public bool autoRefreshCache = true;

        [Tooltip("Auto-refresh interval (seconds)")]
        public float autoRefreshInterval = 60f;

        [Header("Rate Limiting")]
        [Tooltip("Max score submissions per minute")]
        public int maxSubmissionsPerMinute = 10;

        #endregion

        #region State

        private bool _isInitialized = false;
        private PlayerLeaderboardStats _playerStats;

        // Leaderboard cache
        private Dictionary<string, LeaderboardData> _leaderboardCache = new Dictionary<string, LeaderboardData>();
        private Dictionary<string, DateTime> _cacheTimestamps = new Dictionary<string, DateTime>();

        // Rate limiting
        private Queue<DateTime> _recentSubmissions = new Queue<DateTime>();

        public bool IsInitialized => _isInitialized;

        #endregion

        #region Events

        public event Action<LeaderboardData> OnLeaderboardFetchedEvent;
        public event Action<LeaderboardStatType, long> OnScoreSubmittedEvent;
        public event Action<string, int> OnRankChangedEvent;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Log("Leaderboard service created");
        }

        private void Start()
        {
            Initialize();

            // Start auto-refresh
            if (autoRefreshCache)
            {
                InvokeRepeating(nameof(RefreshAllCachedLeaderboards), autoRefreshInterval, autoRefreshInterval);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes leaderboard system.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            Log("Initializing leaderboard system...");

            // Initialize player stats
            _playerStats = new PlayerLeaderboardStats
            {
                playerID = GetPlayerID(),
                playerName = GetPlayerName(),
                lastUpdated = DateTime.UtcNow
            };

            // Load cached leaderboards
            LoadCachedLeaderboards();

            _isInitialized = true;
            Log($"Leaderboard system initialized - {leaderboardDefinitions.Count} leaderboards configured");
        }

        #endregion

        #region Score Submission

        /// <summary>
        /// Submits score to leaderboard.
        /// </summary>
        public async Task<bool> SubmitScore(LeaderboardStatType statType, long score, double decimalScore = 0)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LeaderboardService] Not initialized yet!");
                return false;
            }

            // Rate limit check
            if (!CheckRateLimit())
            {
                Debug.LogWarning("[LeaderboardService] Rate limit exceeded - too many submissions");
                return false;
            }

            // Validate score (anti-cheat)
            if (!ValidateScore(statType, score, decimalScore))
            {
                Debug.LogWarning($"[LeaderboardService] Invalid score rejected: {statType} = {score}");
                return false;
            }

            // Update player stats
            UpdatePlayerStats(statType, score, decimalScore);

            // Submit to all applicable leaderboards
            bool anySuccess = false;
            foreach (var definition in leaderboardDefinitions)
            {
                if (definition.statType == statType)
                {
                    bool success = await SubmitToLeaderboard(definition, score, decimalScore);
                    if (success)
                        anySuccess = true;
                }
            }

            if (anySuccess)
            {
                OnScoreSubmittedEvent?.Invoke(statType, score);
                Log($"Score submitted: {statType} = {score}");
            }

            return anySuccess;
        }

        /// <summary>
        /// Submits to specific leaderboard.
        /// </summary>
        private async Task<bool> SubmitToLeaderboard(LeaderboardDefinition definition, long score, double decimalScore)
        {
            try
            {
                // TODO: Integrate with Unity Gaming Services Leaderboards
                // Example:
                // var leaderboardsService = LeaderboardsService.Instance;
                // await leaderboardsService.AddPlayerScoreAsync(
                //     definition.leaderboardID,
                //     score
                // );

                // For now, simulate success
                Log($"Submitted to leaderboard: {definition.displayName} = {score}");

                // Invalidate cache for this leaderboard
                InvalidateCache(definition.GetLeaderboardKey());

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeaderboardService] Failed to submit to {definition.leaderboardID}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Submits multiple stats at once (batch).
        /// </summary>
        public async Task<bool> SubmitBatch(Dictionary<LeaderboardStatType, (long score, double decimalScore)> stats)
        {
            bool allSuccess = true;

            foreach (var kvp in stats)
            {
                bool success = await SubmitScore(kvp.Key, kvp.Value.score, kvp.Value.decimalScore);
                if (!success)
                    allSuccess = false;
            }

            return allSuccess;
        }

        #endregion

        #region Leaderboard Fetching

        /// <summary>
        /// Fetches leaderboard by ID.
        /// </summary>
        public async Task<LeaderboardData> FetchLeaderboard(
            string leaderboardID,
            int pageNumber = 0,
            int pageSize = 20,
            bool forceRefresh = false)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LeaderboardService] Not initialized yet!");
                return null;
            }

            // Find definition
            var definition = leaderboardDefinitions.Find(d => d.leaderboardID == leaderboardID);
            if (definition == null)
            {
                Debug.LogWarning($"[LeaderboardService] Leaderboard not found: {leaderboardID}");
                return null;
            }

            // Check cache
            string cacheKey = definition.GetLeaderboardKey();
            if (!forceRefresh && IsCacheValid(cacheKey))
            {
                Log($"Returning cached leaderboard: {leaderboardID}");
                return _leaderboardCache[cacheKey];
            }

            // Fetch from server
            var leaderboardData = await FetchLeaderboardFromServer(definition, pageNumber, pageSize);

            if (leaderboardData != null)
            {
                // Cache it
                _leaderboardCache[cacheKey] = leaderboardData;
                _cacheTimestamps[cacheKey] = DateTime.UtcNow;

                OnLeaderboardFetchedEvent?.Invoke(leaderboardData);
                Log($"Fetched leaderboard: {leaderboardID} ({leaderboardData.entries.Count} entries)");
            }

            return leaderboardData;
        }

        /// <summary>
        /// Fetches leaderboard from server.
        /// </summary>
        private async Task<LeaderboardData> FetchLeaderboardFromServer(
            LeaderboardDefinition definition,
            int pageNumber,
            int pageSize)
        {
            try
            {
                // TODO: Integrate with Unity Gaming Services Leaderboards
                // Example:
                // var leaderboardsService = LeaderboardsService.Instance;
                // var scoresResponse = await leaderboardsService.GetScoresAsync(
                //     definition.leaderboardID,
                //     new GetScoresOptions
                //     {
                //         Offset = pageNumber * pageSize,
                //         Limit = pageSize
                //     }
                // );

                // For now, return mock data
                var leaderboardData = GenerateMockLeaderboardData(definition, pageNumber, pageSize);

                return leaderboardData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeaderboardService] Failed to fetch leaderboard {definition.leaderboardID}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Fetches player's rank on leaderboard.
        /// </summary>
        public async Task<int> FetchPlayerRank(string leaderboardID)
        {
            var leaderboard = await FetchLeaderboard(leaderboardID);

            if (leaderboard == null)
                return -1;

            // Check if player in top list
            var entry = leaderboard.GetEntryByPlayerID(_playerStats.playerID);
            if (entry != null)
                return entry.rank;

            // Check player entry (if outside top list)
            if (leaderboard.playerEntry != null)
                return leaderboard.playerEntry.rank;

            return -1;
        }

        /// <summary>
        /// Fetches leaderboard around player's rank.
        /// </summary>
        public async Task<LeaderboardData> FetchLeaderboardAroundPlayer(string leaderboardID, int range = 10)
        {
            // Get player's rank first
            int playerRank = await FetchPlayerRank(leaderboardID);

            if (playerRank < 0)
            {
                // Player not ranked, return top leaderboard
                return await FetchLeaderboard(leaderboardID);
            }

            // Calculate page to show player in middle
            int pageSize = range * 2 + 1; // Show range above and below player
            int pageNumber = Mathf.Max(0, (playerRank - range - 1) / pageSize);

            return await FetchLeaderboard(leaderboardID, pageNumber, pageSize);
        }

        #endregion

        #region Friend Leaderboards

        /// <summary>
        /// Fetches friend leaderboard.
        /// </summary>
        public async Task<LeaderboardData> FetchFriendLeaderboard(string leaderboardID)
        {
            // TODO: Integrate with friends system
            // For now, return filtered global leaderboard

            var globalLeaderboard = await FetchLeaderboard(leaderboardID);

            if (globalLeaderboard == null)
                return null;

            // Filter to friends only
            var friendLeaderboard = new LeaderboardData
            {
                definition = globalLeaderboard.definition,
                entries = globalLeaderboard.entries.Where(e => e.isFriend || e.isSelf).ToList(),
                lastUpdated = globalLeaderboard.lastUpdated
            };

            // Update ranks
            for (int i = 0; i < friendLeaderboard.entries.Count; i++)
            {
                friendLeaderboard.entries[i].rank = i + 1;
            }

            return friendLeaderboard;
        }

        #endregion

        #region Player Stats Management

        /// <summary>
        /// Updates player stats.
        /// </summary>
        private void UpdatePlayerStats(LeaderboardStatType statType, long score, double decimalScore)
        {
            switch (statType)
            {
                case LeaderboardStatType.TotalWins:
                    _playerStats.totalWins = (int)score;
                    break;

                case LeaderboardStatType.TotalMatches:
                    _playerStats.totalMatches = (int)score;
                    _playerStats.winRate = _playerStats.totalMatches > 0
                        ? (float)_playerStats.totalWins / _playerStats.totalMatches * 100f
                        : 0f;
                    break;

                case LeaderboardStatType.LongestWinStreak:
                    _playerStats.longestWinStreak = (int)score;
                    break;

                case LeaderboardStatType.CurrentWinStreak:
                    _playerStats.currentWinStreak = (int)score;
                    break;

                case LeaderboardStatType.TotalDamageDealt:
                    _playerStats.totalDamageDealt = score;
                    break;

                case LeaderboardStatType.HighestDamageInMatch:
                    _playerStats.highestDamageInMatch = (int)score;
                    break;

                case LeaderboardStatType.BestAccuracy:
                    _playerStats.bestAccuracy = (float)decimalScore;
                    break;

                case LeaderboardStatType.FastestWin:
                    _playerStats.fastestWin = (float)decimalScore;
                    break;

                case LeaderboardStatType.MMRRating:
                    _playerStats.mmrRating = (int)score;
                    break;
            }

            _playerStats.lastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets player's current stats.
        /// </summary>
        public PlayerLeaderboardStats GetPlayerStats()
        {
            return _playerStats;
        }

        #endregion

        #region Validation & Anti-Cheat

        /// <summary>
        /// Validates score before submission (anti-cheat).
        /// </summary>
        private bool ValidateScore(LeaderboardStatType statType, long score, double decimalScore)
        {
            // Basic validation
            if (score < 0)
                return false;

            // Stat-specific validation
            switch (statType)
            {
                case LeaderboardStatType.BestAccuracy:
                case LeaderboardStatType.AverageAccuracy:
                case LeaderboardStatType.WinRate:
                    // Accuracy/win rate must be 0-100%
                    if (decimalScore < 0 || decimalScore > 100)
                        return false;
                    break;

                case LeaderboardStatType.FastestWin:
                    // Fastest win must be at least 10 seconds (impossible to win faster)
                    if (decimalScore < 10)
                        return false;
                    break;

                case LeaderboardStatType.HighestDamageInMatch:
                    // Damage in match shouldn't exceed reasonable limit
                    if (score > 10000) // Example max
                        return false;
                    break;
            }

            // TODO: Integrate with SuspiciousActivityDetector
            // Check if player has been flagged for cheating

            return true;
        }

        /// <summary>
        /// Checks rate limit for submissions.
        /// </summary>
        private bool CheckRateLimit()
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            // Remove old submissions
            while (_recentSubmissions.Count > 0 && _recentSubmissions.Peek() < oneMinuteAgo)
            {
                _recentSubmissions.Dequeue();
            }

            // Check limit
            if (_recentSubmissions.Count >= maxSubmissionsPerMinute)
                return false;

            // Add current submission
            _recentSubmissions.Enqueue(now);
            return true;
        }

        #endregion

        #region Caching

        /// <summary>
        /// Checks if cached leaderboard is still valid.
        /// </summary>
        private bool IsCacheValid(string cacheKey)
        {
            if (!_leaderboardCache.ContainsKey(cacheKey))
                return false;

            if (!_cacheTimestamps.ContainsKey(cacheKey))
                return false;

            var cacheAge = (DateTime.UtcNow - _cacheTimestamps[cacheKey]).TotalSeconds;
            return cacheAge < cacheExpiration;
        }

        /// <summary>
        /// Invalidates cache for specific leaderboard.
        /// </summary>
        private void InvalidateCache(string cacheKey)
        {
            _leaderboardCache.Remove(cacheKey);
            _cacheTimestamps.Remove(cacheKey);
        }

        /// <summary>
        /// Clears all cached leaderboards.
        /// </summary>
        public void ClearCache()
        {
            _leaderboardCache.Clear();
            _cacheTimestamps.Clear();
            Log("Leaderboard cache cleared");
        }

        /// <summary>
        /// Refreshes all cached leaderboards in background.
        /// </summary>
        private async void RefreshAllCachedLeaderboards()
        {
            var keysToRefresh = new List<string>(_leaderboardCache.Keys);

            foreach (var key in keysToRefresh)
            {
                // Find definition
                var definition = leaderboardDefinitions.Find(d => d.GetLeaderboardKey() == key);
                if (definition != null)
                {
                    await FetchLeaderboard(definition.leaderboardID, forceRefresh: true);
                }
            }
        }

        /// <summary>
        /// Saves cached leaderboards to PlayerPrefs.
        /// </summary>
        private void SaveCachedLeaderboards()
        {
            // TODO: Implement local caching for offline access
        }

        /// <summary>
        /// Loads cached leaderboards from PlayerPrefs.
        /// </summary>
        private void LoadCachedLeaderboards()
        {
            // TODO: Implement local caching for offline access
        }

        #endregion

        #region Mock Data (for testing)

        /// <summary>
        /// Generates mock leaderboard data for testing.
        /// </summary>
        private LeaderboardData GenerateMockLeaderboardData(
            LeaderboardDefinition definition,
            int pageNumber,
            int pageSize)
        {
            var data = new LeaderboardData
            {
                definition = definition,
                lastUpdated = DateTime.UtcNow,
                nextReset = definition.autoReset ? definition.nextResetTime : DateTime.MaxValue
            };

            // Generate mock entries
            int startRank = pageNumber * pageSize + 1;
            for (int i = 0; i < pageSize; i++)
            {
                int rank = startRank + i;
                long score = 10000 - (rank * 100); // Decreasing scores

                var entry = new LeaderboardEntry
                {
                    playerID = $"player_{rank}",
                    playerName = $"Player {rank}",
                    accountLevel = 20 + (pageSize - i),
                    rank = rank,
                    previousRank = rank,
                    score = score,
                    decimalScore = definition.statType == LeaderboardStatType.BestAccuracy ? score / 100.0 : score,
                    formattedScore = definition.FormatScore(score, score / 100.0),
                    lastUpdated = DateTime.UtcNow,
                    isFriend = false,
                    isSelf = rank == 50 // Mock: player is rank 50
                };

                data.entries.Add(entry);
            }

            data.totalEntries = 1000; // Mock total

            return data;
        }

        #endregion

        #region Helper Methods

        private string GetPlayerID()
        {
            // TODO: Get from authentication service
            return "player_12345";
        }

        private string GetPlayerName()
        {
            // TODO: Get from player profile
            return "TestPlayer";
        }

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[LeaderboardService] {message}");
        }

        #endregion
    }
}
