using System;
using Collectibles;
using Stats.Data;
using Unity.Netcode;
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
        
        [Header("Xp Loot")]
        public NetworkObject xpLoot;
        public int xpAmount;
    }
}