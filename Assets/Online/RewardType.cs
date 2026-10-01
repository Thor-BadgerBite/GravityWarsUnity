/// <summary>
/// Types of rewards a battle-pass tier can grant (GDD §12.1). Used by
/// BattlePassSystem's reward tables and BattlePassReward; the second
/// (ScriptableObject-based) battle pass that originally declared this enum
/// was deleted in refactor step 4 (CODE_AUDIT B4).
/// </summary>
public enum RewardType
{
    None,
    Credits,        // Soft currency reward
    Gems,           // Hard currency reward
    ShipBody,
    Tier1Perk,
    Tier2Perk,
    Tier3Perk,
    Active,         // Active perk (used by BattlePassSystem)
    Passive,
    MoveType,
    Missile,
    PrebuildShip,   // Complete ship model (used by BattlePassSystem)
    Skin,
    ColorScheme,
    Decal
}
