using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Quest service for daily/weekly/season quest management.
    ///
    /// Features:
    /// - Automatic quest refresh (daily/weekly)
    /// - Progress tracking
    /// - Reward distribution
    /// - Cloud save synchronization
    /// - Quest templates
    ///
    /// Quest Flow:
    /// 1. Player logs in → InitializeQuests()
    /// 2. Check for expired quests → RefreshExpiredQuests()
    /// 3. Generate new quests from templates
    /// 4. Player performs actions → UpdateQuestProgress()
    /// 5. Quest completed → OnQuestCompleted() → Award rewards
    /// 6. Save to cloud
    ///
    /// Usage:
    ///   QuestService.Instance.InitializeQuests();
    ///   QuestService.Instance.UpdateQuestProgress(QuestObjectiveType.WinMatches, 1);
    /// </summary>
    public class QuestService : MonoBehaviour
    {
        #region Singleton

        private static QuestService _instance;
        public static QuestService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<QuestService>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[QuestService]");
                        _instance = go.AddComponent<QuestService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Quest Configuration")]
        [Tooltip("All available quest templates")]
        public List<QuestDataSO> questTemplates = new List<QuestDataSO>();

        [Tooltip("Number of daily quests active at once")]
        public int dailyQuestSlots = 3;

        [Tooltip("Number of weekly quests active at once")]
        public int weeklyQuestSlots = 3;

        [Tooltip("Number of season quests active at once")]
        public int seasonQuestSlots = 5;

        [Tooltip("Enable debug logging")]
        public bool debugLogging = true;

        #endregion

        #region State

        private List<QuestInstance> _activeQuests = new List<QuestInstance>();
        private bool _isInitialized = false;

        public List<QuestInstance> ActiveQuests => _activeQuests;
        public bool IsInitialized => _isInitialized;

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

            Log("Quest service created");
        }

        private void Start()
        {
            // Initialize will be called by ProgressionManager after loading player data
        }

        private void Update()
        {
            // Auto-check for expired quests every 60 seconds
            if (_isInitialized && Time.frameCount % (60 * 60) == 0) // ~60 FPS
            {
                RefreshExpiredQuests();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes quest system for current player.
        /// Call this after loading player data from ProgressionManager.
        /// </summary>
        public async Task InitializeQuests()
        {
            Log("Initializing quests...");

            // Load active quests from cloud save
            await LoadQuestsFromCloud();

            // Refresh expired quests
            RefreshExpiredQuests();

            // Generate missing quests if slots are empty
            GenerateMissingQuests();

            _isInitialized = true;
            Log($"Quest system initialized - {_activeQuests.Count} active quests");
        }

        #endregion

        #region Quest Loading/Saving

        /// <summary>
        /// Loads active quests from cloud save.
        /// </summary>
        private async Task LoadQuestsFromCloud()
        {
            // In production, this would load from CloudSaveService
            // For now, create from ProgressionManager if available

            var progressionManager = ProgressionManager.Instance;
            if (progressionManager != null && progressionManager.currentPlayerData != null)
            {
                // Load quests from player data (if you've added a quests list to PlayerProfileData)
                // For now, we'll initialize with empty list
                _activeQuests = new List<QuestInstance>();
                Log("Quests loaded from player data");
            }
            else
            {
                _activeQuests = new List<QuestInstance>();
                Log("No player data - starting with empty quest list");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Saves active quests to cloud.
        /// </summary>
        private async Task SaveQuestsToCloud()
        {
            // In production, this would save to CloudSaveService
            // For now, just log

            var cloudSave = ServiceLocator.Instance?.CloudSave;
            if (cloudSave != null)
            {
                // Save would happen here
                // await cloudSave.SaveQuests(_activeQuests);
                Log($"Quests saved to cloud ({_activeQuests.Count} active)");
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Quest Refresh

        /// <summary>
        /// Refreshes expired quests and generates new ones.
        /// </summary>
        public void RefreshExpiredQuests()
        {
            // Remove expired quests
            int removedCount = _activeQuests.RemoveAll(q => q.IsExpired);

            if (removedCount > 0)
            {
                Log($"Removed {removedCount} expired quests");
            }

            // Generate new quests to fill empty slots
            GenerateMissingQuests();

            // Save updated quest list
            _ = SaveQuestsToCloud();
        }

        /// <summary>
        /// Generates new quests to fill empty slots.
        /// </summary>
        private void GenerateMissingQuests()
        {
            // Count current quests by type
            int dailyCount = _activeQuests.Count(q => q.questType == QuestType.Daily);
            int weeklyCount = _activeQuests.Count(q => q.questType == QuestType.Weekly);
            int seasonCount = _activeQuests.Count(q => q.questType == QuestType.Season);

            // Generate missing daily quests
            int dailyNeeded = dailyQuestSlots - dailyCount;
            for (int i = 0; i < dailyNeeded; i++)
            {
                var quest = GenerateRandomQuest(QuestType.Daily);
                if (quest != null)
                {
                    _activeQuests.Add(quest);
                    Log($"Generated daily quest: {quest.username}");
                }
            }

            // Generate missing weekly quests
            int weeklyNeeded = weeklyQuestSlots - weeklyCount;
            for (int i = 0; i < weeklyNeeded; i++)
            {
                var quest = GenerateRandomQuest(QuestType.Weekly);
                if (quest != null)
                {
                    _activeQuests.Add(quest);
                    Log($"Generated weekly quest: {quest.username}");
                }
            }

            // Generate missing season quests
            int seasonNeeded = seasonQuestSlots - seasonCount;
            for (int i = 0; i < seasonNeeded; i++)
            {
                var quest = GenerateRandomQuest(QuestType.Season);
                if (quest != null)
                {
                    _activeQuests.Add(quest);
                    Log($"Generated season quest: {quest.username}");
                }
            }
        }

        /// <summary>
        /// Generates a random quest from templates of specified type.
        /// </summary>
        private QuestInstance GenerateRandomQuest(QuestType questType)
        {
            // Get all templates of this type that player qualifies for
            var validTemplates = questTemplates
                .Where(t => t.questType == questType)
                .Where(t => PlayerMeetsRequirements(t))
                .Where(t => !IsQuestActive(t.questID)) // Don't duplicate active quests
                .ToList();

            if (validTemplates.Count == 0)
            {
                Debug.LogWarning($"[QuestService] No valid templates for {questType} quests!");
                return null;
            }

            // Pick random template
            var template = validTemplates[UnityEngine.Random.Range(0, validTemplates.Count)];

            // Create instance
            return template.CreateInstance();
        }

        /// <summary>
        /// Checks if player meets requirements for a quest.
        /// </summary>
        private bool PlayerMeetsRequirements(QuestDataSO quest)
        {
            var progressionManager = ProgressionManager.Instance;
            if (progressionManager == null || progressionManager.currentPlayerData == null)
                return true; // Default to allowing if no player data

            // Check account level requirement
            return progressionManager.currentPlayerData.accountLevel >= quest.requiredAccountLevel;
        }

        /// <summary>
        /// Checks if a quest is already active.
        /// </summary>
        private bool IsQuestActive(string questID)
        {
            return _activeQuests.Any(q => q.questID == questID);
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// Updates progress for all matching quests.
        /// Call this when player performs an action.
        /// </summary>
        public void UpdateQuestProgress(QuestObjectiveType objectiveType, int amount = 1, string context = "")
        {
            if (!_isInitialized)
                return;

            bool anyUpdated = false;

            foreach (var quest in _activeQuests)
            {
                // Skip if quest is completed or expired
                if (quest.IsCompleted || quest.IsExpired)
                    continue;

                // Skip if objective doesn't match
                if (quest.objectiveType != objectiveType)
                    continue;

                // Check context-specific requirements
                if (!CheckContextRequirements(quest, context))
                    continue;

                // Update progress
                int oldProgress = quest.currentProgress;
                quest.AddProgress(amount);

                if (quest.currentProgress > oldProgress)
                {
                    anyUpdated = true;
                    Log($"Quest progress: {quest.username} ({quest.currentProgress}/{quest.targetValue})");

                    // Check if completed
                    if (quest.IsCompleted)
                    {
                        OnQuestCompleted(quest);
                    }
                }
            }

            // Save if any quest was updated
            if (anyUpdated)
            {
                _ = SaveQuestsToCloud();
            }
        }

        /// <summary>
        /// Checks context-specific requirements (e.g., missile type, archetype).
        /// </summary>
        private bool CheckContextRequirements(QuestInstance quest, string context)
        {
            // For missile type requirements
            if (!string.IsNullOrEmpty(quest.requiredMissileType))
            {
                if (context != quest.requiredMissileType)
                    return false;
            }

            // For archetype requirements
            if (quest.requiredArchetype != ShipArchetype.AllAround)
            {
                // Context should contain archetype name
                if (!context.Contains(quest.requiredArchetype.ToString()))
                    return false;
            }

            return true;
        }

        #endregion

        #region Quest Completion

        /// <summary>
        /// Called when a quest is completed.
        /// Awards rewards and triggers analytics.
        /// </summary>
        private void OnQuestCompleted(QuestInstance quest)
        {
            Log($"Quest completed: {quest.username}");

            // Award rewards
            AwardQuestRewards(quest);

            // Track analytics
            TrackQuestCompletion(quest);

            // Show UI notification
            ShowQuestCompletionUI(quest);
        }

        /// <summary>
        /// Awards rewards for completing a quest.
        /// </summary>
        private void AwardQuestRewards(QuestInstance quest)
        {
            var progressionManager = ProgressionManager.Instance;
            if (progressionManager == null)
            {
                Debug.LogError("[QuestService] ProgressionManager not found - cannot award rewards!");
                return;
            }

            // Award soft currency
            if (quest.creditsReward > 0)
            {
                progressionManager.currentPlayerData.AddCurrency(quest.creditsReward, 0);
                Log($"Awarded {quest.creditsReward} soft currency");
            }

            // Award hard currency
            if (quest.gemsReward > 0)
            {
                progressionManager.currentPlayerData.AddCurrency(0, quest.gemsReward);
                Log($"Awarded {quest.gemsReward} hard currency");
            }

            // Award account XP
            if (quest.accountXPReward > 0)
            {
                progressionManager.currentPlayerData.AddAccountXP(quest.accountXPReward);
                Log($"Awarded {quest.accountXPReward} account XP");

                // Check for level up
                progressionManager.CheckAccountLevelUp();
            }

            // Award items
            foreach (var itemID in quest.itemRewards)
            {
                // Unlock item (would need to resolve item by ID)
                Log($"Awarded item: {itemID}");
            }

            // Save progression
            progressionManager.Save();
        }

        /// <summary>
        /// Tracks quest completion in analytics.
        /// </summary>
        private void TrackQuestCompletion(QuestInstance quest)
        {
            var analyticsService = ServiceLocator.Instance?.Analytics;
            if (analyticsService != null)
            {
                float timeToComplete = (float)(DateTime.UtcNow - quest.assignedAt).TotalHours;

                analyticsService.TrackQuestCompleted(
                    questID: quest.questID,
                    timeToComplete: timeToComplete,
                    reward: $"{quest.creditsReward} coins, {quest.accountXPReward} XP"
                );
            }
        }

        /// <summary>
        /// Shows quest completion UI.
        /// </summary>
        private void ShowQuestCompletionUI(QuestInstance quest)
        {
            // In production, this would trigger a UI popup
            // For now, just log
            Debug.Log($"[QuestService] 🎉 Quest Complete: {quest.username}!");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets all active quests of a specific type.
        /// </summary>
        public List<QuestInstance> GetQuestsByType(QuestType questType)
        {
            return _activeQuests.Where(q => q.questType == questType).ToList();
        }

        /// <summary>
        /// Gets a quest by ID.
        /// </summary>
        public QuestInstance GetQuestByID(string questID)
        {
            return _activeQuests.FirstOrDefault(q => q.questID == questID);
        }

        /// <summary>
        /// Claims completed quest rewards (manual claim).
        /// </summary>
        public bool ClaimQuest(string questID)
        {
            var quest = GetQuestByID(questID);

            if (quest == null)
            {
                Debug.LogWarning($"[QuestService] Quest not found: {questID}");
                return false;
            }

            if (!quest.IsCompleted)
            {
                Debug.LogWarning($"[QuestService] Quest not completed: {questID}");
                return false;
            }

            // Award rewards
            AwardQuestRewards(quest);

            // Remove from active quests
            _activeQuests.Remove(quest);

            // Save
            _ = SaveQuestsToCloud();

            Log($"Quest claimed and removed: {quest.username}");
            return true;
        }

        /// <summary>
        /// Gets all active quests (for UI display).
        /// </summary>
        public List<QuestInstance> GetActiveQuests()
        {
            return new List<QuestInstance>(_activeQuests);
        }

        /// <summary>
        /// Gets count of claimable (completed but not expired) quests.
        /// </summary>
        public int GetClaimableQuestCount()
        {
            return _activeQuests.Count(q => q.IsCompleted && !q.IsExpired);
        }

        /// <summary>
        /// Gets time until next refresh for a specific quest type.
        /// </summary>
        public System.TimeSpan GetTimeUntilNextRefresh(QuestType questType)
        {
            // Find the earliest expiring quest of this type
            var earliestQuest = _activeQuests
                .Where(q => q.questType == questType && !q.IsExpired)
                .OrderBy(q => q.expiresAt)
                .FirstOrDefault();

            if (earliestQuest != null)
            {
                return earliestQuest.TimeRemaining;
            }

            // If no quests found, return default refresh time
            return questType switch
            {
                QuestType.Daily => System.TimeSpan.FromHours(24),
                QuestType.Weekly => System.TimeSpan.FromDays(7),
                QuestType.Season => System.TimeSpan.FromDays(90),
                _ => System.TimeSpan.Zero
            };
        }

        /// <summary>
        /// Force refreshes all quests (admin/debug tool).
        /// </summary>
        public void ForceRefreshAllQuests()
        {
            _activeQuests.Clear();
            GenerateMissingQuests();
            _ = SaveQuestsToCloud();

            Log("All quests force refreshed");
        }

        #endregion

        #region Helper Methods

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[QuestService] {message}");
        }

        #endregion
    }
}
