using System;
using System.Collections.Generic;
using Enemies.Data;
using UnityEngine;

namespace Wave.Patterns
{
    public abstract class SpawnPattern : ScriptableObject
    {
        public abstract void GeneratePositions(
            Vector3 origin,
            EnemyDataSo enemy,
            int count,
            List<Vector3> outPositions);
    }
}