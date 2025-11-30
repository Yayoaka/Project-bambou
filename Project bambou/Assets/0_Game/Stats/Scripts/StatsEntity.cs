using Health;
using Skills;
using Stats.Data;
using UnityEngine;

namespace Stats
{
    public class StatsEntity : MonoBehaviour, IStatsEntity
    {
        public float ComputeStat(HealthModificationData data, bool isCritical)
        {
            throw new System.NotImplementedException();
        }

        public bool ComputeCrit(HealthEventType type)
        {
            throw new System.NotImplementedException();
        }

        public float ComputeReceivedStat(float baseValue, HealthEventType type)
        {
            throw new System.NotImplementedException();
        }
    }
}