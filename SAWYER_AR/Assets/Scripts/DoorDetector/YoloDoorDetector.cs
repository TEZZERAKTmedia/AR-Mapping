using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Barracuda;
using Unity.Collections;


public class YoloDoorDetector : MonoBehaviour
{
    public NNModel yoloModelAsset;
    public ARCameraManager arCameraManager;
    public float confidenceThreshold = 0.6f;
    public DoorTrackerManager trackerManager;

    private Model model;
    private IWorker worker;
    private Texture2D cameraTexture;

    void Start()
    {
        model = ModelLoader.Load(yoloModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
        arCameraManager.frameReceived += OnCameraFrame;
    }

    void OnDestroy()
    {
        worker?.Dispose();
        arCameraManager.frameReceived -= OnCameraFrame;
    }

    void OnCameraFrame(ARCameraFrameEventArgs args)
    {
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        var conversion = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(640, 640),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorX
        };

        var rawData = new NativeArray<byte>(conversion.outputDimensions.x * conversion.outputDimensions.y * 3, Allocator.Temp);
        image.Convert(conversion, rawData);
        image.Dispose();

        if (cameraTexture == null)
            cameraTexture = new Texture2D(conversion.outputDimensions.x, conversion.outputDimensions.y, conversion.outputFormat, false);

        cameraTexture.LoadRawTextureData(rawData);
        cameraTexture.Apply();
        rawData.Dispose();

        using var input = new Tensor(cameraTexture, 3);
        worker.Execute(input);
        using var output = worker.PeekOutput();

        ParseYOLO(output);
    }

    void ParseYOLO(Tensor output)
    {
        int rows = output.shape.height;
        int cols = output.shape.width;

        for (int i = 0; i < rows; i++)
        {
            float x = output[i, 0];
            float y = output[i, 1];
            float w = output[i, 2];
            float h = output[i, 3];
            float conf = output[i, 4];
            float cls = output[i, 5]; // assume 0 = door

            float score = conf * cls;
            if (score < confidenceThreshold) continue;

            Rect bbox = new Rect(
                (x - w / 2f) * Screen.width,
                (y - h / 2f) * Screen.height,
                w * Screen.width,
                h * Screen.height
            );

            trackerManager?.TrackDoor(bbox);
        }
    }
}
