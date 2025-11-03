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
            
        }
    }
}
