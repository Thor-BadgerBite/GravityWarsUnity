#if false
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static DebugExtensions;

public class Missile : MonoBehaviour
{
public delegate void MissileDestroyed(Missile missile);
public static event MissileDestroyed OnMissileDestroyed; // Event for missile destruction
// Existing variables...
private string lastCollisionInfo = "";

[Header("Missile Lost Detection")]
public float gravitationalForceThreshold = 0.01f; // Minimum gravitational force to consider the missile "in play"
public float distanceThreshold = 30f; // Distance threshold beyond which the missile is considered lost
public float velocityThreshold = 0.1f; // Minimum velocity threshold to check if missile is moving away
public float lostCheckDelay = 2f; // Added delay before considering missile lost
public bool IsLostInSpace { get; private set; }

private Vector2 totalGravitationalForce;
private bool missileLostInSpace = false;
private float timeSincePotentiallyLost = 0f;

private Vector2 playfieldCenter = Vector2.zero; // Assuming the playfield is centered at (0, 0)

private CameraController cameraController;
public float maxVelocity = 10f;
public float velocityApproachRate = 0.1f;
// public int maxTrailPoints = 1000;
public float trailPointInterval = 0.01f;
public LineRenderer trajectoryLine;
public float collisionDelay = 0.5f;
public float missileMass = 10f;
public float drag = 0.01f;
public Color maxVelocityColor = Color.red;

public float heatShieldThreshold = 0.8f;
public Color heatShieldColor = new Color(1f, 0.5f, 0f, 0.7f);
public int heatShieldParticleCount = 50;
public float heatShieldParticleSize = 0.1f;

private List<Vector3> trajectoryPoints = new List<Vector3>();
private float lastTrailPointTime;
private Rigidbody2D rb;
private Collider2D missileCollider;
private GameObject trailObject;
private GameObject firedByShip;
private SpriteRenderer spriteRenderer;
private Color originalColor;
private ParticleSystem heatShieldEffect;
private Vector3 startPosition;
private AudioSource flyingSoundSource;


void Start()
{
playfieldCenter = Vector2.zero; // Or wherever your playfield center is
//Debug.Log($"Playfield center set to {playfieldCenter}");
// ... rest of your Start method
}
void Awake()
{
rb = GetComponent<Rigidbody2D>();

if (rb == null)
{
rb = gameObject.AddComponent<Rigidbody2D>();
}
rb.gravityScale = 0;
rb.mass = missileMass;
rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
// Get the CameraController instance
cameraController = Camera.main.GetComponent<CameraController>();
missileCollider = GetComponent<Collider2D>();
if (missileCollider == null)
{
missileCollider = gameObject.AddComponent<CircleCollider2D>();
}
missileCollider.enabled = false;

spriteRenderer = GetComponent<SpriteRenderer>();
if (spriteRenderer != null)
{
originalColor = spriteRenderer.color;
}

SetupTrajectoryLine();
SetupHeatShieldEffect();
flyingSoundSource = gameObject.AddComponent<AudioSource>();
flyingSoundSource.clip = AudioManager.Instance.missileFlySFX;
flyingSoundSource.loop = true;
flyingSoundSource.playOnAwake = false;
flyingSoundSource.volume = AudioManager.Instance.GetSFXVolume();
}

void SetupTrajectoryLine()
{
trailObject = new GameObject("MissileTrail");
trajectoryLine = trailObject.AddComponent<LineRenderer>();
trajectoryLine.startWidth = 0.1f;
trajectoryLine.endWidth = 0.1f;
trajectoryLine.positionCount = 0;
trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
trajectoryLine.startColor = Color.red;
trajectoryLine.endColor = Color.yellow;
trajectoryLine.sortingLayerName = "Planets";
trajectoryLine.sortingOrder = 1;
}

void SetupHeatShieldEffect()
{
GameObject heatShieldObj = new GameObject("HeatShieldEffect");
heatShieldObj.transform.SetParent(transform);
heatShieldObj.transform.localPosition = Vector3.forward * 0.5f; // Slightly in front of the missile

heatShieldEffect = heatShieldObj.AddComponent<ParticleSystem>();
var main = heatShieldEffect.main;
main.simulationSpace = ParticleSystemSimulationSpace.World;
main.startLifetime = 0.2f;
main.startSpeed = 2f;
main.startSize = 0.05f;
main.maxParticles = 1000;

var emission = heatShieldEffect.emission;
emission.rateOverTime = 0;

var shape = heatShieldEffect.shape;
shape.shapeType = ParticleSystemShapeType.Cone;
shape.angle = 30f;
shape.radius = 0.1f;

var colorOverLifetime = heatShieldEffect.colorOverLifetime;
colorOverLifetime.enabled = true;
Gradient gradient = new Gradient();
gradient.SetKeys(
new GradientColorKey[] {
new GradientColorKey(Color.white, 0.0f),
new GradientColorKey(new Color(1f, 0.7f, 0f), 0.2f),
new GradientColorKey(new Color(1f, 0.4f, 0f), 0.5f),
new GradientColorKey(new Color(1f, 0.1f, 0f), 1.0f)
},
new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(0f, 1.0f) }
);
colorOverLifetime.color = gradient;

var velocityOverLifetime = heatShieldEffect.velocityOverLifetime;
velocityOverLifetime.enabled = true;

// Ensure all velocity components (x, y, z) are in the same mode (random between two constants)
velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f); // Random between two constants
velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-1f, 1f); // Random between two constants
velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f); // Ensure z is set in the same mode
}

private Texture2D CreateCircleTexture(int resolution)
{
Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
Color[] colors = new Color[resolution * resolution];

Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
float radius = resolution / 2f;

for (int y = 0; y < resolution; y++)
{
for (int x = 0; x < resolution; x++)
{
float distance = Vector2.Distance(new Vector2(x, y), center);
float t = Mathf.Clamp01(1f - distance / radius);
float alpha = SmoothStep(0f, 1f, t);
colors[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
}
}

texture.SetPixels(colors);
texture.Apply();
return texture;
}

private float SmoothStep(float edge0, float edge1, float x)
{
x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
return x * x * (3 - 2 * x);
}
public void Launch(Vector2 direction, float speed, GameObject firingShip)
{
startPosition = transform.position;
rb.velocity = direction.normalized * speed * 0.5f;
firedByShip = firingShip;
PlayerShip playerShip = firingShip.GetComponent<PlayerShip>();
if (playerShip != null)
{
playerShip.IncrementShotCounter();
}
Debug.Log($"Missile launched from {transform.position} with velocity: {rb.velocity}");
// Start following the missile when it's launched
if (cameraController != null)
{
//cameraController.StartFollowingMissile(this);
}

trajectoryPoints.Clear();
AddTrajectoryPoint(transform.position);
Invoke("EnableCollider", collisionDelay);
AudioManager.Instance.PlayMissileLaunch();
flyingSoundSource.Play(); // Start the looping fly sound
}
public Vector3 GetStartPosition()
{
return startPosition;
}
void EnableCollider()
{
missileCollider.enabled = true;
//firedByShip = null;
}

void FixedUpdate()
{
ApplyGravity();
MoveMissile();
RotateMissile();
UpdateTrajectory();
CheckIfMissileIsLost();
flyingSoundSource.volume = AudioManager.Instance.GetSFXVolume();
//Debug.Log($"Missile at {transform.position} with velocity: {rb.velocity}");
}
void ApplyGravity()
{
Planet[] planets = FindObjectsOfType<Planet>();
totalGravitationalForce = Vector2.zero;

foreach (Planet planet in planets)
{
  
  
  
  
  
  
  
  
    //totalGravitationalForce += planet.CalculateGravitationalPull(rb.position, missileMass);











}

rb.AddForce(totalGravitationalForce);
//Debug.Log($"Total Gravitational Force: {totalGravitationalForce}");
}

void MoveMissile()
{
// Apply drag
rb.velocity *= (1 - drag * Time.fixedDeltaTime);

// Gradually approach max velocity
float currentSpeed = rb.velocity.magnitude;
if (currentSpeed > maxVelocity)
{
Vector2 velocityDirection = rb.velocity.normalized;
rb.velocity = Vector2.Lerp(rb.velocity, velocityDirection * maxVelocity, velocityApproachRate);
}

// Visual indicator of max velocity
UpdateMaxVelocityIndicator(currentSpeed);

// Update heat shield effect
UpdateHeatShieldEffect(currentSpeed);

// Let the physics engine handle the movement
}

void UpdateMaxVelocityIndicator(float currentSpeed)
{
if (spriteRenderer != null)
{
float t = Mathf.Clamp01(currentSpeed / maxVelocity);
spriteRenderer.color = Color.Lerp(originalColor, maxVelocityColor, t);
}
}

void UpdateHeatShieldEffect(float currentSpeed)
{
float speedRatio = currentSpeed / maxVelocity;
var emission = heatShieldEffect.emission;
var main = heatShieldEffect.main;

if (speedRatio >= heatShieldThreshold)
{
float intensityFactor = Mathf.InverseLerp(heatShieldThreshold, 1f, speedRatio);
emission.rateOverTime = 1000 * intensityFactor;
main.startSize = 0.05f * (0.5f + 0.5f * intensityFactor);
main.startSpeed = 2f + 3f * intensityFactor;

if (!heatShieldEffect.isPlaying)
{
heatShieldEffect.Play();
}
}
else
{
emission.rateOverTime = 0;
if (heatShieldEffect.isPlaying)
{
heatShieldEffect.Stop();
}
}
}
void RotateMissile()
{
if (rb.velocity != Vector2.zero)
{
float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
}
}

void UpdateTrajectory()
{
if (Time.time - lastTrailPointTime >= trailPointInterval)
{
AddTrajectoryPoint(transform.position);
lastTrailPointTime = Time.time;
}
}
// This method checks if the missile is lost in space
void CheckIfMissileIsLost()
{
if (missileLostInSpace) return;

float distanceFromPlayfield = Vector2.Distance(transform.position, playfieldCenter);
Vector2 directionToPlayfield = (playfieldCenter - (Vector2)transform.position).normalized;
float velocityDotProduct = Vector2.Dot(rb.velocity.normalized, directionToPlayfield);

bool potentiallyLost = totalGravitationalForce.magnitude < gravitationalForceThreshold &&
distanceFromPlayfield > distanceThreshold &&
velocityDotProduct < velocityThreshold;

if (potentiallyLost)
{
timeSincePotentiallyLost += Time.fixedDeltaTime;
if (timeSincePotentiallyLost >= lostCheckDelay)
{
missileLostInSpace = true;
IsLostInSpace = true;
//Debug.Log($"Missile Lost in Space! Distance: {distanceFromPlayfield}, Grav Force: {totalGravitationalForce.magnitude}, Velocity Dot: {velocityDotProduct}");
GameManager.Instance.MissileLostInSpace();
DestroyMissile();
}
}
else
{
timeSincePotentiallyLost = 0f;
}

// Debug visualization
VisualizeThresholds(distanceFromPlayfield, velocityDotProduct);
}
void VisualizeThresholds(float distance, float velocityDot)
{
//Debug.Log("VisualizeThresholds called");
// Visualize distance threshold
Debug.DrawLine(playfieldCenter, transform.position, Color.yellow);
DebugExtensions.DrawCircle(playfieldCenter, distanceThreshold, Color.red);

// Visualize velocity direction
Debug.DrawRay(transform.position, rb.velocity.normalized * 5f, Color.blue);
Debug.DrawRay(transform.position, (playfieldCenter - (Vector2)transform.position).normalized * 5f, Color.green);

// Log values
//Debug.Log($"Distance: {distance}, Grav Force: {totalGravitationalForce.magnitude}, Velocity Dot: {velocityDot}");
}
public static class DebugExtension
{
public static void DrawCircle(Vector3 position, float radius, Color color, int segments = 32)
{
Vector3 prevPos = position + new Vector3(radius, 0, 0);
for (int i = 0; i < segments + 1; i++)
{
float angle = (float)i / (float)segments * 360 * Mathf.Deg2Rad;
Vector3 newPos = position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
Debug.DrawLine(prevPos, newPos, color);
prevPos = newPos;
}
}
}
void AddTrajectoryPoint(Vector3 point)
{
trajectoryPoints.Add(point);
trajectoryLine.positionCount = trajectoryPoints.Count;
trajectoryLine.SetPositions(trajectoryPoints.ToArray());
}

void OnCollisionEnter2D(Collision2D collision)
{
Vector2 collisionPoint = collision.GetContact(0).point;
HandleCollision(collision.collider, collisionPoint);
}

void HandleCollision(Collider2D collider, Vector2 collisionPoint)
{
//Debug.Log($"Missile collided with: {collider.gameObject.name} at {collisionPoint}");

if (collider.GetComponent<Planet>() != null)
{
Planet planet = collider.GetComponent<Planet>();
lastCollisionInfo = $"collided with {planet.planetName}";
DestroyMissile(collisionPoint);
}
else if (collider.GetComponent<PlayerShip>() != null)
{
PlayerShip ship = collider.GetComponent<PlayerShip>();
lastCollisionInfo = $"hit {ship.playerName}'s ship!";
Debug.Log($"Missile hit {ship.playerName}'s ship!");
DestroyEnemyShip(ship, collisionPoint);
DestroyMissile(collisionPoint);
}
else
{
lastCollisionInfo = "collided with an object";
DestroyMissile(collisionPoint);
}
}

void DestroyEnemyShip(PlayerShip ship, Vector2 collisionPoint)
{
Debug.Log($"Destroying {ship.playerName}'s ship at {collisionPoint}");

// Create explosion effect where the ship was hit
CreateExplosionEffect(ship.transform.position);

// Hide the ship instead of destroying it
ship.HideShip();

// Notify the GameManager that the ship was destroyed
GameManager.Instance.ShipDestroyed(ship);

// Destroy the missile
Destroy(gameObject);
}
public string GetLastCollisionInfo()
{
return lastCollisionInfo;
}
void CreateExplosionEffect(Vector3 position)
{
GameObject explosionObj = new GameObject("ExplosionEffect");
explosionObj.transform.position = position;

ParticleSystem explosionSystem = explosionObj.AddComponent<ParticleSystem>();

// Stop the particle system if it's already playing
if (explosionSystem.isPlaying)
{
explosionSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
}

// Configure the particle system now that it's stopped
var main = explosionSystem.main;
main.duration = 0.5f; // Set the duration only when the system is stopped
main.loop = false;
main.startLifetime = 0.5f;
main.startSpeed = 5f;
main.startSize = 0.5f;
main.maxParticles = 500;

var emission = explosionSystem.emission;
emission.rateOverTime = 0;
emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 500) });

var shape = explosionSystem.shape;
shape.shapeType = ParticleSystemShapeType.Sphere;
shape.radius = 0.1f;

var colorOverLifetime = explosionSystem.colorOverLifetime;
colorOverLifetime.enabled = true;
Gradient gradient = new Gradient();
gradient.SetKeys(
new GradientColorKey[] {
new GradientColorKey(Color.white, 0f),
new GradientColorKey(Color.yellow, 0.2f),
new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
new GradientColorKey(Color.red, 0.8f)
},
new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
);
colorOverLifetime.color = gradient;

var sizeOverLifetime = explosionSystem.sizeOverLifetime;
sizeOverLifetime.enabled = true;
sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

var renderer = explosionSystem.GetComponent<ParticleSystemRenderer>();
renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
renderer.material.mainTexture = CreateCircleTexture(32);

// Play the particle system after configuration
explosionSystem.Play();

// Destroy the particle system after its duration + start lifetime
Destroy(explosionObj, main.duration + main.startLifetime.constantMax);
}

public void DestroyMissile(Vector2? collisionPoint = null)
{
Vector2 finalPoint = collisionPoint ?? (Vector2)transform.position;

// Update the trail one last time to the final point
if (trailObject != null)
{
trailObject.transform.SetParent(null);
UpdateTrailToCollisionPoint(finalPoint);

// Don't destroy the trail object here
// Instead, pass it to the player ship to manage
if (firedByShip != null)
{
PlayerShip playerShip = firedByShip.GetComponent<PlayerShip>();
if (playerShip != null)
{
playerShip.SetLastMissileTrail(trailObject);
trailObject = null; // Prevent double destruction
}
}

if (trailObject != null)
{
Destroy(trailObject);
}
}

CreateExplosionEffect(finalPoint);

// Destroy other components immediately
if (heatShieldEffect != null)
{
Destroy(heatShieldEffect.gameObject);
}

// Notify the GameManager about the missile destruction
OnMissileDestroyed?.Invoke(this);

// Stop following the missile when it's destroyed
if (cameraController != null)
{
cameraController.StopFollowingMissile();
}

// Destroy the missile object
flyingSoundSource.Stop(); // Stop the flying sound
AudioManager.Instance.PlayMissileDestroyed();
Destroy(gameObject);
}
void OnDestroy()
{
if (flyingSoundSource != null && flyingSoundSource.isPlaying)
{
flyingSoundSource.Stop();
}
}
void UpdateTrailToCollisionPoint(Vector3 collisionPoint)
{
// Add the collision point to the trajectory
AddTrajectoryPoint(collisionPoint);

// Update the LineRenderer
trajectoryLine.positionCount = trajectoryPoints.Count;
trajectoryLine.SetPositions(trajectoryPoints.ToArray());
}
IEnumerator FadeOutTrail()
{
float fadeDuration = 2f;
float elapsedTime = 0f;
Color startColor = trajectoryLine.startColor;
Color endColor = trajectoryLine.endColor;

while (elapsedTime < fadeDuration)
{
elapsedTime += Time.deltaTime;
float alpha = 1f - (elapsedTime / fadeDuration);

trajectoryLine.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
trajectoryLine.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);

yield return null;
}

Destroy(trailObject);
}
}
#endif