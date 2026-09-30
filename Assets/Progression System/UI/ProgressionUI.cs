using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays player account progression (level, XP, stats).
/// Shows on main menu or as overlay during gameplay.
/// </summary>
public class ProgressionUI : MonoBehaviour
{
    [Header("References")]
    public ProgressionManager progressionManager;

    [Header("Account Info")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI accountLevelText;
    public Image accountXPBar;
    public TextMeshProUGUI accountXPText;

    [Header("Currency Display")]
    public TextMeshProUGUI softCurrencyText;
    public TextMeshProUGUI hardCurrencyText;

    [Header("Stats Display")]
    public TextMeshProUGUI totalMatchesText;
    public TextMeshProUGUI winRateText;
    public TextMeshProUGUI totalDamageText;

    [Header("Ship Progression (Selected Ship)")]
    public GameObject shipProgressionPanel;
    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipLevelText;
    public Image shipXPBar;
    public TextMeshProUGUI shipXPText;
    public TextMeshProUGUI shipStatsText;

    [Header("Next Unlock Preview")]
    public GameObject nextUnlockPanel;
    public Image nextUnlockIcon;
    public TextMeshProUGUI nextUnlockText;
    public TextMeshProUGUI nextUnlockLevelText;

    private CustomShipLoadout currentlyViewedShip;

    void Start()
    {
        if (progressionManager == null)
            progressionManager = ProgressionManager.Instance;

        RefreshUI();
    }

    /// <summary>
    /// Refreshes all UI elements with current player data
    /// </summary>
    public void RefreshUI()
    {
        if (progressionManager == null || progressionManager.currentPlayerData == null)
            return;

        PlayerAccountData data = progressionManager.currentPlayerData;

        UpdateAccountInfo(data);
        UpdateCurrency(data);
        UpdateStats(data);
        UpdateShipProgression();
        UpdateNextUnlock(data);
    }

    /// <summary>
    /// Updates account info section
    /// </summary>
    private void UpdateAccountInfo(PlayerAccountData data)
    {
        // Player name
        if (playerNameText != null)
            playerNameText.text = data.username;

        // Account level
        if (accountLevelText != null)
            accountLevelText.text = $"Level {data.level}";

        // XP bar
        if (accountXPBar != null && accountXPText != null)
        {
            int currentLevelXP = GetXPForLevel(data.level);
            int nextLevelXP = GetXPForLevel(data.level + 1);
            int xpIntoLevel = data.currentXP - currentLevelXP;
            int xpNeeded = nextLevelXP - currentLevelXP;

            float fillAmount = (float)xpIntoLevel / xpNeeded;
            accountXPBar.fillAmount = Mathf.Clamp01(fillAmount);
            accountXPText.text = $"{xpIntoLevel} / {xpNeeded} XP";
        }
    }

    /// <summary>
    /// Updates currency display
    /// </summary>
    private void UpdateCurrency(PlayerAccountData data)
    {
        if (softCurrencyText != null)
            softCurrencyText.text = data.credits.ToString();

        if (hardCurrencyText != null)
            hardCurrencyText.text = data.gems.ToString();
    }

    /// <summary>
    /// Updates stats display
    /// </summary>
    private void UpdateStats(PlayerAccountData data)
    {
        if (totalMatchesText != null)
            totalMatchesText.text = $"Matches: {data.totalMatchesPlayed}";

        if (winRateText != null)
        {
            float winRate = data.totalMatchesPlayed > 0
                ? (float)data.totalMatchesWon / data.totalMatchesPlayed * 100f
                : 0f;
            winRateText.text = $"Win Rate: {winRate:F1}%";
        }

        if (totalDamageText != null)
            totalDamageText.text = $"Total Damage: {data.totalDamageDealt:N0}";
    }

    /// <summary>
    /// Updates ship progression panel (if a ship is selected)
    /// </summary>
    private void UpdateShipProgression()
    {
        if (currentlyViewedShip == null)
        {
            if (shipProgressionPanel != null)
                shipProgressionPanel.SetActive(false);
            return;
        }

        if (shipProgressionPanel != null)
            shipProgressionPanel.SetActive(true);

        PlayerAccountData data = progressionManager.currentPlayerData;
        ShipProgressionEntry progression = data.GetShipProgression(currentlyViewedShip);

        if (progression == null) return;

        // Ship name & level
        if (shipNameText != null)
            shipNameText.text = currentlyViewedShip.loadoutName;

        if (shipLevelText != null)
            shipLevelText.text = $"Level {progression.shipLevel}";

        // XP bar
        if (shipXPBar != null && shipXPText != null)
        {
            if (progression.shipLevel >= 20)
            {
                shipXPBar.fillAmount = 1.0f;
                shipXPText.text = "MAX LEVEL";
            }
            else
            {
                float fillAmount = progression.GetLevelProgress();
                int currentLevelXP = ShipProgressionEntry.GetXPRequiredForLevel(progression.shipLevel);
                int nextLevelXP = ShipProgressionEntry.GetXPRequiredForLevel(progression.shipLevel + 1);
                int xpIntoLevel = progression.shipXP - currentLevelXP;
                int xpNeeded = nextLevelXP - currentLevelXP;

                shipXPBar.fillAmount = fillAmount;
                shipXPText.text = $"{xpIntoLevel} / {xpNeeded} XP";
            }
        }

        // Ship stats
        if (shipStatsText != null)
        {
            shipStatsText.text = $"Matches: {progression.matchesPlayed} | Wins: {progression.matchesWon}\n" +
                                 $"Rounds Won: {progression.roundsWon} | Kills: {progression.totalKills}\n" +
                                 $"Total Damage: {progression.totalDamage:N0}";
        }
    }

    /// <summary>
    /// Updates next unlock preview - sourced from BattlePassSystem (the
    /// canonical battle pass; see ProgressionManager for why the old
    /// freeBattlePass/BattlePassData path was removed). Tracks the battle
    /// pass's own level, not account level - they're different progression
    /// axes fed by the same match XP.
    /// </summary>
    private void UpdateNextUnlock(PlayerAccountData data)
    {
        if (nextUnlockPanel == null) return;

        var bp = BattlePassSystem.Instance;
        if (bp == null)
        {
            nextUnlockPanel.SetActive(false);
            return;
        }

        int nextLevel = bp.GetCurrentLevel() + 1;
        BattlePassReward nextReward = bp.GetFreeReward(nextLevel);
        if (nextReward == null)
        {
            nextUnlockPanel.SetActive(false);
            return;
        }

        nextUnlockPanel.SetActive(true);

        // Display reward info
        if (nextUnlockText != null)
            nextUnlockText.text = nextReward.displayName;

        if (nextUnlockLevelText != null)
            nextUnlockLevelText.text = $"Unlocks at Battle Pass Level {nextLevel}";

        // Display icon, resolved from the content databases by reward id
        // (BattlePassReward only carries an id string, not a live SO ref).
        if (nextUnlockIcon != null && !string.IsNullOrEmpty(nextReward.rewardId))
        {
            Sprite icon = ResolveRewardIcon(nextReward.rewardId);
            if (icon != null)
                nextUnlockIcon.sprite = icon;
        }
    }

    /// <summary>
    /// Looks up a reward's icon by id across the content databases
    /// (bodies/perks/passives/missiles/ships). Returns null for
    /// currency/skin rewards, which have no ScriptableObject to draw from.
    /// </summary>
    private Sprite ResolveRewardIcon(string rewardId)
    {
        var pm = progressionManager;
        if (pm == null) return null;

        var body = pm.allShipBodies.Find(b => b != null && b.name == rewardId);
        if (body != null) return body.icon;

        var perk = pm.allPerks.Find(p => p != null && p.name == rewardId);
        if (perk != null) return perk.icon;

        var passive = pm.allPassives.Find(p => p != null && p.name == rewardId);
        if (passive != null) return passive.icon;

        var missile = pm.allMissiles.Find(m => m != null && m.name == rewardId);
        if (missile != null) return missile.icon;

        var ship = pm.allShipPresets.Find(s => s != null && s.name == rewardId);
        if (ship != null) return ship.shipIcon;

        return null;
    }

    /// <summary>
    /// Sets which ship to display in the ship progression panel
    /// </summary>
    public void SetViewedShip(CustomShipLoadout loadout)
    {
        currentlyViewedShip = loadout;
        UpdateShipProgression();
    }

    /// <summary>
    /// Gets account XP required for a specific level (simple linear formula)
    /// </summary>
    private int GetXPForLevel(int level)
    {
        return 1000 + (level * 500);
    }

    /// <summary>
    /// Extracts icon sprite from a ScriptableObject
    /// </summary>
    private Sprite GetIconFromScriptableObject(ScriptableObject obj)
    {
        if (obj is ShipBodySO body) return body.icon;
        if (obj is ActivePerkSO perk) return perk.icon;
        if (obj is PassiveAbilitySO passive) return passive.icon;
        if (obj is MoveTypeSO moveType) return moveType.icon;
        if (obj is MissilePresetSO missile) return missile.icon;

        return null;
    }

    /// <summary>
    /// Call this when XP is awarded to animate the bar
    /// </summary>
    public void AnimateXPGain(int xpAmount)
    {
        // TODO: Add smooth fill animation
        RefreshUI();
    }

    /// <summary>
    /// Shows level-up notification
    /// </summary>
    public void ShowLevelUpNotification(int newLevel)
    {
        Debug.Log($"[ProgressionUI] LEVEL UP! Now Level {newLevel}");
        // TODO: Show fancy level-up popup with rewards
        RefreshUI();
    }
}
