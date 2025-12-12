using UnityEngine;

namespace Wave
{
    [CreateAssetMenu(menuName = "Waves/Steps/Wait Until Clear")]
    public class WaitUntilClearStep : WaveStep
    {
        public override void OnStepEnter(WaveContext context) {}

        public override bool OnStepUpdate(WaveContext context, float deltaTime)
        {
            return context.aliveEnemies <= 0;
        }
    }
}