using FishNet.Connection;
using FishNet.Object;
using Fragsurf.Movement;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StatusFireworksLaunchedExternal : NetworkBehaviour
{
    [Header("Launched player settings")]
    private float launchedPlayerDamageMultiplier = 3.0f;
    private float launchedPlayerMinActiveTime = 0.1f;

    private NetworkObject targetServer; // Access only from server
    private NetworkObject targetClient; // Access only from client

    [SerializeField] private GameObject launchedPlayerTrailPrefab;
    private GameObject currentTrail;

    private CancellationTokenSource ctsServer;
    private CancellationTokenSource ctsClient;

    private PlayerHealth healthServer;
    private SurfCharacter characterClient;

    public Action OnExternalRemoveRequest;

    [Server]
    public void AddStatus(NetworkObject _target) => AddStatusTask(_target).Forget();
    private async UniTaskVoid AddStatusTask(NetworkObject _target)
    {
        // ============ Adding

        targetServer = _target;
        healthServer = targetServer.GetComponent<PlayerHealth>();
        healthServer.externalDamageMultiplier *= launchedPlayerDamageMultiplier;

        AddEffectsObserver(targetServer);
        AddEffectsServer();

        ctsServer = new CancellationTokenSource();
        await StatusEffectLifecycle(ctsServer.Token);
        ctsServer.Dispose();

        // ============ Removing

        healthServer.externalDamageMultiplier /= launchedPlayerDamageMultiplier;
        RemoveEffectsServer();
        RemoveEffectsObserver();
        WaitingForPlayerToTouchGroundCancel(_target.Owner);
        OnExternalRemoveRequest?.Invoke();

        Despawn(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveStatusServerRequest() => RemoveStatusServer();

    public void RemoveStatusServer()
    {
        try
        {
            ctsServer?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Token source was already disposed, ignore
        }

    }

    public void RemoveStatusFromInternal()
    {
        ctsServer?.Cancel();
        ctsClient?.Cancel();
    }

    [Server]
    private async UniTask StatusEffectLifecycle(CancellationToken cancellationToken)
    {
        // ======== Min period =========

        float elapsedTime = 0f;
        while (elapsedTime < launchedPlayerMinActiveTime)
        {
            if (cancellationToken.IsCancellationRequested) return;
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // ======== While in air ========

        TargetStartEndCondition(targetServer.Owner, targetServer);

        while (true)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await UniTask.Yield();
        }
    }

    void AddEffectsServer()
    {
        /*var trailGO = Instantiate(launchedPlayerTrailPrefab);

        currentTrail = trailGO.GetComponent<NetworkObject>();

        currentTrail.transform.SetParent(transform, false);

        currentTrail.transform.localPosition = Vector3.zero;

        Spawn(currentTrail, targetClient.Owner);*/
    }

    void RemoveEffectsServer()
    {
        //Despawn(currentTrail);
    }

    // ============================================================== 
    // ============================================================== CLIENT
    // ==============================================================

    [ObserversRpc]
    void AddEffectsObserver(NetworkObject target)
    {
        currentTrail = Instantiate(launchedPlayerTrailPrefab);

        var trailScript = currentTrail.GetComponent<LaunchedPlayerTrail>();

        trailScript.Initialize(target.OwnerId, target);
    }

    [ObserversRpc]
    void RemoveEffectsObserver()
    {
        Destroy(currentTrail);
    }

    [TargetRpc]
    public void TargetStartEndCondition(NetworkConnection conn, NetworkObject obj) => TargetStartEndConditionTask(conn, obj).Forget();

    private bool interruptedClientCheck = true;

    private async UniTaskVoid TargetStartEndConditionTask(NetworkConnection conn, NetworkObject obj)
    {
        targetClient = obj;
        characterClient = obj.GetComponent<SurfCharacter>();

        ctsClient = new CancellationTokenSource();
        await WaitingForPlayerToTouchGround(ctsClient.Token);
        ctsClient.Dispose();

        if (!interruptedClientCheck)
            RemoveStatusServerRequest();
    }


    private async UniTask WaitingForPlayerToTouchGround(CancellationToken cancellationToken)
    {
        while (characterClient.groundObject == null)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await UniTask.Yield();
        }

        interruptedClientCheck = false;
    }

    [TargetRpc]
    public void WaitingForPlayerToTouchGroundCancel(NetworkConnection conn)
    {
        try
        {
            ctsClient.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Token source was already disposed, ignore
        }
    }
}
