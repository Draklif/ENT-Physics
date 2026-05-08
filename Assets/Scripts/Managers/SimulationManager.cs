using System;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [Header("Simulation Settings")]
    [Tooltip("Tiempo de cada paso de simulación (Δt) en segundos.")]
    [Min(0.0001f)]
    public float updateTime = 0.02f;

    [Tooltip("Escala del tiempo de simulación.")]
    [Range(0f, 10f)]
    public float timeScale = 1f;

    [Tooltip("Indica si la simulación está en pausa.")]
    public bool isPaused = false;

    [Header("Simulation State")]
    [SerializeField] private float timer = 0f;

    public float SimulationTime { get; private set; } = 0f;
    public int StepCount { get; private set; } = 0;

    // Eventos
    public event Action<float> OnSimulationStep;
    public event Action OnSimulationReset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime * timeScale;
        while (timer >= updateTime)
        {
            Step();
            timer -= updateTime;
        }
    }

    private void Start()
    {
        InputManager.Instance.OnPause += TogglePause;
        InputManager.Instance.OnRestart += ResetSimulation;
    }

    private void Step()
    {
        SimulationTime += updateTime;
        StepCount++;
        OnSimulationStep?.Invoke(updateTime);
    }

    #region Public Controls
    public void Play() => isPaused = false;

    public void Pause() => isPaused = true;

    public void TogglePause() => isPaused = !isPaused;

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale);
    }

    public void SetUpdateTime(float dt)
    {
        updateTime = Mathf.Max(0.0001f, dt);
    }

    public void ResetSimulation()
    {
        timer = 0f;
        SimulationTime = 0f;
        StepCount = 0;
        OnSimulationReset?.Invoke();
    }
    #endregion
}
