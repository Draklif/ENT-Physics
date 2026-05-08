using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerController controls;

    // Sistema
    public event Action OnPause;
    public event Action OnRestart;

    // Mouse - drag/throw
    public event Action OnGrabPressed;
    public event Action OnGrabReleased;
    public event Action OnThrowPressed;
    public event Action OnThrowReleased;

    // Cámara
    public event Action OnToggleCursor;

    // Edición de mundo
    public event Action OnPinToggle;
    public event Action OnLinkToggle;

    // Estados continuos
    public Vector2 PointerPosition { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookDelta { get; private set; }
    public float AscendInput { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new PlayerController();

        // Sistema
        controls.Player.Pause.performed += _ => OnPause?.Invoke();
        controls.Player.Restart.performed += _ => OnRestart?.Invoke();

        // Mouse
        controls.Player.Grab.started += _ => OnGrabPressed?.Invoke();
        controls.Player.Grab.canceled += _ => OnGrabReleased?.Invoke();
        controls.Player.Throw.started += _ => OnThrowPressed?.Invoke();
        controls.Player.Throw.canceled += _ => OnThrowReleased?.Invoke();

        controls.Player.PointerPosition.performed += ctx => PointerPosition = ctx.ReadValue<Vector2>();

        // Cámara
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => MoveInput = Vector2.zero;
        controls.Player.Look.performed += ctx => LookDelta = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += _ => LookDelta = Vector2.zero;
        controls.Player.Ascend.performed += ctx => AscendInput = ctx.ReadValue<float>();
        controls.Player.Ascend.canceled += _ => AscendInput = 0f;
        controls.Player.ToggleCursor.performed += _ => OnToggleCursor?.Invoke();

        // Edición
        controls.Player.PinToggle.performed += _ => OnPinToggle?.Invoke();
        controls.Player.LinkToggle.performed += _ => OnLinkToggle?.Invoke();
    }

    void OnEnable() { controls.Player.Enable(); }
    void OnDisable() { controls.Player.Disable(); }
}