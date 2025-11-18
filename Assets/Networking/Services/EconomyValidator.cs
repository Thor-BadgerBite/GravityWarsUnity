using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Server-side economy validator for anti-cheat.
    ///
    /// Validates all progression changes to detect:
    /// - Impossible XP gains
    /// - Currency manipulation
    /// - Invalid unlocks
    /// - Unrealistic progression rates
    ///
    /// This runs alongside ProgressionManager to validate changes before saving.
    ///
    /// Usage:
    ///   bool isValid = EconomyValidator.ValidateXPGain(oldXP, newXP, source);
    ///   if (!isValid) {
    ///       // Reject change and flag account
    ///   }
    /// </summary>
    public class EconomyValidator : MonoBehaviour
    {
        #region Singleton

        private static EconomyValidator _instance;
        public static EconomyValidator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[EconomyValidator]");
                    _instance = go.AddComponent<EconomyValidator>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("XP Validation Limits")]
        [Tooltip("Maximum XP gain per match")]
        public int maxXPPerMatch = 1000;

        [Tooltip("Maximum XP gain per hour (prevents XP farming)")]
        public int maxXPPerHour = 5000;

        [Header("Currency Validation Limits")]
        [Tooltip("Maximum soft currency gain per match")]
        public int maxSoftCurrencyPerMatch = 500;

        [Tooltip("Maximum hard currency gain per match (should be rare)")]
        public int maxHardCurrencyPerMatch = 50;

        [Header("Progression Rate Limits")]
        [Tooltip("Minimum time to reach Level 10 (hours)")]
        public float minTimeToLevel10 = 2f;

        [Tooltip("Minimum time to reach Level 50 (hours)")]
        public float minTimeToLevel50 = 50f;

        [Header("Suspicious Activity Settings")]
        [Tooltip("Enable automatic account flagging")]
        public bool enableAutomaticFlagging = true;

        [Tooltip("Number of violations before account is flagged")]
        public int violationsBeforeFlag = 3;

        #endregion

        #region State Tracking

        private Dictionary<string, PlayerViolationRecord> _violationRecords = new Dictionary<string, PlayerViolationRecord>();
        private Dictionary<string, List<XPGainRecord>> _xpHistory = new Dictionary<string, List<XPGainRecord>>();

        #endregion

        #region XP Validation

        /// <summary>
        /// Validates an XP gain is legitimate.
        /// </summary>
        /// <param name="playerID">Player's unique ID</param>
        /// <param name="oldXP">Previous XP value</param>
        /// <param name="newXP">New XP value</param>
        /// <param name="source">Source of XP (e.g., "match", "quest")</param>
        /// <param name="accountCreatedDate">When account was created</param>
        /// <returns>True if valid, false if suspicious</returns>
        public ValidationResult ValidateXPGain(
            string playerID,
            int oldXP,
            int newXP,
            string source,
            DateTime accountCreatedDate)
        {
            int xpGained = newXP - oldXP;

            // Negative XP is always invalid
            if (xpGained < 0)
            {
                return Reject(playerID, "XP Decrease Detected", $"XP went from {oldXP} to {newXP}");
            }

            // Check max XP per match
            if (source == "match" && xpGained > maxXPPerMatch)
            {
                return Reject(playerID, "Excessive XP Gain", $"Gained {xpGained} XP in one match (max: {maxXPPerMatch})");
            }

            // Check XP gain rate (per hour)
            if (!ValidateXPRate(playerID, xpGained))
            {
                return Reject(playerID, "XP Farming Detected", $"XP gain rate too high");
            }

            // Check progression rate (time-based)
            int newLevel = CalculateLevelFromXP(newXP);
            if (!ValidateProgressionRate(newLevel, accountCreatedDate))
            {
                return Reject(playerID, "Impossible Progression Rate", $"Reached Level {newLevel} too quickly");
            }

            // Record XP gain for history
            RecordXPGain(playerID, xpGained, source);

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Validates XP gain rate (anti-farming).
        /// </summary>
        private bool ValidateXPRate(string playerID, int xpGained)
        {
            if (!_xpHistory.ContainsKey(playerID))
            {
                _xpHistory[playerID] = new List<XPGainRecord>();
            }

            var history = _xpHistory[playerID];

            // Remove old records (older than 1 hour)
            history.RemoveAll(r => (DateTime.UtcNow - r.timestamp).TotalHours > 1);

            // Calculate XP gained in last hour
            int xpLastHour = 0;
            foreach (var record in history)
            {
                xpLastHour += record.xpGained;
            }

            // Check if adding this gain would exceed limit
            if (xpLastHour + xpGained > maxXPPerHour)
            {
                Debug.LogWarning($"[EconomyValidator] XP rate limit exceeded for {playerID}: {xpLastHour + xpGained}/{maxXPPerHour} per hour");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Records XP gain for history tracking.
        /// </summary>
        private void RecordXPGain(string playerID, int xpGained, string source)
        {
            if (!_xpHistory.ContainsKey(playerID))
            {
                _xpHistory[playerID] = new List<XPGainRecord>();
            }

            _xpHistory[playerID].Add(new XPGainRecord
            {
                xpGained = xpGained,
                source = source,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Validates progression rate is realistic.
        /// </summary>
        private bool ValidateProgressionRate(int level, DateTime accountCreatedDate)
        {
            float accountAgeHours = (float)(DateTime.UtcNow - accountCreatedDate).TotalHours;

            // Check Level 10 threshold
            if (level >= 10 && accountAgeHours < minTimeToLevel10)
            {
                Debug.LogWarning($"[EconomyValidator] Reached Level {level} in {accountAgeHours:F1} hours (min: {minTimeToLevel10})");
                return false;
            }

            // Check Level 50 threshold
            if (level >= 50 && accountAgeHours < minTimeToLevel50)
            {
                Debug.LogWarning($"[EconomyValidator] Reached Level {level} in {accountAgeHours:F1} hours (min: {minTimeToLevel50})");
                return false;
            }

            return true;
        }

        #endregion

        #region Currency Validation

        /// <summary>
        /// Validates a currency gain is legitimate.
        /// </summary>
        public ValidationResult ValidateCurrencyGain(
            string playerID,
            string currencyType,
            int oldAmount,
            int newAmount,
            string source)
        {
            int gained = newAmount - oldAmount;

            // Negative currency is always invalid
            if (gained < 0 && source != "purchase") // Purchases can decrease currency
            {
                return Reject(playerID, "Currency Decrease Without Purchase", $"{currencyType} went from {oldAmount} to {newAmount}");
            }

            // Check max gain limits
            int maxGain = currencyType == "soft" ? maxSoftCurrencyPerMatch : maxHardCurrencyPerMatch;

            if (source == "match" && gained > maxGain)
            {
                return Reject(playerID, "Excessive Currency Gain", $"Gained {gained} {currencyType} currency in one match (max: {maxGain})");
            }

            // Hard currency should rarely be gained (only from battle pass, special events)
            if (currencyType == "hard" && source == "match")
            {
                return Reject(playerID, "Invalid Hard Currency Source", "Hard currency cannot be earned from matches");
            }

            return new ValidationResult { IsValid = true };
        }

        #endregion

        #region Unlock Validation

        /// <summary>
        /// Validates an item unlock is legitimate.
        /// </summary>
        public ValidationResult ValidateUnlock(
            string playerID,
            string itemType,
            string itemName,
            int accountLevel,
            string unlockSource)
        {
            // Check if item requires specific level
            // (This would reference a database of items and their requirements)

            // For now, just basic validation
            if (accountLevel < 1)
            {
                return Reject(playerID, "Invalid Account Level", $"Level {accountLevel} cannot unlock items");
            }

            // Validate unlock source is valid
            var validSources = new[] { "battlepass", "quest", "purchase", "level_up", "default" };
            if (Array.IndexOf(validSources, unlockSource) == -1)
            {
                return Reject(playerID, "Invalid Unlock Source", $"Unknown source: {unlockSource}");
            }

            return new ValidationResult { IsValid = true };
        }

        #endregion

        #region Violation Tracking

        /// <summary>
        /// Rejects a validation and records the violation.
        /// </summary>
        private ValidationResult Reject(string playerID, string violationType, string details)
        {
            Debug.LogWarning($"[EconomyValidator] Validation failed for {playerID}: {violationType} - {details}");

            // Record violation
            RecordViolation(playerID, violationType, details);

            return new ValidationResult
            {
                IsValid = false,
                RejectionReason = violationType,
                RejectionDetails = details
            };
        }

        /// <summary>
        /// Records a violation for a player.
        /// </summary>
        private void RecordViolation(string playerID, string violationType, string details)
        {
            if (!_violationRecords.ContainsKey(playerID))
            {
                _violationRecords[playerID] = new PlayerViolationRecord
                {
                    playerID = playerID,
                    violations = new List<Violation>()
                };
            }

            var record = _violationRecords[playerID];
            record.violations.Add(new Violation
            {
                type = violationType,
                details = details,
                timestamp = DateTime.UtcNow
            });

            // Check if player should be flagged
            if (enableAutomaticFlagging && record.violations.Count >= violationsBeforeFlag)
            {
                FlagAccount(playerID);
            }
        }

        /// <summary>
        /// Flags an account for suspicious activity.
        /// </summary>
        private void FlagAccount(string playerID)
        {
            Debug.LogError($"[EconomyValidator] ACCOUNT FLAGGED: {playerID} (Violations: {_violationRecords[playerID].violations.Count})");

            // In production, this would:
            // 1. Mark account in database
            // 2. Send notification to admin dashboard
            // 3. Potentially ban account
            // 4. Log to security monitoring system

            // For now, just log it
            _violationRecords[playerID].isFlagged = true;
            _violationRecords[playerID].flaggedAt = DateTime.UtcNow;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Returns true if player account is flagged for suspicious activity.
        /// </summary>
        public bool IsAccountFlagged(string playerID)
        {
            return _violationRecords.ContainsKey(playerID) && _violationRecords[playerID].isFlagged;
        }

        /// <summary>
        /// Gets violation record for a player (for admin review).
        /// </summary>
        public PlayerViolationRecord GetViolationRecord(string playerID)
        {
            return _violationRecords.ContainsKey(playerID) ? _violationRecords[playerID] : null;
        }

        /// <summary>
        /// Clears violations for a player (admin action after review).
        /// </summary>
        public void ClearViolations(string playerID)
        {
            if (_violationRecords.ContainsKey(playerID))
            {
                _violationRecords[playerID].violations.Clear();
                _violationRecords[playerID].isFlagged = false;
                Debug.Log($"[EconomyValidator] Violations cleared for {playerID}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculates account level from total XP.
        /// </summary>
        private int CalculateLevelFromXP(int totalXP)
        {
            // Account leveling formula (example - adjust to match your actual formula)
            // Level 1-50, quadratic XP curve
            for (int level = 1; level <= 50; level++)
            {
                int xpRequired = 100 + (level * level * 50); // Example formula
                if (totalXP < xpRequired)
                {
                    return level - 1;
                }
            }
            return 50; // Max level
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Result of an economy validation check.
    /// </summary>
    public struct ValidationResult
    {
        public bool IsValid;
        public string RejectionReason;
        public string RejectionDetails;
    }

    /// <summary>
    /// Record of violations for a player.
    /// </summary>
    [Serializable]
    public class PlayerViolationRecord
    {
        public string playerID;
        public List<Violation> violations;
        public bool isFlagged;
        public DateTime flaggedAt;
    }

    /// <summary>
    /// A single violation record.
    /// </summary>
    [Serializable]
    public class Violation
    {
        public string type;
        public string details;
        public DateTime timestamp;
    }

    /// <summary>
    /// Record of an XP gain (for rate limiting).
    /// </summary>
    [Serializable]
    public class XPGainRecord
    {
        public int xpGained;
        public string source;
        public DateTime timestamp;
    }

    #endregion
}
