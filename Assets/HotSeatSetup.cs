using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotSeatSetup : MonoBehaviour
{
    public TMP_InputField player1NameInput;
    public TMP_InputField player2NameInput;
    public Slider winningScoreSlider;
    public Slider turnDurationSlider;
    public Slider prepTimeSlider;
    public TextMeshProUGUI winningScoreText;
    public TextMeshProUGUI turnDurationText;
    public TextMeshProUGUI prepTimeText;
    public Button startGameButton;
    public GameManager gameManager;
    public Slider unitsToSpawnSlider;
    public TextMeshProUGUI unitsToSpawnText;

    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        
        // Set default values and add listeners
        SetupSlider(winningScoreSlider, gameManager.winningScore, UpdateWinningScoreText);
        SetupSlider(turnDurationSlider, gameManager.turnDuration, UpdateTurnDurationText);
        SetupSlider(prepTimeSlider, gameManager.preparationTime, UpdatePrepTimeText);
        SetupSlider(unitsToSpawnSlider, gameManager.unitsToSpawn, UpdateUnitsToSpawnText);
    }

    private void SetupSlider(Slider slider, float defaultValue, UnityEngine.Events.UnityAction<float> updateAction)
    {
        slider.value = defaultValue;
        slider.onValueChanged.AddListener(updateAction);
        updateAction.Invoke(defaultValue);
    }

    private void UpdateUnitsToSpawnText(float value)
    {
        unitsToSpawnText.text = $"Units to Spawn: {value:0}";
        gameManager.UpdateUnitsToSpawn(value);
    }

    private void UpdateWinningScoreText(float value)
    {
        winningScoreText.text = $"Winning Score: {value:0}";
    }

    private void UpdateTurnDurationText(float value)
    {
        turnDurationText.text = $"Turn Duration: {value:0.0}s";
    }

    private void UpdatePrepTimeText(float value)
    {
        prepTimeText.text = $"Prep Time: {value:0.0}s";
    }

public void StartGame()
{
    // Get names from input fields
    string player1Name = string.IsNullOrEmpty(player1NameInput.text) ? "Player 1" : player1NameInput.text;
    string player2Name = string.IsNullOrEmpty(player2NameInput.text) ? "Player 2" : player2NameInput.text;

    // Log names for debugging
    Debug.Log($"Player 1 Name: {player1Name}, Player 2 Name: {player2Name}");

    // Store names in GameManager
    gameManager.player1Name = player1Name;
    gameManager.player2Name = player2Name;

    gameManager.StartGame();
}
}
