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

    /// <summary>
    /// Calculates gravitational force magnitude.
    /// NOTE: Does NOT multiply by objectMass to allow mass-dependent trajectories.
    /// Formula: F = G * M / r² (mass-independent force)
    /// When used with rb.AddForce(), heavier objects accelerate less: a = F / mass
    /// </summary>
    public float CalculateGravitationalForce(Vector3 objectPosition, float objectMass, float distance)
    {
        // GAMEPLAY PHYSICS: Don't multiply by objectMass so that mass affects trajectory
        // Light missiles: Low mass → High acceleration → Curves more in gravity wells
        // Heavy missiles: High mass → Low acceleration → Flies straighter
        return gravitationalConstant * (mass / (distance * distance));
    }

    /// <summary>
    /// Calculates gravitational pull vector on an object.
    /// objectMass parameter kept for API compatibility but not used in calculation.
    /// </summary>
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

