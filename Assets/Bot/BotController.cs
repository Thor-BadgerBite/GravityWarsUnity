using System.Collections;
using UnityEngine;

/// <summary>
/// AI opponent for practice mode and offline play.
///
/// How it aims: simulates missile trajectories with the SAME physics the game
/// uses (planet gravity, drag, velocity clamp - mirrors
/// PlayerShip.PredictMissileTrajectory), samples many angle/velocity
/// combinations, picks the one whose path gets closest to the enemy ship, then
/// applies a difficulty-based aiming error before firing.
///
/// Difficulty 0 = wild shots, 1 = near-perfect.
///
/// Attached automatically by GameManager when 'player2IsBot' is enabled.
/// </summary>
[RequireComponent(typeof(PlayerShip))]
public class BotController : MonoBehaviour
{
    [Header("Behaviour")]
    [Range(0f, 1f)] public float difficulty = 0.6f;
    [Tooltip("Seconds the bot 'thinks' before shooting (feels human)")]
    public float minThinkTime = 1.2f;
    public float maxThinkTime = 3f;

    [Header("Simulation")]
    [Tooltip("Physics steps per simulated trajectory")]
    public int simulationSteps = 600;
    [Tooltip("Distance to the enemy that counts as a direct hit")]
    public float hitRadius = 1.6f;

    private PlayerShip _ship;
    private bool _actingThisTurn;
    private bool _wasControlsEnabled;

    public void Configure(float difficultyLevel)
    {
        difficulty = Mathf.Clamp01(difficultyLevel);
    }

    private void Awake()
    {
        _ship = GetComponent<PlayerShip>();
    }

    private void Update()
    {
        if (_ship == null || _ship.isGhost) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        bool isMyTurn = gm.CurrentPlayer == _ship && _ship.controlsEnabled;

        // Reset the per-turn flag when our controls get switched off
        if (_wasControlsEnabled && !_ship.controlsEnabled)
            _actingThisTurn = false;
        _wasControlsEnabled = _ship.controlsEnabled;

        if (isMyTurn && !_actingThisTurn)
        {
            _actingThisTurn = true;

            // Take over: block human keyboard input for this ship
            _ship.controlsEnabled = false;

            StartCoroutine(PlayTurn());
        }
    }

    private IEnumerator PlayTurn()
    {
        // "Think" for a moment so the bot feels human
        yield return new WaitForSeconds(Random.Range(minThinkTime, maxThinkTime));

        if (_ship == null || _ship.isDestroyed) yield break;

        PlayerShip enemy = FindEnemy();
        if (enemy == null || enemy.isDestroyed) yield break;

        // Find the best shot
        (float angle, float velocity, float score) best = FindBestShot(enemy);

        // Apply difficulty-based error (worse bots miss more)
        float errorScale = 1f - difficulty;
        float angleError = RandomGaussian() * errorScale * 10f;      // up to ~±10° at difficulty 0
        float velocityError = 1f + RandomGaussian() * errorScale * 0.12f;

        float finalAngle = best.angle + angleError;
        float finalVelocity = best.velocity * velocityError;

        _ship.BotSetAim(finalAngle, finalVelocity);

        // Small pause after aiming, then fire
        yield return new WaitForSeconds(0.4f);

        if (_ship != null && !_ship.isDestroyed)
        {
            Debug.Log($"[Bot] Firing - angle {finalAngle:F1}°, velocity {finalVelocity:F2} (best score {best.score:F2})");
            _ship.BotFire();
        }
    }

    private PlayerShip FindEnemy()
    {
        var gm = GameManager.Instance;
        if (gm == null) return null;
        return _ship.isLeftPlayer ? gm.player2Ship : gm.player1Ship;
    }

    #region Shot Search

    /// <summary>
    /// Two-pass search over angle/velocity space:
    /// coarse sweep (6° steps), then refinement around the best candidate.
    /// </summary>
    private (float angle, float velocity, float score) FindBestShot(PlayerShip enemy)
    {
        var range = _ship.GetLaunchVelocityRange();
        Vector3 target = enemy.transform.position;

        float bestAngle = 0f, bestVelocity = range.max, bestScore = float.MaxValue;

        // Pass 1: coarse sweep
        float[] coarseVelocities =
        {
            Mathf.Lerp(range.min, range.max, 0.4f),
            Mathf.Lerp(range.min, range.max, 0.7f),
            range.max
        };

        for (float angle = 0f; angle < 360f; angle += 6f)
        {
            foreach (float vel in coarseVelocities)
            {
                float score = SimulateShot(angle, vel, target);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestAngle = angle;
                    bestVelocity = vel;
                }
            }
        }

        // Pass 2: refine around the best candidate
        for (float angle = bestAngle - 6f; angle <= bestAngle + 6f; angle += 1.5f)
        {
            for (float t = -0.15f; t <= 0.15f; t += 0.075f)
            {
                float vel = Mathf.Clamp(bestVelocity * (1f + t), range.min, range.max);
                float score = SimulateShot(angle, vel, target);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestAngle = angle;
                    bestVelocity = vel;
                }
            }
        }

        return (bestAngle, bestVelocity, bestScore);
    }

    /// <summary>
    /// Simulates a shot and returns the closest distance the missile gets to
    /// the target (0 = direct hit). Mirrors PlayerShip.PredictMissileTrajectory:
    /// same gravity, drag, velocity clamping and the 0.5x launch factor.
    /// </summary>
    private float SimulateShot(float angleDegrees, float launchVelocity, Vector3 target)
    {
        // Missile physics parameters (same source as trajectory preview)
        float mass = 1.5f, drag = 0.01f, maxVel = 10f, approachRate = 0.1f;
        if (_ship.equippedMissile != null)
        {
            mass = _ship.equippedMissile.Mass;
            drag = _ship.equippedMissile.drag;
            maxVel = _ship.equippedMissile.maxVelocity;
            approachRate = _ship.equippedMissile.velocityApproachRate;
        }

        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        Vector3 pos = _ship.transform.position + dir * _ship.missileSpawnDistance;
        Vector3 vel = dir * launchVelocity * 0.5f; // matches Missile3D.Launch

        Planet[] planets = GameManager.GetCachedPlanets();
        float dt = Time.fixedDeltaTime;
        float closest = Vector3.Distance(pos, target);

        for (int i = 0; i < simulationSteps; i++)
        {
            Vector3 totalForce = Vector3.zero;
            foreach (Planet planet in planets)
            {
                totalForce += planet.CalculateGravitationalPull(pos, mass);
            }
            vel += (totalForce / mass) * dt;
            vel *= (1f - drag * dt);

            if (vel.magnitude > maxVel)
            {
                vel = Vector3.Lerp(vel, vel.normalized * maxVel, approachRate);
            }

            pos += vel * dt;

            float dist = Vector3.Distance(pos, target);
            if (dist < closest) closest = dist;

            // Direct hit - can't do better
            if (closest <= hitRadius) return 0f;

            // Planet collision ends the flight
            if (HitsPlanet(pos, planets)) break;
        }

        return closest;
    }

    private static bool HitsPlanet(Vector3 pos, Planet[] planets)
    {
        foreach (Planet planet in planets)
        {
            if (planet == null) continue;
            // Approximate planet radius from its scale
            float radius = planet.transform.localScale.x * 0.5f;
            if (Vector3.Distance(pos, planet.transform.position) < radius)
                return true;
        }
        return false;
    }

    /// <summary>Approximate standard normal via central limit (3 uniform samples).</summary>
    private static float RandomGaussian()
    {
        return (Random.value + Random.value + Random.value) - 1.5f;
    }

    #endregion
}
