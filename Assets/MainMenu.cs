using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Button[] leftButtons;  // Single Player modes
    public Button[] rightButtons; // Multiplayer modes
    public Button loginButton;
    public Button settingsButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;

    private void Start()
    {
        Debug.Log("MainMenuManager Start method called");
        SetupButtons();
    }

private void SetupButtons()
{
    Debug.Log("Setting up buttons");

    // Single Player modes (left side)
    SetupButton(leftButtons[0], "VS. AI Mode", 
        "Enter a cerebral duel against our advanced AI opponent. Test your strategic prowess, adapt to evolving tactics, and outsmart a computer mind that learns from your every move. Perfect for honing your skills or enjoying a challenging game anytime.",
        StartAIMode);

    SetupButton(leftButtons[1], "Puzzle Mode", 
        "Immerse yourself in a world of mind-bending challenges. Tackle a diverse array of meticulously crafted puzzles that will push your logical thinking to its limits. From quick brain teasers to complex, multi-layered enigmas, sharpen your problem-solving skills and unlock new levels of strategic thinking.",
        StartPuzzleMode);

    SetupButton(leftButtons[2], "Sandbox Mode", 
        "Unleash your creativity in this boundless playground. Experiment with game mechanics, create custom scenarios, and explore 'what-if' situations without restrictions. Perfect for testing strategies, learning the intricacies of the game, or simply enjoying freeform play. Your imagination is the only limit!",
        StartSandboxMode);

    // Multiplayer modes (right side)
    SetupButton(rightButtons[0], "Hotseat Mode", 
        "Experience the thrill of face-to-face competition. Challenge a friend or family member on the same device, taking turns to outmaneuver each other. Ideal for local multiplayer sessions, this mode combines the immediacy of personal interaction with the depth of strategic gameplay.",
        StartHotseatMode);

    SetupButton(rightButtons[1], "PvP Online Mode", 
        "Step into the global arena and test your skills against players worldwide. Engage in real-time matches, climb the international leaderboards, and forge your reputation as a master strategist. With skill-based matchmaking and a vibrant community, every game is a new adventure.",
        StartPvPOnlineMode);

    SetupButton(rightButtons[2], "Tournament Mode", 
        "Enter the crucible of competition in high-stakes tournaments. Battle through brackets, outlast your opponents, and aim for the top in daily, weekly, and monthly events. Earn exclusive rewards, climb the ranks, and prove yourself as the ultimate champion in intense, structured competition.",
        StartTourneyMode);

    SetupButton(loginButton, "Login", 
        "Access your personalized game hub. Track your progress across all modes, view detailed statistics, and unlock achievements. Sync your data across devices, connect with friends, and access exclusive content. Your gateway to a fully personalized gaming experience awaits!",
        OpenLoginMenu);

    SetupButton(settingsButton, "Settings", 
        "Tailor every aspect of your game experience. Adjust audio and visual settings, customize controls, fine-tune difficulty levels, and set accessibility options. From performance tweaks to interface customization, ensure your gameplay experience is perfectly attuned to your preferences.",
        OpenSettingsMenu);
}

    private void SetupButton(Button button, string title, string description, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            Debug.LogError($"Button for '{title}' is null!");
            return;
        }

        Debug.Log($"Setting up button: {title}");

        button.onClick.AddListener(action);
        button.onClick.AddListener(() => Debug.Log($"Button clicked: {title}"));

        // Add event trigger for hover
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerEnter, (data) => UpdateHoverText(title, description));
        AddEventTriggerListener(eventTrigger, EventTriggerType.PointerExit, (data) => ClearHoverText());
    }

    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener((data) => { action((BaseEventData)data); });
        trigger.triggers.Add(entry);
    }

    private void UpdateHoverText(string title, string description)
    {
        titleText.text = title;
        infoText.text = description;
    }

    private void ClearHoverText()
    {
        titleText.text = "";
        infoText.text = "";
    }

    private void StartAIMode() { Debug.Log("AI Mode not yet implemented"); }
    private void StartPuzzleMode() { Debug.Log("Puzzle Mode not yet implemented"); }
    private void StartSandboxMode() { Debug.Log("Sandbox Mode not yet implemented"); }
    private void StartHotseatMode() 
    { 
        Debug.Log("Attempting to load HotSeat scene");
        SceneManager.LoadScene("HotSeat"); 
    }
    private void StartPvPOnlineMode() { Debug.Log("PvP Online Mode not yet implemented"); }
    private void StartTourneyMode() { Debug.Log("Tourney Mode not yet implemented"); }
    private void OpenLoginMenu() { Debug.Log("Login menu not yet implemented"); }
    private void OpenSettingsMenu() { Debug.Log("Settings menu not yet implemented"); }
}