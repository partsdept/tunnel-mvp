using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Slideshow mode controller. Hotkeys N and Space only respond when
/// Slideshow mode is active.
///
/// Hotkeys (also callable via OSC dispatch):
///   N      cycle through playlist sources
///   Space  toggle pause/resume
/// </summary>
public class SlideshowController : MonoBehaviour
{
    [Header("Mode reference")]
    public OutputModeController outputModeController;

    [Header("Cells (16, in plate position order)")]
    public SlideshowCell[] cells = new SlideshowCell[16];

    [Header("Numbered cards (1-16, baseline playlist)")]
    public Texture[] numberedCards = new Texture[16];

    [Header("Random pool source folder (under Resources)")]
    public string randomPoolFolder = "SlideshowImages";

    [Header("Initial state")]
    public bool startPaused = true;

    private Texture[] randomPool;
    private enum Source { Numbered, RandomPool }
    private Source currentSource = Source.Numbered;
    private bool paused;

    void Start()
    {
        randomPool = Resources.LoadAll<Texture>(randomPoolFolder);
        if (randomPool == null || randomPool.Length == 0)
        {
            Debug.LogWarning($"SlideshowController: no textures found in Resources/{randomPoolFolder}/");
        }
        else
        {
            Debug.Log($"SlideshowController: loaded {randomPool.Length} images from {randomPoolFolder}");
        }

        paused = startPaused;
        ApplySourceToCells();
        ApplyPauseToCells();
    }

    void Update()
    {
        if (outputModeController == null) return;
        if (outputModeController.GetCurrentMode() != OutputModeController.Mode.Slideshow) return;

        if (Input.GetKeyDown(KeyCode.N)) CyclePlaylistPublic();
        else if (Input.GetKeyDown(KeyCode.Space)) TogglePausePublic();
    }

    public void CyclePlaylistPublic()
    {
        currentSource = (currentSource == Source.Numbered) ? Source.RandomPool : Source.Numbered;
        ApplySourceToCells();
        Debug.Log($"Slideshow source: {currentSource}");
    }

    public void TogglePausePublic()
    {
        paused = !paused;
        ApplyPauseToCells();
        Debug.Log($"Slideshow paused: {paused}");
    }

    private void ApplySourceToCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == null) continue;

            switch (currentSource)
            {
                case Source.Numbered:
                    if (i < numberedCards.Length && numberedCards[i] != null)
                    {
                        cells[i].SetPlaylist(new Texture[] { numberedCards[i] });
                    }
                    break;
                case Source.RandomPool:
                    if (randomPool != null && randomPool.Length > 0)
                    {
                        cells[i].SetPlaylist(new List<Texture>(randomPool));
                    }
                    break;
            }
        }
    }

    private void ApplyPauseToCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == null) continue;
            cells[i].Paused = paused;
        }
    }
}
