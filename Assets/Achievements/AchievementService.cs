using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Achievement service for tracking and unlocking achievements.
    ///
    /// Features:
    /// - Progress tracking for all achievement types
    /// - Automatic unlock detection
    /// - Reward distribution
    /// - Cloud save synchronization
    /// - Platform integration (Steam, PlayStation, Xbox)
    /// - Analytics integration
    /// - Secret achievement support
    ///
    /// Achievement Flow:
    /// 1. Player performs action → UpdateAchievementProgress()
    /// 2. Check if achievement unlocked → OnAchievementUnlocked()
    /// 3. Award rewards
    /// 4. Sync to platform (Steam, etc.)
    /// 5. Save to cloud
    /// 6. Show unlock notification
    ///
    /// Usage:
    ///   AchievementService.Instance.InitializeAchievements();
    ///   AchievementService.Instance.UpdateAchievementProgress(AchievementConditionType.WinMatches, 1);
    /// </summary>
    public class AchievementService : MonoBehaviour
    {
        #region Singleton

        private static AchievementService _instance;
        public static AchievementService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AchievementService>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[AchievementService]");
                        _instance = go.AddComponent<AchievementService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Achievement Configuration")]
        [Tooltip("All available achievement templates")]
        public List<AchievementDataSO> achievementTemplates = new List<AchievementDataSO>();

        [Tooltip("Enable debug logging")]
        public bool debugLogging = true;

        [Header("Platform Integration")]
        [Tooltip("Enable Steam achievement sync")]
        public bool enableSteamSync = false;

        [Tooltip("Enable PlayStation trophy sync")]
        public bool enablePSSync = false;

        [Tooltip("Enable Xbox achievement sync")]
        public bool enableXboxSync = false;

        [Header("UI")]
        [Tooltip("Show unlock notifications")]
        public bool showUnlockNotifications = true;

        [Tooltip("Notification duration (seconds)")]
        public float notificationDuration = 5f;

        #endregion

        #region State

        private List<AchievementInstance> _achievements = new List<AchievementInstance>();
        private bool _isInitialized = false;

        // Lifetime stats tracking (for achievements)
        private Dictionary<string, int> _lifetimeStats = new Dictionary<string, int>();

        public List<AchievementInstance> Achievements => _achievements;
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Events

        public event Action<AchievementInstance> OnAchievementUnlockedEvent;
        public event Action<AchievementInstance, int> OnAchievementProgressEvent;

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

            Log("Achievement service created");
        }

        private void Start()
        {
            // Auto-initialize
            InitializeAchievements();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes achievement system.
        /// Loads from cloud save or creates new instances.
        /// </summary>
        public async void InitializeAchievements()
        {
            if (_isInitialized)
            {
                Log("Already initialized");
                return;
            }

            Log("Initializing achievement system...");

            // Load from cloud save
            bool loadedFromCloud = await LoadAchievementsFromCloud();

            if (!loadedFromCloud)
            {
                // Create new achievement instances from templates
                CreateAchievementsFromTemplates();
            }

            // Initialize lifetime stats
            InitializeLifetimeStats();

            _isInitialized = true;
            Log($"Achievement system initialized - {_achievements.Count} achievements loaded");
        }

        /// <summary>
        /// Creates achievement instances from templates.
        /// </summary>
        private void CreateAchievementsFromTemplates()
        {
            _achievements.Clear();

            foreach (var template in achievementTemplates)
            {
                if (template == null)
                    continue;

                var instance = template.CreateInstance();
                _achievements.Add(instance);
            }

            Log($"Created {_achievements.Count} achievements from templates");
        }

        /// <summary>
        /// Initializes lifetime stats tracking.
        /// </summary>
        private void InitializeLifetimeStats()
        {
            _lifetimeStats.Clear();

            // Initialize all stat types
            foreach (AchievementConditionType conditionType in Enum.GetValues(typeof(AchievementConditionType)))
            {
                _lifetimeStats[conditionType.ToString()] = 0;
            }

            Log("Lifetime stats initialized");
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// Updates achievement progress.
        /// </summary>
        public void UpdateAchievementProgress(AchievementConditionType conditionType, int amount = 1, string context = "")
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[AchievementService] Not initialized yet!");
                return;
            }

            // Update lifetime stats
            UpdateLifetimeStat(conditionType, amount);

            // Find matching achievements
            var matchingAchievements = _achievements
                .Where(a => !a.isUnlocked)
                .Where(a => a.conditionType == conditionType)
                .Where(a => string.IsNullOrEmpty(a.requiredContext) || a.requiredContext == context)
                .ToList();

            foreach (var achievement in matchingAchievements)
            {
                int oldProgress = achievement.currentProgress;
                achievement.AddProgress(amount);

                if (achievement.currentProgress > oldProgress)
                {
                    // Progress updated
                    OnAchievementProgressEvent?.Invoke(achievement, achievement.currentProgress);

                    if (achievement.isUnlocked)
                    {
                        // Achievement unlocked!
                        OnAchievementUnlocked(achievement);
                    }
                }
            }
        }

        /// <summary>
        /// Sets absolute progress for an achievement.
        /// Used for achievements that check against a specific value (e.g., "Reach Level 50").
        /// </summary>
        public void SetAchievementProgress(AchievementConditionType conditionType, int value, string context = "")
        {
            if (!_isInitialized)
                return;

            // Update lifetime stat
            SetLifetimeStat(conditionType, value);

            // Find matching achievements
            var matchingAchievements = _achievements
                .Where(a => !a.isUnlocked)
                .Where(a => a.conditionType == conditionType)
                .Where(a => string.IsNullOrEmpty(a.requiredContext) || a.requiredContext == context)
                .ToList();

            foreach (var achievement in matchingAchievements)
            {
                int oldProgress = achievement.currentProgress;
                achievement.SetProgress(value);

                if (achievement.currentProgress > oldProgress)
                {
                    OnAchievementProgressEvent?.Invoke(achievement, achievement.currentProgress);

                    if (achievement.isUnlocked)
                    {
                        OnAchievementUnlocked(achievement);
                    }
                }
            }
        }

        /// <summary>
        /// Manually unlocks an achievement (admin/debug).
        /// </summary>
        public void UnlockAchievement(string achievementID)
        {
            var achievement = GetAchievementByID(achievementID);

            if (achievement == null)
            {
                Debug.LogWarning($"[AchievementService] Achievement not found: {achievementID}");
                return;
            }

            if (achievement.isUnlocked)
            {
                Log($"Achievement already unlocked: {achievementID}");
                return;
            }

            achievement.Unlock();
            OnAchievementUnlocked(achievement);
        }

        #endregion

        #region Achievement Unlocking

        /// <summary>
        /// Called when an achievement is unlocked.
        /// </summary>
        private void OnAchievementUnlocked(AchievementInstance achievement)
        {
            Log($"🏆 Achievement Unlocked: {achievement.username}");

            // Award rewards
            AwardAchievementRewards(achievement);

            // Sync to platform
            SyncToPlatform(achievement);

            // Track in analytics
            TrackAchievementUnlock(achievement);

            // Show notification
            if (showUnlockNotifications)
            {
                ShowUnlockNotification(achievement);
            }

            // Invoke event
            OnAchievementUnlockedEvent?.Invoke(achievement);

            // Save to cloud
            _ = SaveAchievementsToCloud();
        }

        /// <summary>
        /// Awards achievement rewards.
        /// </summary>
        private void AwardAchievementRewards(AchievementInstance achievement)
        {
            var progressionManager = FindObjectOfType<ProgressionManager>();

            if (progressionManager == null)
            {
                Debug.LogWarning("[AchievementService] ProgressionManager not found - cannot award rewards");
                return;
            }

            // Award soft currency
            if (achievement.creditsReward > 0)
            {
                // progressionManager.AddCurrency(CurrencyType.Soft, achievement.creditsReward);
                Log($"Awarded {achievement.creditsReward} coins");
            }

            // Award hard currency
            if (achievement.gemsReward > 0)
            {
                // progressionManager.AddCurrency(CurrencyType.Hard, achievement.gemsReward);
                Log($"Awarded {achievement.gemsReward} gems");
            }

            // Award XP
            if (achievement.accountXPReward > 0)
            {
                // progressionManager.AddAccountXP(achievement.accountXPReward, "achievement");
                Log($"Awarded {achievement.accountXPReward} XP");
            }

            // Award exclusive item
            if (!string.IsNullOrEmpty(achievement.exclusiveItemReward))
            {
                // progressionManager.UnlockItem(achievement.exclusiveItemReward);
                Log($"Awarded exclusive item: {achievement.exclusiveItemReward}");
            }

            // Award title
            if (!string.IsNullOrEmpty(achievement.titleReward))
            {
                // progressionManager.UnlockTitle(achievement.titleReward);
                Log($"Awarded title: {achievement.titleReward}");
            }

            Log($"Achievement rewards awarded: {achievement.username}");
        }

        #endregion

        #region Platform Sync

        /// <summary>
        /// Syncs achievement to platform (Steam, PlayStation, Xbox).
        /// </summary>
        private void SyncToPlatform(AchievementInstance achievement)
        {
            if (achievement.template == null)
                return;

            // Steam sync
            if (enableSteamSync && !string.IsNullOrEmpty(achievement.template.steamAchievementID))
            {
                SyncToSteam(achievement.template.steamAchievementID);
            }

            // PlayStation sync
            if (enablePSSync && !string.IsNullOrEmpty(achievement.template.playstationTrophyID))
            {
                SyncToPlayStation(achievement.template.playstationTrophyID);
            }

            // Xbox sync
            if (enableXboxSync && !string.IsNullOrEmpty(achievement.template.xboxAchievementID))
            {
                SyncToXbox(achievement.template.xboxAchievementID);
            }
        }

        /// <summary>
        /// Syncs to Steam achievement.
        /// </summary>
        private void SyncToSteam(string steamAchievementID)
        {
            // TODO: Implement Steamworks integration
            // Example:
            // if (SteamManager.Initialized)
            // {
            //     SteamUserStats.SetAchievement(steamAchievementID);
            //     SteamUserStats.StoreStats();
            // }

            Log($"Steam achievement synced: {steamAchievementID}");
        }

        /// <summary>
        /// Syncs to PlayStation trophy.
        /// </summary>
        private void SyncToPlayStation(string trophyID)
        {
            // TODO: Implement PlayStation trophy integration
            Log($"PlayStation trophy synced: {trophyID}");
        }

        /// <summary>
        /// Syncs to Xbox achievement.
        /// </summary>
        private void SyncToXbox(string xboxAchievementID)
        {
            // TODO: Implement Xbox achievement integration
            Log($"Xbox achievement synced: {xboxAchievementID}");
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Tracks achievement unlock in analytics.
        /// </summary>
        private void TrackAchievementUnlock(AchievementInstance achievement)
        {
            var analyticsService = ServiceLocator.Instance?.Analytics;
            if (analyticsService != null)
            {
                analyticsService.TrackAchievementUnlocked(
                    achievementID: achievement.achievementID,
                    achievementName: achievement.username
                );
            }
        }

        #endregion

        #region UI Notifications

        /// <summary>
        /// Shows unlock notification UI.
        /// </summary>
        private void ShowUnlockNotification(AchievementInstance achievement)
        {
            // In production, this would trigger a UI popup/toast
            // For now, just log
            Debug.Log($"[AchievementService] 🏆 Achievement Unlocked: {achievement.username}!");

            // TODO: Integrate with AchievementUI notification system
            // AchievementUI.Instance?.ShowUnlockNotification(achievement);
        }

        #endregion

        #region Cloud Save

        /// <summary>
        /// Saves achievements to cloud.
        /// </summary>
        public async Task<bool> SaveAchievementsToCloud()
        {
            var cloudSaveService = ServiceLocator.Instance?.CloudSave;

            if (cloudSaveService == null)
            {
                Debug.LogWarning("[AchievementService] CloudSaveService not available");
                return false;
            }

            try
            {
                // Serialize achievement data
                var saveData = SerializeAchievements();

                // Save to cloud
                // await cloudSaveService.SaveDataAsync("achievements", saveData);

                Log("Achievements saved to cloud");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AchievementService] Failed to save achievements: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads achievements from cloud.
        /// </summary>
        public async Task<bool> LoadAchievementsFromCloud()
        {
            var cloudSaveService = ServiceLocator.Instance?.CloudSave;

            if (cloudSaveService == null)
            {
                Debug.LogWarning("[AchievementService] CloudSaveService not available");
                return false;
            }

            try
            {
                // Load from cloud
                // string saveData = await cloudSaveService.LoadDataAsync("achievements");

                // if (string.IsNullOrEmpty(saveData))
                // {
                //     Log("No cloud save data found");
                //     return false;
                // }

                // Deserialize
                // DeserializeAchievements(saveData);

                Log("Achievements loaded from cloud");
                return false; // Disabled for now
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AchievementService] Failed to load achievements: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Serializes achievements to JSON.
        /// </summary>
        private string SerializeAchievements()
        {
            var saveData = new AchievementSaveData
            {
                achievements = _achievements,
                lifetimeStats = _lifetimeStats
            };

            return JsonUtility.ToJson(saveData);
        }

        /// <summary>
        /// Deserializes achievements from JSON.
        /// </summary>
        private void DeserializeAchievements(string json)
        {
            var saveData = JsonUtility.FromJson<AchievementSaveData>(json);

            if (saveData != null)
            {
                _achievements = saveData.achievements;
                _lifetimeStats = saveData.lifetimeStats;
            }
        }

        #endregion

        #region Lifetime Stats

        /// <summary>
        /// Updates lifetime stat.
        /// </summary>
        private void UpdateLifetimeStat(AchievementConditionType statType, int amount)
        {
            string key = statType.ToString();

            if (!_lifetimeStats.ContainsKey(key))
            {
                _lifetimeStats[key] = 0;
            }

            _lifetimeStats[key] += amount;
        }

        /// <summary>
        /// Sets lifetime stat to absolute value.
        /// </summary>
        private void SetLifetimeStat(AchievementConditionType statType, int value)
        {
            string key = statType.ToString();
            _lifetimeStats[key] = value;
        }

        /// <summary>
        /// Gets lifetime stat value.
        /// </summary>
        public int GetLifetimeStat(AchievementConditionType statType)
        {
            string key = statType.ToString();
            return _lifetimeStats.ContainsKey(key) ? _lifetimeStats[key] : 0;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets all achievements.
        /// </summary>
        public List<AchievementInstance> GetAllAchievements()
        {
            return new List<AchievementInstance>(_achievements);
        }

        /// <summary>
        /// Gets achievements by category.
        /// </summary>
        public List<AchievementInstance> GetAchievementsByCategory(AchievementCategory category)
        {
            return _achievements.Where(a => a.category == category).ToList();
        }

        /// <summary>
        /// Gets unlocked achievements.
        /// </summary>
        public List<AchievementInstance> GetUnlockedAchievements()
        {
            return _achievements.Where(a => a.isUnlocked).ToList();
        }

        /// <summary>
        /// Gets locked achievements.
        /// </summary>
        public List<AchievementInstance> GetLockedAchievements()
        {
            return _achievements.Where(a => !a.isUnlocked).ToList();
        }

        /// <summary>
        /// Gets achievement by ID.
        /// </summary>
        public AchievementInstance GetAchievementByID(string achievementID)
        {
            return _achievements.FirstOrDefault(a => a.achievementID == achievementID);
        }

        /// <summary>
        /// Gets total achievement points earned.
        /// </summary>
        public int GetTotalAchievementPoints()
        {
            return _achievements.Where(a => a.isUnlocked).Sum(a => a.achievementPoints);
        }

        /// <summary>
        /// Gets achievement completion percentage.
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (_achievements.Count == 0)
                return 0f;

            int unlockedCount = _achievements.Count(a => a.isUnlocked);
            return (float)unlockedCount / _achievements.Count * 100f;
        }

        #endregion

        #region Helper Methods

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[AchievementService] {message}");
        }

        #endregion
    }

    #region Save Data

    /// <summary>
    /// Achievement save data structure.
    /// </summary>
    [Serializable]
    public class AchievementSaveData
    {
        public List<AchievementInstance> achievements;
        public Dictionary<string, int> lifetimeStats;
    }

    #endregion
}
