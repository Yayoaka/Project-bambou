using UnityEngine;
using Effect;
using Effect.Stats.Data;
using Stats.Data;
using Stats.Data.Stats.Data;

namespace Stats
{
    public class StatsComponent : MonoBehaviour, IStatsComponent
    {
        [Header("Offensive Stats")]
        [SerializeField] private float attackDamage = 20f;
        [SerializeField] private float abilityPower = 15f;

        [Header("Critical")]
        [SerializeField] private float critChance = 0.10f;      // 10%
        [SerializeField] private float critMultiplier = 2.0f;   // 200%

        [Header("Defensive Stats")]
        [SerializeField] private float armor = 10f;
        [SerializeField] private float magicResist = 10f;

        [Header("Outgoing Modifiers")]
        [SerializeField] private float outgoingDamageModifier = 1f;
        [SerializeField] private float outgoingHealModifier = 1f;
        [SerializeField] private float outgoingShieldModifier = 1f;

        public void SetStats(StatsData s)
        {
            attackDamage = s.abilityDamage;
            abilityPower = s.abilityPower;

            critChance = s.critChance;
            critMultiplier = s.critMultiplier;

            armor = s.armorResistance;
            magicResist = s.magicResistance;
        }

        public float GetStat(StatType type)
        {
            return type switch
            {
                StatType.AttackDamage  => attackDamage,
                StatType.AbilityPower  => abilityPower,

                StatType.CritChance    => critChance,
                StatType.CritMultiplier => critMultiplier,

                StatType.Armor         => armor,
                StatType.MagicResist   => magicResist,

                _ => 0f
            };
        }

        public bool ComputeCrit(EffectType type)
        {
            return Random.value < critChance;
        }

        public float GetCritMultiplier(EffectType type)
        {
            return critMultiplier;
        }

        public float ComputeDamageDealt(float rawDamage, bool crit)
        {
            return crit ? rawDamage * critMultiplier : rawDamage;
        }

        public float ComputeDamageTaken(float rawDamage, EffectType type)
        {
            return type switch
            {
                EffectType.Physical => ApplyMitigation(rawDamage, armor),
                EffectType.Magical  => ApplyMitigation(rawDamage, magicResist),
                EffectType.True     => rawDamage,
                EffectType.Heal     => rawDamage,
                _                   => rawDamage
            };
        }

        private float ApplyMitigation(float rawDamage, float defense)
        {
            return Mathf.Max(0f, rawDamage * (100f / (100f + defense)));
        }

        public float ModifyOutgoingEffect(float value, EffectModifierType type)
        {
            return type switch
            {
                EffectModifierType.OutgoingDamage => value * outgoingDamageModifier,
                EffectModifierType.OutgoingHeal   => value * outgoingHealModifier,
                EffectModifierType.OutgoingShield => value * outgoingShieldModifier,
                _                                  => value
            };
        }
    }
}
