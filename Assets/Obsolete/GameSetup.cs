#if false
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameSetup : MonoBehaviour
{
    [System.Serializable]
    public class PlanetInfo
    {
        public GameObject prefab;
        public string name;
        public float mass;
        public int units;
    }

    [Header("Planet Spawning")]
    public PlanetInfo[] planetInfos;
    public int unitsToSpawn = 4;
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

    private List<PlanetInfo> availablePlanets;
    private float width;
    private float height;
    private List<SpawnedPlanet> spawnedPlanets = new List<SpawnedPlanet>();
    private List<Planet> planetComponents = new List<Planet>();

    private class SpawnedPlanet
    {
        public GameObject gameObject;
        public Vector2 position;
        public float size;
    }

    void Start()
    {
        SetupSpawnArea();
        SpawnPlanets();
        PlaceShips();
    }

    void SetupSpawnArea()
    {
        Camera mainCamera = Camera.main;
        height = 2f * mainCamera.orthographicSize;
        width = height * mainCamera.aspect;
        //Debug.Log($"Spawn area set up: Width = {width}, Height = {height}");
    }

    void SpawnPlanets()
    {
        InitializeAvailablePlanets();

        if (planetInfos.Length == 0)
        {
            Debug.LogError("No planet prefabs assigned!");
            return;
        }

        int remainingUnits = unitsToSpawn;
        //Debug.Log($"Starting planet spawning. Units to spawn: {remainingUnits}");

        while (remainingUnits > 0 && availablePlanets.Count > 0)
        {
            List<PlanetInfo> validPlanets = availablePlanets.Where(p => p.units <= remainingUnits).ToList();
            if (validPlanets.Count == 0)
            {
                //Debug.Log("No more valid planets to spawn.");
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

            //Debug.Log($"Remaining units: {remainingUnits}");
        }

        RepositionPlanets();
    }
    public void ClearPlanets()
    {
        foreach (var planet in planetComponents)
        {
            Destroy(planet.gameObject);  // Destroy the planet GameObject
        }
        planetComponents.Clear();  // Clear the list of planet references
        spawnedPlanets.Clear();    // Also clear the list of spawned planets

        Debug.Log("Cleared all planets for the new round.");
    }    

    void InitializeAvailablePlanets()
    {
        availablePlanets = new List<PlanetInfo>(planetInfos);
        foreach (var planet in availablePlanets)
        {
            //Debug.Log($"Initialized planet: {planet.name}, Mass: {planet.mass}, Units: {planet.units}");
        }
    }

    bool FindValidSpawnPosition(GameObject prefab, out Vector2 position)
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

    bool IsValidSpawnPosition(Vector2 position, GameObject prefab)
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

    void SpawnPlanet(PlanetInfo planetInfo, Vector2 position)
    {
        GameObject planet = Instantiate(planetInfo.prefab, position, Quaternion.identity);
        
        Planet planetComponent = planet.GetComponent<Planet>() ?? planet.AddComponent<Planet>();
        planetComponent.SetPlanetProperties(planetInfo.name, planetInfo.mass);
        planetComponents.Add(planetComponent);

        planet.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        Renderer planetRenderer = planet.GetComponent<Renderer>();
        float size = Mathf.Max(planetRenderer.bounds.size.x, planetRenderer.bounds.size.y);

        spawnedPlanets.Add(new SpawnedPlanet { gameObject = planet, position = position, size = size });

        //Debug.Log($"Planet '{planetInfo.name}' spawned at {position}, Size: {size}, Scale: {planet.transform.localScale}");
    }

    void RepositionPlanets()
    {
        //Debug.Log("Repositioning overlapping planets...");
        for (int i = 0; i < repositionIterations; i++)
        {
            RepositionOverlappingPlanets();
        }

        foreach (var planet in spawnedPlanets)
        {
            planet.gameObject.transform.position = planet.position;
            //Debug.Log($"Final position for '{planet.gameObject.name}': {planet.position}, Size: {planet.size}");
        }
    }

    void RepositionOverlappingPlanets()
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

                    //Debug.Log($"Adjusted positions: {p1.gameObject.name} to {p1.position}, {p2.gameObject.name} to {p2.position}");
                }
            }
        }
    }

    Vector2 ClampToSpawnArea(Vector2 position, float size)
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

    void PlaceShips()
    {
        // Player 1 (left side)
        Vector3 player1Position = GetValidShipPosition(true);
        GameObject player1 = Instantiate(playerShipPrefab, player1Position, Quaternion.Euler(0, 0, 0));
        player1.name = "Player1Ship";
        PlayerShip player1Ship = player1.GetComponent<PlayerShip>();
        player1Ship.playerName = "Player 1";

        // Player 2 (right side)
        Vector3 player2Position = GetValidShipPosition(false);
        GameObject player2 = Instantiate(playerShipPrefab, player2Position, Quaternion.Euler(0, 0, 180));
        player2.name = "Player2Ship";
        PlayerShip player2Ship = player2.GetComponent<PlayerShip>();
        player2Ship.playerName = "Player 2";

        // Set up GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.player1Ship = player1Ship;
            gameManager.player2Ship = player2Ship;
        }
        else
        {
            Debug.LogError("GameManager not found in the scene!");
        }
    }

    public void RepositionShips()
    {
        // Reposition Player 1
        Vector3 player1Position = GetValidShipPosition(true);
        if (GameManager.Instance.player1Ship != null)
        {
            GameManager.Instance.player1Ship.transform.position = player1Position;
            GameManager.Instance.player1Ship.ShowShip();  // Make Player 1's ship visible agai
            Debug.Log("Repositioned Player 1.");
        }

        // Reposition Player 2
        Vector3 player2Position = GetValidShipPosition(false);
        if (GameManager.Instance.player2Ship != null)
        {
            GameManager.Instance.player2Ship.transform.position = player2Position;
            GameManager.Instance.player2Ship.ShowShip();  // Make Player 2's ship visible again
            Debug.Log("Repositioned Player 2.");
        }
    }
    Vector3 GetValidShipPosition(bool isLeftSide)
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

    Vector3 GetRandomShipPosition(bool isLeftSide)
    {
        float horizontalPosition = Random.Range(minDistanceFromCenter, maxDistanceFromCenter);
        if (isLeftSide) horizontalPosition = -horizontalPosition;

        float verticalPosition = Random.Range(-height / 2f + topBottomOffset, height / 2f - topBottomOffset);

        return new Vector3(horizontalPosition, verticalPosition, 0);
    }

    bool ShipOverlapsWithPlanet(Vector3 position)
    {
        foreach (Planet planet in planetComponents)
        {
            CircleCollider2D planetCollider = planet.GetComponent<CircleCollider2D>();
            if (planetCollider == null)
            {
                Debug.LogWarning($"Planet {planet.name} does not have a CircleCollider2D component!");
                continue;
            }

            float distance = Vector2.Distance(position, planet.transform.position);
            float minDistance = shipCollisionRadius + planetCollider.radius * planet.transform.localScale.x;
            if (distance < minDistance)
            {
                return true;
            }
        }
        return false;
    }

    bool IsWithinValidVerticalRange(float yPosition)
    {
        return yPosition >= -height / 2f + topBottomOffset && 
               yPosition <= height / 2f - topBottomOffset;
    }
    public void ResetForNewRound()
    {
        ClearAllMissileTrails(); // Add this line to clear all trails
        ClearPlanets();            // Remove old planets
        SpawnPlanets();            // Spawn new random planets
        RepositionShips();         // Reposition the ships to new random positions
        //GameManager.Instance.ClearAllMissileTrails(); // Use GameManager's method

        Debug.Log("Reset the game for the new round.");
    }
    public void ClearAllMissileTrails()
    {
        if (GameManager.Instance.player1Ship != null)
        {
            GameManager.Instance.player1Ship.ClearLastMissileTrail();
        }
        if (GameManager.Instance.player2Ship != null)
        {
            GameManager.Instance.player2Ship.ClearLastMissileTrail();
        }
        Debug.Log("Cleared all missile trails for the new round.");
    }

}
#endif