using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Records missile flight paths for two purposes:
/// 1. KILLSHOT REPLAY - the trajectory of the shot that ends a round/match is
///    kept and can be replayed (slow-mo) from the results screen.
/// 2. TRICKSHOT DETECTION - if a shot's path curved heavily through gravity
///    wells before hitting, it counts as a "Gravity Assist" trickshot
///    (banner + bonus XP + stat tracking).
///
/// Wiring (all hooks are one-liners):
/// - Missile3D.Launch      → BeginTrack(missile, firingShip)
/// - samples itself        → Update() polls tracked missile positions
/// - PlayerShip.TakeDamage → NotifyShipDamaged(victim, fatal)
/// </summary>
public class KillshotRecorder : MonoBehaviour
{
    public static KillshotRecorder Instance { get; private set; }

    [Header("Sampling")]
    [Tooltip("Seconds between trajectory samples")]
    [SerializeField] private float sampleInterval = 0.05f;

    [Header("Trickshot Detection")]
    [Tooltip("Total heading change (degrees) along the path to count as a gravity assist")]
    [SerializeField] private float trickshotCurvatureThreshold = 100f;

    /// <summary>One recorded missile flight.</summary>
    public class RecordedShot
    {
        public PlayerShip shooter;
        public readonly List<Vector3> points = new List<Vector3>(256);
        public float totalCurvatureDeg;

        public bool IsTrickshot(float threshold) => totalCurvatureDeg >= threshold;
    }

    private class TrackedMissile
    {
        public Missile3D missile;
        public RecordedShot shot;
        public float lastSampleTime;
    }

    private readonly List<TrackedMissile> _tracked = new List<TrackedMissile>();

    /// <summary>The last shot that dealt damage (per shooter side, most recent).</summary>
    private RecordedShot _lastHitShot;

    /// <summary>The shot that ended the last round (fatal hit).</summary>
    public RecordedShot LastKillshot { get; private set; }

    /// <summary>Raised when a trickshot lands (for banners/celebrations).</summary>
    public event System.Action<RecordedShot> OnTrickshotLanded;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(this); return; }
    }

    private void OnEnable()
    {
        Missile3D.OnMissileDestroyed += HandleMissileDestroyed;
    }

    private void OnDisable()
    {
        Missile3D.OnMissileDestroyed -= HandleMissileDestroyed;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Ensures a recorder exists (attached next to the GameManager if possible).</summary>
    public static KillshotRecorder GetOrCreate()
    {
        if (Instance != null) return Instance;
        var host = GameManager.Instance != null ? GameManager.Instance.gameObject : new GameObject("KillshotRecorder");
        return host.AddComponent<KillshotRecorder>();
    }

    /// <summary>Call when a match starts.</summary>
    public void ResetMatch()
    {
        _tracked.Clear();
        _lastHitShot = null;
        LastKillshot = null;
    }

    /// <summary>Called by Missile3D.Launch - starts recording this missile's path.</summary>
    public void BeginTrack(Missile3D missile, GameObject firingShip)
    {
        if (missile == null) return;

        var shot = new RecordedShot
        {
            shooter = firingShip != null ? firingShip.GetComponent<PlayerShip>() : null
        };
        shot.points.Add(missile.transform.position);

        _tracked.Add(new TrackedMissile
        {
            missile = missile,
            shot = shot,
            lastSampleTime = Time.time
        });
    }

    private void Update()
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            var t = _tracked[i];
            if (t.missile == null)
            {
                // Destroyed without event (safety) - finalize
                _tracked.RemoveAt(i);
                continue;
            }

            if (Time.time - t.lastSampleTime >= sampleInterval)
            {
                AddSample(t.shot, t.missile.transform.position);
                t.lastSampleTime = Time.time;
            }
        }
    }

    private static void AddSample(RecordedShot shot, Vector3 pos)
    {
        var pts = shot.points;
        pts.Add(pos);

        // Accumulate curvature: angle between consecutive segments
        int n = pts.Count;
        if (n >= 3)
        {
            Vector3 prevDir = pts[n - 2] - pts[n - 3];
            Vector3 currDir = pts[n - 1] - pts[n - 2];
            if (prevDir.sqrMagnitude > 0.0001f && currDir.sqrMagnitude > 0.0001f)
            {
                shot.totalCurvatureDeg += Vector3.Angle(prevDir, currDir);
            }
        }
    }

    private void HandleMissileDestroyed(Missile3D missile)
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            if (_tracked[i].missile == missile)
            {
                // Final position sample
                AddSample(_tracked[i].shot, missile.transform.position);
                _tracked.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Called from PlayerShip.TakeDamage. Attributes the hit to the opponent's
    /// most recent tracked shot, checks trickshot, and keeps the killshot on a
    /// fatal hit.
    /// </summary>
    public void NotifyShipDamaged(PlayerShip victim, bool fatal)
    {
        RecordedShot hitShot = FindActiveShotAgainst(victim);
        if (hitShot == null) hitShot = _lastHitShot; // fallback (AoE after despawn)
        if (hitShot == null) return;

        _lastHitShot = hitShot;

        // Trickshot check - the path curved hard through gravity before landing
        if (hitShot.IsTrickshot(trickshotCurvatureThreshold))
        {
            if (hitShot.shooter != null)
                MatchStatsTracker.Instance?.RecordTrickshot(hitShot.shooter);

            OnTrickshotLanded?.Invoke(hitShot);
            GameManager.Instance?.ShowBanner("☄ GRAVITY ASSIST! ☄");
            Debug.Log($"[Killshot] Trickshot! Curvature: {hitShot.totalCurvatureDeg:F0}°");
        }

        if (fatal)
        {
            LastKillshot = hitShot;
            Debug.Log($"[Killshot] Killshot recorded ({hitShot.points.Count} points, curvature {hitShot.totalCurvatureDeg:F0}°)");
        }
    }

    /// <summary>The most recent in-flight (or just-landed) shot fired at this victim.</summary>
    private RecordedShot FindActiveShotAgainst(PlayerShip victim)
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            var shooter = _tracked[i].shot.shooter;
            if (shooter != null && shooter != victim)
                return _tracked[i].shot;
        }
        return null;
    }
}
