using Unity.Netcode;
using UnityEngine;
using System;

namespace GravityWars.Multiplayer
{
    /// <summary>
    /// Networked player ship with server-authoritative movement and combat.
    ///
    /// Features:
    /// - Server-authoritative position, rotation, velocity
    /// - Client prediction for local player (smooth movement)
    /// - Server reconciliation (correct client mispredictions)
    /// - Networked health and damage
    /// - Networked missile firing
    /// - Network ownership validation
    ///
    /// Architecture:
    /// - Server is authoritative for all game state
    /// - Clients send input commands to server
    /// - Server processes input and updates NetworkVariables
    /// - Clients interpolate NetworkVariables for smooth visuals
    ///
    /// Usage:
    /// - Attach to PlayerShip prefab
    /// - Mark prefab as NetworkObject
    /// - Add to NetworkManager's spawnable prefabs list
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkedPlayerShip : NetworkBehaviour
    {
        #region Serialized Fields

        [Header("Ship Configuration")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float gravityStrength = 10f;

        [Header("Combat Configuration")]
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private Transform missileSpawnPoint;
        [SerializeField] private float fireCooldown = 0.5f;
        [SerializeField] private int maxMissiles = 10;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer shipSprite;
        [SerializeField] private ParticleSystem thrustParticles;
        [SerializeField] private ParticleSystem damageParticles;

        #endregion

        #region Network Variables

        /// <summary>
        /// Server-authoritative position. Clients interpolate to this value.
        /// </summary>
        private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
            Vector2.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Server-authoritative rotation. Clients interpolate to this value.
        /// </summary>
        private NetworkVariable<float> _networkRotation = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Server-authoritative velocity for physics prediction.
        /// </summary>
        private NetworkVariable<Vector2> _networkVelocity = new NetworkVariable<Vector2>(
            Vector2.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Server-authoritative health. Damage is validated server-side.
        /// </summary>
        private NetworkVariable<float> _networkHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Missiles remaining. Server tracks to prevent cheating.
        /// </summary>
        private NetworkVariable<int> _networkMissilesRemaining = new NetworkVariable<int>(
            10,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Whether ship is thrusting (for visual effects).
        /// </summary>
        private NetworkVariable<bool> _networkIsThrusting = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        #endregion

        #region Events

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int> OnMissilesChanged;
        public event Action OnMissileFired;

        #endregion

        #region Private State

        private Rigidbody2D _rigidbody;
        private float _lastFireTime = 0f;
        private bool _isDead = false;

        // Client prediction
        private Vector2 _predictedPosition;
        private float _predictedRotation;
        private Vector2 _predictedVelocity;

        // Input buffering
        private struct PlayerInput
        {
            public float moveInput;
            public float rotateInput;
            public bool fireInput;
            public uint tick;
        }
        private PlayerInput _currentInput;
        private uint _clientTick = 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Initialize network variables
            if (IsServer)
            {
                _networkPosition.Value = transform.position;
                _networkRotation.Value = transform.rotation.eulerAngles.z;
                _networkVelocity.Value = _rigidbody.velocity;
                _networkHealth.Value = maxHealth;
                _networkMissilesRemaining.Value = maxMissiles;
            }

            // Subscribe to network variable changes
            _networkHealth.OnValueChanged += OnHealthChangedCallback;
            _networkMissilesRemaining.OnValueChanged += OnMissilesChangedCallback;

            // Initialize predicted state for client
            if (IsClient && IsOwner)
            {
                _predictedPosition = _networkPosition.Value;
                _predictedRotation = _networkRotation.Value;
                _predictedVelocity = _networkVelocity.Value;
            }

            Debug.Log($"[NetworkedShip] Spawned - IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _networkHealth.OnValueChanged -= OnHealthChangedCallback;
            _networkMissilesRemaining.OnValueChanged -= OnMissilesChangedCallback;
        }

        private void Update()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                // Owner handles input and prediction
                HandleInput();

                if (IsClient && !IsServer)
                {
                    // Client-side prediction for local player
                    ClientPrediction();
                }
            }

            // All clients interpolate remote players
            if (IsClient && !IsOwner)
            {
                InterpolateTransform();
            }

            // Update visual effects
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            if (!IsSpawned) return;

            if (IsServer)
            {
                // Server processes physics
                ServerPhysicsUpdate();
            }
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            if (_isDead) return;

            // Gather input
            float moveInput = 0f;
            float rotateInput = 0f;
            bool fireInput = false;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveInput = 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                rotateInput = 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                rotateInput = -1f;
            if (Input.GetKeyDown(KeyCode.Space))
                fireInput = true;

            // Store input for this tick
            _currentInput = new PlayerInput
            {
                moveInput = moveInput,
                rotateInput = rotateInput,
                fireInput = fireInput,
                tick = _clientTick++
            };

            // Update thrusting state (owner can write to this NetworkVariable)
            _networkIsThrusting.Value = moveInput > 0f;

            // Send input to server
            if (!IsServer)
            {
                SendInputToServerRpc(_currentInput.moveInput, _currentInput.rotateInput, _currentInput.fireInput, _currentInput.tick);
            }
            else
            {
                // If we're host, process input directly
                ProcessInput(_currentInput.moveInput, _currentInput.rotateInput, _currentInput.fireInput);
            }
        }

        [ServerRpc]
        private void SendInputToServerRpc(float moveInput, float rotateInput, bool fireInput, uint tick)
        {
            // Server validates and processes input
            ProcessInput(moveInput, rotateInput, fireInput);
        }

        private void ProcessInput(float moveInput, float rotateInput, bool fireInput)
        {
            if (_isDead) return;

            // Process movement (will be applied in FixedUpdate on server)
            _currentInput.moveInput = moveInput;
            _currentInput.rotateInput = rotateInput;

            // Process firing
            if (fireInput)
            {
                TryFireMissile();
            }
        }

        #endregion

        #region Server Physics

        private void ServerPhysicsUpdate()
        {
            if (_isDead) return;

            // Apply rotation
            float rotationDelta = _currentInput.rotateInput * rotationSpeed * Time.fixedDeltaTime;
            _rigidbody.rotation += rotationDelta;

            // Apply thrust
            if (_currentInput.moveInput > 0f)
            {
                Vector2 thrustDirection = transform.up;
                _rigidbody.AddForce(thrustDirection * moveSpeed * _currentInput.moveInput, ForceMode2D.Force);
            }

            // Apply gravity (toward center of arena, for example)
            Vector2 gravityDirection = (Vector2.zero - (Vector2)transform.position).normalized;
            _rigidbody.AddForce(gravityDirection * gravityStrength, ForceMode2D.Force);

            // Limit velocity
            if (_rigidbody.velocity.magnitude > moveSpeed * 2f)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * moveSpeed * 2f;
            }

            // Update network variables
            _networkPosition.Value = transform.position;
            _networkRotation.Value = transform.rotation.eulerAngles.z;
            _networkVelocity.Value = _rigidbody.velocity;
        }

        #endregion

        #region Client Prediction

        private void ClientPrediction()
        {
            // Predict movement based on local input
            float rotationDelta = _currentInput.rotateInput * rotationSpeed * Time.deltaTime;
            _predictedRotation += rotationDelta;

            if (_currentInput.moveInput > 0f)
            {
                Vector2 thrustDirection = Quaternion.Euler(0, 0, _predictedRotation) * Vector2.up;
                _predictedVelocity += thrustDirection * moveSpeed * _currentInput.moveInput * Time.deltaTime;
            }

            // Apply gravity
            Vector2 gravityDirection = (Vector2.zero - _predictedPosition).normalized;
            _predictedVelocity += gravityDirection * gravityStrength * Time.deltaTime;

            // Limit velocity
            if (_predictedVelocity.magnitude > moveSpeed * 2f)
            {
                _predictedVelocity = _predictedVelocity.normalized * moveSpeed * 2f;
            }

            _predictedPosition += _predictedVelocity * Time.deltaTime;

            // Reconciliation: blend toward server state
            float reconciliationSpeed = 10f;
            _predictedPosition = Vector2.Lerp(_predictedPosition, _networkPosition.Value, Time.deltaTime * reconciliationSpeed);
            _predictedRotation = Mathf.LerpAngle(_predictedRotation, _networkRotation.Value, Time.deltaTime * reconciliationSpeed);
            _predictedVelocity = Vector2.Lerp(_predictedVelocity, _networkVelocity.Value, Time.deltaTime * reconciliationSpeed);

            // Apply predicted transform
            transform.position = _predictedPosition;
            transform.rotation = Quaternion.Euler(0, 0, _predictedRotation);
        }

        #endregion

        #region Remote Interpolation

        private void InterpolateTransform()
        {
            // Smooth interpolation for remote players
            float interpolationSpeed = 15f;
            transform.position = Vector2.Lerp(transform.position, _networkPosition.Value, Time.deltaTime * interpolationSpeed);

            float targetRotation = _networkRotation.Value;
            float currentRotation = transform.rotation.eulerAngles.z;
            float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * interpolationSpeed);
            transform.rotation = Quaternion.Euler(0, 0, newRotation);
        }

        #endregion

        #region Combat

        private void TryFireMissile()
        {
            if (!IsServer) return; // Server-authoritative
            if (_isDead) return;

            // Check cooldown
            if (Time.time - _lastFireTime < fireCooldown)
                return;

            // Check missiles remaining
            if (_networkMissilesRemaining.Value <= 0)
                return;

            // Fire missile
            _lastFireTime = Time.time;
            _networkMissilesRemaining.Value--;

            // Spawn missile
            if (missilePrefab != null && missileSpawnPoint != null)
            {
                Vector3 spawnPos = missileSpawnPoint.position;
                Quaternion spawnRot = missileSpawnPoint.rotation;

                GameObject missileObj = Instantiate(missilePrefab, spawnPos, spawnRot);
                NetworkObject missileNetObj = missileObj.GetComponent<NetworkObject>();

                if (missileNetObj != null)
                {
                    missileNetObj.SpawnWithOwnership(OwnerClientId);

                    // Set missile velocity
                    NetworkedMissile missile = missileObj.GetComponent<NetworkedMissile>();
                    if (missile != null)
                    {
                        missile.Initialize(OwnerClientId, transform.up * 15f + (Vector3)_rigidbody.velocity);
                    }
                }
            }

            // Notify clients
            FireMissileClientRpc();
        }

        [ClientRpc]
        private void FireMissileClientRpc()
        {
            OnMissileFired?.Invoke();

            // Play fire sound/effects
            // AudioManager.Instance.PlaySFX("MissileFire");
        }

        /// <summary>
        /// Server-authoritative damage application.
        /// Called by NetworkedMissile when it hits this ship.
        /// </summary>
        public void TakeDamage(float damage, ulong attackerClientId)
        {
            if (!IsServer) return; // Server-authoritative
            if (_isDead) return;

            _networkHealth.Value -= damage;
            _networkHealth.Value = Mathf.Max(0, _networkHealth.Value);

            // Notify clients of damage
            TakeDamageClientRpc(damage);

            // Check for death
            if (_networkHealth.Value <= 0)
            {
                Die(attackerClientId);
            }
        }

        [ClientRpc]
        private void TakeDamageClientRpc(float damage)
        {
            // Play damage effects
            if (damageParticles != null)
            {
                damageParticles.Play();
            }

            // Screen shake for owner
            if (IsOwner)
            {
                // CameraShake.Instance.Shake(0.2f, 0.3f);
            }
        }

        private void Die(ulong killerClientId)
        {
            if (!IsServer) return;
            if (_isDead) return;

            _isDead = true;

            // Notify clients
            DieClientRpc(killerClientId);

            // Server handles respawn logic
            // NetworkGameManager.Instance.OnPlayerDeath(OwnerClientId, killerClientId);
        }

        [ClientRpc]
        private void DieClientRpc(ulong killerClientId)
        {
            OnDeath?.Invoke();

            // Play death effects
            // ParticleManager.Instance.PlayExplosion(transform.position);
            // AudioManager.Instance.PlaySFX("ShipExplosion");

            // Hide ship visually
            if (shipSprite != null)
            {
                shipSprite.enabled = false;
            }
        }

        #endregion

        #region Network Variable Callbacks

        private void OnHealthChangedCallback(float previousValue, float newValue)
        {
            OnHealthChanged?.Invoke(newValue);
        }

        private void OnMissilesChangedCallback(int previousValue, int newValue)
        {
            OnMissilesChanged?.Invoke(newValue);
        }

        #endregion

        #region Visuals

        private void UpdateVisuals()
        {
            // Update thrust particles
            if (thrustParticles != null)
            {
                if (_networkIsThrusting.Value && !_isDead)
                {
                    if (!thrustParticles.isPlaying)
                        thrustParticles.Play();
                }
                else
                {
                    if (thrustParticles.isPlaying)
                        thrustParticles.Stop();
                }
            }

            // Update ship color based on health
            if (shipSprite != null && !_isDead)
            {
                float healthPercent = _networkHealth.Value / maxHealth;
                shipSprite.color = Color.Lerp(Color.red, Color.white, healthPercent);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Server-only: Respawn the ship at a new position.
        /// </summary>
        public void Respawn(Vector2 position, float rotation)
        {
            if (!IsServer) return;

            _isDead = false;
            _networkHealth.Value = maxHealth;
            _networkMissilesRemaining.Value = maxMissiles;
            _networkPosition.Value = position;
            _networkRotation.Value = rotation;

            transform.position = position;
            transform.rotation = Quaternion.Euler(0, 0, rotation);
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;

            RespawnClientRpc(position, rotation);
        }

        [ClientRpc]
        private void RespawnClientRpc(Vector2 position, float rotation)
        {
            if (shipSprite != null)
            {
                shipSprite.enabled = true;
            }

            transform.position = position;
            transform.rotation = Quaternion.Euler(0, 0, rotation);

            if (IsOwner)
            {
                _predictedPosition = position;
                _predictedRotation = rotation;
                _predictedVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Gets current health (read from NetworkVariable).
        /// </summary>
        public float GetHealth()
        {
            return _networkHealth.Value;
        }

        /// <summary>
        /// Gets missiles remaining (read from NetworkVariable).
        /// </summary>
        public int GetMissilesRemaining()
        {
            return _networkMissilesRemaining.Value;
        }

        /// <summary>
        /// Checks if ship is dead.
        /// </summary>
        public bool IsDead()
        {
            return _isDead;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (!IsSpawned) return;

            // Draw velocity
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_rigidbody.velocity);

            // Draw predicted position (for owner client)
            if (IsClient && IsOwner && !IsServer)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_predictedPosition, 0.5f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_networkPosition.Value, 0.6f);
            }
        }

        #endregion
    }
}
