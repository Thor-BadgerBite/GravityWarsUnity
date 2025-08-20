using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Missile Barrage")]
public class MissileBarrageSO : ActivePerkSO
{
    [Header("Missile Barrage")]
    [Tooltip("Damage factor per missile (e.g. 0.4 = 40%)")]
    public float damageFactor = 0.4f;
    [Tooltip("Spread angle ± (degrees)")]
    public float spread = 5f;
    [Tooltip("Seconds between missiles")]
    public float interval = 1f;

    private void Reset()
    {
        tier = 2;
        cost = 2;
    }

    public override IActivePerk CreatePerk() => new MissileBarragePerk(this);
}
