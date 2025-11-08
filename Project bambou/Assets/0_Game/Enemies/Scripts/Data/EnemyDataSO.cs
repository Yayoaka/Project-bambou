using UnityEngine;

namespace Enemies.Data
{
    [CreateAssetMenu(menuName = "Enemies/New Entity")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string Id;

        [Header("Stats")]
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
        
        [Header("Prefabs")]
        public GameObject VisualPrefab;

        //Maybe later son
        /*[Header("Behavior")]
        public EnemyBehaviorTreeSO BehaviorTree;
        public Shader ShaderOverride;*/
    }
}