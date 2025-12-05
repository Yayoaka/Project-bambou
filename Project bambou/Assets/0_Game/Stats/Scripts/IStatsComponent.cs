using Effect;
using Effect.Stats.Data;
using Stats.Data;

namespace Stats
{
    public interface IStatsComponent
    {
        float GetStat(StatType type);

        public void SetStats(StatsData data);

        bool ComputeCrit(EffectType type);
        float GetCritMultiplier(EffectType type);

        float ComputeDamageTaken(float rawDamage, EffectType type);

        float ComputeDamageDealt(float rawDamage, bool crit);

        float ModifyOutgoingEffect(float value, EffectModifierType modifierType);
    }

}