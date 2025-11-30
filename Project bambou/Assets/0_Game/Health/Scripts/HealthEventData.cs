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
        public HealthEventType Type;
        public bool Critical;
    }
}