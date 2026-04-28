using UnityEngine;

/// <summary>
/// Keeps the 16 arch preview face materials in sync with whatever
/// the OutputModeController is currently displaying. When the mode
/// changes, Output_Material's Base Map gets reassigned to the new
/// mode's RT; this script copies that texture to all 16 face
/// materials so the 3D preview tracks the active mode.
///
/// We poll Output_Material's _BaseMap each frame rather than
/// hooking into mode-change events. It's simpler and the cost is
/// trivial (one texture comparison per frame).
/// </summary>
public class Preview3DController : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("The output material whose Base Map we track.")]
    public Material outputMaterial;

    [Header("Face materials (16, in OBJ face order)")]
    public Material[] faceMaterials = new Material[16];

    private Texture lastSeenTexture;

    void LateUpdate()
    {
        if (outputMaterial == null) return;

        Texture current = outputMaterial.GetTexture("_BaseMap");
        if (current == lastSeenTexture) return;

        lastSeenTexture = current;
        for (int i = 0; i < faceMaterials.Length; i++)
        {
            if (faceMaterials[i] != null)
            {
                faceMaterials[i].SetTexture("_BaseMap", current);
            }
        }
    }
}
