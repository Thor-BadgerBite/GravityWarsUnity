using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor utility that generates the full playable content set:
///  - 10 ship bodies (all 4 archetypes)
///  - 17 passive abilities
///  - 18 active perks (6 families x 3 tiers)
///  - 9 missile presets (Light / Medium / Heavy, Mk I-III)
///  - 3 extra move types
///  - 10 prebuilt ships (ShipPresetSO)
///  - Season 1 battle pass (30 tiers, free + premium tracks)
///
/// Usage: Tools → Gravity Wars → Generate Game Content
///
/// Assets are written to Assets/Resources/GeneratedContent/ so
/// ProgressionManager.PopulateContentDatabases() picks them up automatically.
/// Asset file names intentionally match the progression unlock ids
/// (ExtendedProgressionData / MissileRetrofitSystem) so unlock lists resolve
/// directly to these assets.
///
/// Re-running the generator updates existing assets in place (idempotent).
/// </summary>
public class GameContentGenerator : EditorWindow
{
    private const string ROOT = "Assets/Resources/GeneratedContent/";
    private const string BODIES_PATH = ROOT + "ShipBodies/";
    private const string PASSIVES_PATH = ROOT + "Passives/";
    private const string PERKS_PATH = ROOT + "Perks/";
    private const string MISSILES_PATH = ROOT + "Missiles/";
    private const string MOVETYPES_PATH = ROOT + "MoveTypes/";
    private const string SHIPS_PATH = ROOT + "Ships/";
    private const string BATTLEPASS_PATH = ROOT + "BattlePass/";

    // Existing hand-made assets we reuse
    private const string STANDARD_MOVE_PATH = "Assets/Ship System/Standard Move.asset";
    private const string LEVELING_ALLAROUND = "Assets/Ship System/AllAroundLevelingMain.asset";
    private const string LEVELING_TANK = "Assets/Ship System/TankLevelingMain.asset";
    private const string LEVELING_DD = "Assets/Ship System/DDLevelingMain.asset";
    private const string LEVELING_CTRL = "Assets/Ship System/ControllerLevelingMain.asset";

    private int _created;
    private int _updated;

    [MenuItem("Tools/Gravity Wars/Generate Game Content")]
    public static void ShowWindow()
    {
        GetWindow<GameContentGenerator>("Game Content Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Content Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("Generates ship bodies, passives, perks, missiles,");
        GUILayout.Label("move types, prebuilt ships and the Season 1 battle pass.");
        GUILayout.Label(ROOT, EditorStyles.miniLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate ALL Content", GUILayout.Height(40)))
        {
            GenerateAll();
        }

        GUILayout.Space(20);
        GUILayout.Label("Individual generators:", EditorStyles.boldLabel);
        if (GUILayout.Button("Ship Bodies")) { Begin(); GenerateShipBodies(); Finish("ship bodies"); }
        if (GUILayout.Button("Passives")) { Begin(); GeneratePassives(); Finish("passives"); }
        if (GUILayout.Button("Active Perks")) { Begin(); GeneratePerks(); Finish("perks"); }
        if (GUILayout.Button("Missiles")) { Begin(); GenerateMissiles(); Finish("missiles"); }
        if (GUILayout.Button("Move Types")) { Begin(); GenerateMoveTypes(); Finish("move types"); }
        if (GUILayout.Button("Prebuilt Ships")) { Begin(); GeneratePrebuiltShips(); Finish("prebuilt ships"); }
        if (GUILayout.Button("Battle Pass (Season 1)")) { Begin(); GenerateBattlePass(); Finish("battle pass"); }
    }

    private void GenerateAll()
    {
        Begin();
        GenerateShipBodies();
        GeneratePassives();
        GeneratePerks();
        GenerateMissiles();
        GenerateMoveTypes();
        GeneratePrebuiltShips();   // References bodies/passives/perks/missiles
        GenerateBattlePass();      // References everything
        Finish("content items");
    }

    #region Helpers

    private void Begin()
    {
        _created = 0;
        _updated = 0;
        EnsureFolder(BODIES_PATH);
        EnsureFolder(PASSIVES_PATH);
        EnsureFolder(PERKS_PATH);
        EnsureFolder(MISSILES_PATH);
        EnsureFolder(MOVETYPES_PATH);
        EnsureFolder(SHIPS_PATH);
        EnsureFolder(BATTLEPASS_PATH);
    }

    private void Finish(string what)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[GameContentGenerator] Done - {_created} created, {_updated} updated ({what})");
        EditorUtility.DisplayDialog("Content Generator",
            $"Generation complete!\n\nCreated: {_created}\nUpdated: {_updated}", "OK");
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
        {
            System.IO.Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Loads the asset if it exists, otherwise creates it. Marks dirty either way.
    /// </summary>
    private T GetOrCreate<T>(string folder, string assetName) where T : ScriptableObject
    {
        string path = folder + assetName + ".asset";
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            _created++;
        }
        else
        {
            _updated++;
        }
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private T Load<T>(string folder, string assetName) where T : ScriptableObject
    {
        return AssetDatabase.LoadAssetAtPath<T>(folder + assetName + ".asset");
    }

    private static void SetArchetypeFlags(PassiveAbilitySO p, ShipArchetype archetype)
    {
        // The target archetype plus AllAround bodies can use it
        p.allowTank = archetype == ShipArchetype.Tank || archetype == ShipArchetype.AllAround;
        p.allowDamageDealer = archetype == ShipArchetype.DamageDealer || archetype == ShipArchetype.AllAround;
        p.allowController = archetype == ShipArchetype.Controller || archetype == ShipArchetype.AllAround;
        p.allowAllAround = true;

        if (archetype == ShipArchetype.AllAround)
        {
            p.allowTank = p.allowDamageDealer = p.allowController = true;
        }
    }

    #endregion

    #region Ship Bodies

    private void GenerateShipBodies()
    {
        CreateBody("body_allaround_standard", "Standard Frame", ShipArchetype.AllAround,
            10000f, 100f, 1.0f, 3, 0, true, true, true,
            "Basic all-around chassis. Balanced in every way.");
        CreateBody("body_allaround_tactical", "Tactical Frame", ShipArchetype.AllAround,
            11000f, 110f, 1.05f, 3, 37, true, true, true,
            "Advanced balanced chassis with improved plating.");
        CreateBody("body_allaround_elite", "Elite Frame", ShipArchetype.AllAround,
            12000f, 120f, 1.1f, 3, 53, true, true, true,
            "Superior all-around chassis for veteran pilots.");

        CreateBody("body_tank_reinforced", "Reinforced Hull", ShipArchetype.Tank,
            14000f, 180f, 0.85f, 2, 25, false, true, true,
            "Heavy armor chassis. Cannot mount light missiles.");
        CreateBody("body_tank_fortress", "Fortress Hull", ShipArchetype.Tank,
            16000f, 220f, 0.8f, 2, 39, false, true, true,
            "Fortified defensive chassis built to outlast anything.");
        CreateBody("body_tank_colossus", "Colossus Hull", ShipArchetype.Tank,
            18000f, 260f, 0.75f, 2, 57, false, false, true,
            "Massive tank chassis. Heavy missiles only.");

        CreateBody("body_dd_striker", "Striker Chassis", ShipArchetype.DamageDealer,
            8000f, 60f, 1.3f, 3, 31, true, true, false,
            "Agile assault frame. Trades armor for firepower.");
        CreateBody("body_dd_reaper", "Reaper Chassis", ShipArchetype.DamageDealer,
            7500f, 50f, 1.45f, 3, 41, true, true, false,
            "High-damage glass cannon frame.");

        CreateBody("body_ctrl_tactician", "Tactician Frame", ShipArchetype.Controller,
            9500f, 90f, 1.0f, 4, 27, true, true, false,
            "Tactical control chassis with an extra action point.");
        CreateBody("body_ctrl_phantom", "Phantom Frame", ShipArchetype.Controller,
            9000f, 80f, 1.05f, 4, 43, true, true, false,
            "Stealth specialist chassis for precise play.");
    }

    private void CreateBody(string id, string displayName, ShipArchetype archetype,
        float health, float armor, float dmgMult, int actionPoints, int reqLevel,
        bool light, bool medium, bool heavy, string description)
    {
        var body = GetOrCreate<ShipBodySO>(BODIES_PATH, id);
        body.bodyName = displayName;
        body.archetype = archetype;
        body.baseHealth = health;
        body.baseArmor = armor;
        body.baseDamageMultiplier = dmgMult;
        body.actionPointsPerTurn = actionPoints;
        body.requiredAccountLevel = reqLevel;
        body.canUseLightMissiles = light;
        body.canUseMediumMissiles = medium;
        body.canUseHeavyMissiles = heavy;
        body.description = description;
    }

    #endregion

    #region Passives

    private void GeneratePassives()
    {
        CreatePassive("passive_shield_regen", "Shield Regeneration", PassiveType.EnhancedRegeneration,
            ShipArchetype.AllAround, 8, 1.5f, "Slowly regenerates hull integrity every turn.");
        CreatePassive("passive_armor_boost_1", "Armor Boost I", PassiveType.DamageResistance,
            ShipArchetype.Tank, 5, 0.10f, "Reduces incoming damage by 10%.");
        CreatePassive("passive_armor_boost_2", "Armor Boost II", PassiveType.DamageResistance,
            ShipArchetype.Tank, 22, 0.20f, "Reduces incoming damage by 20%.");
        CreatePassive("passive_fortified", "Fortified", PassiveType.DamageResistance,
            ShipArchetype.Tank, 35, 0.15f, "Reinforced plating reduces incoming damage by 15%.");
        CreatePassive("passive_damage_boost_1", "Damage Boost", PassiveType.DamageBoost,
            ShipArchetype.DamageDealer, 11, 0f, "Increases outgoing damage.");
        CreatePassive("passive_critical_strike", "Critical Strike", PassiveType.CriticalEnhancement,
            ShipArchetype.DamageDealer, 29, 0f, "Improved chance to land devastating critical hits.");
        CreatePassive("passive_critical_immunity", "Critical Immunity", PassiveType.CriticalImmunity,
            ShipArchetype.Tank, 46, 0f, "Immune to critical hits.");
        CreatePassive("passive_lifesteal", "Lifesteal", PassiveType.Lifesteal,
            ShipArchetype.DamageDealer, 33, 0.2f, "Heal for 20% of the damage you deal.");
        CreatePassive("passive_sniper_mode", "Sniper Mode", PassiveType.SniperMode,
            ShipArchetype.Controller, 17, 0f, "Enhanced aiming for long-range precision.");
        CreatePassive("passive_unmovable", "Unmovable", PassiveType.Unmovable,
            ShipArchetype.Tank, 58, 0f, "Immune to knockback and push effects.");
        CreatePassive("passive_last_stand", "Last Stand", PassiveType.LastChance,
            ShipArchetype.Tank, 82, 0f, "Survive a killing blow once per round with 1 HP.");
        CreatePassive("passive_adaptive_armor", "Adaptive Armor", PassiveType.AdaptiveArmor,
            ShipArchetype.Tank, 66, 0f, "Armor adapts and strengthens during combat.");
        CreatePassive("passive_adaptive_damage", "Adaptive Damage", PassiveType.AdaptiveDamage,
            ShipArchetype.DamageDealer, 54, 0f, "Weapons adapt and strengthen during combat.");
        CreatePassive("passive_precision_engineering", "Precision Engineering", PassiveType.PrecisionEngineering,
            ShipArchetype.Controller, 47, 0f, "Improved projectile consistency and control.");
        CreatePassive("passive_collision_avoidance", "Collision Avoidance", PassiveType.CollisionAvoidance,
            ShipArchetype.Controller, 63, 0f, "Automated thrusters help avoid planetary collisions.");
        CreatePassive("passive_momentum", "Momentum", PassiveType.IncreaseDamageOnHighSpeed,
            ShipArchetype.DamageDealer, 64, 0.25f, "High-speed missiles deal +25% damage.");
        CreatePassive("passive_bulwark", "Bulwark", PassiveType.ReduceDamageFromHighSpeed,
            ShipArchetype.Tank, 70, 0.25f, "Take -25% damage from high-speed missiles.");
    }

    private void CreatePassive(string id, string displayName, PassiveType type,
        ShipArchetype archetype, int reqLevel, float value1, string description)
    {
        var p = GetOrCreate<PassiveAbilitySO>(PASSIVES_PATH, id);
        p.passiveName = displayName;
        p.passiveType = type;
        p.description = description;
        p.unlockLevel = reqLevel;
        p.requiredAccountLevel = reqLevel;
        p.value1 = value1;
        SetArchetypeFlags(p, archetype);
    }

    #endregion

    #region Active Perks

    private void GeneratePerks()
    {
        // Multi Missile family (fires 3 missiles in a spread)
        CreateMulti("multi_missile_t1", "Multi Missile I", 1, 1, 9, 0.6f, 5f);
        CreateMulti("multi_missile_t2", "Multi Missile II", 2, 2, 36, 0.75f, 6f);
        CreateMulti("multi_missile_t3", "Multi Missile III", 3, 3, 75, 0.9f, 7f);

        // Cluster Missile family (splits mid-flight)
        CreateCluster("cluster_missile_t1", "Cluster Missile I", 1, 1, 13, 0.6f, 5f);
        CreateCluster("cluster_missile_t2", "Cluster Missile II", 2, 2, 40, 0.75f, 6f);
        CreateCluster("cluster_missile_t3", "Cluster Missile III", 3, 3, 83, 0.9f, 8f);

        // Explosive Missile family (AoE blast)
        CreateExplosive("explosive_missile_t1", "Explosive Missile I", 1, 1, 16, 8f, 2f, 20f);
        CreateExplosive("explosive_missile_t2", "Explosive Missile II", 2, 2, 48, 12f, 3f, 30f);
        CreateExplosive("explosive_missile_t3", "Explosive Missile III", 3, 3, 88, 16f, 4f, 45f);

        // Pusher Missile family (knockback)
        CreatePusher("pusher_missile_t1", "Pusher Missile I", 1, 1, 19, 1.5f, 0.7f);
        CreatePusher("pusher_missile_t2", "Pusher Missile II", 2, 2, 55, 2f, 0.8f);
        CreatePusher("pusher_missile_t3", "Pusher Missile III", 3, 3, 93, 3f, 0.9f);

        // Overcharged Cannon family (damage multiplier)
        CreateOvercharged("overcharged_cannon_t1", "Overcharged Cannon I", 1, 1, 27, 1.3f);
        CreateOvercharged("overcharged_cannon_t2", "Overcharged Cannon II", 2, 2, 60, 1.5f);
        CreateOvercharged("overcharged_cannon_t3", "Overcharged Cannon III", 3, 3, 98, 1.8f);

        // Missile Barrage family (sequential volley)
        CreateBarrage("missile_barrage_t1", "Missile Barrage I", 1, 1, 30, 0.3f, 5f, 1f);
        CreateBarrage("missile_barrage_t2", "Missile Barrage II", 2, 2, 67, 0.4f, 5f, 0.8f);
        CreateBarrage("missile_barrage_t3", "Missile Barrage III", 3, 3, 95, 0.5f, 6f, 0.6f);
    }

    private void SetPerkCommon(ActivePerkSO perk, string displayName, int tier, int cost, int reqLevel)
    {
        perk.perkName = displayName;
        perk.tier = tier;
        perk.cost = cost;
        perk.requiredAccountLevel = reqLevel;
        perk.allowTank = perk.allowDamageDealer = perk.allowController = perk.allowAllAround = true;
    }

    private void CreateMulti(string id, string name, int tier, int cost, int reqLevel, float dmg, float spread)
    {
        var p = GetOrCreate<MultiMissileSO>(PERKS_PATH, id);
        SetPerkCommon(p, name, tier, cost, reqLevel);
        p.damageFactor = dmg;
        p.spreadAngle = spread;
    }

    private void CreateCluster(string id, string name, int tier, int cost, int reqLevel, float dmg, float spread)
    {
        var p = GetOrCreate<ClusterMissileSO>(PERKS_PATH, id);
        SetPerkCommon(p, name, tier, cost, reqLevel);
        p.damageFactor = dmg;
        p.spreadAngle = spread;
    }

    private void CreateExplosive(string id, string name, int tier, int cost, int reqLevel,
        float blastRadius, float damageFactor, float pushStrength)
    {
        var p = GetOrCreate<ExplosiveMissileSO>(PERKS_PATH, id);
        SetPerkCommon(p, name, tier, cost, reqLevel);
        p.blastRadius = blastRadius;
        p.damageFactor = damageFactor;
        p.pushStrength = pushStrength;
    }

    private void CreatePusher(string id, string name, int tier, int cost, int reqLevel,
        float knockback, float damageFactor)
    {
        var p = GetOrCreate<PusherMissileSO>(PERKS_PATH, id);
        SetPerkCommon(p, name, tier, cost, reqLevel);
        p.knockbackMultiplier = knockback;
        p.damageFactor = damageFactor;
    }

    private void CreateOvercharged(string id, string name, int tier, int cost, int reqLevel, float dmgMult)
    {
        var p = GetOrCreate<OverchargedCannonSO>(PERKS_PATH, id);
        SetPerkCommon(p, name, tier, cost, reqLevel);
        p.damageMultiplier = dmgMult;
    }

    private void CreateBarrage(string id, string name, int tier, int cost, int reqLevel,
        float dmg, float spread, float interval)
    {
        var p = GetOrCreate<MissileBarrageSO>(PERKS_PATH, id);
        SetPerkCommon(p, name, tier, cost, reqLevel);
        p.damageFactor = dmg;
        p.spread = spread;
        p.interval = interval;
    }

    #endregion

    #region Missiles

    private void GenerateMissiles()
    {
        // Medium (Standard) - balanced, usable by every body
        CreateMissile("standard_mk1", "Standard Mk-I", MissileType.Medium, 1,
            physicsMass: 1.5f, displayMass: 500f, payload: 2500f, fuel: 100f,
            maxVelocity: 10f, push: 2f);
        CreateMissile("standard_mk2", "Standard Mk-II", MissileType.Medium, 4,
            physicsMass: 1.5f, displayMass: 520f, payload: 2750f, fuel: 110f,
            maxVelocity: 10.5f, push: 2.2f);
        CreateMissile("standard_mk3", "Standard Mk-III", MissileType.Medium, 11,
            physicsMass: 1.5f, displayMass: 540f, payload: 3000f, fuel: 120f,
            maxVelocity: 11f, push: 2.4f);

        // Light - fast, low damage
        CreateMissile("light_swarm", "Swarm Light", MissileType.Light, 8,
            physicsMass: 0.9f, displayMass: 280f, payload: 1600f, fuel: 90f,
            maxVelocity: 13f, push: 1.2f);
        CreateMissile("light_vortex", "Vortex Light", MissileType.Light, 14,
            physicsMass: 0.9f, displayMass: 300f, payload: 1800f, fuel: 100f,
            maxVelocity: 13.5f, push: 1.3f);
        CreateMissile("light_phantom", "Phantom Light", MissileType.Light, 22,
            physicsMass: 0.85f, displayMass: 310f, payload: 2000f, fuel: 110f,
            maxVelocity: 14f, push: 1.4f);

        // Heavy - slow, devastating
        CreateMissile("heavy_titan", "Titan Heavy", MissileType.Heavy, 9,
            physicsMass: 2.6f, displayMass: 900f, payload: 4000f, fuel: 120f,
            maxVelocity: 7.5f, push: 3.5f);
        CreateMissile("heavy_crusher", "Crusher Heavy", MissileType.Heavy, 17,
            physicsMass: 2.8f, displayMass: 980f, payload: 4500f, fuel: 130f,
            maxVelocity: 7f, push: 4f);
        CreateMissile("heavy_apocalypse", "Apocalypse Heavy", MissileType.Heavy, 27,
            physicsMass: 3.0f, displayMass: 1100f, payload: 5000f, fuel: 140f,
            maxVelocity: 6.5f, push: 4.5f);
    }

    private void CreateMissile(string id, string displayName, MissileType type, int reqLevel,
        float physicsMass, float displayMass, float payload, float fuel,
        float maxVelocity, float push)
    {
        var m = GetOrCreate<MissilePresetSO>(MISSILES_PATH, id);
        m.missileName = displayName;
        m.missileType = type;
        m.requiredAccountLevel = reqLevel;
        m.physicsMass = physicsMass;
        m.displayMass = displayMass;
        m.payload = payload;
        m.fuel = fuel;
        m.maxVelocity = maxVelocity;
        m.pushStrength = push;
    }

    #endregion

    #region Move Types

    private void GenerateMoveTypes()
    {
        // Heavy Thrusters - slow but long burn (Tank flavor)
        var heavy = GetOrCreate<MoveTypeSO>(MOVETYPES_PATH, "move_heavy_thrusters");
        heavy.moveTypeName = "Heavy Thrusters";
        heavy.category = MoveTypeCategory.Normal;
        heavy.description = "Slow but steady thrusters with a long burn. Built for heavy hulls.";
        heavy.requiredAccountLevel = 8;
        heavy.minMoveSpeed = 1.5f;
        heavy.maxMoveSpeed = 7f;
        heavy.moveDeceleration = 3f;
        heavy.moveDuration = 3.2f;
        heavy.allowTank = true;
        heavy.allowAllAround = true;
        heavy.allowDamageDealer = false;
        heavy.allowController = false;

        // Precision Drift - ghost preview (Controller flavor)
        var precision = GetOrCreate<MoveTypeSO>(MOVETYPES_PATH, "move_precision_drift");
        precision.moveTypeName = "Precision Drift";
        precision.category = MoveTypeCategory.Precision;
        precision.description = "Shows a ghost preview of your destination for precise positioning.";
        precision.requiredAccountLevel = 15;
        precision.minMoveSpeed = 2f;
        precision.maxMoveSpeed = 9f;
        precision.moveDeceleration = 5f;
        precision.moveDuration = 2.2f;
        precision.allowController = true;
        precision.allowAllAround = true;
        precision.allowDamageDealer = true;
        precision.allowTank = false;

        // Warp Jump - instant teleport (late unlock)
        var warp = GetOrCreate<MoveTypeSO>(MOVETYPES_PATH, "move_warp_jump");
        warp.moveTypeName = "Warp Jump";
        warp.category = MoveTypeCategory.Warp;
        warp.description = "Instantly warp to the target location. No travel time, no regrets.";
        warp.requiredAccountLevel = 30;
        warp.minMoveSpeed = 2f;
        warp.maxMoveSpeed = 10f;
        warp.warpZoomDuration = 0.3f;
        warp.allowController = true;
        warp.allowDamageDealer = true;
        warp.allowAllAround = true;
        warp.allowTank = false;
    }

    #endregion

    #region Prebuilt Ships

    private void GeneratePrebuiltShips()
    {
        var standardMove = AssetDatabase.LoadAssetAtPath<MoveTypeSO>(STANDARD_MOVE_PATH);
        var heavyMove = Load<MoveTypeSO>(MOVETYPES_PATH, "move_heavy_thrusters") ?? standardMove;
        var precisionMove = Load<MoveTypeSO>(MOVETYPES_PATH, "move_precision_drift") ?? standardMove;

        var lvlAll = AssetDatabase.LoadAssetAtPath<ShipLevelingFormulaSO>(LEVELING_ALLAROUND);
        var lvlTank = AssetDatabase.LoadAssetAtPath<ShipLevelingFormulaSO>(LEVELING_TANK);
        var lvlDD = AssetDatabase.LoadAssetAtPath<ShipLevelingFormulaSO>(LEVELING_DD);
        var lvlCtrl = AssetDatabase.LoadAssetAtPath<ShipLevelingFormulaSO>(LEVELING_CTRL);

        // Starter ship - free for everyone
        CreateShip("starter_ship", "Star Sparrow", 0, false,
            "Your first ship. Reliable, balanced, and ready for anything.",
            body: Load<ShipBodySO>(BODIES_PATH, "body_allaround_standard"),
            formula: lvlAll, move: standardMove,
            missile: Load<MissilePresetSO>(MISSILES_PATH, "standard_mk1"),
            passive: Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_shield_regen"),
            t1: Load<MultiMissileSO>(PERKS_PATH, "multi_missile_t1"),
            t2: null, t3: null);

        CreateShip("nova_class", "Nova Class", 3, false,
            "A sleek and agile all-around fighter.",
            Load<ShipBodySO>(BODIES_PATH, "body_allaround_standard"), lvlAll, standardMove,
            Load<MissilePresetSO>(MISSILES_PATH, "standard_mk1"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_shield_regen"),
            Load<MultiMissileSO>(PERKS_PATH, "multi_missile_t1"),
            Load<ClusterMissileSO>(PERKS_PATH, "cluster_missile_t2"), null);

        CreateShip("titan_defender", "Titan Defender", 6, false,
            "Heavy armor plating, built to withstand punishment.",
            Load<ShipBodySO>(BODIES_PATH, "body_tank_reinforced"), lvlTank, heavyMove,
            Load<MissilePresetSO>(MISSILES_PATH, "heavy_titan"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_armor_boost_1"),
            Load<PusherMissileSO>(PERKS_PATH, "pusher_missile_t1"),
            Load<ClusterMissileSO>(PERKS_PATH, "cluster_missile_t2"), null);

        CreateShip("phoenix_mk1", "Phoenix Mk-I", 7, false,
            "Enhanced maneuverability with improved firepower.",
            Load<ShipBodySO>(BODIES_PATH, "body_allaround_standard"), lvlAll, standardMove,
            Load<MissilePresetSO>(MISSILES_PATH, "standard_mk2"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_damage_boost_1"),
            Load<MultiMissileSO>(PERKS_PATH, "multi_missile_t1"),
            Load<ExplosiveMissileSO>(PERKS_PATH, "explosive_missile_t2"), null);

        CreateShip("eclipse_striker", "Eclipse Striker", 10, false,
            "A gift for reaching competitive play!",
            Load<ShipBodySO>(BODIES_PATH, "body_allaround_tactical"), lvlAll, standardMove,
            Load<MissilePresetSO>(MISSILES_PATH, "standard_mk2"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_shield_regen"),
            Load<ClusterMissileSO>(PERKS_PATH, "cluster_missile_t1"),
            Load<OverchargedCannonSO>(PERKS_PATH, "overcharged_cannon_t2"),
            Load<ExplosiveMissileSO>(PERKS_PATH, "explosive_missile_t3"));

        CreateShip("bastion_class", "Bastion Class", 12, false,
            "Impenetrable shields and reinforced hull.",
            Load<ShipBodySO>(BODIES_PATH, "body_tank_fortress"), lvlTank, heavyMove,
            Load<MissilePresetSO>(MISSILES_PATH, "heavy_titan"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_fortified"),
            Load<PusherMissileSO>(PERKS_PATH, "pusher_missile_t1"),
            Load<MissileBarrageSO>(PERKS_PATH, "missile_barrage_t2"), null);

        CreateShip("viper_assault", "Viper Assault", 16, false,
            "Lightning-fast attacks with devastating firepower.",
            Load<ShipBodySO>(BODIES_PATH, "body_dd_striker"), lvlDD, standardMove,
            Load<MissilePresetSO>(MISSILES_PATH, "light_swarm"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_lifesteal"),
            Load<MultiMissileSO>(PERKS_PATH, "multi_missile_t1"),
            Load<OverchargedCannonSO>(PERKS_PATH, "overcharged_cannon_t2"), null);

        CreateShip("juggernaut", "Juggernaut", 18, false,
            "Massive firepower with unbreakable defenses.",
            Load<ShipBodySO>(BODIES_PATH, "body_tank_colossus"), lvlTank, heavyMove,
            Load<MissilePresetSO>(MISSILES_PATH, "heavy_crusher"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_armor_boost_2"),
            Load<PusherMissileSO>(PERKS_PATH, "pusher_missile_t1"),
            Load<ClusterMissileSO>(PERKS_PATH, "cluster_missile_t2"),
            Load<ExplosiveMissileSO>(PERKS_PATH, "explosive_missile_t3"));

        CreateShip("reaper_class", "Reaper Class", 19, false,
            "High-energy weapons for maximum destruction.",
            Load<ShipBodySO>(BODIES_PATH, "body_dd_reaper"), lvlDD, standardMove,
            Load<MissilePresetSO>(MISSILES_PATH, "light_vortex"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_critical_strike"),
            Load<OverchargedCannonSO>(PERKS_PATH, "overcharged_cannon_t1"),
            Load<ExplosiveMissileSO>(PERKS_PATH, "explosive_missile_t2"),
            Load<MissileBarrageSO>(PERKS_PATH, "missile_barrage_t3"));

        CreateShip("nexus_command", "Nexus Command", 26, false,
            "Deploy tactical abilities to control the battlefield.",
            Load<ShipBodySO>(BODIES_PATH, "body_ctrl_tactician"), lvlCtrl, precisionMove,
            Load<MissilePresetSO>(MISSILES_PATH, "standard_mk3"),
            Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_sniper_mode"),
            Load<ClusterMissileSO>(PERKS_PATH, "cluster_missile_t1"),
            Load<PusherMissileSO>(PERKS_PATH, "pusher_missile_t2"),
            Load<MissileBarrageSO>(PERKS_PATH, "missile_barrage_t3"));
    }

    private void CreateShip(string id, string displayName, int reqLevel, bool premium,
        string description, ShipBodySO body, ShipLevelingFormulaSO formula, MoveTypeSO move,
        MissilePresetSO missile, PassiveAbilitySO passive,
        ActivePerkSO t1, ActivePerkSO t2, ActivePerkSO t3)
    {
        var ship = GetOrCreate<ShipPresetSO>(SHIPS_PATH, id);
        ship.shipName = displayName;
        ship.description = description;
        ship.requiredAccountLevel = reqLevel;
        ship.isPremiumShip = premium;
        ship.shipBody = body;
        ship.levelingFormula = formula;
        ship.moveType = move;
        ship.defaultMissile = missile;
        ship.passives = passive != null
            ? new PassiveAbilitySO[] { passive }
            : new PassiveAbilitySO[0];
        ship.tier1Perk = t1;
        ship.tier2Perk = t2;
        ship.tier3Perk = t3;
    }

    #endregion

    #region Battle Pass

    private void GenerateBattlePass()
    {
        var pass = GetOrCreate<BattlePassData>(BATTLEPASS_PATH, "Season1_BattlePass");
        pass.battlePassID = "season_1";
        pass.displayName = "Season 1: Cosmic Dawn";
        pass.isSeasonal = true;
        pass.seasonNumber = 1;
        pass.seasonStartDate = "2026-01-01";
        pass.seasonEndDate = "2026-12-31";
        pass.description = "The first season of Gravity Wars. Climb 30 tiers of rewards across free and premium tracks.";

        var tiers = new List<BattlePassTier>();
        for (int i = 1; i <= 30; i++)
        {
            var tier = new BattlePassTier
            {
                tierNumber = i,
                xpRequired = i * 1000,
                freeReward = BuildFreeReward(i),
                premiumReward = BuildPremiumReward(i)
            };
            tiers.Add(tier);
        }
        pass.tiers = tiers.ToArray();
    }

    private UnlockableReward BuildFreeReward(int tier)
    {
        var reward = new UnlockableReward();

        switch (tier)
        {
            case 3:
                reward.rewardType = RewardType.Missile;
                reward.rewardItem = Load<MissilePresetSO>(MISSILES_PATH, "standard_mk2");
                break;
            case 6:
                reward.rewardType = RewardType.Passive;
                reward.rewardItem = Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_shield_regen");
                break;
            case 10:
                reward.rewardType = RewardType.PrebuildShip;
                reward.rewardItem = Load<ShipPresetSO>(SHIPS_PATH, "nova_class");
                break;
            case 14:
                reward.rewardType = RewardType.Tier1Perk;
                reward.rewardItem = Load<MultiMissileSO>(PERKS_PATH, "multi_missile_t1");
                break;
            case 18:
                reward.rewardType = RewardType.Missile;
                reward.rewardItem = Load<MissilePresetSO>(MISSILES_PATH, "light_swarm");
                break;
            case 22:
                reward.rewardType = RewardType.Passive;
                reward.rewardItem = Load<PassiveAbilitySO>(PASSIVES_PATH, "passive_damage_boost_1");
                break;
            case 26:
                reward.rewardType = RewardType.Tier2Perk;
                reward.rewardItem = Load<ClusterMissileSO>(PERKS_PATH, "cluster_missile_t2");
                break;
            case 30:
                reward.rewardType = RewardType.PrebuildShip;
                reward.rewardItem = Load<ShipPresetSO>(SHIPS_PATH, "eclipse_striker");
                break;
            default:
                // Credits filler, scaling with tier; small gem drops every 5 tiers
                reward.rewardType = RewardType.Credits;
                reward.softCurrencyAmount = 250 + (tier * 50);
                if (tier % 5 == 0) reward.hardCurrencyAmount = 10;
                break;
        }

        return reward;
    }

    private UnlockableReward BuildPremiumReward(int tier)
    {
        var reward = new UnlockableReward();

        switch (tier)
        {
            case 2:
                reward.rewardType = RewardType.Skin;
                reward.skinID = "skin_premium_platinum";
                break;
            case 5:
                reward.rewardType = RewardType.PrebuildShip;
                reward.rewardItem = Load<ShipPresetSO>(SHIPS_PATH, "phoenix_mk1");
                break;
            case 8:
                reward.rewardType = RewardType.Tier2Perk;
                reward.rewardItem = Load<ExplosiveMissileSO>(PERKS_PATH, "explosive_missile_t2");
                break;
            case 12:
                reward.rewardType = RewardType.Skin;
                reward.skinID = "skin_premium_cosmic";
                break;
            case 15:
                reward.rewardType = RewardType.PrebuildShip;
                reward.rewardItem = Load<ShipPresetSO>(SHIPS_PATH, "bastion_class");
                break;
            case 18:
                reward.rewardType = RewardType.Missile;
                reward.rewardItem = Load<MissilePresetSO>(MISSILES_PATH, "heavy_titan");
                break;
            case 21:
                reward.rewardType = RewardType.Skin;
                reward.skinID = "skin_premium_diamond";
                break;
            case 25:
                reward.rewardType = RewardType.PrebuildShip;
                reward.rewardItem = Load<ShipPresetSO>(SHIPS_PATH, "viper_assault");
                break;
            case 28:
                reward.rewardType = RewardType.Tier3Perk;
                reward.rewardItem = Load<ExplosiveMissileSO>(PERKS_PATH, "explosive_missile_t3");
                break;
            case 30:
                reward.rewardType = RewardType.PrebuildShip;
                reward.rewardItem = Load<ShipPresetSO>(SHIPS_PATH, "nexus_command");
                break;
            default:
                // Gems filler, scaling with tier
                reward.rewardType = RewardType.Gems;
                reward.hardCurrencyAmount = 20 + (tier * 2);
                reward.softCurrencyAmount = 500 + (tier * 50);
                break;
        }

        return reward;
    }

    #endregion
}
