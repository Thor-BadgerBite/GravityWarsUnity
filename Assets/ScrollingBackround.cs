using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public Transform cameraTransform; // The camera to follow
    public Vector2 parallaxFactor = new Vector2(0.5f, 0.5f); // Speed factor for the background movement
    public Vector2 textureScale = new Vector2(10f, 10f); // Scale for the background texture to repeat

    private Vector3 lastCameraPosition;
    private Renderer backgroundRenderer;

    void Start()
    {
        lastCameraPosition = cameraTransform.position;
        backgroundRenderer = GetComponent<Renderer>();

        // Scale the texture if needed
        backgroundRenderer.material.mainTextureScale = textureScale;
    }

    void Update()
    {
        Vector3 deltaCameraPosition = cameraTransform.position - lastCameraPosition;

        // Move the background slightly based on the camera's movement
        Vector2 offset = new Vector2(deltaCameraPosition.x * parallaxFactor.x, deltaCameraPosition.y * parallaxFactor.y);
        backgroundRenderer.material.mainTextureOffset += offset;

        lastCameraPosition = cameraTransform.position;
    }
}