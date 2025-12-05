using System;
using Health;

namespace Interfaces
{
    public interface IHealthComponent
    {
        float MaxHealth { get; }
        float CurrentHealth { get; }
        
        bool IsAlive { get; }
        
        event Action OnDeath;
        event Action<HealthEventData> OnHit;
        
        void ApplyDamage(HealthEventData data);
        void ApplyHeal(HealthEventData data);
        void HandleDeath(HealthEventData data);
    }
}