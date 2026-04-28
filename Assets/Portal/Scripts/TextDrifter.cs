using UnityEngine;
using TMPro;

/// <summary>
/// Drifts a TextMeshPro object horizontally across a screen-width region.
/// Wraps when the text has fully exited one edge, respawning so its
/// trailing edge appears at the opposite edge — i.e. the text scrolls
/// fully off the screen, then enters from the other side starting with
/// its last character.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextDrifter : MonoBehaviour
{
    [Tooltip("Drift speed in world units per second. Positive = left-to-right.")]
    public float driftSpeed = 1.0f;

    [Tooltip("Half-width of the visible canvas/screen area in world units. " +
             "ArchCanvas (Arch_Camera Size 5, 2.25:1) = 11.25. " +
             "Output_Camera Size 5, 16:9 = 8.89.")]
    public float screenHalfWidth = 11.25f;

    private TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    void Update()
    {
        tmp.ForceMeshUpdate();
        float textWidth = tmp.renderedWidth;
        float halfText = textWidth / 2f;

        Vector3 pos = transform.localPosition;
        pos.x += driftSpeed * Time.deltaTime;

        if (driftSpeed > 0f)
        {
            if (pos.x - halfText > screenHalfWidth)
            {
                pos.x = -screenHalfWidth - halfText;
            }
        }
        else if (driftSpeed < 0f)
        {
            if (pos.x + halfText < -screenHalfWidth)
            {
                pos.x = screenHalfWidth + halfText;
            }
        }

        transform.localPosition = pos;
    }
}