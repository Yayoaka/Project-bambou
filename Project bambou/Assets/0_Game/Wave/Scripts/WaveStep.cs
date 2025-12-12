using UnityEngine;

namespace Wave
{
    public abstract class WaveStep : ScriptableObject
    {
        public abstract void Start(WaveContext context);
        public abstract bool Tick(WaveContext context, float deltaTime);
    }
}