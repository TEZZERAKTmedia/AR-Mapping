using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DoorSpawner : MonoBehaviour
{
    [Header("Prefab (real dimensions, e.g. 1×2×0.05 m)")]
    public GameObject doorPrefab;

    [Header("References")]
    public YoloInterface yolo;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Tooltip("Minimum distance between spawned doors in meters")]
    public float minimumDoorSpacing = 1.0f;

    private List<Vector3> spawnedDoorCenters = new();
    private List<ARRaycastHit> rayHits = new();

    void OnEnable()
    {
        yolo.OnDoorDetected += PlaceDoor;
        Debug.Log("[SPAWNER] Subscribed to YOLO detection event.");
    }

    void OnDisable()
    {
        yolo.OnDoorDetected -= PlaceDoor;
        Debug.Log("[SPAWNER] Unsubscribed from YOLO detection event.");
    }

    void PlaceDoor(Rect box)
    {
        Vector2 screenPoint = box.center;
        Debug.Log($"[SPAWNER] Received YOLO box: {box}");

        if (!raycastManager.Raycast(screenPoint, rayHits, TrackableType.PlaneWithinPolygon))
        {
            Debug.LogWarning($"[SPAWNER] ❌ No AR plane under screen point {screenPoint}");
            return;
        }

        var hit = rayHits[0];
        Pose hitPose = hit.pose;
        Vector3 center = hitPose.position;

        foreach (var prev in spawnedDoorCenters)
        {
            if (Vector3.Distance(center, prev) < minimumDoorSpacing)
            {
                Debug.Log($"[SPAWNER] ❌ Skipping duplicate door. Too close to existing one at {prev}");
                return;
            }
        }

        // Raycast left/right edges to align the door
        Vector2 leftEdge = new Vector2(box.xMin + 2, box.center.y);
        Vector2 rightEdge = new Vector2(box.xMax - 2, box.center.y);

        Debug.Log($"[SPAWNER] Raycasting for door orientation at edges: L={leftEdge}, R={rightEdge}");

        if (!raycastManager.Raycast(leftEdge, rayHits, TrackableType.PlaneWithinPolygon) ||
            !raycastManager.Raycast(rightEdge, rayHits, TrackableType.PlaneWithinPolygon))
        {
            Debug.LogWarning("[SPAWNER] ❌ Could not raycast both door edges for alignment");
            return;
        }

        Vector3 worldLeft = rayHits[0].pose.position;
        Vector3 worldRight = rayHits[1].pose.position;
        Vector3 direction = (worldRight - worldLeft).normalized;
        Quaternion doorRotation = Quaternion.LookRotation(direction, Vector3.up);

        Debug.Log($"[SPAWNER] ✅ Door alignment vector: {direction}, rotation: {doorRotation.eulerAngles}");

        // Create anchor
        GameObject anchorGO = new GameObject("DoorAnchor");
        anchorGO.transform.position = hitPose.position;
        anchorGO.transform.rotation = doorRotation;
        anchorGO.AddComponent<ARAnchor>();
        Debug.Log($"[SPAWNER] ✅ Anchor created at {hitPose.position}");

        // Instantiate and scale door
        GameObject door = Instantiate(doorPrefab, hitPose.position, doorRotation);
        door.transform.SetParent(anchorGO.transform, worldPositionStays: true);
        Debug.Log($"[SPAWNER] ✅ Door instantiated at {hitPose.position}");

        Vector3 realSize = ComputeRealSize(box);
        Vector3 modelSize = GetModelSize(door);

        door.transform.localScale = new Vector3(
            realSize.x / modelSize.x,
            realSize.y / modelSize.y,
            modelSize.z / modelSize.z
        );

        Debug.Log($"[SPAWNER] ✅ Door scaled to match YOLO: {door.transform.localScale}");

        LockBottomToGround(door, hitPose.position.y);
        Debug.Log("[SPAWNER] ✅ Door grounded to detected surface");

        spawnedDoorCenters.Add(center);
        Debug.Log($"[SPAWNER] ✅ Door placement complete.\n---");
    }

    Vector3 ComputeRealSize(Rect box)
    {
        List<ARRaycastHit> tmpHits = new();
        Vector2[] corners = {
            new Vector2(box.xMin+2, box.yMax-2),
            new Vector2(box.xMax-2, box.yMax-2),
            new Vector2(box.xMin+2, box.yMin+2),
            new Vector2(box.xMax-2, box.yMin+2)
        };

        List<Vector3> worldPts = new();
        foreach (var c in corners)
        {
            if (raycastManager.Raycast(c, tmpHits, TrackableType.PlaneWithinPolygon))
                worldPts.Add(tmpHits[0].pose.position);
        }

        if (worldPts.Count < 4)
        {
            Debug.LogWarning("[SPAWNER] ❌ Couldn't compute real size — fallback to Vector3.one");
            return Vector3.one;
        }

        float width  = Vector3.Distance(worldPts[0], worldPts[1]);
        float height = Vector3.Distance(worldPts[2], worldPts[3]);
        Debug.Log($"[SPAWNER] ✅ Real size estimated from corners: width={width}, height={height}");
        return new Vector3(width, height, 1f);
    }

    Vector3 GetModelSize(GameObject obj)
    {
        var rends = obj.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
        {
            Debug.LogWarning("[SPAWNER] ⚠️ No renderer found on object to measure size");
            return Vector3.one;
        }

        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);

        Vector3 scale = obj.transform.localScale;
        Vector3 size = new Vector3(
            b.size.x / scale.x,
            b.size.y / scale.y,
            b.size.z / scale.z
        );

        Debug.Log($"[SPAWNER] Model size before scaling: {size}");
        return size;
    }

    void LockBottomToGround(GameObject obj, float groundY)
    {
        var rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("[SPAWNER] ⚠️ Renderer missing during grounding");
            return;
        }

        float bottomY = rend.bounds.min.y;
        obj.transform.position += Vector3.up * (groundY - bottomY);
        Debug.Log($"[SPAWNER] Door adjusted up by {(groundY - bottomY):F3} to sit flush with the ground.");
    }
}
