using System;
using System.Linq;
using Data;
using UnityEngine;

namespace Stats.Data
{
    public enum HealthEventType
    {
        Physical,
        Magical,
        True,
        Healing,
    }

    [Serializable]
    public struct EventData
    {
        public HealthEventType Type;
        public string Name;
        public Color Color;
    }
    
    [CreateAssetMenu(menuName = "Stats/Stats Database")]
    public class StatsDatabase : GameData
    {
        [SerializeField] private EventData[] _events;
        
        public EventData[] Events => _events;

        public EventData GetEvent(HealthEventType type)
        {
            return Events.FirstOrDefault(x => x.Type == type);
        }
    }
}