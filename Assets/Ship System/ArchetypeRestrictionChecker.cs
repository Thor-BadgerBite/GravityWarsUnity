using UnityEngine;

/// <summary>
/// Central validation system for archetype restrictions.
/// Ensures balance by preventing overpowered combinations.
/// </summary>
public static class ArchetypeRestrictionChecker
{
    /// <summary>
    /// Validates a complete ship configuration for balance
    /// </summary>
    public static ValidationResult ValidateShipConfiguration(ShipPresetSO preset)
    {
        ValidationResult result = new ValidationResult();

        if (preset == null || preset.shipBody == null)
        {
            result.AddError("Ship preset or body is null!");
            return result;
        }

        ShipArchetype archetype = preset.shipBody.archetype;

        // Validate passives
        ValidatePassives(preset, archetype, result);

        // Validate perks
        ValidatePerks(preset, archetype, result);

        // Validate move type
        ValidateMoveType(preset, archetype, result);

        // Validate missile compatibility
        ValidateMissileCompatibility(preset, result);

        // Check for overpowered combinations
        CheckForOPCombinations(preset, archetype, result);

        return result;
    }

    private static void ValidatePassives(ShipPresetSO preset, ShipArchetype archetype, ValidationResult result)
    {
        if (preset.passives == null) return;

        foreach (var passive in preset.passives)
        {
            if (passive == null) continue;

            if (!passive.CanBeUsedBy(archetype))
            {
                result.AddError($"Passive '{passive.passiveName}' cannot be used by {archetype}");
            }
        }
    }

    private static void ValidatePerks(ShipPresetSO preset, ShipArchetype archetype, ValidationResult result)
    {
        if (preset.tier1Perk != null && !preset.tier1Perk.CanBeUsedBy(archetype))
        {
            result.AddError($"Tier 1 perk '{preset.tier1Perk.perkName}' cannot be used by {archetype}");
        }

        if (preset.tier2Perk != null && !preset.tier2Perk.CanBeUsedBy(archetype))
        {
            result.AddError($"Tier 2 perk '{preset.tier2Perk.perkName}' cannot be used by {archetype}");
        }

        if (preset.tier3Perk != null && !preset.tier3Perk.CanBeUsedBy(archetype))
        {
            result.AddError($"Tier 3 perk '{preset.tier3Perk.perkName}' cannot be used by {archetype}");
        }
    }

    private static void ValidateMoveType(ShipPresetSO preset, ShipArchetype archetype, ValidationResult result)
    {
        if (preset.moveType == null)
        {
            result.AddWarning("No move type assigned - will use default");
            return;
        }

        if (!preset.moveType.CanBeUsedBy(archetype))
        {
            result.AddError($"Move type '{preset.moveType.moveTypeName}' cannot be used by {archetype}");
        }
    }

    private static void ValidateMissileCompatibility(ShipPresetSO preset, ValidationResult result)
    {
        if (preset.defaultMissile == null || preset.shipBody == null) return;

        if (!preset.shipBody.CanUseMissileType(preset.defaultMissile.missileType))
        {
            result.AddError($"Default missile '{preset.defaultMissile.missileName}' ({preset.defaultMissile.missileType}) is incompatible with ship body");
        }
    }

    private static void CheckForOPCombinations(ShipPresetSO preset, ShipArchetype archetype, ValidationResult result)
    {
        // Check for known overpowered combinations
        if (archetype == ShipArchetype.Tank)
        {
            // Tank + AdaptiveArmor = Unkillable
            if (HasPassiveType(preset, PassiveType.AdaptiveArmor))
            {
                result.AddWarning("Tank + AdaptiveArmor is OVERPOWERED! This will snowball to unkillable.");
            }

            // Tank + Regen = Very strong
            if (HasPassiveType(preset, PassiveType.EnhancedRegeneration))
            {
                result.AddWarning("Tank + EnhancedRegeneration is very strong. Monitor balance in testing.");
            }

            // Tank + DamageResistance = Stacks with armor
            if (HasPassiveType(preset, PassiveType.DamageResistance))
            {
                result.AddWarning("Tank + DamageResistance stacks with high armor. May be too strong.");
            }
        }

        // Check action points vs archetype
        if (archetype == ShipArchetype.Controller && preset.shipBody.actionPointsPerTurn != 4)
        {
            result.AddWarning("Controller should have 4 action points for unique advantage!");
        }
        else if (archetype != ShipArchetype.Controller && preset.shipBody.actionPointsPerTurn != 3)
        {
            result.AddWarning($"{archetype} should have 3 action points for balance!");
        }

        // Check move type exclusivity
        if (preset.moveType != null && preset.moveType.category == MoveTypeCategory.Warp)
        {
            if (archetype != ShipArchetype.Controller)
            {
                result.AddError("Warp move should be EXCLUSIVE to Controller ships!");
            }
        }
    }

    private static bool HasPassiveType(ShipPresetSO preset, PassiveType type)
    {
        if (preset.passives == null) return false;

        foreach (var passive in preset.passives)
        {
            if (passive != null && passive.passiveType == type)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Result of a validation check
/// </summary>
public class ValidationResult
{
    public bool IsValid => errors.Count == 0;
    public int ErrorCount => errors.Count;
    public int WarningCount => warnings.Count;

    private System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
    private System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();

    public void AddError(string message)
    {
        errors.Add(message);
        Debug.LogError($"[VALIDATION ERROR] {message}");
    }

    public void AddWarning(string message)
    {
        warnings.Add(message);
        Debug.LogWarning($"[VALIDATION WARNING] {message}");
    }

    public string GetSummary()
    {
        if (IsValid && warnings.Count == 0)
        {
            return "✓ All validations passed!";
        }

        string summary = "";

        if (errors.Count > 0)
        {
            summary += $"<color=red>ERRORS ({errors.Count}):</color>\n";
            foreach (var error in errors)
            {
                summary += $"  • {error}\n";
            }
        }

        if (warnings.Count > 0)
        {
            summary += $"<color=yellow>WARNINGS ({warnings.Count}):</color>\n";
            foreach (var warning in warnings)
            {
                summary += $"  • {warning}\n";
            }
        }

        return summary.TrimEnd('\n');
    }

    public override string ToString()
    {
        return GetSummary();
    }
}
