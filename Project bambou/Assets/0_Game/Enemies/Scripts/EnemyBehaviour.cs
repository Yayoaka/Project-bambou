using Enemies.Data;
using Enemies.Health;
using Enemies.Lod;
using Entity;
using Health;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Enemies
{
    public class EnemyBehaviour : EntityBehaviour<EnemyBehaviour>, IAffectable
    {
        private EnemyHealth _healthComponent;
        private EnemyActivation _activation;

        private GameObject _visual;

        private void Awake()
        {
            _healthComponent = InitComponent<EnemyHealth>();
            _activation = InitComponent<EnemyActivation>();
        }

        public void Init(EnemyDataSO data)
        {
            SpawnVisual(data);
        }
        
        private void SpawnVisual(EnemyDataSO data)
        {
            if (_visual != null) return;

            var prefab = data.VisualPrefab;
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
    }
}