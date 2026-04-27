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

    [Tooltip("Half-width of the visible screen area in world units. " +
             "Output_Camera at Size 5 with 16:9 aspect = 8.89.")]
    public float screenHalfWidth = 8.89f;

    private TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // Force layout update so renderedWidth is current
        tmp.ForceMeshUpdate();
        float textWidth = tmp.renderedWidth;
        float halfText = textWidth / 2f;

        Vector3 pos = transform.localPosition;
        pos.x += driftSpeed * Time.deltaTime;

        if (driftSpeed > 0f)
        {
            // Moving right. Wrap when text's left edge crosses past the
            // right screen edge (text fully gone), respawn with right
            // edge at the left screen edge (one char's worth visible).
            if (pos.x - halfText > screenHalfWidth)
            {
                pos.x = -screenHalfWidth - halfText;
            }
        }
        else if (driftSpeed < 0f)
        {
            // Moving left. Wrap when text's right edge crosses past the
            // left screen edge, respawn with left edge at right screen edge.
            if (pos.x + halfText < -screenHalfWidth)
            {
                pos.x = screenHalfWidth + halfText;
            }
        }

        transform.localPosition = pos;
    }
}