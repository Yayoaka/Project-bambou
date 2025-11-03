using Unity.Entities;

namespace Enemies.Lod
{
    public struct EnemyLodData : IComponentData
    {
        public int Interval;
        public int Counter;
    }
}