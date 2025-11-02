using UnityEngine;

namespace Enemies
{
    public sealed class EnemyContext
    {
        public Transform Transform { get; }
        public EnemyStats Stats { get; private set; }
        
        public Enemy Enemy  { get; private set; }

        public EnemyContext(Transform t, EnemyStats stats, Enemy enemy)
        {
            Transform = t;
            Stats = stats;
            Enemy = enemy;
        }

        public void SetStats(EnemyStats stats)
        {
            Stats = stats;
        }
    }
}
