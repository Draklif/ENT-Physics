using UnityEngine;

public class ParticleDragger : MonoBehaviour, IForceGenerator
{
    [Header("Cámara")]
    public Camera cam;

    [Header("Selección")]
    [Min(0f)] public float pickRadiusPadding = 0.1f;

    [Header("Drag (resorte virtual)")]
    [Min(0f)] public float stiffnessPerMass = 80f;
    [Range(0f, 2f)] public float dampingRatio = 1f;
    [Min(0f)] public float maxForce = 500f;

    [Header("Throw (lanzamiento)")]
    [Tooltip("Impulso por unidad de distancia entre cursor y partícula al soltar.")]
    [Min(0f)] public float throwStrength = 8f;
    [Min(0f)] public float maxThrowImpulse = 40f;

    private enum Mode { None, Drag, Throw }
    private Mode mode = Mode.None;

    private Particle grabbed;
    private Vector3 cursorWorldPos;
    private float grabDepth;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void OnEnable()
    {
        ParticleWorld.Register((IForceGenerator)this);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnGrabPressed += BeginDrag;
            InputManager.Instance.OnGrabReleased += EndDrag;
            InputManager.Instance.OnThrowPressed += BeginThrow;
            InputManager.Instance.OnThrowReleased += EndThrow;
        }
    }

    private void OnDisable()
    {
        ParticleWorld.Unregister((IForceGenerator)this);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnGrabPressed -= BeginDrag;
            InputManager.Instance.OnGrabReleased -= EndDrag;
            InputManager.Instance.OnThrowPressed -= BeginThrow;
            InputManager.Instance.OnThrowReleased -= EndThrow;
        }
    }

    private void Update()
    {
        if (grabbed != null) UpdateCursorWorldPos();
    }

    // ---- Selección ----

    private Particle PickParticleUnderCursor()
    {
        Ray ray = cam.ScreenPointToRay(InputManager.Instance.PointerPosition);
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

    private void UpdateCursorWorldPos()
    {
        Ray ray = cam.ScreenPointToRay(InputManager.Instance.PointerPosition);
        float t = grabDepth / Vector3.Dot(ray.direction, cam.transform.forward);
        cursorWorldPos = ray.origin + ray.direction * t;
    }

    // ---- Drag ----

    private void BeginDrag()
    {
        if (mode != Mode.None) return;
        Particle p = PickParticleUnderCursor();
        if (p == null) return;

        grabbed = p;
        grabDepth = Vector3.Dot(p.Position - cam.transform.position, cam.transform.forward);
        cursorWorldPos = p.Position;
        mode = Mode.Drag;
    }

    private void EndDrag()
    {
        if (mode != Mode.Drag) return;
        grabbed = null;
        mode = Mode.None;
    }

    // ---- Throw ----

    private void BeginThrow()
    {
        if (mode != Mode.None) return;
        Particle p = PickParticleUnderCursor();
        if (p == null) return;

        grabbed = p;
        grabDepth = Vector3.Dot(p.Position - cam.transform.position, cam.transform.forward);
        cursorWorldPos = p.Position;
        mode = Mode.Throw;
    }

    private void EndThrow()
    {
        if (mode != Mode.Throw) return;

        // El impulso va del cursor hacia la partícula invertido:
        // si arrastraste el cursor a la izquierda de la partícula, ésta sale a la derecha.
        // Equivale a una resortera: la separación cursor-partícula es la "tensión".
        Vector3 pull = grabbed.Position - cursorWorldPos;
        Vector3 impulse = pull * throwStrength;
        if (impulse.magnitude > maxThrowImpulse)
            impulse = impulse.normalized * maxThrowImpulse;

        grabbed.AddImpulse(impulse);
        grabbed = null;
        mode = Mode.None;
    }

    // ---- Aplicación de fuerzas (solo para drag) ----

    public void ApplyForces(float dt)
    {
        if (mode != Mode.Drag || grabbed == null) return;

        float k = stiffnessPerMass * grabbed.Mass;
        float c = 2f * dampingRatio * Mathf.Sqrt(k * grabbed.Mass);

        Vector3 displacement = cursorWorldPos - grabbed.Position;
        Vector3 force = k * displacement - c * grabbed.Velocity;

        if (force.magnitude > maxForce)
            force = force.normalized * maxForce;

        grabbed.AddForce(force);
    }

    private void OnDrawGizmos()
    {
        if (grabbed == null) return;
        Gizmos.color = (mode == Mode.Drag) ? Color.yellow : new Color(1f, 0.4f, 0.2f);
        Gizmos.DrawLine(cursorWorldPos, grabbed.Position);
        Gizmos.DrawWireSphere(cursorWorldPos, 0.1f);
    }
}