using UnityEngine;

public class AnchoredSpring : MonoBehaviour, IForceGenerator
{
    [Header("Conexión")]
    public Particle particle;
    public Transform anchor;

    [Header("Parámetros del resorte")]
    [Min(0f)] public float stiffness = 50f;
    [Min(0f)] public float restLength = 1f;
    [Min(0f)] public float damping = 0.5f;

    private void OnEnable() { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }

    public void ApplyForces(float dt)
    {
        if (particle == null || anchor == null) return;

        Vector3 delta = particle.Position - anchor.position;
        float length = delta.magnitude;
        if (length < Mathf.Epsilon) return;

        Vector3 direction = delta / length;

        // Ley de Hooke: F = -k(L - L₀) d̂
        float stretch = length - restLength;
        Vector3 elasticForce = -stiffness * stretch * direction;

        // Amortiguamiento sobre el eje del resorte
        float vAlongAxis = Vector3.Dot(particle.Velocity, direction);
        Vector3 dampingForce = -damping * vAlongAxis * direction;

        particle.AddForce(elasticForce + dampingForce);
    }

    private void OnDrawGizmos()
    {
        if (particle == null || anchor == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(anchor.position, particle.Position);
        Gizmos.DrawWireSphere(anchor.position, 0.1f);
    }
}