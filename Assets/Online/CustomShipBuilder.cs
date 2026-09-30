using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Custom Ship Builder - ID-BASED WRAPPER around the canonical builder.
///
/// The single source of truth for ship building rules is
/// ProgressionManager.ValidateLoadoutBuild / CreateCustomLoadout (SO-based):
/// - Ship body + move type (archetype-compatible, unlocked)
/// - Exactly 1 passive (archetype-restricted)
/// - 3 active perks, one from each tier (1/2/3)
/// - Missile is NOT part of the build - it is retrofitted before each match
///   (MissileSelectionUI); changing it never resets ship XP
/// - Custom slots limited by account level (1/2/3 at levels 1/20/40)
///
/// This wrapper resolves string ids (as used by the online/progression
/// schedules - which match the generated asset names) to the actual
/// ScriptableObjects and delegates. Use it from online flows that only have
/// ids; use ProgressionManager directly when you already hold SO references.
/// </summary>
public static class CustomShipBuilder
{
    /// <summary>
    /// Validate a custom ship configuration by component ids.
    /// Delegates to ProgressionManager.ValidateLoadoutBuild (canonical rules).
    /// </summary>
    public static ShipBuildValidation ValidateShipBuild(
        PlayerAccountData profile,
        string bodyId,
        string passiveId,
        string tier1ActiveId,
        string tier2ActiveId,
        string tier3ActiveId,
        string shipName = "Custom Ship")
    {
        var pm = ProgressionManager.Instance;
        if (pm == null)
        {
            var failed = new ShipBuildValidation();
            failed.Fail("ProgressionManager not available - cannot validate builds");
            return failed;
        }

        var body = ResolveBody(pm, bodyId);
        if (body == null)
        {
            var failed = new ShipBuildValidation();
            failed.Fail($"Unknown ship body id '{bodyId}' (no matching ShipBodySO asset)");
            return failed;
        }

        var passives = new List<PassiveAbilitySO>();
        var passive = ResolvePassive(pm, passiveId);
        if (passive != null) passives.Add(passive);

        return pm.ValidateLoadoutBuild(
            shipName,
            body,
            ResolveDefaultMoveType(pm, body),
            null, // missile is retrofitted pre-match, never part of the build
            ResolvePerk(pm, tier1ActiveId),
            ResolvePerk(pm, tier2ActiveId),
            ResolvePerk(pm, tier3ActiveId),
            passives);
    }

    /// <summary>
    /// Create a custom ship by component ids and add it to the player's loadouts.
    /// Delegates to ProgressionManager.CreateCustomLoadout (canonical path).
    /// Returns the created loadout or null if validation fails.
    /// </summary>
    public static CustomShipLoadout CreateCustomShip(
        PlayerAccountData profile,
        string bodyId,
        string passiveId,
        string tier1ActiveId,
        string tier2ActiveId,
        string tier3ActiveId,
        string customName)
    {
        var pm = ProgressionManager.Instance;
        if (pm == null)
        {
            Debug.LogError("[CustomShipBuilder] ProgressionManager not available - cannot build ships");
            return null;
        }

        var body = ResolveBody(pm, bodyId);
        if (body == null)
        {
            Debug.LogError($"[CustomShipBuilder] Unknown ship body id '{bodyId}'");
            return null;
        }

        var passives = new List<PassiveAbilitySO>();
        var passive = ResolvePassive(pm, passiveId);
        if (passive != null) passives.Add(passive);

        // Missile intentionally omitted - selected before each match.
        return pm.CreateCustomLoadout(
            customName,
            body,
            ResolveDefaultMoveType(pm, body),
            null,
            ResolvePerk(pm, tier1ActiveId),
            ResolvePerk(pm, tier2ActiveId),
            ResolvePerk(pm, tier3ActiveId),
            passives);
    }

    /// <summary>
    /// Delete a ship from the player's account (frees a slot but loses ALL progress).
    /// Can delete BOTH prebuild ships and custom ships.
    /// Returns true if successfully deleted.
    /// </summary>
    public static bool DeleteShip(PlayerAccountData profile, string shipId, bool isPrebuildShip)
    {
        bool deleted;

        if (isPrebuildShip)
        {
            deleted = profile.unlockedShipModels.Remove(shipId);
            if (deleted) Debug.Log($"[CustomShipBuilder] Deleted prebuild ship: {shipId}");
            else Debug.LogWarning($"[CustomShipBuilder] Prebuild ship not found: {shipId}");
        }
        else
        {
            var loadout = profile.customLoadouts.FirstOrDefault(l => l.loadoutID == shipId);
            if (loadout != null)
            {
                var progression = profile.GetShipProgression(loadout);
                int shipLevel = progression != null ? progression.shipLevel : 1;
                profile.customLoadouts.Remove(loadout);
                Debug.Log($"[CustomShipBuilder] Deleted custom ship '{loadout.loadoutName}' (Level {shipLevel})");
                deleted = true;
            }
            else
            {
                Debug.LogWarning($"[CustomShipBuilder] Custom ship not found: {shipId}");
                deleted = false;
            }
        }

        if (deleted && ProgressionManager.Instance != null &&
            ProgressionManager.Instance.currentPlayerData == profile)
        {
            ProgressionManager.Instance.Save();
        }

        return deleted;
    }

    #region UI Helpers (SO-based)

    /// <summary>
    /// All perks the player has unlocked, organized by tier (for builder UI).
    /// </summary>
    public static Dictionary<int, List<ActivePerkSO>> GetActivesByTier(PlayerAccountData profile)
    {
        var byTier = new Dictionary<int, List<ActivePerkSO>>
        {
            { 1, new List<ActivePerkSO>() },
            { 2, new List<ActivePerkSO>() },
            { 3, new List<ActivePerkSO>() }
        };

        var pm = ProgressionManager.Instance;
        if (pm == null) return byTier;

        foreach (var perk in pm.allPerks)
        {
            if (perk == null || !byTier.ContainsKey(perk.tier)) continue;
            if (pm.IsUnlocked(perk))
                byTier[perk.tier].Add(perk);
        }

        return byTier;
    }

    /// <summary>
    /// All unlocked passives compatible with a ship body's archetype (for builder UI).
    /// </summary>
    public static List<PassiveAbilitySO> GetCompatiblePassives(PlayerAccountData profile, ShipArchetype archetype)
    {
        var compatible = new List<PassiveAbilitySO>();

        var pm = ProgressionManager.Instance;
        if (pm == null) return compatible;

        foreach (var passive in pm.allPassives)
        {
            if (passive == null) continue;
            if (pm.IsUnlocked(passive) && passive.CanBeUsedBy(archetype))
                compatible.Add(passive);
        }

        return compatible;
    }

    #endregion

    #region Id → ScriptableObject Resolution

    private static ShipBodySO ResolveBody(ProgressionManager pm, string id)
    {
        return pm.allShipBodies.FirstOrDefault(b => b != null && b.name == id);
    }

    private static PassiveAbilitySO ResolvePassive(ProgressionManager pm, string id)
    {
        return pm.allPassives.FirstOrDefault(p => p != null && p.name == id);
    }

    private static ActivePerkSO ResolvePerk(ProgressionManager pm, string id)
    {
        return pm.allPerks.FirstOrDefault(p => p != null && p.name == id);
    }

    /// <summary>
    /// Default move type for id-based builds: the first unlocked move type the
    /// body's archetype can use (standard move is always unlocked for new
    /// accounts, so this resolves for everyone).
    /// </summary>
    private static MoveTypeSO ResolveDefaultMoveType(ProgressionManager pm, ShipBodySO body)
    {
        return pm.allMoveTypes.FirstOrDefault(m =>
            m != null && pm.IsUnlocked(m) && m.CanBeUsedBy(body.archetype));
    }

    #endregion
}

/// <summary>
/// Validation result for ship building, with per-error messages for UI display.
/// </summary>
public class ShipBuildValidation
{
    public bool isValid = true;
    public List<string> errors = new List<string>();

    /// <summary>Marks the validation as failed with a reason.</summary>
    public void Fail(string error)
    {
        isValid = false;
        errors.Add(error);
    }

    /// <summary>All errors joined for quick display.</summary>
    public string GetErrorText() => string.Join("\n", errors);
}
