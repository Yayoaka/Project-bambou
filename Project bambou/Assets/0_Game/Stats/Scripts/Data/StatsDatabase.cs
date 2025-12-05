using System;
using System.Linq;
using Data;
using Effect;
using UnityEngine;

namespace Stats.Data
{
    [Serializable]
    public struct EventData
    {
        public EffectType Type;
        public string Name;
        public Color Color;
    }
    
    [CreateAssetMenu(menuName = "Stats/Stats Database")]
    public class StatsDatabase : GameData
    {
        [SerializeField] private EventData[] _events;
        
        public EventData[] Events => _events;

        public EventData GetEvent(EffectType type)
        {
            return Events.FirstOrDefault(x => x.Type == type);
        }
    }
}