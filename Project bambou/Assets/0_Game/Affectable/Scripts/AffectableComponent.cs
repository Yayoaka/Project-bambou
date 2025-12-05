using System.Collections;
using Buff;
using Effect;
using Health;
using Interfaces;
using Skills.Data;
using Stats;
using Stats.Data;
using UnityEngine;

namespace Affectable
{
    public class AffectableComponent : MonoBehaviour, IAffectable
    {
        private IHealthComponent _health;
        private ShieldComponent _shield;
        private BuffComponent _buffs;
        private IStatsComponent _stats;

        private bool _isStunned;
        private bool _isRooted;

        private void Awake()
        {
            _health = GetComponent<IHealthComponent>();
            _shield = GetComponent<ShieldComponent>();
            _buffs  = GetComponent<BuffComponent>();
            _stats  = GetComponent<IStatsComponent>();

            if (_health == null)
                Debug.LogError($"{name} has no IHealthComponent but uses IAffectable!");
        }

        // -------------------------------------------------------
        // DAMAGE
        // -------------------------------------------------------
        public void Damage(HealthEventData data)
        {
            float dmg = data.Amount;

            // Shield first
            if (_shield != null)
                dmg = _shield.AbsorbDamage(dmg);

            if (dmg <= 0f)
                return;

            // Mitigation (armor/mr)
            dmg = _stats.ComputeDamageTaken(dmg, data.Type);

            data.Amount = dmg;
            _health.ApplyDamage(data);
        }

        // -------------------------------------------------------
        // HEAL
        // -------------------------------------------------------
        public void Heal(HealthEventData data)
        {
            if (data.Amount > 0f)
                _health.ApplyHeal(data);
        }

        // -------------------------------------------------------
        // SHIELD
        // -------------------------------------------------------
        public void AddShield(float amount, float duration)
        {
            _shield?.AddShield(amount, duration);
        }

        // -------------------------------------------------------
        // BUFFS / DEBUFFS
        // -------------------------------------------------------
        public void AddBuff(StatType stat, float amount, float duration, bool percent = false)
        {
            _buffs?.AddBuff(stat, amount, duration, percent);
        }

        public void AddDebuff(StatType stat, float amount, float duration, bool percent = false)
        {
            // On réutilise le même système (valeur négative = debuff)
            _buffs?.AddBuff(stat, -Mathf.Abs(amount), duration, percent);
        }

        public void RemoveDebuffs()
        {
            // Tu peux plus tard ajouter un Cleanup complet dans BuffComponent
        }

        // -------------------------------------------------------
        // DOT
        // -------------------------------------------------------
        public void ApplyDot(EffectData e, IStatsComponent sourceStats, ulong sourceId)
        {
            StartCoroutine(DotRoutine(e, sourceStats, sourceId));
        }

        private IEnumerator DotRoutine(EffectData e, IStatsComponent sourceStats, ulong sourceId)
        {
            float elapsed = 0f;

            while (elapsed < e.duration)
            {
                yield return new WaitForSeconds(e.tickDelay);
                elapsed += e.tickDelay;

                bool crit = sourceStats.ComputeCrit(e.effectType);
                float dmg = sourceStats.ComputeDamageDealt(e.baseValue, crit);

                var h = new HealthEventData
                {
                    Amount = dmg,
                    Critical = crit,
                    SourceId = sourceId,
                    Type = e.effectType,
                    HitPoint = transform.position
                };

                Damage(h);
            }
        }

        // -------------------------------------------------------
        // HOT
        // -------------------------------------------------------
        public void ApplyHot(EffectData e, IStatsComponent sourceStats, ulong sourceId)
        {
            StartCoroutine(HotRoutine(e, sourceStats, sourceId));
        }

        private IEnumerator HotRoutine(EffectData e, IStatsComponent sourceStats, ulong sourceId)
        {
            float elapsed = 0f;

            while (elapsed < e.duration)
            {
                yield return new WaitForSeconds(e.tickDelay);
                elapsed += e.tickDelay;

                float healAmount = e.baseValue; // AP scaling possible plus tard

                var h = new HealthEventData
                {
                    Amount = healAmount,
                    Critical = false,
                    SourceId = sourceId,
                    Type = EffectType.Heal,
                    HitPoint = transform.position
                };

                Heal(h);
            }
        }

        // -------------------------------------------------------
        // STUN
        // -------------------------------------------------------
        public void ApplyStun(float duration)
        {
            if (_isStunned) return;
            StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            _isStunned = true;
            yield return new WaitForSeconds(duration);
            _isStunned = false;
        }

        // -------------------------------------------------------
        // ROOT
        // -------------------------------------------------------
        public void ApplyRoot(float duration)
        {
            if (_isRooted) return;
            StartCoroutine(RootRoutine(duration));
        }

        private IEnumerator RootRoutine(float duration)
        {
            _isRooted = true;
            yield return new WaitForSeconds(duration);
            _isRooted = false;
        }

        // -------------------------------------------------------
        // TAUNT
        // -------------------------------------------------------
        public void ApplyTaunt(ulong sourceId, float duration)
        {
            // hook AI later
        }

        // -------------------------------------------------------
        // KNOCKBACK
        // -------------------------------------------------------
        public void Knockback(Vector3 force)
        {
            if (TryGetComponent<Rigidbody>(out var rb))
                rb.AddForce(force, ForceMode.Impulse);
        }

        public void AddDamageTakenModifier(float value, float duration)
        {
            // Ce rôle doit être migré dans BuffComponent via StatType.DamageReduction
        }
    }
}
