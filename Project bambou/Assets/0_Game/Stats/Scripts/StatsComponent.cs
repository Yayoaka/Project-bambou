using System;
using Buff;
using UnityEngine;
using Effect;
using Effect.Stats.Data;
using Stats.Data;
using Random = UnityEngine.Random;

namespace Stats
{
    [RequireComponent(typeof(BuffComponent))]
    public class StatsComponent : MonoBehaviour, IStatsComponent
    {
        public event Action OnStatsChanged;
        
        private StatsData _baseStats;   // injecté via SetStats
        private BuffComponent _buffs;

        private void Awake()
        {
            _buffs = GetComponent<BuffComponent>();
            _buffs.OnBuffChanged += UpdateStats;
        }

        private void OnDestroy()
        {
            _buffs.OnBuffChanged -= UpdateStats;
        }
        
        // ------------------------------------------------------
        //        Inject stats depuis un ScriptableObject
        // ------------------------------------------------------
        public void SetStats(StatsData data)
        {
            _baseStats = data;
            
            OnStatsChanged?.Invoke();
        }

        // ------------------------------------------------------
        //                GET STAT (buff-aware)
        // ------------------------------------------------------
        public float GetStat(StatType type)
        {
            if (_baseStats == null)
            {
                Debug.LogError($"{name} : StatsComponent appelé avant SetStats() !");
                return 0;
            }

            float baseValue = type switch
            {
                StatType.AttackDamage   => _baseStats.abilityDamage,
                StatType.AbilityPower   => _baseStats.abilityPower,

                StatType.CritChance     => _baseStats.critChance,
                StatType.CritMultiplier => _baseStats.critMultiplier,

                StatType.Armor          => _baseStats.armorResistance,
                StatType.MagicResist    => _baseStats.magicResistance,

                StatType.MaxHealth      => _baseStats.health,
                StatType.AttackSpeed    => _baseStats.attackSpeed,
                StatType.MoveSpeed      => _baseStats.moveSpeed,

                _ => 0f
            };

            return _buffs.GetModifiedStat(type, baseValue);
        }

        // ------------------------------------------------------
        //                CRITICAL
        // ------------------------------------------------------
        public bool ComputeCrit(EffectType type)
        {
            return Random.value < GetStat(StatType.CritChance);
        }

        public float GetCritMultiplier(EffectType type)
        {
            return GetStat(StatType.CritMultiplier);
        }

        public float ComputeDamageDealt(float rawDamage, bool crit)
        {
            return crit ? rawDamage * GetCritMultiplier(EffectType.Physical) : rawDamage;
        }

        // ------------------------------------------------------
        //                MITIGATION
        // ------------------------------------------------------
        public float ComputeDamageTaken(float rawDamage, EffectType type)
        {
            float defense = type switch
            {
                EffectType.Physical => GetStat(StatType.Armor),
                EffectType.Magical  => GetStat(StatType.MagicResist),
                _                   => 0
            };

            return Mathf.Max(0f, rawDamage * (100f / (100f + defense)));
        }

        public float ModifyOutgoingEffect(float value, EffectModifierType modifier)
        {
            return value; // sera amélioré plus tard si besoin
        }

        private void UpdateStats(BuffEntry entry)
        {
            OnStatsChanged?.Invoke();
        }
    }
}
