using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// One-shot editor utility to build the 16-quad slideshow scene.
/// Run via menu: Portal > Build Slideshow Scene.
///
/// Creates 16 quads as children of Slideshow_Scene, each with its
/// own material referencing the corresponding slide_screen_NN texture.
/// Idempotent: re-running clears existing quads and rebuilds.
/// </summary>
public class SlideshowSceneBuilder
{
    // Plate position layout — matches the global Col/Row mapping.
    // Each entry: quad name, screen number (1-16), grid position (col, row) where col 1-4, row 1-4.
    private struct SlideQuadConfig
    {
        public string name;
        public int screenNumber;
        public int col;
        public int row;

        public SlideQuadConfig(string n, int s, int c, int r)
        {
            name = n; screenNumber = s; col = c; row = r;
        }
    }

    private static readonly SlideQuadConfig[] Configs = new[]
    {
        // Q1 cells (top-left of plate)
        new SlideQuadConfig("Slide_Q1_TL", 1, 1, 1),
        new SlideQuadConfig("Slide_Q1_TR", 2, 2, 1),
        new SlideQuadConfig("Slide_Q1_BL", 3, 1, 2),
        new SlideQuadConfig("Slide_Q1_BR", 4, 2, 2),
        // Q2 cells (bottom-left)
        new SlideQuadConfig("Slide_Q2_TL", 5, 1, 3),
        new SlideQuadConfig("Slide_Q2_TR", 6, 2, 3),
        new SlideQuadConfig("Slide_Q2_BL", 7, 1, 4),
        new SlideQuadConfig("Slide_Q2_BR", 8, 2, 4),
        // Q3 cells (top-right)
        new SlideQuadConfig("Slide_Q3_TL", 9,  3, 1),
        new SlideQuadConfig("Slide_Q3_TR", 10, 4, 1),
        new SlideQuadConfig("Slide_Q3_BL", 11, 3, 2),
        new SlideQuadConfig("Slide_Q3_BR", 12, 4, 2),
        // Q4 cells (bottom-right)
        new SlideQuadConfig("Slide_Q4_TL", 13, 3, 3),
        new SlideQuadConfig("Slide_Q4_TR", 14, 4, 3),
        new SlideQuadConfig("Slide_Q4_BL", 15, 3, 4),
        new SlideQuadConfig("Slide_Q4_BR", 16, 4, 4),
    };

    // Camera view: orthographic Size 5 = 10 tall, 16:9 aspect = 17.78 wide.
    // Cell center positions: col 1-4 -> X = -6.67, -2.22, +2.22, +6.67
    //                       row 1-4 -> Y = +3.75, +1.25, -1.25, -3.75
    private const float CellWidth = 4.5f;   // slightly wider than 4.44 for coverage
    private const float CellHeight = 2.5f;
    private static readonly float[] ColX = { -6.67f, -2.22f, 2.22f, 6.67f };
    private static readonly float[] RowY = { 3.75f, 1.25f, -1.25f, -3.75f };

    private const string TextureFolder = "Assets/Portal/SlideshowCards";
    private const string MaterialFolder = "Assets/Portal/SlideshowMaterials";
    private const string SlideshowSceneName = "Slideshow_Scene";
    private const string SlideshowLayerName = "Slideshow";

    [MenuItem("Portal/Build Slideshow Scene")]
    public static void BuildSlideshowScene()
    {
        // Find the Slideshow_Scene container
        GameObject sceneRoot = GameObject.Find(SlideshowSceneName);
        if (sceneRoot == null)
        {
            Debug.LogError($"Could not find GameObject named '{SlideshowSceneName}'. " +
                           "Create an empty GameObject with that name first.");
            return;
        }

        int slideshowLayer = LayerMask.NameToLayer(SlideshowLayerName);
        if (slideshowLayer == -1)
        {
            Debug.LogError($"Layer '{SlideshowLayerName}' not found. Add it via Edit > Project Settings > Tags and Layers.");
            return;
        }

        // Make sure material folder exists
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/Portal", "SlideshowMaterials");
        }

        // Clear any existing slide quads (children that start with "Slide_")
        // Iterate in reverse so we don't disturb indices while destroying.
        for (int i = sceneRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = sceneRoot.transform.GetChild(i);
            if (child.name.StartsWith("Slide_"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // Find the URP Unlit shader
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit == null)
        {
            Debug.LogError("Could not find shader 'Universal Render Pipeline/Unlit'. Is URP installed?");
            return;
        }

        int created = 0;
        foreach (var cfg in Configs)
        {
            // Load the texture for this screen
            string texPath = $"{TextureFolder}/slide_screen_{cfg.screenNumber:D2}.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
            {
                Debug.LogWarning($"Texture not found: {texPath}. Skipping {cfg.name}.");
                continue;
            }

            // Create or reuse the material asset
            string matName = $"Slide{cfg.screenNumber:D2}_Material";
            string matPath = $"{MaterialFolder}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(urpUnlit);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = urpUnlit;
            }
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(mat);

            // Create the quad GameObject
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = cfg.name;
            quad.layer = slideshowLayer;
            quad.transform.SetParent(sceneRoot.transform, false);
            quad.transform.localPosition = new Vector3(
                ColX[cfg.col - 1], RowY[cfg.row - 1], 0f);
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = new Vector3(CellWidth, CellHeight, 1f);

            // Quads come with a MeshCollider by default — remove it (not needed for compositor)
            Object.DestroyImmediate(quad.GetComponent<MeshCollider>());

            // Assign the material
            MeshRenderer mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Slideshow scene built: {created} quads created/updated.");
        EditorUtility.SetDirty(sceneRoot);
    }
}