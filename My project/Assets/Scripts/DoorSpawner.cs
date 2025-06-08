using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;


public class DoorSpawner : MonoBehaviour
{
    public YoloInterface yoloInterface;
    public ARRaycastManager raycastManager;

    void OnEnable()
    {
        yoloInterface.OnObjectDetected += HandleDetection;
    }

    void OnDisable()
    {
        yoloInterface.OnObjectDetected -= HandleDetection;
    }

    void HandleDetection(string className, Vector2 screenPosition)
    {
        List<ARRaycastHit> hits = new();
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.Planes))
        {
            GameObject prefabToSpawn = yoloInterface.detectionPrefabs.Find(p => p.className == className)?.prefab;
            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, hits[0].pose.position, hits[0].pose.rotation);
            }
        }
    }
}
