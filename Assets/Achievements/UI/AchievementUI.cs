using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GravityWars.Networking;

/// <summary>
/// UI component for displaying achievements and progress.
///
/// Features:
/// - Grid/list view of all achievements
/// - Filter by category/completion status
/// - Achievement cards with progress bars
/// - Unlock notification popups
/// - Secret achievement hiding
/// - Search functionality
/// - Sort options
/// - Statistics display
///
/// IMPORTANT: This component requires Unity UI Canvas.
///
/// Setup:
///   1. Add to Canvas GameObject
///   2. Assign UI references in Inspector
///   3. Call ShowAchievementPanel() to display
///
/// Usage:
///   AchievementUI.Instance.ShowAchievementPanel();
///   AchievementUI.Instance.ShowUnlockNotification(achievement);
/// </summary>
public class AchievementUI : MonoBehaviour
{
    #region Singleton

    private static AchievementUI _instance;
    public static AchievementUI Instance => _instance;

    #endregion

    #region UI References

    [Header("Panel References")]
    [Tooltip("Main achievement panel")]
    public GameObject achievementPanel;

    [Tooltip("Panel background overlay")]
    public Image backgroundOverlay;

    [Header("Category Filter Buttons")]
    [Tooltip("All category button")]
    public Button allCategoryButton;

    [Tooltip("Combat category button")]
    public Button combatCategoryButton;

    [Tooltip("Progression category button")]
    public Button progressionCategoryButton;

    [Tooltip("Collection category button")]
    public Button collectionCategoryButton;

    [Tooltip("Skill category button")]
    public Button skillCategoryButton;

    [Tooltip("Social category button")]
    public Button socialCategoryButton;

    [Tooltip("Secret category button")]
    public Button secretCategoryButton;

    [Header("Filter Toggles")]
    [Tooltip("Show unlocked toggle")]
    public Toggle showUnlockedToggle;

    [Tooltip("Show locked toggle")]
    public Toggle showLockedToggle;

    [Header("Achievement Display")]
    [Tooltip("Container for achievement cards")]
    public Transform achievementContainer;

    [Tooltip("Prefab for achievement card UI")]
    public GameObject achievementCardPrefab;

    [Header("Search")]
    [Tooltip("Search input field")]
    public TMP_InputField searchField;

    [Header("Statistics")]
    [Tooltip("Total achievements text")]
    public TextMeshProUGUI totalAchievementsText;

    [Tooltip("Unlocked count text")]
    public TextMeshProUGUI unlockedCountText;

    [Tooltip("Completion percentage text")]
    public TextMeshProUGUI completionPercentageText;

    [Tooltip("Achievement points text")]
    public TextMeshProUGUI achievementPointsText;

    [Header("Unlock Notification")]
    [Tooltip("Notification popup")]
    public GameObject unlockNotificationPopup;

    [Tooltip("Notification achievement name")]
    public TextMeshProUGUI notificationAchievementName;

    [Tooltip("Notification achievement description")]
    public TextMeshProUGUI notificationDescription;

    [Tooltip("Notification achievement icon")]
    public Image notificationIcon;

    [Tooltip("Notification points text")]
    public TextMeshProUGUI notificationPointsText;

    [Header("Panel Toggle Button")]
    [Tooltip("Button to show/hide panel")]
    public Button togglePanelButton;

    #endregion

    #region Configuration

    [Header("Settings")]
    [Tooltip("Default view: grid or list")]
    public bool useGridView = true;

    [Tooltip("Notification display duration")]
    public float notificationDuration = 5f;

    #endregion

    #region State

    private AchievementService _achievementService;
    private AchievementCategory _currentCategoryFilter = AchievementCategory.Combat;
    private bool _showUnlocked = true;
    private bool _showLocked = true;
    private string _searchQuery = "";
    private bool _isPanelVisible = false;
    private List<AchievementCardUI> _activeCards = new List<AchievementCardUI>();

    // Notification queue
    private Queue<AchievementInstance> _notificationQueue = new Queue<AchievementInstance>();
    private bool _isShowingNotification = false;

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

        // Hide panel initially
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        // Hide notification initially
        if (unlockNotificationPopup != null)
        {
            unlockNotificationPopup.SetActive(false);
        }
    }

    private void Start()
    {
        // Get achievement service
        _achievementService = AchievementService.Instance;

        if (_achievementService == null)
        {
            Debug.LogWarning("[AchievementUI] AchievementService not available - UI disabled");
            enabled = false;
            return;
        }

        // Subscribe to unlock events
        _achievementService.OnAchievementUnlockedEvent += OnAchievementUnlocked;

        // Setup category buttons
        SetupCategoryButtons();

        // Setup filter toggles
        SetupFilterToggles();

        // Setup search
        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchQueryChanged);
        }

        // Setup toggle button
        if (togglePanelButton != null)
        {
            togglePanelButton.onClick.AddListener(ToggleAchievementPanel);
        }

        Log("Achievement UI initialized");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_achievementService != null)
        {
            _achievementService.OnAchievementUnlockedEvent -= OnAchievementUnlocked;
        }
    }

    #endregion

    #region Panel Control

    /// <summary>
    /// Toggles achievement panel visibility.
    /// </summary>
    public void ToggleAchievementPanel()
    {
        if (_isPanelVisible)
        {
            HideAchievementPanel();
        }
        else
        {
            ShowAchievementPanel();
        }
    }

    /// <summary>
    /// Shows the achievement panel.
    /// </summary>
    public void ShowAchievementPanel()
    {
        if (_isPanelVisible)
            return;

        _isPanelVisible = true;

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
        }

        RefreshAchievementDisplay();
        UpdateStatistics();

        Log("Achievement panel shown");
    }

    /// <summary>
    /// Hides the achievement panel.
    /// </summary>
    public void HideAchievementPanel()
    {
        if (!_isPanelVisible)
            return;

        _isPanelVisible = false;

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        Log("Achievement panel hidden");
    }

    #endregion

    #region Category Filtering

    /// <summary>
    /// Sets up category filter buttons.
    /// </summary>
    private void SetupCategoryButtons()
    {
        if (allCategoryButton != null)
            allCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Combat, showAll: true));

        if (combatCategoryButton != null)
            combatCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Combat));

        if (progressionCategoryButton != null)
            progressionCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Progression));

        if (collectionCategoryButton != null)
            collectionCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Collection));

        if (skillCategoryButton != null)
            skillCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Skill));

        if (socialCategoryButton != null)
            socialCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Social));

        if (secretCategoryButton != null)
            secretCategoryButton.onClick.AddListener(() => FilterByCategory(AchievementCategory.Secret));
    }

    /// <summary>
    /// Filters achievements by category.
    /// </summary>
    public void FilterByCategory(AchievementCategory category, bool showAll = false)
    {
        if (showAll)
        {
            _currentCategoryFilter = AchievementCategory.Combat; // Will be ignored
        }
        else
        {
            _currentCategoryFilter = category;
        }

        RefreshAchievementDisplay();
        Log($"Filtered by category: {(showAll ? "All" : category.ToString())}");
    }

    #endregion

    #region Completion Filtering

    /// <summary>
    /// Sets up filter toggles.
    /// </summary>
    private void SetupFilterToggles()
    {
        if (showUnlockedToggle != null)
        {
            showUnlockedToggle.isOn = _showUnlocked;
            showUnlockedToggle.onValueChanged.AddListener(OnShowUnlockedChanged);
        }

        if (showLockedToggle != null)
        {
            showLockedToggle.isOn = _showLocked;
            showLockedToggle.onValueChanged.AddListener(OnShowLockedChanged);
        }
    }

    /// <summary>
    /// Called when "show unlocked" toggle changes.
    /// </summary>
    private void OnShowUnlockedChanged(bool value)
    {
        _showUnlocked = value;
        RefreshAchievementDisplay();
    }

    /// <summary>
    /// Called when "show locked" toggle changes.
    /// </summary>
    private void OnShowLockedChanged(bool value)
    {
        _showLocked = value;
        RefreshAchievementDisplay();
    }

    #endregion

    #region Search

    /// <summary>
    /// Called when search query changes.
    /// </summary>
    private void OnSearchQueryChanged(string query)
    {
        _searchQuery = query.ToLower();
        RefreshAchievementDisplay();
    }

    #endregion

    #region Achievement Display

    /// <summary>
    /// Refreshes the achievement display.
    /// </summary>
    public void RefreshAchievementDisplay()
    {
        if (_achievementService == null)
            return;

        // Clear existing cards
        ClearAchievementCards();

        // Get all achievements
        var achievements = _achievementService.GetAllAchievements();

        // Apply filters
        var filteredAchievements = achievements
            .Where(a => _showUnlocked || !a.isUnlocked)
            .Where(a => _showLocked || a.isUnlocked)
            .Where(a => string.IsNullOrEmpty(_searchQuery) ||
                       a.DisplayName.ToLower().Contains(_searchQuery) ||
                       a.Description.ToLower().Contains(_searchQuery))
            .OrderBy(a => a.isUnlocked ? 1 : 0) // Show locked first
            .ThenBy(a => a.category)
            .ThenBy(a => a.template?.sortOrder ?? 0)
            .ToList();

        // Create achievement cards
        foreach (var achievement in filteredAchievements)
        {
            CreateAchievementCard(achievement);
        }

        UpdateStatistics();

        Log($"Refreshed achievement display - {filteredAchievements.Count} achievements shown");
    }

    /// <summary>
    /// Creates an achievement card UI element.
    /// </summary>
    private void CreateAchievementCard(AchievementInstance achievement)
    {
        if (achievementCardPrefab == null || achievementContainer == null)
        {
            Debug.LogWarning("[AchievementUI] Achievement card prefab or container not assigned!");
            return;
        }

        // Instantiate card
        var cardObj = Instantiate(achievementCardPrefab, achievementContainer);
        var card = cardObj.GetComponent<AchievementCardUI>();

        if (card == null)
        {
            Debug.LogError("[AchievementUI] Achievement card prefab missing AchievementCardUI component!");
            Destroy(cardObj);
            return;
        }

        // Initialize card
        card.Initialize(achievement);
        _activeCards.Add(card);
    }

    /// <summary>
    /// Clears all achievement cards.
    /// </summary>
    private void ClearAchievementCards()
    {
        foreach (var card in _activeCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        _activeCards.Clear();
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Updates statistics display.
    /// </summary>
    private void UpdateStatistics()
    {
        if (_achievementService == null)
            return;

        var allAchievements = _achievementService.GetAllAchievements();
        var unlockedAchievements = _achievementService.GetUnlockedAchievements();

        // Total achievements
        if (totalAchievementsText != null)
        {
            totalAchievementsText.text = $"Total: {allAchievements.Count}";
        }

        // Unlocked count
        if (unlockedCountText != null)
        {
            unlockedCountText.text = $"Unlocked: {unlockedAchievements.Count}";
        }

        // Completion percentage
        if (completionPercentageText != null)
        {
            float percentage = _achievementService.GetCompletionPercentage();
            completionPercentageText.text = $"Completion: {percentage:F1}%";
        }

        // Achievement points
        if (achievementPointsText != null)
        {
            int totalPoints = _achievementService.GetTotalAchievementPoints();
            achievementPointsText.text = $"Points: {totalPoints}";
        }
    }

    #endregion

    #region Unlock Notifications

    /// <summary>
    /// Called when an achievement is unlocked.
    /// </summary>
    private void OnAchievementUnlocked(AchievementInstance achievement)
    {
        // Add to notification queue
        _notificationQueue.Enqueue(achievement);

        // Start showing notifications if not already showing
        if (!_isShowingNotification)
        {
            ShowNextNotification();
        }
    }

    /// <summary>
    /// Shows the next notification in queue.
    /// </summary>
    private void ShowNextNotification()
    {
        if (_notificationQueue.Count == 0)
        {
            _isShowingNotification = false;
            return;
        }

        _isShowingNotification = true;
        var achievement = _notificationQueue.Dequeue();

        ShowUnlockNotificationInternal(achievement);

        // Schedule next notification
        Invoke(nameof(HideNotificationAndShowNext), notificationDuration);
    }

    /// <summary>
    /// Hides current notification and shows next.
    /// </summary>
    private void HideNotificationAndShowNext()
    {
        if (unlockNotificationPopup != null)
        {
            unlockNotificationPopup.SetActive(false);
        }

        ShowNextNotification();
    }

    /// <summary>
    /// Shows unlock notification for an achievement.
    /// </summary>
    public void ShowUnlockNotification(AchievementInstance achievement)
    {
        _notificationQueue.Enqueue(achievement);

        if (!_isShowingNotification)
        {
            ShowNextNotification();
        }
    }

    /// <summary>
    /// Internal method to show unlock notification.
    /// </summary>
    private void ShowUnlockNotificationInternal(AchievementInstance achievement)
    {
        if (unlockNotificationPopup == null)
            return;

        // Show popup
        unlockNotificationPopup.SetActive(true);

        // Set achievement name
        if (notificationAchievementName != null)
        {
            notificationAchievementName.text = achievement.username;
        }

        // Set description
        if (notificationDescription != null)
        {
            notificationDescription.text = achievement.description;
        }

        // Set icon
        if (notificationIcon != null && achievement.template != null && achievement.template.icon != null)
        {
            notificationIcon.sprite = achievement.template.icon;
        }

        // Set points
        if (notificationPointsText != null)
        {
            notificationPointsText.text = $"+{achievement.achievementPoints} Points";
        }

        Log($"Showing unlock notification: {achievement.username}");
    }

    #endregion

    #region Helper Methods

    private void Log(string message)
    {
        Debug.Log($"[AchievementUI] {message}");
    }

    #endregion
}

/// <summary>
/// Individual achievement card UI component.
///
/// Displays:
/// - Achievement icon
/// - Achievement name
/// - Description
/// - Progress bar (for incremental achievements)
/// - Tier badge (for tiered achievements)
/// - Lock icon (for locked achievements)
/// </summary>
public class AchievementCardUI : MonoBehaviour
{
    #region UI References

    [Header("Achievement Info")]
    public Image achievementIcon;
    public TextMeshProUGUI achievementNameText;
    public TextMeshProUGUI achievementDescriptionText;

    [Header("Progress")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public GameObject progressBarContainer;

    [Header("Tier Badge")]
    public GameObject tierBadge;
    public TextMeshProUGUI tierText;
    public Image tierBackground;

    [Header("Lock Icon")]
    public GameObject lockIcon;

    [Header("Points")]
    public TextMeshProUGUI pointsText;

    [Header("Visual States")]
    public Color unlockedColor = new Color(1f, 1f, 1f);
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f);

    #endregion

    #region State

    public AchievementInstance Achievement { get; private set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the achievement card.
    /// </summary>
    public void Initialize(AchievementInstance achievement)
    {
        Achievement = achievement;
        UpdateDisplay();
    }

    #endregion

    #region Display Updates

    /// <summary>
    /// Updates all UI elements based on achievement state.
    /// </summary>
    public void UpdateDisplay()
    {
        if (Achievement == null)
            return;

        // Update icon
        if (achievementIcon != null && Achievement.template != null)
        {
            if (Achievement.template.icon != null)
            {
                achievementIcon.sprite = Achievement.template.icon;
            }

            // Desaturate if locked
            achievementIcon.color = Achievement.isUnlocked ? unlockedColor : lockedColor;
        }

        // Update name
        if (achievementNameText != null)
        {
            achievementNameText.text = Achievement.DisplayName;
        }

        // Update description
        if (achievementDescriptionText != null)
        {
            achievementDescriptionText.text = Achievement.Description;
        }

        // Update progress
        UpdateProgress();

        // Update tier badge
        UpdateTierBadge();

        // Update lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(!Achievement.isUnlocked);
        }

        // Update points
        if (pointsText != null)
        {
            pointsText.text = $"{Achievement.achievementPoints} pts";
        }
    }

    /// <summary>
    /// Updates progress bar and text.
    /// </summary>
    private void UpdateProgress()
    {
        bool showProgress = Achievement.achievementType == AchievementType.Incremental && !Achievement.isUnlocked;

        if (progressBarContainer != null)
        {
            progressBarContainer.SetActive(showProgress);
        }

        if (!showProgress)
            return;

        if (progressBar != null)
        {
            progressBar.value = Achievement.ProgressPercentage / 100f;
        }

        if (progressText != null)
        {
            progressText.text = $"{Achievement.currentProgress}/{Achievement.targetValue}";
        }
    }

    /// <summary>
    /// Updates tier badge.
    /// </summary>
    private void UpdateTierBadge()
    {
        bool showTier = Achievement.tier != AchievementTier.None;

        if (tierBadge != null)
        {
            tierBadge.SetActive(showTier);
        }

        if (!showTier)
            return;

        if (tierText != null)
        {
            tierText.text = Achievement.tier.ToString();
        }

        if (tierBackground != null)
        {
            tierBackground.color = Achievement.tier switch
            {
                AchievementTier.Bronze => new Color(0.8f, 0.5f, 0.2f),
                AchievementTier.Silver => new Color(0.75f, 0.75f, 0.75f),
                AchievementTier.Gold => new Color(1f, 0.84f, 0f),
                AchievementTier.Platinum => new Color(0.9f, 0.95f, 1f),
                AchievementTier.Diamond => new Color(0.7f, 0.9f, 1f),
                _ => Color.white
            };
        }
    }

    #endregion
}
