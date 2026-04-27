using UnityEngine;

/// <summary>
/// Controls which render texture is shown on the output quad.
/// Modes are independent rendering pipelines; hotkeys cycle between them.
///
/// O = Q-pipeline composite (QPipeline_RT)
/// L = slideshow (Slideshow_RT)
/// A = arch 2D canvas (ArchPlate_RT)
/// 3 = arch 3D test scene (Arch3DPlate_RT)
/// </summary>
public class OutputModeController : MonoBehaviour
{
    [Header("Output target")]
    public Material outputMaterial;

    [Header("Mode render textures")]
    public RenderTexture qPipelineRT;
    public RenderTexture slideshowRT;
    public RenderTexture archRT;
    public RenderTexture arch3DRT;

    public enum Mode { QPipeline, Slideshow, Arch, Arch3D }

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
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetMode(Mode.Arch3D);
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
            case Mode.Arch3D:    target = arch3DRT;    break;
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
