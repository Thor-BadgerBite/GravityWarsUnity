using UnityEngine;
public class PusherMissilePerk : IActivePerk
{
    readonly PusherMissileSO so;
    bool used;

    public PusherMissilePerk(PusherMissileSO so) { this.so = so; }

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

        // flag the next missile as "pusher"
        ship.nextPushEnabled         = true;
        ship.nextPushDamageFactor    = so.damageFactor;
        ship.nextPushKnockbackFactor = so.knockbackMultiplier;

        Debug.Log($"[PusherMissilePerk] Perk activated: damage ×{so.damageFactor}, knockback ×{so.knockbackMultiplier}");
    }
}
