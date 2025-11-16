using UnityEngine;

/// <summary>
/// Defines how a ship archetype scales with levels (1-20).
/// Only 4 instances needed - one per archetype.
/// Create via: Right-click -> Create -> GravityWars -> Ship System -> Leveling Formula
/// </summary>
[CreateAssetMenu(fileName = "NewLevelingFormula", menuName = "GravityWars/Ship System/Leveling Formula")]
public class ShipLevelingFormulaSO : ScriptableObject
{
    [Header("Archetype")]
    [Tooltip("Which archetype does this formula apply to?")]
    public ShipArchetype archetype = ShipArchetype.AllAround;

    [Header("Stat Scaling Formulas")]
    [Tooltip("Health multiplier per level above 1 (e.g., 0.03 = +3% HP per level)")]
    [Range(0.01f, 0.05f)]
    public float healthScalingPerLevel = 0.03f;

    [Tooltip("Armor added per level above 1 (e.g., 3.0 = +3 armor per level)")]
    [Range(0.5f, 5f)]
    public float armorScalingPerLevel = 3.0f;

    [Tooltip("Damage multiplier added per level above 1 (e.g., 0.03 = +0.03 per level)")]
    [Range(0.01f, 0.05f)]
    public float damageScalingPerLevel = 0.03f;

    [Header("Preview")]
    [Tooltip("Preview level to show calculations (for testing)")]
    [Range(1, 20)]
    public int previewLevel = 10;

    [Header("Read-Only Preview (Inspector Only)")]
    [SerializeField, HideInInspector] private string previewStats;

    /// <summary>
    /// Calculates max health at a given level using this formula
    /// </summary>
    public float CalculateHealthAtLevel(float baseHealth, int level)
    {
        int levelOffset = Mathf.Max(0, level - 1);
        return baseHealth * (1f + healthScalingPerLevel * levelOffset);
    }

    /// <summary>
    /// Calculates armor at a given level using this formula
    /// </summary>
    public float CalculateArmorAtLevel(float baseArmor, int level)
    {
        int levelOffset = Mathf.Max(0, level - 1);
        return baseArmor + (armorScalingPerLevel * levelOffset);
    }

    /// <summary>
    /// Calculates damage multiplier at a given level using this formula
    /// </summary>
    public float CalculateDamageAtLevel(float baseDamage, int level)
    {
        int levelOffset = Mathf.Max(0, level - 1);
        return baseDamage + (damageScalingPerLevel * levelOffset);
    }

    /// <summary>
    /// Calculates effective HP (considering armor reduction)
    /// </summary>
    public float CalculateEffectiveHP(float health, float armor)
    {
        float armorReduction = armor / (armor + 400f);
        return health / (1f - armorReduction);
    }

    /// <summary>
    /// Validates scaling values based on archetype
    /// </summary>
    void OnValidate()
    {
        ValidateScalingValues();
        UpdatePreview();
    }

    private void ValidateScalingValues()
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                // Tank should have high health/armor scaling, low damage
                if (healthScalingPerLevel < 0.035f)
                {
                    Debug.LogWarning($"[{name}] Tank health scaling seems low. Recommended: 0.035+");
                }
                if (damageScalingPerLevel > 0.02f)
                {
                    Debug.LogWarning($"[{name}] Tank damage scaling seems high. Recommended: 0.015-0.02");
                }
                break;

            case ShipArchetype.DamageDealer:
                // DD should have low health/armor scaling, high damage
                if (healthScalingPerLevel > 0.025f)
                {
                    Debug.LogWarning($"[{name}] DamageDealer health scaling seems high. Recommended: 0.02");
                }
                if (damageScalingPerLevel < 0.035f)
                {
                    Debug.LogWarning($"[{name}] DamageDealer damage scaling seems low. Recommended: 0.04+");
                }
                break;

            case ShipArchetype.AllAround:
                // AllAround should be balanced
                if (healthScalingPerLevel < 0.025f || healthScalingPerLevel > 0.035f)
                {
                    Debug.LogWarning($"[{name}] AllAround health scaling should be ~0.03");
                }
                if (damageScalingPerLevel < 0.025f || damageScalingPerLevel > 0.035f)
                {
                    Debug.LogWarning($"[{name}] AllAround damage scaling should be ~0.03");
                }
                break;

            case ShipArchetype.Controller:
                // Controller should be similar to AllAround but slightly adjusted
                if (healthScalingPerLevel > 0.025f)
                {
                    Debug.LogWarning($"[{name}] Controller health scaling seems high. Recommended: 0.02");
                }
                break;
        }
    }

    private void UpdatePreview()
    {
        // Calculate preview stats at selected level
        // Using typical base values
        float baseHP = 10000f;
        float baseArmor = 100f;
        float baseDmg = 1.0f;

        float hp = CalculateHealthAtLevel(baseHP, previewLevel);
        float armor = CalculateArmorAtLevel(baseArmor, previewLevel);
        float dmg = CalculateDamageAtLevel(baseDmg, previewLevel);
        float effectiveHP = CalculateEffectiveHP(hp, armor);
        float armorReduction = armor / (armor + 400f);

        previewStats = $"=== {archetype} at Level {previewLevel} ===\n" +
                      $"Health: {hp:F0}\n" +
                      $"Armor: {armor:F1} ({armorReduction * 100:F1}% reduction)\n" +
                      $"Effective HP: {effectiveHP:F0}\n" +
                      $"Damage Mult: {dmg:F2}\n" +
                      $"Missile Dmg: {(2500 * dmg):F0} (with 2500 payload)";
    }

    /// <summary>
    /// Returns a stats preview string for UI display
    /// </summary>
    public string GetPreviewStats(float baseHealth, float baseArmor, float baseDamage, int level)
    {
        float hp = CalculateHealthAtLevel(baseHealth, level);
        float armor = CalculateArmorAtLevel(baseArmor, level);
        float dmg = CalculateDamageAtLevel(baseDamage, level);
        float effectiveHP = CalculateEffectiveHP(hp, armor);

        return $"<b>Level {level} Stats:</b>\n" +
               $"HP: {hp:F0} (Effective: {effectiveHP:F0})\n" +
               $"Armor: {armor:F1}\n" +
               $"Damage: ×{dmg:F2}";
    }
}
