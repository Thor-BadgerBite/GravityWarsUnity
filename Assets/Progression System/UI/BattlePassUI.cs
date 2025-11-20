using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays the battle pass with tiers, rewards, and progress.
/// Shows both free and premium tracks.
/// </summary>
public class BattlePassUI : MonoBehaviour
{
    [Header("References")]
    public ProgressionManager progressionManager;
    public BattlePassData battlePass; // Set to seasonal battle pass

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

        if (battlePass == null && progressionManager.seasonalBattlePass != null)
            battlePass = progressionManager.seasonalBattlePass;

        purchasePremiumButton.onClick.AddListener(OnPurchasePremium);

        RefreshUI();
    }

    /// <summary>
    /// Refreshes entire battle pass UI
    /// </summary>
    public void RefreshUI()
    {
        if (progressionManager == null || battlePass == null) return;

        PlayerProfileData data = progressionManager.currentPlayerData;

        UpdateHeader(data);
        UpdateTiers(data);
        UpdatePurchasePanel(data);
    }

    /// <summary>
    /// Updates header section (title, tier, XP bar)
    /// </summary>
    private void UpdateHeader(PlayerProfileData data)
    {
        // Title
        if (battlePassTitleText != null)
            battlePassTitleText.text = battlePass.username;

        // Current tier
        if (currentTierText != null)
        {
            int currentTier = data.battlePassTier + 1; // Display as 1-indexed
            int maxTier = battlePass.GetTierCount();
            currentTierText.text = $"Tier {currentTier} / {maxTier}";
        }

        // XP bar
        if (battlePassXPBar != null && battlePassXPText != null)
        {
            int currentTierXP = 0;
            int nextTierXP = 1000;

            if (data.battlePassTier < battlePass.GetTierCount())
            {
                currentTierXP = battlePass.GetTier(data.battlePassTier)?.xpRequired ?? 0;
                nextTierXP = battlePass.GetXPForNextTier(data.battlePassTier);
            }

            int xpIntoTier = data.battlePassXP - currentTierXP;
            int xpNeeded = nextTierXP - currentTierXP;

            if (data.battlePassTier >= battlePass.GetTierCount() - 1)
            {
                battlePassXPBar.fillAmount = 1.0f;
                battlePassXPText.text = "MAX TIER";
            }
            else
            {
                float fillAmount = Mathf.Clamp01((float)xpIntoTier / xpNeeded);
                battlePassXPBar.fillAmount = fillAmount;
                battlePassXPText.text = $"{xpIntoTier} / {xpNeeded} XP";
            }
        }
    }

    /// <summary>
    /// Updates tier display (scrollable list of all tiers)
    /// </summary>
    private void UpdateTiers(PlayerProfileData data)
    {
        // Clear existing
        foreach (var item in tierItems)
        {
            Destroy(item);
        }
        tierItems.Clear();

        // Create tier items
        for (int i = 0; i < battlePass.GetTierCount(); i++)
        {
            BattlePassTier tier = battlePass.GetTier(i);
            if (tier == null) continue;

            GameObject tierObj = Instantiate(tierItemPrefab, tierContainer);
            tierItems.Add(tierObj);

            SetupTierItem(tierObj, tier, data, i);
        }

        // Scroll to current tier
        if (scrollRect != null)
        {
            float scrollPos = (float)data.battlePassTier / battlePass.GetTierCount();
            scrollRect.verticalNormalizedPosition = 1f - scrollPos;
        }
    }

    /// <summary>
    /// Sets up a single tier item display
    /// </summary>
    private void SetupTierItem(GameObject tierObj, BattlePassTier tier, PlayerProfileData data, int tierIndex)
    {
        // Tier number
        TextMeshProUGUI tierNumberText = tierObj.transform.Find("TierNumber")?.GetComponent<TextMeshProUGUI>();
        if (tierNumberText != null)
            tierNumberText.text = $"{tier.tierNumber}";

        // Free reward
        Transform freeRewardPanel = tierObj.transform.Find("FreeReward");
        if (freeRewardPanel != null)
        {
            SetupRewardDisplay(freeRewardPanel, tier.freeReward, tierIndex <= data.battlePassTier);
        }

        // Premium reward
        Transform premiumRewardPanel = tierObj.transform.Find("PremiumReward");
        if (premiumRewardPanel != null)
        {
            bool isPremiumUnlocked = data.hasPremiumBattlePass && tierIndex <= data.battlePassTier;
            SetupRewardDisplay(premiumRewardPanel, tier.premiumReward, isPremiumUnlocked);

            // Grey out if player doesn't have premium pass
            if (!data.hasPremiumBattlePass)
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

        // Highlight current tier
        Image background = tierObj.GetComponent<Image>();
        if (background != null && tierIndex == data.battlePassTier)
        {
            background.color = new Color(1f, 1f, 0f, 0.3f); // Yellow highlight
        }
    }

    /// <summary>
    /// Sets up reward display (icon, name, claimed status)
    /// </summary>
    private void SetupRewardDisplay(Transform rewardPanel, UnlockableReward reward, bool isClaimed)
    {
        if (!reward.HasReward())
        {
            rewardPanel.gameObject.SetActive(false);
            return;
        }

        rewardPanel.gameObject.SetActive(true);

        // Reward icon
        Image iconImage = rewardPanel.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && reward.rewardItem != null)
        {
            Sprite icon = GetIconFromScriptableObject(reward.rewardItem);
            if (icon != null)
                iconImage.sprite = icon;
        }

        // Reward name
        TextMeshProUGUI nameText = rewardPanel.Find("Name")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = reward.GetDisplayText();

        // Claimed checkmark
        GameObject claimedMark = rewardPanel.Find("ClaimedMark")?.gameObject;
        if (claimedMark != null)
            claimedMark.SetActive(isClaimed);

        // Click to view details
        Button rewardButton = rewardPanel.GetComponent<Button>();
        if (rewardButton != null)
        {
            rewardButton.onClick.RemoveAllListeners();
            rewardButton.onClick.AddListener(() => ShowRewardDetails(reward));
        }
    }

    /// <summary>
    /// Updates the purchase premium panel
    /// </summary>
    private void UpdatePurchasePanel(PlayerProfileData data)
    {
        if (purchasePremiumPanel == null) return;

        if (data.hasPremiumBattlePass)
        {
            purchasePremiumPanel.SetActive(false);
        }
        else
        {
            purchasePremiumPanel.SetActive(true);

            // Set price (example: 1000 gems)
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

        bool success = progressionManager.PurchasePremiumBattlePass(gemCost);
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

        rewardsPopup.SetActive(true);

        // Clear previous rewards
        foreach (Transform child in rewardsContainer)
        {
            Destroy(child.gameObject);
        }

        // Show all premium rewards up to current tier
        PlayerProfileData data = progressionManager.currentPlayerData;
        for (int i = 0; i <= data.battlePassTier; i++)
        {
            BattlePassTier tier = battlePass.GetTier(i);
            if (tier != null && tier.premiumReward.HasReward())
            {
                GameObject rewardObj = Instantiate(rewardItemPrefab, rewardsContainer);
                TextMeshProUGUI rewardText = rewardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (rewardText != null)
                    rewardText.text = $"Tier {tier.tierNumber}: {tier.premiumReward.GetDisplayText()}";
            }
        }
    }

    /// <summary>
    /// Shows reward details popup
    /// </summary>
    private void ShowRewardDetails(UnlockableReward reward)
    {
        Debug.Log($"[BattlePassUI] Reward details: {reward.GetDisplayText()}");
        // TODO: Show fancy popup with reward details
    }

    /// <summary>
    /// Extracts icon from ScriptableObject
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
    /// Animates tier unlock
    /// </summary>
    public void AnimateTierUnlock(int newTier)
    {
        Debug.Log($"[BattlePassUI] Tier {newTier} unlocked!");
        // TODO: Add fancy animation
        RefreshUI();
    }
}
