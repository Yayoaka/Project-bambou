using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    public enum CastMode
    {
        Default,
        ToCursor,
        ToClosestEnemy,
        ToAnyTarget,
        OnCursor,
        OnClosestEnemy,
        OnAnyTarget,
    }

    [Serializable]
    public class EffectCastData
    {
        public CastMode castMode = CastMode.Default;
        public float targetSearchRange = 10f;

        // Projectile
        public bool spawnProjectile;
        public GameObject projectilePrefab;
        public bool destroyOnHit = true;
        public float projectileSpeed = 10f;
        public float projectileLifetime = 3f;
        public int additionalProjectileCount;

        // Zone
        public bool spawnZone;
        public GameObject zonePrefab;

        public float firstTickDelay = 0f;
        public bool loop = false;
        public float tickRate = 1f;
        public float zoneLifetime = 3f;
        public int additionalZoneCount;

        // Common
        public bool followCaster;

        public List<EffectData> appliedEffects = new();
    }
}