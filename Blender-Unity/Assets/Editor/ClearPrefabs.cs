using UnityEngine;
using UnityEditor;
using System.IO;

public class ClearPrefabs
{
    [MenuItem("Tools/Clear All Prefabs in Prefabs Folder")]
    public static void ClearAllPrefabs()
    {
        string prefabFolder = "Assets/Prefabs";
        if (!Directory.Exists(prefabFolder))
        {
            Debug.LogWarning("⚠️ Prefab folder does not exist.");
            return;
        }

        string[] prefabFiles = Directory.GetFiles(prefabFolder, "*.prefab", SearchOption.TopDirectoryOnly);
        foreach (string filePath in prefabFiles)
        {
            AssetDatabase.DeleteAsset(filePath);
            Debug.Log($"🗑️ Deleted: {filePath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("✅ All prefabs cleared from Assets/Prefabs.");
    }
}

