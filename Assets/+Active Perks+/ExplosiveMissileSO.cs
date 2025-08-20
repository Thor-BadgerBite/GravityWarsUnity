using UnityEngine;

[CreateAssetMenu(menuName="Perks/Explosive Missile")]
public class ExplosiveMissileSO : ActivePerkSO
{
    [Header("Explosion Parameters")]
    public float blastRadius    = 12f;
    public float damageFactor   = 3f;   // 3× normal payload
    public float pushStrength   = 30f;  // base impulse

    private void Reset()
    {
        tier = 3;
        cost = 3;
    }

    public override IActivePerk CreatePerk() => new ExplosiveMissilePerk(this);
}
