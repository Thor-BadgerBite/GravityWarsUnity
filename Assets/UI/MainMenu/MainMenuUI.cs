using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using GravityWars.Online;

/// <summary>
/// Manages the main menu UI elements (Brawl Stars style).
///
/// Features:
/// - Player info display (username, level, XP, ELO, rank)
/// - Currency display (credits, gems)
/// - Game mode buttons (Ranked, Casual, Local Hotseat, Training)
/// - Navigation to other screens (Ships, Achievements, Settings, etc.)
/// - Animated transitions
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    #region Inspector References

    [Header("Player Info Panel")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image xpFillBar;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private Image rankIcon;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("Currency Panel")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private Button addCreditsButton;
    [SerializeField] private Button addGemsButton;

    [Header("Game Mode Buttons")]
    [SerializeField] private Button rankedButton;
    [SerializeField] private Button casualButton;
    [SerializeField] private Button localHotseatButton;
    [SerializeField] private Button trainingButton;

    [Header("Navigation Buttons")]
    [SerializeField] private Button shipsButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button questsButton;

    [Header("Top Bar")]
    [SerializeField] private Button notificationsButton;
    [SerializeField] private GameObject notificationBadge;
    [SerializeField] private TextMeshProUGUI notificationCountText;

    [Header("Rank Icons")]
    [SerializeField] private Sprite cadetRankIcon;
    [SerializeField] private Sprite midshipmanRankIcon;
    [SerializeField] private Sprite ensignRankIcon;
    [SerializeField] private Sprite subLieutenantRankIcon;
    [SerializeField] private Sprite lieutenantRankIcon;
    [SerializeField] private Sprite lieutenantCommanderRankIcon;
    [SerializeField] private Sprite commanderRankIcon;
    [SerializeField] private Sprite captainRankIcon;
    [SerializeField] private Sprite seniorCaptainRankIcon;
    [SerializeField] private Sprite commodoreRankIcon;
    [SerializeField] private Sprite rearAdmiralRankIcon;
    [SerializeField] private Sprite rearAdmiralUpperHalfRankIcon;
    [SerializeField] private Sprite viceAdmiralRankIcon;
    [SerializeField] private Sprite admiralRankIcon;
    [SerializeField] private Sprite highAdmiralRankIcon;
    [SerializeField] private Sprite fleetAdmiralRankIcon;
    [SerializeField] private Sprite supremeAdmiralRankIcon;
    [SerializeField] private Sprite grandAdmiralRankIcon;

    [Header("Animation")]
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;

    #endregion

    #region Events

    // Game mode events
    public event Action OnRankedClicked;
    public event Action OnCasualClicked;
    public event Action OnLocalHotseatClicked;
    public event Action OnTrainingClicked;

    // Navigation events
    public event Action OnShipsClicked;
    public event Action OnAchievementsClicked;
    public event Action OnSettingsClicked;
    public event Action OnProfileClicked;
    public event Action OnLeaderboardClicked;
    public event Action OnQuestsClicked;
    public event Action OnNotificationsClicked;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        SetupButtonListeners();
        FadeIn();
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Setup all button click listeners.
    /// </summary>
    private void SetupButtonListeners()
    {
        // Game mode buttons
        if (rankedButton != null) rankedButton.onClick.AddListener(() => OnRankedClicked?.Invoke());
        if (casualButton != null) casualButton.onClick.AddListener(() => OnCasualClicked?.Invoke());
        if (localHotseatButton != null) localHotseatButton.onClick.AddListener(() => OnLocalHotseatClicked?.Invoke());
        if (trainingButton != null) trainingButton.onClick.AddListener(() => OnTrainingClicked?.Invoke());

        // Navigation buttons
        if (shipsButton != null) shipsButton.onClick.AddListener(() => OnShipsClicked?.Invoke());
        if (achievementsButton != null) achievementsButton.onClick.AddListener(() => OnAchievementsClicked?.Invoke());
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        if (profileButton != null) profileButton.onClick.AddListener(() => OnProfileClicked?.Invoke());
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(() => OnLeaderboardClicked?.Invoke());
        if (questsButton != null) questsButton.onClick.AddListener(() => OnQuestsClicked?.Invoke());
        if (notificationsButton != null) notificationsButton.onClick.AddListener(() => OnNotificationsClicked?.Invoke());
    }

    /// <summary>
    /// Remove all button listeners.
    /// </summary>
    private void RemoveButtonListeners()
    {
        if (rankedButton != null) rankedButton.onClick.RemoveAllListeners();
        if (casualButton != null) casualButton.onClick.RemoveAllListeners();
        if (localHotseatButton != null) localHotseatButton.onClick.RemoveAllListeners();
        if (trainingButton != null) trainingButton.onClick.RemoveAllListeners();

        if (shipsButton != null) shipsButton.onClick.RemoveAllListeners();
        if (achievementsButton != null) achievementsButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (profileButton != null) profileButton.onClick.RemoveAllListeners();
        if (leaderboardButton != null) leaderboardButton.onClick.RemoveAllListeners();
        if (questsButton != null) questsButton.onClick.RemoveAllListeners();
        if (notificationsButton != null) notificationsButton.onClick.RemoveAllListeners();
    }

    #endregion

    #region Player Info Display

    /// <summary>
    /// Update all player info displays from profile data.
    /// </summary>
    public void UpdatePlayerInfo(PlayerAccountData profile)
    {
        if (profile == null)
        {
            Debug.LogError("[MainMenuUI] Cannot update player info - profile is null");
            return;
        }

        // Username and level
        if (usernameText != null)
            usernameText.text = profile.username;

        if (levelText != null)
            levelText.text = $"Level {profile.level}";

        // XP bar
        if (xpFillBar != null)
        {
            float xpProgress = (float)profile.currentXP / profile.xpForNextLevel;
            xpFillBar.fillAmount = xpProgress;
        }

        if (xpText != null)
            xpText.text = $"{profile.currentXP} / {profile.xpForNextLevel} XP";

        // ELO and rank
        if (eloText != null)
            eloText.text = $"{profile.eloRating} ELO";

        if (rankText != null)
            rankText.text = ELORatingSystem.GetRankDisplayName(profile.currentRank);

        if (rankIcon != null)
            rankIcon.sprite = GetRankIcon(profile.currentRank);

        // Currency
        if (creditsText != null)
            creditsText.text = FormatCurrency(profile.credits);

        if (gemsText != null)
            gemsText.text = FormatCurrency(profile.gems);

        Debug.Log($"[MainMenuUI] Updated player info for {profile.username}");
    }

    /// <summary>
    /// Get rank icon sprite based on competitive rank.
    /// </summary>
    private Sprite GetRankIcon(CompetitiveRank rank)
    {
        switch (rank)
        {
            case CompetitiveRank.Cadet: return cadetRankIcon;
            case CompetitiveRank.Midshipman: return midshipmanRankIcon;
            case CompetitiveRank.Ensign: return ensignRankIcon;
            case CompetitiveRank.SubLieutenant: return subLieutenantRankIcon;
            case CompetitiveRank.Lieutenant: return lieutenantRankIcon;
            case CompetitiveRank.LieutenantCommander: return lieutenantCommanderRankIcon;
            case CompetitiveRank.Commander: return commanderRankIcon;
            case CompetitiveRank.Captain: return captainRankIcon;
            case CompetitiveRank.SeniorCaptain: return seniorCaptainRankIcon;
            case CompetitiveRank.Commodore: return commodoreRankIcon;
            case CompetitiveRank.RearAdmiral: return rearAdmiralRankIcon;
            case CompetitiveRank.RearAdmiralUpperHalf: return rearAdmiralUpperHalfRankIcon;
            case CompetitiveRank.ViceAdmiral: return viceAdmiralRankIcon;
            case CompetitiveRank.Admiral: return admiralRankIcon;
            case CompetitiveRank.HighAdmiral: return highAdmiralRankIcon;
            case CompetitiveRank.FleetAdmiral: return fleetAdmiralRankIcon;
            case CompetitiveRank.SupremeAdmiral: return supremeAdmiralRankIcon;
            case CompetitiveRank.GrandAdmiral: return grandAdmiralRankIcon;
            default: return lieutenantRankIcon;
        }
    }

    /// <summary>
    /// Format currency with K/M suffixes for large numbers.
    /// </summary>
    private string FormatCurrency(int amount)
    {
        if (amount >= 1000000)
            return $"{amount / 1000000f:F1}M";
        else if (amount >= 1000)
            return $"{amount / 1000f:F1}K";
        else
            return amount.ToString();
    }

    #endregion

    #region Currency Updates

    /// <summary>
    /// Update credits display.
    /// </summary>
    public void UpdateCredits(int credits)
    {
        if (creditsText != null)
            creditsText.text = FormatCurrency(credits);
    }

    /// <summary>
    /// Update gems display.
    /// </summary>
    public void UpdateGems(int gems)
    {
        if (gemsText != null)
            gemsText.text = FormatCurrency(gems);
    }

    /// <summary>
    /// Play currency gain animation.
    /// </summary>
    public void PlayCurrencyGainAnimation(bool isGems, int amount)
    {
        // TODO: Implement particle effect or animated text
        Debug.Log($"[MainMenuUI] Currency gain animation: {(isGems ? "Gems" : "Credits")} +{amount}");
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Update notification badge.
    /// </summary>
    public void UpdateNotifications(int count)
    {
        if (notificationBadge != null)
        {
            notificationBadge.SetActive(count > 0);
        }

        if (notificationCountText != null)
        {
            notificationCountText.text = count > 99 ? "99+" : count.ToString();
        }
    }

    #endregion

    #region Button States

    /// <summary>
    /// Enable/disable ranked button (e.g., if player hasn't completed tutorial).
    /// </summary>
    public void SetRankedButtonEnabled(bool enabled)
    {
        if (rankedButton != null)
            rankedButton.interactable = enabled;
    }

    /// <summary>
    /// Set game mode button highlight (show which mode is currently selected).
    /// </summary>
    public void HighlightGameModeButton(string mode)
    {
        // Reset all highlights
        ResetButtonHighlights(rankedButton);
        ResetButtonHighlights(casualButton);
        ResetButtonHighlights(localHotseatButton);
        ResetButtonHighlights(trainingButton);

        // Highlight selected mode
        Button selectedButton = null;
        switch (mode.ToLower())
        {
            case "ranked": selectedButton = rankedButton; break;
            case "casual": selectedButton = casualButton; break;
            case "hotseat": selectedButton = localHotseatButton; break;
            case "training": selectedButton = trainingButton; break;
        }

        if (selectedButton != null)
        {
            var colors = selectedButton.colors;
            colors.normalColor = new Color(1f, 0.8f, 0.3f); // Gold highlight
            selectedButton.colors = colors;
        }
    }

    private void ResetButtonHighlights(Button button)
    {
        if (button == null) return;

        var colors = button.colors;
        colors.normalColor = Color.white;
        button.colors = colors;
    }

    #endregion

    #region Animations

    /// <summary>
    /// Fade in the main menu.
    /// </summary>
    private void FadeIn()
    {
        if (mainCanvasGroup == null) return;

        mainCanvasGroup.alpha = 0f;
        LeanTween.alphaCanvas(mainCanvasGroup, 1f, fadeInDuration).setEase(LeanTweenType.easeOutCubic);
    }

    /// <summary>
    /// Fade out the main menu.
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        if (mainCanvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        LeanTween.alphaCanvas(mainCanvasGroup, 0f, fadeInDuration)
            .setEase(LeanTweenType.easeInCubic)
            .setOnComplete(onComplete);
    }

    /// <summary>
    /// Play button press animation (scale bounce).
    /// </summary>
    public void PlayButtonPressAnimation(Button button)
    {
        if (button == null) return;

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null) return;

        Vector3 originalScale = rect.localScale;
        LeanTween.scale(rect, originalScale * 0.95f, 0.1f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                LeanTween.scale(rect, originalScale, 0.1f).setEase(LeanTweenType.easeOutCubic);
            });
    }

    #endregion

    #region Utility

    /// <summary>
    /// Show/hide specific UI panels.
    /// </summary>
    public void SetPanelActive(string panelName, bool active)
    {
        // Find panel by name and set active
        Transform panel = transform.Find(panelName);
        if (panel != null)
        {
            panel.gameObject.SetActive(active);
        }
        else
        {
            Debug.LogWarning($"[MainMenuUI] Panel not found: {panelName}");
        }
    }

    #endregion
}
