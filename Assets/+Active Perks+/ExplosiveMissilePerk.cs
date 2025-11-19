using UnityEngine;
public class ExplosiveMissilePerk : IActivePerk
{
    readonly ExplosiveMissileSO so;
    bool used;

    public ExplosiveMissilePerk(ExplosiveMissileSO so) { this.so = so; }

    public string Name => so.perkName;
    public int    Tier => so.tier;
    public int    Cost => so.cost;

    public bool CanActivate(PlayerShip ship)
        => ship.currentMode == PlayerShip.PlayerActionMode.Fire
        && !used
        && ship.movesRemainingThisRound >= Cost;

    public void Activate(PlayerShip ship)
    {
        used = true;
        // BUG FIX: Removed duplicate action point deduction (PerkManager.ConsumeToggledPerk() handles this)
        // BUG FIX: Removed UpdateFightingUI call (also handled by ConsumeToggledPerk)

        // flag the next missile
        ship.nextExplosiveEnabled      = true;
        ship.nextExplRadius            = so.blastRadius;
        ship.nextExplDamageFactor      = so.damageFactor;
        ship.nextExplPushStrength      = so.pushStrength;
        Debug.Log($"[ExplosiveMissilePerk] Perk activated: blast radius {so.blastRadius}, damage factor {so.damageFactor}, push strength {so.pushStrength}");
    }
}
