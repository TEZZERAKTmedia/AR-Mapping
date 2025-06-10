using System;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class YoloInterface : MonoBehaviour
{
    [Header("Model & Camera")]
    public NNModel yoloModelAsset;
    public ARCameraManager arCameraManager;

    [Header("Detection Settings")]
    [Tooltip("Number of frames to skip between inferences.")]
    public int skipFrames = 5;
    [Tooltip("Minimum confidence to accept a detection.")]
    [Range(0f, 1f)]
    public float confidenceThreshold = 0.6f;

    public event Action<Rect> OnDoorDetected;

    private Model runtimeModel;
    private IWorker worker;
    private Texture2D cameraTexture;
    private int frameCount = 0;

    void Start()
    {
        Debug.Log("[YOLO] Initializing model...");
        try
        {
            runtimeModel = ModelLoader.Load(yoloModelAsset);
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
            arCameraManager.frameReceived += OnCameraFrameReceived;
            Debug.Log("[YOLO] Model loaded and worker created successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[YOLO] Failed to initialize model: {e.Message}");
        }
    }

    void OnDestroy()
    {
        Debug.Log("[YOLO] Cleaning up model worker and unsubscribing from camera frames...");
        arCameraManager.frameReceived -= OnCameraFrameReceived;
        worker?.Dispose();
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        frameCount++;
        if (frameCount % skipFrames != 0) return;
        frameCount = 0;

        Debug.Log("[YOLO] Acquiring CPU image...");
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Debug.LogWarning("[YOLO] Failed to acquire camera image.");
            return;
        }

        var conv = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(640, 640),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorX
        };

        int dataSize = conv.outputDimensions.x * conv.outputDimensions.y * 3;
        var rawData = new NativeArray<byte>(dataSize, Allocator.Temp);

        try
        {
            image.Convert(conv, rawData);
        }
        catch (Exception e)
        {
            Debug.LogError($"[YOLO] Failed image conversion: {e.Message}");
            rawData.Dispose();
            image.Dispose();
            return;
        }

        image.Dispose();

        if (cameraTexture == null)
        {
            cameraTexture = new Texture2D(conv.outputDimensions.x, conv.outputDimensions.y, conv.outputFormat, false);
            Debug.Log("[YOLO] Camera texture created.");
        }

        cameraTexture.LoadRawTextureData(rawData);
        cameraTexture.Apply();
        rawData.Dispose();

        Debug.Log("[YOLO] Running model on current frame...");
        RunModel(cameraTexture);
    }

    void RunModel(Texture2D tex)
    {
        using var input = new Tensor(tex, 3);
        try
        {
            worker.Execute(input);
            using var output = worker.PeekOutput();
            Debug.Log("[YOLO] Output tensor received. Beginning parse...");
            ParseYOLOOutput(output);
        }
        catch (Exception e)
        {
            Debug.LogError($"[YOLO] Model execution error: {e.Message}");
        }
    }

    void ParseYOLOOutput(Tensor output)
    {
        int rows = output.shape.height;    // typically 25200
        int cols = output.shape.width;     // typically 6

        Debug.Log($"[YOLO] Output tensor shape: ({rows}, {cols})");

        if (cols != 6)
        {
            Debug.LogError($"[YOLO] Unexpected output shape: expected (25200, 6), got ({rows}, {cols})");
            return;
        }

        bool found = false;

        for (int i = 0; i < rows; i++)
        {
            float x = output[i, 0] / cameraTexture.width;
            float y = output[i, 1] / cameraTexture.height;
            float w = output[i, 2] / cameraTexture.width;
            float h = output[i, 3] / cameraTexture.height;

            float conf = output[i, 4];
            float classScore = output[i, 5];
            float score = conf * classScore;

            Debug.Log($"[YOLO] Normalized: x={x:F2}, y={y:F2}, w={w:F2}, h={h:F2}, score={score:F2}");

            if (score < confidenceThreshold)
                continue;

            if (LookupLabel(0) != "door")
                continue;

            Rect screenRect = new Rect(
                (x - w / 2f) * Screen.width,
                (y - h / 2f) * Screen.height,
                w * Screen.width,
                h * Screen.height
            );

            Debug.Log($"[YOLO] ✅ Door detected at {screenRect}, score={score:F2}");
            OnDoorDetected?.Invoke(screenRect);
            break;

        }

        if (!found)
        {
            Debug.Log("[YOLO] ❌ No valid door detections this frame.");
        }
    }

    string LookupLabel(int classId)
    {
        return classId == 0 ? "door" : "";
    }
}
