using Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Player.ECS
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PlayerMapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var map = new NativeParallelHashMap<int, float3>(8, Allocator.Persistent);
            state.EntityManager.CreateSingleton(new PlayerMapSingleton { Map = map });
            
            state.RequireForUpdate<PlayerMapSingleton>();
        }

        public void OnDestroy(ref SystemState state)
        {
            var map = SystemAPI.GetSingleton<PlayerMapSingleton>().Map;
            if (map.IsCreated) map.Dispose();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var map = SystemAPI.GetSingleton<PlayerMapSingleton>().Map;
            map.Clear();

            foreach (var player in SystemAPI.Query<RefRO<PlayerData>>())
            {
                var id = player.ValueRO.Id;
                var pos = player.ValueRO.Position;
                map.TryAdd(id, pos);
            }
        }
    }
}