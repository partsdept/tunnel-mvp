using UnityEngine;

/// <summary>
/// Cross-mode overlays and view toggles.
///
/// Hotkeys:
///   H   heartbeat sweep (output layer)
///   D   debug overlay text (output layer)
///   TAB toggle between flat 8K view and 3D arch preview
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
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleSweep();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ToggleDebugOverlay();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePreview();
        }
    }

    void ToggleSweep()
    {
        if (sweepObject == null) { Debug.LogWarning("Sweep object not assigned"); return; }
        bool nowActive = !sweepObject.activeSelf;
        sweepObject.SetActive(nowActive);
        Debug.Log("Sweep: " + (nowActive ? "on" : "off"));
    }

    void ToggleDebugOverlay()
    {
        if (debugOverlayObject == null) { Debug.LogWarning("Debug overlay object not assigned"); return; }
        bool nowActive = !debugOverlayObject.activeSelf;
        debugOverlayObject.SetActive(nowActive);
        Debug.Log("Debug overlay: " + (nowActive ? "on" : "off"));
    }

    void TogglePreview()
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
