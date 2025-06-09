using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class DoorSpawner : MonoBehaviour
{
    [Header("Prefab (modeled at real dims, e.g. 1×2×0.05 m)")]
    public GameObject doorPrefab;

    [Header("References")]
    public YoloInterface yolo;
    ARRaycastManager raycastManager;
    ARPlaneManager   planeManager;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager   = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        yolo.OnDoorDetected += PlaceDoor;
    }

    void OnDisable()
    {
        yolo.OnDoorDetected -= PlaceDoor;
    }

    void PlaceDoor(Rect box)
    {
        // helper: raycast a single screen point onto a vertical plane
        Vector3 Sample(Vector2 sp)
        {
            var hits = new List<ARRaycastHit>();
            raycastManager.Raycast(sp, hits, TrackableType.PlaneWithinPolygon);
            return hits[0].pose.position;
        }

        // sample 4 points just inside the box
        Vector2 bl = new Vector2(box.xMin + 2,       box.yMax - 2);
        Vector2 br = new Vector2(box.xMax - 2,       box.yMax - 2);
        Vector2 tl = new Vector2(box.xMin + box.width/2, box.yMin + 2);
        Vector2 tr = new Vector2(box.xMax - 2,       box.yMin + 2);

        Vector3 pL = Sample(bl);
        Vector3 pR = Sample(br);
        Vector3 pT = Sample(tl);
        Vector3 pB = Sample(tr);

        float realWidth  = Vector3.Distance(pL, pR);
        float realHeight = Vector3.Distance(pT, pB);

        // compute center
        Vector3 center = (pL + pR + pT + pB) / 4f;

        // get the plane normal via the same raycast
        var planeHit = new List<ARRaycastHit>();
        raycastManager.Raycast(bl, planeHit, TrackableType.PlaneWithinPolygon);
        ARPlane wall = planeManager.GetPlane(planeHit[0].trackableId);
        Quaternion rot = Quaternion.LookRotation(-wall.transform.up, Vector3.up);

        // instantiate & scale
        GameObject door = Instantiate(doorPrefab, center, rot);
        Vector3 modelSize = GetModelSize(door); // your prefab’s native dims in meters
        door.transform.localScale = new Vector3(
            realWidth  / modelSize.x,
            realHeight / modelSize.y,
            modelSize.z / modelSize.z // keep thickness as modeled
        );
    }

    // Utility to read your prefab’s bounds at unit scale
    Vector3 GetModelSize(GameObject obj)
    {
        var renders = obj.GetComponentsInChildren<Renderer>();
        Bounds b = renders[0].bounds;
        for (int i = 1; i < renders.Length; i++)
            b.Encapsulate(renders[i].bounds);

        // since bounds are in world-space (after scaling),
        // divide by current scale to recover native size:
        Vector3 scale = obj.transform.localScale;
        return new Vector3(
            b.size.x / scale.x,
            b.size.y / scale.y,
            b.size.z / scale.z
        );
    }
}
