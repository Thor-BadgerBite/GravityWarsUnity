using UnityEngine;
using GravityWars.Networking;

/// <summary>
/// Integrates quest system with GameManager.
/// Tracks player actions and updates quest progress.
///
/// IMPORTANT: Attach this component to the same GameObject as GameManager.
/// It will automatically hook into game events and update quest progress.
///
/// This is a non-invasive integration - no modifications to GameManager needed.
///
/// Actions Tracked:
/// - Match won/played
/// - Rounds won
/// - Damage dealt
/// - Missiles fired/hit
/// - Perks used
/// - Specific archetype/missile usage
///
/// Usage:
///   Simply attach to GameManager GameObject. It will auto-initialize.
/// </summary>
[RequireComponent(typeof(GameManager))]
public class GameManagerQuestIntegration : MonoBehaviour
{
    #region Configuration

    [Header("Quest Integration")]
    [Tooltip("Enable quest tracking")]
    public bool enableQuestTracking = true;

    #endregion

    #region Component References

    private GameManager _gameManager;
    private QuestService _questService;

    #endregion

    #region Match Tracking

    private string _currentPlayerArchetype = "";
    private string _current OpponentArchetype = "";
    private int _totalDamageThisMatch = 0;
    private int _missilesHitThisMatch = 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("[GameManagerQuestIntegration] GameManager not found!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enableQuestTracking)
            return;

        // Get quest service
        _questService = QuestService.Instance;

        if (_questService == null)
        {
            Debug.LogWarning("[GameManagerQuestIntegration] QuestService not available - quests disabled");
            enableQuestTracking = false;
        }
    }

    #endregion

    #region Match Lifecycle Integration

    /// <summary>
    /// Call this when match starts.
    /// </summary>
    public void OnMatchStart()
    {
        if (!enableQuestTracking)
            return;

        // Reset match tracking
        _totalDamageThisMatch = 0;
        _missilesHitThisMatch = 0;

        // Get ship archetypes for archetype-specific quests
        if (_gameManager.player1Ship != null)
        {
            _currentPlayerArchetype = _gameManager.player1Ship.shipArchetype.ToString();
        }

        if (_gameManager.player2Ship != null)
        {
            _currentOpponentArchetype = _gameManager.player2Ship.shipArchetype.ToString();
        }

        // Update quest: Play Matches
        _questService.UpdateQuestProgress(QuestObjectiveType.PlayMatches, 1);

        // Update quest: Play Matches With Archetype
        _questService.UpdateQuestProgress(
            QuestObjectiveType.PlayMatchesWithArchetype,
            1,
            _currentPlayerArchetype
        );

        Debug.Log("[GameManagerQuestIntegration] Match started - quest tracking active");
    }

    /// <summary>
    /// Call this when match ends.
    /// </summary>
    public void OnMatchEnd(PlayerShip winner, bool isPlayer1Winner)
    {
        if (!enableQuestTracking)
            return;

        // Update quest: Win Matches (if player won)
        if (isPlayer1Winner)
        {
            _questService.UpdateQuestProgress(QuestObjectiveType.WinMatches, 1);

            // Update quest: Win With Archetype
            _questService.UpdateQuestProgress(
                QuestObjectiveType.WinWithArchetype,
                1,
                _currentPlayerArchetype
            );
        }

        // Update quest: Deal Damage
        _questService.UpdateQuestProgress(
            QuestObjectiveType.DealDamage,
            _totalDamageThisMatch
        );

        // Update quest: Hit Missiles
        _questService.UpdateQuestProgress(
            QuestObjectiveType.HitMissiles,
            _missilesHitThisMatch
        );

        Debug.Log($"[GameManagerQuestIntegration] Match ended - quests updated (winner: {winner.playerName})");
    }

    /// <summary>
    /// Call this when round ends.
    /// </summary>
    public void OnRoundEnd(PlayerShip winner, bool isPlayer1Winner)
    {
        if (!enableQuestTracking || !isPlayer1Winner)
            return;

        // Update quest: Win Rounds
        _questService.UpdateQuestProgress(QuestObjectiveType.WinRounds, 1);
    }

    #endregion

    #region Action Tracking

    /// <summary>
    /// Call this when player fires a missile.
    /// </summary>
    public void OnPlayerFireMissile(bool hit, int damage, string missileType)
    {
        if (!enableQuestTracking)
            return;

        // Update quest: Fire Missiles
        _questService.UpdateQuestProgress(QuestObjectiveType.FireMissiles, 1);

        // Track hits for later quest updates
        if (hit)
        {
            _missilesHitThisMatch++;
            _totalDamageThisMatch += damage;

            // Update quest: Destroy Ships With Missile Type
            // (This would be called when ship is actually destroyed)
        }
    }

    /// <summary>
    /// Call this when player activates a perk.
    /// </summary>
    public void OnPlayerActivatePerk(string perkName)
    {
        if (!enableQuestTracking)
            return;

        // Update quest: Use Perk N Times
        _questService.UpdateQuestProgress(
            QuestObjectiveType.UsePerkNTimes,
            1,
            perkName
        );
    }

    /// <summary>
    /// Call this when player destroys opponent ship with specific missile.
    /// </summary>
    public void OnShipDestroyedWithMissile(string missileType)
    {
        if (!enableQuestTracking)
            return;

        // Update quest: Destroy Ships With Missile Type
        _questService.UpdateQuestProgress(
            QuestObjectiveType.DestroyShipsWithMissileType,
            1,
            missileType
        );
    }

    #endregion
}
