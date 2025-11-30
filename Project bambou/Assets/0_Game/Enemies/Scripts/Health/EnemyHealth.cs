using System;
using Character;
using Entity;
using Health;
using Health.CombatText;
using Unity.Netcode;
using UnityEngine;

namespace Enemies.Health
{
    public class EnemyHealth : EntityComponent<EnemyBehaviour>, IHealthComponent
    {
        public float MaxHealth { get; } = 10;
        public float CurrentHealth { get; private set; }

        public bool IsAlive => CurrentHealth > 0;

        public event Action OnDeath;
        public event Action<HealthEventData> OnHit;

        public override void Init(EnemyBehaviour owner)
        {
            base.Init(owner);
            
            CurrentHealth = MaxHealth;
        }
        
        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive) return;

            CurrentHealth -= Mathf.Clamp(data.Amount, 0, MaxHealth);
            
            data.HitPoint = transform.position;
            CombatTextSystem.Instance.DoDamageText(data);
            
            OnHit?.Invoke(data);

            if (CurrentHealth <= 0)
                HandleDeath(data);
        }

        public void HandleDeath(HealthEventData data)
        {
            if (!IsServer) return;
            
            SpawnDeathEffectRpc();
            
            OnDeath?.Invoke();
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void SpawnDeathEffectRpc()
        {
            Instantiate(Owner.Data.deathVisual, transform.position, Quaternion.identity);
        }
    }
}