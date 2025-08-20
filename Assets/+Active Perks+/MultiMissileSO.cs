// Assets/+Active Perks+/MultiMissileSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Multi Missile")]
public class MultiMissileSO : ActivePerkSO
{
    [Header("Spread Settings")]
    [Tooltip("Payload factor applied to each missile (e.g. 0.75 = 75% damage)")]
    public float damageFactor = 0.75f;

    [Tooltip("Spread angle in degrees for the side missiles (± this value)")]
    public float spreadAngle = 5f;

    private void Reset()
    {
        tier = 1;
        cost = 1;
    }

    public override IActivePerk CreatePerk() => new MultiMissilePerk(this);
}
