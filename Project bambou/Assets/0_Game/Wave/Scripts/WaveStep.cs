using UnityEngine;

namespace Wave
{
    public abstract class WaveStep : ScriptableObject
    {
        public abstract void OnStepEnter(WaveContext context);
        public abstract bool OnStepUpdate(WaveContext context, float deltaTime);
    }
}