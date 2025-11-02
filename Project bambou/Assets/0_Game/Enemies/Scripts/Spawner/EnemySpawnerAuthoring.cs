using Unity.Entities;
using UnityEngine;

namespace Enemies.Spawner
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public int count = 100;
        public float radius = 10f;

        public class Baker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring a)
            {
                var spawnerEntity = GetEntity(TransformUsageFlags.None);
                var enemyEntityPrefab = GetEntity(a.enemyPrefab, TransformUsageFlags.Dynamic);

                AddComponent(spawnerEntity, new EnemySpawnerData
                {
                    Prefab = enemyEntityPrefab,
                    Count = a.count,
                    Radius = a.radius,
                    Spawned = false
                });
            }
        }
    }
}