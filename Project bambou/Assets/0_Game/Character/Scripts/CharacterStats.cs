using Health;
using Stats;

namespace Character
{
    public class CharacterStats : CharacterComponent, IStatsEntity
    {
        public float ComputeStat(float baseValue, float bonusPercentage, HealthEventType type)
        {
            return baseValue;
        }

        public bool ComputeCrit(HealthEventType type)
        {
            return false;
        }
    }
}