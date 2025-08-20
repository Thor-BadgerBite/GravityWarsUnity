using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform playfieldCenter;
    public float playfieldRadiusX = 16f;
    public float playfieldRadiusY = 9f;
    public float expandedPlayfieldRadiusX;
    public float expandedPlayfieldRadiusY;
    public float zoomOutFactor = 0.5f;
    public float zoomSpeed = 2f;
    public float panSpeed = 2f;
    public float maxZoomOut = 40f;
    public float cameraMarginFactor = 0.1f; // 10% extra margin



    public float shipZoomThreshold = 10f;
    public float shipZoomFactor = 0.7f;
    public float shipZoomSpeed = 3f;
    public float shipZoomOutSpeed = 5f;

    public float initialZoomDisableDistance = 5f;

    [Header("Slow Motion Settings")]
    public float slowMotionTimeScale = 0.5f;
    public float slowMotionDuration = 1f;
    public float slowMotionTransitionTime = 0.3f;

    private Camera mainCamera;
    private Missile3D currentMissile;
    private Vector3 originalCameraPosition;
    private float originalCameraSize;
    private bool isFollowingMissile = false;
    private bool shipDestroyed = false; // renamed for clarity
    private Vector3 explosionImpactPosition; // stores the collision point
    private float normalTimeScale;
    private Coroutine slowMotionCoroutine;

    void Start()
    {
        mainCamera = Camera.main;
        originalCameraPosition = mainCamera.transform.position;
        originalCameraSize = mainCamera.orthographicSize;
        normalTimeScale = Time.timeScale;
        // Initially, expanded values are equal to the defaults.
        expandedPlayfieldRadiusX = playfieldRadiusX;
        expandedPlayfieldRadiusY = playfieldRadiusY;

    }

    void Update()
    {
        if (isFollowingMissile && (currentMissile != null || shipDestroyed))
        {
            FollowMissile();
        }
        else
        {
            FocusAllShips();
        }
    }

    public void StartFollowingMissile3D(Missile3D missile)
    {
        currentMissile = missile;
        isFollowingMissile = true;
        Debug.Log("Started following missile");
    }

    /// <summary>
    /// Stops following the missile. If delay is true, waits for the destruction delay
    /// and uses the provided impact point as the focus for zoom.
    /// </summary>
    private void StopFollowingMissileCallback()
{
    currentMissile = null;
    isFollowingMissile = false;
    shipDestroyed = false;
    // Reset slow motion if needed…
    Debug.Log("Stopped following missile via callback");
}

    public void StopFollowingMissileWithInvoke(bool delay, Vector3 impactPoint)
    {
        if (delay)
        {
            shipDestroyed = true;
            explosionImpactPosition = impactPoint;
            Debug.Log($"[CameraController] (Invoke) Holding zoom on explosion at {explosionImpactPosition} for {GameManager.Instance.destructionDelay} seconds.");
            Invoke(nameof(StopFollowingMissileCallback), GameManager.Instance.destructionDelay);
        }
        else
        {
            StopFollowingMissileCallback();
        }
    }


    private void FollowMissile()
    {
        // Use the impact point if the ship was destroyed; otherwise, use the missile's current position.
        Vector3 targetPoint = shipDestroyed ? explosionImpactPosition : currentMissile.transform.position;
        Vector3 missileStartPosition = (currentMissile != null) ? currentMissile.GetStartPosition() : explosionImpactPosition;

        // Use the expanded playfield values (which were updated in FocusAllShips)
        float distanceFromPlayfieldX = Mathf.Abs(targetPoint.x - playfieldCenter.position.x) - expandedPlayfieldRadiusX;
        float distanceFromPlayfieldY = Mathf.Abs(targetPoint.y - playfieldCenter.position.y) - expandedPlayfieldRadiusY;

        bool isOutsideX = distanceFromPlayfieldX > 0;
        bool isOutsideY = distanceFromPlayfieldY > 0;

        bool isCloseToShip = false;
        float closestShipDistance = float.MaxValue;

        PlayerShip[] activeShips = FindObjectsOfType<PlayerShip>();
        foreach (PlayerShip ship in activeShips)
        {
            if (ship != null && ship.gameObject.activeSelf)
            {
                float distanceToShip = Vector3.Distance(targetPoint, ship.transform.position);
                if (distanceToShip < shipZoomThreshold && Vector3.Distance(targetPoint, missileStartPosition) > initialZoomDisableDistance)
                {
                    isCloseToShip = true;
                    closestShipDistance = Mathf.Min(closestShipDistance, distanceToShip);

                    if (slowMotionCoroutine == null)
                    {
                        slowMotionCoroutine = StartCoroutine(SlowMotionEffect());
                    }
                }
            }
        }

        // Instead of using originalCameraSize as the base, use the current zoom.
        float baseZoom = mainCamera.orthographicSize;
        float targetZoom = baseZoom;
        Vector3 targetPosition = originalCameraPosition;

        if (isOutsideX || isOutsideY)
        {
            targetPosition = new Vector3(targetPoint.x, targetPoint.y, originalCameraPosition.z);
            if (isOutsideX)
                targetZoom += (distanceFromPlayfieldX * zoomOutFactor);
            if (isOutsideY)
                targetZoom += (distanceFromPlayfieldY * zoomOutFactor);
            targetZoom = Mathf.Clamp(targetZoom, baseZoom, maxZoomOut);
        }
        else if (isCloseToShip || shipDestroyed)
        {
            float shipZoomAmount = 1 - (closestShipDistance / shipZoomThreshold);
            // Lerp between baseZoom and a zoomed-in value (baseZoom * shipZoomFactor)
            targetZoom = Mathf.Lerp(baseZoom, baseZoom * shipZoomFactor, shipZoomAmount);
            targetPosition = new Vector3(targetPoint.x, targetPoint.y, originalCameraPosition.z);
            // Debug.Log(...);
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, panSpeed * Time.unscaledDeltaTime);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, (isCloseToShip ? shipZoomSpeed : zoomSpeed) * Time.unscaledDeltaTime);
    }


   private void FocusAllShips()
    {
        // Keep the camera's position pinned to originalCameraPosition
        // so there's no panning around.
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            originalCameraPosition,
            panSpeed * Time.unscaledDeltaTime
        );

        // Find how far any ship is beyond the standard “playfieldRadiusX or playfieldRadiusY”.
        // Then we decide if we must zoom out.
        // If all ships are well inside, we try to zoom in (but never below originalCameraSize).

        PlayerShip[] ships = FindObjectsOfType<PlayerShip>();
        if (ships == null || ships.Length == 0)
        {
            // No ships? Just smoothly move camera size back to original.
            float newSize = Mathf.Lerp(mainCamera.orthographicSize, originalCameraSize, zoomSpeed * Time.unscaledDeltaTime);
            mainCamera.orthographicSize = newSize;
            return;
        }

        // 1) For each ship, see how far it is from the playfield center in X and Y.
        //    If ship is outside X or Y boundary, we compute how much extra size we need.

        float neededZoom = originalCameraSize;  // Start from your default
        Vector2 centerPos = new Vector2(playfieldCenter.position.x, playfieldCenter.position.y);

        foreach (var ship in ships)
        {
            if (!ship.gameObject.activeSelf) 
                continue;

            Vector2 shipPos = new Vector2(ship.transform.position.x, ship.transform.position.y);
            Vector2 distFromCenter = shipPos - centerPos;

            // how far beyond boundary in X?
            float overX = Mathf.Abs(distFromCenter.x) - playfieldRadiusX;
            // how far beyond boundary in Y?
            float overY = Mathf.Abs(distFromCenter.y) - playfieldRadiusY;

            // If overX > 0 or overY > 0, the ship is outside the official boundary.

            // We want to figure out how much we must increase orthographicSize 
            // so that the boundary is effectively bigger.
            // The default boundary is (playfieldRadiusY) at the camera’s current size = originalCameraSize.
            // 
            // e.g. if the camera’s size must be scaled up proportionally 
            // so that the ship is within the new boundary in X or Y dimension.

            if (overX > 0)
            {
                // We have to scale the camera so that halfWidth in world units 
                // at the camera is big enough to contain the new ship X distance from center.
                // The camera half-width = mainCamera.orthographicSize * aspect.
                // We want: shipDistX <= newCameraSize * aspect.  
                // So newCameraSize >= shipDistX / aspect. 

                float requiredHalfWidth = Mathf.Abs(distFromCenter.x) * (1 + cameraMarginFactor);
                float requiredSizeX = requiredHalfWidth / mainCamera.aspect;
                neededZoom = Mathf.Max(neededZoom, requiredSizeX);
            }

            if (overY > 0)
            {
                // Similarly for Y dimension, newCameraSize >= shipDistY
                float requiredHalfHeight = Mathf.Abs(distFromCenter.y) * (1 + cameraMarginFactor);
                float requiredSizeY = requiredHalfHeight;
                neededZoom = Mathf.Max(neededZoom, requiredSizeY);
            }
        }

        // 2) If no ship is outside the boundary, neededZoom remains originalCameraSize 
        //    (or what we found for the largest ship).
        // But if some ship was outside, neededZoom might become bigger than originalCameraSize.

        // 3) We clamp between originalCameraSize and maxZoomOut
        neededZoom = Mathf.Clamp(neededZoom, originalCameraSize, maxZoomOut);

        // 4) Smoothly LERP the current camera size to that new size
        float newSizeSmooth = Mathf.Lerp(
            mainCamera.orthographicSize, 
            neededZoom, 
            zoomSpeed * Time.unscaledDeltaTime
        );
        mainCamera.orthographicSize = newSizeSmooth;
        // Update the expanded playfield radii based on the new camera size.
        // The camera's half-height (in world units) is newSizeSmooth and half-width is newSizeSmooth * aspect.
        expandedPlayfieldRadiusY = newSizeSmooth;
        expandedPlayfieldRadiusX = newSizeSmooth * mainCamera.aspect;
        //Debug.Log($"[CameraController] Zooming out to {newSizeSmooth} (expandedPlayfieldRadiusX={expandedPlayfieldRadiusX}, expandedPlayfieldRadiusY={expandedPlayfieldRadiusY})");

    }



    private IEnumerator SlowMotionEffect()
    {
        float elapsedTime = 0f;
        while (elapsedTime < slowMotionTransitionTime)
        {
            Time.timeScale = Mathf.Lerp(normalTimeScale, slowMotionTimeScale, elapsedTime / slowMotionTransitionTime);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        elapsedTime = 0f;
        while (elapsedTime < slowMotionTransitionTime)
        {
            Time.timeScale = Mathf.Lerp(slowMotionTimeScale, normalTimeScale, elapsedTime / slowMotionTransitionTime);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
        slowMotionCoroutine = null;
    }
}
