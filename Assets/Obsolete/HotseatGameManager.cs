#if false
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class HotseatGameManager : GameModeBase
{
    [Header("Hotseat Setup Screen")]
    public GameObject setupScreen;
    public TMP_InputField player1NameInput;
    public TMP_InputField player2NameInput;
    public Slider winningScoreSlider;
    public Slider turnDurationSlider;
    public Slider prepTimeSlider;
    public Slider unitsToSpawnSlider;
    public TextMeshProUGUI winningScoreText;
    public TextMeshProUGUI turnDurationText;
    public TextMeshProUGUI prepTimeText;
    public TextMeshProUGUI unitsToSpawnText;
    public Button startGameButton;

    [Header("Hotseat Game Screen")]
    public GameObject gameScreen;

    [Header("Player Ships")]
    public PlayerShip player1Ship;
    public PlayerShip player2Ship;
    private PlayerShip currentPlayer;

    private int unitsToSpawn = 4;

    // Assume GameEngine.Instance exists for spawning and placement.
    private GameEngine gameEngine { get { return GameEngine.Instance; } }

    protected override void Awake()
    {
        base.Awake();
        InitializeUI();
    }

    protected override void Start()
    {
        base.Start();
        setupScreen.SetActive(true);
        gameScreen.SetActive(false);
    }

    #region UI Setup

    private void InitializeUI()
    {
        startGameButton.onClick.AddListener(StartGame);
        SetupSlider(winningScoreSlider, winningScore, UpdateWinningScoreText);
        SetupSlider(turnDurationSlider, turnDuration, UpdateTurnDurationText);
        SetupSlider(prepTimeSlider, preparationTime, UpdatePrepTimeText);
        SetupSlider(unitsToSpawnSlider, unitsToSpawn, UpdateUnitsToSpawnText);
    }

    private void SetupSlider(Slider slider, float defaultValue, UnityEngine.Events.UnityAction<float> updateAction)
    {
        slider.value = defaultValue;
        slider.onValueChanged.AddListener(updateAction);
        updateAction.Invoke(defaultValue);
    }

    private void UpdateWinningScoreText(float value)
    {
        winningScore = Mathf.RoundToInt(value);
        winningScoreText.text = $"Winning Score: {winningScore}";
    }

    private void UpdateTurnDurationText(float value)
    {
        turnDuration = value;
        turnDurationText.text = $"Turn Duration: {value:0.0}s";
    }

    private void UpdatePrepTimeText(float value)
    {
        preparationTime = value;
        prepTimeText.text = $"Prep Time: {value:0.0}s";
    }

    private void UpdateUnitsToSpawnText(float value)
    {
        unitsToSpawn = Mathf.RoundToInt(value);
        unitsToSpawnText.text = $"Units to Spawn: {unitsToSpawn}";
    }

    #endregion

    #region Game Initialization

    public override void StartGame()
    {
        setupScreen.SetActive(false);
        gameScreen.SetActive(true);
        InitializeGame();
        SetupPlayerUI();
        // Start the consolidated turn phase for the first turn.
        // By default, start with player1Ship.
        currentPlayer = player1Ship;
        StartCoroutine(RunTurnPhase());
    }

    private void InitializeGame()
    {
        gameEngine.ClearGameObjects();
        SpawnPlayersAndPlanets();
        SetPlayerNames();
    }

    private void SpawnPlayersAndPlanets()
    {
        gameEngine.SpawnPlanets(unitsToSpawn);
        PlacePlayerShips();
    }

    private void PlacePlayerShips()
    {
        // Place left ship:
        Vector3 pos1 = gameEngine.GetPlayerShipPosition(true);
        GameObject p1 = Instantiate(gameEngine.playerShipPrefab, pos1, Quaternion.Euler(0, 90, -90));
        p1.name = "Player1Ship";
        player1Ship = p1.GetComponent<PlayerShip>();
        player1Ship.isLeftPlayer = true;
        player1Ship.Initialize("Player 1", this);

        // Place right ship:
        Vector3 pos2 = gameEngine.GetPlayerShipPosition(false);
        GameObject p2 = Instantiate(gameEngine.playerShipPrefab, pos2, Quaternion.Euler(0, -90, -90));
        p2.name = "Player2Ship";
        player2Ship = p2.GetComponent<PlayerShip>();
        player2Ship.isLeftPlayer = false;
        player2Ship.Initialize("Player 2", this);

        SetupPlayerUI(player1Ship);
        SetupPlayerUI(player2Ship);
    }

    private void SetPlayerNames()
    {
        string name1 = string.IsNullOrEmpty(player1NameInput.text) ? "Player 1" : player1NameInput.text;
        string name2 = string.IsNullOrEmpty(player2NameInput.text) ? "Player 2" : player2NameInput.text;
        if (player1Ship != null)
            player1Ship.Initialize(name1, this);
        if (player2Ship != null)
            player2Ship.Initialize(name2, this);
        UpdateScoreDisplay();
    }

    private void SetupPlayerUI()
    {
        SetupPlayerUI(player1Ship);
        SetupPlayerUI(player2Ship);
    }

    private void SetupPlayerUI(PlayerShip ship)
    {
        GameObject uiObj = Instantiate(playerUIPrefab, gameCanvas.transform);
        PlayerUI ui = uiObj.GetComponent<PlayerUI>();
        ui.Initialize(ship, gameCanvas);
        ship.playerUI = ui;
        ui.SetActive(false);
    }

    #endregion

    #region Turn and Round Management

    /// <summary>
    /// Consolidated coroutine that runs one turn:
    /// – Preparation countdown  
    /// – Enables controls and starts turn  
    /// – Waits for turn duration; if timeout, ends turn
    /// (Missile events should also call EndTurn externally.)
    /// </summary>
    private IEnumerator RunTurnPhase()
    {
        // Preparation phase:
        turnText.text = $"Starting {currentPlayer.playerName}'s Turn";
        yield return StartCoroutine(CountdownTimer($"{currentPlayer.playerName} get ready in", preparationTime));
        
        // Start turn:
        isTurnActive = true;
        missileFired = false;
        currentPlayer.EnableControls(true);
        currentPlayer.ShowTrajectoryLine();
        turnText.text = $"{currentPlayer.playerName}'s Turn";

        // Wait for the turn duration.
        yield return StartCoroutine(CountdownTimer("Time to make a shot", turnDuration));

        // If turn is still active and no missile was fired, end turn due to timeout.
        if (isTurnActive && !missileFired)
        {
            currentPlayer.EnableControls(false);
            currentPlayer.HideTrajectoryLine();
            yield return StartCoroutine(EndTurnPhase("Player failed to make a shot in time."));
        }
        // Otherwise, the missile events (e.g., OnMissileDestroyed) should handle turn ending.
    }

    /// <summary>
    /// Ends the current turn, shows a message, waits for infoFadeDuration,
    /// then switches turn and restarts the turn phase.
    /// </summary>
    private IEnumerator EndTurnPhase(string message)
    {
        isTurnActive = false;
        missileFired = false;
        ClearTimerText();
        //ShowInfoText(message);
        yield return new WaitForSeconds(infoFadeDuration);
        SwitchTurn();
        StartCoroutine(RunTurnPhase());
    }

    /// <summary>
    /// Toggles the current player.
    /// </summary>
    private void SwitchTurn()
    {
        currentPlayer = (currentPlayer == player1Ship) ? player2Ship : player1Ship;
    }

    /// <summary>
    /// Called when a ship is destroyed.
    /// The winning ship’s score is updated and then the round resets
    /// with the losing ship starting the next round.
    /// </summary>
    public override void ShipDestroyed(PlayerShip destroyedShip)
    {
        Debug.Log($"{destroyedShip.playerName}'s ship destroyed");
        PlayerShip winningShip = (destroyedShip == player1Ship) ? player2Ship : player1Ship;
        winningShip.score++;
        UpdateScoreDisplay();
        StartCoroutine(HandleShipDestruction(destroyedShip, winningShip));
    }

    private IEnumerator HandleShipDestruction(PlayerShip destroyedShip, PlayerShip winningShip)
    {
        isTurnActive = false;
        missileFired = false;
        SetOverlayAlpha(0f);
        string message = $"{winningShip.playerName} won round {currentRound}.\nPrepare for round {++currentRound}";
        yield return StartCoroutine(FadeOverlay(true, message));

        // Reset the round and make the losing ship (destroyedShip) start the new round.
        ResetForNewRound();
        currentPlayer = destroyedShip;
        yield return new WaitForSeconds(4f);
        StartCoroutine(RunTurnPhase());
    }

    /// <summary>
    /// Resets the game objects for a new round.
    /// </summary>
    public void ResetForNewRound()
    {
        ClearAllMissileTrails();
        InitializeGame();
        Debug.Log("Reset the game for the new round.");
    }

    #endregion

    #region Missile and Turn Events

    public override void MissileFired()
    {
        Debug.Log("Missile Fired");
        missileFired = true;
        isTurnActive = false;
        currentPlayer.EnableControls(false);
        ClearTimerText();
        StartCoroutine(MissileFlightPhase());
    }

    private IEnumerator MissileFlightPhase()
    {
        yield return StartCoroutine(CountdownTimer("Missile in Flight", maxMissileFlightTime));
        if (!isTurnActive && missileFired)
        {
            Missile3D activeMissile = FindObjectOfType<Missile3D>();
            if (activeMissile != null)
                activeMissile.DestroyMissile();
        }
    }

    public override void OnMissileDestroyed(Missile3D missile)
    {
        Debug.Log("Missile Destroyed");
        if (!isTurnActive && missileFired)
        {
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);
            ClearTimerText();
            string collisionInfo = missile.GetLastCollisionInfo();
            StartCoroutine(EndTurnPhase(string.IsNullOrEmpty(collisionInfo) ? "Missile destroyed!" : $"Missile {collisionInfo}"));
        }
    }

    public override void MissileLostInSpace()
    {
        Debug.Log("Missile Lost in Space");
        if (!isTurnActive && missileFired)
        {
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);
            ClearTimerText();
            StartCoroutine(EndTurnPhase("Missile Lost in Space!"));
        }
    }

    protected override void StartNextTurn()
    {
        // Not used since we consolidate turn switching in RunTurnPhase.
    }

    public override void UpdateScoreDisplay()
    {
        if (player1Ship != null && player2Ship != null)
        {
            scoreText.text = $"{player1Ship.playerName}: {player1Ship.score} | {player2Ship.playerName}: {player2Ship.score}";
        }
    }

    /// <summary>
    /// Clears missile trails on both ships.
    /// </summary>
    public void ClearAllMissileTrails()
    {
        if (player1Ship != null) player1Ship.ClearLastMissileTrail();
        if (player2Ship != null) player2Ship.ClearLastMissileTrail();
    }

    #endregion

    #region (Optional) Additional UI/Transition Helpers

    // You can add additional helper methods here if needed.

    #endregion

    // OnDestroy unsubscribes from missile events.
    void OnDestroy()
    {
        Missile3D.OnMissileDestroyed -= OnMissileDestroyed;
    }
}
#endif