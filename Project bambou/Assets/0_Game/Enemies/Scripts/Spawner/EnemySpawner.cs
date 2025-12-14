using System.Collections.Generic;
using Enemies.Data;
using Network;
using Networking;
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
            if (!IsServer)
                return;

            PlayerCharacterManager.OnPlayerSpawned += RegisterPlayer;
            PlayerCharacterManager.OnPlayerUnspawned += UnregisterPlayer;
        }

        private void OnDestroy()
        {
            PlayerCharacterManager.OnPlayerSpawned -= RegisterPlayer;
            PlayerCharacterManager.OnPlayerUnspawned -= UnregisterPlayer;
        }

        #region Public API

        // --------------------------------------------------
        // PLAYER REGISTRATION
        // --------------------------------------------------
        public void RegisterPlayer(GameObject player)
        {
            if (player == null)
                return;

            var t = player.transform;
            if (!_players.Contains(t))
                _players.Add(t);
        }

        public void UnregisterPlayer(GameObject player)
        {
            if (player == null)
                return;

            _players.Remove(player.transform);
        }

        // --------------------------------------------------
        // SPAWN ORIGIN (USED BY WAVES)
        // --------------------------------------------------
        public Vector3 GetSpawnOrigin()
        {
            if (_players.Count == 0)
                return Vector3.zero;

            // Swarm-like: pick a random alive player
            var player = _players[Random.Range(0, _players.Count)];
            return player != null ? player.position : Vector3.zero;
        }

        // --------------------------------------------------
        // SPAWN (PATTERN / FORCED POSITION)
        // --------------------------------------------------
        public void Spawn(EnemyDataSo data, Vector3 position)
        {
            if (!IsServer)
                return;

            var pooled = NetworkObjectPool.Instance.Get(enemyPrefab, position);
            pooled.transform.SetPositionAndRotation(position, Quaternion.identity);

            pooled.GetComponent<EnemyBehaviour>().Init(data);
            EnemyManager.RegisterSpawn();
        }

        // --------------------------------------------------
        // SPAWN (FALLBACK RANDOM)
        // --------------------------------------------------
        public void Spawn(EnemyDataSo data)
        {
            if (!IsServer || _players.Count == 0)
                return;

            var pos = FindValidPosition();
            if (pos == Vector3.zero)
                return;

            Spawn(data, pos);
        }

        #endregion

        #region Position Resolution

        private Vector3 FindValidPosition()
        {
            const int tries = 20;

            var origin = GetSpawnOrigin();
            var pos = RandomPositionAround(origin);

            return pos;
        }

        private Vector3 RandomPositionAround(Vector3 origin)
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var dist = Random.Range(minRadius, maxRadius);

            return origin + new Vector3(
                Mathf.Cos(angle) * dist,
                0f,
                Mathf.Sin(angle) * dist
            );
        }

        #endregion
    }
}
