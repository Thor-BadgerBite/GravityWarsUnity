using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Security.Cryptography;
using System.Text;
using GravityWars.CloudSave;
using SaveData = GravityWars.CloudSave.SaveData;

namespace GravityWars.Networking
{
    /// <summary>
    /// Comprehensive cloud save service for server-side player data synchronization.
    ///
    /// Features:
    /// - Automatic cloud sync with offline queue
    /// - Advanced conflict resolution (newest, highest progress, or merge strategies)
    /// - Data versioning for migration support
    /// - Anti-cheat validation (hash verification, integrity checks)
    /// - Multi-device support with device tracking
    /// - Automatic backup system
    /// - Rate limiting for save operations
    ///
    /// Usage:
    ///   await CloudSaveService.Instance.SaveToCloud(saveData);
    ///   var data = await CloudSaveService.Instance.LoadFromCloud();
    /// </summary>
    public class CloudSaveService : MonoBehaviour
    {
        #region Singleton

        private static CloudSaveService _instance;
        public static CloudSaveService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CloudSaveService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CloudSaveService");
                        _instance = go.AddComponent<CloudSaveService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Constants

        private const string SAVE_DATA_KEY = "player_save_data_v2";
        private const string SAVE_METADATA_KEY = "save_metadata_v2";
        private const string SAVE_HASH_KEY = "save_data_hash_v2";
        private const string BACKUP_DATA_KEY = "player_save_backup_v2";
        private const string LAST_SYNC_TIME_KEY = "last_sync_timestamp_v2";

        // NEW: PlayerAccountData keys for competitive multiplayer
        private const string PLAYER_PROFILE_KEY = "player_profile_data_v1";
        private const string PROFILE_HASH_KEY = "player_profile_hash_v1";
        private const string PROFILE_BACKUP_KEY = "player_profile_backup_v1";

        private const float MIN_SAVE_INTERVAL = 5f; // Minimum seconds between saves (rate limiting)
        private const int MAX_OFFLINE_QUEUE_SIZE = 50;

        #endregion

        #region Events

        public event Action<SaveData> OnSaveCompleted;
        public event Action<SaveData> OnLoadCompleted;
        public event Action<string> OnSaveError;
        public event Action OnConflictDetected;

        #endregion

        #region State

        private Queue<SaveData> _offlineQueue = new Queue<SaveData>();
        private bool _isSyncing = false;
        private float _lastSaveTime = 0f;
        private SaveData _currentSaveData = null;
        private bool _hasUnsavedChanges = false;

        #endregion

        #region Configuration

        [Header("Cloud Save Configuration")]
        [SerializeField] private bool enableAutoSync = true;
        [SerializeField] private float autoSyncInterval = 300f; // 5 minutes
        [SerializeField] private ConflictResolutionStrategy defaultConflictStrategy = ConflictResolutionStrategy.TakeNewest;
        [SerializeField] private bool enableBackups = true;
        [SerializeField] private bool enableAntiCheat = true;

        #endregion

        #region Public API - Save

        /// <summary>
        /// Saves player data to Unity Cloud Save with validation and anti-cheat.
        /// </summary>
        public async Task<bool> SaveToCloud(SaveData data, bool forceImmediate = false)
        {
            if (data == null)
            {
                Debug.LogError("[CloudSave] Cannot save null data");
                OnSaveError?.Invoke("Cannot save null data");
                return false;
            }

            // Rate limiting (unless forced)
            if (!forceImmediate && Time.time - _lastSaveTime < MIN_SAVE_INTERVAL)
            {
                Debug.LogWarning($"[CloudSave] Save rate limited. Wait {MIN_SAVE_INTERVAL - (Time.time - _lastSaveTime):F1}s");
                _hasUnsavedChanges = true;
                return false;
            }

            // Validate data integrity before saving
            if (enableAntiCheat && !data.Validate())
            {
                Debug.LogError("[CloudSave] Data validation failed - possible tampering detected");
                OnSaveError?.Invoke("Data validation failed");
                return false;
            }

            // Update save metadata
            data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            data.saveCount++;
            data.deviceID = SystemInfo.deviceUniqueIdentifier;

            // Check if online
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - queuing save for later");
                QueueSave(data);
                return false;
            }

            try
            {
                // Backup current cloud save before overwriting (if enabled)
                if (enableBackups)
                {
                    await BackupCurrentSave();
                }

                // Serialize save data
                string saveJson = JsonUtility.ToJson(data);
                string metadataJson = JsonUtility.ToJson(new SaveMetadata(data));
                string dataHash = enableAntiCheat ? ComputeDataHash(saveJson) : "";

                // Prepare cloud save dictionary
                var cloudData = new Dictionary<string, object>
                {
                    { SAVE_DATA_KEY, saveJson },
                    { SAVE_METADATA_KEY, metadataJson },
                    { LAST_SYNC_TIME_KEY, DateTime.UtcNow.ToString("o") }
                };

                if (enableAntiCheat)
                {
                    cloudData[SAVE_HASH_KEY] = dataHash;
                }

                // Save to Unity Cloud Save
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.SaveAsync(cloudData);

                Debug.Log($"[CloudSave] ✓ Successfully saved to cloud for player: {data.playerID}");
                Debug.Log($"[CloudSave]   - Save count: {data.saveCount}");
                Debug.Log($"[CloudSave]   - Device: {data.deviceID}");
                Debug.Log($"[CloudSave]   - Data size: {saveJson.Length} bytes");

                _lastSaveTime = Time.time;
                _currentSaveData = data;
                _hasUnsavedChanges = false;

                OnSaveCompleted?.Invoke(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to save to cloud: {e.Message}\n{e.StackTrace}");
                OnSaveError?.Invoke(e.Message);

                // Queue for retry
                QueueSave(data);
                return false;
            }
        }

        /// <summary>
        /// Convenience wrapper to save the unified PlayerAccountData directly.
        /// </summary>
        public Task<bool> SaveToCloud(PlayerAccountData accountData, bool forceImmediate = false)
        {
            if (accountData == null)
                return Task.FromResult(false);

            var wrapper = new SaveData
            {
                playerProfile = accountData
            };

            return SaveToCloud(wrapper, forceImmediate);
        }

        /// <summary>
        /// Queues save data for later cloud sync (offline mode).
        /// </summary>
        public void QueueSave(SaveData data)
        {
            if (data == null) return;

            if (_offlineQueue.Count >= MAX_OFFLINE_QUEUE_SIZE)
            {
                Debug.LogWarning($"[CloudSave] Offline queue full ({MAX_OFFLINE_QUEUE_SIZE}), removing oldest");
                _offlineQueue.Dequeue();
            }

            _offlineQueue.Enqueue(data);
            Debug.Log($"[CloudSave] Queued save for later. Queue size: {_offlineQueue.Count}");
        }

        #endregion

        #region Public API - Load

        /// <summary>
        /// Loads player save data from Unity Cloud Save.
        /// Returns null if no cloud save exists (new player).
        /// </summary>
        public async Task<SaveData> LoadFromCloud()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot load from cloud");
                OnSaveError?.Invoke("Cannot load while offline");
                return null;
            }

            try
            {
                // Load all save data keys
                var cloudData = await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { SAVE_DATA_KEY, SAVE_METADATA_KEY, SAVE_HASH_KEY, LAST_SYNC_TIME_KEY }
                );

                // Check if save exists
                if (!cloudData.ContainsKey(SAVE_DATA_KEY))
                {
                    Debug.Log("[CloudSave] No cloud save found - new player");
                    return null;
                }

                // Deserialize save data
                string saveJson = cloudData[SAVE_DATA_KEY].Value.GetAsString();
                SaveData data = JsonUtility.FromJson<SaveData>(saveJson);

                // Verify hash (anti-cheat)
                if (enableAntiCheat && cloudData.ContainsKey(SAVE_HASH_KEY))
                {
                    string storedHash = cloudData[SAVE_HASH_KEY].Value.GetAsString();
                    string computedHash = ComputeDataHash(saveJson);

                    if (storedHash != computedHash)
                    {
                        Debug.LogWarning("[CloudSave] ⚠ Hash mismatch - data may have been tampered with!");
                        OnSaveError?.Invoke("Data integrity check failed");
                        // In production: flag account, reject data, or trigger investigation
                    }
                }

                // Validate loaded data
                if (enableAntiCheat && !data.Validate())
                {
                    Debug.LogError("[CloudSave] Loaded data failed validation");
                    OnSaveError?.Invoke("Loaded data is invalid");
                    return null;
                }

                Debug.Log($"[CloudSave] ✓ Successfully loaded from cloud for player: {data.playerID}");
                Debug.Log($"[CloudSave]   - Level: {data.progression.level}, XP: {data.progression.experience}");
                Debug.Log($"[CloudSave]   - Save count: {data.saveCount}");
                Debug.Log($"[CloudSave]   - Last save: {DateTimeOffset.FromUnixTimeSeconds(data.lastSaveTimestamp)}");

                _currentSaveData = data;
                OnLoadCompleted?.Invoke(data);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to load from cloud: {e.Message}\n{e.StackTrace}");
                OnSaveError?.Invoke(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Loads only the player profile from cloud save data.
        /// </summary>
        public async Task<PlayerAccountData> LoadAccountFromCloud()
        {
            var save = await LoadFromCloud();
            return save?.playerProfile;
        }

        /// <summary>
        /// Loads save data with automatic conflict resolution.
        /// Merges cloud and local data if both exist.
        /// </summary>
        public async Task<SaveData> LoadWithConflictResolution(SaveData localData)
        {
            SaveData cloudData = await LoadFromCloud();

            if (cloudData == null)
            {
                // No cloud save exists, use local
                Debug.Log("[CloudSave] No cloud save found, using local data");
                return localData;
            }

            if (localData == null)
            {
                // No local save exists, use cloud
                Debug.Log("[CloudSave] No local save found, using cloud data");
                return cloudData;
            }

            // Both exist - resolve conflict
            Debug.LogWarning("[CloudSave] Conflict detected - both cloud and local saves exist");
            OnConflictDetected?.Invoke();

            ConflictResolutionResult result = ResolveConflict(cloudData, localData, defaultConflictStrategy);

            Debug.Log($"[CloudSave] Conflict resolved using strategy: {result.strategyUsed}");
            Debug.Log($"[CloudSave] {result.conflictDetails}");

            return result.resolvedData;
        }

        #endregion

        #region Public API - Conflict Resolution

        /// <summary>
        /// Resolves conflicts between cloud and local save data.
        /// </summary>
        public ConflictResolutionResult ResolveConflict(SaveData cloudData, SaveData localData, ConflictResolutionStrategy strategy)
        {
            var result = new ConflictResolutionResult
            {
                hadConflict = true,
                strategyUsed = strategy
            };

            switch (strategy)
            {
                case ConflictResolutionStrategy.TakeNewest:
                    result.resolvedData = (cloudData.lastSaveTimestamp > localData.lastSaveTimestamp) ? cloudData : localData;
                    result.conflictDetails = $"Selected {(result.resolvedData == cloudData ? "cloud" : "local")} data (newer timestamp)";
                    break;

                case ConflictResolutionStrategy.TakeHighestProgress:
                    result.resolvedData = (cloudData.progression.level >= localData.progression.level) ? cloudData : localData;
                    result.conflictDetails = $"Selected {(result.resolvedData == cloudData ? "cloud" : "local")} data (level {result.resolvedData.progression.level})";
                    break;

                case ConflictResolutionStrategy.Merge:
                    result.resolvedData = MergeSaveData(cloudData, localData);
                    result.conflictDetails = "Merged cloud and local data (union of unlocks, max values for stats)";
                    break;

                case ConflictResolutionStrategy.AskUser:
                    // For now, default to TakeNewest
                    // In production: show UI dialog to user
                    result.resolvedData = (cloudData.lastSaveTimestamp > localData.lastSaveTimestamp) ? cloudData : localData;
                    result.conflictDetails = "User choice required (defaulting to newest)";
                    Debug.LogWarning("[CloudSave] AskUser strategy not implemented, defaulting to TakeNewest");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Merges cloud and local save data using intelligent conflict resolution.
        /// Strategy: Max values for stats, union for unlocks, newest for settings.
        /// </summary>
        public SaveData MergeSaveData(SaveData cloudData, SaveData localData)
        {
            if (cloudData == null) return localData;
            if (localData == null) return cloudData;

            Debug.Log("[CloudSave] Performing deep merge of cloud and local data...");

            SaveData merged = new SaveData();

            // Meta - prefer cloud
            merged.playerID = cloudData.playerID;
            merged.saveVersion = cloudData.saveVersion;
            merged.deviceID = cloudData.deviceID;
            merged.saveCount = Mathf.Max(cloudData.saveCount, localData.saveCount);
            merged.lastSaveTimestamp = Mathf.Max(cloudData.lastSaveTimestamp, localData.lastSaveTimestamp);

            // Profile - merge intelligently
            merged.profile = MergeProfile(cloudData.profile, localData.profile);

            // Currency - take max (never lose currency)
            merged.currency = MergeCurrency(cloudData.currency, localData.currency);

            // Progression - take highest
            merged.progression = MergeProgression(cloudData.progression, localData.progression);

            // Quests - merge active quests
            merged.quests = MergeQuests(cloudData.quests, localData.quests);

            // Achievements - union of unlocked
            merged.achievements = MergeAchievements(cloudData.achievements, localData.achievements);

            // Statistics - take max values
            merged.statistics = MergeStatistics(cloudData.statistics, localData.statistics);

            // Settings - prefer newest
            merged.settings = (cloudData.lastSaveTimestamp > localData.lastSaveTimestamp)
                ? cloudData.settings
                : localData.settings;

            // Unlockables - union
            merged.unlockables = MergeUnlockables(cloudData.unlockables, localData.unlockables);

            // Leaderboard stats - take best
            merged.leaderboardStats = MergeLeaderboardStats(cloudData.leaderboardStats, localData.leaderboardStats);

            // Analytics queue - combine
            merged.analyticsQueue = MergeAnalyticsQueue(cloudData.analyticsQueue, localData.analyticsQueue);

            Debug.Log("[CloudSave] ✓ Merge complete");
            return merged;
        }

        /// <summary>
        /// Lightweight merge for PlayerAccountData-only use cases.
        /// </summary>
        public PlayerAccountData MergeData(PlayerAccountData cloudData, PlayerAccountData localData)
        {
            if (cloudData == null) return localData;
            if (localData == null) return cloudData;

            // Prefer the most recently updated profile based on last login timestamp
            return cloudData.lastLoginTimestamp >= localData.lastLoginTimestamp ? cloudData : localData;
        }

        #endregion

        #region Public API - Backup & Delete

        /// <summary>
        /// Creates a backup of the current cloud save.
        /// </summary>
        private async Task<bool> BackupCurrentSave()
        {
            try
            {
                var currentData = await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { SAVE_DATA_KEY }
                );

                if (currentData.ContainsKey(SAVE_DATA_KEY))
                {
                    string backupJson = currentData[SAVE_DATA_KEY].Value.GetAsString();
                    var backupData = new Dictionary<string, object>
                    {
                        { BACKUP_DATA_KEY, backupJson }
                    };
                    await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.SaveAsync(backupData);
                    Debug.Log("[CloudSave] Backup created successfully");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CloudSave] Backup failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes all cloud save data for the current player.
        /// WARNING: This is irreversible!
        /// </summary>
        public async Task<bool> DeleteCloudSave()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot delete cloud save");
                return false;
            }

            try
            {
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(SAVE_DATA_KEY);
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(SAVE_METADATA_KEY);
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(SAVE_HASH_KEY);
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(BACKUP_DATA_KEY);
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(LAST_SYNC_TIME_KEY);

                Debug.Log("[CloudSave] ✓ Cloud save deleted successfully");
                _currentSaveData = null;
                _hasUnsavedChanges = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to delete cloud save: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
        }

        #endregion

        #region Offline Queue Processing

        private void Update()
        {
            // Process offline queue when connection is restored
            if (!_isSyncing && _offlineQueue.Count > 0 && IsOnline())
            {
                ProcessOfflineQueue();
            }

            // Auto-sync if enabled and has unsaved changes
            if (enableAutoSync && _hasUnsavedChanges && Time.time - _lastSaveTime >= autoSyncInterval)
            {
                if (_currentSaveData != null)
                {
                    SaveToCloud(_currentSaveData, forceImmediate: true);
                }
            }
        }

        private async void ProcessOfflineQueue()
        {
            _isSyncing = true;

            Debug.Log($"[CloudSave] Processing offline queue ({_offlineQueue.Count} items)");

            int processedCount = 0;
            while (_offlineQueue.Count > 0 && IsOnline())
            {
                var data = _offlineQueue.Dequeue();
                bool success = await SaveToCloud(data, forceImmediate: true);

                if (success)
                {
                    processedCount++;
                }
                else
                {
                    // Put it back if save failed
                    _offlineQueue.Enqueue(data);
                    break;
                }
            }

            Debug.Log($"[CloudSave] Processed {processedCount} queued saves");
            _isSyncing = false;
        }

        #endregion

        #region Anti-Cheat & Validation

        /// <summary>
        /// Computes SHA256 hash of save data JSON for integrity verification.
        /// </summary>
        private string ComputeDataHash(string json)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        #endregion

        #region Merge Helper Methods

        private PlayerAccountData MergeProfile(PlayerAccountData cloud, PlayerAccountData local)
        {
            var merged = new PlayerAccountData();
            merged.username = (cloud.lastLoginTimestamp > local.lastLoginTimestamp) ? cloud.username : local.username;
            merged.avatarID = (cloud.lastLoginTimestamp > local.lastLoginTimestamp) ? cloud.avatarID : local.avatarID;
            merged.customTitle = (cloud.lastLoginTimestamp > local.lastLoginTimestamp) ? cloud.customTitle : local.customTitle;
            merged.accountCreatedTimestamp = Mathf.Min(cloud.accountCreatedTimestamp, local.accountCreatedTimestamp);
            merged.lastLoginTimestamp = Mathf.Max(cloud.lastLoginTimestamp, local.lastLoginTimestamp);
            merged.totalPlaytimeSeconds = Mathf.Max(cloud.totalPlaytimeSeconds, local.totalPlaytimeSeconds);
            merged.loginStreak = Mathf.Max(cloud.loginStreak, local.loginStreak);
            merged.lastLoginStreakTimestamp = Mathf.Max(cloud.lastLoginStreakTimestamp, local.lastLoginStreakTimestamp);
            return merged;
        }

        private CurrencyData MergeCurrency(CurrencyData cloud, CurrencyData local)
        {
            var merged = new CurrencyData();
            merged.credits = Mathf.Max(cloud.credits, local.credits);
            merged.gems = Mathf.Max(cloud.gems, local.gems);
            merged.premiumCurrency = Mathf.Max(cloud.premiumCurrency, local.premiumCurrency);
            merged.lifetimeSoftCurrencyEarned = Mathf.Max(cloud.lifetimeSoftCurrencyEarned, local.lifetimeSoftCurrencyEarned);
            merged.lifetimeSoftCurrencySpent = Mathf.Max(cloud.lifetimeSoftCurrencySpent, local.lifetimeSoftCurrencySpent);
            merged.lifetimeHardCurrencyEarned = Mathf.Max(cloud.lifetimeHardCurrencyEarned, local.lifetimeHardCurrencyEarned);
            merged.lifetimeHardCurrencySpent = Mathf.Max(cloud.lifetimeHardCurrencySpent, local.lifetimeHardCurrencySpent);
            // Merge transaction history (combine both, keep last 100)
            merged.recentTransactions = new List<CurrencyTransaction>();
            merged.recentTransactions.AddRange(cloud.recentTransactions);
            merged.recentTransactions.AddRange(local.recentTransactions);
            merged.recentTransactions.Sort((a, b) => b.timestamp.CompareTo(a.timestamp));
            if (merged.recentTransactions.Count > 100)
                merged.recentTransactions.RemoveRange(100, merged.recentTransactions.Count - 100);
            return merged;
        }

        private ProgressionData MergeProgression(ProgressionData cloud, ProgressionData local)
        {
            var merged = new ProgressionData();
            merged.level = Mathf.Max(cloud.level, local.level);
            merged.experience = Mathf.Max(cloud.experience, local.experience);
            merged.prestigeLevel = Mathf.Max(cloud.prestigeLevel, local.prestigeLevel);
            merged.prestigePoints = Mathf.Max(cloud.prestigePoints, local.prestigePoints);

            // Merge skill unlocks (union)
            var skillDict = new Dictionary<string, int>();
            for (int i = 0; i < cloud.unlockedSkillIDs.Count; i++)
                skillDict[cloud.unlockedSkillIDs[i]] = cloud.skillLevels[i];
            for (int i = 0; i < local.unlockedSkillIDs.Count; i++)
            {
                string skillID = local.unlockedSkillIDs[i];
                if (skillDict.ContainsKey(skillID))
                    skillDict[skillID] = Mathf.Max(skillDict[skillID], local.skillLevels[i]);
                else
                    skillDict[skillID] = local.skillLevels[i];
            }
            merged.unlockedSkillIDs = new List<string>(skillDict.Keys);
            merged.skillLevels = new List<int>(skillDict.Values);

            // Merge tutorial and milestones (union)
            merged.completedTutorials = UnionLists(cloud.completedTutorials, local.completedTutorials);
            merged.reachedMilestones = UnionLists(cloud.reachedMilestones, local.reachedMilestones);
            merged.hasCompletedOnboarding = cloud.hasCompletedOnboarding || local.hasCompletedOnboarding;
            return merged;
        }

        private QuestSaveData MergeQuests(QuestSaveData cloud, QuestSaveData local)
        {
            var merged = new QuestSaveData();

            // Merge active quests (prefer cloud if same questID, otherwise combine)
            var questDict = new Dictionary<string, ActiveQuestData>();
            foreach (var quest in cloud.activeQuests)
                questDict[quest.questID] = quest;
            foreach (var quest in local.activeQuests)
            {
                if (questDict.ContainsKey(quest.questID))
                {
                    // Take highest progress
                    if (quest.currentProgress > questDict[quest.questID].currentProgress)
                        questDict[quest.questID] = quest;
                }
                else
                {
                    questDict[quest.questID] = quest;
                }
            }
            merged.activeQuests = new List<ActiveQuestData>(questDict.Values);

            // Merge completed/claimed (union)
            merged.completedQuestIDs = UnionLists(cloud.completedQuestIDs, local.completedQuestIDs);
            merged.claimedQuestIDs = UnionLists(cloud.claimedQuestIDs, local.claimedQuestIDs);

            // Refresh timestamps - take newest
            merged.lastDailyRefreshTimestamp = Mathf.Max(cloud.lastDailyRefreshTimestamp, local.lastDailyRefreshTimestamp);
            merged.lastWeeklyRefreshTimestamp = Mathf.Max(cloud.lastWeeklyRefreshTimestamp, local.lastWeeklyRefreshTimestamp);
            merged.lastSeasonRefreshTimestamp = Mathf.Max(cloud.lastSeasonRefreshTimestamp, local.lastSeasonRefreshTimestamp);

            // Stats - take max
            merged.totalQuestsCompleted = Mathf.Max(cloud.totalQuestsCompleted, local.totalQuestsCompleted);
            merged.dailyQuestsCompleted = Mathf.Max(cloud.dailyQuestsCompleted, local.dailyQuestsCompleted);
            merged.weeklyQuestsCompleted = Mathf.Max(cloud.weeklyQuestsCompleted, local.weeklyQuestsCompleted);
            merged.seasonQuestsCompleted = Mathf.Max(cloud.seasonQuestsCompleted, local.seasonQuestsCompleted);
            return merged;
        }

        private AchievementSaveData MergeAchievements(AchievementSaveData cloud, AchievementSaveData local)
        {
            var merged = new AchievementSaveData();

            // Merge achievement progress
            var achievementDict = new Dictionary<string, (int progress, int tier, bool unlocked, long timestamp)>();

            for (int i = 0; i < cloud.achievementIDs.Count; i++)
            {
                achievementDict[cloud.achievementIDs[i]] = (
                    cloud.achievementProgress[i],
                    cloud.currentTiers[i],
                    cloud.isUnlocked[i],
                    cloud.unlockTimestamps[i]
                );
            }

            for (int i = 0; i < local.achievementIDs.Count; i++)
            {
                string achID = local.achievementIDs[i];
                if (achievementDict.ContainsKey(achID))
                {
                    var existing = achievementDict[achID];
                    achievementDict[achID] = (
                        Mathf.Max(existing.progress, local.achievementProgress[i]),
                        Mathf.Max(existing.tier, local.currentTiers[i]),
                        existing.unlocked || local.isUnlocked[i],
                        Mathf.Min(existing.timestamp, local.unlockTimestamps[i]) // First unlock timestamp
                    );
                }
                else
                {
                    achievementDict[achID] = (
                        local.achievementProgress[i],
                        local.currentTiers[i],
                        local.isUnlocked[i],
                        local.unlockTimestamps[i]
                    );
                }
            }

            merged.achievementIDs = new List<string>(achievementDict.Keys);
            merged.achievementProgress = new List<int>();
            merged.currentTiers = new List<int>();
            merged.isUnlocked = new List<bool>();
            merged.unlockTimestamps = new List<long>();

            foreach (var kvp in achievementDict)
            {
                merged.achievementProgress.Add(kvp.Value.progress);
                merged.currentTiers.Add(kvp.Value.tier);
                merged.isUnlocked.Add(kvp.Value.unlocked);
                merged.unlockTimestamps.Add(kvp.Value.timestamp);
            }

            // Merge lifetime stats
            merged.lifetimeStats = new Dictionary<string, int>();
            foreach (var kvp in cloud.lifetimeStats)
                merged.lifetimeStats[kvp.Key] = kvp.Value;
            foreach (var kvp in local.lifetimeStats)
            {
                if (merged.lifetimeStats.ContainsKey(kvp.Key))
                    merged.lifetimeStats[kvp.Key] = Mathf.Max(merged.lifetimeStats[kvp.Key], kvp.Value);
                else
                    merged.lifetimeStats[kvp.Key] = kvp.Value;
            }

            merged.totalAchievementPoints = Mathf.Max(cloud.totalAchievementPoints, local.totalAchievementPoints);
            merged.totalAchievementsUnlocked = Mathf.Max(cloud.totalAchievementsUnlocked, local.totalAchievementsUnlocked);
            merged.totalSecretAchievementsUnlocked = Mathf.Max(cloud.totalSecretAchievementsUnlocked, local.totalSecretAchievementsUnlocked);
            return merged;
        }

        private PlayerStatistics MergeStatistics(PlayerStatistics cloud, PlayerStatistics local)
        {
            var merged = new PlayerStatistics();
            merged.totalMatchesPlayed = Mathf.Max(cloud.totalMatchesPlayed, local.totalMatchesPlayed);
            merged.totalMatchesWon = Mathf.Max(cloud.totalMatchesWon, local.totalMatchesWon);
            merged.totalMatchesLost = Mathf.Max(cloud.totalMatchesLost, local.totalMatchesLost);
            merged.totalRoundsPlayed = Mathf.Max(cloud.totalRoundsPlayed, local.totalRoundsPlayed);
            merged.totalRoundsWon = Mathf.Max(cloud.totalRoundsWon, local.totalRoundsWon);
            merged.currentWinStreak = Mathf.Max(cloud.currentWinStreak, local.currentWinStreak);
            merged.longestWinStreak = Mathf.Max(cloud.longestWinStreak, local.longestWinStreak);
            merged.currentLossStreak = Mathf.Max(cloud.currentLossStreak, local.currentLossStreak);
            merged.totalDamageDealt = Mathf.Max(cloud.totalDamageDealt, local.totalDamageDealt);
            merged.totalDamageTaken = Mathf.Max(cloud.totalDamageTaken, local.totalDamageTaken);
            merged.totalMissilesFired = Mathf.Max(cloud.totalMissilesFired, local.totalMissilesFired);
            merged.totalMissilesHit = Mathf.Max(cloud.totalMissilesHit, local.totalMissilesHit);
            merged.totalKills = Mathf.Max(cloud.totalKills, local.totalKills);
            merged.totalDeaths = Mathf.Max(cloud.totalDeaths, local.totalDeaths);
            merged.perfectWins = Mathf.Max(cloud.perfectWins, local.perfectWins);
            merged.flawlessRounds = Mathf.Max(cloud.flawlessRounds, local.flawlessRounds);
            merged.comebackWins = Mathf.Max(cloud.comebackWins, local.comebackWins);
            merged.totalPlaytimeSeconds = Mathf.Max(cloud.totalPlaytimeSeconds, local.totalPlaytimeSeconds);
            merged.fastestWinSeconds = Mathf.Min(cloud.fastestWinSeconds, local.fastestWinSeconds);
            merged.longestMatchSeconds = Mathf.Max(cloud.longestMatchSeconds, local.longestMatchSeconds);
            merged.trickshotHits = Mathf.Max(cloud.trickshotHits, local.trickshotHits);
            merged.selfDestructs = Mathf.Max(cloud.selfDestructs, local.selfDestructs);
            merged.environmentalKills = Mathf.Max(cloud.environmentalKills, local.environmentalKills);

            // Merge weapon stats
            merged.weaponStats = new Dictionary<string, WeaponStats>();
            foreach (var kvp in cloud.weaponStats)
                merged.weaponStats[kvp.Key] = kvp.Value;
            foreach (var kvp in local.weaponStats)
            {
                if (merged.weaponStats.ContainsKey(kvp.Key))
                {
                    var cloudWeapon = merged.weaponStats[kvp.Key];
                    merged.weaponStats[kvp.Key] = new WeaponStats
                    {
                        weaponID = kvp.Key,
                        shotsFired = Mathf.Max(cloudWeapon.shotsFired, kvp.Value.shotsFired),
                        shotsHit = Mathf.Max(cloudWeapon.shotsHit, kvp.Value.shotsHit),
                        kills = Mathf.Max(cloudWeapon.kills, kvp.Value.kills),
                        damageDealt = Mathf.Max(cloudWeapon.damageDealt, kvp.Value.damageDealt)
                    };
                }
                else
                {
                    merged.weaponStats[kvp.Key] = kvp.Value;
                }
            }

            // Merge map stats
            merged.mapStats = new Dictionary<string, MapStats>();
            foreach (var kvp in cloud.mapStats)
                merged.mapStats[kvp.Key] = kvp.Value;
            foreach (var kvp in local.mapStats)
            {
                if (merged.mapStats.ContainsKey(kvp.Key))
                {
                    var cloudMap = merged.mapStats[kvp.Key];
                    merged.mapStats[kvp.Key] = new MapStats
                    {
                        mapID = kvp.Key,
                        matchesPlayed = Mathf.Max(cloudMap.matchesPlayed, kvp.Value.matchesPlayed),
                        matchesWon = Mathf.Max(cloudMap.matchesWon, kvp.Value.matchesWon),
                        favoriteCount = Mathf.Max(cloudMap.favoriteCount, kvp.Value.favoriteCount)
                    };
                }
                else
                {
                    merged.mapStats[kvp.Key] = kvp.Value;
                }
            }

            return merged;
        }

        private UnlockablesData MergeUnlockables(UnlockablesData cloud, UnlockablesData local)
        {
            var merged = new UnlockablesData();
            merged.unlockedSkins = UnionLists(cloud.unlockedSkins, local.unlockedSkins);
            merged.unlockedTrails = UnionLists(cloud.unlockedTrails, local.unlockedTrails);
            merged.unlockedEmotes = UnionLists(cloud.unlockedEmotes, local.unlockedEmotes);
            merged.unlockedTitles = UnionLists(cloud.unlockedTitles, local.unlockedTitles);
            merged.unlockedAvatars = UnionLists(cloud.unlockedAvatars, local.unlockedAvatars);
            merged.unlockedShips = UnionLists(cloud.unlockedShips, local.unlockedShips);
            merged.unlockedWeapons = UnionLists(cloud.unlockedWeapons, local.unlockedWeapons);
            merged.unlockedPerks = UnionLists(cloud.unlockedPerks, local.unlockedPerks);
            merged.unlockedMaps = UnionLists(cloud.unlockedMaps, local.unlockedMaps);
            merged.unlockedGameModes = UnionLists(cloud.unlockedGameModes, local.unlockedGameModes);

            // Active items - prefer cloud
            merged.activeSkin = string.IsNullOrEmpty(cloud.activeSkin) ? local.activeSkin : cloud.activeSkin;
            merged.activeTrail = string.IsNullOrEmpty(cloud.activeTrail) ? local.activeTrail : cloud.activeTrail;
            merged.activeTitle = string.IsNullOrEmpty(cloud.activeTitle) ? local.activeTitle : cloud.activeTitle;
            merged.activeAvatar = cloud.activeAvatar != 0 ? cloud.activeAvatar : local.activeAvatar;
            merged.activeShip = string.IsNullOrEmpty(cloud.activeShip) ? local.activeShip : cloud.activeShip;
            merged.activeWeaponLoadout = cloud.activeWeaponLoadout.Count > 0 ? cloud.activeWeaponLoadout : local.activeWeaponLoadout;
            merged.activePerks = cloud.activePerks.Count > 0 ? cloud.activePerks : local.activePerks;
            return merged;
        }

        private LeaderboardStatsData MergeLeaderboardStats(LeaderboardStatsData cloud, LeaderboardStatsData local)
        {
            var merged = new LeaderboardStatsData();
            merged.bestScore = Mathf.Max(cloud.bestScore, local.bestScore);
            merged.bestWinStreak = Mathf.Max(cloud.bestWinStreak, local.bestWinStreak);
            merged.bestAccuracy = Math.Max(cloud.bestAccuracy, local.bestAccuracy);
            merged.fastestWinSeconds = Mathf.Min(cloud.fastestWinSeconds, local.fastestWinSeconds);
            merged.highestDamageInMatch = Mathf.Max(cloud.highestDamageInMatch, local.highestDamageInMatch);
            merged.currentMMR = Mathf.Max(cloud.currentMMR, local.currentMMR);
            merged.peakMMR = Mathf.Max(cloud.peakMMR, local.peakMMR);
            merged.rankedPoints = Mathf.Max(cloud.rankedPoints, local.rankedPoints);
            merged.currentSeasonWins = Mathf.Max(cloud.currentSeasonWins, local.currentSeasonWins);
            merged.currentSeasonMatches = Mathf.Max(cloud.currentSeasonMatches, local.currentSeasonMatches);
            merged.currentSeasonStartTimestamp = Mathf.Min(cloud.currentSeasonStartTimestamp, local.currentSeasonStartTimestamp);

            // Merge submission timestamps
            merged.lastSubmissionTimestamps = new Dictionary<string, long>();
            foreach (var kvp in cloud.lastSubmissionTimestamps)
                merged.lastSubmissionTimestamps[kvp.Key] = kvp.Value;
            foreach (var kvp in local.lastSubmissionTimestamps)
            {
                if (merged.lastSubmissionTimestamps.ContainsKey(kvp.Key))
                    merged.lastSubmissionTimestamps[kvp.Key] = Mathf.Max(merged.lastSubmissionTimestamps[kvp.Key], kvp.Value);
                else
                    merged.lastSubmissionTimestamps[kvp.Key] = kvp.Value;
            }
            return merged;
        }

        private AnalyticsQueueData MergeAnalyticsQueue(AnalyticsQueueData cloud, AnalyticsQueueData local)
        {
            var merged = new AnalyticsQueueData();
            merged.queuedEvents = new List<QueuedAnalyticsEvent>();
            merged.queuedEvents.AddRange(cloud.queuedEvents);
            merged.queuedEvents.AddRange(local.queuedEvents);
            merged.queuedEvents.Sort((a, b) => a.timestamp.CompareTo(b.timestamp)); // Oldest first
            merged.lastUploadTimestamp = Mathf.Max(cloud.lastUploadTimestamp, local.lastUploadTimestamp);
            merged.failedUploadCount = Mathf.Min(cloud.failedUploadCount, local.failedUploadCount); // Take lower for retry
            return merged;
        }

        private List<string> UnionLists(List<string> list1, List<string> list2)
        {
            var union = new HashSet<string>();
            if (list1 != null)
                foreach (var item in list1)
                    union.Add(item);
            if (list2 != null)
                foreach (var item in list2)
                    union.Add(item);
            return new List<string>(union);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if device has internet connection and Unity Services are ready.
        /// </summary>
        private bool IsOnline()
        {
            bool hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
            bool servicesReady = ServiceLocator.Instance != null && ServiceLocator.Instance.IsReady();
            return hasInternet && servicesReady;
        }

        /// <summary>
        /// Gets the current save data (local copy).
        /// </summary>
        public SaveData GetCurrentSaveData()
        {
            return _currentSaveData;
        }

        /// <summary>
        /// Marks that local save data has changed and needs to be synced.
        /// </summary>
        public void MarkDirty()
        {
            _hasUnsavedChanges = true;
        }

        #endregion

        #region PlayerAccountData API (Competitive Multiplayer)

        /// <summary>
        /// Save complete player profile for competitive multiplayer.
        /// Includes ELO rating, match history, statistics, and all account data.
        /// </summary>
        public async Task<bool> SavePlayerProfile(PlayerAccountData profile)
        {
            if (profile == null)
            {
                Debug.LogError("[CloudSave] Cannot save null PlayerAccountData");
                return false;
            }

            // Rate limiting check
            if (Time.time - _lastSaveTime < MIN_SAVE_INTERVAL)
            {
                Debug.LogWarning($"[CloudSave] Profile save rate limited");
                return false;
            }

            // Check if online
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot save player profile");
                return false;
            }

            try
            {
                // Backup current profile before overwriting
                if (enableBackups)
                {
                    await BackupCurrentProfile();
                }

                // Serialize profile data
                string profileJson = JsonUtility.ToJson(profile);
                string profileHash = enableAntiCheat ? ComputeDataHash(profileJson) : "";

                // Prepare cloud save dictionary
                var cloudData = new Dictionary<string, object>
                {
                    { PLAYER_PROFILE_KEY, profileJson },
                    { LAST_SYNC_TIME_KEY, DateTime.UtcNow.ToString("o") }
                };

                if (enableAntiCheat)
                {
                    cloudData[PROFILE_HASH_KEY] = profileHash;
                }

                // Save to Unity Cloud Save
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.SaveAsync(cloudData);

                Debug.Log($"[CloudSave] ✓ Successfully saved player profile for: {profile.username}");
                Debug.Log($"[CloudSave]   - ELO: {profile.eloRating}, Rank: {profile.currentRank}");
                Debug.Log($"[CloudSave]   - Matches: {profile.rankedMatchesPlayed + profile.casualMatchesPlayed}");
                Debug.Log($"[CloudSave]   - Profile size: {profileJson.Length} bytes");

                _lastSaveTime = Time.time;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to save player profile: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Load complete player profile from cloud storage.
        /// Returns null if no profile exists (new player).
        /// </summary>
        public async Task<PlayerAccountData> LoadPlayerProfile()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot load player profile");
                return null;
            }

            try
            {
                // Load profile data keys
                var cloudData = await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { PLAYER_PROFILE_KEY, PROFILE_HASH_KEY, LAST_SYNC_TIME_KEY }
                );

                // Check if profile exists
                if (!cloudData.ContainsKey(PLAYER_PROFILE_KEY))
                {
                    Debug.Log("[CloudSave] No player profile found - new player");
                    return null;
                }

                // Deserialize profile data
                string profileJson = cloudData[PLAYER_PROFILE_KEY].Value.GetAsString();
                PlayerAccountData profile = JsonUtility.FromJson<PlayerAccountData>(profileJson);

                // Verify hash (anti-cheat)
                if (enableAntiCheat && cloudData.ContainsKey(PROFILE_HASH_KEY))
                {
                    string storedHash = cloudData[PROFILE_HASH_KEY].Value.GetAsString();
                    string computedHash = ComputeDataHash(profileJson);

                    if (storedHash != computedHash)
                    {
                        Debug.LogWarning("[CloudSave] ⚠ Profile hash mismatch - possible tampering detected!");
                        // In production: flag account for investigation
                    }
                }

                Debug.Log($"[CloudSave] ✓ Successfully loaded player profile: {profile.username}");
                Debug.Log($"[CloudSave]   - ELO: {profile.eloRating}, Rank: {profile.currentRank}");
                Debug.Log($"[CloudSave]   - Level: {profile.level}, XP: {profile.currentXP}");
                Debug.Log($"[CloudSave]   - Ranked W/L: {profile.rankedMatchesWon}/{profile.rankedMatchesPlayed - profile.rankedMatchesWon}");

                return profile;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to load player profile: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Update specific profile fields (ELO, stats, match history).
        /// More efficient than saving entire profile.
        /// </summary>
        public async Task<bool> UpdateProfileFields(string playerId, Dictionary<string, object> updates)
        {
            if (updates == null || updates.Count == 0)
            {
                Debug.LogWarning("[CloudSave] No fields to update");
                return false;
            }

            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot update profile fields");
                return false;
            }

            try
            {
                // Load current profile
                PlayerAccountData profile = await LoadPlayerProfile();
                if (profile == null)
                {
                    Debug.LogError("[CloudSave] Cannot update - profile does not exist");
                    return false;
                }

                // Apply updates using reflection (or manual field updates)
                foreach (var kvp in updates)
                {
                    var field = typeof(PlayerAccountData).GetField(kvp.Key);
                    if (field != null)
                    {
                        field.SetValue(profile, kvp.Value);
                    }
                    else
                    {
                        Debug.LogWarning($"[CloudSave] Field not found: {kvp.Key}");
                    }
                }

                // Save updated profile
                return await SavePlayerProfile(profile);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to update profile fields: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create backup of current player profile.
        /// </summary>
        private async Task<bool> BackupCurrentProfile()
        {
            try
            {
                var currentData = await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { PLAYER_PROFILE_KEY }
                );

                if (currentData.ContainsKey(PLAYER_PROFILE_KEY))
                {
                    string backupJson = currentData[PLAYER_PROFILE_KEY].Value.GetAsString();
                    var backupData = new Dictionary<string, object>
                    {
                        { PROFILE_BACKUP_KEY, backupJson }
                    };
                    await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.SaveAsync(backupData);
                    Debug.Log("[CloudSave] Profile backup created successfully");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CloudSave] Profile backup failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Restore player profile from backup.
        /// </summary>
        public async Task<PlayerAccountData> RestoreProfileFromBackup()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot restore profile backup");
                return null;
            }

            try
            {
                var backupData = await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { PROFILE_BACKUP_KEY }
                );

                if (!backupData.ContainsKey(PROFILE_BACKUP_KEY))
                {
                    Debug.LogWarning("[CloudSave] No profile backup found");
                    return null;
                }

                string backupJson = backupData[PROFILE_BACKUP_KEY].Value.GetAsString();
                PlayerAccountData profile = JsonUtility.FromJson<PlayerAccountData>(backupJson);

                Debug.Log($"[CloudSave] ✓ Restored profile from backup: {profile.username}");
                return profile;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to restore profile backup: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete player profile from cloud (account deletion).
        /// WARNING: This is irreversible!
        /// </summary>
        public async Task<bool> DeletePlayerProfile()
        {
            if (!IsOnline())
            {
                Debug.LogWarning("[CloudSave] Offline - cannot delete player profile");
                return false;
            }

            try
            {
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(PLAYER_PROFILE_KEY);
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(PROFILE_HASH_KEY);
                await Unity.Services.CloudSave.CloudSaveService.Instance.Data.Player.DeleteAsync(PROFILE_BACKUP_KEY);

                Debug.Log("[CloudSave] ✓ Player profile deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudSave] Failed to delete player profile: {e.Message}");
                return false;
            }
        }

        #endregion
    }
}
