# Achievement System Setup Guide

## Overview

The achievement system provides permanent, lifetime achievements to reward player progression and skill.

**Features:**
- 50+ achievement templates
- Tiered achievements (Bronze/Silver/Gold/Platinum)
- Secret achievements (hidden until unlocked)
- Platform integration hooks (Steam, PlayStation, Xbox)
- Achievement points system
- Unlock notifications
- Cloud save synchronization
- Analytics integration

---

## Installation Steps

### Step 1: Generate Achievement Templates

1. In Unity Editor, go to **Tools → Gravity Wars → Generate Achievement Templates**
2. Click **"Generate All Achievement Templates"**
3. This creates 50+ achievement ScriptableObjects in `Assets/Achievements/Templates/`

Achievement categories include:
- **Combat** (15 achievements): Win matches, deal damage, hit missiles
- **Progression** (7 achievements): Reach levels, earn currency
- **Collection** (5 achievements): Unlock all ships/missiles/perks/cosmetics
- **Skill** (8 achievements): Win streaks, flawless victories, perfect accuracy
- **Social** (5 achievements): Play with friends, online matches
- **Secret** (6 achievements): Hidden challenges

### Step 2: Setup AchievementService

1. Create a new GameObject in your main scene: `[AchievementService]`
2. Add the **AchievementService** component to it
3. In the Inspector, assign the achievement templates:
   - Expand the **Achievement Templates** list
   - Set size to 50+ (or however many you generated)
   - Drag all achievement templates from `Assets/Achievements/Templates/` into the list

**Configuration Options:**
- **Debug Logging**: Enable for testing
- **Enable Steam Sync**: Enable if using Steamworks
- **Enable PS Sync**: Enable if targeting PlayStation
- **Enable Xbox Sync**: Enable if targeting Xbox
- **Show Unlock Notifications**: Enable to show popup notifications
- **Notification Duration**: How long notifications display (default: 5s)

### Step 3: Integrate with GameManager

1. Select your **GameManager** GameObject
2. Add the **GameManagerAchievementIntegration** component
3. Enable **Achievement Tracking** in Inspector

This component automatically tracks:
- Matches won/played
- Rounds won
- Win streaks
- Damage dealt
- Missiles fired/hit
- Flawless victories
- Quick victories
- Perfect accuracy matches

**Important:** Add these method calls to your GameManager code:

```csharp
// In your match start method:
GetComponent<GameManagerAchievementIntegration>()?.OnMatchStart();

// In your match end method:
GetComponent<GameManagerAchievementIntegration>()?.OnMatchEnd(winner, isPlayer1Winner);

// In your round end method:
GetComponent<GameManagerAchievementIntegration>()?.OnRoundEnd(winner, isPlayer1Winner);

// When player fires missile:
GetComponent<GameManagerAchievementIntegration>()?.OnPlayerFireMissile(hit, damage, missileType);

// When player takes damage:
GetComponent<GameManagerAchievementIntegration>()?.OnPlayerTakeDamage(damage);

// When enemy ship is destroyed:
GetComponent<GameManagerAchievementIntegration>()?.OnEnemyShipDestroyed(missileType);

// When player activates perk:
GetComponent<GameManagerAchievementIntegration>()?.OnPlayerActivatePerk(perkName);

// When playing with friend:
GetComponent<GameManagerAchievementIntegration>()?.OnPlayWithFriend(friendID);
```

### Step 4: Integrate with ProgressionManager

1. Select your **ProgressionManager** GameObject
2. Add the **ProgressionManagerAchievementIntegration** component
3. Enable **Achievement Tracking** in Inspector

This component tracks:
- Account level reached
- Total currency earned/spent
- Items unlocked (ships, missiles, perks, cosmetics)
- Battle pass tier reached
- Daily/weekly quests completed

**Important:** Add these method calls to your ProgressionManager code:

```csharp
// When player gains XP:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnAccountXPGained(xpGained);

// When player earns currency:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnCurrencyEarned(currencyType, amount);

// When player spends currency:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnCurrencySpent(currencyType, amount, itemPurchased);

// When player unlocks item:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnItemUnlocked(itemType, itemName, unlockSource);

// When battle pass tier up:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnBattlePassTierUp(tierNumber, isPremium);

// When daily quest completed:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnDailyQuestCompleted();

// When weekly quest completed:
GetComponent<ProgressionManagerAchievementIntegration>()?.OnWeeklyQuestCompleted();
```

### Step 5: Setup Achievement UI

1. Create a Canvas GameObject if you don't have one
2. Add the **AchievementUI** component to the Canvas
3. Setup UI references in Inspector:

**Panel References:**
- **Achievement Panel**: Main achievement panel GameObject
- **Background Overlay**: Semi-transparent background image

**Category Filter Buttons:**
- **All Category Button**: Show all achievements
- **Combat Category Button**: Filter by Combat
- **Progression Category Button**: Filter by Progression
- **Collection Category Button**: Filter by Collection
- **Skill Category Button**: Filter by Skill
- **Social Category Button**: Filter by Social
- **Secret Category Button**: Filter by Secret

**Filter Toggles:**
- **Show Unlocked Toggle**: Toggle to show unlocked achievements
- **Show Locked Toggle**: Toggle to show locked achievements

**Achievement Display:**
- **Achievement Container**: Transform to hold achievement cards (use GridLayoutGroup)
- **Achievement Card Prefab**: Prefab for individual achievement cards

**Search:**
- **Search Field**: TMP_InputField for searching achievements

**Statistics:**
- **Total Achievements Text**: Total number of achievements
- **Unlocked Count Text**: Number unlocked
- **Completion Percentage Text**: % completion
- **Achievement Points Text**: Total points earned

**Unlock Notification:**
- **Unlock Notification Popup**: Popup shown when achievement unlocks
- **Notification Achievement Name**: Achievement name text
- **Notification Description**: Achievement description text
- **Notification Icon**: Achievement icon image
- **Notification Points Text**: Points awarded text

**Panel Toggle:**
- **Toggle Panel Button**: Button to show/hide achievement panel

### Step 6: Create Achievement Card Prefab

Create a prefab for achievement cards with these components:

**Structure:**
```
AchievementCard (Prefab)
├── Background (Image)
├── AchievementIcon (Image)
├── LockIcon (Image - shown when locked)
├── TierBadge (GameObject)
│   ├── TierBackground (Image - colored by tier)
│   └── TierText (TextMeshProUGUI - "Bronze", "Silver", etc.)
├── AchievementName (TextMeshProUGUI)
├── AchievementDescription (TextMeshProUGUI)
├── ProgressBarContainer (GameObject - only for incremental)
│   ├── ProgressBar (Slider)
│   └── ProgressText (TextMeshProUGUI)
└── PointsText (TextMeshProUGUI)
```

**Components:**
- Add **AchievementCardUI** component to the root
- Assign all UI references in Inspector

---

## Usage

### Initialization

The achievement system initializes automatically when the game starts:

```csharp
// In your game initialization:
AchievementService.Instance.InitializeAchievements();
```

### Updating Achievement Progress

Achievement progress is updated automatically through the integration components. If you need manual updates:

```csharp
// Update achievement progress (incremental):
AchievementService.Instance.UpdateAchievementProgress(
    AchievementConditionType.WinMatches,
    amount: 1
);

// Set achievement progress (absolute value):
AchievementService.Instance.SetAchievementProgress(
    AchievementConditionType.ReachAccountLevel,
    value: 50
);

// With context (for archetype-specific achievements):
AchievementService.Instance.UpdateAchievementProgress(
    AchievementConditionType.WinWithArchetype,
    amount: 1,
    context: "Tank"
);

// Manually unlock achievement (admin/debug):
AchievementService.Instance.UnlockAchievement("combat_firstblood");
```

### Displaying Achievement UI

```csharp
// Show achievement panel:
AchievementUI.Instance.ShowAchievementPanel();

// Hide achievement panel:
AchievementUI.Instance.HideAchievementPanel();

// Toggle achievement panel:
AchievementUI.Instance.ToggleAchievementPanel();
```

### Getting Achievement Data

```csharp
// Get all achievements:
var allAchievements = AchievementService.Instance.GetAllAchievements();

// Get unlocked achievements:
var unlocked = AchievementService.Instance.GetUnlockedAchievements();

// Get locked achievements:
var locked = AchievementService.Instance.GetLockedAchievements();

// Get achievements by category:
var combatAchievements = AchievementService.Instance.GetAchievementsByCategory(AchievementCategory.Combat);

// Get specific achievement:
var achievement = AchievementService.Instance.GetAchievementByID("combat_firstblood");

// Get total points:
int totalPoints = AchievementService.Instance.GetTotalAchievementPoints();

// Get completion percentage:
float completion = AchievementService.Instance.GetCompletionPercentage();
```

### Listening to Achievement Events

```csharp
// Subscribe to unlock event:
AchievementService.Instance.OnAchievementUnlockedEvent += OnAchievementUnlocked;

// Subscribe to progress event:
AchievementService.Instance.OnAchievementProgressEvent += OnAchievementProgress;

// Event handlers:
void OnAchievementUnlocked(AchievementInstance achievement)
{
    Debug.Log($"Achievement unlocked: {achievement.displayName}");
}

void OnAchievementProgress(AchievementInstance achievement, int newProgress)
{
    Debug.Log($"Achievement progress: {achievement.displayName} - {newProgress}/{achievement.targetValue}");
}
```

---

## Achievement Types

### Single Achievements
Unlocked once when condition is met (0 → 1).

Example: "First Blood" - Win your first match

### Incremental Achievements
Progress over time (0 → target).

Example: "Warrior" - Win 10 matches (0/10, 1/10, 2/10, ... 10/10)

### Tiered Achievements
Multiple difficulty tiers for the same achievement.

Example:
- Bronze: Win 10 matches
- Silver: Win 50 matches
- Gold: Win 100 matches
- Platinum: Win 500 matches

---

## Achievement Condition Types

### Match-Based
- **WinMatches**: Win X matches
- **PlayMatches**: Play X matches
- **WinRounds**: Win X rounds
- **WinMatchesInRow**: Win X matches in a row (win streak)
- **WinWithoutTakingDamage**: Win without taking damage (flawless)

### Combat-Based
- **DealDamage**: Deal X total damage (lifetime)
- **DealDamageInOneMatch**: Deal X damage in one match
- **FireMissiles**: Fire X missiles
- **HitMissiles**: Hit X missiles
- **AchieveAccuracy**: Achieve X% accuracy
- **DestroyShipsWithMissileType**: Destroy ships with specific missile type

### Perk-Based
- **UsePerkNTimes**: Use perks X times
- **WinWithPerk**: Win with specific perk
- **UseAllPerks**: Use all perks at least once

### Ship-Based
- **WinWithArchetype**: Win with specific archetype
- **WinWithAllArchetypes**: Win with all archetypes
- **UnlockAllShips**: Unlock all ship archetypes

### Progression
- **ReachAccountLevel**: Reach level X
- **EarnTotalCurrency**: Earn X total currency
- **SpendTotalCurrency**: Spend X total currency
- **UnlockAllItems**: Unlock all items

### Collection
- **UnlockAllMissiles**: Unlock all missile types
- **UnlockAllPerks**: Unlock all perks
- **UnlockAllCosmetics**: Unlock all cosmetics

### Skill-Based
- **WinWithPerfectAccuracy**: Win with 95%+ accuracy
- **WinIn60Seconds**: Win in under 60 seconds
- **DealDamageWithSingleShot**: Deal X damage with one shot

### Social
- **PlayWithFriend**: Play with a friend
- **WinAgainstFriend**: Win against a friend
- **CompleteMatchAgainstPlayer**: Complete match against player

### Special
- **PlayOnAllMaps**: Play on all maps
- **WinOnAllMaps**: Win on all maps
- **CompleteDailyQuest**: Complete X daily quests
- **CompleteWeeklyQuest**: Complete X weekly quests
- **ReachBattlePassMaxTier**: Reach max battle pass tier

---

## Customizing Achievements

### Creating New Achievement Templates

1. Right-click in Project → **Create → GravityWars/Achievement**
2. Configure in Inspector:
   - **Achievement ID**: Unique identifier (e.g., "combat_firstblood")
   - **Display Name**: Name shown to player
   - **Description**: Achievement description
   - **Achievement Type**: Single/Incremental/Tiered
   - **Category**: Combat/Progression/Collection/Skill/Social/Secret
   - **Condition Type**: What player must do
   - **Target Value**: How many times
   - **Is Secret**: Hidden until unlocked?
   - **Tier**: None/Bronze/Silver/Gold/Platinum/Diamond
   - **Rewards**: Coins, gems, XP, exclusive items, titles
   - **Achievement Points**: Points awarded
   - **Required Context**: Level, archetype, missile type

3. Add to AchievementService's achievement templates list

### Modifying Achievement Templates

Edit existing templates in `Assets/Achievements/Templates/` to change:
- Rewards
- Target values
- Tier levels
- Requirements

---

## Cloud Save Integration

Achievements are automatically saved to cloud:

```csharp
// Achievements are saved when:
// - Achievement unlocked
// - Achievement progress updated

// Manual save:
await AchievementService.Instance.SaveAchievementsToCloud();

// Manual load:
await AchievementService.Instance.LoadAchievementsFromCloud();
```

---

## Platform Integration

### Steam Achievements

1. Enable Steam sync in AchievementService
2. Add Steam achievement IDs to templates
3. Achievement system will automatically sync to Steam

```csharp
// In achievement template:
steamAchievementID = "ACH_FIRST_BLOOD";

// Syncs automatically on unlock
```

### PlayStation Trophies

1. Enable PS sync in AchievementService
2. Add PlayStation trophy IDs to templates
3. System will sync to PlayStation

### Xbox Achievements

1. Enable Xbox sync in AchievementService
2. Add Xbox achievement IDs to templates
3. System will sync to Xbox

---

## Analytics Integration

Achievement events are automatically tracked:

- Achievement unlocked
- Achievement progress updated
- Achievement viewed
- Time to unlock

View analytics in your Unity Analytics dashboard.

---

## Secret Achievements

Secret achievements are hidden until unlocked:

```csharp
// In achievement template:
isSecret = true;

// Display will show:
// Name: "???"
// Description: "This is a secret achievement."

// After unlocking:
// Name: "Tank Commander"
// Description: "Win 100 matches with the Tank archetype."
```

---

## Tiered Achievement System

Tiered achievements have multiple difficulty levels:

```csharp
// Bronze tier:
targetValue = 10;
tier = AchievementTier.Bronze;

// Silver tier:
targetValue = 50;
tier = AchievementTier.Silver;

// Gold tier:
targetValue = 100;
tier = AchievementTier.Gold;

// Platinum tier:
targetValue = 500;
tier = AchievementTier.Platinum;
```

Tier badge colors:
- Bronze: Orange (#CC8033)
- Silver: Light gray (#BFBFBF)
- Gold: Gold (#FFD700)
- Platinum: Light blue (#E5F2FF)
- Diamond: Cyan (#B3E5FF)

---

## Testing

### Debug Commands

```csharp
// Manually unlock achievement:
AchievementService.Instance.UnlockAchievement("combat_firstblood");

// Get lifetime stat:
int totalWins = AchievementService.Instance.GetLifetimeStat(AchievementConditionType.WinMatches);

// Check completion:
float completion = AchievementService.Instance.GetCompletionPercentage();
```

### Testing Achievement Unlocks

1. Start the game
2. Open achievement panel (assign to a UI button)
3. Perform actions that trigger achievements
4. Watch unlock notifications appear
5. Check Unity Console for achievement tracking logs

---

## Troubleshooting

### Achievements not appearing

- Ensure AchievementService has achievement templates assigned
- Check that InitializeAchievements() was called
- Enable debug logging in AchievementService

### Achievement progress not updating

- Verify integration components are attached to GameManager/ProgressionManager
- Check that On*() methods are being called
- Enable debug logging to see achievement update events

### Rewards not being awarded

- Check ProgressionManager is connected to AchievementService
- Verify reward values are set in achievement templates
- Check Unity Console for error messages

### UI not displaying

- Verify all UI references are assigned in Inspector
- Check Canvas is set to Screen Space - Overlay
- Ensure AchievementCardUI component is on achievement card prefab

### Platform sync not working

- Verify platform SDK is installed (Steamworks, etc.)
- Enable platform sync in AchievementService
- Check platform achievement IDs are set in templates
- Check Unity Console for platform sync errors

---

## Architecture Notes

**Non-Invasive Design:**
- Integration components attach to existing GameObjects
- No modifications to existing code required
- All tracking happens through public methods

**Lifetime Tracking:**
- All achievement progress is lifetime (never resets)
- Achievements are permanent once unlocked
- Separate from season-based quests

**Scalability:**
- Easy to add new condition types
- Achievement templates can be created without code changes
- Supports unlimited achievement templates

**Performance:**
- Progress updates are instant (no polling)
- Cloud saves are async (non-blocking)
- UI updates only when panel is visible

---

## Next Steps

After setup is complete:

1. Test achievement system in Play Mode
2. Adjust rewards/difficulty based on playtesting
3. Create additional achievement templates for variety
4. Setup Steam/console platform integration
5. Design achievement icons
6. Setup achievement notification sounds/animations
7. Add achievement showcase to player profile

---

## Support

For issues or questions, see:
- Unity Console logs (enable debug logging)
- AchievementService component Inspector
- Achievement template ScriptableObjects

**Common Issues:**
- "AchievementService not available" → Ensure AchievementService GameObject exists in scene
- "Achievement not found" → Check achievement ID matches template
- "Achievement templates empty" → Run Achievement Template Generator
- "Platform sync failed" → Check platform SDK installation and IDs
