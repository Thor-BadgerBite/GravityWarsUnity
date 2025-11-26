using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// UI manager for the Ships Garage panel.
/// Handles UI updates, animations, and user interactions.
///
/// Layout:
/// - Top: Title bar with close button (X)
/// - Left: Ship info panel with stats and 3D model
/// - Right: Scrollable ship inventory with archetype tabs
/// - Bottom left: Equip button
/// </summary>
public class ShipsGarageUI : MonoBehaviour
{
    #region Inspector References

    [Header("Main Panel")]
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private GameObject garagePanel;

    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cancelButton;

    [Header("Ship Info Panel (Left)")]
    [SerializeField] private GameObject shipInfoPanel;
    [SerializeField] private TextMeshProUGUI shipNameText;
    [SerializeField] private TextMeshProUGUI shipTypeText;
    [SerializeField] private TextMeshProUGUI shipLevelText;
    [SerializeField] private TextMeshProUGUI shipDamageText;
    [SerializeField] private TextMeshProUGUI shipHealthText;
    [SerializeField] private TextMeshProUGUI shipArmorText;
    [SerializeField] private Image xpProgressBar;
    [SerializeField] private TextMeshProUGUI xpText;

    [Header("Equip Button")]
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI equipButtonText;
    [SerializeField] private GameObject equippedIndicator;

    [Header("Ship Inventory (Right)")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private ScrollRect inventoryScrollRect;
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject shipCardPrefab;
    [SerializeField] private ToggleGroup shipToggleGroup;

    [Header("Archetype Tabs")]
    [SerializeField] private Button allTabButton;
    [SerializeField] private Button tankTabButton;
    [SerializeField] private Button ddTabButton;
    [SerializeField] private Button controllerTabButton;
    [SerializeField] private Button allAroundTabButton;

    [Header("Tab Visuals")]
    [SerializeField] private Color activeTabColor = new Color(1f, 0.8f, 0.3f);
    [SerializeField] private Color inactiveTabColor = Color.white;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    #endregion

    #region Events

    public event Action<ShipBodySO> OnShipSelected;
    public event Action OnEquipClicked;
    public event Action<ShipArchetype, bool> OnFilterChanged; // archetype, showAll
    public event Action OnCloseClicked;

    #endregion

    #region State

    private List<ShipInventoryCard> _shipCards = new List<ShipInventoryCard>();
    private ShipBodySO _currentlyDisplayedShip;
    private Button _activeTabButton;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        SetupButtonListeners();
        SetActiveTab(allTabButton); // Start with "All" tab active
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Setup all button click listeners.
    /// </summary>
    private void SetupButtonListeners()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

        if (cancelButton != null)
            cancelButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

        if (equipButton != null)
            equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke());

        // Archetype tabs
        if (allTabButton != null)
            allTabButton.onClick.AddListener(() => OnTabClicked(null, true));

        if (tankTabButton != null)
            tankTabButton.onClick.AddListener(() => OnTabClicked(ShipArchetype.Tank, false));

        if (ddTabButton != null)
            ddTabButton.onClick.AddListener(() => OnTabClicked(ShipArchetype.DamageDealer, false));

        if (controllerTabButton != null)
            controllerTabButton.onClick.AddListener(() => OnTabClicked(ShipArchetype.Controller, false));

        if (allAroundTabButton != null)
            allAroundTabButton.onClick.AddListener(() => OnTabClicked(ShipArchetype.AllAround, false));
    }

    /// <summary>
    /// Remove all button listeners.
    /// </summary>
    private void RemoveButtonListeners()
    {
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        if (equipButton != null) equipButton.onClick.RemoveAllListeners();
        if (allTabButton != null) allTabButton.onClick.RemoveAllListeners();
        if (tankTabButton != null) tankTabButton.onClick.RemoveAllListeners();
        if (ddTabButton != null) ddTabButton.onClick.RemoveAllListeners();
        if (controllerTabButton != null) controllerTabButton.onClick.RemoveAllListeners();
        if (allAroundTabButton != null) allAroundTabButton.onClick.RemoveAllListeners();
    }

    #endregion

    #region Ship Display

    /// <summary>
    /// Display all ships in the inventory.
    /// </summary>
    public void DisplayShips(List<ShipBodySO> ships, string equippedShipId)
    {
        // Clear existing cards
        ClearInventory();

        if (ships == null || ships.Count == 0)
        {
            Debug.LogWarning("[ShipsGarageUI] No ships to display!");
            return;
        }

        // Create card for each ship
        foreach (var ship in ships)
        {
            CreateShipCard(ship, ship.name == equippedShipId);
        }

        Debug.Log($"[ShipsGarageUI] Displayed {ships.Count} ships");
    }

    /// <summary>
    /// Create a ship card in the inventory.
    /// </summary>
    private void CreateShipCard(ShipBodySO ship, bool isEquipped)
    {
        if (shipCardPrefab == null || inventoryContent == null)
        {
            Debug.LogError("[ShipsGarageUI] Ship card prefab or inventory content not assigned!");
            return;
        }

        GameObject cardObj = Instantiate(shipCardPrefab, inventoryContent);
        ShipInventoryCard card = cardObj.GetComponent<ShipInventoryCard>();

        if (card != null)
        {
            card.Setup(ship, isEquipped);

            // Set toggle group for mutual exclusivity
            if (shipToggleGroup != null)
            {
                card.SetToggleGroup(shipToggleGroup);
            }

            card.OnCardClicked += () => OnShipSelected?.Invoke(ship);
            _shipCards.Add(card);
        }
        else
        {
            Debug.LogError("[ShipsGarageUI] ShipInventoryCard component not found on prefab!");
        }
    }

    /// <summary>
    /// Clear all ship cards from inventory.
    /// </summary>
    private void ClearInventory()
    {
        foreach (var card in _shipCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        _shipCards.Clear();
    }

    /// <summary>
    /// Refresh the inventory (used after equipping a ship).
    /// </summary>
    public void RefreshInventory()
    {
        // Update equipped indicators on all cards
        // This will be called by the controller after equipping
    }

    #endregion

    #region Ship Info Panel

    /// <summary>
    /// Update the ship info panel with selected ship data.
    /// </summary>
    public void UpdateShipInfo(ShipBodySO ship, ShipProgressionEntry progression, bool isEquipped)
    {
        if (ship == null)
        {
            Debug.LogWarning("[ShipsGarageUI] Cannot update ship info - ship is null!");
            return;
        }

        _currentlyDisplayedShip = ship;

        // Ship name
        if (shipNameText != null)
            shipNameText.text = ship.bodyName;

        // Ship type (archetype)
        if (shipTypeText != null)
            shipTypeText.text = GetArchetypeDisplayName(ship.archetype);

        // Ship level
        int shipLevel = progression != null ? progression.shipLevel : 1;
        if (shipLevelText != null)
            shipLevelText.text = $"Level: {shipLevel}";

        // Damage (numerical damage with equipped missile)
        // For now, we assume medium missile base damage of 1000 for display
        // This would normally come from the equipped missile
        if (shipDamageText != null)
        {
            int baseMissileDamage = 1000;
            int displayDamage = Mathf.RoundToInt(baseMissileDamage * ship.baseDamageMultiplier);
            shipDamageText.text = displayDamage.ToString();
        }

        // Health (current health of ship)
        if (shipHealthText != null)
            shipHealthText.text = Mathf.RoundToInt(ship.baseHealth).ToString();

        // Armor (current armor of ship)
        if (shipArmorText != null)
            shipArmorText.text = Mathf.RoundToInt(ship.baseArmor).ToString();

        // XP Progress
        if (progression != null)
        {
            float progress = progression.GetLevelProgress();
            int currentXP = progression.shipXP;
            int nextLevelXP = ShipProgressionEntry.GetXPRequiredForLevel(shipLevel + 1);

            if (xpProgressBar != null)
                xpProgressBar.fillAmount = progress;

            if (xpText != null)
                xpText.text = $"{currentXP}/{nextLevelXP}";
        }
        else
        {
            // No progression data, show 0 XP
            if (xpProgressBar != null)
                xpProgressBar.fillAmount = 0f;

            if (xpText != null)
                xpText.text = "0/275";
        }

        // Update equip button
        UpdateEquipButton(isEquipped);

        Debug.Log($"[ShipsGarageUI] Updated ship info: {ship.bodyName}");
    }

    /// <summary>
    /// Update equip button state.
    /// </summary>
    private void UpdateEquipButton(bool isEquipped)
    {
        if (equipButton != null)
        {
            equipButton.interactable = !isEquipped;
        }

        if (equipButtonText != null)
        {
            equipButtonText.text = isEquipped ? "EQUIPPED" : "EQUIP";
        }

        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(isEquipped);
        }
    }

    /// <summary>
    /// Get display name for ship archetype.
    /// </summary>
    private string GetArchetypeDisplayName(ShipArchetype archetype)
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                return "TANK";
            case ShipArchetype.DamageDealer:
                return "DAMAGE DEALER";
            case ShipArchetype.Controller:
                return "CONTROLLER";
            case ShipArchetype.AllAround:
                return "ALL-AROUND";
            default:
                return archetype.ToString().ToUpper();
        }
    }

    #endregion

    #region Tabs

    /// <summary>
    /// Handle tab click.
    /// </summary>
    private void OnTabClicked(ShipArchetype? archetype, bool showAll)
    {
        // Update active tab visual
        Button clickedButton = showAll ? allTabButton : GetTabButton(archetype.Value);
        SetActiveTab(clickedButton);

        // Notify controller
        OnFilterChanged?.Invoke(archetype ?? ShipArchetype.AllAround, showAll);

        Debug.Log($"[ShipsGarageUI] Tab clicked: {(showAll ? "All" : archetype.ToString())}");
    }

    /// <summary>
    /// Get tab button for archetype.
    /// </summary>
    private Button GetTabButton(ShipArchetype archetype)
    {
        switch (archetype)
        {
            case ShipArchetype.Tank: return tankTabButton;
            case ShipArchetype.DamageDealer: return ddTabButton;
            case ShipArchetype.Controller: return controllerTabButton;
            case ShipArchetype.AllAround: return allAroundTabButton;
            default: return allTabButton;
        }
    }

    /// <summary>
    /// Set active tab visual.
    /// </summary>
    private void SetActiveTab(Button tabButton)
    {
        if (tabButton == null) return;

        // Reset all tabs
        ResetTabVisual(allTabButton);
        ResetTabVisual(tankTabButton);
        ResetTabVisual(ddTabButton);
        ResetTabVisual(controllerTabButton);
        ResetTabVisual(allAroundTabButton);

        // Highlight active tab
        var image = tabButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = activeTabColor;
        }

        _activeTabButton = tabButton;
    }

    /// <summary>
    /// Reset tab visual to inactive state.
    /// </summary>
    private void ResetTabVisual(Button tabButton)
    {
        if (tabButton == null) return;

        var image = tabButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = inactiveTabColor;
        }
    }

    #endregion

    #region Animations

    /// <summary>
    /// Fade in the garage panel.
    /// </summary>
    public void FadeIn()
    {
        if (mainCanvasGroup == null) return;

        mainCanvasGroup.alpha = 0f;
        mainCanvasGroup.blocksRaycasts = true;
        mainCanvasGroup.interactable = true;

        LeanTween.alphaCanvas(mainCanvasGroup, 1f, fadeInDuration)
            .setEase(LeanTweenType.easeOutCubic);

        Debug.Log("[ShipsGarageUI] Fading in");
    }

    /// <summary>
    /// Fade out the garage panel.
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        if (mainCanvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        mainCanvasGroup.blocksRaycasts = false;
        mainCanvasGroup.interactable = false;

        LeanTween.alphaCanvas(mainCanvasGroup, 0f, fadeOutDuration)
            .setEase(LeanTweenType.easeInCubic)
            .setOnComplete(onComplete);

        Debug.Log("[ShipsGarageUI] Fading out");
    }

    /// <summary>
    /// Play button press animation.
    /// </summary>
    public void PlayButtonPressAnimation(Button button)
    {
        if (button == null) return;

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null) return;

        Vector3 originalScale = rect.localScale;
        LeanTween.scale(rect, originalScale * 0.95f, 0.1f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                LeanTween.scale(rect, originalScale, 0.1f).setEase(LeanTweenType.easeOutCubic);
            });
    }

    #endregion

    #region Utility

    /// <summary>
    /// Get currently displayed ship.
    /// </summary>
    public ShipBodySO GetCurrentlyDisplayedShip()
    {
        return _currentlyDisplayedShip;
    }

    #endregion
}
