using System;
using System.Collections.Generic;
using Effect;
using Stats.Data;
using UnityEngine;

namespace Skills.Data
{
    [CreateAssetMenu(fileName = "SpellData", menuName = "Spells/SpellData")]
    public class SpellData : ScriptableObject
    {
        [Header("Info")]
        public string spellName;
        public string spellDescription;
        public Sprite spellIcon;
        public float cooldown;
        public bool animate;
        public bool autoCast;

        [Header("Effects applied immediately on cast")]
        public List<EffectData> gameplayEffects = new();

        [Header("Effects manifested in the world (projectiles, zones...)")]
        public List<EffectCastData> castEffects = new();
    }
}
