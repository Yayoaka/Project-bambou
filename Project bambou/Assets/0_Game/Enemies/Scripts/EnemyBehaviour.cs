using System.Collections;
using System.Collections.Generic;
using Collectibles;
using Data;
using Enemies.AI;
using Enemies.Data;
using Enemies.Lod;
using Entity;
using Health;
using Interfaces;
using Network;
using Stats;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;

namespace Enemies
{
    public class EnemyBehaviour : EntityBehaviour<EnemyBehaviour>, INetworkPoolable
    {
        public EnemyDataSo Data { get; private set; }

        private NetworkVariable<FixedString64Bytes> _enemyId = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public IStatsComponent Stats;
        public EnemyAI ai;
        private HealthComponent _health;
        private EnemyActivation _activation;

        private GameObject _visual;
        private bool _isInitialized;

        public void Init(EnemyDataSo data)
        {
            if (!IsServer) return;

            _enemyId.Value = data.id;
            InitRpc(data.id);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void InitRpc(string id)
        {
            InitEnemy(id);
        }

        private void InitEnemy(string id)
        {
            if (_isInitialized) return;
            Stats ??= GetComponent<IStatsComponent>();
            _health ??= GetComponent<HealthComponent>();
            _activation ??= InitComponent<EnemyActivation>();
            ai ??= InitComponent<EnemyAI>();
            
            _activation.LateInit();
            ai.LateInit();
            
            _health.OnDeath += OnKill;
            
            var database = GameDatabase.Get<EnemyDatabase>();
            Data = database.GetEnemy(id);

            SpawnVisual(Data);

            _isInitialized = true;
        }

        private void SpawnVisual(EnemyDataSo data)
        {
            if (_visual != null) return;

            if (data.visualPrefab == null)
            {
                Debug.LogError("Missing visualPrefab in EnemyData!");
                return;
            }

            _visual = Instantiate(data.visualPrefab, transform);
            Stats.SetStats(data.stats);
        }

        private void OnKill()
        {
            if (!IsServer) return;
            
            SpawnXp();
            
            KillRpc();
            
            EnemyManager.RegisterDeath();
            
            NetworkObjectPool.Instance.Return(NetworkObject);
        }

        private void SpawnXp()
        {
            var xpNet = NetworkObjectPool.Instance.Get(Data.xpLoot, transform.position);

            var collectible = xpNet.GetComponent<XpCollectible>();
            collectible.Init(Data.xpAmount);
        }
        
        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void KillRpc()
        {
            if (_visual != null)
            {
                Destroy(_visual);
                _visual = null;
            }

            if (Data != null && Data.deathVisual != null)
                Instantiate(Data.deathVisual, transform.position, Quaternion.identity);

            _health.OnDeath -= OnKill;
        }

        public override void OnNetworkSpawn()
        {
            if (!_isInitialized && !_enemyId.Value.Value.IsNullOrEmpty())
            {
                InitEnemy(_enemyId.Value.Value);
            }
        }
        
        public void OnPoolAcquire()
        {
            gameObject.SetActive(true);
            
            if (_health) _health.enabled = true;
            if (ai) ai.enabled = true;
            if (_activation) _activation.enabled = true;

            if (_visual != null)
                _visual.SetActive(true);
        }

        public void OnPoolRelease()
        {
            gameObject.SetActive(false);
            
            if (_health) _health.enabled = false;
            if (ai) ai.enabled = false;
            if (_activation) _activation.enabled = false;

            if (_visual != null)
                _visual.SetActive(false);
        }
    }
}
