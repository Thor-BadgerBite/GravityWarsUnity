using UnityEngine;
public class Planet : MonoBehaviour
{
    public string planetName;
    public float mass;
    public static float gravitationalConstant = 0.5f;

    private SphereCollider sphereCollider;
    private CircleCollider2D circleCollider2D;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        UpdateColliderSize();
    }

    void UpdateColliderSize()
    {
        if (sphereCollider != null && meshRenderer != null)
        {
            sphereCollider.radius = meshRenderer.bounds.extents.y / transform.localScale.y;
        }
    }

    public float CalculateGravitationalForce(Vector3 objectPosition, float objectMass, float distance)
    {
        return gravitationalConstant * ((mass * objectMass) / (distance * distance));
    }

    public Vector3 CalculateGravitationalPull(Vector3 objectPosition, float objectMass)
    {
        Vector3 direction = transform.position - objectPosition;
        float distance = direction.magnitude;

        if (distance == 0) return Vector3.zero; // Avoid division by zero

        float forceMagnitude = CalculateGravitationalForce(objectPosition, objectMass, distance);
        Vector3 pull = direction.normalized * forceMagnitude;
        return pull;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
    }

    public void SetPlanetProperties(string name, float planetMass)
    {
        planetName = name;
        mass = planetMass;
        UpdateColliderSize();
    }
}

