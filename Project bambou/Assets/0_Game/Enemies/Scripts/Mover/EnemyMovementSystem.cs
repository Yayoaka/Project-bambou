using Enemies.AI;
using Enemies.Lod;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemies.Mover
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct EnemyMovementSystem : ISystem
    {
        private int _frameCounter;
        
        public void OnUpdate(ref SystemState state)
        {
            _frameCounter++;

            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (transform, tick, target, config) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<EnemyLodData>, RefRO<EnemyTargetData>, RefRO<EnemyConfigData>>())
            {
                tick.ValueRW.Counter++;
                if (tick.ValueRO.Counter < tick.ValueRO.Interval)
                    continue;
                
                tick.ValueRW.Counter = 0;

                var effectiveDt = SystemAPI.Time.DeltaTime * tick.ValueRO.Interval;
                MoveEnemy(ref transform.ValueRW, in target.ValueRO, in config.ValueRO, effectiveDt);
            }
        }
        
        private static void MoveEnemy(ref LocalTransform transform, in EnemyTargetData target, in EnemyConfigData config, float dt)
        {
            ref var cfg = ref config.Config.Value;
            var dir = math.normalizesafe(target.TargetPosition - transform.Position);
            transform.Position += dir * cfg.MoveSpeed * dt;
        }
    }
}