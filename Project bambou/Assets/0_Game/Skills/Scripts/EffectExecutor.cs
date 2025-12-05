using Effect;
using Health;
using Interfaces;
using Skills.Data;
using Stats;
using Stats.Data;
using UnityEngine;

namespace Skills
{
    public static class EffectExecutor
    {
        public static void Execute(
            EffectData data,
            IStatsComponent sourceStats,
            IAffectable source,
            IAffectable target,
            Vector3 posOrDir)
        {
            switch (data.kind)
            {
                case EffectKind.Damage:
                    ApplyDamage(data, sourceStats, source, target);
                    break;

                case EffectKind.Heal:
                    ApplyHeal(data, sourceStats, source, target);
                    break;

                case EffectKind.Shield:
                    target?.AddShield(ComputeEffectValue(data, sourceStats), data.duration);
                    break;

                case EffectKind.Buff:
                    target?.AddBuff(
                        stat: data.targetStat,
                        amount: ComputeEffectValue(data, sourceStats),
                        duration: data.duration,
                        percent: data.isPercent
                    );
                    break;

                case EffectKind.Debuff:
                    target?.AddDebuff(
                        stat: data.targetStat,
                        amount: ComputeEffectValue(data, sourceStats),
                        duration: data.duration,
                        percent: data.isPercent
                    );
                    break;

                case EffectKind.Taunt:
                    target?.ApplyTaunt(source, data.duration);
                    break;

                case EffectKind.Dot:
                    ApplyDot(data, sourceStats, source, target);
                    break;

                case EffectKind.Hot:
                    ApplyHot(data, sourceStats, source, target);
                    break;

                case EffectKind.Knockback:
                    target?.Knockback(posOrDir.normalized * ComputeEffectValue(data, sourceStats));
                    break;

                case EffectKind.Stun:
                    target?.ApplyStun(data.duration);
                    break;

                case EffectKind.Root:
                    target?.ApplyRoot(data.duration);
                    break;

                case EffectKind.Cleanse:
                    target?.RemoveDebuffs();
                    break;

                case EffectKind.Vulnerability:
                    target?.AddDamageTakenModifier(ComputeEffectValue(data, sourceStats), data.duration);
                    break;
            }
        }

        private static float ComputeEffectValue(EffectData data, IStatsComponent stats)
        {
            float AD = stats.GetStat(StatType.AttackDamage);
            float AP = stats.GetStat(StatType.AbilityPower);

            float value = data.baseValue;
            value += AD * data.percentAD;
            value += AP * data.percentAP;

            return Mathf.Max(0, value);
        }

        private static void ApplyDamage(EffectData data, IStatsComponent stats, IAffectable source, IAffectable target)
        {
            if (target == null) return;

            float amount = ComputeEffectValue(data, stats);
            bool crit = stats.ComputeCrit(data.effectType);

            amount = stats.ComputeDamageDealt(amount, crit);

            var evt = new HealthEventData()
            {
                Amount = amount,
                Critical = crit,
                Source = source,
                Type = data.effectType
            };

            target.Damage(evt);
        }

        private static void ApplyHeal(EffectData data, IStatsComponent stats, IAffectable source, IAffectable target)
        {
            if (target == null) return;

            float amount = ComputeEffectValue(data, stats);

            target.Heal(new HealthEventData
            {
                Amount = amount,
                Critical = false,
                Source = source,
                Type = EffectType.Heal
            });
        }

        private static void ApplyDot(EffectData data, IStatsComponent stats, IAffectable source, IAffectable target)
        {
            target?.ApplyDot(data, stats, source);
        }

        private static void ApplyHot(EffectData data, IStatsComponent stats, IAffectable source, IAffectable target)
        {
            target?.ApplyHot(data, stats, source);
        }
    }
}
