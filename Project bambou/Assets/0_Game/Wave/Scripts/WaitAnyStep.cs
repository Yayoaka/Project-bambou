using UnityEngine;

namespace Wave
{
    [CreateAssetMenu(menuName = "Waves/Steps/Wait Any")]
    public class WaitAnyStep : WaveStep
    {
        public float maxDuration = 10f;
        private float _timer;

        public override void Start(WaveContext context)
        {
            _timer = 0f;
        }

        public override bool Tick(WaveContext context, float deltaTime)
        {
            _timer += deltaTime;

            if (context.aliveEnemies <= 0)
                return true;

            return _timer >= maxDuration;
        }
    }
}