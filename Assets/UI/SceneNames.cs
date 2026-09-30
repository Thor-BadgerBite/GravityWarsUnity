/// <summary>
/// The only place scene names are spelled out (GDD §15.1, OQ1 decision).
/// Every scene load goes through these constants; never LoadScene("literal").
/// The three scenes here are the whole build (ProjectSettings/EditorBuildSettings.asset,
/// in this order): SplashScreen → MainMenu (hub, panels inside) → Match (hotseat/bot, later online).
/// </summary>
public static class SceneNames
{
    /// <summary>Boot scene: intro, then loads the hub.</summary>
    public const string SplashScreen = "SplashScreen";

    /// <summary>The hub. Garage, settings, missile selection and results-return all live here as panels.</summary>
    public const string MainMenu = "MainMenu";

    /// <summary>The one match scene: hotseat and bot now, online in M7.</summary>
    public const string Match = "Match";
}
