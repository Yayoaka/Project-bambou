using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class EffectCastData
    {
        [Header("Projectile")]
        public bool spawnProjectile;
        public GameObject projectilePrefab;

        [Header("Zone")]
        public bool spawnZone;
        public GameObject zonePrefab;

        [Header("Spawn Params")]
        public bool onCursor;
        public bool toCursor;
        public bool followCaster;

        [Header("Effects applied by this cast")]
        public List<EffectData> appliedEffects = new();
    }
}