using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class PrefabButtonSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    [SerializeField] private GameObject prefabToSpawn;

    [Header("AR References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    private static readonly List<ARRaycastHit> hits = new();

    public void SpawnObject()
    {
        if (prefabToSpawn == null || raycastManager == null || planeManager == null)
        {
            Debug.LogWarning("Missing prefab or AR managers.");
            return;
        }

        Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);

        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Debug.Log("No plane hit.");
            return;
        }

        var hit = hits[0];
        Pose hitPose = hit.pose;
        ARPlane hitPlane = planeManager.GetPlane(hit.trackableId);

        if (hitPlane == null)
        {
            Debug.LogWarning("No ARPlane data.");
            return;
        }

        // Use plane normal to rotate the object toward the wall (or up if horizontal)
        Vector3 forward = hitPlane.normal; // wall-normal (use .normal in Unity 2023+)
        if (Vector3.Dot(Vector3.up, forward) > 0.9f)
            forward = -Camera.main.transform.forward; // fallback if flat floor plane

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        // Spawn object
        GameObject spawned = Instantiate(prefabToSpawn, hitPose.position, rotation);

        // Align base of object to the plane (ground)
        AlignBaseToYPlane(spawned, hitPose.position.y);
    }

    void AlignBaseToYPlane(GameObject obj, float groundY)
    {
        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        float bottomY = rend.bounds.min.y;
        float offset = groundY - bottomY;

        obj.transform.position += Vector3.up * offset;
    }
}
