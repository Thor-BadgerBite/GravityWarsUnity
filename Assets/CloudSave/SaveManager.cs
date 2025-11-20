using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using GravityWars.Networking;

namespace GravityWars.CloudSave
{
    /// <summary>
    /// SaveManager orchestrates all save/load operations across all game systems.
    ///
    /// Responsibilities:
    /// - Centralized save/load coordination
    /// - Auto-save with configurable intervals
    /// - Data collection from all services
    /// - Data distribution to all services
    /// - Local persistence (PlayerPrefs fallback)
    /// - Save/load event broadcasting
    ///
    /// Usage:
    ///   SaveManager.Instance.SaveGame();
    ///   await SaveManager.Instance.LoadGame();
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Singleton

        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SaveManager");
                        _instance = go.AddComponent<SaveManager>();
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

        #region Configuration

        [Header("Auto-Save Configuration")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveIntervalSeconds = 300f; // 5 minutes
        [SerializeField] private bool saveOnApplicationQuit = true;
        [SerializeField] private bool saveOnApplicationPause = true;

        [Header("Save Strategies")]
        [SerializeField] private bool enableCloudSave = true;
        [SerializeField] private bool enableLocalSave = true;
        [SerializeField] private bool loadFromCloudOnStart = true;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = false;

        #endregion

        #region Events

        public event Action<SaveData> OnSaveStarted;
        public event Action<SaveData> OnSaveCompleted;
        public event Action<SaveData> OnLoadCompleted;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;

        #endregion

        #region State

        private SaveData _currentSaveData;
        private float _timeSinceLastAutoSave = 0f;
        private bool _isSaving = false;
        private bool _isLoading = false;
        private bool _hasInitialized = false;

        private const string LOCAL_SAVE_KEY = "gravity_wars_save_data";

        #endregion

        #region Initialization

        private void Start()
        {
            if (!_hasInitialized)
            {
                Initialize();
            }
        }

        private async void Initialize()
        {
            _hasInitialized = true;
            Log("SaveManager initializing...");

            // Load game data
            await LoadGame();

            // Start auto-save coroutine
            if (enableAutoSave)
            {
                StartCoroutine(AutoSaveCoroutine());
            }

            Log("SaveManager initialized successfully");
        }

        #endregion

        #region Public API - Save

        /// <summary>
        /// Saves the current game state to both local and cloud storage.
        /// </summary>
        public async Task<bool> SaveGame(bool forceImmediate = false)
        {
            if (_isSaving)
            {
                LogWarning("Save already in progress, skipping");
                return false;
            }

            _isSaving = true;
            Log("=== SAVE GAME STARTED ===");

            try
            {
                // Collect data from all systems
                SaveData saveData = CollectSaveData();

                if (saveData == null)
                {
                    LogError("Failed to collect save data");
                    OnSaveError?.Invoke("Failed to collect save data");
                    return false;
                }

                OnSaveStarted?.Invoke(saveData);

                // Save to local storage (always, as fallback)
                if (enableLocalSave)
                {
                    SaveToLocal(saveData);
                }

                // Save to cloud storage
                bool cloudSaveSuccess = false;
                if (enableCloudSave)
                {
                    cloudSaveSuccess = await CloudSaveService.Instance.SaveToCloud(saveData, forceImmediate);
                }

                _currentSaveData = saveData;
                _timeSinceLastAutoSave = 0f;

                OnSaveCompleted?.Invoke(saveData);
                Log($"=== SAVE GAME COMPLETED (Cloud: {cloudSaveSuccess}, Local: {enableLocalSave}) ===");

                return true;
            }
            catch (Exception e)
            {
                LogError($"Save game failed: {e.Message}\n{e.StackTrace}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// Saves game without waiting (fire-and-forget).
        /// </summary>
        public void SaveGameAsync()
        {
            _ = SaveGame(forceImmediate: false);
        }

        #endregion

        #region Public API - Load

        /// <summary>
        /// Loads game data from cloud or local storage.
        /// </summary>
        public async Task<bool> LoadGame()
        {
            if (_isLoading)
            {
                LogWarning("Load already in progress, skipping");
                return false;
            }

            _isLoading = true;
            Log("=== LOAD GAME STARTED ===");

            try
            {
                SaveData saveData = null;

                // Try cloud load first
                if (enableCloudSave && loadFromCloudOnStart)
                {
                    SaveData localData = LoadFromLocal();
                    saveData = await CloudSaveService.Instance.LoadWithConflictResolution(localData);
                }

                // Fallback to local if cloud failed
                if (saveData == null && enableLocalSave)
                {
                    saveData = LoadFromLocal();
                }

                // If still no save data, create new
                if (saveData == null)
                {
                    Log("No existing save found, creating new save data");
                    saveData = CreateNewSaveData();
                }

                // Distribute data to all systems
                DistributeSaveData(saveData);

                _currentSaveData = saveData;
                OnLoadCompleted?.Invoke(saveData);

                Log($"=== LOAD GAME COMPLETED ===");
                Log($"  Player: {saveData.playerID}");
                Log($"  Level: {saveData.progression.level}");
                Log($"  Currency: {saveData.currency.credits}c / {saveData.currency.gems}g");

                return true;
            }
            catch (Exception e)
            {
                LogError($"Load game failed: {e.Message}\n{e.StackTrace}");
                OnLoadError?.Invoke(e.Message);
                return false;
            }
            finally
            {
                _isLoading = false;
            }
        }

        #endregion

        #region Data Collection

        /// <summary>
        /// Collects save data from all game systems/services.
        /// </summary>
        private SaveData CollectSaveData()
        {
            Log("Collecting save data from all systems...");

            SaveData data = _currentSaveData != null ? _currentSaveData : CreateNewSaveData();

            // Update timestamps
            data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            data.profile.lastLoginTimestamp = data.lastSaveTimestamp;

            // Collect from ProgressionManager
            if (ProgressionManager.Instance != null)
            {
                data.progression.level = ProgressionManager.Instance.AccountLevel;
                data.progression.experience = ProgressionManager.Instance.AccountXP;
                // Note: More fields could be collected from ProgressionManager if they exist
            }

            // Collect from EconomyService
            // TODO: Implement EconomyService or use ProgressionManager directly
            /* COMMENTED OUT - EconomyService not implemented yet
            if (EconomyService.Instance != null)
            {
                data.currency.credits = EconomyService.Instance.GetSoftCurrency();
                data.currency.gems = EconomyService.Instance.GetHardCurrency();
                // Note: EconomyService would need methods to expose these values
            }
            */

            // Collect from QuestService
            if (QuestService.Instance != null)
            {
                var activeQuests = QuestService.Instance.GetActiveQuests();
                data.quests.activeQuests.Clear();

                foreach (var quest in activeQuests)
                {
                    data.quests.activeQuests.Add(new ActiveQuestData
                    {
                        questID = quest.questID,
                        currentProgress = quest.currentProgress,
                        acceptedTimestamp = quest.acceptedTimestamp,
                        expirationTimestamp = quest.expirationTimestamp,
                        isCompleted = quest.isCompleted,
                        isClaimed = quest.isClaimed
                    });
                }

                // Note: Would need methods to get completed/claimed quest lists
            }

            // Collect from AchievementService
            if (AchievementService.Instance != null)
            {
                var achievements = AchievementService.Instance.GetAllAchievements();
                data.achievements.achievementIDs.Clear();
                data.achievements.achievementProgress.Clear();
                data.achievements.currentTiers.Clear();
                data.achievements.isUnlocked.Clear();
                data.achievements.unlockTimestamps.Clear();

                foreach (var achievement in achievements)
                {
                    data.achievements.achievementIDs.Add(achievement.achievementID);
                    data.achievements.achievementProgress.Add(achievement.currentProgress);
                    data.achievements.currentTiers.Add(achievement.currentTier);
                    data.achievements.isUnlocked.Add(achievement.isUnlocked);
                    data.achievements.unlockTimestamps.Add(achievement.unlockTimestamp);
                }

                data.achievements.totalAchievementPoints = AchievementService.Instance.GetTotalAchievementPoints();
                // Note: Would need method to get total points
            }

            // Collect from AnalyticsService
            if (AnalyticsService.Instance != null)
            {
                // Note: Could collect queued events if AnalyticsService exposes them
            }

            // Note: Additional services could be added here:
            // - LeaderboardService (stats tracking)
            // - Unlockables (from shop or progression system)
            // - Settings (from settings manager)

            Log($"Save data collected: {data.saveVersion}");
            return data;
        }

        #endregion

        #region Data Distribution

        /// <summary>
        /// Distributes loaded save data to all game systems/services.
        /// </summary>
        private void DistributeSaveData(SaveData data)
        {
            if (data == null)
            {
                LogError("Cannot distribute null save data");
                return;
            }

            Log("Distributing save data to all systems...");

            // Distribute to ProgressionManager
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.SetAccountLevel(data.progression.level);
                ProgressionManager.Instance.SetAccountXP(data.progression.experience);
                // Note: ProgressionManager would need setter methods
            }

            // Distribute to EconomyService
            // TODO: Implement EconomyService or use ProgressionManager directly
            /* COMMENTED OUT - EconomyService not implemented yet
            if (EconomyService.Instance != null)
            {
                EconomyService.Instance.SetSoftCurrency(data.currency.credits);
                EconomyService.Instance.SetHardCurrency(data.currency.gems);
                // Note: EconomyService would need setter methods
            }
            */

            // Distribute to QuestService
            if (QuestService.Instance != null)
            {
                // Note: QuestService would need a method to restore active quests
                // QuestService.Instance.RestoreQuests(data.quests);
            }

            // Distribute to AchievementService
            if (AchievementService.Instance != null)
            {
                // Note: AchievementService would need a method to restore progress
                // AchievementService.Instance.RestoreAchievements(data.achievements);
            }

            // Distribute to Settings
            if (data.settings != null)
            {
                AudioListener.volume = data.settings.masterVolume;
                QualitySettings.vSyncCount = data.settings.vsyncEnabled ? 1 : 0;
                Application.targetFrameRate = data.settings.targetFramerate;
                // Note: More settings could be applied here
            }

            Log("Save data distributed successfully");
        }

        #endregion

        #region Local Save/Load

        /// <summary>
        /// Saves data to local storage using PlayerPrefs.
        /// </summary>
        private void SaveToLocal(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(LOCAL_SAVE_KEY, json);
                PlayerPrefs.Save();
                Log($"Saved to local storage ({json.Length} bytes)");
            }
            catch (Exception e)
            {
                LogError($"Local save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Loads data from local storage using PlayerPrefs.
        /// </summary>
        private SaveData LoadFromLocal()
        {
            try
            {
                if (PlayerPrefs.HasKey(LOCAL_SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(LOCAL_SAVE_KEY);
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    Log($"Loaded from local storage ({json.Length} bytes)");
                    return data;
                }
                else
                {
                    Log("No local save found");
                    return null;
                }
            }
            catch (Exception e)
            {
                LogError($"Local load failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes local save data.
        /// </summary>
        public void DeleteLocalSave()
        {
            PlayerPrefs.DeleteKey(LOCAL_SAVE_KEY);
            PlayerPrefs.Save();
            Log("Local save deleted");
        }

        #endregion

        #region New Save Data

        /// <summary>
        /// Creates new save data for a new player.
        /// </summary>
        private SaveData CreateNewSaveData()
        {
            Log("Creating new save data...");

            SaveData data = new SaveData
            {
                playerID = GeneratePlayerID(),
                saveVersion = "1.0.0",
                deviceID = SystemInfo.deviceUniqueIdentifier,
                lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                saveCount = 0
            };

            // Initialize profile
            data.profile = new PlayerAccountData
            {
                displayName = "Player",
                avatarID = 0,
                accountCreatedTimestamp = data.lastSaveTimestamp,
                lastLoginTimestamp = data.lastSaveTimestamp,
                lastLoginStreakTimestamp = data.lastSaveTimestamp,
                loginStreak = 1
            };

            // Initialize currency (starting amounts)
            data.currency = new CurrencyData
            {
                softCurrency = 1000, // Starting credits
                hardCurrency = 0,    // Starting gems
                premiumCurrency = 0
            };

            // Initialize progression
            data.progression = new ProgressionData
            {
                level = 1,
                experience = 0,
                hasCompletedOnboarding = false
            };

            // Initialize default settings
            data.settings = new PlayerSettings
            {
                masterVolume = 1.0f,
                musicVolume = 0.7f,
                sfxVolume = 0.8f,
                qualityLevel = 2,
                vsyncEnabled = true,
                targetFramerate = 60,
                showTutorialHints = true,
                autoSaveEnabled = true
            };

            // Initialize default unlockables
            data.unlockables = new UnlockablesData
            {
                activeShip = "default"
            };

            // Initialize leaderboard stats
            data.leaderboardStats = new LeaderboardStatsData
            {
                currentMMR = 1000,
                peakMMR = 1000,
                currentSeasonStartTimestamp = data.lastSaveTimestamp
            };

            Log($"New save data created for player: {data.playerID}");
            return data;
        }

        /// <summary>
        /// Generates a unique player ID.
        /// </summary>
        private string GeneratePlayerID()
        {
            // In production, this would come from authentication service
            return "player_" + Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        #endregion

        #region Auto-Save

        private IEnumerator AutoSaveCoroutine()
        {
            Log($"Auto-save enabled (interval: {autoSaveIntervalSeconds}s)");

            while (true)
            {
                yield return new WaitForSeconds(1f);
                _timeSinceLastAutoSave += 1f;

                if (_timeSinceLastAutoSave >= autoSaveIntervalSeconds)
                {
                    Log("Auto-save triggered");
                    SaveGameAsync();
                }
            }
        }

        #endregion

        #region Application Events

        private void OnApplicationQuit()
        {
            if (saveOnApplicationQuit && !_isSaving)
            {
                Log("Application quitting - saving game...");

                // Synchronous save (blocking)
                SaveData saveData = CollectSaveData();
                if (saveData != null && enableLocalSave)
                {
                    SaveToLocal(saveData);
                }

                // Queue cloud save for next session
                if (enableCloudSave && saveData != null)
                {
                    CloudSaveService.Instance.QueueSave(saveData);
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && saveOnApplicationPause && !_isSaving)
            {
                Log("Application paused - saving game...");
                SaveGameAsync();
            }
        }

        #endregion

        #region Public Utilities

        /// <summary>
        /// Gets the current save data.
        /// </summary>
        public SaveData GetCurrentSaveData()
        {
            return _currentSaveData;
        }

        /// <summary>
        /// Checks if save data exists (player has played before).
        /// </summary>
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(LOCAL_SAVE_KEY) || _currentSaveData != null;
        }

        /// <summary>
        /// Deletes all save data (local and cloud).
        /// WARNING: Irreversible!
        /// </summary>
        public async Task<bool> DeleteAllSaveData()
        {
            LogWarning("Deleting ALL save data...");

            // Delete local
            DeleteLocalSave();

            // Delete cloud
            bool cloudDeleted = false;
            if (enableCloudSave)
            {
                cloudDeleted = await CloudSaveService.Instance.DeleteCloudSave();
            }

            // Reset current save
            _currentSaveData = null;

            Log("All save data deleted");
            return cloudDeleted;
        }

        /// <summary>
        /// Forces an immediate save (bypasses auto-save timer).
        /// </summary>
        public async Task<bool> ForceSave()
        {
            return await SaveGame(forceImmediate: true);
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[SaveManager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SaveManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SaveManager] {message}");
        }

        #endregion
    }
}
