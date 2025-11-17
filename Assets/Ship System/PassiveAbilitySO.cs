using UnityEngine;

/// <summary>
/// Defines a passive ability that can be equipped to ships.
/// Passives are always active and unlock at level 10.
/// Create via: Right-click -> Create -> GravityWars -> Ship System -> Passive Ability
/// </summary>
[CreateAssetMenu(fileName = "NewPassive", menuName = "GravityWars/Ship System/Passive Ability")]
public class PassiveAbilitySO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name of this passive (e.g., 'Sniper Mode')")]
    public string passiveName = "New Passive";

    [Tooltip("Icon for UI display")]
    public Sprite icon;

    [Tooltip("Passive type - used to identify which passive to enable")]
    public PassiveType passiveType = PassiveType.None;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description = "Describe what this passive does.";

    [Header("Unlock Requirements")]
    [Tooltip("Level required to unlock this passive")]
    [Range(1, 20)]
    public int unlockLevel = 10;

    [Tooltip("Account level required to unlock this passive (0 = available from start)")]
    [Range(0, 50)]
    public int requiredAccountLevel = 0;

    [Header("Archetype Restrictions")]
    [Tooltip("Can Tank ships use this passive?")]
    public bool allowTank = true;

    [Tooltip("Can DamageDealer ships use this passive?")]
    public bool allowDamageDealer = true;

    [Tooltip("Can Controller ships use this passive?")]
    public bool allowController = true;

    [Tooltip("Can AllAround ships use this passive?")]
    public bool allowAllAround = true;

    [Header("Passive-Specific Parameters")]
    [Tooltip("Generic float value (use depends on passive type)")]
    public float value1 = 0f;

    [Tooltip("Generic float value (use depends on passive type)")]
    public float value2 = 0f;

    [Tooltip("Generic bool flag (use depends on passive type)")]
    public bool flag1 = false;

    /// <summary>
    /// Checks if this passive can be equipped to a ship with the given archetype
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
    /// Applies this passive to a PlayerShip (does NOT reset other passives - supports multiple passives!)
    /// </summary>
    public void ApplyToShip(PlayerShip ship)
    {
        // NOTE: We do NOT reset all passives here - this allows multiple passives to coexist!
        // The caller (ShipPresetSO) is responsible for resetting if needed

        // Enable the specific passive
        switch (passiveType)
        {
            case PassiveType.SniperMode:
                ship.sniperMode = true;
                break;

            case PassiveType.Unmovable:
                ship.unmovable = true;
                break;

            case PassiveType.EnhancedRegeneration:
                ship.enhancedRegeneration = true;
                ship.regenRate = value1 > 0 ? value1 : 1f;
                break;

            case PassiveType.DamageResistance:
                ship.damageResistancePassive = true;
                ship.damageResistancePercentage = value1 > 0 ? value1 : 0.15f;
                break;

            case PassiveType.CriticalImmunity:
                ship.criticalImmunity = true;
                break;

            case PassiveType.CriticalEnhancement:
                ship.CriticalEnhancement = true;
                break;

            case PassiveType.DamageBoost:
                ship.damageBoostPassive = true;
                break;

            case PassiveType.LastChance:
                ship.hasLastChancePassive = true;
                break;

            case PassiveType.AdaptiveArmor:
                ship.adaptiveArmorPassive = true;
                break;

            case PassiveType.AdaptiveDamage:
                ship.adaptiveDamagePassive = true;
                break;

            case PassiveType.PrecisionEngineering:
                ship.precisionEngineering = true;
                break;

            case PassiveType.CollisionAvoidance:
                ship.collisionAvoidancePassive = true;
                break;

            case PassiveType.Lifesteal:
                ship.lifestealPassive = true;
                ship.lifestealPercent = value1 > 0 ? value1 : 0.2f;
                break;

            case PassiveType.ReduceDamageFromHighSpeed:
                ship.reduceDamageFromHighSpeedMissiles = true;
                ship.highSpeedDamageReductionPercent = value1 > 0 ? value1 : 0.2f;
                break;

            case PassiveType.IncreaseDamageOnHighSpeed:
                ship.increaseDamageOnHighSpeedMissiles = true;
                ship.highSpeedDamageAmplifyPercent = value1 > 0 ? value1 : 0.2f;
                break;

            case PassiveType.None:
            default:
                // No passive enabled
                break;
        }
    }

    /// <summary>
    /// Resets all passive flags on a ship (call this ONCE before applying multiple passives)
    /// </summary>
    public static void ResetAllPassives(PlayerShip ship)
    {
        ship.sniperMode = false;
        ship.unmovable = false;
        ship.enhancedRegeneration = false;
        ship.damageResistancePassive = false;
        ship.criticalImmunity = false;
        ship.CriticalEnhancement = false;
        ship.damageBoostPassive = false;
        ship.hasLastChancePassive = false;
        ship.adaptiveArmorPassive = false;
        ship.adaptiveDamagePassive = false;
        ship.precisionEngineering = false;
        ship.collisionAvoidancePassive = false;
        ship.lifestealPassive = false;
        ship.reduceDamageFromHighSpeedMissiles = false;
        ship.increaseDamageOnHighSpeedMissiles = false;
    }

    /// <summary>
    /// Validates archetype restrictions based on balance recommendations
    /// </summary>
    void OnValidate()
    {
        ValidateBalanceRestrictions();
    }

    private void ValidateBalanceRestrictions()
    {
        // Warn about potentially overpowered combinations
        switch (passiveType)
        {
            case PassiveType.AdaptiveArmor:
                if (allowTank)
                {
                    Debug.LogWarning($"[{name}] AdaptiveArmor on Tank is OVERPOWERED! Consider disabling allowTank.");
                }
                break;

            case PassiveType.EnhancedRegeneration:
                if (allowTank)
                {
                    Debug.LogWarning($"[{name}] EnhancedRegeneration on Tank is very strong! Tank already has high HP.");
                }
                break;

            case PassiveType.DamageResistance:
                if (allowTank)
                {
                    Debug.LogWarning($"[{name}] DamageResistance on Tank stacks with armor! Consider balance testing.");
                }
                break;

            case PassiveType.CriticalEnhancement:
                if (allowTank)
                {
                    Debug.LogWarning($"[{name}] CriticalEnhancement is better suited for DamageDealer ships.");
                }
                break;
        }
    }

    /// <summary>
    /// Returns a formatted info string for UI display
    /// </summary>
    public string GetInfoText()
    {
        return $"<b>{passiveName}</b>\n" +
               $"{description}\n\n" +
               $"Unlock: Level {unlockLevel}\n" +
               $"Allowed: {GetAllowedArchetypes()}";
    }
}

/// <summary>
/// Enum for all passive ability types
/// </summary>
public enum PassiveType
{
    None,
    SniperMode,
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
    ReduceDamageFromHighSpeed,
    IncreaseDamageOnHighSpeed
}
