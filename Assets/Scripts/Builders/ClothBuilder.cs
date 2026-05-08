using UnityEngine;

public class ClothBuilder : MonoBehaviour
{
    [Header("Topología")]
    [Min(2)] public int width = 8;
    [Min(2)] public int height = 8;
    [Min(0.01f)] public float spacing = 0.3f;

    [Header("Orientación")]
    public Vector3 rightAxis = Vector3.right;
    public Vector3 downAxis = Vector3.forward;  // tela horizontal por defecto

    [Header("Anclaje de esquinas")]
    public bool pinTopLeft = true;
    public bool pinTopRight = true;
    public bool pinBottomLeft = false;
    public bool pinBottomRight = false;

    [Header("Prefab")]
    public Particle particlePrefab;

    [Header("Resortes estructurales (vecinos)")]
    [Min(0f)] public float structuralStiffness = 120f;
    [Min(0f)] public float structuralDamping = 1f;

    [Header("Resortes de cizalla (diagonales)")]
    [Min(0f)] public float shearStiffness = 60f;
    [Min(0f)] public float shearDamping = 0.5f;

    private Particle[,] grid;

    private void Start()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("ClothBuilder: falta prefab.");
            return;
        }

        Vector3 right = rightAxis.normalized * spacing;
        Vector3 down = downAxis.normalized * spacing;

        // Crear la grilla de partículas
        grid = new Particle[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = transform.position + right * x + down * y;
                Particle p = Instantiate(particlePrefab, pos, Quaternion.identity, transform);
                p.name = $"Cloth_{x}_{y}";
                p.initialPosition = pos;
                grid[x, y] = p;
            }
        }

        // Resortes estructurales: cada nodo se conecta con su vecino derecho y abajo.
        // (No al izquierdo/arriba para evitar duplicados — la conexión es simétrica.)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x + 1 < width) CreateSpring(grid[x, y], grid[x + 1, y], spacing,
                                                 structuralStiffness, structuralDamping);
                if (y + 1 < height) CreateSpring(grid[x, y], grid[x, y + 1], spacing,
                                                 structuralStiffness, structuralDamping);
            }
        }

        // Resortes de cizalla: diagonales (\\ y /).
        float diag = spacing * Mathf.Sqrt(2f);
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                CreateSpring(grid[x, y], grid[x + 1, y + 1], diag,
                             shearStiffness, shearDamping);
                CreateSpring(grid[x + 1, y], grid[x, y + 1], diag,
                             shearStiffness, shearDamping);
            }
        }

        // Anclar esquinas pedidas
        if (pinTopLeft) PinCorner(grid[0, 0]);
        if (pinTopRight) PinCorner(grid[width - 1, 0]);
        if (pinBottomLeft) PinCorner(grid[0, height - 1]);
        if (pinBottomRight) PinCorner(grid[width - 1, height - 1]);
    }

    private void CreateSpring(Particle a, Particle b, float restLength, float k, float c)
    {
        var go = new GameObject($"Spring_{a.name}_{b.name}");
        go.transform.SetParent(transform);
        var s = go.AddComponent<Spring>();
        s.a = a; s.b = b;
        s.stiffness = k;
        s.damping = c;
        s.restLength = restLength;
    }

    private void PinCorner(Particle p)
    {
        // Creamos un transform que se queda en la posición inicial.
        var anchor = new GameObject($"Pin_{p.name}").transform;
        anchor.SetParent(transform);
        anchor.position = p.initialPosition;

        var go = new GameObject($"AnchorSpring_{p.name}");
        go.transform.SetParent(transform);
        var s = go.AddComponent<AnchoredSpring>();
        s.particle = p;
        s.anchor = anchor;
        s.stiffness = structuralStiffness * 2f;  // pin más rígido para que no flote
        s.damping = structuralDamping;
        s.restLength = 0f;
    }
}