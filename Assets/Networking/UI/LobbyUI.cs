using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

namespace GravityWars.Networking.UI
{
    /// <summary>
    /// UI controller for lobby screen (waiting room before match starts).
    ///
    /// Features:
    /// - Display lobby name and code
    /// - Show list of players in lobby
    /// - Ready/Not Ready status for each player
    /// - Leave Lobby button
    /// - Auto-start match when both players ready
    ///
    /// Usage:
    ///   Attach to a Canvas.
    ///   Assign UI references in Inspector.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        #region UI References

        [Header("Main Panels")]
        public GameObject lobbyPanel;

        [Header("Lobby Info")]
        public TextMeshProUGUI lobbyNameText;
        public TextMeshProUGUI lobbyCodeText;
        public Button copyCodeButton;

        [Header("Player List")]
        public Transform playerListContainer;
        public GameObject playerListItemPrefab;

        [Header("Actions")]
        public Button readyButton;
        public TextMeshProUGUI readyButtonText;
        public Button leaveLobbyButton;

        [Header("Status")]
        public TextMeshProUGUI statusText;

        #endregion

        #region State

        private bool isReady = false;
        private List<GameObject> playerListItems = new List<GameObject>();

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Subscribe to button clicks
            if (readyButton != null)
                readyButton.onClick.AddListener(OnReadyButtonClicked);

            if (leaveLobbyButton != null)
                leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);

            if (copyCodeButton != null)
                copyCodeButton.onClick.AddListener(OnCopyCodeClicked);

            // Subscribe to lobby events
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayerJoined += OnPlayerJoined;
                LobbyManager.Instance.OnPlayerLeft += OnPlayerLeft;
                LobbyManager.Instance.OnLobbyLeft += OnLobbyLeft;
            }

            // Hide initially
            Hide();
        }

        private void OnDestroy()
        {
            // Unsubscribe from button clicks
            if (readyButton != null)
                readyButton.onClick.RemoveListener(OnReadyButtonClicked);

            if (leaveLobbyButton != null)
                leaveLobbyButton.onClick.RemoveListener(OnLeaveLobbyClicked);

            if (copyCodeButton != null)
                copyCodeButton.onClick.RemoveListener(OnCopyCodeClicked);

            // Unsubscribe from lobby events
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                LobbyManager.Instance.OnPlayerLeft -= OnPlayerLeft;
                LobbyManager.Instance.OnLobbyLeft -= OnLobbyLeft;
            }
        }

        private void Update()
        {
            // Poll lobby for updates
            // (In a real implementation, you'd use Unity Lobby Service's polling or events)
            // For now, just refresh every 2 seconds
            if (lobbyPanel != null && lobbyPanel.activeSelf)
            {
                if (Time.frameCount % 120 == 0) // Every 2 seconds at 60 FPS
                {
                    RefreshLobbyUI();
                }
            }
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// Called when Ready button is clicked.
        /// </summary>
        private void OnReadyButtonClicked()
        {
            Debug.Log("[LobbyUI] Ready button clicked");

            isReady = !isReady;

            // Update button text
            if (readyButtonText != null)
                readyButtonText.text = isReady ? "Not Ready" : "Ready";

            // Update lobby with ready status
            // (In full implementation, this would update lobby data via Lobby Service)

            // Update UI
            RefreshLobbyUI();

            // Check if both players ready
            CheckIfBothPlayersReady();
        }

        /// <summary>
        /// Called when Leave Lobby button is clicked.
        /// </summary>
        private void OnLeaveLobbyClicked()
        {
            Debug.Log("[LobbyUI] Leave Lobby clicked");

            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.LeaveLobby();
            }

            // Return to matchmaking
            Hide();

            var matchmakingUI = FindObjectOfType<OnlineMatchmakingUI>();
            if (matchmakingUI != null)
            {
                matchmakingUI.Show();
            }
        }

        /// <summary>
        /// Called when Copy Code button is clicked.
        /// </summary>
        private void OnCopyCodeClicked()
        {
            if (LobbyManager.Instance != null && LobbyManager.Instance.CurrentLobby != null)
            {
                string lobbyCode = LobbyManager.Instance.CurrentLobby.LobbyCode;

                // Copy to clipboard
                GUIUtility.systemCopyBuffer = lobbyCode;

                Debug.Log($"[LobbyUI] Copied lobby code to clipboard: {lobbyCode}");

                // Show feedback
                if (statusText != null)
                    statusText.text = "Lobby code copied to clipboard!";
            }
        }

        #endregion

        #region Lobby Event Handlers

        /// <summary>
        /// Called when a player joins the lobby.
        /// </summary>
        private void OnPlayerJoined(Player player)
        {
            Debug.Log($"[LobbyUI] Player joined: {player.Id}");

            RefreshLobbyUI();
        }

        /// <summary>
        /// Called when a player leaves the lobby.
        /// </summary>
        private void OnPlayerLeft(Player player)
        {
            Debug.Log($"[LobbyUI] Player left: {player.Id}");

            RefreshLobbyUI();
        }

        /// <summary>
        /// Called when local player leaves the lobby.
        /// </summary>
        private void OnLobbyLeft()
        {
            Debug.Log("[LobbyUI] Left lobby");

            Hide();
        }

        #endregion

        #region UI Management

        /// <summary>
        /// Refreshes the lobby UI with latest data.
        /// </summary>
        private void RefreshLobbyUI()
        {
            if (LobbyManager.Instance == null || LobbyManager.Instance.CurrentLobby == null)
                return;

            var lobby = LobbyManager.Instance.CurrentLobby;

            // Update lobby info
            if (lobbyNameText != null)
                lobbyNameText.text = lobby.Name;

            if (lobbyCodeText != null)
                lobbyCodeText.text = $"Code: {lobby.LobbyCode}";

            // Update player list
            RefreshPlayerList(lobby);

            // Update status
            if (statusText != null)
            {
                int playerCount = lobby.Players.Count;
                int maxPlayers = lobby.MaxPlayers;
                statusText.text = $"Players: {playerCount}/{maxPlayers}";
            }
        }

        /// <summary>
        /// Refreshes the player list display.
        /// </summary>
        private void RefreshPlayerList(Lobby lobby)
        {
            // Clear existing list items
            foreach (var item in playerListItems)
            {
                if (item != null)
                    Destroy(item);
            }
            playerListItems.Clear();

            // Create new list items for each player
            foreach (var player in lobby.Players)
            {
                CreatePlayerListItem(player);
            }
        }

        /// <summary>
        /// Creates a player list item UI element.
        /// </summary>
        private void CreatePlayerListItem(Player player)
        {
            if (playerListItemPrefab == null || playerListContainer == null)
                return;

            GameObject item = Instantiate(playerListItemPrefab, playerListContainer);
            playerListItems.Add(item);

            // Get display name from player data
            string displayName = "Player";
            if (player.Data != null && player.Data.ContainsKey("DisplayName"))
            {
                displayName = player.Data["DisplayName"].Value;
            }

            // Get account level
            string accountLevel = "";
            if (player.Data != null && player.Data.ContainsKey("AccountLevel"))
            {
                accountLevel = $"(Lv. {player.Data["AccountLevel"].Value})";
            }

            // Update UI elements in the prefab
            var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"{displayName} {accountLevel}";
            }

            // Show ready status (would come from player data in full implementation)
            // For now, just show player ID
            Debug.Log($"[LobbyUI] Player: {displayName} {accountLevel}");
        }

        /// <summary>
        /// Checks if both players are ready and starts the match if so.
        /// </summary>
        private void CheckIfBothPlayersReady()
        {
            if (LobbyManager.Instance == null || LobbyManager.Instance.CurrentLobby == null)
                return;

            var lobby = LobbyManager.Instance.CurrentLobby;

            // Check if we have 2 players
            if (lobby.Players.Count < 2)
            {
                Debug.Log("[LobbyUI] Waiting for second player...");
                return;
            }

            // In full implementation, check if both players have ready status = true
            // For now, just check player count

            // Both players present - start match countdown
            if (statusText != null)
                statusText.text = "Both players ready! Starting match...";

            // Start match after short delay
            Invoke(nameof(StartMatch), 3f);
        }

        /// <summary>
        /// Starts the online match.
        /// </summary>
        private void StartMatch()
        {
            Debug.Log("[LobbyUI] Starting match!");

            // Hide lobby UI
            Hide();

            // Load game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("HotSeat");
            // Note: The scene will have OnlineGameAdapter enabled for online mode
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the lobby UI.
        /// </summary>
        public void Show()
        {
            if (lobbyPanel != null)
                lobbyPanel.SetActive(true);

            RefreshLobbyUI();

            Debug.Log("[LobbyUI] Shown");
        }

        /// <summary>
        /// Hides the lobby UI.
        /// </summary>
        public void Hide()
        {
            if (lobbyPanel != null)
                lobbyPanel.SetActive(false);

            isReady = false;

            if (readyButtonText != null)
                readyButtonText.text = "Ready";

            Debug.Log("[LobbyUI] Hidden");
        }

        #endregion
    }
}
