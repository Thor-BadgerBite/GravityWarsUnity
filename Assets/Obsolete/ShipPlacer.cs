#if false
using UnityEngine;
using System.Collections.Generic;

public class ShipPlacer : MonoBehaviour
{
    public GameObject playerShipPrefab;
    public float minDistanceFromCenter = 25f;
    public float maxDistanceFromCenter = 28f;
    public float shipCollisionRadius = 1f; // Adjust based on your ship's size
    public int maxPlacementAttempts = 10;
    public float topBottomOffset = 5f; // New variable for top/bottom offset

    private float screenHeight;
    private float screenWidth;
    private List<Planet> planets;

    void Start()
    {
        CalculateScreenDimensions();
        GetAllPlanets();
        PlaceShips();
    }

    void CalculateScreenDimensions()
    {
        Camera mainCamera = Camera.main;
        screenHeight = 2f * mainCamera.orthographicSize;
        screenWidth = screenHeight * mainCamera.aspect;
    }

    void GetAllPlanets()
    {
        planets = new List<Planet>(FindObjectsOfType<Planet>());
    }

    void PlaceShips()
    {
        // Player 1 (left side)
        Vector3 player1Position = GetValidPosition(true);
        GameObject player1 = Instantiate(playerShipPrefab, player1Position, Quaternion.Euler(0, 0, 0));
        player1.name = "Player1Ship";

        // Player 2 (right side)
        Vector3 player2Position = GetValidPosition(false);
        GameObject player2 = Instantiate(playerShipPrefab, player2Position, Quaternion.Euler(0, 0, 180));
        player2.name = "Player2Ship";
    }

    Vector3 GetValidPosition(bool isLeftSide)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3 position = GetRandomPosition(isLeftSide);
            if (!OverlapsWithPlanet(position) && IsWithinValidVerticalRange(position.y))
            {
                return position;
            }
        }

        Debug.LogWarning("Could not find a valid position for the ship. Placing at default position.");
        return GetRandomPosition(isLeftSide);
    }

    Vector3 GetRandomPosition(bool isLeftSide)
    {
        float horizontalPosition = Random.Range(minDistanceFromCenter, maxDistanceFromCenter);
        if (isLeftSide) horizontalPosition = -horizontalPosition;

        float verticalPosition = Random.Range(-screenHeight / 2f + topBottomOffset, screenHeight / 2f - topBottomOffset);

        return new Vector3(horizontalPosition, verticalPosition, 0);
    }

    bool OverlapsWithPlanet(Vector3 position)
    {
        foreach (Planet planet in planets)
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
        return yPosition >= -screenHeight / 2f + topBottomOffset && 
               yPosition <= screenHeight / 2f - topBottomOffset;
    }
}
#endif