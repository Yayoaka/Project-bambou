using Effect;
using Health;
using Stats;
using UnityEngine;

namespace Interfaces
{
    public interface IAffectable
    {
        void Damage(HealthEventData data);
        void Heal(HealthEventData data);

        void AddShield(float amount, float duration);
        void AddBuff(float amount, float duration);
        void AddDebuff(float amount, float duration);

        void ApplyTaunt(IAffectable source, float duration);

        void ApplyDot(EffectData data, IStatsComponent stats, IAffectable source);
        void ApplyHot(EffectData data, IStatsComponent stats, IAffectable source);

        void ApplyStun(float duration);
        void ApplyRoot(float duration);
        void RemoveDebuffs();
        void AddDamageTakenModifier(float value, float duration);

        void Knockback(Vector3 force);
    }
}