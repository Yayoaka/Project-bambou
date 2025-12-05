using Effect;
using Health;
using UnityEngine;
using Stats.Data;
using Skills.Data;
using Stats;

namespace Interfaces
{
    public interface IAffectable
    {
        // Damage / Heal
        void Damage(HealthEventData data);
        void Heal(HealthEventData data);

        // Shield
        void AddShield(float amount, float duration);

        // Buff / Debuff (nouvelle signature)
        void AddBuff(StatType stat, float amount, float duration, bool percent = false);
        void AddDebuff(StatType stat, float amount, float duration, bool percent = false);

        void RemoveDebuffs();

        // DOT / HOT
        void ApplyDot(EffectData data, IStatsComponent sourceStats, ulong sourceId);
        void ApplyHot(EffectData data, IStatsComponent sourceStats, ulong sourceId);

        // Crowd control
        void ApplyStun(float duration);
        void ApplyRoot(float duration);
        void ApplyTaunt(ulong sourceId, float duration);

        // Knockback
        void Knockback(Vector3 force);

        // Can stay or be removed (migrated to BuffComponent)
        void AddDamageTakenModifier(float value, float duration);
    }
}