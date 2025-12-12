using System.Collections.Generic;
using UnityEngine;

namespace Wave
{
    public enum WaveEndPolicy
    {
        Stop,           // noting
        Loop,           // restart
        LoadNext,       // next timeline
        Endless,        // todo idk
        SignalGameState // end
    }
    
    [CreateAssetMenu(menuName = "Waves/Wave Timeline")]
    public class WaveTimeline : ScriptableObject
    {
        public List<WaveStep> steps = new();

        [Header("End Behavior")]
        public WaveEndPolicy endPolicy = WaveEndPolicy.Stop;
        public WaveTimeline nextTimeline;
    }
}