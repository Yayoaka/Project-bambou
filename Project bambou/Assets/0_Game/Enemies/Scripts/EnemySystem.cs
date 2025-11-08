using Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemies
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemySystem : ISystem
    {
        private const int FarTicks = 50;
        private const int CloseTicks = 10;
        
        private static int _tickUntilNextFarUpdate;
        private static int _tickUntilNextCloseUpdate;

        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (prefabData, enemyEntity) in SystemAPI.Query<RefRO<EnemyPrefabData>>().WithEntityAccess())
            {
                var enemyVisual = ecb.Instantiate(prefabData.ValueRO.Value);
                
                ecb.AddComponent(enemyVisual, new Parent { Value = enemyEntity });

                ecb.SetComponent(enemyVisual, LocalTransform.FromPositionRotationScale(
                    float3.zero, quaternion.identity, 1f));
                
                ecb.RemoveComponent<EnemyPrefabData>(enemyEntity);
            }
            
            ecb.Playback(state.EntityManager);
        }
    }
}
