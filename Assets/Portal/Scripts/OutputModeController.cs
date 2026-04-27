using UnityEngine;

/// <summary>
/// Controls which render texture is shown on the output quad.
/// Modes are independent rendering pipelines (Q-pipeline composite,
/// slideshow, arch). Hotkeys cycle between them.
///
/// O = Q-pipeline composite (QPipeline_RT)
/// L = slideshow (Slideshow_RT)
/// A = arch (ArchPlate_RT)
/// </summary>
public class OutputModeController : MonoBehaviour
{
    [Header("Output target")]
    public Material outputMaterial;

    [Header("Mode render textures")]
    public RenderTexture qPipelineRT;
    public RenderTexture slideshowRT;
    public RenderTexture archRT;

    public enum Mode { QPipeline, Slideshow, Arch }

    [SerializeField] private Mode startMode = Mode.QPipeline;
    private Mode currentMode;

    void Start()
    {
        SetMode(startMode);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SetMode(Mode.QPipeline);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            SetMode(Mode.Slideshow);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetMode(Mode.Arch);
        }
    }

    public void SetMode(Mode mode)
    {
        if (outputMaterial == null)
        {
            Debug.LogError("OutputModeController: outputMaterial not assigned.");
            return;
        }

        RenderTexture target = null;
        switch (mode)
        {
            case Mode.QPipeline: target = qPipelineRT; break;
            case Mode.Slideshow: target = slideshowRT; break;
            case Mode.Arch:      target = archRT;      break;
        }

        if (target == null)
        {
            Debug.LogError($"OutputModeController: render texture for mode {mode} not assigned.");
            return;
        }

        outputMaterial.SetTexture("_BaseMap", target);
        currentMode = mode;
        Debug.Log($"Output mode: {mode}");
    }
}