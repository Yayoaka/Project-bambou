using UnityEngine;

namespace Enemies.Data
{
    [CreateAssetMenu(menuName = "Enemies/New Entity")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public GameObject VisualPrefab;

        [Header("Stats")]
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;

        //Maybe later son
        /*[Header("Behavior")]
        public EnemyBehaviorTreeSO BehaviorTree;
        public Shader ShaderOverride;*/
    }
}