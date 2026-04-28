using UnityEngine;

/// <summary>
/// Owns top-level display modes. Each mode is a complete description
/// of what's on screen.
///
/// Hotkeys:
///   C  Q-pipeline solid colors
///   Q  Q-pipeline test cards
///   S  Slideshow
///   A  ArchCanvas (with nested M/T/P toggles via ArchCanvasController)
///   3  Arch3D test scene
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

    [Header("Q-pipeline camera and quad renderers")]
    public Camera qCompositeCamera;
    public Renderer qComposite_Q1Renderer;
    public Renderer qComposite_Q2Renderer;
    public Renderer qComposite_Q3Renderer;
    public Renderer qComposite_Q4Renderer;

    [Header("Q-pipeline content: Solid Colors")]
    public Material q1Solid;
    public Material q2Solid;
    public Material q3Solid;
    public Material q4Solid;

    [Header("Q-pipeline content: Test Cards")]
    public Material q1TestCard;
    public Material q2TestCard;
    public Material q3TestCard;
    public Material q4TestCard;

    [Header("Slideshow camera")]
    public Camera slideshowCamera;

    [Header("ArchCanvas cameras")]
    public Camera archCamera;
    public Camera archCompositeCamera;

    [Header("Arch3D cameras")]
    public Camera arch3DCamera;
    public Camera arch3DCompositeCamera;

    public enum Mode
    {
        QPipeline_Solid,
        QPipeline_TestCards,
        Slideshow,
        Arch,
        Arch3D,
    }

    [SerializeField] private Mode startMode = Mode.QPipeline_TestCards;
    private Mode currentMode;

    public Mode GetCurrentMode() => currentMode;

    void Start()
    {
        SetMode(startMode);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetMode(Mode.QPipeline_Solid);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SetMode(Mode.QPipeline_TestCards);
        }
        else if (Input.GetKeyDown(KeyCode.S))
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

        SetCamerasEnabled(false, qCompositeCamera);
        SetCamerasEnabled(false, slideshowCamera);
        SetCamerasEnabled(false, archCamera, archCompositeCamera);
        SetCamerasEnabled(false, arch3DCamera, arch3DCompositeCamera);

        RenderTexture target = null;
        switch (mode)
        {
            case Mode.QPipeline_Solid:
                target = qPipelineRT;
                AssignMaterial(qComposite_Q1Renderer, q1Solid);
                AssignMaterial(qComposite_Q2Renderer, q2Solid);
                AssignMaterial(qComposite_Q3Renderer, q3Solid);
                AssignMaterial(qComposite_Q4Renderer, q4Solid);
                SetCamerasEnabled(true, qCompositeCamera);
                break;

            case Mode.QPipeline_TestCards:
                target = qPipelineRT;
                AssignMaterial(qComposite_Q1Renderer, q1TestCard);
                AssignMaterial(qComposite_Q2Renderer, q2TestCard);
                AssignMaterial(qComposite_Q3Renderer, q3TestCard);
                AssignMaterial(qComposite_Q4Renderer, q4TestCard);
                SetCamerasEnabled(true, qCompositeCamera);
                break;

            case Mode.Slideshow:
                target = slideshowRT;
                SetCamerasEnabled(true, slideshowCamera);
                break;

            case Mode.Arch:
                target = archRT;
                SetCamerasEnabled(true, archCamera, archCompositeCamera);
                break;

            case Mode.Arch3D:
                target = arch3DRT;
                SetCamerasEnabled(true, arch3DCamera, arch3DCompositeCamera);
                break;
        }

        if (target == null)
        {
            Debug.LogError($"OutputModeController: render texture for mode {mode} not assigned.");
            return;
        }

        outputMaterial.SetTexture("_BaseMap", target);
        currentMode = mode;
        Debug.Log($"Mode: {mode}");
    }

    private static void SetCamerasEnabled(bool enabled, params Camera[] cameras)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].enabled = enabled;
            }
        }
    }

    private static void AssignMaterial(Renderer r, Material m)
    {
        if (r == null || m == null) return;
        r.material = m;
    }
}
