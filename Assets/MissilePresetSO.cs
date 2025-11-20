using UnityEngine;

/// <summary>
/// Defines a missile type with all its properties.
/// Create instances via: Right-click -> Create -> GravityWars -> Missile Preset
/// </summary>
[CreateAssetMenu(fileName = "NewMissile", menuName = "GravityWars/Missile Preset")]
public class MissilePresetSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name for this missile (e.g., 'Hellfire-MK2')")]
    public string missileName = "Standard Missile";

    [Tooltip("Missile classification")]
    public MissileType missileType = MissileType.Medium;

    [Tooltip("Optional visual model prefab - if null, uses default missile mesh")]
    public GameObject visualModelPrefab;

    [Tooltip("Icon for UI display")]
    public Sprite icon;

    [Header("Unlock Requirements")]
    [Tooltip("Account level required to unlock this missile (0 = available from start)")]
    [Range(0, 50)]
    public int requiredAccountLevel = 0;

    [Header("Physics Properties")]
    [Tooltip("Display mass shown to player (200-1000 lbs). Physics mass is auto-calculated.")]
    [Range(200f, 1000f)]
    public float displayMass = 500f;

    [Tooltip("READONLY: Actual physics mass used in calculations (0.6-3.0). Auto-calculated from displayMass unless overridden.")]
    public float Mass => overridePhysicsMass ? customPhysicsMass : (displayMass / 333.33f);  // Converts 200-1000 lbs to 0.6-3.0 range, or uses override

    [Header("--- Advanced: Override Physics Mass (Optional) ---")]
    [Tooltip("If true, uses customPhysicsMass instead of auto-calculated value")]
    public bool overridePhysicsMass = false;

    [Tooltip("Custom physics mass (only used if overridePhysicsMass is true)")]
    [Range(0.6f, 3.0f)]
    public float customPhysicsMass = 1.5f;

    [Header("Launch Velocity (Initial Firing Speed)")]
    [Tooltip("Minimum launch velocity - how slow you can fire this missile (m/s)")]
    [Range(0.1f, 5f)]
    public float minLaunchVelocity = 0.1f;

    [Tooltip("Maximum launch velocity - how hard you can 'throw' this missile (m/s). Heavy missiles = harder to throw fast, Light missiles = easy to throw fast")]
    [Range(1f, 30f)]
    public float maxLaunchVelocity = 10f;

    [Header("Max Flight Velocity (Speed Cap During Flight)")]
    [Tooltip("Maximum flight speed in m/s - terminal velocity during flight after gravity acceleration. Heavy missiles can have HIGH values to gain devastating speed through gravity wells!")]
    [Range(5f, 500f)]
    public float maxVelocity = 10f;

    [Tooltip("Drag coefficient - higher = slows down faster")]
    [Range(0.005f, 0.05f)]
    public float drag = 0.01f;

    [Tooltip("How quickly missile approaches max velocity (0.1 = gradual, 1.0 = instant)")]
    [Range(0.05f, 1f)]
    public float velocityApproachRate = 0.1f;

    [Header("Fuel System")]
    [Tooltip("Total fuel capacity in lbs")]
    [Range(30f, 300f)]
    public float fuel = 100f;

    [Tooltip("Fuel burn rate in lbs/second")]
    [Range(1f, 5f)]
    public float fuelConsumptionRate = 2f;

    [Header("Damage & Impact")]
    [Tooltip("Base damage on direct hit")]
    [Range(500f, 10000f)]
    public float payload = 2500f;

    [Tooltip("Knockback force applied to hit ship - CRITICAL: Heavy missiles push harder!")]
    [Range(0.5f, 10f)]
    public float pushStrength = 2f;

    [Tooltip("±10% means damage varies from 90% to 110% of payload")]
    [Range(0f, 0.2f)]
    public float damageVariation = 0.1f;

    [Header("Self-Destruct (Manual Detonation)")]
    [Tooltip("Blast radius when manually detonated (Space key)")]
    [Range(2f, 8f)]
    public float selfDestructRadius = 4f;

    [Tooltip("Damage multiplier on self-destruct (0.5 = 50% of payload)")]
    [Range(0.1f, 1f)]
    public float selfDestructDamageFactor = 0.5f;

    [Tooltip("Push force on self-destruct")]
    [Range(0.5f, 5f)]
    public float selfDestructPushStrength = 2f;

    [Header("Visual & Audio")]
    [Tooltip("Trail color for this missile type")]
    public Color trailColor = new Color(1f, 0.5f, 0f); // Orange

    [Tooltip("Mesh color tint at max velocity")]
    public Color maxVelocityColor = Color.red;

    [Tooltip("Custom launch sound (optional)")]
    public AudioClip launchSound;

    [Tooltip("Custom flying sound (optional)")]
    public AudioClip flyingSound;

    [Tooltip("Custom explosion sound (optional)")]
    public AudioClip explosionSound;

    [Header("Advanced")]
    [Tooltip("Maximum tilt angle during banking turns")]
    [Range(20f, 60f)]
    public float maxTiltAngle = 45f;

    [Tooltip("How sensitive the missile is to turning (affects banking)")]
    [Range(0.1f, 1f)]
    public float bankingSensitivity = 0.5f;

    /// <summary>
    /// Applies all preset values to a Missile3D instance
    /// </summary>
    public void ApplyToMissile(Missile3D missile)
    {
        // Physics
        missile.missileMass = Mass;  // Use Mass property which handles override automatically
        missile.maxVelocity = maxVelocity;
        missile.drag = drag;
        missile.velocityApproachRate = velocityApproachRate;

        // Fuel
        missile.fuel = fuel;
        missile.fuelConsumptionRate = fuelConsumptionRate;

        // Damage
        missile.payload = payload;
        missile.pushStrength = pushStrength;
        missile.damageVariation = damageVariation;

        // Self-destruct
        missile.detRadius = selfDestructRadius;
        missile.detDamageFactor = selfDestructDamageFactor;
        // Note: pushStrength is used for both impact and self-destruct currently

        // Visuals
        missile.maxVelocityColor = maxVelocityColor;
        missile.maxTiltAngle = maxTiltAngle;
        missile.bankingSensitivity = bankingSensitivity;

        // Apply visual model if provided
        if (visualModelPrefab != null)
        {
            ApplyVisualModel(missile);
        }

        // Apply trail color
        LineRenderer trail = missile.GetComponentInChildren<LineRenderer>();
        if (trail != null)
        {
            trail.startColor = trailColor;
            trail.endColor = trailColor * 0.5f; // Fade at end
        }
    }

    /// <summary>
    /// Swaps the missile's visual model if a custom one is provided
    /// </summary>
    private void ApplyVisualModel(Missile3D missile)
    {
        // Find existing mesh renderer
        MeshRenderer existingRenderer = missile.GetComponent<MeshRenderer>();
        MeshFilter existingFilter = missile.GetComponent<MeshFilter>();

        if (existingRenderer != null && visualModelPrefab != null)
        {
            // Get mesh from the prefab
            MeshFilter prefabFilter = visualModelPrefab.GetComponent<MeshFilter>();
            MeshRenderer prefabRenderer = visualModelPrefab.GetComponent<MeshRenderer>();

            if (prefabFilter != null && existingFilter != null)
            {
                existingFilter.mesh = prefabFilter.sharedMesh;
            }

            if (prefabRenderer != null)
            {
                existingRenderer.sharedMaterial = prefabRenderer.sharedMaterial;
            }
        }
    }

    /// <summary>
    /// Returns the calculated flight time in seconds based on fuel capacity
    /// </summary>
    public float GetMaxFlightTime()
    {
        return fuel / fuelConsumptionRate;
    }

    /// <summary>
    /// Returns a formatted stats string for UI display
    /// </summary>
    public string GetStatsDescription()
    {
        return $"<b>{missileName}</b> ({missileType})\n" +
               $"Speed: {maxVelocity} m/s\n" +
               $"Damage: {payload}\n" +
               $"Push: {pushStrength}\n" +
               $"Fuel: {fuel} lbs ({GetMaxFlightTime():F1}s)\n" +
               $"Mass: {displayMass:F0} lbs (Physics: {Mass:F2})";
    }
}

/// <summary>
/// Missile classification enum
/// </summary>
public enum MissileType
{
    Light,      // Fast, low damage, low fuel, weak push
    Medium,     // Balanced stats
    Heavy       // Slow, high damage, high fuel, strong push
}
