using UnityEditor;
using UnityEngine;

public sealed class ShineBoxMaterialPostprocessor : AssetPostprocessor
{
    private const string ModelPath = "Assets/Resources/shine blue.fbx";
    private const string MaterialPath = "Assets/Resources/Materials/box 2.mat";

    private void OnPostprocessModel(GameObject root)
    {
        if (!string.Equals(assetPath, ModelPath, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            Debug.LogWarning($"Could not restore the shine box material: {MaterialPath}");
            return;
        }

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials.Length == 0)
            {
                renderer.sharedMaterial = material;
                continue;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }
}
