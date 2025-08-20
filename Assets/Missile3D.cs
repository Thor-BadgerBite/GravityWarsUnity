using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static DebugExtensions;
public class Missile3D : MonoBehaviour
{
    public bool isShipDestruction = false;
    public bool isDestroyed = false;
        // Add this with your other variables at the top of the class
    private AudioSource audioSource;
    public delegate void MissileDestroyed(Missile3D missile);
    public static event MissileDestroyed OnMissileDestroyed;
    [HideInInspector] public bool  isCluster = false;
    [HideInInspector] public float clusterDamageFactor = 1f;
    [HideInInspector] public float clusterSpreadDeg    = 5f;

    [Header("Missile Lost Detection")]
    public float gravitationalForceThreshold = 0.01f;
    public float distanceThreshold = 30f;
    public float velocityThreshold = 0.1f;
    public float lostCheckDelay = 2f;
    public bool IsLostInSpace { get; private set; }

    [Header("Missile Properties")]
    public float maxVelocity = 10f;
    public float velocityApproachRate = 0.1f;
    public float trailPointInterval = 0.01f;
    public float collisionDelay = 0.5f;
    public float missileMass = 10f;
    public float drag = 0.01f;
    [Header("Missile Rotation")]
    public float maxTiltAngle = 45f; // Maximum banking angle during turns
    public float rotationSpeed = 10f;
    public float bankingSensitivity = 0.5f; // Adjust this to control how much the missile banks during turns
    private Vector3 lastVelocity;
    [Header("Missile Payload")]
    public float payload = 2500f;   // Base missile damage in the 2500 range
    public float damageVariation = 0.1f; // ±10% variation

    [Header("Missile Fuel")]
    public float fuel = 100f;   // 100 lbs of fuel
    public float fuelConsumptionRate = 2f; // e.g. 5 lbs per second

    [Header("Visual Effects")]
    public Color maxVelocityColor = Color.red;
    public float heatShieldThreshold = 0.8f;
    public Color heatShieldColor = new Color(1f, 0.5f, 0f, 0.7f);
    public int heatShieldParticleCount = 50;
    public float heatShieldParticleSize = 0.1f;
    [Header("Collision Avoidance (RCS) Settings")]
    // Some user-tunable fields:
    public float rcsDetectionRadius = 1f;    // OverlapSphere radius
    public float rcsSafeDistance    = 2f;    // Additional buffer beyond actual planet radius
    public float rcsForce           = 2f;    // Max lateral thrust (acceleration) if extremely close
    public float rcsFalloffDistance = 1f;    // Distance window for factor = 0..1
    public float rcsMaxLateralSpeed = 2f;    // Soft clamp on sideways velocity
    public bool  rcsUseNearestPlanet = true; // If true, only avoid the single nearest planet
    [Header("Predictive RCS Settings")]
    public float rcsLookaheadTime = 2f;     // how many seconds ahead to predict
    public float rcsMaxAvoidForce  = 2f;     // same as your rcsForce
    public float rcsBufferDistance = 1f;     // safe margin around each planet

    [Header("Self‑Destruct")]
    [Tooltip("How big the explosion radius is")]
    public float detRadius = 4f;
    [Tooltip("Payload × this factor on self‑destruct")]
    public float detDamageFactor = 0.5f;
    [Tooltip("Explosion impulse strength on ships")]
    public float pushStrength = 2f;

    private string lastCollisionInfo = "";
    private Vector3 totalGravitationalForce;
    private bool missileLostInSpace = false;
    private float timeSincePotentiallyLost = 0f;
    private Vector3 playfieldCenter = Vector3.zero;

    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private float lastTrailPointTime;
    private Rigidbody rb;
    private CapsuleCollider missileCollider;
    private GameObject trailObject;
    private LineRenderer trajectoryLine;
    private GameObject firedByShip;
    private GameObject firedUponShip;
    private MeshRenderer meshRenderer;
    private Material missileMaterial;
    private Color originalColor;
    private ParticleSystem heatShieldEffect;
    private Vector3 startPosition;
    private AudioSource flyingSoundSource;
    private CameraController cameraController;
    private float attackerDamageMultiplier = 1f;

    void Start()
    {
        playfieldCenter = Vector3.zero;
    }

    void Awake()
    {
        SetupRigidbody();
        SetupCollider();
        SetupRenderer();
        SetupTrajectoryLine();
        SetupHeatShieldEffect();
        SetupAudio();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.mass = missileMass;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezePositionZ; // Only freeze Z position
    }

    void SetupCollider()
    {
        missileCollider = GetComponent<CapsuleCollider>();
        if (missileCollider == null)
        {
            missileCollider = gameObject.AddComponent<CapsuleCollider>();
            // Set proper collider size and orientation
            missileCollider.radius = 0.25f;  // Adjust based on your missile model
            missileCollider.height = 1f;     // Adjust based on your missile model
            missileCollider.direction = 1;    // Align with the missile's forward direction (Y-axis)
            missileCollider.isTrigger = false; // Make sure it's not a trigger
        }
        missileCollider.enabled = false;  // Will be enabled after delay
    }

void SetupRenderer()
{
    meshRenderer = GetComponent<MeshRenderer>();
    if (meshRenderer != null)
    {
        missileMaterial = meshRenderer.material;
        originalColor = missileMaterial.color;
    }
    else
    {
        Debug.LogError("MeshRenderer component not found on missile!");
        // Add visual debug for missing renderer
        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2, Color.red, 5f);
    }
}

    void SetupTrajectoryLine()
    {
        trailObject = new GameObject("MissileTrail");
        trajectoryLine = trailObject.AddComponent<LineRenderer>();
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.positionCount = 0;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.red;
        trajectoryLine.endColor = Color.yellow;
        trajectoryLine.sortingLayerName = "Planets";
        trajectoryLine.sortingOrder = 1;
    }
void SetupHeatShieldEffect()
    {
        GameObject heatShieldObj = new GameObject("HeatShieldEffect");
        heatShieldObj.transform.SetParent(transform);
        heatShieldObj.transform.localPosition = Vector3.forward * 0.5f;

        heatShieldEffect = heatShieldObj.AddComponent<ParticleSystem>();
        var main = heatShieldEffect.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 0.2f;
        main.startSpeed = 2f;
        main.startSize = 0.05f;
        main.maxParticles = 1000;

        var emission = heatShieldEffect.emission;
        emission.rateOverTime = 0;

        var shape = heatShieldEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.1f;

        var colorOverLifetime = heatShieldEffect.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(new Color(1f, 0.7f, 0f), 0.2f),
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0.5f),
                new GradientColorKey(new Color(1f, 0.1f, 0f), 1.0f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;

        var velocityOverLifetime = heatShieldEffect.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
    }

    private void SetupAudio()
    {
        try
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager instance not found!");
                return;
            }

            // Setup main audio source
            audioSource = AudioManager.Instance.Setup3DAudioSource(gameObject);
            
            // Setup flying sound source
            GameObject flyingSoundObj = new GameObject("FlyingSound");
            if (flyingSoundObj != null)
            {
                flyingSoundObj.transform.SetParent(transform);
                flyingSoundObj.transform.localPosition = Vector3.zero;
                
                flyingSoundSource = flyingSoundObj.AddComponent<AudioSource>();
                if (flyingSoundSource != null)
                {
                    flyingSoundSource.spatialBlend = 1f;
                    flyingSoundSource.loop = true;
                    flyingSoundSource.playOnAwake = false;
                    flyingSoundSource.minDistance = 1f;
                    flyingSoundSource.maxDistance = 20f;
                    flyingSoundSource.rolloffMode = AudioRolloffMode.Linear;
                    
                    if (AudioManager.Instance.missileFlySFX != null)
                    {
                        flyingSoundSource.clip = AudioManager.Instance.missileFlySFX;
                    }
                    else
                    {
                        Debug.LogWarning("Missile flying sound effect not assigned in AudioManager!");
                    }
                }
                else
                {
                    Debug.LogError("Failed to add AudioSource component to flying sound object!");
                }
            }
            else
            {
                Debug.LogError("Failed to create flying sound object!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetupAudio: {e.Message}");
        }
    }

    // When playing sounds in Missile3D
    public void PlayLaunchSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMissileLaunch3D(transform.position);
        }
    }

    public void Launch(Vector3 direction, float speed, GameObject firingShip, float attackerDamageMultiplier)
    {
        this.attackerDamageMultiplier = attackerDamageMultiplier;
        startPosition = transform.position;
        rb.velocity = direction.normalized * speed * 0.5f;
        firedByShip = firingShip;
        // Set initial rotation: X based on firing angle, Y=90, Z=-90
        float initialAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion initialRotation = Quaternion.Euler(initialAngle, 90, -90);
        transform.rotation = initialRotation;
        
        PlayerShip playerShip = firingShip.GetComponent<PlayerShip>();
        if (playerShip != null)
        {
            playerShip.IncrementShotCounter();
        }

        if (cameraController != null)
        {
            cameraController.StartFollowingMissile3D(this);
        }

        trajectoryPoints.Clear();
        AddTrajectoryPoint(transform.position);
        StartCoroutine(EnableColliderWithDelay());
        
        // Add null checks for audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMissileLaunch();
        }
        if (flyingSoundSource != null)
        {
            flyingSoundSource.Play();
        }
    }

    private IEnumerator EnableColliderWithDelay()
    {
        yield return new WaitForSeconds(collisionDelay);
        missileCollider.enabled = true;
    }
    void Update()
    {
        if (!isDestroyed && fuel > 0f)
        {
            float deltaFuel = fuelConsumptionRate * Time.deltaTime;
            fuel -= deltaFuel;

            if (fuel <= 0f)
            {
                // kill missile
                DestroyMissile();
            }
            if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.sqrMagnitude > 0.1f)
            {
                if (isCluster)
                    SplitCluster();
                else
                    SelfDestruct();
            }
        }
    }
    private void SelfDestruct()
    {
        Vector3 center = transform.position;
        Collider[] hits = Physics.OverlapSphere(center, detRadius);
        var damaged = new HashSet<PlayerShip>();
        var shooter = firedByShip.GetComponent<PlayerShip>();
        foreach (var col in hits)
        {
            // try to find a PlayerShip on this collider
            var ship = col.GetComponent<PlayerShip>() 
                    ?? col.GetComponentInParent<PlayerShip>();
            if (ship == null || ship == shooter || damaged.Contains(ship))
                continue;
            damaged.Add(ship);
            // distance from blast center
            float dist = Vector3.Distance(center, col.transform.position);
            // ratio = 1.0 at 0m, → 0.0 at detRadius
            float ratio = Mathf.Clamp01(1f - (dist / detRadius));

            // scaled damage
            float dmg = payload * detDamageFactor * ratio;
            ship.TakeDamage(dmg, center);

            // scaled push
            var shipRb = ship.GetComponent<Rigidbody>();
            if (shipRb != null && !ship.unmovable)
            {
                float force = pushStrength * ratio;
                shipRb.AddExplosionForce(
                    force,
                    center,
                    detRadius,
                    /* upwardsModifier */ 1f,
                    ForceMode.Impulse
                );
            }
        }

        CreateExplosionEffect(center);
        DestroyMissile(center, triggerEndTurn: true, shipHit: false);
    }
    /// <summary>
    /// Split this cluster missile into 3:
    /// the original (center) now does 75% damage,
    /// plus two new fragments at ±spread.
    /// </summary>
   public void SplitCluster()
{
    if (!isCluster) return;
    isCluster = false;

    // 1) scale down the center missile’s payload
    payload *= clusterDamageFactor;

    // 2) capture parent’s current velocity and heading
    Vector3 centerPos = transform.position;
    Vector3 parentVel = rb.velocity;
    float  speed     = parentVel.magnitude;
    float  baseAngle = Mathf.Atan2(parentVel.y, parentVel.x) * Mathf.Rad2Deg;

    // 3) spawn the two side fragments at ±spread
    foreach (float offset in new float[]{ -clusterSpreadDeg, +clusterSpreadDeg })
    {
        float angle = baseAngle + offset;
        float rad   = angle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

        // clone this missile (so it carries over visuals, trail, etc.)
        var sideObj = Instantiate(gameObject, centerPos, Quaternion.Euler(0,0,angle));
        var m       = sideObj.GetComponent<Missile3D>();

        // disable further splitting and scale its damage
        m.isCluster           = false;
        m.payload            *= clusterDamageFactor;

        // ——— IMPORTANT ———
        // because your Launch() does: rb.velocity = dir * speedParam * 0.5f;
        // we pass in speed*2 so that after the *0.5f you end up with exactly 'speed'
        m.Launch(dir, speed * 2f, firedByShip, attackerDamageMultiplier);
    }
}


    void FixedUpdate()
    {
        ApplyGravity();
        //AvoidPlanetsWithRCS(); // NEW: Call our collision avoidance method.
        AvoidPlanetsPredictively();
        MoveMissile();
        RotateMissile();
        UpdateTrajectory();
        CheckIfMissileIsLost();
       // Add null checks for audio
        if (flyingSoundSource != null && AudioManager.Instance != null)
        {
            flyingSoundSource.volume = AudioManager.Instance.GetSFXVolume();
        }
    }
    /// <summary>
    /// Uses a spherecast in the missile’s forward direction to detect nearby planets.
    /// If a planet is detected within the specified distance, gently adjusts the missile’s velocity
    /// to steer away from the planet.
    /// </summary>
    /// 
    bool TryGetTimeToCollision(
    Vector3 p, Vector3 v,
    Vector3 planetPos, float R,
    float maxHorizon, out float ttc)
    {
        // Solve a t^2 + b t + c = 0
        Vector3 s = p - planetPos;
        float a = Vector3.Dot(v,v);
        float b = 2f * Vector3.Dot(s,v);
        float c = Vector3.Dot(s,s) - R*R;
        float disc = b*b - 4f*a*c;
        if (disc < 0f || a < Mathf.Epsilon) { ttc = 0; return false; }

        float sqrtD = Mathf.Sqrt(disc);
        float t1 = (-b - sqrtD)/(2f*a);
        float t2 = (-b + sqrtD)/(2f*a);

        // pick smallest positive
        ttc = Mathf.Min(t1>0f? t1:Mathf.Infinity,
                    t2>0f? t2:Mathf.Infinity);
        return ttc>0f && ttc <= maxHorizon;
    }


private void AvoidPlanetsPredictively()
{
    if (firedByShip == null) return;
    var shooter = firedByShip.GetComponent<PlayerShip>();
    if (!shooter.isPassiveUnlocked || !shooter.collisionAvoidancePassive) return;

    Vector3 p = transform.position;
    Vector3 v = rb.velocity;
    if (v.sqrMagnitude < 0.01f) return;
    
    int planetLayer = LayerMask.GetMask("Planet");
    Collider[] hits = Physics.OverlapSphere(p, v.magnitude * rcsLookaheadTime, planetLayer);
    if (hits.Length == 0) return;

    // Find the planet with smallest TTC (if any)
    float bestT = float.MaxValue;
    Collider best = null;
    foreach(var c in hits)
    {
        var sphere = c.GetComponent<SphereCollider>();
        if (sphere == null) continue;
        // compute true world‑radius
        Vector3 lossy = c.transform.lossyScale;
        float scale = Mathf.Max(lossy.x, Mathf.Max(lossy.y, lossy.z));
        float R = sphere.radius*scale + rcsBufferDistance;

        if (TryGetTimeToCollision(p, v, c.transform.position, R,
                                 rcsLookaheadTime, out float ttc))
        {
            if (ttc < bestT)
            {
                bestT = ttc;
                best  = c;
            }
        }
    }
    if (best == null) return;

    // How strongly do we avoid?  ramp = 1 - (ttc / lookahead)
    float ramp = 1f - Mathf.Clamp01(bestT / rcsLookaheadTime);

    // Compute a pure tangential direction around that planet:
    Vector3 toPlanet = best.transform.position - p;
    Vector3 normal   = Vector3.Cross(v, Vector3.forward).normalized;
    // choose sign so we steer *away*
    if (Vector3.Dot(normal, toPlanet) > 0f)
        normal = -normal;

    // apply gentle acceleration
    rb.AddForce(normal * (rcsMaxAvoidForce * ramp),
                ForceMode.Acceleration);
}


    private void AvoidPlanetsWithRCS()
    {
        // 1) Check if the shooter has collisionAvoidancePassive
        if (firedByShip == null) return;
        PlayerShip shooter = firedByShip.GetComponent<PlayerShip>();
        if (!shooter.isPassiveUnlocked) return;
        if (shooter == null || !shooter.collisionAvoidancePassive) return;

        // 2) OverlapSphere to find all planet colliders
        int planetLayer = LayerMask.GetMask("Planet");  // or however your layers are set up
        Collider[] hits = Physics.OverlapSphere(transform.position, rcsDetectionRadius, planetLayer);

        if (hits == null || hits.Length == 0) return; // no planets found

        // Optionally pick the single nearest planet (if rcsUseNearestPlanet = true)
        // or just apply to all.
        if (rcsUseNearestPlanet)
        {
            // Find the single nearest planet
            float nearestDist = float.MaxValue;
            Collider bestCollider = null;

            foreach (var c in hits)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    bestCollider = c;
                }
            }

            if (bestCollider != null)
            {
                ApplyRCSAvoidance(bestCollider);
            }
        }
        else
        {
            // Apply RCS to each planet found
            foreach (var c in hits)
            {
                ApplyRCSAvoidance(c);
            }
        }
    }

    /// <summary>
    /// Actually does the distance checks, calculates a factor, 
    /// and applies a sideways acceleration to the missile rigidbody.
    /// </summary>
    private void ApplyRCSAvoidance(Collider planetCollider)
    {
        Planet planet = planetCollider.GetComponent<Planet>();
        if (planet == null) return;

        // 1) Compute distance from missile to planet center
        Vector3 toPlanet = planet.transform.position - transform.position;
        float distance   = toPlanet.magnitude;

        // 2) Determine planet's scaled radius
        SphereCollider sphere = planet.GetComponent<SphereCollider>();
        if (!sphere) return;

        float scale = Mathf.Max(planet.transform.lossyScale.x,
                                planet.transform.lossyScale.y,
                                planet.transform.lossyScale.z);
        float planetRadius = sphere.radius * scale;
        float desiredSafeRadius = planetRadius + rcsSafeDistance;

        // If we're outside "desiredSafeRadius + rcsFalloffDistance", do nothing
        float outerLimit = desiredSafeRadius + rcsFalloffDistance;
        if (distance > outerLimit) return;

        // 3) Compute how "close" we are in [0..1], 
        //    where 0 means distance=outerLimit, 1 means distance=desiredSafeRadius or less
        //    You could use a smooth approach (like an inverse-lerp or polynomial).
        float margin = distance - desiredSafeRadius;  // negative if inside planet
        float factor = 1f - Mathf.Clamp01(margin / rcsFalloffDistance);

        // 4) Decide which way to push the missile. 
        //    We want to push it tangentially so it "slides around" the planet.
        //    A simple approach is cross(forward, toPlanet) again, 
        //    or push directly away from the planet. We'll do "directly away" for simplicity:
        //
        //    But you said you want minimal changes to ballistic path, so let's do a 
        //    perpendicular approach. If you do fully "away from planet," 
        //    it changes the radial component a lot. 
        //    I'll demonstrate "semi-tangent" approach below:

        // Let "forward" = missile's velocity direction if you prefer. 
        // We'll define "forward" as our current velocity direction, ignoring Z.
        Vector3 velocityDir = rb.velocity;
        velocityDir.z = 0f;
        velocityDir.Normalize();
        if (velocityDir.sqrMagnitude < 0.01f)
        {
            // fallback to transform.forward if velocity is nearly zero
            velocityDir = transform.forward;
            velocityDir.z = 0f;
            velocityDir.Normalize();
        }

        // 4a) We want a direction that is somewhat perpendicular 
        // but also somewhat away from the planet. We'll do a weighted blend:
        // tangentDir = cross(planetUp, velocityDir) or cross(velocityDir, planetUp).
        // But let's keep it simple:
        Vector3 fromPlanet = (transform.position - planet.transform.position).normalized;

        // Weighted approach: 70% keep velocity direction, 30% away from planet
        // Then remove the forward overlap:
        Vector3 combined = Vector3.Lerp(velocityDir, fromPlanet, 0.3f);
        // Now remove any "forward" component so that we don't kill ballistic path. 
        // We'll keep only the side slip. So we can do a cross/cross trick:
        Vector3 sideways = Vector3.Cross(Vector3.Cross(velocityDir, combined), velocityDir);
        // (That basically projects 'combined' onto the plane perpendicular to velocityDir.)
        sideways.Normalize();

        // Alternatively, if you want a purely perpendicular approach:
        // Vector3 sideways = Vector3.Cross(Vector3.up, velocityDir);
        // Decide sign by dot with fromPlanet, etc.

        // 5) The final strength = rcsForce * factor
        float finalStrength = rcsForce * factor;

        // 6) Apply that as an Acceleration so missile mass won't hamper it
        rb.AddForce(sideways * finalStrength, ForceMode.Acceleration);

        // 7) Optionally clamp lateral velocity
        // That means we measure the velocity component perpendicular to velocityDir, 
        // and if it’s too big, we scale it back:
        Vector3 lateral = Vector3.ProjectOnPlane(rb.velocity, velocityDir);
        if (lateral.magnitude > rcsMaxLateralSpeed)
        {
            // clamp the lateral portion
            Vector3 newLateral = lateral.normalized * rcsMaxLateralSpeed;
            // keep forward portion
            Vector3 forwardPortion = Vector3.Project(rb.velocity, velocityDir);
            rb.velocity = forwardPortion + newLateral;
        }

        // Debug lines:
        Debug.DrawLine(transform.position, transform.position + sideways * 5f, Color.cyan, 0.1f);
        // For debug logging:
        // Debug.Log($"[{name}] Avoiding {planet.planetName}, factor={factor:F2}, finalStr={finalStrength:F2}, velocity={rb.velocity.magnitude:F2}");
    }






    void ApplyGravity()
    {
        Planet[] planets = FindObjectsOfType<Planet>();
        totalGravitationalForce = Vector3.zero;

        foreach (Planet planet in planets)
        {
            totalGravitationalForce += planet.CalculateGravitationalPull(rb.position, missileMass);
        }

        rb.AddForce(totalGravitationalForce);
    }

    void MoveMissile()
    {
        // Apply drag
        rb.velocity *= (1 - drag * Time.fixedDeltaTime);

        // Gradually approach max velocity
        float currentSpeed = rb.velocity.magnitude;
        if (currentSpeed > maxVelocity)
        {
            Vector3 velocityDirection = rb.velocity.normalized;
            rb.velocity = Vector3.Lerp(rb.velocity, velocityDirection * maxVelocity, velocityApproachRate);
        }

        // Keep the missile in the 2D plane
        Vector3 position = transform.position;
        position.z = 0;
        transform.position = position;

        // Debug log for movement
        //Debug.Log($"Missile Velocity: {rb.velocity}, Speed: {currentSpeed}, Position: {transform.position}");

        // Visual indicator of max velocity
        UpdateMaxVelocityIndicator(currentSpeed);

        // Update heat shield effect
        UpdateHeatShieldEffect(currentSpeed);
    }
    void UpdateMaxVelocityIndicator(float currentSpeed)
    {
        if (missileMaterial != null)
        {
            float t = Mathf.Clamp01(currentSpeed / maxVelocity);
            missileMaterial.color = Color.Lerp(originalColor, maxVelocityColor, t);
        }
    }

    void UpdateHeatShieldEffect(float currentSpeed)
    {
        float speedRatio = currentSpeed / maxVelocity;
        var emission = heatShieldEffect.emission;
        var main = heatShieldEffect.main;

        if (speedRatio >= heatShieldThreshold)
        {
            float intensityFactor = Mathf.InverseLerp(heatShieldThreshold, 1f, speedRatio);
            emission.rateOverTime = 1000 * intensityFactor;
            main.startSize = 0.05f * (0.5f + 0.5f * intensityFactor);
            main.startSpeed = 2f + 3f * intensityFactor;

            if (!heatShieldEffect.isPlaying)
            {
                heatShieldEffect.Play();
            }
        }
        else
        {
            emission.rateOverTime = 0;
            if (heatShieldEffect.isPlaying)
            {
                heatShieldEffect.Stop();
            }
        }
    }

    //void RotateMissile()
    //{
    //    if (rb.velocity != Vector3.zero)
    //    {
    //        // Calculate rotation based on velocity direction
    //        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
    //       transform.rotation = targetRotation;
    //    }
    //}

void RotateMissile()
{
    if (rb.velocity.magnitude > 0.1f)
    {
        // Get the direction of motion in the XY plane
        Vector3 velocityXY = new Vector3(rb.velocity.x, rb.velocity.y, 0).normalized;
        
        // Calculate the main flight angle (X rotation)
        float flightAngle = -1 * Mathf.Atan2(velocityXY.y, velocityXY.x) * Mathf.Rad2Deg;
        
        // Calculate banking (Z rotation offset from -90)
        Vector3 currentVelocityDir = rb.velocity.normalized;
        float turnRate = 0f;
        
        if (lastVelocity != Vector3.zero)
        {
            // Calculate turn rate based on velocity change
            turnRate = Vector3.SignedAngle(lastVelocity, currentVelocityDir, Vector3.forward);
            // Apply banking sensitivity and clamp
            turnRate = Mathf.Clamp(turnRate * bankingSensitivity, -maxTiltAngle, maxTiltAngle);
        }
        
        // Final rotation: X=flight angle, Y=90 (sideways), Z=-90+banking
        Quaternion targetRotation = Quaternion.Euler(flightAngle, 90, -90 + turnRate);
        
        // Smoothly rotate to target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        
        // Store current velocity for next frame
        lastVelocity = currentVelocityDir;

        // Debug visualization
        Debug.DrawRay(transform.position, transform.right * 2f, Color.red); // X axis
        Debug.DrawRay(transform.position, transform.up * 2f, Color.green); // Y axis
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.blue); // Z axis
    }
}
    void UpdateTrajectory()
    {
        if (Time.time - lastTrailPointTime >= trailPointInterval)
        {
            AddTrajectoryPoint(transform.position);
            lastTrailPointTime = Time.time;
            
            // Debug log for positions
            //Debug.Log($"Missile Position: {transform.position}, Last Trail Point: {trajectoryPoints[trajectoryPoints.Count - 1]}");
            
            // Visual debug
            Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.red, 0.1f);
        }
    }

    void CheckIfMissileIsLost()
    {
        if (missileLostInSpace) return;

        float distanceFromPlayfield = Vector3.Distance(transform.position, playfieldCenter);
        Vector3 directionToPlayfield = (playfieldCenter - transform.position).normalized;
        float velocityDotProduct = Vector3.Dot(rb.velocity.normalized, directionToPlayfield);

        bool potentiallyLost = totalGravitationalForce.magnitude < gravitationalForceThreshold &&
                              distanceFromPlayfield > distanceThreshold &&
                              velocityDotProduct < velocityThreshold;

        if (potentiallyLost)
        {
            timeSincePotentiallyLost += Time.fixedDeltaTime;
            if (timeSincePotentiallyLost >= lostCheckDelay)
            {
                missileLostInSpace = true;
                IsLostInSpace = true;
                GameManager.Instance.MissileLostInSpace();
                DestroyMissile(null, true, false);
            }
        }
        else
        {
            timeSincePotentiallyLost = 0f;
        }

        VisualizeThresholds(distanceFromPlayfield, velocityDotProduct);
    }

    void VisualizeThresholds(float distance, float velocityDot)
    {
        Debug.DrawLine(playfieldCenter, transform.position, Color.yellow);
        DrawCircle(playfieldCenter, distanceThreshold, Color.red);
        Debug.DrawRay(transform.position, rb.velocity.normalized * 5f, Color.blue);
        Debug.DrawRay(transform.position, (playfieldCenter - transform.position).normalized * 5f, Color.green);
    }

    void AddTrajectoryPoint(Vector3 point)
    {
        trajectoryPoints.Add(point);
        trajectoryLine.positionCount = trajectoryPoints.Count;
        trajectoryLine.SetPositions(trajectoryPoints.ToArray());
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Missile collision detected with: {collision.gameObject.name}");
        Vector3 collisionPoint = collision.contacts[0].point;
        HandleCollision(collision.collider, collisionPoint);
    }
    // Add this method to help debug
//    void OnTriggerEnter(Collider other)
//    {
//        Debug.Log($"Missile trigger detected with: {other.gameObject.name}");
//        HandleCollision(other, transform.position);
//   }
void HandleCollision(Collider collider, Vector3 collisionPoint)
{
    if (isDestroyed) return;
    // Get the attacker’s ship component from firedByShip
    PlayerShip attacker = firedByShip != null ? firedByShip.GetComponent<PlayerShip>() : null;

    PlayerShip ship = collider.GetComponent<PlayerShip>();
    if (ship == null)
    {
        ship = collider.GetComponentInParent<PlayerShip>();
    }

    if (ship != null)
    {
        // If the ship is movable, apply an impulse based on the missile's momentum.
        if (!ship.unmovable && ship.isPassiveUnlocked)
        {
            Rigidbody shipRb = ship.GetComponent<Rigidbody>();
            if (shipRb != null)
            {
                Debug.Log("Ship mass: " + shipRb.mass + ", isKinematic: " + shipRb.isKinematic);
                // Ensure the Rigidbody is awake and non-kinematic
                shipRb.WakeUp();
                // Calculate missile momentum (mass * velocity)
                Vector3 missileMomentum = rb.mass * rb.velocity;
                // Optionally scale the force if it's too small
                float forceScale = 2f; // Adjust this factor as needed
                Vector3 appliedForce = missileMomentum * forceScale;
                Debug.Log("Applying impulse: " + appliedForce + " at " + collisionPoint);
                shipRb.AddForceAtPosition(appliedForce, collisionPoint, ForceMode.Impulse);
                //shipRb.AddExplosionForce(10f, collisionPoint, 2f, 0.1f, ForceMode.Impulse);
            }
        }

        // Continue with your existing damage and collision logic:
        // 1) Determine which sub-part was hit
        Transform partRoot = FindPartRoot(collider.transform);

        // 2) Compute damage (with ±damageVariation and part multipliers)
        
        float randomFactor = Random.Range(1f - damageVariation, 1f + damageVariation);
        if (attacker != null && attacker.precisionEngineering && attacker.isPassiveUnlocked)
        {
            randomFactor = 1f;
        }

        float baseDamage = payload * randomFactor;
        

        // Get the standard multiplier based on the part hit
        float partMult = GetPartMultiplier(partRoot.name);

        // If the target ship has critical immunity and the hit part is the core,
        // override the multiplier to 1.0 (i.e. no extra critical damage)
       if (partRoot.name.ToLower().Contains("core"))
        {
            // First, if the attacker has critical enhancement, set multiplier to 1.5
            if (attacker != null && attacker.CriticalEnhancement && attacker.isPassiveUnlocked)
            {
                partMult = 1.5f;
            }
            // Then, if the target ship has critical immunity, override multiplier to 1.0
            if (ship != null && ship.criticalImmunity && ship.isPassiveUnlocked)
            {
                partMult = 1.0f;
            }
        }
        float rawDamage = baseDamage * partMult * attackerDamageMultiplier;
        rawDamage = Mathf.Max(rawDamage, 1f);  // Ensure at least 1 damage

        // 3) Determine explosion force based on missile speed
        float speed = rb.velocity.magnitude;
        float normalized = Mathf.InverseLerp(0f, maxVelocity, speed);
        float minForce = 0.5f;
        float maxForce = 6f;
        float explosionForce = Mathf.Lerp(minForce, maxForce, normalized);

        // 4) Detach the hit part with some explosion force
        DetachPart(partRoot, collisionPoint, explosionForce);

        // 5) Apply damage to the ship
        float speedthreshold = rb.velocity.magnitude;
        bool isHighSpeed = (speedthreshold >= 0.8f * maxVelocity);

        // If high‐speed and attacker has the offense perk, amplify damage:
        if (isHighSpeed && attacker != null && attacker.increaseDamageOnHighSpeedMissiles && attacker.isPassiveUnlocked)
        {
            float bonus = attacker.highSpeedDamageAmplifyPercent; 
            // E.g. 0.2 => +20% damage
            rawDamage *= (1f + bonus);
            Debug.Log($"[Missile] Attacker {attacker.playerName} high‐speed bonus => rawDamage now {rawDamage}");
        }

        // If high‐speed and defender has the defense perk, reduce damage:
        if (isHighSpeed && ship.reduceDamageFromHighSpeedMissiles && ship.isPassiveUnlocked)
        {
            float reduction = ship.highSpeedDamageReductionPercent;
            // E.g. 0.2 => 20% less
            rawDamage *= (1f - reduction);
            Debug.Log($"[Missile] Defender {ship.playerName} high‐speed reduction => rawDamage now {rawDamage}");
        }
        ship.TakeDamage(rawDamage, collisionPoint);
        if (attacker != null && attacker.lifestealPassive && rawDamage > 0f && attacker.isPassiveUnlocked)
        {
            float amountToHeal = rawDamage * attacker.lifestealPercent;
            attacker.Heal(amountToHeal);
        }

        ship.IncreaseAdaptiveArmor(); // (Make sure this method exists in PlayerShip)
        attacker.IncreaseAdaptiveDamage(); // If the missile hit a ship, increase the attacker's adaptive damage:
       
        // 6) Destroy the missile
        DestroyMissile(collisionPoint, true, true);
        return;
    }
    else if (collider.GetComponent<Planet>() != null)
    {
        Planet planet = collider.GetComponent<Planet>();
        lastCollisionInfo = $"collided with {planet.planetName}";
        DestroyMissile(collisionPoint, true, false);
    }
    else
    {
        Debug.Log($"Collided with unknown object: {collider.name}");
        lastCollisionInfo = "collided with an object";
        DestroyMissile(collisionPoint, true, false);
    }
}



    // Helper method to debug object hierarchy
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
}
private Transform FindPartRoot(Transform hitTransform)
{
    // We'll climb up until we find a name that suggests it's a top-level part
    while (hitTransform != null)
    {
        string nameLower = hitTransform.name.ToLower();
        if (nameLower.Contains("wing") 
         || nameLower.Contains("engine")
         || nameLower.Contains("fin")
         || nameLower.Contains("plasma")
         || nameLower.Contains("core")
         || nameLower.Contains("tail")
         || nameLower.Contains("weapon"))
        {
            // Found our sub-part root
            return hitTransform;
        }

        // otherwise climb up
        // If we eventually reach the main ship root => break
        if (hitTransform.parent == null) break;

        hitTransform = hitTransform.parent;
    }

    // fallback => just return the object we started with
    return hitTransform;
}


private float GetPartMultiplier(string partName)
{
    string lower = partName.ToLower();

    if (lower.Contains("wing"))        return 0.85f;  // -15%
    if (lower.Contains("weapon"))      return 0.90f;  // -10%
    if (lower.Contains("plasma"))      return 1.10f;  // +10%
    if (lower.Contains("engine"))      return 1.15f;  // +15%
    if (lower.Contains("core"))        return 1.30f;  // +30%

    // tail, fin => no change
    return 1f;
}

private void DetachPart(Transform part, Vector3 impactPoint, float explosionForce)
{
    // If it is “Core”, skip, or whichever logic you have
    if (part.name.Contains("Core")) return;

    // Ensure it has a RB
    Rigidbody rbPart = part.GetComponent<Rigidbody>();
    if (rbPart == null) rbPart = part.gameObject.AddComponent<Rigidbody>();
    rbPart.isKinematic = false;
    rbPart.useGravity = false;
    rbPart.constraints = RigidbodyConstraints.None;

    // Unparent now
    part.parent = null;

    // Explosion force or forward push
    //rbPart.AddExplosionForce(explosionForce, impactPoint, 2f, 0.1f, ForceMode.Impulse);
    Vector3 missileDir = (part.position - impactPoint).normalized; 
    rbPart.AddForce(missileDir * explosionForce, ForceMode.Impulse);
    // Optional random torque
    Vector3 randomTorque = Random.insideUnitSphere * Random.Range(1f, 4f);
    rbPart.AddTorque(randomTorque, ForceMode.Impulse);

    // Then recursively do the same to all children
    // BUT each child is about to lose part as parent, so do this carefully:
    // We'll gather the children first in a list to avoid messing up the loop
    List<Transform> children = new List<Transform>();
    for (int i = 0; i < part.childCount; i++)
    {
        children.Add(part.GetChild(i));
    }

    foreach (Transform child in children)
    {
        DetachPart(child, impactPoint, explosionForce);
    }
}





void DestroyEnemyShip(PlayerShip ship, Vector3 collisionPoint)
{
    if (isDestroyed)
    {
        Debug.LogWarning("DestroyEnemyShip called, but missile is already destroyed.");
        return;
    }

    //isDestroyed = true;
    Debug.Log($"Destroying {ship.playerName}'s ship at {collisionPoint}");

    //CreateExplosionEffect(ship.transform.position);
    isShipDestruction = true;
    //ship.HideShip();
    ship.ExplodeShip(collisionPoint);
    GameManager.Instance.ShipDestroyed(ship);
    //DestroyMissile(collisionPoint);
}


    public string GetLastCollisionInfo()
    {
        return lastCollisionInfo;
    }

    void CreateExplosionEffect(Vector3 position)
    {
        GameObject explosionObj = new GameObject("ExplosionEffect");
        explosionObj.transform.position = position;

        ParticleSystem explosionSystem = explosionObj.AddComponent<ParticleSystem>();
        if (explosionSystem.isPlaying)
        {
            explosionSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Main module configuration
        var main = explosionSystem.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.5f;
        main.maxParticles = 500;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission module
        var emission = explosionSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 500) });

        // Shape module
        var shape = explosionSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // Color over lifetime
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

        // Size over lifetime
        var sizeOverLifetime = explosionSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

        // Renderer
        var renderer = explosionSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.mainTexture = CreateExplosionTexture();

        explosionSystem.Play();
        Destroy(explosionObj, main.duration + main.startLifetime.constantMax);
    }

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
                float alpha = SmoothStep(0f, 1f, t);
                colors[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    private float SmoothStep(float edge0, float edge1, float x)
    {
        x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        return x * x * (3 - 2 * x);
    }

public void DestroyMissile(Vector3? collisionPoint = null, bool triggerEndTurn = true, bool shipHit = false)
{
    if (isDestroyed)
    {
        Debug.LogWarning("DestroyMissile called, but missile is already destroyed.");
        return;
    }

    Debug.Log("Destroying Missile...");
    isDestroyed = true;
    Vector3 finalPoint = collisionPoint ?? transform.position;

    if (trailObject != null)
    {
        trailObject.transform.SetParent(null);
        UpdateTrailToCollisionPoint(finalPoint);

        if (firedByShip != null)
        {
            PlayerShip playerShip = firedByShip.GetComponent<PlayerShip>();
            if (playerShip != null)
            {
                playerShip.SetLastMissileTrail(trailObject);
                trailObject = null;
            }
        }

        if (trailObject != null)
        {
            Destroy(trailObject);
        }
    }

    CreateExplosionEffect(finalPoint);

    if (heatShieldEffect != null)
    {
        Destroy(heatShieldEffect.gameObject);
    }

    // Only invoke the missile destroyed event if triggerEndTurn is true.
    if (triggerEndTurn)
    {
        OnMissileDestroyed?.Invoke(this);
    }

    if (cameraController != null)
    {
        cameraController.StopFollowingMissileWithInvoke(!triggerEndTurn, finalPoint);
    }

    if (flyingSoundSource != null)
    {
        flyingSoundSource.Stop();
    }

    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayMissileDestroyed();
    }
    // NEW: If the missile misses a ship, reset that defending ship's adaptive armor.
    if (!shipHit && firedUponShip != null) {
        PlayerShip defendingShip = firedUponShip.GetComponent<PlayerShip>();
        if (defendingShip != null) {
            defendingShip.ResetAdaptiveArmor();
        }
    }
    // NEW: If the missile did not hit a ship, reset the attacker's adaptive damage:
    if (!shipHit && firedByShip != null)
    {
        PlayerShip attacker = firedByShip.GetComponent<PlayerShip>();
        if (attacker != null && attacker.adaptiveDamagePassive)
        {
            attacker.ResetAdaptiveDamage();
        }
    }
    Destroy(gameObject);
}


    void OnDestroy()
    {
        if (flyingSoundSource != null && flyingSoundSource.isPlaying)
        {
            flyingSoundSource.Stop();
        }
    }

    void UpdateTrailToCollisionPoint(Vector3 collisionPoint)
    {
        AddTrajectoryPoint(collisionPoint);
        trajectoryLine.positionCount = trajectoryPoints.Count;
        trajectoryLine.SetPositions(trajectoryPoints.ToArray());
    }

    public Vector3 GetStartPosition()
    {
        return startPosition;
    }

    IEnumerator FadeOutTrail()
    {
        float fadeDuration = 2f;
        float elapsedTime = 0f;
        Color startColor = trajectoryLine.startColor;
        Color endColor = trajectoryLine.endColor;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - (elapsedTime / fadeDuration);

            trajectoryLine.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            trajectoryLine.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);

            yield return null;
        }

        Destroy(trailObject);
    }
}

