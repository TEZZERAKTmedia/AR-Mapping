using Unity.Barracuda;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class YoloInterface : MonoBehaviour
{
    public NNModel yoloModelAsset;
    public ARCameraManager arCameraManager;

    private Model runtimeModel;
    private IWorker worker;
    private Texture2D cameraTexture;

    void Start()
    {
        runtimeModel = ModelLoader.Load(yoloModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDestroy()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
        worker.Dispose();
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        // Convert to Texture2D
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(640, 640),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorX
        };

        int dataSize = conversionParams.outputDimensions.x * conversionParams.outputDimensions.y * 3;
        var rawTextureData = new Unity.Collections.NativeArray<byte>(dataSize, Unity.Collections.Allocator.Temp);
        image.Convert(conversionParams, rawTextureData);

        image.Dispose();

        if (cameraTexture == null)
            cameraTexture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);

        cameraTexture.LoadRawTextureData(rawTextureData);
        cameraTexture.Apply();

        RunModel(cameraTexture);
    }

    void RunModel(Texture2D texture)
    {
        using var input = new Tensor(texture, 3);
        worker.Execute(input);
        using var output = worker.PeekOutput();

        ParseYOLOOutput(output);
    }

    void ParseYOLOOutput(Tensor output)
    {
        // TODO: Process output (shape should be [1, 25200, 6] for YOLOv5)
        // Format: [x_center, y_center, width, height, confidence, class_score]
        Debug.Log($"Output Tensor shape: {output.shape}");
    }
}
