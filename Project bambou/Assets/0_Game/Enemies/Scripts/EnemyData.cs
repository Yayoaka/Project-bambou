using Unity.Entities;

namespace Enemies
{
    public struct EnemyData : IComponentData
    {
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
    }
}
