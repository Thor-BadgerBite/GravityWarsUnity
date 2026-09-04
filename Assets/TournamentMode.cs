using UnityEngine;

/// <summary>
/// Tournament / normalized mode: when enabled, every ship's stats are applied
/// at a fixed reference level instead of its actual progression level, making
/// matches purely skill-based (no power advantage from grind).
///
/// Ship XP is still earned normally - only the in-match stat scaling changes.
/// Toggle from a game-mode selection UI or before starting a match:
///   TournamentMode.Enabled = true;
/// </summary>
public static class TournamentMode
{
    /// <summary>Whether normalized stats are active for the next/current match.</summary>
    public static bool Enabled = false;

    /// <summary>The reference level all ships are normalized to while enabled.</summary>
    public static int NormalizedLevel = 10;

    /// <summary>
    /// The level a ship's stats should be computed at.
    /// Returns the actual level when tournament mode is off.
    /// </summary>
    public static int GetEffectiveLevel(int actualLevel)
    {
        return Enabled ? Mathf.Max(1, NormalizedLevel) : actualLevel;
    }
}
