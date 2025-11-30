using System;
using UnityEngine;

namespace Enemies.Data
{
    [CreateAssetMenu(menuName = "Enemies/New Entity")]
    [Serializable]
    public class EnemyDataSo : ScriptableObject
    {
        [Header("Identity")]
        public string id;

        [Header("Stats")]
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackRange;
        
        [Header("Prefabs")]
        public GameObject visualPrefab;
        public GameObject deathVisual;
    }
}