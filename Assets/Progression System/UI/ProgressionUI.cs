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
            accountLevelText.text = $"Level {data.accountLevel}";

        // XP bar
        if (accountXPBar != null && accountXPText != null)
        {
            int currentLevelXP = GetXPForLevel(data.accountLevel);
            int nextLevelXP = GetXPForLevel(data.accountLevel + 1);
            int xpIntoLevel = data.accountXP - currentLevelXP;
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
    /// Updates next unlock preview
    /// </summary>
    private void UpdateNextUnlock(PlayerAccountData data)
    {
        if (nextUnlockPanel == null) return;

        // Get next unlock from free battle pass
        if (progressionManager.freeBattlePass == null)
        {
            nextUnlockPanel.SetActive(false);
            return;
        }

        int nextLevel = data.accountLevel + 1;
        if (nextLevel > progressionManager.freeBattlePass.GetTierCount())
        {
            nextUnlockPanel.SetActive(false);
            return;
        }

        BattlePassTier nextTier = progressionManager.freeBattlePass.GetTier(nextLevel - 1);
        if (nextTier == null || !nextTier.freeReward.HasReward())
        {
            nextUnlockPanel.SetActive(false);
            return;
        }

        nextUnlockPanel.SetActive(true);

        // Display reward info
        if (nextUnlockText != null)
            nextUnlockText.text = nextTier.freeReward.GetDisplayText();

        if (nextUnlockLevelText != null)
            nextUnlockLevelText.text = $"Unlocks at Level {nextLevel}";

        // Display icon (if ScriptableObject reward)
        if (nextUnlockIcon != null && nextTier.freeReward.rewardItem != null)
        {
            Sprite icon = GetIconFromScriptableObject(nextTier.freeReward.rewardItem);
            if (icon != null)
                nextUnlockIcon.sprite = icon;
        }
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
