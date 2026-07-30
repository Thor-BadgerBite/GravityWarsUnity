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

    [Header("Misc")]
    [SerializeField] private TextMeshProUGUI matchDurationText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button returnToMenuButton;

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
}
