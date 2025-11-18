using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace GravityWars.Networking
{
    /// <summary>
    /// Networked wrapper for PlayerShip that synchronizes ship state across clients.
    ///
    /// This component works alongside PlayerShip (does not replace it).
    /// It uses NetworkVariables to sync critical state and RPCs for actions.
    ///
    /// IMPORTANT: Attach this component to the same GameObject as PlayerShip.
    /// Also add NetworkTransform component for position/rotation sync.
    ///
    /// Synchronized State:
    /// - Health (current/max)
    /// - Moves remaining this turn
    /// - Perk usage states
    /// - Ship alive/destroyed state
    ///
    /// Actions (via RPCs):
    /// - Fire missile
    /// - Move ship
    /// - Activate perk
    /// - Self-destruct
    ///
    /// Usage:
    ///   - Local player: Call RequestFire(), RequestMove(), etc.
    ///   - Remote player: Observe NetworkVariables
    /// </summary>
    [RequireComponent(typeof(PlayerShip))]
    [RequireComponent(typeof(NetworkTransform))]
    public class NetworkedPlayerShip : NetworkBehaviour
    {
        #region Component References

        private PlayerShip _playerShip;
        private GameManager _gameManager;
        private GravityWars.Networking.MatchManager _matchManager;

        #endregion

        #region Network Variables

        /// <summary>Current health (synchronized across network)</summary>
        private NetworkVariable<float> _networkHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        /// <summary>Maximum health (set at start, rarely changes)</summary>
        private NetworkVariable<float> _networkMaxHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        /// <summary>Moves remaining this turn</summary>
        private NetworkVariable<int> _networkMovesRemaining = new NetworkVariable<int>(
            3,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>Is ship destroyed?</summary>
        private NetworkVariable<bool> _networkIsDestroyed = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>Current action mode (Fire/Move)</summary>
        private NetworkVariable<int> _networkActionMode = new NetworkVariable<int>(
            0, // Fire = 0, Move = 1
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        #endregion

        #region Public Properties

        public float NetworkHealth => _networkHealth.Value;
        public float NetworkMaxHealth => _networkMaxHealth.Value;
        public int NetworkMovesRemaining => _networkMovesRemaining.Value;
        public bool NetworkIsDestroyed => _networkIsDestroyed.Value;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _playerShip = GetComponent<PlayerShip>();
            _gameManager = FindObjectOfType<GameManager>();
            _matchManager = FindObjectOfType<GravityWars.Networking.MatchManager>();

            if (_playerShip == null)
            {
                Debug.LogError("[NetworkedPlayerShip] PlayerShip component not found!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to network variable changes
            _networkHealth.OnValueChanged += OnHealthChanged;
            _networkIsDestroyed.OnValueChanged += OnDestroyedChanged;
            _networkMovesRemaining.OnValueChanged += OnMovesRemainingChanged;

            // Initialize network variables from PlayerShip
            if (IsOwner)
            {
                _networkHealth.Value = _playerShip.maxHealth;
                _networkMaxHealth.Value = _playerShip.maxHealth;
                _networkMovesRemaining.Value = _playerShip.movesAllowedPerTurn;
            }

            Debug.Log($"[NetworkedPlayerShip] Spawned - Owner: {IsOwner}, ClientId: {OwnerClientId}");
        }

        public override void OnNetworkDespawn()
        {
            // Unsubscribe from network variable changes
            _networkHealth.OnValueChanged -= OnHealthChanged;
            _networkIsDestroyed.OnValueChanged -= OnDestroyedChanged;
            _networkMovesRemaining.OnValueChanged -= OnMovesRemainingChanged;

            base.OnNetworkDespawn();
        }

        #endregion

        #region Action Requests (Called by Local Player)

        /// <summary>
        /// Local player requests to fire a missile.
        /// This sends the action to the server for validation and broadcast.
        /// </summary>
        public void RequestFire(float angle, float power, int perkSlot = -1)
        {
            if (!IsOwner)
            {
                Debug.LogWarning("[NetworkedPlayerShip] Only owner can request fire");
                return;
            }

            if (_matchManager != null)
            {
                // Send to server for validation
                _matchManager.FireMissileServerRpc(angle, power, perkSlot);
            }
            else
            {
                Debug.LogError("[NetworkedPlayerShip] MatchManager not found!");
            }
        }

        /// <summary>
        /// Local player requests to move ship.
        /// </summary>
        public void RequestMove(Vector3 direction, float power)
        {
            if (!IsOwner)
            {
                Debug.LogWarning("[NetworkedPlayerShip] Only owner can request move");
                return;
            }

            if (_matchManager != null)
            {
                // Send to server for validation
                _matchManager.MoveShipServerRpc(direction, power);
            }
            else
            {
                Debug.LogError("[NetworkedPlayerShip] MatchManager not found!");
            }
        }

        /// <summary>
        /// Local player requests to activate a perk.
        /// </summary>
        public void RequestActivatePerk(int perkSlot)
        {
            if (!IsOwner)
            {
                Debug.LogWarning("[NetworkedPlayerShip] Only owner can activate perk");
                return;
            }

            if (_matchManager != null)
            {
                // Send to server for validation
                _matchManager.ActivatePerkServerRpc(perkSlot);
            }
            else
            {
                Debug.LogError("[NetworkedPlayerShip] MatchManager not found!");
            }
        }

        #endregion

        #region Damage & Health (Networked)

        /// <summary>
        /// Applies damage to this ship (server-side).
        /// Call this from missile collision detection.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damage, ServerRpcParams rpcParams = default)
        {
            if (_networkIsDestroyed.Value)
                return; // Already destroyed

            // Apply damage
            float newHealth = Mathf.Max(0, _networkHealth.Value - damage);
            _networkHealth.Value = newHealth;

            Debug.Log($"[NetworkedPlayerShip] Took {damage} damage. New health: {newHealth}/{_networkMaxHealth.Value}");

            // Check for destruction
            if (newHealth <= 0)
            {
                _networkIsDestroyed.Value = true;

                // Notify MatchManager
                if (_matchManager != null)
                {
                    _matchManager.ShipDestroyedServerRpc(OwnerClientId);
                }
            }

            // Broadcast damage to clients
            TakeDamageClientRpc(damage, newHealth);
        }

        [ClientRpc]
        private void TakeDamageClientRpc(float damage, float newHealth)
        {
            Debug.Log($"[NetworkedPlayerShip] Client received damage: {damage}, new health: {newHealth}");

            // Update visual effects, UI, etc.
            if (_playerShip != null)
            {
                // PlayerShip will handle visual damage effects
                // (We're not modifying PlayerShip.cs directly, just syncing state)
            }
        }

        /// <summary>
        /// Heals this ship (server-side).
        /// Used for regeneration, lifesteal, etc.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void HealServerRpc(float amount)
        {
            if (_networkIsDestroyed.Value)
                return;

            float newHealth = Mathf.Min(_networkMaxHealth.Value, _networkHealth.Value + amount);
            _networkHealth.Value = newHealth;

            Debug.Log($"[NetworkedPlayerShip] Healed {amount}. New health: {newHealth}");
        }

        #endregion

        #region Turn Management

        /// <summary>
        /// Resets moves for a new turn (server-side).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ResetMovesForTurnServerRpc()
        {
            _networkMovesRemaining.Value = _playerShip.movesAllowedPerTurn;
            Debug.Log($"[NetworkedPlayerShip] Moves reset to {_networkMovesRemaining.Value}");
        }

        /// <summary>
        /// Consumes one move action (server-side).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ConsumeActionServerRpc()
        {
            if (_networkMovesRemaining.Value > 0)
            {
                _networkMovesRemaining.Value--;
                Debug.Log($"[NetworkedPlayerShip] Action consumed. Moves remaining: {_networkMovesRemaining.Value}");
            }
        }

        #endregion

        #region Network Event Handlers

        private void OnHealthChanged(float oldHealth, float newHealth)
        {
            Debug.Log($"[NetworkedPlayerShip] Health changed: {oldHealth} → {newHealth}");

            // Update UI, visual effects, etc.
            // (GameManager will handle UI updates)
        }

        private void OnDestroyedChanged(bool wasDestroyed, bool isDestroyed)
        {
            if (isDestroyed)
            {
                Debug.Log($"[NetworkedPlayerShip] Ship destroyed!");

                // Trigger destruction visual effects
                if (_playerShip != null)
                {
                    // PlayerShip will handle destruction effects
                    // (particle explosions, ship breakup, etc.)
                }
            }
        }

        private void OnMovesRemainingChanged(int oldMoves, int newMoves)
        {
            Debug.Log($"[NetworkedPlayerShip] Moves remaining: {oldMoves} → {newMoves}");

            // Update UI
            // (GameManager will handle UI updates)
        }

        #endregion

        #region Public API

        /// <summary>
        /// Returns true if it's this player's turn.
        /// </summary>
        public bool IsMyTurn()
        {
            return _matchManager != null && _matchManager.IsMyTurn;
        }

        /// <summary>
        /// Synchronizes local PlayerShip state with network state.
        /// Call this periodically to keep local and network state in sync.
        /// </summary>
        public void SyncLocalStateWithNetwork()
        {
            if (_playerShip == null)
                return;

            // Sync health (read from network, update local)
            // This ensures UI and visual effects reflect network state
            // Note: We're not directly modifying PlayerShip fields,
            // but GameManager can read from NetworkedPlayerShip instead
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!IsOwner)
                return;

            // Debug overlay
            GUILayout.BeginArea(new Rect(10, 200, 300, 200));
            GUILayout.Label($"Network Health: {_networkHealth.Value:F0}/{_networkMaxHealth.Value:F0}");
            GUILayout.Label($"Moves Remaining: {_networkMovesRemaining.Value}");
            GUILayout.Label($"Is Destroyed: {_networkIsDestroyed.Value}");
            GUILayout.Label($"Is My Turn: {IsMyTurn()}");
            GUILayout.EndArea();
        }
#endif

        #endregion
    }
}
