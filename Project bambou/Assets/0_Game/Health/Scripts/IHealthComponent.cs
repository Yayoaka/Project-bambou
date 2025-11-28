namespace Health
{
    public interface IHealthComponent
    {
        void ApplyDamage(HealthEventData data);
        bool IsAlive { get; }
    }
}
