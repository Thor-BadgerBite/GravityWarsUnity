using UnityEngine;
using UnityEngine.UI;

public class PerkManager : MonoBehaviour
{
    [Header("Perk Slots (Tier 1 / 2 / 3)")]
    public ActivePerkSO tier1Perk;
    public ActivePerkSO tier2Perk;
    public ActivePerkSO tier3Perk;

    // backing arrays
    private ActivePerkSO[] _soSlots      = new ActivePerkSO[3];
    private IActivePerk[]  _equipped     = new IActivePerk[3];
    private bool[]         _usedThisTurn = new bool[3];

    // the three UI icons you’ll hand in via SetIconSlots(...)
    private Image[]        _perkIcons;

    private PlayerShip     _ship;
    private int            _toggledSlot   = -1;
    private int            _lastShotsCount;

    // <-- add this:
    private PlayerShip.PlayerActionMode _lastMode;

    void Awake()
    {
        _ship = GetComponent<PlayerShip>();

        // Load perks from ship preset if available (NEW SYSTEM)
        if (_ship.shipPreset != null)
        {
            _soSlots[0] = _ship.shipPreset.tier1Perk;
            _soSlots[1] = _ship.shipPreset.tier2Perk;
            _soSlots[2] = _ship.shipPreset.tier3Perk;
            Debug.Log($"<color=cyan>[PerkManager] Loaded perks from ship preset: {_ship.shipPreset.shipName}</color>");
        }
        else
        {
            // Fallback to inspector fields (LEGACY SYSTEM - backward compatibility)
            _soSlots[0] = tier1Perk;
            _soSlots[1] = tier2Perk;
            _soSlots[2] = tier3Perk;
            Debug.LogWarning($"[PerkManager] No ship preset found, using inspector perk fields (old system).");
        }

        // instantiate the runtime perks
        for (int i = 0; i < 3; i++)
        {
            _equipped[i]     = _soSlots[i]?.CreatePerk();
            _usedThisTurn[i] = false;
        }

        // so we can detect when they actually fire
        _lastShotsCount = _ship.shotsThisRound;
        _lastMode       = _ship.currentMode;   // initialize it

    }

    void Update()
    {
        if (!_ship.controlsEnabled) return;

        // toggle keys 1/2/3
        if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ToggleSlot(2);

        // REMOVED: Shot detection (obsolete, now handled by ActivateToggledPerk + ConsumeToggledPerk)
        // The old approach detected shots too late (after missile spawned)

        // Refresh UI if the action mode changed
        if (_ship.currentMode != _lastMode)
        {
            _lastMode = _ship.currentMode;
            RefreshUI();
        }
    }

    /// <summary>
    /// Give me the 3 UI‐Image components so I can drive them.
    /// </summary>
    public void SetIconSlots(Image[] icons)
    {
        if (icons == null || icons.Length != 3)
        {
            Debug.LogError("PerkManager needs exactly 3 icon slots!");
            return;
        }
        _perkIcons = icons;
        
        RefreshUI();
    }

    private void ToggleSlot(int slot)
    {
        var so   = _soSlots[slot];
        var perk = _equipped[slot];

        // 1) no perk assigned
        // 2) ship not high enough level
        // 3) not otherwise valid (enough moves, correct mode, one‑per‑turn rules, etc.)
        if (so == null
        || _ship.shipLevel < so.minLevel
        || !perk.CanActivate(_ship))
            return;

        // toggle on/off
        _toggledSlot = (_toggledSlot == slot) ? -1 : slot;
        RefreshUI();
    }

    /// <summary>
    /// NEW: Activates the toggled perk to set flags BEFORE firing.
    /// Called from PlayerShip.FireMissile() BEFORE spawning the missile.
    /// Does NOT deduct action points yet - that happens in ConsumeToggledPerk().
    /// </summary>
    public void ActivateToggledPerk()
    {
        if (_toggledSlot < 0) return;

        var so = _soSlots[_toggledSlot];
        var perk = _equipped[_toggledSlot];

        // Final validation before activation
        if (so == null || perk == null || !perk.CanActivate(_ship))
        {
            Debug.LogWarning($"[PerkManager] Cannot activate perk in slot {_toggledSlot}!");
            _toggledSlot = -1;
            RefreshUI();
            return;
        }

        // Activate the perk (sets flags like nextMultiEnabled, etc.)
        Debug.Log($"[PerkManager] Activating {so.perkName} BEFORE firing missile");
        perk.Activate(_ship);
        _usedThisTurn[_toggledSlot] = true;
    }

    /// <summary>
    /// NEW: Consumes the action points and clears toggle AFTER firing.
    /// Called from GameManager.PlayerActionUsed() AFTER the missile spawns.
    /// </summary>
    public void ConsumeToggledPerk()
    {
        if (_toggledSlot < 0) return;

        // Deduct action points
        int cost = _equipped[_toggledSlot].Cost;
        _ship.movesRemainingThisRound = Mathf.Max(0, _ship.movesRemainingThisRound - cost);

        Debug.Log($"[PerkManager] Consumed {cost} action point(s) for perk in slot {_toggledSlot}");

        // Clear the toggle
        _toggledSlot = -1;

        // Update UI
        GameManager.Instance.UpdateFightingUI_AtRoundStart();
        RefreshUI();
    }

    private void HandleShotFired()
    {
        // OBSOLETE: This method is no longer used.
        // Perk activation split into ActivateToggledPerk() and ConsumeToggledPerk()
        Debug.LogWarning("[PerkManager] HandleShotFired() is obsolete!");
    }

    public void RefreshUI()
    {
        if (_perkIcons == null) return;

        for (int i = 0; i < 3; i++)
        {
            var so   = _soSlots[i];
            var icon = _perkIcons[i];

            if (so == null)
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            icon.gameObject.SetActive(true);
            icon.sprite = so.icon;

            // locked by level
            if (_ship.shipLevel < so.minLevel)
            {
                icon.color = Color.black;   // or a “locked” tint
            }
            // toggle‑on state
            else if (_toggledSlot == i)
            {
                icon.color = Color.yellow;
            }
            // can’t activate (cost/mode/rule)
            else if (!(_equipped[i]?.CanActivate(_ship) ?? false))
            {
                icon.color = Color.gray;
            }
            // ready
            else
            {
                icon.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Call this at the start of each new turn to clear usage flags.
    /// </summary>
    public void ResetPerTurn()
    {
        for (int i = 0; i < _usedThisTurn.Length; i++)
        {
            // only Tier‑2/3, leave Tier‑1 unlimited:
            if (_soSlots[i] != null && _soSlots[i].tier > 1)
                _usedThisTurn[i] = false;
        }
        _toggledSlot = -1;
        RefreshUI();
    }

    public void OnActionExecuted()
    {
        // NEW: Just consume the action points and clear toggle
        // (Perk was already activated in ActivateToggledPerk() before firing)
        ConsumeToggledPerk();
    }
}
