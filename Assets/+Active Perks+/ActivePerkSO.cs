using UnityEngine;
public abstract class ActivePerkSO : ScriptableObject
{
    [Header("Identity")]
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

    [Header("Archetype Restrictions")]
    [Tooltip("Can Tank ships use this perk?")]
    public bool allowTank = true;

    [Tooltip("Can DamageDealer ships use this perk?")]
    public bool allowDamageDealer = true;

    [Tooltip("Can Controller ships use this perk?")]
    public bool allowController = true;

    [Tooltip("Can AllAround ships use this perk?")]
    public bool allowAllAround = true;

    [Header("Unlock Requirements")]
    [Tooltip("Account level required to unlock this perk (0 = available from start)")]
    [Range(0, 50)]
    public int requiredAccountLevel = 0;

    void OnValidate()
    {
        cost = tier;
        // auto‑set unlock level based on tier
        minLevel = tier == 1 ? 5 : tier == 2 ? 15 : 20;
    }

    /// <summary>
    /// Checks if this perk can be used by a ship with the given archetype
    /// </summary>
    public bool CanBeUsedBy(ShipArchetype archetype)
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                return allowTank;
            case ShipArchetype.DamageDealer:
                return allowDamageDealer;
            case ShipArchetype.Controller:
                return allowController;
            case ShipArchetype.AllAround:
                return allowAllAround;
            default:
                return false;
        }
    }

    /// <summary>
    /// Returns a list of allowed archetypes as a string
    /// </summary>
    public string GetAllowedArchetypes()
    {
        string result = "";
        if (allowTank) result += "Tank, ";
        if (allowDamageDealer) result += "DamageDealer, ";
        if (allowController) result += "Controller, ";
        if (allowAllAround) result += "AllAround, ";

        return result.TrimEnd(',', ' ');
    }

    public abstract IActivePerk CreatePerk();
}
