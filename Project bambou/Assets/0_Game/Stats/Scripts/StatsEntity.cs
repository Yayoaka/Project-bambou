using Health;
using UnityEngine;

namespace Stats
{
    public class StatsEntity : MonoBehaviour, IStatsEntity
    {
        public float ComputeStat(float baseValue, float bonusPercentage, HealthEventType type)
        {
            throw new System.NotImplementedException();
        }

        public bool ComputeCrit(HealthEventType type)
        {
            throw new System.NotImplementedException();
        }
    }
}