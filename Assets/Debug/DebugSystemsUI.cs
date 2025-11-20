using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GravityWars.Networking;
using GravityWars.CloudSave;
using System.Text;

namespace GravityWars.DebugUI
{
    /// <summary>
    /// Debug UI for testing all Phase 4+ systems.
    ///
    /// Provides buttons and controls to test:
    /// - Cloud Save System
    /// - Quest System
    /// - Achievement System
    /// - Leaderboard System
    /// - Analytics System
    /// - Economy System
    ///
    /// Usage:
    /// 1. Create Canvas with this component
    /// 2. Assign UI references in Inspector
    /// 3. Press ~ key to toggle debug UI
    /// </summary>
    public class DebugSystemsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Transform contentParent;

        [Header("Configuration")]
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // ~ key
        [SerializeField] private bool startHidden = true;

        private StringBuilder _statusLog = new StringBuilder();
        private const int MAX_LOG_LINES = 100;

        private void Start()
        {
            if (startHidden && debugPanel != null)
            {
                debugPanel.SetActive(false);
            }

            CreateDebugButtons();
            LogStatus("Debug UI initialized. Press ~ to toggle.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDebugUI();
            }
        }

        #region UI Management

        private void ToggleDebugUI()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(!debugPanel.activeSelf);

                if (debugPanel.activeSelf)
                {
                    RefreshStatus();
                }
            }
        }

        private void LogStatus(string message)
        {
            string timestampedMessage = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            _statusLog.AppendLine(timestampedMessage);
            UnityEngine.Debug.Log($"[DebugUI] {message}");

            // Limit log size
            string fullLog = _statusLog.ToString();
            string[] lines = fullLog.Split('\n');
            if (lines.Length > MAX_LOG_LINES)
            {
                _statusLog.Clear();
                for (int i = lines.Length - MAX_LOG_LINES; i < lines.Length; i++)
                {
                    _statusLog.AppendLine(lines[i]);
                }
            }

            if (statusText != null)
            {
                statusText.text = _statusLog.ToString();

                // Auto-scroll to bottom
                if (scrollView != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollView.verticalNormalizedPosition = 0f;
                }
            }
        }

        private void RefreshStatus()
        {
            LogStatus("=== SYSTEM STATUS ===");

            // SaveManager status
            if (SaveManager.Instance != null)
            {
                SaveData data = SaveManager.Instance.GetCurrentSaveData();
                if (data != null)
                {
                    LogStatus($"✓ SaveManager: Player {data.playerID}");
                    LogStatus($"  Level: {data.progression.level}, XP: {data.progression.experience}");
                    LogStatus($"  Currency: {data.currency.credits}c / {data.currency.gems}g");
                    LogStatus($"  Active Quests: {data.quests.activeQuests.Count}");
                    LogStatus($"  Achievement Points: {data.achievements.totalAchievementPoints}");
                }
                else
                {
                    LogStatus("✗ SaveManager: No save data loaded");
                }
            }
            else
            {
                LogStatus("✗ SaveManager: Not found");
            }

            // CloudSaveService status
            if (CloudSaveService.Instance != null)
            {
                LogStatus("✓ CloudSaveService: Ready");
            }
            else
            {
                LogStatus("✗ CloudSaveService: Not found");
            }

            // QuestService status
            if (QuestService.Instance != null)
            {
                int activeCount = QuestService.Instance.GetActiveQuests().Count;
                LogStatus($"✓ QuestService: {activeCount} active quests");
            }
            else
            {
                LogStatus("✗ QuestService: Not found");
            }

            // AchievementService status
            if (AchievementService.Instance != null)
            {
                int total = AchievementService.Instance.GetAllAchievements().Count;
                int points = AchievementService.Instance.GetTotalAchievementPoints();
                LogStatus($"✓ AchievementService: {total} achievements, {points} points");
            }
            else
            {
                LogStatus("✗ AchievementService: Not found");
            }

            // LeaderboardService status
            if (LeaderboardService.Instance != null)
            {
                LogStatus("✓ LeaderboardService: Ready");
            }
            else
            {
                LogStatus("✗ LeaderboardService: Not found");
            }

            // AnalyticsService status
            if (AnalyticsService.Instance != null)
            {
                LogStatus("✓ AnalyticsService: Ready");
            }
            else
            {
                LogStatus("✗ AnalyticsService: Not found");
            }

            LogStatus("====================");
        }

        #endregion

        #region Button Creation

        private void CreateDebugButtons()
        {
            if (contentParent == null)
            {
                UnityEngine.Debug.LogError("[DebugUI] Content parent not assigned!");
                return;
            }

            // Clear existing buttons
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            // Save System Buttons
            CreateSection("SAVE SYSTEM");
            CreateButton("Save Game", OnSaveGame);
            CreateButton("Load Game", OnLoadGame);
            CreateButton("Force Save", OnForceSave);
            CreateButton("Delete All Saves", OnDeleteAllSaves);
            CreateButton("Print Save Data", OnPrintSaveData);

            // Quest System Buttons
            CreateSection("QUEST SYSTEM");
            CreateButton("Refresh Quests", OnRefreshQuests);
            CreateButton("Complete Quest +1", OnCompleteQuestProgress);
            CreateButton("Print Active Quests", OnPrintActiveQuests);
            CreateButton("Claim All Completed", OnClaimAllCompleted);

            // Achievement System Buttons
            CreateSection("ACHIEVEMENT SYSTEM");
            CreateButton("Unlock Random Achievement", OnUnlockRandomAchievement);
            CreateButton("Add Progress +10", OnAddAchievementProgress);
            CreateButton("Print All Achievements", OnPrintAchievements);
            CreateButton("Get Achievement Points", OnGetAchievementPoints);

            // Leaderboard System Buttons
            CreateSection("LEADERBOARD SYSTEM");
            CreateButton("Fetch Global Leaderboard", OnFetchGlobalLeaderboard);
            CreateButton("Submit Test Score", OnSubmitTestScore);
            CreateButton("Jump to Player", OnJumpToPlayer);

            // Economy System Buttons
            CreateSection("ECONOMY SYSTEM");
            CreateButton("Add 1000 Credits", () => OnAddCurrency(1000, 0));
            CreateButton("Add 100 Gems", () => OnAddCurrency(0, 100));
            CreateButton("Print Currency", OnPrintCurrency);

            // Analytics System Buttons
            CreateSection("ANALYTICS SYSTEM");
            CreateButton("Track Test Event", OnTrackTestEvent);
            CreateButton("Print Analytics Queue", OnPrintAnalyticsQueue);

            // General Buttons
            CreateSection("GENERAL");
            CreateButton("Refresh Status", RefreshStatus);
            CreateButton("Clear Log", OnClearLog);
        }

        private void CreateSection(string title)
        {
            GameObject sectionObj = new GameObject($"Section_{title}");
            sectionObj.transform.SetParent(contentParent, false);

            TextMeshProUGUI sectionText = sectionObj.AddComponent<TextMeshProUGUI>();
            sectionText.text = $"--- {title} ---";
            sectionText.fontSize = 18;
            sectionText.fontStyle = FontStyles.Bold;
            sectionText.alignment = TextAlignmentOptions.Center;

            RectTransform rt = sectionObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 40);
        }

        private void CreateButton(string label, System.Action onClick)
        {
            GameObject buttonObj = new GameObject($"Button_{label}");
            buttonObj.transform.SetParent(contentParent, false);

            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = label;
            buttonText.fontSize = 14;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            RectTransform buttonRT = buttonObj.GetComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(0, 35);

            button.onClick.AddListener(() => onClick?.Invoke());

            // Add hover effect
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            button.colors = colors;
        }

        #endregion

        #region Save System Handlers

        private async void OnSaveGame()
        {
            LogStatus("Saving game...");
            bool success = await SaveManager.Instance.SaveGame();
            LogStatus(success ? "✓ Save successful" : "✗ Save failed");
        }

        private async void OnLoadGame()
        {
            LogStatus("Loading game...");
            bool success = await SaveManager.Instance.LoadGame();
            LogStatus(success ? "✓ Load successful" : "✗ Load failed");
            RefreshStatus();
        }

        private async void OnForceSave()
        {
            LogStatus("Force saving...");
            bool success = await SaveManager.Instance.ForceSave();
            LogStatus(success ? "✓ Force save successful" : "✗ Force save failed");
        }

        private async void OnDeleteAllSaves()
        {
            LogStatus("Deleting all saves...");
            bool success = await SaveManager.Instance.DeleteAllSaveData();
            LogStatus(success ? "✓ All saves deleted" : "✗ Delete failed");
        }

        private void OnPrintSaveData()
        {
            SaveData data = SaveManager.Instance.GetCurrentSaveData();
            if (data != null)
            {
                string json = JsonUtility.ToJson(data, true);
                LogStatus($"SaveData:\n{json}");
            }
            else
            {
                LogStatus("No save data loaded");
            }
        }

        #endregion

        #region Quest System Handlers

        private void OnRefreshQuests()
        {
            if (QuestService.Instance != null)
            {
                // Trigger manual refresh
                LogStatus("Refreshing quests...");
                // Note: QuestService auto-refreshes, so just print current state
                var quests = QuestService.Instance.GetActiveQuests();
                LogStatus($"✓ {quests.Count} active quests");
            }
            else
            {
                LogStatus("✗ QuestService not found");
            }
        }

        private void OnCompleteQuestProgress()
        {
            if (QuestService.Instance != null)
            {
                var quests = QuestService.Instance.GetActiveQuests();
                if (quests.Count > 0)
                {
                    // Add progress to first quest
                    var quest = quests[0];
                    QuestService.Instance.UpdateQuestProgress(quest.data.objectiveType, 1);
                    LogStatus($"✓ Added progress to quest: {quest.questID}");
                }
                else
                {
                    LogStatus("No active quests");
                }
            }
        }

        private void OnPrintActiveQuests()
        {
            if (QuestService.Instance != null)
            {
                var quests = QuestService.Instance.GetActiveQuests();
                LogStatus($"=== ACTIVE QUESTS ({quests.Count}) ===");
                foreach (var quest in quests)
                {
                    string status = quest.isCompleted ? "COMPLETED" : $"{quest.currentProgress}/{quest.data.targetValue}";
                    LogStatus($"- {quest.questID}: {status}");
                }
            }
        }

        private void OnClaimAllCompleted()
        {
            if (QuestService.Instance != null)
            {
                var quests = QuestService.Instance.GetActiveQuests();
                int claimed = 0;
                foreach (var quest in quests)
                {
                    if (quest.isCompleted && !quest.isClaimed)
                    {
                        QuestService.Instance.ClaimQuest(quest.questID);
                        claimed++;
                    }
                }
                LogStatus($"✓ Claimed {claimed} quests");
            }
        }

        #endregion

        #region Achievement System Handlers

        private void OnUnlockRandomAchievement()
        {
            if (AchievementService.Instance != null)
            {
                var achievements = AchievementService.Instance.GetAllAchievements();
                var locked = achievements.FindAll(a => !a.isUnlocked);
                if (locked.Count > 0)
                {
                    var random = locked[Random.Range(0, locked.Count)];
                    AchievementService.Instance.UnlockAchievement(random.achievementID);
                    LogStatus($"✓ Unlocked: {random.achievementID}");
                }
                else
                {
                    LogStatus("All achievements already unlocked!");
                }
            }
        }

        private void OnAddAchievementProgress()
        {
            if (AchievementService.Instance != null)
            {
                // Add progress to first achievement condition type
                AchievementService.Instance.UpdateAchievementProgress(AchievementConditionType.WinMatches, 10);
                LogStatus("✓ Added +10 progress to WinMatches achievements");
            }
        }

        private void OnPrintAchievements()
        {
            if (AchievementService.Instance != null)
            {
                var achievements = AchievementService.Instance.GetAllAchievements();
                LogStatus($"=== ACHIEVEMENTS ({achievements.Count}) ===");
                foreach (var ach in achievements)
                {
                    string status = ach.isUnlocked ? "UNLOCKED" : $"{ach.currentProgress}/{ach.data.targetValue}";
                    LogStatus($"- {ach.achievementID}: {status} ({ach.data.points}pts)");
                }
            }
        }

        private void OnGetAchievementPoints()
        {
            if (AchievementService.Instance != null)
            {
                int points = AchievementService.Instance.GetTotalAchievementPoints();
                float completion = AchievementService.Instance.GetCompletionPercentage();
                LogStatus($"Achievement Points: {points}");
                LogStatus($"Completion: {completion:F1}%");
            }
        }

        #endregion

        #region Leaderboard System Handlers

        private async void OnFetchGlobalLeaderboard()
        {
            if (LeaderboardService.Instance != null)
            {
                LogStatus("Fetching global leaderboard...");
                var data = await LeaderboardService.Instance.FetchLeaderboard("total_wins", 1, 10);
                if (data != null)
                {
                    LogStatus($"✓ Fetched {data.entries.Count} entries");
                    foreach (var entry in data.entries)
                    {
                        LogStatus($"  #{entry.rank}: {entry.playerName} - {entry.score}");
                    }
                }
            }
        }

        private async void OnSubmitTestScore()
        {
            if (LeaderboardService.Instance != null)
            {
                LogStatus("Submitting test score...");
                bool success = await LeaderboardService.Instance.SubmitScore(LeaderboardStatType.TotalWins, Random.Range(10, 100));
                LogStatus(success ? "✓ Score submitted" : "✗ Submit failed");
            }
        }

        private async void OnJumpToPlayer()
        {
            if (LeaderboardService.Instance != null)
            {
                LogStatus("Jumping to player position...");
                var data = await LeaderboardService.Instance.FetchLeaderboardAroundPlayer("total_wins", 5);
                if (data != null)
                {
                    LogStatus($"✓ Found {data.entries.Count} entries around player");
                }
            }
        }

        #endregion

        #region Economy System Handlers

        private void OnAddCurrency(int soft, int hard)
        {
            SaveData data = SaveManager.Instance.GetCurrentSaveData();
            if (data != null)
            {
                data.currency.credits += soft;
                data.currency.gems += hard;
                LogStatus($"✓ Added {soft}c / {hard}g");
                LogStatus($"  Total: {data.currency.credits}c / {data.currency.gems}g");
            }
        }

        private void OnPrintCurrency()
        {
            SaveData data = SaveManager.Instance.GetCurrentSaveData();
            if (data != null)
            {
                LogStatus($"Currency: {data.currency.credits}c / {data.currency.gems}g");
                LogStatus($"Lifetime Earned: {data.currency.lifetimeSoftCurrencyEarned}c / {data.currency.lifetimeHardCurrencyEarned}g");
                LogStatus($"Lifetime Spent: {data.currency.lifetimeSoftCurrencySpent}c / {data.currency.lifetimeHardCurrencySpent}g");
            }
        }

        #endregion

        #region Analytics System Handlers

        private void OnTrackTestEvent()
        {
            if (AnalyticsService.Instance != null)
            {
                AnalyticsService.Instance.TrackMatchCompleted(
                    isWin: true,
                    duration: 120,
                    damageDealt: 500,
                    missilesHit: 10,
                    missilesFired: 15
                );
                LogStatus("✓ Tracked test match event");
            }
        }

        private void OnPrintAnalyticsQueue()
        {
            SaveData data = SaveManager.Instance.GetCurrentSaveData();
            if (data != null && data.analyticsQueue != null)
            {
                LogStatus($"Analytics Queue: {data.analyticsQueue.queuedEvents.Count} events");
                LogStatus($"Last Upload: {System.DateTimeOffset.FromUnixTimeSeconds(data.analyticsQueue.lastUploadTimestamp)}");
            }
        }

        #endregion

        #region General Handlers

        private void OnClearLog()
        {
            _statusLog.Clear();
            if (statusText != null)
            {
                statusText.text = "";
            }
            LogStatus("Log cleared");
        }

        #endregion
    }
}
