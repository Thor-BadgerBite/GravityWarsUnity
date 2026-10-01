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

    [Header("Hub Panels (in-scene, GDD §15.3)")]
    [Tooltip("Optional until M3: the garage panel opened by the Ships button")]
    [SerializeField] private ShipsGarageController shipsGarage;

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
        // Both are wired in M3 (GDD Implementation Plan); until then the
        // controller only has to get the player from the hub into a match.
        if (shipViewer == null)
        {
            Debug.LogWarning("[MainMenuController] ShipViewer3D not assigned - no 3D ship in the hub (M3).");
        }

        if (menuUI == null)
        {
            Debug.LogWarning("[MainMenuController] MainMenuUI not assigned - only PlayNow() is reachable (M3).");
        }
    }

    /// <summary>
    /// Initialize the main menu.
    /// Loads player profile, displays ship, sets up UI.
    /// </summary>
    private async Task InitializeMainMenu()
    {
        Debug.Log("[MainMenuController] Initializing main menu...");

        // The online account profile when signed in, the local
        // ProgressionManager profile otherwise - same fallback
        // BattlePassSystem.GetActiveProfile() already uses. Previously this
        // hard-required AccountSystem + a live sign-in and bailed out with
        // an error otherwise, even though every playtest this project has
        // actually run went through the local/offline path.
        _currentProfile = GetActiveProfile();

        if (_currentProfile == null)
        {
            Debug.LogError("[MainMenuController] No player profile available (not signed in and no local ProgressionManager data).");
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
    /// The profile to display/edit: the online account profile when signed
    /// in, the local ProgressionManager profile otherwise. Mirrors
    /// BattlePassSystem.GetActiveProfile().
    /// </summary>
    private PlayerAccountData GetActiveProfile()
    {
        if (AccountSystem.Instance != null && AccountSystem.Instance.IsSignedIn)
            return AccountSystem.Instance.CurrentPlayerProfile;

        return ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
    }

    /// <summary>
    /// Persists the current profile through whichever backing store is
    /// active (cloud profile update if signed in, local save otherwise).
    /// </summary>
    private void SaveActiveProfile()
    {
        if (AccountSystem.Instance != null && AccountSystem.Instance.IsSignedIn)
            _ = AccountSystem.Instance.UpdateProfileAsync(_currentProfile);
        else
            ProgressionManager.Instance?.Save();
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

    // GDD §15.3: the hub loads exactly one other scene, the match. Which
    // opponent the match has (hotseat or bot) is decided inside the match
    // scene's setup panel for now (GameManager.player2IsBot); online modes
    // arrive with M7.

    private void HandleRankedClicked()
    {
        Debug.Log("[MainMenuController] Ranked mode selected");

        if (!CheckRankedUnlocked())
        {
            int unlockLevel = ProgressionSystem.RANKED_UNLOCK_LEVEL;
            ShowLockedMessage($"Ranked mode unlocks at Level {unlockLevel}! (Current: Level {_currentProfile.level})");
            return;
        }

        ShowLockedMessage("Ranked play arrives with online play (GDD M7).");
    }

    private void HandleCasualClicked()
    {
        Debug.Log("[MainMenuController] Casual mode selected");
        ShowLockedMessage("Online casual play arrives with GDD M7.");
    }

    private void HandleLocalHotseatClicked()
    {
        Debug.Log("[MainMenuController] Local hotseat selected");
        LoadMatch();
    }

    private void HandleTrainingClicked()
    {
        Debug.Log("[MainMenuController] Training mode selected");
        LoadMatch();
    }

    #endregion

    #region Event Handlers - Navigation

    // Hub screens are panels inside this scene (GDD §15.3, OQ1). Panels that
    // do not exist yet are wired in M3; until then the buttons say so.

    private void HandleShipsClicked()
    {
        Debug.Log("[MainMenuController] Ships garage selected");

        if (shipsGarage != null)
        {
            shipsGarage.OpenGarage();
            return;
        }

        ShowLockedMessage("Ships garage panel not wired yet (GDD M3).");
    }

    private void HandleAchievementsClicked()
    {
        Debug.Log("[MainMenuController] Achievements selected");
        ShowLockedMessage("Achievements panel not wired yet (GDD M3).");
    }

    private void HandleSettingsClicked()
    {
        Debug.Log("[MainMenuController] Settings selected");

        if (SettingsUI.Instance != null)
        {
            SettingsUI.Instance.Show();
            return;
        }

        ShowLockedMessage("Settings panel not placed yet (GDD M1 editor step).");
    }

    private void HandleProfileClicked()
    {
        Debug.Log("[MainMenuController] Profile selected");
        ShowLockedMessage("Profile panel not wired yet (GDD M3).");
    }

    private void HandleLeaderboardClicked()
    {
        Debug.Log("[MainMenuController] Leaderboard selected");
        ShowLockedMessage("Leaderboard panel not wired yet (GDD M3).");
    }

    private void HandleQuestsClicked()
    {
        Debug.Log("[MainMenuController] Quests selected");
        ShowLockedMessage("Quests panel not wired yet (GDD M3).");
    }

    private void HandleNotificationsClicked()
    {
        Debug.Log("[MainMenuController] Notifications clicked");
        // TODO: Open notifications panel
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// PLAY NOW: leaves the hub for the match scene. Bound to the hub's
    /// PlayNowButton OnClick in the editor (GDD M1). The match scene's own
    /// setup panel takes it from there.
    /// </summary>
    public void PlayNow()
    {
        Debug.Log("[MainMenuController] Play Now");
        LoadMatch();
    }

    /// <summary>
    /// Loads the match scene, fading the hub UI out first when there is one.
    /// </summary>
    private void LoadMatch()
    {
        Debug.Log($"[MainMenuController] Loading scene: {SceneNames.Match}");

        if (menuUI != null)
        {
            menuUI.FadeOut(() =>
            {
                SceneManager.LoadScene(SceneNames.Match);
            });
        }
        else
        {
            SceneManager.LoadScene(SceneNames.Match);
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
        _currentProfile = GetActiveProfile();

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

        SaveActiveProfile();

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
