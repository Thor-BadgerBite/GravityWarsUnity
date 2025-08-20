// Assets/+Active Perks+/ClusterMissileSO.cs
using UnityEngine;

[CreateAssetMenu(menuName="Perks/Cluster Missile")]
public class ClusterMissileSO : ActivePerkSO
{
    [Header("Cluster Settings")]
    [Tooltip("Payload factor applied to each fragment (e.g. 0.75 = 75% damage)")]
    public float damageFactor = 0.75f;

    [Tooltip("Angular spread for the two side fragments (± this value)")]
    public float spreadAngle = 5f;

    private void Reset()
    {
        tier = 2;
        cost = 2;
    }

    public override IActivePerk CreatePerk() => new ClusterMissilePerk(this);
}
