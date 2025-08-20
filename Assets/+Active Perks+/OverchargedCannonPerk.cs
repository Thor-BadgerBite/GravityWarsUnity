using System.Collections;
using UnityEngine;

public class OverchargedCannonPerk : IActivePerk
{
    private readonly OverchargedCannonSO _so;
    private bool _usedThisTurn = false;

    public string Name => _so.perkName;
    public int Tier  => _so.tier;
    public int Cost  => _so.cost;        // <-- now reads directly from the SO
    public OverchargedCannonPerk(OverchargedCannonSO so)
    {
        _so = so;
    }

    public bool CanActivate(PlayerShip ship)
    {
        // hybrid rule: Tier‑1 unlimited × moves, Tier‑2+ once‑per‑turn
        if (ship.movesRemainingThisRound < Cost) return false;
        if (Tier > 1 && _usedThisTurn)      return false;
        return ship.currentMode == PlayerShip.PlayerActionMode.Fire;
    }

    public void Activate(PlayerShip ship)
    {
        _usedThisTurn = true;
        ship.damageMultiplier *= _so.damageMultiplier;

        // hook into the next FireMissile call
        ship.StartCoroutine(ResetAfterShot(ship));
    }

    private IEnumerator ResetAfterShot(PlayerShip ship)
    {
        // wait until they actually fire
        yield return new WaitUntil(() => ship.shotsThisRound > 0);

        // reset damage
        ship.damageMultiplier /= _so.damageMultiplier;
    }
}
