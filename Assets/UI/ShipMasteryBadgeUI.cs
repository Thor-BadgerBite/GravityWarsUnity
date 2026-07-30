using UnityEngine;
using TMPro;

/// <summary>
/// Displays a ship's mastery prestige: level + title ("Veteran", "Ace",
/// "Master", "Legend"). Place next to a ship's name in the lobby, ship
/// selection or results screen and call SetLoadout().
/// Mastery makes ship investment visible to the opponent - the whole point
/// of prestige.
/// </summary>
public class ShipMasteryBadgeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI badgeText;

    [Header("Title Colors")]
    [SerializeField] private Color veteranColor = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private Color aceColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color masterColor = new Color(1f, 0.5f, 0.9f);
    [SerializeField] private Color legendColor = new Color(1f, 0.3f, 0.2f);

    /// <summary>Shows the badge for a specific loadout's progression.</summary>
    public void SetLoadout(CustomShipLoadout loadout)
    {
        var data = ProgressionManager.Instance != null ? ProgressionManager.Instance.currentPlayerData : null;
        if (badgeText == null || data == null || loadout == null)
        {
            Clear();
            return;
        }

        var progression = data.GetShipProgression(loadout);
        SetProgression(progression);
    }

    /// <summary>Shows the badge for a progression entry directly.</summary>
    public void SetProgression(ShipProgressionEntry progression)
    {
        if (badgeText == null) return;

        if (progression == null)
        {
            Clear();
            return;
        }

        string title = progression.GetMasteryTitle();
        if (string.IsNullOrEmpty(title))
        {
            badgeText.text = $"Lv.{progression.shipLevel}";
            badgeText.color = Color.white;
        }
        else
        {
            badgeText.text = $"Lv.{progression.shipLevel} • {title}";
            badgeText.color = TitleColor(progression.shipLevel);
        }
    }

    public void Clear()
    {
        if (badgeText != null) badgeText.text = "";
    }

    private Color TitleColor(int level)
    {
        if (level >= 20) return legendColor;
        if (level >= 15) return masterColor;
        if (level >= 10) return aceColor;
        return veteranColor;
    }
}
