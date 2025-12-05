using System;
using Stats.Data;
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
        public StatsData stats;
        
        [Header("Prefabs")]
        public GameObject visualPrefab;
        public GameObject deathVisual;
    }
}