using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Splits the imported PCI arch OBJ into 16 individual GameObjects,
/// one per screen face. Each GameObject gets its own mesh containing
/// just that face, plus a MeshRenderer ready for an individual material.
///
/// Centroid recentering: the OBJ comes in with arbitrary world coords
/// from Rhino. After splitting, we translate all faces so the overall
/// centroid is at world origin (makes the camera math sane).
///
/// Naming: faces are named ArchFace_00 through ArchFace_15 in OBJ order.
/// You'll likely want to remap face -> screen position visually.
///
/// Run via menu: Portal > Build Arch Preview Faces.
/// </summary>
public class ArchFaceSplitter
{
    private const string SceneRootName = "Preview3D_Scene";
    private const string FacesParentName = "ArchFaces";
    private const string LayerName = "Preview3D";
    private const string ObjAssetPath = "Assets/Portal/ArchGeometry/PCI_SCREEN_FACES_ONLY_V2.obj";
    private const string MaterialFolder = "Assets/Portal/ArchGeometry/FaceMaterials";

    [MenuItem("Portal/Build Arch Preview Faces")]
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

        // Load the OBJ asset
        GameObject objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ObjAssetPath);
        if (objPrefab == null)
        {
            Debug.LogError($"OBJ not found at {ObjAssetPath}. Ensure it has been imported.");
            return;
        }

        // Find the mesh inside the OBJ prefab
        MeshFilter sourceMeshFilter = objPrefab.GetComponentInChildren<MeshFilter>();
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            Debug.LogError("Could not find a Mesh in the OBJ prefab.");
            return;
        }
        Mesh sourceMesh = sourceMeshFilter.sharedMesh;
        Vector3[] verts = sourceMesh.vertices;
        Vector2[] uvs = sourceMesh.uv;
        Vector3[] normals = sourceMesh.normals;
        int[] tris = sourceMesh.triangles;

        // The OBJ has 16 quads; quads import as 32 triangles (2 per quad).
        // We split by every 6 indices = 1 quad worth of triangles.
        int triCount = tris.Length;
        int facesExpected = 16;
        int trisPerFace = triCount / facesExpected;
        if (triCount != facesExpected * trisPerFace)
        {
            Debug.LogWarning($"Triangle count {triCount} doesn't divide evenly into {facesExpected} faces. Got {trisPerFace} per face.");
        }

        // Material folder
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/Portal/ArchGeometry", "FaceMaterials");
        }

        // Find a parent GameObject for the faces; clear if it exists.
        Transform existingParent = sceneRoot.transform.Find(FacesParentName);
        if (existingParent != null)
        {
            Object.DestroyImmediate(existingParent.gameObject);
        }
        GameObject facesParent = new GameObject(FacesParentName);
        facesParent.transform.SetParent(sceneRoot.transform, false);
        facesParent.layer = layer;

        // Compute centroid across all vertices used by the 16 faces, for recentering.
        Vector3 sumPositions = Vector3.zero;
        int countedVerts = 0;
        HashSet<int> usedVertIndices = new HashSet<int>();
        for (int i = 0; i < tris.Length; i++) usedVertIndices.Add(tris[i]);
        foreach (int idx in usedVertIndices)
        {
            sumPositions += verts[idx];
            countedVerts++;
        }
        Vector3 centroid = sumPositions / Mathf.Max(countedVerts, 1);

        // Apply OBJ scale factor (since we set scale to 0.001 in import settings,
        // Unity may already be applying that to the mesh's vertices, or it may
        // be applied at GameObject scale. We read from the prefab's transform).
        // For safety: just use mesh-space directly and rely on prefab scale.
        // The objPrefab transform's scale is the import scale.

        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit == null)
        {
            Debug.LogError("URP Unlit shader not found.");
            return;
        }

        for (int faceIdx = 0; faceIdx < facesExpected; faceIdx++)
        {
            // Extract this face's triangle indices
            int triStart = faceIdx * trisPerFace;
            int triEnd = triStart + trisPerFace;
            List<int> faceTriIndices = new List<int>();
            for (int t = triStart; t < triEnd && t < tris.Length; t++)
            {
                faceTriIndices.Add(tris[t]);
            }

            // Build a new mesh with just this face's vertices + a fresh index list.
            // Remap original vertex indices to local ones.
            Dictionary<int, int> remap = new Dictionary<int, int>();
            List<Vector3> faceVerts = new List<Vector3>();
            List<Vector2> faceUVs = new List<Vector2>();
            List<Vector3> faceNormals = new List<Vector3>();
            List<int> faceTris = new List<int>();

            foreach (int origIdx in faceTriIndices)
            {
                if (!remap.ContainsKey(origIdx))
                {
                    remap[origIdx] = faceVerts.Count;
                    faceVerts.Add(verts[origIdx] - centroid);
                    if (uvs != null && origIdx < uvs.Length) faceUVs.Add(uvs[origIdx]);
                    if (normals != null && origIdx < normals.Length) faceNormals.Add(normals[origIdx]);
                }
                faceTris.Add(remap[origIdx]);
            }

            Mesh m = new Mesh();
            m.name = $"ArchFace_{faceIdx:D2}_Mesh";
            m.SetVertices(faceVerts);
            if (faceUVs.Count > 0) m.SetUVs(0, faceUVs);
            if (faceNormals.Count > 0) m.SetNormals(faceNormals);
            m.SetTriangles(faceTris, 0);
            m.RecalculateBounds();

            // Save the mesh asset so it persists
            string meshPath = $"Assets/Portal/ArchGeometry/ArchFace_{faceIdx:D2}_Mesh.asset";
            AssetDatabase.CreateAsset(m, meshPath);

            // Create the GameObject
            GameObject face = new GameObject($"ArchFace_{faceIdx:D2}");
            face.layer = layer;
            face.transform.SetParent(facesParent.transform, false);

            // Apply the same scale the OBJ import used (look up from prefab's transform).
            // Since the import uses Scale Factor 0.001, the prefab transform's scale
            // will reflect that. We mirror it on our generated GameObject so geometry
            // sizes match.
            Vector3 scale = objPrefab.transform.lossyScale;
            face.transform.localScale = scale;

            MeshFilter mf = face.AddComponent<MeshFilter>();
            mf.sharedMesh = m;

            MeshRenderer mr = face.AddComponent<MeshRenderer>();

            // Each face gets its own material. Initially all 16 materials are
            // identical and white-tinted — they'll be configured separately
            // (UV transforms picked per-face) once we identify which face is
            // which physical screen.
            string matName = $"ArchFace_{faceIdx:D2}_Material";
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
            mat.SetColor("_BaseColor", Color.white);
            mat.doubleSidedGI = true;
            // Make double-sided so we can see faces from inside the arch
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            EditorUtility.SetDirty(mat);
            mr.sharedMaterial = mat;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Built {facesExpected} arch face GameObjects under '{FacesParentName}'. Centroid was at {centroid}.");
        EditorUtility.SetDirty(sceneRoot);
    }
}
