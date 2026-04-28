using UnityEngine;

/// <summary>
/// Layer-toggles within ArchCanvas mode (hotkey A).
/// M, T, P, B, N only respond when ArchCanvas is the active output mode.
///
/// Hotkeys (also callable via OSC dispatch):
///   M  markers
///   T  dynamic text
///   P  particle field
///   B  backdrop on/off
///   N  cycle backdrop image
/// </summary>
public class ArchCanvasController : MonoBehaviour
{
    [Header("Mode reference")]
    public OutputModeController outputModeController;

    [Header("Layer GameObjects")]
    public GameObject markersObject;
    public GameObject dynamicTextObject;
    public GameObject particleObject;

    [Header("Backdrop")]
    public GameObject backdropObject;
    public Renderer backdropRenderer;
    public Texture[] backdropTextures;

    [Header("Initial state")]
    public bool markersVisibleAtStart = true;
    public bool dynamicTextVisibleAtStart = true;
    public bool particleVisibleAtStart = false;
    public bool backdropVisibleAtStart = true;

    private int backdropIndex = 0;

    void Start()
    {
        if (markersObject != null) markersObject.SetActive(markersVisibleAtStart);
        if (dynamicTextObject != null) dynamicTextObject.SetActive(dynamicTextVisibleAtStart);
        if (particleObject != null) particleObject.SetActive(particleVisibleAtStart);
        if (backdropObject != null) backdropObject.SetActive(backdropVisibleAtStart);
        ApplyBackdropTexture();
    }

    void Update()
    {
        if (outputModeController == null) return;
        if (outputModeController.GetCurrentMode() != OutputModeController.Mode.Arch) return;

        if (Input.GetKeyDown(KeyCode.M)) ToggleMarkersPublic();
        else if (Input.GetKeyDown(KeyCode.T)) ToggleDynamicTextPublic();
        else if (Input.GetKeyDown(KeyCode.P)) ToggleParticlesPublic();
        else if (Input.GetKeyDown(KeyCode.B)) ToggleBackdropPublic();
        else if (Input.GetKeyDown(KeyCode.N)) CycleBackdropPublic();
    }

    public void ToggleMarkersPublic()
    {
        if (markersObject == null) { Debug.LogWarning("Markers object not assigned"); return; }
        bool nowActive = !markersObject.activeSelf;
        markersObject.SetActive(nowActive);
        Debug.Log("ArchCanvas markers: " + (nowActive ? "on" : "off"));
    }

    public void ToggleDynamicTextPublic()
    {
        if (dynamicTextObject == null) { Debug.LogWarning("Dynamic text object not assigned"); return; }
        bool nowActive = !dynamicTextObject.activeSelf;
        dynamicTextObject.SetActive(nowActive);
        Debug.Log("ArchCanvas dynamic text: " + (nowActive ? "on" : "off"));
    }

    public void ToggleParticlesPublic()
    {
        if (particleObject == null) { Debug.LogWarning("Particle object not assigned"); return; }
        bool nowActive = !particleObject.activeSelf;
        particleObject.SetActive(nowActive);
        Debug.Log("ArchCanvas particles: " + (nowActive ? "on" : "off"));
    }

    public void ToggleBackdropPublic()
    {
        if (backdropObject == null) { Debug.LogWarning("Backdrop object not assigned"); return; }
        bool nowActive = !backdropObject.activeSelf;
        backdropObject.SetActive(nowActive);
        Debug.Log("ArchCanvas backdrop: " + (nowActive ? "on" : "off"));
    }

    public void CycleBackdropPublic()
    {
        if (backdropTextures == null || backdropTextures.Length == 0)
        {
            Debug.LogWarning("No backdrop textures assigned");
            return;
        }
        backdropIndex = (backdropIndex + 1) % backdropTextures.Length;
        ApplyBackdropTexture();
        Debug.Log($"ArchCanvas backdrop image: {backdropIndex + 1} of {backdropTextures.Length}");
    }

    private void ApplyBackdropTexture()
    {
        if (backdropRenderer == null) return;
        if (backdropTextures == null || backdropTextures.Length == 0) return;
        if (backdropIndex < 0 || backdropIndex >= backdropTextures.Length) return;
        backdropRenderer.material.SetTexture("_BaseMap", backdropTextures[backdropIndex]);
    }
}
