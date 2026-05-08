using UnityEngine;

public class ParticleLinker : MonoBehaviour
{
    [Header("Cámara")]
    public Camera cam;

    [Header("Selección")]
    [Min(0f)] public float pickRadiusPadding = 0.1f;

    [Header("Parámetros del Spring")]
    [Min(0f)] public float stiffness = 80f;
    [Min(0f)] public float damping = 1f;
    [Tooltip("Si es 0 o negativo, se usa la distancia inicial entre A y B como restLength.")]
    public float restLength = 0f;

    [Header("Visualización")]
    public Color pendingColor = new Color(0.2f, 0.8f, 1f);

    // Partícula seleccionada como "A" esperando una "B" para formar el Spring.
    private Particle pending;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnLinkToggle += HandleLinkToggle;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnLinkToggle -= HandleLinkToggle;
    }

    private void HandleLinkToggle()
    {
        Particle target = PickParticleAtPointer();
        if (target == null) return;

        // Caso 1: no hay pendiente.
        if (pending == null)
        {
            // Si ya tiene Springs → interpretar como "limpiar enlaces".
            if (target.GetComponent<Spring>() != null)
            {
                RemoveAllSprings(target);
            }
            else
            {
                pending = target;
            }
            return;
        }

        // Caso 2: hay pendiente y se hizo click sobre la misma → cancelar.
        if (target == pending)
        {
            pending = null;
            return;
        }

        // Caso 3: hay pendiente y target es distinta → crear Spring.
        // Si target ya tiene Springs, los borramos primero según la regla acordada.
        if (target.GetComponent<Spring>() != null)
            RemoveAllSprings(target);

        CreateSpring(target, pending);
        pending = null;
    }

    private void CreateSpring(Particle owner, Particle other)
    {
        Spring s = owner.gameObject.AddComponent<Spring>();
        s.a = other;
        s.b = owner;
        s.stiffness = stiffness;
        s.damping = damping;

        // Si restLength <= 0, usamos la distancia actual.
        s.restLength = (restLength > 0f)
            ? restLength
            : Vector3.Distance(owner.Position, other.Position);
    }

    private void RemoveAllSprings(Particle p)
    {
        Spring[] springs = p.GetComponents<Spring>();
        foreach (Spring s in springs) Destroy(s);
    }

    private Particle PickParticleAtPointer()
    {
        Vector2 screenPos = InputManager.Instance.PointerPosition;

        Ray ray = cam.ScreenPointToRay(screenPos);
        Particle best = null;
        float bestDist = float.MaxValue;

        foreach (Particle p in ParticleWorld.All)
        {
            Vector3 toP = p.Position - ray.origin;
            float alongRay = Vector3.Dot(toP, ray.direction);
            if (alongRay < 0f) continue;

            Vector3 closestOnRay = ray.origin + ray.direction * alongRay;
            float perpDist = (p.Position - closestOnRay).magnitude;

            if (perpDist <= p.Radius + pickRadiusPadding && alongRay < bestDist)
            {
                best = p;
                bestDist = alongRay;
            }
        }
        return best;
    }

    private void OnDrawGizmos()
    {
        if (pending == null) return;
        Gizmos.color = pendingColor;
        Gizmos.DrawWireSphere(pending.Position, pending.Radius * 1.3f);
    }
}