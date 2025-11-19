using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GravityWars.Networking.UI
{
    /// <summary>
    /// UI controller for network connection status display.
    ///
    /// Shows:
    /// - Connection state (Connected/Disconnected/Connecting)
    /// - Network latency (ping in ms)
    /// - Player count
    /// - Connection quality indicator (Good/Fair/Poor)
    ///
    /// This is displayed during online matches to give players
    /// real-time feedback about their connection.
    ///
    /// Usage:
    ///   Attach to a Canvas in the game scene.
    ///   Position in corner of screen (non-intrusive).
    /// </summary>
    public class ConnectionStatusUI : MonoBehaviour
    {
        #region UI References

        [Header("Main Panel")]
        public GameObject statusPanel;

        [Header("Status Display")]
        public TextMeshProUGUI connectionStateText;
        public TextMeshProUGUI latencyText;
        public Image connectionQualityIndicator;

        [Header("Quality Colors")]
        public Color goodConnectionColor = Color.green;
        public Color fairConnectionColor = Color.yellow;
        public Color poorConnectionColor = Color.red;
        public Color disconnectedColor = Color.gray;

        [Header("Update Settings")]
        [Tooltip("How often to update UI (seconds)")]
        public float updateInterval = 0.5f;

        #endregion

        #region State

        private float lastUpdateTime;
        private NetworkService _networkService;
        private GravityWarsNetworkManager _networkManager;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Get service references
            _networkService = ServiceLocator.Instance?.Network;
            _networkManager = GravityWarsNetworkManager.Instance;

            // Start with panel hidden (show only in online matches)
            if (statusPanel != null)
                statusPanel.SetActive(false);

            lastUpdateTime = Time.time;
        }

        private void Update()
        {
            // Update UI at regular intervals
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateUI();
                lastUpdateTime = Time.time;
            }
        }

        #endregion

        #region UI Update

        /// <summary>
        /// Updates all UI elements with current network state.
        /// </summary>
        private void UpdateUI()
        {
            if (_networkService == null || _networkManager == null)
                return;

            // Update connection state
            UpdateConnectionState();

            // Update latency
            UpdateLatency();

            // Update connection quality indicator
            UpdateConnectionQuality();
        }

        /// <summary>
        /// Updates the connection state text.
        /// </summary>
        private void UpdateConnectionState()
        {
            if (connectionStateText == null)
                return;

            var state = _networkService.CurrentState;

            string stateText = state switch
            {
                NetworkService.NetworkState.Connected => "Connected",
                NetworkService.NetworkState.Hosting => "Hosting",
                NetworkService.NetworkState.Connecting => "Connecting...",
                NetworkService.NetworkState.Disconnecting => "Disconnecting...",
                NetworkService.NetworkState.Disconnected => "Disconnected",
                _ => "Unknown"
            };

            connectionStateText.text = stateText;

            // Color code based on state
            connectionStateText.color = state == NetworkService.NetworkState.Connected ||
                                       state == NetworkService.NetworkState.Hosting
                ? goodConnectionColor
                : disconnectedColor;
        }

        /// <summary>
        /// Updates the latency (ping) display.
        /// </summary>
        private void UpdateLatency()
        {
            if (latencyText == null)
                return;

            if (!_networkService.IsConnected)
            {
                latencyText.text = "Ping: --";
                return;
            }

            float rtt = _networkService.GetRTT();
            latencyText.text = $"Ping: {rtt:F0}ms";

            // Color code based on latency
            if (rtt < 50f)
                latencyText.color = goodConnectionColor;
            else if (rtt < 100f)
                latencyText.color = fairConnectionColor;
            else
                latencyText.color = poorConnectionColor;
        }

        /// <summary>
        /// Updates the connection quality indicator (colored dot/icon).
        /// </summary>
        private void UpdateConnectionQuality()
        {
            if (connectionQualityIndicator == null)
                return;

            if (!_networkService.IsConnected)
            {
                connectionQualityIndicator.color = disconnectedColor;
                return;
            }

            float rtt = _networkService.GetRTT();

            // Set color based on latency
            if (rtt < 50f)
                connectionQualityIndicator.color = goodConnectionColor; // Good (green)
            else if (rtt < 100f)
                connectionQualityIndicator.color = fairConnectionColor; // Fair (yellow)
            else if (rtt < 150f)
                connectionQualityIndicator.color = poorConnectionColor; // Poor (orange/red)
            else
                connectionQualityIndicator.color = disconnectedColor; // Very poor (gray)
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the connection status UI.
        /// Call this when starting an online match.
        /// </summary>
        public void Show()
        {
            if (statusPanel != null)
            {
                statusPanel.SetActive(true);
                UpdateUI(); // Immediate update
            }

            Debug.Log("[ConnectionStatusUI] Shown");
        }

        /// <summary>
        /// Hides the connection status UI.
        /// Call this when returning to menu or offline mode.
        /// </summary>
        public void Hide()
        {
            if (statusPanel != null)
                statusPanel.SetActive(false);

            Debug.Log("[ConnectionStatusUI] Hidden");
        }

        /// <summary>
        /// Returns true if connection quality is good (latency < 150ms).
        /// GameManager can use this to warn players about poor connection.
        /// </summary>
        public bool IsConnectionQualityGood()
        {
            if (_networkService == null || !_networkService.IsConnected)
                return false;

            return _networkService.IsLatencyGood();
        }

        /// <summary>
        /// Gets current latency in milliseconds.
        /// </summary>
        public float GetCurrentLatency()
        {
            if (_networkService == null)
                return 0f;

            return _networkService.GetRTT();
        }

        #endregion

        #region Disconnect Warning

        /// <summary>
        /// Shows a warning if connection quality degrades during match.
        /// </summary>
        public void ShowConnectionWarning(string message)
        {
            Debug.LogWarning($"[ConnectionStatusUI] {message}");

            // Show warning popup (implement your UI warning system here)
            // Example: UIManager.Instance.ShowWarning(message);
        }

        /// <summary>
        /// Monitors connection quality and shows warnings if it degrades.
        /// Call this periodically during online matches.
        /// </summary>
        public void MonitorConnectionQuality()
        {
            if (_networkService == null || !_networkService.IsConnected)
                return;

            float rtt = _networkService.GetRTT();

            if (rtt > 200f)
            {
                ShowConnectionWarning("Poor connection! High latency detected.");
            }
        }

        #endregion
    }
}
