using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Per-cell slideshow logic. Each Slideshow cell uses two stacked quads
/// (back and front). The back shows the current image at full opacity;
/// the front fades in the next image. When the fade completes, the
/// roles swap and a new fade begins after the next timer interval.
///
/// The cell's playlist is set externally by SlideshowController. Each
/// cell holds its own randomized order through the source pool, and
/// each interval/initial-offset is randomized independently to stagger
/// the visual rhythm.
/// </summary>
public class SlideshowCell : MonoBehaviour
{
    [Header("Quads")]
    [Tooltip("Back quad: shows the current (settled) image.")]
    public Renderer backQuad;
    [Tooltip("Front quad: fades in the next image, then swaps to back.")]
    public Renderer frontQuad;

    [Header("Timing")]
    [Tooltip("Range of seconds between fade starts (per-cell, randomized each cycle).")]
    public Vector2 intervalRange = new Vector2(3f, 7f);

    [Tooltip("Crossfade duration in seconds.")]
    public float fadeDuration = 0.5f;

    private List<Texture> playlist;
    private int playlistIndex;
    private float timer;
    private bool isFading;
    private float fadeProgress;
    private Texture currentTexture;
    private Texture nextTexture;
    private MaterialPropertyBlock backMpb;
    private MaterialPropertyBlock frontMpb;

    public bool Paused { get; set; } = true;

    void Awake()
    {
        backMpb = new MaterialPropertyBlock();
        frontMpb = new MaterialPropertyBlock();
        playlist = new List<Texture>();
    }

    /// <summary>
    /// Replaces this cell's playlist with a randomized order through
    /// the provided source. Resets timer and shows the first image.
    /// </summary>
    public void SetPlaylist(IList<Texture> source)
    {
        playlist.Clear();
        playlist.AddRange(source);
        // Fisher-Yates shuffle
        for (int i = playlist.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (playlist[i], playlist[j]) = (playlist[j], playlist[i]);
        }

        if (playlist.Count == 0) return;

        playlistIndex = 0;
        currentTexture = playlist[0];
        nextTexture = playlist[Mathf.Min(1, playlist.Count - 1)];

        SetBackTexture(currentTexture);
        SetFrontTexture(nextTexture);
        SetFrontAlpha(0f);

        // Random initial offset so cells don't tick in lockstep
        timer = Random.Range(0f, intervalRange.y);
        isFading = false;
        fadeProgress = 0f;
    }

    void Update()
    {
        if (Paused) return;
        if (playlist == null || playlist.Count < 2) return;

        if (isFading)
        {
            fadeProgress += Time.deltaTime / fadeDuration;
            if (fadeProgress >= 1f)
            {
                // Fade complete: front becomes the new back
                currentTexture = nextTexture;
                playlistIndex = (playlistIndex + 1) % playlist.Count;
                nextTexture = playlist[(playlistIndex + 1) % playlist.Count];

                SetBackTexture(currentTexture);
                SetFrontTexture(nextTexture);
                SetFrontAlpha(0f);

                isFading = false;
                fadeProgress = 0f;
                timer = Random.Range(intervalRange.x, intervalRange.y);
            }
            else
            {
                SetFrontAlpha(fadeProgress);
            }
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isFading = true;
                fadeProgress = 0f;
            }
        }
    }

    private void SetBackTexture(Texture t)
    {
        if (backQuad == null || t == null) return;
        backMpb.SetTexture("_BaseMap", t);
        backQuad.SetPropertyBlock(backMpb);
    }

    private void SetFrontTexture(Texture t)
    {
        if (frontQuad == null || t == null) return;
        frontMpb.SetTexture("_BaseMap", t);
        frontQuad.SetPropertyBlock(frontMpb);
    }

    private void SetFrontAlpha(float alpha)
    {
        if (frontQuad == null) return;
        frontMpb.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
        frontQuad.SetPropertyBlock(frontMpb);
    }
}
