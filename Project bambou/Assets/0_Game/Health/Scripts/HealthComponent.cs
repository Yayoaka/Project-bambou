using System;
using Enemies;
using Entity;
using Health.CombatText;
using Unity.Netcode;
using UnityEngine;

namespace Health
{
    public class HealthComponent : NetworkBehaviour, IHealthComponent
    {
        [SerializeField] private float maxHealth = 100;

        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public event Action<HealthEventData> OnHit;
        public event Action OnDeath;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
        }

        [ContextMenu("Do Damage")]
        private void DoDamage()
        {
            ApplyDamage(new HealthEventData()
            {
                Amount = 12,
                Critical = false,
                Type = HealthEventType.Physical,
                Source = null
            });
        }

        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive) return;

            var finalDamage = data.Amount;
            CurrentHealth -= finalDamage;

            data.HitPoint = transform.position;
            CombatTextSystem.Instance.DoDamageText(data);
            
            OnHit?.Invoke(data);

            if (CurrentHealth <= 0)
                HandleDeath(data);
        }

        public virtual void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
        }
    }
}