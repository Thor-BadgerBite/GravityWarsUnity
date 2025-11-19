using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GravityWars.Networking;

/// <summary>
/// UI component for displaying leaderboards.
///
/// Features:
/// - Multiple leaderboard tabs (Global, Friends, Seasonal)
/// - Stat type selection (Wins, Damage, Accuracy, Streaks, etc.)
/// - Pagination for large leaderboards
/// - Player highlight (scroll to player's position)
/// - Rank change indicators
/// - Refresh button
/// - Time frame selection (All-Time, Season, Monthly, Weekly, Daily)
///
/// IMPORTANT: This component requires Unity UI Canvas.
///
/// Setup:
///   1. Add to Canvas GameObject
///   2. Assign UI references in Inspector
///   3. Call ShowLeaderboard Panel() to display
///
/// Usage:
///   LeaderboardUI.Instance.ShowLeaderboardPanel();
///   LeaderboardUI.Instance.SelectLeaderboard(leaderboardID);
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    #region Singleton

    private static LeaderboardUI _instance;
    public static LeaderboardUI Instance => _instance;

    #endregion

    #region UI References

    [Header("Panel References")]
    [Tooltip("Main leaderboard panel")]
    public GameObject leaderboardPanel;

    [Tooltip("Panel background overlay")]
    public Image backgroundOverlay;

    [Header("Leaderboard Selection")]
    [Tooltip("Dropdown for leaderboard selection")]
    public TMP_Dropdown leaderboardDropdown;

    [Header("Scope Tabs")]
    [Tooltip("Global leaderboard tab")]
    public Button globalTabButton;

    [Tooltip("Friends leaderboard tab")]
    public Button friendsTabButton;

    [Tooltip("Seasonal leaderboard tab")]
    public Button seasonalTabButton;

    [Header("Entry Display")]
    [Tooltip("Container for leaderboard entries")]
    public Transform entryContainer;

    [Tooltip("Prefab for leaderboard entry")]
    public GameObject entryPrefab;

    [Tooltip("Scroll rect for scrolling entries")]
    public ScrollRect scrollRect;

    [Header("Pagination")]
    [Tooltip("Previous page button")]
    public Button previousPageButton;

    [Tooltip("Next page button")]
    public Button nextPageButton;

    [Tooltip("Page number text")]
    public TextMeshProUGUI pageNumberText;

    [Tooltip("Jump to player button")]
    public Button jumpToPlayerButton;

    [Header("Header")]
    [Tooltip("Leaderboard title")]
    public TextMeshProUGUI leaderboardTitleText;

    [Tooltip("Last updated text")]
    public TextMeshProUGUI lastUpdatedText;

    [Tooltip("Refresh button")]
    public Button refreshButton;

    [Header("Loading")]
    [Tooltip("Loading indicator")]
    public GameObject loadingIndicator;

    [Header("Player Info")]
    [Tooltip("Player rank text")]
    public TextMeshProUGUI playerRankText;

    [Tooltip("Player score text")]
    public TextMeshProUGUI playerScoreText;

    [Header("Panel Toggle")]
    [Tooltip("Button to show/hide panel")]
    public Button togglePanelButton;

    #endregion

    #region Configuration

    [Header("Settings")]
    [Tooltip("Entries per page")]
    public int entriesPerPage = 20;

    [Tooltip("Auto-refresh interval (seconds)")]
    public float autoRefreshInterval = 60f;

    [Tooltip("Enable auto-refresh")]
    public bool enableAutoRefresh = true;

    #endregion

    #region State

    private LeaderboardService _leaderboardService;
    private LeaderboardScope _currentScope = LeaderboardScope.Global;
    private string _currentLeaderboardID;
    private LeaderboardData _currentLeaderboardData;
    private int _currentPage = 0;
    private bool _isPanelVisible = false;
    private bool _isLoading = false;

    private List<LeaderboardEntryUI> _activeEntryUIs = new List<LeaderboardEntryUI>();

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
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        // Hide loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }

    private void Start()
    {
        // Get leaderboard service
        _leaderboardService = LeaderboardService.Instance;

        if (_leaderboardService == null)
        {
            Debug.LogWarning("[LeaderboardUI] LeaderboardService not available - UI disabled");
            enabled = false;
            return;
        }

        // Setup UI
        SetupTabs();
        SetupLeaderboardDropdown();
        SetupPagination();
        SetupButtons();

        // Auto-refresh
        if (enableAutoRefresh)
        {
            InvokeRepeating(nameof(RefreshCurrentLeaderboard), autoRefreshInterval, autoRefreshInterval);
        }

        Log("Leaderboard UI initialized");
    }

    #endregion

    #region Panel Control

    /// <summary>
    /// Toggles leaderboard panel visibility.
    /// </summary>
    public void ToggleLeaderboardPanel()
    {
        if (_isPanelVisible)
        {
            HideLeaderboardPanel();
        }
        else
        {
            ShowLeaderboardPanel();
        }
    }

    /// <summary>
    /// Shows the leaderboard panel.
    /// </summary>
    public void ShowLeaderboardPanel()
    {
        if (_isPanelVisible)
            return;

        _isPanelVisible = true;

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
        }

        // Load default leaderboard if none selected
        if (string.IsNullOrEmpty(_currentLeaderboardID))
        {
            SelectFirstLeaderboard();
        }
        else
        {
            RefreshCurrentLeaderboard();
        }

        Log("Leaderboard panel shown");
    }

    /// <summary>
    /// Hides the leaderboard panel.
    /// </summary>
    public void HideLeaderboardPanel()
    {
        if (!_isPanelVisible)
            return;

        _isPanelVisible = false;

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        Log("Leaderboard panel hidden");
    }

    #endregion

    #region UI Setup

    /// <summary>
    /// Sets up scope tab buttons.
    /// </summary>
    private void SetupTabs()
    {
        if (globalTabButton != null)
            globalTabButton.onClick.AddListener(() => SwitchScope(LeaderboardScope.Global));

        if (friendsTabButton != null)
            friendsTabButton.onClick.AddListener(() => SwitchScope(LeaderboardScope.Friends));

        if (seasonalTabButton != null)
            seasonalTabButton.onClick.AddListener(() => SwitchScope(LeaderboardScope.Global)); // TODO: Add seasonal scope
    }

    /// <summary>
    /// Sets up leaderboard dropdown.
    /// </summary>
    private void SetupLeaderboardDropdown()
    {
        if (leaderboardDropdown == null)
            return;

        // Populate dropdown with leaderboard definitions
        leaderboardDropdown.options.Clear();

        foreach (var definition in _leaderboardService.leaderboardDefinitions)
        {
            leaderboardDropdown.options.Add(new TMP_Dropdown.OptionData(definition.displayName));
        }

        leaderboardDropdown.onValueChanged.AddListener(OnLeaderboardDropdownChanged);
        leaderboardDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Sets up pagination buttons.
    /// </summary>
    private void SetupPagination()
    {
        if (previousPageButton != null)
            previousPageButton.onClick.AddListener(PreviousPage);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);

        if (jumpToPlayerButton != null)
            jumpToPlayerButton.onClick.AddListener(JumpToPlayer);
    }

    /// <summary>
    /// Sets up other buttons.
    /// </summary>
    private void SetupButtons()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(() => RefreshCurrentLeaderboard(forceRefresh: true));

        if (togglePanelButton != null)
            togglePanelButton.onClick.AddListener(ToggleLeaderboardPanel);
    }

    #endregion

    #region Leaderboard Selection

    /// <summary>
    /// Selects first leaderboard from list.
    /// </summary>
    private void SelectFirstLeaderboard()
    {
        if (_leaderboardService.leaderboardDefinitions.Count > 0)
        {
            SelectLeaderboard(_leaderboardService.leaderboardDefinitions[0].leaderboardID);
        }
    }

    /// <summary>
    /// Selects leaderboard by ID.
    /// </summary>
    public async void SelectLeaderboard(string leaderboardID)
    {
        if (_isLoading)
            return;

        _currentLeaderboardID = leaderboardID;
        _currentPage = 0;

        await LoadLeaderboard();
    }

    /// <summary>
    /// Called when dropdown selection changes.
    /// </summary>
    private void OnLeaderboardDropdownChanged(int index)
    {
        if (index < 0 || index >= _leaderboardService.leaderboardDefinitions.Count)
            return;

        var definition = _leaderboardService.leaderboardDefinitions[index];
        SelectLeaderboard(definition.leaderboardID);
    }

    #endregion

    #region Scope Switching

    /// <summary>
    /// Switches leaderboard scope (Global/Friends).
    /// </summary>
    private void SwitchScope(LeaderboardScope scope)
    {
        _currentScope = scope;
        _currentPage = 0;

        RefreshCurrentLeaderboard();

        Log($"Switched to {scope} scope");
    }

    #endregion

    #region Leaderboard Loading

    /// <summary>
    /// Loads current leaderboard.
    /// </summary>
    private async System.Threading.Tasks.Task LoadLeaderboard()
    {
        if (string.IsNullOrEmpty(_currentLeaderboardID))
            return;

        _isLoading = true;
        ShowLoadingIndicator();

        try
        {
            // Fetch leaderboard
            LeaderboardData leaderboardData;

            if (_currentScope == LeaderboardScope.Friends)
            {
                leaderboardData = await _leaderboardService.FetchFriendLeaderboard(_currentLeaderboardID);
            }
            else
            {
                leaderboardData = await _leaderboardService.FetchLeaderboard(
                    _currentLeaderboardID,
                    _currentPage,
                    entriesPerPage
                );
            }

            if (leaderboardData != null)
            {
                _currentLeaderboardData = leaderboardData;
                DisplayLeaderboard(leaderboardData);
            }
        }
        finally
        {
            _isLoading = false;
            HideLoadingIndicator();
        }
    }

    /// <summary>
    /// Refreshes current leaderboard.
    /// </summary>
    private async void RefreshCurrentLeaderboard(bool forceRefresh = false)
    {
        if (string.IsNullOrEmpty(_currentLeaderboardID) || !_isPanelVisible)
            return;

        await LoadLeaderboard();
    }

    #endregion

    #region Display

    /// <summary>
    /// Displays leaderboard data.
    /// </summary>
    private void DisplayLeaderboard(LeaderboardData leaderboardData)
    {
        if (leaderboardData == null)
            return;

        // Clear existing entries
        ClearEntries();

        // Update header
        if (leaderboardTitleText != null)
        {
            leaderboardTitleText.text = leaderboardData.definition.displayName;
        }

        if (lastUpdatedText != null)
        {
            var timeSince = System.DateTime.UtcNow - leaderboardData.lastUpdated;
            lastUpdatedText.text = $"Updated {FormatTimeSince(timeSince)} ago";
        }

        // Create entry UIs
        foreach (var entry in leaderboardData.entries)
        {
            CreateEntryUI(entry);
        }

        // Update pagination
        UpdatePaginationButtons();

        // Update player info
        UpdatePlayerInfo(leaderboardData);

        Log($"Displayed leaderboard: {leaderboardData.entries.Count} entries");
    }

    /// <summary>
    /// Creates entry UI for leaderboard entry.
    /// </summary>
    private void CreateEntryUI(LeaderboardEntry entry)
    {
        if (entryPrefab == null || entryContainer == null)
        {
            Debug.LogWarning("[LeaderboardUI] Entry prefab or container not assigned!");
            return;
        }

        // Instantiate entry
        var entryObj = Instantiate(entryPrefab, entryContainer);
        var entryUI = entryObj.GetComponent<LeaderboardEntryUI>();

        if (entryUI == null)
        {
            Debug.LogError("[LeaderboardUI] Entry prefab missing LeaderboardEntryUI component!");
            Destroy(entryObj);
            return;
        }

        // Initialize entry
        entryUI.Initialize(entry);
        _activeEntryUIs.Add(entryUI);
    }

    /// <summary>
    /// Clears all entry UIs.
    /// </summary>
    private void ClearEntries()
    {
        foreach (var entryUI in _activeEntryUIs)
        {
            if (entryUI != null)
            {
                Destroy(entryUI.gameObject);
            }
        }
        _activeEntryUIs.Clear();
    }

    /// <summary>
    /// Updates player info display.
    /// </summary>
    private void UpdatePlayerInfo(LeaderboardData leaderboardData)
    {
        var playerEntry = leaderboardData.playerEntry ?? leaderboardData.GetEntryByPlayerID(_leaderboardService.GetPlayerStats().playerID);

        if (playerEntry != null)
        {
            if (playerRankText != null)
            {
                playerRankText.text = $"Your Rank: {playerEntry.GetFormattedRank()}";
            }

            if (playerScoreText != null)
            {
                playerScoreText.text = $"Score: {playerEntry.formattedScore}";
            }
        }
        else
        {
            if (playerRankText != null)
            {
                playerRankText.text = "Your Rank: Unranked";
            }

            if (playerScoreText != null)
            {
                playerScoreText.text = "";
            }
        }
    }

    #endregion

    #region Pagination

    /// <summary>
    /// Goes to previous page.
    /// </summary>
    private void PreviousPage()
    {
        if (_currentPage <= 0)
            return;

        _currentPage--;
        RefreshCurrentLeaderboard();
    }

    /// <summary>
    /// Goes to next page.
    /// </summary>
    private void NextPage()
    {
        _currentPage++;
        RefreshCurrentLeaderboard();
    }

    /// <summary>
    /// Jumps to player's position in leaderboard.
    /// </summary>
    private async void JumpToPlayer()
    {
        if (string.IsNullOrEmpty(_currentLeaderboardID))
            return;

        _isLoading = true;
        ShowLoadingIndicator();

        try
        {
            var leaderboardData = await _leaderboardService.FetchLeaderboardAroundPlayer(
                _currentLeaderboardID,
                range: entriesPerPage / 2
            );

            if (leaderboardData != null)
            {
                _currentLeaderboardData = leaderboardData;
                DisplayLeaderboard(leaderboardData);

                // Scroll to player entry
                ScrollToPlayerEntry();
            }
        }
        finally
        {
            _isLoading = false;
            HideLoadingIndicator();
        }
    }

    /// <summary>
    /// Scrolls to player entry.
    /// </summary>
    private void ScrollToPlayerEntry()
    {
        var playerEntry = _activeEntryUIs.Find(e => e.Entry.isSelf);

        if (playerEntry != null && scrollRect != null)
        {
            // Calculate scroll position
            Canvas.ForceUpdateCanvases();

            var entryTransform = playerEntry.GetComponent<RectTransform>();
            var containerTransform = entryContainer as RectTransform;

            if (entryTransform != null && containerTransform != null)
            {
                float contentHeight = containerTransform.rect.height;
                float viewportHeight = scrollRect.viewport.rect.height;
                float entryY = entryTransform.anchoredPosition.y;

                float scrollPosition = Mathf.Clamp01((entryY + contentHeight / 2 - viewportHeight / 2) / (contentHeight - viewportHeight));
                scrollRect.verticalNormalizedPosition = 1f - scrollPosition;
            }
        }
    }

    /// <summary>
    /// Updates pagination button states.
    /// </summary>
    private void UpdatePaginationButtons()
    {
        // Previous button
        if (previousPageButton != null)
        {
            previousPageButton.interactable = _currentPage > 0;
        }

        // Next button
        if (nextPageButton != null)
        {
            bool hasMorePages = _currentLeaderboardData != null &&
                                (_currentPage + 1) * entriesPerPage < _currentLeaderboardData.totalEntries;
            nextPageButton.interactable = hasMorePages;
        }

        // Page number
        if (pageNumberText != null)
        {
            int totalPages = _currentLeaderboardData != null
                ? Mathf.CeilToInt((float)_currentLeaderboardData.totalEntries / entriesPerPage)
                : 1;
            pageNumberText.text = $"Page {_currentPage + 1} / {totalPages}";
        }
    }

    #endregion

    #region Loading Indicator

    /// <summary>
    /// Shows loading indicator.
    /// </summary>
    private void ShowLoadingIndicator()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Hides loading indicator.
    /// </summary>
    private void HideLoadingIndicator()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Formats time since as human-readable string.
    /// </summary>
    private string FormatTimeSince(System.TimeSpan timeSince)
    {
        if (timeSince.TotalMinutes < 1)
            return "moments";
        else if (timeSince.TotalMinutes < 60)
            return $"{(int)timeSince.TotalMinutes} min";
        else if (timeSince.TotalHours < 24)
            return $"{(int)timeSince.TotalHours} hr";
        else
            return $"{(int)timeSince.TotalDays} day";
    }

    private void Log(string message)
    {
        Debug.Log($"[LeaderboardUI] {message}");
    }

    #endregion
}

/// <summary>
/// Individual leaderboard entry UI component.
///
/// Displays:
/// - Rank with medal for top 3
/// - Player name and avatar
/// - Score
/// - Rank change indicator
/// - Highlight for current player
/// </summary>
public class LeaderboardEntryUI : MonoBehaviour
{
    #region UI References

    [Header("Entry Info")]
    public TextMeshProUGUI rankText;
    public Image rankMedal;
    public Image playerAvatar;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI rankChangeText;
    public Image background;

    [Header("Medal Sprites")]
    public Sprite goldMedalSprite;
    public Sprite silverMedalSprite;
    public Sprite bronzeMedalSprite;

    [Header("Colors")]
    public Color selfColor = new Color(1f, 0.9f, 0.5f);
    public Color friendColor = new Color(0.5f, 0.9f, 1f);
    public Color normalColor = new Color(1f, 1f, 1f);

    #endregion

    #region State

    public LeaderboardEntry Entry { get; private set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the entry UI.
    /// </summary>
    public void Initialize(LeaderboardEntry entry)
    {
        Entry = entry;
        UpdateDisplay();
    }

    #endregion

    #region Display

    /// <summary>
    /// Updates all UI elements.
    /// </summary>
    public void UpdateDisplay()
    {
        if (Entry == null)
            return;

        // Update rank
        if (rankText != null)
        {
            rankText.text = Entry.rank.ToString();
        }

        // Update rank medal (top 3)
        UpdateRankMedal();

        // Update player name
        if (playerNameText != null)
        {
            playerNameText.text = Entry.playerName;
        }

        // Update score
        if (scoreText != null)
        {
            scoreText.text = Entry.formattedScore;
        }

        // Update rank change
        if (rankChangeText != null)
        {
            string change = Entry.GetRankChange();
            rankChangeText.text = change;

            if (change.StartsWith("+"))
                rankChangeText.color = Color.green;
            else if (change.StartsWith("-"))
                rankChangeText.color = Color.red;
            else
                rankChangeText.color = Color.gray;
        }

        // Update background color
        UpdateBackgroundColor();

        // TODO: Load player avatar
        if (playerAvatar != null)
        {
            // playerAvatar.sprite = LoadAvatar(Entry.playerAvatarURL);
        }
    }

    /// <summary>
    /// Updates rank medal for top 3.
    /// </summary>
    private void UpdateRankMedal()
    {
        if (rankMedal == null)
            return;

        if (Entry.rank == 1 && goldMedalSprite != null)
        {
            rankMedal.sprite = goldMedalSprite;
            rankMedal.gameObject.SetActive(true);
        }
        else if (Entry.rank == 2 && silverMedalSprite != null)
        {
            rankMedal.sprite = silverMedalSprite;
            rankMedal.gameObject.SetActive(true);
        }
        else if (Entry.rank == 3 && bronzeMedalSprite != null)
        {
            rankMedal.sprite = bronzeMedalSprite;
            rankMedal.gameObject.SetActive(true);
        }
        else
        {
            rankMedal.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates background color based on player status.
    /// </summary>
    private void UpdateBackgroundColor()
    {
        if (background == null)
            return;

        if (Entry.isSelf)
        {
            background.color = selfColor;
        }
        else if (Entry.isFriend)
        {
            background.color = friendColor;
        }
        else
        {
            background.color = normalColor;
        }
    }

    #endregion
}
