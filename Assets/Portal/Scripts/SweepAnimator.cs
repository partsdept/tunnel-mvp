using UnityEngine;

/// <summary>
/// Animates a horizontal sweep across the composite output.
/// Acts as a heartbeat indicator: visible motion proves the
/// render loop is alive even when content is static.
///
/// Visibility is controlled externally (DisplayModeController
/// toggles the GameObject active state). When active, the
/// quad's localPosition.x oscillates between minX and maxX.
/// </summary>
public class SweepAnimator : MonoBehaviour
{
    [Tooltip("Leftmost X position in local space.")]
    public float minX = -8.89f;

    [Tooltip("Rightmost X position in local space.")]
    public float maxX = 8.89f;

    [Tooltip("Seconds for one full left-to-right pass.")]
    public float periodSeconds = 4f;

    void Update()
    {
        float phase = (Time.time % periodSeconds) / periodSeconds;
        float x = Mathf.Lerp(minX, maxX, phase);

        Vector3 pos = transform.localPosition;
        pos.x = x;
        pos.z = -0.1f; // keep in front of Q quads
        transform.localPosition = pos;
    }
}