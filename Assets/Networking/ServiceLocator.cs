using UnityEngine;

namespace GravityWars.Networking
{
    /// <summary>
    /// Central service registry and dependency injection hub for all game services.
    /// Provides singleton access to networking, cloud save, analytics, and other services.
    ///
    /// Usage:
    ///   ServiceLocator.Instance.CloudSave.SavePlayerData(data);
    ///   ServiceLocator.Instance.Analytics.TrackEvent("match_started");
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        #region Singleton

        private static ServiceLocator _instance;
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ServiceLocator>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[ServiceLocator]");
                        _instance = go.AddComponent<ServiceLocator>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Service References

        private NetworkService _networkService;
        private CloudSaveService _cloudSaveService;
        private AnalyticsService _analyticsService;
        private QuestService _questService;
        private AchievementService _achievementService;
        private LeaderboardService _leaderboardService;

        /// <summary>Network service for online multiplayer, matchmaking, and relay connections.</summary>
        public NetworkService Network
        {
            get
            {
                if (_networkService == null)
                    _networkService = GetOrCreateService<NetworkService>();
                return _networkService;
            }
        }

        /// <summary>Cloud save service for server-side player data synchronization.</summary>
        public CloudSaveService CloudSave
        {
            get
            {
                if (_cloudSaveService == null)
                    _cloudSaveService = GetOrCreateService<CloudSaveService>();
                return _cloudSaveService;
            }
        }

        /// <summary>Analytics service for tracking player events and behavior.</summary>
        public AnalyticsService Analytics
        {
            get
            {
                if (_analyticsService == null)
                    _analyticsService = GetOrCreateService<AnalyticsService>();
                return _analyticsService;
            }
        }

        /// <summary>Quest service for daily/weekly/season quest management.</summary>
        public QuestService Quests
        {
            get
            {
                if (_questService == null)
                    _questService = GetOrCreateService<QuestService>();
                return _questService;
            }
        }

        /// <summary>Achievement service for tracking and unlocking achievements.</summary>
        public AchievementService Achievements
        {
            get
            {
                if (_achievementService == null)
                    _achievementService = GetOrCreateService<AchievementService>();
                return _achievementService;
            }
        }

        /// <summary>Leaderboard service for global and archetype-specific rankings.</summary>
        public LeaderboardService Leaderboards
        {
            get
            {
                if (_leaderboardService == null)
                    _leaderboardService = GetOrCreateService<LeaderboardService>();
                return _leaderboardService;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Enforce singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[ServiceLocator] Initialized - Service hub ready");
        }

        private async void Start()
        {
            // Initialize Unity Gaming Services
            await InitializeUnityServices();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// Gets or creates a service component on this GameObject.
        /// </summary>
        private T GetOrCreateService<T>() where T : MonoBehaviour
        {
            T service = GetComponent<T>();
            if (service == null)
            {
                service = gameObject.AddComponent<T>();
                Debug.Log($"[ServiceLocator] Created service: {typeof(T).Name}");
            }
            return service;
        }

        /// <summary>
        /// Initializes Unity Gaming Services (Authentication, Core Services).
        /// Must be called before using any UGS features.
        /// </summary>
        private async System.Threading.Tasks.Task InitializeUnityServices()
        {
            try
            {
                // Check if already initialized
                if (Unity.Services.Core.UnityServices.State == Unity.Services.Core.ServicesInitializationState.Initialized)
                {
                    Debug.Log("[ServiceLocator] Unity Services already initialized");
                    return;
                }

                Debug.Log("[ServiceLocator] Initializing Unity Gaming Services...");

                // Initialize Core Services
                await Unity.Services.Core.UnityServices.InitializeAsync();

                Debug.Log("[ServiceLocator] Unity Services initialized successfully");

                // Sign in anonymously (can be replaced with proper auth later)
                await AuthenticateAnonymously();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ServiceLocator] Failed to initialize Unity Services: {e.Message}");
            }
        }

        /// <summary>
        /// Authenticates the player anonymously.
        /// For Phase 4+, this will be replaced with proper account authentication.
        /// </summary>
        private async System.Threading.Tasks.Task AuthenticateAnonymously()
        {
            try
            {
                if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log($"[ServiceLocator] Already signed in as: {Unity.Services.Authentication.AuthenticationService.Instance.PlayerId}");
                    return;
                }

                Debug.Log("[ServiceLocator] Signing in anonymously...");

                await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();

                string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                Debug.Log($"[ServiceLocator] Signed in successfully! Player ID: {playerId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ServiceLocator] Failed to sign in: {e.Message}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Returns true if Unity Services are initialized and player is authenticated.
        /// </summary>
        public bool IsReady()
        {
            return Unity.Services.Core.UnityServices.State == Unity.Services.Core.ServicesInitializationState.Initialized
                && Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn;
        }

        /// <summary>
        /// Gets the current authenticated player ID.
        /// </summary>
        public string GetPlayerId()
        {
            if (!IsReady())
                return null;

            return Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
        }

        #endregion
    }
}
