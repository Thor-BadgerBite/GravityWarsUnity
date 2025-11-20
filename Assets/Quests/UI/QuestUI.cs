using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GravityWars.Networking;

/// <summary>
/// UI component for displaying quests and progress.
///
/// Features:
/// - Tabbed interface (Daily/Weekly/Season)
/// - Quest cards with progress bars
/// - Claim buttons for completed quests
/// - Expiration timers
/// - Notification badges
/// - Slide-in/out panel animation
///
/// IMPORTANT: This component requires Unity UI Canvas.
///
/// Setup:
///   1. Add to Canvas GameObject
///   2. Assign UI references in Inspector
///   3. Call ShowQuestPanel() to display
///
/// Usage:
///   QuestUI.Instance.ShowQuestPanel();
///   QuestUI.Instance.RefreshQuestDisplay();
/// </summary>
public class QuestUI : MonoBehaviour
{
    #region Singleton

    private static QuestUI _instance;
    public static QuestUI Instance => _instance;

    #endregion

    #region UI References

    [Header("Panel References")]
    [Tooltip("Main quest panel (for slide animation)")]
    public RectTransform questPanel;

    [Tooltip("Panel background overlay")]
    public Image backgroundOverlay;

    [Header("Tab Buttons")]
    [Tooltip("Daily quests tab button")]
    public Button dailyTabButton;

    [Tooltip("Weekly quests tab button")]
    public Button weeklyTabButton;

    [Tooltip("Season quests tab button")]
    public Button seasonTabButton;

    [Header("Quest Display")]
    [Tooltip("Container for quest cards")]
    public Transform questContainer;

    [Tooltip("Prefab for quest card UI")]
    public GameObject questCardPrefab;

    [Tooltip("Text showing time until next refresh")]
    public TextMeshProUGUI nextRefreshText;

    [Header("Notification Badge")]
    [Tooltip("Notification badge (shows count of claimable quests)")]
    public GameObject notificationBadge;

    [Tooltip("Badge count text")]
    public TextMeshProUGUI badgeCountText;

    [Header("Panel Toggle Button")]
    [Tooltip("Button to show/hide quest panel")]
    public Button togglePanelButton;

    [Tooltip("Button icon (rotates when panel is open)")]
    public RectTransform toggleButtonIcon;

    #endregion

    #region Configuration

    [Header("Animation Settings")]
    [Tooltip("Panel slide animation duration")]
    public float panelSlideDuration = 0.3f;

    [Tooltip("Panel position when hidden (off-screen)")]
    public Vector2 hiddenPosition = new Vector2(400f, 0f);

    [Tooltip("Panel position when shown")]
    public Vector2 shownPosition = new Vector2(0f, 0f);

    [Header("Tab Colors")]
    [Tooltip("Active tab color")]
    public Color activeTabColor = new Color(1f, 0.8f, 0.2f);

    [Tooltip("Inactive tab color")]
    public Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f);

    #endregion

    #region State

    private QuestService _questService;
    private QuestType _currentTab = QuestType.Daily;
    private bool _isPanelVisible = false;
    private List<QuestCardUI> _activeQuestCards = new List<QuestCardUI>();

    // Animation
    private Vector2 _targetPosition;
    private float _animationProgress = 0f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Validate references
        if (questPanel == null)
        {
            Debug.LogError("[QuestUI] Quest panel not assigned!");
            enabled = false;
            return;
        }

        // Start with panel hidden
        questPanel.anchoredPosition = hiddenPosition;
        _isPanelVisible = false;

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Get quest service
        _questService = QuestService.Instance;

        if (_questService == null)
        {
            Debug.LogWarning("[QuestUI] QuestService not available - UI disabled");
            enabled = false;
            return;
        }

        // Setup tab buttons
        if (dailyTabButton != null)
            dailyTabButton.onClick.AddListener(() => SwitchTab(QuestType.Daily));

        if (weeklyTabButton != null)
            weeklyTabButton.onClick.AddListener(() => SwitchTab(QuestType.Weekly));

        if (seasonTabButton != null)
            seasonTabButton.onClick.AddListener(() => SwitchTab(QuestType.Season));

        // Setup toggle button
        if (togglePanelButton != null)
            togglePanelButton.onClick.AddListener(ToggleQuestPanel);

        // Default to daily tab
        SwitchTab(QuestType.Daily);

        // Update notification badge
        UpdateNotificationBadge();

        Log("Quest UI initialized");
    }

    private void Update()
    {
        // Animate panel slide
        if (_animationProgress < 1f)
        {
            _animationProgress += Time.deltaTime / panelSlideDuration;
            questPanel.anchoredPosition = Vector2.Lerp(
                _isPanelVisible ? hiddenPosition : shownPosition,
                _targetPosition,
                Mathf.SmoothStep(0f, 1f, _animationProgress)
            );
        }

        // Update next refresh timer (if panel is visible)
        if (_isPanelVisible && nextRefreshText != null)
        {
            UpdateNextRefreshText();
        }

        // Update quest progress bars
        foreach (var card in _activeQuestCards)
        {
            card.UpdateProgressBar();
        }
    }

    #endregion

    #region Panel Control

    /// <summary>
    /// Toggles quest panel visibility.
    /// </summary>
    public void ToggleQuestPanel()
    {
        if (_isPanelVisible)
        {
            HideQuestPanel();
        }
        else
        {
            ShowQuestPanel();
        }
    }

    /// <summary>
    /// Shows the quest panel with slide animation.
    /// </summary>
    public void ShowQuestPanel()
    {
        if (_isPanelVisible)
            return;

        _isPanelVisible = true;
        _targetPosition = shownPosition;
        _animationProgress = 0f;

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
        }

        // Rotate toggle button icon
        if (toggleButtonIcon != null)
        {
            toggleButtonIcon.localRotation = Quaternion.Euler(0f, 0f, 180f);
        }

        RefreshQuestDisplay();
        Log("Quest panel shown");
    }

    /// <summary>
    /// Hides the quest panel with slide animation.
    /// </summary>
    public void HideQuestPanel()
    {
        if (!_isPanelVisible)
            return;

        _isPanelVisible = false;
        _targetPosition = hiddenPosition;
        _animationProgress = 0f;

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        // Rotate toggle button icon back
        if (toggleButtonIcon != null)
        {
            toggleButtonIcon.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        Log("Quest panel hidden");
    }

    #endregion

    #region Tab Switching

    /// <summary>
    /// Switches to a different quest tab.
    /// </summary>
    public void SwitchTab(QuestType questType)
    {
        _currentTab = questType;

        // Update tab button colors
        UpdateTabButtonColors();

        // Refresh quest display
        RefreshQuestDisplay();

        Log($"Switched to {questType} tab");
    }

    /// <summary>
    /// Updates tab button colors based on active tab.
    /// </summary>
    private void UpdateTabButtonColors()
    {
        if (dailyTabButton != null)
        {
            var colors = dailyTabButton.colors;
            colors.normalColor = _currentTab == QuestType.Daily ? activeTabColor : inactiveTabColor;
            dailyTabButton.colors = colors;
        }

        if (weeklyTabButton != null)
        {
            var colors = weeklyTabButton.colors;
            colors.normalColor = _currentTab == QuestType.Weekly ? activeTabColor : inactiveTabColor;
            weeklyTabButton.colors = colors;
        }

        if (seasonTabButton != null)
        {
            var colors = seasonTabButton.colors;
            colors.normalColor = _currentTab == QuestType.Season ? activeTabColor : inactiveTabColor;
            seasonTabButton.colors = colors;
        }
    }

    #endregion

    #region Quest Display

    /// <summary>
    /// Refreshes the quest display for current tab.
    /// </summary>
    public void RefreshQuestDisplay()
    {
        if (_questService == null)
            return;

        // Clear existing quest cards
        ClearQuestCards();

        // Get quests for current tab
        var quests = _questService.GetActiveQuests()
            .Where(q => q.questType == _currentTab)
            .OrderBy(q => q.IsCompleted ? 1 : 0) // Show incomplete first
            .ThenBy(q => q.TimeRemaining.TotalSeconds)
            .ToList();

        // Create quest cards
        foreach (var quest in quests)
        {
            CreateQuestCard(quest);
        }

        // Update next refresh text
        UpdateNextRefreshText();

        // Update notification badge
        UpdateNotificationBadge();

        Log($"Refreshed quest display - {quests.Count} quests shown");
    }

    /// <summary>
    /// Creates a quest card UI element.
    /// </summary>
    private void CreateQuestCard(QuestInstance quest)
    {
        if (questCardPrefab == null || questContainer == null)
        {
            Debug.LogWarning("[QuestUI] Quest card prefab or container not assigned!");
            return;
        }

        // Instantiate card
        var cardObj = Instantiate(questCardPrefab, questContainer);
        var card = cardObj.GetComponent<QuestCardUI>();

        if (card == null)
        {
            Debug.LogError("[QuestUI] Quest card prefab missing QuestCardUI component!");
            Destroy(cardObj);
            return;
        }

        // Initialize card
        card.Initialize(quest, this);
        _activeQuestCards.Add(card);
    }

    /// <summary>
    /// Clears all quest cards.
    /// </summary>
    private void ClearQuestCards()
    {
        foreach (var card in _activeQuestCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        _activeQuestCards.Clear();
    }

    #endregion

    #region Quest Actions

    /// <summary>
    /// Claims reward for a completed quest.
    /// </summary>
    public void ClaimQuest(string questID)
    {
        if (_questService == null)
            return;

        // Claim quest through service
        bool success = _questService.ClaimQuest(questID);

        if (success)
        {
            // Refresh display
            RefreshQuestDisplay();

            // Play claim sound/animation (TODO)
            Log($"Quest claimed: {questID}");
        }
        else
        {
            Debug.LogWarning($"[QuestUI] Failed to claim quest: {questID}");
        }
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Updates the "next refresh" timer text.
    /// </summary>
    private void UpdateNextRefreshText()
    {
        if (nextRefreshText == null || _questService == null)
            return;

        var timeRemaining = _questService.GetTimeUntilNextRefresh(_currentTab);

        if (timeRemaining.TotalSeconds > 0)
        {
            // Format time remaining
            if (timeRemaining.TotalHours >= 1)
            {
                nextRefreshText.text = $"Next refresh in: {timeRemaining.Hours}h {timeRemaining.Minutes}m";
            }
            else
            {
                nextRefreshText.text = $"Next refresh in: {timeRemaining.Minutes}m {timeRemaining.Seconds}s";
            }
        }
        else
        {
            nextRefreshText.text = "Refreshing...";
        }
    }

    /// <summary>
    /// Updates notification badge (shows count of claimable quests).
    /// </summary>
    private void UpdateNotificationBadge()
    {
        if (notificationBadge == null || _questService == null)
            return;

        int claimableCount = _questService.GetClaimableQuestCount();

        if (claimableCount > 0)
        {
            notificationBadge.SetActive(true);

            if (badgeCountText != null)
            {
                badgeCountText.text = claimableCount.ToString();
            }
        }
        else
        {
            notificationBadge.SetActive(false);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Call this when a quest is updated externally.
    /// </summary>
    public void OnQuestUpdated(string questID)
    {
        // Find card and update it
        var card = _activeQuestCards.FirstOrDefault(c => c.Quest?.questID == questID);
        if (card != null)
        {
            card.UpdateDisplay();
        }

        UpdateNotificationBadge();
    }

    /// <summary>
    /// Call this when a quest is completed.
    /// </summary>
    public void OnQuestCompleted(string questID)
    {
        // Refresh display to move completed quest to bottom
        RefreshQuestDisplay();

        // Play completion animation/sound (TODO)
        Log($"Quest completed notification: {questID}");
    }

    #endregion

    #region Helper Methods

    private void Log(string message)
    {
        Debug.Log($"[QuestUI] {message}");
    }

    #endregion
}

/// <summary>
/// Individual quest card UI component.
///
/// Displays:
/// - Quest name
/// - Quest description
/// - Progress bar
/// - Reward info
/// - Claim button (when complete)
/// - Expiration timer
/// </summary>
public class QuestCardUI : MonoBehaviour
{
    #region UI References

    [Header("Quest Info")]
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public Image difficultyIcon;

    [Header("Progress")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("Rewards")]
    public TextMeshProUGUI rewardText;
    public Image rewardIcon;

    [Header("Timer")]
    public TextMeshProUGUI timerText;

    [Header("Claim Button")]
    public Button claimButton;
    public GameObject completedCheckmark;

    [Header("Visual States")]
    public Color incompleteColor = new Color(0.3f, 0.3f, 0.3f);
    public Color completeColor = new Color(0.2f, 0.8f, 0.2f);
    public Color expiredColor = new Color(0.8f, 0.2f, 0.2f);

    #endregion

    #region State

    public QuestInstance Quest { get; private set; }
    private QuestUI _questUI;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the quest card with quest data.
    /// </summary>
    public void Initialize(QuestInstance quest, QuestUI questUI)
    {
        Quest = quest;
        _questUI = questUI;

        // Setup claim button
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(OnClaimButtonClicked);
        }

        UpdateDisplay();
    }

    #endregion

    #region Display Updates

    /// <summary>
    /// Updates all UI elements based on quest state.
    /// </summary>
    public void UpdateDisplay()
    {
        if (Quest == null)
            return;

        // Update name
        if (questNameText != null)
        {
            questNameText.text = Quest.displayName;
        }

        // Update description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = Quest.description;
        }

        // Update progress
        UpdateProgressBar();

        // Update rewards
        UpdateRewardDisplay();

        // Update timer
        UpdateTimerDisplay();

        // Update claim button
        UpdateClaimButton();

        // Update difficulty icon (if assigned)
        UpdateDifficultyIcon();
    }

    /// <summary>
    /// Updates progress bar and text.
    /// </summary>
    public void UpdateProgressBar()
    {
        if (Quest == null)
            return;

        if (progressBar != null)
        {
            progressBar.value = Quest.ProgressPercentage;

            // Update progress bar color based on state
            var fillImage = progressBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                if (Quest.IsExpired)
                {
                    fillImage.color = expiredColor;
                }
                else if (Quest.IsCompleted)
                {
                    fillImage.color = completeColor;
                }
                else
                {
                    fillImage.color = incompleteColor;
                }
            }
        }

        if (progressText != null)
        {
            progressText.text = $"{Quest.currentProgress}/{Quest.targetValue}";
        }
    }

    /// <summary>
    /// Updates reward display.
    /// </summary>
    private void UpdateRewardDisplay()
    {
        if (Quest == null || rewardText == null)
            return;

        // Build reward string
        var rewards = new List<string>();

        if (Quest.softCurrencyReward > 0)
        {
            rewards.Add($"{Quest.softCurrencyReward} Coins");
        }

        if (Quest.hardCurrencyReward > 0)
        {
            rewards.Add($"{Quest.hardCurrencyReward} Gems");
        }

        if (Quest.accountXPReward > 0)
        {
            rewards.Add($"{Quest.accountXPReward} XP");
        }

        if (Quest.itemRewards != null && Quest.itemRewards.Count > 0)
        {
            rewards.Add($"{Quest.itemRewards.Count} Item(s)");
        }

        rewardText.text = "Rewards: " + string.Join(", ", rewards);
    }

    /// <summary>
    /// Updates expiration timer.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (Quest == null || timerText == null)
            return;

        if (Quest.IsExpired)
        {
            timerText.text = "EXPIRED";
            timerText.color = expiredColor;
        }
        else
        {
            var timeRemaining = Quest.TimeRemaining;

            if (timeRemaining.TotalHours >= 1)
            {
                timerText.text = $"{timeRemaining.Hours}h {timeRemaining.Minutes}m";
            }
            else if (timeRemaining.TotalMinutes >= 1)
            {
                timerText.text = $"{timeRemaining.Minutes}m {timeRemaining.Seconds}s";
            }
            else
            {
                timerText.text = $"{timeRemaining.Seconds}s";
            }

            // Warning color when less than 1 hour
            timerText.color = timeRemaining.TotalHours < 1
                ? new Color(1f, 0.5f, 0f)
                : Color.white;
        }
    }

    /// <summary>
    /// Updates claim button state.
    /// </summary>
    private void UpdateClaimButton()
    {
        if (Quest == null)
            return;

        bool canClaim = Quest.IsCompleted && !Quest.IsExpired;

        if (claimButton != null)
        {
            claimButton.gameObject.SetActive(canClaim);
            claimButton.interactable = canClaim;
        }

        if (completedCheckmark != null)
        {
            // Show checkmark if already claimed (would need a "claimed" flag in QuestInstance)
            completedCheckmark.SetActive(Quest.IsCompleted);
        }
    }

    /// <summary>
    /// Updates difficulty icon based on quest difficulty.
    /// </summary>
    private void UpdateDifficultyIcon()
    {
        if (Quest == null || difficultyIcon == null)
            return;

        // Color based on difficulty
        difficultyIcon.color = Quest.difficulty switch
        {
            QuestDifficulty.Easy => new Color(0.5f, 1f, 0.5f),      // Green
            QuestDifficulty.Medium => new Color(1f, 1f, 0.5f),      // Yellow
            QuestDifficulty.Hard => new Color(1f, 0.5f, 0.2f),      // Orange
            QuestDifficulty.VeryHard => new Color(1f, 0.2f, 0.2f),  // Red
            _ => Color.white
        };
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// Called when claim button is clicked.
    /// </summary>
    private void OnClaimButtonClicked()
    {
        if (Quest == null || _questUI == null)
            return;

        if (Quest.IsCompleted && !Quest.IsExpired)
        {
            _questUI.ClaimQuest(Quest.questID);
        }
    }

    #endregion
}
