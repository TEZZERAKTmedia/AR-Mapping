using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DoorDetector : MonoBehaviour
{
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject doorwayPrefab;

    // Door size thresholds (in meters)
    private const float MinDoorWidth = 0.7f;
    private const float MaxDoorWidth = 1.2f;
    private const float MinDoorHeight = 1.8f;

    private bool detectionStarted = false;
    private List<Vector3> doorPositions = new List<Vector3>();

    void Update()
    {
        if (!detectionStarted)
        {
            DetectDoorways();
            detectionStarted = true; // Only run once unless you want to update dynamically
        }
    }

    void DetectDoorways()
    {
        var verticalPlanes = new List<ARPlane>();

        // Step 1: Filter vertical planes
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.Vertical && plane.extents.x >= MinDoorWidth)
            {
                verticalPlanes.Add(plane);
            }
        }

        // Step 2: Find pairs of vertical planes that are close enough to be door sides
        for (int i = 0; i < verticalPlanes.Count; i++)
        {
            for (int j = i + 1; j < verticalPlanes.Count; j++)
            {
                ARPlane a = verticalPlanes[i];
                ARPlane b = verticalPlanes[j];

                float distance = Vector3.Distance(a.center, b.center);
                if (distance >= MinDoorWidth && distance <= MaxDoorWidth)
                {
                    // Check height — could later use mesh bounds if needed
                    if (a.extents.y >= MinDoorHeight || b.extents.y >= MinDoorHeight)
                    {
                        Vector3 doorPosition = (a.center + b.center) / 2f;

                        if (!IsDuplicate(doorPosition))
                        {
                            doorPositions.Add(doorPosition);
                            PlaceDoorMarker(doorPosition, a, b);
                        }
                    }
                }
            }
        }
    }

    void PlaceDoorMarker(Vector3 position, ARPlane a, ARPlane b)
    {
        Quaternion rotation = Quaternion.LookRotation(b.center - a.center);
        Instantiate(doorwayPrefab, position, rotation);
        Debug.Log($"🚪 Doorway detected at {position}");
    }

    bool IsDuplicate(Vector3 newPos)
    {
        foreach (var pos in doorPositions)
        {
            if (Vector3.Distance(pos, newPos) < 0.2f) return true;
        }
        return false;
    }
}
