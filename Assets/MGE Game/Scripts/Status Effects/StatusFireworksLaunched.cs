using FishNet;
using FishNet.Object;
using UnityEngine;

public class StatusFireworksLaunched : StatusEffect
{
    public const string EffectName = "Status Fireworks Launched";

    private GameObject statusNOprefab;
    private NetworkObject statusNO;
    private StatusFireworksLaunchedExternal statusExternal;

    private bool cancelledFromExternal = false;

    public StatusFireworksLaunched(GameObject effectPrefab)
    {
        statusNOprefab = effectPrefab;
    }

    public override void Apply(StatusEffectTarget target)
    {
        var obj = GameObject.Instantiate(statusNOprefab);
        statusNO = obj.GetComponent<NetworkObject>();
        InstanceFinder.NetworkManager.ServerManager.Spawn(statusNO);

        statusExternal = statusNO.GetComponent<StatusFireworksLaunchedExternal>();

        statusExternal.AddStatus(target);

        statusExternal.OnExternalRemoveRequest += RemoveFromExternal;

        base.Apply(target);
    }
    public override void Remove()
    {
        if (markedForRemoval)
            return;

        statusExternal.OnExternalRemoveRequest -= RemoveFromExternal;

        if (!cancelledFromExternal)
            statusExternal.RemoveStatusServer();

        base.Remove();
    }

    public void RemoveFromExternal()
    {
        cancelledFromExternal = true;
        Remove();
    }

    public override void Update()
    {

    }

    public override string GetEffectName()
    {
        return EffectName;
    }
}