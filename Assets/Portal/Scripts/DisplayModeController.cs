using UnityEngine;

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

    [Header("Sweep Overlay (toggle V)")]
    public GameObject sweepObject;

    void Start()
    {
        SetTestCards();
        // Sweep starts hidden — uncomment to default visible:
        // if (sweepObject != null) sweepObject.SetActive(true);
        if (sweepObject != null) sweepObject.SetActive(false);
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
        else if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleSweep();
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

    void ToggleSweep()
    {
        if (sweepObject == null)
        {
            Debug.LogWarning("Sweep object not assigned");
            return;
        }
        bool nowActive = !sweepObject.activeSelf;
        sweepObject.SetActive(nowActive);
        Debug.Log("Sweep: " + (nowActive ? "on" : "off"));
    }
}