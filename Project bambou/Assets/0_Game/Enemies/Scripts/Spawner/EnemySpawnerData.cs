using Unity.Entities;

namespace Enemies.Spawner
{
    public struct EnemySpawnerData : IComponentData
    {
        public Entity Prefab;
        public int Count;
        public float Radius;
        public bool Spawned;
    }
}