# Unity Setup Guide - Phase 4+ Systems
## Complete Manual Setup Instructions

**Last Updated:** Phase 4.6 Complete
**Estimated Setup Time:** 2-3 hours

This document provides step-by-step instructions for setting up all Phase 4+ systems in Unity Editor.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Phase 4.1 - Foundation Services](#phase-41---foundation-services)
3. [Phase 4.2 - Multiplayer Foundation](#phase-42---multiplayer-foundation)
4. [Phase 4.3 - Analytics & Economy](#phase-43---analytics--economy)
5. [Phase 4.4 - Quest System](#phase-44---quest-system)
6. [Phase 4.5 - Achievement System](#phase-45---achievement-system)
7. [Phase 4.6 - Leaderboard System](#phase-46---leaderboard-system)
8. [Final Integration](#final-integration)
9. [Testing Checklist](#testing-checklist)

---

## Prerequisites

### Required Packages
1. **TextMeshPro** - Import TMP Essentials when prompted
2. **Unity UI** - Should be included by default

### Project Structure
Ensure you have these existing components (from earlier phases):
- `GameManager` - Your main game controller
- `ProgressionManager` - Your progression system
- `PlayerAccountData` - Your player data structure
- Main game scene

---

## Phase 4.1 - Foundation Services

### Step 1: Create Service Locator

1. **In Hierarchy:** Create empty GameObject
   - Name: `[ServiceLocator]`
   - Add Component: `ServiceLocator`
   - ✅ Check: "Don't Destroy On Load" is enabled

2. **Create Child Services:**
   - Right-click `[ServiceLocator]` → Create Empty
   - Name: `[CloudSaveService]`
   - Add Component: `CloudSaveService`
   - Configuration in Inspector:
     - Enable Debug Logging: ✅ (for testing)
     - Auto Save Interval: `300` (5 minutes)
     - Max Retry Attempts: `3`

3. **Add Authentication Service:**
   - Right-click `[ServiceLocator]` → Create Empty
   - Name: `[AuthenticationService]`
   - Add Component: `AuthenticationService`
   - Configuration:
     - Enable Debug Logging: ✅
     - Enable Anonymous Login: ✅
     - Auto Login On Start: ✅

4. **Add Network Connection Manager:**
   - Right-click `[ServiceLocator]` → Create Empty
   - Name: `[NetworkConnectionManager]`
   - Add Component: `NetworkConnectionManager`
   - Configuration:
     - Enable Debug Logging: ✅
     - Connection Timeout: `10`
     - Max Reconnect Attempts: `3`
     - Reconnect Delay: `2`

**Result:** You should have:
```
[ServiceLocator]
├── [CloudSaveService]
├── [AuthenticationService]
└── [NetworkConnectionManager]
```

---

## Phase 4.2 - Multiplayer Foundation

### Step 2: Create Multiplayer Services

1. **Add Matchmaking Service:**
   - Create empty GameObject in scene root
   - Name: `[MatchmakingService]`
   - Add Component: `MatchmakingService`
   - Configuration:
     - Enable Debug Logging: ✅
     - Default Queue: `"default-queue"`
     - Matchmaking Timeout: `60`

2. **Add Relay Service:**
   - Create empty GameObject in scene root
   - Name: `[RelayService]`
   - Add Component: `RelayService`
   - Configuration:
     - Enable Debug Logging: ✅
     - Max Players: `2`
     - Region: `"auto"` (or your preferred region)

3. **Add Lobby Service:**
   - Create empty GameObject in scene root
   - Name: `[LobbyService]`
   - Add Component: `LobbyService`
   - Configuration:
     - Enable Debug Logging: ✅
     - Max Lobbies To Fetch: `20`
     - Lobby Heartbeat Interval: `15`

### Step 3: Integrate with GameManager

1. **Select your GameManager GameObject**
2. **Add Component:** `GameManagerMultiplayerIntegration`
3. **Configuration:**
   - Enable Multiplayer Tracking: ✅
   - Track All Match Events: ✅

**Note:** This component will track match events for multiplayer stats (once implemented).

---

## Phase 4.3 - Analytics & Economy

### Step 4: Create Analytics Service

1. **Create Analytics Service:**
   - Create empty GameObject in scene root
   - Name: `[AnalyticsService]`
   - Add Component: `AnalyticsService`
   - Configuration:
     - Enable Debug Logging: ✅
     - Enable Unity Analytics: ❌ (until you setup Unity Analytics)
     - Batch Size: `10`
     - Flush Interval: `30`

### Step 5: Add Analytics Integrations

1. **On GameManager GameObject:**
   - Add Component: `GameManagerAnalytics`
   - Configuration:
     - Enable Analytics Tracking: ✅

2. **On ProgressionManager GameObject:**
   - Add Component: `ProgressionManagerAnalytics`
   - Configuration:
     - Enable Analytics Tracking: ✅

### Step 6: Create Economy Services

1. **Create Economy Validator:**
   - Create empty GameObject in scene root
   - Name: `[EconomyValidator]`
   - Add Component: `EconomyValidator`
   - Configuration:
     - Enable Debug Logging: ✅
     - Max XP Per Match: `500`
     - Max XP Per Hour: `2000`
     - Max Currency Per Match: `1000`
     - Enable Strict Validation: ✅

2. **Create Rate Limiter:**
   - Create empty GameObject in scene root
   - Name: `[RateLimiter]`
   - Add Component: `RateLimiter`
   - Configuration:
     - Enable Debug Logging: ✅
     - Default Max Requests: `10`
     - Default Window Seconds: `60`

3. **Create Suspicious Activity Detector:**
   - Create empty GameObject in scene root
   - Name: `[SuspiciousActivityDetector]`
   - Add Component: `SuspiciousActivityDetector`
   - Configuration:
     - Enable Debug Logging: ✅
     - Auto Flag Threshold: `75`
     - Minimum Matches For Analysis: `10`

---

## Phase 4.4 - Quest System

### Step 7: Generate Quest Templates

1. **In Unity Menu:** `Tools → Gravity Wars → Generate Quest Templates`
2. **Click:** "Generate All Quest Templates"
3. **Result:** 25 quest ScriptableObjects created in `Assets/Quests/Templates/`

### Step 8: Create Quest Service

1. **Create Quest Service:**
   - Create empty GameObject in scene root
   - Name: `[QuestService]`
   - Add Component: `QuestService`

2. **Assign Quest Templates:**
   - In Inspector, expand `Quest Templates` list
   - Set Size: `25`
   - **Drag ALL quest templates** from `Assets/Quests/Templates/` folder into the list
     - Daily quests (11 files)
     - Weekly quests (7 files)
     - Season quests (7 files)

3. **Configuration:**
   - Daily Quest Slots: `3`
   - Weekly Quest Slots: `3`
   - Season Quest Slots: `5`
   - Debug Logging: ✅

### Step 9: Add Quest Integrations

1. **On GameManager GameObject:**
   - Add Component: `GameManagerQuestIntegration`
   - Configuration:
     - Enable Quest Tracking: ✅

2. **On ProgressionManager GameObject:**
   - Add Component: `ProgressionManagerQuestIntegration`
   - Configuration:
     - Enable Quest Tracking: ✅

### Step 10: Create Quest UI (Canvas Setup)

**If you don't have a Canvas:**
1. Right-click Hierarchy → UI → Canvas
2. Name: `MainCanvas`
3. Canvas Scaler:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

**Create Quest Panel:**

1. **Create Quest Panel:**
   - Right-click `MainCanvas` → UI → Panel
   - Name: `QuestPanel`
   - Set Active: ❌ (will be shown/hidden by script)
   - RectTransform:
     - Anchors: Right side (preset: right-center)
     - Pos X: `400` (off-screen)
     - Width: `400`
     - Height: `800`

2. **Add Quest UI Component:**
   - Select `QuestPanel`
   - Add Component: `QuestUI`

3. **Create UI Elements:**

   **a) Background Overlay:**
   - Right-click `MainCanvas` → UI → Image
   - Name: `QuestBackgroundOverlay`
   - RectTransform: Stretch to fill (preset: stretch-stretch)
   - Color: Black with Alpha `100`
   - Set Active: ❌

   **b) Category Tabs:**
   - Right-click `QuestPanel` → UI → Button
   - Name: `DailyTabButton`
   - Position: Top-left
   - Repeat for: `WeeklyTabButton`, `SeasonTabButton`

   **c) Quest Container:**
   - Right-click `QuestPanel` → UI → Scroll View
   - Name: `QuestScrollView`
   - Delete Scrollbar Horizontal
   - In Content:
     - Add Component: `Vertical Layout Group`
     - Spacing: `10`
     - Child Alignment: Upper Center
     - Child Force Expand: Width ✅, Height ❌

   **d) Next Refresh Text:**
   - Right-click `QuestPanel` → UI → Text - TextMeshPro
   - Name: `NextRefreshText`
   - Position: Top-center
   - Text: "Next refresh in: --"

   **e) Notification Badge:**
   - Right-click `QuestPanel` → UI → Image
   - Name: `NotificationBadge`
   - Position: Top-right corner
   - Add child: Text - TextMeshPro (`BadgeCountText`)

   **f) Toggle Button:**
   - Right-click `MainCanvas` → UI → Button
   - Name: `QuestToggleButton`
   - Position: Right edge of screen
   - Text: "Quests"

4. **Create Quest Card Prefab:**

   **Create temporary quest card:**
   - Right-click Hierarchy → UI → Image
   - Name: `QuestCard`
   - Width: `380`, Height: `120`

   **Add child elements:**
   ```
   QuestCard
   ├── QuestNameText (TextMeshPro)
   ├── QuestDescriptionText (TextMeshPro)
   ├── ProgressBar (Slider)
   │   └── Fill (Image)
   ├── ProgressText (TextMeshPro)
   ├── RewardText (TextMeshPro)
   ├── TimerText (TextMeshPro)
   └── ClaimButton (Button)
       └── ButtonText (TextMeshPro)
   ```

   **Add QuestCardUI component:**
   - Select `QuestCard`
   - Add Component: `QuestCardUI`
   - **Assign all UI references in Inspector**

   **Create Prefab:**
   - Drag `QuestCard` from Hierarchy to `Assets/Quests/UI/Prefabs/`
   - Delete `QuestCard` from Hierarchy

5. **Assign QuestUI References:**
   - Select `QuestPanel`
   - In `QuestUI` component, assign:
     - Quest Panel: `QuestPanel` RectTransform
     - Background Overlay: `QuestBackgroundOverlay` Image
     - Daily Tab Button: `DailyTabButton`
     - Weekly Tab Button: `WeeklyTabButton`
     - Season Tab Button: `SeasonTabButton`
     - Quest Container: `QuestScrollView/Viewport/Content`
     - Quest Card Prefab: `QuestCard` prefab from Assets
     - Next Refresh Text: `NextRefreshText`
     - Notification Badge: `NotificationBadge`
     - Badge Count Text: `BadgeCountText`
     - Toggle Panel Button: `QuestToggleButton`
   - Panel Slide Duration: `0.3`
   - Hidden Position: `(400, 0)`
   - Shown Position: `(0, 0)`

---

## Phase 4.5 - Achievement System

### Step 11: Generate Achievement Templates

1. **In Unity Menu:** `Tools → Gravity Wars → Generate Achievement Templates`
2. **Click:** "Generate All Achievement Templates"
3. **Result:** 50+ achievement ScriptableObjects created in `Assets/Achievements/Templates/`

### Step 12: Create Achievement Service

1. **Create Achievement Service:**
   - Create empty GameObject in scene root
   - Name: `[AchievementService]`
   - Add Component: `AchievementService`

2. **Assign Achievement Templates:**
   - In Inspector, expand `Achievement Templates` list
   - Set Size: `50+` (count the files in Templates folder)
   - **Drag ALL achievement templates** from `Assets/Achievements/Templates/` into the list
     - Combat achievements (~15)
     - Progression achievements (~7)
     - Collection achievements (~5)
     - Skill achievements (~8)
     - Social achievements (~5)
     - Secret achievements (~6)

3. **Configuration:**
   - Debug Logging: ✅
   - Enable Steam Sync: ❌ (until you setup Steamworks)
   - Enable PS Sync: ❌
   - Enable Xbox Sync: ❌
   - Show Unlock Notifications: ✅
   - Notification Duration: `5`

### Step 13: Add Achievement Integrations

1. **On GameManager GameObject:**
   - Add Component: `GameManagerAchievementIntegration`
   - Configuration:
     - Enable Achievement Tracking: ✅
     - Perfect Accuracy Threshold: `95`
     - Quick Victory Time Limit: `60`

2. **On ProgressionManager GameObject:**
   - Add Component: `ProgressionManagerAchievementIntegration`
   - Configuration:
     - Enable Achievement Tracking: ✅

### Step 14: Create Achievement UI

**Create Achievement Panel:**

1. **Create Panel:**
   - Right-click `MainCanvas` → UI → Panel
   - Name: `AchievementPanel`
   - Set Active: ❌
   - RectTransform:
     - Anchors: Center
     - Width: `1200`
     - Height: `800`

2. **Add Achievement UI Component:**
   - Select `AchievementPanel`
   - Add Component: `AchievementUI`

3. **Create UI Elements:**

   **a) Category Filter Buttons:**
   - Create buttons for each category:
     - `AllCategoryButton`
     - `CombatCategoryButton`
     - `ProgressionCategoryButton`
     - `CollectionCategoryButton`
     - `SkillCategoryButton`
     - `SocialCategoryButton`
     - `SecretCategoryButton`
   - Position in a horizontal row at top

   **b) Filter Toggles:**
   - Right-click `AchievementPanel` → UI → Toggle
   - Name: `ShowUnlockedToggle`
   - Label: "Show Unlocked"
   - Is On: ✅
   - Repeat for: `ShowLockedToggle`

   **c) Achievement Container:**
   - Right-click `AchievementPanel` → UI → Scroll View
   - Name: `AchievementScrollView`
   - In Content:
     - Add Component: `Grid Layout Group`
     - Cell Size: `(350, 120)`
     - Spacing: `(10, 10)`
     - Constraint: Fixed Column Count = `3`

   **d) Search Field:**
   - Right-click `AchievementPanel` → UI → Input Field - TextMeshPro
   - Name: `SearchField`
   - Placeholder: "Search achievements..."

   **e) Statistics:**
   - Create TextMeshPro elements:
     - `TotalAchievementsText`: "Total: 50"
     - `UnlockedCountText`: "Unlocked: 0"
     - `CompletionPercentageText`: "Completion: 0%"
     - `AchievementPointsText`: "Points: 0"

   **f) Unlock Notification Popup:**
   - Right-click `MainCanvas` → UI → Image
   - Name: `UnlockNotificationPopup`
   - Position: Top-center
   - Set Active: ❌
   - Add children:
     - `NotificationAchievementName` (TextMeshPro)
     - `NotificationDescription` (TextMeshPro)
     - `NotificationIcon` (Image)
     - `NotificationPointsText` (TextMeshPro)

   **g) Toggle Button:**
   - Right-click `MainCanvas` → UI → Button
   - Name: `AchievementToggleButton`
   - Position: Left edge of screen
   - Text: "Achievements"

4. **Create Achievement Card Prefab:**

   **Create card:**
   - Right-click Hierarchy → UI → Image
   - Name: `AchievementCard`
   - Width: `340`, Height: `110`

   **Add elements:**
   ```
   AchievementCard
   ├── AchievementIcon (Image)
   ├── LockIcon (Image - shown when locked)
   ├── TierBadge (Image)
   │   └── TierText (TextMeshPro)
   ├── AchievementNameText (TextMeshPro)
   ├── AchievementDescriptionText (TextMeshPro)
   ├── ProgressBarContainer (GameObject)
   │   ├── ProgressBar (Slider)
   │   └── ProgressText (TextMeshPro)
   └── PointsText (TextMeshPro)
   ```

   **Add AchievementCardUI component:**
   - Select `AchievementCard`
   - Add Component: `AchievementCardUI`
   - **Assign all UI references**
   - Colors:
     - Unlocked Color: `(1, 1, 1)`
     - Locked Color: `(0.5, 0.5, 0.5)`

   **Create Prefab:**
   - Drag to `Assets/Achievements/UI/Prefabs/`
   - Delete from Hierarchy

5. **Assign AchievementUI References:**
   - Select `AchievementPanel`
   - In `AchievementUI` component, assign all created UI elements

---

## Phase 4.6 - Leaderboard System

### Step 15: Create Leaderboard Service

1. **Create Leaderboard Service:**
   - Create empty GameObject in scene root
   - Name: `[LeaderboardService]`
   - Add Component: `LeaderboardService`

2. **Configure Leaderboard Definitions:**
   - **Option A - Manual (Recommended for now):**
     - In Inspector, expand `Leaderboard Definitions`
     - Set Size: `10`
     - For each entry, configure:
       - Leaderboard ID: e.g., `"global_total_wins_alltime"`
       - Display Name: e.g., `"Total Wins"`
       - Description: e.g., `"All-time match wins leaderboard"`
       - Scope: `Global`
       - Stat Type: `TotalWins`
       - Time Frame: `AllTime`
       - Ship Filter: `All`
       - Score Format: `"{0} wins"`
       - Descending: ✅
       - Max Entries: `1000`
       - Entries Per Page: `20`
       - Auto Reset: ❌

   - **Repeat for all 10 default leaderboards:**
     1. Total Wins (All-Time)
     2. Longest Win Streak (All-Time)
     3. Total Damage Dealt (All-Time)
     4. Highest Damage - Single Match (All-Time)
     5. Best Accuracy (All-Time)
     6. Fastest Win (All-Time)
     7. Win Rate (All-Time)
     8. Weekly Wins (Weekly, Auto Reset ✅)
     9. Monthly Wins (Monthly, Auto Reset ✅)
     10. Ranked MMR (Season, Auto Reset ✅)

3. **Configuration:**
   - Debug Logging: ✅
   - Cache Expiration: `300` (5 minutes)
   - Auto Refresh Cache: ✅
   - Auto Refresh Interval: `60`
   - Max Submissions Per Minute: `10`

### Step 16: Add Leaderboard Integration

1. **On GameManager GameObject:**
   - Add Component: `GameManagerLeaderboardIntegration`
   - Configuration:
     - Enable Leaderboard Tracking: ✅
     - Submit After Match: ✅
     - Batch Submit Interval: `300`

### Step 17: Create Leaderboard UI

**Create Leaderboard Panel:**

1. **Create Panel:**
   - Right-click `MainCanvas` → UI → Panel
   - Name: `LeaderboardPanel`
   - Set Active: ❌
   - RectTransform:
     - Anchors: Center
     - Width: `1000`
     - Height: `800`

2. **Add Leaderboard UI Component:**
   - Select `LeaderboardPanel`
   - Add Component: `LeaderboardUI`

3. **Create UI Elements:**

   **a) Leaderboard Dropdown:**
   - Right-click `LeaderboardPanel` → UI → Dropdown - TextMeshPro
   - Name: `LeaderboardDropdown`
   - Position: Top-center
   - Options will be populated by script

   **b) Scope Tab Buttons:**
   - Create buttons:
     - `GlobalTabButton` - "Global"
     - `FriendsTabButton` - "Friends"
     - `SeasonalTabButton` - "Seasonal"

   **c) Entry Container:**
   - Right-click `LeaderboardPanel` → UI → Scroll View
   - Name: `LeaderboardScrollView`
   - In Content:
     - Add Component: `Vertical Layout Group`
     - Spacing: `5`

   **d) Pagination:**
   - Create buttons:
     - `PreviousPageButton` - "< Previous"
     - `NextPageButton` - "Next >"
   - Create text:
     - `PageNumberText` - "Page 1 / 10"
   - Create button:
     - `JumpToPlayerButton` - "Jump to My Rank"

   **e) Header:**
   - Create texts:
     - `LeaderboardTitleText` - Large, bold
     - `LastUpdatedText` - Small, gray
   - Create button:
     - `RefreshButton` - Refresh icon

   **f) Loading Indicator:**
   - Right-click `LeaderboardPanel` → UI → Image
   - Name: `LoadingIndicator`
   - Set Active: ❌
   - Add rotating spinner image

   **g) Player Info:**
   - Create texts:
     - `PlayerRankText` - "Your Rank: 50th"
     - `PlayerScoreText` - "Score: 100 wins"

   **h) Toggle Button:**
   - Right-click `MainCanvas` → UI → Button
   - Name: `LeaderboardToggleButton`
   - Position: Top edge of screen
   - Text: "Leaderboards"

4. **Create Leaderboard Entry Prefab:**

   **Create entry:**
   - Right-click Hierarchy → UI → Image
   - Name: `LeaderboardEntry`
   - Width: `950`, Height: `60`

   **Add elements:**
   ```
   LeaderboardEntry
   ├── RankText (TextMeshPro) - "1"
   ├── RankMedal (Image) - Gold/Silver/Bronze
   ├── PlayerAvatar (Image)
   ├── PlayerNameText (TextMeshPro)
   ├── ScoreText (TextMeshPro)
   └── RankChangeText (TextMeshPro) - "+5", "-2"
   ```

   **Add LeaderboardEntryUI component:**
   - Select `LeaderboardEntry`
   - Add Component: `LeaderboardEntryUI`
   - **Assign all references**
   - **Assign medal sprites:**
     - Gold Medal Sprite: (create/import)
     - Silver Medal Sprite: (create/import)
     - Bronze Medal Sprite: (create/import)
   - Colors:
     - Self Color: `(1, 0.9, 0.5)` - Yellow
     - Friend Color: `(0.5, 0.9, 1)` - Blue
     - Normal Color: `(1, 1, 1)` - White

   **Create Prefab:**
   - Drag to `Assets/Leaderboards/UI/Prefabs/`
   - Delete from Hierarchy

5. **Assign LeaderboardUI References:**
   - Select `LeaderboardPanel`
   - In `LeaderboardUI` component, assign all elements
   - Configuration:
     - Entries Per Page: `20`
     - Auto Refresh Interval: `60`
     - Enable Auto Refresh: ✅

---

## Final Integration

### Step 18: Connect Services to Service Locator

1. **Select `[ServiceLocator]` GameObject**
2. **In ServiceLocator component:**
   - **Cloud Save Reference:** Drag `[CloudSaveService]` GameObject
   - **Authentication Reference:** Drag `[AuthenticationService]` GameObject
   - **Analytics Reference:** Drag `[AnalyticsService]` GameObject

### Step 19: Scene Setup Checklist

**Verify your scene has:**

Services (in scene root):
- ✅ `[ServiceLocator]`
  - ✅ `[CloudSaveService]`
  - ✅ `[AuthenticationService]`
  - ✅ `[NetworkConnectionManager]`
- ✅ `[MatchmakingService]`
- ✅ `[RelayService]`
- ✅ `[LobbyService]`
- ✅ `[AnalyticsService]`
- ✅ `[EconomyValidator]`
- ✅ `[RateLimiter]`
- ✅ `[SuspiciousActivityDetector]`
- ✅ `[QuestService]`
- ✅ `[AchievementService]`
- ✅ `[LeaderboardService]`

Game Objects (existing):
- ✅ `GameManager` (with 3 new integration components)
- ✅ `ProgressionManager` (with 3 new integration components)

UI (under Canvas):
- ✅ `QuestPanel` (with QuestUI component)
- ✅ `AchievementPanel` (with AchievementUI component)
- ✅ `LeaderboardPanel` (with LeaderboardUI component)
- ✅ Toggle buttons for each panel

Prefabs:
- ✅ `QuestCard.prefab`
- ✅ `AchievementCard.prefab`
- ✅ `LeaderboardEntry.prefab`

Assets:
- ✅ 25 Quest templates in `Assets/Quests/Templates/`
- ✅ 50+ Achievement templates in `Assets/Achievements/Templates/`

---

## Testing Checklist

### Test Phase 4.1 - Services
1. ✅ Play Mode - Check Console for service initialization logs
2. ✅ ServiceLocator finds all services
3. ✅ CloudSaveService initializes
4. ✅ AuthenticationService attempts anonymous login

### Test Phase 4.3 - Analytics
1. ✅ Play a match - Check analytics events in Console
2. ✅ GameManagerAnalytics tracks match events
3. ✅ ProgressionManagerAnalytics tracks XP gains

### Test Phase 4.4 - Quests
1. ✅ Click Quest Toggle Button
2. ✅ Quest panel slides in
3. ✅ 3 daily quests appear
4. ✅ Quest progress bars show 0/X
5. ✅ Play match - Quest progress updates
6. ✅ Complete quest - Claim button appears

### Test Phase 4.5 - Achievements
1. ✅ Click Achievement Toggle Button
2. ✅ Achievement panel opens
3. ✅ 50+ achievements displayed
4. ✅ Filter by category works
5. ✅ Search works
6. ✅ Play match - "First Blood" achievement unlocks
7. ✅ Notification popup shows

### Test Phase 4.6 - Leaderboards
1. ✅ Click Leaderboard Toggle Button
2. ✅ Leaderboard panel opens
3. ✅ Select "Total Wins" from dropdown
4. ✅ Mock leaderboard data displays (1000 entries)
5. ✅ Pagination works (Next/Previous)
6. ✅ Jump to Player button works
7. ✅ Player rank shows (mock: rank 50)

---

## Common Issues & Solutions

### Issue: "ServiceLocator not found"
**Solution:** Ensure `[ServiceLocator]` GameObject exists and has ServiceLocator component

### Issue: "Quest templates empty"
**Solution:** Run `Tools → Gravity Wars → Generate Quest Templates` and assign them in QuestService

### Issue: "UI not displaying"
**Solution:** Check Canvas render mode is Screen Space - Overlay, verify all UI references assigned

### Issue: "NullReferenceException in QuestUI"
**Solution:** Ensure Quest Card Prefab has QuestCardUI component with all references assigned

### Issue: "Achievements not unlocking"
**Solution:**
- Verify AchievementService has all templates assigned
- Check GameManagerAchievementIntegration is calling OnMatchEnd()
- Enable Debug Logging to see what's happening

### Issue: "Leaderboard shows no data"
**Solution:** This is expected - showing mock data. Will connect to real backend later.

---

## Next Steps

After completing this setup:

1. **Test all systems** using the Testing Checklist
2. **Read the Game State Documentation** to understand what's been built
3. **Read the Multiplayer Implementation Guide** to implement real online play
4. **Start connecting to Unity Gaming Services** (Authentication, Cloud Save, Analytics)
5. **Implement actual multiplayer networking** (Netcode for GameObjects)

---

## Support

If you encounter issues:
1. Check Unity Console for error messages
2. Verify all GameObjects exist and have components
3. Ensure all UI references are assigned (Inspector shows no "None" references)
4. Enable Debug Logging on all services to see what's happening
5. Check that prefabs are properly configured

**Estimated Total Setup Time:** 2-3 hours (depending on UI creation speed)

**Good luck! 🚀**
