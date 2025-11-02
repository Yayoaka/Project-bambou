using Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemies.Distance
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemySystem))]
    public partial struct EnemyDistanceSystem : ISystem
    {
        private const float FarRange = 60f;
        private const float CloseRange = 30f;

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            using var players = CollectPlayers(ref state);

            foreach (var (transform, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithEntityAccess())
            {
                var distance = GetMinDistanceToPlayer(in players, transform.ValueRO.Position);
                var dist = math.sqrt(distance);

                if (SystemAPI.HasComponent<FarTag>(enemyEntity))
                    ecb.RemoveComponent<FarTag>(enemyEntity);
                if (SystemAPI.HasComponent<CloseTag>(enemyEntity))
                    ecb.RemoveComponent<CloseTag>(enemyEntity);
                if (SystemAPI.HasComponent<VisibleTag>(enemyEntity))
                    ecb.RemoveComponent<VisibleTag>(enemyEntity);

                if (dist > FarRange)
                    ecb.AddComponent<FarTag>(enemyEntity);
                else if (dist > CloseRange)
                    ecb.AddComponent<CloseTag>(enemyEntity);
                else
                    ecb.AddComponent<VisibleTag>(enemyEntity);
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