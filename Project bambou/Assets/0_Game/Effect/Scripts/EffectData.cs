using System;
using Stats.Data;
using UnityEngine;

namespace Effect
{
    [Serializable]
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(EffectData))]
#endif
    public class EffectData
    {
        [HideInInspector] public float baseValue;
        [HideInInspector] public float percentAD;
        [HideInInspector] public float percentAP;
        [HideInInspector] public StatType targetStat;
        [HideInInspector] public bool isPercent;

        [HideInInspector] public float duration;
        [HideInInspector] public float tickDelay;
        [HideInInspector] public bool loop;

        [HideInInspector] public EffectType effectType;

        public EffectKind kind;
    }
}