using Enemies.Spawner;
using Unity.Entities;
using UnityEngine;

namespace Enemies
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public int maxHealth = 100;
        public float moveSpeed = 100;
        public int attackDamage = 1;

        public class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring a)
            {
                var enemyEntity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(enemyEntity, new EnemyData
                {
                    MaxHealth = a.maxHealth,
                    MoveSpeed = a.moveSpeed,
                    AttackDamage = a.attackDamage
                });
            }
        }
    }
}
