using Enemies.AI;
using Enemies.Data;
using Enemies.Lod;
using Enemies.Spawner;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Enemies
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public EnemyDataSO config;

        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var cfg = authoring.config;

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<EnemyConfigBlob>();
                root.MaxHealth = cfg.MaxHealth;
                root.MoveSpeed = cfg.MoveSpeed;
                root.AttackDamage = cfg.AttackDamage;
                root.AttackRange = cfg.AttackRange;
                var blobRef = builder.CreateBlobAssetReference<EnemyConfigBlob>(Allocator.Persistent);
                builder.Dispose();

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new EnemyConfigData { Config = blobRef });
                AddComponent(entity, new EnemyStateData { CurrentHealth = cfg.MaxHealth });
                AddComponent(entity, new EnemyLodData { Interval = 1, Counter = 0 });
                AddComponent(entity, new EnemyTargetData { TargetPosition = new float3() });
            }
        }
    }
}
