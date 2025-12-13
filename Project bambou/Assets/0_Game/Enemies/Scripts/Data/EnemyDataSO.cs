using System;
using Collectibles;
using Effect;
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
        public EffectData effect;
        public float radius = 1;
        
        [Header("Prefabs")]
        public GameObject visualPrefab;
        public GameObject deathVisual;
        
        [Header("Xp Loot")]
        public NetworkObject xpLoot;
        public int xpAmount;
    }
}