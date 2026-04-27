using UnityEngine;

/// <summary>
/// Slowly rotates the GameObject around configured axes. Used on the
/// Arch3D test content root to provide continuous motion, both as a
/// performance test signal (frame drops show as visible jitter) and
/// for visual interest.
/// </summary>
public class RotationDriver : MonoBehaviour
{
    [Tooltip("Degrees per second around each local axis.")]
    public Vector3 rotationSpeed = new Vector3(5f, 12f, 0f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}
