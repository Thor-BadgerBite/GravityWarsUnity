# Phase 4+ Architecture Blueprint
## Gravity Wars - Online Multiplayer & Live Services

**Last Updated**: 2025-11-17
**Status**: Architecture Design
**Quality Standard**: Flawless - No compromises on quality, security, or player experience

---

## Table of Contents
1. [Technology Stack](#technology-stack)
2. [Architecture Overview](#architecture-overview)
3. [Feature Breakdown](#feature-breakdown)
4. [Implementation Roadmap](#implementation-roadmap)
5. [Quality Assurance Strategy](#quality-assurance-strategy)
6. [Security & Anti-Cheat](#security--anti-cheat)
7. [Scalability Considerations](#scalability-considerations)

---

## 1. Technology Stack

### Backend Infrastructure

**Option A: Unity Gaming Services (UGS) - RECOMMENDED**
- **Pros**:
  - Native Unity integration
  - Managed infrastructure (no server maintenance)
  - Built-in anti-cheat (Unity Fair Play)
  - Comprehensive analytics
  - Auto-scaling
  - Free tier + reasonable pricing
- **Cons**:
  - Vendor lock-in
  - Limited customization
  - Requires Unity Account authentication

**Services Used**:
- **Netcode for GameObjects** - Multiplayer networking
- **Relay** - P2P relay without dedicated servers
- **Lobby** - Matchmaking and room creation
- **Cloud Save** - Server-side save synchronization
- **Analytics** - Event tracking and funnel analysis
- **Cloud Code** - Server-side game logic validation
- **Economy** - Currency and inventory management
- **Leaderboards** - Global rankings
- **Achievements** - Cross-platform achievements

**Option B: Custom Backend (Self-Hosted)**
- **Technology**: Node.js + Express + PostgreSQL + Redis
- **Networking**: Mirror Networking or Photon 2
- **Pros**:
  - Full control
  - No vendor lock-in
  - Custom business logic
- **Cons**:
  - Requires DevOps expertise
  - Infrastructure costs
  - More development time
  - Must build anti-cheat from scratch

**Option C: Hybrid Approach**
- Unity Gaming Services for core features
- Custom microservices for specialized logic
- Best of both worlds but increased complexity

### **RECOMMENDATION**: Start with Unity Gaming Services (Option A)

**Rationale**:
1. Faster time to market
2. Lower infrastructure costs initially
3. Built-in security and anti-cheat
4. Native Unity SDK integration
5. Can migrate to custom backend later if needed

---

## 2. Architecture Overview

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        UNITY CLIENT                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ GameManager  │  │NetworkManager│  │MatchManager  │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
│         │                  │                  │              │
│  ┌──────▼──────────────────▼──────────────────▼───────┐    │
│  │          ServiceLocator (Singleton Hub)            │    │
│  └──────┬──────┬──────┬──────┬──────┬──────┬─────────┘    │
│         │      │      │      │      │      │                │
│    ┌────▼──┐ ┌▼────┐┌▼────┐┌▼────┐┌▼────┐┌▼────┐          │
│    │Network│ │Cloud││Quest││Achv ││Ldrbr││Anlyt│          │
│    │Service│ │Save││Svc  ││Svc  ││Svc  ││Svc  │          │
│    └───┬───┘ └┬────┘└┬────┘└┬────┘└┬────┘└┬────┘          │
└────────┼──────┼──────┼──────┼──────┼──────┼───────────────┘
         │      │      │      │      │      │
    ─────▼──────▼──────▼──────▼──────▼──────▼─────
         HTTPS/WebSocket/UDP (Encrypted)
    ──────────────────────────────────────────────
         │      │      │      │      │      │
┌────────▼──────▼──────▼──────▼──────▼──────▼───────────────┐
│              UNITY GAMING SERVICES (UGS)                   │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐     │
│  │  Relay   │ │Cloud Save│ │Cloud Code│ │ Economy  │     │
│  │ (Netcode)│ │          │ │(Validate)│ │          │     │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘     │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐     │
│  │  Lobby   │ │Analytics │ │Leaderbrds│ │Achievmts │     │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘     │
└────────────────────────────────────────────────────────────┘
```

### Data Flow Architecture

```
MATCH LIFECYCLE
───────────────

1. MATCHMAKING
   Player → LobbyService.FindMatch() → Lobby Server
   ↓
   Match Found → JoinLobby(lobbyId) → Wait for P2
   ↓
   Both Ready → Relay.AllocateConnection() → Get JoinCode
   ↓
   Connect to Relay Server

2. GAMEPLAY
   Player Actions → NetworkManager.SendRPC()
   ↓
   Relay Server (P2P forwarding)
   ↓
   Opponent receives → GameManager.ProcessAction()
   ↓
   Local Simulation (deterministic physics)
   ↓
   Match End Detection

3. VALIDATION & REWARDS
   Match End → Both clients submit MatchResult
   ↓
   CloudCode.ValidateMatch(result1, result2)
   ↓
   If consensus → Award XP/Currency
   ↓
   CloudSave.UpdateProgression()
   ↓
   Check Quests/Achievements → Award if completed
   ↓
   Update Leaderboards
   ↓
   Analytics.LogMatchComplete()
```

---

## 3. Feature Breakdown

### 3.1 Online Multiplayer Integration

#### Components to Build

**A. NetworkManager (Singleton)**
- Location: `/Assets/Networking/NetworkManager.cs`
- Responsibilities:
  - Unity Transport initialization
  - Relay connection management
  - RPC handling
  - Network state synchronization
  - Disconnect/reconnect handling
  - Latency monitoring

**B. MatchManager (Singleton)**
- Location: `/Assets/Networking/MatchManager.cs`
- Responsibilities:
  - Match state machine
  - Turn synchronization
  - Action validation (client-side prediction)
  - Deterministic physics synchronization
  - Match result calculation
  - Cheating detection (action timing, impossible moves)

**C. LobbyManager (Singleton)**
- Location: `/Assets/Networking/LobbyManager.cs`
- Responsibilities:
  - Quick match (auto-matchmaking)
  - Custom lobby creation
  - Invite friends (future)
  - Lobby chat (future)
  - Player ready states
  - Skill-based matchmaking integration

**D. NetworkedPlayerShip (Component)**
- Location: `/Assets/Networking/NetworkedPlayerShip.cs`
- Extends: `PlayerShip`
- Adds:
  - `NetworkObject` component
  - `NetworkTransform` for position sync
  - `NetworkVariables` for health, moves, perk states
  - RPC methods for firing, moving, perk activation
  - Client-side prediction + server reconciliation

#### Networking Architecture

**Turn-Based Design** (Perfect for Gravity Wars)
- No need for real-time position sync during opponent's turn
- Only sync:
  1. Action inputs (aim angle, power, perk activation)
  2. Physics start state (ship position/velocity)
  3. Missile trajectory checkpoints (validation)
  4. End-of-turn state (health, moves remaining)

**Deterministic Physics**
- Both clients simulate physics identically
- Use fixed timestep (`FixedUpdate`)
- Seed random number generator for consistency
- Validate final states match (anti-cheat)

**Message Types**
```csharp
// RPC Commands
PlayerActionRPC(angle, power, perkSlot)
MissileFireRPC(startPos, angle, velocity, perkData)
MovementRPC(startPos, direction, power)
PerkActivateRPC(slotIndex)
TurnEndRPC(finalHealth, finalPosition, hash)

// State Sync
SyncPlayerState(health, moves, perks, passives)
SyncMissileCheckpoint(position, velocity, fuel, timestamp)
MatchResultRPC(winner, stats, hash)
```

#### Matchmaking Strategy

**Phase 1: Simple Matchmaking**
- Quick Play button → Join first available lobby
- If no lobby → Create new lobby
- Max wait time: 30 seconds → Bot match offer

**Phase 2: Skill-Based Matchmaking (SBMM)**
- Track player ELO/MMR (Elo rating system)
- Match players within ±100 MMR range
- Expand range by 50 every 10 seconds if no match
- Prioritize connection quality (ping < 100ms)

**Phase 3: Advanced Features**
- Ranked/Casual queues
- Tournament brackets
- Custom game lobbies
- Replay system

#### Network Optimization

**Bandwidth Optimization**
- Turn-based = very low bandwidth
- Only send deltas (changed values)
- Compress floats to 16-bit for angles/power
- Batch non-critical updates

**Latency Mitigation**
- Client-side prediction for local player
- Interpolation for opponent actions
- Rollback netcode if states desync (rare)

**Disconnect Handling**
- Auto-reconnect within 30 seconds
- Pause match during reconnect
- AI takeover if disconnect > 30s
- Match abandoned if both disconnect

---

### 3.2 Server-Side Save Sync

#### Current System Analysis
- **Current**: `SaveSystem.cs` saves to local JSON file
- **Problem**: No cloud backup, no multi-device, no anti-cheat

#### New Architecture

**CloudSaveService (Singleton)**
- Location: `/Assets/Networking/Services/CloudSaveService.cs`
- Responsibilities:
  - Wrap Unity Cloud Save API
  - Automatic sync on account changes
  - Conflict resolution (last-write-wins vs merge)
  - Offline queue (pending writes)
  - Version control (detect save corruption)

**Modified SaveSystem**
```csharp
// Hybrid approach
SaveSystem.SavePlayerData(data)
{
    // 1. Save locally (instant)
    SaveToLocalFile(data);

    // 2. Queue cloud sync (async)
    if (NetworkReachability.Available)
        CloudSaveService.Instance.SyncToCloud(data);
    else
        CloudSaveService.Instance.QueueForLater(data);
}

SaveSystem.LoadPlayerData()
{
    // 1. Load from cloud (authoritative)
    if (NetworkReachability.Available)
    {
        var cloudData = await CloudSaveService.Instance.LoadFromCloud();

        // 2. Merge with local if newer
        var localData = LoadFromLocalFile();
        return MergeData(cloudData, localData);
    }

    // 3. Fallback to local if offline
    return LoadFromLocalFile();
}
```

**Conflict Resolution Strategy**

**Option A: Last-Write-Wins (Simple)**
- Use timestamp to determine newest data
- Overwrite older data
- Risk: Data loss if clocks desync

**Option B: Merge Strategy (Complex but Better)**
```csharp
MergeData(cloudData, localData)
{
    // Take highest values for cumulative stats
    merged.accountLevel = Max(cloud.accountLevel, local.accountLevel);
    merged.totalMatchesPlayed = Max(...);

    // Union of unlocked items
    merged.unlockedShips = cloud.unlockedShips.Union(local.unlockedShips);

    // Most recent for settings
    merged.displayName = (cloud.lastLoginDate > local.lastLoginDate)
        ? cloud.displayName
        : local.displayName;

    return merged;
}
```

**Anti-Cheat Integration**
- Server validates progression deltas
- Example: Can't go from Level 1 → Level 50 in 1 second
- Hash save data to detect tampering
- Rate limit save requests

**Data Versioning**
```csharp
[Serializable]
public class PlayerAccountData
{
    public int saveVersion = 2; // Increment on breaking changes
    public string playerID;
    // ... rest of data
}

// Migration logic
if (loadedData.saveVersion < 2)
    MigrateFromV1ToV2(loadedData);
```

---

### 3.3 Analytics Tracking

#### Events to Track

**Player Lifecycle**
- `account_created` (displayName, platform, timestamp)
- `session_start` (accountLevel, playTime, returnFrequency)
- `session_end` (duration, matchesPlayed, xpGained)

**Match Events**
- `match_started` (mode, shipLoadout, opponentShipLoadout, mapPlanets)
- `match_completed` (winner, duration, roundsPlayed, totalDamage, accuracyRate)
- `round_completed` (roundNumber, winner, shotsThisRound, damageTaken)
- `player_action` (actionType: fire/move/perk, success: bool, timestamp)

**Progression Events**
- `account_level_up` (newLevel, xpSource, timeSinceLastLevel)
- `ship_level_up` (loadoutKey, newLevel, matchesPlayed)
- `item_unlocked` (itemType, itemName, unlockSource: battlePass/quest/purchase)
- `currency_earned` (currencyType, amount, source: match/quest/battlePass)
- `currency_spent` (currencyType, amount, itemPurchased)

**Engagement Events**
- `quest_started` (questID, difficulty)
- `quest_progress` (questID, currentProgress, targetProgress)
- `quest_completed` (questID, timeToComplete, reward)
- `achievement_unlocked` (achievementID, rarity, completionPercentage)
- `battlepass_tier_up` (tierNumber, isPremium, reward)

**Economy Events**
- `loadout_created` (loadoutID, shipBody, perks, passives)
- `loadout_equipped` (loadoutID, previousLoadout)
- `missile_changed` (loadoutID, oldMissile, newMissile)

**Monetization Events** (Future)
- `store_viewed` (storeSection)
- `item_clicked` (itemID, price)
- `purchase_initiated` (itemID, price, currencyType)
- `purchase_completed` (itemID, price, success: bool)

#### Analytics Service

**Location**: `/Assets/Networking/Services/AnalyticsService.cs`

```csharp
public class AnalyticsService : MonoBehaviour
{
    public static AnalyticsService Instance { get; private set; }

    // Unity Analytics + Custom backend
    public void TrackEvent(string eventName, Dictionary<string, object> parameters)
    {
        // 1. Send to Unity Analytics
        Analytics.CustomEvent(eventName, parameters);

        // 2. Send to custom backend (future)
        if (customAnalyticsEnabled)
            SendToCustomBackend(eventName, parameters);

        // 3. Log locally for debugging
        if (Debug.isDebugBuild)
            Debug.Log($"[Analytics] {eventName}: {Json(parameters)}");
    }

    // Convenience methods
    public void TrackMatchComplete(MatchResult result) { ... }
    public void TrackLevelUp(int newLevel, string source) { ... }
    public void TrackItemUnlock(string itemType, string itemName) { ... }
}
```

#### Integration Points

**GameManager.cs**
```csharp
void GameOver(PlayerShip winner)
{
    // Existing code...

    // NEW: Track match completion
    AnalyticsService.Instance.TrackMatchComplete(new MatchResult {
        winner = winner.playerName,
        duration = matchDuration,
        roundsPlayed = currentRound,
        player1Loadout = player1Ship.shipPreset.name,
        player2Loadout = player2Ship.shipPreset.name,
        totalDamageDealt = player1TotalDamage + player2TotalDamage,
        // ... more stats
    });
}
```

**ProgressionManager.cs**
```csharp
void CheckAccountLevelUp()
{
    if (xpOverflow >= xpRequired)
    {
        // Existing code...

        // NEW: Track level up
        AnalyticsService.Instance.TrackLevelUp(
            currentPlayerData.accountLevel,
            "match_xp"
        );
    }
}
```

#### Analytics Dashboard

**Unity Analytics Dashboard** (Built-in)
- Funnel analysis (how many players reach Level 10?)
- Retention curves (Day 1, Day 7, Day 30)
- Heatmaps (which ships are most popular?)
- Session duration distribution

**Custom Queries** (SQL on backend)
```sql
-- Most used ship combinations
SELECT shipBody, tier1Perk, tier2Perk, COUNT(*) as matches
FROM match_events
WHERE event_name = 'match_started'
GROUP BY shipBody, tier1Perk, tier2Perk
ORDER BY matches DESC
LIMIT 10;

-- Win rate by ship archetype
SELECT shipArchetype,
       SUM(CASE WHEN winner THEN 1 ELSE 0 END) / COUNT(*) as winRate
FROM match_events
GROUP BY shipArchetype;
```

---

### 3.4 Daily Quests System

#### Quest Types

**Daily Quests** (Refresh every 24 hours)
- Examples:
  - "Win 3 matches" → Reward: 100 soft currency
  - "Deal 30,000 damage" → Reward: 50 soft currency
  - "Fire 20 missiles" → Reward: 25 soft currency
  - "Use a Heavy missile to destroy an opponent" → Reward: 75 soft currency + 10 XP

**Weekly Quests** (Refresh every 7 days)
- Examples:
  - "Win 10 matches" → Reward: 500 soft currency + 100 hard currency
  - "Reach a 5-win streak" → Reward: 300 soft currency
  - "Unlock a new ship" → Reward: 200 soft currency

**Season Quests** (Permanent during season)
- Examples:
  - "Reach Account Level 20" → Reward: Exclusive ship skin
  - "Unlock all Tier 1 perks" → Reward: 1000 soft currency
  - "Complete 50 daily quests" → Reward: Premium battle pass

#### Quest Data Model

```csharp
[Serializable]
public class QuestData : ScriptableObject
{
    public string questID;              // Unique identifier
    public string displayName;
    public string description;
    public QuestType questType;         // Daily, Weekly, Season
    public QuestObjectiveType objective; // WinMatches, DealDamage, etc.

    public int targetValue;             // How many to complete
    public int currentProgress;         // Player's progress

    public List<RewardData> rewards;    // What you get
    public DateTime expiresAt;          // When it expires
    public int requiredAccountLevel;    // Unlock requirement

    public bool IsCompleted => currentProgress >= targetValue;
    public float ProgressPercentage => (float)currentProgress / targetValue;
}

public enum QuestType { Daily, Weekly, Season }

public enum QuestObjectiveType
{
    WinMatches,
    PlayMatches,
    DealDamage,
    DestroyShipsWithMissileType,
    UsePerkNTimes,
    ReachWinStreak,
    UnlockItem,
    ReachAccountLevel,
    EarnCurrency
}

[Serializable]
public class RewardData
{
    public RewardType type;     // XP, SoftCurrency, HardCurrency, Item
    public int amount;
    public string itemID;       // If type = Item
}
```

#### Quest Service

**Location**: `/Assets/Networking/Services/QuestService.cs`

```csharp
public class QuestService : MonoBehaviour
{
    public static QuestService Instance { get; private set; }

    private List<QuestData> activeQuests = new List<QuestData>();

    // Called on login
    public async Task InitializeQuests()
    {
        // 1. Load active quests from cloud save
        activeQuests = await CloudSaveService.LoadQuests();

        // 2. Check if daily/weekly quests need refresh
        RefreshExpiredQuests();

        // 3. Sync with server (get latest quest catalog)
        await SyncQuestCatalog();
    }

    // Called when player performs actions
    public void UpdateQuestProgress(QuestObjectiveType objective, int amount = 1)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.objective == objective && !quest.IsCompleted)
            {
                quest.currentProgress += amount;

                if (quest.IsCompleted)
                {
                    OnQuestCompleted(quest);
                }

                // Save progress
                CloudSaveService.Instance.SaveQuests(activeQuests);
            }
        }
    }

    void OnQuestCompleted(QuestData quest)
    {
        // Award rewards
        foreach (var reward in quest.rewards)
        {
            ProgressionManager.Instance.GrantReward(reward);
        }

        // Analytics
        AnalyticsService.Instance.TrackEvent("quest_completed", new Dictionary<string, object> {
            { "questID", quest.questID },
            { "questType", quest.questType.ToString() },
            { "timeToComplete", (DateTime.Now - quest.expiresAt.AddDays(-1)).TotalHours }
        });

        // UI notification
        UIManager.Instance.ShowQuestCompletePopup(quest);
    }

    void RefreshExpiredQuests()
    {
        // Remove expired quests
        activeQuests.RemoveAll(q => DateTime.Now > q.expiresAt);

        // Generate new daily quests if needed
        if (activeQuests.Count(q => q.questType == QuestType.Daily) < 3)
        {
            GenerateDailyQuests();
        }

        // Generate weekly quests
        if (activeQuests.Count(q => q.questType == QuestType.Weekly) < 3)
        {
            GenerateWeeklyQuests();
        }
    }
}
```

#### Integration Points

**GameManager.cs**
```csharp
void EndTurn()
{
    // Existing code...

    // NEW: Update quest progress
    if (shotFiredThisTurn)
    {
        QuestService.Instance.UpdateQuestProgress(
            QuestObjectiveType.FireMissiles, 1
        );
    }
}

void GameOver(PlayerShip winner)
{
    // Existing code...

    // NEW: Update quest progress
    QuestService.Instance.UpdateQuestProgress(
        QuestObjectiveType.PlayMatches, 1
    );

    if (winner == player1Ship && isLocalPlayer)
    {
        QuestService.Instance.UpdateQuestProgress(
            QuestObjectiveType.WinMatches, 1
        );
    }

    QuestService.Instance.UpdateQuestProgress(
        QuestObjectiveType.DealDamage,
        (int)totalDamageDealtThisMatch
    );
}
```

#### Quest UI

**Location**: `/Assets/Progression System/UI/QuestUI.cs`

**UI Elements**:
- Quest panel (slide-in from left)
- List of active quests with progress bars
- Claim button when complete
- Notification badge when new quest available
- Expiration timer for daily/weekly quests

---

### 3.5 Achievements System

#### Achievement Categories

**Combat Achievements**
- "First Blood" - Win your first match
- "Sharpshooter" - Hit 10 shots in a row
- "Heavy Hitter" - Deal 50,000 damage in one match
- "Untouchable" - Win a match without taking damage
- "Master Tactician" - Win using only movement (no shooting)

**Progression Achievements**
- "Rising Star" - Reach Account Level 10
- "Veteran" - Reach Account Level 50
- "Ship Master" - Max level a ship to Level 20
- "Collector" - Unlock all ships
- "Arsenal" - Unlock all perks

**Skill Achievements**
- "Gravity Wizard" - Hit a shot that orbits 3 planets
- "Sniper Elite" - Hit from 100+ units away
- "Close Call" - Win with less than 100 HP remaining
- "Comeback King" - Win after being down 0-2 in rounds

**Social Achievements** (Future)
- "Friendly Fire" - Play 10 matches with friends
- "Tournament Victor" - Win a tournament

**Hidden Achievements**
- "Secret discoveries" - Easter eggs in maps
- "Perfect Game" - Win 10-0 across all rounds in a match

#### Achievement Data Model

```csharp
[Serializable]
public class AchievementData : ScriptableObject
{
    public string achievementID;
    public string displayName;
    public string description;
    public string hiddenDescription;    // Shown if isHidden = true

    public bool isHidden;               // Don't show until unlocked
    public AchievementRarity rarity;    // Common, Rare, Epic, Legendary

    public Sprite icon;
    public Sprite lockedIcon;

    public AchievementTrigger trigger;  // What triggers this
    public int targetValue;             // How many times
    public int currentProgress;         // Player's progress

    public List<RewardData> rewards;

    public DateTime unlockedAt;
    public bool IsUnlocked => currentProgress >= targetValue;
}

public enum AchievementRarity { Common, Rare, Epic, Legendary }

public enum AchievementTrigger
{
    WinMatches,
    WinStreak,
    ReachAccountLevel,
    ReachShipLevel,
    DealDamageInMatch,
    HitShotDistance,
    WinWithoutDamage,
    UnlockAllShips,
    // ... many more
}
```

#### Achievement Service

**Location**: `/Assets/Networking/Services/AchievementService.cs`

```csharp
public class AchievementService : MonoBehaviour
{
    public static AchievementService Instance { get; private set; }

    public List<AchievementData> allAchievements;  // Master list
    private Dictionary<string, AchievementData> playerAchievements;

    public async Task Initialize()
    {
        // Load from cloud save
        playerAchievements = await CloudSaveService.LoadAchievements();

        // Sync with Unity Achievements API
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            SyncWithGameCenter();
        else if (Application.platform == RuntimePlatform.Android)
            SyncWithGooglePlay();
    }

    public void CheckAchievement(AchievementTrigger trigger, int value)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.trigger == trigger && !achievement.IsUnlocked)
            {
                if (value >= achievement.targetValue)
                {
                    UnlockAchievement(achievement);
                }
                else
                {
                    UpdateAchievementProgress(achievement, value);
                }
            }
        }
    }

    void UnlockAchievement(AchievementData achievement)
    {
        achievement.unlockedAt = DateTime.Now;
        achievement.currentProgress = achievement.targetValue;

        // Award rewards
        foreach (var reward in achievement.rewards)
        {
            ProgressionManager.Instance.GrantReward(reward);
        }

        // Analytics
        AnalyticsService.Instance.TrackEvent("achievement_unlocked", new Dictionary<string, object> {
            { "achievementID", achievement.achievementID },
            { "rarity", achievement.rarity.ToString() }
        });

        // Platform achievements
        ReportToPlatform(achievement.achievementID);

        // UI popup
        UIManager.Instance.ShowAchievementUnlocked(achievement);

        // Save
        CloudSaveService.Instance.SaveAchievements(playerAchievements);
    }
}
```

#### Integration Points

**GameManager.cs**
```csharp
void GameOver(PlayerShip winner)
{
    // Existing code...

    // NEW: Check achievements
    if (winner == player1Ship)
    {
        AchievementService.Instance.CheckAchievement(
            AchievementTrigger.WinMatches,
            ProgressionManager.Instance.currentPlayerData.totalMatchesWon
        );

        if (loser.currentHealth == loser.maxHealth)
        {
            AchievementService.Instance.CheckAchievement(
                AchievementTrigger.WinWithoutDamage, 1
            );
        }
    }
}
```

**Missile3D.cs**
```csharp
void OnCollisionEnter(Collision collision)
{
    // Existing code...

    // NEW: Track shot distance for achievement
    float shotDistance = Vector3.Distance(launchPosition, collision.transform.position);

    if (shotDistance > 100f)
    {
        AchievementService.Instance.CheckAchievement(
            AchievementTrigger.HitShotDistance,
            (int)shotDistance
        );
    }
}
```

---

### 3.6 Leaderboards System

#### Leaderboard Types

**Global Leaderboards**
1. **Account Level** - Highest account levels
2. **Win Rate** - Best win percentage (min 20 matches)
3. **Total Wins** - Most matches won
4. **Damage Dealt** - Lifetime damage
5. **Win Streak** - Longest current win streak

**Ship-Specific Leaderboards**
6. **Tank Masters** - Best Tank archetype players
7. **Damage Dealer Aces** - Best DD players
8. **Controller Experts** - Best Controller players
9. **All-Around Champions** - Best All-Around players

**Time-Based Leaderboards**
10. **Daily Top Players** - Today's best
11. **Weekly Champions** - This week's top
12. **Season Rankings** - Current season standings

**Special Leaderboards**
13. **Tournament Ladder** - Tournament MMR
14. **Clan Rankings** (Future) - Best clans

#### Leaderboard Data Model

```csharp
[Serializable]
public class LeaderboardEntry
{
    public int rank;
    public string playerID;
    public string displayName;
    public int score;               // Varies by leaderboard type
    public string metadata;         // JSON for extra info (ship loadout, etc.)
    public DateTime lastUpdated;

    // Visual extras
    public Sprite profilePicture;   // Future
    public string clanTag;          // Future
}

public enum LeaderboardType
{
    AccountLevel,
    WinRate,
    TotalWins,
    DamageDealt,
    WinStreak,
    TankMasters,
    DamageDealerAces,
    ControllerExperts,
    AllAroundChampions,
    DailyTop,
    WeeklyChampions,
    SeasonRankings
}
```

#### Leaderboard Service

**Location**: `/Assets/Networking/Services/LeaderboardService.cs`

```csharp
public class LeaderboardService : MonoBehaviour
{
    public static LeaderboardService Instance { get; private set; }

    // Fetch top N players for a leaderboard
    public async Task<List<LeaderboardEntry>> GetLeaderboard(
        LeaderboardType type,
        int offset = 0,
        int limit = 100)
    {
        // Call Unity Leaderboards API
        var entries = await Leaderboards.GetScoresAsync(
            leaderboardId: type.ToString(),
            offset: offset,
            limit: limit
        );

        return ParseEntries(entries);
    }

    // Get player's rank on a leaderboard
    public async Task<int> GetPlayerRank(LeaderboardType type, string playerID)
    {
        var result = await Leaderboards.GetPlayerRangeAsync(
            leaderboardId: type.ToString(),
            playerIds: new[] { playerID }
        );

        return result.PlayerScores[0].Rank;
    }

    // Get players around you (e.g., ±10 ranks)
    public async Task<List<LeaderboardEntry>> GetPlayersAroundMe(
        LeaderboardType type,
        int range = 10)
    {
        var myRank = await GetPlayerRank(type, ProgressionManager.Instance.currentPlayerData.playerID);

        var startRank = Mathf.Max(0, myRank - range);
        var entries = await GetLeaderboard(type, startRank, range * 2 + 1);

        return entries;
    }

    // Update player's score on a leaderboard
    public async Task UpdateLeaderboard(LeaderboardType type, int newScore)
    {
        await Leaderboards.AddPlayerScoreAsync(
            leaderboardId: type.ToString(),
            score: newScore,
            metadata: GetMetadataForLeaderboard(type)
        );
    }

    string GetMetadataForLeaderboard(LeaderboardType type)
    {
        // Attach extra info (e.g., ship loadout for archetype leaderboards)
        var data = ProgressionManager.Instance.currentPlayerData;

        return JsonUtility.ToJson(new {
            accountLevel = data.accountLevel,
            winRate = (float)data.totalMatchesWon / data.totalMatchesPlayed,
            favoriteShip = GetMostUsedShip()
        });
    }
}
```

#### Integration Points

**ProgressionManager.cs**
```csharp
void AwardMatchXP(bool won, int roundsWon, int damageDealt, CustomShipLoadout usedLoadout)
{
    // Existing code...

    // NEW: Update leaderboards
    LeaderboardService.Instance.UpdateLeaderboard(
        LeaderboardType.TotalWins,
        currentPlayerData.totalMatchesWon
    );

    LeaderboardService.Instance.UpdateLeaderboard(
        LeaderboardType.DamageDealt,
        currentPlayerData.totalDamageDealt
    );

    float winRate = (float)currentPlayerData.totalMatchesWon / currentPlayerData.totalMatchesPlayed;
    if (currentPlayerData.totalMatchesPlayed >= 20)
    {
        LeaderboardService.Instance.UpdateLeaderboard(
            LeaderboardType.WinRate,
            (int)(winRate * 10000) // Store as integer (e.g., 75.5% = 7550)
        );
    }

    // Update archetype-specific leaderboards
    var archetype = GetArchetypeFromLoadout(usedLoadout);
    LeaderboardService.Instance.UpdateLeaderboard(
        GetLeaderboardForArchetype(archetype),
        currentPlayerData.totalMatchesWon
    );
}
```

#### Leaderboard UI

**Location**: `/Assets/Progression System/UI/LeaderboardUI.cs`

**UI Elements**:
- Dropdown to select leaderboard type
- Scrollable list of entries (rank, name, score)
- Highlight player's entry
- "View Around Me" button
- Filter options (Friends only, Clan only)
- Refresh button with cooldown
- Prize indicators for top 10/100

---

## 4. Implementation Roadmap

### Phase 4.1: Foundation (Weeks 1-2)
**Goal**: Set up infrastructure and core networking

**Milestones**:
1. Unity Gaming Services setup
   - Create UGS project
   - Install packages (Netcode, Relay, Lobby, Cloud Save, Analytics)
   - Configure authentication
   - Test basic connectivity

2. Service architecture
   - Create `ServiceLocator` singleton hub
   - Create `NetworkService` wrapper
   - Create `CloudSaveService` wrapper
   - Create `AnalyticsService` wrapper

3. Modified save system
   - Extend `SaveSystem` with cloud sync
   - Implement conflict resolution
   - Add offline queue
   - Test multi-device sync

**Deliverables**:
- ✅ UGS project configured
- ✅ All service singletons created
- ✅ Cloud save working with conflict resolution
- ✅ Basic analytics tracking

**Quality Gates**:
- [ ] Can authenticate player
- [ ] Can save/load from cloud
- [ ] Offline queue works
- [ ] Analytics events appear in dashboard

---

### Phase 4.2: Online Multiplayer (Weeks 3-6)
**Goal**: Full online PvP matchmaking and gameplay

**Milestones**:
1. **NetworkManager implementation**
   - Unity Transport setup
   - Relay integration
   - RPC framework
   - Network state sync

2. **LobbyManager implementation**
   - Quick match flow
   - Lobby creation/joining
   - Player ready states
   - Connection handshake

3. **MatchManager implementation**
   - Turn synchronization
   - Action validation
   - Deterministic physics
   - Match result validation

4. **NetworkedPlayerShip**
   - Extend `PlayerShip` with networking
   - Client-side prediction
   - Server reconciliation
   - Perk synchronization

5. **UI updates**
   - Online mode menu
   - Matchmaking screen
   - Connection status indicators
   - Latency display

**Deliverables**:
- ✅ Quick match working end-to-end
- ✅ Deterministic physics across clients
- ✅ Match results validated server-side
- ✅ Disconnect/reconnect handling

**Quality Gates**:
- [ ] Can find match in <30 seconds
- [ ] Physics perfectly synced (no desyncs)
- [ ] Latency <100ms feels responsive
- [ ] Disconnect recovery works
- [ ] No exploitable cheats

---

### Phase 4.3: Analytics & Economy (Week 7)
**Goal**: Track all player actions and validate economy

**Milestones**:
1. **Analytics integration**
   - Instrument all game events
   - Test event flow to dashboard
   - Create custom analytics queries
   - Set up funnels/retention tracking

2. **Economy validation**
   - Server-side XP validation
   - Currency anti-cheat
   - Rate limiting
   - Suspicious activity detection

**Deliverables**:
- ✅ 30+ analytics events tracked
- ✅ Dashboard showing player behavior
- ✅ Server validates all progression
- ✅ Anti-cheat detects impossible progressions

**Quality Gates**:
- [ ] All events appear in dashboard within 5 minutes
- [ ] Server rejects tampered save data
- [ ] Impossible XP gains blocked

---

### Phase 4.4: Quests System (Weeks 8-9)
**Goal**: Daily/Weekly/Season quests with rewards

**Milestones**:
1. **Quest data model**
   - Create `QuestData` ScriptableObject
   - Define 20+ quest templates
   - Set up reward system

2. **QuestService implementation**
   - Quest refresh logic
   - Progress tracking
   - Reward granting
   - Cloud sync

3. **Quest UI**
   - Quest panel
   - Progress bars
   - Claim buttons
   - Notifications

4. **Integration**
   - Hook quest updates into GameManager
   - Track all relevant actions
   - Test quest completion flow

**Deliverables**:
- ✅ 20+ quests defined
- ✅ Quest refresh working daily/weekly
- ✅ Rewards granted correctly
- ✅ UI shows progress in real-time

**Quality Gates**:
- [ ] Quests refresh at midnight UTC
- [ ] Progress syncs across devices
- [ ] Rewards can't be double-claimed
- [ ] Expired quests removed

---

### Phase 4.5: Achievements System (Week 10)
**Goal**: 50+ achievements with platform integration

**Milestones**:
1. **Achievement data model**
   - Create `AchievementData` ScriptableObject
   - Define 50+ achievements (common → legendary)
   - Design achievement icons

2. **AchievementService implementation**
   - Achievement checking logic
   - Platform API integration (Game Center, Google Play)
   - Unlock flow
   - Cloud sync

3. **Achievement UI**
   - Achievement list screen
   - Unlock popup
   - Rarity indicators
   - Progress tracking

4. **Integration**
   - Hook achievement checks into game events
   - Test hidden achievements
   - Test rare edge-case achievements

**Deliverables**:
- ✅ 50+ achievements defined
- ✅ Platform achievements synced
- ✅ Unlock popups working
- ✅ All achievements testable

**Quality Gates**:
- [ ] Achievements unlock reliably
- [ ] Platform sync works (iOS/Android)
- [ ] Hidden achievements stay hidden
- [ ] No duplicate unlocks

---

### Phase 4.6: Leaderboards System (Week 11)
**Goal**: Global and archetype-specific leaderboards

**Milestones**:
1. **Leaderboard configuration**
   - Create 10+ leaderboard types in UGS
   - Set up reset schedules (daily/weekly/seasonal)
   - Configure pagination

2. **LeaderboardService implementation**
   - Fetch leaderboard data
   - Update player scores
   - Get player rank
   - Get players around you

3. **Leaderboard UI**
   - Leaderboard screen
   - Type selector
   - Scrollable list
   - Rank highlights

4. **Integration**
   - Update leaderboards after matches
   - Test ranking accuracy
   - Test reset schedules

**Deliverables**:
- ✅ 10+ leaderboards live
- ✅ Real-time rank updates
- ✅ UI shows top 100 + player rank
- ✅ Daily/weekly resets working

**Quality Gates**:
- [ ] Rankings accurate within 1 minute
- [ ] Pagination works smoothly
- [ ] Player can find their rank quickly
- [ ] Cheaters can be banned from leaderboards

---

### Phase 4.7: Polish & Optimization (Week 12)
**Goal**: Bug fixes, performance, and final touches

**Tasks**:
1. **Performance optimization**
   - Reduce network bandwidth
   - Optimize cloud save frequency
   - Cache leaderboard queries
   - Compress analytics payloads

2. **Bug fixes**
   - Fix all known issues
   - Edge case testing
   - Stress testing (many concurrent players)

3. **UI polish**
   - Loading states
   - Error messages
   - Offline mode UX
   - Accessibility

4. **Documentation**
   - API documentation
   - Player-facing help docs
   - Admin tools guide

**Deliverables**:
- ✅ No critical bugs
- ✅ Performance targets met
- ✅ All features documented

**Quality Gates**:
- [ ] 60 FPS maintained during online matches
- [ ] No memory leaks
- [ ] All UI states handled
- [ ] Documentation complete

---

### Phase 4.8: Testing & Launch (Week 13)
**Goal**: Final QA and soft launch

**Tasks**:
1. **Beta testing**
   - Recruit 50-100 beta testers
   - Monitor for issues
   - Collect feedback

2. **Load testing**
   - Simulate 1000+ concurrent players
   - Test server capacity
   - Monitor costs

3. **Security audit**
   - Penetration testing
   - Anti-cheat validation
   - Data privacy compliance

4. **Soft launch**
   - Release to small region
   - Monitor analytics
   - Fix critical issues

5. **Global launch**
   - Full release
   - Marketing push
   - Post-launch support

**Deliverables**:
- ✅ Beta test complete
- ✅ Security audit passed
- ✅ Soft launch successful
- ✅ Global launch

---

## 5. Quality Assurance Strategy

### Testing Approach

**Unit Testing**
- Use NUnit for Unity
- Test all service classes
- Mock network calls
- Aim for 80%+ code coverage

**Integration Testing**
- Test service interactions
- Test save/load flow
- Test network sync
- Test quest/achievement unlocks

**End-to-End Testing**
- Full match flow (matchmaking → gameplay → results)
- Multi-device save sync
- Offline → online transitions

**Load Testing**
- Simulate 1000+ concurrent matches
- Test server response times
- Monitor bandwidth usage
- Test database query performance

**Security Testing**
- Attempt to cheat (modify saves, inject invalid actions)
- Test authentication bypass
- SQL injection attempts (if using custom backend)
- Rate limiting validation

### Quality Metrics

**Performance**
- Maintain 60 FPS during online matches
- Matchmaking time <30 seconds
- Cloud save sync <2 seconds
- Leaderboard load time <1 second
- Memory usage <500 MB

**Reliability**
- 99.9% uptime (8.7 hours downtime per year)
- <0.1% match desync rate
- <1% save data corruption
- 100% achievement unlock accuracy

**User Experience**
- <3 clicks to start a match
- Clear error messages for all failures
- Offline mode fully functional
- <100ms input latency

---

## 6. Security & Anti-Cheat

### Threat Model

**Client-Side Attacks**
1. Save file tampering (edit JSON to give max level)
2. Memory editing (Cheat Engine to modify health)
3. Network packet injection (send fake "I won" messages)
4. Time manipulation (speed up quest timers)

**Server-Side Attacks**
1. DDoS attacks
2. SQL injection (if custom backend)
3. API abuse (spam requests)
4. Account takeover

### Defense Strategies

**Save Data Protection**
```csharp
// Hash save data to detect tampering
[Serializable]
public class PlayerAccountData
{
    // ... existing fields

    public string dataHash;  // SHA256 hash of all fields

    public void ComputeHash()
    {
        var json = JsonUtility.ToJson(this);
        dataHash = SHA256(json);
    }

    public bool ValidateHash()
    {
        var currentHash = dataHash;
        ComputeHash();
        return currentHash == dataHash;
    }
}

// Server validates on load
if (!loadedData.ValidateHash())
{
    // Potential tampering detected
    LogSuspiciousActivity(playerID);
    ResetToLastKnownGoodSave();
}
```

**Server-Side Validation**
```csharp
// Cloud Code validates match results
[CloudCodeFunction]
public static MatchValidationResult ValidateMatch(
    MatchResult player1Result,
    MatchResult player2Result)
{
    // Both players must submit results
    if (player1Result.matchID != player2Result.matchID)
        return MatchValidationResult.Reject("Match ID mismatch");

    // Results must agree on winner
    if (player1Result.winner != player2Result.winner)
        return MatchValidationResult.Reject("Winner disagreement - investigating");

    // Results must agree on final scores (within tolerance)
    if (Abs(player1Result.finalScore1 - player2Result.finalScore1) > 100)
        return MatchValidationResult.Reject("Score mismatch");

    // XP gain must be reasonable
    if (player1Result.xpGained > MAX_XP_PER_MATCH)
        return MatchValidationResult.Reject("XP too high");

    // Match duration must be reasonable
    if (player1Result.duration < 30 || player1Result.duration > 600)
        return MatchValidationResult.Reject("Unrealistic match duration");

    return MatchValidationResult.Accept;
}
```

**Rate Limiting**
```csharp
// Prevent API spam
public class RateLimiter
{
    private Dictionary<string, Queue<DateTime>> requestHistory = new();

    public bool AllowRequest(string playerID, int maxRequestsPerMinute = 60)
    {
        if (!requestHistory.ContainsKey(playerID))
            requestHistory[playerID] = new Queue<DateTime>();

        var history = requestHistory[playerID];
        var oneMinuteAgo = DateTime.Now.AddMinutes(-1);

        // Remove old requests
        while (history.Count > 0 && history.Peek() < oneMinuteAgo)
            history.Dequeue();

        // Check limit
        if (history.Count >= maxRequestsPerMinute)
            return false;

        // Log request
        history.Enqueue(DateTime.Now);
        return true;
    }
}
```

**Deterministic Physics Validation**
```csharp
// Both clients simulate physics identically
// Compare checksums every N frames to detect desyncs

public class PhysicsValidator
{
    public string GetStateChecksum()
    {
        var state = new {
            player1Pos = player1Ship.transform.position,
            player1Health = player1Ship.currentHealth,
            player2Pos = player2Ship.transform.position,
            player2Health = player2Ship.currentHealth,
            missilePos = activeMissile?.transform.position,
            frameNumber = Time.frameCount
        };

        return SHA256(JsonUtility.ToJson(state));
    }

    public void ValidateStateWithOpponent(string opponentChecksum)
    {
        var myChecksum = GetStateChecksum();

        if (myChecksum != opponentChecksum)
        {
            Debug.LogWarning("Physics desync detected! Rolling back...");
            // Trigger rollback to last known good state
            RollbackPhysics();
        }
    }
}
```

**Anti-Cheat Summary**
- Save data hashed and validated server-side
- Match results cross-validated between clients
- Server applies logic validation (XP caps, duration limits)
- Rate limiting prevents API abuse
- Deterministic physics with checksum validation
- Suspicious activity logged for manual review
- Progressive bans (warning → temp ban → perma ban)

---

## 7. Scalability Considerations

### Infrastructure Scaling

**Unity Gaming Services** handles most scaling automatically:
- Relay servers auto-scale
- Cloud Save uses CDN
- Analytics ingests millions of events
- Leaderboards optimized for reads

**Custom Backend** (if needed later):
- Use Kubernetes for auto-scaling
- PostgreSQL with read replicas
- Redis cache for leaderboards/sessions
- CloudFlare CDN for static assets

### Database Design

**Player Data** (PostgreSQL)
```sql
-- Main account table
CREATE TABLE accounts (
    player_id UUID PRIMARY KEY,
    display_name VARCHAR(50),
    account_level INT,
    account_xp INT,
    soft_currency INT,
    hard_currency INT,
    created_at TIMESTAMP,
    last_login TIMESTAMP
);

-- Ship progression (many-to-one with accounts)
CREATE TABLE ship_progression (
    id SERIAL PRIMARY KEY,
    player_id UUID REFERENCES accounts(player_id),
    loadout_key VARCHAR(255),
    ship_level INT,
    ship_xp INT,
    matches_played INT,
    wins INT
);

-- Match history
CREATE TABLE match_history (
    match_id UUID PRIMARY KEY,
    player1_id UUID,
    player2_id UUID,
    winner_id UUID,
    duration INT,
    rounds_played INT,
    created_at TIMESTAMP
);

-- Indexes for performance
CREATE INDEX idx_accounts_last_login ON accounts(last_login);
CREATE INDEX idx_ship_progression_player ON ship_progression(player_id);
CREATE INDEX idx_match_history_player1 ON match_history(player1_id);
CREATE INDEX idx_match_history_player2 ON match_history(player2_id);
```

**Leaderboard Optimization**
- Use materialized views for computed rankings
- Refresh every 5 minutes (not real-time)
- Cache top 100 in Redis
- Use pagination for deep queries

**Analytics Storage**
- Use time-series database (InfluxDB or TimescaleDB)
- Aggregate old data (store hourly/daily summaries after 30 days)
- Archive data >1 year to cold storage

### Cost Optimization

**Unity Gaming Services Pricing** (Estimated)
- Free tier: Up to 50 CCU (concurrent users)
- Beyond free tier: ~$0.05 per active user per month

**Projections**:
- 1,000 DAU (daily active users) = ~$50/month
- 10,000 DAU = ~$500/month
- 100,000 DAU = ~$5,000/month

**Cost Reduction Strategies**:
- Compress network payloads
- Cache leaderboard queries client-side
- Batch analytics events
- Use CDN for static assets
- Archive old analytics data

---

## 8. Rollout Strategy

### Soft Launch Regions
Start with a small region to test infrastructure:
- Canada or Australia (English-speaking, smaller population)
- Monitor for 1-2 weeks
- Fix critical issues
- Gather feedback

### Global Launch
- Stagger by region (Americas → Europe → Asia)
- Monitor server load
- Scale infrastructure proactively
- 24/7 support during first week

### Post-Launch Support
- Weekly updates for first month
- Bi-weekly balance patches
- Monthly content updates (new ships, perks, quests)
- Seasonal events every 3 months

---

## 9. Next Steps - Your Decision Points

Before we start coding, I need your decisions on:

### **A. Technology Stack**
1. ✅ Approve Unity Gaming Services as primary backend?
2. ✅ Approve Netcode for GameObjects for networking?
3. Do you want a custom backend eventually, or stick with UGS?

### **B. Scope**
1. Should we implement ALL Phase 4+ features, or prioritize some first?
   - Recommendation: Prioritize Multiplayer → Save Sync → Analytics → Quests → Achievements → Leaderboards
2. Any features you want to add beyond the list?

### **C. Timeline**
1. Are you OK with 13 weeks (3 months) for full Phase 4+ implementation?
2. Do you want weekly progress reviews?

### **D. Testing**
1. Do you have beta testers available?
2. Should we set up automated testing (unit tests, integration tests)?

### **E. Monetization** (Future)
1. Are you planning to monetize (IAP, battle pass sales)?
2. If yes, should we design the economy with that in mind?

---

## 10. Let's Begin

Once you approve this architecture, I'll start with **Phase 4.1: Foundation** and work through each phase methodically.

**My promise to you**:
- ✅ Flawless code with comments and documentation
- ✅ Security-first design (anti-cheat, validation)
- ✅ Scalable architecture (supports 100,000+ players)
- ✅ Comprehensive testing before moving to next phase
- ✅ Regular progress updates
- ✅ Clean, maintainable code following your existing patterns

**Ready to build something amazing?** Let me know your decisions on the points above, and we'll start coding! 🚀
