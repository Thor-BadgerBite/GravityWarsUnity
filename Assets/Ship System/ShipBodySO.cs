using UnityEngine;

/// <summary>
/// Defines a ship body (chassis) with base stats for a specific archetype.
/// This represents the "base frame" of a ship before customization.
/// Create via: Right-click -> Create -> GravityWars -> Ship System -> Ship Body
/// </summary>
[CreateAssetMenu(fileName = "NewShipBody", menuName = "GravityWars/Ship System/Ship Body")]
public class ShipBodySO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name for this ship body (e.g., 'Heavy Chassis MK-I')")]
    public string bodyName = "Standard Chassis";

    [Tooltip("Ship archetype - determines stat ranges and restrictions")]
    public ShipArchetype archetype = ShipArchetype.AllAround;

    [Tooltip("Visual prefab for this ship body (optional - can be assigned to PlayerShip directly)")]
    public GameObject visualPrefab;

    [Tooltip("Icon for UI display")]
    public Sprite icon;

    [Header("Base Stats (Level 1)")]
    [Tooltip("Starting health points at level 1")]
    [Range(8000f, 15000f)]
    public float baseHealth = 10000f;

    [Tooltip("Starting armor value at level 1")]
    [Range(80f, 120f)]
    public float baseArmor = 100f;

    [Tooltip("Starting damage multiplier at level 1")]
    [Range(0.8f, 1.2f)]
    public float baseDamageMultiplier = 1.0f;

    [Header("Action Points")]
    [Tooltip("Number of actions per turn (3 normally, 4 for Controller)")]
    [Range(3, 4)]
    public int actionPointsPerTurn = 3;

    [Header("Missile Restrictions")]
    [Tooltip("Can this ship body use Light missiles?")]
    public bool canUseLightMissiles = true;

    [Tooltip("Can this ship body use Medium missiles?")]
    public bool canUseMediumMissiles = true;

    [Tooltip("Can this ship body use Heavy missiles?")]
    public bool canUseHeavyMissiles = true;

    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "A balanced ship chassis suitable for all-around combat.";

    /// <summary>
    /// Validates stat ranges based on archetype when values change in Inspector
    /// </summary>
    void OnValidate()
    {
        ValidateArchetypeStats();
        ValidateActionPoints();
        ValidateMissileRestrictions();
    }

    private void ValidateArchetypeStats()
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                // Tanks MUST have high HP
                if (baseHealth < 11000f)
                {
                    Debug.LogWarning($"[{name}] Tank ships should have baseHealth >= 11000. Current: {baseHealth}");
                    baseHealth = 11000f;
                }
                break;

            case ShipArchetype.DamageDealer:
                // DDs should have lower HP
                if (baseHealth > 10000f)
                {
                    Debug.LogWarning($"[{name}] DamageDealer ships should have baseHealth <= 10000. Current: {baseHealth}");
                    baseHealth = 10000f;
                }
                break;

            case ShipArchetype.Controller:
                // Controllers have moderate HP
                if (baseHealth > 10000f)
                {
                    Debug.LogWarning($"[{name}] Controller ships should have baseHealth <= 10000. Current: {baseHealth}");
                    baseHealth = 10000f;
                }
                break;

            case ShipArchetype.AllAround:
                // AllAround is flexible, no strict limits
                break;
        }
    }

    private void ValidateActionPoints()
    {
        // Controller gets 4 action points, others get 3
        if (archetype == ShipArchetype.Controller && actionPointsPerTurn != 4)
        {
            Debug.LogWarning($"[{name}] Controller ships should have 4 action points! Auto-corrected.");
            actionPointsPerTurn = 4;
        }
        else if (archetype != ShipArchetype.Controller && actionPointsPerTurn != 3)
        {
            Debug.LogWarning($"[{name}] {archetype} ships should have 3 action points! Auto-corrected.");
            actionPointsPerTurn = 3;
        }
    }

    private void ValidateMissileRestrictions()
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                // Tanks can't use Light missiles
                if (canUseLightMissiles)
                {
                    Debug.LogWarning($"[{name}] Tank ships cannot use Light missiles! Auto-corrected.");
                    canUseLightMissiles = false;
                }
                break;

            case ShipArchetype.Controller:
                // Controllers can't use Heavy missiles
                if (canUseHeavyMissiles)
                {
                    Debug.LogWarning($"[{name}] Controller ships cannot use Heavy missiles! Auto-corrected.");
                    canUseHeavyMissiles = false;
                }
                break;

            case ShipArchetype.DamageDealer:
            case ShipArchetype.AllAround:
                // No restrictions
                break;
        }
    }

    /// <summary>
    /// Checks if this ship body can use a specific missile type
    /// </summary>
    public bool CanUseMissileType(MissileType missileType)
    {
        switch (missileType)
        {
            case MissileType.Light:
                return canUseLightMissiles;
            case MissileType.Medium:
                return canUseMediumMissiles;
            case MissileType.Heavy:
                return canUseHeavyMissiles;
            default:
                return false;
        }
    }

    /// <summary>
    /// Returns a formatted description of missile restrictions
    /// </summary>
    public string GetMissileRestrictionsText()
    {
        string allowed = "";
        if (canUseLightMissiles) allowed += "Light, ";
        if (canUseMediumMissiles) allowed += "Medium, ";
        if (canUseHeavyMissiles) allowed += "Heavy, ";

        return allowed.TrimEnd(',', ' ');
    }

    /// <summary>
    /// Returns a formatted stats summary for UI display
    /// </summary>
    public string GetStatsPreview()
    {
        return $"<b>{bodyName}</b> ({archetype})\n" +
               $"Base HP: {baseHealth:F0}\n" +
               $"Base Armor: {baseArmor:F0}\n" +
               $"Base Damage: ×{baseDamageMultiplier:F2}\n" +
               $"Action Points: {actionPointsPerTurn}\n" +
               $"Missiles: {GetMissileRestrictionsText()}";
    }
}
