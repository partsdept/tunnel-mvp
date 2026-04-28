using UnityEngine;
using extOSC;

/// <summary>
/// Listens for /portal/keystroke OSC messages and dispatches them to
/// the appropriate controller, as if the key had been pressed on the
/// keyboard. The keyboard handlers in each controller's Update() also
/// remain active — this just adds OSC as a parallel input source.
///
/// OSC messages have one string argument representing the key:
///   "C", "Q", "S", "A", "3"   (modes — note "3" is the digit 3)
///   "H", "D"                  (cross-mode overlays)
///   "M", "T", "P", "B", "N"   (ArchCanvas nested toggles)
///   "TAB"                     (preview view toggle)
///   "SPACE"                   (slideshow pause/resume)
///   "R"                       (preview camera reset)
///
/// Wiring: drop on a GameObject with an OSCReceiver component.
/// Reference all the controllers; this script delegates to their public
/// methods rather than invoking their private hotkey handlers.
/// </summary>
public class OscInputController : MonoBehaviour
{
    [Header("OSC")]
    public OSCReceiver oscReceiver;
    public string oscAddress = "/portal/keystroke";

    [Header("Target controllers")]
    public OutputModeController outputModeController;
    public OverlayController overlayController;
    public ArchCanvasController archCanvasController;
    public SlideshowController slideshowController;

    void Start()
    {
        if (oscReceiver == null)
        {
            Debug.LogError("OscInputController: oscReceiver not assigned.");
            return;
        }
        oscReceiver.Bind(oscAddress, OnKeystroke);
        Debug.Log($"OscInputController listening on {oscAddress}");
    }

    void OnKeystroke(OSCMessage message)
    {
        if (!message.ToString(out string keystroke))
        {
            Debug.LogWarning("OscInputController: keystroke message had no string argument");
            return;
        }

        Debug.Log($"OSC keystroke: {keystroke}");
        DispatchKey(keystroke);
    }

    /// <summary>
    /// Dispatches a key string to the appropriate controller's public
    /// method. Mirrors what the keyboard hotkeys do in each Update().
    /// </summary>
    private void DispatchKey(string key)
    {
        switch (key.ToUpperInvariant())
        {
            // Output modes
            case "C": outputModeController?.SetMode(OutputModeController.Mode.QPipeline_Solid); break;
            case "Q": outputModeController?.SetMode(OutputModeController.Mode.QPipeline_TestCards); break;
            case "S": outputModeController?.SetMode(OutputModeController.Mode.Slideshow); break;
            case "A": outputModeController?.SetMode(OutputModeController.Mode.Arch); break;
            case "3": outputModeController?.SetMode(OutputModeController.Mode.Arch3D); break;

            // Cross-mode overlays
            case "H": overlayController?.ToggleSweepPublic(); break;
            case "D": overlayController?.ToggleDebugOverlayPublic(); break;
            case "TAB": overlayController?.TogglePreviewPublic(); break;

            // ArchCanvas nested
            case "M": archCanvasController?.ToggleMarkersPublic(); break;
            case "T": archCanvasController?.ToggleDynamicTextPublic(); break;
            case "P": archCanvasController?.ToggleParticlesPublic(); break;
            case "B": archCanvasController?.ToggleBackdropPublic(); break;
            case "N":
                // N is overloaded: ArchCanvas cycles backdrop, Slideshow cycles playlist.
                // Dispatch based on current mode.
                DispatchN();
                break;

            // Slideshow nested
            case "SPACE": slideshowController?.TogglePausePublic(); break;

            default:
                Debug.LogWarning($"OscInputController: unknown keystroke '{key}'");
                break;
        }
    }

    private void DispatchN()
    {
        if (outputModeController == null) return;
        var mode = outputModeController.GetCurrentMode();
        if (mode == OutputModeController.Mode.Arch)
        {
            archCanvasController?.CycleBackdropPublic();
        }
        else if (mode == OutputModeController.Mode.Slideshow)
        {
            slideshowController?.CyclePlaylistPublic();
        }
    }
}
