// Assets/+Active Perks+/MultiMissilePerk.cs
using UnityEngine;

public class MultiMissilePerk : IActivePerk
{
    readonly MultiMissileSO so;
    bool used;

    public MultiMissilePerk(MultiMissileSO so) { this.so = so; }

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
        ship.movesRemainingThisRound -= Cost;
        GameManager.Instance.UpdateFightingUI_AtRoundStart();

        // flag the next shot to be a multi-missile volley
        ship.nextMultiEnabled         = true;
        ship.nextMultiDamageFactor    = so.damageFactor;
        ship.nextMultiSpreadDeg       = so.spreadAngle;

        Debug.Log($"Multi Missile perk activated: spread ±{so.spreadAngle}°, damageFactor {so.damageFactor}");
    }
}
