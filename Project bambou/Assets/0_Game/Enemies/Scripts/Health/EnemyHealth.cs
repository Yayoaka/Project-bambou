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

            //var finalDamage = EntityBehaviour.healthComponent.Stats.ComputeReceivedStat(data.Amount, data.Type);

            //CurrentHealth -= Mathf.Clamp(finalDamage, 0, MaxHealth);
            
            data.HitPoint = transform.position;
            CombatTextSystem.Instance.DoDamageText(data);
            
            OnHit?.Invoke(data);

            if (CurrentHealth <= 0)
                HandleDeath(data);
        }

        public void HandleDeath(HealthEventData data)
        {
            throw new NotImplementedException();
        }
    }
}