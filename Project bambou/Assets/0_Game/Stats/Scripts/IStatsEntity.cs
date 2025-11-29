using Health;

namespace Stats
{
    public interface IStatsEntity
    {
        public float ComputeStat(float baseValue, float bonusPercentage, HealthEventType type);
        public bool ComputeCrit(HealthEventType type);
    }
}