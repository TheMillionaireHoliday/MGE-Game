using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    private Dictionary<int, Transform> playersDict = new Dictionary<int, Transform>();
    private List<Transform> playersList = new List<Transform>();

    // Public methods
    public Transform GetPlayerByID(int clientId)
    {
        playersDict.TryGetValue(clientId, out Transform player);
        return player;
    }

    public List<Transform> GetAllPlayers() => playersList;

    [Server]
    public void AddPlayer(int clientId, Transform player)
    {
        playersDict[clientId] = player;
        playersList.Add(player);
    }

    [Server]
    public void RemovePlayer(int clientId)
    {
        if (playersDict.TryGetValue(clientId, out Transform player))
        {
            playersDict.Remove(clientId);
            playersList.Remove(player);
        }
    }
}
