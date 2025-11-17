using System.Collections;
using UnityEngine;

public class MissileBarragePerk : IActivePerk
{
    private readonly MissileBarrageSO so;
    private bool used;

    public MissileBarragePerk(MissileBarrageSO so) { this.so = so; }

    public string Name => so.perkName;
    public int Tier => so.tier;
    public int Cost => so.cost;

    public bool CanActivate(PlayerShip ship)
        => ship.currentMode == PlayerShip.PlayerActionMode.Fire
           && !used
           && ship.movesRemainingThisRound >= Cost;

    public void Activate(PlayerShip ship)
    {
        used = true;
        // BUG FIX: Removed duplicate action point deduction (PerkManager.ConsumeToggledPerk() handles this)
        // BUG FIX: Removed UpdateFightingUI call (also handled by ConsumeToggledPerk)

        ship.StartCoroutine(FireMissileBarrage(ship));
        Debug.Log($"[MissileBarragePerk] Perk activated: firing barrage of {4} missiles");
    }

    private IEnumerator FireMissileBarrage(PlayerShip ship)
    {
        float baseAngle = ship.GetFiringAngle();
        Vector3 basePos = ship.transform.position +
                          new Vector3(Mathf.Cos(baseAngle * Mathf.Deg2Rad), Mathf.Sin(baseAngle * Mathf.Deg2Rad), 0) * ship.missileSpawnDistance;

        for (int i = 0; i < 4; i++)
        {
            // For the first missile, use the aimed angle; for others, randomize ±spread
            float angle = (i == 0)
                ? baseAngle
                : baseAngle + Random.Range(-so.spread, so.spread);

            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
            var missileObj = Object.Instantiate(ship.missilePrefab, basePos, Quaternion.Euler(0, 0, angle));
            var missile = missileObj.GetComponent<Missile3D>();

            // Scale payload
            missile.payload *= so.damageFactor;
            // Optionally tag as a "barrage" missile if needed

            missile.Launch(dir, ship.launchVelocity, ship.gameObject, ship.damageMultiplier);

            // (Optional) Remove extra trails from missiles after the first if needed

            // Wait interval before next
            if (i < 3)
                yield return new WaitForSeconds(so.interval);
        }
    }
}