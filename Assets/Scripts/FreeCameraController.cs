using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("Movimiento")]
    [Min(0f)] public float moveSpeed = 6f;
    [Min(0f)] public float boostMultiplier = 2f;

    [Header("Mirar")]
    [Min(0f)] public float lookSensitivity = 0.15f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    private float yaw;
    private float pitch;
    private bool cursorCaptured;

    private void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y; pitch = e.x;

        SetCursorCaptured(false);

        if (InputManager.Instance != null)
            InputManager.Instance.OnToggleCursor += ToggleCursor;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnToggleCursor -= ToggleCursor;
    }

    private void Update()
    {
        var im = InputManager.Instance;
        if (im == null) return;

        if (cursorCaptured)
        {
            Vector2 look = im.LookDelta * lookSensitivity;
            yaw += look.x;
            pitch -= look.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        Vector2 move = im.MoveInput;
        float ascend = im.AscendInput;
        Vector3 dir = transform.right * move.x + transform.forward * move.y + Vector3.up * ascend;
        float speed = moveSpeed * boostMultiplier;
        transform.position += dir * speed * Time.deltaTime;
    }

    private void ToggleCursor() => SetCursorCaptured(!cursorCaptured);

    private void SetCursorCaptured(bool captured)
    {
        cursorCaptured = captured;
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
    }

    public bool IsCursorCaptured => cursorCaptured;
}