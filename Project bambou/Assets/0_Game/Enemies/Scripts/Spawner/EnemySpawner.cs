using System.Collections.Generic;
using Enemies.Data;
using Network;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Spawner
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Header("Spawn Area")]
        [SerializeField] private float minRadius = 75f;
        [SerializeField] private float maxRadius = 125f;
        [SerializeField] private float minDistanceFromPlayers = 50f;

        private readonly List<Transform> _players = new();

        [SerializeField] private NetworkObject enemyPrefab;

        public override void OnNetworkSpawn()
        {
            PlayerCharacterManager.OnPlayerSpawned += RegisterPlayer;
            PlayerCharacterManager.OnPlayerUnspawned += UnregisterPlayer;
        }
        
        #region Public API

        public void RegisterPlayer(GameObject player)
        {
            if (player == null || _players.Contains(player.transform))
                return;

            _players.Add(player.transform);
        }

        public void UnregisterPlayer(GameObject player)
        {
            if (player == null)
                return;

            _players.Remove(player.transform);
        }

        public void Spawn(EnemyDataSo data)
        {
            if (!IsServer || _players.Count == 0)
                return;

            var pos = FindValidPosition();
            if (pos == Vector3.zero)
                return;

            var pooled = NetworkObjectPool.Instance.Get(enemyPrefab);
            pooled.transform.SetPositionAndRotation(pos, Quaternion.identity);

            pooled.Spawn();
            pooled.GetComponent<EnemyBehaviour>().Init(data);
            EnemyManager.RegisterSpawn();
        }

        #endregion

        #region Position Resolution

        private Vector3 FindValidPosition()
        {
            var tries = 20;

            while (tries-- > 0)
            {
                var pos = RandomPosition();
                if (IsFarEnoughFromAllPlayers(pos))
                    return pos;
            }

            Debug.LogWarning("EnemySpawner: no valid spawn position found.");
            return Vector3.zero;
        }

        private bool IsFarEnoughFromAllPlayers(Vector3 position)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                var player = _players[i];
                if (player == null)
                    continue;

                if (Vector3.Distance(position, player.position) < minDistanceFromPlayers)
                    return false;
            }

            return true;
        }

        private Vector3 RandomPosition()
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var dist = Random.Range(minRadius, maxRadius);

            return new Vector3(
                Mathf.Cos(angle) * dist,
                0f,
                Mathf.Sin(angle) * dist
            );
        }

        #endregion
    }
}
