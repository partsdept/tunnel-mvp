using UnityEngine;

/// <summary>
/// Controls overlays that operate independently of display mode.
/// Sweep (V), particle (K), text (X). All toggle GameObject active state.
/// </summary>
public class OverlayController : MonoBehaviour
{
    [Header("Sweep overlay (toggle V)")]
    public GameObject sweepObject;

    [Header("Particle / sparkle overlay (toggle K)")]
    public GameObject particleObject;

    [Header("Text overlay (toggle X)")]
    public GameObject textObject;

    void Start()
    {
        if (sweepObject != null) sweepObject.SetActive(false);
        if (particleObject != null) particleObject.SetActive(false);
        if (textObject != null) textObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleSweep();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleParticle();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleText();
        }
    }

    void ToggleSweep()
    {
        if (sweepObject == null) { Debug.LogWarning("Sweep object not assigned"); return; }
        bool nowActive = !sweepObject.activeSelf;
        sweepObject.SetActive(nowActive);
        Debug.Log("Sweep: " + (nowActive ? "on" : "off"));
    }

    void ToggleParticle()
    {
        if (particleObject == null) { Debug.LogWarning("Particle object not assigned"); return; }
        bool nowActive = !particleObject.activeSelf;
        particleObject.SetActive(nowActive);
        Debug.Log("Particle: " + (nowActive ? "on" : "off"));
    }

    void ToggleText()
    {
        if (textObject == null) { Debug.LogWarning("Text object not assigned"); return; }
        bool nowActive = !textObject.activeSelf;
        textObject.SetActive(nowActive);
        Debug.Log("Text: " + (nowActive ? "on" : "off"));
    }
}