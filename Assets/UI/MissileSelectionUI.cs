using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pre-match missile selection screen (retrofit system).
/// Shows the missiles the player has unlocked that are compatible with the
/// selected ship body, and stores the choice on the loadout
/// (CustomShipLoadout.equippedMissileName) - changing missiles never resets
/// ship XP, since the missile is excluded from the progression key.
///
/// Setup:
/// - Place on a (initially inactive) panel shown before entering a match.
/// - Assign a container (vertical/grid layout) and a button prefab with a
///   TextMeshProUGUI label; entries are generated at runtime.
/// - Call ShowForLoadout(loadout) or ShowForEquippedLoadout().
/// </summary>
public class MissileSelectionUI : MonoBehaviour
{
    public static MissileSelectionUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject selectionPanel;

    [Header("List")]
    [Tooltip("Parent transform the missile entries are spawned under")]
    [SerializeField] private Transform entriesContainer;
    [Tooltip("Prefab with a Button + TextMeshProUGUI label")]
    [SerializeField] private GameObject entryButtonPrefab;

    [Header("Details")]
    [SerializeField] private TextMeshProUGUI selectedMissileNameText;
    [SerializeField] private TextMeshProUGUI selectedMissileStatsText;
    [SerializeField] private TextMeshProUGUI shipInfoText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    /// <summary>Fired when the player confirms a missile (preset may be null if id-only).</summary>
    public event System.Action<MissilePresetSO> OnMissileConfirmed;

    private CustomShipLoadout _activeLoadout;
    private MissilePresetSO _selectedMissile;
    private readonly List<GameObject> _spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelection);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);

        if (selectionPanel != null)
            selectionPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #region Show / Hide

    /// <summary>
    /// Opens the selection screen for the currently equipped loadout.
    /// </summary>
    public void ShowForEquippedLoadout()
    {
        var data = ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
        if (data == null || data.customShipLoadouts.Count == 0)
        {
            Debug.LogWarning("[MissileSelectionUI] No loadouts available");
            return;
        }

        var loadout = data.customShipLoadouts.Find(l => l.loadoutID == data.currentEquippedShipId)
                      ?? data.customShipLoadouts[0];
        ShowForLoadout(loadout);
    }

    /// <summary>
    /// Opens the selection screen for a specific loadout.
    /// </summary>
    public void ShowForLoadout(CustomShipLoadout loadout)
    {
        if (loadout == null) return;

        _activeLoadout = loadout;
        _selectedMissile = null;

        if (selectionPanel != null) selectionPanel.SetActive(true);
        else gameObject.SetActive(true);

        var body = FindShipBody(loadout.shipBodyName);
        if (shipInfoText != null)
        {
            shipInfoText.text = body != null
                ? $"{loadout.loadoutName} — {body.archetype} ({body.GetMissileRestrictionsText()})"
                : loadout.loadoutName;
        }

        PopulateList(body);
        UpdateDetails();
    }

    public void Hide()
    {
        ClearEntries();
        if (selectionPanel != null) selectionPanel.SetActive(false);
        else gameObject.SetActive(false);
    }

    #endregion

    #region List Population

    private void PopulateList(ShipBodySO body)
    {
        ClearEntries();

        foreach (var missile in GetCompatibleUnlockedMissiles(body))
        {
            CreateEntry(missile);
        }

        if (_spawnedEntries.Count == 0)
        {
            Debug.LogWarning("[MissileSelectionUI] No compatible unlocked missiles for this ship");
        }
    }

    /// <summary>
    /// All missiles the player owns that the given body can carry.
    /// </summary>
    public List<MissilePresetSO> GetCompatibleUnlockedMissiles(ShipBodySO body)
    {
        var result = new List<MissilePresetSO>();
        var pm = ProgressionManager.Instance;
        if (pm == null) return result;

        foreach (var missile in pm.allMissiles)
        {
            if (missile == null) continue;
            if (!pm.IsUnlocked(missile)) continue;
            if (body != null && !body.CanUseMissileType(missile.missileType)) continue;
            result.Add(missile);
        }
        return result;
    }

    private void CreateEntry(MissilePresetSO missile)
    {
        if (entryButtonPrefab == null || entriesContainer == null) return;

        var entry = Instantiate(entryButtonPrefab, entriesContainer);
        _spawnedEntries.Add(entry);

        var label = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            bool isEquipped = _activeLoadout != null && _activeLoadout.equippedMissileName == missile.name;
            label.text = isEquipped ? $"► {missile.missileName}" : missile.missileName;
        }

        var button = entry.GetComponent<Button>();
        if (button != null)
        {
            var captured = missile;
            button.onClick.AddListener(() => SelectMissile(captured));
        }
    }

    private void ClearEntries()
    {
        foreach (var entry in _spawnedEntries)
        {
            if (entry != null) Destroy(entry);
        }
        _spawnedEntries.Clear();
    }

    #endregion

    #region Selection

    private void SelectMissile(MissilePresetSO missile)
    {
        _selectedMissile = missile;
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        if (_selectedMissile != null)
        {
            if (selectedMissileNameText != null)
                selectedMissileNameText.text = _selectedMissile.missileName;
            if (selectedMissileStatsText != null)
                selectedMissileStatsText.text = _selectedMissile.GetStatsDescription();
            if (confirmButton != null)
                confirmButton.interactable = true;
        }
        else
        {
            string current = _activeLoadout != null && !string.IsNullOrEmpty(_activeLoadout.equippedMissileName)
                ? _activeLoadout.equippedMissileName
                : "None";
            if (selectedMissileNameText != null)
                selectedMissileNameText.text = $"Equipped: {current}";
            if (selectedMissileStatsText != null)
                selectedMissileStatsText.text = "Select a missile to see its stats.";
            if (confirmButton != null)
                confirmButton.interactable = false;
        }
    }

    /// <summary>
    /// Stores the selection on the loadout and saves.
    /// Changing missiles keeps ship XP (missile is not part of the progression key).
    /// </summary>
    public void ConfirmSelection()
    {
        if (_selectedMissile == null || _activeLoadout == null) return;

        _activeLoadout.equippedMissileName = _selectedMissile.name;

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.Save();

        Debug.Log($"[MissileSelectionUI] Equipped '{_selectedMissile.missileName}' on '{_activeLoadout.loadoutName}'");

        OnMissileConfirmed?.Invoke(_selectedMissile);
        Hide();
    }

    #endregion

    private ShipBodySO FindShipBody(string bodyName)
    {
        var pm = ProgressionManager.Instance;
        if (pm == null || string.IsNullOrEmpty(bodyName)) return null;
        return pm.allShipBodies.Find(b => b != null && b.name == bodyName);
    }
}
