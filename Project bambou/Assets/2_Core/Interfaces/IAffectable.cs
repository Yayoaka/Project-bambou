using Health;

namespace Interfaces
{
    public interface IAffectable
    {
        void Damage(HealthEventData healthEventData);
    }
}