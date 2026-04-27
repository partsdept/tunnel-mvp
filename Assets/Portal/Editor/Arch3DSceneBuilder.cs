using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// One-shot editor utility to build the Arch3D test scene with a
/// representative-complexity 3D environment for performance testing.
/// Run via menu: Portal > Build Arch3D Test Scene.
///
/// Creates 50 randomly placed primitive objects as children of
/// Arch3D_TestContent, with URP Lit materials, plus directional and
/// point lights. The Arch3D_TestContent root has a RotationDriver for
/// continuous slow rotation.
///
/// Idempotent: re-running clears existing test content and rebuilds.
/// </summary>
public class Arch3DSceneBuilder
{
    private const string SceneRootName = "Arch3D_Scene";
    private const string ContentRootName = "Arch3D_TestContent";
    private const string LayerName = "Arch3D";
    private const string MaterialFolder = "Assets/Portal/Arch3DMaterials";

    private const int ObjectCount = 50;
    // Volume around origin where objects spawn. Adjusted so a perspective
    // camera at z=-12 with FOV 60 sees them comfortably.
    private static readonly Vector3 SpawnVolume = new Vector3(20f, 8f, 6f);
    private const float MinScale = 0.6f;
    private const float MaxScale = 1.6f;

    [MenuItem("Portal/Build Arch3D Test Scene")]
    public static void Build()
    {
        GameObject sceneRoot = GameObject.Find(SceneRootName);
        if (sceneRoot == null)
        {
            Debug.LogError($"Could not find GameObject named '{SceneRootName}'. Create it first.");
            return;
        }

        int layer = LayerMask.NameToLayer(LayerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{LayerName}' not found. Add it via Project Settings.");
            return;
        }

        // Material folder
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/Portal", "Arch3DMaterials");
        }

        // Find / create content root
        Transform existing = sceneRoot.transform.Find(ContentRootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject content = new GameObject(ContentRootName);
        content.layer = layer;
        content.transform.SetParent(sceneRoot.transform, false);
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        content.AddComponent<RotationDriver>();

        // Find URP Lit shader
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("URP Lit shader not found. Is URP installed?");
            return;
        }

        // Pre-create a small palette of materials (reuse to keep draw call counts realistic)
        const int paletteSize = 8;
        Material[] palette = new Material[paletteSize];
        for (int i = 0; i < paletteSize; i++)
        {
            string matName = $"Arch3DPalette_{i:D2}";
            string matPath = $"{MaterialFolder}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(urpLit);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = urpLit;
            }
            // Vary hue across palette
            float hue = (float)i / paletteSize;
            Color color = Color.HSVToRGB(hue, 0.7f, 0.85f);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Metallic", Random.Range(0f, 0.4f));
            mat.SetFloat("_Smoothness", Random.Range(0.3f, 0.7f));
            EditorUtility.SetDirty(mat);
            palette[i] = mat;
        }

        // Spawn primitives
        PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder };
        Random.InitState(42); // deterministic spawn pattern across rebuilds

        for (int i = 0; i < ObjectCount; i++)
        {
            PrimitiveType pt = types[Random.Range(0, types.Length)];
            GameObject obj = GameObject.CreatePrimitive(pt);
            obj.name = $"Obj_{i:D3}_{pt}";
            obj.layer = layer;
            obj.transform.SetParent(content.transform, false);
            obj.transform.localPosition = new Vector3(
                Random.Range(-SpawnVolume.x / 2f, SpawnVolume.x / 2f),
                Random.Range(-SpawnVolume.y / 2f, SpawnVolume.y / 2f),
                Random.Range(-SpawnVolume.z / 2f, SpawnVolume.z / 2f)
            );
            obj.transform.localRotation = Random.rotation;
            float s = Random.Range(MinScale, MaxScale);
            obj.transform.localScale = new Vector3(s, s, s);

            // Remove auto-generated collider (not needed for rendering test)
            Collider col = obj.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            mr.sharedMaterial = palette[Random.Range(0, paletteSize)];
        }

        // Add directional light if not present
        Transform dirLightT = sceneRoot.transform.Find("Arch3D_DirectionalLight");
        GameObject dirLight;
        if (dirLightT == null)
        {
            dirLight = new GameObject("Arch3D_DirectionalLight");
            dirLight.transform.SetParent(sceneRoot.transform, false);
            Light l = dirLight.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.95f, 0.85f);
            l.intensity = 1.2f;
            l.shadows = LightShadows.Soft;
        }
        else
        {
            dirLight = dirLightT.gameObject;
        }
        dirLight.layer = layer;
        dirLight.transform.localRotation = Quaternion.Euler(45f, 30f, 0f);

        // Add point light for accent
        Transform ptLightT = sceneRoot.transform.Find("Arch3D_PointLight");
        GameObject ptLight;
        if (ptLightT == null)
        {
            ptLight = new GameObject("Arch3D_PointLight");
            ptLight.transform.SetParent(sceneRoot.transform, false);
            Light l = ptLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.4f, 0.6f, 1f);
            l.intensity = 5f;
            l.range = 15f;
        }
        else
        {
            ptLight = ptLightT.gameObject;
        }
        ptLight.layer = layer;
        ptLight.transform.localPosition = new Vector3(0f, 0f, -3f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Arch3D test scene built: {ObjectCount} objects, palette of {paletteSize} materials.");
        EditorUtility.SetDirty(sceneRoot);
    }
}
