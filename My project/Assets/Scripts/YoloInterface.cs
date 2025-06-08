using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

[Serializable]
public class DetectionPrefab
{
    public string className;
    public GameObject prefab;
}

public class YoloInterface : MonoBehaviour
{
    public NNModel yoloModelAsset;
    public ARCameraManager arCameraManager;
    public List<DetectionPrefab> detectionPrefabs;
    public event Action<string, Vector2> OnObjectDetected;

    private Dictionary<string, GameObject> classToPrefab;
    private Model runtimeModel;
    private IWorker worker;
    private Texture2D cameraTexture;

    private int frameCount = 0;
    public int skipFrames = 5;

    void Start()
    {
        runtimeModel = ModelLoader.Load(yoloModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        arCameraManager.frameReceived += OnCameraFrameReceived;

        classToPrefab = new();
        foreach (var item in detectionPrefabs)
            classToPrefab[item.className] = item.prefab;
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

        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(320, 320),
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
        // Loop over detections
        for (int i = 0; i < output.shape.channels; i++)
        {
            float confidence = output[0, 0, 4, i];
            if (confidence > 0.6f)
            {
                int classId = Mathf.RoundToInt(output[0, 0, 5, i]);
                string label = classId.ToString(); // Replace with label lookup if needed

                if (classToPrefab.TryGetValue(label, out var prefab))
                {
                    float x = output[0, 0, 0, i];
                    float y = output[0, 0, 1, i];
                    OnObjectDetected?.Invoke(label, new Vector2(x, y));
                }
            }
        }
    }
}
