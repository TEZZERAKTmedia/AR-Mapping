using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXBatchProcessor
{
    [MenuItem("Tools/Convert FBX To Standalone Prefab")]
    public static void ProcessFBX()
    {
        string importPath = "fbx"; // non-Assets folder
        string outputPath = "Assets/Prefabs";

        if (!Directory.Exists(importPath))
        {
            Debug.LogError("[❌] 'fbx' folder not found.");
            return;
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string[] fbxFiles = Directory.GetFiles(importPath, "*.fbx", SearchOption.AllDirectories);
        foreach (var fbxPath in fbxFiles)
        {
            string unityPath = "Assets" + fbxPath.Replace(Directory.GetCurrentDirectory(), "").Replace("\\", "/");
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(unityPath);

            if (obj != null)
            {
                string name = Path.GetFileNameWithoutExtension(fbxPath);
                string prefabPath = $"{outputPath}/{name}_Clean.prefab";

                GameObject clone = Object.Instantiate(obj);
                PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);
                Object.DestroyImmediate(clone);

                Debug.Log($"✅ Created: {prefabPath}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not load: {fbxPath}");
            }
        }

        AssetDatabase.Refresh();
    }
}

