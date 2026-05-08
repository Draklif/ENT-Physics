using System.Collections.Generic;
using UnityEngine;

public class ParticlePinner : MonoBehaviour
{
    [Header("Cámara")]
    public Camera cam;

    [Header("Selección")]
    [Min(0f)] public float pickRadiusPadding = 0.1f;

    [Header("Parámetros del pin (resorte rígido)")]
    [Min(0f)] public float stiffness = 500f;
    [Min(0f)] public float damping = 20f;

    // Mapa de partículas pinneadas → GameObject del AnchoredSpring asociado.
    private Dictionary<Particle, GameObject> activePins = new();

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnPinToggle += TogglePinUnderCursor;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnPinToggle -= TogglePinUnderCursor;
    }

    private void TogglePinUnderCursor()
    {
        Particle p = PickParticleAtPointer();
        if (p == null) return;

        if (activePins.TryGetValue(p, out GameObject existingPin))
        {
            // Ya está pinneada → desclavar.
            if (existingPin != null) Destroy(existingPin);
            activePins.Remove(p);
        }
        else
        {
            // No está pinneada → clavar.
            CreatePin(p);
        }
    }

    private void CreatePin(Particle p)
    {
        GameObject pinGO = new GameObject($"Pin_{p.name}");
        pinGO.transform.position = p.Position;

        AnchoredSpring spring = pinGO.AddComponent<AnchoredSpring>();
        spring.particle = p;
        spring.anchor = pinGO.transform;
        spring.stiffness = stiffness;
        spring.damping = damping;
        spring.restLength = 0f;

        activePins[p] = pinGO;
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
}