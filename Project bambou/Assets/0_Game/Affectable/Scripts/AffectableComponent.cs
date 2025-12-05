using Effect;
using Health;
using Interfaces;
using Skills.Data;
using Stats;
using UnityEngine;

namespace Affectable
{
    public class AffectableComponent : MonoBehaviour, IAffectable
    {
        private IHealthComponent _health;
        private ShieldComponent _shield;

        private void Awake()
        {
            _health = GetComponent<IHealthComponent>();
            _shield = GetComponent<ShieldComponent>();

            if (_health == null)
                Debug.LogError($"{name} has no IHealthComponent but uses IAffectable!");
        }

        public void Damage(HealthEventData data)
        {
            if (data.Amount <= 0f)
                return;

            float dmg = data.Amount;

            if (_shield != null)
                dmg = _shield.AbsorbDamage(dmg);

            if (dmg <= 0f)
                return;

            data.Amount = dmg;
            _health.ApplyDamage(data);
        }
        
        public void Heal(HealthEventData data)
        {
            if (data.Amount <= 0f)
                return;

            _health.ApplyHeal(data);
        }

        public void AddShield(float amount, float duration)
        {
            if (_shield == null)
            {
                Debug.LogWarning($"{name} received a Shield effect but has no ShieldComponent.");
                return;
            }

            _shield.AddShield(amount, duration);
        }

        public void AddBuff(float amount, float duration)
        {
            // Implémenté plus tard
        }

        public void AddDebuff(float amount, float duration)
        {
            // Implémenté plus tard
        }

        public void ApplyTaunt(IAffectable source, float duration)
        {
            // Implémenté plus tard
        }

        public void ApplyDot(EffectData data, IStatsComponent stats, IAffectable source)
        {
            // Implémenté plus tard
        }

        public void ApplyHot(EffectData data, IStatsComponent stats, IAffectable source)
        {
            // Implémenté plus tard
        }

        public void ApplyStun(float duration)
        {
            // Implémenté plus tard
        }

        public void ApplyRoot(float duration)
        {
            // Implémenté plus tard
        }

        public void RemoveDebuffs()
        {
            // Implémenté plus tard
        }

        public void AddDamageTakenModifier(float value, float duration)
        {
            // Implémenté plus tard
        }

        public void Knockback(Vector3 force)
        {
            // Implémenté plus tard
        }
    }
}