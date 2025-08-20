using UnityEngine;
public abstract class ActivePerkSO : ScriptableObject
{
    [Tooltip("Display name")]
    public string perkName;

    [Tooltip("Tier (1‑3)")]
    [Range(1,3)]
    public int tier = 1;

    [Tooltip("Move‑point cost to activate this perk (defaults to tier)")]
    public int cost = 1;

    [Tooltip("Icon for this perk")]
    public Sprite icon;

    /// <summary>
    /// Minimum ship level required to unlock this perk.
    /// Tier 1 → 5, Tier 2 → 15, Tier 3 → 20
    /// </summary>
    [HideInInspector] public int minLevel;

    void OnValidate()
    {
        cost = tier;
        // auto‑set unlock level based on tier
        minLevel = tier == 1 ? 5 : tier == 2 ? 15 : 20;
    }

    public abstract IActivePerk CreatePerk();
}
