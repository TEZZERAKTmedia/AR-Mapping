using Unity.Barracuda;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

public class YoloInterface : MonoBehaviour
{
    public NNModel yoloModelAsset;
    public ARCameraManager arCameraManager;

    private Model runtimeModel;
    private IWorker worker;
    private Texture2D cameraTexture;

    private int frameCount = 0;
    public int skipFrames = 5; // YOLO runs once every 5 frames

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
        frameCount++;
        if (frameCount % skipFrames != 0)
            return;

        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(320, 320), // ⚠️ Consider reducing this for performance
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorX
        };

        int dataSize = conversionParams.outputDimensions.x * conversionParams.outputDimensions.y * 3;
        var rawTextureData = new NativeArray<byte>(dataSize, Allocator.Temp);
        image.Convert(conversionParams, rawTextureData);
        image.Dispose();

        if (cameraTexture == null)
            cameraTexture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);

        cameraTexture.LoadRawTextureData(rawTextureData);
        cameraTexture.Apply();
        rawTextureData.Dispose();

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
        // Example output: [1, 1, 6, 25200]
        Debug.Log($"Output Tensor shape: {output.shape}");
    }
}
