using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slow-motion replay of the recorded killshot trajectory.
/// Draws the path progressively with a LineRenderer and moves an optional
/// marker along it. Runs on unscaled time so it works while the game is paused
/// on the results screen.
///
/// Setup:
/// - Add to a GameObject in the game scene (can live on the results screen).
/// - Assign a LineRenderer (world space) and optionally a marker transform.
/// - MatchResultsUI triggers it via PlayLastKillshot().
/// </summary>
public class KillshotReplayUI : MonoBehaviour
{
    public static KillshotReplayUI Instance { get; private set; }

    [Header("Rendering")]
    [Tooltip("World-space line that draws the trajectory")]
    [SerializeField] private LineRenderer replayLine;
    [Tooltip("Optional marker (e.g. missile ghost) that travels along the path")]
    [SerializeField] private Transform marker;

    [Header("Playback")]
    [Tooltip("Seconds the full replay takes (slow-mo feel)")]
    [SerializeField] private float replayDuration = 3f;
    [Tooltip("Hide the line this many seconds after the replay ends (0 = keep)")]
    [SerializeField] private float lingerSeconds = 2f;

    [Header("Optional UI")]
    [SerializeField] private GameObject replayBadge;   // "KILLSHOT REPLAY" label
    [SerializeField] private Button replayButton;      // re-watch button

    private Coroutine _playback;

    private void Awake()
    {
        Instance = this;

        if (replayButton != null)
            replayButton.onClick.AddListener(PlayLastKillshot);

        if (replayLine != null) replayLine.positionCount = 0;
        if (marker != null) marker.gameObject.SetActive(false);
        if (replayBadge != null) replayBadge.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>True when there is a killshot available to replay.</summary>
    public bool HasKillshot =>
        KillshotRecorder.Instance != null &&
        KillshotRecorder.Instance.LastKillshot != null &&
        KillshotRecorder.Instance.LastKillshot.points.Count >= 2;

    /// <summary>Plays (or replays) the last recorded killshot.</summary>
    public void PlayLastKillshot()
    {
        if (!HasKillshot)
        {
            Debug.Log("[KillshotReplay] No killshot recorded");
            return;
        }

        if (_playback != null) StopCoroutine(_playback);
        _playback = StartCoroutine(Playback(KillshotRecorder.Instance.LastKillshot));
    }

    private IEnumerator Playback(KillshotRecorder.RecordedShot shot)
    {
        var points = shot.points;

        if (replayBadge != null) replayBadge.SetActive(true);
        if (replayLine != null) replayLine.positionCount = 0;
        if (marker != null) marker.gameObject.SetActive(true);

        float elapsed = 0f;
        int shown = 0;

        while (elapsed < replayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / replayDuration);

            // Progressive line draw
            int targetCount = Mathf.Max(2, Mathf.RoundToInt(t * points.Count));
            if (replayLine != null && targetCount != shown)
            {
                replayLine.positionCount = targetCount;
                for (int i = shown; i < targetCount; i++)
                    replayLine.SetPosition(i, points[i]);
                shown = targetCount;
            }

            // Marker rides the path
            if (marker != null)
            {
                float fIndex = t * (points.Count - 1);
                int i0 = Mathf.FloorToInt(fIndex);
                int i1 = Mathf.Min(i0 + 1, points.Count - 1);
                marker.position = Vector3.Lerp(points[i0], points[i1], fIndex - i0);
            }

            yield return null;
        }

        if (marker != null) marker.gameObject.SetActive(false);

        if (lingerSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(lingerSeconds);
            if (replayLine != null) replayLine.positionCount = 0;
            if (replayBadge != null) replayBadge.SetActive(false);
        }

        _playback = null;
    }
}
