using UnityEngine;

namespace Wave
{
    [CreateAssetMenu(menuName = "Waves/Steps/Wait Time")]
    public class WaitTimeStep : WaveStep
    {
        public float duration = 5f;
        private float _timer;

        public override void OnStepEnter(WaveContext context)
        {
            _timer = 0f;
        }

        public override bool OnStepUpdate(WaveContext context, float deltaTime)
        {
            _timer += deltaTime;
            return _timer >= duration;
        }
    }
}