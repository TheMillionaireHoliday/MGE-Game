using FishNet.Object;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    public GameObject mainCamera;
    public GameObject viewmodelCamera;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            mainCamera.SetActive(true);
            viewmodelCamera.SetActive(true);
        }
    }
}