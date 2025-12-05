using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    public enum CastDirectionMode
    {
        Default,
        ToCursor,
        ToClosestEnemy,
        ToAnyTarget
    }

    [Serializable]
    public class EffectCastData
    {
        [HideInInspector]public CastDirectionMode castDirectionMode = CastDirectionMode.Default;

        [HideInInspector]public float targetSearchRange = 10f;

        [HideInInspector]public bool spawnProjectile;
        [HideInInspector]public GameObject projectilePrefab;

        [HideInInspector]public bool spawnZone;
        [HideInInspector]public GameObject zonePrefab;

        [HideInInspector]public bool onCursor;
        [HideInInspector]public bool followCaster;

        [HideInInspector]public List<EffectData> appliedEffects = new();
    }
}