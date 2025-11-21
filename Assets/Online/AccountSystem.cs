using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using GravityWars.Networking;

/// <summary>
/// Manages player accounts with username/password authentication.
/// Handles registration, login, password management, and session persistence.
/// Integrates with Unity Authentication and CloudSaveService.
/// </summary>
public class AccountSystem : MonoBehaviour
{
    #region Singleton

    public static AccountSystem Instance { get; private set; }

    #endregion

    #region Configuration

    [Header("Account Settings")]
    [SerializeField] private int minUsernameLength = 3;
    [SerializeField] private int maxUsernameLength = 16;
    [SerializeField] private int minPasswordLength = 6;
    [SerializeField] private bool requireEmailVerification = false;

    #endregion

    #region State

    private bool _isInitialized = false;
    private bool _isSignedIn = false;
    private PlayerAccountData _currentPlayerProfile;
    private string _currentPlayerId;

    #endregion

    #region Properties

    public bool IsInitialized => _isInitialized;
    public bool IsSignedIn => _isSignedIn;
    public PlayerAccountData CurrentPlayerProfile => _currentPlayerProfile;
    public string CurrentPlayerId => _currentPlayerId;
    public string CurrentUsername => _currentPlayerProfile?.username;

    #endregion

    #region Events

    public event Action<PlayerAccountData> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action OnLogout;
    public event Action<PlayerAccountData> OnRegistrationSuccess;
    public event Action<string> OnRegistrationFailed;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await InitializeAsync();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize Unity Services and Authentication.
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        if (_isInitialized)
        {
            Debug.Log("[AccountSystem] Already initialized");
            return true;
        }

        try
        {
            Debug.Log("[AccountSystem] Initializing Unity Services...");

            // Initialize Unity Services
            await UnityServices.InitializeAsync();

            // Set up authentication event handlers
            AuthenticationService.Instance.SignedIn += OnAuthSignedIn;
            AuthenticationService.Instance.SignedOut += OnAuthSignedOut;
            AuthenticationService.Instance.SignInFailed += OnAuthSignInFailed;
            AuthenticationService.Instance.Expired += OnAuthSessionExpired;

            _isInitialized = true;
            Debug.Log("[AccountSystem] Initialization successful");

            // Check if already signed in from previous session
            if (AuthenticationService.Instance.IsSignedIn)
            {
                await LoadCurrentPlayerProfile();
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Initialization failed: {e.Message}");
            return false;
        }
    }

    #endregion

    #region Registration

    /// <summary>
    /// Register a new account with username and password.
    /// </summary>
    public async Task<AccountResult> RegisterAsync(string username, string password)
    {
        if (!_isInitialized)
        {
            return AccountResult.Failure("Account system not initialized");
        }

        // Validate username
        var usernameValidation = ValidateUsername(username);
        if (!usernameValidation.isValid)
        {
            return AccountResult.Failure(usernameValidation.errorMessage);
        }

        // Validate password
        var passwordValidation = ValidatePassword(password);
        if (!passwordValidation.isValid)
        {
            return AccountResult.Failure(passwordValidation.errorMessage);
        }

        try
        {
            Debug.Log($"[AccountSystem] Registering account for username: {username}");

            // Check if username is already taken
            bool isTaken = await IsUsernameTaken(username);
            if (isTaken)
            {
                return AccountResult.Failure("Username already taken");
            }

            // Sign up with Unity Authentication (username/password)
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            // Get player ID
            _currentPlayerId = AuthenticationService.Instance.PlayerId;

            // Create new player profile
            _currentPlayerProfile = CreateNewPlayerProfile(_currentPlayerId, username);

            // Save profile to cloud
            bool saved = await SavePlayerProfile(_currentPlayerProfile);
            if (!saved)
            {
                Debug.LogError("[AccountSystem] Failed to save new player profile!");
                return AccountResult.Failure("Failed to save player profile");
            }

            _isSignedIn = true;

            Debug.Log($"[AccountSystem] Registration successful - Player ID: {_currentPlayerId}, Username: {username}");
            OnRegistrationSuccess?.Invoke(_currentPlayerProfile);

            return AccountResult.Success(_currentPlayerProfile);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"[AccountSystem] Registration failed: {e.Message}");
            OnRegistrationFailed?.Invoke(e.Message);
            return AccountResult.Failure($"Registration failed: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Unexpected registration error: {e.Message}");
            OnRegistrationFailed?.Invoke(e.Message);
            return AccountResult.Failure($"Unexpected error: {e.Message}");
        }
    }

    /// <summary>
    /// Create a new player profile with default values.
    /// New players receive:
    /// - Starting ELO: 1200 (Gold rank)
    /// - Starter ship: "starter_ship"
    /// - Starting credits: 1000
    /// - Level 1 with 0 XP
    /// </summary>
    private PlayerAccountData CreateNewPlayerProfile(string playerId, string username)
    {
        const string STARTER_SHIP_ID = "starter_ship";
        const int STARTING_CREDITS = 1000;

        var profile = new PlayerAccountData
        {
            playerId = playerId,
            username = username,
            accountCreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            lastLoginTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

            // Competitive stats
            eloRating = ELORatingSystem.STARTING_ELO,
            peakEloRating = ELORatingSystem.STARTING_ELO,
            currentRank = CompetitiveRank.Gold,

            // Progression
            level = 1,
            currentXP = 0,
            xpForNextLevel = 1000,
            credits = STARTING_CREDITS,
            gems = 0,

            // Ships - Initialize with starter ship
            unlockedShipModels = new List<string> { STARTER_SHIP_ID },
            currentEquippedShipId = STARTER_SHIP_ID,

            dataVersion = 1
        };

        Debug.Log($"[AccountSystem] Created new player profile with starter ship: {STARTER_SHIP_ID}");
        return profile;
    }

    #endregion

    #region Login / Logout

    /// <summary>
    /// Sign in with username and password.
    /// </summary>
    public async Task<AccountResult> LoginAsync(string username, string password)
    {
        if (!_isInitialized)
        {
            return AccountResult.Failure("Account system not initialized");
        }

        if (_isSignedIn)
        {
            return AccountResult.Failure("Already signed in");
        }

        try
        {
            Debug.Log($"[AccountSystem] Logging in with username: {username}");

            // Sign in with Unity Authentication
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            // Get player ID
            _currentPlayerId = AuthenticationService.Instance.PlayerId;

            // Load player profile from cloud
            bool loaded = await LoadCurrentPlayerProfile();
            if (!loaded)
            {
                Debug.LogError("[AccountSystem] Failed to load player profile!");
                await LogoutAsync();
                return AccountResult.Failure("Failed to load player profile");
            }

            // Update last login timestamp
            _currentPlayerProfile.lastLoginTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await SavePlayerProfile(_currentPlayerProfile);

            _isSignedIn = true;

            Debug.Log($"[AccountSystem] Login successful - Player ID: {_currentPlayerId}, Username: {_currentPlayerProfile.username}");
            OnLoginSuccess?.Invoke(_currentPlayerProfile);

            return AccountResult.Success(_currentPlayerProfile);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"[AccountSystem] Login failed: {e.Message}");
            OnLoginFailed?.Invoke(e.Message);
            return AccountResult.Failure($"Login failed: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Unexpected login error: {e.Message}");
            OnLoginFailed?.Invoke(e.Message);
            return AccountResult.Failure($"Unexpected error: {e.Message}");
        }
    }

    /// <summary>
    /// Sign out current player.
    /// </summary>
    public async Task<bool> LogoutAsync()
    {
        if (!_isSignedIn)
        {
            Debug.LogWarning("[AccountSystem] Not signed in");
            return false;
        }

        try
        {
            Debug.Log($"[AccountSystem] Logging out player: {_currentPlayerProfile?.username}");

            // Save profile before logout
            if (_currentPlayerProfile != null)
            {
                await SavePlayerProfile(_currentPlayerProfile);
            }

            // Sign out from Unity Authentication
            AuthenticationService.Instance.SignOut();

            _currentPlayerProfile = null;
            _currentPlayerId = null;
            _isSignedIn = false;

            OnLogout?.Invoke();

            Debug.Log("[AccountSystem] Logout successful");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Logout error: {e.Message}");
            return false;
        }
    }

    #endregion

    #region Profile Management

    /// <summary>
    /// Load current player's profile from cloud storage.
    /// </summary>
    private async Task<bool> LoadCurrentPlayerProfile()
    {
        try
        {
            if (CloudSaveService.Instance == null)
            {
                Debug.LogError("[AccountSystem] CloudSaveService not available!");
                return false;
            }

            // Load profile from cloud using CloudSaveService
            _currentPlayerProfile = await CloudSaveService.Instance.LoadPlayerProfile();

            // If no profile exists (new player), return false
            if (_currentPlayerProfile == null)
            {
                Debug.LogWarning("[AccountSystem] No profile found - this should only happen for new accounts");
                return false;
            }

            Debug.Log($"[AccountSystem] Loaded profile for {_currentPlayerProfile.username} (ELO: {_currentPlayerProfile.eloRating})");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Failed to load profile: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Save player profile to cloud storage.
    /// </summary>
    private async Task<bool> SavePlayerProfile(PlayerAccountData profile)
    {
        try
        {
            if (CloudSaveService.Instance == null)
            {
                Debug.LogError("[AccountSystem] CloudSaveService not available!");
                return false;
            }

            // Save profile to cloud using CloudSaveService
            bool success = await CloudSaveService.Instance.SavePlayerProfile(profile);

            if (success)
            {
                Debug.Log($"[AccountSystem] Saved profile for {profile.username} (ELO: {profile.eloRating})");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Failed to save profile: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Update current player's profile.
    /// </summary>
    public async Task<bool> UpdateProfileAsync(PlayerAccountData updatedProfile)
    {
        if (!_isSignedIn)
        {
            Debug.LogWarning("[AccountSystem] Not signed in");
            return false;
        }

        _currentPlayerProfile = updatedProfile;
        return await SavePlayerProfile(updatedProfile);
    }

    #endregion

    #region Password Management

    /// <summary>
    /// Change password for current user.
    /// </summary>
    public async Task<AccountResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (!_isSignedIn)
        {
            return AccountResult.Failure("Not signed in");
        }

        // Validate new password
        var validation = ValidatePassword(newPassword);
        if (!validation.isValid)
        {
            return AccountResult.Failure(validation.errorMessage);
        }

        try
        {
            // Unity Authentication password change
            // Note: This requires re-authentication with current password
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);

            Debug.Log("[AccountSystem] Password changed successfully");
            return AccountResult.Success(null);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"[AccountSystem] Password change failed: {e.Message}");
            return AccountResult.Failure($"Password change failed: {e.Message}");
        }
    }

    /// <summary>
    /// Request password reset (email-based).
    /// </summary>
    public async Task<AccountResult> RequestPasswordResetAsync(string email)
    {
        try
        {
            // Unity Authentication password reset
            // Note: Requires email to be linked to account
            Debug.Log($"[AccountSystem] Password reset requested for: {email}");

            // TODO: Implement email-based password reset
            return AccountResult.Failure("Password reset not yet implemented");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Password reset request failed: {e.Message}");
            return AccountResult.Failure($"Password reset failed: {e.Message}");
        }
    }

    #endregion

    #region Username Management

    /// <summary>
    /// Check if username is already taken.
    /// </summary>
    private async Task<bool> IsUsernameTaken(string username)
    {
        // TODO: Query server/database for username existence
        // For now, always return false (username available)
        await Task.Delay(100); // Simulate network delay
        return false;
    }

    /// <summary>
    /// Change username for current user.
    /// </summary>
    public async Task<AccountResult> ChangeUsernameAsync(string newUsername)
    {
        if (!_isSignedIn)
        {
            return AccountResult.Failure("Not signed in");
        }

        // Validate new username
        var validation = ValidateUsername(newUsername);
        if (!validation.isValid)
        {
            return AccountResult.Failure(validation.errorMessage);
        }

        // Check if username is taken
        bool isTaken = await IsUsernameTaken(newUsername);
        if (isTaken)
        {
            return AccountResult.Failure("Username already taken");
        }

        try
        {
            // Update profile
            _currentPlayerProfile.username = newUsername;
            await SavePlayerProfile(_currentPlayerProfile);

            Debug.Log($"[AccountSystem] Username changed to: {newUsername}");
            return AccountResult.Success(_currentPlayerProfile);
        }
        catch (Exception e)
        {
            Debug.LogError($"[AccountSystem] Username change failed: {e.Message}");
            return AccountResult.Failure($"Username change failed: {e.Message}");
        }
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate username format and length.
    /// </summary>
    private (bool isValid, string errorMessage) ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return (false, "Username cannot be empty");
        }

        if (username.Length < minUsernameLength)
        {
            return (false, $"Username must be at least {minUsernameLength} characters");
        }

        if (username.Length > maxUsernameLength)
        {
            return (false, $"Username must be at most {maxUsernameLength} characters");
        }

        // Check for valid characters (alphanumeric + underscore)
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
        {
            return (false, "Username can only contain letters, numbers, and underscores");
        }

        return (true, null);
    }

    /// <summary>
    /// Validate password strength.
    /// </summary>
    private (bool isValid, string errorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Password cannot be empty");
        }

        if (password.Length < minPasswordLength)
        {
            return (false, $"Password must be at least {minPasswordLength} characters");
        }

        return (true, null);
    }

    #endregion

    #region Authentication Event Handlers

    private void OnAuthSignedIn()
    {
        Debug.Log("[AccountSystem] Authentication signed in event");
    }

    private void OnAuthSignedOut()
    {
        Debug.Log("[AccountSystem] Authentication signed out event");
    }

    private void OnAuthSignInFailed(RequestFailedException e)
    {
        Debug.LogError($"[AccountSystem] Authentication sign-in failed: {e.Message}");
    }

    private async void OnAuthSessionExpired()
    {
        Debug.LogWarning("[AccountSystem] Authentication session expired");
        await LogoutAsync();
    }

    #endregion

    #region Debug / Testing

    /// <summary>
    /// Get account status for debugging.
    /// </summary>
    public string GetAccountStatus()
    {
        if (!_isInitialized)
        {
            return "Account System: Not Initialized";
        }

        if (!_isSignedIn)
        {
            return "Account System: Initialized, Not Signed In";
        }

        return $"Account System: Signed In\n" +
               $"Player ID: {_currentPlayerId}\n" +
               $"Username: {_currentPlayerProfile?.username}\n" +
               $"ELO: {_currentPlayerProfile?.eloRating}\n" +
               $"Rank: {_currentPlayerProfile?.currentRank}\n" +
               $"Level: {_currentPlayerProfile?.level}";
    }

    #endregion
}

/// <summary>
/// Account operation result.
/// </summary>
public class AccountResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; }
    public PlayerAccountData Profile { get; private set; }

    private AccountResult(bool isSuccess, string errorMessage, PlayerAccountData profile)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Profile = profile;
    }

    public static AccountResult Success(PlayerAccountData profile)
    {
        return new AccountResult(true, null, profile);
    }

    public static AccountResult Failure(string errorMessage)
    {
        return new AccountResult(false, errorMessage, null);
    }
}
