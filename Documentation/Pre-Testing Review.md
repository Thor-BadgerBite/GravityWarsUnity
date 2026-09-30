# 🔍 PRE-TESTING REVIEW - CRITICAL ISSUES FOUND

## ⚠️ CRITICAL ISSUE #1: Dual Player Data Systems

**Problem:** The project has TWO separate player data structures that don't integrate:

### PlayerProfileData (Assets/Online/PlayerProfileData.cs)
**Used by:**
- AccountSystem
- MatchHistoryManager
- CloudSaveService
- **NEW: ProgressionSystem** ✅
- **NEW: ExtendedProgressionData** ✅
- **NEW: BattlePassSystem** ✅
- **NEW: MissileRetrofitSystem** ✅
- **NEW: CustomShipBuilder** ✅

**Fields:**
- `unlockedShipBodies` (List<string>)
- `unlockedPassives` (List<string>)
- `unlockedActives` (List<string>)
- `unlockedMissiles` (List<string>)
- `unlockedSkins` (List<string>)
- `customLoadouts` (List<CustomShipLoadout>)

### PlayerAccountData (Assets/Progression System/PlayerAccountData.cs)
**Used by:**
- SaveSystem
- ProgressionManager
- BattlePassUI
- ProgressionUI
- CosmeticsSystem
- QuestService
- AnalyticsService

**Fields:**
- `unlockedShipBodyIDs` (List<string>) ❌ Different name!
- `unlockedPassiveIDs` (List<string>) ❌ Different name!
- `unlockedTier1PerkIDs`, `unlockedTier2PerkIDs`, `unlockedTier3PerkIDs` ❌ Separate lists!
- `unlockedMissileIDs` (List<string>) ❌ Different name!
- `unlockedSkinIDs` (List<string>) ❌ Different name!
- `customShipLoadouts` (List<CustomShipLoadout>) ⚠️ Same name, different list!
- `shipProgressionData` (List<ShipProgressionEntry>) ✅ Ship XP tracking!
- `battlePassTier`, `battlePassXP`, `hasPremiumBattlePass` ✅ Battle pass tracking!

### 🔴 IMPACT:

**Our new progression systems update `PlayerProfileData`, but:**
- UI systems read from `PlayerAccountData`
- SaveSystem saves `PlayerAccountData`
- ProgressionManager manages `PlayerAccountData`

**Result:** Our unlocks won't show in the UI and won't persist!

### ✅ SOLUTION OPTIONS:

**Option 1: Merge Into PlayerProfileData (Recommended)**
- Remove `PlayerAccountData`
- Migrate all systems to use `PlayerProfileData`
- Add missing fields from `PlayerAccountData` to `PlayerProfileData`:
  - `shipProgressionData` (for ship XP tracking)
  - Battle pass fields (if not using BattlePassSystem.cs)

**Option 2: Keep Both, Add Synchronization**
- Create sync method that copies between both structures
- Call sync after every unlock/level-up
- More complex, prone to bugs

**Option 3: Use PlayerAccountData for Everything**
- Update our new systems to use `PlayerAccountData`
- Rename fields to match (`unlockedShipBodyIDs` etc.)
- Update MatchHistoryManager and AccountSystem

---

## ⚠️ CRITICAL ISSUE #2: Starter Ship & Initial Unlocks

**Problem:** No clear initialization for new players!

### What happens when a new player creates an account?

**Current State:**
- `PlayerProfileData` has no initialization method
- `PlayerAccountData` has `InitializeDefaultUnlocks()` but it's empty
- `ProgressionSystem.STARTER_SHIP = "starter_ship"` is defined but never given to player

### ✅ SOLUTION:

Create account initialization that gives:
```csharp
// Level 1 starter content
- Starter Ship: "starter_ship" (All-Around)
- Standard Mk-I Missile (level 1 unlock)
- Custom Slot #1 (level 1 unlock)
- All-Around ship class (level 1 unlock)
```

**Where to add this:**
- In `AccountSystem.OnRegistrationSuccess()` or
- In `PlayerProfileData` constructor or
- In `MatchHistoryManager.CheckLevelUp()` when creating fresh profile

---

## ⚠️ ISSUE #3: Ship Progression (XP/Leveling) Not Implemented

**Problem:** Custom ships should level up, but there's no XP tracking!

### PlayerAccountData Has This:
```csharp
public List<ShipProgressionEntry> shipProgressionData = new List<ShipProgressionEntry>();
```

### PlayerProfileData Doesn't Have This:
Custom ships are created with no level/XP tracking mechanism.

### ✅ SOLUTION:

**Option A:** Add to PlayerProfileData:
```csharp
public List<ShipProgressionEntry> shipProgressionData = new List<ShipProgressionEntry>();
```

**Option B:** Add XP tracking to CustomShipLoadout directly (but PlayerAccountData version doesn't have this)

**Option C:** Use PlayerAccountData's shipProgressionData system

---

## ⚠️ ISSUE #4: Battle Pass XP Source Missing

**Problem:** Battle pass has XP tracking but no XP source!

### BattlePassSystem.cs has:
```csharp
public void AddBattlePassXP(int xp) { /* adds XP */ }
```

### But WHERE does battle pass XP come from?

**Missing:**
- Match completion XP for battle pass
- Daily quest XP for battle pass
- Integration with MatchHistoryManager

### ✅ SOLUTION:

Add to `MatchHistoryManager.CalculateRewards()`:
```csharp
// Add battle pass XP
int battlePassXP = isWinner ? 100 : 50;
BattlePassSystem.Instance.AddBattlePassXP(battlePassXP);
```

Or create separate system for battle pass XP.

---

## ⚠️ ISSUE #5: Missile Selection Before Matches Not Implemented

**Problem:** System says "missiles selected before matches" but there's no implementation!

### What's Missing:
- Pre-match lobby UI for missile selection
- Validation that missile is compatible with selected ship
- Passing missile choice to match

### Current State:
- `MissileRetrofitSystem.IsMissileCompatible()` exists ✅
- Player has `unlockedMissiles` list ✅
- `CustomShipLoadout.equippedMissileName` field exists ✅
- NO system to populate `equippedMissileName` ❌

### ✅ SOLUTION:

Create `MissileSelectionUI.cs`:
```csharp
// Shows compatible missiles for selected ship
// Updates CustomShipLoadout.equippedMissileName
// Called before entering match
```

---

## ⚠️ ISSUE #6: Move Types Referenced But Not Explained

**Problem:** `CustomShipLoadout` has `moveTypeName` field, but we never discussed move types!

```csharp
public string moveTypeName;  // MoveTypeSO.name
```

### Questions:
1. What are move types?
2. How do they relate to ship bodies?
3. Should they be unlockable?
4. Are they separate from ship bodies or part of them?

### ✅ SOLUTION:

**Clarify with user:**
- Are move types a separate system?
- Or should this be removed/merged with ship bodies?

For now, we set it to empty string, but this might cause issues.

---

## ⚠️ ISSUE #7: Field Name Inconsistencies

### PlayerProfileData vs PlayerAccountData:

| Feature | PlayerProfileData | PlayerAccountData |
|---------|-------------------|-------------------|
| Ship Bodies | `unlockedShipBodies` | `unlockedShipBodyIDs` |
| Passives | `unlockedPassives` | `unlockedPassiveIDs` |
| Actives | `unlockedActives` | `unlockedTier1PerkIDs`<br>`unlockedTier2PerkIDs`<br>`unlockedTier3PerkIDs` |
| Missiles | `unlockedMissiles` | `unlockedMissileIDs` |
| Skins | `unlockedSkins` | `unlockedSkinIDs` |
| Credits | `credits` | `softCurrency` |
| Gems | `gems` | `hardCurrency` |

**Impact:** Code confusion, potential bugs when migrating between systems.

### ✅ SOLUTION:

Choose ONE naming convention and stick to it across both systems.

---

## ✅ WORKING FEATURES

These are fully implemented and ready:

### 1. Account Level Progression (Levels 1-100)
- ✅ XP requirements calculated
- ✅ Level-up rewards (credits, gems)
- ✅ 40 prebuild ships with unlock schedule
- ✅ 16 ship bodies with unlock schedule
- ✅ 30 passives with archetype restrictions
- ✅ 20 actives with tier system
- ✅ UnlockType enum expanded
- ✅ ProgressionSystem.GetLevelUpReward() returns all unlocks
- ✅ MatchHistoryManager.ApplyUnlock() handles all types

### 2. Battle Pass System
- ✅ 25 level structure
- ✅ Free track rewards (25 items)
- ✅ Premium track rewards (25 items)
- ✅ XP tracking per season
- ✅ Reward claiming system
- ⚠️ Missing: XP source (where does battle pass XP come from?)

### 3. Missile Retrofit System
- ✅ 17 missiles with unlock levels
- ✅ Type-based compatibility (Standard, Light, Heavy, etc.)
- ✅ IsMissileCompatible() validation
- ✅ All-Around ships can use any missile
- ⚠️ Missing: Pre-match missile selection UI

### 4. Custom Ship Building
- ✅ Build validation (body + passive + 3 actives)
- ✅ Archetype restriction checking
- ✅ Tier requirement checking (one from each tier)
- ✅ CustomShipBuilder.CreateCustomShip()
- ✅ CustomShipBuilder.DeleteShip()
- ✅ Helper methods for UI (GetActivesByTier, GetCompatiblePassives)
- ⚠️ Missing: Ship XP/leveling tracking
- ⚠️ Missing: Move type handling

### 5. Documentation
- ✅ COMPLETE_PROGRESSION_GUIDE.md (900+ lines)
- ✅ CUSTOM_SHIP_BUILDING_GUIDE.md (800+ lines)
- ✅ Level-by-level unlock schedule
- ✅ Example builds
- ✅ Code examples

---

## 🔧 RECOMMENDED FIXES BEFORE TESTING

### Priority 1: Critical (Must Fix)

1. **Resolve Dual Player Data System**
   - Choose: Merge into PlayerProfileData OR use PlayerAccountData
   - Update all systems to use ONE data structure
   - Estimated time: 2-3 hours

2. **Add Starter Ship Initialization**
   - Give new players starter content at level 1
   - Estimated time: 30 minutes

3. **Add Ship Progression Tracking**
   - Implement ship XP/leveling system
   - Track XP per custom ship
   - Estimated time: 1-2 hours

### Priority 2: Important (Should Fix)

4. **Add Battle Pass XP Source**
   - Integrate with match completion
   - Add to MatchHistoryManager
   - Estimated time: 30 minutes

5. **Clarify/Remove Move Types**
   - Either implement properly or remove field
   - Estimated time: Variable (depends on requirements)

### Priority 3: Polish (Can Fix Later)

6. **Create Missile Selection System**
   - Pre-match UI for selecting missiles
   - Estimated time: 2-3 hours (UI work)

7. **Standardize Field Names**
   - Consistent naming across both systems
   - Estimated time: 1 hour

---

## 📋 TESTING CHECKLIST

Once fixes are applied, test:

### Account Creation & Initialization
- [ ] New account receives starter ship
- [ ] New account receives starter missile
- [ ] New account at level 1 with correct XP
- [ ] Custom slot #1 available from start

### Progression System
- [ ] Playing matches gives XP
- [ ] Leveling up triggers rewards
- [ ] Credits/gems awarded correctly
- [ ] Unlocks applied to player account

### Ship Unlocks
- [ ] Prebuild ships unlock at correct levels
- [ ] Ship bodies unlock at correct levels
- [ ] Passives unlock at correct levels
- [ ] Actives unlock at correct levels
- [ ] Missiles unlock at correct levels

### Custom Ship Building
- [ ] Can select ship body
- [ ] Only compatible passives shown for selected body
- [ ] Must select one active from each tier
- [ ] Validation catches incompatible builds
- [ ] Custom ship created successfully
- [ ] Custom ship appears in loadout list

### Ship Deletion
- [ ] Can delete prebuild ships
- [ ] Can delete custom ships
- [ ] Confirmation warning shown
- [ ] Progress lost permanently
- [ ] Slot freed up

### Battle Pass
- [ ] Battle pass XP awarded after matches
- [ ] Leveling up battle pass works
- [ ] Free rewards claimable
- [ ] Premium rewards locked without purchase
- [ ] Premium pass purchase unlocks rewards

### Missile System
- [ ] Missiles unlock at correct levels
- [ ] Can see unlocked missiles
- [ ] Compatibility checking works
- [ ] (Future) Pre-match missile selection

---

## 💡 QUESTIONS FOR USER

1. **Which player data system should we use?**
   - Keep PlayerProfileData and migrate everything to it?
   - Keep PlayerAccountData and update our new systems?
   - Merge both?

2. **What are "Move Types"?**
   - Are they a separate unlockable system?
   - Or part of ship bodies?
   - Should we implement or remove?

3. **Ship Leveling Details:**
   - How much XP per match should ships gain?
   - What are the benefits of leveling up ships?
   - Max ship level?

4. **Battle Pass XP:**
   - How much XP per match (win/loss)?
   - XP from quests/challenges?
   - Flat amount or performance-based?

5. **Testing Approach:**
   - Fix critical issues first, then test?
   - Or test now to see what breaks?

---

## 📊 SUMMARY

**Systems Created:** 7 major systems (Progression, Battle Pass, Missiles, Custom Ships, etc.)

**Lines of Code:** ~3,500 lines

**Documentation:** ~1,700 lines

**Files Created:** 7 new files

**Files Modified:** 4 existing files

**Critical Issues:** 7 found

**Status:** ⚠️ **NOT READY FOR TESTING** - Critical issues must be resolved first

**Recommendation:** Address Priority 1 issues before pulling into testing branch.
