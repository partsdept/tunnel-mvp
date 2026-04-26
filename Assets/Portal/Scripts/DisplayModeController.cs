using UnityEngine;

/// <summary>
/// Swaps the materials on the four Q quads to switch between
/// display modes (solid color, test card, etc.). Bound to keys
/// for live mode-switching during development and verification.
///
/// Attach to a manager GameObject in the scene. Wire each Renderer
/// and material via the Inspector.
/// </summary>
public class DisplayModeController : MonoBehaviour
{
    [Header("Q Quad Renderers")]
    public Renderer q1Renderer;
    public Renderer q2Renderer;
    public Renderer q3Renderer;
    public Renderer q4Renderer;

    [Header("Solid Color Materials (mode S)")]
    public Material q1Solid;
    public Material q2Solid;
    public Material q3Solid;
    public Material q4Solid;

    [Header("Test Card Materials (mode T)")]
    public Material q1TestCard;
    public Material q2TestCard;
    public Material q3TestCard;
    public Material q4TestCard;

    void Start()
    {
        // Default to test cards on launch — change to SetSolid() if you
        // prefer solid colors as the default startup state.
        SetTestCards();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SetSolid();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            SetTestCards();
        }
    }

    void SetSolid()
    {
        q1Renderer.material = q1Solid;
        q2Renderer.material = q2Solid;
        q3Renderer.material = q3Solid;
        q4Renderer.material = q4Solid;
        Debug.Log("Display mode: solid colors");
    }

    void SetTestCards()
    {
        q1Renderer.material = q1TestCard;
        q2Renderer.material = q2TestCard;
        q3Renderer.material = q3TestCard;
        q4Renderer.material = q4TestCard;
        Debug.Log("Display mode: test cards");
    }
}