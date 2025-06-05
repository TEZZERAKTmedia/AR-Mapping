using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LimitedIndexSpawner : MonoBehaviour
{
    public ObjectSpawner objectSpawner;
    public int[] spawnLimits; // One entry per prefab index
    private int[] spawnCounts;

    private void Awake()
    {
        spawnCounts = new int[spawnLimits.Length];
    }

    public void TrySpawn(int index)
    {
        if (index < 0 || index >= spawnLimits.Length) return;

        if (spawnCounts[index] >= spawnLimits[index])
        {
            Debug.Log($"Spawn limit reached for index {index}.");
            return;
        }

        objectSpawner.Spawn(index);
        spawnCounts[index]++;
    }
}
