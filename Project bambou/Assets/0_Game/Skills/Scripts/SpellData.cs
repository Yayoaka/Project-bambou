using System;
using Health;
using UnityEngine;

namespace Skills
{
    public enum SpellType
    {
        Projectile,
        Zone,
        Target
    }

    [Serializable]
    public struct EffectData
    {
        public SpellType type;
        public float duration;
        public HealthEventType effectType;
        public HealthModificationData HealthModification;
        public GameObject effectPrefab;
        public bool onCursor;
        public bool toCursor;
        public bool followCaster;
        public bool loop;
        public float tickDelay;
    }

    [Serializable]
    public struct HealthModificationData
    {
        public float BaseAbilityDamage;
        public float BaseAbilityPower;
        public float BaseHeal;
        public float BonusDamagePercentage;
        public float BonusPowerPercentage;
    }
    
    [CreateAssetMenu(fileName = "SpellData", menuName = "Spells/SpellData", order = 0)]
    public class SpellData : ScriptableObject
    {
        [SerializeField] private string spellName;
        [SerializeField] private string spellDescription;
        [SerializeField] private Sprite spellIcon;
        [SerializeField] private float cooldown;
        [SerializeField] private EffectData[] effects;
        [SerializeField] private bool animate;

        public string SpellName => spellName;
        public string SpellDescription => spellDescription;
        public Sprite SpellIcon => spellIcon;
        public float Cooldown => cooldown;
        public EffectData[] Effects => effects;
        public bool Animate => animate;
    }
}
