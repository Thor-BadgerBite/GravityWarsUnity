using UnityEngine;

/// <summary>
/// Defines a move type (Normal/Precision/Warp) with archetype restrictions.
/// Only 3 instances needed - one for each move type.
/// Create via: Right-click -> Create -> GravityWars -> Ship System -> Move Type
/// </summary>
[CreateAssetMenu(fileName = "NewMoveType", menuName = "GravityWars/Ship System/Move Type")]
public class MoveTypeSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name of this move type")]
    public string moveTypeName = "Normal Move";

    [Tooltip("Icon for UI display")]
    public Sprite icon;

    [Tooltip("Move type category")]
    public MoveTypeCategory category = MoveTypeCategory.Normal;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description = "Standard ship movement with deceleration.";

    [Header("Archetype Restrictions")]
    [Tooltip("Can Tank ships use this move type?")]
    public bool allowTank = true;

    [Tooltip("Can DamageDealer ships use this move type?")]
    public bool allowDamageDealer = true;

    [Tooltip("Can Controller ships use this move type?")]
    public bool allowController = true;

    [Tooltip("Can AllAround ships use this move type?")]
    public bool allowAllAround = true;

    [Header("Movement Speed (Slingshot)")]
    [Tooltip("Minimum slingshot speed when velocity slider is at 0%")]
    [Range(1f, 5f)]
    public float minMoveSpeed = 2f;

    [Tooltip("Maximum slingshot speed when velocity slider is at 100%")]
    [Range(5f, 15f)]
    public float maxMoveSpeed = 10f;

    [Tooltip("Deceleration rate during slingshot move (m/s²)")]
    [Range(2f, 8f)]
    public float moveDeceleration = 4f;

    [Tooltip("Maximum duration of slingshot move (seconds)")]
    [Range(1f, 5f)]
    public float moveDuration = 2.5f;

    [Header("Warp Parameters (Warp Move Only)")]
    [Tooltip("Zoom duration for warp moves (only applies to Warp)")]
    [Range(0.1f, 1f)]
    public float warpZoomDuration = 0.3f;

    [Tooltip("Minimum scale factor for warp zoom (only applies to Warp)")]
    [Range(0.1f, 0.5f)]
    public float minScaleFactor = 0.2f;

    [Tooltip("Post-warp shake duration (only applies to Warp)")]
    [Range(0.5f, 2f)]
    public float postWarpShakeTime = 1.0f;

    [Tooltip("Post-warp shake angle (only applies to Warp)")]
    [Range(5f, 30f)]
    public float postWarpShakeAngle = 15f;

    /// <summary>
    /// Checks if this move type can be used by a ship with the given archetype
    /// </summary>
    public bool CanBeUsedBy(ShipArchetype archetype)
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                return allowTank;
            case ShipArchetype.DamageDealer:
                return allowDamageDealer;
            case ShipArchetype.Controller:
                return allowController;
            case ShipArchetype.AllAround:
                return allowAllAround;
            default:
                return false;
        }
    }

    /// <summary>
    /// Returns a list of allowed archetypes as a string
    /// </summary>
    public string GetAllowedArchetypes()
    {
        string result = "";
        if (allowTank) result += "Tank, ";
        if (allowDamageDealer) result += "DamageDealer, ";
        if (allowController) result += "Controller, ";
        if (allowAllAround) result += "AllAround, ";

        return result.TrimEnd(',', ' ');
    }

    /// <summary>
    /// Applies this move type to a PlayerShip
    /// </summary>
    public void ApplyToShip(PlayerShip ship)
    {
        // Reset all move types first
        ship.precisionMove = false;
        ship.warpMove = false;

        // Apply movement speed parameters (all move types use these)
        ship.minMoveSpeed = minMoveSpeed;
        ship.maxMoveSpeed = maxMoveSpeed;
        ship.moveDeceleration = moveDeceleration;
        ship.moveDuration = moveDuration;

        // Enable the specific move type
        switch (category)
        {
            case MoveTypeCategory.Normal:
                // Normal move is default - no flags needed
                break;

            case MoveTypeCategory.Precision:
                ship.precisionMove = true;
                break;

            case MoveTypeCategory.Warp:
                ship.warpMove = true;
                // Apply warp-specific parameters (NOTE: These require SerializeField warp parameters in PlayerShip)
                // ship.warpZoomDuration = warpZoomDuration;
                // ship.minScaleFactor = minScaleFactor;
                // ship.postWarpShakeTime = postWarpShakeTime;
                // ship.postWarpShakeAngle = postWarpShakeAngle;
                break;
        }
    }

    /// <summary>
    /// Validates archetype restrictions based on design rules
    /// </summary>
    void OnValidate()
    {
        ValidateDesignRules();
    }

    private void ValidateDesignRules()
    {
        switch (category)
        {
            case MoveTypeCategory.Normal:
                // Normal move should be available to all
                if (!allowTank || !allowDamageDealer || !allowController || !allowAllAround)
                {
                    Debug.LogWarning($"[{name}] Normal move should be available to ALL archetypes!");
                }
                break;

            case MoveTypeCategory.Precision:
                // Precision move should NOT be available to Tank
                if (allowTank)
                {
                    Debug.LogWarning($"[{name}] Precision move should NOT be available to Tank (too bulky)! Auto-corrected.");
                    allowTank = false;
                }
                break;

            case MoveTypeCategory.Warp:
                // Warp move should ONLY be available to Controller
                if (allowTank || allowDamageDealer || allowAllAround)
                {
                    Debug.LogWarning($"[{name}] Warp move should be EXCLUSIVE to Controller! Auto-corrected.");
                    allowTank = false;
                    allowDamageDealer = false;
                    allowAllAround = false;
                }
                if (!allowController)
                {
                    Debug.LogWarning($"[{name}] Warp move should be available to Controller! Auto-corrected.");
                    allowController = true;
                }
                break;
        }
    }

    /// <summary>
    /// Returns a formatted info string for UI display
    /// </summary>
    public string GetInfoText()
    {
        string categoryText = category switch
        {
            MoveTypeCategory.Normal => "Standard movement with deceleration",
            MoveTypeCategory.Precision => "Shows ghost preview of final position",
            MoveTypeCategory.Warp => "Instant teleport with zoom animation (Controller exclusive!)",
            _ => "Unknown move type"
        };

        return $"<b>{moveTypeName}</b>\n" +
               $"{categoryText}\n\n" +
               $"{description}\n\n" +
               $"Allowed: {GetAllowedArchetypes()}";
    }
}

/// <summary>
/// Enum for move type categories
/// </summary>
public enum MoveTypeCategory
{
    Normal,      // Standard slingshot move
    Precision,   // Shows ghost preview
    Warp         // Instant teleport
}
