using System.Linq;
using UnityEngine;

/// <summary>
/// The missing link between the meta-game and the actual match:
/// applies the player's selected ship (a ShipPresetSO or a custom
/// CustomShipLoadout) onto the PlayerShip instances that GameManager spawns.
///
/// Before this existed, ships always played with the prefab's Inspector
/// defaults and every loadout/missile selection was ignored in-game.
///
/// Called from GameManager.PlaceShips right after the ships are instantiated
/// (PlayerShip.Start and PerkManager slot reload pick everything up).
/// </summary>
public static class MatchLoadoutBridge
{
    /// <summary>
    /// Applies an explicit ShipPresetSO to a spawned ship (prebuilt ships).
    /// </summary>
    public static void ApplyPreset(PlayerShip ship, ShipPresetSO preset)
    {
        if (ship == null || preset == null) return;

        ship.shipPreset = preset;
        if (preset.defaultMissile != null)
            ship.equippedMissile = preset.defaultMissile;

        // PerkManager reads the preset in Awake (already ran at Instantiate),
        // so it must reload its slots now that the preset changed.
        ship.GetComponent<PerkManager>()?.ReloadSlotsFromPreset();

        Debug.Log($"[LoadoutBridge] Applied preset '{preset.shipName}' to {ship.playerName}");
    }

    /// <summary>
    /// Applies the local player's currently equipped custom loadout to a
    /// spawned ship. Resolves every component by name from the
    /// ProgressionManager content databases, builds a runtime ShipPresetSO and
    /// lets the normal preset pipeline (PlayerShip.Start) apply it.
    /// Returns false when there is nothing to apply (prefab defaults remain).
    /// </summary>
    public static bool ApplyEquippedLoadout(PlayerShip ship)
    {
        var pm = ProgressionManager.Instance;
        var data = pm != null ? pm.currentPlayerData : null;
        if (ship == null || data == null || data.customShipLoadouts.Count == 0)
            return false;

        var loadout = data.customShipLoadouts.Find(l =>
                          l.loadoutID == data.currentEquippedShipId ||
                          l.loadoutID == data.selectedCasualLoadoutId)
                      ?? data.customShipLoadouts[0];

        return ApplyLoadout(ship, loadout);
    }

    /// <summary>
    /// Applies a specific custom loadout to a spawned ship.
    /// </summary>
    public static bool ApplyLoadout(PlayerShip ship, CustomShipLoadout loadout)
    {
        var pm = ProgressionManager.Instance;
        if (ship == null || loadout == null || pm == null) return false;

        var body = pm.allShipBodies.FirstOrDefault(b => b != null && b.name == loadout.shipBodyName);
        if (body == null)
        {
            Debug.LogWarning($"[LoadoutBridge] Body '{loadout.shipBodyName}' not found - keeping prefab defaults");
            return false;
        }

        // Build a runtime preset so the standard ApplyToShip pipeline runs
        var runtimePreset = ScriptableObject.CreateInstance<ShipPresetSO>();
        runtimePreset.name = loadout.loadoutName;
        runtimePreset.shipName = loadout.loadoutName;
        runtimePreset.shipBody = body;
        runtimePreset.moveType = pm.allMoveTypes.FirstOrDefault(m => m != null && m.name == loadout.moveTypeName)
                                 ?? pm.allMoveTypes.FirstOrDefault(m => m != null && pm.IsUnlocked(m) && m.CanBeUsedBy(body.archetype));
        runtimePreset.tier1Perk = FindPerk(pm, loadout.tier1PerkName);
        runtimePreset.tier2Perk = FindPerk(pm, loadout.tier2PerkName);
        runtimePreset.tier3Perk = FindPerk(pm, loadout.tier3PerkName);
        runtimePreset.passives = loadout.passiveNames
            .Select(n => pm.allPassives.FirstOrDefault(p => p != null && p.name == n))
            .Where(p => p != null)
            .ToArray();
        runtimePreset.defaultMissile = pm.allMissiles.FirstOrDefault(m => m != null && m.name == loadout.equippedMissileName);

        ship.shipPreset = runtimePreset;

        // Missile: the loadout's retrofitted missile (falls back to prefab's)
        if (runtimePreset.defaultMissile != null)
            ship.equippedMissile = runtimePreset.defaultMissile;

        // Ship XP: the loadout's earned progression carries into the match
        var progression = pm.currentPlayerData != null
            ? pm.currentPlayerData.GetShipProgression(loadout)
            : null;
        if (progression != null)
        {
            ship.shipXP = progression.shipXP;
        }

        // PerkManager already ran Awake - reload slots from the new preset
        ship.GetComponent<PerkManager>()?.ReloadSlotsFromPreset();

        Debug.Log($"[LoadoutBridge] Applied loadout '{loadout.loadoutName}' to {ship.playerName} " +
                  $"(body: {body.bodyName}, missile: {loadout.equippedMissileName})");
        return true;
    }

    private static ActivePerkSO FindPerk(ProgressionManager pm, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return pm.allPerks.FirstOrDefault(p => p != null && p.name == name);
    }
}
