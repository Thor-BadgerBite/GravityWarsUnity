# Gravity Wars Unity Codebase Analysis Report
**Generated**: November 20, 2025
**Codebase Version**: Phase 4+ Architecture
**Status**: Critical Data Structure Conflicts Detected

---

## EXECUTIVE SUMMARY

The Gravity Wars codebase has **three conflicting player data structures** that are being used inconsistently throughout the system. This creates:
- **Compilation errors** in multiple files
- **Type mismatch issues** between data structures
- **Inconsistent field naming** across systems
- **Missing methods** that are called on wrong types

**Impact**: The project cannot compile cleanly. ProgressionManager and QuestService will fail to compile due to accessing non-existent fields/methods on PlayerProfileData.

---

## 1. PROJECT STRUCTURE

### Main Directories
```
/Assets
├── Progression System/        (Local/offline progression)
│   ├── PlayerAccountData.cs  (CONFLICTS HERE)
│   ├── ProgressionManager.cs
│   ├── SaveSystem.cs
│   └── UI/
├── Online/                     (Online multiplayer systems)
│   ├── PlayerProfileData.cs   (CONFLICTS HERE)
│   ├── AccountSystem.cs
│   ├── BattlePassSystem.cs
│   ├── MatchHistoryManager.cs
│   └── [10 other multiplayer files]
├── CloudSave/                  (Cloud persistence)
│   ├── SaveData.cs            (CONFLICTS HERE)
│   └── SaveManager.cs
├── Networking/
│   ├── NetworkManager.cs
│   ├── MatchManager.cs
│   ├── Services/
│   │   ├── CloudSaveService.cs
│   │   ├── QuestService.cs
│   │   └── [6 other services]
├── Quests/
├── Achievements/
├── Leaderboards/
├── Multiplayer/
└── [Other systems]
```

### Total C# Files: 158

---

## 2. CONFLICTING DATA STRUCTURES

### 2.1 PlayerAccountData
**Location**: `/Assets/Progression System/PlayerAccountData.cs`
**Purpose**: Local progression and account data (standalone/offline mode)
**Status**: Fully defined with constructor

**Key Fields**:
```csharp
public int saveVersion = 3;
public string playerID;
public string username;
public int accountLevel = 1;          // <-- KEY FIELD
public int accountXP = 0;             // <-- KEY FIELD
public int credits = 0;               // Earned through gameplay
public int gems = 0;                  // Premium currency
public int battlePassTier = 0;        // <-- EXCLUSIVE TO PlayerAccountData
public int battlePassXP = 0;          // <-- EXCLUSIVE TO PlayerAccountData
public bool hasPremiumBattlePass = false; // <-- EXCLUSIVE TO PlayerAccountData
public List<CustomShipLoadout> customShipLoadouts;
public List<ShipProgressionEntry> shipProgressionData;
public List<MatchResultData> recentMatches;
public List<QuestProgressData> activeQuests;
// ... more fields

public PlayerAccountData(string id, string name) { ... }
public void AddAccountXP(int amount) { ... }
public void AddShipXP(CustomShipLoadout loadout, int amount) { ... }
```

### 2.2 PlayerProfileData
**Location**: `/Assets/Online/PlayerProfileData.cs`
**Purpose**: Online multiplayer player profile (server synchronization)
**Status**: Missing constructor, missing fields

**Key Fields**:
```csharp
public string playerId;
public string username;
public int level = 1;                // <-- DIFFERENT NAME (accountLevel)
public int currentXP;                // <-- DIFFERENT NAME (accountXP)
public int xpForNextLevel = 1000;    // NEW FIELD
public int credits;                  // Same as PlayerAccountData
public int gems;                     // Same as PlayerAccountData
// MISSING: battlePassTier, battlePassXP, hasPremiumBattlePass
public int eloRating = 1200;
public int peakEloRating = 1200;
public List<CustomShipLoadout> customLoadouts;
public List<ShipProgressionEntry> shipProgressionData;
// ... more fields

// NO CONSTRUCTOR DEFINED!
// NO AddAccountXP() method!
// NO AddShipXP() method!
```

### 2.3 SaveData (In CloudSave)
**Location**: `/Assets/CloudSave/SaveData.cs`
**Purpose**: Cloud save structure (complex nested data)
**Status**: Completely different architecture

**Key Fields**:
```csharp
public string playerID;
public string saveVersion = "1.0.0";
public PlayerProfileData playerProfile;  // Contains PlayerProfileData
public CurrencyData currency;
public ProgressionData progression;      // Has separate level/experience fields!
// ... more fields

// Inside ProgressionData:
public int level = 1;
public int experience = 0;               // NOT accountXP or currentXP!
public int prestigeLevel = 0;
// ... more

// Inside CurrencyData:
public int softCurrency = 0;             // OLD NAMING!
public int hardCurrency = 0;             // OLD NAMING!
public int premiumCurrency = 0;
```

---

## 3. CRITICAL CONFLICTS & COMPILATION ERRORS

### 3.1 ProgressionManager.cs - MAJOR CONFLICT

**Location**: `/Assets/Progression System/ProgressionManager.cs`

**Problem**: Declares PlayerProfileData but uses PlayerAccountData fields

```csharp
// Line 13:
public PlayerProfileData currentPlayerData;  // <-- Wrong type!

// Line 73:
Debug.Log($"[ProgressionManager] Loaded account: {currentPlayerData.username} (Level {currentPlayerData.accountLevel})");
// ERROR: PlayerProfileData has .level, NOT .accountLevel

// Line 87:
currentPlayerData = new PlayerProfileData(playerID, displayName);
// ERROR: PlayerProfileData has NO constructor!

// Line 219:
if (currentPlayerData.hasPremiumBattlePass)  // ERROR: Field doesn't exist!

// Line 228:
currentPlayerData.AddAccountXP(totalAccountXP);  // ERROR: Method doesn't exist!

// Line 234:
currentPlayerData.AddShipXP(usedLoadout, totalShipXP);  // ERROR: Method doesn't exist!

// Line 238:
currentPlayerData.battlePassXP += totalAccountXP;  // ERROR: Field doesn't exist!

// Line 259-269:
int xpForNextLevel = 1000 + (currentPlayerData.accountLevel * 500);  // ERROR: Field doesn't exist!
while (currentPlayerData.accountXP >= xpForNextLevel && currentPlayerData.accountLevel < 50) { ... }
currentPlayerData.accountLevel++;
```

**Impact**: ProgressionManager will NOT COMPILE

### 3.2 QuestService.cs - SECONDARY CONFLICT

**Location**: `/Assets/Networking/Services/QuestService.cs` (Line 433)

```csharp
progressionManager.currentPlayerData.AddAccountXP(quest.accountXPReward);
// ERROR: PlayerProfileData doesn't have AddAccountXP() method
```

**Impact**: QuestService completion will NOT COMPILE

### 3.3 SaveSystem.cs - TYPE MISMATCH

**Location**: `/Assets/Progression System/SaveSystem.cs`

```csharp
// Line 31: Takes PlayerProfileData
public static void SavePlayerData(PlayerProfileData data)

// Line 46: Takes PlayerProfileData
public static void SavePlayerDataLocal(PlayerProfileData data)

// Line 115-118: Returns PlayerProfileData
public static PlayerProfileData LoadPlayerData()
{
    return LoadPlayerDataLocal();
}

// But SaveData.cs has its own complete structure!
// SaveManager.cs manages SaveData, not PlayerProfileData
```

**Issue**: SaveSystem and SaveManager are handling different data structures. SaveSystem handles PlayerProfileData, but SaveData is a more comprehensive structure used by SaveManager.

---

## 4. FIELD NAMING INCONSISTENCIES

### 4.1 Account/Player Level
| System | Field Name | Type |
|--------|-----------|------|
| PlayerAccountData | `accountLevel` | int (1-50) |
| PlayerProfileData | `level` | int (1-100+) |
| SaveData (ProgressionData) | `level` | int |
| ProgressionManager | Expects `accountLevel` | ❌ MISMATCH |

### 4.2 Player Experience/XP
| System | Field Name | Type |
|--------|-----------|------|
| PlayerAccountData | `accountXP` | int |
| PlayerProfileData | `currentXP` | int |
| SaveData (ProgressionData) | `experience` | int |
| ProgressionManager | Expects `accountXP` | ❌ MISMATCH |

### 4.3 Currency Naming
| System | Soft Currency | Hard Currency |
|--------|---------------|---------------|
| PlayerAccountData | `credits` | `gems` |
| PlayerProfileData | `credits` | `gems` |
| SaveData (CurrencyData) | `softCurrency` | `hardCurrency` |
| BattlePassData | `softCurrencyAmount` | `hardCurrencyAmount` |
| QuestDataSO | `softCurrencyReward` | `hardCurrencyReward` |
| AchievementDataSO | `softCurrencyReward` | `hardCurrencyReward` |
| NetworkGameManager | `softCurrency` / `hardCurrency` | ❌ OLD NAMING |

### 4.4 Battle Pass Fields
| Field | PlayerAccountData | PlayerProfileData | SaveData |
|-------|-------------------|-------------------|----------|
| `battlePassTier` | ✓ Present | ✗ Missing | ? |
| `battlePassXP` | ✓ Present | ✗ Missing | ? |
| `hasPremiumBattlePass` | ✓ Present | ✗ Missing | ? |
| `currentSeasonID` | ✓ Present | ? | ? |

---

## 5. CORE GAME SYSTEMS

### 5.1 Progression System (Account XP)
- **Uses**: PlayerAccountData (intended), but ProgressionManager declares PlayerProfileData
- **Issues**: Type mismatch, field name conflicts
- **Controls**: Level 1-50, Account XP, Battle Pass progression
- **Formula**: `XP = 1000 + (level × 500)`

### 5.2 Ship Progression System
- **Structure**: CustomShipLoadout + ShipProgressionEntry
- **Shared by**: PlayerAccountData AND PlayerProfileData (duplicate!)
- **Level**: 1-20 per ship loadout
- **Formula**: `XP = 200 + (75 × Level²)`

### 5.3 Battle Pass System
- **Location**: `/Assets/Online/BattlePassSystem.cs`
- **Tiers**: 25 levels per season
- **Data**: Only stored in PlayerAccountData
- **Issue**: ProgressionManager can't access these fields from PlayerProfileData

### 5.4 ELO/Competitive Ranking
- **Present in**: Both PlayerAccountData and PlayerProfileData
- **Fields**: eloRating, peakEloRating, currentRank, rankedMatchesPlayed, rankedMatchesWon, etc.
- **System**: CompetitiveRank enum (Bronze through Grandmaster)

### 5.5 Online Multiplayer
- **Manager**: LobbyManager, MatchManager, NetworkManager
- **Data**: Uses PlayerProfileData (but integration incomplete)
- **Matchmaking**: Skill-based (uses ELO)
- **Reward System**: Needs access to progression data (CONFLICTS HERE)

### 5.6 Quest System
- **Location**: `/Assets/Quests/QuestService.cs`
- **Data**: QuestProgressData (stored in PlayerAccountData)
- **Issue**: Line 433 tries to call AddAccountXP() on wrong type
- **Types**: Daily, Weekly, Seasonal, Event quests

### 5.7 Daily Quests
- **Location**: `/Assets/Quests/` folder
- **Data**: Tracked in QuestProgressData lists
- **Tracked by**: activeQuests, completedQuests lists
- **Integration**: Incomplete due to data structure conflicts

### 5.8 Achievement System
- **Location**: `/Assets/Achievements/`
- **Data**: AchievementDataSO ScriptableObjects
- **Tracking**: In PlayerProfileData (unlockedAchievements list)
- **Issue**: Uses softCurrencyReward/hardCurrencyReward (old naming)

### 5.9 Leaderboard System
- **Location**: `/Assets/Leaderboards/`
- **Service**: LeaderboardService.cs
- **Tracked by**: SaveData (LeaderboardStatsData)
- **Stats**: ELO rating, win streaks, accuracy, etc.

### 5.10 Custom Ship Builder
- **Location**: `/Assets/Online/CustomShipBuilder.cs`
- **Data**: Stores CustomShipLoadout configurations
- **Progression**: Links to ShipProgressionEntry
- **Shared**: Between PlayerAccountData and PlayerProfileData

---

## 6. FILES USING PLAYER DATA

### Using PlayerAccountData:
1. `/Assets/Progression System/PlayerAccountData.cs` (definition)
2. `/Assets/Progression System/ProgressionManager.cs` (attempts to, but uses wrong type!)
3. `/Assets/Progression System/UI/ProgressionUI.cs` (updates UI from data)
4. `/Assets/Online/MatchHistoryManager.cs` (uses PlayerProfileData, not PlayerAccountData)

### Using PlayerProfileData:
1. `/Assets/Online/PlayerProfileData.cs` (definition)
2. `/Assets/Online/AccountSystem.cs` (authentication and account creation)
3. `/Assets/Online/MatchHistoryManager.cs` (post-match rewards)
4. `/Assets/Online/CustomShipBuilder.cs`
5. `/Assets/Online/MatchmakingService.cs`
6. `/Assets/Progression System/ProgressionManager.cs` (WRONG TYPE DECLARATION!)
7. `/Assets/Progression System/SaveSystem.cs` (save/load operations)
8. `/Assets/Networking/Services/CloudSaveService.cs`
9. `/Assets/Quests/QuestService.cs`
10. `/Assets/Analytics/ProgressionManagerAnalytics.cs`

### Using SaveData:
1. `/Assets/CloudSave/SaveData.cs` (definition)
2. `/Assets/CloudSave/SaveManager.cs` (primary manager)
3. `/Assets/Networking/Services/CloudSaveService.cs`
4. `/Assets/Networking/Integration/ServiceIntegrationHelper.cs`
5. `/Assets/Debug/DebugSystemsUI.cs`

---

## 7. IDENTIFIED ISSUES SUMMARY

### 🔴 Critical (Won't Compile)
1. **ProgressionManager.cs**
   - Line 13: Declares `PlayerProfileData currentPlayerData`
   - Line 87: Tries to instantiate `new PlayerProfileData(playerID, displayName)` - NO CONSTRUCTOR
   - Lines 219-269: Accesses `.accountLevel`, `.accountXP`, `.battlePassTier`, `.battlePassXP`, `.hasPremiumBattlePass`, `.AddAccountXP()`, `.AddShipXP()` - FIELDS/METHODS DON'T EXIST
   - **Fix**: Change to `PlayerAccountData` OR add missing fields/methods to `PlayerProfileData`

2. **QuestService.cs**
   - Line 433: Calls `progressionManager.currentPlayerData.AddAccountXP()` - METHOD DOESN'T EXIST ON PlayerProfileData
   - **Fix**: Add AddAccountXP() to PlayerProfileData OR create wrapper method

### 🟠 High Priority (Runtime Issues)
1. **SaveSystem/SaveManager Mismatch**
   - SaveSystem.cs uses PlayerProfileData
   - SaveManager.cs uses SaveData (more comprehensive)
   - They're not in sync

2. **Currency Naming Inconsistency**
   - Some systems use credits/gems (new naming)
   - Others use softCurrency/hardCurrency (old naming)
   - Creates confusion and maintenance issues

3. **Battle Pass Missing from PlayerProfileData**
   - battlePassTier, battlePassXP, hasPremiumBattlePass only in PlayerAccountData
   - But ProgressionManager expects them on PlayerProfileData

### 🟡 Medium Priority (Design Issues)
1. **Three Separate Data Structures**
   - PlayerAccountData (offline)
   - PlayerProfileData (online)
   - SaveData (cloud)
   - Need unified approach or clear separation

2. **Missing Constructors**
   - PlayerProfileData has no constructor with (playerId, username)
   - ProgressionManager tries to create new instances

3. **Field Naming Inconsistency**
   - accountLevel vs level
   - accountXP vs currentXP
   - Makes code maintenance difficult

---

## 8. RECOMMENDATIONS FOR UNIFICATION

### Option 1: Unified Single Data Structure (RECOMMENDED)
Create a single `PlayerData` class that:
- Contains all fields from PlayerAccountData AND PlayerProfileData
- Has clear sections: Local Data, Online Data, Competitive Data, Progression Data
- Includes all necessary constructors and methods
- Deprecate PlayerAccountData and PlayerProfileData

**Pros**:
- Single source of truth
- No type mismatches
- Easier maintenance

**Cons**:
- Requires refactoring many files
- Larger data structure

### Option 2: Extend PlayerProfileData (ALTERNATIVE)
Make PlayerProfileData the master class and:
- Add missing fields: battlePassTier, battlePassXP, hasPremiumBattlePass
- Add missing constructor: `PlayerProfileData(string id, string name)`
- Add missing methods: AddAccountXP(), AddShipXP()
- Rename fields for consistency: level → accountLevel, currentXP → accountXP
- Deprecate PlayerAccountData

**Pros**:
- Fewer changes than Option 1
- Already being used in online systems

**Cons**:
- Breaks compatibility with Online systems that expect the current field names
- Less comprehensive than Option 1

### Option 3: Keep Separate but Sync
Maintain both PlayerAccountData and PlayerProfileData, but:
- Add synchronization layer between them
- Define clear usage boundaries (local vs online)
- Add mapping/conversion methods

**Pros**:
- Minimal immediate changes
- Gradual migration possible

**Cons**:
- Most complex to maintain
- Sync bugs likely
- Doesn't solve naming inconsistency

### Recommended Action Items:
1. **Immediate Fix** (to unblock compilation):
   - Change ProgressionManager to use PlayerAccountData
   - Add AddAccountXP() method to PlayerProfileData as wrapper
   - Fix QuestService to use correct method

2. **Short Term** (next sprint):
   - Unify currency naming (credits/gems everywhere)
   - Add missing constructors and methods
   - Standardize field names (level vs accountLevel)

3. **Long Term** (design cleanup):
   - Implement Option 1 or Option 2
   - Full refactoring of data systems
   - Comprehensive testing

---

## 9. FILES NEEDING IMMEDIATE FIXES

### Must Fix (Compilation Errors):
1. `/Assets/Progression System/ProgressionManager.cs` - Line 13, 87, 219-269
2. `/Assets/Networking/Services/QuestService.cs` - Line 433
3. `/Assets/Online/PlayerProfileData.cs` - Add missing constructor and methods

### Should Review:
4. `/Assets/Online/AccountSystem.cs` - Verify PlayerProfileData initialization
5. `/Assets/Online/MatchHistoryManager.cs` - Uses PlayerProfileData, check field access
6. `/Assets/Online/BattlePassSystem.cs` - References battle pass fields
7. `/Assets/CloudSave/SaveManager.cs` - Verify SaveData/PlayerProfileData handling
8. `/Assets/Progression System/SaveSystem.cs` - Type consistency
9. `/Assets/Networking/Services/CloudSaveService.cs` - Data conversion logic

### Currency Naming Updates Needed:
- `/Assets/Multiplayer/NetworkGameManager.cs` - Uses softCurrency/hardCurrency
- `/Assets/Progression System/BattlePassData.cs` - Uses softCurrencyAmount/hardCurrencyAmount
- `/Assets/Quests/QuestDataSO.cs` - Uses softCurrencyReward/hardCurrencyReward
- `/Assets/Achievements/AchievementDataSO.cs` - Uses softCurrencyReward/hardCurrencyReward
- All remaining ScriptableObject definitions

---

## 10. DOCUMENTATION REFERENCES

**Key Documentation Files**:
- `/Documentation/PHASE_4_ARCHITECTURE.md` - 1778 lines, Architecture blueprint
- `/Documentation/PHASE_4_PROGRESS.md` - 401 lines, Progress tracking
- `/Assets/Online/COMPLETE_PROGRESSION_GUIDE.md` - 767 lines, Progression guide
- `/Assets/Online/CUSTOM_SHIP_BUILDING_GUIDE.md` - 435 lines, Ship building
- `/Assets/Online/PROGRESSION_SYSTEM_GUIDE.md` - 535 lines

**Architecture Notes**:
- Phase 4+ uses Unity Gaming Services (Netcode, Relay, Cloud Save, etc.)
- Intended flow: Player → Progression Manager → Save Manager → Cloud Save
- Currently broken due to data structure conflicts

---

## 11. QUICK FIX CHECKLIST

- [ ] Add to PlayerProfileData:
  - [ ] Constructor: `PlayerProfileData(string id, string name)`
  - [ ] Method: `public void AddAccountXP(int amount)`
  - [ ] Method: `public void AddShipXP(CustomShipLoadout loadout, int amount)`
  - [ ] Field: `public int battlePassTier = 0`
  - [ ] Field: `public int battlePassXP = 0`
  - [ ] Field: `public bool hasPremiumBattlePass = false`

- [ ] Change ProgressionManager.cs line 13 from PlayerProfileData to PlayerAccountData OR create bridge

- [ ] Fix QuestService.cs line 433 to use correct method call

- [ ] Standardize all currency references to credits/gems (not softCurrency/hardCurrency)

- [ ] Update SaveData structure if it's meant to be primary

- [ ] Add unit tests for data persistence

---

## FILES & PATHS

**Data Structure Definitions**:
- `/home/user/GravityWarsUnity/Assets/Progression System/PlayerAccountData.cs`
- `/home/user/GravityWarsUnity/Assets/Online/PlayerProfileData.cs`
- `/home/user/GravityWarsUnity/Assets/CloudSave/SaveData.cs`

**Main Systems**:
- `/home/user/GravityWarsUnity/Assets/Progression System/ProgressionManager.cs`
- `/home/user/GravityWarsUnity/Assets/Progression System/SaveSystem.cs`
- `/home/user/GravityWarsUnity/Assets/CloudSave/SaveManager.cs`
- `/home/user/GravityWarsUnity/Assets/Online/AccountSystem.cs`

**Problem Files**:
- `/home/user/GravityWarsUnity/Assets/Progression System/ProgressionManager.cs` (CRITICAL)
- `/home/user/GravityWarsUnity/Assets/Networking/Services/QuestService.cs` (CRITICAL)
- `/home/user/GravityWarsUnity/Assets/Online/MatchHistoryManager.cs` (Check)
- `/home/user/GravityWarsUnity/Assets/Networking/Services/CloudSaveService.cs` (Check)

