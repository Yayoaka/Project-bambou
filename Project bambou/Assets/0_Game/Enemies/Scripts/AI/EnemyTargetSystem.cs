using Player.ECS;
using Unity.Burst;
using Unity.Entities;

namespace Enemies.AI
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMapSystem))]
    public partial struct EnemyTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerMapSingleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var map = SystemAPI.GetSingleton<PlayerMapSingleton>().Map;
            if (!map.IsCreated) return;

            foreach (var (targetId, targetData) in 
                     SystemAPI.Query<RefRO<EnemyTargetId>, RefRW<EnemyTargetData>>())
            {
                var id = targetId.ValueRO.PlayerId;
                if (map.TryGetValue(id, out var pos))
                {
                    targetData.ValueRW.TargetPosition = pos;
                }
            }
        }
    }
}