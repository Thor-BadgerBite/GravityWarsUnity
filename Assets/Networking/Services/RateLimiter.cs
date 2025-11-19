using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Rate limiter service for API and action throttling.
    ///
    /// Prevents abuse by limiting how frequently players can perform actions:
    /// - Cloud save requests
    /// - Matchmaking attempts
    /// - Leaderboard queries
    /// - Analytics events
    ///
    /// Uses token bucket algorithm for smooth rate limiting.
    ///
    /// Usage:
    ///   if (RateLimiter.Instance.AllowRequest(playerID, "cloud_save")) {
    ///       // Proceed with save
    ///   } else {
    ///       // Reject - too many requests
    ///   }
    /// </summary>
    public class RateLimiter : MonoBehaviour
    {
        #region Singleton

        private static RateLimiter _instance;
        public static RateLimiter Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[RateLimiter]");
                    _instance = go.AddComponent<RateLimiter>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #endregion

        #region Rate Limit Configurations

        // Define rate limits for different action types
        private Dictionary<string, RateLimitConfig> _rateLimits = new Dictionary<string, RateLimitConfig>
        {
            // Cloud Save: Max 10 requests per minute
            { "cloud_save", new RateLimitConfig { maxRequests = 10, windowSeconds = 60 } },

            // Matchmaking: Max 5 attempts per minute
            { "matchmaking", new RateLimitConfig { maxRequests = 5, windowSeconds = 60 } },

            // Leaderboard queries: Max 20 per minute
            { "leaderboard", new RateLimitConfig { maxRequests = 20, windowSeconds = 60 } },

            // Analytics events: Max 100 per minute (allows burst of events)
            { "analytics", new RateLimitConfig { maxRequests = 100, windowSeconds = 60 } },

            // Lobby operations: Max 10 per minute
            { "lobby", new RateLimitConfig { maxRequests = 10, windowSeconds = 60 } },

            // Quest progress updates: Max 30 per minute
            { "quest_update", new RateLimitConfig { maxRequests = 30, windowSeconds = 60 } },

            // Achievement checks: Max 50 per minute
            { "achievement_check", new RateLimitConfig { maxRequests = 50, windowSeconds = 60 } },

            // General API: Max 60 per minute (1 per second)
            { "api", new RateLimitConfig { maxRequests = 60, windowSeconds = 60 } }
        };

        #endregion

        #region State Tracking

        // Player request history: playerID -> actionType -> list of timestamps
        private Dictionary<string, Dictionary<string, Queue<DateTime>>> _requestHistory
            = new Dictionary<string, Dictionary<string, Queue<DateTime>>>();

        #endregion

        #region Public API

        /// <summary>
        /// Checks if a request should be allowed based on rate limits.
        /// </summary>
        /// <param name="playerID">Player's unique ID</param>
        /// <param name="actionType">Type of action (e.g., "cloud_save", "matchmaking")</param>
        /// <returns>True if request is allowed, false if rate limited</returns>
        public bool AllowRequest(string playerID, string actionType)
        {
            // Get rate limit config for this action type
            if (!_rateLimits.ContainsKey(actionType))
            {
                Debug.LogWarning($"[RateLimiter] No rate limit config for '{actionType}' - allowing request");
                return true;
            }

            var config = _rateLimits[actionType];

            // Initialize history for this player/action if needed
            if (!_requestHistory.ContainsKey(playerID))
            {
                _requestHistory[playerID] = new Dictionary<string, Queue<DateTime>>();
            }

            if (!_requestHistory[playerID].ContainsKey(actionType))
            {
                _requestHistory[playerID][actionType] = new Queue<DateTime>();
            }

            var history = _requestHistory[playerID][actionType];

            // Remove old requests outside the time window
            var cutoffTime = DateTime.UtcNow.AddSeconds(-config.windowSeconds);
            while (history.Count > 0 && history.Peek() < cutoffTime)
            {
                history.Dequeue();
            }

            // Check if under limit
            if (history.Count >= config.maxRequests)
            {
                Debug.LogWarning($"[RateLimiter] Rate limit exceeded for {playerID} on '{actionType}' ({history.Count}/{config.maxRequests} in {config.windowSeconds}s)");
                return false;
            }

            // Allow request and record it
            history.Enqueue(DateTime.UtcNow);
            return true;
        }

        /// <summary>
        /// Gets current request count for a player/action in the current window.
        /// </summary>
        public int GetRequestCount(string playerID, string actionType)
        {
            if (!_requestHistory.ContainsKey(playerID) ||
                !_requestHistory[playerID].ContainsKey(actionType))
            {
                return 0;
            }

            return _requestHistory[playerID][actionType].Count;
        }

        /// <summary>
        /// Gets time until next request is allowed (in seconds).
        /// Returns 0 if request would be allowed now.
        /// </summary>
        public float GetTimeUntilNextRequest(string playerID, string actionType)
        {
            if (!_rateLimits.ContainsKey(actionType))
            {
                return 0f;
            }

            var config = _rateLimits[actionType];

            if (!_requestHistory.ContainsKey(playerID) ||
                !_requestHistory[playerID].ContainsKey(actionType))
            {
                return 0f;
            }

            var history = _requestHistory[playerID][actionType];

            // If under limit, can request now
            if (history.Count < config.maxRequests)
            {
                return 0f;
            }

            // Calculate when oldest request in window will expire
            var oldestRequest = history.Peek();
            var expiresAt = oldestRequest.AddSeconds(config.windowSeconds);
            var timeUntilExpiry = (float)(expiresAt - DateTime.UtcNow).TotalSeconds;

            return Mathf.Max(0f, timeUntilExpiry);
        }

        /// <summary>
        /// Clears rate limit history for a player (admin action).
        /// </summary>
        public void ClearHistory(string playerID)
        {
            if (_requestHistory.ContainsKey(playerID))
            {
                _requestHistory.Remove(playerID);
                Debug.Log($"[RateLimiter] History cleared for {playerID}");
            }
        }

        /// <summary>
        /// Clears rate limit history for a specific action type (admin action).
        /// </summary>
        public void ClearHistory(string playerID, string actionType)
        {
            if (_requestHistory.ContainsKey(playerID) &&
                _requestHistory[playerID].ContainsKey(actionType))
            {
                _requestHistory[playerID].Remove(actionType);
                Debug.Log($"[RateLimiter] History cleared for {playerID} on '{actionType}'");
            }
        }

        #endregion

        #region Configuration Management

        /// <summary>
        /// Updates rate limit configuration for an action type.
        /// </summary>
        public void SetRateLimit(string actionType, int maxRequests, int windowSeconds)
        {
            _rateLimits[actionType] = new RateLimitConfig
            {
                maxRequests = maxRequests,
                windowSeconds = windowSeconds
            };

            Debug.Log($"[RateLimiter] Rate limit updated for '{actionType}': {maxRequests} requests per {windowSeconds}s");
        }

        /// <summary>
        /// Gets current rate limit config for an action type.
        /// </summary>
        public RateLimitConfig GetRateLimit(string actionType)
        {
            return _rateLimits.ContainsKey(actionType)
                ? _rateLimits[actionType]
                : new RateLimitConfig { maxRequests = 60, windowSeconds = 60 }; // Default
        }

        #endregion

        #region Cleanup

        private void Update()
        {
            // Periodically clean up old history (every 60 seconds)
            if (Time.frameCount % (60 * 60) == 0) // ~60 FPS
            {
                CleanupOldHistory();
            }
        }

        /// <summary>
        /// Removes very old request history to prevent memory leaks.
        /// </summary>
        private void CleanupOldHistory()
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-10); // Keep last 10 minutes
            int cleaned = 0;

            foreach (var playerHistory in _requestHistory.Values)
            {
                foreach (var actionHistory in playerHistory.Values)
                {
                    while (actionHistory.Count > 0 && actionHistory.Peek() < cutoffTime)
                    {
                        actionHistory.Dequeue();
                        cleaned++;
                    }
                }
            }

            if (cleaned > 0)
            {
                Debug.Log($"[RateLimiter] Cleaned up {cleaned} old request records");
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Configuration for a rate limit.
    /// </summary>
    [Serializable]
    public struct RateLimitConfig
    {
        [Tooltip("Maximum number of requests allowed")]
        public int maxRequests;

        [Tooltip("Time window in seconds")]
        public int windowSeconds;

        public override string ToString()
        {
            return $"{maxRequests} requests per {windowSeconds}s";
        }
    }

    #endregion
}
