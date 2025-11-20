using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Custom Ship Builder System
/// Handles the creation of custom ships from unlocked components:
/// - Ship Body (defines archetype + base stats + 3D model)
/// - 1 Passive Ability (with archetype restriction)
/// - 3 Active Abilities (one from each tier: 1, 2, 3)
/// - Custom Name
///
/// Custom ships act like prebuild ships and start at level 1.
/// Players can delete ships (prebuild or custom) to free slots, but lose all progress.
/// </summary>
public static class CustomShipBuilder
{
    /// <summary>
    /// Validate if a custom ship configuration is valid.
    /// Checks:
    /// - Ship body is unlocked
    /// - Passive is unlocked and compatible with body archetype
    /// - All 3 actives are unlocked, one from each tier
    /// - Player has an available custom slot
    /// </summary>
    public static ShipBuildValidation ValidateShipBuild(
        PlayerProfileData profile,
        string bodyId,
        string passiveId,
        string tier1ActiveId,
        string tier2ActiveId,
        string tier3ActiveId)
    {
        var validation = new ShipBuildValidation();

        // Check if ship body is unlocked
        if (!profile.unlockedShipBodies.Contains(bodyId))
        {
            validation.isValid = false;
            validation.errors.Add("Ship body is not unlocked");
            return validation;
        }

        // Get ship body data to check archetype
        var shipBody = ExtendedProgressionData.GetAllShipBodies()
            .FirstOrDefault(b => b.bodyId == bodyId);

        if (shipBody == null)
        {
            validation.isValid = false;
            validation.errors.Add("Invalid ship body ID");
            return validation;
        }

        // Check if passive is unlocked
        if (!profile.unlockedPassives.Contains(passiveId))
        {
            validation.isValid = false;
            validation.errors.Add("Passive ability is not unlocked");
        }

        // Check if passive is compatible with ship archetype
        var passive = ExtendedProgressionData.GetAllPassives()
            .FirstOrDefault(p => p.passiveId == passiveId);

        if (passive == null)
        {
            validation.isValid = false;
            validation.errors.Add("Invalid passive ID");
        }
        else if (passive.compatibleArchetype != shipBody.shipClass)
        {
            validation.isValid = false;
            validation.errors.Add($"Passive '{passive.username}' is not compatible with {shipBody.shipClass} archetype");
        }

        // Check Tier 1 active
        if (!ValidateActiveAbility(profile, tier1ActiveId, 1, out string tier1Error))
        {
            validation.isValid = false;
            validation.errors.Add(tier1Error);
        }

        // Check Tier 2 active
        if (!ValidateActiveAbility(profile, tier2ActiveId, 2, out string tier2Error))
        {
            validation.isValid = false;
            validation.errors.Add(tier2Error);
        }

        // Check Tier 3 active
        if (!ValidateActiveAbility(profile, tier3ActiveId, 3, out string tier3Error))
        {
            validation.isValid = false;
            validation.errors.Add(tier3Error);
        }

        // Check if player has available custom slots
        int maxSlots = ProgressionSystem.GetUnlockedCustomSlots(profile.level);
        int currentCustomShips = profile.customLoadouts.Count;

        if (currentCustomShips >= maxSlots)
        {
            validation.isValid = false;
            validation.errors.Add($"No available custom slots (max: {maxSlots}). Delete a ship to free a slot.");
        }

        return validation;
    }

    /// <summary>
    /// Validate a single active ability (check if unlocked and correct tier).
    /// </summary>
    private static bool ValidateActiveAbility(PlayerProfileData profile, string activeId, int expectedTier, out string error)
    {
        error = null;

        if (!profile.unlockedActives.Contains(activeId))
        {
            error = $"Tier {expectedTier} active ability is not unlocked";
            return false;
        }

        var active = ExtendedProgressionData.GetAllActives()
            .FirstOrDefault(a => a.activeId == activeId);

        if (active == null)
        {
            error = $"Invalid Tier {expectedTier} active ID";
            return false;
        }

        if (active.tier != expectedTier)
        {
            error = $"Active '{active.username}' is Tier {active.tier}, but Tier {expectedTier} is required";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Create a custom ship and add it to player's loadouts.
    /// Returns the created loadout or null if validation fails.
    /// </summary>
    public static CustomShipLoadout CreateCustomShip(
        PlayerProfileData profile,
        string bodyId,
        string passiveId,
        string tier1ActiveId,
        string tier2ActiveId,
        string tier3ActiveId,
        string customName)
    {
        // Validate the build
        var validation = ValidateShipBuild(profile, bodyId, passiveId, tier1ActiveId, tier2ActiveId, tier3ActiveId);

        if (!validation.isValid)
        {
            Debug.LogError($"[CustomShipBuilder] Build validation failed:\n{string.Join("\n", validation.errors)}");
            return null;
        }

        // Validate custom name
        if (string.IsNullOrWhiteSpace(customName))
        {
            Debug.LogError("[CustomShipBuilder] Ship name cannot be empty");
            return null;
        }

        if (customName.Length > 30)
        {
            Debug.LogError("[CustomShipBuilder] Ship name too long (max 30 characters)");
            return null;
        }

        // Create the custom loadout (using existing CustomShipLoadout structure)
        var loadout = new CustomShipLoadout
        {
            loadoutID = Guid.NewGuid().ToString(),
            loadoutName = customName,
            shipBodyName = bodyId,
            passiveNames = new List<string> { passiveId },  // Single passive (user wants only 1)
            tier1PerkName = tier1ActiveId,
            tier2PerkName = tier2ActiveId,
            tier3PerkName = tier3ActiveId,
            // equippedMissileName is selected separately before matches, not during building
            moveTypeName = "", // TODO: Set default move type based on ship body or remove if not needed
            skinID = "",
            colorSchemeID = "",
            decalID = ""
        };

        // Add to player's custom loadouts
        profile.customLoadouts.Add(loadout);

        Debug.Log($"[CustomShipBuilder] Created custom ship '{customName}' (ID: {loadout.loadoutID})");
        return loadout;
    }

    /// <summary>
    /// Delete a ship from player's account (frees a slot but loses ALL progress).
    /// Can delete BOTH prebuild ships and custom ships.
    /// Returns true if successfully deleted.
    /// </summary>
    public static bool DeleteShip(PlayerProfileData profile, string shipId, bool isPrebuildShip)
    {
        if (isPrebuildShip)
        {
            // Delete a prebuild ship (remove from unlocked list)
            if (profile.unlockedShipModels.Contains(shipId))
            {
                profile.unlockedShipModels.Remove(shipId);
                Debug.Log($"[CustomShipBuilder] Deleted prebuild ship: {shipId}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[CustomShipBuilder] Prebuild ship not found: {shipId}");
                return false;
            }
        }
        else
        {
            // Delete a custom ship (remove from loadouts)
            var loadout = profile.customLoadouts.FirstOrDefault(l => l.loadoutID == shipId);

            if (loadout != null)
            {
                var progression = profile.GetShipProgression(loadout);
                int shipLevel = progression != null ? progression.shipLevel : 1;
                profile.customLoadouts.Remove(loadout);
                Debug.Log($"[CustomShipBuilder] Deleted custom ship '{loadout.loadoutName}' (Level {shipLevel})");
                return true;
            }
            else
            {
                Debug.LogWarning($"[CustomShipBuilder] Custom ship not found: {shipId}");
                return false;
            }
        }
    }

    /// <summary>
    /// Get all actives unlocked by player, organized by tier.
    /// Useful for UI to display available actives per tier.
    /// </summary>
    public static Dictionary<int, List<ActiveUnlock>> GetActivesByTier(PlayerProfileData profile)
    {
        var activesByTier = new Dictionary<int, List<ActiveUnlock>>
        {
            { 1, new List<ActiveUnlock>() },
            { 2, new List<ActiveUnlock>() },
            { 3, new List<ActiveUnlock>() }
        };

        var allActives = ExtendedProgressionData.GetAllActives();

        foreach (var activeId in profile.unlockedActives)
        {
            var active = allActives.FirstOrDefault(a => a.activeId == activeId);
            if (active != null)
            {
                activesByTier[active.tier].Add(active);
            }
        }

        return activesByTier;
    }

    /// <summary>
    /// Get all passives compatible with a specific ship archetype.
    /// Useful for UI to show only compatible passives.
    /// </summary>
    public static List<PassiveUnlock> GetCompatiblePassives(PlayerProfileData profile, ShipClass archetype)
    {
        var compatiblePassives = new List<PassiveUnlock>();
        var allPassives = ExtendedProgressionData.GetAllPassives();

        foreach (var passiveId in profile.unlockedPassives)
        {
            var passive = allPassives.FirstOrDefault(p => p.passiveId == passiveId);
            if (passive != null && passive.compatibleArchetype == archetype)
            {
                compatiblePassives.Add(passive);
            }
        }

        return compatiblePassives;
    }
}

/// <summary>
/// Validation result for ship building.
/// </summary>
public class ShipBuildValidation
{
    public bool isValid = true;
    public List<string> errors = new List<string>();
}
