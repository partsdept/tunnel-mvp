using UnityEngine;

/// <summary>
/// Orbit / zoom / pan camera controller for the 3D arch preview.
/// Modeled loosely after PeasyCam: a target point in world space,
/// camera orbits around it on a sphere of variable radius.
///
/// Inputs:
///   Left mouse drag      orbit (yaw/pitch around target)
///   Right mouse drag     pan (translate target in camera space)
///   Scroll wheel         zoom (change radius)
///   R                    reset to defaults
///
/// Attach to the camera. The camera's transform is driven entirely
/// by this script in LateUpdate; manual transform changes will be
/// overwritten.
/// </summary>
public class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("World point the camera orbits around.")]
    public Vector3 target = Vector3.zero;

    [Header("Initial pose")]
    [Tooltip("Distance from target at startup.")]
    public float initialRadius = 8f;
    [Tooltip("Yaw in degrees at startup (around world Y).")]
    public float initialYaw = 0f;
    [Tooltip("Pitch in degrees at startup (above horizon).")]
    public float initialPitch = 15f;

    [Header("Limits")]
    [Tooltip("Minimum allowed distance from target.")]
    public float minRadius = 1f;
    [Tooltip("Maximum allowed distance from target.")]
    public float maxRadius = 100f;
    [Tooltip("Min/max pitch in degrees, to prevent flipping over the top.")]
    public Vector2 pitchClamp = new Vector2(-85f, 85f);

    [Header("Sensitivity")]
    public float orbitSpeed = 0.3f;
    public float panSpeed = 0.01f;
    public float zoomSpeed = 0.1f;

    private float radius;
    private float yaw;
    private float pitch;

    private Vector3 initialTarget;

    void Start()
    {
        initialTarget = target;
        ResetView();
    }

    void LateUpdate()
    {
        // Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetView();
        }

        // Orbit (left mouse drag)
        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            yaw += dx * orbitSpeed * 60f * Time.deltaTime * 100f;
            pitch -= dy * orbitSpeed * 60f * Time.deltaTime * 100f;
            pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);
        }

        // Pan (right mouse drag)
        if (Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            // Pan in camera-local axes: right = camera right vector; up = camera up vector
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            // Scale pan by current radius so it feels consistent at any zoom
            target -= right * dx * panSpeed * radius;
            target -= up * dy * panSpeed * radius;
        }

        // Zoom (scroll wheel)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            radius -= scroll * zoomSpeed * radius * 10f;
            radius = Mathf.Clamp(radius, minRadius, maxRadius);
        }

        // Apply yaw/pitch/radius to the transform
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0f, 0f, -radius);
        transform.position = target + offset;
        transform.LookAt(target);
    }

    public void ResetView()
    {
        target = initialTarget;
        radius = initialRadius;
        yaw = initialYaw;
        pitch = initialPitch;
    }
}
