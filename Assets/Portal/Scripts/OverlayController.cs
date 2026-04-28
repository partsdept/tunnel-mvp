using UnityEngine;

/// <summary>
/// Cross-mode overlays and view toggles.
///
/// Hotkeys (also callable via OSC dispatch):
///   H   heartbeat sweep
///   D   debug overlay text
///   TAB toggle between flat output and 3D arch preview
/// </summary>
public class OverlayController : MonoBehaviour
{
    [Header("Heartbeat sweep (toggle H)")]
    public GameObject sweepObject;

    [Header("Debug overlay text (toggle D)")]
    public GameObject debugOverlayObject;

    [Header("View toggle (TAB)")]
    public Camera outputCamera;
    public Camera preview3DCamera;

    void Start()
    {
        if (sweepObject != null) sweepObject.SetActive(false);
        if (debugOverlayObject != null) debugOverlayObject.SetActive(false);
        SetPreviewActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) ToggleSweepPublic();
        else if (Input.GetKeyDown(KeyCode.D)) ToggleDebugOverlayPublic();
        else if (Input.GetKeyDown(KeyCode.Tab)) TogglePreviewPublic();
    }

    public void ToggleSweepPublic()
    {
        if (sweepObject == null) { Debug.LogWarning("Sweep object not assigned"); return; }
        bool nowActive = !sweepObject.activeSelf;
        sweepObject.SetActive(nowActive);
        Debug.Log("Sweep: " + (nowActive ? "on" : "off"));
    }

    public void ToggleDebugOverlayPublic()
    {
        if (debugOverlayObject == null) { Debug.LogWarning("Debug overlay object not assigned"); return; }
        bool nowActive = !debugOverlayObject.activeSelf;
        debugOverlayObject.SetActive(nowActive);
        Debug.Log("Debug overlay: " + (nowActive ? "on" : "off"));
    }

    public void TogglePreviewPublic()
    {
        if (outputCamera == null || preview3DCamera == null)
        {
            Debug.LogWarning("Output and/or Preview3D camera not assigned");
            return;
        }
        bool previewIsActive = preview3DCamera.enabled;
        SetPreviewActive(!previewIsActive);
        Debug.Log("Preview3D: " + (!previewIsActive ? "on" : "off"));
    }

    private void SetPreviewActive(bool previewActive)
    {
        if (outputCamera != null) outputCamera.enabled = !previewActive;
        if (preview3DCamera != null) preview3DCamera.enabled = previewActive;
    }
}
