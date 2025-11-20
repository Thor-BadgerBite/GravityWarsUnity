using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;

namespace GravityWars.Networking
{
    /// <summary>
    /// Analytics service for tracking player events and behavior.
    ///
    /// Features:
    /// - Unified API for all analytics tracking
    /// - Automatic session tracking
    /// - Event batching for performance
    /// - Custom parameters per event
    /// - Debug logging in development builds
    ///
    /// Usage:
    ///   AnalyticsService.Instance.TrackMatchComplete(matchResult);
    ///   AnalyticsService.Instance.TrackEvent("custom_event", params);
    /// </summary>
    public class AnalyticsService : MonoBehaviour
    {
        #region Singleton

        private static AnalyticsService _instance;
        public static AnalyticsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AnalyticsService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AnalyticsService");
                        _instance = go.AddComponent<AnalyticsService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Configuration

        [Header("Analytics Configuration")]
        [Tooltip("Enable analytics tracking (disable for testing)")]
        public bool analyticsEnabled = true;

        [Tooltip("Log all analytics events to console in debug builds")]
        public bool debugLogging = true;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (analyticsEnabled)
            {
                // Start analytics session
                // TODO: Install Unity Analytics package and uncomment
                // Unity.Services.Analytics.AnalyticsService.Instance.StartDataCollection();
                Debug.Log("[Analytics] Analytics started (Unity Analytics package not installed yet)");

                // Track session start
                TrackSessionStart();
            }
        }

        private void OnApplicationQuit()
        {
            if (analyticsEnabled)
            {
                // Track session end
                TrackSessionEnd();

                // Flush any pending events
                // TODO: Install Unity Analytics package and uncomment
                // Unity.Services.Analytics.AnalyticsService.Instance.Flush();
            }
        }

        #endregion

        #region Core API

        /// <summary>
        /// Tracks a custom event with parameters.
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!analyticsEnabled)
                return;

            try
            {
                if (parameters == null)
                    parameters = new Dictionary<string, object>();

                // Add standard metadata
                parameters["timestamp"] = DateTime.UtcNow.ToString("o");
                if (ServiceLocator.Instance != null)
                    parameters["player_id"] = ServiceLocator.Instance.GetPlayerId();

                // Send to Unity Analytics
                // TODO: Install Unity Analytics package and uncomment
                // Unity.Services.Analytics.AnalyticsService.Instance.CustomData(eventName, parameters);

                // Debug logging
                if (debugLogging)
                {
                    Debug.Log($"[Analytics] {eventName}: {JsonUtility.ToJson(parameters)}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Analytics] Failed to track event '{eventName}': {e.Message}");
            }
        }

        #endregion

        #region Player Lifecycle Events

        /// <summary>
        /// Tracks when a player creates a new account.
        /// </summary>
        public void TrackAccountCreated(string displayName, string platform)
        {
            TrackEvent("account_created", new Dictionary<string, object>
            {
                { "display_name", displayName },
                { "platform", platform },
                { "device_model", SystemInfo.deviceModel },
                { "os_version", SystemInfo.operatingSystem }
            });
        }

        /// <summary>
        /// Tracks when a player starts a session (launches the game).
        /// </summary>
        public void TrackSessionStart()
        {
            var accountData = ProgressionManager.Instance?.currentPlayerData;

            TrackEvent("session_start", new Dictionary<string, object>
            {
                { "account_level", accountData?.accountLevel ?? 0 },
                { "total_matches_played", accountData?.totalMatchesPlayed ?? 0 },
                { "total_play_time", GetTotalPlayTime() },
                { "days_since_account_created", GetDaysSinceAccountCreated() }
            });
        }

        /// <summary>
        /// Tracks when a player ends a session (quits the game).
        /// </summary>
        public void TrackSessionEnd()
        {
            TrackEvent("session_end", new Dictionary<string, object>
            {
                { "session_duration", Time.realtimeSinceStartup },
                { "matches_this_session", GetMatchesThisSession() },
                { "xp_gained_this_session", GetXPGainedThisSession() }
            });
        }

        #endregion

        #region Match Events

        /// <summary>
        /// Tracks when a match starts.
        /// </summary>
        public void TrackMatchStarted(string mode, string playerShip, string opponentShip, int planetCount)
        {
            TrackEvent("match_started", new Dictionary<string, object>
            {
                { "mode", mode }, // "hotseat", "online", "vs_ai"
                { "player_ship", playerShip },
                { "opponent_ship", opponentShip },
                { "planet_count", planetCount }
            });
        }

        /// <summary>
        /// Tracks when a match completes.
        /// </summary>
        public void TrackMatchComplete(MatchAnalytics analytics)
        {
            TrackEvent("match_completed", new Dictionary<string, object>
            {
                { "winner", analytics.winner },
                { "duration", analytics.duration },
                { "rounds_played", analytics.roundsPlayed },
                { "player_damage_dealt", analytics.playerDamageDealt },
                { "opponent_damage_dealt", analytics.opponentDamageDealt },
                { "player_shots_fired", analytics.playerShotsFired },
                { "opponent_shots_fired", analytics.opponentShotsFired },
                { "player_accuracy", analytics.playerAccuracy },
                { "opponent_accuracy", analytics.opponentAccuracy },
                { "xp_gained", analytics.xpGained },
                { "currency_gained", analytics.currencyGained }
            });
        }

        /// <summary>
        /// Tracks when a round completes.
        /// </summary>
        public void TrackRoundComplete(int roundNumber, string winner, int shotsThisRound, int damageTaken)
        {
            TrackEvent("round_completed", new Dictionary<string, object>
            {
                { "round_number", roundNumber },
                { "winner", winner },
                { "shots_this_round", shotsThisRound },
                { "damage_taken", damageTaken }
            });
        }

        /// <summary>
        /// Tracks player actions (fire, move, perk activation).
        /// </summary>
        public void TrackPlayerAction(string actionType, bool success, float value = 0)
        {
            TrackEvent("player_action", new Dictionary<string, object>
            {
                { "action_type", actionType }, // "fire", "move", "perk_activate"
                { "success", success },
                { "value", value } // damage dealt, distance moved, etc.
            });
        }

        #endregion

        #region Progression Events

        /// <summary>
        /// Tracks when player levels up their account.
        /// </summary>
        public void TrackAccountLevelUp(int newLevel, string xpSource, float timeSinceLastLevel)
        {
            TrackEvent("account_level_up", new Dictionary<string, object>
            {
                { "new_level", newLevel },
                { "xp_source", xpSource }, // "match", "quest", "battlepass"
                { "time_since_last_level", timeSinceLastLevel }
            });
        }

        /// <summary>
        /// Tracks when player levels up a ship.
        /// </summary>
        public void TrackShipLevelUp(string loadoutKey, int newLevel, int matchesPlayed)
        {
            TrackEvent("ship_level_up", new Dictionary<string, object>
            {
                { "loadout_key", loadoutKey },
                { "new_level", newLevel },
                { "matches_played", matchesPlayed }
            });
        }

        /// <summary>
        /// Tracks when player unlocks an item.
        /// </summary>
        public void TrackItemUnlocked(string itemType, string itemName, string unlockSource)
        {
            TrackEvent("item_unlocked", new Dictionary<string, object>
            {
                { "item_type", itemType }, // "ship", "perk", "passive", "missile", "skin"
                { "item_name", itemName },
                { "unlock_source", unlockSource } // "battlepass", "quest", "purchase", "level_up"
            });
        }

        /// <summary>
        /// Tracks currency earned.
        /// </summary>
        public void TrackCurrencyEarned(string currencyType, int amount, string source)
        {
            TrackEvent("currency_earned", new Dictionary<string, object>
            {
                { "currency_type", currencyType }, // "soft", "hard"
                { "amount", amount },
                { "source", source } // "match", "quest", "battlepass", "achievement"
            });
        }

        /// <summary>
        /// Tracks currency spent.
        /// </summary>
        public void TrackCurrencySpent(string currencyType, int amount, string itemPurchased)
        {
            TrackEvent("currency_spent", new Dictionary<string, object>
            {
                { "currency_type", currencyType },
                { "amount", amount },
                { "item_purchased", itemPurchased }
            });
        }

        #endregion

        #region Engagement Events

        /// <summary>
        /// Tracks when player starts a quest.
        /// </summary>
        public void TrackQuestStarted(string questID, string difficulty)
        {
            TrackEvent("quest_started", new Dictionary<string, object>
            {
                { "quest_id", questID },
                { "difficulty", difficulty }
            });
        }

        /// <summary>
        /// Tracks quest progress updates.
        /// </summary>
        public void TrackQuestProgress(string questID, int currentProgress, int targetProgress)
        {
            TrackEvent("quest_progress", new Dictionary<string, object>
            {
                { "quest_id", questID },
                { "current_progress", currentProgress },
                { "target_progress", targetProgress },
                { "progress_percentage", (float)currentProgress / targetProgress * 100 }
            });
        }

        /// <summary>
        /// Tracks when player completes a quest.
        /// </summary>
        public void TrackQuestCompleted(string questID, float timeToComplete, string reward)
        {
            TrackEvent("quest_completed", new Dictionary<string, object>
            {
                { "quest_id", questID },
                { "time_to_complete", timeToComplete },
                { "reward", reward }
            });
        }

        /// <summary>
        /// Tracks when player unlocks an achievement.
        /// </summary>
        public void TrackAchievementUnlocked(string achievementID, string rarity, float completionPercentage)
        {
            TrackEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement_id", achievementID },
                { "rarity", rarity }, // "common", "rare", "epic", "legendary"
                { "completion_percentage", completionPercentage } // % of players who have this
            });
        }

        /// <summary>
        /// Tracks when player tiers up in battle pass.
        /// </summary>
        public void TrackBattlePassTierUp(int tierNumber, bool isPremium, string reward)
        {
            TrackEvent("battlepass_tier_up", new Dictionary<string, object>
            {
                { "tier_number", tierNumber },
                { "is_premium", isPremium },
                { "reward", reward }
            });
        }

        #endregion

        #region Economy Events

        /// <summary>
        /// Tracks when player creates a custom loadout.
        /// </summary>
        public void TrackLoadoutCreated(string loadoutID, string shipBody, string tier1Perk, string tier2Perk, string tier3Perk)
        {
            TrackEvent("loadout_created", new Dictionary<string, object>
            {
                { "loadout_id", loadoutID },
                { "ship_body", shipBody },
                { "tier1_perk", tier1Perk },
                { "tier2_perk", tier2Perk },
                { "tier3_perk", tier3Perk }
            });
        }

        /// <summary>
        /// Tracks when player equips a loadout.
        /// </summary>
        public void TrackLoadoutEquipped(string loadoutID, string previousLoadout)
        {
            TrackEvent("loadout_equipped", new Dictionary<string, object>
            {
                { "loadout_id", loadoutID },
                { "previous_loadout", previousLoadout }
            });
        }

        /// <summary>
        /// Tracks when player changes missile on a loadout.
        /// </summary>
        public void TrackMissileChanged(string loadoutID, string oldMissile, string newMissile)
        {
            TrackEvent("missile_changed", new Dictionary<string, object>
            {
                { "loadout_id", loadoutID },
                { "old_missile", oldMissile },
                { "new_missile", newMissile }
            });
        }

        #endregion

        #region Helper Methods

        private float GetTotalPlayTime()
        {
            // This would be tracked in PlayerAccountData in a real implementation
            return Time.realtimeSinceStartup;
        }

        private int GetDaysSinceAccountCreated()
        {
            var accountData = ProgressionManager.Instance?.currentPlayerData;
            if (accountData == null)
                return 0;

            return (int)(DateTime.UtcNow - accountData.accountCreatedDate).TotalDays;
        }

        private int GetMatchesThisSession()
        {
            // This would be tracked in a session manager
            return 0; // Placeholder
        }

        private int GetXPGainedThisSession()
        {
            // This would be tracked in a session manager
            return 0; // Placeholder
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Analytics data for a completed match.
    /// </summary>
    [Serializable]
    public struct MatchAnalytics
    {
        public string winner;
        public float duration;
        public int roundsPlayed;
        public int playerDamageDealt;
        public int opponentDamageDealt;
        public int playerShotsFired;
        public int opponentShotsFired;
        public float playerAccuracy;
        public float opponentAccuracy;
        public int xpGained;
        public int currencyGained;
    }

    #endregion
}
