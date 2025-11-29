using Health;
using Skills;
using Stats;
using UnityEngine;

namespace Character
{
    public class CharacterStats : CharacterComponent, IStatsEntity
    {
        private Data.Stats _stats;

        public void SetStats(Data.Stats stats)
        {
            _stats = stats;
        }
        
        public float ComputeStat(HealthModificationData data, bool isCritical)
        {
            var finalValue = data.BaseAbilityDamage + data.BaseAbilityPower;

            finalValue += data.BonusDamagePercentage * _stats.abilityDamage;

            finalValue += data.BonusPowerPercentage * _stats.abilityPower;

            return isCritical ? finalValue * _stats.critMultiplier : finalValue;
        }

        public bool ComputeCrit(HealthEventType type)
        {
            var critChance = _stats.critChance;

            return Random.value < critChance;
        }

        public float ComputeReceivedStat(float baseValue, HealthEventType type)
        {
            throw new System.NotImplementedException();
        }
    }
}