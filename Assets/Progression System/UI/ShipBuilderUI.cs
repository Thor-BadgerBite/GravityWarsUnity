using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI controller for the ship customization/builder screen.
/// Allows players to create custom ship loadouts from unlocked components.
/// </summary>
public class ShipBuilderUI : MonoBehaviour
{
    [Header("References")]
    public ProgressionManager progressionManager;

    [Header("Input Fields")]
    public TMP_InputField loadoutNameInput;

    [Header("Component Selection Panels")]
    public Transform shipBodyPanel;
    public Transform tier1PerkPanel;
    public Transform tier2PerkPanel;
    public Transform tier3PerkPanel;
    public Transform passivePanel;
    public Transform moveTypePanel;
    public Transform missilePanel;
    public Transform skinPanel;
    public Transform colorSchemePanel;

    [Header("Selected Component Display")]
    public TextMeshProUGUI selectedBodyText;
    public TextMeshProUGUI selectedTier1PerkText;
    public TextMeshProUGUI selectedTier2PerkText;
    public TextMeshProUGUI selectedTier3PerkText;
    public TextMeshProUGUI selectedPassiveText;
    public TextMeshProUGUI selectedMoveTypeText;
    public TextMeshProUGUI selectedMissileText;

    [Header("Preview Panel")]
    public TextMeshProUGUI previewStatsText;
    public Image previewShipIcon;
    public GameObject validationErrorPanel;
    public TextMeshProUGUI validationErrorText;

    [Header("Buttons")]
    public Button saveLoadoutButton;
    public Button clearButton;
    public Button backButton;

    [Header("Prefabs")]
    public GameObject componentButtonPrefab;

    // Currently selected components
    private ShipBodySO selectedBody;
    private ActivePerkSO selectedTier1Perk;
    private ActivePerkSO selectedTier2Perk;
    private ActivePerkSO selectedTier3Perk;
    private PassiveAbilitySO selectedPassive;
    private MoveTypeSO selectedMoveType;
    private MissilePresetSO selectedMissile;
    private string selectedSkinID;
    private string selectedColorSchemeID;

    void Start()
    {
        if (progressionManager == null)
            progressionManager = ProgressionManager.Instance;

        // Setup button listeners
        saveLoadoutButton.onClick.AddListener(OnSaveLoadout);
        clearButton.onClick.AddListener(OnClearSelections);
        backButton.onClick.AddListener(OnBack);

        // Populate component panels
        PopulateAllPanels();

        // Update UI
        UpdateSelectedComponentsDisplay();
        UpdatePreview();
    }

    /// <summary>
    /// Populates all component selection panels with unlocked items
    /// </summary>
    private void PopulateAllPanels()
    {
        PopulateShipBodies();
        PopulatePerks(1, tier1PerkPanel);
        PopulatePerks(2, tier2PerkPanel);
        PopulatePerks(3, tier3PerkPanel);
        PopulatePassives();
        PopulateMoveTypes();
        PopulateMissiles();
        // PopulateCosmetics(); // Add later
    }

    /// <summary>
    /// Populates ship body selection panel
    /// </summary>
    private void PopulateShipBodies()
    {
        ClearPanel(shipBodyPanel);

        var unlockedBodies = progressionManager.GetUnlockedShipBodies();

        foreach (var body in unlockedBodies)
        {
            CreateComponentButton(
                shipBodyPanel,
                body.bodyName,
                body.icon,
                () => OnSelectBody(body)
            );
        }
    }

    /// <summary>
    /// Populates perk selection panel for a specific tier
    /// </summary>
    private void PopulatePerks(int tier, Transform panel)
    {
        ClearPanel(panel);

        var unlockedPerks = progressionManager.GetUnlockedPerks(tier);

        foreach (var perk in unlockedPerks)
        {
            CreateComponentButton(
                panel,
                perk.perkName,
                perk.icon,
                () => OnSelectPerk(tier, perk)
            );
        }

        // Add "None" option
        CreateComponentButton(
            panel,
            "None",
            null,
            () => OnSelectPerk(tier, null)
        );
    }

    /// <summary>
    /// Populates passive selection panel
    /// </summary>
    private void PopulatePassives()
    {
        ClearPanel(passivePanel);

        var unlockedPassives = progressionManager.GetUnlockedPassives();

        foreach (var passive in unlockedPassives)
        {
            CreateComponentButton(
                passivePanel,
                passive.passiveName,
                passive.icon,
                () => OnSelectPassive(passive)
            );
        }

        // Add "None" option
        CreateComponentButton(
            passivePanel,
            "None",
            null,
            () => OnSelectPassive(null)
        );
    }

    /// <summary>
    /// Populates move type selection panel
    /// </summary>
    private void PopulateMoveTypes()
    {
        ClearPanel(moveTypePanel);

        // Get all move types (filter by archetype compatibility later)
        foreach (var moveType in progressionManager.allMoveTypes)
        {
            if (progressionManager.IsUnlocked(moveType))
            {
                CreateComponentButton(
                    moveTypePanel,
                    moveType.moveTypeName,
                    moveType.icon,
                    () => OnSelectMoveType(moveType)
                );
            }
        }
    }

    /// <summary>
    /// Populates missile selection panel
    /// </summary>
    private void PopulateMissiles()
    {
        ClearPanel(missilePanel);

        foreach (var missile in progressionManager.allMissiles)
        {
            if (progressionManager.IsUnlocked(missile))
            {
                CreateComponentButton(
                    missilePanel,
                    missile.missileName,
                    missile.icon,
                    () => OnSelectMissile(missile)
                );
            }
        }
    }

    /// <summary>
    /// Creates a component selection button
    /// </summary>
    private void CreateComponentButton(Transform parent, string label, Sprite icon, System.Action onClick)
    {
        GameObject buttonObj = Instantiate(componentButtonPrefab, parent);

        // Setup text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
            buttonText.text = label;

        // Setup icon
        Image buttonIcon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        if (buttonIcon != null && icon != null)
            buttonIcon.sprite = icon;

        // Setup click handler
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => onClick?.Invoke());
    }

    /// <summary>
    /// Clears all children from a panel
    /// </summary>
    private void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    #region SELECTION HANDLERS

    private void OnSelectBody(ShipBodySO body)
    {
        selectedBody = body;
        UpdateSelectedComponentsDisplay();
        UpdatePreview();

        // Re-populate perks/passives/move types to filter by archetype
        PopulatePerks(1, tier1PerkPanel);
        PopulatePerks(2, tier2PerkPanel);
        PopulatePerks(3, tier3PerkPanel);
        PopulatePassives();
        PopulateMoveTypes();
        PopulateMissiles();
    }

    private void OnSelectPerk(int tier, ActivePerkSO perk)
    {
        switch (tier)
        {
            case 1: selectedTier1Perk = perk; break;
            case 2: selectedTier2Perk = perk; break;
            case 3: selectedTier3Perk = perk; break;
        }
        UpdateSelectedComponentsDisplay();
        UpdatePreview();
    }

    private void OnSelectPassive(PassiveAbilitySO passive)
    {
        selectedPassive = passive;
        UpdateSelectedComponentsDisplay();
        UpdatePreview();
    }

    private void OnSelectMoveType(MoveTypeSO moveType)
    {
        selectedMoveType = moveType;
        UpdateSelectedComponentsDisplay();
        UpdatePreview();
    }

    private void OnSelectMissile(MissilePresetSO missile)
    {
        selectedMissile = missile;
        UpdateSelectedComponentsDisplay();
        UpdatePreview();
    }

    #endregion

    /// <summary>
    /// Updates the "Selected Components" display
    /// </summary>
    private void UpdateSelectedComponentsDisplay()
    {
        selectedBodyText.text = selectedBody != null ? selectedBody.bodyName : "None";
        selectedTier1PerkText.text = selectedTier1Perk != null ? selectedTier1Perk.perkName : "None";
        selectedTier2PerkText.text = selectedTier2Perk != null ? selectedTier2Perk.perkName : "None";
        selectedTier3PerkText.text = selectedTier3Perk != null ? selectedTier3Perk.perkName : "None";
        selectedPassiveText.text = selectedPassive != null ? selectedPassive.passiveName : "None";
        selectedMoveTypeText.text = selectedMoveType != null ? selectedMoveType.moveTypeName : "None";
        selectedMissileText.text = selectedMissile != null ? selectedMissile.missileName : "None";
    }

    /// <summary>
    /// Updates the preview panel with stats and validation
    /// </summary>
    private void UpdatePreview()
    {
        // Check if minimum requirements met
        if (selectedBody == null || selectedMoveType == null || selectedMissile == null)
        {
            previewStatsText.text = "Select Body, Move Type, and Missile to preview";
            validationErrorPanel.SetActive(false);
            saveLoadoutButton.interactable = false;
            return;
        }

        // Validate configuration
        string validationError = ValidateLoadout();
        if (!string.IsNullOrEmpty(validationError))
        {
            validationErrorPanel.SetActive(true);
            validationErrorText.text = validationError;
            saveLoadoutButton.interactable = false;
        }
        else
        {
            validationErrorPanel.SetActive(false);
            saveLoadoutButton.interactable = true;
        }

        // Display stats preview
        previewStatsText.text = GenerateStatsPreview();

        // Display ship icon
        if (selectedBody != null && selectedBody.icon != null)
            previewShipIcon.sprite = selectedBody.icon;
    }

    /// <summary>
    /// Validates the current loadout configuration
    /// </summary>
    private string ValidateLoadout()
    {
        if (selectedBody == null)
            return "ERROR: No ship body selected";

        if (selectedMoveType == null)
            return "ERROR: No move type selected";

        if (selectedMissile == null)
            return "ERROR: No missile selected";

        ShipArchetype archetype = selectedBody.archetype;

        // Check move type compatibility
        if (!selectedMoveType.CanBeUsedBy(archetype))
            return $"ERROR: {selectedMoveType.moveTypeName} cannot be used by {archetype}";

        // Check missile compatibility
        if (!selectedBody.CanUseMissileType(selectedMissile.missileType))
            return $"ERROR: {selectedBody.bodyName} cannot use {selectedMissile.missileType} missiles";

        // Check perk compatibility
        if (selectedTier1Perk != null && !selectedTier1Perk.CanBeUsedBy(archetype))
            return $"ERROR: {selectedTier1Perk.perkName} cannot be used by {archetype}";

        if (selectedTier2Perk != null && !selectedTier2Perk.CanBeUsedBy(archetype))
            return $"ERROR: {selectedTier2Perk.perkName} cannot be used by {archetype}";

        if (selectedTier3Perk != null && !selectedTier3Perk.CanBeUsedBy(archetype))
            return $"ERROR: {selectedTier3Perk.perkName} cannot be used by {archetype}";

        // Check passive compatibility
        if (selectedPassive != null && !selectedPassive.CanBeUsedBy(archetype))
            return $"ERROR: {selectedPassive.passiveName} cannot be used by {archetype}";

        return ""; // Valid!
    }

    /// <summary>
    /// Generates stats preview text
    /// </summary>
    private string GenerateStatsPreview()
    {
        string preview = $"<b>{selectedBody.bodyName}</b> ({selectedBody.archetype})\n\n";
        preview += $"<b>Base Stats (Level 1):</b>\n";
        preview += $"HP: {selectedBody.baseHealth:F0}\n";
        preview += $"Armor: {selectedBody.baseArmor:F0}\n";
        preview += $"Damage: ×{selectedBody.baseDamageMultiplier:F2}\n";
        preview += $"Action Points: {selectedBody.actionPointsPerTurn}\n\n";

        preview += $"<b>Movement:</b> {selectedMoveType.moveTypeName}\n";
        preview += $"<b>Missile:</b> {selectedMissile.missileName} ({selectedMissile.missileType})\n\n";

        if (selectedPassive != null)
            preview += $"<b>Passive:</b> {selectedPassive.passiveName}\n";

        preview += $"\n<b>Active Perks:</b>\n";
        if (selectedTier1Perk != null) preview += $"Tier 1: {selectedTier1Perk.perkName}\n";
        if (selectedTier2Perk != null) preview += $"Tier 2: {selectedTier2Perk.perkName}\n";
        if (selectedTier3Perk != null) preview += $"Tier 3: {selectedTier3Perk.perkName}\n";

        return preview;
    }

    /// <summary>
    /// Saves the current loadout
    /// </summary>
    private void OnSaveLoadout()
    {
        string loadoutName = string.IsNullOrEmpty(loadoutNameInput.text)
            ? $"{selectedBody.bodyName} Loadout"
            : loadoutNameInput.text;

        List<PassiveAbilitySO> passives = new List<PassiveAbilitySO>();
        if (selectedPassive != null)
            passives.Add(selectedPassive);

        CustomShipLoadout loadout = progressionManager.CreateCustomLoadout(
            loadoutName,
            selectedBody,
            selectedMoveType,
            selectedMissile,
            selectedTier1Perk,
            selectedTier2Perk,
            selectedTier3Perk,
            passives
        );

        if (loadout != null)
        {
            Debug.Log($"[ShipBuilderUI] Saved loadout: {loadoutName}");
            // Show success message, return to menu, etc.
        }
    }

    /// <summary>
    /// Clears all selections
    /// </summary>
    private void OnClearSelections()
    {
        selectedBody = null;
        selectedTier1Perk = null;
        selectedTier2Perk = null;
        selectedTier3Perk = null;
        selectedPassive = null;
        selectedMoveType = null;
        selectedMissile = null;
        selectedSkinID = null;
        selectedColorSchemeID = null;

        UpdateSelectedComponentsDisplay();
        UpdatePreview();
    }

    /// <summary>
    /// Returns to main menu
    /// </summary>
    private void OnBack()
    {
        // Load main menu scene or hide this panel
        gameObject.SetActive(false);
    }
}
