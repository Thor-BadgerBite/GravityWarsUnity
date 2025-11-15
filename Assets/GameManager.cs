using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // for multi-missile handling
    private int multiMissilesRemaining = 0;
    private bool inMultiMissileMode    = false;

    // ----------------------------------
    // Existing Fields
    // ----------------------------------
    private int storedScore1 = 0;
    private int storedScore2 = 0;
    enum MoveType { Regular = 0, Precision, Warp }
    [Header("Game Settings")]
    public int winningScore = 3;
    public float preparationTime = 3f;
    public float turnDuration = 15f;
    public float maxMissileFlightTime = 30f;
    public float infoFadeDuration = 2f;
    public float destructionDelay = 3f;
    public float transitionDuration = 1f;
    public float gameOverDuration = 5f;

    [Header("Planet Spawning")]
    public PlanetInfo[] planetInfos;
    public int unitsToSpawn = 4;
    public float minDistanceBetweenPlanets = 1.5f;
    public int maxSpawnAttempts = 100;
    public int repositionIterations = 5;
    public float repositionForce = 0.5f;

    [Header("Ship Placement")]
    public GameObject playerShipPrefab;
    public float minDistanceFromCenter = 25f;
    public float maxDistanceFromCenter = 28f;
    public float shipCollisionRadius = 1f;
    public int maxShipPlacementAttempts = 10;
    public float topBottomOffset = 5f;

    [Header("Player Ships")]
    public PlayerShip player1Ship;
    public PlayerShip player2Ship;

    [Header("Player Names (Set by HotSeatSetup)")]
    public string player1Name = "Player 1";
    public string player2Name = "Player 2";

    // ----------------------------------
    // OLD UI references
    // ----------------------------------
    [Header("UI Elements (Old)")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI timerText;   // Possibly replaced by BubbleTimer + TimerText
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI scoreText;   // Possibly replaced by new UI
    public Image transitionOverlay;
    public TextMeshProUGUI transitionText;
    public GameObject playerUIPrefab;
    public Canvas gameCanvas;
    public GameObject setupScreen;
    public GameObject gameScreen;

    // ----------------------------------
    // NEW UI references
    // ----------------------------------

    [Header("NEW Fighting-Game Style UI")]

    // Timer bubble
    public Slider bubbleTimer;        // The slider for the bubble timer
    public TextMeshProUGUI timerBubbleText; // The TMP for the bubble timer numeric text (seconds)

    // Player1 UI
    [Header("Player 1 UI")]
    public TextMeshProUGUI player1NameText;
    public Slider health1Bar;
    public TextMeshProUGUI health1Text;   // "55 / 100"
    public Slider moves1Bar;
    // If you have more complicated "dividers" for moves, you might store them or generate them in code.
    // --- New references for “multi-segment” bars:
    public Transform moves1DividersParent;   // the "Dividers" empty container
    public TextMeshProUGUI ship1ModelText;
    public TextMeshProUGUI ship1LevelText;

    // Player2 UI
    [Header("Player 2 UI")]
    public TextMeshProUGUI player2NameText;
    public Slider health2Bar;
    public TextMeshProUGUI health2Text;
    public Slider moves2Bar;
    public Transform moves2DividersParent;   // the "Dividers" empty container
    public TextMeshProUGUI ship2ModelText;
    public TextMeshProUGUI ship2LevelText;

    // Possibly references to the new "score" text if you display it
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public GameObject spacerPrefab;          // the newly created Spacer prefab
    [Header("Passive UI")]
    public Transform ship1PassivesPanel;    // assign Ship1PassivesPanel
    public GameObject ship1PassiveTemplate; // assign Ship1PassiveTemplate
    public Transform ship2PassivesPanel;    // assign Ship1PassivesPanel
    public GameObject ship2PassiveTemplate; // assign Ship1PassiveTemplate

    public Sprite[] passivesActiveIcons;   // length 15
    public Sprite[] passivesInactiveIcons; // length 15

    [Header("Player 1 Perk Icons (3 slots)")]
    public Image[] player1PerkIcons = new Image[3];

    [Header("Player 2 Perk Icons (3 slots)")]
    public Image[] player2PerkIcons = new Image[3];

    public Image player1MoveIcon;
    public Image player2MoveIcon;

    // Active versions
    public Sprite regularMoveActive;
    public Sprite precisionMoveActive;
    public Sprite warpMoveActive;

    // Inactive (grayed‑out) versions
    public Sprite regularMoveInactive;
    public Sprite precisionMoveInactive;
    public Sprite warpMoveInactive;
    // ----------------------------------
    // Internal
    // ----------------------------------
    private PlayerShip currentPlayer;
    private bool isTurnActive = false;
    private Coroutine activeCoroutine;
    private Coroutine timerCoroutine;
    private Coroutine missileFlightCoroutine;

    private bool missileFired = false;
    private int currentRound = 1;

    private List<PlanetInfo> availablePlanets;
    private float width;
    private float height;

    public float Width => width;
    public float Height => height;

    private List<SpawnedPlanet> spawnedPlanets = new List<SpawnedPlanet>();
    private List<Planet> planetComponents = new List<Planet>();

    private class SpawnedPlanet
    {
        public GameObject gameObject;
        public Vector2 position;
        public float size;
    }

    [System.Serializable]
    public class PlanetInfo
    {
        public GameObject prefab;
        public string name;
        public float mass;
        public int units;
    }
    public enum PassiveType {
        SniperMode = 0,
        Unmovable,
        EnhancedRegeneration,
        DamageResistance,
        CriticalImmunity,
        CriticalEnhancement,
        DamageBoost,
        LastChance,
        AdaptiveArmor,
        AdaptiveDamage,
        PrecisionEngineering,
        CollisionAvoidance,
        Lifesteal,
        ReduceDamageFromHighSpeedMissiles,
        IncreaseDamageOnHighSpeedMissiles
        // total = 15
    }
    // ---------------------------------------------------------
    // MONOBEHAVIOUR
    // ---------------------------------------------------------
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Missile3D.OnMissileDestroyed += OnMissileDestroyed;
        setupScreen.SetActive(true);
        gameScreen.SetActive(false);
        SetupSpawnArea();

        // If you want to initialize new UI right away, you can do so here.
        // e.g., bubbleTimer.maxValue = turnDuration;
        //       bubbleTimer.value    = turnDuration;
        // But we often do it right after StartGame or once ships are placed.
    }

    void OnDestroy()
    {
        Missile3D.OnMissileDestroyed -= OnMissileDestroyed;
    }
    /// <summary>
    /// Called by PlayerShip to say “I’m about to launch N missiles at once”.
    /// </summary>
    public void BeginMultiMissile(int count)
    {
        inMultiMissileMode      = true;
        multiMissilesRemaining  = count;
    }

    // ---------------------------------------------------------
    // SETUP
    // ---------------------------------------------------------
    void SetupSpawnArea()
    {
        Camera mainCamera = Camera.main;
        height = 2f * mainCamera.orthographicSize;
        width = height * mainCamera.aspect;
    }

    public void UpdateUnitsToSpawn(float value)
    {
        unitsToSpawn = Mathf.RoundToInt(value);
    }

    public void StartGame()
    {
        setupScreen.SetActive(false);
        gameScreen.SetActive(true);
        InitializeGame();
        SetupPlayerUI(); // old UI hooking - optional
        var pm1 = player1Ship.GetComponent<PerkManager>();
    pm1.SetIconSlots(player1PerkIcons);

    var pm2 = player2Ship.GetComponent<PerkManager>();
    pm2.SetIconSlots(player2PerkIcons);

        StartCoroutine(StartGamePhase());
    }

    void InitializeGame()
    {
        ClearExistingPlanetsAndShips();
        SpawnPlanets();
        PlaceShips();

        // Now that ships exist, set their names from the fields:
        SetPlayerNames(player1Name, player2Name);

        // ---- Initialize the new UI bars, if you want:
        UpdateFightingUI_AtRoundStart();
        SetupMoveDividers(moves1DividersParent, spacerPrefab, player1Ship.movesAllowedPerTurn);
        SetupMoveDividers(moves2DividersParent, spacerPrefab, player2Ship.movesAllowedPerTurn);


    }
    public void UpdateFightingUI_AtRoundStart()
    {
        // Player1
        player1NameText.text = player1Ship.playerName;
        health1Bar.maxValue = player1Ship.GetMaxHealth();
        health1Bar.value    = player1Ship.GetCurrentHealth();
        health1Text.text    = $"{player1Ship.GetCurrentHealth()}/{player1Ship.GetMaxHealth()}";
        moves1Bar.maxValue  = player1Ship.movesAllowedPerTurn;
        moves1Bar.value     = player1Ship.movesRemainingThisRound;
        // Now set the Ship1ModelText and Ship1LevelText (example):
        if (ship1ModelText != null) 
            ship1ModelText.text = player1Ship.shipModelName;  // e.g. "Star Sparrow"

        if (ship1LevelText != null) 
            ship1LevelText.text = "Level : " + player1Ship.shipLevel;
        // Player1
        UpdateMoveIconUI(player1Ship, player1MoveIcon);
        
        // if you want multiple segments or dividing lines, do a function that duplicates 
        // a "tick" object for each step. Example: (movesAllowedPerTurn - 1) ticks or so.

        // Player2
        player2NameText.text = player2Ship.playerName;
        health2Bar.maxValue  = player2Ship.GetMaxHealth();
        health2Bar.value     = player2Ship.GetCurrentHealth();
        health2Text.text     = $"{player2Ship.GetCurrentHealth()}/{player2Ship.GetMaxHealth()}";
        moves2Bar.maxValue   = player2Ship.movesAllowedPerTurn;
        moves2Bar.value      = player2Ship.movesRemainingThisRound;
        // For Ship2 UI as well:
        if (ship2ModelText != null)
            ship2ModelText.text = player2Ship.shipModelName;  // e.g. "Star Sparrow"

        if (ship2LevelText != null)
            ship2LevelText.text = "Level : " + player2Ship.shipLevel;
        // Player2
        UpdateMoveIconUI(player2Ship, player2MoveIcon);

        // Then call for player1:
        // Now fill passives for player1
        PopulatePassivesUI(player1Ship, ship1PassivesPanel, ship1PassiveTemplate);

        // Do the same for player2 if you like
        PopulatePassivesUI(player2Ship, ship2PassivesPanel, ship2PassiveTemplate);

    }


   void ClearExistingPlanetsAndShips()
    {
        // Destroy all planets
        foreach (var planet in FindObjectsOfType<Planet>())
        {
            Destroy(planet.gameObject);
        }
        planetComponents.Clear();
        spawnedPlanets.Clear();

        // Destroy all old ships
        PlayerShip[] existingShips = FindObjectsOfType<PlayerShip>();
        foreach (var ship in existingShips)
        {
            Destroy(ship.gameObject);
        }

        // NEW: Destroy any leftover PlayerUI objects
        PlayerUI[] existingUIs = FindObjectsOfType<PlayerUI>();
        foreach (var ui in existingUIs)
        {
            Destroy(ui.gameObject);
        }
    }

    /// <summary>
    /// Clones the spacerPrefab under the given dividersParent for the specified movesAllowed.
    /// If movesAllowed is 0, the entire bar is hidden.
    /// </summary>
    private void SetupMoveDividers(Transform dividersParent, GameObject spacerPrefab, int movesAllowed)
    {
        // 1) If movesAllowed <= 0 => hide the entire container
        if (movesAllowed <= 0)
        {
            // In many cases, the slider (MovesBar) is the parent of "Dividers",
            // so we can hide that entire hierarchy. E.g.:
            dividersParent.parent.gameObject.SetActive(false);
            return;
        }

        // Otherwise, ensure it's shown:
        dividersParent.parent.gameObject.SetActive(true);

        // 2) Clear out old children
        foreach (Transform child in dividersParent)
        {
            Destroy(child.gameObject);
        }

        // 3) Instantiate as many spacers as movesAllowed
        //    (If you only want movesAllowed - 1, just change the loop.)
        for (int i = 0; i < movesAllowed; i++)
        {
            GameObject newSpacer = Instantiate(spacerPrefab, dividersParent);
            newSpacer.name = $"Spacer_{i + 1}";
            // Optionally position or configure each spacer here
        }
    }

    void SpawnPlanets()
    {
        InitializeAvailablePlanets();
        if (planetInfos.Length == 0)
        {
            Debug.LogError("No planet prefabs assigned!");
            return;
        }

        int remainingUnits = unitsToSpawn;

        while (remainingUnits > 0 && availablePlanets.Count > 0)
        {
            List<PlanetInfo> validPlanets = availablePlanets.Where(p => p.units <= remainingUnits).ToList();
            if (validPlanets.Count == 0) break;

            int randomIndex = Random.Range(0, validPlanets.Count);
            PlanetInfo selectedPlanetInfo = validPlanets[randomIndex];
            availablePlanets.Remove(selectedPlanetInfo);

            Vector2 position;
            bool validPosition = FindValidSpawnPosition(selectedPlanetInfo.prefab, out position);
            if (validPosition)
            {
                SpawnPlanet(selectedPlanetInfo, position);
                remainingUnits -= selectedPlanetInfo.units;
            }
            else
            {
                Debug.LogWarning($"Could not spawn planet '{selectedPlanetInfo.name}': No valid position found");
            }
        }

        RepositionPlanets();
    }

    void InitializeAvailablePlanets()
    {
        availablePlanets = new List<PlanetInfo>(planetInfos);
    }

    bool FindValidSpawnPosition(GameObject prefab, out Vector2 position)
{
    Renderer prefabRenderer = prefab.GetComponent<Renderer>();
    if (prefabRenderer == null)
    {
        Debug.LogError($"Prefab {prefab.name} does not have a Renderer component!");
        position = Vector2.zero;
        return false;
    }

    // 1) Calculate the planet’s approximate radius (half the largest dimension).
    float planetSize = Mathf.Max(prefabRenderer.bounds.size.x, prefabRenderer.bounds.size.y);
    float planetRadius = planetSize / 2f;

    // 2) Suppose we allow up to 40% overlap outside playfield. That means 60% must stay in.
    //    So margin = 0.6 * planetRadius. Adjust the 0.6 factor as you see fit
    //    (0.8 => only 20% can be out, 1.0 => fully inside, etc.).
    float overlapFactor = 0.6f;
    float margin = planetRadius * overlapFactor;

    // For each spawn attempt:
    for (int i = 0; i < maxSpawnAttempts; i++)
    {
        // 3) Pick a random x,y within your width/height, offset by ‘margin’
        float randX = Random.Range(-width / 2f + margin, width / 2f - margin);
        float randY = Random.Range(-height / 2f + margin, height / 2f - margin);
        Vector2 candidatePos = new Vector2(randX, randY);

        // 4) Now check if it’s valid => not too close to existing planets
        //    or if you have a central region requirement, etc.
        if (IsValidSpawnPosition(candidatePos, prefab))
        {
            position = candidatePos;
            return true;
        }
    }

    // If none found after all attempts
    position = Vector2.zero;
    return false;
}


    bool IsValidSpawnPosition(Vector2 position, GameObject prefab)
    {
        Renderer prefabRenderer = prefab.GetComponent<Renderer>();
        if (prefabRenderer == null)
        {
            Debug.LogError($"Prefab {prefab.name} does not have a Renderer component!");
            return false;
        }

        float prefabSize = Mathf.Max(prefabRenderer.bounds.size.x, prefabRenderer.bounds.size.y);

        foreach (var planet in spawnedPlanets)
        {
            float minDistance = (prefabSize + planet.size) * minDistanceBetweenPlanets / 2;
            if (Vector2.Distance(position, planet.position) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    void SpawnPlanet(PlanetInfo planetInfo, Vector2 position)
    {
        GameObject planet = Instantiate(planetInfo.prefab, position, Quaternion.identity);
        Planet planetComponent = planet.GetComponent<Planet>() ?? planet.AddComponent<Planet>();
        planetComponent.SetPlanetProperties(planetInfo.name, planetInfo.mass);
        planetComponents.Add(planetComponent);

        planet.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        Renderer planetRenderer = planet.GetComponent<Renderer>();
        float size = Mathf.Max(planetRenderer.bounds.size.x, planetRenderer.bounds.size.y);
        spawnedPlanets.Add(new SpawnedPlanet { gameObject = planet, position = position, size = size });
    }

    void RepositionPlanets()
    {
        for (int i = 0; i < repositionIterations; i++)
        {
            RepositionOverlappingPlanets();
        }

        foreach (var planet in spawnedPlanets)
        {
            planet.gameObject.transform.position = planet.position;
        }
    }

    void RepositionOverlappingPlanets()
    {
        for (int i = 0; i < spawnedPlanets.Count; i++)
        {
            for (int j = i + 1; j < spawnedPlanets.Count; j++)
            {
                SpawnedPlanet p1 = spawnedPlanets[i];
                SpawnedPlanet p2 = spawnedPlanets[j];

                float minDistance = (p1.size + p2.size) * minDistanceBetweenPlanets / 2;
                Vector2 direction = p2.position - p1.position;
                float currentDistance = direction.magnitude;

                if (currentDistance < minDistance)
                {
                    float overlap = minDistance - currentDistance;
                    direction = direction.normalized;

                    Vector2 repositionVector = direction * overlap * repositionForce;
                    p1.position -= repositionVector;
                    p2.position += repositionVector;

                    p1.position = ClampToSpawnArea(p1.position, p1.size);
                    p2.position = ClampToSpawnArea(p2.position, p2.size);
                }
            }
        }
    }

    Vector2 ClampToSpawnArea(Vector2 position, float size)
    {
        float halfSize = size / 2;
        float minX = -width / 2 + halfSize;
        float maxX = width / 2 - halfSize;
        float minY = -height / 2 + halfSize;
        float maxY = height / 2 - halfSize;

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }

    void PlaceShips()
    {
        Vector3 player1Position = GetValidShipPosition(true);
        GameObject player1 = Instantiate(playerShipPrefab, player1Position, Quaternion.Euler(0, 90, -90));
        player1.name = "Player1Ship";
        player1Ship = player1.GetComponent<PlayerShip>();
        player1Ship.playerName = "Player 1";
        player1Ship.isLeftPlayer = true;

        Vector3 player2Position = GetValidShipPosition(false);
        GameObject player2 = Instantiate(playerShipPrefab, player2Position, Quaternion.Euler(0, -90, -90));
        player2.name = "Player2Ship";
        player2Ship = player2.GetComponent<PlayerShip>();
        player2Ship.playerName = "Player 2";
        player2Ship.isLeftPlayer = false;

        // Restore stored scores
        player1Ship.score = storedScore1;
        player2Ship.score = storedScore2;

        // IMPORTANT: Reset movesRemainingThisRound to 1 for each ship
        player1Ship.movesRemainingThisRound = player1Ship.movesAllowedPerTurn;
        player2Ship.movesRemainingThisRound = player2Ship.movesAllowedPerTurn;
        // after you instantiate & configure player1Ship + player2Ship...
        var pm1 = player1Ship.GetComponent<PerkManager>();
        pm1.SetIconSlots(player1PerkIcons);
        Debug.Log($"icon set for player1");
        var pm2 = player2Ship.GetComponent<PerkManager>();
        pm2.SetIconSlots(player2PerkIcons);
        Debug.Log($"icon set for player2");

    }

    Vector3 GetValidShipPosition(bool isLeftSide)
    {
        for (int attempt = 0; attempt < maxShipPlacementAttempts; attempt++)
        {
            Vector3 position = GetRandomShipPosition(isLeftSide);
            if (!ShipOverlapsWithPlanet(position) && IsWithinValidVerticalRange(position.y))
            {
                return position;
            }
        }

        Debug.LogWarning("Could not find a valid position for the ship. Placing at default position.");
        return GetRandomShipPosition(isLeftSide);
    }

    Vector3 GetRandomShipPosition(bool isLeftSide)
    {
        float horizontalPosition = Random.Range(minDistanceFromCenter, maxDistanceFromCenter);
        if (isLeftSide) horizontalPosition = -horizontalPosition;

        float verticalPosition = Random.Range(-height / 2f + topBottomOffset, height / 2f - topBottomOffset);
        return new Vector3(horizontalPosition, verticalPosition, 0);
    }

    private bool ShipOverlapsWithPlanet(Vector3 position)
    {
        foreach (Planet planet in planetComponents)
        {
            SphereCollider planetCollider = planet.GetComponent<SphereCollider>();
            if (planetCollider == null)
            {
                planetCollider = planet.gameObject.AddComponent<SphereCollider>();
                Renderer renderer = planet.GetComponent<Renderer>();
                if (renderer != null)
                {
                    planetCollider.radius = renderer.bounds.extents.magnitude / 2f;
                }
            }

            float distance = Vector3.Distance(position, planet.transform.position);
            float minDist = shipCollisionRadius + planetCollider.radius * planet.transform.localScale.x;
            if (distance < minDist)
            {
                return true;
            }
        }
        return false;
    }

    bool IsWithinValidVerticalRange(float yPosition)
    {
        return (yPosition >= -height / 2f + topBottomOffset &&
                yPosition <=  height / 2f - topBottomOffset);
    }


    void SetPlayerNames(string name1, string name2)
    {
        if (player1Ship != null)
        {
            player1Ship.playerName = name1;
            if (player1NameText != null) player1NameText.text = name1;
        }
        if (player2Ship != null)
        {
            player2Ship.playerName = name2;
            if (player2NameText != null) player2NameText.text = name2;
        }
        UpdateScoreDisplay();
    }

    void SetupPlayerUI()
    {
        // If you are still using the old per-player UI prefab logic
        SetupUIForPlayer(player1Ship);
        SetupUIForPlayer(player2Ship);
    }

    void SetupUIForPlayer(PlayerShip ship)
    {
        // old approach for your “playerUI” prefab
        GameObject uiObject = Instantiate(playerUIPrefab, gameCanvas.transform);
        PlayerUI playerUI = uiObject.GetComponent<PlayerUI>();
        playerUI.Initialize(ship, gameCanvas);
        ship.playerUI = playerUI;
        playerUI.SetActive(false);
        var pm = ship.GetComponent<PerkManager>();
        if (ship.isLeftPlayer)
            pm.SetIconSlots(player1PerkIcons);
        else
            pm.SetIconSlots(player2PerkIcons);
    }

    // ---------------------------------------------------------
    // MAIN GAME PHASE
    // ---------------------------------------------------------
    IEnumerator StartGamePhase()
    {
        Debug.Log("Starting Game Phase");
        yield return new WaitForSeconds(2f);
        StartPreparationPhase(player1Ship);
        SetOverlayAlpha(1f);
        yield return StartCoroutine(FadeOverlay(false));
        player1Ship.GetComponent<PerkManager>().RefreshUI();
        player2Ship.GetComponent<PerkManager>().RefreshUI();
    }

    void StartPreparationPhase(PlayerShip nextPlayer)
    {
        nextPlayer.StopOverTimeEffects();

        // Determine the enemy ship
        PlayerShip enemyShip = (nextPlayer == player1Ship) ? player2Ship : player1Ship;
    
        // Start regeneration on the inactive ship (the enemy)
        enemyShip.StartOverTimeEffects();
        Debug.Log($"Starting Preparation Phase for {nextPlayer.playerName}");
        currentPlayer = nextPlayer;
        turnText.text = $"Starting {currentPlayer.playerName}'s Turn";
        StartCoroutine(PreparationPhase());
    
    }

    IEnumerator PreparationPhase()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountdownTimer($"{currentPlayer.playerName} get ready in", preparationTime));

        if (Mathf.Approximately(transitionOverlay.color.a, 1f))
        {
            yield return StartCoroutine(FadeOverlay(false));
        }
        yield return timerCoroutine;

        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        Debug.Log($"Starting Player Turn for {currentPlayer.playerName}");
        currentPlayer.OnStartOfTurn();
        isTurnActive = true;
        missileFired = false;
        currentPlayer.EnableControls(true);
        currentPlayer.ShowLastMissileTrail();
        turnText.text = $"{currentPlayer.playerName}'s Turn";

        // (Optional) default to fire
        currentPlayer.currentMode = PlayerShip.PlayerActionMode.Fire;
        currentPlayer.GetComponent<PerkManager>()?.ResetPerTurn();
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(TurnTimer());
    }

    IEnumerator TurnTimer()
    {
        float updateInterval = 0.1f;
        float timeLeft = turnDuration;

        bubbleTimer.maxValue = turnDuration;
        bubbleTimer.value = turnDuration;

        while (timeLeft > 0f && isTurnActive)
        {
            timeLeft -= updateInterval;
            if (timeLeft < 0f) timeLeft = 0f;

            bubbleTimer.value = timeLeft;
            timerBubbleText.text = Mathf.CeilToInt(timeLeft).ToString();

            // if the turn ended early, break out
            if (!isTurnActive) break;

            yield return new WaitForSeconds(updateInterval);
        }

        // After the loop ends, if still isTurnActive, it means time ran out
        if (timeLeft <= 0f && isTurnActive)
        {
            EndTurn("No action taken in time!");
        }
    }


    // Called when Fire or Move is used
    public void PlayerActionUsed()
{
    // 1) Let the PerkManager handle any queued perk (fire *or* move)
    currentPlayer.GetComponent<PerkManager>()?.OnActionExecuted();

    // 2) If this was a Move *and* BoostJets asked us to skip ending the turn:
    if (currentPlayer.currentMode == PlayerShip.PlayerActionMode.Move
        && currentPlayer.skipNextMoveEndsTurn)
    {
        // clear the flag and *do nothing else* — keep the turn active
        currentPlayer.skipNextMoveEndsTurn = false;
        // refresh only UI elements that changed (moves bar, icons…)
        UpdateFightingUI_AtRoundStart();
        return;
    }

    // 3) Common “end‐of‐action” logic
    bool wasFire = (currentPlayer.currentMode == PlayerShip.PlayerActionMode.Fire);

    // only fire sets missileFired = true
    missileFired = wasFire;
    isTurnActive = false;
    currentPlayer.EnableControls(false);

    if (activeCoroutine != null) StopCoroutine(activeCoroutine);
    if (timerCoroutine  != null) StopCoroutine(timerCoroutine);

    // clear the timer
    bubbleTimer.value       = 0f;
    timerBubbleText.text    = "0";

    if (wasFire)
    {
        // 4a) If we fired, start the missile flight phase
        missileFlightCoroutine = StartCoroutine(MissileFlightPhase());
    }
    else
    {
        currentPlayer.movesRemainingThisRound--;
        UpdateFightingUI_AtRoundStart();
        // 4b) Move → end turn normally
        EndTurn($"Move used by {currentPlayer.playerName}!");
        // refresh the move icon
        
        if (currentPlayer.isLeftPlayer)
            UpdateMoveIconUI(currentPlayer, player1MoveIcon);
        else
            UpdateMoveIconUI(currentPlayer, player2MoveIcon);
    }
}


    IEnumerator MissileFlightPhase()
    {
        // 1) Grab a reference to the active missile
        //    (We expect one just fired. If not found, we bail out.)
        Missile3D activeMissile = FindObjectOfType<Missile3D>();
        if (activeMissile == null)
        {
            yield break; // no missile => just end
        }

        // 2) The bubbleTimer now depends on the missile's total fuel
        float initialFuel = activeMissile.fuel; // e.g. 100
        bubbleTimer.maxValue = initialFuel;
        bubbleTimer.value    = initialFuel;

        // We'll update the timer 10x per second => dt=0.1
        float updateInterval = 0.1f;

        while (!isTurnActive && missileFired && activeMissile != null && !activeMissile.isDestroyed)
        {
            // NOTE: Fuel is consumed in Missile3D.Update(), we just READ it here for display
            // Update bubble display to show remaining fuel
            bubbleTimer.value = activeMissile.fuel;
            timerBubbleText.text = $"{(int)activeMissile.fuel}";  // e.g. integer lbs

            // Missile3D.Update() handles fuel depletion and self-destruction
            // No need to check or modify fuel here

            yield return new WaitForSeconds(updateInterval);
        }

        // Once we exit the while, either missile was destroyed or turn ended
        // if missile is still alive => do something optional
        // But typically we do nothing; it's ended.
    }


    void OnMissileDestroyed(Missile3D missile)
    {
         if (inMultiMissileMode)
        {
            multiMissilesRemaining--;
            if (multiMissilesRemaining > 0)
            {
                // still missiles in flight → do nothing
                return;
            }
            // that was the last one
            inMultiMissileMode = false;
        }
        // Stop the flight phase if it’s still running
        if (missileFlightCoroutine != null)
        {
            StopCoroutine(missileFlightCoroutine);
            missileFlightCoroutine = null;
        }
        Debug.Log("Missile Destroyed event");
        if (!isTurnActive && missileFired && currentPlayer.currentMode == PlayerShip.PlayerActionMode.Fire)
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);

            EndTurn("Missile destroyed!");
        }
    }
    public void MissileLostInSpace()
    {
        Debug.Log("Missile Lost in Space");
        if (!isTurnActive && missileFired && currentPlayer.currentMode == PlayerShip.PlayerActionMode.Fire)
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            ClearTimerText();
            EndTurn("Missile Lost in Space!");
        }
    }
    void EndTurn(string message)
    {
        Debug.Log($"Ending Turn: {message}");
        currentPlayer.StopEngineLoopIfPlaying();
        isTurnActive = false;
        missileFired = false;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        ShowInfoText(message);
        StartCoroutine(DelayedNextTurn());

        currentPlayer.EnableControls(false);
    }

    IEnumerator DelayedNextTurn()
    {
        yield return new WaitForSeconds(infoFadeDuration);
        PlayerShip nextPlayer = (currentPlayer == player1Ship) ? player2Ship : player1Ship;
        StartPreparationPhase(nextPlayer);
    }

    // ---------------------------------------------------------
    // SHIP DESTROYED / ROUND HANDLING
    // ---------------------------------------------------------
    public void ShipDestroyed(PlayerShip destroyedShip)
    {
        Debug.Log($"{destroyedShip.playerName}'s ship destroyed");

        if (destroyedShip.playerUI != null)
        {
            Destroy(destroyedShip.playerUI.gameObject);
            destroyedShip.playerUI = null;
        }

        PlayerShip winningShip = (destroyedShip == player1Ship) ? player2Ship : player1Ship;
        if (destroyedShip == player1Ship)
        {
            storedScore2++;
            winningShip.score = storedScore2;
        }
        else
        {
            storedScore1++;
            winningShip.score = storedScore1;
        }
        UpdateScoreDisplay();
        StartCoroutine(HandleShipDestruction(destroyedShip, winningShip));
    }

    private IEnumerator HandleShipDestruction(PlayerShip destroyedShip, PlayerShip winningShip)
    {
        SetOverlayAlpha(0f);
        string message = "";
        if (winningShip.score >= winningScore)
        {
            message = $"Game Over!\n{winningShip.playerName} wins the game!";
        }
        else
        {
            currentRound++;
            message = $"{winningShip.playerName} won round {currentRound - 1}.\nPrepare for round {currentRound}";
        }

        yield return new WaitForSecondsRealtime(destructionDelay);
        yield return StartCoroutine(FadeOverlay(true, message));

        if (winningShip.score >= winningScore)
        {
            yield return StartCoroutine(GameOver(winningShip));
        }
        else
        {
            yield return StartCoroutine(StartNextRound(destroyedShip));
        }
    }

    IEnumerator StartNextRound(PlayerShip destroyedShip)
    {
        Debug.Log("Starting Next Round");
        bool lostLeft = destroyedShip.isLeftPlayer;
        ResetForNewRound();
        yield return new WaitForSeconds(4f);
        PlayerShip newStartingPlayer = lostLeft ? player1Ship : player2Ship;
        StartPreparationPhase(newStartingPlayer);
    }

    public void ResetForNewRound()
    {
        ClearAllMissileTrails();
        InitializeGame();

        // destroy leftover UI
        PlayerUI[] leftoverUIs = FindObjectsOfType<PlayerUI>(true);
        foreach (var ui in leftoverUIs)
        {
            Destroy(ui.gameObject);
        }
        SetupPlayerUI();
    }

    IEnumerator GameOver(PlayerShip winner)
    {
        Debug.Log($"Game Over. {winner.playerName} wins!");
        yield return StartCoroutine(FadeOverlay(true, $"Game Over!\n{winner.playerName} wins the game!"));
        yield return new WaitForSeconds(gameOverDuration);
        ResetScores();
        currentRound = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ResetScores()
    {
        storedScore1 = 0;
        storedScore2 = 0;
        if (player1Ship != null) player1Ship.score = 0;
        if (player2Ship != null) player2Ship.score = 0;
        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        // If you are now using separate Player1ScoreText / Player2ScoreText, do:
        if (player1ScoreText != null) player1ScoreText.text = storedScore1.ToString();
        if (player2ScoreText != null) player2ScoreText.text = storedScore2.ToString();

        // Or keep the old one if you want:
        if (scoreText != null)
        {
            scoreText.text = $"{player1Ship.playerName}: {player1Ship.score} | {player2Ship.playerName}: {player2Ship.score}";
        }
    }

    public void ClearAllMissileTrails()
    {
        player1Ship?.ClearLastMissileTrail();
        player2Ship?.ClearLastMissileTrail();
    }

    void SetOverlayAlpha(float alpha)
    {
        transitionOverlay.color = new Color(
            transitionOverlay.color.r,
            transitionOverlay.color.g,
            transitionOverlay.color.b,
            alpha
        );
        transitionText.color = new Color(
            transitionText.color.r,
            transitionText.color.g,
            transitionText.color.b,
            alpha
        );
    }

    IEnumerator FadeOverlay(bool fadeIn, string message = "")
    {
        transitionText.text = message;
        transitionOverlay.gameObject.SetActive(true);

        float elapsedTime = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha   = fadeIn ? 1f : 0f;
        SetOverlayAlpha(startAlpha);

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / transitionDuration);
            SetOverlayAlpha(alpha);
            yield return null;
        }
        SetOverlayAlpha(endAlpha);

        if (!fadeIn)
        {
            transitionOverlay.gameObject.SetActive(false);
        }
    }

    IEnumerator CountdownTimer(string message, float duration)
    {
        float timeLeft = duration;
        bubbleTimer.maxValue = duration;        // set the slider’s max 
        while (timeLeft > 0)
        {
            timeLeft -= 1f;
            bubbleTimer.value = 0;       
            timerBubbleText.text = "";
            timerText.text = $"{message}: {timeLeft:F0}s";
            yield return new WaitForSeconds(1f);
        }
        ClearTimerText();
        bubbleTimer.value = 0f; 
        timerBubbleText.text = "";
    }


    void ShowInfoText(string message)
    {
        StartCoroutine(FadeText(infoText, message, infoFadeDuration));
    }
    void ClearTimerText()
    {
        timerText.text = "";
    }
    IEnumerator FadeText(TextMeshProUGUI textComponent, string message, float duration)
    {
        textComponent.text = message;
        textComponent.alpha = 1;
        yield return new WaitForSeconds(duration);

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            textComponent.alpha = Mathf.Clamp01(1 - elapsedTime / duration);
            yield return null;
        }
        textComponent.text = "";
    }
    private List<PassiveType> GetActivePassives(PlayerShip ship)
    {
        var result = new List<PassiveType>();

        if (ship.sniperMode)                 result.Add(PassiveType.SniperMode);
        if (ship.unmovable)                  result.Add(PassiveType.Unmovable);
        if (ship.enhancedRegeneration)       result.Add(PassiveType.EnhancedRegeneration);
        if (ship.damageResistancePassive)    result.Add(PassiveType.DamageResistance);
        if (ship.criticalImmunity)           result.Add(PassiveType.CriticalImmunity);
        if (ship.CriticalEnhancement)        result.Add(PassiveType.CriticalEnhancement);
        if (ship.damageBoostPassive)         result.Add(PassiveType.DamageBoost);
        if (ship.hasLastChancePassive)       result.Add(PassiveType.LastChance);
        if (ship.adaptiveArmorPassive)       result.Add(PassiveType.AdaptiveArmor);
        if (ship.adaptiveDamagePassive)      result.Add(PassiveType.AdaptiveDamage);
        if (ship.precisionEngineering)       result.Add(PassiveType.PrecisionEngineering);
        if (ship.collisionAvoidancePassive)  result.Add(PassiveType.CollisionAvoidance);
        if (ship.lifestealPassive)           result.Add(PassiveType.Lifesteal);
        if (ship.reduceDamageFromHighSpeedMissiles)
            result.Add(PassiveType.ReduceDamageFromHighSpeedMissiles);
        if (ship.increaseDamageOnHighSpeedMissiles)
            result.Add(PassiveType.IncreaseDamageOnHighSpeedMissiles);

        return result;
    }
    private void PopulatePassivesUI(PlayerShip ship, Transform passivesPanel, GameObject template)
    {
        // Clear out old icons
        foreach (Transform child in passivesPanel)
        {
            // If child is the template, skip it
            if (child.gameObject != template)
                Destroy(child.gameObject);
        }

        // We'll hold all passives that are actually "on" for this ship
        // E.g. read each bool and add the corresponding ID if true.
        List<int> passiveIndices = GetEnabledPassiveIndices(ship);

        // If none, hide the template and return
        template.SetActive(false);
        if (passiveIndices.Count == 0) return;

        // For each enabled passive:
        foreach (int index in passiveIndices)
        {
            // Decide if it's unlocked or not
            // e.g. isUnlocked = (ship.shipLevel >= 10);
            bool isUnlocked = (ship.shipLevel >= 10);

            // Decide which sprite => "Passives 2_x" or "Inactive Passives 2_x"
            Sprite chosenSprite = isUnlocked
                ? passivesActiveIcons[index]
                : passivesInactiveIcons[index];

            // Instantiate
            GameObject clone = Instantiate(template, passivesPanel);
            clone.SetActive(true);

            // Find the "Image" child
            Image iconImg = clone.transform.Find("Image").GetComponent<Image>();
            iconImg.sprite = chosenSprite;
        }
    }
    private List<int> GetEnabledPassiveIndices(PlayerShip ship)
{
    List<int> passives = new List<int>();
    
    // Keep the checks in the exact order of your PassiveType enum:
    
    // 0) SniperMode
    if (ship.sniperMode)
        passives.Add(0);
    
    // 1) Unmovable
    if (ship.unmovable)
        passives.Add(1);
    
    // 2) EnhancedRegeneration
    if (ship.enhancedRegeneration)
        passives.Add(2);
    
    // 3) DamageResistance
    if (ship.damageResistancePassive)
        passives.Add(3);
    
    // 4) CriticalImmunity
    if (ship.criticalImmunity)
        passives.Add(4);
    
    // 5) CriticalEnhancement
    if (ship.CriticalEnhancement)
        passives.Add(5);
    
    // 6) DamageBoost
    if (ship.damageBoostPassive)
        passives.Add(6);
    
    // 7) LastChance
    if (ship.hasLastChancePassive)
        passives.Add(7);
    
    // 8) AdaptiveArmor
    if (ship.adaptiveArmorPassive)
        passives.Add(8);
    
    // 9) AdaptiveDamage
    if (ship.adaptiveDamagePassive)
        passives.Add(9);
    
    // 10) PrecisionEngineering
    if (ship.precisionEngineering)
        passives.Add(10);
    
    // 11) CollisionAvoidance
    if (ship.collisionAvoidancePassive)
        passives.Add(11);
    
    // 12) Lifesteal
    if (ship.lifestealPassive)
        passives.Add(12);
    
    // 13) ReduceDamageFromHighSpeedMissiles
    if (ship.reduceDamageFromHighSpeedMissiles)
        passives.Add(13);
    
    // 14) IncreaseDamageOnHighSpeedMissiles
    if (ship.increaseDamageOnHighSpeedMissiles)
        passives.Add(14);

    return passives;
}


    Sprite GetMoveSprite(MoveType type, bool enabled)
    {
        switch(type)
        {
            case MoveType.Precision: 
                return enabled ? precisionMoveActive : precisionMoveInactive;
            case MoveType.Warp:      
                return enabled ? warpMoveActive      : warpMoveInactive;
            case MoveType.Regular:
            default:                  
                return enabled ? regularMoveActive  : regularMoveInactive;
        }
    }

    /// <summary>
    /// Decides which MoveType this ship uses.
    /// </summary>
    MoveType GetShipMoveType(PlayerShip ship)
    {
        if (ship.warpMove)     return MoveType.Warp;
        if (ship.precisionMove) return MoveType.Precision;
        return MoveType.Regular;
    }

    /// <summary>
    /// Updates one icon based on the ship’s move‐type
    /// and movesRemainingThisRound.
    /// </summary>
    void UpdateMoveIconUI(PlayerShip ship, Image icon)
    {
        // determine regular vs precision vs warp
        MoveType mt = GetShipMoveType(ship);

        // enabled if we still have moves left
        bool enabled = ship.movesRemainingThisRound > 0;

        icon.sprite = GetMoveSprite(mt, enabled);
    }



}
