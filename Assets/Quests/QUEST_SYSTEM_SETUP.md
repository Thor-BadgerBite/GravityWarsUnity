# Quest System Setup Guide

## Overview

The quest system provides daily/weekly/season quests to keep players engaged and reward progression.

**Features:**
- 3 daily quests (refresh every 24 hours)
- 3 weekly quests (refresh every 7 days)
- 5 season quests (refresh every 90 days)
- 15+ objective types
- Automatic reward distribution
- Cloud save synchronization
- Analytics integration

---

## Installation Steps

### Step 1: Generate Quest Templates

1. In Unity Editor, go to **Tools → Gravity Wars → Generate Quest Templates**
2. Click **"Generate All Quest Templates"**
3. This creates 25 quest ScriptableObjects in `Assets/Quests/Templates/`

Quest templates include:
- 11 daily quests (easy/medium)
- 7 weekly quests (medium/hard)
- 7 season quests (hard/very hard)

### Step 2: Setup QuestService

1. Create a new GameObject in your main scene: `[QuestService]`
2. Add the **QuestService** component to it
3. In the Inspector, assign the quest templates:
   - Expand the **Quest Templates** list
   - Set size to 25 (or however many you generated)
   - Drag all quest templates from `Assets/Quests/Templates/` into the list

**Configuration Options:**
- **Daily Quest Slots**: 3 (how many daily quests active at once)
- **Weekly Quest Slots**: 3
- **Season Quest Slots**: 5
- **Debug Logging**: Enable for testing

### Step 3: Integrate with GameManager

1. Select your **GameManager** GameObject
2. Add the **GameManagerQuestIntegration** component
3. Enable **Quest Tracking** in Inspector

This component automatically tracks:
- Match won/played
- Rounds won
- Damage dealt
- Missiles fired/hit
- Perks used

**Important:** Add these method calls to your GameManager code:

```csharp
// In your match start method:
GetComponent<GameManagerQuestIntegration>()?.OnMatchStart();

// In your match end method:
GetComponent<GameManagerQuestIntegration>()?.OnMatchEnd(winner, isPlayer1Winner);

// In your round end method:
GetComponent<GameManagerQuestIntegration>()?.OnRoundEnd(winner, isPlayer1Winner);

// When player fires missile:
GetComponent<GameManagerQuestIntegration>()?.OnPlayerFireMissile(hit, damage, missileType);

// When player activates perk:
GetComponent<GameManagerQuestIntegration>()?.OnPlayerActivatePerk(perkName);
```

### Step 4: Integrate with ProgressionManager

1. Select your **ProgressionManager** GameObject
2. Add the **ProgressionManagerQuestIntegration** component
3. Enable **Quest Tracking** in Inspector

This component tracks:
- Account level reached
- Currency earned
- Items unlocked
- Ships leveled up

**Important:** Add these method calls to your ProgressionManager code:

```csharp
// When player gains XP:
GetComponent<ProgressionManagerQuestIntegration>()?.OnAccountXPGained(xpGained);

// When player earns currency:
GetComponent<ProgressionManagerQuestIntegration>()?.OnCurrencyEarned(currencyType, amount);

// When player unlocks item:
GetComponent<ProgressionManagerQuestIntegration>()?.OnItemUnlocked(itemType, itemName, unlockSource);

// When ship levels up:
GetComponent<ProgressionManagerQuestIntegration>()?.OnShipLevelUp(loadoutKey, newLevel);
```

### Step 5: Setup Quest UI

1. Create a Canvas GameObject if you don't have one
2. Add the **QuestUI** component to the Canvas
3. Setup UI references in Inspector:

**Panel References:**
- **Quest Panel**: RectTransform for the main quest panel
- **Background Overlay**: Semi-transparent background image

**Tab Buttons:**
- **Daily Tab Button**: Button for daily quests
- **Weekly Tab Button**: Button for weekly quests
- **Season Tab Button**: Button for season quests

**Quest Display:**
- **Quest Container**: Transform to hold quest cards (use VerticalLayoutGroup)
- **Quest Card Prefab**: Prefab for individual quest cards
- **Next Refresh Text**: TextMeshProUGUI showing time until refresh

**Notification Badge:**
- **Notification Badge**: GameObject with count of claimable quests
- **Badge Count Text**: TextMeshProUGUI showing the count

**Panel Toggle:**
- **Toggle Panel Button**: Button to show/hide quest panel
- **Toggle Button Icon**: Icon that rotates when panel opens

**Animation Settings:**
- **Panel Slide Duration**: 0.3 seconds
- **Hidden Position**: (400, 0) - off-screen right
- **Shown Position**: (0, 0) - on-screen

### Step 6: Create Quest Card Prefab

Create a prefab for quest cards with these components:

**Structure:**
```
QuestCard (Prefab)
├── Background (Image)
├── QuestName (TextMeshProUGUI)
├── QuestDescription (TextMeshProUGUI)
├── ProgressBar (Slider)
│   └── Fill (Image)
├── ProgressText (TextMeshProUGUI)
├── RewardText (TextMeshProUGUI)
├── TimerText (TextMeshProUGUI)
└── ClaimButton (Button)
    └── ButtonText (TextMeshProUGUI)
```

**Components:**
- Add **QuestCardUI** component to the root
- Assign all UI references in Inspector

---

## Usage

### Initialization

The quest system initializes automatically when the game starts:

```csharp
// In your game initialization:
QuestService.Instance.InitializeQuests();
```

### Updating Quest Progress

Quest progress is updated automatically through the integration components. If you need manual updates:

```csharp
// Update quest progress:
QuestService.Instance.UpdateQuestProgress(
    QuestObjectiveType.WinMatches,
    amount: 1
);

// With context (for archetype-specific quests):
QuestService.Instance.UpdateQuestProgress(
    QuestObjectiveType.WinWithArchetype,
    amount: 1,
    context: "Tank"
);
```

### Displaying Quest UI

```csharp
// Show quest panel:
QuestUI.Instance.ShowQuestPanel();

// Hide quest panel:
QuestUI.Instance.HideQuestPanel();

// Toggle quest panel:
QuestUI.Instance.ToggleQuestPanel();

// Refresh quest display:
QuestUI.Instance.RefreshQuestDisplay();
```

### Claiming Quest Rewards

Players can claim quests manually through the UI, or you can claim programmatically:

```csharp
// Claim quest:
QuestService.Instance.ClaimQuest(questID);
```

---

## Quest Objective Types

The system supports 15 quest objective types:

### Match Objectives
- **WinMatches**: Win X matches
- **PlayMatches**: Play X matches (win or lose)
- **WinRounds**: Win X rounds in any matches

### Combat Objectives
- **DealDamage**: Deal X damage to enemy ships
- **FireMissiles**: Fire X missiles
- **HitMissiles**: Hit enemy ships with X missiles
- **DestroyShipsWithMissileType**: Destroy ships with specific missile type

### Perk Objectives
- **UsePerkNTimes**: Use any perk X times
- **WinWithPerk**: Win matches with specific perk equipped

### Ship Objectives
- **PlayMatchesWithArchetype**: Play matches with specific ship archetype
- **WinWithArchetype**: Win matches with specific ship archetype

### Streak Objectives
- **ReachWinStreak**: Reach a win streak of X matches

### Progression Objectives
- **UnlockItem**: Unlock X items
- **ReachAccountLevel**: Reach account level X
- **EarnCurrency**: Earn X currency
- **LevelUpShip**: Level up ships X times

---

## Customizing Quests

### Creating New Quest Templates

1. Right-click in Project → **Create → GravityWars/Quest**
2. Configure in Inspector:
   - **Quest ID**: Unique identifier (e.g., "daily_win_5_matches")
   - **Display Name**: Name shown to player
   - **Description**: Quest description
   - **Quest Type**: Daily/Weekly/Season
   - **Objective Type**: What player must do
   - **Target Value**: How many times
   - **Rewards**: Coins, gems, XP, items
   - **Requirements**: Level, archetype, missile type

3. Add to QuestService's quest templates list

### Modifying Quest Templates

Edit existing templates in `Assets/Quests/Templates/` to change:
- Rewards
- Difficulty
- Target values
- Requirements

---

## Cloud Save Integration

Quests are automatically saved to cloud:

```csharp
// Quests are saved when:
// - Quest progress updated
// - Quest completed
// - Quest claimed
// - Quest expired/refreshed

// Manual save:
await QuestService.Instance.SaveQuestsToCloud();

// Manual load:
await QuestService.Instance.LoadQuestsFromCloud();
```

---

## Analytics Integration

Quest events are automatically tracked:

- Quest completed
- Quest claimed
- Quest progress updated
- Quest expired

View analytics in your Unity Analytics dashboard.

---

## Testing

### Debug Commands

```csharp
// Force refresh all quests (debug):
QuestService.Instance.ForceRefreshAllQuests();

// Get all quests by type:
var dailyQuests = QuestService.Instance.GetQuestsByType(QuestType.Daily);

// Get quest by ID:
var quest = QuestService.Instance.GetQuestByID("daily_win_3_matches");

// Check claimable count:
int claimable = QuestService.Instance.GetClaimableQuestCount();
```

### Testing Quest Progression

1. Start the game
2. Open quest panel (assign to a UI button)
3. Play matches and watch quest progress update
4. Complete quests and claim rewards
5. Check Unity Console for quest tracking logs

---

## Troubleshooting

### Quests not appearing

- Ensure QuestService has quest templates assigned
- Check that InitializeQuests() was called
- Enable debug logging in QuestService

### Quest progress not updating

- Verify integration components are attached to GameManager/ProgressionManager
- Check that OnMatchStart/OnMatchEnd methods are being called
- Enable debug logging to see quest update events

### Rewards not being awarded

- Check ProgressionManager is connected to QuestService
- Verify reward values are set in quest templates
- Check Unity Console for error messages

### UI not displaying

- Verify all UI references are assigned in Inspector
- Check Canvas is set to Screen Space - Overlay
- Ensure QuestCardUI component is on quest card prefab

---

## Architecture Notes

**Non-Invasive Design:**
- Integration components attach to existing GameObjects
- No modifications to existing code required
- All tracking happens through public methods

**Scalability:**
- Easy to add new objective types
- Quest templates can be created without code changes
- Supports unlimited quest templates

**Performance:**
- Auto-refresh runs every 60 seconds (configurable)
- Cloud saves are async (non-blocking)
- UI updates only when panel is visible

---

## Next Steps

After setup is complete:

1. Test quest system in Play Mode
2. Adjust rewards/difficulty based on playtesting
3. Create additional quest templates for variety
4. Connect to your IAP system for hard currency rewards
5. Setup quest notifications (push notifications for completed quests)

---

## Support

For issues or questions, see:
- Unity Console logs (enable debug logging)
- QuestService component Inspector
- Quest template ScriptableObjects

**Common Issues:**
- "QuestService not available" → Ensure QuestService GameObject exists in scene
- "Quest not found" → Check quest ID matches template
- "Quest templates empty" → Run Quest Template Generator
