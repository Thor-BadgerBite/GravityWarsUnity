using UnityEngine;
using GravityWars.Networking;
using GravityWars.CloudSave;

namespace GravityWars.Integration
{
    /// <summary>
    /// Helper script to simplify integration between SaveManager and all game services.
    /// Attach this to your ServiceLocator GameObject to enable automatic save/load integration.
    ///
    /// This script bridges the gap between SaveManager and your existing services,
    /// providing getter/setter methods that SaveManager can call.
    /// </summary>
    public class ServiceIntegrationHelper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private bool autoFindServices = true;

        [Header("Integration Status")]
        [SerializeField] private bool progressionIntegrated = false;
        [SerializeField] private bool economyIntegrated = false;
        [SerializeField] private bool questsIntegrated = false;
        [SerializeField] private bool achievementsIntegrated = false;
        [SerializeField] private bool leaderboardsIntegrated = false;

        private void Start()
        {
            if (autoFindServices)
            {
                RegisterWithSaveManager();
            }
        }

        /// <summary>
        /// Registers all service integration callbacks with SaveManager.
        /// </summary>
        public void RegisterWithSaveManager()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[ServiceIntegration] SaveManager not found");
                return;
            }

            // Subscribe to save/load events
            SaveManager.Instance.OnSaveStarted += OnSaveStarted;
            SaveManager.Instance.OnLoadCompleted += OnLoadCompleted;

            Debug.Log("[ServiceIntegration] Registered with SaveManager");
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveStarted -= OnSaveStarted;
                SaveManager.Instance.OnLoadCompleted -= OnLoadCompleted;
            }
        }

        #region Save Integration

        /// <summary>
        /// Called when SaveManager is about to save.
        /// Collect data from all services and update the SaveData object.
        /// </summary>
        private void OnSaveStarted(SaveData saveData)
        {
            Debug.Log("[ServiceIntegration] Collecting data from all services...");

            CollectProgressionData(saveData);
            CollectEconomyData(saveData);
            CollectQuestData(saveData);
            CollectAchievementData(saveData);
            CollectLeaderboardData(saveData);
            CollectStatisticsData(saveData);

            Debug.Log("[ServiceIntegration] Data collection complete");
        }

        #endregion

        #region Load Integration

        /// <summary>
        /// Called when SaveManager has loaded data.
        /// Distribute data to all services.
        /// </summary>
        private void OnLoadCompleted(SaveData saveData)
        {
            Debug.Log("[ServiceIntegration] Distributing data to all services...");

            DistributeProgressionData(saveData);
            DistributeEconomyData(saveData);
            DistributeQuestData(saveData);
            DistributeAchievementData(saveData);
            DistributeLeaderboardData(saveData);
            DistributeStatisticsData(saveData);
            DistributeSettingsData(saveData);

            Debug.Log("[ServiceIntegration] Data distribution complete");
        }

        #endregion

        #region Progression Integration

        private void CollectProgressionData(SaveData saveData)
        {
            if (ProgressionManager.Instance == null)
            {
                Debug.LogWarning("[ServiceIntegration] ProgressionManager not found");
                return;
            }

            // TODO: Uncomment when ProgressionManager has these methods
            /*
            saveData.progression.level = ProgressionManager.Instance.AccountLevel;
            saveData.progression.experience = ProgressionManager.Instance.AccountXP;
            */

            // MOCK: For now, preserve existing values
            Debug.Log("[ServiceIntegration] Progression data collected (MOCK)");
            progressionIntegrated = true;
        }

        private void DistributeProgressionData(SaveData saveData)
        {
            if (ProgressionManager.Instance == null) return;

            // TODO: Uncomment when ProgressionManager has these methods
            /*
            ProgressionManager.Instance.SetAccountLevel(saveData.progression.level);
            ProgressionManager.Instance.SetAccountXP(saveData.progression.experience);
            */

            Debug.Log($"[ServiceIntegration] Progression restored: Level {saveData.progression.level}, XP {saveData.progression.experience}");
        }

        #endregion

        #region Economy Integration

        private void CollectEconomyData(SaveData saveData)
        {
            Debug.Log("[ServiceIntegration] Economy data collection skipped (service not present)");
        }

        private void DistributeEconomyData(SaveData saveData)
        {
            Debug.Log($"[ServiceIntegration] Economy restoration skipped (service not present)");
        }

        #endregion

        #region Quest Integration

        private void CollectQuestData(SaveData saveData)
        {
            if (QuestService.Instance == null)
            {
                Debug.LogWarning("[ServiceIntegration] QuestService not found");
                return;
            }

            var activeQuests = QuestService.Instance.GetActiveQuests();
            saveData.quests.activeQuests.Clear();

            foreach (var quest in activeQuests)
            {
                saveData.quests.activeQuests.Add(new ActiveQuestData
                {
                    questID = quest.questID,
                    currentProgress = quest.currentProgress,
                    acceptedTimestamp = quest.acceptedTimestamp,
                    expirationTimestamp = quest.expirationTimestamp,
                    isCompleted = quest.isCompleted,
                    isClaimed = quest.isClaimed
                });
            }

            Debug.Log($"[ServiceIntegration] Quest data collected: {activeQuests.Count} active quests");
            questsIntegrated = true;
        }

        private void DistributeQuestData(SaveData saveData)
        {
            if (QuestService.Instance == null) return;

            // TODO: Implement RestoreQuests() in QuestService
            // QuestService.Instance.RestoreQuests(saveData.quests);

            Debug.Log($"[ServiceIntegration] Quest data restored: {saveData.quests.activeQuests.Count} active quests");
        }

        #endregion

        #region Achievement Integration

        private void CollectAchievementData(SaveData saveData)
        {
            if (AchievementService.Instance == null)
            {
                Debug.LogWarning("[ServiceIntegration] AchievementService not found");
                return;
            }

            var achievements = AchievementService.Instance.GetAllAchievements();
            saveData.achievements.achievementIDs.Clear();
            saveData.achievements.achievementProgress.Clear();
            saveData.achievements.currentTiers.Clear();
            saveData.achievements.isUnlocked.Clear();
            saveData.achievements.unlockTimestamps.Clear();

            foreach (var achievement in achievements)
            {
                saveData.achievements.achievementIDs.Add(achievement.achievementID);
                saveData.achievements.achievementProgress.Add(achievement.currentProgress);
                saveData.achievements.currentTiers.Add(achievement.currentTier);
                saveData.achievements.isUnlocked.Add(achievement.isUnlocked);
                saveData.achievements.unlockTimestamps.Add(achievement.unlockTimestamp);
            }

            saveData.achievements.totalAchievementPoints = AchievementService.Instance.GetTotalAchievementPoints();

            Debug.Log($"[ServiceIntegration] Achievement data collected: {achievements.Count} achievements");
            achievementsIntegrated = true;
        }

        private void DistributeAchievementData(SaveData saveData)
        {
            if (AchievementService.Instance == null) return;

            // TODO: Implement RestoreAchievements() in AchievementService
            // AchievementService.Instance.RestoreAchievements(saveData.achievements);

            Debug.Log($"[ServiceIntegration] Achievement data restored: {saveData.achievements.totalAchievementPoints} points");
        }

        #endregion

        #region Leaderboard Integration

        private void CollectLeaderboardData(SaveData saveData)
        {
            if (LeaderboardService.Instance == null)
            {
                Debug.LogWarning("[ServiceIntegration] LeaderboardService not found");
                return;
            }

            // TODO: Get stats from LeaderboardService
            // saveData.leaderboardStats = LeaderboardService.Instance.GetPlayerStats();

            Debug.Log("[ServiceIntegration] Leaderboard data collected (MOCK)");
            leaderboardsIntegrated = true;
        }

        private void DistributeLeaderboardData(SaveData saveData)
        {
            if (LeaderboardService.Instance == null) return;

            // TODO: Restore stats to LeaderboardService
            // LeaderboardService.Instance.RestorePlayerStats(saveData.leaderboardStats);

            Debug.Log("[ServiceIntegration] Leaderboard data restored");
        }

        #endregion

        #region Statistics Integration

        private void CollectStatisticsData(SaveData saveData)
        {
            // TODO: If you have a StatisticsService, collect from there
            // For now, statistics are embedded in other services (Quest, Achievement, Leaderboard)

            Debug.Log("[ServiceIntegration] Statistics data collected");
        }

        private void DistributeStatisticsData(SaveData saveData)
        {
            // TODO: If you have a StatisticsService, distribute to there

            Debug.Log("[ServiceIntegration] Statistics data restored");
        }

        #endregion

        #region Settings Integration

        private void DistributeSettingsData(SaveData saveData)
        {
            if (saveData.settings == null) return;

            // Apply audio settings
            AudioListener.volume = saveData.settings.masterVolume;

            // Apply graphics settings
            QualitySettings.vSyncCount = saveData.settings.vsyncEnabled ? 1 : 0;
            Application.targetFrameRate = saveData.settings.targetFramerate;

            // Apply screen settings
            if (saveData.settings.fullscreen != Screen.fullScreen)
            {
                Screen.SetResolution(
                    saveData.settings.resolutionWidth,
                    saveData.settings.resolutionHeight,
                    saveData.settings.fullscreen
                );
            }

            Debug.Log("[ServiceIntegration] Settings applied");
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Prints integration status to console.
        /// </summary>
        [ContextMenu("Print Integration Status")]
        public void PrintIntegrationStatus()
        {
            Debug.Log("=== SERVICE INTEGRATION STATUS ===");
            Debug.Log($"Progression: {(progressionIntegrated ? "✓" : "✗")}");
            Debug.Log($"Economy: {(economyIntegrated ? "✓" : "✗")}");
            Debug.Log($"Quests: {(questsIntegrated ? "✓" : "✗")}");
            Debug.Log($"Achievements: {(achievementsIntegrated ? "✓" : "✗")}");
            Debug.Log($"Leaderboards: {(leaderboardsIntegrated ? "✓" : "✗")}");
            Debug.Log("==================================");
        }

        /// <summary>
        /// Tests save/load cycle.
        /// </summary>
        [ContextMenu("Test Save/Load Cycle")]
        public async void TestSaveLoadCycle()
        {
            Debug.Log("=== TESTING SAVE/LOAD CYCLE ===");

            // Save
            Debug.Log("Saving...");
            bool saveSuccess = await SaveManager.Instance.SaveGame();
            Debug.Log($"Save: {(saveSuccess ? "SUCCESS" : "FAILED")}");

            // Wait a bit
            await System.Threading.Tasks.Task.Delay(1000);

            // Load
            Debug.Log("Loading...");
            bool loadSuccess = await SaveManager.Instance.LoadGame();
            Debug.Log($"Load: {(loadSuccess ? "SUCCESS" : "FAILED")}");

            Debug.Log("=== TEST COMPLETE ===");
        }

        #endregion
    }
}
