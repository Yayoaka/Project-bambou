using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Enemies.Visual;
using Unity.Rendering;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Enemies.Spawner
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EnemySpawnerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var spawner in SystemAPI.Query<RefRW<EnemySpawnerData>>())
            {
                if (spawner.ValueRO.Spawned)
                    continue;

                var prefab = spawner.ValueRO.Prefab;
                var count = spawner.ValueRO.Count;
                var radius = spawner.ValueRO.Radius;

                for (int i = 0; i < count; i++)
                {
                    var e = ecb.Instantiate(prefab);

                    var rnd = Random.CreateFromIndex((uint)(i + 1))
                                  .NextFloat2Direction() *
                              Random.CreateFromIndex((uint)(i + 1234)).NextFloat(0f, radius);

                    var pos = new float3(rnd.x, 0f, rnd.y);
                    ecb.SetComponent(e, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 1f));
                }

                spawner.ValueRW.Spawned = true;
            }

            ecb.Playback(state.EntityManager);
        }
    }
}