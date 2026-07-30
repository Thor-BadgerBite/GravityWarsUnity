using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings screen: audio, graphics, controls and account options.
/// Settings are stored in PlayerAccountData.preferences (cloud-synced when
/// online) and mirrored to PlayerPrefs so they apply before any profile loads.
///
/// Setup:
/// - Place on a (initially inactive) settings panel.
/// - Wire the sliders/toggles/dropdown in the Inspector.
/// - Call Show() / Hide() from the main menu.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    // PlayerPrefs keys (mirror of PlayerPreferences for pre-login boot)
    private const string KEY_MASTER = "settings_master_volume";
    private const string KEY_MUSIC = "settings_music_volume";
    private const string KEY_SFX = "settings_sfx_volume";
    private const string KEY_QUALITY = "settings_quality";
    private const string KEY_VSYNC = "settings_vsync";
    private const string KEY_FULLSCREEN = "settings_fullscreen";
    private const string KEY_SENSITIVITY = "settings_sensitivity";
    private const string KEY_INVERT_Y = "settings_invert_y";

    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Controls")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle invertYToggle;

    [Header("Account")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private Button logoutButton;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetDefaultsButton;

    private bool _suppressCallbacks;

    private void Awake()
    {
        Instance = this;

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(_ => OnSettingChanged());
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(_ => OnSettingChanged());
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(_ => OnSettingChanged());
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(_ => OnSettingChanged());
        if (vsyncToggle != null) vsyncToggle.onValueChanged.AddListener(_ => OnSettingChanged());
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(_ => OnSettingChanged());
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(_ => OnSettingChanged());
        if (invertYToggle != null) invertYToggle.onValueChanged.AddListener(_ => OnSettingChanged());

        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (resetDefaultsButton != null) resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);

        PopulateQualityDropdown();
    }

    private void Start()
    {
        // Apply persisted settings on boot even if the panel is never opened
        ApplySettings(LoadPreferences());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #region Show / Hide

    public void Show()
    {
        var prefs = LoadPreferences();
        RefreshUI(prefs);

        if (usernameText != null)
        {
            var profile = GetProfile();
            usernameText.text = profile != null ? profile.username : "Guest";
        }

        if (settingsPanel != null) settingsPanel.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        SavePreferences();

        if (settingsPanel != null) settingsPanel.SetActive(false);
        else gameObject.SetActive(false);
    }

    #endregion

    #region Load / Save

    private PlayerAccountData GetProfile()
    {
        return ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
    }

    /// <summary>
    /// Loads preferences from the player profile, falling back to PlayerPrefs.
    /// </summary>
    private PlayerPreferences LoadPreferences()
    {
        var profile = GetProfile();
        if (profile != null && profile.preferences != null)
            return profile.preferences;

        // No profile yet - use the PlayerPrefs mirror
        var prefs = new PlayerPreferences
        {
            masterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f),
            musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 0.7f),
            sfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 0.8f),
            qualityLevel = PlayerPrefs.GetInt(KEY_QUALITY, QualitySettings.GetQualityLevel()),
            vSync = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1,
            fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1,
            mouseSensitivity = PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1.0f),
            invertYAxis = PlayerPrefs.GetInt(KEY_INVERT_Y, 0) == 1
        };
        return prefs;
    }

    /// <summary>
    /// Reads the UI controls, applies the settings and persists them.
    /// </summary>
    private void SavePreferences()
    {
        var prefs = ReadUIIntoPreferences();
        ApplySettings(prefs);

        // Mirror to PlayerPrefs (pre-login boot)
        PlayerPrefs.SetFloat(KEY_MASTER, prefs.masterVolume);
        PlayerPrefs.SetFloat(KEY_MUSIC, prefs.musicVolume);
        PlayerPrefs.SetFloat(KEY_SFX, prefs.sfxVolume);
        PlayerPrefs.SetInt(KEY_QUALITY, prefs.qualityLevel);
        PlayerPrefs.SetInt(KEY_VSYNC, prefs.vSync ? 1 : 0);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, prefs.fullscreen ? 1 : 0);
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, prefs.mouseSensitivity);
        PlayerPrefs.SetInt(KEY_INVERT_Y, prefs.invertYAxis ? 1 : 0);
        PlayerPrefs.Save();

        // Persist on the profile (cloud-synced when online)
        var profile = GetProfile();
        if (profile != null)
        {
            profile.preferences = prefs;
            ProgressionManager.Instance.Save();
        }
    }

    private PlayerPreferences ReadUIIntoPreferences()
    {
        var current = LoadPreferences();

        if (masterVolumeSlider != null) current.masterVolume = masterVolumeSlider.value;
        if (musicVolumeSlider != null) current.musicVolume = musicVolumeSlider.value;
        if (sfxVolumeSlider != null) current.sfxVolume = sfxVolumeSlider.value;
        if (qualityDropdown != null) current.qualityLevel = qualityDropdown.value;
        if (vsyncToggle != null) current.vSync = vsyncToggle.isOn;
        if (fullscreenToggle != null) current.fullscreen = fullscreenToggle.isOn;
        if (sensitivitySlider != null) current.mouseSensitivity = sensitivitySlider.value;
        if (invertYToggle != null) current.invertYAxis = invertYToggle.isOn;

        return current;
    }

    private void RefreshUI(PlayerPreferences prefs)
    {
        _suppressCallbacks = true;

        if (masterVolumeSlider != null) masterVolumeSlider.value = prefs.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = prefs.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = prefs.sfxVolume;
        if (qualityDropdown != null) qualityDropdown.value = Mathf.Clamp(prefs.qualityLevel, 0, qualityDropdown.options.Count - 1);
        if (vsyncToggle != null) vsyncToggle.isOn = prefs.vSync;
        if (fullscreenToggle != null) fullscreenToggle.isOn = prefs.fullscreen;
        if (sensitivitySlider != null) sensitivitySlider.value = prefs.mouseSensitivity;
        if (invertYToggle != null) invertYToggle.isOn = prefs.invertYAxis;

        _suppressCallbacks = false;
    }

    #endregion

    #region Apply

    /// <summary>
    /// Applies settings to the engine and audio systems.
    /// </summary>
    private void ApplySettings(PlayerPreferences prefs)
    {
        // Audio
        AudioListener.volume = prefs.masterVolume;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicVolume = prefs.musicVolume;
            AudioManager.Instance.sfxVolume = prefs.sfxVolume;
        }

        // Graphics
        QualitySettings.SetQualityLevel(Mathf.Clamp(prefs.qualityLevel, 0, QualitySettings.names.Length - 1));
        QualitySettings.vSyncCount = prefs.vSync ? 1 : 0;
        if (Screen.fullScreen != prefs.fullscreen)
            Screen.fullScreen = prefs.fullscreen;
        Application.targetFrameRate = prefs.targetFrameRate > 0 ? prefs.targetFrameRate : 60;
    }

    private void OnSettingChanged()
    {
        if (_suppressCallbacks) return;

        // Live-apply while dragging sliders; final save happens on Hide()
        ApplySettings(ReadUIIntoPreferences());
    }

    #endregion

    #region Actions

    private void PopulateQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);
    }

    private void ResetToDefaults()
    {
        RefreshUI(new PlayerPreferences());
        OnSettingChanged();
    }

    private async void OnLogoutClicked()
    {
        if (AccountSystem.Instance != null && AccountSystem.Instance.IsSignedIn)
        {
            await AccountSystem.Instance.LogoutAsync();
        }
        Hide();
    }

    #endregion
}
