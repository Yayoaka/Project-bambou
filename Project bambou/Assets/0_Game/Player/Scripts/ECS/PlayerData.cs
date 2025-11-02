using Unity.Entities;
using Unity.Mathematics;

namespace Player
{
    public struct PlayerData : IComponentData
    {
        public int Id;          
        public float3 Position;
    }
}
