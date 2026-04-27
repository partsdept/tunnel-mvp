using UnityEngine;

/// <summary>
/// Lightweight FPS counter that draws a small text overlay in the
/// top-left corner of the Game view. Uses a moving average over the
/// last N frames to smooth jitter.
///
/// Attach to any GameObject in the scene. The overlay renders via
/// OnGUI and stays visible regardless of which mode the output controller
/// is showing (it draws to screen space, after all rendering).
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    [Tooltip("Number of frames to average for FPS smoothing.")]
    public int sampleFrames = 60;

    [Tooltip("Position of the overlay in screen pixels from top-left.")]
    public Vector2 screenPosition = new Vector2(20f, 20f);

    [Tooltip("Font size in pixels.")]
    public int fontSize = 36;

    private float[] frameTimes;
    private int frameIndex = 0;
    private GUIStyle style;

    void Start()
    {
        frameTimes = new float[sampleFrames];
    }

    void Update()
    {
        frameTimes[frameIndex] = Time.unscaledDeltaTime;
        frameIndex = (frameIndex + 1) % frameTimes.Length;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = Color.white;
        }

        float total = 0f;
        for (int i = 0; i < frameTimes.Length; i++) total += frameTimes[i];
        float avgDt = total / frameTimes.Length;
        float fps = avgDt > 0f ? 1f / avgDt : 0f;

        string text = $"{fps:F1} fps  ({avgDt * 1000f:F1} ms)";
        // Background pill for readability
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(screenPosition.x - 8, screenPosition.y - 4, size.x + 16, size.y + 8), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(screenPosition.x, screenPosition.y, size.x, size.y), text, style);
    }
}
