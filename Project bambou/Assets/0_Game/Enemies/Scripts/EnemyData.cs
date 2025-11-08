using Unity.Entities;
using Unity.Mathematics;

namespace Enemies
{
    public struct EnemyConfigData : IComponentData
    {
        public BlobAssetReference<EnemyConfigBlob> Config;
    }

    public struct EnemyStateData : IComponentData
    {
        public float CurrentHealth;
    }
    
    public struct EnemyConfigBlob
    {
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
    }
    
    public struct EnemyPrefabData : IComponentData
    {
        public Entity Value;
    }
}
