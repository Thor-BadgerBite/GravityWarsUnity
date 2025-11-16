using UnityEngine;
using System.Collections;

public class PlayerShip : MonoBehaviour
{
    // ---------------------------------------------------
    // 1) NEW: archetype & leveling fields
    // ---------------------------------------------------

    [Header("Ship Preset System")]
    [Tooltip("Ship preset defining this ship's configuration (NEW SYSTEM - required!)")]
    public ShipPresetSO shipPreset;

    [Header("Player Progression (NOT in preset - this is your XP!)")]
    [Tooltip("XP for this specific ship. This is YOUR progress, not ship config!")]
    public float shipXP = 6250f;

    [Tooltip("Ship current level (1..20). Calculated from XP in RecalcLevelFromXP().")]
    public int shipLevel = 1;

    [Tooltip("XP needed to reach the next level. (Derived, not manually set.)")]
    [HideInInspector] public float xpNeededForNextLevel = 0f;

    // ============================================================
    // FIELDS BELOW ARE MANAGED BY SHIP PRESET - DO NOT EDIT IN INSPECTOR!
    // They are hidden from inspector but accessible to code.
    // ============================================================

    [HideInInspector] public ShipArchetype shipArchetype = ShipArchetype.AllAround;  // From ShipBodySO
    [HideInInspector] public string shipModelName = "Star Sparrow";                  // From ShipBodySO
    [HideInInspector] public float baseHealth = 10000f;                              // From ShipBodySO
    [HideInInspector] public float armor = 100f;                                     // From ShipBodySO
    [HideInInspector] public float damageMultiplier = 1f;                            // From ShipBodySO
    [HideInInspector] public float maxHealth = 100f;                                 // Calculated from baseHealth
    [HideInInspector] public int movesAllowedPerTurn = 3;                            // From ShipBodySO

    // Rotation settings (from ShipBodySO)
    [HideInInspector] public float rotationSpeed = 50f;
    [HideInInspector] public float maxTiltAngle = 40f;
    [HideInInspector] public float tiltSpeed = 5f;
    [HideInInspector] public float fineRotationSpeedMultiplier = 0.2f;
    [HideInInspector] public float fineTiltSpeedMultiplier = 0.2f;

    // Movement settings (from MoveTypeSO)
    [HideInInspector] public float minMoveSpeed = 2f;
    [HideInInspector] public float maxMoveSpeed = 10f;
    [HideInInspector] public float moveDeceleration = 4f;
    [HideInInspector] public float moveDuration = 2.5f;

    // Passive flags (from PassiveAbilitySO)
    [HideInInspector] public bool precisionMove = false;
    [HideInInspector] public bool warpMove = false;
    [HideInInspector] public bool sniperMode = true;
    [HideInInspector] public bool unmovable = false;
    [HideInInspector] public bool enhancedRegeneration = false;
    [HideInInspector] public float regenRate = 1f;
    [HideInInspector] public bool damageResistancePassive = false;
    [HideInInspector] public float damageResistancePercentage = 0.15f;
    [HideInInspector] public bool criticalImmunity = false;
    [HideInInspector] public bool CriticalEnhancement = false;
    [HideInInspector] public bool damageBoostPassive = false;
    [HideInInspector] public bool hasLastChancePassive = false;
    [HideInInspector] public bool adaptiveArmorPassive = false;
    [HideInInspector] public bool adaptiveDamagePassive = false;
    [HideInInspector] public bool precisionEngineering = false;
    [HideInInspector] public bool collisionAvoidancePassive = false;
    [HideInInspector] public bool lifestealPassive = false;
    [HideInInspector] public float lifestealPercent = 0.2f;
    [HideInInspector] public bool reduceDamageFromHighSpeedMissiles = false;
    [HideInInspector] public float highSpeedDamageReductionPercent = 0.2f;
    [HideInInspector] public bool increaseDamageOnHighSpeedMissiles = false;
    [HideInInspector] public float highSpeedDamageAmplifyPercent = 0.2f;

    // ============================================================
    // END OF PRESET-MANAGED FIELDS
    // ============================================================

    /// <summary>
    /// Use this if you want a maximum level of 20, or adjust as needed.
    /// </summary>
    private const int MAX_LEVEL = 20;

    private Coroutine regenerationCoroutine;
    private Coroutine damageBoostCoroutine;
    public enum PlayerActionMode { Fire, Move }
    private AudioSource engineLoopSource;
    private bool precisionMoveTemp = false; // the ephemeral toggle for ghost usage
    [HideInInspector] public int score = 0;

    [Header("Player Assignment")]
    public string playerName;
    public bool isLeftPlayer = true;

    [Header("Global Game Settings (same for all ships)")]
    public KeyCode fireKey = KeyCode.Space;
    public float missileSpawnDistance = 2f;
    public float cooldownTime = 1f;
    public int predictionSteps = 100;
    public GameObject missilePrefab;

    [Header("Missile Launch Velocity (fallback if no missile equipped)")]
    [Tooltip("Minimum launch velocity for missiles (fallback default)")]
    public float minLaunchVelocity = 0.1f;
    [Tooltip("Maximum launch velocity for missiles (fallback default)")]
    public float maxLaunchVelocity = 10f;

    [Header("Missile Selection (choose before match)")]
    [Tooltip("The missile type this ship is equipped with (unlimited ammo)")]
    public MissilePresetSO equippedMissile;

    [Tooltip("If true, ignores ship archetype restrictions for testing")]
    public bool ignoreRestrictions = false; 

    // ALLOW ONLY ONCE PER ROUND:
    [HideInInspector] public int movesRemainingThisRound = 1;

    // True once this ship is destroyed; used to prevent multiple collisions awarding multiple points
    [HideInInspector] public bool isDestroyed = false;

    [HideInInspector] public bool controlsEnabled = false;
    [HideInInspector] public int shotsThisRound = 0;
    [HideInInspector] public bool isPassiveUnlocked = false;
    [HideInInspector] public bool skipNextMoveEndsTurn = false;
    [HideInInspector] public bool  nextExplosiveEnabled = false;
    [HideInInspector] public float nextExplRadius       = 0f;
    [HideInInspector] public float nextExplDamageFactor = 1f;
    [HideInInspector] public float nextExplPushStrength = 0f;
    [HideInInspector] public bool nextMultiEnabled = false;
    [HideInInspector] public float nextMultiDamageFactor = 0.75f;  // 75% payload
    [HideInInspector] public float nextMultiSpreadDeg    = 5f;     // ±5°
    [HideInInspector] public bool   nextClusterEnabled       = false;
    [HideInInspector] public float  nextClusterDamageFactor  = 1f;
    [HideInInspector] public float  nextClusterSpreadDeg     = 5f;
    [HideInInspector] public bool  nextPushEnabled           = false;
    [HideInInspector] public float nextPushDamageFactor      = 1f;
    [HideInInspector] public float nextPushKnockbackFactor   = 1f;

    // Runtime state
    private bool lastChanceUsed = false;
    [HideInInspector] public bool isGhost = false;
    [HideInInspector] public float currentHealth;
    private float baseDamageMultiplier;
    private float baseArmorValue;
    private GameObject ghostShipInstance;
    [HideInInspector] public PlayerActionMode currentMode = PlayerActionMode.Fire;
    [HideInInspector] public float launchVelocity;
    [HideInInspector] public float lastFireTime;
    private LineRenderer trajectoryLine;
    private float missileDrag;
    private Vector3 initialPosition;
    private GameObject lastMissileTrailObject;

    private float currentZRotation = 0f;
    private float currentTilt = 0f;
    private bool isFineTuning = false;

    private Rigidbody rb;
    private Quaternion initialRotation;

    [HideInInspector] public PlayerUI playerUI; // assigned externally
    [SerializeField] private float warpZoomDuration    = 0.3f; 
    [SerializeField] private float minScaleFactor     = 0.2f; 
    [SerializeField] private float postWarpShakeTime  = 1.0f;  // how long to shake after zoom-in
    [SerializeField] private float postWarpShakeAngle = 15f;   // max degrees of rotation +/- on X

    // -----------------------
    // Unity Setup
    // -----------------------
    void Start()
    {
        // *** CRITICAL FIX: Apply ship preset FIRST before using any values! ***
        if (shipPreset != null)
        {
            // Apply ship configuration from ScriptableObject preset
            shipPreset.ApplyToShip(this);
            Debug.Log($"<color=green>[{playerName}] Applied ship preset: {shipPreset.shipName}</color>");
        }
        else
        {
            // Fallback to inspector values (backward compatibility)
            Debug.LogWarning($"[{playerName}] No ship preset assigned! Using inspector values (old system).");
        }

        // Store base values AFTER applying preset
        baseArmorValue   = armor;
        baseDamageMultiplier = damageMultiplier;

        // Set initial health
        currentHealth    = baseHealth;
        maxHealth        = baseHealth;

        if (!isGhost)
        {
            ghostShipInstance = Instantiate(gameObject, transform.position, transform.rotation);

            // Make sure we name it clearly
            ghostShipInstance.name = $"{name}_Ghost";
            
            // Mark the clone as “isGhost” so it won’t create its own ghost
            PlayerShip ghostPS = ghostShipInstance.GetComponent<PlayerShip>();
            if (ghostPS != null)
            {
                ghostPS.isGhost = true;
            }

            // Strip off the PlayerShip script (if you want NO ship logic)
            Destroy(ghostPS);

            // Remove collisions, rigidbodies, line renderers, etc.
            Collider[] colliders = ghostShipInstance.GetComponentsInChildren<Collider>();
            foreach (var c in colliders) Destroy(c);

            Rigidbody[] bodies = ghostShipInstance.GetComponentsInChildren<Rigidbody>();
            foreach (var b in bodies) Destroy(b);

            LineRenderer[] lines = ghostShipInstance.GetComponentsInChildren<LineRenderer>();
            foreach (var l in lines) Destroy(l);

            // Make materials translucent
            Renderer[] allRenderers = ghostShipInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in allRenderers)
            {
                var mat = rend.material;
                Color c = mat.color;
                c.a = 0.35f;   // ~35% alpha
                mat.color = c;
            }

            // Hide by default
            ghostShipInstance.SetActive(false);
        }
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // Freeze position by default so we don’t drift
        rb.constraints = RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        initialPosition = transform.position;

        // Initialize launch velocity to mid-range (use equipped missile's range if available)
        if (equippedMissile != null)
        {
            launchVelocity = (equippedMissile.minLaunchVelocity + equippedMissile.maxLaunchVelocity) / 2f;
        }
        else
        {
            launchVelocity = (minLaunchVelocity + maxLaunchVelocity) / 2f;
        }

        controlsEnabled = false;

        SetupTrajectoryLine();
        SetupMissileDrag();
        SetInitialRotation();
        // *** Important ***: Recalc level from the shipXP the user may have set in the Inspector
        RecalcLevelFromXP();
        isPassiveUnlocked = (shipLevel >= 10);
        // Then apply stat scaling
        UpdateStatsFromLevel();
        // Make sure current health is not above new max
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (GameManager.Instance != null)
            GameManager.Instance.UpdateFightingUI_AtRoundStart();
    }
    
    // --------------------------------------
    // 2) XP / Leveling system
    // --------------------------------------

    /// <summary>
    /// Recompute shipLevel from shipXP. Then store xpNeededForNextLevel.
    /// </summary>
    public void RecalcLevelFromXP()
    {
        // We'll do a simple iterative approach until we either run out of XP or hit max level
        shipLevel = 1; // start from 1
        float xpPool = shipXP;

        for (int L = 1; L < MAX_LEVEL; L++)
        {
            float needed = XPNeededForNext(L);
            if (xpPool >= needed)
            {
                xpPool -= needed;
                shipLevel = L + 1;
            }
            else
                break;
            //Debug.Log($"[LevelCalc] Final shipLevel = {shipLevel}, XP to next = {xpNeededForNextLevel}");
        }

        // Now compute how much we need for (shipLevel -> shipLevel+1)
        xpNeededForNextLevel = (shipLevel < MAX_LEVEL) 
            ? XPNeededForNext(shipLevel) 
            : 9999999f; // or 0, since we are maxed
    }

    /// <summary>
    /// For example: XP needed to go from L to L+1. 
    /// Let’s define a small quadratic e.g. 200 + 75*L^2. 
    /// You can tweak or store in a data table.
    /// </summary>
    float XPNeededForNext(int L)
    {
        // If L >= MAX_LEVEL => no further needed
        if (L >= MAX_LEVEL) return 9999999f;
        float required = 200f + 75f * (L * L); 
        return required;
    }

    /// <summary>
    /// After we have final shipLevel, apply a scaling to base stats
    /// that depends on the archetype (and shipLevel).
    /// NEW: Uses ShipLevelingFormulaSO if ship preset is assigned, otherwise fallback to hardcoded formulas.
    /// </summary>
    public void UpdateStatsFromLevel()
    {
        // Try to use ScriptableObject system first
        if (shipPreset != null && shipPreset.GetLevelingFormula() != null)
        {
            UpdateStatsFromPreset();
            return;
        }

        // Fallback to hardcoded formulas (with BALANCE FIXES applied!)
        UpdateStatsFromHardcodedFormulas();
    }

    /// <summary>
    /// Uses ShipLevelingFormulaSO from ship preset (NEW SYSTEM)
    /// </summary>
    private void UpdateStatsFromPreset()
    {
        ShipLevelingFormulaSO formula = shipPreset.GetLevelingFormula();
        if (formula == null)
        {
            Debug.LogWarning($"{playerName}: Ship preset has no leveling formula! Using hardcoded fallback.");
            UpdateStatsFromHardcodedFormulas();
            return;
        }

        // Calculate stats using ScriptableObject formulas
        maxHealth = formula.CalculateHealthAtLevel(baseHealth, shipLevel);
        armor = formula.CalculateArmorAtLevel(baseArmorValue, shipLevel);
        damageMultiplier = formula.CalculateDamageAtLevel(baseDamageMultiplier, shipLevel);

        // Clamp current health
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($"{playerName} (PRESET) => L{shipLevel}, HP={maxHealth:F0}, Armor={armor:F1}, DMGx={damageMultiplier:F2}");
    }

    /// <summary>
    /// Uses hardcoded formulas (LEGACY SYSTEM with BALANCE FIXES)
    /// </summary>
    private void UpdateStatsFromHardcodedFormulas()
    {
        // This "level offset" = how many increments we are above level 1
        int Loffset = shipLevel - 1;
        if (Loffset < 0) Loffset = 0;

        // Apply formulas based on archetype (with BALANCE FIXES!)
        switch (shipArchetype)
        {
            case ShipArchetype.Tank:
                // BALANCE FIX: Reduced damage scaling from 0.02 → 0.015
                maxHealth = baseHealth * (1f + 0.04f * Loffset);
                armor     = baseArmorValue + (4f   * Loffset);
                damageMultiplier = baseDamageMultiplier + (0.015f * Loffset);  // ← NERFED!
                break;

            case ShipArchetype.DamageDealer:
                maxHealth = baseHealth * (1f + 0.02f * Loffset);
                armor = baseArmorValue + (1f   * Loffset);
                damageMultiplier = baseDamageMultiplier + (0.04f * Loffset);
                break;

            case ShipArchetype.AllAround:
                maxHealth = baseHealth * (1f + 0.03f * Loffset);
                armor     = baseArmorValue + (3f * Loffset);
                damageMultiplier = baseDamageMultiplier + (0.03f * Loffset);
                break;

            case ShipArchetype.Controller:
                // BALANCE FIX: Buffed damage scaling from 0.025 → 0.03
                maxHealth = baseHealth * (1f + 0.02f * Loffset);
                armor = baseArmorValue + (2f * Loffset);
                damageMultiplier = baseDamageMultiplier + (0.03f * Loffset);  // ← BUFFED!
                break;
        }

        currentHealth = maxHealth;
        // If the current health is above new max, clamp it
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($"{playerName} (HARDCODED) => L{shipLevel}, HP={maxHealth:F0}, Armor={armor:F1}, DMGx={damageMultiplier:F2}");
    }

    void SetupTrajectoryLine()
    {
        trajectoryLine = GetComponent<LineRenderer>();
        if (trajectoryLine == null)
        {
            trajectoryLine = gameObject.AddComponent<LineRenderer>();
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        }
        trajectoryLine.positionCount = 0;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.startColor = Color.green;
        trajectoryLine.endColor = Color.yellow;
    }

    void SetupMissileDrag()
    {
        if (missilePrefab != null)
        {
            Missile3D missileComp = missilePrefab.GetComponent<Missile3D>();
            if (missileComp != null)
            {
                missileDrag = missileComp.drag;
            }
        }
    }

    void SetInitialRotation()
    {
        if (isLeftPlayer)
        {
            initialRotation = Quaternion.identity;
            currentZRotation = 0f;
        }
        else
        {
            initialRotation = Quaternion.Euler(0, 0, 180);
            currentZRotation = 180f;
        }
        rb.MoveRotation(initialRotation);
    }

    public void Initialize(string name)
    {
        playerName = name;
        if (playerUI == null)
        {
            Debug.LogError($"PlayerUI not assigned for {playerName}!");
        }
    }

    // -----------------------
    // Main Update
    // -----------------------
void Update()
{
    if (!controlsEnabled) return;

    // Keep z=0
    Vector3 pos = transform.position;
    pos.z = 0;
    transform.position = pos;

    // Fine tuning if Shift pressed
    isFineTuning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    // Toggle Fire/Move with M
    if (Input.GetKeyDown(KeyCode.M))
    {
        if (movesRemainingThisRound > 0)
        {
            // If warpMove is true, do warp instantly
            if (warpMove)
            {
                WarpShip();
                // do NOT end the turn here. The user can still do another action (fire, or warp again)
            }
            else
            {
                // normal toggling between Fire and Slingshot move
                bool wasFire = (currentMode == PlayerActionMode.Fire);
                currentMode = wasFire ? PlayerActionMode.Move : PlayerActionMode.Fire;
                Debug.Log($"{playerName} switched mode to {currentMode}");

                if (currentMode == PlayerActionMode.Move)
                {
                    // Hide the line
                    trajectoryLine.positionCount = 0;
                    // Start engine loop if not playing
                    if (engineLoopSource == null)
                    {
                        engineLoopSource = AudioManager.Instance.StartEngineLoop3D(this.transform);
                    }
                }
                else // switched back to Fire
                {
                    // Stop engine loop if running
                    if (engineLoopSource != null)
                    {
                        AudioManager.Instance.StopEngineLoop3D(engineLoopSource);
                        engineLoopSource = null;
                    }
                }
            }
        }
        else
        {
            Debug.Log($"{playerName} has no moves left this round!");
        }
    }


    // If in Move mode, update engine loop pitch each frame
    if (currentMode == PlayerActionMode.Move && engineLoopSource != null)
    {
        float velocityPercent = Mathf.Clamp01(
            (launchVelocity - minLaunchVelocity) / (maxLaunchVelocity - minLaunchVelocity)
        );
        AudioManager.Instance.UpdateEngineLoop(engineLoopSource, velocityPercent);
    }

    // Rotate with Left/Right
    float rotationInput = 0f;
    if (Input.GetKey(KeyCode.LeftArrow))  rotationInput = 1f;   // CCW
    if (Input.GetKey(KeyCode.RightArrow)) rotationInput = -1f;  // CW

    float usedRotationSpeed = isFineTuning ? rotationSpeed * fineRotationSpeedMultiplier : rotationSpeed;
    float rotationAmount = rotationInput * usedRotationSpeed * Time.deltaTime;
    currentZRotation = Mathf.Repeat(currentZRotation + rotationAmount, 360f);
    ApplyTiltAndRotation(rotationInput);

    // Adjust launch velocity - use missile-specific ranges if equipped
    float effectiveMinLaunch = minLaunchVelocity;
    float effectiveMaxLaunch = maxLaunchVelocity;

    if (equippedMissile != null)
    {
        effectiveMinLaunch = equippedMissile.minLaunchVelocity;
        effectiveMaxLaunch = equippedMissile.maxLaunchVelocity;
    }

    float velInput = 0f;
    if (Input.GetKey(KeyCode.UpArrow))   velInput = 1f;
    if (Input.GetKey(KeyCode.DownArrow)) velInput = -1f;
    float vChange = velInput * (effectiveMaxLaunch - effectiveMinLaunch) * 0.5f * Time.deltaTime;
    launchVelocity = Mathf.Clamp(launchVelocity + vChange, effectiveMinLaunch, effectiveMaxLaunch);

    // If Fire mode => show missile line
    if (currentMode == PlayerActionMode.Fire)
    {
        UpdateMissileTrajectoryPreview();
    }
    else
    {
        // Move mode => no line, just ghost if precisionMove
        if (!isGhost && ghostShipInstance != null)
        {
            if (precisionMoveTemp)
            {
                ghostShipInstance.SetActive(true);
                PositionGhostShip();
            }
            else
            {
                ghostShipInstance.SetActive(false);
            }
        }
    }

    // Press Space -> Fire or Move
    if (Input.GetKeyDown(fireKey) && Time.time > lastFireTime + cooldownTime)
    {
        if (currentMode == PlayerActionMode.Fire)
        {
            FireMissile();
        }
        else
        {
            PerformSlingshotMove();
        }
    }

    // Update UI
    playerUI?.UpdateUI();
}



    void ApplyTiltAndRotation(float rotationInput)
    {
        float targetTilt = rotationInput * maxTiltAngle;
        float usedTiltSpeed = isFineTuning ? tiltSpeed * fineTiltSpeedMultiplier : tiltSpeed;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * usedTiltSpeed);

        // Combine Z rotation + X tilt
        Quaternion zRot = Quaternion.Euler(0, 0, currentZRotation);
        Quaternion tiltRot = Quaternion.AngleAxis(currentTilt, Vector3.right);
        Quaternion newRot = zRot * tiltRot;
        rb.MoveRotation(newRot);
    }

    // ----------------------
    // Fire Mode
    // ----------------------
    void UpdateMissileTrajectoryPreview()
    {
        if (!trajectoryLine) return;

        float angle = GetFiringAngle() * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        Vector3 startPos = transform.position + dir * missileSpawnDistance;
        Vector3 initVel = dir * launchVelocity;
        PredictMissileTrajectory(startPos, initVel);
    }

    void PredictMissileTrajectory(Vector3 initialPos, Vector3 initialVel)
    {
        // Use cached planets for performance (avoids expensive FindObjectsOfType every frame!)
        Planet[] planets = GameManager.GetCachedPlanets();
        Vector3 currentPos = initialPos;
        Vector3 currentVel = initialVel * 0.5f;

        // Decide how many steps to use.
        // If sniperMode is true: use full predictionSteps
        // If false: use half (rounded down).
        int usedSteps = sniperMode ? predictionSteps : predictionSteps / 2;

        trajectoryLine.positionCount = usedSteps + 1;
        trajectoryLine.SetPosition(0, currentPos);

        float stepTime = Time.fixedDeltaTime;

        // Use equipped missile preset stats for prediction, or fallback to prefab
        float missileMass, missileDragCoef, maxVel, velApproachRate;
        if (equippedMissile != null)
        {
            missileMass = equippedMissile.Mass;  // Use calculated physics mass
            missileDragCoef = equippedMissile.drag;
            maxVel = equippedMissile.maxVelocity;
            velApproachRate = equippedMissile.velocityApproachRate;
        }
        else if (missilePrefab != null)
        {
            Missile3D m3d = missilePrefab.GetComponent<Missile3D>();
            missileMass = m3d.missileMass;
            missileDragCoef = m3d.drag;
            maxVel = m3d.maxVelocity;
            velApproachRate = m3d.velocityApproachRate;
        }
        else
        {
            // Fallback defaults (Standard missile values)
            missileMass = 1.5f;  // Standard missile physics mass
            missileDragCoef = 0.01f;
            maxVel = 10f;
            velApproachRate = 0.1f;
        }

        for (int i = 1; i <= usedSteps; i++)
        {
            Vector3 totalForce = Vector3.zero;
            foreach (Planet planet in planets)
            {
                totalForce += planet.CalculateGravitationalPull(currentPos, missileMass);
            }
            // CRITICAL: Divide by mass to match rb.AddForce(force, ForceMode.Force) behavior
            // This makes mass cancel out, ensuring prediction matches actual flight
            currentVel += (totalForce / missileMass) * stepTime;
            currentVel *= (1 - missileDragCoef * stepTime);

            // clamp velocity if > maxVelocity
            if (currentVel.magnitude > maxVel)
            {
                Vector3 dir = currentVel.normalized;
                currentVel = Vector3.Lerp(currentVel, dir * maxVel, velApproachRate);
            }

            currentPos += currentVel * stepTime;
            trajectoryLine.SetPosition(i, currentPos);
        }
    }


    void FireMissile()
{
    lastFireTime = Time.time;
    // NOTE: PlayerActionUsed() moved to END of function to ensure missile is spawned first

    // 1) Capture the angle you want to fire at:
    float baseAngle    = GetFiringAngle();
    float baseRadians  = baseAngle * Mathf.Deg2Rad;
    Vector3 baseDir    = new Vector3(Mathf.Cos(baseRadians), Mathf.Sin(baseRadians), 0f);
    Vector3 spawnPos   = transform.position + baseDir * missileSpawnDistance;
    // --- CLUSTER MISSILE BRANCH ---
    if (nextClusterEnabled && missilePrefab != null)
    {
        var obj = Instantiate(missilePrefab, spawnPos, Quaternion.Euler(0,0,baseAngle));
        var m   = obj.GetComponent<Missile3D>();

        // Apply equipped missile preset stats
        ApplyMissilePreset(m);

        // mark it as cluster-capable
        m.isCluster              = true;
        m.clusterDamageFactor    = nextClusterDamageFactor;
        m.clusterSpreadDeg       = nextClusterSpreadDeg;
        Debug.Log($"{playerName} fired a cluster missile (×{m.clusterDamageFactor}, ±{m.clusterSpreadDeg}°)");
        nextClusterEnabled       = false;
        m.Launch(baseDir, launchVelocity, this.gameObject, this.damageMultiplier);
        trajectoryLine.positionCount = 0;
        GameManager.Instance.PlayerActionUsed();  // Moved here - after missile spawn
        return;
    }
    // --- PUSHER MISSILE BRANCH ---
    if (nextPushEnabled && missilePrefab != null)
    {
        var obj = Instantiate(missilePrefab, spawnPos, Quaternion.Euler(0,0,baseAngle));
        var m   = obj.GetComponent<Missile3D>();

        // Apply equipped missile preset stats
        ApplyMissilePreset(m);

        // scale damage
        m.payload *= nextPushDamageFactor;

        // scale its built-in push-on-hit force
        m.pushStrength *= nextPushKnockbackFactor;

        Debug.Log($"Fired pusher missile: dmg×{nextPushDamageFactor}, knockback×{nextPushKnockbackFactor}");

        // clear the flag and fire
        nextPushEnabled = false;
        trajectoryLine.positionCount = 0;
        m.Launch(baseDir, launchVelocity, this.gameObject, this.damageMultiplier);
        GameManager.Instance.PlayerActionUsed();  // Moved here - after missile spawn
        return;
    }

    // --- MULTI MISSILE BRANCH ---
    if (nextMultiEnabled && missilePrefab != null)
    {   GameManager.Instance.BeginMultiMissile(3);
        float spread = nextMultiSpreadDeg;  // e.g. ±5°
        // we want offsets [-spread, 0, +spread]
        for (int i = 0; i < 3; i++)
        {
            float thisAngle = baseAngle + (i - 1) * spread;    // i=0 => -spread, i=1 => 0, i=2 => +spread
            float thisRad   = thisAngle * Mathf.Deg2Rad;
            Vector3 thisDir = new Vector3(Mathf.Cos(thisRad),
                                          Mathf.Sin(thisRad),
                                          0f);

            // spawn & orient
            var go = Instantiate(missilePrefab,
                                 spawnPos,
                                 Quaternion.Euler(0, 0, thisAngle));
            var m  = go.GetComponent<Missile3D>();

            // Apply equipped missile preset stats
            ApplyMissilePreset(m);

            // scale payload:
            m.payload *= nextMultiDamageFactor;

            // destroy the extra‐trail on the side missiles
            if (i != 1)
            {
                var trailLR = go.GetComponentInChildren<LineRenderer>();
                if (trailLR != null)
                    Destroy(trailLR.gameObject);
            }

            // finally launch each one
            m.Launch(thisDir,
                     launchVelocity,
                     this.gameObject,
                     this.damageMultiplier);
        }

        // reset flag and bail out
        nextMultiEnabled        = false;
        trajectoryLine.positionCount = 0;
        GameManager.Instance.PlayerActionUsed();  // Moved here - after missile spawn
        return;
    }

    // --- SINGLE MISSILE BRANCH ---
    if (missilePrefab != null)
    {
        var missileObj = Instantiate(missilePrefab,
                                     spawnPos,
                                     Quaternion.Euler(0, 0, baseAngle));
        var missile = missileObj.GetComponent<Missile3D>();

        // Apply equipped missile preset stats
        ApplyMissilePreset(missile);

        if (nextExplosiveEnabled)
        {
            missile.detRadius         = nextExplRadius;
            missile.detDamageFactor   = nextExplDamageFactor;
            missile.pushStrength      = nextExplPushStrength;
            Debug.Log($"{playerName} fired an explosive missile with radius {nextExplRadius} and damage factor {nextExplDamageFactor}");
            nextExplosiveEnabled      = false;
        }

        if (missile != null)
            missile.Launch(baseDir,
                           launchVelocity,
                           this.gameObject,
                           this.damageMultiplier);
    }

    trajectoryLine.positionCount = 0;
    GameManager.Instance.PlayerActionUsed();  // Moved here - after missile spawn
}

    /// <summary>
    /// Applies the equipped missile preset stats to a spawned missile instance
    /// </summary>
    private void ApplyMissilePreset(Missile3D missile)
    {
        if (equippedMissile != null)
        {
            equippedMissile.ApplyToMissile(missile);
        }
        else
        {
            Debug.LogWarning($"{playerName}: No missile preset equipped! Using default missile stats.");
        }
    }

    /// <summary>
    /// Checks if this ship archetype can use the given missile type
    /// </summary>
    public bool CanUseMissile(MissilePresetSO missile)
    {
        if (ignoreRestrictions) return true;
        if (missile == null) return false;

        switch (shipArchetype)
        {
            case ShipArchetype.Tank:
                // Tanks can only use Medium and Heavy missiles
                return missile.missileType == MissileType.Medium ||
                       missile.missileType == MissileType.Heavy;

            case ShipArchetype.DamageDealer:
                // Damage dealers can use all types
                return true;

            case ShipArchetype.AllAround:
                // All-around ships can use all types
                return true;

            case ShipArchetype.Controller:
                // Controllers can only use Light and Medium missiles
                return missile.missileType == MissileType.Light ||
                       missile.missileType == MissileType.Medium;

            default:
                return true;
        }
    }

    /// <summary>
    /// Gets a description of what missile types this ship can use
    /// </summary>
    public string GetAllowedMissileTypes()
    {
        switch (shipArchetype)
        {
            case ShipArchetype.Tank:
                return "Medium, Heavy";
            case ShipArchetype.DamageDealer:
                return "Light, Medium, Heavy";
            case ShipArchetype.AllAround:
                return "Light, Medium, Heavy";
            case ShipArchetype.Controller:
                return "Light, Medium";
            default:
                return "All";
        }
    }

    // ----------------------
    // Move Mode (Slingshot)
    // ----------------------
    void UpdateMoveLinePreview()
    {
        if (!trajectoryLine) return;

        // We just draw a short thick arrow
        trajectoryLine.positionCount = 2;
        trajectoryLine.startWidth = 0.2f;
        trajectoryLine.endWidth = 0.2f;

        float angle = GetFiringAngle() * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

        float arrowLen = 2f;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + dir * arrowLen;

        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
    }

    void PerformSlingshotMove()
    {
        if (movesRemainingThisRound <= 0)
        {
            Debug.Log($"{playerName} has NO moves left this round!");
            return;
        }

        lastFireTime = Time.time;
        // Immediately hide ghost if it exists
        precisionMoveTemp = false;
        if (ghostShipInstance != null)
        {
            ghostShipInstance.SetActive(false);
        }
;
        AudioManager.Instance.StopEngineLoop3D(engineLoopSource);
        engineLoopSource = null;
        // PLAY THE ONE-SHOT SOUND:
        // Example: 3D at ship’s position
        AudioManager.Instance.PlaySlingshotSound();
        rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        float angle = GetFiringAngle() * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

        // Get effective launch velocity ranges (missile-specific if equipped)
        float effectiveMinLaunch = minLaunchVelocity;
        float effectiveMaxLaunch = maxLaunchVelocity;
        if (equippedMissile != null)
        {
            effectiveMinLaunch = equippedMissile.minLaunchVelocity;
            effectiveMaxLaunch = equippedMissile.maxLaunchVelocity;
        }

        // 1) Find how far along the "velocity" slider we are (0..1)
        float velocityPercent =
            (launchVelocity - effectiveMinLaunch)
            / (effectiveMaxLaunch - effectiveMinLaunch);
        // clamp just in case
        velocityPercent = Mathf.Clamp01(velocityPercent);
        // if we’re in Move Mode, update the engine loop pitch
        if (currentMode == PlayerActionMode.Move && engineLoopSource != null)
        {
            AudioManager.Instance.UpdateEngineLoop(engineLoopSource, velocityPercent);
        }

        // 2) Lerp to get the base slingshot speed
        float baseSlingshotSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, velocityPercent);

        // 3) Add ±10% random flair
        float randomFactor = Random.Range(0.9f, 1.1f);
        float finalSpeed = baseSlingshotSpeed * randomFactor;

        Debug.Log($"Slingshot speed for {playerName}: {finalSpeed:F2} " +
                $"(base={baseSlingshotSpeed:F2}, randomFactor={randomFactor:F2})");

        // movesRemainingThisRound--;
        // moves are now consumed by the PerkManager when it sees a “move performed”
        UpdateMovesUI();
        StartCoroutine(SlingshotCoroutine(dir * finalSpeed));
    }


    IEnumerator SlingshotCoroutine(Vector3 startVelocity)
    {
        rb.velocity = startVelocity;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            if (rb.velocity.magnitude < 0.1f)
            {
                rb.velocity = Vector3.zero;
                break;
            }
            Vector3 decel = -rb.velocity.normalized * moveDeceleration * Time.deltaTime;
            rb.velocity += decel;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Stop ship and refreeze
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezePosition |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        trajectoryLine.positionCount = 0;
        GameManager.Instance.PlayerActionUsed();
    }

    // ----------------------
    // Collision => Destroy
    // ----------------------
    /// <summary>
    /// If we collide with a planet, we do a single destruction event.
    /// We ensure that if `isDestroyed` is already true, we ignore.
    /// </summary>
void OnCollisionEnter(Collision collision)
{
    if (isDestroyed) return; // make sure this only happens once

    Planet planet = collision.collider.GetComponent<Planet>();
    if (planet != null)
    {
        Debug.Log($"{playerName} collided with {planet.planetName} -> destroyed");

        // 1) “Explode” the ship or visually hide it
        DestroyShipWithExplosion(collision.contacts[0].point);

        // 2) Let GameManager handle round logic & UI cleanup
        GameManager.Instance.ShipDestroyed(this);
    }
}


    // ----------------------
    // Helpers
    // ----------------------
    public float GetFiringAngle()
    {
        float zRot = currentZRotation;
        if (zRot > 180f) zRot -= 360f;
        return zRot;
    }

    public void EnableControls(bool enable = true)
    {
        controlsEnabled = enable;
        playerUI?.SetActive(enable);
    }

    /// <summary>
    /// For external calls that just want to forcibly kill the ship (no explosion effect).
    /// </summary>
    public void DestroyShip()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // if engine loop is playing, stop it
        if (engineLoopSource != null)
        {
            AudioManager.Instance.StopEngineLoop3D(engineLoopSource);
            engineLoopSource = null;
        }

        EnableControls(false);
        GetComponent<Renderer>().enabled = false;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;
        AudioManager.Instance.PlayShipExplosion();
    }


    /// <summary>
    /// Same as DestroyShip(), but spawns an explosion effect.
    /// </summary>
    public void DestroyShipWithExplosion(Vector3 impactPoint)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Hide / disable
        EnableControls(false);
        Renderer rend = GetComponent<Renderer>();
        if (rend) rend.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        AudioManager.Instance.PlayShipExplosion();

        // Optionally do the same explosion effect the missile does:
        CreateExplosionEffect(impactPoint);

        // If you want to do camera zoom or slow motion, 
        // you could call a camera controller method here, e.g.:
        //  Camera.main.GetComponent<CameraController>().FocusOnShipExplosion(impactPoint);

        // Also apply the "break apart" logic if you like (child parts, etc.)
        ExplodeShip(impactPoint);
    }

    /// <summary>
    /// Creates a simple "explosion" particle effect at the given point, 
    /// just like the missile's code. 
    /// (Copied from the missile if you want consistency.)
    /// </summary>
    private void CreateExplosionEffect(Vector3 position)
    {
        GameObject explosionObj = new GameObject("ShipExplosionEffect");
        explosionObj.transform.position = position;

        ParticleSystem explosionSystem = explosionObj.AddComponent<ParticleSystem>();
        var main = explosionSystem.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.5f;
        main.maxParticles = 500;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = explosionSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 500) });

        var shape = explosionSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var colorOverLifetime = explosionSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.yellow, 0.2f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                new GradientColorKey(Color.red, 0.8f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = explosionSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

        // Renderer
        var psRenderer = explosionSystem.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psRenderer.material.mainTexture = CreateExplosionTexture();

        explosionSystem.Play();
        Destroy(explosionObj, main.duration + main.startLifetime.constantMax);
    }

    /// <summary>
    /// Creates a simple radial fade texture for the explosion.
    /// </summary>
    private Texture2D CreateExplosionTexture()
    {
        int resolution = 32;
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] colors = new Color[resolution * resolution];

        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(1f - distance / radius);
                float alpha = t * t * (3f - 2f * t); // SmoothStep
                colors[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    public void HideShip()
    {
        Renderer rend = GetComponent<Renderer>();
        Collider coll = GetComponent<Collider>();
        if (rend != null) rend.enabled = false;
        if (coll != null) coll.enabled = false;
        Debug.Log($"{playerName}'s ship hidden");
    }

    public void ShowShip()
    {
        Renderer rend = GetComponent<Renderer>();
        Collider coll = GetComponent<Collider>();
        if (rend != null) rend.enabled = true;
        if (coll != null) coll.enabled = true;
        Debug.Log($"{playerName}'s ship visible");
    }

    public void IncrementShotCounter()
    {
        shotsThisRound++;
    }

    public void ClearLastMissileTrail()
    {
        if (lastMissileTrailObject != null)
        {
            Destroy(lastMissileTrailObject);
            lastMissileTrailObject = null;
        }
    }

    public void SetLastMissileTrail(GameObject trailObject)
    {
        if (lastMissileTrailObject != null)
        {
            Destroy(lastMissileTrailObject);
        }
        lastMissileTrailObject = trailObject;
    }

    public void ShowLastMissileTrail()
    {
        if (lastMissileTrailObject != null)
        {
            lastMissileTrailObject.SetActive(shotsThisRound > 0);
        }
    }

    public void HideTrajectoryLine()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 0; // clear the line
        }
        if (lastMissileTrailObject != null)
        {
            lastMissileTrailObject.SetActive(false);
        }
    }

    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    /// <summary>
    /// This is the “break apart ship” logic you already have. Called from DestroyShipWithExplosion.
    /// </summary>
    public void ExplodeShip(Vector3 impactPoint)
    {
        Debug.Log($"Exploding ship {playerName} at {impactPoint}");
        Transform[] parts = GetComponentsInChildren<Transform>();
        foreach (Transform part in parts)
        {
            if (part == this.transform) continue;

            Rigidbody rbPart = part.GetComponent<Rigidbody>();
            if (rbPart == null)
                rbPart = part.gameObject.AddComponent<Rigidbody>();

            rbPart.isKinematic = false;
            rbPart.useGravity = false;
            rbPart.constraints = RigidbodyConstraints.None;

            float explosionForce = 5f;
            float explosionRadius = 2f;
            float upwardsModifier = 0.01f;
            rbPart.AddExplosionForce(explosionForce, impactPoint, explosionRadius, upwardsModifier, ForceMode.Impulse);
            rbPart.AddTorque(Random.insideUnitSphere * Random.Range(0.5f, 2f), ForceMode.Impulse);

            part.parent = null; // detach from main ship
        }

        Renderer parentRenderer = GetComponent<Renderer>();
        if (parentRenderer != null) parentRenderer.enabled = false;
        Collider parentCollider = GetComponent<Collider>();
        if (parentCollider != null) parentCollider.enabled = false;
    }
    public void StopEngineLoopIfPlaying()
    {
        if (engineLoopSource != null)
        {
            AudioManager.Instance.StopEngineLoop3D(engineLoopSource);
            engineLoopSource = null;
        }
    }
    private void PositionGhostShip()
    {
        // We'll do a short time-step sim of the final slingshot
        float stepTime = 0.05f;
        float simTime = 0f;
        float simulatedDuration = moveDuration;

        // Get effective launch velocity ranges (missile-specific if equipped)
        float effectiveMinLaunch = minLaunchVelocity;
        float effectiveMaxLaunch = maxLaunchVelocity;
        if (equippedMissile != null)
        {
            effectiveMinLaunch = equippedMissile.minLaunchVelocity;
            effectiveMaxLaunch = equippedMissile.maxLaunchVelocity;
        }

        // Calculate final speed ignoring ±10% random factor for simpler preview
        float velocityPercent = Mathf.Clamp01(
            (launchVelocity - effectiveMinLaunch) / (effectiveMaxLaunch - effectiveMinLaunch)
        );
        float baseSlingshotSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, velocityPercent);
        float finalSpeed = baseSlingshotSpeed;

        Vector3 dir = Quaternion.Euler(0, 0, GetFiringAngle()) * Vector3.right;
        Vector3 pos = transform.position;
        Vector3 vel = dir.normalized * finalSpeed;

        while (simTime < simulatedDuration && vel.magnitude > 0.1f)
        {
            // decelerate
            Vector3 decel = -vel.normalized * moveDeceleration * stepTime;
            vel += decel;
            if (vel.magnitude < 0.1f) vel = Vector3.zero;

            pos += vel * stepTime;
            simTime += stepTime;
        }

        ghostShipInstance.transform.position = pos;

        // rotate ghost to final heading
        if (vel.magnitude > 0.01f)
        {
            float finalAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            ghostShipInstance.transform.rotation = Quaternion.Euler(0, 0, finalAngle);
        }
        else
        {
            // fallback
            ghostShipInstance.transform.rotation = transform.rotation;
        }
    }
    public void OnStartOfTurn()
    {
        //movesRemainingThisRound = movesAllowedPerTurn;  // if you added movesAllowedPerTurn
        if (precisionMove)
        {
            precisionMoveTemp = true; // automatically allow ghost if perk is on
        }
        else
        {
            precisionMoveTemp = false;
        }
    }
   public void WarpShip()
{
    if (movesRemainingThisRound <= 0)
    {
        Debug.Log($"{playerName} has no moves left for warp!");
        return;
    }

    AudioManager.Instance.StopEngineLoop3D(engineLoopSource);
    engineLoopSource = null;

    movesRemainingThisRound--;
    UpdateMovesUI();
    if (!FindWarpPosition(out Vector3 warpDestination))
    {
        Debug.Log($"Ship {playerName} couldn't find a valid warp spot. Doing nothing.");
        return;
    }

    // Start a coroutine that does zoom-out → warp → zoom-in → final shake
    StartCoroutine(WarpSequence(warpDestination));
}

    // The method that tries to find a random valid warp position
    // Similar to how you spawn the ship initially
    bool FindWarpPosition(out Vector3 position)
    {
        int maxAttempts = 20;

        float maxDist = GameManager.Instance.maxDistanceFromCenter;
        float minDist = GameManager.Instance.minDistanceFromCenter;
        float offset = GameManager.Instance.topBottomOffset;

        // <-- here we use the property for height
        float halfHeight = GameManager.Instance.Height / 2f;

        for (int i = 0; i < maxAttempts; i++)
        {
            float xPos;
            if (isLeftPlayer)
                xPos = Random.Range(-maxDist, -minDist);
            else
                xPos = Random.Range(minDist, maxDist);

            float yMin = -halfHeight + offset;
            float yMax =  halfHeight - offset;
            float yPos = Random.Range(yMin, yMax);

            Vector3 candidatePos = new Vector3(xPos, yPos, 0);

            if (!ShipOverlapsWithPlanet(candidatePos))
            {
                position = candidatePos;
                return true;
            }
        }
        position = Vector3.zero;
        return false;
    }

    bool ShipOverlapsWithPlanet(Vector3 testPos)
    {
        // Use cached planets for performance
        Planet[] planets = GameManager.GetCachedPlanets();
        foreach (Planet planet in planets)
        {
            SphereCollider planetCollider = planet.GetComponent<SphereCollider>();
            if (planetCollider == null)
            {
                planetCollider = planet.gameObject.AddComponent<SphereCollider>();
                // set radius from planet's size if needed
            }

            float distance = Vector3.Distance(testPos, planet.transform.position);

            // <-- use GameManager.Instance.shipCollisionRadius
            float minDist = GameManager.Instance.shipCollisionRadius 
                        + planetCollider.radius * planet.transform.localScale.x;

            if (distance < minDist)
            {
                return true;
            }
        }
        return false;
    }


    private IEnumerator WarpSequence(Vector3 warpDestination)
{
    // 1) Zoom-out from scale=1.0 => scale=minScaleFactor
    yield return StartCoroutine(ZoomAnimation(fromScale: 1f, toScale: minScaleFactor, warpZoomDuration));

    // 2) Warp
    transform.position = warpDestination;
    // Keep your orientation. No forced rotation.

    // 3) Zoom-in from scale=minScaleFactor => scale=1.0
    yield return StartCoroutine(ZoomAnimation(fromScale: minScaleFactor, toScale: 1f, warpZoomDuration));

    // 4) Shake for postWarpShakeTime seconds
    yield return StartCoroutine(ShakeAnimation(postWarpShakeTime, postWarpShakeAngle));

    // Optionally play a SFX after warp completes
    AudioManager.Instance.PlaySlingshotSound();
    Debug.Log($"{playerName} warped to {warpDestination} (moves left: {movesRemainingThisRound}).");
}
    private IEnumerator ZoomAnimation(float fromScale, float toScale, float duration)
{
    Vector3 originalScale = transform.localScale;
    Quaternion originalRot = transform.localRotation;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        float scaleFactor = Mathf.Lerp(fromScale, toScale, t);
        transform.localScale = Vector3.one * scaleFactor;

        // Keep rotation exactly as it was at the start (no shaking yet).
        transform.localRotation = originalRot;

        yield return null;
    }

    // Snap to final
    transform.localScale = Vector3.one * toScale;
    transform.localRotation = originalRot;
}
    public void UpdateMovesUI()
    {
        if (isLeftPlayer)
        {
            GameManager.Instance.moves1Bar.value = movesRemainingThisRound;
            // Optionally remove or gray out "move chunk" objects. 
        }
        else
        {
            GameManager.Instance.moves2Bar.value = movesRemainingThisRound;
        }
    }
private IEnumerator ShakeAnimation(float shakeDuration, float maxAngle)
{
    // We store original scale & rotation, so we can reapply them after each random offset
    Vector3 originalScale = transform.localScale;
    Quaternion originalRot = transform.localRotation;

    float elapsed = 0f;
    while (elapsed < shakeDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / shakeDuration;

        // E.g. from 100% shake at start down to 0% at end
        float fade = 1f - t;  // or keep it constant if you want uniform shaking the entire time

        float rx = Random.Range(-maxAngle, maxAngle) * fade;
        float rz = Random.Range(-maxAngle, maxAngle) * fade;
        transform.localRotation = originalRot * Quaternion.Euler(rx, 0f, 0f);


        // Keep scale at 1.0, or if you want a small bounce in scale, you can do similar approach
        transform.localScale = originalScale;

        yield return null;
    }

    // Reset rotation fully at the end
    transform.localRotation = originalRot;
    transform.localScale    = originalScale;
}
    public void TakeDamage(float rawDamage, Vector3 hitPoint)
    {
        if (isDestroyed) return;

        // Calculate damage reduction using fixed armor:
        float armorReduction = armor / (armor + 400f); // e.g., 100/(100+400) = 0.2 or 20% reduction
        // Base effective damage from missile:
        float effectiveDamage = rawDamage * (1f - armorReduction);

        // If the ship has the damage resistance passive, reduce damage further.
        if (damageResistancePassive && isPassiveUnlocked)
        {
            effectiveDamage *= (1f - damageResistancePercentage);
        }
        // Check for Last Chance if the ship has the passive
        if (hasLastChancePassive && currentHealth - effectiveDamage <= 0f && isPassiveUnlocked)
        {
            if (!lastChanceUsed)
            {
                lastChanceUsed = true;
                currentHealth = 1f;
                Debug.Log($"{playerName} Last Chance activated! Health set to 1 instead of dying.");
                UpdateHealthUI();
                return; // Exit early so the ship survives this hit.
            }
        }
        currentHealth -= effectiveDamage;
        Debug.Log($"{playerName} took {effectiveDamage} damage! Remaining health = {currentHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Debug.Log($"{playerName} reached 0 health -> destroyed!");
            DestroyShipWithExplosion(hitPoint);
            GameManager.Instance.ShipDestroyed(this);
        }
        else
        {
            StartCoroutine(StabilizeRotationOverTime(5f, 2f));
        }
    }


    private void UpdateHealthUI()
    {
        if (isLeftPlayer)
        {
            GameManager.Instance.health1Bar.maxValue = maxHealth;
            GameManager.Instance.health1Bar.value = currentHealth;
            GameManager.Instance.health1Text.text = $"{currentHealth}/{maxHealth}";
        }
        else
        {
            GameManager.Instance.health2Bar.maxValue = maxHealth;
            GameManager.Instance.health2Bar.value = currentHealth;
            GameManager.Instance.health2Text.text = $"{currentHealth}/{maxHealth}";
        }
    }

private IEnumerator StabilizeRotationOverTime(float totalDuration, float delayBeforeStabilization)
{
    // Wait for a short delay so the ship can spin naturally.
    yield return new WaitForSeconds(delayBeforeStabilization);

    // Record the desired rotation (keeping the last Z value, with X and Y set to 0).
    Quaternion targetRotation = Quaternion.Euler(0, 0, currentZRotation);
    
    // Set PD controller gains (tune these values to get the desired effect)
    float Kp = 50f; // proportional gain (corrects angular error)
    float Kd = 5f;  // derivative gain (damps the angular velocity)
    
    float elapsed = 0f;
    float stabilizationTime = totalDuration - delayBeforeStabilization; // remaining time for stabilization

    while (elapsed < stabilizationTime)
    {
        // Compute the difference between the current rotation and target rotation.
        // deltaRotation represents the rotation needed to get from current to target.
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        
        // Make sure we get the shortest angle
        if (angleInDegrees > 180f)
        {
            angleInDegrees = 360f - angleInDegrees;
        }
        
        // Convert the angle error to radians for proper scaling.
        float angleError = angleInDegrees * Mathf.Deg2Rad;
        
        // PD controller: apply corrective torque proportional to the error and subtract damping.
        Vector3 correctiveTorque = Kp * angleError * axis - Kd * rb.angularVelocity;
        rb.AddTorque(correctiveTorque, ForceMode.Force);
        
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    // Optionally, when done, zero the angular velocity to "lock in" the final state.
    rb.angularVelocity = Vector3.zero;
    // And snap to the target rotation:
    rb.MoveRotation(targetRotation);
}



    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }


    public void StartOverTimeEffects()
    {
        if (damageBoostPassive && damageBoostCoroutine == null && isPassiveUnlocked)
        {
            damageBoostCoroutine = StartCoroutine(DamageBoostCoroutine());
            //Debug.Log($"{playerName} Damage Boost started.");
        }
        if (enhancedRegeneration && regenerationCoroutine == null && isPassiveUnlocked)
        {
            regenerationCoroutine = StartCoroutine(RegenerationCoroutine());
            //Debug.Log($"{playerName} Regeneration started.");
        }
    }

    /// <summary>
    /// Stops any overtime effects.
    /// This should be called when the enemy turn ends (or when the ship becomes active).
    /// Note: We do not reset damageMultiplier here.
    /// </summary>
    public void StopOverTimeEffects()
    {
        if (damageBoostCoroutine != null)
        {
            StopCoroutine(damageBoostCoroutine);
            damageBoostCoroutine = null;
        }
        if (regenerationCoroutine != null)
        {
            StopCoroutine(regenerationCoroutine);
            regenerationCoroutine = null;
        }
    }
    /// <summary>
    /// Resets the overtime effects. In our design, this resets damageMultiplier to its base value.
    /// This should be called at the start of each round.
    /// </summary>
    public void ResetOverTimeEffects()
    {
        if  (damageBoostPassive)
        {
            damageMultiplier = baseDamageMultiplier;
        }
        Debug.Log($"{playerName} damage multiplier reset to {damageMultiplier}");
         // Reset the Last Chance tracking if the ship has that passive.
        if (hasLastChancePassive)
        {
            lastChanceUsed = false;
        }
        if (adaptiveArmorPassive)
        {
            armor = baseArmorValue;
            Debug.Log($"{playerName} adaptive armor reset to base value {armor}");
        }
        if (adaptiveDamagePassive)
        {
            damageMultiplier = baseDamageMultiplier;
            Debug.Log($"{playerName} adaptive damage reset to base value {damageMultiplier}");
        }
    }
    private IEnumerator RegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);  // Regenerate every 1 second.
            if (currentHealth < maxHealth)
            {
                currentHealth += regenRate;
                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                UpdateHealthUI();
                //Debug.Log($"{playerName} regenerated {regenRate} HP. Current HP: {currentHealth}");
            }
        }
    }

    private IEnumerator DamageBoostCoroutine()
    {
        float elapsed = 0f;
        // Choose k so that at 120 seconds, the boost approaches 2.0.
        // For example, using: multiplier = 1 + (damageBoostCap - 1)*(1 - exp(-k*t))
        // If we want damageBoostCap = 2, then when t=120, (1 - exp(-k*120)) should be near 1.
        // One option: k = 4.60517/120 (since 1 - exp(-4.60517) ≈ 0.99)
        float k = 4.60517f / 120f;
        float damageBoostCap = 2f;

        while (elapsed < 120f)
        {
            elapsed += Time.deltaTime;
            // Increase damageMultiplier from 1 to damageBoostCap exponentially.
            damageMultiplier = baseDamageMultiplier + (damageBoostCap - baseDamageMultiplier) * (1f - Mathf.Exp(-k * elapsed));
            yield return null;
        }
        damageMultiplier = damageBoostCap;
    }
    // Add a helper method to increase adaptive armor:
    public void IncreaseAdaptiveArmor()
    {
        if (adaptiveArmorPassive && isPassiveUnlocked)
        {
            // Increase current armor by 10%
            armor *= 1.1f;
            Debug.Log($"{playerName} adaptive armor increased to {armor}");
        }
    }
    public void ResetAdaptiveArmor()
    {
        if (adaptiveArmorPassive && isPassiveUnlocked)
        {
            armor = baseArmorValue;
            Debug.Log($"{playerName} adaptive armor reset to base value {armor}");
        }
    }
    // Add these two helper methods to handle adaptive damage:
    public void IncreaseAdaptiveDamage()
    {
        if (adaptiveDamagePassive && isPassiveUnlocked)
        {
            // Increase current damageMultiplier by 10% (or any factor you choose)
            damageMultiplier *= 1.1f;
            Debug.Log($"{playerName} adaptive damage increased to {damageMultiplier}");
        }
    }

    public void ResetAdaptiveDamage()
    {
        if (adaptiveDamagePassive && isPassiveUnlocked)
        {
            damageMultiplier = baseDamageMultiplier;
            Debug.Log($"{playerName} adaptive damage reset to base value {damageMultiplier}");
        }
    }
    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        Debug.Log($"{playerName} lifesteal healed {amount} HP. " +
                $"Was {oldHealth}, now {currentHealth}");
        UpdateHealthUI(); 
    }

}
