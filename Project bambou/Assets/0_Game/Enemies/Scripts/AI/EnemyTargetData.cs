using Unity.Entities;
using Unity.Mathematics;

namespace Enemies.AI
{
    public struct EnemyTargetId : IComponentData
     {
         public int PlayerId;
     }
    
    public struct EnemyTargetData : IComponentData
    {
        public float3 TargetPosition;
    }
}