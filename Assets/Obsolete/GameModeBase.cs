#if false
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public abstract class GameModeBase : MonoBehaviour
{
    [Header("Game Settings")]
    public int winningScore = 3;
    public float preparationTime = 3f;
    public float turnDuration = 15f;
    public float maxMissileFlightTime = 30f;
    public float infoFadeDuration = 2f;
    public float transitionDuration = 1f;
    public float gameOverDuration = 5f;

    [Header("UI Base Elements")]
    public Canvas gameCanvas;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI scoreText;
    public Image transitionOverlay;
    public TextMeshProUGUI transitionText;
    public GameObject playerUIPrefab;

    // Shared state used by derived classes.
    protected Coroutine timerCoroutine;
    protected int currentRound = 1;
    protected bool isTurnActive = false;
    protected bool missileFired = false;

    #region Shared UI Helpers
        protected virtual void Awake()
    {
        // Set up shared engine stuff.
        //gameEngine = GameEngine.Instance;
        //InitializeGameEngine();
    }

    protected virtual void Start()
    {
        // Subscribe to events common to all modes.
        Missile3D.OnMissileDestroyed += OnMissileDestroyed;
    }
    protected void ClearTimerText()
    {
        if (timerText != null)
            timerText.text = "";
    }

    protected IEnumerator CountdownTimer(string message, float duration)
    {
        float timeLeft = duration;
        while (timeLeft > 0f)
        {
            if (timerText != null)
                timerText.text = $"{message}: {timeLeft:F0}s";
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        ClearTimerText();
    }

    protected IEnumerator FadeText(TextMeshProUGUI textComponent, string message, float duration)
    {
        textComponent.text = message;
        textComponent.alpha = 1;
        yield return new WaitForSeconds(duration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textComponent.alpha = Mathf.Clamp01(1 - elapsed / duration);
            yield return null;
        }
        textComponent.text = "";
    }

    protected IEnumerator FadeOverlay(bool fadeIn, string message = "")
    {
        transitionText.text = message;
        transitionOverlay.gameObject.SetActive(true);

        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        SetOverlayAlpha(startAlpha);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / transitionDuration);
            SetOverlayAlpha(alpha);
            yield return null;
        }

        SetOverlayAlpha(endAlpha);

        if (!fadeIn)
            transitionOverlay.gameObject.SetActive(false);

        yield break;
    }

    protected void SetOverlayAlpha(float alpha)
    {
        Color c = transitionOverlay.color;
        transitionOverlay.color = new Color(c.r, c.g, c.b, alpha);
        Color tc = transitionText.color;
        transitionText.color = new Color(tc.r, tc.g, tc.b, alpha);
    }

    #endregion

    #region Abstract Methods

    public abstract void StartGame();
    public abstract void ShipDestroyed(PlayerShip destroyedShip);
    public abstract void MissileFired();
    public abstract void MissileLostInSpace();
    public abstract void OnMissileDestroyed(Missile3D missile);
    protected abstract void StartNextTurn();
    public abstract void UpdateScoreDisplay();

    #endregion
}
#endif