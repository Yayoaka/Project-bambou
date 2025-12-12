using UnityEngine;

namespace Wave
{
    [CreateAssetMenu(menuName = "Waves/Steps/Wait Until Clear")]
    public class WaitUntilClearStep : WaveStep
    {
        public override void Start(WaveContext context) {}

        public override bool Tick(WaveContext context, float deltaTime)
        {
            return context.aliveEnemies <= 0;
        }
    }
}