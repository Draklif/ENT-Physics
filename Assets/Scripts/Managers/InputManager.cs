using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerController controls;

    // Buttons
    public event Action OnPause;
    public event Action OnRestart;

    // Mouse
    public event Action OnGrabPressed;
    public event Action OnGrabReleased;
    public event Action OnThrowPressed;
    public event Action OnThrowReleased;

    public Vector2 PointerPosition { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new PlayerController();

        controls.Player.Pause.performed += _ => OnPause?.Invoke();
        controls.Player.Restart.performed += _ => OnRestart?.Invoke();

        controls.Player.Grab.started += _ => OnGrabPressed?.Invoke();
        controls.Player.Grab.canceled += _ => OnGrabReleased?.Invoke();
        controls.Player.Throw.started += _ => OnThrowPressed?.Invoke();
        controls.Player.Throw.canceled += _ => OnThrowReleased?.Invoke();

        controls.Player.PointerPosition.performed += ctx => PointerPosition = ctx.ReadValue<Vector2>();
    }

    void OnEnable() { controls.Player.Enable(); }
    void OnDisable() { controls.Player.Disable(); }
}