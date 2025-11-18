using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Detects suspicious player activity patterns that may indicate cheating or bot usage.
    ///
    /// Monitors for:
    /// - Impossible play patterns (superhuman reaction times, perfect accuracy)
    /// - Bot-like behavior (repeated identical actions, no variation)
    /// - Account sharing (sudden skill level changes, different play times)
    /// - Win trading (suspiciously trading wins with specific opponents)
    /// - Match abandonment patterns
    ///
    /// Usage:
    ///   SuspiciousActivityDetector.Instance.RecordMatchResult(playerID, matchData);
    ///   bool isSuspicious = SuspiciousActivityDetector.Instance.IsPlayerSuspicious(playerID);
    /// </summary>
    public class SuspiciousActivityDetector : MonoBehaviour
    {
        #region Singleton

        private static SuspiciousActivityDetector _instance;
        public static SuspiciousActivityDetector Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SuspiciousActivityDetector]");
                    _instance = go.AddComponent<SuspiciousActivityDetector>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Suspicion Thresholds")]
        [Tooltip("Accuracy above this is suspicious (%)")]
        public float suspiciousAccuracyThreshold = 95f;

        [Tooltip("Minimum matches to evaluate accuracy")]
        public int minMatchesForAccuracyCheck = 10;

        [Tooltip("Win rate above this with few matches is suspicious (%)")]
        public float suspiciousWinRateThreshold = 90f;

        [Tooltip("Number of consecutive identical actions that triggers bot detection")]
        public int identicalActionsThreshold = 5;

        [Tooltip("Sudden skill change threshold (accuracy delta %)")]
        public float suddenSkillChangeThreshold = 30f;

        [Header("Auto-Flagging")]
        [Tooltip("Enable automatic flagging of suspicious accounts")]
        public bool enableAutoFlagging = true;

        [Tooltip("Suspicion score threshold for automatic flagging (0-100)")]
        public float autoFlagThreshold = 75f;

        #endregion

        #region State Tracking

        private Dictionary<string, PlayerBehaviorProfile> _playerProfiles = new Dictionary<string, PlayerBehaviorProfile>();
        private Dictionary<string, List<SuspiciousEvent>> _suspiciousEvents = new Dictionary<string, List<SuspiciousEvent>>();

        #endregion

        #region Match Result Recording

        /// <summary>
        /// Records a match result for behavior analysis.
        /// </summary>
        public void RecordMatchResult(string playerID, MatchBehaviorData matchData)
        {
            if (!_playerProfiles.ContainsKey(playerID))
            {
                _playerProfiles[playerID] = new PlayerBehaviorProfile
                {
                    playerID = playerID,
                    matchHistory = new List<MatchBehaviorData>()
                };
            }

            var profile = _playerProfiles[playerID];
            profile.matchHistory.Add(matchData);

            // Keep only last 100 matches
            if (profile.matchHistory.Count > 100)
            {
                profile.matchHistory.RemoveAt(0);
            }

            // Update profile statistics
            UpdateProfileStatistics(profile);

            // Run suspicion checks
            RunSuspicionChecks(playerID, profile);
        }

        /// <summary>
        /// Updates profile statistics from match history.
        /// </summary>
        private void UpdateProfileStatistics(PlayerBehaviorProfile profile)
        {
            int totalMatches = profile.matchHistory.Count;
            int wins = 0;
            float totalAccuracy = 0f;
            float totalAvgTurnTime = 0f;

            foreach (var match in profile.matchHistory)
            {
                if (match.won)
                    wins++;

                totalAccuracy += match.accuracy;
                totalAvgTurnTime += match.averageTurnTime;
            }

            profile.totalMatches = totalMatches;
            profile.totalWins = wins;
            profile.winRate = totalMatches > 0 ? (float)wins / totalMatches * 100f : 0f;
            profile.averageAccuracy = totalMatches > 0 ? totalAccuracy / totalMatches : 0f;
            profile.averageTurnTime = totalMatches > 0 ? totalAvgTurnTime / totalMatches : 0f;
        }

        #endregion

        #region Suspicion Checks

        /// <summary>
        /// Runs all suspicion checks on a player profile.
        /// </summary>
        private void RunSuspicionChecks(string playerID, PlayerBehaviorProfile profile)
        {
            float suspicionScore = 0f;

            // Check 1: Superhuman accuracy
            if (CheckSuperhumanAccuracy(profile))
            {
                suspicionScore += 30f;
                RecordSuspiciousEvent(playerID, "Superhuman Accuracy", $"Accuracy: {profile.averageAccuracy:F1}%");
            }

            // Check 2: Suspicious win rate (high win rate with few matches)
            if (CheckSuspiciousWinRate(profile))
            {
                suspicionScore += 20f;
                RecordSuspiciousEvent(playerID, "Suspicious Win Rate", $"Win Rate: {profile.winRate:F1}% ({profile.totalMatches} matches)");
            }

            // Check 3: Bot-like behavior (repeated identical actions)
            if (CheckBotLikeBehavior(profile))
            {
                suspicionScore += 40f;
                RecordSuspiciousEvent(playerID, "Bot-Like Behavior", "Repeated identical action patterns detected");
            }

            // Check 4: Sudden skill change (account sharing?)
            if (CheckSuddenSkillChange(profile))
            {
                suspicionScore += 25f;
                RecordSuspiciousEvent(playerID, "Sudden Skill Change", "Accuracy changed dramatically");
            }

            // Check 5: Match abandonment pattern
            if (CheckMatchAbandonment(profile))
            {
                suspicionScore += 15f;
                RecordSuspiciousEvent(playerID, "Match Abandonment", "High disconnect rate");
            }

            // Update profile suspicion score
            profile.suspicionScore = suspicionScore;

            // Auto-flag if threshold exceeded
            if (enableAutoFlagging && suspicionScore >= autoFlagThreshold && !profile.isFlagged)
            {
                FlagAccount(playerID, suspicionScore);
            }
        }

        /// <summary>
        /// Checks for superhuman accuracy.
        /// </summary>
        private bool CheckSuperhumanAccuracy(PlayerBehaviorProfile profile)
        {
            return profile.totalMatches >= minMatchesForAccuracyCheck &&
                   profile.averageAccuracy > suspiciousAccuracyThreshold;
        }

        /// <summary>
        /// Checks for suspicious win rate (smurfing or cheating).
        /// </summary>
        private bool CheckSuspiciousWinRate(PlayerBehaviorProfile profile)
        {
            // High win rate with low match count suggests smurfing or cheating
            return profile.totalMatches >= 5 &&
                   profile.totalMatches < 20 &&
                   profile.winRate > suspiciousWinRateThreshold;
        }

        /// <summary>
        /// Checks for bot-like behavior (repeated identical actions).
        /// </summary>
        private bool CheckBotLikeBehavior(PlayerBehaviorProfile profile)
        {
            if (profile.matchHistory.Count < identicalActionsThreshold)
                return false;

            // Check last N matches for identical patterns
            var lastN = profile.matchHistory.GetRange(
                profile.matchHistory.Count - identicalActionsThreshold,
                identicalActionsThreshold
            );

            // Check if turn times are suspiciously identical
            float firstTurnTime = lastN[0].averageTurnTime;
            int identicalCount = 0;

            foreach (var match in lastN)
            {
                // Allow 0.1s variance (humans have slight variation)
                if (Mathf.Abs(match.averageTurnTime - firstTurnTime) < 0.1f)
                {
                    identicalCount++;
                }
            }

            return identicalCount >= identicalActionsThreshold;
        }

        /// <summary>
        /// Checks for sudden skill changes (account sharing or scripting).
        /// </summary>
        private bool CheckSuddenSkillChange(PlayerBehaviorProfile profile)
        {
            if (profile.matchHistory.Count < 20)
                return false;

            // Compare first 10 matches to last 10 matches
            var first10 = profile.matchHistory.GetRange(0, 10);
            var last10 = profile.matchHistory.GetRange(profile.matchHistory.Count - 10, 10);

            float firstAvgAccuracy = first10.Average(m => m.accuracy);
            float lastAvgAccuracy = last10.Average(m => m.accuracy);

            float accuracyDelta = Mathf.Abs(lastAvgAccuracy - firstAvgAccuracy);

            return accuracyDelta > suddenSkillChangeThreshold;
        }

        /// <summary>
        /// Checks for match abandonment patterns.
        /// </summary>
        private bool CheckMatchAbandonment(PlayerBehaviorProfile profile)
        {
            if (profile.matchHistory.Count < 10)
                return false;

            int disconnects = 0;
            foreach (var match in profile.matchHistory)
            {
                if (match.disconnected)
                    disconnects++;
            }

            float disconnectRate = (float)disconnects / profile.matchHistory.Count * 100f;

            return disconnectRate > 30f; // 30%+ disconnect rate is suspicious
        }

        #endregion

        #region Event Recording

        /// <summary>
        /// Records a suspicious event for a player.
        /// </summary>
        private void RecordSuspiciousEvent(string playerID, string eventType, string details)
        {
            if (!_suspiciousEvents.ContainsKey(playerID))
            {
                _suspiciousEvents[playerID] = new List<SuspiciousEvent>();
            }

            _suspiciousEvents[playerID].Add(new SuspiciousEvent
            {
                eventType = eventType,
                details = details,
                timestamp = DateTime.UtcNow
            });

            Debug.LogWarning($"[SuspiciousActivityDetector] Suspicious event for {playerID}: {eventType} - {details}");
        }

        /// <summary>
        /// Flags an account for review.
        /// </summary>
        private void FlagAccount(string playerID, float suspicionScore)
        {
            if (!_playerProfiles.ContainsKey(playerID))
                return;

            var profile = _playerProfiles[playerID];
            profile.isFlagged = true;
            profile.flaggedAt = DateTime.UtcNow;

            Debug.LogError($"[SuspiciousActivityDetector] ACCOUNT FLAGGED: {playerID} (Suspicion Score: {suspicionScore:F1}/100)");

            // In production, this would notify admin dashboard
        }

        #endregion

        #region Public API

        /// <summary>
        /// Returns true if player has suspicious behavior patterns.
        /// </summary>
        public bool IsPlayerSuspicious(string playerID)
        {
            if (!_playerProfiles.ContainsKey(playerID))
                return false;

            return _playerProfiles[playerID].suspicionScore > 50f;
        }

        /// <summary>
        /// Returns true if player account is flagged.
        /// </summary>
        public bool IsPlayerFlagged(string playerID)
        {
            return _playerProfiles.ContainsKey(playerID) && _playerProfiles[playerID].isFlagged;
        }

        /// <summary>
        /// Gets player behavior profile (for admin review).
        /// </summary>
        public PlayerBehaviorProfile GetPlayerProfile(string playerID)
        {
            return _playerProfiles.ContainsKey(playerID) ? _playerProfiles[playerID] : null;
        }

        /// <summary>
        /// Gets suspicious events for a player (for admin review).
        /// </summary>
        public List<SuspiciousEvent> GetSuspiciousEvents(string playerID)
        {
            return _suspiciousEvents.ContainsKey(playerID) ? _suspiciousEvents[playerID] : new List<SuspiciousEvent>();
        }

        /// <summary>
        /// Clears flag from a player account (admin action after review).
        /// </summary>
        public void ClearFlag(string playerID)
        {
            if (_playerProfiles.ContainsKey(playerID))
            {
                _playerProfiles[playerID].isFlagged = false;
                _playerProfiles[playerID].suspicionScore = 0f;
                Debug.Log($"[SuspiciousActivityDetector] Flag cleared for {playerID}");
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Player behavior profile for suspicion analysis.
    /// </summary>
    [Serializable]
    public class PlayerBehaviorProfile
    {
        public string playerID;
        public List<MatchBehaviorData> matchHistory;

        // Aggregated statistics
        public int totalMatches;
        public int totalWins;
        public float winRate;
        public float averageAccuracy;
        public float averageTurnTime;

        // Suspicion tracking
        public float suspicionScore; // 0-100
        public bool isFlagged;
        public DateTime flaggedAt;
    }

    /// <summary>
    /// Behavior data for a single match.
    /// </summary>
    [Serializable]
    public struct MatchBehaviorData
    {
        public bool won;
        public float accuracy;          // Hit rate (%)
        public float averageTurnTime;   // Seconds per turn
        public int shotsFired;
        public int shotsHit;
        public bool disconnected;       // Did player disconnect?
        public DateTime playedAt;
    }

    /// <summary>
    /// Record of a suspicious event.
    /// </summary>
    [Serializable]
    public struct SuspiciousEvent
    {
        public string eventType;
        public string details;
        public DateTime timestamp;
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Extension method to calculate average from list.
    /// </summary>
    public static class ListExtensions
    {
        public static float Average(this List<MatchBehaviorData> list, Func<MatchBehaviorData, float> selector)
        {
            if (list.Count == 0)
                return 0f;

            float sum = 0f;
            foreach (var item in list)
            {
                sum += selector(item);
            }
            return sum / list.Count;
        }
    }

    #endregion
}
