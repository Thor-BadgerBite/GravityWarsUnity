/// <summary>
/// Central on/off switches for noisy, high-frequency debug logging (things
/// that fire every frame or every few frames, e.g. while aiming).
/// Keep these OFF by default - flip one on temporarily when you actually
/// need to debug that specific system, then flip it back off. Leaving them
/// on floods the Console and makes it hard to spot real issues during
/// normal playtesting.
/// </summary>
public static class DebugSettings
{
    /// <summary>
    /// Logs missile trajectory prediction stats (mass/drag/velocity) every
    /// ~60 frames while a player is aiming. Useful when diagnosing trajectory
    /// physics, otherwise just spam.
    /// </summary>
    public static bool verboseTrajectoryLogging = false;
}
