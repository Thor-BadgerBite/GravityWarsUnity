using UnityEngine;
using Unity.Netcode;

namespace GravityWars.Networking
{
    /// <summary>
    /// Adapter that bridges GameManager with online multiplayer networking.
    ///
    /// This component sits between GameManager and the networking layer,
    /// translating local game events into network messages and vice versa.
    ///
    /// IMPORTANT: Attach this to the same GameObject as GameManager.
    ///
    /// How it works:
    /// 1. GameManager runs normally (handles local game logic, UI, physics)
    /// 2. OnlineGameAdapter intercepts player actions and sends them over network
    /// 3. Remote clients receive actions and replay them locally (deterministic)
    /// 4. Both clients simulate the exact same physics, ensuring sync
    ///
    /// Usage:
    ///   - Attach to GameManager GameObject
    ///   - Set isOnlineMode = true before starting match
    ///   - GameManager will automatically use network for actions
    /// </summary>
    [RequireComponent(typeof(GameManager))]
    public class OnlineGameAdapter : MonoBehaviour
    {
        #region Configuration

        [Header("Online Mode")]
        [Tooltip("Is this an online match? (Set by LobbyManager before starting)")]
        public bool isOnlineMode = false;

        [Tooltip("Enable debug logging")]
        public bool debugLogging = true;

        #endregion

        #region Component References

        private GameManager _gameManager;
        private MatchManager _matchManager;
        private NetworkedPlayerShip _player1NetworkedShip;
        private NetworkedPlayerShip _player2NetworkedShip;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();

            if (_gameManager == null)
            {
                Debug.LogError("[OnlineGameAdapter] GameManager component not found!");
            }
        }

        private void Start()
        {
            if (isOnlineMode)
            {
                InitializeOnlineMode();
            }
        }

        #endregion

        #region Online Mode Initialization

        /// <summary>
        /// Initializes online mode connections and references.
        /// </summary>
        private void InitializeOnlineMode()
        {
            Log("Initializing online mode...");

            // Find MatchManager
            _matchManager = FindObjectOfType<MatchManager>();
            if (_matchManager == null)
            {
                Debug.LogError("[OnlineGameAdapter] MatchManager not found! Cannot run online match.");
                isOnlineMode = false;
                return;
            }

            // Find or add NetworkedPlayerShip components to player ships
            if (_gameManager.player1Ship != null)
            {
                _player1NetworkedShip = _gameManager.player1Ship.GetComponent<NetworkedPlayerShip>();
                if (_player1NetworkedShip == null)
                {
                    _player1NetworkedShip = _gameManager.player1Ship.gameObject.AddComponent<NetworkedPlayerShip>();
                }
            }

            if (_gameManager.player2Ship != null)
            {
                _player2NetworkedShip = _gameManager.player2Ship.GetComponent<NetworkedPlayerShip>();
                if (_player2NetworkedShip == null)
                {
                    _player2NetworkedShip = _gameManager.player2Ship.gameObject.AddComponent<NetworkedPlayerShip>();
                }
            }

            Log("Online mode initialized successfully");
        }

        #endregion

        #region Action Interception (Called by GameManager)

        /// <summary>
        /// Called when local player fires a missile.
        /// If online mode, sends action over network instead of executing locally.
        /// </summary>
        public void OnPlayerFireMissile(PlayerShip ship, float angle, float power, int perkSlot = -1)
        {
            if (!isOnlineMode)
            {
                // Local mode - GameManager handles it normally
                return;
            }

            Log($"Player fired missile (online): angle={angle}, power={power}, perk={perkSlot}");

            // Get NetworkedPlayerShip for this ship
            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null && networkedShip.IsOwner)
            {
                // Send action over network
                networkedShip.RequestFire(angle, power, perkSlot);
            }
            else
            {
                Debug.LogWarning($"[OnlineGameAdapter] Cannot fire - not owner or networked ship not found");
            }
        }

        /// <summary>
        /// Called when local player moves their ship.
        /// </summary>
        public void OnPlayerMove(PlayerShip ship, Vector3 direction, float power)
        {
            if (!isOnlineMode)
            {
                return;
            }

            Log($"Player moved ship (online): direction={direction}, power={power}");

            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null && networkedShip.IsOwner)
            {
                networkedShip.RequestMove(direction, power);
            }
            else
            {
                Debug.LogWarning($"[OnlineGameAdapter] Cannot move - not owner or networked ship not found");
            }
        }

        /// <summary>
        /// Called when local player activates a perk.
        /// </summary>
        public void OnPlayerActivatePerk(PlayerShip ship, int perkSlot)
        {
            if (!isOnlineMode)
            {
                return;
            }

            Log($"Player activated perk (online): slot={perkSlot}");

            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null && networkedShip.IsOwner)
            {
                networkedShip.RequestActivatePerk(perkSlot);
            }
            else
            {
                Debug.LogWarning($"[OnlineGameAdapter] Cannot activate perk - not owner");
            }
        }

        #endregion

        #region Damage & Health Sync

        /// <summary>
        /// Called when a ship takes damage.
        /// Syncs damage to network if online mode.
        /// </summary>
        public void OnShipTakeDamage(PlayerShip ship, float damage)
        {
            if (!isOnlineMode)
            {
                return;
            }

            Log($"Ship took damage (online): {damage}");

            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null)
            {
                // Send damage to server for validation and broadcast
                networkedShip.TakeDamageServerRpc(damage);
            }
        }

        /// <summary>
        /// Called when a ship is healed.
        /// </summary>
        public void OnShipHeal(PlayerShip ship, float amount)
        {
            if (!isOnlineMode)
            {
                return;
            }

            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null)
            {
                networkedShip.HealServerRpc(amount);
            }
        }

        #endregion

        #region Turn Management Integration

        /// <summary>
        /// Called when a turn starts.
        /// Syncs turn state with MatchManager if online.
        /// </summary>
        public void OnTurnStart(PlayerShip activePlayer)
        {
            if (!isOnlineMode || _matchManager == null)
            {
                return;
            }

            Log($"Turn started for: {activePlayer.playerName}");

            // MatchManager handles turn sync automatically via NetworkVariables
            // GameManager just needs to enable/disable controls based on IsMyTurn

            var networkedShip = activePlayer == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null)
            {
                // Only enable controls if it's this player's turn
                bool isMyTurn = networkedShip.IsMyTurn();
                Log($"Is my turn: {isMyTurn}");

                // GameManager will handle control enabling/disabling
                // based on this information
            }
        }

        /// <summary>
        /// Called when a turn ends.
        /// </summary>
        public void OnTurnEnd()
        {
            if (!isOnlineMode || _matchManager == null)
            {
                return;
            }

            Log("Turn ended");

            // MatchManager will handle switching to next player
            _matchManager.EndTurn();
        }

        #endregion

        #region Match End Integration

        /// <summary>
        /// Called when a ship is destroyed.
        /// Notifies MatchManager if online.
        /// </summary>
        public void OnShipDestroyed(PlayerShip ship)
        {
            if (!isOnlineMode || _matchManager == null)
            {
                return;
            }

            Log($"Ship destroyed: {ship.playerName}");

            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null && NetworkManager.Singleton.IsServer)
            {
                // Server notifies MatchManager
                _matchManager.ShipDestroyedServerRpc(networkedShip.OwnerClientId);
            }
        }

        /// <summary>
        /// Called when match ends.
        /// Awards XP and returns to lobby.
        /// </summary>
        public void OnMatchEnd(PlayerShip winner)
        {
            if (!isOnlineMode)
            {
                return;
            }

            Log($"Match ended - Winner: {winner.playerName}");

            // Award progression (will sync to cloud)
            // GameManager handles this normally

            // After progression awarded, disconnect and return to lobby
            StartCoroutine(ReturnToLobbyCoroutine());
        }

        private System.Collections.IEnumerator ReturnToLobbyCoroutine()
        {
            // Wait for progression UI to finish
            yield return new WaitForSeconds(5f);

            // Leave lobby and disconnect
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.LeaveLobby();
            }

            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns true if it's the local player's turn.
        /// </summary>
        public bool IsMyTurn(PlayerShip ship)
        {
            if (!isOnlineMode)
            {
                return true; // In local mode, always allow actions (GameManager handles turn order)
            }

            var networkedShip = ship == _gameManager.player1Ship ? _player1NetworkedShip : _player2NetworkedShip;

            if (networkedShip != null)
            {
                return networkedShip.IsMyTurn();
            }

            return false;
        }

        /// <summary>
        /// Returns true if this is an online match.
        /// GameManager can check this to decide whether to use network or local logic.
        /// </summary>
        public bool IsOnlineMatch()
        {
            return isOnlineMode;
        }

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[OnlineGameAdapter] {message}");
        }

        #endregion

        #region Public API for GameManager

        /// <summary>
        /// Enables this adapter for online mode.
        /// Call this from LobbyManager before starting the match.
        /// </summary>
        public void EnableOnlineMode()
        {
            isOnlineMode = true;
            InitializeOnlineMode();
            Log("Online mode enabled");
        }

        /// <summary>
        /// Disables online mode (returns to local hot-seat).
        /// </summary>
        public void DisableOnlineMode()
        {
            isOnlineMode = false;
            Log("Online mode disabled");
        }

        #endregion
    }
}
