using Enemies.Health;
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

        private void Awake()
        {
            _healthComponent = InitComponent<EnemyHealth>();
        }

        public void Damage(HealthEventData healthEventData)
        {
            _healthComponent.ApplyDamage(healthEventData);
        }
    }
}