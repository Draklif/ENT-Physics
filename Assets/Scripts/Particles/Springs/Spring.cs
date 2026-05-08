using UnityEngine;

public class Spring : MonoBehaviour, IForceGenerator
{
    [Header("Conexión")]
    public Particle a;
    public Particle b;

    [Header("Parámetros del resorte")]
    [Min(0f)] public float stiffness = 50f;
    [Min(0f)] public float restLength = 1f;
    [Min(0f)] public float damping = 0.5f;

    private void OnEnable() { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }

    public void ApplyForces(float dt)
    {
        if (a == null || b == null) return;

        Vector3 delta = b.Position - a.Position;
        float length = delta.magnitude;
        if (length < Mathf.Epsilon) return;

        Vector3 direction = delta / length;

        // Ley de Hooke
        float stretch = length - restLength;
        Vector3 elasticForce = -stiffness * stretch * direction;

        // Amortiguamiento por velocidad relativa sobre el eje
        Vector3 relVel = b.Velocity - a.Velocity;
        float vRelAlongAxis = Vector3.Dot(relVel, direction);
        Vector3 dampingForce = -damping * vRelAlongAxis * direction;

        Vector3 totalForce = elasticForce + dampingForce;

        // Tercera ley de Newton
        b.AddForce(totalForce);
        a.AddForce(-totalForce);
    }

    private void OnDrawGizmos()
    {
        if (a == null || b == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a.Position, b.Position);
    }
}