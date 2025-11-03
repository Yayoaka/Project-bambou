using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Player.ECS
{
    public struct PlayerMapSingleton : IComponentData
    {
        public NativeParallelHashMap<int, float3> Map;
    }
}