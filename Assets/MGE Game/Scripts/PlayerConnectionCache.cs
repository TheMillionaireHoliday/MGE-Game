/*
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConnectionCache : NetworkBehaviour
{
    private static Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    private static Dictionary<int, NetworkConnection> connections = new Dictionary<int, NetworkConnection>();

    public static GameObject GetPlayerObject(NetworkConnection connection)
    {
        if (connection == null) return null;
        playerObjects.TryGetValue(connection.ClientId, out GameObject player);
        return player;
    }

    public static GameObject GetPlayerObject(int clientId)
    {
        playerObjects.TryGetValue(clientId, out GameObject player);
        return player;
    }

    public static NetworkConnection GetConnection(int clientId)
    {
        connections.TryGetValue(clientId, out NetworkConnection conn);
        return conn;
    }

    public static bool TryGetPlayer(NetworkConnection connection, out GameObject player)
    {
        return playerObjects.TryGetValue(connection.ClientId, out player);
    }

    // Server only: Register player
    [Server]
    public static void RegisterPlayer(NetworkConnection connection, GameObject player)
    {
        int id = connection.ClientId;
        playerObjects[id] = player;
        connections[id] = connection;
        Debug.Log($"Registered player {id} -> {player.name}");
    }

    [Server]
    public static void UnregisterPlayer(NetworkConnection connection)
    {
        int id = connection.ClientId;
        playerObjects.Remove(id);
        connections.Remove(id);
        Debug.Log($"Unregistered player {id}");
    }

    // Auto-register when player spawns
    public override void OnStartServer()
    {
        base.OnStartServer();
        RegisterPlayer(Owner, gameObject);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        UnregisterPlayer(Owner);
    }
}
*/