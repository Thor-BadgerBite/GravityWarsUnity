using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hub popup host (GDD §15.3): the one owner of the hub's BackgroundDimmer.
/// Every hub panel that opens on top of the hub - the garage today; settings,
/// missile selection, quests, achievements, profile... as they are placed -
/// goes through this component:
///   - while a popup is open the dimmer is active and drawn right under it,
///   - closing the popup hides the dimmer,
///   - clicking the dimmer closes the current popup,
///   - one popup at a time: opening a second one closes the first.
///
/// Editor setup (one per hub scene): add to MainMenuCanvas/PanelsContainer and
/// drag PanelsContainer/BackgroundDimmer into Background Dimmer. The dimmer
/// stays disabled in the scene; the host enables it only while a popup is
/// open. Popups should be siblings of the dimmer (children of PanelsContainer)
/// so the draw order hub < dimmer < popup holds.
///
/// Open(GameObject) and Close(GameObject) take the popup as a plain Object
/// argument, so a Button's OnClick can call them directly for panels without
/// a controller. Panels with a controller (garage, settings, missiles) call
/// the static ShowPanel/HidePanel, which fall back to SetActive when no host
/// exists in the scene (the same panel used outside the hub).
///
/// Scene-bound: plain scene singleton (Instance set in Awake, cleared in
/// OnDestroy), never DontDestroyOnLoad (GDD Implementation Plan).
/// </summary>
public class HubPopupHost : MonoBehaviour
{
    public static HubPopupHost Instance { get; private set; }

    [Header("Dimmer")]
    [Tooltip("Full-screen raycast-blocking image shown behind the open popup. Leave it disabled in the scene.")]
    [SerializeField] private GameObject backgroundDimmer;

    private GameObject _openPopup;

    /// <summary>The popup currently open, or null.</summary>
    public GameObject OpenPopup => _openPopup;

    public bool IsPopupOpen => _openPopup != null;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[HubPopupHost] A second host exists in the scene; this one is ignored.");
            enabled = false;
            return;
        }

        Instance = this;

        if (backgroundDimmer == null)
        {
            Debug.LogError("[HubPopupHost] Background Dimmer not assigned - popups will open without a dimmer.");
            return;
        }

        backgroundDimmer.SetActive(false);
        HookDimmerClick();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Open / Close

    /// <summary>
    /// Opens a popup above the dimmer. Any other open popup is closed first.
    /// Wireable from a Button OnClick (drag the panel in as the argument).
    /// </summary>
    public void Open(GameObject popup)
    {
        if (popup == null) return;

        if (_openPopup != null && _openPopup != popup)
        {
            _openPopup.SetActive(false);
        }

        _openPopup = popup;

        if (backgroundDimmer != null)
        {
            backgroundDimmer.SetActive(true);
            backgroundDimmer.transform.SetAsLastSibling();
        }

        popup.SetActive(true);
        popup.transform.SetAsLastSibling(); // draw order: hub < dimmer < popup
    }

    /// <summary>
    /// Closes the given popup and hides the dimmer. Ignored when that popup is
    /// not the open one, so a late close (after a fade) cannot hide a popup
    /// that replaced it.
    /// </summary>
    public void Close(GameObject popup)
    {
        if (popup == null || popup != _openPopup) return;

        _openPopup = null;
        popup.SetActive(false);

        if (backgroundDimmer != null)
        {
            backgroundDimmer.SetActive(false);
        }
    }

    /// <summary>Closes whatever popup is open (dimmer click).</summary>
    public void CloseCurrent()
    {
        if (_openPopup != null)
        {
            Close(_openPopup);
        }
    }

    #endregion

    #region Helpers for panel controllers

    /// <summary>
    /// Shows a panel through the host when there is one in the scene,
    /// otherwise just activates it (the panel used outside the hub).
    /// </summary>
    public static void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        if (Instance != null) Instance.Open(panel);
        else panel.SetActive(true);
    }

    /// <summary>Counterpart of ShowPanel.</summary>
    public static void HidePanel(GameObject panel)
    {
        if (panel == null) return;

        if (Instance != null) Instance.Close(panel);
        else panel.SetActive(false);
    }

    #endregion

    #region Dimmer

    /// <summary>
    /// The dimmer is a plain raycast-blocking Image in the scene. A Button
    /// with no visual transition turns its click into "close the popup"
    /// without a second script or extra editor wiring; an existing Button on
    /// the dimmer is reused.
    /// </summary>
    private void HookDimmerClick()
    {
        Button button = backgroundDimmer.GetComponent<Button>();
        if (button == null)
        {
            button = backgroundDimmer.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
        }

        button.onClick.AddListener(CloseCurrent);
    }

    #endregion
}
