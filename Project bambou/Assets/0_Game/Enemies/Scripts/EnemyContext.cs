using UnityEngine;

namespace Enemies
{
    public sealed class EnemyContext
    {
        public Transform Transform { get; }
        public EnemyStats Stats { get; private set; }

        public EnemyContext(Transform t, EnemyStats stats)
        {
            Transform = t;
            Stats = stats;
        }

        public void SetStats(EnemyStats stats)
        {
            Stats = stats;
        }
    }
}
