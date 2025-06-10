using UnityEngine;

public class DoorOverlay : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform); // face user
    }
}
