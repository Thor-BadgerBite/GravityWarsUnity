using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a ship skin (visual customization).
/// Create via: Right-click -> Create -> GravityWars -> Cosmetics -> Ship Skin
/// </summary>
[CreateAssetMenu(fileName = "NewSkin", menuName = "GravityWars/Cosmetics/Ship Skin")]
public class ShipSkinSO : ScriptableObject
{
    [Header("Skin Info")]
    public string skinID;
    public string skinName;
    public Sprite icon;

    [TextArea(2, 3)]
    public string description;

    [Header("Restrictions")]
    [Tooltip("Which ship archetype can use this skin? (leave all unchecked for universal)")]
    public bool tankOnly = false;
    public bool damageDealerOnly = false;
    public bool controllerOnly = false;
    public bool allAroundOnly = false;

    [Header("Acquisition")]
    public bool isPremiumExclusive = false;
    public bool isSeasonalExclusive = false;
    public int seasonNumber = 0;

    [Header("Visual Model")]
    [Tooltip("The 3D model prefab for this skin")]
    public GameObject modelPrefab;

    [Header("Materials")]
    [Tooltip("Override materials for ship parts")]
    public Material bodyMaterial;
    public Material wingMaterial;
    public Material weaponMaterial;
    public Material engineMaterial;

    /// <summary>
    /// Checks if this skin can be used by a specific archetype
    /// </summary>
    public bool CanBeUsedBy(ShipArchetype archetype)
    {
        // If no restrictions, it's universal
        if (!tankOnly && !damageDealerOnly && !controllerOnly && !allAroundOnly)
            return true;

        return archetype switch
        {
            ShipArchetype.Tank => tankOnly,
            ShipArchetype.DamageDealer => damageDealerOnly,
            ShipArchetype.Controller => controllerOnly,
            ShipArchetype.AllAround => allAroundOnly,
            _ => false
        };
    }

    void OnValidate()
    {
        if (string.IsNullOrEmpty(skinID))
            skinID = name;
    }
}

/// <summary>
/// Defines a color scheme for ship customization.
/// Create via: Right-click -> Create -> GravityWars -> Cosmetics -> Color Scheme
/// </summary>
[CreateAssetMenu(fileName = "NewColorScheme", menuName = "GravityWars/Cosmetics/Color Scheme")]
public class ColorSchemeSO : ScriptableObject
{
    [Header("Color Scheme Info")]
    public string colorSchemeID;
    public string schemeName;
    public Sprite icon;

    [Header("Colors")]
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.gray;
    public Color accentColor = Color.blue;
    public Color emissionColor = Color.cyan;

    [Header("Acquisition")]
    public bool isPremiumExclusive = false;
    public int unlockLevel = 0; // 0 = starter, >0 = requires account level

    void OnValidate()
    {
        if (string.IsNullOrEmpty(colorSchemeID))
            colorSchemeID = name;
    }

    /// <summary>
    /// Applies this color scheme to a ship's materials
    /// </summary>
    public void ApplyToShip(GameObject shipObject)
    {
        // Find all renderers in ship
        MeshRenderer[] renderers = shipObject.GetComponentsInChildren<MeshRenderer>();

        foreach (var renderer in renderers)
        {
            // Create material instance
            Material mat = new Material(renderer.material);

            // Apply colors based on part name
            if (renderer.name.Contains("Body"))
            {
                mat.color = primaryColor;
            }
            else if (renderer.name.Contains("Wing"))
            {
                mat.color = secondaryColor;
            }
            else if (renderer.name.Contains("Weapon") || renderer.name.Contains("Engine"))
            {
                mat.color = accentColor;
            }

            // Apply emission if supported
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");
            }

            renderer.material = mat;
        }

        Debug.Log($"[ColorScheme] Applied {schemeName} to ship");
    }
}

/// <summary>
/// Defines a decal that can be applied to ships.
/// Create via: Right-click -> Create -> GravityWars -> Cosmetics -> Decal
/// </summary>
[CreateAssetMenu(fileName = "NewDecal", menuName = "GravityWars/Cosmetics/Decal")]
public class DecalSO : ScriptableObject
{
    [Header("Decal Info")]
    public string decalID;
    public string decalName;
    public Sprite icon;

    [Header("Decal Texture")]
    public Texture2D decalTexture;
    public Vector2 decalScale = Vector2.one;
    public Vector2 decalOffset = Vector2.zero;

    [Header("Placement")]
    [Tooltip("Which part of the ship to place decal on")]
    public DecalPlacement placement = DecalPlacement.Body;

    [Header("Acquisition")]
    public bool isPremiumExclusive = false;
    public bool isAchievementReward = false;
    public string achievementID;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(decalID))
            decalID = name;
    }
}

public enum DecalPlacement
{
    Body,
    Wings,
    Engine,
    Weapon
}

/// <summary>
/// Manages cosmetic application to ships at runtime.
/// </summary>
public class CosmeticsApplier : MonoBehaviour
{
    [Header("Cosmetic Databases")]
    public List<ShipSkinSO> allSkins = new List<ShipSkinSO>();
    public List<ColorSchemeSO> allColorSchemes = new List<ColorSchemeSO>();
    public List<DecalSO> allDecals = new List<DecalSO>();

    /// <summary>
    /// Applies cosmetics from a loadout to a ship GameObject
    /// </summary>
    public void ApplyCosmeticsToShip(GameObject shipObject, CustomShipLoadout loadout)
    {
        if (shipObject == null || loadout == null) return;

        // Apply skin (replaces model)
        if (!string.IsNullOrEmpty(loadout.skinID))
        {
            ApplySkin(shipObject, loadout.skinID);
        }

        // Apply color scheme
        if (!string.IsNullOrEmpty(loadout.colorSchemeID))
        {
            ApplyColorScheme(shipObject, loadout.colorSchemeID);
        }

        // Apply decal
        if (!string.IsNullOrEmpty(loadout.decalID))
        {
            ApplyDecal(shipObject, loadout.decalID);
        }
    }

    /// <summary>
    /// Applies a skin to a ship
    /// </summary>
    private void ApplySkin(GameObject shipObject, string skinID)
    {
        ShipSkinSO skin = allSkins.Find(s => s.skinID == skinID);
        if (skin == null || skin.modelPrefab == null)
        {
            Debug.LogWarning($"[CosmeticsApplier] Skin not found: {skinID}");
            return;
        }

        // Instantiate new model as child (preserve ship logic)
        // In production, you'd replace the visual mesh while keeping physics/logic
        GameObject visualModel = Instantiate(skin.modelPrefab, shipObject.transform);
        visualModel.transform.localPosition = Vector3.zero;
        visualModel.transform.localRotation = Quaternion.identity;

        // Hide original model (or destroy it)
        MeshRenderer originalRenderer = shipObject.GetComponent<MeshRenderer>();
        if (originalRenderer != null)
            originalRenderer.enabled = false;

        Debug.Log($"[CosmeticsApplier] Applied skin: {skin.skinName}");
    }

    /// <summary>
    /// Applies a color scheme to a ship
    /// </summary>
    private void ApplyColorScheme(GameObject shipObject, string colorSchemeID)
    {
        ColorSchemeSO scheme = allColorSchemes.Find(c => c.colorSchemeID == colorSchemeID);
        if (scheme == null)
        {
            Debug.LogWarning($"[CosmeticsApplier] Color scheme not found: {colorSchemeID}");
            return;
        }

        scheme.ApplyToShip(shipObject);
    }

    /// <summary>
    /// Applies a decal to a ship
    /// </summary>
    private void ApplyDecal(GameObject shipObject, string decalID)
    {
        DecalSO decal = allDecals.Find(d => d.decalID == decalID);
        if (decal == null || decal.decalTexture == null)
        {
            Debug.LogWarning($"[CosmeticsApplier] Decal not found: {decalID}");
            return;
        }

        // Find target part based on placement
        string targetPartName = decal.placement.ToString();
        Transform targetPart = FindChildRecursive(shipObject.transform, targetPartName);

        if (targetPart == null)
        {
            Debug.LogWarning($"[CosmeticsApplier] Target part not found: {targetPartName}");
            return;
        }

        // Apply decal texture to material
        MeshRenderer renderer = targetPart.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(renderer.material);
            mat.SetTexture("_MainTex", decal.decalTexture);
            mat.SetTextureScale("_MainTex", decal.decalScale);
            mat.SetTextureOffset("_MainTex", decal.decalOffset);
            renderer.material = mat;
        }

        Debug.Log($"[CosmeticsApplier] Applied decal: {decal.decalName}");
    }

    /// <summary>
    /// Recursively finds a child transform by name
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
                return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Gets all unlocked skins for a player
    /// </summary>
    public List<ShipSkinSO> GetUnlockedSkins(PlayerAccountData playerData)
    {
        List<ShipSkinSO> unlocked = new List<ShipSkinSO>();
        foreach (var skin in allSkins)
        {
            if (playerData.unlockedSkinIDs.Contains(skin.skinID))
                unlocked.Add(skin);
        }
        return unlocked;
    }

    /// <summary>
    /// Gets all unlocked color schemes for a player
    /// </summary>
    public List<ColorSchemeSO> GetUnlockedColorSchemes(PlayerAccountData playerData)
    {
        List<ColorSchemeSO> unlocked = new List<ColorSchemeSO>();
        foreach (var scheme in allColorSchemes)
        {
            if (playerData.unlockedColorSchemeIDs.Contains(scheme.colorSchemeID))
                unlocked.Add(scheme);
        }
        return unlocked;
    }
}
