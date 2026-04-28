using UnityEngine;

/// <summary>
/// Controls layer-toggles within ArchCanvas mode (hotkey A).
/// M, T, P, B, N only respond when ArchCanvas is the active output mode;
/// no-ops in other modes to avoid surprising the user.
///
/// Hotkeys (active only in ArchCanvas mode):
///   M  toggle markers (corner markers + reference lines)
///   T  toggle dynamic drifting text
///   P  toggle particle field
///   B  toggle backdrop visibility
///   N  cycle through backdrop images
/// </summary>
public class ArchCanvasController : MonoBehaviour
{
    [Header("Mode reference")]
    [Tooltip("Drag PortalControllers GameObject (or whichever has OutputModeController).")]
    public OutputModeController outputModeController;

    [Header("Layer GameObjects")]
    public GameObject markersObject;
    public GameObject dynamicTextObject;
    public GameObject particleObject;

    [Header("Backdrop")]
    [Tooltip("The backdrop quad GameObject (toggled by B).")]
    public GameObject backdropObject;
    [Tooltip("The backdrop's renderer (used to swap textures via N).")]
    public Renderer backdropRenderer;
    [Tooltip("Textures cycled through by N. First is shown at startup.")]
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMarkers();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleDynamicText();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleParticles();
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBackdrop();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            CycleBackdrop();
        }
    }

    void ToggleMarkers()
    {
        if (markersObject == null) { Debug.LogWarning("Markers object not assigned"); return; }
        bool nowActive = !markersObject.activeSelf;
        markersObject.SetActive(nowActive);
        Debug.Log("ArchCanvas markers: " + (nowActive ? "on" : "off"));
    }

    void ToggleDynamicText()
    {
        if (dynamicTextObject == null) { Debug.LogWarning("Dynamic text object not assigned"); return; }
        bool nowActive = !dynamicTextObject.activeSelf;
        dynamicTextObject.SetActive(nowActive);
        Debug.Log("ArchCanvas dynamic text: " + (nowActive ? "on" : "off"));
    }

    void ToggleParticles()
    {
        if (particleObject == null) { Debug.LogWarning("Particle object not assigned"); return; }
        bool nowActive = !particleObject.activeSelf;
        particleObject.SetActive(nowActive);
        Debug.Log("ArchCanvas particles: " + (nowActive ? "on" : "off"));
    }

    void ToggleBackdrop()
    {
        if (backdropObject == null) { Debug.LogWarning("Backdrop object not assigned"); return; }
        bool nowActive = !backdropObject.activeSelf;
        backdropObject.SetActive(nowActive);
        Debug.Log("ArchCanvas backdrop: " + (nowActive ? "on" : "off"));
    }

    void CycleBackdrop()
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

    void ApplyBackdropTexture()
    {
        if (backdropRenderer == null) return;
        if (backdropTextures == null || backdropTextures.Length == 0) return;
        if (backdropIndex < 0 || backdropIndex >= backdropTextures.Length) return;

        // Use the renderer's material instance (via .material, not .sharedMaterial)
        // so changes don't persist to the asset.
        backdropRenderer.material.SetTexture("_BaseMap", backdropTextures[backdropIndex]);
    }
}
