using System;
using Effect.Stats.Data;
using Health.CombatText;
using Interfaces;
using Stats.Data;
using Unity.Netcode;
using UnityEngine;

namespace Health
{
    public class HealthComponent : NetworkBehaviour, IHealthComponent
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action OnHealthChanged;
        public event Action OnDeath;

        private Stats.IStatsComponent _stats;

        private void Awake()
        {
            MaxHealth = -1;
            CurrentHealth = MaxHealth;

            _stats = GetComponent<Stats.IStatsComponent>();
            _stats.OnStatsChanged += UpdateStats;
        }

        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive)
                return;

            float amount = data.Amount;

            if (_stats != null)
            {
                amount = _stats.ComputeDamageTaken(amount, data.Type);
                amount = _stats.ModifyOutgoingEffect(amount, EffectModifierType.IncomingDamage);
            }

            float finalDamage = Mathf.Max(0, amount);
            CurrentHealth -= finalDamage;

            data.Amount = finalDamage;
            data.HitPoint = transform.position;

            CombatTextSystem.Instance.DoText(data);
            OnHealthChanged?.Invoke();

            if (CurrentHealth <= 0f)
                HandleDeath(data);
        }

        public void ApplyHeal(HealthEventData data)
        {
            if (!IsAlive)
                return;

            float healAmount = data.Amount;

            if (_stats != null)
            {
                healAmount = _stats.ModifyOutgoingEffect(healAmount, EffectModifierType.OutgoingHeal);
            }

            healAmount = Mathf.Max(0, healAmount);
            CurrentHealth = Mathf.Min(CurrentHealth + healAmount, MaxHealth);

            data.Amount = healAmount;
            data.HitPoint = transform.position;

            CombatTextSystem.Instance.DoText(data);
            OnHealthChanged?.Invoke();
        }

        private void UpdateStats()
        {
            var maxHealth = _stats.GetStat(StatType.MaxHealth);

            if (Mathf.Approximately(MaxHealth, -1))
                CurrentHealth = maxHealth;
            
            MaxHealth = maxHealth;
        }
        
        public void ResetHealth()
        {
            var maxHealth = _stats.GetStat(StatType.MaxHealth);

            CurrentHealth = maxHealth;
            
            MaxHealth = maxHealth;
        }

        public void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
        }
    }
}
