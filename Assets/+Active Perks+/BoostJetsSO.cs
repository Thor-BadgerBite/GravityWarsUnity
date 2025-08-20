// BoostJetsSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Boost Jets")]
public class BoostJetsSO : ActivePerkSO
{
    // No extra fields needed here—just create the runtime Perk:
    public override IActivePerk CreatePerk() => new BoostJetsPerk(this);
}
