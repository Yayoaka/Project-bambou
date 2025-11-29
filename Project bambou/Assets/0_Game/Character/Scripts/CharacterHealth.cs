using Health;

namespace Character
{
    public class CharacterHealth : CharacterComponent, IHealthComponent
    {
        public void ApplyDamage(HealthEventData data)
        {
            throw new System.NotImplementedException();
        }

        public bool IsAlive { get; }
    }
}