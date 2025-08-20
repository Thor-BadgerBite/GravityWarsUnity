using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Overcharged Cannon")]
public class OverchargedCannonSO : ActivePerkSO
{
    [Tooltip("How much extra damage (e.g. 1.5 == +50%)")]
    public float damageMultiplier = 1.5f;

    public override IActivePerk CreatePerk() => new OverchargedCannonPerk(this);
}
