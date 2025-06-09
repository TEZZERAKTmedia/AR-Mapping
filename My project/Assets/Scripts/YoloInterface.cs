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
        // Load and prepare the Barracuda model
        runtimeModel = ModelLoader.Load(yoloModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);

        // Subscribe to camera frames
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDestroy()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
        worker.Dispose();
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (++frameCount % skipFrames != 0) return;
        frameCount = 0;

        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        // Convert to a small RGB24 Texture2D
        var conv = new XRCpuImage.ConversionParams
        {
            inputRect       = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(320, 320),
            outputFormat     = TextureFormat.RGB24,
            transformation   = XRCpuImage.Transformation.MirrorX
        };
        int dataSize = conv.outputDimensions.x * conv.outputDimensions.y * 3;
        var rawData = new NativeArray<byte>(dataSize, Allocator.Temp);
        image.Convert(conv, rawData);
        image.Dispose();

        if (cameraTexture == null)
            cameraTexture = new Texture2D(conv.outputDimensions.x, conv.outputDimensions.y, conv.outputFormat, false);

        cameraTexture.LoadRawTextureData(rawData);
        cameraTexture.Apply();
        rawData.Dispose();

        RunModel(cameraTexture);
    }

    void RunModel(Texture2D tex)
    {
        using var input = new Tensor(tex, 3);
        worker.Execute(input);
        using var output = worker.PeekOutput();
        ParseYOLOOutput(output);
    }

    void ParseYOLOOutput(Tensor output)
    {
        // Assuming output shape: [1,1,channels,detCount]
        int detCount = output.shape.channels;
        for (int i = 0; i < detCount; i++)
        {
            float conf = output[0, 0, 4, i];
            if (conf < confidenceThreshold)
                continue;

            int classId = Mathf.RoundToInt(output[0, 0, 5, i]);
            if (LookupLabel(classId) != "door")
                continue;

            // normalized bbox center & size
            float cx = output[0, 0, 0, i];
            float cy = output[0, 0, 1, i];
            float w  = output[0, 0, 2, i];
            float h  = output[0, 0, 3, i];

            // convert to screen-pixel Rect
            Rect screenRect = new Rect(
                (cx - w/2f) * Screen.width,
                (cy - h/2f) * Screen.height,
                w * Screen.width,
                h * Screen.height
            );

            OnDoorDetected?.Invoke(screenRect);
            break; // only handle the first door per frame
        }
    }

    // Replace this with your own mapping from classId → string label
    string LookupLabel(int classId)
    {
        // e.g. 0 = door, 1 = window, etc.
        return classId == 0 ? "door" : "";
    }
}
