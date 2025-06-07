using UnityEngine;

public class DockingButtonSpawner : MonoBehaviour
{
    [SerializeField]
    private string objectNameToSpawn = "DockingStation";

    public void SpawnObject()
    {
        var spawner = LimitedObjectSpawner.Instance;
        if (spawner == null)
        {
            Debug.LogWarning("LimitedObjectSpawner instance not found.");
            return;
        }

        Camera cameraToFace = Camera.main;
        if (cameraToFace == null)
        {
            Debug.LogWarning("Main Camera not found.");
            return;
        }

        Vector3 spawnPosition = cameraToFace.transform.position + cameraToFace.transform.forward * 1.0f;
        Quaternion spawnRotation = Quaternion.LookRotation(-cameraToFace.transform.forward);

        spawner.TrySpawn(objectNameToSpawn, spawnPosition, spawnRotation, out _);
    }
}
