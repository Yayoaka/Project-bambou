using Health;
using Skills;

namespace Stats
{
    public interface IStatsEntity
    {
        public float ComputeStat(HealthModificationData data, bool isCritical);
        public bool ComputeCrit(HealthEventType type);
        public float ComputeReceivedStat(float baseValue, HealthEventType type);
    }
}