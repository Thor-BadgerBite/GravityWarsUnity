# Active Perks System - Bug Fixes

## Date: 2025-11-17

## Bugs Fixed

### 🐛 Bug #1: Perk Persists to Next Round
**Symptom:** When you arm a perk in round N and fire, then in round N+1 you fire normally (no perk), the perk from round N still activates.

**Root Cause:**
The perk activation was happening AFTER the missile was spawned:
1. Press '1' to arm perk → `_toggledSlot = 0`
2. Press Space → `FireMissile()` is called
3. `FireMissile()` checks `if (nextMultiEnabled)` → **FALSE** (not set yet!)
4. Fires NORMAL missile
5. At END of `FireMissile()`: calls `PlayerActionUsed()`
6. `PlayerActionUsed()` → `OnActionExecuted()` → `HandleShotFired()`
7. `HandleShotFired()` → `perk.Activate()` → sets `nextMultiEnabled = TRUE`
   - **TOO LATE!** Missile already spawned!
8. Next round: Checks `nextMultiEnabled` → TRUE → fires special missile

**Fix:**
- Split perk handling into two phases:
  1. `ActivateToggledPerk()` - Called at START of `FireMissile()`, sets the flags BEFORE checking them
  2. `ConsumeToggledPerk()` - Called at END via `PlayerActionUsed()`, deducts action points and clears toggle

**Files Modified:**
- `Assets/+Active Perks+/PerkManager.cs` - Added `ActivateToggledPerk()` and `ConsumeToggledPerk()` methods
- `Assets/PlayerShip.cs` - Added perk activation call at START of `FireMissile()`

---

### 🐛 Bug #2: Double Action Point Deduction
**Symptom:** Firing with a Tier-1 perk (cost 1) deducts 2 action points instead of 1.

**Root Cause:**
Action points were being deducted TWICE:
1. First in `PerkManager.HandleShotFired()`: `ship.movesRemainingThisRound -= cost`
2. Then AGAIN in each perk's `Activate()` method: `ship.movesRemainingThisRound -= Cost`

**Fix:**
Removed the duplicate deduction from ALL perk `Activate()` methods. Only `PerkManager.ConsumeToggledPerk()` should deduct action points.

**Files Modified:**
- `Assets/+Active Perks+/MultiMissilePerk.cs`
- `Assets/+Active Perks+/ExplosiveMissilePerk.cs`
- `Assets/+Active Perks+/ClusterMissilePerk.cs`
- `Assets/+Active Perks+/PusherMissilePerk.cs`
- `Assets/+Active Perks+/MissileBarragePerk.cs`

**Changes:**
- Removed: `ship.movesRemainingThisRound -= Cost;`
- Removed: `GameManager.Instance.UpdateFightingUI_AtRoundStart();`
- Added: Comments explaining the fix

---

## New Flow (Fixed)

### Correct Perk Activation Flow:
```
1. Player presses '1' → PerkManager.ToggleSlot(0) → _toggledSlot = 0
2. Player presses Space → PlayerShip.Update() detects input
3. PlayerShip.FireMissile() is called:
   a. Calls PerkManager.ActivateToggledPerk() ← NEW!
      - Validates the perk
      - Calls perk.Activate(ship)
      - Sets flags (nextMultiEnabled = true, etc.)
      - Does NOT deduct action points yet
   b. Checks flags (nextMultiEnabled, etc.)
   c. Spawns missiles based on flags
   d. At END: Calls GameManager.PlayerActionUsed()
4. GameManager.PlayerActionUsed():
   a. Calls PerkManager.OnActionExecuted()
   b. OnActionExecuted() calls ConsumeToggledPerk() ← NEW!
      - Deducts action points ONCE
      - Clears _toggledSlot
      - Updates UI
   c. Ends the turn
```

---

## Testing Checklist

- [ ] Arm a Tier-1 perk (e.g., Multi-Missile) and fire
  - Verify: 3 missiles spawn immediately
  - Verify: Only 1 action point is deducted
- [ ] Fire again next round WITHOUT arming any perk
  - Verify: Normal missile fires (not special)
- [ ] Test with all perk types:
  - [ ] Multi-Missile
  - [ ] Cluster Missile (requires pressing Space mid-flight)
  - [ ] Explosive Missile
  - [ ] Pusher Missile
  - [ ] Missile Barrage (fires 4 missiles over time)
- [ ] Test with Tier-2 and Tier-3 perks
  - Verify correct action point deduction (2 for Tier-2, 3 for Tier-3)

---

## Technical Notes

### Why the timing matters:
The flags (`nextMultiEnabled`, `nextExplosiveEnabled`, etc.) must be set BEFORE `FireMissile()` checks them. Previously, they were set AFTER in the cleanup phase, which meant the next shot would incorrectly use them.

### Why we split the methods:
We can't just move `PlayerActionUsed()` to the start because it:
1. Ends the turn (sets `isTurnActive = false`)
2. Disables controls
3. Starts missile flight coroutine

We need perk activation BEFORE spawning, but turn ending AFTER spawning.

### Obsolete code kept for reference:
- `PerkManager.HandleShotFired()` - Now logs a warning if called
- `PerkManager.Update()` shot detection logic - Removed (was too late in the flow)

---

## Debug Logs Added

All perk `Activate()` methods now log with their class name:
- `[MultiMissilePerk] Perk activated: spread ±5°, damageFactor 0.75`
- `[ExplosiveMissilePerk] Perk activated: blast radius 4, damage factor 0.5`
- etc.

PerkManager logs:
- `[PerkManager] Activating {perkName} BEFORE firing missile`
- `[PerkManager] Consumed {cost} action point(s) for perk in slot {slot}`

These help trace the exact activation timing.

---

## Future Improvements (Optional)

1. **Validation:** Add more robust checks in `ActivateToggledPerk()` to prevent edge cases
2. **UI Feedback:** Add visual/audio feedback when a perk activates
3. **Cooldowns:** Consider adding per-perk cooldowns instead of just "once per turn"
4. **Undo:** Allow players to un-toggle a perk before firing

---

## Author Notes

The core issue was a timing problem in the game loop. The perk system tried to detect when shots were fired using `shotsThisRound` counter, but by the time it detected the increment, the missile was already spawned. The solution was to make perk activation explicit and synchronous at the correct point in the flow.

The double deduction was a simpler bug - just redundant code that wasn't caught during initial implementation because the perks likely worked fine in isolation but not with the manager.
