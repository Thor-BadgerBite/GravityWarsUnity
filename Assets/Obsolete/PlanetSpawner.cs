#if false
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlanetSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PlanetInfo
    {
        public GameObject prefab;
        public string name;
        public float mass;
        public int units;
    }

    public PlanetInfo[] planetInfos;
    public int unitsToSpawn = 4;
    public float minDistanceBetweenPlanets = 1.5f;
    public int maxSpawnAttempts = 100;
    public int repositionIterations = 50;
    public float repositionForce = 1f;
    public float visiblePlanetPortion = 0.25f;
    public float spawnAreaMultiplier = 2.5f; // This will make the spawn area 50% larger than the visible area

    private List<PlanetInfo> availablePlanets;
    private float width;
    private float height;
    private List<SpawnedPlanet> spawnedPlanets = new List<SpawnedPlanet>();

    private class SpawnedPlanet
    {
        public GameObject gameObject;
        public Vector2 position;
        public float size;
    }

    void Start()
    {
        InitializeAvailablePlanets();
        SetupSpawnArea();
        SpawnPlanets();
    }

    void InitializeAvailablePlanets()
    {
        availablePlanets = new List<PlanetInfo>(planetInfos);
    }

    void SetupSpawnArea()
    {
        Camera mainCamera = Camera.main;
        height = 2f * mainCamera.orthographicSize;
        width = height * mainCamera.aspect;
    }

    void SpawnPlanets()
    {
        if (planetInfos.Length == 0)
        {
            Debug.LogError("No planet prefabs assigned!");
            return;
        }

        int remainingUnits = unitsToSpawn;
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(unitsToSpawn));
        float cellSize = Mathf.Max(width, height) * spawnAreaMultiplier / gridSize;

        while (remainingUnits > 0 && availablePlanets.Count > 0)
        {
            List<PlanetInfo> validPlanets = availablePlanets.Where(p => p.units <= remainingUnits).ToList();
            if (validPlanets.Count == 0) break;

            int randomIndex = Random.Range(0, validPlanets.Count);
            PlanetInfo selectedPlanetInfo = validPlanets[randomIndex];

            Vector2 position = GetGridPosition(gridSize, cellSize);
            SpawnPlanet(selectedPlanetInfo, position);
            
            availablePlanets.Remove(selectedPlanetInfo);
            remainingUnits -= selectedPlanetInfo.units;
        }

        RepositionPlanets();
    }

    Vector2 GetGridPosition(int gridSize, float cellSize)
    {
        float spawnWidth = width * spawnAreaMultiplier;
        float spawnHeight = height * spawnAreaMultiplier;
        float startX = -spawnWidth / 2 + cellSize / 2;
        float startY = -spawnHeight / 2 + cellSize / 2;

        int x = Random.Range(0, gridSize);
        int y = Random.Range(0, gridSize);

        return new Vector2(startX + x * cellSize, startY + y * cellSize);
    }

    void SpawnPlanet(PlanetInfo planetInfo, Vector2 position)
    {
        GameObject planet = Instantiate(planetInfo.prefab, position, Quaternion.identity);
        
        Planet planetComponent = planet.GetComponent<Planet>() ?? planet.AddComponent<Planet>();
        planetComponent.SetPlanetProperties(planetInfo.name, planetInfo.mass);

        planet.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        Renderer planetRenderer = planet.GetComponent<Renderer>();
        float size = Mathf.Max(planetRenderer.bounds.size.x, planetRenderer.bounds.size.y);

        spawnedPlanets.Add(new SpawnedPlanet { gameObject = planet, position = position, size = size });
    }

    void RepositionPlanets()
    {
        for (int i = 0; i < repositionIterations; i++)
        {
            bool anyOverlap = false;
            foreach (var planet in spawnedPlanets)
            {
                Vector2 force = Vector2.zero;
                foreach (var otherPlanet in spawnedPlanets)
                {
                    if (planet != otherPlanet)
                    {
                        Vector2 direction = planet.position - otherPlanet.position;
                        float distance = direction.magnitude;
                        float minDistance = (planet.size + otherPlanet.size) * minDistanceBetweenPlanets / 2;

                        if (distance < minDistance)
                        {
                            anyOverlap = true;
                            float forceMagnitude = (minDistance - distance) / minDistance;
                            force += direction.normalized * forceMagnitude * repositionForce;
                        }
                    }
                }

                planet.position += force;
                planet.position = EnsurePartialVisibility(planet.position, planet.size);
            }

            if (!anyOverlap) break;
        }

        // Apply final positions
        foreach (var planet in spawnedPlanets)
        {
            planet.gameObject.transform.position = planet.position;
        }
    }

    Vector2 EnsurePartialVisibility(Vector2 position, float size)
    {
        float halfSize = size / 2;
        float visibleSize = size * visiblePlanetPortion;

        float minX = -width / 2 - halfSize + visibleSize;
        float maxX = width / 2 + halfSize - visibleSize;
        float minY = -height / 2 - halfSize + visibleSize;
        float maxY = height / 2 + halfSize - visibleSize;

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }
}
#endif