using UnityEngine;

namespace Health
{
    public enum EventType
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
        public GameObject Source;
        public EventType Type;
        public bool Critical;
    }
}