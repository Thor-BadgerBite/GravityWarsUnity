using UnityEngine;

[CreateAssetMenu(menuName="Perks/Pusher Missile")]
public class PusherMissileSO : ActivePerkSO
{
    [Header("Knockback & Damage")]
    [Tooltip("Multiplier applied to the knockback force on impact")]
    public float knockbackMultiplier = 2f;

    [Tooltip("Damage factor applied to the missile payload (e.g. 0.8 = 80% damage)")]
    public float damageFactor = 0.8f;

    private void Reset()
    {
        // Tier 1 perk
        tier = 1;
        cost = 1;
    }

    public override IActivePerk CreatePerk() => new PusherMissilePerk(this);
}
