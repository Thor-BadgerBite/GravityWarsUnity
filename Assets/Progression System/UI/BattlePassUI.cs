using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays the battle pass with tiers, rewards, and progress.
/// Shows both free and premium tracks.
///
/// Sourced from BattlePassSystem.Instance (hardcoded, real reward tables -
/// see IMPLEMENTATION_STATUS.md). This used to read from a BattlePassData
/// ScriptableObject via progressionManager.seasonalBattlePass, but no such
/// asset was ever created in the project, so that path was permanently
/// null and this whole screen silently did nothing. BattlePassSystem is
/// now the single canonical battle pass implementation.
/// </summary>
public class BattlePassUI : MonoBehaviour
{
    [Header("References")]
    public ProgressionManager progressionManager;

    [Header("Header")]
    public TextMeshProUGUI battlePassTitleText;
    public TextMeshProUGUI currentTierText;
    public Image battlePassXPBar;
    public TextMeshProUGUI battlePassXPText;

    [Header("Tier Display")]
    public Transform tierContainer;
    public GameObject tierItemPrefab;
    public ScrollRect scrollRect;

    [Header("Purchase Premium")]
    public GameObject purchasePremiumPanel;
    public TextMeshProUGUI premiumPriceText;
    public Button purchasePremiumButton;

    [Header("Rewards Popup")]
    public GameObject rewardsPopup;
    public Transform rewardsContainer;
    public GameObject rewardItemPrefab;

    private List<GameObject> tierItems = new List<GameObject>();

    void Start()
    {
        if (progressionManager == null)
            progressionManager = ProgressionManager.Instance;

        purchasePremiumButton.onClick.AddListener(OnPurchasePremium);

        RefreshUI();
    }

    /// <summary>
    /// Refreshes entire battle pass UI
    /// </summary>
    public void RefreshUI()
    {
        var bp = BattlePassSystem.Instance;
        if (progressionManager == null || bp == null) return;

        PlayerAccountData data = progressionManager.currentPlayerData;
        if (data == null) return;

        UpdateHeader(bp);
        UpdateTiers(bp);
        UpdatePurchasePanel(data);
    }

    /// <summary>
    /// Updates header section (title, tier, XP bar)
    /// </summary>
    private void UpdateHeader(BattlePassSystem bp)
    {
        if (battlePassTitleText != null)
            battlePassTitleText.text = bp.GetSeasonName();

        if (currentTierText != null)
        {
            int currentLevel = bp.GetCurrentLevel() + 1; // Display as 1-indexed
            currentTierText.text = $"Tier {currentLevel} / {bp.GetMaxLevel()}";
        }

        if (battlePassXPBar != null && battlePassXPText != null)
        {
            if (bp.GetCurrentLevel() >= bp.GetMaxLevel())
            {
                battlePassXPBar.fillAmount = 1.0f;
                battlePassXPText.text = "MAX TIER";
            }
            else
            {
                int xpNeeded = bp.GetXPForNextLevel();
                float fillAmount = Mathf.Clamp01((float)bp.GetCurrentXP() / xpNeeded);
                battlePassXPBar.fillAmount = fillAmount;
                battlePassXPText.text = $"{bp.GetCurrentXP()} / {xpNeeded} XP";
            }
        }
    }

    /// <summary>
    /// Updates tier display (scrollable list of all tiers). Levels are
    /// 1-indexed to match BattlePassSystem's reward dictionaries.
    /// </summary>
    private void UpdateTiers(BattlePassSystem bp)
    {
        foreach (var item in tierItems)
            Destroy(item);
        tierItems.Clear();

        for (int level = 1; level <= bp.GetMaxLevel(); level++)
        {
            GameObject tierObj = Instantiate(tierItemPrefab, tierContainer);
            tierItems.Add(tierObj);

            SetupTierItem(tierObj, bp, level);
        }

        if (scrollRect != null)
        {
            float scrollPos = (float)bp.GetCurrentLevel() / bp.GetMaxLevel();
            scrollRect.verticalNormalizedPosition = 1f - scrollPos;
        }
    }

    /// <summary>
    /// Sets up a single tier item display
    /// </summary>
    private void SetupTierItem(GameObject tierObj, BattlePassSystem bp, int level)
    {
        TextMeshProUGUI tierNumberText = tierObj.transform.Find("TierNumber")?.GetComponent<TextMeshProUGUI>();
        if (tierNumberText != null)
            tierNumberText.text = $"{level}";

        Transform freeRewardPanel = tierObj.transform.Find("FreeReward");
        if (freeRewardPanel != null)
            SetupRewardDisplay(freeRewardPanel, bp.GetFreeReward(level), bp.IsFreeRewardClaimed(level));

        Transform premiumRewardPanel = tierObj.transform.Find("PremiumReward");
        if (premiumRewardPanel != null)
        {
            SetupRewardDisplay(premiumRewardPanel, bp.GetPremiumReward(level), bp.IsPremiumRewardClaimed(level));

            if (!bp.IsPremiumUnlocked())
            {
                Image[] images = premiumRewardPanel.GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    Color greyedOut = img.color;
                    greyedOut.a = 0.5f;
                    img.color = greyedOut;
                }
            }
        }

        Image background = tierObj.GetComponent<Image>();
        if (background != null && level - 1 == bp.GetCurrentLevel())
        {
            background.color = new Color(1f, 1f, 0f, 0.3f); // Yellow highlight
        }
    }

    /// <summary>
    /// Sets up reward display (icon, name, claimed status). reward is null
    /// for levels that don't grant anything on that track.
    /// </summary>
    private void SetupRewardDisplay(Transform rewardPanel, BattlePassReward reward, bool isClaimed)
    {
        if (reward == null)
        {
            rewardPanel.gameObject.SetActive(false);
            return;
        }

        rewardPanel.gameObject.SetActive(true);

        Image iconImage = rewardPanel.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null)
        {
            Sprite icon = ResolveRewardIcon(reward.rewardId);
            if (icon != null)
                iconImage.sprite = icon;
        }

        TextMeshProUGUI nameText = rewardPanel.Find("Name")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = reward.displayName;

        GameObject claimedMark = rewardPanel.Find("ClaimedMark")?.gameObject;
        if (claimedMark != null)
            claimedMark.SetActive(isClaimed);

        // Rewards auto-grant on level-up now (BattlePassSystem.OnLevelUp) -
        // this button just shows details, it doesn't need to claim anything.
        Button rewardButton = rewardPanel.GetComponent<Button>();
        if (rewardButton != null)
        {
            rewardButton.onClick.RemoveAllListeners();
            rewardButton.onClick.AddListener(() => ShowRewardDetails(reward));
        }
    }

    /// <summary>
    /// Looks up a reward's icon by id across the content databases
    /// (bodies/ships/perks/passives/missiles). Returns null for
    /// currency/skin rewards, which have no ScriptableObject to draw from.
    /// </summary>
    private Sprite ResolveRewardIcon(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId) || progressionManager == null) return null;

        var body = progressionManager.allShipBodies.Find(b => b != null && b.name == rewardId);
        if (body != null) return body.icon;

        var ship = progressionManager.allShipPresets.Find(s => s != null && s.name == rewardId);
        if (ship != null) return ship.shipIcon;

        var perk = progressionManager.allPerks.Find(p => p != null && p.name == rewardId);
        if (perk != null) return perk.icon;

        var passive = progressionManager.allPassives.Find(p => p != null && p.name == rewardId);
        if (passive != null) return passive.icon;

        var missile = progressionManager.allMissiles.Find(m => m != null && m.name == rewardId);
        if (missile != null) return missile.icon;

        return null;
    }

    /// <summary>
    /// Updates the purchase premium panel
    /// </summary>
    private void UpdatePurchasePanel(PlayerAccountData data)
    {
        if (purchasePremiumPanel == null) return;

        if (data.hasPremiumBattlePass)
        {
            purchasePremiumPanel.SetActive(false);
        }
        else
        {
            purchasePremiumPanel.SetActive(true);

            if (premiumPriceText != null)
                premiumPriceText.text = "1000 Gems";
        }
    }

    /// <summary>
    /// Handles premium battle pass purchase
    /// </summary>
    private void OnPurchasePremium()
    {
        int gemCost = 1000; // Adjust as needed

        bool success = BattlePassSystem.Instance != null && BattlePassSystem.Instance.PurchasePremiumPass(gemCost);
        if (success)
        {
            Debug.Log("[BattlePassUI] Premium Battle Pass purchased!");
            ShowPremiumRewardsPopup();
            RefreshUI();
        }
        else
        {
            Debug.LogWarning("[BattlePassUI] Purchase failed (insufficient funds or already owned)");
            // Show error popup
        }
    }

    /// <summary>
    /// Shows popup with all retroactive premium rewards
    /// </summary>
    private void ShowPremiumRewardsPopup()
    {
        if (rewardsPopup == null) return;
        var bp = BattlePassSystem.Instance;
        if (bp == null) return;

        rewardsPopup.SetActive(true);

        foreach (Transform child in rewardsContainer)
            Destroy(child.gameObject);

        for (int level = 1; level <= bp.GetCurrentLevel(); level++)
        {
            var reward = bp.GetPremiumReward(level);
            if (reward == null) continue;

            GameObject rewardObj = Instantiate(rewardItemPrefab, rewardsContainer);
            TextMeshProUGUI rewardText = rewardObj.GetComponentInChildren<TextMeshProUGUI>();
            if (rewardText != null)
                rewardText.text = $"Level {level}: {reward.displayName}";
        }
    }

    /// <summary>
    /// Shows reward details popup
    /// </summary>
    private void ShowRewardDetails(BattlePassReward reward)
    {
        Debug.Log($"[BattlePassUI] Reward details: {reward.displayName}");
        // TODO: Show fancy popup with reward details
    }

    /// <summary>
    /// Animates tier unlock
    /// </summary>
    public void AnimateTierUnlock(int newTier)
    {
        Debug.Log($"[BattlePassUI] Tier {newTier} unlocked!");
        // TODO: Add fancy animation
        RefreshUI();
    }
}
