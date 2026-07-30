using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Post-match results screen.
/// Shows winner banner, match statistics (damage, accuracy, rounds),
/// progression rewards (XP, credits, level-ups, battle pass tiers)
/// and Play Again / Return to Menu buttons.
///
/// Setup:
/// - Place on a (initially inactive) full-screen panel in the game scene.
/// - GameManager finds it automatically at match end and calls Show().
/// </summary>
public class MatchResultsUI : MonoBehaviour
{
    public static MatchResultsUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject resultsPanel;

    [Header("Banner")]
    [SerializeField] private TextMeshProUGUI winnerBannerText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Winner Stats")]
    [SerializeField] private TextMeshProUGUI winnerNameText;
    [SerializeField] private TextMeshProUGUI winnerDamageText;
    [SerializeField] private TextMeshProUGUI winnerMissilesText;
    [SerializeField] private TextMeshProUGUI winnerAccuracyText;

    [Header("Loser Stats")]
    [SerializeField] private TextMeshProUGUI loserNameText;
    [SerializeField] private TextMeshProUGUI loserDamageText;
    [SerializeField] private TextMeshProUGUI loserMissilesText;
    [SerializeField] private TextMeshProUGUI loserAccuracyText;

    [Header("Rewards")]
    [SerializeField] private TextMeshProUGUI xpGainedText;
    [SerializeField] private TextMeshProUGUI creditsGainedText;
    [SerializeField] private TextMeshProUGUI eloChangeText;
    [SerializeField] private GameObject levelUpBanner;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private GameObject battlePassBanner;
    [SerializeField] private TextMeshProUGUI battlePassText;

    [Header("Engagement Banners")]
    [SerializeField] private GameObject firstWinBanner;        // "FIRST WIN OF THE DAY - 2x BP XP!"
    [SerializeField] private GameObject streakBanner;
    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private GameObject closeMatchBanner;      // "SO CLOSE! +50 XP"
    [SerializeField] private TextMeshProUGUI closeMatchText;
    [SerializeField] private GameObject trickshotBanner;       // "GRAVITY ASSIST BONUS"
    [SerializeField] private TextMeshProUGUI trickshotText;
    [SerializeField] private TextMeshProUGUI rivalryText;      // "vs Alex: 3-1"

    [Header("Killshot Replay")]
    [SerializeField] private Button watchReplayButton;

    [Header("Misc")]
    [SerializeField] private TextMeshProUGUI matchDurationText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private Button requeueButton;             // online: instant re-queue

    /// <summary>Raised when the player wants to jump straight into another online match.</summary>
    public event System.Action OnRequeueRequested;

    [Header("Navigation")]
    [Tooltip("Scene to load when returning to the main menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        Instance = this;

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        if (requeueButton != null)
            requeueButton.onClick.AddListener(() => OnRequeueRequested?.Invoke());
        if (watchReplayButton != null)
            watchReplayButton.onClick.AddListener(() => KillshotReplayUI.Instance?.PlayLastKillshot());

        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Populates and shows the results screen.
    /// </summary>
    public void Show(MatchResultsSummary summary)
    {
        if (summary == null) return;

        if (resultsPanel != null)
            resultsPanel.SetActive(true);
        else
            gameObject.SetActive(true);

        SetText(winnerBannerText, $"{summary.winnerName} WINS!");
        SetText(finalScoreText, $"{summary.winnerScore} - {summary.loserScore}");

        // Winner column
        SetText(winnerNameText, summary.winnerName);
        FillStatsColumn(summary.winnerStats, winnerDamageText, winnerMissilesText, winnerAccuracyText);

        // Loser column
        SetText(loserNameText, summary.loserName);
        FillStatsColumn(summary.loserStats, loserDamageText, loserMissilesText, loserAccuracyText);

        // Rewards
        SetText(xpGainedText, $"+{summary.xpGained} XP");
        SetText(creditsGainedText, $"+{summary.creditsGained} Credits");

        if (eloChangeText != null)
        {
            bool showElo = summary.eloChange != 0;
            eloChangeText.gameObject.SetActive(showElo);
            if (showElo)
                eloChangeText.text = summary.eloChange > 0 ? $"+{summary.eloChange} ELO" : $"{summary.eloChange} ELO";
        }

        if (levelUpBanner != null)
        {
            levelUpBanner.SetActive(summary.leveledUp);
            if (summary.leveledUp)
                SetText(levelUpText, $"LEVEL UP! Level {summary.newLevel}");
        }

        if (battlePassBanner != null)
        {
            bool showBp = summary.battlePassTiersGained > 0;
            battlePassBanner.SetActive(showBp);
            if (showBp)
                SetText(battlePassText, $"Battle Pass +{summary.battlePassTiersGained} Tier{(summary.battlePassTiersGained > 1 ? "s" : "")}");
        }

        if (matchDurationText != null)
        {
            int minutes = Mathf.FloorToInt(summary.matchDurationSeconds / 60f);
            int seconds = Mathf.FloorToInt(summary.matchDurationSeconds % 60f);
            matchDurationText.text = $"Match time: {minutes:00}:{seconds:00}";
        }

        // ===== Engagement banners =====

        if (firstWinBanner != null)
            firstWinBanner.SetActive(summary.firstWinOfTheDay);

        if (streakBanner != null)
        {
            bool showStreak = summary.winStreak >= 2;
            streakBanner.SetActive(showStreak);
            if (showStreak)
            {
                string bonus = summary.streakBonusCredits > 0 ? $"  (+{summary.streakBonusCredits} credits)" : "";
                SetText(streakText, $"🔥 {summary.winStreak} WIN STREAK{bonus}");
            }
        }

        if (closeMatchBanner != null)
        {
            closeMatchBanner.SetActive(summary.closeMatch && summary.closeMatchBonusXP > 0);
            if (summary.closeMatch)
                SetText(closeMatchText, $"SO CLOSE! +{summary.closeMatchBonusXP} XP");
        }

        if (trickshotBanner != null)
        {
            bool showTrick = summary.trickshotBonusXP > 0;
            trickshotBanner.SetActive(showTrick);
            if (showTrick)
                SetText(trickshotText, $"☄ GRAVITY ASSIST BONUS +{summary.trickshotBonusXP} XP");
        }

        // Rivalry line (online matches with a known opponent)
        if (rivalryText != null)
        {
            var data = ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
            if (data != null && !string.IsNullOrEmpty(summary.opponentUsername))
            {
                var (wins, losses) = data.GetHeadToHeadRecord(summary.opponentUsername);
                rivalryText.text = (wins + losses) > 0
                    ? $"vs {summary.opponentUsername}: {wins}-{losses}"
                    : "";
            }
            else
            {
                rivalryText.text = "";
            }
        }

        // Killshot replay: auto-play once, keep the button for re-watching
        bool hasReplay = KillshotReplayUI.Instance != null && KillshotReplayUI.Instance.HasKillshot;
        if (watchReplayButton != null)
            watchReplayButton.gameObject.SetActive(hasReplay);
        if (hasReplay)
            KillshotReplayUI.Instance.PlayLastKillshot();
    }

    private void FillStatsColumn(
        MatchStatsTracker.PlayerStats stats,
        TextMeshProUGUI damageText,
        TextMeshProUGUI missilesText,
        TextMeshProUGUI accuracyText)
    {
        if (stats == null) return;
        SetText(damageText, $"Damage: {stats.damageDealt}");
        SetText(missilesText, $"Missiles: {stats.missilesHit}/{stats.missilesFired}");
        SetText(accuracyText, $"Accuracy: {stats.Accuracy * 100f:F0}%");
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null) target.text = value;
    }

    private void OnPlayAgainClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnReturnToMenuClicked()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName) &&
            Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning($"[MatchResultsUI] Main menu scene '{mainMenuSceneName}' not in build settings - reloading current scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

/// <summary>
/// Everything the results screen needs about a finished match.
/// Built by GameManager.AwardMatchProgression().
/// </summary>
public class MatchResultsSummary
{
    public string winnerName;
    public string loserName;
    public bool player1Won;
    public int winnerScore;
    public int loserScore;

    public MatchStatsTracker.PlayerStats winnerStats;
    public MatchStatsTracker.PlayerStats loserStats;

    public float matchDurationSeconds;

    // Progression rewards (local account)
    public int xpGained;
    public int creditsGained;
    public int eloChange;               // 0 for offline/hotseat matches
    public bool leveledUp;
    public int newLevel;
    public int battlePassTiersGained;

    // Engagement extras
    public bool firstWinOfTheDay;       // 2x battle pass XP was applied
    public int winStreak;               // local player's current streak
    public int streakBonusCredits;      // credits from streak milestones
    public bool closeMatch;             // loss was one round short
    public int closeMatchBonusXP;
    public int trickshotBonusXP;        // gravity assist bonus XP
    public string opponentUsername;     // for the rivalry line (online)
}
