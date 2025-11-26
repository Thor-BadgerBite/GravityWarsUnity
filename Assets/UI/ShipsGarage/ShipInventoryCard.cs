using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Represents a single ship card in the Ships Garage inventory.
/// Displays ship icon, name, archetype, and equipped status.
///
/// Features:
/// - Ship icon/thumbnail
/// - Ship name
/// - Archetype indicator
/// - Equipped badge (Active Ship)
/// - Selection highlight
/// - Click interaction
/// </summary>
public class ShipInventoryCard : MonoBehaviour
{
    #region Inspector References

    [Header("Card Visuals")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image shipIcon;
    [SerializeField] private Image archetypeIcon;
    [SerializeField] private Image selectionHighlight;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI shipNameText;
    [SerializeField] private TextMeshProUGUI archetypeText;

    [Header("Status Indicators")]
    [SerializeField] private GameObject equippedBadge;
    [SerializeField] private TextMeshProUGUI equippedText;

    [Header("Button")]
    [SerializeField] private Button cardButton;

    [Header("Colors")]
    [SerializeField] private Color tankColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color ddColor = new Color(1f, 0.5f, 0.2f);
    [SerializeField] private Color controllerColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private Color allAroundColor = new Color(0.5f, 0.8f, 0.5f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.3f);
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.25f);

    #endregion

    #region Events

    public event Action OnCardClicked;

    #endregion

    #region State

    private ShipBodySO _ship;
    private bool _isEquipped;
    private bool _isSelected;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(HandleCardClick);
        }
    }

    private void OnDestroy()
    {
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
        }
    }

    #endregion

    #region Setup

    /// <summary>
    /// Setup the card with ship data.
    /// </summary>
    public void Setup(ShipBodySO ship, bool isEquipped)
    {
        if (ship == null)
        {
            Debug.LogError("[ShipInventoryCard] Cannot setup card - ship is null!");
            return;
        }

        _ship = ship;
        _isEquipped = isEquipped;

        // Ship name
        if (shipNameText != null)
            shipNameText.text = ship.bodyName;

        // Archetype
        if (archetypeText != null)
            archetypeText.text = GetArchetypeShortName(ship.archetype);

        // Ship icon
        if (shipIcon != null && ship.icon != null)
            shipIcon.sprite = ship.icon;

        // Archetype color
        Color archetypeColor = GetArchetypeColor(ship.archetype);
        if (archetypeIcon != null)
            archetypeIcon.color = archetypeColor;

        // Equipped badge
        if (equippedBadge != null)
            equippedBadge.SetActive(isEquipped);

        if (equippedText != null && isEquipped)
            equippedText.text = "ACTIVE";

        // Update card appearance
        UpdateCardAppearance();

        Debug.Log($"[ShipInventoryCard] Setup card for {ship.bodyName}");
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Handle card click.
    /// </summary>
    private void HandleCardClick()
    {
        OnCardClicked?.Invoke();
        PlayClickAnimation();
    }

    /// <summary>
    /// Set card selection state.
    /// </summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateCardAppearance();
    }

    /// <summary>
    /// Update card appearance based on state.
    /// </summary>
    private void UpdateCardAppearance()
    {
        // Update selection highlight
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(_isSelected);
            selectionHighlight.color = selectedColor;
        }

        // Update card background
        if (cardBackground != null)
        {
            if (_isSelected)
            {
                cardBackground.color = new Color(selectedColor.r * 0.3f, selectedColor.g * 0.3f, selectedColor.b * 0.3f);
            }
            else if (_isEquipped)
            {
                cardBackground.color = new Color(0.3f, 0.3f, 0.35f);
            }
            else
            {
                cardBackground.color = normalColor;
            }
        }
    }

    #endregion

    #region Animations

    /// <summary>
    /// Play click animation (scale bounce).
    /// </summary>
    private void PlayClickAnimation()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;

        Vector3 originalScale = rect.localScale;
        LeanTween.cancel(gameObject);

        LeanTween.scale(rect, originalScale * 0.95f, 0.1f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                LeanTween.scale(rect, originalScale, 0.1f).setEase(LeanTweenType.easeOutElastic);
            });
    }

    #endregion

    #region Utility

    /// <summary>
    /// Get color for ship archetype.
    /// </summary>
    private Color GetArchetypeColor(ShipArchetype archetype)
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                return tankColor;
            case ShipArchetype.DamageDealer:
                return ddColor;
            case ShipArchetype.Controller:
                return controllerColor;
            case ShipArchetype.AllAround:
                return allAroundColor;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Get short name for archetype (for card display).
    /// </summary>
    private string GetArchetypeShortName(ShipArchetype archetype)
    {
        switch (archetype)
        {
            case ShipArchetype.Tank:
                return "TANK";
            case ShipArchetype.DamageDealer:
                return "DD";
            case ShipArchetype.Controller:
                return "CTRL";
            case ShipArchetype.AllAround:
                return "ALL";
            default:
                return archetype.ToString().ToUpper();
        }
    }

    /// <summary>
    /// Get the ship this card represents.
    /// </summary>
    public ShipBodySO GetShip()
    {
        return _ship;
    }

    /// <summary>
    /// Check if this ship is equipped.
    /// </summary>
    public bool IsEquipped()
    {
        return _isEquipped;
    }

    #endregion
}
