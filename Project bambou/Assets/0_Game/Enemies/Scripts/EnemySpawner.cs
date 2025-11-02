using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn settings")]
        [SerializeField] private int _count = 10;
        [SerializeField] private float _radius = 10f;
        [SerializeField] private float _minPlayerDistance = 3f;

        [Header("Timing")]
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField] private float _spawnDelay = 0.1f;

        private List<PlayerEntity> _players = new();
        
        private void Start()
        {
            if (_spawnOnStart)
                StartCoroutine(SpawnRoutine());

            _players = EnemiesManager.Instance.GetPlayers;
        }

        private void OnEnable()
        {
            EnemiesManager.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnDisable()
        {
            EnemiesManager.OnPlayerSpawned -= OnPlayerSpawned;
        }

        #region Enemies Spawn
        
        private System.Collections.IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(_spawnDelay);
            
            for (int i = 0; i < _count; i++)
            {
                SpawnOne();
                yield return new WaitForSeconds(_spawnDelay);
            }
        }

        private void SpawnOne()
        {
            // Random position around this spawner
            var pos = GetValidSpawnPosition();

            var enemy = EnemiesPool.Instance.Get();
            enemy.transform.SetPositionAndRotation(pos, Quaternion.identity);
        }
        
        private Vector3 GetValidSpawnPosition()
        {
            const int maxAttempts = 20;

            for (int i = 0; i < maxAttempts; i++)
            {
                var pos = transform.position + Random.insideUnitSphere * _radius;
                pos.y = transform.position.y;

                var tooClose = false;
                foreach (var p in _players)
                {
                    if (p == null) continue;
                    float dist = Vector3.Distance(pos, p.transform.position);
                    if (dist < _minPlayerDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                    return pos;
            }

            // fallback (if all tries failed)
            return transform.position;
        }

        #endregion
        
        #region Events

        private void OnPlayerSpawned(PlayerEntity player)
        {
            _players.Add(player);
        }
        
        #endregion
        
        #if UNITY_EDITOR
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _radius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _minPlayerDistance);
        }
        
        #endif
    }
}
