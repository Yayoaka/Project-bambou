using Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemies
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct EnemySystem : ISystem
    {
        private const int FarTicks = 50;
        private const int CloseTicks = 10;
        
        private static int _tickUntilNextFarUpdate;
        private static int _tickUntilNextCloseUpdate;
        
        public void OnUpdate(ref SystemState state)
        {
            using var players = CollectPlayers(ref state);

            if (_tickUntilNextFarUpdate <= 0)
            {
                _tickUntilNextFarUpdate = FarTicks;
                
                foreach (var enemy in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyData>, RefRO<FarTag>>())
                {
                    ref var localTransform = ref enemy.Item1.ValueRW;

                    var closestPlayer = GetClosestPlayer(players, localTransform.Position);

                    MoveEnemy(ref state, ref localTransform, closestPlayer, enemy.Item2.ValueRO.MoveSpeed * FarTicks);
                }
            }
            else
            {
                _tickUntilNextFarUpdate--;
            }

            if (_tickUntilNextCloseUpdate <= 0)
            {
                _tickUntilNextCloseUpdate = CloseTicks;
                
                foreach (var enemy in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyData>, RefRO<CloseTag>>())
                {
                    ref var localTransform = ref enemy.Item1.ValueRW;

                    var closestPlayer = GetClosestPlayer(players, localTransform.Position);

                    MoveEnemy(ref state, ref localTransform, closestPlayer, enemy.Item2.ValueRO.MoveSpeed * CloseTicks);
                }
            }
            else
            {
                _tickUntilNextCloseUpdate--;
            }
            
            foreach (var enemy in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyData>, RefRO<VisibleTag>>())
            {
                ref var localTransform = ref enemy.Item1.ValueRW;

                var closestPlayer = GetClosestPlayer(players, localTransform.Position);

                MoveEnemy(ref state, ref localTransform, closestPlayer, enemy.Item2.ValueRO.MoveSpeed);
            }
        }

        private NativeList<PlayerData> CollectPlayers(ref SystemState state)
        {
            var players = new NativeList<PlayerData>(Allocator.Temp);

            foreach (var p in SystemAPI.Query<RefRO<PlayerData>>())
                players.Add(p.ValueRO);

            return players;
        }
        
        private PlayerData GetClosestPlayer(in NativeList<PlayerData> players, float3 position)
        {
            var minDistance = float.MaxValue;
            var closestPlayer = players[0];
            
            foreach (var player in players)
            {
                var distance = math.lengthsq(position - player.Position);

                if (!(minDistance > distance)) continue;
                
                minDistance = distance;
                closestPlayer = player;
            }
            return closestPlayer;
        }

        private void MoveEnemy(ref SystemState state, ref LocalTransform transform, PlayerData player, float speed)
        {
            var dir = math.normalizesafe(player.Position - transform.Position);
            transform.Position += dir * speed * SystemAPI.Time.DeltaTime;
        }
    }
}
