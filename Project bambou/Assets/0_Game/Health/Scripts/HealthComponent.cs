using System;
using Effect.Stats.Data;
using Health.CombatText;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Health
{
    public class HealthComponent : NetworkBehaviour, IHealthComponent
    {
        [SerializeField] private float baseMaxHealth = 100f;

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<HealthEventData> OnHit;
        public event Action OnDeath;

        private Stats.IStatsComponent _stats;

        private void Awake()
        {
            MaxHealth = baseMaxHealth;
            CurrentHealth = MaxHealth;

            _stats = GetComponent<Stats.IStatsComponent>();
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
            OnHit?.Invoke(data);

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
            OnHit?.Invoke(data);
        }

        public void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
        }
    }
}
