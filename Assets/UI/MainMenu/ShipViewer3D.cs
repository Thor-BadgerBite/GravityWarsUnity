using UnityEngine;
using System.Collections;

/// <summary>
/// Displays and rotates the player's currently equipped ship in 3D on the main menu.
/// Similar to Brawl Stars character viewer - showcases the ship with smooth rotation and lighting.
///
/// Features:
/// - Automatic slow rotation (idle showcase)
/// - Touch/mouse drag to manually rotate
/// - Smooth camera transitions
/// - Dynamic lighting
/// - Ship scaling to fit viewport
/// </summary>
public class ShipViewer3D : MonoBehaviour
{
    #region Inspector Configuration

    [Header("Ship Display")]
    [SerializeField] private Transform shipContainer;  // Parent transform for instantiated ships
    [SerializeField] private Vector3 shipPositionOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float shipScale = 1f;

    [Header("Camera Settings")]
    [SerializeField] private Camera shipCamera;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2f, -10f);

    [Header("Rotation Settings")]
    [SerializeField] private bool enableAutoRotation = true;
    [SerializeField] private float autoRotationSpeed = 15f;  // Degrees per second
    [SerializeField] private bool enableManualRotation = true;
    [SerializeField] private float manualRotationSpeed = 200f;

    [Header("Lighting")]
    [SerializeField] private Light mainLight;
    [SerializeField] private Light fillLight;
    [SerializeField] private Color mainLightColor = new Color(1f, 0.95f, 0.9f);
    [SerializeField] private Color fillLightColor = new Color(0.5f, 0.6f, 0.8f);

    [Header("Animation")]
    [SerializeField] private float shipEntryDuration = 0.8f;
    [SerializeField] private AnimationCurve shipEntryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    #endregion

    #region State

    private GameObject _currentShipInstance;
    private string _currentShipId;
    private float _currentRotationY = 0f;
    private bool _isDragging = false;
    private Vector3 _lastMousePosition;
    private bool _isTransitioning = false;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeCamera();
        InitializeLighting();
    }

    private void Update()
    {
        if (_currentShipInstance == null) return;

        // Handle rotation
        if (enableAutoRotation && !_isDragging && !_isTransitioning)
        {
            AutoRotateShip();
        }

        if (enableManualRotation)
        {
            HandleManualRotation();
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize camera for ship viewing.
    /// </summary>
    private void InitializeCamera()
    {
        if (shipCamera == null)
        {
            Debug.LogError("[ShipViewer3D] Ship camera not assigned!");
            return;
        }

        shipCamera.transform.position = cameraOffset;
        shipCamera.transform.LookAt(shipContainer.position);
        shipCamera.clearFlags = CameraClearFlags.SolidColor;
        shipCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // Dark blue background
    }

    /// <summary>
    /// Initialize lighting for ship showcase.
    /// </summary>
    private void InitializeLighting()
    {
        if (mainLight != null)
        {
            mainLight.type = LightType.Directional;
            mainLight.color = mainLightColor;
            mainLight.intensity = 1.2f;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        if (fillLight != null)
        {
            fillLight.type = LightType.Directional;
            fillLight.color = fillLightColor;
            fillLight.intensity = 0.5f;
            fillLight.transform.rotation = Quaternion.Euler(-20f, 120f, 0f);
        }
    }

    #endregion

    #region Ship Management

    /// <summary>
    /// Load and display a ship by its ID.
    /// </summary>
    public void DisplayShip(string shipId)
    {
        if (_currentShipId == shipId && _currentShipInstance != null)
        {
            Debug.Log($"[ShipViewer3D] Ship {shipId} already displayed");
            return;
        }

        Debug.Log($"[ShipViewer3D] Loading ship: {shipId}");

        // Clear current ship
        ClearCurrentShip();

        // Load ship prefab from Resources
        GameObject shipPrefab = LoadShipPrefab(shipId);
        if (shipPrefab == null)
        {
            Debug.LogError($"[ShipViewer3D] Failed to load ship prefab: {shipId}");
            return;
        }

        // Instantiate ship
        _currentShipInstance = Instantiate(shipPrefab, shipContainer);
        _currentShipInstance.transform.localPosition = shipPositionOffset;
        _currentShipInstance.transform.localScale = Vector3.one * shipScale;
        _currentShipInstance.transform.localRotation = Quaternion.Euler(0f, _currentRotationY, 0f);

        _currentShipId = shipId;

        // Disable physics and gameplay components (this is just for display)
        DisableGameplayComponents(_currentShipInstance);

        // Play entry animation
        StartCoroutine(PlayShipEntryAnimation());

        Debug.Log($"[ShipViewer3D] Ship {shipId} displayed successfully");
    }

    /// <summary>
    /// Load ship prefab from Resources folder.
    /// Attempts multiple paths: Ships/, Prefabs/Ships/, PlayerShips/
    /// </summary>
    private GameObject LoadShipPrefab(string shipId)
    {
        // Try multiple resource paths
        string[] possiblePaths = new string[]
        {
            $"Ships/{shipId}",
            $"Prefabs/Ships/{shipId}",
            $"PlayerShips/{shipId}",
            shipId  // Direct path
        };

        foreach (string path in possiblePaths)
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                Debug.Log($"[ShipViewer3D] Loaded ship from: Resources/{path}");
                return prefab;
            }
        }

        Debug.LogWarning($"[ShipViewer3D] Ship prefab not found in Resources: {shipId}");
        return null;
    }

    /// <summary>
    /// Disable gameplay components on display ship (physics, controls, etc).
    /// </summary>
    private void DisableGameplayComponents(GameObject shipObject)
    {
        // Disable Rigidbody
        var rb = shipObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable Colliders
        var colliders = shipObject.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Disable PlayerShip script if present
        var playerShip = shipObject.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            playerShip.enabled = false;
        }

        // Disable any audio sources
        var audioSources = shipObject.GetComponentsInChildren<AudioSource>();
        foreach (var audio in audioSources)
        {
            audio.enabled = false;
        }
    }

    /// <summary>
    /// Clear currently displayed ship.
    /// </summary>
    private void ClearCurrentShip()
    {
        if (_currentShipInstance != null)
        {
            Destroy(_currentShipInstance);
            _currentShipInstance = null;
            _currentShipId = null;
        }
    }

    /// <summary>
    /// Play ship entry animation (smooth scale-in).
    /// </summary>
    private IEnumerator PlayShipEntryAnimation()
    {
        _isTransitioning = true;

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * shipScale;
        float elapsed = 0f;

        _currentShipInstance.transform.localScale = startScale;

        while (elapsed < shipEntryDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shipEntryDuration;
            float curveValue = shipEntryCurve.Evaluate(t);

            _currentShipInstance.transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);

            yield return null;
        }

        _currentShipInstance.transform.localScale = endScale;
        _isTransitioning = false;
    }

    #endregion

    #region Rotation

    /// <summary>
    /// Auto-rotate ship slowly for showcase.
    /// </summary>
    private void AutoRotateShip()
    {
        _currentRotationY += autoRotationSpeed * Time.deltaTime;
        _currentRotationY = Mathf.Repeat(_currentRotationY, 360f);

        if (_currentShipInstance != null)
        {
            _currentShipInstance.transform.localRotation = Quaternion.Euler(0f, _currentRotationY, 0f);
        }
    }

    /// <summary>
    /// Handle manual rotation via touch/mouse drag.
    /// </summary>
    private void HandleManualRotation()
    {
        // Mouse/touch input
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - _lastMousePosition;
            float rotationDelta = delta.x * manualRotationSpeed * Time.deltaTime;

            _currentRotationY += rotationDelta;
            _currentRotationY = Mathf.Repeat(_currentRotationY, 360f);

            if (_currentShipInstance != null)
            {
                _currentShipInstance.transform.localRotation = Quaternion.Euler(0f, _currentRotationY, 0f);
            }

            _lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// Set rotation angle directly.
    /// </summary>
    public void SetRotation(float angleY)
    {
        _currentRotationY = angleY;
        if (_currentShipInstance != null)
        {
            _currentShipInstance.transform.localRotation = Quaternion.Euler(0f, _currentRotationY, 0f);
        }
    }

    /// <summary>
    /// Smoothly rotate to a specific angle.
    /// </summary>
    public void RotateTo(float targetAngle, float duration = 0.5f)
    {
        StartCoroutine(SmoothRotateTo(targetAngle, duration));
    }

    private IEnumerator SmoothRotateTo(float targetAngle, float duration)
    {
        _isTransitioning = true;

        float startAngle = _currentRotationY;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            _currentRotationY = Mathf.LerpAngle(startAngle, targetAngle, t);

            if (_currentShipInstance != null)
            {
                _currentShipInstance.transform.localRotation = Quaternion.Euler(0f, _currentRotationY, 0f);
            }

            yield return null;
        }

        _currentRotationY = targetAngle;
        _isTransitioning = false;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get currently displayed ship ID.
    /// </summary>
    public string GetCurrentShipId()
    {
        return _currentShipId;
    }

    /// <summary>
    /// Enable/disable auto rotation.
    /// </summary>
    public void SetAutoRotation(bool enabled)
    {
        enableAutoRotation = enabled;
    }

    /// <summary>
    /// Set auto rotation speed.
    /// </summary>
    public void SetAutoRotationSpeed(float speed)
    {
        autoRotationSpeed = speed;
    }

    /// <summary>
    /// Play a celebratory spin animation (for unlocking new ships).
    /// </summary>
    public void PlayCelebrationSpin()
    {
        StartCoroutine(CelebrationSpinAnimation());
    }

    private IEnumerator CelebrationSpinAnimation()
    {
        _isTransitioning = true;

        float spinDuration = 1.5f;
        float elapsed = 0f;
        float startAngle = _currentRotationY;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;

            _currentRotationY = startAngle + (720f * t); // Two full rotations

            if (_currentShipInstance != null)
            {
                // Add slight bounce
                float bounce = Mathf.Sin(t * Mathf.PI) * 0.3f;
                Vector3 pos = shipPositionOffset + Vector3.up * bounce;
                _currentShipInstance.transform.localPosition = pos;
                _currentShipInstance.transform.localRotation = Quaternion.Euler(0f, _currentRotationY, 0f);
            }

            yield return null;
        }

        // Reset position
        if (_currentShipInstance != null)
        {
            _currentShipInstance.transform.localPosition = shipPositionOffset;
        }

        _isTransitioning = false;
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        if (shipContainer == null) return;

        // Draw ship container position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shipContainer.position, 0.5f);

        // Draw camera view direction
        if (shipCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(shipCamera.transform.position, shipContainer.position);
        }
    }

    #endregion
}
