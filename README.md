# Portal of Collective Imagination — Display Mac MVP

Unity 6 project that drives the 8K composite plate for the arch sculpture's 16-screen tunnel display. This MVP demonstrates the rendering architecture, validates capacity on the target hardware (M4 Mac Mini base), and provides the interaction team with a working substrate to extend.

## What this project does

The Display Mac is responsible for producing a single 8K (7680×4320) HDMI signal that gets split through hardware into 16 independent 1920×1080 panels arranged on the arch. This Unity project produces that signal. Multiple rendering "modes" demonstrate different content patterns the piece can use:

- **Q-pipeline**: 4-quadrant composite test patterns for diagnostic purposes
- **Slideshow**: 16 individual numbered cells, one per physical screen, for verifying the splitter's screen-mapping
- **ArchCanvas**: an unrolled-arch authoring space where content is laid out on a flat 2D canvas, then transformed (strip-and-rotate) onto the 8K plate so it appears continuous across the physical arch
- **Arch3D**: a 3D scene rendered through the same arch transform, for capacity-testing real 3D content

A 3D preview mode (TAB) shows whatever the active mode is producing, mapped onto the actual arch geometry from Rhino — letting the team see how content will look on the real physical structure without building it.

## Running

### First open (cloned repo, fresh Unity install)

After opening the project for the first time, the Hierarchy may appear empty. To load the actual scene:

1. In the Project panel, navigate to `Assets/Scenes/`
2. Double-click `SampleScene.unity`

The Hierarchy will populate with all the GameObjects (PortalControllers, ArchCanvas_Scene, Output_Scene, etc.). Unity remembers this for subsequent opens.

If you see warnings about Input Manager being deprecated, ignore them. Verify Edit → Project Settings → Player → Other Settings → Active Input Handling is set to **Both**.

If TextMeshPro shows errors, run Window → TextMeshPro → Import TMP Essential Resources.

If the 3D preview shows no arch geometry (only an empty Preview3D_Scene), run Portal → Build Arch Preview Faces from the menu. This regenerates the 16 split face GameObjects from the imported OBJ.

### Daily use

Press Play in the editor, or build a standalone .app for fullscreen testing. Use the hotkeys below to switch modes.

## Hotkeys

```
Output modes (top-level):
  C     Q-pipeline solid colors
  Q     Q-pipeline test cards
  S     Slideshow
  A     ArchCanvas
  3     Arch3D test scene

Cross-mode overlays:
  H     Heartbeat sweep on output (system-alive indicator)
  D     Debug overlay text on output (diagnostic title)

View toggle:
  TAB   Flat 8K output ↔ 3D arch preview

Within ArchCanvas mode (no-op outside A):
  M     Markers (corner reference markers + apex/midline)
  T     Drifting title text
  P     Particle field
  B     Backdrop image on/off
  N     Cycle backdrop image

Preview camera (when in 3D preview):
  R                    Reset to default view
  Mouse drag (left)    Orbit around target
  Mouse drag (right)   Pan target
  Scroll wheel         Zoom
```

## Architecture

The rendering chain is a multi-stage pipeline:

```
Mode pipelines             Compositors           Output
─────────────              ──────────             ──────
QComposite_Camera     →    QPipeline_RT     ┐
Slideshow_Camera      →    Slideshow_RT     ├→ Output_Material → Output_Camera → screen
Arch_Camera (canvas)  →    Arch_RT          │
  ↓ ArchComposite_Camera →   ArchPlate_RT   ┤
Arch3D_Camera         →    Arch3D_RT        │
  ↓ Arch3DComposite_Camera → Arch3DPlate_RT ┘

                                            └→ Preview3D_Camera → screen (when TAB)
```

Only the active mode's cameras are enabled at any given time. OutputModeController handles the camera enable/disable transitions when the user changes modes. This is the optimization that takes the M4 base Mini from ~9 fps (all cameras running) to ~37 fps (active mode only).

Output_Material's Base Map is set by OutputModeController to whichever mode RT is active. Output_Quad samples it in flat output mode. In 3D preview, 16 individual face materials track Output_Material's Base Map (via Preview3DController) and each samples a 1920×1080 region for its physical position on the arch.

### Scenes (top-level GameObjects in SampleScene)

```
QComposite_Scene       — 4-cell Q-pipeline composite (renders QPipeline_RT)
Slideshow_Scene        — 16-cell slideshow (renders Slideshow_RT)
ArchCanvas_Scene       — unrolled-arch canvas (renders Arch_RT)
  ArchCanvas_Camera
  ArchCanvas_Markers   — corner markers + reference lines
  ArchCanvas_DynamicText — 4 drifting TMP text instances
  ArchCanvas_Backdrop  — cosmic background quad with cycling textures
  ArchParticleField    — particle system on arch canvas
ArchComposite_Scene    — strip-and-rotate transform (Arch_RT → ArchPlate_RT)
Arch3D_Scene           — 3D capacity-test content (renders Arch3D_RT)
Arch3DComposite_Scene  — strip-and-rotate (Arch3D_RT → Arch3DPlate_RT)
Preview3D_Scene        — 16 arch faces from imported OBJ + orbit camera
Output_Scene           — Output_Camera + Output_Quad + sweep + debug overlay
PortalControllers      — empty GameObject hosting all controller scripts
```

### Controller scripts

- **OutputModeController**: hotkey C/Q/S/A/3 → mode switching; manages camera enable states; sets Output_Material's Base Map
- **ArchCanvasController**: hotkey M/T/P/B/N (nested under ArchCanvas mode); toggles markers/text/particles/backdrop; cycles backdrop images
- **OverlayController**: hotkey H/D/TAB; cross-mode overlays + flat/preview view toggle
- **Preview3DController**: syncs 16 arch face materials with Output_Material's current Base Map
- **OrbitCameraController**: orbit/pan/zoom/reset on Preview3D_Camera

### Plate layout

```
+---+---+---+---+
|Q1 |Q1 |Q3 |Q3 |    Top rows of physical screens (near apex)
|TL |TR |TL |TR |
+---+---+---+---+
|Q1 |Q1 |Q3 |Q3 |
|BL |BR |BL |BR |
+---+---+---+---+
|Q2 |Q2 |Q4 |Q4 |
|TL |TR |TL |TR |
+---+---+---+---+
|Q2 |Q2 |Q4 |Q4 |    Bottom rows of physical screens (near floor)
|BL |BR |BL |BR |
+---+---+---+---+
   left side    right side
```

Q1+Q2 = left half of arch; Q3+Q4 = right half. Within each side: top half = upper rows, bottom half = lower rows. The plate is paired left/right symmetrically for cabling.

### Strip-and-rotate transform

The ArchCanvas is conceptually the unrolled arch. It's 8640×3840 (8 screens around the arch × 2 screens along the tunnel), wider than the plate. The strip-and-rotate transform takes each half of the canvas and rotates it 90° to fit onto each half of the plate:

- Left half of canvas → CCW rotation → left half of plate
- Right half of canvas → CW rotation → right half of plate

Reading the canvas top-to-bottom traces: right-floor → right-wall → apex → left-wall → left-floor.

## Capacity findings (M4 Mac Mini base)

Measured in fullscreen build, after the on-demand camera optimization:

| Mode | FPS |
|------|-----|
| Q-pipeline (test cards or solid colors) | ~45 |
| Slideshow | ~45 |
| ArchCanvas (no overlays) | ~35 |
| ArchCanvas + 20K particles | ~33 |
| ArchCanvas + 50K particles | ~14 (CPU-bound on per-particle script) |
| Arch3D (50 lit primitives, shadows) | ~25-30 |

The strip-and-rotate transform's fixed cost (~25ms per frame) is the ArchCanvas mode's ceiling. Up to ~20K particles is essentially free; beyond that the per-particle Update loop becomes CPU-bound.

The M4 Mini base is fragment-bound for 8K rendering. Pixel-fill cost dominates; geometry count is essentially free within reasonable limits. Object count went from 50 to 5 with no measurable framerate change.

The M4 Pro MacBook Pro hits ~63 fps in Arch3D mode — roughly 2× the Mini, scaling roughly with GPU core count.

## Hardware target

Apple M4 Mac Mini base ($600). Validates that the architecture works on the lowest tier of Apple Silicon currently shipping. Stronger hardware (M4 Pro, M4 Max) would give more headroom but isn't required for the demonstrated content profiles.

## Adding new content

For arch canvas content (the typical case for the team):

1. Create a new GameObject under `ArchCanvas_Scene` with Layer = Arch
2. Set its Transform within the canvas's coordinate space (~22.5 wide × 10 tall, centered at origin)
3. Whatever the Arch_Camera sees gets rendered into Arch_RT, then through the strip-and-rotate transform onto the plate
4. Optional: add nested toggle support via ArchCanvasController if it's a layer that should be independently controllable

For new top-level modes (less common):

1. Build a new scene container with its own camera and render texture
2. Add a Mode enum value to OutputModeController
3. Wire the new mode's RT and camera reference; add a switch case in SetMode
4. Add a hotkey in OutputModeController.Update()

## Future work

Planned by the interaction team or follow-on sessions:

- OSC integration with the Sensor Mac (microphones, Whisper, Qwen pipeline)
- Watched-folder loader for Flux-generated PNGs from the Drawing Mac
- Multi-language font fallback chain (CJK, Devanagari, Arabic via Noto Sans variants)
- Wood backlight controller (separate Arduino/Teensy channel)
- Watchdog for unattended desert deployment
- Thermal validation under desert conditions
- Real Fresco 8K splitter validation on bench
