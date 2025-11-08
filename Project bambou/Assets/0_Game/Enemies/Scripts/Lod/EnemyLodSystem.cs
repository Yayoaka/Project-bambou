using Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemies.Lod
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemySystem))]
    public partial struct EnemyLodSystem : ISystem
    {
        private const float FarRange = 60f;
        private const float CloseRange = 30f;

        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            using var players = CollectPlayers(ref state);

            foreach (var (transform, lodData) in SystemAPI.Query<RefRO<LocalTransform>,RefRW<EnemyLodData>>())
            {
                var distance = GetMinDistanceToPlayer(in players, transform.ValueRO.Position);
                var dist = math.sqrt(distance);

                if (dist > FarRange)
                    lodData.ValueRW.Interval = 50;
                else if (dist > CloseRange)
                    lodData.ValueRW.Interval = 10;
                else
                    lodData.ValueRW.Interval = 1;
            }

            ecb.Playback(state.EntityManager);
        }
        
        private NativeList<PlayerData> CollectPlayers(ref SystemState state)
        {
            var players = new NativeList<PlayerData>(Allocator.Temp);

            foreach (var p in SystemAPI.Query<RefRO<PlayerData>>())
                players.Add(p.ValueRO);

            return players;
        }
        
        private float GetMinDistanceToPlayer(in NativeList<PlayerData> players, float3 position)
        {
            var minDistance = float.MaxValue;
            
            foreach (var player in players)
            {
                var distance = math.lengthsq(position - player.Position);

                if (!(minDistance > distance)) continue;
                
                minDistance = distance;
            }
            
            return minDistance;
        }
    }
}