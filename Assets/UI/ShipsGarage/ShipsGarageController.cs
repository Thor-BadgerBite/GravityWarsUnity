using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controller for the Ships Garage panel.
/// Manages ship selection, equipment, and 3D preview.
/// Similar to character selection in Brawl Stars.
///
/// Features:
/// - Displays all unlocked ships
/// - Filters by archetype (Tank, DD, Controller, All-Around)
/// - Shows ship stats and progression
/// - 3D ship preview with rotation
/// - Equip button to set active ship
/// </summary>
public class ShipsGarageController : MonoBehaviour
{
    #region Inspector References

    [Header("Components")]
    [SerializeField] private ShipsGarageUI garageUI;
    [SerializeField] private ShipViewer3D shipViewer;

    [Header("Archetype Filter")]
    [SerializeField] private ShipArchetype currentFilter = ShipArchetype.AllAround;
    [SerializeField] private bool showAllArchetypes = true;

    #endregion

    #region State

    private PlayerAccountData _playerData;
    private List<ShipBodySO> _allShips = new List<ShipBodySO>();
    private List<ShipBodySO> _filteredShips = new List<ShipBodySO>();
    private ShipBodySO _selectedShip;
    private ShipProgressionEntry _selectedShipProgression;
    private bool _isInitialized = false;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        ValidateReferences();
        Initialize();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Validate that all required components are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        if (garageUI == null)
        {
            Debug.LogError("[ShipsGarageController] ShipsGarageUI not assigned!");
        }

        if (shipViewer == null)
        {
            Debug.LogError("[ShipsGarageController] ShipViewer3D not assigned!");
        }
    }

    /// <summary>
    /// Initialize the Ships Garage.
    /// </summary>
    private void Initialize()
    {
        Debug.Log("[ShipsGarageController] Initializing Ships Garage...");

        // Get player data from ProgressionManager
        if (ProgressionManager.Instance == null)
        {
            Debug.LogError("[ShipsGarageController] ProgressionManager not found!");
            return;
        }

        _playerData = ProgressionManager.Instance.currentPlayerData;

        if (_playerData == null)
        {
            Debug.LogError("[ShipsGarageController] Player data not found!");
            return;
        }

        // Load all unlocked ships
        LoadUnlockedShips();

        // Subscribe to UI events
        SubscribeToEvents();

        // Initialize UI with default filter (show all)
        showAllArchetypes = true;
        ApplyFilter();

        // Select the currently equipped ship, or first ship if none equipped
        SelectInitialShip();

        _isInitialized = true;
        Debug.Log($"[ShipsGarageController] Initialized with {_allShips.Count} unlocked ships");
    }

    /// <summary>
    /// Load all unlocked ships from ProgressionManager.
    /// </summary>
    private void LoadUnlockedShips()
    {
        _allShips = ProgressionManager.Instance.GetUnlockedShipBodies();

        if (_allShips == null || _allShips.Count == 0)
        {
            Debug.LogWarning("[ShipsGarageController] No ships unlocked! Granting starter ships...");
            GrantStarterShips();
            _allShips = ProgressionManager.Instance.GetUnlockedShipBodies();
        }

        Debug.Log($"[ShipsGarageController] Loaded {_allShips.Count} unlocked ships");
    }

    /// <summary>
    /// Grant starter ships if player has none unlocked.
    /// </summary>
    private void GrantStarterShips()
    {
        // Grant one starter ship of each archetype (for testing)
        var allBodies = ProgressionManager.Instance.allShipBodies;

        foreach (var archetype in System.Enum.GetValues(typeof(ShipArchetype)))
        {
            var starterShip = allBodies.FirstOrDefault(s =>
                s.archetype == (ShipArchetype)archetype &&
                s.requiredAccountLevel == 0);

            if (starterShip != null)
            {
                ProgressionManager.Instance.UnlockItem(starterShip);
                Debug.Log($"[ShipsGarageController] Granted starter ship: {starterShip.bodyName}");
            }
        }
    }

    /// <summary>
    /// Select the initial ship to display.
    /// </summary>
    private void SelectInitialShip()
    {
        ShipBodySO shipToSelect = null;

        // Try to select currently equipped ship
        if (!string.IsNullOrEmpty(_playerData.currentEquippedShipId))
        {
            shipToSelect = _filteredShips.FirstOrDefault(s => s.name == _playerData.currentEquippedShipId);
        }

        // Fall back to first ship in filtered list
        if (shipToSelect == null && _filteredShips.Count > 0)
        {
            shipToSelect = _filteredShips[0];
        }

        if (shipToSelect != null)
        {
            SelectShip(shipToSelect);
        }
    }

    #endregion

    #region Event Subscriptions

    /// <summary>
    /// Subscribe to UI events.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (garageUI == null) return;

        garageUI.OnShipSelected += HandleShipSelected;
        garageUI.OnEquipClicked += HandleEquipClicked;
        garageUI.OnFilterChanged += HandleFilterChanged;
        garageUI.OnCloseClicked += HandleCloseClicked;
    }

    /// <summary>
    /// Unsubscribe from UI events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (garageUI == null) return;

        garageUI.OnShipSelected -= HandleShipSelected;
        garageUI.OnEquipClicked -= HandleEquipClicked;
        garageUI.OnFilterChanged -= HandleFilterChanged;
        garageUI.OnCloseClicked -= HandleCloseClicked;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handle ship selection from inventory.
    /// </summary>
    private void HandleShipSelected(ShipBodySO ship)
    {
        SelectShip(ship);
    }

    /// <summary>
    /// Handle equip button click.
    /// </summary>
    private void HandleEquipClicked()
    {
        EquipSelectedShip();
    }

    /// <summary>
    /// Handle archetype filter change.
    /// </summary>
    private void HandleFilterChanged(ShipArchetype archetype, bool showAll)
    {
        currentFilter = archetype;
        showAllArchetypes = showAll;
        ApplyFilter();
    }

    /// <summary>
    /// Handle close button click.
    /// </summary>
    private void HandleCloseClicked()
    {
        CloseGarage();
    }

    #endregion

    #region Ship Selection

    /// <summary>
    /// Select a ship and update UI.
    /// </summary>
    private void SelectShip(ShipBodySO ship)
    {
        if (ship == null) return;

        _selectedShip = ship;

        // Get progression data for this ship
        _selectedShipProgression = GetShipProgression(ship);

        // Update 3D viewer
        if (shipViewer != null)
        {
            shipViewer.DisplayShip(ship.name);
        }

        // Update UI
        if (garageUI != null)
        {
            garageUI.UpdateShipInfo(ship, _selectedShipProgression, IsShipEquipped(ship));
        }

        Debug.Log($"[ShipsGarageController] Selected ship: {ship.bodyName}");
    }

    /// <summary>
    /// Get progression data for a ship body.
    /// Note: Ships can have different progression based on their loadout configuration.
    /// For the garage preview, we show the highest level version or create a default entry.
    /// </summary>
    private ShipProgressionEntry GetShipProgression(ShipBodySO ship)
    {
        // Find all loadouts using this ship body
        var loadoutsWithShip = _playerData.customShipLoadouts
            .Where(l => l.shipBodyName == ship.name)
            .ToList();

        if (loadoutsWithShip.Count == 0)
        {
            // No loadout exists yet, create a default progression entry
            return new ShipProgressionEntry(ship.name, ship.bodyName);
        }

        // Find the highest level progression for this ship body
        ShipProgressionEntry highestProgression = null;
        int highestLevel = 0;

        foreach (var loadout in loadoutsWithShip)
        {
            var progression = _playerData.GetShipProgression(loadout);
            if (progression != null && progression.shipLevel > highestLevel)
            {
                highestLevel = progression.shipLevel;
                highestProgression = progression;
            }
        }

        return highestProgression ?? new ShipProgressionEntry(ship.name, ship.bodyName);
    }

    /// <summary>
    /// Check if a ship is currently equipped.
    /// </summary>
    private bool IsShipEquipped(ShipBodySO ship)
    {
        return _playerData.currentEquippedShipId == ship.name;
    }

    #endregion

    #region Ship Equipment

    /// <summary>
    /// Equip the currently selected ship.
    /// </summary>
    private void EquipSelectedShip()
    {
        if (_selectedShip == null)
        {
            Debug.LogWarning("[ShipsGarageController] No ship selected!");
            return;
        }

        // Check if already equipped
        if (IsShipEquipped(_selectedShip))
        {
            Debug.Log($"[ShipsGarageController] {_selectedShip.bodyName} is already equipped");
            return;
        }

        // Update player data
        _playerData.currentEquippedShipId = _selectedShip.name;

        // Save
        ProgressionManager.Instance.Save();

        // Update UI to reflect equipped status
        if (garageUI != null)
        {
            garageUI.UpdateShipInfo(_selectedShip, _selectedShipProgression, true);
            garageUI.RefreshInventory(); // Refresh to update equipped indicators on cards
        }

        // Notify MainMenuController if it exists
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.UpdateEquippedShip(_selectedShip.name);
        }

        Debug.Log($"[ShipsGarageController] Equipped ship: {_selectedShip.bodyName}");

        // Play celebration animation
        if (shipViewer != null)
        {
            shipViewer.PlayCelebrationSpin();
        }

        // Close the garage window after equipping
        CloseGarage();
    }

    #endregion

    #region Filtering

    /// <summary>
    /// Apply archetype filter to ship list.
    /// </summary>
    private void ApplyFilter()
    {
        if (showAllArchetypes)
        {
            _filteredShips = new List<ShipBodySO>(_allShips);
        }
        else
        {
            _filteredShips = _allShips.Where(s => s.archetype == currentFilter).ToList();
        }

        // Sort ships by archetype, then by level requirement
        _filteredShips = _filteredShips
            .OrderBy(s => s.archetype)
            .ThenBy(s => s.requiredAccountLevel)
            .ToList();

        // Update UI
        if (garageUI != null)
        {
            garageUI.DisplayShips(_filteredShips, _playerData.currentEquippedShipId);
        }

        // If current selection is not in filtered list, select first ship
        if (_selectedShip != null && !_filteredShips.Contains(_selectedShip))
        {
            if (_filteredShips.Count > 0)
            {
                SelectShip(_filteredShips[0]);
            }
        }

        Debug.Log($"[ShipsGarageController] Applied filter: {(showAllArchetypes ? "All" : currentFilter.ToString())} - {_filteredShips.Count} ships");
    }

    #endregion

    #region Garage Control

    /// <summary>
    /// Close the Ships Garage and return to main menu.
    /// </summary>
    private void CloseGarage()
    {
        Debug.Log("[ShipsGarageController] Closing Ships Garage");

        // Save before closing
        if (_isInitialized)
        {
            ProgressionManager.Instance.Save();
        }

        // Fade out and destroy
        if (garageUI != null)
        {
            garageUI.FadeOut(() =>
            {
                gameObject.SetActive(false);
            });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Open the Ships Garage panel.
    /// </summary>
    public void OpenGarage()
    {
        gameObject.SetActive(true);

        if (!_isInitialized)
        {
            Initialize();
        }
        else
        {
            // Refresh data
            LoadUnlockedShips();
            ApplyFilter();
            SelectInitialShip();
        }

        // Fade in
        if (garageUI != null)
        {
            garageUI.FadeIn();
        }

        Debug.Log("[ShipsGarageController] Opened Ships Garage");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get currently selected ship.
    /// </summary>
    public ShipBodySO GetSelectedShip()
    {
        return _selectedShip;
    }

    /// <summary>
    /// Get all unlocked ships.
    /// </summary>
    public List<ShipBodySO> GetAllShips()
    {
        return new List<ShipBodySO>(_allShips);
    }

    /// <summary>
    /// Get filtered ships.
    /// </summary>
    public List<ShipBodySO> GetFilteredShips()
    {
        return new List<ShipBodySO>(_filteredShips);
    }

    /// <summary>
    /// Check if garage is initialized.
    /// </summary>
    public bool IsInitialized()
    {
        return _isInitialized;
    }

    #endregion
}
