using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

[RequireComponent(typeof(ARMeshManager))]
public class MeshToBoxVisualizer : MonoBehaviour
{
    private ARMeshManager meshManager;
    private Dictionary<MeshFilter, GameObject> meshBoxes = new Dictionary<MeshFilter, GameObject>();

    public Material boxMaterial;

    void Awake()
    {
        meshManager = GetComponent<ARMeshManager>();
        meshManager.meshesChanged += OnMeshesChanged;
    }

    void OnDestroy()
    {
        meshManager.meshesChanged -= OnMeshesChanged;
    }

    void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        foreach (var meshFilter in args.added)
        {
            CreateBox(meshFilter);
        }

        foreach (var meshFilter in args.updated)
        {
            UpdateBox(meshFilter);
        }
    }

    void CreateBox(MeshFilter mf)
    {
        var bounds = mf.mesh.bounds;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(mf.transform, false);
        go.transform.localPosition = bounds.center;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = bounds.size;
        go.GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;

        meshBoxes[mf] = go;
    }

    void UpdateBox(MeshFilter mf)
    {
        if (!meshBoxes.TryGetValue(mf, out GameObject box)) return;

        var bounds = mf.mesh.bounds;
        box.transform.localPosition = bounds.center;
        box.transform.localScale = bounds.size;
    }
}

