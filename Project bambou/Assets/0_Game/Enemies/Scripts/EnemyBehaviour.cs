using Data;
using Enemies.Data;
using Enemies.Health;
using Enemies.Lod;
using Entity;
using Health;
using Interfaces;
using Network;
using Stats.Data;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Enemies
{
    public class EnemyBehaviour : EntityBehaviour<EnemyBehaviour>, IAffectable
    {
        public EnemyDataSo  Data { get; private set; }
        
        private NetworkVariable<FixedString64Bytes> _enemyId = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private EnemyHealth _healthComponent;
        private EnemyActivation _activation;

        private GameObject _visual;
        
        private bool _isInitialized;

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void Init(EnemyDataSo data)
        {
            if (IsServer)
            {
                _enemyId.Value = data.id;
                InitRpc(data.id);
            }
        }
        
        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void InitRpc(string id)
        {
            InitEnemy(id);
        }

        private void InitEnemy(string id)
        {
            _healthComponent = InitComponent<EnemyHealth>();
            _activation = InitComponent<EnemyActivation>();

            _healthComponent.OnDeath += OnKill;
            
            var database = GameDatabase.Get<EnemyDatabase>();
            Data = database.GetEnemy(id);
            SpawnVisual(Data);
            
            _isInitialized = true;
        }
        
        private void SpawnVisual(EnemyDataSo data)
        {
            if (_visual != null) return;

            var prefab = data.visualPrefab;
            if (prefab == null)
            {
                Debug.LogError("CharacterVisualPrefab is missing in CharacterData!");
                return;
            }

            _visual = Instantiate(prefab, transform);
        }

        public void Damage(HealthEventData healthEventData)
        {
            _healthComponent.ApplyDamage(healthEventData);
        }

        private void OnKill()
        {
            if (IsServer)
            {
                KillRpc();
                NetworkObjectPool.Instance.Return(NetworkObject);
            }
        }
        
        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void KillRpc()
        { 
            Destroy(_visual);
            
            _healthComponent.OnDeath -= OnKill;
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