using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Minimal ship card for Ships Garage inventory.
/// Displays only the ship icon with a Toggle component.
/// When clicked, notifies the controller to update the stats panel and model viewer.
///
/// Structure:
/// ShipCardPrefab
/// ├── ShipIcon (Image)
/// └── Toggle (on root GameObject)
/// </summary>
[RequireComponent(typeof(Toggle))]
public class ShipInventoryCard : MonoBehaviour
{
    #region Inspector References

    [Header("Card Visuals")]
    [SerializeField] private Image shipIcon;

    #endregion

    #region Components

    private Toggle _toggle;

    #endregion

    #region Events

    public event Action OnCardClicked;

    #endregion

    #region State

    private ShipBodySO _ship;
    private bool _isEquipped;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        if (_toggle != null)
        {
            _toggle.onValueChanged.AddListener(HandleToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (_toggle != null)
        {
            _toggle.onValueChanged.RemoveAllListeners();
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

        // Ship icon
        if (shipIcon != null && ship.icon != null)
        {
            shipIcon.sprite = ship.icon;
        }

        Debug.Log($"[ShipInventoryCard] Setup card for {ship.bodyName}");
    }

    /// <summary>
    /// Set the toggle group for this card.
    /// </summary>
    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        if (_toggle != null)
        {
            _toggle.group = toggleGroup;
        }
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Handle toggle value changed.
    /// </summary>
    private void HandleToggleChanged(bool isOn)
    {
        if (isOn)
        {
            OnCardClicked?.Invoke();
        }
    }

    /// <summary>
    /// Set this card as selected.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (_toggle != null)
        {
            _toggle.isOn = selected;
        }
    }

    #endregion

    #region Utility

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
