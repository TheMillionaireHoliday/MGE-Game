using FishNet.Component.Spawning;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using Fragsurf.Movement;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnManager : NetworkBehaviour
{
    [SerializeField] private PlayerSpawner playerSpawner;

    private void Awake()
    {
        GlobalEventsManager.Instance.OnPlayerDeath_Server += OnDeath;
        GlobalEventsManager.Instance.OnGameStarted_Server += RespawnAllPlayers;
    }
    private void OnDestroy()
    {
        if (GlobalEventsManager.Instance != null)
        {
            GlobalEventsManager.Instance.OnPlayerDeath_Server -= OnDeath;
            GlobalEventsManager.Instance.OnGameStarted_Server -= RespawnAllPlayers;
        }
    }

    [Server]
    private void OnDeath(PlayerDiedEvent _e) => RespawnPlayer(_e.playerNO);

    [Server]
    public void RespawnPlayer(NetworkObject playerNO)
    {
        var resettableComponents = new List<IResettableServer>();

        playerNO.GetComponentsInChildren<IResettableServer>(resettableComponents);
        foreach (var component in resettableComponents)
        {
            component.ResetStateServer();
        }

        Vector3 position;
        Quaternion rotation;

        playerSpawner.SetSpawnPoint(playerNO.transform, out position, out rotation);

        RespawnPlayerClient(playerNO.Owner, playerNO.transform, position, rotation);
        RespawnPlayerObserver(playerNO);
    }

    [TargetRpc]
    public void RespawnPlayerClient(NetworkConnection conn, Transform playerTransform, Vector3 position, Quaternion rotation)
    {
        playerTransform.position = position;
        playerTransform.GetComponent<NetworkTransform>().Teleport();

        playerTransform.GetComponent<SurfCharacter>().SetVelocity(conn, Vector3.zero);

        playerTransform.GetComponentInChildren<PlayerAiming>().SetRotation(rotation.eulerAngles);
    }

    [ObserversRpc(ExcludeOwner = false)]
    public void RespawnPlayerObserver(NetworkObject player)
    {
        var resettableComponents = new List<IResettableObserver>();

        player.GetComponentsInChildren<IResettableObserver>(resettableComponents);
        foreach (var component in resettableComponents)
        {
            component.ResetStateObserver();
        }
    }

    [Server]
    public void RespawnAllPlayers()
    {
        var foundPlayers = FindObjectsByType<SurfCharacter>(FindObjectsSortMode.None);

        foreach (SurfCharacter player in foundPlayers)
        {
            RespawnPlayer(player);
        }
    }
}