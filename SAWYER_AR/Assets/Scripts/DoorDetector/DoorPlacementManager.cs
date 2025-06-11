using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Barracuda;
using TMPro;

/// <summary>
/// Holds one detection result from YOLOv5.
/// </summary>
public struct YoloDetection
{
    public string label;
    public float confidence;
    public float cx, cy, w, h; // normalized center x/y and width/height
}

/// <summary>
/// Static utility class for image conversion, YOLO decoding, and debug drawing.
/// </summary>
public static class YoloUtils
{
   public static class ImageConversionUtility
    {
        public static Texture2D ConvertAndResize(XRCpuImage img, int size)
        {
            XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, img.width, img.height),
                outputDimensions = new Vector2Int(size, size),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            var rawTextureData = new NativeArray<byte>(size * size * 4, Allocator.Temp);
            img.Convert(conversionParams, rawTextureData);

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(rawTextureData);
            tex.Apply();
            rawTextureData.Dispose();
            return tex;
        }
    }

    public static List<YoloDetection> Decode(Tensor output, float threshold, int imageSize)
    {
        List<YoloDetection> results = new List<YoloDetection>();
        for (int i = 0; i < output.shape.batch; i++)
        {
            float conf = output[i, 4];
            if (conf >= threshold)
            {
                results.Add(new YoloDetection
                {
                    label = "door",
                    confidence = conf,
                    cx = output[i, 0],
                    cy = output[i, 1],
                    w  = output[i, 2],
                    h  = output[i, 3]
                });
            }
        }
        return results;
    }

    public static void DrawDebugBoxes(List<YoloDetection> dets, int imageSize)
    {
        foreach (var det in dets)
        {
            float halfW = det.w * Screen.width * 0.5f;
            float halfH = det.h * Screen.height * 0.5f;
            float cx = det.cx * Screen.width;
            float cy = (1f - det.cy) * Screen.height;

            Rect rect = new Rect(cx - halfW, cy - halfH, halfW * 2, halfH * 2);
            DrawRect(rect, Color.green);
        }
    }

    private static void DrawRect(Rect rect, Color color)
    {
        Vector2 topLeft = new Vector2(rect.xMin, rect.yMin);
        Vector2 topRight = new Vector2(rect.xMax, rect.yMin);
        Vector2 bottomRight = new Vector2(rect.xMax, rect.yMax);
        Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMax);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }

    public static Vector2[] GetBoxMidpoints(YoloDetection det, int screenW, int screenH)
    {
        float halfW = det.w * screenW * 0.5f;
        float halfH = det.h * screenH * 0.5f;
        float cx = det.cx * screenW;
        float cy = (1f - det.cy) * screenH;

        Vector2 left  = new Vector2(cx - halfW, cy);
        Vector2 right = new Vector2(cx + halfW, cy);
        Vector2 top   = new Vector2(cx, cy - halfH);
        Vector2 bot   = new Vector2(cx, cy + halfH);
        return new[] { left, right, top, bot };
    }

/// <summary>
/// Manages real-time door detection and placement in AR using YOLOv5 and AR Foundation.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class DoorPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARCameraManager    cameraManager;
    [SerializeField] private ARRaycastManager   raycastManager;
    [SerializeField] private ARAnchorManager    anchorManager;

    [Header("YOLOv5 Model")]
    [SerializeField] private NNModel doorModelAsset;
    [Range(0f, 1f)] [Tooltip("Confidence threshold for detections")] public float detectionThreshold = 0.5f;
    [Tooltip("Model input width & height (e.g. 640)")]
    [SerializeField] private int inputImageSize = 640;

    [Header("Door Quad Prefab")]
    [SerializeField] private GameObject doorQuadPrefab;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugBoxes = true;
    [SerializeField] private RawImage debugRawImage;
    [SerializeField] private TextMeshProUGUI debugText;

    private Model   model;
    private IWorker worker;

    private bool isProcessing = false;
    private Stopwatch stopwatch = new Stopwatch();
    private float lastInferenceMs, lastDecodeMs, lastTotalMs;

    private List<DoorInfo> trackedDoors = new List<DoorInfo>();
    private const float mergeDistance = 0.5f;
    private const int evictionFrameThreshold = 30;
    private int currentFrame = 0;

    void Awake()
    {
        model  = ModelLoader.Load(doorModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
    }

    void OnEnable()  => cameraManager.frameReceived += OnCameraFrame;
    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrame;
        worker?.Dispose();
    }

    void Update()
    {
        currentFrame++;
        // Evict stale doors
        for (int i = trackedDoors.Count - 1; i >= 0; i--)
        {
            if (currentFrame - trackedDoors[i].lastSeenFrame > evictionFrameThreshold)
            {
                // Remove anchor and quad
                anchorManager.TryRemoveAnchor(trackedDoors[i].anchor);
                Destroy(trackedDoors[i].anchor.gameObject);
                Destroy(trackedDoors[i].quad);
                trackedDoors.RemoveAt(i);
            }
        }

        if (showDebugBoxes && debugText)
        {
            debugText.text =
                $"Doors: {trackedDoors.Count}\n" +
                $"Inf: {lastInferenceMs:F1}ms  Dec: {lastDecodeMs:F1}ms  Total: {lastTotalMs:F1}ms";
        }
    }

    private void OnCameraFrame(ARCameraFrameEventArgs args)
    {
        if (isProcessing) return;
        isProcessing = true;

        stopwatch.Restart();
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            isProcessing = false;
            return;
        }

        Texture2D tex = YoloUtils.ImageConversionUtility.ConvertAndResize(image, inputImageSize);
        image.Dispose();
        if (showDebugBoxes && debugRawImage) debugRawImage.texture = tex;

        using (var input = new Tensor(tex, 3))
        {
            Destroy(tex);
            float prepMs = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            worker.Execute(input);
            using (Tensor output = worker.PeekOutput())
            {
                lastInferenceMs = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                var detections = YoloUtils.Decode(output, detectionThreshold, inputImageSize);
                lastDecodeMs = stopwatch.ElapsedMilliseconds;
                lastTotalMs = prepMs + lastInferenceMs + lastDecodeMs;

                if (showDebugBoxes) YoloUtils.DrawDebugBoxes(detections, inputImageSize);

                foreach (var det in detections)
                {
                    if (det.label == "door") HandleDoorDetection(det);
                }
            }
        }

        isProcessing = false;
    }

    private void HandleDoorDetection(YoloDetection det)
    {
        Vector2 screenPt = new Vector2(det.cx * Screen.width, (1f - det.cy) * Screen.height);
        if (!RaycastToWorld(screenPt, out Pose centerPose)) return;

        foreach (var info in trackedDoors)
        {
            if (Vector3.Distance(centerPose.position, info.anchor.transform.position) < mergeDistance)
            {
                info.lastSeenFrame = currentFrame;
                UpdateDoorQuad(info, det);
                return;
            }
        }

        // 1) Create anchor GameObject
        var anchorGO = new GameObject("DoorAnchor");
        anchorGO.transform.SetPositionAndRotation(centerPose.position, centerPose.rotation);
        // 2) Add ARAnchor component
        var anchor = anchorGO.AddComponent<ARAnchor>();
        if (anchor == null) return;

        // 3) Instantiate and track quad
        var quad = Instantiate(doorQuadPrefab, anchor.transform);
        trackedDoors.Add(new DoorInfo(anchor, quad, currentFrame));
        UpdateDoorQuad(trackedDoors[^1], det);
    }

    private void UpdateDoorQuad(DoorInfo info, YoloDetection det)
    {
        Vector2[] pts = YoloUtils.GetBoxMidpoints(det, Screen.width, Screen.height);
        var worldPoses = new List<Pose>();
        foreach (var pt in pts)
        {
            if (RaycastToWorld(pt, out Pose p)) worldPoses.Add(p);
        }
        if (worldPoses.Count < 4) return;

        Vector3 left  = worldPoses[0].position;
        Vector3 right = worldPoses[1].position;
        Vector3 top   = worldPoses[2].position;
        Vector3 bot   = worldPoses[3].position;
        float realW   = Vector3.Distance(left, right);
        float realH   = Vector3.Distance(top, bot);
        Vector3 center= (left + right + top + bot) * 0.25f;
        Vector3 horiz = (right - left).normalized;
        Vector3 normal= Vector3.Cross(Vector3.up, horiz).normalized;

        var t = info.quad.transform;
        t.position   = center;
        t.rotation   = Quaternion.LookRotation(normal, Vector3.up);
        t.localScale = new Vector3(realW, realH, 1f);
    }

    private bool RaycastToWorld(Vector2 screenPt, out Pose pose)
    {
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPt, hits,
            TrackableType.PlaneWithinPolygon | TrackableType.FeaturePoint))
        {
            pose = hits[0].pose;
            return true;
        }
        pose = default;
        return false;
    }

    private class DoorInfo
    {
        public ARAnchor   anchor;
        public GameObject quad;
        public int        lastSeenFrame;
        public DoorInfo(ARAnchor a, GameObject q, int frame)
        {
            anchor = a;
            quad   = q;
            lastSeenFrame = frame;
        }
    }
}
