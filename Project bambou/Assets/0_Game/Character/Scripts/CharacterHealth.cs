using System;
using Entity;
using Health;
using Health.CombatText;
using UnityEngine;

namespace Character
{
    public class CharacterHealth : EntityComponent<CharacterBehaviour>, IHealthComponent
    {
        public float MaxHealth { get; private set; }

        public float CurrentHealth { get; private set; }

        public bool IsAlive => CurrentHealth > 0;

        public event Action<HealthEventData> OnHit;
        public event Action OnDeath;
        
        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive) return;

            var finalDamage = Owner.Stats.ComputeReceivedStat(data.Amount, data.Type);

            CurrentHealth -= Mathf.Clamp(finalDamage, 0, MaxHealth);

            data.HitPoint = transform.position;
            CombatTextSystem.Instance.DoDamageText(data);
            
            OnHit?.Invoke(data);

            if (CurrentHealth <= 0)
                HandleDeath(data);
        }
        
        public void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
        }
    }
}