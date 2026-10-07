using FishNet.Object;
using UnityEngine;

public class PlayerWorldModelManager : NetworkBehaviour
{
    public MeshRenderer meshRenderer;
    public override void OnStartClient()
    {
        if (IsOwner)
            meshRenderer.enabled = false;
    }
}
