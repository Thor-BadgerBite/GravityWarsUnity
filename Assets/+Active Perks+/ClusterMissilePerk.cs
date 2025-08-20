// Assets/+Active Perks+/ClusterMissilePerk.cs
using UnityEngine;

public class ClusterMissilePerk : IActivePerk
{
    readonly ClusterMissileSO so;
    bool used;

    public ClusterMissilePerk(ClusterMissileSO so) { this.so = so; }

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

        // flag the next missile to be a cluster
        ship.nextClusterEnabled          = true;
        ship.nextClusterDamageFactor     = so.damageFactor;
        ship.nextClusterSpreadDeg        = so.spreadAngle;
        Debug.Log($"Cluster missile perk activated: damage×{so.damageFactor}, spread±{so.spreadAngle}°");
    }
}
