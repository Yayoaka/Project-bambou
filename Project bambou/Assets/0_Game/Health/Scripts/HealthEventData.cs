using Interfaces;
using UnityEngine;

namespace Health
{
    public enum HealthEventType
    {
        Physical,
        Magical,
        True,
        Healing,
    }
    public struct HealthEventData
    {
        public float Amount;
        public Vector3 HitPoint;
        public IAffectable Source;
        public HealthEventType Type;
        public bool Critical;
    }
}