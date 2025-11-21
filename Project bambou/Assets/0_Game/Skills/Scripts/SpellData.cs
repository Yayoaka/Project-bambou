using System;
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
        public SpellType Type;
        public float Duration;
        public GameObject Effect;
        public bool OnCursor;
    }
    
    [CreateAssetMenu(fileName = "SpellData", menuName = "Spells/SpellData", order = 0)]
    public class SpellData : ScriptableObject
    {
        public string SpellName;
        public string SpellDescription;
        public Sprite SpellIcon;
        public float Cooldown;
        public EffectData[] Effects;
    }
}
