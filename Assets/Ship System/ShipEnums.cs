/// <summary>
/// Ship archetype types defining playstyle and stat scaling.
/// </summary>
public enum ShipArchetype
{
    Tank,           // High HP, high armor, lower damage
    DamageDealer,   // Low HP, low armor, high damage
    AllAround,      // Balanced stats
    Controller      // Tactical with extra action points
}

/// <summary>
/// Missile type categories.
/// </summary>
public enum MissileType
{
    Light,      // Fast, low damage, low fuel, weak push
    Medium,     // Balanced stats
    Heavy       // Slow, high damage, high fuel, strong push
}
