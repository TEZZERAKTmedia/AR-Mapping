using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DoorTrackerManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject doorOverlayPrefab;
    private readonly List<Vector3> trackedDoorPositions = new();
    private static List<ARRaycastHit> hits = new();

    public void TrackDoor(Rect screenRect)
    {
        Vector2 screenCenter = new(screenRect.center.x, screenRect.center.y);
        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.Planes)) return;

        Pose pose = hits[0].pose;
        Vector3 worldPos = pose.position;

        foreach (var pos in trackedDoorPositions)
        {
            if (Vector3.Distance(pos, worldPos) < 0.5f) return; // already tracked
        }

        trackedDoorPositions.Add(worldPos);
        Debug.Log($"[TRACKER] ✅ New door mapped at {worldPos}");

        if (doorOverlayPrefab != null)
            Instantiate(doorOverlayPrefab, worldPos, Quaternion.identity);
    }
}
