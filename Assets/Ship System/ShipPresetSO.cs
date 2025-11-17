using UnityEngine;

/// <summary>
/// Complete ship preset combining body, leveling formula, passives, perks, and move type.
/// This represents a complete pre-built ship OR a custom ship configuration.
/// Create via: Right-click -> Create -> GravityWars -> Ship System -> Ship Preset
/// </summary>
[CreateAssetMenu(fileName = "NewShipPreset", menuName = "GravityWars/Ship System/Ship Preset")]
public class ShipPresetSO : ScriptableObject
{
    [Header("Ship Identity")]
    [Tooltip("Display name of this ship (e.g., 'Iron Fortress')")]
    public string shipName = "New Ship";

    [Tooltip("Icon for ship selection UI")]
    public Sprite shipIcon;

    [TextArea(3, 5)]
    public string description = "Describe this ship's playstyle and strengths.";

    [Header("Core Components")]
    [Tooltip("Ship body defining archetype and base stats")]
    public ShipBodySO shipBody;

    [Tooltip("Leveling formula (auto-selected based on archetype if null)")]
    public ShipLevelingFormulaSO levelingFormula;

    [Header("Passive Abilities")]
    [Tooltip("Passive abilities (max supported by system, currently 1)")]
    public PassiveAbilitySO[] passives = new PassiveAbilitySO[1];

    [Header("Active Perks")]
    [Tooltip("Tier 1 perk (unlocks at level 5)")]
    public ActivePerkSO tier1Perk;

    [Tooltip("Tier 2 perk (unlocks at level 15)")]
    public ActivePerkSO tier2Perk;

    [Tooltip("Tier 3 perk (unlocks at level 20)")]
    public ActivePerkSO tier3Perk;

    [Header("Movement")]
    [Tooltip("Move type for this ship")]
    public MoveTypeSO moveType;

    [Header("Starting Missile (Optional)")]
    [Tooltip("Default missile equipped at game start (can be changed in loadout)")]
    public MissilePresetSO defaultMissile;

    [Header("Unlock Requirements")]
    [Tooltip("Account level required to unlock this ship (0 = available from start)")]
    [Range(0, 50)]
    public int requiredAccountLevel = 0;

    [Tooltip("Is this a premium ship? (seasonal battle pass exclusive)")]
    public bool isPremiumShip = false;

    [Header("Validation")]
    [SerializeField, HideInInspector] private string validationStatus = "";

    /// <summary>
    /// Applies this preset to a PlayerShip instance
    /// </summary>
    public void ApplyToShip(PlayerShip ship)
    {
        if (!Validate())
        {
            Debug.LogError($"Cannot apply {name} to ship: Validation failed!\n{validationStatus}");
            return;
        }

        // Apply ship body stats
        if (shipBody != null)
        {
            ship.shipModelName = shipBody.bodyName;
            ship.shipArchetype = shipBody.archetype;
            ship.baseHealth = shipBody.baseHealth;
            ship.armor = shipBody.baseArmor;
            ship.damageMultiplier = shipBody.baseDamageMultiplier;
            ship.movesAllowedPerTurn = shipBody.actionPointsPerTurn;

            // CRITICAL: Set base values BEFORE UpdateStatsFromLevel() uses them!
            ship.baseArmorValue = shipBody.baseArmor;
            ship.baseDamageMultiplier = shipBody.baseDamageMultiplier;

            // Apply rotation/handling settings
            ship.rotationSpeed = shipBody.rotationSpeed;
            ship.maxTiltAngle = shipBody.maxTiltAngle;
            ship.tiltSpeed = shipBody.tiltSpeed;
            ship.fineRotationSpeedMultiplier = shipBody.fineRotationMultiplier;
            ship.fineTiltSpeedMultiplier = shipBody.fineTiltMultiplier;
        }

        // Apply passives (supports multiple passives!)
        if (passives != null && passives.Length > 0)
        {
            // Reset ALL passives ONCE before applying
            PassiveAbilitySO.ResetAllPassives(ship);

            // Apply each passive in the array (they stack!)
            foreach (var passive in passives)
            {
                if (passive != null)
                {
                    passive.ApplyToShip(ship);
                    Debug.Log($"  → Applied passive: {passive.passiveName}");
                }
            }
        }

        // Apply move type
        if (moveType != null)
        {
            moveType.ApplyToShip(ship);
        }

        // Note: Perks are applied via PerkManager, not here
        // Note: Missile selection happens in loadout, not here

        // Recalculate stats based on current level
        ship.UpdateStatsFromLevel();

        Debug.Log($"Applied ship preset '{shipName}' to {ship.playerName}");
    }

    /// <summary>
    /// Validates this preset configuration
    /// </summary>
    public bool Validate()
    {
        validationStatus = "";
        bool isValid = true;

        // Check ship body
        if (shipBody == null)
        {
            validationStatus += "ERROR: Ship body is not assigned!\n";
            isValid = false;
        }

        // Check leveling formula (can be auto-selected)
        if (levelingFormula == null && shipBody != null)
        {
            validationStatus += "WARNING: No leveling formula assigned. Will use default for archetype.\n";
        }

        // Validate archetype compatibility
        if (shipBody != null)
        {
            ShipArchetype archetype = shipBody.archetype;

            // Check passives
            for (int i = 0; i < passives.Length; i++)
            {
                if (passives[i] != null && !passives[i].CanBeUsedBy(archetype))
                {
                    validationStatus += $"ERROR: Passive '{passives[i].passiveName}' cannot be used by {archetype}!\n";
                    isValid = false;
                }
            }

            // Check perks
            if (tier1Perk != null && !tier1Perk.CanBeUsedBy(archetype))
            {
                validationStatus += $"ERROR: Tier 1 perk '{tier1Perk.perkName}' cannot be used by {archetype}!\n";
                isValid = false;
            }
            if (tier2Perk != null && !tier2Perk.CanBeUsedBy(archetype))
            {
                validationStatus += $"ERROR: Tier 2 perk '{tier2Perk.perkName}' cannot be used by {archetype}!\n";
                isValid = false;
            }
            if (tier3Perk != null && !tier3Perk.CanBeUsedBy(archetype))
            {
                validationStatus += $"ERROR: Tier 3 perk '{tier3Perk.perkName}' cannot be used by {archetype}!\n";
                isValid = false;
            }

            // Check move type
            if (moveType != null && !moveType.CanBeUsedBy(archetype))
            {
                validationStatus += $"ERROR: Move type '{moveType.moveTypeName}' cannot be used by {archetype}!\n";
                isValid = false;
            }

            // Check default missile
            if (defaultMissile != null && !shipBody.CanUseMissileType(defaultMissile.missileType))
            {
                validationStatus += $"ERROR: Default missile '{defaultMissile.missileName}' ({defaultMissile.missileType}) cannot be used by this ship body!\n";
                isValid = false;
            }
        }

        if (isValid)
        {
            validationStatus = "✓ All validations passed!";
        }

        return isValid;
    }

    /// <summary>
    /// Auto-selects leveling formula based on ship body archetype
    /// </summary>
    public ShipLevelingFormulaSO GetLevelingFormula()
    {
        if (levelingFormula != null)
        {
            return levelingFormula;
        }

        if (shipBody == null)
        {
            Debug.LogWarning($"{name}: Cannot auto-select leveling formula - no ship body assigned!");
            return null;
        }

        // Try to find matching leveling formula in project
        // (In practice, you'd cache these or use a lookup table)
        string formulaName = $"{shipBody.archetype}LevelingFormula";
        ShipLevelingFormulaSO formula = Resources.Load<ShipLevelingFormulaSO>(formulaName);

        if (formula == null)
        {
            Debug.LogWarning($"{name}: Could not find leveling formula '{formulaName}' in Resources folder!");
        }

        return formula;
    }

    /// <summary>
    /// Returns a formatted preview string for UI display
    /// </summary>
    public string GetPreviewText(int atLevel = 1)
    {
        if (shipBody == null)
        {
            return "<color=red>Invalid ship - no body assigned!</color>";
        }

        string text = $"<b>{shipName}</b> ({shipBody.archetype})\n";
        text += $"{description}\n\n";

        // Stats at specified level
        ShipLevelingFormulaSO formula = GetLevelingFormula();
        if (formula != null)
        {
            float hp = formula.CalculateHealthAtLevel(shipBody.baseHealth, atLevel);
            float armor = formula.CalculateArmorAtLevel(shipBody.baseArmor, atLevel);
            float dmg = formula.CalculateDamageAtLevel(shipBody.baseDamageMultiplier, atLevel);
            float effectiveHP = formula.CalculateEffectiveHP(hp, armor);

            text += $"<b>Level {atLevel} Stats:</b>\n";
            text += $"HP: {hp:F0} (Effective: {effectiveHP:F0})\n";
            text += $"Armor: {armor:F1}\n";
            text += $"Damage: ×{dmg:F2}\n";
            text += $"Actions: {shipBody.actionPointsPerTurn}\n\n";
        }

        // Passive
        if (passives.Length > 0 && passives[0] != null)
        {
            text += $"<b>Passive:</b> {passives[0].passiveName}\n";
        }

        // Move type
        if (moveType != null)
        {
            text += $"<b>Move:</b> {moveType.moveTypeName}\n";
        }

        // Missile restrictions
        text += $"<b>Missiles:</b> {shipBody.GetMissileRestrictionsText()}\n";

        return text;
    }

    /// <summary>
    /// Validates on Inspector changes
    /// </summary>
    void OnValidate()
    {
        Validate();
    }
}
