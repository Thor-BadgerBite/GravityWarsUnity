using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main-menu widget that always shows the player's next goal:
/// "Level 7 → Phoenix Mk-I" with an XP progress bar.
/// The player should close the game knowing what tomorrow brings.
///
/// Setup: place on the main menu, assign the texts/bar, it refreshes on enable.
/// </summary>
public class NextUnlockWidget : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nextUnlockText;
    [SerializeField] private TextMeshProUGUI xpProgressText;
    [SerializeField] private Slider xpProgressBar;

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>Refreshes the widget from current progression data.</summary>
    public void Refresh()
    {
        var data = ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
        if (data == null)
        {
            if (nextUnlockText != null) nextUnlockText.text = "";
            return;
        }

        // XP progress toward the next account level
        int xpForNext = 1000 + (data.level * 500); // Matches ProgressionManager level formula
        if (xpProgressBar != null)
        {
            xpProgressBar.maxValue = xpForNext;
            xpProgressBar.value = Mathf.Clamp(data.currentXP, 0, xpForNext);
        }
        if (xpProgressText != null)
        {
            xpProgressText.text = $"Level {data.level}  •  {data.currentXP}/{xpForNext} XP";
        }

        // What's waiting at the next unlock level?
        if (nextUnlockText != null)
        {
            nextUnlockText.text = DescribeNextUnlock(data.level);
        }
    }

    /// <summary>
    /// Finds the nearest upcoming reward across all unlock schedules
    /// (prebuilt ships, bodies, passives, actives, missiles, features).
    /// </summary>
    public static string DescribeNextUnlock(int currentLevel)
    {
        for (int level = currentLevel + 1; level <= 100; level++)
        {
            var ship = ExtendedProgressionData.GetPrebuildShip(level);
            if (ship != null) return $"Level {level} → 🚀 {ship.displayName}";

            var body = ExtendedProgressionData.GetShipBody(level);
            if (body != null) return $"Level {level} → 🔧 {body.displayName}";

            var missile = GravityWars.Online.MissileRetrofitSystem.GetMissileUnlock(level);
            if (missile != null) return $"Level {level} → 💥 {missile.displayName}";

            var passive = ExtendedProgressionData.GetPassive(level);
            if (passive != null) return $"Level {level} → ⚡ {passive.displayName}";

            var active = ExtendedProgressionData.GetActive(level);
            if (active != null) return $"Level {level} → 💫 {active.displayName}";

            if (level == ProgressionSystem.RANKED_UNLOCK_LEVEL)
                return $"Level {level} → 🏆 Ranked Mode";
            if (level == ProgressionSystem.CUSTOM_SLOT_2_UNLOCK_LEVEL)
                return $"Level {level} → 🛠 Custom Ship Slot #2";
            if (level == ProgressionSystem.CUSTOM_SLOT_3_UNLOCK_LEVEL)
                return $"Level {level} → 🛠 Custom Ship Slot #3";
        }

        return "All unlocks earned - you are a legend!";
    }
}
