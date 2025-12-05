using System;
using System.Collections.Generic;
using Skills.Data;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class EffectData
    {
        public EffectKind kind;

        [Header("Base power (raw damage / heal)")]
        public float baseValue;

        [Header("Scaling")]
        [Tooltip("Range between 0 and 1")]public float percentAD;
        [Tooltip("Range between 0 and 1")]public float percentAP;

        [Header("Periodic (dot/hot)")]
        public float duration;
        public float tickDelay = 1f;
        public bool loop;

        public EffectType effectType;
    }
}