using System;
using Health.CombatText;
using UnityEngine;

namespace Health
{
    public class HealthComponent : MonoBehaviour, IHealthComponent
    {
        [SerializeField] private float maxHealth = 100;

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
                Type = EventType.Physical,
                HitPoint = transform.position,
                Source = gameObject
            });
        }

        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive) return;

            var finalDamage = data.Amount; // placeholder
            CurrentHealth -= finalDamage;

            CombatTextSystem.Instance.DoDamageText(data);
            
            OnHit?.Invoke(data);

            if (CurrentHealth <= 0)
                HandleDeath(data);
        }

        protected virtual void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
            // Base death logic if needed
        }
    }
}