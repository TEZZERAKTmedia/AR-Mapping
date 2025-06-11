using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Barracuda;

[RequireComponent(typeof(ARRaycastManager))]
public class DoorPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;

    [Header("YOLOv5 Model")]
    [SerializeField] private NNModel doorModelAsset;
    [Range(0f, 1f)] public float detectionThreshold = 0.5f;

    [Header("Door Quad Prefab")]
    [SerializeField] private GameObject doorQuadPrefab;

    // internal Barracuda
    private Model model;
    private IWorker worker;

    // tracked doors
    private List<DoorInfo> trackedDoors = new List<DoorInfo>();
    private const float mergeDistance = 0.5f; // meters

    void Awake()
    {
        // load Barracuda model
        model = ModelLoader.Load(doorModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrame;
    }
    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrame;
        worker?.Dispose();
    }

    void OnCameraFrame(ARCameraFrameEventArgs args)
    {
        // 1. Acquire camera image
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        // 2. Convert image to Texture2D & create input Tensor
        // (Utility method needed for conversion)
        Texture2D tex = image.ConvertToRGBA32();
        image.Dispose();
        var input = new Tensor(tex, channels:3);
        Destroy(tex);

        // 3. Run model
        worker.Execute(input);
        Tensor output = worker.PeekOutput();
        input.Dispose();

        // 4. Decode detections
        var detections = YoloUtils.DecodeBoxes(output, detectionThreshold);
        output.Dispose();

        // 5. Process each detection
        foreach (var det in detections)
        {
            if (det.label != "door") continue;
            HandleDoorDetection(det);
        }
    }

    void HandleDoorDetection(YoloDetection det)
    {
        // screen-space box in pixels
        Rect box = det.box;
        Vector2 center = box.center;

        // 5a. Center raycast
        if (!RaycastToWorld(center, out Pose centerPose))
            return;

        // 5b. Check clustering: same door?
        foreach (var info in trackedDoors)
        {
            float dist = Vector3.Distance(centerPose.position, info.anchor.transform.position);
            if (dist < mergeDistance)
            {
                // update existing door
                UpdateDoorQuad(info, box);
                return;
            }
        }

        // 5c. New door: create anchor and quad
        ARAnchor anchor = anchorManager.AddAnchor(centerPose);
        if (anchor == null) return;

        GameObject quad = Instantiate(doorQuadPrefab, anchor.transform);
        DoorInfo newInfo = new DoorInfo(anchor, quad);
        trackedDoors.Add(newInfo);
        UpdateDoorQuad(newInfo, box);
    }

    void UpdateDoorQuad(DoorInfo info, Rect box)
    {
        // compute 4 edge midpoints
        Vector2 leftMid   = new Vector2(box.xMin, box.center.y);
        Vector2 rightMid  = new Vector2(box.xMax, box.center.y);
        Vector2 topMid    = new Vector2(box.center.x, box.yMin);
        Vector2 bottomMid = new Vector2(box.center.x, box.yMax);

        if (!RaycastToWorld(leftMid, out Pose leftPose) ||
            !RaycastToWorld(rightMid, out Pose rightPose) ||
            !RaycastToWorld(topMid, out Pose topPose) ||
            !RaycastToWorld(bottomMid, out Pose bottomPose))
            return;

        // measurements
        float realWidth  = Vector3.Distance(leftPose.position, rightPose.position);
        float realHeight = Vector3.Distance(topPose.position, bottomPose.position);
        Vector3 realCenter =
            (leftPose.position + rightPose.position + topPose.position + bottomPose.position) * 0.25f;

        // normal from horizontal edge
        Vector3 horiz = (rightPose.position - leftPose.position).normalized;
        Vector3 normal = Vector3.Cross(Vector3.up, horiz).normalized;

        // apply transform & scale
        Transform quadTransform = info.quad.transform;
        quadTransform.position = realCenter;
        quadTransform.rotation = Quaternion.LookRotation(normal, Vector3.up);
        quadTransform.localScale = new Vector3(realWidth, realHeight, 1f);
    }

    bool RaycastToWorld(Vector2 screenPt, out Pose pose)
    {
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPt, hits,
            TrackableType.PlaneEstimated | TrackableType.FeaturePoint))
        {
            pose = hits[0].pose;
            return true;
        }
        pose = default;
        return false;
    }

    // helper struct
    class DoorInfo
    {
        public ARAnchor anchor;
        public GameObject quad;
        public DoorInfo(ARAnchor a, GameObject q) { anchor = a; quad = q; }
    }

    // placeholder for YOLO detection data
    struct YoloDetection
    {
        public string label;
        public float confidence;
        public Rect box;
    }

    static class YoloUtils
    {
        public static List<YoloDetection> DecodeBoxes(Tensor output, float threshold)
        {
            // TODO: implement YOLOv5 output decoding (NMS, box conversion)
            return new List<YoloDetection>();
        }
    }
}
