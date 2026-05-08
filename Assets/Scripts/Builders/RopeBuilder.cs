using UnityEngine;

public class RopeBuilder : MonoBehaviour
{
    [Header("Topología")]
    [Min(2)] public int particleCount = 10;
    [Min(0.01f)] public float segmentLength = 0.4f;
    public Vector3 direction = Vector3.down;

    [Header("Anclaje")]
    public bool pinFirst = true;
    public bool pinLast = false;
    public Transform endAnchor;

    [Header("Prefab")]
    public Particle particlePrefab;

    [Header("Parámetros del resorte")]
    [Min(0f)] public float stiffness = 80f;
    [Min(0f)] public float damping = 1f;

    private void Start()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("RopeBuilder: falta prefab.");
            return;
        }

        Vector3 step = direction.normalized * segmentLength;

        // Crear partículas
        Particle[] ps = new Particle[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 pos = transform.position + step * i;
            Particle p = Instantiate(particlePrefab, pos, Quaternion.identity, transform);
            p.name = $"RopeNode_{i}";
            p.initialPosition = pos;
            ps[i] = p;
        }

        // Conectar consecutivos con Spring
        for (int i = 0; i < particleCount - 1; i++)
            CreateSpring(ps[i], ps[i + 1]);

        // Anclas
        if (pinFirst)
            CreateAnchoredSpring(ps[0], transform);
        if (pinLast && endAnchor != null)
            CreateAnchoredSpring(ps[particleCount - 1], endAnchor);
    }

    private void CreateSpring(Particle a, Particle b)
    {
        var go = new GameObject($"Spring_{a.name}_{b.name}");
        go.transform.SetParent(transform);
        var s = go.AddComponent<Spring>();
        s.a = a; s.b = b;
        s.stiffness = stiffness;
        s.damping = damping;
        s.restLength = segmentLength;
    }

    private void CreateAnchoredSpring(Particle p, Transform anchor)
    {
        var go = new GameObject($"AnchorSpring_{p.name}");
        go.transform.SetParent(transform);
        var s = go.AddComponent<AnchoredSpring>();
        s.particle = p;
        s.anchor = anchor;
        s.stiffness = stiffness;
        s.damping = damping;
        s.restLength = 0f;
    }
}