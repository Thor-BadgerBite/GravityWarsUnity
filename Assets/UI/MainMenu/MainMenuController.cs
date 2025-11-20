using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

/// <summary>
/// Main menu controller (Brawl Stars style).
/// Coordinates between ShipViewer3D, MainMenuUI, and AccountSystem.
///
/// Features:
/// - Displays player's equipped ship in 3D
/// - Shows player stats (username, level, ELO, rank)
/// - Handles navigation to different game modes
/// - Manages profile updates
/// - Handles scene transitions
/// </summary>
public class MainMenuController : MonoBehaviour
{
    #region Singleton

    public static MainMenuController Instance { get; private set; }

    #endregion

    #region Inspector References

    [Header("Components")]
    [SerializeField] private ShipViewer3D shipViewer;
    [SerializeField] private MainMenuUI menuUI;

    [Header("Scene Names")]
    [SerializeField] private string rankedMatchmakingScene = "RankedMatchmaking";
    [SerializeField] private string casualMatchmakingScene = "CasualMatchmaking";
    [SerializeField] private string localHotseatScene = "LocalHotseat";
    [SerializeField] private string trainingScene = "Training";
    [SerializeField] private string shipsScene = "ShipsGarage";
    [SerializeField] private string achievementsScene = "Achievements";
    [SerializeField] private string settingsScene = "Settings";
    [SerializeField] private string profileScene = "Profile";
    [SerializeField] private string leaderboardScene = "Leaderboard";
    [SerializeField] private string questsScene = "Quests";

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mainMenuMusic;

    #endregion

    #region State

    private PlayerAccountData _currentProfile;
    private bool _isInitialized = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ValidateReferences();
    }

    private async void Start()
    {
        await InitializeMainMenu();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Validate that all required components are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        if (shipViewer == null)
        {
            Debug.LogError("[MainMenuController] ShipViewer3D not assigned! Please assign in inspector.");
        }

        if (menuUI == null)
        {
            Debug.LogError("[MainMenuController] MainMenuUI not assigned! Please assign in inspector.");
        }
    }

    /// <summary>
    /// Initialize the main menu.
    /// Loads player profile, displays ship, sets up UI.
    /// </summary>
    private async Task InitializeMainMenu()
    {
        Debug.Log("[MainMenuController] Initializing main menu...");

        // Check if account system is ready
        if (AccountSystem.Instance == null)
        {
            Debug.LogError("[MainMenuController] AccountSystem not found! Make sure player is logged in.");
            return;
        }

        if (!AccountSystem.Instance.IsSignedIn)
        {
            Debug.LogError("[MainMenuController] Player not signed in! Redirect to login screen.");
            // TODO: Load login scene
            return;
        }

        // Get current player profile
        _currentProfile = AccountSystem.Instance.CurrentPlayerProfile;

        if (_currentProfile == null)
        {
            Debug.LogError("[MainMenuController] Failed to load player profile!");
            return;
        }

        // Initialize UI
        InitializeUI();

        // Display player's equipped ship
        DisplayPlayerShip();

        // Start background music
        PlayBackgroundMusic();

        // Subscribe to events
        SubscribeToEvents();

        _isInitialized = true;
        Debug.Log("[MainMenuController] Main menu initialized successfully");
    }

    /// <summary>
    /// Initialize UI with player data.
    /// </summary>
    private void InitializeUI()
    {
        if (menuUI == null) return;

        // Update player info
        menuUI.UpdatePlayerInfo(_currentProfile);

        // Update notifications (check for new achievements, messages, etc.)
        int notificationCount = GetNotificationCount();
        menuUI.UpdateNotifications(notificationCount);

        // Check if ranked is unlocked (e.g., requires completing tutorial)
        bool rankedUnlocked = CheckRankedUnlocked();
        menuUI.SetRankedButtonEnabled(rankedUnlocked);
    }

    /// <summary>
    /// Display player's currently equipped ship.
    /// </summary>
    private void DisplayPlayerShip()
    {
        if (shipViewer == null) return;

        string equippedShipId = _currentProfile.currentEquippedShipId;

        if (string.IsNullOrEmpty(equippedShipId))
        {
            Debug.LogWarning("[MainMenuController] No ship equipped! Using default starter ship.");
            equippedShipId = "starter_ship";
        }

        shipViewer.DisplayShip(equippedShipId);
    }

    /// <summary>
    /// Play background music.
    /// </summary>
    private void PlayBackgroundMusic()
    {
        if (musicSource == null || mainMenuMusic == null) return;

        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    #endregion

    #region Event Subscriptions

    /// <summary>
    /// Subscribe to UI events.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (menuUI == null) return;

        // Game mode events
        menuUI.OnRankedClicked += HandleRankedClicked;
        menuUI.OnCasualClicked += HandleCasualClicked;
        menuUI.OnLocalHotseatClicked += HandleLocalHotseatClicked;
        menuUI.OnTrainingClicked += HandleTrainingClicked;

        // Navigation events
        menuUI.OnShipsClicked += HandleShipsClicked;
        menuUI.OnAchievementsClicked += HandleAchievementsClicked;
        menuUI.OnSettingsClicked += HandleSettingsClicked;
        menuUI.OnProfileClicked += HandleProfileClicked;
        menuUI.OnLeaderboardClicked += HandleLeaderboardClicked;
        menuUI.OnQuestsClicked += HandleQuestsClicked;
        menuUI.OnNotificationsClicked += HandleNotificationsClicked;
    }

    /// <summary>
    /// Unsubscribe from UI events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (menuUI == null) return;

        menuUI.OnRankedClicked -= HandleRankedClicked;
        menuUI.OnCasualClicked -= HandleCasualClicked;
        menuUI.OnLocalHotseatClicked -= HandleLocalHotseatClicked;
        menuUI.OnTrainingClicked -= HandleTrainingClicked;

        menuUI.OnShipsClicked -= HandleShipsClicked;
        menuUI.OnAchievementsClicked -= HandleAchievementsClicked;
        menuUI.OnSettingsClicked -= HandleSettingsClicked;
        menuUI.OnProfileClicked -= HandleProfileClicked;
        menuUI.OnLeaderboardClicked -= HandleLeaderboardClicked;
        menuUI.OnQuestsClicked -= HandleQuestsClicked;
        menuUI.OnNotificationsClicked -= HandleNotificationsClicked;
    }

    #endregion

    #region Event Handlers - Game Modes

    private void HandleRankedClicked()
    {
        Debug.Log("[MainMenuController] Ranked mode selected");

        if (!CheckRankedUnlocked())
        {
            int unlockLevel = ProgressionSystem.RANKED_UNLOCK_LEVEL;
            ShowLockedMessage($"Ranked mode unlocks at Level {unlockLevel}! (Current: Level {_currentProfile.level})");
            return;
        }

        LoadScene(rankedMatchmakingScene);
    }

    private void HandleCasualClicked()
    {
        Debug.Log("[MainMenuController] Casual mode selected");
        LoadScene(casualMatchmakingScene);
    }

    private void HandleLocalHotseatClicked()
    {
        Debug.Log("[MainMenuController] Local hotseat selected");
        LoadScene(localHotseatScene);
    }

    private void HandleTrainingClicked()
    {
        Debug.Log("[MainMenuController] Training mode selected");
        LoadScene(trainingScene);
    }

    #endregion

    #region Event Handlers - Navigation

    private void HandleShipsClicked()
    {
        Debug.Log("[MainMenuController] Ships garage selected");
        LoadScene(shipsScene);
    }

    private void HandleAchievementsClicked()
    {
        Debug.Log("[MainMenuController] Achievements selected");
        LoadScene(achievementsScene);
    }

    private void HandleSettingsClicked()
    {
        Debug.Log("[MainMenuController] Settings selected");
        LoadScene(settingsScene);
    }

    private void HandleProfileClicked()
    {
        Debug.Log("[MainMenuController] Profile selected");
        LoadScene(profileScene);
    }

    private void HandleLeaderboardClicked()
    {
        Debug.Log("[MainMenuController] Leaderboard selected");
        LoadScene(leaderboardScene);
    }

    private void HandleQuestsClicked()
    {
        Debug.Log("[MainMenuController] Quests selected");
        LoadScene(questsScene);
    }

    private void HandleNotificationsClicked()
    {
        Debug.Log("[MainMenuController] Notifications clicked");
        // TODO: Open notifications panel
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// Load a scene with fade transition.
    /// </summary>
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MainMenuController] Scene name not set!");
            return;
        }

        Debug.Log($"[MainMenuController] Loading scene: {sceneName}");

        // Fade out UI
        if (menuUI != null)
        {
            menuUI.FadeOut(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    #endregion

    #region Profile Updates

    /// <summary>
    /// Refresh player profile from cloud.
    /// Call this when returning from another scene.
    /// </summary>
    public async Task RefreshProfile()
    {
        if (AccountSystem.Instance == null || !AccountSystem.Instance.IsSignedIn)
        {
            Debug.LogError("[MainMenuController] Cannot refresh profile - not signed in");
            return;
        }

        _currentProfile = AccountSystem.Instance.CurrentPlayerProfile;

        if (_currentProfile != null && menuUI != null)
        {
            menuUI.UpdatePlayerInfo(_currentProfile);
        }
    }

    /// <summary>
    /// Update equipped ship (called when player changes ship).
    /// </summary>
    public void UpdateEquippedShip(string shipId)
    {
        if (shipViewer == null) return;

        _currentProfile.currentEquippedShipId = shipId;
        shipViewer.DisplayShip(shipId);

        // Save profile
        _ = AccountSystem.Instance.UpdateProfileAsync(_currentProfile);

        Debug.Log($"[MainMenuController] Equipped ship updated: {shipId}");
    }

    #endregion

    #region Utility

    /// <summary>
    /// Check if ranked mode is unlocked.
    /// Uses ProgressionSystem for level-based unlocking.
    /// </summary>
    private bool CheckRankedUnlocked()
    {
        return ProgressionSystem.IsRankedUnlocked(_currentProfile.level);
    }

    /// <summary>
    /// Get notification count (achievements, messages, etc.).
    /// </summary>
    private int GetNotificationCount()
    {
        int count = 0;

        // Check for unclaimed achievements
        // Check for new messages
        // Check for quest completions
        // etc.

        return count;
    }

    /// <summary>
    /// Show locked feature message.
    /// </summary>
    private void ShowLockedMessage(string message)
    {
        Debug.LogWarning($"[MainMenuController] Feature locked: {message}");
        // TODO: Show UI popup
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get current player profile.
    /// </summary>
    public PlayerAccountData GetCurrentProfile()
    {
        return _currentProfile;
    }

    /// <summary>
    /// Get ship viewer component.
    /// </summary>
    public ShipViewer3D GetShipViewer()
    {
        return shipViewer;
    }

    /// <summary>
    /// Get main menu UI component.
    /// </summary>
    public MainMenuUI GetMenuUI()
    {
        return menuUI;
    }

    /// <summary>
    /// Check if main menu is initialized.
    /// </summary>
    public bool IsInitialized()
    {
        return _isInitialized;
    }

    #endregion
}
