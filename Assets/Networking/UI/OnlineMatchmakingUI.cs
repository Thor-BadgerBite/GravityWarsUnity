using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace GravityWars.Networking.UI
{
    /// <summary>
    /// UI controller for online matchmaking screen.
    ///
    /// Features:
    /// - Quick Match button (auto-matchmaking)
    /// - Create Custom Lobby button
    /// - Join by Code input field
    /// - Loading/searching indicators
    /// - Error messages
    ///
    /// Usage:
    ///   Attach to a Canvas in the Main Menu scene.
    ///   Assign UI references in Inspector.
    /// </summary>
    public class OnlineMatchmakingUI : MonoBehaviour
    {
        #region UI References

        [Header("Main Panels")]
        [Tooltip("Root panel for matchmaking UI")]
        public GameObject matchmakingPanel;

        [Header("Buttons")]
        public Button quickMatchButton;
        public Button createLobbyButton;
        public Button joinByCodeButton;
        public Button backButton;

        [Header("Input Fields")]
        public TMP_InputField lobbyCodeInput;
        public TMP_InputField lobbyNameInput;

        [Header("Status Display")]
        public TextMeshProUGUI statusText;
        public GameObject loadingSpinner;

        [Header("Error Display")]
        public GameObject errorPanel;
        public TextMeshProUGUI errorText;
        public Button errorOkButton;

        #endregion

        #region State

        private bool isSearching = false;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Subscribe to button clicks
            if (quickMatchButton != null)
                quickMatchButton.onClick.AddListener(OnQuickMatchClicked);

            if (createLobbyButton != null)
                createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);

            if (joinByCodeButton != null)
                joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (errorOkButton != null)
                errorOkButton.onClick.AddListener(OnErrorOkClicked);

            // Hide loading spinner initially
            if (loadingSpinner != null)
                loadingSpinner.SetActive(false);

            // Hide error panel initially
            if (errorPanel != null)
                errorPanel.SetActive(false);

            // Subscribe to lobby events
#if UNITY_NETCODE_GAMEOBJECTS
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnLobbyCreated += OnLobbyCreated;
                LobbyManager.Instance.OnLobbyJoined += OnLobbyJoined;
            }
#endif
        }

        private void OnDestroy()
        {
            // Unsubscribe from button clicks
            if (quickMatchButton != null)
                quickMatchButton.onClick.RemoveListener(OnQuickMatchClicked);

            if (createLobbyButton != null)
                createLobbyButton.onClick.RemoveListener(OnCreateLobbyClicked);

            if (joinByCodeButton != null)
                joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);

            if (errorOkButton != null)
                errorOkButton.onClick.RemoveListener(OnErrorOkClicked);

            // Unsubscribe from lobby events
#if UNITY_NETCODE_GAMEOBJECTS
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnLobbyCreated -= OnLobbyCreated;
                LobbyManager.Instance.OnLobbyJoined -= OnLobbyJoined;
            }
#endif
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// Called when Quick Match button is clicked.
        /// </summary>
        private async void OnQuickMatchClicked()
        {
            if (isSearching)
                return;

            Debug.Log("[OnlineMatchmakingUI] Quick Match clicked");

            SetSearching(true);
            SetStatus("Searching for match...");

#if UNITY_NETCODE_GAMEOBJECTS
            bool success = await LobbyManager.Instance.QuickMatch();

            if (!success)
            {
                SetSearching(false);
                ShowError("Failed to find match. Please try again.");
            }
#else
            await Task.CompletedTask;
            SetSearching(false);
            ShowError("Online matchmaking requires Unity Netcode for GameObjects package.");
#endif

            // If successful, OnLobbyJoined will be called automatically
        }

        /// <summary>
        /// Called when Create Lobby button is clicked.
        /// </summary>
        private async void OnCreateLobbyClicked()
        {
            if (isSearching)
                return;

            Debug.Log("[OnlineMatchmakingUI] Create Lobby clicked");

            string lobbyName = lobbyNameInput != null && !string.IsNullOrEmpty(lobbyNameInput.text)
                ? lobbyNameInput.text
                : "Gravity Wars Match";

            SetSearching(true);
            SetStatus("Creating lobby...");

#if UNITY_NETCODE_GAMEOBJECTS
            bool success = await LobbyManager.Instance.CreateLobby(lobbyName, isPrivate: false);

            if (!success)
            {
                SetSearching(false);
                ShowError("Failed to create lobby. Please try again.");
            }
#else
            await Task.CompletedTask;
            SetSearching(false);
            ShowError("Online matchmaking requires Unity Netcode for GameObjects package.");
#endif

            // If successful, OnLobbyCreated will be called automatically
        }

        /// <summary>
        /// Called when Join by Code button is clicked.
        /// </summary>
        private async void OnJoinByCodeClicked()
        {
            if (isSearching)
                return;

            if (lobbyCodeInput == null || string.IsNullOrEmpty(lobbyCodeInput.text))
            {
                ShowError("Please enter a lobby code.");
                return;
            }

            string lobbyCode = lobbyCodeInput.text.Trim().ToUpper();

            Debug.Log($"[OnlineMatchmakingUI] Join by Code: {lobbyCode}");

            SetSearching(true);
            SetStatus($"Joining lobby {lobbyCode}...");

#if UNITY_NETCODE_GAMEOBJECTS
            bool success = await LobbyManager.Instance.JoinLobbyByCode(lobbyCode);

            if (!success)
            {
                SetSearching(false);
                ShowError($"Failed to join lobby '{lobbyCode}'. Please check the code and try again.");
            }
#else
            await Task.CompletedTask;
            SetSearching(false);
            ShowError("Online matchmaking requires Unity Netcode for GameObjects package.");
#endif

            // If successful, OnLobbyJoined will be called automatically
        }

        /// <summary>
        /// Called when Back button is clicked.
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("[OnlineMatchmakingUI] Back clicked");

            // Hide matchmaking panel (return to main menu)
            if (matchmakingPanel != null)
                matchmakingPanel.SetActive(false);

            // Show main menu (implement your main menu navigation here)
            // Example: MainMenuManager.Instance.ShowMainMenu();
        }

        /// <summary>
        /// Called when error OK button is clicked.
        /// </summary>
        private void OnErrorOkClicked()
        {
            HideError();
        }

        #endregion

        #region Lobby Event Handlers

        /// <summary>
        /// Called when lobby is created successfully.
        /// </summary>
        private void OnLobbyCreated(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            Debug.Log($"[OnlineMatchmakingUI] Lobby created: {lobby.Name}");

            SetSearching(false);
            SetStatus($"Lobby created! Code: {lobby.LobbyCode}");

            // Transition to lobby UI
            ShowLobbyUI();
        }

        /// <summary>
        /// Called when lobby is joined successfully.
        /// </summary>
        private void OnLobbyJoined(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            Debug.Log($"[OnlineMatchmakingUI] Lobby joined: {lobby.Name}");

            SetSearching(false);
            SetStatus($"Joined lobby: {lobby.Name}");

            // Transition to lobby UI
            ShowLobbyUI();
        }

        #endregion

        #region UI State Management

        /// <summary>
        /// Sets searching state (enables/disables buttons, shows/hides spinner).
        /// </summary>
        private void SetSearching(bool searching)
        {
            isSearching = searching;

            // Enable/disable buttons
            if (quickMatchButton != null)
                quickMatchButton.interactable = !searching;

            if (createLobbyButton != null)
                createLobbyButton.interactable = !searching;

            if (joinByCodeButton != null)
                joinByCodeButton.interactable = !searching;

            // Show/hide loading spinner
            if (loadingSpinner != null)
                loadingSpinner.SetActive(searching);
        }

        /// <summary>
        /// Updates status text.
        /// </summary>
        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        /// <summary>
        /// Shows error message.
        /// </summary>
        private void ShowError(string message)
        {
            Debug.LogWarning($"[OnlineMatchmakingUI] Error: {message}");

            if (errorPanel != null)
                errorPanel.SetActive(true);

            if (errorText != null)
                errorText.text = message;
        }

        /// <summary>
        /// Hides error message.
        /// </summary>
        private void HideError()
        {
            if (errorPanel != null)
                errorPanel.SetActive(false);
        }

        /// <summary>
        /// Transitions to lobby UI screen.
        /// </summary>
        private void ShowLobbyUI()
        {
            // Hide matchmaking panel
            if (matchmakingPanel != null)
                matchmakingPanel.SetActive(false);

            // Show lobby UI (activate LobbyUI component)
#if UNITY_NETCODE_GAMEOBJECTS
            var lobbyUI = FindObjectOfType<LobbyUI>();
            if (lobbyUI != null)
            {
                lobbyUI.Show();
            }
            else
            {
                Debug.LogWarning("[OnlineMatchmakingUI] LobbyUI component not found in scene!");
            }
#else
            Debug.LogWarning("[OnlineMatchmakingUI] LobbyUI requires Unity Netcode for GameObjects package.");
#endif
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the matchmaking UI.
        /// Call this from main menu.
        /// </summary>
        public void Show()
        {
            if (matchmakingPanel != null)
                matchmakingPanel.SetActive(true);

            SetStatus("Choose a matchmaking option");
            SetSearching(false);
        }

        /// <summary>
        /// Hides the matchmaking UI.
        /// </summary>
        public void Hide()
        {
            if (matchmakingPanel != null)
                matchmakingPanel.SetActive(false);
        }

        #endregion
    }
}
