using Effect;
using Interfaces;
using Stats.Data;
using UnityEngine;

namespace Health
{
    public struct HealthEventData
    {
        public float Amount;
        public Vector3 HitPoint;
        public IAffectable Source;
        public EffectType Type;
        public bool Critical;
    }
}