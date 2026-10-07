using System.Collections.Generic;
using UnityEngine;

public class GetFarthestSpawn
{
    [SerializeField]
    private PlayerManager playerManager;

    private List<Transform> spawnPoints;
    public Transform GetSpawnPosition(Transform _playerTransform)
    {
        Transform bestSpawn = spawnPoints[0];
        float maxMinimumDistance = 0f;

        foreach (Transform spawnPoint in spawnPoints)
        {
            float minDistance = float.MaxValue;

            foreach (Transform player in playerManager.GetAllPlayers())
            {
                if (player == _playerTransform)
                    continue;

                float dist = Vector3.Distance(spawnPoint.position, player.position);
                minDistance = Mathf.Min(minDistance, dist);
            }

            if (minDistance > maxMinimumDistance)
            {
                maxMinimumDistance = minDistance;
                bestSpawn = spawnPoint;
            }
        }

        return bestSpawn;
    }

    public Transform GetSpawnPosition(int _clientId) => GetSpawnPosition(playerManager.GetPlayerByID(_clientId));
}
