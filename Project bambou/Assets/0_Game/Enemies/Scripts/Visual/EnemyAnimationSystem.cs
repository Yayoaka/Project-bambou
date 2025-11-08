using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Enemies.Visual
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct EnemyAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var anim in SystemAPI.Query<RefRW<EnemyAnimationState>>())
            {
                anim.ValueRW.Time += deltaTime * anim.ValueRO.Speed;
            }
        }
    }
}