using System;
using Health;
using Health.CombatText;
using UnityEngine;

namespace Character
{
    public class CharacterHealth : CharacterComponent, IHealthComponent
    {
        private float _maxHealth = 100;

        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public event Action<HealthEventData> OnHit;
        public event Action OnDeath;
        
        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive) return;

            var finalDamage = CharacterBehaviour.Stats.ComputeReceivedStat(data.Amount, data.Type);

            CurrentHealth -= Mathf.Clamp(finalDamage, 0, _maxHealth);

            data.HitPoint = transform.position;
            CombatTextSystem.Instance.DoDamageText(data);
            
            OnHit?.Invoke(data);

            if (CurrentHealth <= 0)
                HandleDeath(data);
        }
        
        protected virtual void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
        }
    }
}