using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A spawner that limits how many times each object prefab can be spawned.
/// Supports spawn by name or selected index, raycast-driven positioning, and camera-facing orientation.
/// </summary>
public class LimitedObjectSpawner : MonoBehaviour
{
    public static LimitedObjectSpawner Instance { get; private set; }

    [System.Serializable]
    public class SpawnableObject
    {
        public string objectName;
        public GameObject prefab;
        public int maxSpawnCount = 1;

        [HideInInspector] public int currentSpawnCount = 0;
        [HideInInspector] public List<GameObject> spawnedInstances = new List<GameObject>();
    }

    [SerializeField]
    private List<SpawnableObject> spawnableObjects;

    [SerializeField]
    private Camera cameraToFace;

    [SerializeField]
    private int selectedObjectIndex = 0;

    [SerializeField]
    private float spawnAngleRange = 45f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cameraToFace == null)
            cameraToFace = Camera.main;
    }

    // -------- Access Methods --------

    public void SetSelectedObjectIndex(int index)
    {
        selectedObjectIndex = Mathf.Clamp(index, 0, spawnableObjects.Count - 1);
    }

    public void SetSelectedObjectByName(string name)
    {
        int index = spawnableObjects.FindIndex(o => o.objectName == name);
        if (index != -1)
            selectedObjectIndex = index;
        else
            Debug.LogWarning($"[LimitedObjectSpawner] No object with name '{name}' found.");
    }

    public GameObject GetPrefab(string objectName)
    {
        var spawnable = spawnableObjects.Find(obj => obj.objectName == objectName);
        return spawnable?.prefab;
    }

    public bool CanSpawn(string objectName)
    {
        var spawnable = spawnableObjects.Find(obj => obj.objectName == objectName);
        return spawnable != null && spawnable.currentSpawnCount < spawnable.maxSpawnCount;
    }

    // -------- Raycast-Based Spawning --------

    public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
    {
        if (selectedObjectIndex < 0 || selectedObjectIndex >= spawnableObjects.Count)
        {
            Debug.LogWarning("[LimitedObjectSpawner] Invalid selected object index.");
            return false;
        }

        var spawnable = spawnableObjects[selectedObjectIndex];
        string objectName = spawnable.objectName;

        if (!CanSpawn(objectName))
        {
            Debug.Log($"[LimitedObjectSpawner] Limit reached for {objectName}. Skipping spawn.");
            return false;
        }

        // Face the camera
        Vector3 forward = cameraToFace.transform.position - spawnPoint;
        Vector3 projectedForward = Vector3.ProjectOnPlane(forward, spawnNormal);
        Quaternion spawnRotation = Quaternion.LookRotation(projectedForward, spawnNormal);

        // Add some random yaw rotation
        float randomRotation = Random.Range(-spawnAngleRange, spawnAngleRange);
        spawnRotation *= Quaternion.Euler(0f, randomRotation, 0f);

        // Spawn
        if (TrySpawn(objectName, spawnPoint, spawnRotation, out GameObject newObject))
        {
            return true;
        }

        return false;
    }

    // -------- Direct Instantiation --------

    public bool TrySpawn(string objectName, Vector3 position, Quaternion rotation, out GameObject spawnedObject)
    {
        spawnedObject = null;

        var spawnable = spawnableObjects.Find(obj => obj.objectName == objectName);
        if (spawnable == null)
        {
            Debug.LogWarning($"[LimitedObjectSpawner] No object found with name {objectName}");
            return false;
        }

        if (spawnable.currentSpawnCount >= spawnable.maxSpawnCount)
        {
            Debug.Log($"[LimitedObjectSpawner] Max spawn count reached for {objectName}");
            return false;
        }

        spawnedObject = Instantiate(spawnable.prefab, position, rotation);
        spawnable.currentSpawnCount++;
        spawnable.spawnedInstances.Add(spawnedObject);

        Debug.Log($"[LimitedObjectSpawner] Spawned {objectName} ({spawnable.currentSpawnCount}/{spawnable.maxSpawnCount})");
        return true;
    }

    // -------- Cleanup --------

    public void ClearAllSpawnedObjects()
    {
        foreach (var spawnable in spawnableObjects)
        {
            foreach (var obj in spawnable.spawnedInstances)
            {
                if (obj != null)
                    Destroy(obj);
            }

            spawnable.spawnedInstances.Clear();
            spawnable.currentSpawnCount = 0;
        }

        Debug.Log("[LimitedObjectSpawner] All limited spawned objects cleared.");
    }
}
