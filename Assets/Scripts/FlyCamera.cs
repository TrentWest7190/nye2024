using Cinemachine;
using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float acceleration = 50; // how fast you accelerate
    public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
    public float lookSensitivity = 1; // mouse look sensitivity
    public float lookSmoothing = 0.5f;
    public float lookDamping = 0.1f;
    public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input
    public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable
    public bool lockMovement = false;

    Vector3 velocity; // current velocity

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private CinemachineVirtualCamera _camera;

    public float zoomSmoothing = 1f;
    public float zoomSensitivity = 1f;
    private float targetFOV;
    private float currentFOV;

    public float dutchSensitivity = 1f;

    static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = value == false;
        }
    }

    private void Awake()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();

        targetFOV = _camera.m_Lens.FieldOfView;
        currentFOV = targetFOV;
    }

    void OnEnable()
    {
        if (focusOnEnable) Focused = true;
    }

    void OnDisable() => Focused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            lockMovement = !lockMovement;
        }
        // Input
        if (Focused && !Input.GetMouseButton(1))
            UpdateInput();
        else if (Input.GetMouseButtonDown(0))
            Focused = true;
        else if (Input.GetMouseButton(1))
            UpdateZoom();

        // Physics
        velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;

        // Smoothly interpolate the current FOV towards the target FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, zoomSmoothing * Time.deltaTime);

        // Apply the current FOV to the Cinemachine camera
        _camera.m_Lens.FieldOfView = currentFOV;
    }

    void UpdateZoom()
    {
        float mouseY = Input.GetAxis("Mouse Y") * zoomSensitivity;
        float mouseX = Input.GetAxis("Mouse X") * dutchSensitivity;

        // Update the target FOV based on mouse movement
        targetFOV -= mouseY; // Invert the movement to make it more intuitive

        _camera.m_Lens.Dutch -= mouseX;
    }

    void UpdateInput()
    {
        if (lockMovement) return;
        // Position
        velocity += GetAccelerationVector() * Time.deltaTime;

        // Rotation
        Vector2 targetMouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        // Smoothly interpolate the mouse delta using damping
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, lookDamping);
        Quaternion rotation = transform.rotation;
        Quaternion horiz = Quaternion.AngleAxis(currentMouseDelta.x, Vector3.up);
        Quaternion vert = Quaternion.AngleAxis(currentMouseDelta.y, Vector3.right);
        transform.rotation = Quaternion.Slerp(transform.rotation, horiz * rotation * vert, 1f / lookSmoothing);

        // Leave cursor lock
        if (Input.GetKeyDown(KeyCode.Escape))
            Focused = false;
    }

    Vector3 GetAccelerationVector()
    {
        Vector3 moveInput = default;

        void AddMovement(KeyCode key, Vector3 dir)
        {
            if (Input.GetKey(key))
                moveInput += dir;
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);

        // Apply global directions for up and down
        if (Input.GetKey(KeyCode.Space))
            moveInput += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl))
            moveInput += Vector3.down;

        // Convert local input (excluding up and down) to world space
        Vector3 localDirection = new Vector3(moveInput.x, 0, moveInput.z);
        Vector3 worldDirection = transform.TransformDirection(localDirection) + new Vector3(0, moveInput.y, 0);

        Debug.Log(worldDirection); // for debugging

        if (Input.GetKey(KeyCode.LeftShift))
            return worldDirection * (acceleration * accSprintMultiplier); // "sprinting"
        return worldDirection * acceleration; // "walking"
    }
}