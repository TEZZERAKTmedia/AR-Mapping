using System;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] objectPrefabs;
    public Transform spawnPoint;
    public bool applyRandomRotation = true;

    [Tooltip("Index to determine which object to spawn.")]
    public int spawnOptionIndex = 0;

    /// <summary>
    /// Event triggered when an object is spawned.
    /// </summary>
    public event Action<GameObject> objectSpawned;

    public void Spawn()
    {
        Spawn(spawnOptionIndex);
    }

    public void Spawn(int index)
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogWarning("ObjectSpawner: No prefabs assigned.");
            return;
        }

        if (index < 0 || index >= objectPrefabs.Length)
        {
            Debug.LogWarning($"ObjectSpawner: Invalid spawn index {index}.");
            return;
        }

        GameObject prefab = objectPrefabs[index];
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rotation = applyRandomRotation ? UnityEngine.Random.rotation : Quaternion.identity;


        GameObject spawned = Instantiate(prefab, position, rotation);
        objectSpawned?.Invoke(spawned);

        Debug.Log($"Spawned {prefab.name} at {position}");
    }
}
