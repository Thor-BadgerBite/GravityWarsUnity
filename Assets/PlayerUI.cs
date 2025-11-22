using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider velocitySlider;
    public Text angleText;
    public Text velocityText;
    public Text nameText;

    private PlayerShip playerShip;
    private RectTransform rectTransform;
    private Canvas canvas;
    public void Initialize(PlayerShip ship, Canvas gameCanvas)
    {
        playerShip = ship;
        nameText.text = ship.playerName;
        //Debug.Log($"PlayerUI initialized for {ship.playerName}"); // Debug log for confirmation
        rectTransform = GetComponent<RectTransform>();
        canvas = gameCanvas;
    }
public PlayerShip GetAttachedShip() 
{ 
    return playerShip; 
}
public string GetAttachedShipName() 
{ 
    return (playerShip != null) ? playerShip.playerName : "NULL"; 
}

    public void UpdateUI()
    {
        if (playerShip == null) return;

        // Get the firing angle from the ship
        float rawAngle = playerShip.GetFiringAngle();

        // Convert the angle to the 0-360 scale
        float displayAngle = (rawAngle + 360) % 360;

        // Update angle text
        angleText.text = $"Angle: {displayAngle:F1}°";

        // Update velocity slider and text - use equipped missile's actual min/max velocities
        float velocityPercentage = (playerShip.launchVelocity - playerShip.EffectiveMinLaunchVelocity) / (playerShip.EffectiveMaxLaunchVelocity - playerShip.EffectiveMinLaunchVelocity);
        velocitySlider.value = velocityPercentage;
        velocityText.text = $"Velocity: {velocityPercentage * 100:F0}%";

        // Update position to be under the ship
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, playerShip.transform.position);
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, Camera.main, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint + new Vector2(0, -50); // Offset downwards
        }
    }

public void SetActive(bool active)
{
    //Debug.Log($"Setting UI active state for {(playerShip != null ? playerShip.playerName : "unknown player")}: {active}");
    gameObject.SetActive(active);
    //Debug.Log($"UI active state is now: {gameObject.activeSelf}");
}
}