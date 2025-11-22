using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extended progression data for levels 1-100.
/// Contains all prebuild ships, ship bodies, passives, actives that unlock through leveling.
///
/// This is the SKELETON system with placeholder names.
/// Replace with actual ship names, stats, and assets later.
/// </summary>
public static class ExtendedProgressionData
{
    #region Prebuild Ship Unlocks (40 ships total - 10 per archetype)

    /// <summary>
    /// Complete prebuild ships that unlock through account XP.
    /// These are ready-to-use ships with pre-configured stats, passives, actives.
    /// Players can use these immediately without customization.
    /// </summary>
    public static readonly Dictionary<int, PrebuildShipUnlock> PREBUILD_SHIP_UNLOCKS = new Dictionary<int, PrebuildShipUnlock>
    {
        // ========== ALL-AROUND PREBUILD SHIPS (10 ships) ==========
        { 3, new PrebuildShipUnlock("nova_class", "Nova Class", ShipClass.AllAround, "Sleek and agile all-around fighter") },
        { 7, new PrebuildShipUnlock("phoenix_mk1", "Phoenix Mk-I", ShipClass.AllAround, "Enhanced maneuverability") },
        { 10, new PrebuildShipUnlock("eclipse_striker", "Eclipse Striker", ShipClass.AllAround, "Ranked milestone reward") },
        { 15, new PrebuildShipUnlock("horizon_scout", "Horizon Scout", ShipClass.AllAround, "Long-range reconnaissance") },
        { 20, new PrebuildShipUnlock("vanguard_elite", "Vanguard Elite", ShipClass.AllAround, "Elite all-purpose combat ship") },
        { 28, new PrebuildShipUnlock("centurion_mk2", "Centurion Mk-II", ShipClass.AllAround, "Advanced tactical fighter") },
        { 36, new PrebuildShipUnlock("paladin_class", "Paladin Class", ShipClass.AllAround, "Balanced superiority") },
        { 45, new PrebuildShipUnlock("omega_apex", "Omega Apex", ShipClass.AllAround, "Pinnacle of engineering") },
        { 60, new PrebuildShipUnlock("sovereign_prime", "Sovereign Prime", ShipClass.AllAround, "Supreme versatile warship") },
        { 80, new PrebuildShipUnlock("cosmic_harbinger", "Cosmic Harbinger", ShipClass.AllAround, "Ultimate all-around ship") },

        // ========== TANK PREBUILD SHIPS (10 ships) ==========
        { 6, new PrebuildShipUnlock("titan_defender", "Titan Defender", ShipClass.Tank, "Heavy armor plating") },
        { 12, new PrebuildShipUnlock("bastion_class", "Bastion Class", ShipClass.Tank, "Impenetrable shields") },
        { 18, new PrebuildShipUnlock("juggernaut", "Juggernaut", ShipClass.Tank, "Unbreakable defenses") },
        { 25, new PrebuildShipUnlock("fortress_prime", "Fortress Prime", ShipClass.Tank, "Mobile fortress") },
        { 32, new PrebuildShipUnlock("colossus_mk3", "Colossus Mk-III", ShipClass.Tank, "Massive defensive platform") },
        { 40, new PrebuildShipUnlock("bulwark_supreme", "Bulwark Supreme", ShipClass.Tank, "Supreme durability") },
        { 50, new PrebuildShipUnlock("aegis_titan", "Aegis Titan", ShipClass.Tank, "Legendary defensive ship") },
        { 65, new PrebuildShipUnlock("monolith_class", "Monolith Class", ShipClass.Tank, "Unstoppable juggernaut") },
        { 75, new PrebuildShipUnlock("citadel_eternal", "Citadel Eternal", ShipClass.Tank, "Eternal guardian") },
        { 90, new PrebuildShipUnlock("invincible_apex", "Invincible Apex", ShipClass.Tank, "Ultimate tank ship") },

        // ========== DAMAGE DEALER PREBUILD SHIPS (10 ships) ==========
        { 16, new PrebuildShipUnlock("viper_assault", "Viper Assault", ShipClass.DamageDealer, "Lightning-fast attacks") },
        { 19, new PrebuildShipUnlock("reaper_class", "Reaper Class", ShipClass.DamageDealer, "High-energy weapons") },
        { 23, new PrebuildShipUnlock("spectre_hunter", "Spectre Hunter", ShipClass.DamageDealer, "Precision strikes") },
        { 30, new PrebuildShipUnlock("crimson_tempest", "Crimson Tempest", ShipClass.DamageDealer, "Legendary veteran reward") },
        { 35, new PrebuildShipUnlock("devastator_mk4", "Devastator Mk-IV", ShipClass.DamageDealer, "Pure destruction") },
        { 42, new PrebuildShipUnlock("annihilator_prime", "Annihilator Prime", ShipClass.DamageDealer, "Maximum firepower") },
        { 52, new PrebuildShipUnlock("executioner_class", "Executioner Class", ShipClass.DamageDealer, "Instant elimination") },
        { 62, new PrebuildShipUnlock("apocalypse_mk7", "Apocalypse Mk-VII", ShipClass.DamageDealer, "Apocalyptic damage") },
        { 72, new PrebuildShipUnlock("obliterator_supreme", "Obliterator Supreme", ShipClass.DamageDealer, "Total annihilation") },
        { 85, new PrebuildShipUnlock("ragnarok_ultimate", "Ragnarok Ultimate", ShipClass.DamageDealer, "Ultimate DD ship") },

        // ========== CONTROLLER PREBUILD SHIPS (10 ships) ==========
        { 26, new PrebuildShipUnlock("nexus_command", "Nexus Command", ShipClass.Controller, "Tactical control") },
        { 29, new PrebuildShipUnlock("oracle_class", "Oracle Class", ShipClass.Controller, "Advanced targeting") },
        { 34, new PrebuildShipUnlock("phantom_ops", "Phantom Ops", ShipClass.Controller, "Stealth operations") },
        { 38, new PrebuildShipUnlock("tactician_mk2", "Tactician Mk-II", ShipClass.Controller, "Strategic mastery") },
        { 44, new PrebuildShipUnlock("enigma_class", "Enigma Class", ShipClass.Controller, "Unpredictable tactics") },
        { 48, new PrebuildShipUnlock("infinity_command", "Infinity Command", ShipClass.Controller, "Ultimate versatility") },
        { 56, new PrebuildShipUnlock("maestro_elite", "Maestro Elite", ShipClass.Controller, "Orchestrates battlefield") },
        { 68, new PrebuildShipUnlock("overseer_prime", "Overseer Prime", ShipClass.Controller, "Total battlefield control") },
        { 78, new PrebuildShipUnlock("puppetmaster_class", "Puppetmaster Class", ShipClass.Controller, "Manipulates the field") },
        { 95, new PrebuildShipUnlock("celestial_monarch", "Celestial Monarch", ShipClass.Controller, "Ultimate controller") }
    };

    #endregion

    #region Ship Body Unlocks (16 bodies - 4 per archetype for custom building)

    /// <summary>
    /// Ship bodies/chassis for custom ship building.
    /// These are the 3D models with base stats that players can customize with passives/actives.
    /// </summary>
    public static readonly Dictionary<int, ShipBodyUnlock> SHIP_BODY_UNLOCKS = new Dictionary<int, ShipBodyUnlock>
    {
        // ========== ALL-AROUND BODIES (4 bodies) ==========
        { 21, new ShipBodyUnlock("body_allaround_standard", "Standard Frame", ShipClass.AllAround, "Basic all-around chassis") },
        { 37, new ShipBodyUnlock("body_allaround_tactical", "Tactical Frame", ShipClass.AllAround, "Advanced balanced chassis") },
        { 53, new ShipBodyUnlock("body_allaround_elite", "Elite Frame", ShipClass.AllAround, "Superior all-around chassis") },
        { 71, new ShipBodyUnlock("body_allaround_apex", "Apex Frame", ShipClass.AllAround, "Ultimate versatile chassis") },

        // ========== TANK BODIES (4 bodies) ==========
        { 21, new ShipBodyUnlock("body_tank_reinforced", "Reinforced Hull", ShipClass.Tank, "Heavy armor chassis") },
        { 39, new ShipBodyUnlock("body_tank_fortress", "Fortress Hull", ShipClass.Tank, "Fortified defensive chassis") },
        { 57, new ShipBodyUnlock("body_tank_colossus", "Colossus Hull", ShipClass.Tank, "Massive tank chassis") },
        { 73, new ShipBodyUnlock("body_tank_invincible", "Invincible Hull", ShipClass.Tank, "Ultimate tank chassis") },

        // ========== DAMAGE DEALER BODIES (4 bodies) ==========
        { 21, new ShipBodyUnlock("body_dd_striker", "Striker Chassis", ShipClass.DamageDealer, "Agile assault frame") },
        { 41, new ShipBodyUnlock("body_dd_reaper", "Reaper Chassis", ShipClass.DamageDealer, "High-damage glass cannon") },
        { 59, new ShipBodyUnlock("body_dd_devastator", "Devastator Chassis", ShipClass.DamageDealer, "Maximum firepower frame") },
        { 77, new ShipBodyUnlock("body_dd_annihilator", "Annihilator Chassis", ShipClass.DamageDealer, "Ultimate DD chassis") },

        // ========== CONTROLLER BODIES (4 bodies) ==========
        { 27, new ShipBodyUnlock("body_ctrl_tactician", "Tactician Frame", ShipClass.Controller, "Tactical control chassis") },
        { 43, new ShipBodyUnlock("body_ctrl_phantom", "Phantom Frame", ShipClass.Controller, "Stealth specialist chassis") },
        { 61, new ShipBodyUnlock("body_ctrl_maestro", "Maestro Frame", ShipClass.Controller, "Advanced control chassis") },
        { 79, new ShipBodyUnlock("body_ctrl_sovereign", "Sovereign Frame", ShipClass.Controller, "Ultimate controller chassis") }
    };

    #endregion

    #region Passive Ability Unlocks (30 passives)

    /// <summary>
    /// Passive abilities for custom ship building.
    /// Passives have archetype restrictions - they can only be equipped on compatible ship bodies.
    /// </summary>
    public static readonly Dictionary<int, PassiveUnlock> PASSIVE_UNLOCKS = new Dictionary<int, PassiveUnlock>
    {
        // Basic Passives (Early game)
        { 5, new PassiveUnlock("passive_armor_boost_1", "Armor Boost I", ShipClass.Tank, "Increases armor by 10%") },
        { 8, new PassiveUnlock("passive_speed_boost_1", "Speed Boost I", ShipClass.DamageDealer, "Increases speed by 10%") },
        { 11, new PassiveUnlock("passive_damage_boost_1", "Damage Boost I", ShipClass.DamageDealer, "Increases damage by 10%") },
        { 14, new PassiveUnlock("passive_shield_regen", "Shield Regeneration", ShipClass.AllAround, "Slowly regenerates shields") },
        { 17, new PassiveUnlock("passive_energy_efficient", "Energy Efficiency", ShipClass.Controller, "Reduces energy consumption") },

        // Intermediate Passives (Mid game)
        { 22, new PassiveUnlock("passive_armor_boost_2", "Armor Boost II", ShipClass.Tank, "Increases armor by 20%") },
        { 24, new PassiveUnlock("passive_speed_boost_2", "Speed Boost II", ShipClass.DamageDealer, "Increases speed by 20%") },
        { 26, new PassiveUnlock("passive_damage_boost_2", "Damage Boost II", ShipClass.DamageDealer, "Increases damage by 20%") },
        { 29, new PassiveUnlock("passive_critical_strike", "Critical Strike", ShipClass.DamageDealer, "15% chance for critical hits") },
        { 31, new PassiveUnlock("passive_evasion", "Evasion", ShipClass.AllAround, "10% chance to evade attacks") },
        { 33, new PassiveUnlock("passive_lifesteal", "Lifesteal", ShipClass.DamageDealer, "Heal 5% of damage dealt") },
        { 35, new PassiveUnlock("passive_fortified", "Fortified", ShipClass.Tank, "Reduces incoming damage by 10%") },
        { 37, new PassiveUnlock("passive_overcharge", "Overcharge", ShipClass.AllAround, "Boosts all systems by 5%") },

        // Advanced Passives (Late game)
        { 46, new PassiveUnlock("passive_armor_boost_3", "Armor Boost III", ShipClass.Tank, "Increases armor by 30%") },
        { 49, new PassiveUnlock("passive_speed_boost_3", "Speed Boost III", ShipClass.DamageDealer, "Increases speed by 30%") },
        { 51, new PassiveUnlock("passive_damage_boost_3", "Damage Boost III", ShipClass.DamageDealer, "Increases damage by 30%") },
        { 54, new PassiveUnlock("passive_berserker", "Berserker", ShipClass.DamageDealer, "Damage increases as health decreases") },
        { 58, new PassiveUnlock("passive_juggernaut", "Juggernaut", ShipClass.Tank, "Immune to crowd control") },
        { 63, new PassiveUnlock("passive_phantom", "Phantom", ShipClass.Controller, "Chance to become untargetable") },
        { 66, new PassiveUnlock("passive_adaptive_armor", "Adaptive Armor", ShipClass.Tank, "Armor increases over time in combat") },

        // Elite Passives (Endgame)
        { 70, new PassiveUnlock("passive_regeneration_supreme", "Supreme Regeneration", ShipClass.Tank, "Fast health and shield regen") },
        { 74, new PassiveUnlock("passive_overdrive", "Overdrive", ShipClass.AllAround, "All stats increased by 15%") },
        { 76, new PassiveUnlock("passive_titan_soul", "Titan Soul", ShipClass.Tank, "Massive stat boost when low health") },
        { 81, new PassiveUnlock("passive_phoenix_rebirth", "Phoenix Rebirth", ShipClass.AllAround, "Revive once per match at 50% HP") },
        { 86, new PassiveUnlock("passive_celestial_blessing", "Celestial Blessing", ShipClass.AllAround, "Random powerful buffs") },
        { 91, new PassiveUnlock("passive_omega_protocol", "Omega Protocol", ShipClass.AllAround, "Ultimate passive - all stats +20%") },

        // Unique/Special Passives
        { 47, new PassiveUnlock("passive_gravity_master", "Gravity Master", ShipClass.Controller, "Improved planet interactions") },
        { 64, new PassiveUnlock("passive_momentum", "Momentum", ShipClass.DamageDealer, "Damage increases with speed") },
        { 82, new PassiveUnlock("passive_last_stand", "Last Stand", ShipClass.Tank, "Become extremely powerful at <25% HP") },
        { 96, new PassiveUnlock("passive_god_mode", "Ascension", ShipClass.AllAround, "Legendary passive - near-invincible") }
    };

    #endregion

    #region Active Ability Unlocks (20 actives)

    /// <summary>
    /// Active abilities (Perks) for custom ship building.
    /// Organized into 3 tiers - players must select one ability from EACH tier when building.
    /// Tier 1: Basic abilities (low cooldown, fundamental effects)
    /// Tier 2: Advanced abilities (moderate cooldown, tactical effects)
    /// Tier 3: Ultimate abilities (high cooldown, game-changing effects)
    /// </summary>
    public static readonly Dictionary<int, ActiveUnlock> ACTIVE_UNLOCKS = new Dictionary<int, ActiveUnlock>
    {
        // TIER 1 ACTIVES (Basic - 8 total)
        { 9, new ActiveUnlock("active_boost", "Afterburner", 1, 10f, "Temporary speed boost") },
        { 13, new ActiveUnlock("active_shield", "Energy Shield", 1, 15f, "Temporary invulnerability") },
        { 16, new ActiveUnlock("active_repair", "Emergency Repair", 1, 30f, "Restore 30% health") },
        { 19, new ActiveUnlock("active_emp_pulse", "EMP Pulse", 1, 20f, "Disrupt enemy targeting") },
        { 27, new ActiveUnlock("active_cloak", "Stealth Cloak", 1, 25f, "Become invisible briefly") },
        { 30, new ActiveUnlock("active_overload", "Weapon Overload", 1, 15f, "Triple damage for short time") },
        { 32, new ActiveUnlock("active_blink", "Phase Blink", 1, 12f, "Teleport short distance") },
        { 50, new ActiveUnlock("active_tactical_swap", "Tactical Swap", 1, 35f, "Swap positions with enemy") },

        // TIER 2 ACTIVES (Advanced - 7 total)
        { 36, new ActiveUnlock("active_gravity_well", "Gravity Well", 2, 30f, "Pull enemies toward you") },
        { 40, new ActiveUnlock("active_time_dilation", "Time Dilation", 2, 45f, "Slow time in area") },
        { 48, new ActiveUnlock("active_nova_bomb", "Nova Bomb", 2, 40f, "Massive area explosion") },
        { 55, new ActiveUnlock("active_fortress_mode", "Fortress Mode", 2, 60f, "Immobile but +100% armor") },
        { 60, new ActiveUnlock("active_berserker_rage", "Berserker Rage", 2, 50f, "+50% damage, -50% defense") },
        { 67, new ActiveUnlock("active_mirror_clone", "Mirror Clone", 2, 45f, "Create decoy copy of ship") },
        { 69, new ActiveUnlock("active_singularity", "Singularity", 2, 90f, "Create black hole") },

        // TIER 3 ACTIVES (Ultimate - 5 total)
        { 75, new ActiveUnlock("active_ultima", "Ultima", 3, 120f, "Screen-clearing ultimate attack") },
        { 83, new ActiveUnlock("active_phoenix_burst", "Phoenix Burst", 3, 180f, "Death-defying resurrection") },
        { 88, new ActiveUnlock("active_ragnarok", "Ragnarok", 3, 150f, "Apocalyptic destruction") },
        { 93, new ActiveUnlock("active_ascension", "Ascension", 3, 200f, "Become god-like temporarily") },
        { 98, new ActiveUnlock("active_omega_strike", "Omega Strike", 3, 300f, "Ultimate one-shot kill ability") }
    };

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get prebuild ship unlock for specific level.
    /// </summary>
    public static PrebuildShipUnlock GetPrebuildShip(int level)
    {
        return PREBUILD_SHIP_UNLOCKS.ContainsKey(level) ? PREBUILD_SHIP_UNLOCKS[level] : null;
    }

    /// <summary>
    /// Get ship body unlock for specific level.
    /// </summary>
    public static ShipBodyUnlock GetShipBody(int level)
    {
        return SHIP_BODY_UNLOCKS.ContainsKey(level) ? SHIP_BODY_UNLOCKS[level] : null;
    }

    /// <summary>
    /// Get passive unlock for specific level.
    /// </summary>
    public static PassiveUnlock GetPassive(int level)
    {
        return PASSIVE_UNLOCKS.ContainsKey(level) ? PASSIVE_UNLOCKS[level] : null;
    }

    /// <summary>
    /// Get active unlock for specific level.
    /// </summary>
    public static ActiveUnlock GetActive(int level)
    {
        return ACTIVE_UNLOCKS.ContainsKey(level) ? ACTIVE_UNLOCKS[level] : null;
    }

    /// <summary>
    /// Get all ship bodies (for custom ship builder UI).
    /// </summary>
    public static List<ShipBodyUnlock> GetAllShipBodies()
    {
        return SHIP_BODY_UNLOCKS.Values.ToList();
    }

    /// <summary>
    /// Get all passives (for custom ship builder UI).
    /// </summary>
    public static List<PassiveUnlock> GetAllPassives()
    {
        return PASSIVE_UNLOCKS.Values.ToList();
    }

    /// <summary>
    /// Get all actives (for custom ship builder UI).
    /// </summary>
    public static List<ActiveUnlock> GetAllActives()
    {
        return ACTIVE_UNLOCKS.Values.ToList();
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Prebuild ship unlock data.
/// </summary>
[Serializable]
public class PrebuildShipUnlock
{
    public string shipId;
    public string displayName;
    public ShipClass shipClass;
    public string description;

    public PrebuildShipUnlock(string id, string name, ShipClass shipClass, string desc)
    {
        this.shipId = id;
        this.displayName = name;
        this.shipClass = shipClass;
        this.description = desc;
    }
}

/// <summary>
/// Ship body/chassis unlock for custom building.
/// </summary>
[Serializable]
public class ShipBodyUnlock
{
    public string bodyId;
    public string displayName;
    public ShipClass shipClass;
    public string description;

    public ShipBodyUnlock(string id, string name, ShipClass shipClass, string desc)
    {
        this.bodyId = id;
        this.displayName = name;
        this.shipClass = shipClass;
        this.description = desc;
    }
}

/// <summary>
/// Passive ability unlock.
/// Passives have archetype restrictions - they can only be used on compatible ship bodies.
/// </summary>
[Serializable]
public class PassiveUnlock
{
    public string passiveId;
    public string displayName;
    public ShipClass compatibleArchetype;  // Which ship archetype can use this passive
    public string description;

    public PassiveUnlock(string id, string name, ShipClass archetype, string desc)
    {
        this.passiveId = id;
        this.displayName = name;
        this.compatibleArchetype = archetype;
        this.description = desc;
    }
}

/// <summary>
/// Active ability unlock (also called "Perks").
/// Actives are organized into 3 tiers - players select one from each tier when building custom ships.
/// Tier 1: Basic abilities (low cooldown, basic effects)
/// Tier 2: Advanced abilities (moderate cooldown, stronger effects)
/// Tier 3: Ultimate abilities (high cooldown, game-changing effects)
/// </summary>
[Serializable]
public class ActiveUnlock
{
    public string activeId;
    public string displayName;
    public int tier;                // 1, 2, or 3 (player must select one from each tier)
    public float cooldown;          // Cooldown in seconds
    public string description;

    public ActiveUnlock(string id, string name, int tier, float cd, string desc)
    {
        this.activeId = id;
        this.displayName = name;
        this.tier = tier;
        this.cooldown = cd;
        this.description = desc;
    }
}

#endregion
