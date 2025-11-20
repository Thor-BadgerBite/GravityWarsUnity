using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityWars.Online
{
    /// <summary>
    /// Missile retrofit system - manages missile unlocks and loadout selection.
/// Missiles are SEPARATE from ships and can be changed before each match.
/// Players must unlock missiles through progression and can equip compatible missiles.
///
/// IMPORTANT: Missiles are NOT part of ship building!
/// They are selected separately before match starts.
/// </summary>
public static class MissileRetrofitSystem
{
    #region Missile Unlock Schedule

    /// <summary>
    /// All missiles that can be unlocked through account progression.
    /// Players can retrofit these on compatible ships before matches.
    /// </summary>
    private static readonly Dictionary<int, MissileUnlockData> MISSILE_UNLOCKS = new Dictionary<int, MissileUnlockData>
    {
        // STANDARD MISSILES (Available to all ship types)
        { 1, new MissileUnlockData("standard_mk1", "Standard Mk-I", MissileType.Standard, ShipClass.AllAround, "Basic missile with balanced stats") },
        { 4, new MissileUnlockData("standard_mk2", "Standard Mk-II", MissileType.Standard, ShipClass.AllAround, "Improved standard missile") },
        { 11, new MissileUnlockData("standard_mk3", "Standard Mk-III", MissileType.Standard, ShipClass.AllAround, "Advanced standard missile") },

        // LIGHT MISSILES (Fast, low damage, good for DD and All-Around)
        { 8, new MissileUnlockData("light_swarm", "Swarm Light", MissileType.Light, ShipClass.DamageDealer, "Fast-moving light missile") },
        { 14, new MissileUnlockData("light_vortex", "Vortex Light", MissileType.Light, ShipClass.DamageDealer, "High-speed precision missile") },
        { 22, new MissileUnlockData("light_phantom", "Phantom Light", MissileType.Light, ShipClass.Controller, "Stealth light missile") },

        // HEAVY MISSILES (Slow, high damage, good for Tank and DD)
        { 9, new MissileUnlockData("heavy_titan", "Titan Heavy", MissileType.Heavy, ShipClass.Tank, "Devastating heavy payload") },
        { 17, new MissileUnlockData("heavy_crusher", "Crusher Heavy", MissileType.Heavy, ShipClass.Tank, "Armor-piercing heavy missile") },
        { 27, new MissileUnlockData("heavy_apocalypse", "Apocalypse Heavy", MissileType.Heavy, ShipClass.DamageDealer, "Maximum destruction") },

        // TACTICAL MISSILES (Special effects, good for Controller)
        { 13, new MissileUnlockData("tactical_emp", "EMP Tactical", MissileType.Tactical, ShipClass.Controller, "Disables enemy systems") },
        { 20, new MissileUnlockData("tactical_gravity", "Gravity Tactical", MissileType.Tactical, ShipClass.Controller, "Creates gravity well") },
        { 31, new MissileUnlockData("tactical_plasma", "Plasma Tactical", MissileType.Tactical, ShipClass.Controller, "Sustained damage over time") },

        // PIERCING MISSILES (Penetrate armor, good for DD)
        { 24, new MissileUnlockData("piercing_lance", "Lance Piercing", MissileType.Piercing, ShipClass.DamageDealer, "Ignores partial armor") },
        { 33, new MissileUnlockData("piercing_railgun", "Railgun Piercing", MissileType.Piercing, ShipClass.DamageDealer, "Ultra-high penetration") },

        // CLUSTER MISSILES (Multiple warheads, good for All-Around)
        { 28, new MissileUnlockData("cluster_nova", "Nova Cluster", MissileType.Cluster, ShipClass.AllAround, "Splits into multiple warheads") },
        { 38, new MissileUnlockData("cluster_cascade", "Cascade Cluster", MissileType.Cluster, ShipClass.AllAround, "Chain reaction explosions") },

        // ULTIMATE MISSILES (Late-game, very powerful)
        { 45, new MissileUnlockData("ultimate_singularity", "Singularity", MissileType.Ultimate, ShipClass.AllAround, "Creates mini black hole") },
        { 55, new MissileUnlockData("ultimate_omega", "Omega Strike", MissileType.Ultimate, ShipClass.AllAround, "Ultimate devastation") }
    };

    #endregion

    #region Missile Compatibility

    /// <summary>
    /// Check if a missile type is compatible with a ship class.
    /// Some missiles are class-restricted, others are universal.
    /// </summary>
    public static bool IsMissileCompatible(string missileId, ShipClass shipClass)
    {
        var missile = GetMissileData(missileId);
        if (missile == null) return false;

        // Standard and Ultimate missiles work on all ship types
        if (missile.missileType == MissileType.Standard || missile.missileType == MissileType.Ultimate)
            return true;

        // Cluster missiles work on all ships
        if (missile.missileType == MissileType.Cluster)
            return true;

        // Check if ship class matches preferred class or is All-Around
        if (shipClass == ShipClass.AllAround)
            return true; // All-Around can use any missile

        return missile.preferredClass == shipClass;
    }

    /// <summary>
    /// Get list of compatible missiles for a ship class (that player has unlocked).
    /// </summary>
    public static List<MissileUnlockData> GetCompatibleMissiles(ShipClass shipClass, int playerLevel)
    {
        var compatible = new List<MissileUnlockData>();

        foreach (var kvp in MISSILE_UNLOCKS)
        {
            // Check if unlocked
            if (kvp.Key > playerLevel) continue;

            // Check if compatible
            if (IsMissileCompatible(kvp.Value.missileId, shipClass))
            {
                compatible.Add(kvp.Value);
            }
        }

        return compatible;
    }

    #endregion

    #region Missile Data Access

    /// <summary>
    /// Get missile unlock data for a specific level.
    /// </summary>
    public static MissileUnlockData GetMissileUnlock(int level)
    {
        return MISSILE_UNLOCKS.ContainsKey(level) ? MISSILE_UNLOCKS[level] : null;
    }

    /// <summary>
    /// Get missile data by ID.
    /// </summary>
    public static MissileUnlockData GetMissileData(string missileId)
    {
        foreach (var missile in MISSILE_UNLOCKS.Values)
        {
            if (missile.missileId == missileId)
                return missile;
        }
        return null;
    }

    /// <summary>
    /// Get all missiles unlocked up to a specific level.
    /// </summary>
    public static List<MissileUnlockData> GetAllUnlockedMissiles(int level)
    {
        var unlocked = new List<MissileUnlockData>();

        foreach (var kvp in MISSILE_UNLOCKS)
        {
            if (kvp.Key <= level)
            {
                unlocked.Add(kvp.Value);
            }
        }

        return unlocked;
    }

    /// <summary>
    /// Check if player has unlocked a specific missile.
    /// </summary>
    public static bool IsMissileUnlocked(string missileId, int playerLevel)
    {
        foreach (var kvp in MISSILE_UNLOCKS)
        {
            if (kvp.Value.missileId == missileId)
            {
                return kvp.Key <= playerLevel;
            }
        }
        return false;
    }

    #endregion

    #region Missile Types

    /// <summary>
    /// Get display name for missile type.
    /// </summary>
    public static string GetMissileTypeName(MissileType type)
    {
        switch (type)
        {
            case MissileType.Standard: return "Standard";
            case MissileType.Light: return "Light";
            case MissileType.Heavy: return "Heavy";
            case MissileType.Tactical: return "Tactical";
            case MissileType.Piercing: return "Piercing";
            case MissileType.Cluster: return "Cluster";
            case MissileType.Ultimate: return "Ultimate";
            default: return "Unknown";
        }
    }

    /// <summary>
    /// Get description for missile type.
    /// </summary>
    public static string GetMissileTypeDescription(MissileType type)
    {
        switch (type)
        {
            case MissileType.Standard:
                return "Balanced damage and speed. Works with all ship types.";
            case MissileType.Light:
                return "Fast and agile. Lower damage but high accuracy. Best for Damage Dealers.";
            case MissileType.Heavy:
                return "Slow but devastating. High damage, low speed. Best for Tanks.";
            case MissileType.Tactical:
                return "Special effects and utility. Best for Controllers.";
            case MissileType.Piercing:
                return "Armor penetration. Ignores defense. Best for Damage Dealers.";
            case MissileType.Cluster:
                return "Splits into multiple warheads. Area damage. Works with all ships.";
            case MissileType.Ultimate:
                return "Extremely powerful. Late-game missiles. Works with all ships.";
            default:
                return "Unknown missile type.";
        }
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Missile types with different characteristics.
/// </summary>
public enum MissileType
{
    Standard,   // Balanced, works on all ships
    Light,      // Fast, low damage
    Heavy,      // Slow, high damage
    Tactical,   // Special effects (EMP, gravity, etc.)
    Piercing,   // Armor penetration
    Cluster,    // Multiple warheads
    Ultimate    // Late-game, very powerful
}

/// <summary>
/// Data for a missile unlock.
/// </summary>
[Serializable]
public class MissileUnlockData
{
    public string missileId;            // e.g., "heavy_titan"
    public string displayName;          // e.g., "Titan Heavy"
    public MissileType missileType;     // Type of missile
    public ShipClass preferredClass;    // Which ship class it's designed for
    public string description;          // Flavor text

    public MissileUnlockData(string id, string name, MissileType type, ShipClass preferredClass, string desc)
    {
        this.missileId = id;
        this.username = name;
        this.missileType = type;
        this.preferredClass = preferredClass;
        this.description = desc;
    }
}

#endregion
}
