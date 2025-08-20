using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    private float rotationSpeed;
    private Vector3 rotationAxis;

    void Start()
    {
        // Set random rotation speed between 1 and 10
        rotationSpeed = Random.Range(1f, 10f);

        // Set completely random rotation axis
        rotationAxis = Random.onUnitSphere;
    }

    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}