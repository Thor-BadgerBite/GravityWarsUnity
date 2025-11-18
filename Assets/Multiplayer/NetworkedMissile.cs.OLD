using Unity.Netcode;
using UnityEngine;
using System.Collections;

namespace GravityWars.Multiplayer
{
    /// <summary>
    /// Networked missile with server-authoritative physics and collision detection.
    ///
    /// Features:
    /// - Server-authoritative position and velocity
    /// - Client-side interpolation for smooth visuals
    /// - Server-side collision detection (anti-cheat)
    /// - Networked damage application
    /// - Lifetime management
    /// - Trail and explosion effects
    ///
    /// Architecture:
    /// - Server simulates physics and detects collisions
    /// - Server applies damage to hit targets
    /// - Clients receive updates and display visual effects
    /// - Missile despawns after lifetime or collision
    ///
    /// Usage:
    /// - Attach to Missile prefab
    /// - Mark prefab as NetworkObject
    /// - Add to NetworkManager's spawnable prefabs list
    /// - Spawn from NetworkedPlayerShip.FireMissile()
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class NetworkedMissile : NetworkBehaviour
    {
        #region Serialized Fields

        [Header("Missile Configuration")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Visual Effects")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem thrustParticles;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private SpriteRenderer missileSprite;

        [Header("Audio")]
        [SerializeField] private AudioClip launchSound;
        [SerializeField] private AudioClip explosionSound;

        #endregion

        #region Network Variables

        /// <summary>
        /// Server-authoritative position.
        /// </summary>
        private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
            Vector2.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Server-authoritative velocity.
        /// </summary>
        private NetworkVariable<Vector2> _networkVelocity = new NetworkVariable<Vector2>(
            Vector2.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Client ID of the player who fired this missile (for kill attribution).
        /// </summary>
        private NetworkVariable<ulong> _ownerPlayerId = new NetworkVariable<ulong>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        #endregion

        #region Private State

        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private float _spawnTime;
        private bool _hasExploded = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _spawnTime = Time.time;

            // Initialize network variables
            if (IsServer)
            {
                _networkPosition.Value = transform.position;
                _networkVelocity.Value = _rigidbody.velocity;
            }

            // Start lifetime coroutine on server
            if (IsServer)
            {
                StartCoroutine(LifetimeCoroutine());
            }

            // Play launch effects on all clients
            PlayLaunchEffects();

            Debug.Log($"[NetworkedMissile] Spawned - Owner: {_ownerPlayerId.Value}, IsServer: {IsServer}");
        }

        private void Update()
        {
            if (!IsSpawned || _hasExploded) return;

            // Clients interpolate to server position
            if (IsClient && !IsServer)
            {
                InterpolateTransform();
            }
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || _hasExploded) return;

            if (IsServer)
            {
                // Server updates physics
                ServerPhysicsUpdate();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return; // Server-authoritative collision detection
            if (_hasExploded) return;

            // Check if we hit something on our hit layers
            if (((1 << other.gameObject.layer) & hitLayers) != 0)
            {
                HandleCollision(other);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Server-only: Initialize missile with owner and velocity.
        /// Called by NetworkedPlayerShip after spawning.
        /// </summary>
        public void Initialize(ulong ownerClientId, Vector2 initialVelocity)
        {
            if (!IsServer) return;

            _ownerPlayerId.Value = ownerClientId;
            _networkVelocity.Value = initialVelocity;
            _rigidbody.velocity = initialVelocity;

            // Point missile in direction of travel
            if (initialVelocity != Vector2.zero)
            {
                float angle = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        #endregion

        #region Server Physics

        private void ServerPhysicsUpdate()
        {
            // Apply gravity - Use custom Planet-based gravitational system (EXACT same as local hotseat mode)
            Planet[] planets = GameManager.GetCachedPlanets();
            if (planets != null && planets.Length > 0)
            {
                Vector2 totalForce = Vector2.zero;
                foreach (Planet planet in planets)
                {
                    if (planet != null)
                    {
                        totalForce += (Vector2)planet.CalculateGravitationalPull(transform.position, _rigidbody.mass);
                    }
                }
                _rigidbody.AddForce(totalForce, ForceMode2D.Force);
            }

            // Update rotation to face direction of travel
            if (_rigidbody.velocity != Vector2.zero)
            {
                float angle = Mathf.Atan2(_rigidbody.velocity.y, _rigidbody.velocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Update network variables
            _networkPosition.Value = transform.position;
            _networkVelocity.Value = _rigidbody.velocity;
        }

        #endregion

        #region Client Interpolation

        private void InterpolateTransform()
        {
            // Smooth interpolation for clients
            float interpolationSpeed = 20f;
            transform.position = Vector2.Lerp(transform.position, _networkPosition.Value, Time.deltaTime * interpolationSpeed);

            // Point toward velocity
            if (_networkVelocity.Value != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(_networkVelocity.Value.y, _networkVelocity.Value.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.rotation.eulerAngles.z;
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * interpolationSpeed);
                transform.rotation = Quaternion.Euler(0, 0, newAngle);
            }
        }

        #endregion

        #region Collision & Damage

        private void HandleCollision(Collider2D hitCollider)
        {
            if (!IsServer) return;
            if (_hasExploded) return;

            // Try to damage the hit object
            NetworkedPlayerShip hitShip = hitCollider.GetComponent<NetworkedPlayerShip>();
            if (hitShip != null)
            {
                // Don't hit own ship
                if (hitShip.OwnerClientId == _ownerPlayerId.Value)
                    return;

                // Apply damage
                hitShip.TakeDamage(damage, _ownerPlayerId.Value);

                Debug.Log($"[NetworkedMissile] Hit ship {hitShip.OwnerClientId}, dealt {damage} damage");
            }

            // Check for area damage (explosion radius)
            if (explosionRadius > 0)
            {
                ApplyAreaDamage();
            }

            // Explode
            Explode();
        }

        private void ApplyAreaDamage()
        {
            if (!IsServer) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitLayers);

            foreach (Collider2D hit in hits)
            {
                NetworkedPlayerShip ship = hit.GetComponent<NetworkedPlayerShip>();
                if (ship != null && ship.OwnerClientId != _ownerPlayerId.Value)
                {
                    // Apply falloff damage based on distance
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    float damageFalloff = 1f - (distance / explosionRadius);
                    float areaDamage = damage * 0.5f * damageFalloff; // 50% of direct damage, with falloff

                    ship.TakeDamage(areaDamage, _ownerPlayerId.Value);

                    Debug.Log($"[NetworkedMissile] Area damage to ship {ship.OwnerClientId}: {areaDamage}");
                }
            }
        }

        private void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;

            // Notify all clients to play explosion
            ExplodeClientRpc(transform.position);

            // Despawn after short delay (let explosion play)
            if (IsServer)
            {
                StartCoroutine(DespawnAfterDelay(0.1f));
            }
        }

        [ClientRpc]
        private void ExplodeClientRpc(Vector2 explosionPosition)
        {
            PlayExplosionEffects(explosionPosition);
        }

        #endregion

        #region Lifetime Management

        private IEnumerator LifetimeCoroutine()
        {
            yield return new WaitForSeconds(lifetime);

            if (!_hasExploded)
            {
                Debug.Log($"[NetworkedMissile] Lifetime expired, despawning");
                Explode();
            }
        }

        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (IsServer && IsSpawned)
            {
                NetworkObject.Despawn(destroy: true);
            }
        }

        #endregion

        #region Visual & Audio Effects

        private void PlayLaunchEffects()
        {
            // Start thrust particles
            if (thrustParticles != null)
            {
                thrustParticles.Play();
            }

            // Enable trail
            if (trail != null)
            {
                trail.emitting = true;
            }

            // Play launch sound
            if (launchSound != null)
            {
                // AudioManager.Instance.PlaySFX(launchSound, transform.position);
            }
        }

        private void PlayExplosionEffects(Vector2 position)
        {
            // Stop thrust effects
            if (thrustParticles != null)
            {
                thrustParticles.Stop();
            }

            // Stop trail
            if (trail != null)
            {
                trail.emitting = false;
            }

            // Hide missile sprite
            if (missileSprite != null)
            {
                missileSprite.enabled = false;
            }

            // Spawn explosion prefab
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
                Destroy(explosion, 2f); // Cleanup after animation
            }

            // Play explosion sound
            if (explosionSound != null)
            {
                // AudioManager.Instance.PlaySFX(explosionSound, position);
            }

            // Screen shake for nearby players
            // CameraShake.Instance.ShakeAtPosition(position, explosionRadius, 0.3f);
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw explosion radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);

            // Draw velocity
            if (_rigidbody != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)_rigidbody.velocity * 0.5f);
            }

            // Draw server position (if client)
            if (IsClient && !IsServer && IsSpawned)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_networkPosition.Value, 0.2f);
            }
        }

        #endregion
    }
}
