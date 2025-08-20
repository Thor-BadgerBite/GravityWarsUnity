#if false
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameEngine : MonoBehaviour
{
    public static GameEngine Instance { get; private set; }

    [Header("Planet Spawning")]
    public PlanetInfo[] planetInfos;
    public float minDistanceBetweenPlanets = 1.5f;
    public int maxSpawnAttempts = 100;
    public int repositionIterations = 5;
    public float repositionForce = 0.5f;

    [Header("Ship Placement")]
    public GameObject playerShipPrefab;
    public float minDistanceFromCenter = 25f;
    public float maxDistanceFromCenter = 28f;
    public float shipCollisionRadius = 1f;
    public int maxShipPlacementAttempts = 10;
    public float topBottomOffset = 5f;

    private List<SpawnedPlanet> spawnedPlanets = new List<SpawnedPlanet>();
    private List<Planet> planetComponents = new List<Planet>();
    private List<PlanetInfo> availablePlanets;
    private float width;
    private float height;

    [System.Serializable]
    public class PlanetInfo
    {
        public GameObject prefab;
        public string name;
        public float mass;
        public int units;
    }

    private class SpawnedPlanet
    {
        public GameObject gameObject;
        public Vector2 position;
        public float size;
    }

void Awake()
{
    Debug.Log($"GameEngine Awake called on {gameObject.name}");
    
    if (Instance == null)
    {
        Debug.Log("Setting GameEngine Instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Debug.Log($"Destroying duplicate GameEngine on {gameObject.name}");
        Destroy(gameObject);
    }
}

    public void InitializeSpawnArea()
    {
        Camera mainCamera = Camera.main;
        height = 2f * mainCamera.orthographicSize;
        width = height * mainCamera.aspect;
    }

    public void SpawnPlanets(int unitsToSpawn)
    {
        InitializeAvailablePlanets();

        if (planetInfos.Length == 0)
        {
            Debug.LogError("No planet prefabs assigned!");
            return;
        }

        int remainingUnits = unitsToSpawn;

        while (remainingUnits > 0 && availablePlanets.Count > 0)
        {
            List<PlanetInfo> validPlanets = availablePlanets.Where(p => p.units <= remainingUnits).ToList();
            if (validPlanets.Count == 0)
            {
                break;
            }

            int randomIndex = Random.Range(0, validPlanets.Count);
            PlanetInfo selectedPlanetInfo = validPlanets[randomIndex];
            availablePlanets.Remove(selectedPlanetInfo);

            Vector2 position;
            bool validPosition = FindValidSpawnPosition(selectedPlanetInfo.prefab, out position);

            if (validPosition)
            {
                SpawnPlanet(selectedPlanetInfo, position);
                remainingUnits -= selectedPlanetInfo.units;
            }
            else
            {
                Debug.LogWarning($"Could not spawn planet '{selectedPlanetInfo.name}': No valid position found");
            }
        }

        RepositionPlanets();
    }

    private void InitializeAvailablePlanets()
    {
        availablePlanets = new List<PlanetInfo>(planetInfos);
    }

    private bool FindValidSpawnPosition(GameObject prefab, out Vector2 position)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            position = new Vector2(
                Random.Range(-width / 2, width / 2),
                Random.Range(-height / 2, height / 2)
            );

            if (IsValidSpawnPosition(position, prefab))
            {
                return true;
            }
        }

        position = Vector2.zero;
        return false;
    }

    private bool IsValidSpawnPosition(Vector2 position, GameObject prefab)
    {
        Renderer prefabRenderer = prefab.GetComponent<Renderer>();
        if (prefabRenderer == null)
        {
            Debug.LogError($"Prefab {prefab.name} does not have a Renderer component!");
            return false;
        }

        float prefabSize = Mathf.Max(prefabRenderer.bounds.size.x, prefabRenderer.bounds.size.y);

        foreach (var planet in spawnedPlanets)
        {
            float minDistance = (prefabSize + planet.size) * minDistanceBetweenPlanets / 2;
            if (Vector2.Distance(position, planet.position) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    private void SpawnPlanet(PlanetInfo planetInfo, Vector2 position)
    {
        GameObject planet = Instantiate(planetInfo.prefab, position, Quaternion.identity);
        
        Planet planetComponent = planet.GetComponent<Planet>() ?? planet.AddComponent<Planet>();
        planetComponent.SetPlanetProperties(planetInfo.name, planetInfo.mass);
        planetComponents.Add(planetComponent);

        planet.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        Renderer planetRenderer = planet.GetComponent<Renderer>();
        float size = Mathf.Max(planetRenderer.bounds.size.x, planetRenderer.bounds.size.y);

        spawnedPlanets.Add(new SpawnedPlanet { gameObject = planet, position = position, size = size });
    }

    private void RepositionPlanets()
    {
        for (int i = 0; i < repositionIterations; i++)
        {
            RepositionOverlappingPlanets();
        }

        foreach (var planet in spawnedPlanets)
        {
            planet.gameObject.transform.position = planet.position;
        }
    }

    private void RepositionOverlappingPlanets()
    {
        for (int i = 0; i < spawnedPlanets.Count; i++)
        {
            for (int j = i + 1; j < spawnedPlanets.Count; j++)
            {
                SpawnedPlanet p1 = spawnedPlanets[i];
                SpawnedPlanet p2 = spawnedPlanets[j];

                float minDistance = (p1.size + p2.size) * minDistanceBetweenPlanets / 2;
                Vector2 direction = p2.position - p1.position;
                float currentDistance = direction.magnitude;

                if (currentDistance < minDistance)
                {
                    float overlap = minDistance - currentDistance;
                    direction = direction.normalized;

                    Vector2 repositionVector = direction * overlap * repositionForce;
                    p1.position -= repositionVector;
                    p2.position += repositionVector;

                    p1.position = ClampToSpawnArea(p1.position, p1.size);
                    p2.position = ClampToSpawnArea(p2.position, p2.size);
                }
            }
        }
    }

    private Vector2 ClampToSpawnArea(Vector2 position, float size)
    {
        float halfSize = size / 2;
        float minX = -width / 2 + halfSize;
        float maxX = width / 2 - halfSize;
        float minY = -height / 2 + halfSize;
        float maxY = height / 2 - halfSize;

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }

    public Vector3 GetPlayerShipPosition(bool isLeftSide)
    {
        for (int attempt = 0; attempt < maxShipPlacementAttempts; attempt++)
        {
            Vector3 position = GetRandomShipPosition(isLeftSide);
            if (!ShipOverlapsWithPlanet(position) && IsWithinValidVerticalRange(position.y))
            {
                return position;
            }
        }

        Debug.LogWarning("Could not find a valid position for the ship. Placing at default position.");
        return GetRandomShipPosition(isLeftSide);
    }

    private Vector3 GetRandomShipPosition(bool isLeftSide)
    {
        float horizontalPosition = Random.Range(minDistanceFromCenter, maxDistanceFromCenter);
        if (isLeftSide) horizontalPosition = -horizontalPosition;

        float verticalPosition = Random.Range(-height / 2f + topBottomOffset, height / 2f - topBottomOffset);

        return new Vector3(horizontalPosition, verticalPosition, 0);
    }

private bool ShipOverlapsWithPlanet(Vector3 position)
{
    foreach (Planet planet in planetComponents)
    {
        SphereCollider planetCollider = planet.GetComponent<SphereCollider>();
        if (planetCollider == null)
        {
            // Add SphereCollider if missing
            planetCollider = planet.gameObject.AddComponent<SphereCollider>();
            // Set the radius based on the planet's renderer bounds
            Renderer renderer = planet.GetComponent<Renderer>();
            if (renderer != null)
            {
                planetCollider.radius = renderer.bounds.extents.magnitude / 2f;
            }
        }

        float distance = Vector3.Distance(position, planet.transform.position);
        float minDistance = shipCollisionRadius + planetCollider.radius * planet.transform.localScale.x;
        if (distance < minDistance)
        {
            return true;
        }
    }
    return false;
}

    private bool IsWithinValidVerticalRange(float yPosition)
    {
        return yPosition >= -height / 2f + topBottomOffset && 
               yPosition <= height / 2f - topBottomOffset;
    }

    public void ClearGameObjects()
    {
        foreach (var planet in FindObjectsOfType<Planet>())
        {
            Destroy(planet.gameObject);
        }
        planetComponents.Clear();
        spawnedPlanets.Clear();

        PlayerShip[] existingShips = FindObjectsOfType<PlayerShip>();
        foreach (var ship in existingShips)
        {
            Destroy(ship.gameObject);
        }
    }

    public List<Planet> GetPlanetComponents()
    {
        return planetComponents;
    }

    public float GetWorldWidth()
    {
        return width;
    }

    public float GetWorldHeight()
    {
        return height;
    }
}
#endif