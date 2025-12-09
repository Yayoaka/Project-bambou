using Collectibles;
using Data;
using Enemies.AI;
using Enemies.Data;
using Enemies.Lod;
using Entity;
using Health;
using Network;
using Stats;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Enemies
{
    public class EnemyBehaviour : EntityBehaviour<EnemyBehaviour>
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
            Stats = GetComponent<IStatsComponent>();
            _health = GetComponent<HealthComponent>();
            _activation = InitComponent<EnemyActivation>();
            ai = InitComponent<EnemyAI>();
            
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

            var xpNet = NetworkObjectPool.Instance.Get(Data.xpLoot);
            
            xpNet.GetComponent<XpCollectible>().Init(Data.xpAmount);
            
            xpNet.transform.position = transform.position;
            
            KillRpc();

            NetworkObjectPool.Instance.Return(NetworkObject);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void KillRpc()
        {
            if (Data.deathVisual != null)
                Instantiate(Data.deathVisual, transform.position, Quaternion.identity);

            if (_visual != null)
                Destroy(_visual);

            _health.OnDeath -= OnKill;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer && !_isInitialized)
            {
                InitEnemy(_enemyId.Value.Value);
            }
        }
    }
}
