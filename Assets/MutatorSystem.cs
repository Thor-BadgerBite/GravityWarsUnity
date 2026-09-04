using System;
using UnityEngine;

/// <summary>
/// Weekly match mutators - rotating gameplay modifiers that keep the meta fresh.
/// The active mutator is derived deterministically from the ISO week number, so
/// every player sees the same mutator in the same week without a server.
///
/// GameManager applies the active mutator at match setup when
/// <see cref="GameManager"/> has weekly mutators enabled.
/// </summary>
public static class MutatorSystem
{
    public enum Mutator
    {
        None,               // Standard rules week
        LowGravity,         // Planets pull half as hard - long daring shots
        GiantPlanets,       // Bigger, heavier planets - navigate the maze
        DoubleActionPoints  // +2 actions per turn - perk-heavy chaos
    }

    /// <summary>Rotation order. 'None' weeks give the baseline game a regular spotlight.</summary>
    private static readonly Mutator[] ROTATION =
    {
        Mutator.None,
        Mutator.LowGravity,
        Mutator.GiantPlanets,
        Mutator.DoubleActionPoints
    };

    /// <summary>
    /// Optional override for testing / custom matches. Null = use weekly rotation.
    /// </summary>
    public static Mutator? Override = null;

    /// <summary>
    /// The mutator active this week (deterministic across all players).
    /// </summary>
    public static Mutator GetCurrentMutator()
    {
        if (Override.HasValue) return Override.Value;

        // Weeks since a fixed epoch (Monday 2026-01-05) → rotation index
        var epoch = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        int weeks = Mathf.Max(0, (int)((DateTime.UtcNow - epoch).TotalDays / 7.0));
        return ROTATION[weeks % ROTATION.Length];
    }

    /// <summary>Planet gravitational mass multiplier for the active mutator.</summary>
    public static float GetPlanetMassScale(Mutator m)
    {
        switch (m)
        {
            case Mutator.LowGravity: return 0.5f;
            case Mutator.GiantPlanets: return 1.3f;
            default: return 1f;
        }
    }

    /// <summary>Planet visual/collision scale multiplier for the active mutator.</summary>
    public static float GetPlanetSizeScale(Mutator m)
    {
        switch (m)
        {
            case Mutator.GiantPlanets: return 1.4f;
            default: return 1f;
        }
    }

    /// <summary>Extra action points per turn for the active mutator.</summary>
    public static int GetBonusActionPoints(Mutator m)
    {
        return m == Mutator.DoubleActionPoints ? 2 : 0;
    }

    public static string GetDisplayName(Mutator m)
    {
        switch (m)
        {
            case Mutator.LowGravity: return "LOW GRAVITY WEEK";
            case Mutator.GiantPlanets: return "GIANT PLANETS WEEK";
            case Mutator.DoubleActionPoints: return "OVERDRIVE WEEK (+2 Actions)";
            default: return "";
        }
    }

    public static string GetDescription(Mutator m)
    {
        switch (m)
        {
            case Mutator.LowGravity:
                return "Planets pull half as hard. Go for the long shots!";
            case Mutator.GiantPlanets:
                return "Bigger, heavier planets. Thread the needle!";
            case Mutator.DoubleActionPoints:
                return "+2 action points every turn. Unleash your perks!";
            default:
                return "Standard rules this week.";
        }
    }
}
